using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    // The change a room will expand to an adjacent cell during generation
    [SerializeField] float deadRoomChance = 0.5f;
    // The chance each wall in a room has to generate a door
    [SerializeField] float doorChance = 0.5f;
    [SerializeField] public int gridSize = 10;
    [SerializeField] public int roomSize = 5;
    // Stores the room ids which belong to the locations in the grid
    private int[,] grid;

    private static int nextRoomId = 0;

    // Ordered so that the room with id x is at index x
    private static List<Room> rooms = new List<Room>();
    private static List<Vector3> tempDoorLocs = new List<Vector3>();

    public static RoomGenerator Instance;

    // Rooms within region i can be reached from any room in region i but not from rooms belonging to different regions
    private List<List<Room>> regionsList = new List<List<Room>>();
    private List<UnityEngine.Color> regionColors = new List<UnityEngine.Color>();


    void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        grid = new int[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                grid[i, j] = 0;
            }
        }
        GenerateRooms();
        GenerateDoors();
        StartCoroutine(TestConnectivity());
    }

    // Update is called once per frame
    void Update() { }

    private void GenerateRooms()
    {
        // A 1 dimensional unfolding of our grid
        List<int> unusedGridIndices = new List<int>();
        for (int i = 0; i < gridSize * gridSize; i++)
        {
            unusedGridIndices.Add(i);
        }

        while (unusedGridIndices.Count > 0)
        {
            int randomIndex = (int)UnityEngine.Random.Range(0.0f, unusedGridIndices.Count);

            // Pop random usassigned grid cell
            int randomCell = unusedGridIndices[randomIndex];
            unusedGridIndices.RemoveAt(randomIndex);

            // Make a new room
            Room newRoom = new Room(GetNewRoomId(), randomCell);
            rooms.Add(newRoom);

            // Assign room to grid
            grid[GridCellId2GridDim1(randomCell), GridCellId2GridDim2(randomCell)] = newRoom.roomId;

            while (UnityEngine.Random.Range(0.0f, 1.0f) > deadRoomChance)
            {
                List<int> adjRooms = newRoom.GetRoomBorderGridIds();
                int newCellId = GetOpenCell(adjRooms);
                if (newCellId == -1) { break; }

                grid[GridCellId2GridDim1(newCellId), GridCellId2GridDim2(newCellId)] = newRoom.roomId;
                newRoom.gridIds.Add(newCellId);
                unusedGridIndices.Remove(newCellId);
            }
        }
    }

    private void GenerateDoors()
    {
        foreach (Room room in rooms)
        {
            foreach (int roomId in room.gridIds)
            {
                Vector3 cornerLoc = new Vector3(GridCellId2GridDim1(roomId) * roomSize, 0.0f, GridCellId2GridDim2(roomId) * roomSize);
                Vector3 centerRoomOffset = new Vector3(roomSize / 2, 0.0f, roomSize / 2);
                foreach (int adjRoomGridId in room.GetAdjacentGridIds(roomId))
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) < doorChance)
                    {
                        Vector3 offset = room.GetRoomOffset(roomId, adjRoomGridId);
                        tempDoorLocs.Add(cornerLoc + centerRoomOffset + offset / 2);
                        int adjacentRoomId = GridCellId2RoomNum(adjRoomGridId);
                        room.connectedRooms.Add(adjacentRoomId);
                        rooms[adjacentRoomId].connectedRooms.Add(GridCellId2RoomNum(roomId));
                    }
                }
            }
        }
        // for each room
        // draw door with % chance if adj room is not the same id   
    }

    // Randomly choose a cell from the given list which doesn't belong to another room
    private int GetOpenCell(List<int> roomIds)
    {
        while (roomIds.Count > 0)
        {
            int selectorId = (int)UnityEngine.Random.Range(0.0f, roomIds.Count);
            int randomRoomId = roomIds[selectorId];
            if (GridCellId2RoomNum(randomRoomId) == 0)
            {
                return randomRoomId;
            }
            roomIds.RemoveAt(selectorId);
        }
        return -1;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = UnityEngine.Color.white;
        // Make a frame along all rooms
        // Vector3 rightOffset = Vector3.forward * roomSize * gridSize;
        // Vector3 downOffset = Vector3.right * roomSize * gridSize;
        // Gizmos.DrawLine(Vector3.zero, rightOffset);
        // Gizmos.DrawLine(Vector3.zero, downOffset);
        // Gizmos.DrawLine(Vector3.zero + downOffset, rightOffset + downOffset);
        // Gizmos.DrawLine(Vector3.zero + rightOffset, downOffset + rightOffset);

        foreach (Room room in rooms)
        {

            foreach (int cellId in room.gridIds)
            {
                Vector3 cornerLoc = new Vector3(GridCellId2GridDim1(cellId) * roomSize, 0.0f, GridCellId2GridDim2(cellId) * roomSize);

                // It's confusing but trust
                Vector3 cellDownOffset = Vector3.right * roomSize;
                Vector3 cellRightOffset = Vector3.forward * roomSize;

                // UP check
                if (!room.gridIds.Contains(cellId - gridSize))
                {
                    Gizmos.DrawLine(cornerLoc, cornerLoc + cellRightOffset);
                }
                // DOWN check
                if (!room.gridIds.Contains(cellId + gridSize))
                {
                    Gizmos.DrawLine(cornerLoc + cellDownOffset, cornerLoc + cellRightOffset + cellDownOffset);
                }
                // RIGHT check
                if (!room.gridIds.Contains(cellId + 1))
                {
                    Gizmos.DrawLine(cornerLoc + cellRightOffset, cornerLoc + cellDownOffset + cellRightOffset);
                }
                // LEFT check
                if (!room.gridIds.Contains(cellId - 1))
                {
                    Gizmos.DrawLine(cornerLoc, cornerLoc + cellDownOffset);
                }
            }
        }
        foreach (Vector3 loc in tempDoorLocs)
        {
            Gizmos.DrawSphere(loc, 1.0f);
        }
        Gizmos.color = UnityEngine.Color.blue;
        for (int i = 0; i < regionsList.Count; i++)
        {
            List<Room> reg = regionsList[i];
            Gizmos.color = regionColors[i];
            foreach (Room r in reg)
            {
                foreach (int gid in r.gridIds)
                {
                    Vector3 offset = new Vector3(roomSize * 0.5f, 0.0f, roomSize * 0.5f);
                    Vector3 loc = new Vector3(GridCellId2GridDim1(gid) * roomSize, 0.0f, GridCellId2GridDim2(gid) * roomSize);
                    Gizmos.DrawCube(loc + offset, Vector3.one * 2);
                }
            }
        }
    }

    public IEnumerator TestConnectivity()
    {
        List<Room> unvisitedRooms = new List<Room>();
        rooms.ForEach(r => unvisitedRooms.Add(r));

        // Loop over each region
        while (unvisitedRooms.Count > 0)
        {
            // Keep track of the rooms you've visited to add to the region
            List<Room> region = new List<Room>();
            // Keep track of the order you've looked at rooms for backtracking
            List<Room> visitedStack = new List<Room>();

            // Get a room that hasn't been looked at
            Room currentRoom = unvisitedRooms[0];
            visitedStack.Add(currentRoom);

            while (visitedStack.Count > 0)
            {
                int currentDepth = visitedStack.Count - 1;
                currentRoom = visitedStack[currentDepth];
                currentRoom.visited = true;
                unvisitedRooms.Remove(currentRoom);
                region.Add(currentRoom);

                bool foundRoom = false;
                foreach (int rId in currentRoom.connectedRooms)
                {
                    Room conRoom = rooms[rId];
                    if (!conRoom.visited)
                    {
                        visitedStack.Add(conRoom);
                        foundRoom = true;
                        break;
                    }
                }
                if (!foundRoom)
                {
                    visitedStack.RemoveAt(currentDepth);
                }
            }

            regionsList.Add(region);
            float r = UnityEngine.Random.Range(0.0f, 1.0f);
            float g = UnityEngine.Random.Range(0.0f, 1.0f);
            float b = UnityEngine.Random.Range(0.0f, 1.0f);
            regionColors.Add(new UnityEngine.Color(r, g, b));
            yield return new WaitForSeconds(0.5f);
        }
    }

    private int GridCellId2RoomNum(int gridCellId)
    {
        return grid[GridCellId2GridDim1(gridCellId), GridCellId2GridDim2(gridCellId)];
    }

    private int GridCellId2GridDim1(int gridCellId)
    {
        return gridCellId / gridSize;
    }
    private int GridCellId2GridDim2(int gridCellId)
    {
        return gridCellId % gridSize;
    }

    public int GetGridSize()
    {
        return gridSize;
    }

    public int GetRoomSize()
    {
        return roomSize;
    }

    public int GetNewRoomId()
    {
        return nextRoomId++;
    }
}
