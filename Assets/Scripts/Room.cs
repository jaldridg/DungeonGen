using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int roomId;
    // The 1D grid indices which the room occupies
    public List<int> gridIds = new List<int>();

    public Room(int id, int startingGridId) {
        roomId = id;
        gridIds.Add(startingGridId);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<int> GetAdjacentGridIds() {
        HashSet<int> adjRooms = new HashSet<int>();
        int gridSize = RoomGenerator.Instance.GetGridSize();
        // Brute force all 4 sides of each room
        for (int i = 0; i < gridIds.Count; i++) {
            int currId = gridIds[i];
            // Add all 4 directions if they are inside of the grid
            // UP check
            if (currId - gridSize >= 0) {
                if (!gridIds.Contains(currId - gridSize)) {
                    adjRooms.Add(currId - gridSize);
                }
            }
            // DOWN check
            if (currId + gridSize < gridSize * gridSize) {
                if (!gridIds.Contains(currId + gridSize)) {
                    adjRooms.Add(currId + gridSize);
                }
            }
            // RIGHT check
            if ((currId + 1) % gridSize != 0) {
                if (!gridIds.Contains(currId + 1)) {
                    adjRooms.Add(currId + 1);
                }
            }
            // LEFT check
            if (currId % gridSize != 0) {
                if (!gridIds.Contains(currId - 1)) {
                    adjRooms.Add(currId - 1);
                }
            }
        }

        return new List<int>(adjRooms);    
    }
}
