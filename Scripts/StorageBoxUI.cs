using UnityEngine;
using System.Collections.Generic;

public class StorageBoxUI : MonoBehaviour
{
    private StorageBox storageBox;
    private bool showUI = false;
    private Rect windowRect = new Rect(20, 20, 300, 400);
    private int windowID;
    private Vector2 scrollPosition;
    
    private void Start()
    {
        storageBox = GetComponent<StorageBox>();
        windowID = GetInstanceID();
    }
    
    private void OnMouseDown()
    {
        showUI = !showUI;
    }
    
    private void OnGUI()
    {
        if (showUI && storageBox != null)
        {
            windowRect = GUI.Window(windowID, windowRect, DrawStorageWindow, "Storage Box");
        }
    }
    
    private void DrawStorageWindow(int id)
    {
        // Display used slots
        GUILayout.Label($"Slots: {storageBox.GetUsedSlots()} / {storageBox.maxStorageSlots}");
        
        // Get stored resources
        Dictionary<int, float> resources = storageBox.GetStoredResources();
        
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
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        
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
            showUI = false;
        }
        
        // Make window draggable
        GUI.DragWindow();
    }
    
    private void StoreResource(int resourceType)
    {
        if (PlayerInventory.Instance == null || storageBox == null)
            return;
            
        // Get the amount from player inventory
        Dictionary<int, float> playerInventory = PlayerInventory.Instance.GetInventory();
        
        if (playerInventory.ContainsKey(resourceType))
        {
            float amount = playerInventory[resourceType];
            
            // Try to add to storage
            if (storageBox.AddResource(resourceType, amount))
            {
                // Remove from player inventory
                PlayerInventory.Instance.RemoveResource(resourceType, amount);
            }
        }
    }
    
    private void TakeResource(int resourceType)
    {
        if (PlayerInventory.Instance == null || storageBox == null)
            return;
            
        // Get the amount from storage
        Dictionary<int, float> resources = storageBox.GetStoredResources();
        
        if (resources.ContainsKey(resourceType))
        {
            float amount = resources[resourceType];
            
            // Try to add to player inventory
            if (PlayerInventory.Instance.AddResource(resourceType, amount))
            {
                // Remove from storage
                storageBox.RemoveResource(resourceType, amount);
            }
        }
    }
}