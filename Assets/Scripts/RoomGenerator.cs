using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] float roomVariable = 0.5f;
    [SerializeField] public int gridSize = 10;
    [SerializeField] public int roomSize = 5;
    // Stores the room ids which belong to the locations in the grid
    private int[,] grid;

    private static int nextRoomId = 1;

    private static List<Room> rooms = new List<Room>();

    public static RoomGenerator Instance;

    // Start is called before the first frame update

    void Awake()
    {
        if (Instance != this && Instance != null) {
            Destroy(this);
        }
        Instance = this;
    }

    void Start()
    {
        grid = new int[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                grid[i, j] = 0;
            }
        }
        GenerateRooms();
    }

    // Update is called once per frame
    void Update() { }

    private void GenerateRooms() {
        // A 1 dimensional unfolding of our grid
        List<int> unusedGridIndices = new List<int>();
        for (int i = 0; i < gridSize * gridSize; i++) {
            unusedGridIndices.Add(i);
        }
        Debug.Log("UGI size = " + unusedGridIndices.Count);

        while (unusedGridIndices.Count > 0) {
            int randomIndex = (int) Random.Range(0.0f, unusedGridIndices.Count);

            // Pop random usassigned grid cell
            int randomCell = unusedGridIndices[randomIndex];
            unusedGridIndices.RemoveAt(randomIndex);

            // Make a new room
            Room newRoom = new Room(GetNewRoomId(), randomCell);
            rooms.Add(newRoom);
            Debug.Log("added room");

            // Assign room to grid
            grid[GridCellId2GridDim1(randomCell), GridCellId2GridDim2(randomCell)] = newRoom.roomId;

            while (Random.Range(0.0f, 1.0f) > roomVariable) {
                List<int> adjRooms = newRoom.GetAdjacentGridIds();
                int newCellId = GetOpenCell(adjRooms);
                if (newCellId == -1) { break; }

                grid[GridCellId2GridDim1(newCellId), GridCellId2GridDim2(newCellId)] = newRoom.roomId;
                newRoom.gridIds.Add(newCellId);
                unusedGridIndices.Remove(newCellId);
            }  
        }
    }

    // Randomly choose a cell from the given list which doesn't belong to another room
    private int GetOpenCell(List<int> roomIds) {
        while (roomIds.Count > 0) {
            int selectorId = (int) Random.Range(0.0f, roomIds.Count);
            int randomRoomId = roomIds[selectorId];
            if (grid[GridCellId2GridDim1(randomRoomId), GridCellId2GridDim2(randomRoomId)] == 0) {
                return randomRoomId;
            }
            roomIds.RemoveAt(selectorId);
        }
        return -1;
    }

    void OnDrawGizmos()
    {
        // Vector3 gridStart = Vector3.zero;
        // float gridLength = roomSize * gridSize;
        // for (int i = 0; i < gridSize + 1; i++) {
        //     // First dimension
        //     Vector3 incrementalOffset = Vector3.right * i * roomSize;
        //     Gizmos.DrawLine(gridStart + incrementalOffset, gridStart + incrementalOffset + Vector3.forward * gridLength);
        //     // Second dimension
        //     incrementalOffset = Vector3.forward * i * roomSize;
        //     Gizmos.DrawLine(gridStart + incrementalOffset, gridStart + incrementalOffset + Vector3.right * gridLength);
        // }

        // Make a frame along all rooms
        Vector3 rightOffset = Vector3.forward * roomSize * gridSize;
        Vector3 downOffset = Vector3.right * roomSize * gridSize;
        Gizmos.DrawLine(Vector3.zero, rightOffset);
        Gizmos.DrawLine(Vector3.zero, downOffset);
        Gizmos.DrawLine(Vector3.zero + downOffset, rightOffset + downOffset);
        Gizmos.DrawLine(Vector3.zero + rightOffset, downOffset + rightOffset);
        
        foreach (Room room in rooms) {
            foreach (int cellId in room.gridIds) {
                Vector3 cornerLoc = new Vector3(GridCellId2GridDim1(cellId) * roomSize, 0.0f, GridCellId2GridDim2(cellId) * roomSize);
                Vector3 centerRoomOffset = new Vector3(roomSize / 2, 0.0f, roomSize / 2);

                // It's confusing but trust
                Vector3 cellDownOffset = Vector3.right * roomSize;
                Vector3 cellRightOffset = Vector3.forward * roomSize;

                // UP check
                if (!room.gridIds.Contains(cellId - gridSize)) {
                    Gizmos.DrawLine(cornerLoc, cornerLoc + cellRightOffset);
                }
                // DOWN check
                if (!room.gridIds.Contains(cellId + gridSize)) {
                    Gizmos.DrawLine(cornerLoc + cellDownOffset, cornerLoc + cellRightOffset + cellDownOffset);
                }
                // RIGHT check
                if (!room.gridIds.Contains(cellId + 1)) {
                    Gizmos.DrawLine(cornerLoc + cellRightOffset, cornerLoc + cellDownOffset + cellRightOffset);
                }
                // LEFT check
                if (!room.gridIds.Contains(cellId - 1)) {
                    Gizmos.DrawLine(cornerLoc, cornerLoc + cellDownOffset);
                }
            }
        }
    }

    private int GridCellId2GridDim1(int gridCellId) {
        return gridCellId / gridSize;
    }
    private int GridCellId2GridDim2(int gridCellId) {
        return gridCellId % gridSize;
    }

    public int GetGridSize() {
        return gridSize;
    }

    public int GetNewRoomId() {
        return nextRoomId++;
    }
}
