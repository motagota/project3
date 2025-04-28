using UnityEngine;
using System.Collections.Generic;

public class StorageBoxUI : MonoBehaviour
{
    private StorageBox _storageBox;
    private bool _showUI = false;
    private Rect _windowRect = new Rect(20, 20, 300, 400);
    private int _windowID;
    private Vector2 _scrollPosition;
    
    private void Start()
    {
        _storageBox = GetComponent<StorageBox>();
        _windowID = GetInstanceID();
    }
    
    private void OnMouseDown()
    {
        _showUI = !_showUI;
    }
    
    private void OnGUI()
    {
        if (_showUI && _storageBox != null)
        {
            _windowRect = GUI.Window(_windowID, _windowRect, DrawStorageWindow, "Storage Box");
        }
    }
    
    private void DrawStorageWindow(int id)
    {
        // Display used slots
        GUILayout.Label($"Slots: {_storageBox.GetUsedSlots()} / {_storageBox.maxStorageSlots}");
        
        // Get stored resources
        Dictionary<int, float> resources = _storageBox.GetStoredResources();
        
        // Player inventory section
        GUILayout.Label("Player Inventory:", EditorStyles.boldLabel);
        
        if (PlayerInventory.Instance != null)
        {
            Dictionary<int, float> playerInventory = PlayerInventory.Instance.GetInventory();
            
            if (playerInventory.Count == 0)
            {
                GUILayout.Label("Inventory is empty");
            }
            else
            {
                foreach (var item in playerInventory)
                {
                    GUILayout.BeginHorizontal("box");
                    
                    // Resource name and amount
                    string resourceName = StorageBox.GetResourceName(item.Key);
                    GUILayout.Label($"{resourceName}: {item.Value:F1}", GUILayout.Width(150));
                    
                    // Store button
                    if (GUILayout.Button("Store", GUILayout.Width(60)))
                    {
                        StoreResource(item.Key);
                    }
                    
                    GUILayout.EndHorizontal();
                }
            }
        }
        
        // Storage box contents section
        GUILayout.Label("Storage Contents:", EditorStyles.boldLabel);
        
        // Scrollable area for storage items
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));
        
        if (resources.Count == 0)
        {
            GUILayout.Label("Storage is empty");
        }
        else
        {
            foreach (var item in resources)
            {
                GUILayout.BeginHorizontal("box");
                
                // Resource name and amount
                string resourceName = StorageBox.GetResourceName(item.Key);
                GUILayout.Label($"{resourceName}: {item.Value:F1}", GUILayout.Width(150));
                
                // Take button
                if (GUILayout.Button("Take", GUILayout.Width(60)))
                {
                    TakeResource(item.Key);
                }
                
                GUILayout.EndHorizontal();
            }
        }
        
        GUILayout.EndScrollView();
        
        // Close button
        if (GUILayout.Button("Close"))
        {
            _showUI = false;
        }
        
        // Make window draggable
        GUI.DragWindow();
    }
    
    private void StoreResource(int resourceType)
    {
        if (PlayerInventory.Instance == null || _storageBox == null)
            return;
            
        // Get the amount from player inventory
        Dictionary<int, float> playerInventory = PlayerInventory.Instance.GetInventory();
        
        if (playerInventory.ContainsKey(resourceType))
        {
            float amount = playerInventory[resourceType];
            
            // Try to add to storage
            if (_storageBox.AddResource(resourceType, amount))
            {
                // Remove from player inventory
                PlayerInventory.Instance.RemoveResource(resourceType, amount);
            }
        }
    }
    
    private void TakeResource(int resourceType)
    {
        if (PlayerInventory.Instance == null || _storageBox == null)
            return;
            
        // Get the amount from storage
        Dictionary<int, float> resources = _storageBox.GetStoredResources();
        
        if (resources.ContainsKey(resourceType))
        {
            float amount = resources[resourceType];
            
            // Try to add to player inventory
            if (PlayerInventory.Instance.AddResource(resourceType, amount))
            {
                // Remove from storage
                _storageBox.RemoveResource(resourceType, amount);
            }
        }
    }
}