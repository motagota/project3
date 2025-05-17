using UnityEngine;
using V2.Data;

public static class GridUtility
{
    public const float GridSize = 1.0f;
    
    /// <summary>
    /// Snaps a world position to the nearest grid position
    /// </summary>
    public static Vector2Int SnapToGrid(Vector2 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / GridSize);
        int y = Mathf.RoundToInt(worldPosition.y / GridSize);
        return new Vector2Int(x, y);
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
            if (belt.LocalPostion == gridPosition)
            {
                return true;
            }
        }
        
        // Check for connectors
        foreach (var connector in chunk.GetConnectors())
        {
            if (connector.LocalPostion == gridPosition)
            {
                return true;
            }
        }
        
        return false;
    }
}