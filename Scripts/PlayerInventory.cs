using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    // Singleton instance
    public static PlayerInventory Instance { get; private set; }
    
    [Header("Inventory Settings")]
    public int maxInventorySlots = 20;
    public float maxStackSize = 100f;
    
    // Dictionary to store resources: Key = resource type, Value = amount
    private Dictionary<int, float> inventory = new Dictionary<int, float>();
    
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
    
    // Add resources to inventory
    public bool AddResource(int resourceType, float amount)
    {
        if (resourceType <= 0 || resourceType > 4)
            return false;
            
        // Check if we have space
        if (GetUsedSlots() >= maxInventorySlots && !inventory.ContainsKey(resourceType))
            return false;
            
        // Check if we're exceeding stack size
        if (inventory.ContainsKey(resourceType))
        {
            if (inventory[resourceType] + amount > maxStackSize)
                return false;
                
            inventory[resourceType] += amount;
        }
        else
        {
            inventory[resourceType] = amount;
        }
        
        // Notify UI to update
        OnInventoryChanged?.Invoke();
        
        return true;
    }
    
    // Remove resources from inventory
    public bool RemoveResource(int resourceType, float amount)
    {
        if (!inventory.ContainsKey(resourceType) || inventory[resourceType] < amount)
            return false;
            
        inventory[resourceType] -= amount;
        
        // Remove entry if amount is zero
        if (inventory[resourceType] <= 0)
            inventory.Remove(resourceType);
            
        // Notify UI to update
        OnInventoryChanged?.Invoke();
        
        return true;
    }
    
    // Get the number of used slots
    public int GetUsedSlots()
    {
        int count = 0;
        foreach (var resource in inventory)
        {
            if (resource.Value > 0)
                count++;
        }
        return count;
    }
    
    // Get all inventory items
    public Dictionary<int, float> GetInventory()
    {
        return new Dictionary<int, float>(inventory);
    }
    
    public Dictionary<int, int> GetSerializableInventory()
    {
        Dictionary<int, int> result = new Dictionary<int, int>();
        
        foreach (var kvp in resources)
        {
            result[kvp.Key] = kvp.Value;
        }
        
        return result;
    }
    
    public void LoadInventory(Dictionary<int, int> savedInventory)
    {
        resources.Clear();
        
        foreach (var kvp in savedInventory)
        {
            resources[kvp.Key] = kvp.Value;
        }
        
        // Update UI if needed
        UpdateUI();
    }
    
    // Event for UI updates
    public delegate void InventoryChangedHandler();
    public event InventoryChangedHandler OnInventoryChanged;
}