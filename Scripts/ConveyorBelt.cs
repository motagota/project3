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
    
    // List of connectors attached to this conveyor
    private List<ConveyorConnector> connectedConnectors = new List<ConveyorConnector>();
    // Lists for input and output connectors
    private List<ConveyorConnector> inputConnectors = new List<ConveyorConnector>();
    private List<ConveyorConnector> outputConnectors = new List<ConveyorConnector>();
    
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
    
    public void Rotate()
    {
        // Rotate the conveyor by 90 degrees
        transform.Rotate(0, 90, 0);
        
        // Update direction
        direction = (direction + 1) % 4;
        
        // Update connection points
        UpdateConnectionPoints();
    }
    
    public void RegisterConnector(ConveyorConnector connector)
    {
        if (!connectedConnectors.Contains(connector))
        {
            connectedConnectors.Add(connector);
            UpdateConnectionPoints();
        }
    }

    // Add the missing ConnectToConveyor method
    public void ConnectToConveyor(ConveyorConnector connector)
    {
        // Register the connector with this conveyor
        RegisterConnector(connector);
        
        // Also connect the connector to this conveyor
        connector.ConnectToConveyor(this);
    }
    
    // Add this method to connect to another conveyor belt
    public void ConnectToConveyor(ConveyorBelt otherConveyor, ConveyorConnectionType connectionType)
    {
        // Implementation for connecting to another conveyor belt
        Debug.Log($"Connected conveyor at {transform.position} to conveyor at {otherConveyor.transform.position} with type {connectionType}");
    }

    private void UpdateConnectionPoints()
    {
        // Clear existing connections first
        inputConnectors.Clear();
        outputConnectors.Clear();
        
        // Add logic to set input/output points based on connector positions
        foreach (ConveyorConnector connector in connectedConnectors)
        {
            Vector3 relativePos = transform.InverseTransformPoint(connector.transform.position);
            
            if (relativePos.z > 0.5f)
                outputConnectors.Add(connector);
            else if (relativePos.z < -0.5f)
                inputConnectors.Add(connector);
        }
    }
    
    // Add a method to handle transferring items to the next conveyor through connectors
    private bool TransferItemToNextConveyor(ConveyorItem item, bool isFarLane)
    {
        // Check if we have any output connectors
        if (outputConnectors.Count > 0)
        {
            // For simplicity, just use the first output connector
            // You could implement more complex logic to choose which connector to use
            ConveyorConnector outputConnector = outputConnectors[0];
            
            if (outputConnector.connectedConveyor != null)
            {
                // Transfer to the connected conveyor
                return outputConnector.connectedConveyor.AcceptItem(item, isFarLane);
            }
            else if (outputConnector.connectedBuilding != null)
            {
                // Handle transfer to a building
                // This would need to be implemented based on your building interface
                return false;
            }
        }
        
        return false;
    }
    
    // Method to accept an item from another conveyor or building
    public bool AcceptItem(ConveyorItem item, bool useFarLane)
    {
        if (useFarLane)
        {
            item.transform.position = inputPoint.position;
            itemsOnFarLane.Add(item);
            return true;
        }
        else
        {
            item.transform.position = inputPoint.position;
            itemsOnCloseLane.Add(item);
            return true;
        }
    }
}