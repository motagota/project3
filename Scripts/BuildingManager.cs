using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Prefabs")]
    public GameObject minerPrefab;
    public GameObject storageBoxPrefab;
    public GameObject conveyorBeltPrefab; // New prefab for conveyor belt
    public GameObject conveyorConnectorPrefab; // New prefab for conveyor connector
    
    [Header("UI References")]
    public GameObject toolbar;
    public Button minerButton;
    public Button storageBoxButton;
    public Button conveyorBeltButton; // New button for conveyor belt
    public Button connectorButton; // New button for connector
    
    [Header("Placement Settings")]
    public Material ghostMaterial; 
    public Color validPlacementColor = new Color(0, 1, 0, 0.5f); 
    public Color invalidPlacementColor = new Color(1, 0, 0, 0.5f); 
    public float buildingHeight = 0.1f; 
    public LayerMask gridTileLayer;
    public LayerMask buildingLayer; // For detecting existing buildings
    
    private GameObject currentGhost; 
    private BuildingType selectedBuilding = BuildingType.None;
    private float currentRotation = 0f;
    private bool canPlace = false;
    
    // For conveyor belt placement
    private GridTile lastHoveredTile;
    private ConveyorBelt lastPlacedConveyor;
    
    // Enum to track building types
    public enum BuildingType
    {
        None,
        Miner,
        StorageBox,
        ConveyorBelt, // New building type
        Connector     // New building type
    }
    
    private void Start()
    {
        // Set up button listeners
        if (minerButton != null)
        {
            minerButton.onClick.AddListener(() => SelectBuilding(BuildingType.Miner));
        }
        
        // Set up storage box button listener
        if (storageBoxButton != null)
        {
            storageBoxButton.onClick.AddListener(() => SelectBuilding(BuildingType.StorageBox));
        }
        
        // Set up conveyor belt button listener
        if (conveyorBeltButton != null)
        {
            conveyorBeltButton.onClick.AddListener(() => SelectBuilding(BuildingType.ConveyorBelt));
        }
        
        // Set up connector button listener
        if (connectorButton != null)
        {
            connectorButton.onClick.AddListener(() => SelectBuilding(BuildingType.Connector));
        }
        
        // If gridTileLayer is not set, try to find the layer by name
        if (gridTileLayer.value == 0)
        {
            gridTileLayer = 1 << LayerMask.NameToLayer("GridTile");
            
            // If the layer doesn't exist, log a warning
            if (gridTileLayer.value == 0)
            {
                Debug.LogWarning("GridTile layer not found. Please create a layer named 'GridTile' and assign it to all grid tiles.");
            }
        }
    }
    
    private void Update()
    {
        if (selectedBuilding != BuildingType.None)
        {
            HandlePlacementPreview();
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateGhost();
            }
         
            if (Input.GetMouseButtonDown(0) && canPlace) 
            {
                PlaceBuilding();
            }
            else if (Input.GetMouseButtonDown(1))  
            {
                CancelPlacement();
            }
        }
    }
    
    public void SelectBuilding(BuildingType type)
    {
        selectedBuilding = type;
        
        if (currentGhost != null)
        {
            Destroy(currentGhost);
        }
        
        switch (type)
        {
            case BuildingType.Miner:
                currentGhost = CreateGhost(minerPrefab);
                break;
            case BuildingType.StorageBox:
                currentGhost = CreateGhost(storageBoxPrefab);
                break;
            case BuildingType.ConveyorBelt:
                currentGhost = CreateGhost(conveyorBeltPrefab);
                break;
            case BuildingType.Connector:
                currentGhost = CreateGhost(conveyorConnectorPrefab);
                break;
        }        
       
        currentRotation = 0f;
        if (currentGhost != null)
        {
            currentGhost.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
        
        // Reset tracking variables
        lastHoveredTile = null;
        lastPlacedConveyor = null;
    }
    
    private GameObject CreateGhost(GameObject prefab)
    {
        var ghost = Instantiate(prefab);
        ghost.name = $"{prefab.name}_Ghost";
        ghost.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        foreach(var collider in ghost.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        ApplyGhostMaterial(ghost);
        return ghost;
    }
    
    private void HandlePlacementPreview()
    {
        // Cast ray from mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Use the gridTileLayer mask to only hit grid tiles
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridTileLayer))
        {
            // Check if we hit a grid tile
            GridTile tile = hit.collider.GetComponent<GridTile>();
            if (tile != null)
            {
                lastHoveredTile = tile;
                
                // Position ghost on the tile
                Vector3 position = hit.collider.transform.position;
                position.y += buildingHeight; // Raise slightly above ground
                currentGhost.transform.position = position;
                
                // Check if placement is valid
                canPlace = IsValidPlacement(tile);
                
                // Update ghost color based on placement validity
                UpdateGhostColor(canPlace);
            }
        }
    }
    
    private bool IsValidPlacement(GridTile tile)
    {       
        GridComputeManager.GridCell cellData = tile.GetCellData();
        
        // Check if there's already a building on this tile
        Collider[] hitColliders = Physics.OverlapSphere(tile.transform.position, 0.4f, buildingLayer);
        bool hasBuildingOnTile = hitColliders.Length > 0;
        
        // For miners, check if the tile has a resource
        if (selectedBuilding == BuildingType.Miner)
        {
            return cellData.resourceType > 0 && cellData.resourceAmount > 0 && !hasBuildingOnTile;
        }
        // For storage boxes, check if the tile is empty (no resource)
        else if (selectedBuilding == BuildingType.StorageBox)
        {
            // Storage boxes can be placed on any tile without resources
            return (cellData.resourceType == 0 || cellData.resourceAmount <= 0) && !hasBuildingOnTile;
        }
        // For conveyor belts, check if the tile doesn't have a building
        else if (selectedBuilding == BuildingType.ConveyorBelt)
        {
            return !hasBuildingOnTile;
        }
        // For connectors, check if there's a building nearby to connect to
        else if (selectedBuilding == BuildingType.Connector)
        {
            // Check for nearby buildings (miners or storage boxes)
            Collider[] nearbyObjects = Physics.OverlapSphere(tile.transform.position, 1.5f, buildingLayer);
            bool hasNearbyBuilding = false;
            bool hasNearbyConveyor = false;
            
            foreach (var collider in nearbyObjects)
            {
                // Check for buildings
                if (collider.GetComponent<MinerBuilding>() != null || 
                    collider.GetComponent<StorageBox>() != null)
                {
                    hasNearbyBuilding = true;
                }
                
                // Check for conveyor belts
                if (collider.GetComponent<ConveyorBelt>() != null)
                {
                    hasNearbyConveyor = true;
                }
                
                // If we found both, we can stop checking
                if (hasNearbyBuilding && hasNearbyConveyor)
                {
                    break;
                }
            }
            
            // Valid if: no building on this tile AND (has nearby building OR has nearby conveyor)
            return !hasBuildingOnTile && (hasNearbyBuilding || hasNearbyConveyor);
        }
        
        return false;
    }
    
    private void ApplyGhostMaterial(GameObject ghost)
    {
        foreach (var renderer in ghost.GetComponentsInChildren<Renderer>())
        {
            var materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = ghostMaterial;
            }
            renderer.materials = materials;
        }
    }

    private void UpdateGhostColor(bool isValid)
    {
        if (currentGhost == null) return;

        Color targetColor = isValid ? validPlacementColor : invalidPlacementColor;
        foreach (var renderer in currentGhost.GetComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                material.color = targetColor;
            }
        }
    }
    
    private void RotateGhost()
    {
        if (currentGhost == null) return;

        // Rotate by 90 degrees
        currentRotation = (currentRotation + 90) % 360;
        currentGhost.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }
    
    // Add this method to connect buildings to nearby connectors
    private void ConnectToNearbyConnectors(GameObject building, Vector3 position)
    {
        // Find all connectors within a certain radius
        Collider[] colliders = Physics.OverlapSphere(position, 2f, buildingLayer);
        
        foreach (var collider in colliders)
        {
            ConveyorConnector connector = collider.GetComponent<ConveyorConnector>();
            if (connector != null)
            {
                // Connect the building to the connector
                connector.ConnectToBuilding(building.GetComponent<MonoBehaviour>());
                
                // If this is a conveyor belt, also connect the connector to the conveyor
                ConveyorBelt conveyor = building.GetComponent<ConveyorBelt>();
                if (conveyor != null)
                {
                    connector.ConnectToConveyor(conveyor);
                }
            }
        }
    }
    
    // Modify your PlaceBuilding method to call the new connection method
    private void PlaceBuilding()
    {
        if (currentGhost == null) return;

        GameObject building = null;
        
        switch (selectedBuilding)
        {
            case BuildingType.Miner:
                building = Instantiate(minerPrefab, currentGhost.transform.position, currentGhost.transform.rotation);
                break;
            case BuildingType.StorageBox:
                building = Instantiate(storageBoxPrefab, currentGhost.transform.position, currentGhost.transform.rotation);
                break;
            case BuildingType.ConveyorBelt:
                building = Instantiate(conveyorBeltPrefab, currentGhost.transform.position, currentGhost.transform.rotation);
                
                // Set up the conveyor belt
                ConveyorBelt newConveyor = building.GetComponent<ConveyorBelt>();
                if (newConveyor != null)
                {
                    newConveyor.direction = (int)(currentRotation / 90) % 4;
                    
                    // Connect to adjacent conveyors if any
                    ConnectToAdjacentConveyors(newConveyor);
                    
                    // Remember this conveyor for potential future connections
                    lastPlacedConveyor = newConveyor;
                }
                break;
            case BuildingType.Connector:
                building = Instantiate(conveyorConnectorPrefab, currentGhost.transform.position, currentGhost.transform.rotation);
                
                // Set up the connector
                ConveyorConnector newConnector = building.GetComponent<ConveyorConnector>();
                if (newConnector != null)
                {
                    // Find nearby buildings to connect to
                    ConnectConnectorToNearbyBuildings(newConnector, building.transform.position);
                }
                break;
        }
        
        // Connect the new building to any nearby connectors
        if (building != null)
        {
            ConnectToNearbyConnectors(building, building.transform.position);
        }
        
        building.name = selectedBuilding.ToString();
        
        // Get the tile under the building
        Ray ray = new Ray(building.transform.position + Vector3.up, Vector3.down);
        RaycastHit hit;
        
        // Use the gridTileLayer mask to only hit grid tiles
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridTileLayer))
        {
            GridTile tile = hit.collider.GetComponent<GridTile>();
            if (tile != null)
            {
                // Link building to tile based on type
                if (selectedBuilding == BuildingType.Miner)
                {
                    MinerBuilding miner = building.GetComponent<MinerBuilding>();
                    if (miner != null)
                    {
                        miner.Initialize(tile);
                        Debug.Log($"Miner initialized with tile at position {tile.transform.position}");
                    }
                }
                else if (selectedBuilding == BuildingType.StorageBox)
                {
                    StorageBox storage = building.GetComponent<StorageBox>();
                    if (storage != null)
                    {
                        storage.Initialize(tile);
                        Debug.Log($"Storage box initialized with tile at position {tile.transform.position}");
                    }
                }
            }
            else
            {
                Debug.LogError("Hit object doesn't have a GridTile component!");
            }
        }
        else
        {
            Debug.LogError("Failed to find a grid tile under the building!");
        }
    }
    
    private void ConnectToAdjacentConveyors(ConveyorBelt newConveyor)
    {
        // Check for adjacent conveyor belts in all four directions
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward, // North
            Vector3.right,   // East
            Vector3.back,    // South
            Vector3.left     // West
        };
        
        foreach (Vector3 dir in directions)
        {
            // Cast a ray in this direction to find adjacent conveyors
            Ray ray = new Ray(newConveyor.transform.position, dir);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 1.5f, buildingLayer))
            {
                ConveyorBelt adjacentConveyor = hit.collider.GetComponent<ConveyorBelt>();
                if (adjacentConveyor != null)
                {
                    // Determine connection type based on relative positions and rotations
                    DetermineAndCreateConnection(newConveyor, adjacentConveyor, dir);
                }
            }
        }
    }
    
    private void DetermineAndCreateConnection(ConveyorBelt conveyor1, ConveyorBelt conveyor2, Vector3 direction)
    {
        // Get the forward direction of both conveyors
        Vector3 forward1 = conveyor1.transform.forward;
        Vector3 forward2 = conveyor2.transform.forward;
        
        // Normalize direction for comparison
        direction = direction.normalized;
        
        // Check if conveyor1 is pointing toward conveyor2
        if (Vector3.Dot(forward1, direction) > 0.7f)
        {
            // conveyor1 outputs to conveyor2
            conveyor1.ConnectToConveyor(conveyor2, ConveyorConnectionType.Next);
            Debug.Log("Connected as Next");
        }
        // Check if conveyor2 is pointing toward conveyor1
        else if (Vector3.Dot(forward2, -direction) > 0.7f)
        {
            // conveyor2 outputs to conveyor1
            conveyor1.ConnectToConveyor(conveyor2, ConveyorConnectionType.Previous);
            Debug.Log("Connected as Previous");
        }
        // Check for side connections
        else
        {
            // Determine if it's a left or right side connection
            Vector3 right1 = conveyor1.transform.right;
            
            if (Vector3.Dot(right1, direction) > 0.7f)
            {
                // conveyor2 is to the right of conveyor1
                conveyor1.ConnectToConveyor(conveyor2, ConveyorConnectionType.RightSide);
                Debug.Log("Connected as RightSide");
            }
            else if (Vector3.Dot(right1, direction) < -0.7f)
            {
                // conveyor2 is to the left of conveyor1
                conveyor1.ConnectToConveyor(conveyor2, ConveyorConnectionType.LeftSide);
                Debug.Log("Connected as LeftSide");
            }
        }
    }
    
    private void ConnectConnectorToNearbyBuildings(ConveyorConnector connector, Vector3 position)
    {
        // Find all buildings within a certain radius
        Collider[] colliders = Physics.OverlapSphere(position, 2f, buildingLayer);
        
        foreach (var collider in colliders)
        {
            // Skip the connector itself
            if (collider.gameObject == connector.gameObject)
                continue;
                
            // Check for conveyor belts
            ConveyorBelt conveyor = collider.GetComponent<ConveyorBelt>();
            if (conveyor != null)
            {
                connector.ConnectToConveyor(conveyor);
                conveyor.RegisterConnector(connector); // New line
                Debug.Log("Connector connected to conveyor belt");
                continue; // Skip to next collider after connecting
            }
            
            // Check for other buildings (miners, storage boxes, etc.)
            MinerBuilding miner = collider.GetComponent<MinerBuilding>();
            if (miner != null)
            {
                connector.ConnectToBuilding(miner);
                Debug.Log("Connector connected to miner");
                continue;
            }
            
            StorageBox storage = collider.GetComponent<StorageBox>();
            if (storage != null)
            {
                connector.ConnectToBuilding(storage);
                Debug.Log("Connector connected to storage box");
                continue;
            }
        }
    }

    private void ConnectToNearbyConveyor(ConveyorConnector connector)
    {
        // Find nearby conveyor belts
        Collider[] nearbyConveyors = Physics.OverlapSphere(connector.transform.position, 1.0f, buildingLayer);
        
        foreach (var collider in nearbyConveyors)
        {
            ConveyorBelt conveyor = collider.GetComponent<ConveyorBelt>();
            if (conveyor != null)
            {
                connector.ConnectToNearbyConveyor(conveyor);
                conveyor.RegisterConnector(connector); // New line
                Debug.Log("Connector attached to conveyor belt");
                break;
            }
        }
    }
    
    private void CancelPlacement()
    {
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            currentGhost = null;
        }
        
        selectedBuilding = BuildingType.None;
    }
}