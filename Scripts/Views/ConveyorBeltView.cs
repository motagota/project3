using Simulation;
using UnityEngine;

public class ConveyorBeltView : MonoBehaviour
{
    public int conveyorId;
    
    [Header("Visual Settings")]
    public Material conveyorMaterial;
    public GameObject beltMeshObject;
    
    [Header("Connection Points")]
    public Transform inputPointTransform;
    public Transform outputPointTransform;
    
    private void Update()
    {
        // Only update visuals if this conveyor is in the simulation
        if (Simulation.SimulationManager.Instance.conveyorBelts.TryGetValue(conveyorId, out ConveyorBeltData data))
        {
            // Update visual position and rotation
            transform.position = data.position;
            transform.rotation = data.rotation;
            
            // Update connection points
            if (inputPointTransform != null)
                inputPointTransform.position = data.inputPointPosition;
                
            if (outputPointTransform != null)
                outputPointTransform.position = data.outputPointPosition;
            
            // Update conveyor material to scroll
            if (conveyorMaterial != null && beltMeshObject != null)
            {
                float offset = Time.time * data.speed * 0.1f;
                conveyorMaterial.SetTextureOffset("_MainTex", new Vector2(0, offset));
            }
        }
    }
}