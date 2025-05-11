using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct ConveyorBeltData
{
    public int id;
    public Vector3 position;
    public Quaternion rotation;
    public float speed;
    public Vector3 inputPointPosition;
    public Vector3 outputPointPosition;
    public int direction; // 0-3 for N,E,S,W
    public List<int> itemsOnFarLane; // Store IDs instead of references
    public List<int> itemsOnCloseLane;
    public List<int> connectedConveyors;
    public bool isActive;
}