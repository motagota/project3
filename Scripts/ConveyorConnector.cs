using UnityEngine;

public class ConveyorConnector : MonoBehaviour
{
    [Header("Connection Settings")]
    public Transform connectionPoint;
    public bool isInput = true; // True = input (building to belt), False = output (belt to building)
    
    [Header("Visual Feedback")]
    public Renderer inputConnectorRenderer; // Reference to the renderer component
    public Renderer outConnectorRenderer; // Reference to the renderer component
    
    public Material inputNotConnectedMaterial; // Material when not connected
    public Material outputNotConnectedMaterial; // Material when not connected
    
    public Material connectedMaterial; // Material when connected
    public Material waitingForItemMaterial; // Material when connected and waiting for item
    public Material hasItemMaterial; // Material when holding an item (orange glow)
    
    // References to connected objects
    [HideInInspector] public ConveyorBelt connectedConveyor;
    [HideInInspector] public MonoBehaviour connectedBuilding; // Generic reference to any building

    public GameObject inputBuilding;
    public GameObject outputBuilding;
    
    // Add transfer cooldown to prevent too frequent transfers
    private const float TransferCooldown = 0.5f;
    private float _lastTransferTime = 0f;
    
    // Flag to track if we're currently holding an item
    private bool _isHoldingItem = false;
    private ConveyorItem _heldItem = null;
    
    // Emission properties for glowing effect
    private float _emissionIntensity = 1.0f;
    private const float PulseSpeed = 2.0f;
    private const float MaxEmission = 2.0f;

