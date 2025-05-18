using UnityEngine;

namespace V2.Data
{
    public static class ConnectorExtension
    {
        /// <summary>
        /// Extension method to handle StorageBox as an input connector
        /// </summary>
        public static void HandleStorageBoxInput(this Connector connector, StorageBox storageBox)
        {
            // If the connector already has a held item, try to give it to the storage box
            if (connector.GetHeldItem() != null)
            {
                SimulationItem heldItem = connector.GetHeldItem();
                if (storageBox.CanAcceptItem(heldItem))
                {
                    if (storageBox.GiveItem(heldItem))
                    {
                        // Item was successfully given to the storage box
                        Debug.Log($"Connector {connector.ID} gave item {heldItem} to storage box {storageBox.ID}");
                    }
                    else
                    {
                        // Failed to give item to storage box
                        Debug.Log($"Connector {connector.ID} failed to give item {heldItem} to storage box {storageBox.ID}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Extension method to handle StorageBox as an output connector
        /// </summary>
        public static void HandleStorageBoxOutput(this Connector connector, StorageBox storageBox)
        {
            // If the connector doesn't have a held item, try to take one from the storage box
            if (connector.GetHeldItem() == null && storageBox.HasItem)
            {
                SimulationItem item = storageBox.TakeItem();
                if (item != null)
                {
                    // Item was successfully taken from the storage box
                    Debug.Log($"Connector {connector.ID} took item {item} from storage box {storageBox.ID}");
                }
            }
        }
        
        /// <summary>
        /// Extension method to check if a StorageBox can accept an item
        /// </summary>
        public static bool CanStorageBoxAcceptItem(this Connector connector, StorageBox storageBox, SimulationItem item)
        {
            return storageBox.CanAcceptItem(item);
        }
    }
}