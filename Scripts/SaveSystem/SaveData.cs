using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem
{
    [Serializable]
    public class SaveData
    {
        // Serializable versions of Unity types
        [Serializable]
        public struct SerializableVector3
        {
            public float x, y, z;
        
            public SerializableVector3(Vector3 vector)
            {
                x = vector.x;
                y = vector.y;
                z = vector.z;
            }
        
            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
        }
    
        [Serializable]
        public struct SerializableQuaternion
        {
            public float x, y, z, w;
        
            public SerializableQuaternion(Quaternion quaternion)
            {
                x = quaternion.x;
                y = quaternion.y;
                z = quaternion.z;
                w = quaternion.w;
            }
        
            public Quaternion ToQuaternion()
            {
                return new Quaternion(x, y, z, w);
            }
        }
    
        // Player data - use serializable types
        public SerializableVector3 playerPosition;
        public SerializableQuaternion playerRotation;
        public Dictionary<int, int> PlayerInventory = new Dictionary<int, int>();
    
        // Building data
        public List<BuildingData> buildings = new List<BuildingData>();
    
        // Grid data (if needed)
        public List<GridTileData> modifiedTiles = new List<GridTileData>();
    
        // Game time, score, or other global variables
        public float gameTime;
    
        [Serializable]
        public class BuildingData
        {
            public string buildingType;
            public SerializableVector3 position;
            public SerializableQuaternion rotation;
            public int gridX;
            public int gridY;
        
            // Miner-specific data
            public int resourceType;
            public int storedResources;
            public bool isOperating;
            public float partialResources;
        
            // Storage box-specific data
            public Dictionary<int, int> StoredItems = new Dictionary<int, int>();
        }
    
        [Serializable]
        public class GridTileData
        {
            public int x;
            public int y;
            public int groundType;
            public int resourceType;
            public int resourceAmount;
        }
    }
}