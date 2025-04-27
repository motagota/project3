using UnityEngine;

public class ConveyorConnector : MonoBehaviour
{
    [Header("Connection Settings")]
    public Transform connectionPoint;
    public ConveyorConnectionType connectionType = ConveyorConnectionType.Input; // Replace boolean with enum
    
    [Header("Connected Objects")]
    public ConveyorBelt connectedConveyor;
    public MonoBehaviour connectedBuilding; // MinerBuilding, StorageBox, etc.
    
    [Header("Visual")]
    public GameObject connectorModel;
    public Material connectorMaterial;
    
    // Track which lane is being used
    private bool usingFarLane = true;
    private bool isInput;

    private void Start()
    {
        // Initialize connector visuals
        if (connectorModel == null)
        {
            CreateDefaultConnectorVisual();
        }
    }
    
    private void Update()
    {
        if (connectionType == ConveyorConnectionType.Input && connectedConveyor != null && connectedBuilding != null)
        {
            // Handle input logic (building to conveyor)
            HandleInputLogic();
        }
        else if (connectionType == ConveyorConnectionType.Output && connectedConveyor != null && connectedBuilding != null)
        {
            // Handle output logic (conveyor to building)
            HandleOutputLogic();
        }
        else if (connectionType == ConveyorConnectionType.Bidirectional && connectedConveyor != null && connectedBuilding != null)
        {
            // Handle bidirectional logic if needed
            HandleInputLogic();
            HandleOutputLogic();
        }
    }
    
    private void HandleInputLogic()
    {
        // Example: If this is connected to a miner, take resources and put them on the conveyor
        if (connectedBuilding is MinerBuilding miner)
        {
            // Check if miner has resources and conveyor can accept them
            if (miner.StoredResources > 0)
            {
                // Try to place on far lane first
                if (usingFarLane)
                {
                    // Create a new item from the miner's resources
                    ConveyorItem newItem = ConveyorItem.CreateItem(miner.ResourceType, 1, connectionPoint.position);
                    
                    // Try to place on far lane
                    if (connectedConveyor.AcceptItem(newItem, true))
                    {
                        // Successfully placed on far lane
                        miner.StoredResources--;
                    }
                    else
                    {
                        // Far lane is blocked, try close lane next time
                        usingFarLane = false;
                        Destroy(newItem.gameObject); // Clean up the item we couldn't place
                    }
                }
                else
                {
                    // Try close lane
                    ConveyorItem newItem = ConveyorItem.CreateItem(miner.ResourceType, 1, connectionPoint.position);
                    
                    if (connectedConveyor.AcceptItem(newItem, false))
                    {
                        // Successfully placed on close lane
                        miner.StoredResources--;
                        usingFarLane = true; // Reset to try far lane next time
                    }
                    else
                    {
                        // Both lanes are blocked
                        Destroy(newItem.gameObject); // Clean up the item we couldn't place
                    }
                }
            }
        }
    }
    
    private void HandleOutputLogic()
    {
        // Example: If this is connected to a storage box, take items from conveyor and put in storage
        if (connectedBuilding is StorageBox storage)
        {
            // Logic for taking items from conveyor and putting them in storage
            // This would be implemented based on how your StorageBox handles receiving items
        }
    }
    
    public void ConnectToConveyor(ConveyorBelt conveyor)
    {
        connectedConveyor = conveyor;
        
        // Update visual to show connection
        if (connectorModel != null)
        {
            // Point connector toward conveyor
            Vector3 direction = conveyor.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    // Change this method to accept a ConveyorBelt parameter
    public void ConnectToNearbyConveyor(ConveyorBelt conveyor)
    {
        // Simply call the existing ConnectToConveyor method
        ConnectToConveyor(conveyor);
    }
    
    public void ConnectToBuilding(MonoBehaviour building)
    {
        connectedBuilding = building;
    }
    
    private void CreateDefaultConnectorVisual()
    {
        // Create a simple visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);
        visual.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        // Set material
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null && connectorMaterial != null)
        {
            renderer.material = connectorMaterial;
        }
        else if (renderer != null)
        {
            // Default color based on type
            
            renderer.material.color = isInput ? Color.green : Color.red;
        }
        
        connectorModel = visual;
    }
}