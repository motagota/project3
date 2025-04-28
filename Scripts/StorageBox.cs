using UnityEngine;
using System.Collections.Generic;

public class StorageBox : MonoBehaviour
{
    [Header("Storage Settings")]
    public int maxStorageSlots = 12;
    public float maxStackSize = 100f;
    
    // Dictionary to store resources: Key = resource type, Value = amount
    private readonly Dictionary<int, float> storedResources = new Dictionary<int, float>();
    
    // Resource type names for UI display
    private static readonly string[] ResourceNames = new string[] 
    { 
        "None", 
        "Coal", 
        "Iron", 
        "Copper", 
        "Stone" 
    };
    
    private void Awake()
    {
        // Initialize empty storage
        for (int i = 1; i <= 4; i++) // Resource types 1-4
        {
            storedResources[i] = 0f;
        }
    }
    
    // Initialize the storage box with a grid tile
    public void Initialize(GridTile tile)
    {
        if (tile == null)
        {
            Debug.LogError("Attempted to initialize StorageBox with null tile!");
            return;
        }
        
        // Storage box doesn't need to extract resources from the tile,
        // but we can use the tile reference for positioning or other functionality
        Debug.Log($"Storage Box initialized at position {tile.transform.position}");
    }
    
    // Add resources to storage
    public bool AddResource(int resourceType, float amount)
    {
        if (resourceType <= 0 || resourceType > 4)
            return false;
            
        // Check if we have space
        if (GetUsedSlots() >= maxStorageSlots && !storedResources.ContainsKey(resourceType))
            return false;
            
        // Check if we're exceeding stack size
        if (storedResources.ContainsKey(resourceType))
        {
            if (storedResources[resourceType] + amount > maxStackSize)
                return false;
                
            storedResources[resourceType] += amount;
        }
        else
        {
            storedResources[resourceType] = amount;
        }
        
        return true;
    }
    
    // Remove resources from storage
    public bool RemoveResource(int resourceType, float amount)
    {
        if (!storedResources.ContainsKey(resourceType) || storedResources[resourceType] < amount)
            return false;
            
        storedResources[resourceType] -= amount;
        
        // Remove entry if amount is zero
        if (storedResources[resourceType] <= 0)
            storedResources.Remove(resourceType);
            
        return true;
    }
    
    // Get the number of used slots
    public int GetUsedSlots()
    {
        int count = 0;
        foreach (var resource in storedResources)
        {
            if (resource.Value > 0)
                count++;
        }
        return count;
    }
    
    // Get all stored resources
    public Dictionary<int, float> GetStoredResources()
    {
        return new Dictionary<int, float>(storedResources);
    }
    
    // Get resource name by type
    public static string GetResourceName(int resourceType)
    {
        if (resourceType >= 0 && resourceType < ResourceNames.Length)
            return ResourceNames[resourceType];
        return "Unknown";
    }

    // Add these methods to support saving/loading
    public Dictionary<int, int> GetSerializableStorage()
    {
        Dictionary<int, int> result = new Dictionary<int, int>();
        
        foreach (var kvp in storedResources)
        {
            result[kvp.Key] = Mathf.FloorToInt(kvp.Value);
        }
        
        return result;
    }

    public void LoadStorage(Dictionary<int, int> savedStorage)
    {
        storedResources.Clear();
        
        foreach (var kvp in savedStorage)
        {
            storedResources[kvp.Key] = kvp.Value;
        }
    }
    
    // Get an item from storage for conveyor belt transfer
    public ConveyorItem GetItem()
    {
        // Find the first resource type with a non-zero amount
        foreach (var resource in storedResources)
        {
            if (resource.Value > 0)
            {
                // Create a new conveyor item with this resource type
                int resourceType = resource.Key;
                int amount = 1; // Transfer one unit at a time
                
                // Remove the resource from storage
                RemoveResource(resourceType, amount);
                
                // Create and return a new conveyor item
                Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
                return ConveyorItem.CreateItem(resourceType, amount, spawnPosition);
            }
        }
        
        // No resources available
        return null;
    }
    
    // Store an item returned from a conveyor belt
    public void StoreItem(ConveyorItem item)
    {
        if (item != null)
        {
            // Add the item's resources to storage
            AddResource(item.itemType, item.quantity);
            
            // Destroy the conveyor item game object
            Destroy(item.gameObject);
        }
    }
    
    // Accept an item from a conveyor belt
    public bool AcceptItem(ConveyorItem item)
    {
        if (item == null)
            return false;
            
        // Try to add the resource to storage
        bool accepted = AddResource(item.itemType, item.quantity);
        
        if (accepted)
        {
            // If accepted, destroy the conveyor item
            Destroy(item.gameObject);
        }
        
        return accepted;
    }
}