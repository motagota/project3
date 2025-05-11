using Simulation;
using UnityEngine;

public class ConveyorItemView : MonoBehaviour
{
    public int itemId;
    
    private void Update()
    {
        // Only update visuals if this item is in the simulation
        if (Simulation.SimulationManager.Instance.conveyorItems.TryGetValue(itemId, out ConveyorItemData data))
        {
            // Update visual position and rotation
            transform.position = data.position;
            transform.rotation = data.rotation;
        }
        else
        {
            // Item no longer exists in simulation, destroy the view
            Destroy(gameObject);
        }
    }
}