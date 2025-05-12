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
            LocalPostion = localPosition;
            Rotation = 0;
        }

        public void SetPosition(Vector2Int newPosition)
        {
            LocalPostion = newPosition;
        }
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