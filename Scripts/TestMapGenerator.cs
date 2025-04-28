using UnityEngine;
using System.Collections.Generic;

public class TestMapGenerator : MonoBehaviour
{
    [Header("Building Prefabs")]
    public GameObject minerPrefab;
    public GameObject conveyorBeltPrefab;
    public GameObject connectorPrefab;
    public GameObject storageBoxPrefab;
    
    [Header("Grid Settings")]
    public CustomGridRenderer gridManager;
    public int mapWidth = 20;
    public int mapHeight = 20;
    
    // Reference to the building manager
    private BuildingManager buildingManager;
    
    // Track created objects for cleanup
    private List<GameObject> createdObjects = new List<GameObject>();
    
    private void Start()
    {
        buildingManager = FindObjectOfType<BuildingManager>();
        if (buildingManager == null)
        {
            Debug.LogError("BuildingManager not found in scene!");
            return;
        }
        
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<CustomGridRenderer>();
            if (gridManager == null)
            {
                Debug.LogError("GridManager not found in scene!");
                return;
            }
        }
    }
    
    [ContextMenu("Generate Test Map")]
    public void GenerateTestMap()
    {
        // Clear any existing objects
        ClearMap();
        
        // Create mining setups for each resource type
        CreateMiningSetups();
    }
    
    private void ClearMap()
    {
        foreach (GameObject obj in createdObjects)
        {
            if (obj != null)
                DestroyImmediate(obj);
        }
        createdObjects.Clear();
    }
    
    private void CreateMiningSetups()
    {
        // Find one tile of each resource type to place a miner on
        // Based on CustomGridRenderer.cs, resource types are:
        // 1: Coal, 2: Iron, 3: Copper, 4: Stone
        for (int resourceType = 1; resourceType <= 4; resourceType++)
        {
            GridTile resourceTile = FindTileWithResource(resourceType);
            if (resourceTile == null)
            {
                Debug.LogWarning($"Could not find a tile with resource type {resourceType}");
                continue;
            }
            
            // Create a miner on this tile
            GameObject miner = CreateBuilding(minerPrefab, resourceTile);
            
            // Create a path from the miner to a storage box
            CreateConveyorPath(miner, resourceType);
        }
    }
    
    private GridTile FindTileWithResource(int resourceType)
    {
        // Find a tile with the specified resource type
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                GridTile tile = gridManager.GetTileAt(x, z);
                if (tile != null)
                {
                    GridComputeManager.GridCell cellData = tile.GetCellData();
                    if (cellData.resourceType == resourceType && cellData.resourceAmount > 0)
                    {
                        return tile;
                    }
                }
            }
        }
        return null;
    }
    
    private GameObject CreateBuilding(GameObject prefab, GridTile tile)
    {
        // Instantiate the building
        GameObject building = Instantiate(prefab, tile.transform.position, Quaternion.identity);
        building.transform.position = new Vector3(
            tile.transform.position.x,
            tile.transform.position.y + 0.5f, // Adjust height as needed
            tile.transform.position.z
        );
        
        // Add to our list for cleanup
        createdObjects.Add(building);
        
        return building;
    }
    
    private void CreateConveyorPath(GameObject miner, int resourceType)
    {
        // Get the miner's position
        Vector3 minerPos = miner.transform.position;
        int minerX = Mathf.RoundToInt(minerPos.x);
        int minerZ = Mathf.RoundToInt(minerPos.z);
        
        // Create a connector next to the miner
        GridTile connectorTile = gridManager.GetTileAt(minerX + 1, minerZ);
        if (connectorTile == null)
            return;
            
        GameObject connector = CreateBuilding(connectorPrefab, connectorTile);
        
        // Connect the miner to the connector
        ConveyorConnector inputConnector = connector.GetComponentInChildren<ConveyorConnector>();
        if (inputConnector != null)
        {
            inputConnector.ConnectToBuilding(miner.GetComponent<MinerBuilding>());
        }
        
        // Create a path of conveyor belts
        int pathLength = Random.Range(5, 10);
        int currentX = minerX + 2; // Start one tile after the connector
        int currentZ = minerZ;
        
        GameObject previousBelt = null;
        
        for (int i = 0; i < pathLength; i++)
        {
            // Decide direction (0 = straight, 1 = turn)
            int direction = (i > 0 && i < pathLength - 1) ? Random.Range(0, 2) : 0;
            
            if (direction == 1)
            {
                // Turn (50% chance up, 50% chance down)
                int turn = Random.Range(0, 2) == 0 ? 1 : -1;
                currentZ += turn;
            }
            else
            {
                // Straight
                currentX += 1;
            }
            
            // Make sure we're still on the grid
            if (currentX >= mapWidth || currentZ < 0 || currentZ >= mapHeight)
                break;
                
            // Get the tile at this position
            GridTile beltTile = gridManager.GetTileAt(currentX, currentZ);
            if (beltTile == null)
                break;
                
            // Create a conveyor belt
            GameObject belt = CreateBuilding(conveyorBeltPrefab, beltTile);
            
            // Connect to previous belt if needed
            if (previousBelt != null)
            {
                ConveyorBelt prevBeltComponent = previousBelt.GetComponent<ConveyorBelt>();
                ConveyorBelt currentBeltComponent = belt.GetComponent<ConveyorBelt>();
                
                if (prevBeltComponent != null && currentBeltComponent != null)
                {
                    prevBeltComponent.ConnectToConveyor(currentBeltComponent, ConveyorConnectionType.Output);
                }
            }
            
            previousBelt = belt;
        }
        
        // Create a connector and storage box at the end of the path
        if (currentX + 1 < mapWidth)
        {
            // Create the output connector
            GridTile outputConnectorTile = gridManager.GetTileAt(currentX + 1, currentZ);
            if (outputConnectorTile != null)
            {
                GameObject outputConnector = CreateBuilding(connectorPrefab, outputConnectorTile);
                
                // Connect the last belt to the connector
                if (previousBelt != null)
                {
                    ConveyorBelt lastBeltComponent = previousBelt.GetComponent<ConveyorBelt>();
                    ConveyorConnector outputConnectorComponent = outputConnector.GetComponentInChildren<ConveyorConnector>();
                    
                    if (lastBeltComponent != null && outputConnectorComponent != null)
                    {
                        lastBeltComponent.ConnectToConveyor(outputConnectorComponent);
                    }
                }
                
                // Create the storage box
                if (currentX + 2 < mapWidth)
                {
                    GridTile storageTile = gridManager.GetTileAt(currentX + 2, currentZ);
                    if (storageTile != null)
                    {
                        GameObject storage = CreateBuilding(storageBoxPrefab, storageTile);
                        
                        // Connect the connector to the storage box
                        ConveyorConnector storageConnector = outputConnector.GetComponentInChildren<ConveyorConnector>();
                        if (storageConnector != null)
                        {
                            storageConnector.ConnectToBuilding(storage.GetComponent<StorageBox>());
                        }
                    }
                }
            }
        }
    }
}