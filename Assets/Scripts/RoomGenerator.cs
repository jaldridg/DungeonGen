using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    // The change a room will expand to an adjacent cell during generation
    [SerializeField] float deadRoomChance = 0.5f;

    // The proportions of doors added randomly after initial door maze generation completes - dangerous over 1.0
    [SerializeField] float rand2MazeDoorRatio = 0.1f;

    [SerializeField] public int gridSize = 10;
    [SerializeField] public int roomSize = 5;
    // Stores the room ids which belong to the locations in the grid
    private int[,] grid;

    private static int nextRoomId = 0;

    // Ordered so that the room with id x is at index x
    private static List<Room> rooms = new List<Room>();
    private static HashSet<Vector2> walls = new HashSet<Vector2>();
    private static List<Vector3> doorLocs = new List<Vector3>();

    public static RoomGenerator Instance;

    // Rooms within region i can be reached from any room in region i but not from rooms belonging to different regions
    private List<List<Room>> regionsList = new List<List<Room>>();
    private List<UnityEngine.Color> regionColors = new List<UnityEngine.Color>();

    // GameObjects for Physical generation 
    [SerializeField] GameObject floorGO;
    [SerializeField] GameObject wallGO;
    [SerializeField] GameObject doorGO;


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
        GenerateWalls();
        GenerateDoors();
        //StartCoroutine(TestConnectivity());
        BuildRooms();
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

            // Expand grid cells to expand room
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

    private void GenerateWalls()
    {
        foreach (Room room in rooms)
        {
            foreach (int cellId in room.gridIds)
            {
                Vector2 cornerLoc = new Vector2(GridCellId2GridDim1(cellId) * roomSize, GridCellId2GridDim2(cellId) * roomSize);
                float roomHalfSize = roomSize / 2.0f;
                Vector2 roomCenter = new Vector2(cornerLoc.x + roomHalfSize, cornerLoc.y + roomHalfSize);

                // Going right on the grid is forward in Unity space
                // Going down on the grid is right in Unity space
                
                // Use the y coordinate to store whether walls are vertical or horizontal (for later)
                // UP check
                if (!room.gridIds.Contains(cellId - gridSize))
                {
                    walls.Add(roomCenter + Vector2.left * roomHalfSize);
                }
                // DOWN check
                if (!room.gridIds.Contains(cellId + gridSize))
                {
                    walls.Add(roomCenter + Vector2.right * roomHalfSize);
                }
                // RIGHT check
                if (!room.gridIds.Contains(cellId + 1))
                {
                    walls.Add(roomCenter + Vector2.up * roomHalfSize);
                }
                // LEFT check
                if (!room.gridIds.Contains(cellId - 1))
                {
                    walls.Add(roomCenter + Vector2.down * roomHalfSize);
                }
            }
        }
    }
    // Connects rooms like a maze then sprinkles in more doors
    private void GenerateDoors()
    {
        Dictionary<Vector2, float> walls = new Dictionary<Vector2, float>();        Room currentRoom = rooms[0];
        List<Room> visitedStack = new List<Room>() {currentRoom};

        while (visitedStack.Count > 0)
        {
            // Stores room Ids of room pairs in which a door could be generated between
            List<Vector2> roomPairs = new List<Vector2>();

            // Generates room pairs by looping over each cell in a room and getting the neighboring room if unvisited
            foreach (int gId in currentRoom.gridIds)
            {
                foreach (int adjGId in currentRoom.GetAdjacentGridIds(gId))
                {
                    if (!rooms[GridCellId2RoomId(adjGId)].visited)
                    {
                        roomPairs.Add(new Vector2(gId, adjGId));
                    }
                }
            }

            // Check if room is a dead end
            if (roomPairs.Count == 0)
            {
                currentRoom.visited = true;
                currentRoom = visitedStack[visitedStack.Count - 1];
                visitedStack.RemoveAt(visitedStack.Count - 1);
                continue;
            }

            // If not a dead end, choose a door by choosing a random pair
            Vector2 roomPair = roomPairs[UnityEngine.Random.Range(0, roomPairs.Count)];
            int gridId = (int)roomPair[0];
            int adjGridId = (int)roomPair[1];

            // Find door location
            Vector3 offset = currentRoom.GetRoomOffset(gridId, adjGridId) / 2;
            Vector3 gridLoc = new Vector3(roomSize * GridCellId2GridDim1(gridId), 0.0f, roomSize * GridCellId2GridDim2(gridId));
            doorLocs.Add(gridLoc + offset);

            // Connect the rooms
            int adjacentRoomId = GridCellId2RoomId(adjGridId);
            currentRoom.AddConnectedRoom(adjacentRoomId);
            Room nextRoom = rooms[adjacentRoomId];
            nextRoom.AddConnectedRoom(GridCellId2RoomId(gridId));
            currentRoom.visited = true;

            // Prepare for next iteration
            visitedStack.Add(nextRoom);
            currentRoom = nextRoom;
        }


        // Add additional doors for more connections
        int additionalDoors = (int)(rooms.Count * rand2MazeDoorRatio);
        int doorsAdded = 0;
        while (doorsAdded < additionalDoors)
        {
            Room randRoom = rooms[UnityEngine.Random.Range(0, rooms.Count)];
            int randGridId = randRoom.gridIds[UnityEngine.Random.Range(0, randRoom.gridIds.Count)];

            Vector3 cornerLoc = new Vector3(GridCellId2GridDim1(randGridId) * roomSize, 0.0f, GridCellId2GridDim2(randGridId) * roomSize);
            foreach (int adjRoomId in randRoom.GetAdjacentGridIds(randGridId))
            {
                Vector3 offset = randRoom.GetRoomOffset(randGridId, adjRoomId) / 2;
                doorLocs.Add(cornerLoc + offset);
                int adjacentRoomId = GridCellId2RoomId(adjRoomId);
                randRoom.AddConnectedRoom(adjacentRoomId);
                rooms[adjacentRoomId].AddConnectedRoom(GridCellId2RoomId(randGridId));
                doorsAdded++;
            }
        }
    }

    private void BuildRooms()
    {
        Vector3 centerRoomOffset = new Vector3(roomSize * 0.5f, 0.0f, roomSize * 0.5f);
        foreach (Room r in rooms)
        {
            foreach(int gridId in r.gridIds)
            {
                // Make the floor
                Vector3 loc = new Vector3(GridCellId2GridDim1(gridId) * roomSize, 0.0f, GridCellId2GridDim2(gridId) * roomSize);
                GameObject f = Instantiate(floorGO, loc + centerRoomOffset, Quaternion.identity);
                f.transform.localScale = new Vector3(roomSize / 10.0f, 1.0f, roomSize / 10.0f);
            }
        }

        foreach (Vector2 wall in walls)
        {
            Vector3 wall3D = new Vector3(wall.x, roomSize / 4.0f, wall.y);
            GameObject w = Instantiate(wallGO, wall3D, Quaternion.identity);
            // Vertical walls coordinates will be multiples of roomSize and need to be flipped
            bool needToFlip = wall.x % roomSize == 0;
            w.transform.rotation = Quaternion.Euler(0.0f, needToFlip ? 90.0f : 0.0f, 0.0f);
            w.transform.localScale = new Vector3(roomSize / 10.0f, roomSize / 10.0f, 1.0f);
        }
    }

    // Randomly choose a cell from the given list which doesn't belong to another room
    private int GetOpenCell(List<int> roomIds)
    {
        while (roomIds.Count > 0)
        {
            int selectorId = (int)UnityEngine.Random.Range(0.0f, roomIds.Count);
            int randomRoomId = roomIds[selectorId];
            if (GridCellId2RoomId(randomRoomId) == 0)
            {
                return randomRoomId;
            }
            roomIds.RemoveAt(selectorId);
        }
        return -1;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = UnityEngine.Color.black;

        float halfRoomSize = roomSize / 2.0f;
        foreach (Vector2 wall in walls)
        {
            // Vertical walls coordinates will be multiples of roomSize and need to be flipped
            Vector3 offset = wall.x % roomSize == 0 ? Vector3.forward : Vector3.right;
            Vector3 wall3D = new Vector3(wall.x, 0.0f, wall.y);
            Gizmos.DrawLine(wall3D - offset * halfRoomSize, wall3D + offset * halfRoomSize);
        }

        Gizmos.color = UnityEngine.Color.grey;
        Vector3 centerRoomOffset = new Vector3(roomSize * 0.5f, 0.0f, roomSize * 0.5f);
        foreach (Vector3 loc in doorLocs)
        {
            Gizmos.DrawSphere(loc + centerRoomOffset, 1.0f);
        }

        for (int i = 0; i < regionsList.Count; i++)
        {
            List<Room> reg = regionsList[i];
            Gizmos.color = regionColors[i];
            foreach (Room r in reg)
            {
                foreach (int gid in r.gridIds)
                {
                    Vector3 gridScaling = new Vector3(GridCellId2GridDim1(gid) * roomSize, 0.0f, GridCellId2GridDim2(gid) * roomSize);
                    Gizmos.DrawCube(gridScaling + centerRoomOffset, Vector3.one * 2);
                }
            }
        }
    }

    public IEnumerator TestConnectivity()
    {
        List<Room> unvisitedRooms = new List<Room>();
        rooms.ForEach(r => r.visited = false);
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
            yield return new WaitForSeconds(0.1f);
        }
    }

    private int GridCellId2RoomId(int gridCellId)
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
