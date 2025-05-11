using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct StorageBoxData
{
    public int id;
    public Vector3 position;
    public Quaternion rotation;
    public Dictionary<int, float> storedResources; // Resource type -> amount
    public float maxCapacity;
    public Vector3 inputPointPosition;
    public Vector3 outputPointPosition;
    public List<int> connectedConveyorIds;
    public bool isActive;
}