using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a single tile in the grid with ground and resource properties
/// </summary>
public class GridTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Cell data
    [SerializeField] private int groundType;
    [SerializeField] private int resourceType;
    [SerializeField] private float resourceAmount;
    
    // Coordinates in the grid
    [SerializeField] private int gridX;
    [SerializeField] private int gridY;
    
    // References to renderers
    private Renderer groundRenderer;
    private GameObject resourceObject;
    private Renderer resourceRenderer;
    
    // Materials
    private Material groundMaterial;
    private Material resourceMaterial;

    private static UITooltip tooltip;
    private GridComputeManager.GridCell cellData;


    private void Awake()
    {
        if (tooltip == null)
        {
            // Find or create the tooltip
            tooltip = FindAnyObjectByType<UITooltip>();
            if (tooltip == null)
            {
                GameObject tooltipObj = new GameObject("TileTooltip");
                tooltip = tooltipObj.AddComponent<UITooltip>();
            }
        }
    }

    private bool hasBeenModified = false;
    private int originalResourceAmount;

    public void Initialize(int x, int y, GridComputeManager.GridCell cellData, Material groundMaterial, Material resourceMaterial)
    {
        this.gridX = x;
        this.gridY = y;
        this.cellData = cellData;
        
        // Set coordinates
        gridX = x;
        gridY = y;
        
        // Set cell data
        groundType = cellData.groundType;
        resourceType = cellData.resourceType;
        resourceAmount = cellData.resourceAmount;
      
        // Only create ground renderer if we have a material
        if (groundMaterial != null)
        {
            if (groundRenderer == null)
            {
                GameObject groundObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                groundObj.name = $"Ground ({x},{y})";
                groundObj.transform.SetParent(transform);
                groundObj.transform.localPosition = Vector3.zero;
                groundObj.transform.localRotation = Quaternion.Euler(90, 0, 0); // Face upward
                groundObj.transform.localScale = Vector3.one;
                
                groundRenderer = groundObj.GetComponent<Renderer>();
            }
            
           
            if (groundRenderer != null)
            {
                groundRenderer.material = groundMaterial;
            }
        }
        
        // Debug log to check resource data
        if (resourceType > 0) {
            Debug.Log($"Initializing tile ({x},{y}) with resource type {resourceType}, amount {resourceAmount}");
        }
        
        // Make sure resource materials are being passed and used
        if (resourceType > 0 && resourceMaterial != null)
        {
            Debug.Log($"Creating resource object with material: {resourceMaterial.name}");
            CreateResourceObject(resourceMaterial);
        }
        else if (resourceType > 0)
        {
            Debug.LogWarning($"Resource material is null for resource type {resourceType} at ({x}, {y})");
        }
    }
   
    private void CreateResourceObject(Material resourceMat)
    {
        // Only create if we don't already have one
        if (resourceObject == null)
        {
            resourceObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            resourceObject.transform.SetParent(transform);
            
            // Position slightly above ground to prevent z-fighting
            resourceObject.transform.localPosition = new Vector3(0, 0.05f, 0); // Increased height
            
            // Make sure rotation is correct - face upward
            resourceObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            // Make resource visible but smaller than the ground
            resourceObject.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            
            resourceRenderer = resourceObject.GetComponent<Renderer>();
            resourceMaterial = resourceMat;
            
            if (resourceRenderer != null && resourceMaterial != null)
            {
                resourceRenderer.material = resourceMaterial;
                Debug.Log($"Resource renderer material set to: {resourceMaterial.name}");
            }
            else
            {
                Debug.LogError("Failed to set resource material: renderer or material is null");
            }
            
            // Scale based on resource amount (optional)
            float scale = Mathf.Clamp01(resourceAmount / 100f) * 0.3f + 0.5f;
            resourceObject.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
    
    // Remove resource (e.g., when mined)
    public void RemoveResource()
    {
        if (resourceObject != null)
        {
            Destroy(resourceObject);
            resourceObject = null;
        }
        
        resourceType = 0;
        resourceAmount = 0;
    }
    
    // Update the tile with new data
    public void UpdateTile(GridComputeManager.GridCell newData, Material groundMat, Material resourceMat = null)
    {
        // Update ground if changed
        if (groundType != newData.groundType)
        {
            groundType = newData.groundType;
            groundMaterial = groundMat;
            if (groundRenderer != null)
            {
                groundRenderer.material = groundMaterial;
            }
        }
        
        // Update resource if changed
        if (resourceType != newData.resourceType || Mathf.Abs(resourceAmount - newData.resourceAmount) > 0.01f)
        {
            resourceType = newData.resourceType;
            resourceAmount = newData.resourceAmount;
            
            // Remove existing resource object
            if (resourceObject != null)
            {
                Destroy(resourceObject);
                resourceObject = null;
            }
            
            // Create new resource if needed
            if (resourceType > 0 && resourceMat != null)
            {
                CreateResourceObject(resourceMat);
            }
        }
    }
    
    // Get grid coordinates
    public Vector2Int GetGridCoordinates()
    {
        return new Vector2Int(gridX, gridY);
    }
    
    // Get cell data
    public GridComputeManager.GridCell GetCellData()
    {
        return cellData;
    }
    
    // Handle mouse interactions (optional)
    private void OnMouseDown()
    {
        // Example: Log cell info when clicked
        Debug.Log($"Clicked on tile at ({gridX}, {gridY}): Ground={groundType}, Resource={resourceType}, Amount={resourceAmount}");
        
        // You could trigger events here for building placement, resource gathering, etc.
    }
    
    private void OnDestroy()
    {
        if (resourceObject != null)
        {
            Destroy(resourceObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.Show(GetTooltipText());
        }
        else if (UITooltip.Instance != null)
        {
            UITooltip.Instance.Show(GetTooltipText());
        }
    }

    private string GetTooltipText()
    {
        string groundTypeName = GetGroundTypeName(groundType);
        string resourceTypeName = GetResourceTypeName(resourceType);
        
        string tooltipText = $"Tile ({gridX},{gridY})\nGround: {groundTypeName}";
        if (!string.IsNullOrEmpty(resourceTypeName))
        {
            tooltipText += $"\nResource: {resourceTypeName} ({resourceAmount:0.0})";
        }
        
        return tooltipText;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }

    // Make these methods public so they can be called from elsewhere
    public string GetGroundTypeName(int groundType)
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

    public string GetResourceTypeName(int resourceType)
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

    public int GetX()
    {
        return gridX;
    }

    public int GetY()
    {
        return gridY;
    }

    public bool HasBeenModified()
    {
        return hasBeenModified;
    }
}