using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages grid computation using compute shaders
/// </summary>
public class GridComputeManager : MonoBehaviour
{
    public ComputeShader gridComputeShader;
    // Change these values to 10 for a 10x10 grid
    public int gridWidth = 10;
    public int gridHeight = 10;
    public ComputeBuffer gridBuffer;
    public RenderTexture resultTexture;
    public TerrainGenerationParams initTerrainParams;
    [StructLayout(LayoutKind.Sequential)]
    public struct GridCell
    {
        public int groundType;
        public Vector3 color; 
        public int resourceType; // 0: none, 1: coal, 2: iron, 3: copper, 4: stone
        public int resourceAmount; 
    }
  
    public RawImage mapPreview; 
    
    private void Start()
    {
        InitializeCompute();
        // Load terrain parameters from PlayerPrefs
        string paramsJson = PlayerPrefs.GetString("TerrainParams");
        if (!string.IsNullOrEmpty(paramsJson))
        {
            TerrainGenerationParams loadedParams = ScriptableObject.CreateInstance<TerrainGenerationParams>();
            JsonUtility.FromJsonOverwrite(paramsJson, loadedParams);
            GenerateNewMap(loadedParams);
        }
        else
        {
            GenerateNewMap(initTerrainParams); 
        }
    }
    
    public void GenerateNewMap(TerrainGenerationParams terrainParams)
    {
       
        if (terrainParams.seed == 0f)
        {
            terrainParams.seed = UnityEngine.Random.Range(-10000f, 10000f);
        }
        
      
        if (gridBuffer == null)
        {
            InitializeCompute();
        }
        
        // Set parameters
        gridComputeShader.SetInt("gridWidth", gridWidth);
        gridComputeShader.SetInt("gridHeight", gridHeight);
        gridComputeShader.SetFloat("seed", terrainParams.seed);
        
        gridComputeShader.SetFloat("waterLevel", terrainParams.waterLevel);
        gridComputeShader.SetFloat("waterFrequency", terrainParams.waterFrequency);

        gridComputeShader.SetFloat("coalFrequency",terrainParams.coalFrequency);
        gridComputeShader.SetFloat("coalThreshold",terrainParams.coalThreshold);
        gridComputeShader.SetFloat("coalRichness",terrainParams.coalRichness);
        gridComputeShader.SetFloat("ironFrequency",terrainParams.ironFrequency);
        gridComputeShader.SetFloat("ironThreshold",terrainParams.ironThreshold);
        gridComputeShader.SetFloat("ironRichness",terrainParams.ironRichness);
        gridComputeShader.SetFloat("copperFrequency",terrainParams.copperFrequency);
        gridComputeShader.SetFloat("copperThreshold",terrainParams.copperThreshold);
        gridComputeShader.SetFloat("copperRichness", terrainParams.copperRichness);
        gridComputeShader.SetFloat("stoneFrequency", terrainParams.stoneFrequency);
        gridComputeShader.SetFloat("stoneThreshold", terrainParams.stoneThreshold);
        gridComputeShader.SetFloat("stoneRichness", terrainParams.stoneRichness);
        gridComputeShader.SetFloat("startingAreaEnabled", 1.0f); // Enable starting area
        // Change this line
        gridComputeShader.SetFloat("startingAreaRadius", 3.0f);  // Set radius to 3 tiles from center
        
        // To this
        gridComputeShader.SetFloat("startingAreaRadius", 5.0f);  // Increased to 5 tiles from center
        gridComputeShader.SetFloat("resourcePatchSize", terrainParams.resourcePatchSize);
        
        // Dispatch compute shader
        int threadGroupsX = Mathf.CeilToInt(gridWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(gridHeight / 8.0f);
        gridComputeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        
        // Update preview if assigned
        if (mapPreview != null)
        {
            mapPreview.texture = resultTexture;
        }
    }
    
    public void RegenerateWithNewSeed()
    {
        initTerrainParams.seed = UnityEngine.Random.Range(-10000f, 10000f);
        GenerateNewMap(initTerrainParams);
    }

    public void InitializeCompute()
    {
        if (gridComputeShader == null)
        {
            Debug.LogError("Compute shader is missing!");
            return;
        }
        
        int stride = sizeof(int) + sizeof(float) * 3 + sizeof(int) + sizeof(float);
        gridBuffer = new ComputeBuffer(gridWidth * gridHeight, stride);
    
        // Create render texture for visualization
        resultTexture = new RenderTexture(gridWidth, gridHeight, 0, RenderTextureFormat.ARGB32);
        resultTexture.enableRandomWrite = true;
        resultTexture.Create();
    
        // Set compute shader parameters
        int kernelIndex = gridComputeShader.FindKernel("CSMain");
        gridComputeShader.SetBuffer(kernelIndex, "GridBuffer", gridBuffer);
        gridComputeShader.SetTexture(kernelIndex, "Result", resultTexture);
        gridComputeShader.SetInt("gridWidth", gridWidth);
        gridComputeShader.SetInt("gridHeight", gridHeight);
    }

    private void OnDisable()
    {
        CleanupResources();
    }
    
    private void CleanupResources()
    {
        if (gridBuffer != null)
        {
            gridBuffer.Release();
            gridBuffer = null;
        }
        
        if (resultTexture != null)
        {
            resultTexture.Release();
            resultTexture = null;
        }
    }

    // Method to get grid data if needed
    public void GetGridData(out GridCell[] gridData)
    {
        gridData = new GridCell[gridWidth * gridHeight];
        gridBuffer.GetData(gridData);
    }

    // Method to update time parameter for animations if needed
    public void UpdateTime(float time)
    {
        gridComputeShader.SetFloat("time", time);
    }



    public void GetResourceInfo(int x, int y, out int resourceType, out float resourceAmount)
    {
        resourceType = 0;
        resourceAmount = 0;
        
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return;
            
        GridCell[] cells = new GridCell[1];
        gridBuffer.GetData(cells, 0, x + y * gridWidth, 1);
        
        resourceType = cells[0].resourceType;
        resourceAmount = cells[0].resourceAmount;
    }

    public int GetGridWidth()
    {
       
        return gridWidth;
    }
    
    public int GetGridHeight()
    {
       
        return gridHeight; 
    }
    
    public void RenderToTexture(RenderTexture outputTexture)
    { 
        int kernelIndex = gridComputeShader.FindKernel("CSMain");
        // Set the output texture on your compute shader
        gridComputeShader.SetTexture(kernelIndex, "Result", outputTexture);
        
        // In the InitializeComputeShader method or wherever you dispatch the shader
        gridComputeShader.Dispatch(kernelIndex, Mathf.CeilToInt(gridWidth / 8f), Mathf.CeilToInt(gridHeight / 8f), 1);
    }
    
    public void UpdateCell(int x, int y)
    {
        // Implement logic to update a specific cell in the grid
        // This might involve modifying the grid buffer and re-dispatching the compute shader
    }

    public void GenerateResources()
    {
        int kernelIndex = gridComputeShader.FindKernel("CSMain");
        // Set up starting area parameters
        int centerX = gridWidth / 2;
        int centerY = gridHeight / 2;
        int startingAreaRadius = Mathf.Min(gridWidth, gridHeight) / 8; // 1/8th of the smaller dimension
        int patchSize = 3; // Size of each resource patch
        
        // Pass parameters to compute shader
        gridComputeShader.SetVector("startingAreaParams", new Vector4(centerX, centerY, startingAreaRadius, patchSize));
        
        // Dispatch compute shader
        int threadGroupsX = Mathf.CeilToInt(gridWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(gridHeight / 8.0f);
        gridComputeShader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);
        
    }
}
