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
    public GameObject inputConnectorPrefab; // Input connector (building to belt)
    public GameObject outputConnectorPrefab; // Output connector (belt to building)

    [Header("UI References")]
    public GameObject toolbar;
    public Button minerButton;
    public Button storageBoxButton;
    public Button conveyorBeltButton; // New button for conveyor belt
    public Button connectorButton; // New button for connector
    public Button inputConnectorButton; // Input connector button
    public Button outputConnectorButton; // Output connector button
    
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
        ConveyorBelt,
        Connector,
        InputConnector,
        OutputConnector
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
        
        // Set up connector button listeners
        if (inputConnectorButton != null)
        {
            inputConnectorButton.onClick.AddListener(() => SelectBuilding(BuildingType.InputConnector));
        }
        
        if (outputConnectorButton != null)
        {
            outputConnectorButton.onClick.AddListener(() => SelectBuilding(BuildingType.OutputConnector));
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
                
                // If this is a connector ghost, show potential connections
                if (selectedBuilding == BuildingType.Connector && canPlace)
                {
                    PreviewConnectorConnections(currentGhost, position);
                }
            }
        }
    }
    
    private void PreviewConnectorConnections(GameObject ghostConnector, Vector3 position)
    {
        // Find all buildings within a certain radius
        Collider[] colliders = Physics.OverlapSphere(position, 2f, buildingLayer);
        
        // Get references to the input and output objects in the ghost
        Transform inputObject = ghostConnector.transform.Find("InputConnector");
        Transform outputObject = ghostConnector.transform.Find("OutputConnector");
        
        if (inputObject == null || outputObject == null)
        {
            Debug.LogWarning("Ghost connector is missing input or output objects!");
            return;
        }
        
        // Create different materials for input and output
        Material inputMaterial = new Material(ghostMaterial);
        inputMaterial.color = new Color(0, 0.8f, 0, 0.7f); // Green for input
        
        Material outputMaterial = new Material(ghostMaterial);
        outputMaterial.color = new Color(0.8f, 0, 0, 0.7f); // Red for output
        
        // Apply materials to input and output objects
        foreach (var renderer in inputObject.GetComponentsInChildren<Renderer>())
        {
            var materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = inputMaterial;
            }
            renderer.materials = materials;
        }
        
        foreach (var renderer in outputObject.GetComponentsInChildren<Renderer>())
        {
            var materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = outputMaterial;
            }
            renderer.materials = materials;
        }
        
        // Reset positions first
        inputObject.localPosition = new Vector3(-0.25f, 0, 0);
        outputObject.localPosition = new Vector3(0.25f, 0, 0);
        
        bool foundInputConnection = false;
        bool foundOutputConnection = false;
        GameObject inputConnectedObject = null;
        GameObject outputConnectedObject = null;
        
        // Create debug visualization (optional, can be removed in final version)
        GameObject leftDebug = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftDebug.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        leftDebug.transform.position = ghostConnector.transform.position - ghostConnector.transform.right * 0.5f;
        Renderer leftRenderer = leftDebug.GetComponent<Renderer>();
        leftRenderer.material = inputMaterial;
        Destroy(leftDebug, 0.1f); // Remove quickly as this is just for preview
        
        GameObject rightDebug = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightDebug.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        rightDebug.transform.position = ghostConnector.transform.position + ghostConnector.transform.right * 0.5f;
        Renderer rightRenderer = rightDebug.GetComponent<Renderer>();
        rightRenderer.material = outputMaterial;
        Destroy(rightDebug, 0.1f); // Remove quickly as this is just for preview
        
        // Define the grid size (assuming 1 unit grid)
        float gridSize = 1.0f;
        
        foreach (var collider in colliders)
        {
            // Skip the ghost itself
            if (collider.gameObject == ghostConnector)
                continue;
                
            // Get direction and distance to the collider
            Vector3 directionToCollider = collider.transform.position - ghostConnector.transform.position;
            float distance = directionToCollider.magnitude;
            
            // Check if the object is directly adjacent (using grid size with small tolerance)
            bool isDirectlyAdjacent = Mathf.Approximately(distance, gridSize) || distance < gridSize * 1.1f;
            
            // Skip if not directly adjacent
            if (!isDirectlyAdjacent)
                continue;
                
            // Now check direction
            directionToCollider = directionToCollider.normalized;
            float rightDot = Vector3.Dot(ghostConnector.transform.right, directionToCollider);
            float forwardDot = Vector3.Dot(ghostConnector.transform.forward, directionToCollider);
            
            // Check if it's aligned with right or left (not diagonal)
            bool isAlignedHorizontally = Mathf.Abs(rightDot) > 0.7f && Mathf.Abs(forwardDot) < 0.3f;
            
            // Skip if not aligned horizontally (diagonal connections not allowed)
            if (!isAlignedHorizontally)
                continue;
                
            bool isInRightDirection = rightDot > 0.7f; // Object is to the right
            bool isInLeftDirection = rightDot < -0.7f; // Object is to the left
            
            // Check for conveyor belts
            ConveyorBelt conveyor = collider.GetComponent<ConveyorBelt>();
            if (conveyor != null)
            {
                // For input connections (left side)
                if (isInLeftDirection && !foundInputConnection)
                {
                    foundInputConnection = true;
                    inputConnectedObject = collider.gameObject;
                }
                // For output connections (right side)
                else if (isInRightDirection && !foundOutputConnection)
                {
                    foundOutputConnection = true;
                    outputConnectedObject = collider.gameObject;
                }
                continue;
            }
            
            // Check for miners
            MinerBuilding miner = collider.GetComponent<MinerBuilding>();
            if (miner != null)
            {
                // For input connections (left side)
                if (isInLeftDirection && !foundInputConnection)
                {
                    foundInputConnection = true;
                    inputConnectedObject = collider.gameObject;
                }
                // For output connections (right side)
                else if (isInRightDirection && !foundOutputConnection)
                {
                    foundOutputConnection = true;
                    outputConnectedObject = collider.gameObject;
                }
                continue;
            }
            
            // Check for storage boxes
            StorageBox storage = collider.GetComponent<StorageBox>();
            if (storage != null)
            {
                // For input connections (left side)
                if (isInLeftDirection && !foundInputConnection)
                {
                    foundInputConnection = true;
                    inputConnectedObject = collider.gameObject;
                }
                // For output connections (right side)
                else if (isInRightDirection && !foundOutputConnection)
                {
                    foundOutputConnection = true;
                    outputConnectedObject = collider.gameObject;
                }
                continue;
            }
        }
        
        // Position the input object on the connected object if found
        if (foundInputConnection && inputConnectedObject != null)
        {
            // Position the input object on top of the connected object
            Vector3 newPosition = inputConnectedObject.transform.position;
            newPosition.y += 0.1f; // Slight height offset
            
            // Move the input object to the new position
            inputObject.position = newPosition;
            Debug.Log($"Positioned input object on connected object at {newPosition}");
        }
        else
        {
            // Reset input object to its default position
            inputObject.localPosition = new Vector3(-0.25f, 0, 0); // Default offset
            Debug.Log("Reset input object to default position");
        }
        
        // Position the output object on the connected object if found
        if (foundOutputConnection && outputConnectedObject != null)
        {
            // Position the output object on top of the connected object
            Vector3 newPosition = outputConnectedObject.transform.position;
            newPosition.y += 0.1f; // Slight height offset
            
            // Move the output object to the new position
            outputObject.position = newPosition;
            Debug.Log($"Positioned output object on connected object at {newPosition}");
        }
        else
        {
            // Reset output object to its default position
            outputObject.localPosition = new Vector3(0.25f, 0, 0); // Default offset
            Debug.Log("Reset output object to default position");
        }
    }
    
    // Helper method to visualize connections with a line
    private void HighlightConnection(Vector3 start, Vector3 end, Color color)
    {
        GameObject line = new GameObject("ConnectionLine");
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        Destroy(line, 0.1f); // Remove quickly as this is just for preview
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
            // First check if the tile is empty
            if (hasBuildingOnTile)
                return false;
                
            // Check for nearby buildings that could connect to input and output
            Collider[] nearbyObjects = Physics.OverlapSphere(tile.transform.position, 1.5f, buildingLayer);
            
            bool hasInputConnection = false;
            bool hasOutputConnection = false;
            
            // Create a temporary ghost to check connections
            GameObject tempGhost = null;
            if (currentGhost != null)
            {
                tempGhost = currentGhost;
            }
            else
            {
                // If no ghost exists (shouldn't happen), create one temporarily
                tempGhost = CreateGhost(conveyorConnectorPrefab);
                tempGhost.transform.position = tile.transform.position;
                tempGhost.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
            }
            
            // Check each nearby object for potential connections
            foreach (var collider in nearbyObjects)
            {
                // Skip the ghost itself
                if (collider.gameObject == tempGhost)
                    continue;
                        
                // Get direction and distance to the collider
                Vector3 directionToCollider = collider.transform.position - tempGhost.transform.position;
                float distance = directionToCollider.magnitude;
                
                // Check if the object is directly adjacent (using grid size with small tolerance)
                bool isDirectlyAdjacent = Mathf.Approximately(distance, 1.0f) || distance < 1.1f;
                
                // Skip if not directly adjacent
                if (!isDirectlyAdjacent)
                    continue;
                    
                // Now check direction
                directionToCollider = directionToCollider.normalized;
                float rightDot = Vector3.Dot(tempGhost.transform.right, directionToCollider);
                float forwardDot = Vector3.Dot(tempGhost.transform.forward, directionToCollider);
                
                // Check if it's aligned with right or left (not diagonal)
                bool isAlignedHorizontally = Mathf.Abs(rightDot) > 0.7f && Mathf.Abs(forwardDot) < 0.3f;
                
                // Skip if not aligned horizontally (diagonal connections not allowed)
                if (!isAlignedHorizontally)
                    continue;
                    
                bool isInRightDirection = rightDot > 0.7f; // Object is to the right
                bool isInLeftDirection = rightDot < -0.7f; // Object is to the left
                
                // Check for miners - they can only output items, not receive them
                MinerBuilding miner = collider.GetComponent<MinerBuilding>();
                if (miner != null)
                {
                    // Miners can only be on the left side (as output to connector)
                    if (isInLeftDirection)
                    {
                        hasInputConnection = true;
                    }
                    // Miners cannot be on the right side (cannot receive items)
                    continue;
                }
                
                // Check for other valid buildings
                bool isStorageBox = collider.GetComponent<StorageBox>() != null;
                bool isConveyor = collider.GetComponent<ConveyorBelt>() != null;
                
                if (isStorageBox || isConveyor)
                {
                    if (isInLeftDirection)
                    {
                        hasInputConnection = true;
                    }
                    
                    if (isInRightDirection)
                    {
                        hasOutputConnection = true;
                    }
                }
            }
            
            // If we created a temporary ghost, destroy it
            if (tempGhost != currentGhost)
            {
                Destroy(tempGhost);
            }
            
            // Valid only if both input and output connections are available
            return hasInputConnection && hasOutputConnection;
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
        
        // Get references to the input and output objects (assuming they are child objects)
        Transform inputObject = connector.transform.Find("InputConnector");
        Transform outputObject = connector.transform.Find("OutputConnector");
        
        if (inputObject == null || outputObject == null)
        {
            Debug.LogError("Connector is missing input or output objects!");
            return;
        }
        
        bool foundInputConnection = false;
        bool foundOutputConnection = false;
        GameObject inputConnectedObject = null;
        GameObject outputConnectedObject = null;
        
        foreach (var collider in colliders)
        {
            // Skip the connector itself
            if (collider.gameObject == connector.gameObject)
                continue;
                
            // Get direction to the collider
            Vector3 directionToCollider = (collider.transform.position - connector.transform.position).normalized;
            float rightDot = Vector3.Dot(connector.transform.right, directionToCollider);
            bool isInRightDirection = rightDot > 0.5f; // Object is to the right
            bool isInLeftDirection = rightDot < -0.5f; // Object is to the left
            
            // Check for conveyor belts
            ConveyorBelt conveyor = collider.GetComponent<ConveyorBelt>();
            if (conveyor != null)
            {
                // For input connections (left side)
                if (isInLeftDirection && !foundInputConnection)
                {
                    connector.ConnectToConveyor(conveyor);
                    conveyor.RegisterConnector(connector);
                    Debug.Log("Input connector connected to conveyor belt on left side");
                    
                    foundInputConnection = true;
                    inputConnectedObject = collider.gameObject;
                }
                // For output connections (right side)
                else if (isInRightDirection && !foundOutputConnection)
                {
                    connector.ConnectToConveyor(conveyor);
                    conveyor.RegisterConnector(connector);
                    Debug.Log("Output connector connected to conveyor belt on right side");
                    
                    foundOutputConnection = true;
                    outputConnectedObject = collider.gameObject;
                }
                continue;
            }
            
            // Check for miners
            MinerBuilding miner = collider.GetComponent<MinerBuilding>();
            if (miner != null)
            {
                // For input connections (left side)
                if (isInLeftDirection && !foundInputConnection)
                {
                    connector.ConnectToBuilding(miner);
                    Debug.Log("Input connector connected to miner on left side");
                    
                    foundInputConnection = true;
                    inputConnectedObject = collider.gameObject;
                }
                // For output connections (right side)
                else if (isInRightDirection && !foundOutputConnection)
                {
                    connector.ConnectToBuilding(miner);
                    Debug.Log("Output connector connected to miner on right side");
                    
                    foundOutputConnection = true;
                    outputConnectedObject = collider.gameObject;
                }
                continue;
            }
            
            // Check for storage boxes
            StorageBox storage = collider.GetComponent<StorageBox>();
            if (storage != null)
            {
                // For input connections (left side)
                if (isInLeftDirection && !foundInputConnection)
                {
                    connector.ConnectToBuilding(storage);
                    Debug.Log("Input connector connected to storage box on left side");
                    
                    foundInputConnection = true;
                    inputConnectedObject = collider.gameObject;
                }
                // For output connections (right side)
                else if (isInRightDirection && !foundOutputConnection)
                {
                    connector.ConnectToBuilding(storage);
                    Debug.Log("Output connector connected to storage box on right side");
                    
                    foundOutputConnection = true;
                    outputConnectedObject = collider.gameObject;
                }
                continue;
            }
        }
        
        // Position the input object on the connected object if found
        if (foundInputConnection && inputConnectedObject != null)
        {
            // Position the input object on top of the connected object
            Vector3 newPosition = inputConnectedObject.transform.position;
            newPosition.y += 0.1f; // Slight height offset
            
            // Move the input object to the new position
            inputObject.position = newPosition;
            Debug.Log($"Positioned input object on connected object at {newPosition}");
        }
        else
        {
            // Reset input object to its default position
            inputObject.localPosition = new Vector3(-0.25f, 0, 0); // Default offset
            Debug.Log("Reset input object to default position");
        }
        
        // Position the output object on the connected object if found
        if (foundOutputConnection && outputConnectedObject != null)
        {
            // Position the output object on top of the connected object
            Vector3 newPosition = outputConnectedObject.transform.position;
            newPosition.y += 0.1f; // Slight height offset
            
            // Move the output object to the new position
            outputObject.position = newPosition;
            Debug.Log($"Positioned output object on connected object at {newPosition}");
        }
        else
        {
            // Reset output object to its default position
            outputObject.localPosition = new Vector3(0.25f, 0, 0); // Default offset
            Debug.Log("Reset output object to default position");
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