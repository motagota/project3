using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles grid rendering using a chunking system for performance optimization
/// </summary>
[RequireComponent(typeof(GridComputeManager))]
public class CustomGridRenderer : MonoBehaviour
{
    private GridComputeManager _gridManager;
    
    // Materials for different ground types
    public Material waterMaterial;
    public Material sandMaterial;
    public Material grassMaterial;
    public Material rockMaterial;

    // Materials for resources
    public Material coalMaterial;
    public Material ironMaterial;
    public Material copperMaterial;
    public Material stoneMaterial;

    // Store instantiated tiles for updates
    private Dictionary<Vector2Int, GridTile> _gridTiles = new Dictionary<Vector2Int, GridTile>();
    
    // Chunking parameters
    [Header("Chunking Settings")]
    public int chunkSize = 10; // Set to 10 to have a single chunk for a 10x10 grid
    public float visibleChunkDistance = 1f; // Reduced since we have fewer chunks
    private Dictionary<Vector2Int, GameObject> _chunks = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int _currentCenterChunk = new Vector2Int(0, 0);
    
    
    // Debug options
    [Header("Debug Options")]
    public bool debugMode = true;
    public float tileScale = 1.0f;
    public float tileSpacing = 0.2f; 
  
    public GameObject tileInteractionPrefab;

    private TopDownCameraController _cameraController;

    
    private void Start()
    {
        if (!ValidateMaterials())
        {
            Debug.LogError("Required materials are missing!");
            return;
        }
        
        _gridManager = GetComponent<GridComputeManager>();
        
        if (_gridManager == null)
        {
            Debug.LogError("GridComputeManager not found!");
            return;
        }
       
        _cameraController = FindAnyObjectByType<TopDownCameraController>();
        if (_cameraController != null)
        {
            _cameraController.gridCenter = transform;
        }

        // Initial grid rendering with chunks
        RenderGridWithChunks();
    }

    private void Update()
    {
        if (_cameraController != null)
        {         
            UpdateVisibleChunks(_cameraController.transform.position);            
            HandleMouseHover();
        }
    }
    
    private void HandleMouseHover()
    {
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // Since our grid is on the XZ plane (Y = 0), we can calculate the intersection point
        float t = -ray.origin.y / ray.direction.y;
        if (t > 0) // Only process if the ray is pointing downward
        {
            Vector3 worldPoint = ray.origin + t * ray.direction;
            
            // Use the helper method for consistent conversion
            Vector2Int gridCoords = WorldToGridCoordinates(worldPoint);
            int gridX = gridCoords.x;
            int gridY = gridCoords.y;
            
            // Check if we're within grid bounds
            int gridWidth = _gridManager.GetGridWidth();
            int gridHeight = _gridManager.GetGridHeight();
            
            if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
            {
                Vector2Int cellCoords = new Vector2Int(gridX, gridY);
                
                // Get the cell data
                if (_gridTiles.TryGetValue(cellCoords, out GridTile tile))
                {
                    // Show tooltip at mouse position
                    GridComputeManager.GridCell cell = tile.GetCellData();
                    string groundType = GetGroundTypeName(cell.groundType);
                    string resourceType = GetResourceTypeName(cell.resourceType);
                    
                    string tooltipText = $"Tile ({gridX}, {gridY})\nGround: {groundType}";
                    if (!string.IsNullOrEmpty(resourceType))
                    {
                        tooltipText += $"\nResource: {resourceType}";
                    }
                    
                    // Find or create tooltip - Replace FindObjectOfType with FindAnyObjectByType
                    UITooltip tooltip = FindAnyObjectByType<UITooltip>();
                    if (tooltip != null)
                    {
                        tooltip.Show(tooltipText, Input.mousePosition);
                    }
                }
            }
        }
    }
    
    private string GetGroundTypeName(int groundType)
    {
        switch (groundType)
        {
            case 0: return "Water";
            case 1: return "Sand";
            case 2: return "Grass";
            case 3: return "Rock";
            default: return "Unknown";
        }
    }

    private string GetResourceTypeName(int resourceType)
    {
        switch (resourceType)
        {
            case 0: return "";
            case 1: return "Coal";
            case 2: return "Iron";
            case 3: return "Copper";
            case 4: return "Stone";
            default: return "Unknown";
        }
    }

