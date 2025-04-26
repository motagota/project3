using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Prefabs")]
     public GameObject minerPrefab;
    public GameObject storageBoxPrefab; // New prefab for storage box
    
    [Header("UI References")]
    public GameObject toolbar;
    public Button minerButton;
    public Button storageBoxButton; // New button for storage box
    
    [Header("Placement Settings")]
    public Material ghostMaterial; 
    public Color validPlacementColor = new Color(0, 1, 0, 0.5f); 
    public Color invalidPlacementColor = new Color(1, 0, 0, 0.5f); 
    public float buildingHeight = 0.1f; 
    public LayerMask gridTileLayer;
    
    private GameObject currentGhost; 
    private BuildingType selectedBuilding = BuildingType.None;
    private float currentRotation = 0f;
    private bool canPlace = false;
    
    // Enum to track building types
    public enum BuildingType
    {
        None,
        Miner,
        StorageBox // New building type
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
        }        
       
        currentRotation = 0f;
        if (currentGhost != null)
        {
            currentGhost.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
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
        
        // For miners, check if the tile has a resource
        if (selectedBuilding == BuildingType.Miner)
        {
            return cellData.resourceType > 0 && cellData.resourceAmount > 0;
        }
        // For storage boxes, check if the tile is empty (no resource)
        else if (selectedBuilding == BuildingType.StorageBox)
        {
            // Storage boxes can be placed on any tile without resources
            return cellData.resourceType == 0 || cellData.resourceAmount <= 0;
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
        }
        
        if (building != null)
        {
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
        
        // Reset selection
        CancelPlacement();
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