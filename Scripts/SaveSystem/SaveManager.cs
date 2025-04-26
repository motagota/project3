using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    [Header("Save Settings")]
    public string saveFileName = "gamedata.sav";
    public bool autoSave = true;
    public float autoSaveInterval = 300f; // 5 minutes
    
    private float lastAutoSaveTime;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        // Auto-save functionality
        if (autoSave && Time.time - lastAutoSaveTime > autoSaveInterval)
        {
            SaveGame();
            lastAutoSaveTime = Time.time;
        }
    }
    
    public void SaveGame()
    {
        try
        {
            SaveData saveData = new SaveData();
            
            // Save player data
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                saveData.playerPosition = player.transform.position;
                saveData.playerRotation = player.transform.rotation;
                
                // Save player inventory
                PlayerInventory inventory = PlayerInventory.Instance;
                if (inventory != null)
                {
                    saveData.playerInventory = inventory.GetSerializableInventory();
                }
            }
            
            // Save buildings
            SaveBuildings(saveData);
            
            // Save modified grid tiles
            SaveGridTiles(saveData);
            
            // Save game time
            saveData.gameTime = Time.time;
            
            // Serialize and save to file
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, saveData);
            }
            
            Debug.Log($"Game saved successfully to {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving game: {e.Message}");
        }
    }
    
    public void LoadGame()
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            if (File.Exists(savePath))
            {
                // Deserialize save data
                SaveData saveData;
                using (FileStream stream = new FileStream(savePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    saveData = (SaveData)formatter.Deserialize(stream);
                }
                
                // Clear existing game state
                ClearExistingGameState();
                
                // Load player data
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = saveData.playerPosition;
                    player.transform.rotation = saveData.playerRotation;
                    
                    // Load player inventory
                    PlayerInventory inventory = PlayerInventory.Instance;
                    if (inventory != null)
                    {
                        inventory.LoadInventory(saveData.playerInventory);
                    }
                }
                
                // Load buildings
                LoadBuildings(saveData);
                
                // Load modified grid tiles
                LoadGridTiles(saveData);
                
                Debug.Log("Game loaded successfully");
            }
            else
            {
                Debug.LogWarning("No save file found");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading game: {e.Message}");
        }
    }
    
    private void SaveBuildings(SaveData saveData)
    {
        // Find all buildings in the scene
        MinerBuilding[] miners = FindObjectsOfType<MinerBuilding>();
        foreach (var miner in miners)
        {
            SaveData.BuildingData buildingData = new SaveData.BuildingData
            {
                buildingType = "Miner",
                position = miner.transform.position,
                rotation = miner.transform.rotation,
                resourceType = miner.GetResourceType(),
                storedResources = miner.GetStoredResources(),
                isOperating = miner.IsOperating(),
                partialResources = miner.GetPartialResources()
            };
            
            // Get grid coordinates
            if (miner.GetTargetTile() != null)
            {
                buildingData.gridX = miner.GetTargetTile().GetX();
                buildingData.gridY = miner.GetTargetTile().GetY();
            }
            
            saveData.buildings.Add(buildingData);
        }
        
        // Find all storage boxes
        StorageBox[] storageBoxes = FindObjectsOfType<StorageBox>();
        foreach (var box in storageBoxes)
        {
            SaveData.BuildingData buildingData = new SaveData.BuildingData
            {
                buildingType = "StorageBox",
                position = box.transform.position,
                rotation = box.transform.rotation,
                storedItems = box.GetSerializableStorage()
            };
            
            // Get grid coordinates if available
            GridTile tile = box.GetComponent<GridTile>();
            if (tile != null)
            {
                buildingData.gridX = tile.GetX();
                buildingData.gridY = tile.GetY();
            }
            
            saveData.buildings.Add(buildingData);
        }
    }
    
    private void LoadBuildings(SaveData saveData)
    {
        BuildingManager buildingManager = FindObjectOfType<BuildingManager>();
        if (buildingManager == null)
        {
            Debug.LogError("BuildingManager not found in scene");
            return;
        }
        
        foreach (var buildingData in saveData.buildings)
        {
            GameObject building = null;
            
            // Instantiate the appropriate building prefab
            switch (buildingData.buildingType)
            {
                case "Miner":
                    building = Instantiate(buildingManager.minerPrefab, buildingData.position, buildingData.rotation);
                    break;
                case "StorageBox":
                    building = Instantiate(buildingManager.storageBoxPrefab, buildingData.position, buildingData.rotation);
                    break;
            }
            
            if (building != null)
            {
                // Find the tile at the grid coordinates
                GridTile tile = FindTileAtCoordinates(buildingData.gridX, buildingData.gridY);
                
                if (tile != null)
                {
                    // Initialize building with tile
                    if (buildingData.buildingType == "Miner")
                    {
                        MinerBuilding miner = building.GetComponent<MinerBuilding>();
                        if (miner != null)
                        {
                            miner.Initialize(tile);
                            miner.SetStoredResources(buildingData.storedResources);
                            miner.SetPartialResources(buildingData.partialResources);
                            
                            // Start mining if it was operating
                            if (buildingData.isOperating)
                            {
                                miner.StartMining();
                            }
                        }
                    }
                    else if (buildingData.buildingType == "StorageBox")
                    {
                        StorageBox storageBox = building.GetComponent<StorageBox>();
                        if (storageBox != null)
                        {
                            storageBox.Initialize(tile);
                            storageBox.LoadStorage(buildingData.storedItems);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not find tile at coordinates ({buildingData.gridX}, {buildingData.gridY})");
                }
            }
        }
    }
    
    private void SaveGridTiles(SaveData saveData)
    {
        // This is optional - only needed if grid tiles can be modified during gameplay
        // For example, if resources can be depleted
        GridTile[] allTiles = FindObjectsOfType<GridTile>();
        
        foreach (var tile in allTiles)
        {
            // Only save tiles that have been modified from their initial state
            if (tile.HasBeenModified())
            {
                GridComputeManager.GridCell cellData = tile.GetCellData();
                
                SaveData.GridTileData tileData = new SaveData.GridTileData
                {
                    x = tile.GetX(),
                    y = tile.GetY(),
                    groundType = cellData.groundType,
                    resourceType = cellData.resourceType,
                    resourceAmount = cellData.resourceAmount
                };
                
                saveData.modifiedTiles.Add(tileData);
            }
        }
    }
    
    private void LoadGridTiles(SaveData saveData)
    {
        // Update grid tiles with saved data
        foreach (var tileData in saveData.modifiedTiles)
        {
            GridTile tile = FindTileAtCoordinates(tileData.x, tileData.y);
            
            if (tile != null)
            {
                // Update tile data
                tile.UpdateCellData(tileData.groundType, tileData.resourceType, tileData.resourceAmount);
            }
        }
    }
    
    private GridTile FindTileAtCoordinates(int x, int y)
    {
        // Find the tile at the given grid coordinates
        // This depends on how your grid system is implemented
        GridTile[] allTiles = FindObjectsOfType<GridTile>();
        
        foreach (var tile in allTiles)
        {
            if (tile.GetX() == x && tile.GetY() == y)
            {
                return tile;
            }
        }
        
        return null;
    }
    
    private void ClearExistingGameState()
    {
        // Destroy all existing buildings
        MinerBuilding[] miners = FindObjectsOfType<MinerBuilding>();
        foreach (var miner in miners)
        {
            Destroy(miner.gameObject);
        }
        
        StorageBox[] storageBoxes = FindObjectsOfType<StorageBox>();
        foreach (var box in storageBoxes)
        {
            Destroy(box.gameObject);
        }
    }
}