using UnityEngine;
using V2.Data;
using V2.GameObjects;

namespace V2.Factories
{
    public static class StorageBoxFactory
    {
       
        public static V2.Data.StorageBox CreateStorageBox(Vector2Int gridPosition, int slotCount = 12)
        {
            // Create the GameObject
            GameObject storageBoxObject = new GameObject("StorageBox");
            StorageBoxObject storageBoxComponent = storageBoxObject.AddComponent<StorageBoxObject>();
            
            // Initialize the storage box with the grid position
            storageBoxComponent.Initialize(gridPosition);
            
            // Return the data object
            return storageBoxComponent.StorageBoxData;
        }
        
        public static V2.Data.StorageBox CreateStorageBoxWithItems(Vector2Int gridPosition, string[] itemTypes, int slotCount = 12)
        {
            // Create the storage box
            V2.Data.StorageBox storageBox = CreateStorageBox(gridPosition, slotCount);
            
            // Add items to the storage box
            if (itemTypes != null && itemTypes.Length > 0)
            {
                foreach (string itemType in itemTypes)
                {
                    // Create a new item and add it to the storage box
                    SimulationItem item = new SimulationItem(System.Guid.NewGuid().ToString(), itemType);
                    storageBox.GiveItem(item);
                }
            }
            
            return storageBox;
        }
    }
}