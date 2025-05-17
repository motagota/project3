using UnityEngine;

namespace V2.Data
{
    public static class GridUtility
    {
       
        public const float CellSize = 1.0f;
       
        public static Vector2Int SnapToGrid(Vector2 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x / CellSize);
            int y = Mathf.RoundToInt(worldPosition.y / CellSize);
            return new Vector2Int(x, y);
        }
        
        
        public static Vector2 GridToWorld(Vector2Int gridPosition)
        {
            return new Vector2(gridPosition.x * CellSize, gridPosition.y * CellSize);
        }
        
        
        public static bool IsOccupied(ChunkData chunk, Vector2Int gridPosition)
        {
        
            foreach (var machine in chunk.GetMachines())
            {
                if (machine.LocalPosition == gridPosition)
                    return true;
            }
            
            foreach (var connector in chunk.GetConnectors())
            {
                if (connector.LocalPosition == gridPosition)
                    return true;
            }
            
            foreach (var belt in chunk.GetBelts())
            {
                if (belt.LocalPosition == gridPosition)
                    return true;
            }
            
            return false;
        }
    }
}