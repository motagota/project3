using UnityEngine;
using System.Collections.Generic;
using Simulation;

public class StorageBoxView : MonoBehaviour
{
    public int storageId;
    
    [Header("Visual Settings")]
    public GameObject storageBoxMeshObject;
    
    [Header("Connection Points")]
    public Transform inputPointTransform;
    public Transform outputPointTransform;
    
    // Visual indicators for storage level
    public Transform fillLevelIndicator;
    
    private void Update()
    {
        // Only update visuals if this storage box is in the simulation
        if (Simulation.SimulationManager.Instance.storageBoxes.TryGetValue(storageId, out StorageBoxData data))
        {
            // Update visual position and rotation
            transform.position = data.position;
            transform.rotation = data.rotation;
            
            // Update connection points
            if (inputPointTransform != null)
                inputPointTransform.position = data.inputPointPosition;
                
            if (outputPointTransform != null)
                outputPointTransform.position = data.outputPointPosition;
            
            // Update fill level indicator if present
            if (fillLevelIndicator != null)
            {
                // Calculate total stored resources
                float totalStored = 0f;
                foreach (var resource in data.storedResources)
                {
                    totalStored += resource.Value;
                }
                
                // Calculate fill percentage
                float fillPercentage = data.maxCapacity > 0 ? totalStored / data.maxCapacity : 0;
                
                // Update the scale of the fill indicator
                Vector3 scale = fillLevelIndicator.localScale;
                scale.y = Mathf.Max(0.01f, fillPercentage);
                fillLevelIndicator.localScale = scale;
            }
        }
    }
}