using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int roomId;
    // The 1D grid indices which the room occupies
    public List<int> gridIds = new List<int>();

    public Room(int id, int startingGridId)
    {
        roomId = id;
        gridIds.Add(startingGridId);
    }

    // Look at this cells 4 neighbors and return their ids if not the same room
    public List<int> GetAdjacentGridIds(int gridId)
    {
        HashSet<int> adjRooms = new HashSet<int>();
        int gridSize = RoomGenerator.Instance.GetGridSize();
        if (gridId - gridSize >= 0)
        {
            if (!gridIds.Contains(gridId - gridSize))
            {
                adjRooms.Add(gridId - gridSize);
            }
        }
        // DOWN check
        else if (gridId + gridSize < gridSize * gridSize)
        {
            if (!gridIds.Contains(gridId + gridSize))
            {
                adjRooms.Add(gridId + gridSize);
            }
        }
        // RIGHT check
        else if ((gridId + 1) % gridSize != 0)
        {
            if (!gridIds.Contains(gridId + 1))
            {
                adjRooms.Add(gridId + 1);
            }
        }
        // LEFT check
        else if (gridId % gridSize != 0)
        {
            if (!gridIds.Contains(gridId - 1))
            {
                adjRooms.Add(gridId - 1);
            }
        }
        return new List<int>(adjRooms);
    }

    // Get all gridIds of neighboringRooms (forming a border around the room)
    public List<int> GetRoomBorderGridIds()
    {
        HashSet<int> adjRooms = new HashSet<int>();
        for (int i = 0; i < gridIds.Count; i++)
        {
            int currId = gridIds[i];
            foreach (int roomId in GetAdjacentGridIds(currId))
            {
                adjRooms.Add(roomId);
            }
        }
        return new List<int>(adjRooms);
    }

    // Gets a vector offset of the second room from the first, essentailly getting the room's direction
    // Assumes room ids are valid and that the rooms are adjacent
    public Vector3 GetRoomOffset(int originalId, int offsetRoom)
    {
        int gridSize = RoomGenerator.Instance.GetGridSize();
        int roomSize = RoomGenerator.Instance.GetRoomSize();
        // UP check
        if (offsetRoom - originalId == -gridSize)
        {
            return Vector3.left * roomSize;
        }
        // DOWN check
        else if (offsetRoom - originalId == gridSize)
        {
            return Vector3.right * roomSize;
        }
        // RIGHT check
        else if (offsetRoom - originalId == 1)
        {
            return Vector3.forward * roomSize;
        }
        // LEFT check
        else if (offsetRoom - originalId == -1)
        {
            return Vector3.back * roomSize;
        }
        else {
            return Vector3.zero;
        }
    }
}
