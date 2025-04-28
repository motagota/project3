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
    private List<ConveyorItem> _itemsOnFarLane = new List<ConveyorItem>();
    private List<ConveyorItem> _itemsOnCloseLane = new List<ConveyorItem>();
    
    // List of connectors attached to this conveyor
    private List<ConveyorConnector> _connectedConnectors = new List<ConveyorConnector>();
    // Lists for input and output connectors
    private List<ConveyorConnector> _inputConnectors = new List<ConveyorConnector>();
    private List<ConveyorConnector> _outputConnectors = new List<ConveyorConnector>();
    
    // Connected conveyor belts
    private List<ConveyorBelt> _connectedConveyors = new List<ConveyorBelt>();
    
    private void Update()
    {
        // Move items on the far lane
        MoveItemsOnLane(_itemsOnFarLane, farLaneTransform, true);
        
        // Move items on the close lane
        MoveItemsOnLane(_itemsOnCloseLane, closeLaneTransform, false);
        
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
        if (!_connectedConnectors.Contains(connector))
        {
            _connectedConnectors.Add(connector);
            UpdateConnectionPoints();
        }
    }

    // Connect to a conveyor through a connector
    public void ConnectToConveyor(ConveyorConnector connector)
    {
        // Register the connector with this conveyor
        RegisterConnector(connector);
        
        // Also connect the connector to this conveyor
        connector.ConnectToConveyor(this);
    }
    
    // Connect directly to another conveyor belt
    public void ConnectToConveyor(ConveyorBelt otherConveyor, ConveyorConnectionType connectionType)
    {
        // Add the other conveyor to our connected conveyors list if not already there
        if (!_connectedConveyors.Contains(otherConveyor))
        {
            _connectedConveyors.Add(otherConveyor);
            
            // Create a virtual connection based on the connection type
            switch (connectionType)
            {
                case ConveyorConnectionType.Input:
                    // This conveyor receives items from the other conveyor
                    Debug.Log($"Connected conveyor at {transform.position} to receive from conveyor at {otherConveyor.transform.position}");
                    break;
                    
                case ConveyorConnectionType.Output:
                    // This conveyor sends items to the other conveyor
                    Debug.Log($"Connected conveyor at {transform.position} to send to conveyor at {otherConveyor.transform.position}");
                    break;
                    
                case ConveyorConnectionType.Bidirectional:
                    // Items can flow both ways
                    Debug.Log($"Connected conveyor at {transform.position} bidirectionally with conveyor at {otherConveyor.transform.position}");
                    break;
                    
                default:
                    Debug.Log($"Connected conveyor at {transform.position} to conveyor at {otherConveyor.transform.position} with type {connectionType}");
                    break;
            }
            
            // Make sure the other conveyor is also connected to this one (if not already)
            if (!otherConveyor._connectedConveyors.Contains(this))
            {
                // Determine the reciprocal connection type
                ConveyorConnectionType reciprocalType = GetReciprocalConnectionType(connectionType);
                otherConveyor.ConnectToConveyor(this, reciprocalType);
            }
        }
    }
    
    // Helper method to get the reciprocal connection type
    private ConveyorConnectionType GetReciprocalConnectionType(ConveyorConnectionType type)
    {
        switch (type)
        {
            case ConveyorConnectionType.Input:
                return ConveyorConnectionType.Output;
                
            case ConveyorConnectionType.Output:
                return ConveyorConnectionType.Input;
                
            case ConveyorConnectionType.Bidirectional:
                return ConveyorConnectionType.Bidirectional;
                
            case ConveyorConnectionType.Next:
                return ConveyorConnectionType.Previous;
                
            case ConveyorConnectionType.Previous:
                return ConveyorConnectionType.Next;
                
            case ConveyorConnectionType.LeftSide:
                return ConveyorConnectionType.RightSide;
                
            case ConveyorConnectionType.RightSide:
                return ConveyorConnectionType.LeftSide;
                
            default:
                return type;
        }
    }

    private void UpdateConnectionPoints()
    {
        // Clear existing connections first
        _inputConnectors.Clear();
        _outputConnectors.Clear();
        
        // Add logic to set input/output points based on connector positions
        foreach (ConveyorConnector connector in _connectedConnectors)
        {
            Vector3 relativePos = transform.InverseTransformPoint(connector.transform.position);
            
            if (relativePos.z > 0.5f)
                _outputConnectors.Add(connector);
            else if (relativePos.z < -0.5f)
                _inputConnectors.Add(connector);
        }
    }
    
    // Add a method to handle transferring items to the next conveyor through connectors
    private bool TransferItemToNextConveyor(ConveyorItem item, bool isFarLane)
    {
        // First try to transfer through connectors
        if (_outputConnectors.Count > 0)
        {
            // For simplicity, just use the first output connector
            ConveyorConnector outputConnector = _outputConnectors[0];
            
            if (outputConnector.connectedConveyor != null)
            {
                // Transfer to the connected conveyor
                return outputConnector.connectedConveyor.AcceptItem(item, isFarLane);
            }
            else if (outputConnector.connectedBuilding != null)
            {
                // Handle transfer to a building
                return false;
            }
        }
        
        // If no connectors or transfer failed, try direct conveyor connections
        foreach (ConveyorBelt connectedBelt in _connectedConveyors)
        {
            // Check if this is an output connection
            Vector3 dirToConnected = (connectedBelt.transform.position - transform.position).normalized;
            Vector3 forwardDir = transform.forward;
            
            // If the connected belt is roughly in front of this one (output direction)
            if (Vector3.Dot(dirToConnected, forwardDir) > 0.5f)
            {
                // Try to transfer the item
                if (connectedBelt.AcceptItem(item, isFarLane))
                {
                    return true;
                }
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
            _itemsOnFarLane.Add(item);
            return true;
        }
        else
        {
            item.transform.position = inputPoint.position;
            _itemsOnCloseLane.Add(item);
            return true;
        }
    }
}