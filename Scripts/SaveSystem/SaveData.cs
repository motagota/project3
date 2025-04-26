using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // Player data
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public Dictionary<int, int> playerInventory = new Dictionary<int, int>();
    
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
        public Vector3 position;
        public Quaternion rotation;
        public int gridX;
        public int gridY;
        
        // Miner-specific data
        public int resourceType;
        public int storedResources;
        public bool isOperating;
        public float partialResources;
        
        // Storage box-specific data
        public Dictionary<int, int> storedItems = new Dictionary<int, int>();
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