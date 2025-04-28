using UnityEngine;
using System.Collections;
using System;

public class MinerBuilding : MonoBehaviour
{
    [Header("Mining Settings")]
    public float miningRate = 0.1f; // Resource units per second
    public float resourceCapacity = 10f; // Maximum storage
    
    [Header("Visual Feedback")]
    public GameObject miningEffect; // Optional particle effect
    public GameObject activeLightIndicator; // Optional visual indicator for active state
    
    private GridTile _targetTile;
    private int ResourceType { get; set; }
    private int StoredResources { get; set; } = 0;
    private bool _isOperating = false;
    
    // For tracking partial resources until we have a whole unit
    private float _partialResources = 0f;
    
    // UI interaction
    private bool _showUI = false;
    private Rect _windowRect = new Rect(20, 20, 300, 280); // Increased height for new button
    private int _windowID = 0;
    
    public void Initialize(GridTile tile)
    {
        if (tile == null)
        {
            Debug.LogError("Attempted to initialize MinerBuilding with null tile!");
            return;
        }
        
        _targetTile = tile;
        
        try
        {
            GridComputeManager.GridCell cellData = tile.GetCellData();
            ResourceType = cellData.resourceType;
            _windowID = GetInstanceID();
            
            // Start mining if there's a valid resource
            if (ResourceType > 0 && cellData.resourceAmount > 0)
            {
                // Auto-start is now optional - we'll let the player decide
                // StartMining();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing MinerBuilding: {e.Message}");
        }
    }
    
    public void StartMining()
    {
        if (_isOperating)
            return;
            
        _isOperating = true;
        
        // Enable mining effect if available
        if (miningEffect != null)
        {
            miningEffect.SetActive(true);
        }
        
        // Enable active indicator if available
        if (activeLightIndicator != null)
        {
            activeLightIndicator.SetActive(true);
        }
        
        // Start mining coroutine
        StartCoroutine(MiningProcess());
    }
    
    public void StopMining()
    {
        if (!_isOperating)
            return;
            
        _isOperating = false;
        
        // Disable mining effect if available
        if (miningEffect != null)
        {
            miningEffect.SetActive(false);
        }
        
        // Disable active indicator if available
        if (activeLightIndicator != null)
        {
            activeLightIndicator.SetActive(false);
        }
        
        // The coroutine will stop on its own since isOperating is now false
    }
    
    public event Action<MinerBuilding> OnDepleted;
    
    private IEnumerator MiningProcess()
    {
        while (_isOperating)
        {
            // Check if we have capacity
            if (StoredResources < resourceCapacity)
            {
                // Mine resources
                float mineAmount = miningRate * Time.deltaTime;
                
                // Check if the tile still has resources
                if (_targetTile != null)
                {
                    try
                    {
                        GridComputeManager.GridCell cellData = _targetTile.GetCellData();
                        
                        if (cellData.resourceAmount > 0)
                        {
                            // Add to partial resources
                            _partialResources += mineAmount;
                            
                            // When we have at least 1 whole unit, add it to stored resources
                            if (_partialResources >= 1.0f)
                            {
                                int wholeUnits = Mathf.FloorToInt(_partialResources);
                                StoredResources += wholeUnits;
                                _partialResources -= wholeUnits;
                                
                                // Clamp to capacity
                                StoredResources = Mathf.Min(StoredResources, Mathf.FloorToInt(resourceCapacity));
                            }
                        }
                        else
                        {
                            // Resource depleted
                            StopMining();
                            
                            // Notify listeners
                            OnDepleted?.Invoke(this);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in mining process: {e.Message}");
                        StopMining();
                    }
                }
                else
                {
                    Debug.LogError("Mining attempted on null target tile! Stopping miner.");
                    StopMining();
                }
            }
            else
            {
                // Storage full - optionally auto-stop mining
                // Uncomment the next line if you want miners to auto-stop when full
                // StopMining();
            }
            
            yield return null;
        }
    }
    
    // UI interaction methods
    private void OnMouseDown()
    {
        _showUI = !_showUI;
    }
    
    private void OnGUI()
    {
        if (_showUI)
        {
            _windowRect = GUI.Window(_windowID, _windowRect, DrawMinerWindow, "Miner Control");
        }
    }
    
    private void DrawMinerWindow(int id)
    {
        // Check if target tile is valid
        if (_targetTile == null)
        {
            GUILayout.Label("ERROR: No valid tile to mine!");
            
            if (GUILayout.Button("Close"))
            {
                _showUI = false;
            }
            
            GUI.DragWindow();
            return;
        }
        
        // Display resource type and amount
        string resourceName = StorageBox.GetResourceName(ResourceType);
        GUILayout.Label($"Mining: {resourceName}");
        GUILayout.Label($"Stored: {StoredResources} / {Mathf.FloorToInt(resourceCapacity)}");
        
        // Show progress to next whole unit
        GUILayout.Label($"Progress to next unit: {_partialResources:P0}");
        
        // Status indicator
        string statusText = _isOperating ? "Status: <color=green>Active</color>" : "Status: <color=red>Inactive</color>";
        GUILayout.Label(statusText, new GUIStyle(GUI.skin.label) { richText = true });
        
        // Toggle button for active/inactive
        GUI.enabled = _targetTile != null;
        string toggleButtonText = _isOperating ? "Stop Mining" : "Start Mining";
        if (GUILayout.Button(toggleButtonText))
        {
            if (_isOperating)
                StopMining();
            else
                StartMining();
        }
        GUI.enabled = true;
        
        // Collect button
        GUI.enabled = StoredResources > 0;
        if (GUILayout.Button("Collect Resources"))
        {
            CollectResources();
        }
        GUI.enabled = true;
        
        // Close button
        if (GUILayout.Button("Close"))
        {
            _showUI = false;
        }
        
        // Make window draggable
        GUI.DragWindow();
    }
    
    private void CollectResources()
    {
        if (StoredResources > 0 && PlayerInventory.Instance != null)
        {
            // Try to add to player inventory
            if (PlayerInventory.Instance.AddResource(ResourceType, StoredResources))
            {
                // Successfully added to inventory
                StoredResources = 0;
            }
            else
            {
                // Inventory full or other issue
                Debug.Log("Could not add resources to inventory. Inventory might be full.");
            }
        }
    }
    
    // Public getter for operating state
    // Add these methods to support saving/loading
    public GridTile GetTargetTile()
    {
        return _targetTile;
    }
    
    public int GetResourceType()
    {
        return ResourceType;
    }
    
    public int GetStoredResources()
    {
        return StoredResources;
    }
    
    public float GetPartialResources()
    {
        return _partialResources;
    }
    
    public bool IsOperating()
    {
        return _isOperating;
    }
    
    public void SetStoredResources(int amount)
    {
        StoredResources = amount;
    }
    
    public void SetPartialResources(float amount)
    {
        _partialResources = amount;
    }
    
    // Add method for ConveyorConnector to get items
    public ConveyorItem GetNextItem()
    {
        if (StoredResources <= 0)
            return null;
            
        StoredResources--;
        
        // Create a new item of the appropriate resource type
        Vector3 spawnPosition = transform.position + transform.forward * 0.5f;
        return ConveyorItem.CreateItem(ResourceType, 1, spawnPosition);
    }
    
    // Add method for ConveyorConnector to return items if conveyor is full
    public void StoreItem(ConveyorItem item)
    {
        if (item != null && item.itemType == ResourceType)
        {
            StoredResources += item.quantity;
            Destroy(item.gameObject);
        }
    }
}