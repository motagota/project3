using UnityEngine;
using System.Collections.Generic;
using Simulation;

public class StorageSystem
{
    public void Update(Dictionary<int, StorageBoxData> storageBoxes, 
                      Dictionary<int, ConveyorItemData> items,
                      float deltaTime)
    {
        foreach (var kvp in storageBoxes)
        {
            int storageId = kvp.Key;
            StorageBoxData storage = kvp.Value;
            
            if (!storage.isActive)
                continue;
                
            // Process incoming items from connected conveyors
            ProcessIncomingItems(storage, items);
            
            // Update the storage data
          //  storageBoxes[storageId] = storage;
        }
    }
    
    private void ProcessIncomingItems(StorageBoxData storage, Dictionary<int, ConveyorItemData> items)
    {
        // Find items that are close to the storage input point
        List<int> itemsToProcess = new List<int>();
        
        foreach (var kvp in items)
        {
            int itemId = kvp.Key;
            ConveyorItemData item = kvp.Value;
            
            // Check if the item is close to the storage input point
            float distance = Vector3.Distance(item.position, storage.inputPointPosition);
            if (distance < 0.2f)
            {
                itemsToProcess.Add(itemId);
            }
        }
        
        // Process each item
        foreach (int itemId in itemsToProcess)
        {
            if (items.TryGetValue(itemId, out ConveyorItemData item))
            {
                // Try to add the resource to storage
                if (TryAddResourceToStorage(ref storage, item.resourceType, item.resourceAmount))
                {
                    // Remove the item from the simulation
                    items.Remove(itemId);
                }
            }
        }
    }
    
    private bool TryAddResourceToStorage(ref StorageBoxData storage, int resourceType, float amount)
    {
        // Calculate total stored resources
        float totalStored = 0f;
        foreach (var resource in storage.storedResources)
        {
            totalStored += resource.Value;
        }
        
        // Check if there's enough space
        if (totalStored + amount > storage.maxCapacity)
            return false;
            
        // Add the resource
        if (storage.storedResources.ContainsKey(resourceType))
        {
            storage.storedResources[resourceType] += amount;
        }
        else
        {
            storage.storedResources[resourceType] = amount;
        }
        
        return true;
    }
    
    public bool TryRemoveResourceFromStorage(ref StorageBoxData storage, int resourceType, float amount)
    {
        // Check if the storage has enough of this resource
        if (!storage.storedResources.ContainsKey(resourceType) || storage.storedResources[resourceType] < amount)
            return false;
            
        // Remove the resource
        storage.storedResources[resourceType] -= amount;
        
        // Remove the entry if the amount is zero
        if (storage.storedResources[resourceType] <= 0)
        {
            storage.storedResources.Remove(resourceType);
        }
        
        return true;
    }
    
    public bool TryCreateItemFromStorage(ref StorageBoxData storage, int resourceType, float amount, 
                                        Dictionary<int, ConveyorItemData> items)
    {
        // Check if we can remove the resource
        if (!TryRemoveResourceFromStorage(ref storage, resourceType, amount))
            return false;
            
        // Create a new item at the output point
        int itemId = Simulation.SimulationManager.Instance.GetNextId();
        
        ConveyorItemData itemData = new ConveyorItemData
        {
            id = itemId,
            position = storage.outputPointPosition,
            rotation = Quaternion.identity,
            resourceType = resourceType,
            resourceAmount = amount,
            currentConveyorId = -1,
            isOnFarLane = false
        };
        
        // Add the item to the simulation
        items[itemId] = itemData;
        
        return true;
    }
}