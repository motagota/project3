using Simulation;
using UnityEngine;

public class MinerView : MonoBehaviour
{
    public int minerId;
    
   private void Update()
    {
        // Only update visuals if this miner is in the simulation
        if (Simulation.SimulationManager.Instance.miners.TryGetValue(minerId, out MinerData data))
        {
            // Update visual position and rotation
            transform.position = data.position;
            transform.rotation = data.rotation;
           
        }
    }
}