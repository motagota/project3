using UnityEngine;
using System.Collections.Generic;

public class PlayerInventoryUI : MonoBehaviour
{
    private bool showInventory = false;
    private Rect inventoryWindowRect = new Rect(Screen.width - 320, 20, 300, 400);
    private Vector2 scrollPosition;
    
    private void Start()
    {
        // Subscribe to inventory changes
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged += UpdateUI;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged -= UpdateUI;
        }
    }
    
    private void Update()
    {
        // Toggle inventory with a key (e.g., I)
        if (Input.GetKeyDown(KeyCode.I))
        {
            showInventory = !showInventory;
        }
    }
    
    private void OnGUI()
    {
        if (showInventory)
        {
            inventoryWindowRect = GUI.Window(100, inventoryWindowRect, DrawInventoryWindow, "Player Inventory");
        }
    }
    
    private void DrawInventoryWindow(int id)
    {
        if (PlayerInventory.Instance == null)
        {
            GUILayout.Label("Inventory system not initialized!");
            if (GUILayout.Button("Close"))
            {
                showInventory = false;
            }
            return;
        }
        
        // Get inventory contents
        Dictionary<int, float> inventory = PlayerInventory.Instance.GetInventory();
        
        // Display used slots
        GUILayout.Label($"Slots: {PlayerInventory.Instance.GetUsedSlots()} / {PlayerInventory.Instance.maxInventorySlots}");
        
        // Scrollable area for inventory items
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        
        if (inventory.Count == 0)
        {
            GUILayout.Label("Inventory is empty");
        }
        else
        {
            foreach (var item in inventory)
            {
                GUILayout.BeginHorizontal("box");
                
                // Resource name and amount
                string resourceName = StorageBox.GetResourceName(item.Key);
                GUILayout.Label($"{resourceName}: {item.Value:F1}", GUILayout.Width(200));
                
                GUILayout.EndHorizontal();
            }
        }
        
        GUILayout.EndScrollView();
        
        // Close button
        if (GUILayout.Button("Close"))
        {
            showInventory = false;
        }
        
        // Make window draggable
        GUI.DragWindow();
    }
    
    private void UpdateUI()
    {
        // This method is called when inventory changes
        // We don't need to do anything here since OnGUI is called every frame
    }
}