    private void Start()
    {
        // Set initial material
        if (inputConnectorRenderer != null && inputNotConnectedMaterial != null)
        {
            inputConnectorRenderer.material = inputNotConnectedMaterial;
        }
        if (outConnectorRenderer != null && outputNotConnectedMaterial != null)
        {
            outConnectorRenderer.material = outputNotConnectedMaterial;
        }
        
        // Create the materials if they don't exist
        if (waitingForItemMaterial == null)
        {
            waitingForItemMaterial = new Material(connectedMaterial);
            waitingForItemMaterial.EnableKeyword("_EMISSION");
            waitingForItemMaterial.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 1.0f) * 1.5f);
        }
        
        if (hasItemMaterial == null)
        {
            hasItemMaterial = new Material(connectedMaterial);
            hasItemMaterial.EnableKeyword("_EMISSION");
            hasItemMaterial.SetColor("_EmissionColor", new Color(1.0f, 0.5f, 0.0f) * 1.5f); // Orange glow
        }
        
        inputConnectorRenderer.material = waitingForItemMaterial;
    }
    
    private void Update()
    {
        // Update emission intensity for pulsing effect when waiting for item
        if (inputBuilding != null)
        {
            // Pulse the emission when waiting for an item
            _emissionIntensity = 1.0f + Mathf.PingPong(Time.time * PulseSpeed, MaxEmission - 1.0f);
            
            if (!_isHoldingItem && waitingForItemMaterial != null)
            {
                Color baseColor = new Color(0.5f, 0.5f, 1.0f); 
                waitingForItemMaterial.SetColor("_EmissionColor", baseColor * _emissionIntensity);
            }
            else if (_isHoldingItem && hasItemMaterial != null)
            {
                Color baseColor = new Color(1.0f, 0.5f, 0.0f) ; 
                hasItemMaterial.SetColor("_EmissionColor", baseColor * _emissionIntensity);
            }
            
        }
        
        // Only attempt transfers if cooldown has elapsed
        if (Time.time - _lastTransferTime < TransferCooldown)
            return;

        if (outputBuilding != null)
        {
            if (TryTransferFromBuildingToBelt())
            {
                _lastTransferTime = Time.time;
            }
        }
        // Handle item transfer logic based on connection type
        if (inputBuilding != null && connectedBuilding != null)
        {
            // Input connector: Building -> Belt
            if (TryTransferFromBuildingToBelt())
            {
                _lastTransferTime = Time.time;
            }
        }
        else if (!inputBuilding && connectedBuilding != null)
        {
            // Output connector: Belt -> Building
            if (TryTransferFromBeltToBuilding())
            {
                _lastTransferTime = Time.time;
            }
        }
    }
    
    // Connect to a conveyor belt
    public void ConnectToConveyor(ConveyorBelt conveyor)
    {
        connectedConveyor = conveyor;
        UpdateConnectionVisuals();
    }
    
    // Connect to a building (miner, storage, etc.)
    public void ConnectToBuilding(MonoBehaviour building)
    {
        connectedBuilding = building;
        UpdateConnectionVisuals();
    }
    
  
    
    // Update the visual appearance based on connection status
    private void UpdateConnectionVisuals()
    {
        // Update input connector visual
        if (inputConnectorRenderer != null)
        {
            // Change material based on connection status
            if (inputBuilding)
            {
                if (_isHoldingItem)
                {
                    // Show orange glow when holding an item
                    inputConnectorRenderer.material = hasItemMaterial;
                }
                else
                {
                    // Show brighter glow when waiting for an item
                    inputConnectorRenderer.material = waitingForItemMaterial;
                }
            }
            else
            {
                // Not fully connected
                inputConnectorRenderer.material = inputNotConnectedMaterial;
            }
        }
        
        // Update output connector visual
        if (outConnectorRenderer != null)
        {
            // Change material based on connection status
            if ( connectedConveyor != null && connectedBuilding != null)
            {
                if (_isHoldingItem)
                {
                    // Show orange glow when holding an item
                    outConnectorRenderer.material = hasItemMaterial;
                }
                else
                {
                    // Show brighter glow when waiting for an item
                    outConnectorRenderer.material = waitingForItemMaterial;
                }
            }
            else
            {
                // Not fully connected
                outConnectorRenderer.material = outputNotConnectedMaterial;
            }
        }
    }
    
    // Logic for transferring items from building to belt (Input connector)
    private bool TryTransferFromBuildingToBelt()
    {
        // If we're already holding an item, try to place it on the conveyor
        if (_isHoldingItem && _heldItem != null)
        {
       
        bool accepted = connectedConveyor.AcceptItem(_heldItem, true);
  
            if (accepted)
            {
                // Item successfully placed on conveyor
                _isHoldingItem = false;
                _heldItem = null;
                UpdateConnectionVisuals(); 
                return true;
            }
            return false; // Couldn't place on conveyor, will try again next update
        }

        if (connectedBuilding is MinerBuilding miner)
        {
            // Use GetNextItem instead of TryExtractItem
            ConveyorItem item = miner.GetNextItem();
            if (item != null)
            {
                // Try to place directly on conveyor first
          //      bool accepted = connectedConveyor.AcceptItem(item, true);
          bool accepted = false;
                if (accepted)
                {
                    return true;
                }
                else
                {
                    // If conveyor is full, hold the item temporarily
                    _isHoldingItem = true;
                    _heldItem = item;
                    UpdateConnectionVisuals(); // Update visuals to show we're holding an item
                    return true; // We did get an item, even if we couldn't place it yet
                }
            }
        }
        else if (connectedBuilding is StorageBox storage)
        {
            // Similar logic for storage boxes
            // Use GetItem instead of TryExtractItem
            ConveyorItem item = storage.GetItem();
            if (item != null)
            {
                bool accepted = connectedConveyor.AcceptItem(item, true);
                if (accepted)
                {
                    return true;
                }
                else
                {
                    // If conveyor is full, hold the item temporarily
                    _isHoldingItem = true;
                    _heldItem = item;
                    UpdateConnectionVisuals(); // Update visuals to show we're holding an item
                    return true; // We did get an item, even if we couldn't place it yet
                }
            }
        }
        return false;
    }
    
    // Logic for transferring items from belt to building (Output connector)
    private bool TryTransferFromBeltToBuilding()
    {
        // If we're holding an item, try to deliver it to the building
        if (_isHoldingItem && _heldItem != null)
        {
            bool delivered = false;
            
            if (connectedBuilding is StorageBox storage)
            {
                delivered = storage.AcceptItem(_heldItem);
            }
            // Add other building types as needed
            
            if (delivered)
            {
                // Item successfully delivered to building
                _isHoldingItem = false;
                _heldItem = null;
                UpdateConnectionVisuals(); // Update visuals to show we're no longer holding an item
                return true;
            }
            return false; // Couldn't deliver to building, will try again next update
        }
        
        // This would be called by the conveyor belt when an item reaches this connector
        // For now, return false as this is implemented differently
        return false;
    }
    
    // Method for conveyor to deliver items to this connector
    public bool AcceptItemFromBelt(ConveyorItem item)
    {
        if (!isInput)
        {
            // If we're already holding an item, reject new ones
            if (_isHoldingItem)
                return false;
                
            if (connectedBuilding != null)
            {
                // Try to deliver directly to the connected building
                bool delivered = false;
                
                if (connectedBuilding is StorageBox storage)
                {
                    delivered = storage.AcceptItem(item);
                }
                // Add other building types as needed
                
                if (delivered)
                {
                    return true;
                }
                else
                {
                    // If building can't accept it now, hold the item temporarily
                    _isHoldingItem = true;
                    _heldItem = item;
                    UpdateConnectionVisuals(); // Update visuals to show we're holding an item
                    return true; // We did accept the item, even if we couldn't deliver it yet
                }
            }
        }
        return false;
    }
}