    // Clears existing tiles and renders the entire grid using chunks
    public void RenderGridWithChunks()
    {
        // Clear previous chunks and tiles
        ClearGrid();

        GridComputeManager.GridCell[] gridData;
        _gridManager.GetGridData(out gridData); // Get the latest grid data

        int gridWidth = _gridManager.GetGridWidth();
        int gridHeight = _gridManager.GetGridHeight();

        if (debugMode)
        {
            Debug.Log($"Rendering grid with chunks: {gridWidth}x{gridHeight}, Total cells: {gridData.Length}");
        }
        
        // Calculate how many chunks we need
        int chunksX = Mathf.CeilToInt((float)gridWidth / chunkSize);
        int chunksY = Mathf.CeilToInt((float)gridHeight / chunkSize);
        
        // Create chunks around the center
        Vector2Int centerChunk = new Vector2Int(chunksX / 2, chunksY / 2);
        _currentCenterChunk = centerChunk;
        
        int chunkRadius = Mathf.CeilToInt(visibleChunkDistance);
        for (int cy = centerChunk.y - chunkRadius; cy <= centerChunk.y + chunkRadius; cy++)
        {
            for (int cx = centerChunk.x - chunkRadius; cx <= centerChunk.x + chunkRadius; cx++)
            {
                if (cx >= 0 && cx < chunksX && cy >= 0 && cy < chunksY)
                {
                    CreateChunk(new Vector2Int(cx, cy), gridData, gridWidth, gridHeight);
                }
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"Grid rendering complete. Chunks created: {_chunks.Count}");
        }
    }
    
    private void CreateChunk(Vector2Int chunkCoord, GridComputeManager.GridCell[] gridData, int gridWidth, int gridHeight)
    {
        // Calculate chunk boundaries in grid coordinates
        int startX = chunkCoord.x * chunkSize;
        int startY = chunkCoord.y * chunkSize;
        int endX = Mathf.Min(startX + chunkSize, gridWidth);
        int endY = Mathf.Min(startY + chunkSize, gridHeight);
        
        // Create a new GameObject for this chunk
        GameObject chunkObject = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        chunkObject.transform.SetParent(transform);
        chunkObject.transform.localPosition = Vector3.zero;
        
        // Add mesh components
        MeshFilter meshFilter = chunkObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        
        // Set up the mesh renderer to use multiple materials
        meshRenderer.materials = new Material[] 
        { 
            waterMaterial, 
            sandMaterial, 
            grassMaterial, 
            rockMaterial 
        };
        
        // Create the ground mesh for this chunk
        CreateChunkMesh(meshFilter, startX, startY, endX, endY, gridData, gridWidth);
        
        // Create interaction objects for this chunk
        CreateChunkInteractionObjects(chunkObject.transform, startX, startY, endX, endY, gridData, gridWidth);
        
        // Store the chunk
        _chunks[chunkCoord] = chunkObject;
    }
    
