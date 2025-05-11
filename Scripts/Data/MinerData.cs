using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MinerData
{
    public int id;
    public Vector3 position;
    public Quaternion rotation;
    public float miningRate;
    public float miningTimer;
    public int resourceType;
 
    public bool isActive;
}