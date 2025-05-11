using UnityEngine;
using System.Collections.Generic;

public class ConveyorSystem
{
    public void Update(Dictionary<int, ConveyorBeltData> conveyorBelts, 
                      Dictionary<int, ConveyorItemData> conveyorItems, 
                      float deltaTime)
    {
        // Process all conveyor belts
        foreach (var kvp in conveyorBelts)
        {
            int conveyorId = kvp.Key;
            ConveyorBeltData conveyor = kvp.Value;
            
            if (!conveyor.isActive)
                continue;
                
            // Process items on far lane
            ProcessItemsOnLane(conveyor, conveyorItems, conveyor.itemsOnFarLane, true, deltaTime);
            
            // Process items on close lane
            ProcessItemsOnLane(conveyor, conveyorItems, conveyor.itemsOnCloseLane, false, deltaTime);
            
            // Update the conveyor data
            conveyorBelts[conveyorId] = conveyor;
        }
    }
    
    private void ProcessItemsOnLane(ConveyorBeltData conveyor, 
                                   Dictionary<int, ConveyorItemData> conveyorItems,
                                   List<int> itemsOnLane, 
                                   bool isFarLane, 
                                   float deltaTime)
    {
        for (int i = itemsOnLane.Count - 1; i >= 0; i--)
        {
            int itemId = itemsOnLane[i];
            
            if (!conveyorItems.TryGetValue(itemId, out ConveyorItemData item))
            {
                itemsOnLane.RemoveAt(i);
                continue;
            }
            
            // Calculate movement along the conveyor
            Vector3 targetPosition = conveyor.outputPointPosition;
            float distanceToEnd = Vector3.Distance(item.position, targetPosition);
            
            // Move the item
            Vector3 moveDirection = (targetPosition - item.position).normalized;
            item.position += moveDirection * conveyor.speed * deltaTime;
            
            // Check if item reached the end of this conveyor
            if (distanceToEnd < 0.1f)
            {
                // Try to transfer to the next conveyor
                if (TryTransferItemToNextConveyor(conveyor, item, conveyorItems, isFarLane))
                {
                    itemsOnLane.RemoveAt(i);
                }
                else
                {
                    // Item can't move forward, keep it at the end
                    item.position = targetPosition;
                }
            }
            
            // Update the item data
            conveyorItems[itemId] = item;
        }
    }
    
    private bool TryTransferItemToNextConveyor(ConveyorBeltData sourceConveyor, 
                                              ConveyorItemData item,
                                              Dictionary<int, ConveyorItemData> conveyorItems,
                                              bool isFarLane)
    {
        // Implementation for transferring items between conveyors
        // This would check connected conveyors and try to place the item on the next one
        
        // For now, just a placeholder
        return false;
    }
}