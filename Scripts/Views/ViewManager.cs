using UnityEngine;
using System.Collections.Generic;
using Simulation;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }
    
    [Header("Prefabs")]
    public GameObject conveyorBeltPrefab;
    public GameObject conveyorItemPrefab;
    public GameObject minerPrefab;
    public GameObject storageBoxPrefab;
    
    // Track active views
    private Dictionary<int, GameObject> activeViews = new Dictionary<int, GameObject>();
    
    // Camera for culling
    private Camera mainCamera;
    private float cullingDistance = 50f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        mainCamera = Camera.main;
    }
    
    private void Update()
    {
        // Get camera position for culling
        Vector3 cameraPosition = mainCamera.transform.position;
        
        // Update conveyor belt views
        foreach (var kvp in Simulation.SimulationManager.Instance.conveyorBelts)
        {
            int id = kvp.Key;
            ConveyorBeltData data = kvp.Value;
            
            // Check if this conveyor should be visible (within culling distance)
            float distance = Vector3.Distance(cameraPosition, data.position);
            bool shouldBeVisible = distance <= cullingDistance;
            
            if (shouldBeVisible)
            {
                // Create or update view
                if (!activeViews.TryGetValue(id, out GameObject viewObject))
                {
                    // Create new view
                    viewObject = Instantiate(conveyorBeltPrefab, data.position, data.rotation);
                    ConveyorBeltView view = viewObject.GetComponent<ConveyorBeltView>();
                    view.conveyorId = id;
                    activeViews[id] = viewObject;
                }
            }
            else
            {
                // Remove view if it exists
                if (activeViews.TryGetValue(id, out GameObject viewObject))
                {
                    Destroy(viewObject);
                    activeViews.Remove(id);
                }
            }
        }
        
        // Similar logic for other entity types...
    }
    
    // Method to create a new conveyor belt view
    public GameObject CreateConveyorBeltView(int conveyorId)
    {
        if (Simulation.SimulationManager.Instance.conveyorBelts.TryGetValue(conveyorId, out ConveyorBeltData data))
        {
            GameObject viewObject = Instantiate(conveyorBeltPrefab, data.position, data.rotation);
            ConveyorBeltView view = viewObject.GetComponent<ConveyorBeltView>();
            view.conveyorId = conveyorId;
            activeViews[conveyorId] = viewObject;
            return viewObject;
        }
        return null;
    }

    public GameObject CreateMinerView(int minerId)
    {
        if (Simulation.SimulationManager.Instance.miners.TryGetValue(minerId, out MinerData data))
        {
            GameObject viewObject = Instantiate(minerPrefab, data.position, data.rotation);
            MinerView view = viewObject.GetComponent<MinerView>();
            view.minerId = minerId;
            activeViews[minerId] = viewObject;
            return viewObject;
        }
        return null;
    }
    // Method to create a new storage box view
    public GameObject CreateStorageBoxView(int storageId)
    {
        if (Simulation.SimulationManager.Instance.storageBoxes.TryGetValue(storageId, out StorageBoxData data))
        {
            GameObject viewObject = Instantiate(storageBoxPrefab, data.position, data.rotation);
            StorageBoxView view = viewObject.GetComponent<StorageBoxView>();
            view.storageId = storageId;
            activeViews[storageId] = viewObject;
            return viewObject;
        }
        return null;
    }
    
    // Similar methods for other entity types...
}