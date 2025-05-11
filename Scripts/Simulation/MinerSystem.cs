using UnityEngine;
using System.Collections.Generic;
using Simulation;

public class MinerSystem
{
    public void Update(Dictionary<int, MinerData> miners, 
                      Dictionary<int, ConveyorItemData> items,
                      Dictionary<int, ConveyorBeltData> conveyorBelts,
                      float deltaTime)
    {
        // Create a list to store pending operations
        List<PendingItemPlacement> pendingPlacements = new List<PendingItemPlacement>();
        
        foreach (var kvp in miners)
        {
            int minerId = kvp.Key;
            MinerData miner = kvp.Value;
            
            if (!miner.isActive)
                continue;
                
            // Update mining timer
            miner.miningTimer += deltaTime;
            
            // Check if it's time to produce a resource
            if (miner.miningTimer >= 1.0f / miner.miningRate)
            {
                miner.miningTimer = 0;
                
                // Instead of immediately placing the resource, add it to pending operations
                QueueResourcePlacement(miner, pendingPlacements);
            }
            
            // Update the miner data
          //  miners[minerId] = miner;
        }
        
        // Process all pending placements after the iteration is complete
        foreach (var placement in pendingPlacements)
        {
            ProcessItemPlacement(placement, items, conveyorBelts);
        }
    }
    
    // Structure to hold pending item placement data
    private struct PendingItemPlacement
    {
        public int conveyorId;
        public Vector3 position;
        public Quaternion rotation;
        public int resourceType;
        public float resourceAmount;
        public bool isOnFarLane;
    }
    
    private void QueueResourcePlacement(MinerData miner, List<PendingItemPlacement> pendingPlacements)
    {
        }
    
    private void ProcessItemPlacement(PendingItemPlacement placement, 
                                     Dictionary<int, ConveyorItemData> items,
                                     Dictionary<int, ConveyorBeltData> conveyorBelts)
    {
        // Check if the conveyor exists
        if (!conveyorBelts.TryGetValue(placement.conveyorId, out ConveyorBeltData conveyor))
            return;
            
        // Create a new item
        int itemId = Simulation.SimulationManager.Instance.GetNextId();
        
        ConveyorItemData itemData = new ConveyorItemData
        {
            id = itemId,
            position = placement.position,
            rotation = placement.rotation,
            resourceType = placement.resourceType,
            resourceAmount = placement.resourceAmount,
            currentConveyorId = placement.conveyorId,
            isOnFarLane = placement.isOnFarLane
        };
        
        // Add the item to the simulation
        items[itemId] = itemData;
        
        // Add the item to the conveyor's lane
        if (placement.isOnFarLane)
        {
            List<int> farLane = new List<int>(conveyor.itemsOnFarLane);
            farLane.Add(itemId);
            conveyor.itemsOnFarLane = farLane;
        }
        else
        {
            List<int> closeLane = new List<int>(conveyor.itemsOnCloseLane);
            closeLane.Add(itemId);
            conveyor.itemsOnCloseLane = closeLane;
        }
        
        // Update the conveyor data
        conveyorBelts[placement.conveyorId] = conveyor;
    }
}