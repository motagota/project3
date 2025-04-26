using UnityEngine;

/// <summary>
/// Scriptable object for storing terrain generation parameters
/// </summary>
[CreateAssetMenu(fileName = "TerrainParams", menuName = "Game/Terrain Generation Parameters")]
public class TerrainGenerationParams : ScriptableObject
{
    [Header("Seed Settings")]
    public float seed = 0f;

    [Header("Water Settings")]
    [Range(0.1f, 0.5f)]
    public float waterLevel = 0.3f;
    [Range(0.01f, 0.1f)]
    public float waterFrequency = 0.03f;
    [Range(0.05f, 0.2f)]
    public float sandBorderSize = 0.1f;

    [Header("Coal Settings")]
    [Range(0.01f, 0.2f)]
    public float coalFrequency = 0.08f;  // Controls number of patches
    [Range(0.5f, 0.9f)]
    public float coalThreshold = 0.7f;   // Controls size of patches
    [Range(0.1f, 1.0f)]
    public float coalRichness = 0.5f;    // Controls density within patches

    [Header("Iron Settings")]
    [Range(0.01f, 0.2f)]
    public float ironFrequency = 0.06f;
    [Range(0.5f, 0.9f)]
    public float ironThreshold = 0.75f;
    [Range(0.1f, 1.0f)]
    public float ironRichness = 0.5f;

    [Header("Copper Settings")]
    [Range(0.01f, 0.2f)]
    public float copperFrequency = 0.07f;
    [Range(0.5f, 0.9f)]
    public float copperThreshold = 0.73f;
    [Range(0.1f, 1.0f)]
    public float copperRichness = 0.5f;

    [Header("stone Settings")]
    [Range(0.01f, 0.2f)]
    public float stoneFrequency;
    [Range(0.5f, 0.9f)]
    public float stoneThreshold;
    [Range(0.1f, 1.0f)]
    public float stoneRichness;

    // Starting area parameters
    public bool startingAreaEnabled = true;
    public float startingAreaRadius = 10f;
    public float resourcePatchSize = 3f;
    
    [Header("General Terrain")]
    [Range(0.01f, 0.1f)]
    public float terrainScale = 0.03f;
    [Range(0.5f, 0.9f)]
    public float rockThreshold = 0.7f;

    public float sandLevel;
    
    private void OnValidate()
    {
        // Ensure values stay within valid ranges
        waterLevel = Mathf.Clamp(waterLevel, 0.1f, 0.5f);
        waterFrequency = Mathf.Clamp(waterFrequency, 0.01f, 0.1f);
        sandBorderSize = Mathf.Clamp(sandBorderSize, 0.05f, 0.2f);
    }
}