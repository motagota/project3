using UnityEngine;

namespace V2.Data
{
    public class Entity
    {
        static int IDCounter = 0;
        public readonly int ID;

        public Vector2Int LocalPostion;
        public float Rotation;

        public Entity(Vector2Int localPosition)
        {
            ID = IDCounter++;
            // Ensure the position is already a grid position (Vector2Int guarantees integers)
            LocalPostion = localPosition;
            Rotation = 0;
        }

        // Add method to update position with grid snapping
        public void SetPosition(Vector2Int newPosition)
        {
            LocalPostion = newPosition;
        }

        // Add method to update position with automatic grid snapping
        public void SetPosition(Vector2 worldPosition)
        {
            LocalPostion = GridUtility.SnapToGrid(worldPosition);
        }

        public void Rotate()
        {
            Rotation = (Rotation + 90) % 360;
        }

        public virtual void Tick(float dt)
        {
            
        }
    }
}