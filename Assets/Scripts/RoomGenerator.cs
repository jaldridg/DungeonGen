using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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

    private List<Vector3> tempLocs = new List<Vector3>();
    // The number of room "region" which are isolated from other regions
    // Rooms within region i can be reached from any room in region i
    private int dungeonRegions = 1;

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
        foreach (Vector3 loc in tempLocs)
        {
            Gizmos.DrawCube(loc, Vector3.one * 2);
        }
    }

    public IEnumerator TestConnectivity()
    {
        List<Room> unvistedRooms = rooms;

        while (unvistedRooms.Count > 0)
        {
            Vector3 centerOffset = new Vector3(roomSize / 2, 0.0f, roomSize / 2);
            // Used to backtrack (to mimick recursion)
            List<int> visitedOrder = new List<int>();
            Debug.Log("unvistedRooms count:" + unvistedRooms.Count);
            bool stuck = true;
            int currentRoomNum = GridCellId2RoomNum(unvistedRooms[0].gridIds[0]);

            do
            {
                Room currRoom = rooms[currentRoomNum];
                foreach (int id in currRoom.gridIds)
                {
                    Vector3 cornerLoc = new Vector3(GridCellId2GridDim1(id) * roomSize, 0.0f, GridCellId2GridDim2(id) * roomSize);
                    tempLocs.Add(cornerLoc + centerOffset);
                }
                currRoom.visited = true;
                unvistedRooms.Remove(currRoom);

                int currBacktrackPos = visitedOrder.Count - 1;
                while (stuck)
                {
                    List<int> connectedRooms = currRoom.connectedRooms;
                    // Loop over each connected room and visit if not already visited
                    for (int i = 0; i < connectedRooms.Count; i++)
                    {
                        Debug.Log("rooms[currRoom.connectedRooms[i]] = " + rooms[currRoom.connectedRooms[i]]);
                        Room connRoom = rooms[currRoom.connectedRooms[i]];
                        if (!connRoom.visited)
                        {
                            stuck = false;
                            currentRoomNum = connectedRooms[i];
                            visitedOrder.Add(currentRoomNum);
                            break;
                        }
                    }
                    if (currBacktrackPos < 0) { break; }
                    currentRoomNum = visitedOrder[currBacktrackPos--];
                }
                yield return new WaitForSeconds(1.0f);
            } while (!stuck);
            dungeonRegions += 1;
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
