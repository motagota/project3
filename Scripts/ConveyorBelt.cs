using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Conveyor Settings")]
    public float speed = 1.0f; // Speed of items on the belt
    public Transform farLaneTransform; // Transform for the far lane
    public Transform closeLaneTransform; // Transform for the close lane
    
    [Header("Connection Points")]
    public Transform inputPoint; // Where items enter
    public Transform outputPoint; // Where items exit
    
    [Header("Visual Settings")]
    public Material conveyorMaterial; // Material with scrolling texture
    public GameObject beltMeshObject; // The visual mesh of the belt
    
    // Direction the conveyor is facing (0-3 for N,E,S,W)
    [HideInInspector]
    public int direction = 0;
    
    // Items currently on this conveyor
    private List<ConveyorItem> itemsOnFarLane = new List<ConveyorItem>();
    private List<ConveyorItem> itemsOnCloseLane = new List<ConveyorItem>();
    
    // Connected conveyors
    private ConveyorBelt nextConveyor;
    private ConveyorBelt previousConveyor;
    
    // Side connections
    private ConveyorBelt leftSideConnection;
    private ConveyorBelt rightSideConnection;
    
    private void Update()
    {
        // Move items on the far lane
        MoveItemsOnLane(itemsOnFarLane, farLaneTransform, true);
        
        // Move items on the close lane
        MoveItemsOnLane(itemsOnCloseLane, closeLaneTransform, false);
        
        // Update conveyor material to scroll
        if (conveyorMaterial != null && beltMeshObject != null)
        {
            float offset = Time.time * speed * 0.1f;
            conveyorMaterial.SetTextureOffset("_MainTex", new Vector2(0, offset));
        }
    }
    
    private void MoveItemsOnLane(List<ConveyorItem> items, Transform laneTransform, bool isFarLane)
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            ConveyorItem item = items[i];
            if (item == null)
            {
                items.RemoveAt(i);
                continue;
            }
            
            // Calculate movement along the conveyor
            Vector3 targetPosition = outputPoint.position;
            float distanceToEnd = Vector3.Distance(item.transform.position, targetPosition);
            
            // Move the item
            Vector3 moveDirection = (targetPosition - item.transform.position).normalized;
            item.transform.position += moveDirection * speed * Time.deltaTime;
            
            // Check if item reached the end of this conveyor
            if (distanceToEnd < 0.1f)
            {
                // Try to transfer to the next conveyor
                if (TransferItemToNextConveyor(item, isFarLane))
                {
                    items.RemoveAt(i);
                }
                else
                {
                    // Item can't move forward, keep it at the end
                    item.transform.position = targetPosition;
                }
            }
        }
    }
    
    public bool AcceptItem(ConveyorItem item, bool toFarLane)
    {
        // Check if we can accept an item (implement logic for backpressure)
        List<ConveyorItem> targetLane = toFarLane ? itemsOnFarLane : itemsOnCloseLane;
        
        // Simple check: don't allow items too close to each other
        foreach (var existingItem in targetLane)
        {
            if (Vector3.Distance(existingItem.transform.position, inputPoint.position) < 0.5f)
            {
                return false; // Too crowded
            }
        }
        
        // Accept the item
        item.transform.position = inputPoint.position;
        targetLane.Add(item);
        return true;
    }
    
    private bool TransferItemToNextConveyor(ConveyorItem item, bool fromFarLane)
    {
        // Try to transfer to the next inline conveyor
        if (nextConveyor != null)
        {
            // Determine which lane to target based on connection type
            bool toFarLane = DetermineTargetLane(fromFarLane);
            
            if (nextConveyor.AcceptItem(item, toFarLane))
            {
                return true;
            }
        }
        
        // If inline transfer failed, try side connections based on lane
        if (fromFarLane && rightSideConnection != null)
        {
            if (rightSideConnection.AcceptItem(item, true)) // Always to far lane for side connections
            {
                return true;
            }
        }
        else if (!fromFarLane && leftSideConnection != null)
        {
            if (leftSideConnection.AcceptItem(item, true))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private bool DetermineTargetLane(bool fromFarLane)
    {
        // Logic to determine which lane to target on the next conveyor
        // This depends on the relative orientation of the conveyors
        
        // For now, a simple implementation: maintain the same lane
        return fromFarLane;
    }
    
    public void ConnectToConveyor(ConveyorBelt other, ConveyorConnectionType connectionType)
    {
        switch (connectionType)
        {
            case ConveyorConnectionType.Next:
                nextConveyor = other;
                other.previousConveyor = this;
                break;
                
            case ConveyorConnectionType.Previous:
                previousConveyor = other;
                other.nextConveyor = this;
                break;
                
            case ConveyorConnectionType.LeftSide:
                leftSideConnection = other;
                // Determine which side connection to use on the other conveyor
                if (Mathf.Abs(direction - other.direction) % 2 == 0)
                {
                    other.rightSideConnection = this;
                }
                else
                {
                    other.leftSideConnection = this;
                }
                break;
                
            case ConveyorConnectionType.RightSide:
                rightSideConnection = other;
                // Determine which side connection to use on the other conveyor
                if (Mathf.Abs(direction - other.direction) % 2 == 0)
                {
                    other.leftSideConnection = this;
                }
                else
                {
                    other.rightSideConnection = this;
                }
                break;
        }
    }
    
    public void Rotate()
    {
        // Rotate the conveyor by 90 degrees
        transform.Rotate(0, 90, 0);
        
        // Update direction
        direction = (direction + 1) % 4;
        
        // Reset connections when rotated
        DisconnectAllConveyors();
    }
    
    private void DisconnectAllConveyors()
    {
        // Remove this conveyor from all connected conveyors
        if (nextConveyor != null)
        {
            nextConveyor.previousConveyor = null;
            nextConveyor = null;
        }
        
        if (previousConveyor != null)
        {
            previousConveyor.nextConveyor = null;
            previousConveyor = null;
        }
        
        if (leftSideConnection != null)
        {
            if (leftSideConnection.rightSideConnection == this)
                leftSideConnection.rightSideConnection = null;
            if (leftSideConnection.leftSideConnection == this)
                leftSideConnection.leftSideConnection = null;
            leftSideConnection = null;
        }
        
        if (rightSideConnection != null)
        {
            if (rightSideConnection.leftSideConnection == this)
                rightSideConnection.leftSideConnection = null;
            if (rightSideConnection.rightSideConnection == this)
                rightSideConnection.rightSideConnection = null;
            rightSideConnection = null;
        }
    }
}

public enum ConveyorConnectionType
{
    Next,
    Previous,
    LeftSide,
    RightSide
}