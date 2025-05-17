using UnityEngine;
using V2.Data;

public static class GridUtility
{
    public const float GridSize = 1.0f;
    
    /// <summary>
    /// Snaps a world position to the nearest grid position
    /// </summary>
    public static Vector2Int SnapToGrid(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / GridSize);
        int z = Mathf.RoundToInt(worldPosition.z / GridSize);
        return new Vector2Int(x, z);
    }
    
    /// <summary>
    /// Converts a grid position to a world position
    /// </summary>
    public static Vector2 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector2(gridPosition.x * GridSize, gridPosition.y * GridSize);
    }
    
    /// <summary>
    /// Checks if a grid position is occupied by any entity in the chunk
    /// </summary>
    public static bool IsOccupied(ChunkData chunk, Vector2Int gridPosition)
    {
        // Check for machines
        if (chunk.GetMachineAt(gridPosition) != null)
        {
            return true;
        }
        
        // Check for belts
        foreach (var belt in chunk.GetBelts())
        {
            if (belt.LocalPosition == gridPosition)
            {
                return true;
            }
        }
        
        // Check for connectors
        foreach (var connector in chunk.GetConnectors())
        {
            if (connector.LocalPosition == gridPosition)
            {
                return true;
            }
        }
        
        return false;
    }
}