using UnityEngine;

[System.Serializable]
public struct ConveyorItemData
{
    public int id;
    public Vector3 position;
    public Quaternion rotation;
    public int resourceType;
    public float resourceAmount;
    public int currentConveyorId; // ID of the conveyor this item is on
    public bool isOnFarLane;
}