    private void CreateChunkMesh(MeshFilter meshFilter, int startX, int startY, int endX, int endY, 
                                GridComputeManager.GridCell[] gridData, int gridWidth)
    {
        // Create mesh data structures
        List<Vector3> vertices = new List<Vector3>();
        List<int>[] triangles = new List<int>[4]; // One list per material (water, sand, grass, rock)
        List<Vector2> uvs = new List<Vector2>();
        
        for (int i = 0; i < 4; i++)
        {
            triangles[i] = new List<int>();
        }
        
        // Generate mesh data for this chunk
        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                int index = y * gridWidth + x;
                
                if (index >= gridData.Length)
                {
                    Debug.LogError($"Index out of bounds: {index} >= {gridData.Length}");
                    continue;
                }
                
                GridComputeManager.GridCell cell = gridData[index];
                
                // Calculate world position for the current tile with proper spacing
                // We offset by half the grid width to center the grid at origin,
                // then apply scaling and spacing to position each tile correctly
                float worldX = (x - gridWidth / 2f) * (tileScale + tileSpacing);             
                float worldY = (y - gridWidth / 2f + 0.5f) * (tileScale + tileSpacing);
                float halfSize = tileScale / 2f;
                
                // Track vertex count for triangle indices
                int vertCount = vertices.Count;
                
                // Add 4 vertices for the quad (counter-clockwise)
                vertices.Add(new Vector3(worldX - halfSize, 0, worldY - halfSize)); // Bottom-left (0)
                vertices.Add(new Vector3(worldX - halfSize, 0, worldY + halfSize)); // Top-left (1)
                vertices.Add(new Vector3(worldX + halfSize, 0, worldY + halfSize)); // Top-right (2)
                vertices.Add(new Vector3(worldX + halfSize, 0, worldY - halfSize)); // Bottom-right (3)
                
                // Add UVs (matching the new vertex order)
                uvs.Add(new Vector2(0, 0)); // Bottom-left
                uvs.Add(new Vector2(0, 1)); // Top-left
                uvs.Add(new Vector2(1, 1)); // Top-right
                uvs.Add(new Vector2(1, 0)); // Bottom-right
                
                // Add triangles to the appropriate submesh based on ground type
                int submeshIndex = Mathf.Clamp(cell.groundType, 0, 3);
                
                // First triangle (counter-clockwise: bottom-left, top-left, top-right)
                triangles[submeshIndex].Add(vertCount);     // Bottom-left (0)
                triangles[submeshIndex].Add(vertCount + 1); // Top-left (1)
                triangles[submeshIndex].Add(vertCount + 2); // Top-right (2)
                
                // Second triangle (counter-clockwise: bottom-left, top-right, bottom-right)
                triangles[submeshIndex].Add(vertCount);     // Bottom-left (0)
                triangles[submeshIndex].Add(vertCount + 2); // Top-right (2)
                triangles[submeshIndex].Add(vertCount + 3); // Bottom-right (3)
            }
        }
        
        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.name = $"ChunkMesh_{startX}_{startY}";
        
        // Check if we have too many vertices for a 16-bit index buffer
        if (vertices.Count > 65535)
        {
            Debug.LogWarning($"Chunk has {vertices.Count} vertices, which exceeds the 16-bit limit. Using 32-bit index format.");
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
        
        // Set vertices and UVs
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        
        // Set submeshes (one per material)
        mesh.subMeshCount = 4;
        for (int i = 0; i < 4; i++)
        {
            mesh.SetTriangles(triangles[i], i);
        }
        
        // Recalculate normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // Assign the mesh to the mesh filter
        meshFilter.mesh = mesh;
    }
    
    private void CreateChunkInteractionObjects(Transform chunkTransform, int startX, int startY, int endX, int endY, 
                                             GridComputeManager.GridCell[] gridData, int gridWidth)
    {
        // Only create interaction objects if we have a prefab
        if (tileInteractionPrefab == null)
        {
            return;
        }
        
        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                int index = y * gridWidth + x;
                
                if (index >= gridData.Length)
                {
                    continue;
                }
                
                GridComputeManager.GridCell cell = gridData[index];
                Vector2Int cellCoords = new Vector2Int(x, y);
                
                // Calculate world position
                float worldX = (x - gridWidth / 2f + 0.5f) * (tileScale + tileSpacing); 
                float worldY = (y - gridWidth / 2f + 0.5f) * (tileScale + tileSpacing);
                Vector3 position = new Vector3(worldX, 0.01f, worldY); // Slightly above ground
                
                // Create interaction object
                GameObject interactionObject = Instantiate(tileInteractionPrefab, position, Quaternion.identity, chunkTransform);
                
                // Set a more descriptive name with ground type information
                string groundTypeName = GetGroundTypeName(cell.groundType);
                string resourceInfo = cell.resourceType > 0 ? $" ({GetResourceTypeName(cell.resourceType)})" : "";
                interactionObject.name = $"Ground Tile ({x},{y}) - {groundTypeName}{resourceInfo}";
                
                // Set the layer to "GridTile"
                interactionObject.layer = LayerMask.NameToLayer("GridTile");
                
                // Add BoxCollider for mouse interaction
                BoxCollider boxCollider = interactionObject.GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    boxCollider = interactionObject.AddComponent<BoxCollider>();
                }
                // Set collider size to match tile scale
                boxCollider.size = new Vector3(tileScale, 0.1f, tileScale);
                boxCollider.isTrigger = true; // Make it a trigger so it doesn't affect physics
                
                // Add GridTile component
                GridTile gridTile = interactionObject.GetComponent<GridTile>();
                if (gridTile == null)
                {
                    gridTile = interactionObject.AddComponent<GridTile>();
                }
                
                // Initialize with data
                Material resourceMaterial = cell.resourceType > 0 ? GetResourceMaterial(cell.resourceType) : null;
                Material groundMaterial = GetGroundMaterial(cell.groundType); // Get appropriate ground material
                gridTile.Initialize(x, y, cell, groundMaterial, resourceMaterial);
                _gridTiles[cellCoords] = gridTile;
                                
            }
        }
    }

    // Helper to destroy all instantiated tiles and chunks
    private void ClearGrid()
    {
        foreach (var tile in _gridTiles.Values)
        {
            if (tile != null && tile.gameObject != null)
                Destroy(tile.gameObject);
        }
        _gridTiles.Clear();
        
        foreach (var chunk in _chunks.Values)
        {
            if (chunk != null)
                Destroy(chunk);
        }
        _chunks.Clear();
    }
    
    // Updates chunks based on a new center position (e.g., camera or player position)
    public void UpdateVisibleChunks(Vector3 centerPosition)
    {
        int gridWidth = _gridManager.GetGridWidth();
        int gridHeight = _gridManager.GetGridHeight();
        
        // Calculate how many chunks we need
        int chunksX = Mathf.CeilToInt((float)gridWidth / chunkSize);
        int chunksY = Mathf.CeilToInt((float)gridHeight / chunkSize);
        
        // Convert world position to chunk coordinates
        float normalizedX = (centerPosition.x / (tileScale + tileSpacing)) + (gridWidth / 2f);
        float normalizedY = (centerPosition.z / (tileScale + tileSpacing)) + (gridHeight / 2f);
        
        Vector2Int centerChunk = new Vector2Int(
            Mathf.FloorToInt(normalizedX / chunkSize),
            Mathf.FloorToInt(normalizedY / chunkSize)
        );
        
        // If we haven't moved to a new chunk, do nothing
        if (centerChunk == _currentCenterChunk)
            return;
            
        _currentCenterChunk = centerChunk;
        
        // Get the grid data if we need to create new chunks
        GridComputeManager.GridCell[] gridData = null;
        
        // Determine which chunks should be visible
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();
        int chunkRadius = Mathf.CeilToInt(visibleChunkDistance);
        
        for (int cy = centerChunk.y - chunkRadius; cy <= centerChunk.y + chunkRadius; cy++)
        {
            for (int cx = centerChunk.x - chunkRadius; cx <= centerChunk.x + chunkRadius; cx++)
            {
                Vector2Int chunkCoord = new Vector2Int(cx, cy);
                
                // Skip if out of bounds
                if (cx < 0 || cx >= chunksX || cy < 0 || cy >= chunksY)
                    continue;
                    
                chunksToKeep.Add(chunkCoord);
                
                // Create chunk if it doesn't exist
                if (!_chunks.ContainsKey(chunkCoord))
                {
                    // Lazy load grid data only when needed
                    if (gridData == null)
                    {
                        gridData = new GridComputeManager.GridCell[gridWidth * gridHeight];
                        _gridManager.GetGridData(out gridData);
                    }
                    
                    CreateChunk(chunkCoord, gridData, gridWidth, gridHeight);
                }
            }
        }
        
        // Remove chunks that are too far away
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunkCoord in _chunks.Keys)
        {
            if (!chunksToKeep.Contains(chunkCoord))
            {
                chunksToRemove.Add(chunkCoord);
            }
        }
        
        foreach (var chunkCoord in chunksToRemove)
        {
            // Remove tiles in this chunk from the dictionary
            int startX = chunkCoord.x * chunkSize;
            int startY = chunkCoord.y * chunkSize;
            int endX = Mathf.Min(startX + chunkSize, gridWidth);
            int endY = Mathf.Min(startY + chunkSize, gridHeight);
            
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    _gridTiles.Remove(new Vector2Int(x, y));
                }
            }
            
            // Destroy the chunk GameObject
            Destroy(_chunks[chunkCoord]);
            _chunks.Remove(chunkCoord);
        }
    }

    // Helper to get the correct ground material
    // Add this as a field in your class
    private Material debugGroundMaterial;
    
    // Then update the GetGroundMaterial method
    private Material GetGroundMaterial(int groundType)
    {
        switch (groundType)
        {
            case 0: return waterMaterial;
            case 1: return sandMaterial;
            case 2: return grassMaterial;
            case 3: return rockMaterial;
            default: 
                // Create debug material only once
                if (debugGroundMaterial == null)
                {
                    debugGroundMaterial = new Material(Shader.Find("Standard"));
                    debugGroundMaterial.color = Color.magenta;
                    debugGroundMaterial.name = "DEBUG_UnknownGroundType";
                }
                Debug.LogWarning($"Unknown ground type: {groundType} - Using debug material");
                return debugGroundMaterial;
        }
    }

    // Helper to get the correct resource material
    private Material GetResourceMaterial(int resourceType)
    {
        Material result;
        
        switch (resourceType)
        {
            case 1: 
                result = coalMaterial; 
                break;
            case 2: 
                result = ironMaterial; 
                break;
            case 3: 
                result = copperMaterial;
                break;
            case 4: 
                result = stoneMaterial;
                break;
            default: 
                result = null; // No material for unknown or none
                break;
        }
        
        if (result == null && resourceType > 0)
        {
            Debug.LogError($"Material for resource type {resourceType} is null! Check inspector assignments.");
            return new Material(Shader.Find("Standard"));
        }
        
        return result;
    }

    // Updates a single cell visually (only for resources, ground doesn't change)
    public void UpdateGridCell(int x, int y)
    {
        Vector2Int cellCoords = new Vector2Int(x, y);
        
        // Check if we have this tile
        if (_gridTiles.TryGetValue(cellCoords, out GridTile tile))
        {
            // Get updated cell data
            int index = y * _gridManager.GetGridWidth() + x;
            GridComputeManager.GridCell[] gridData = new GridComputeManager.GridCell[1];
            _gridManager.GetGridData(out gridData); // This should be optimized to get just one cell
            
            // Update only the resource on the tile
            Material resourceMaterial = gridData[index].resourceType > 0 ? 
                GetResourceMaterial(gridData[index].resourceType) : null;
                
            tile.UpdateTile(gridData[index], null, resourceMaterial);
        }
    }
    
    // Helper method to convert world position to grid coordinates
    public Vector2Int WorldToGridCoordinates(Vector3 worldPosition)
    {
        int gridWidth = _gridManager.GetGridWidth();
        int gridHeight = _gridManager.GetGridHeight();
        
        // Calculate grid coordinates based on world position
        float normalizedX = (worldPosition.x / (tileScale + tileSpacing)) + (gridWidth / 2f);
        float normalizedY = (worldPosition.z / (tileScale + tileSpacing)) + (gridHeight / 2f);
        
        return new Vector2Int(
            Mathf.FloorToInt(normalizedX),
            Mathf.FloorToInt(normalizedY)
        );
    }

    // Validate that all required materials are assigned
    private bool ValidateMaterials()
    {
        // Check ground materials
        if (waterMaterial == null || sandMaterial == null || grassMaterial == null || rockMaterial == null)
        {
            Debug.LogError("One or more ground materials are missing!");
            return false;
        }
        
        // Check resource materials
        if (coalMaterial == null || ironMaterial == null || copperMaterial == null || stoneMaterial == null)
        {
            Debug.LogError("One or more resource materials are missing!");
            return false;
        }
        
        return true;
    }
    
    // And the reverse conversion
    private Vector3 GridToWorldCoordinates(int gridX, int gridY)
    {
        int gridWidth = _gridManager.GetGridWidth();
        int gridHeight = _gridManager.GetGridHeight();
        
        float worldX = (gridX - gridWidth / 2f) * (tileScale + tileSpacing);
        float worldZ = (gridY - gridHeight / 2f) * (tileScale + tileSpacing);
        
        return new Vector3(worldX, 0, worldZ);
    }

    public GridTile GetTileAt(int i, int i1)
    {
        _gridTiles.TryGetValue(new Vector2Int(i, i1), out GridTile tile);
        return tile;
    }
}