using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GridComputeManager))]
public class GridRenderer : MonoBehaviour
{
    private GridComputeManager gridManager;
    public Tilemap tilemap;
    public TileBase waterTile;
    public TileBase sandTile;
    public TileBase grassTile;
    public TileBase rockTile;
    private GridComputeManager.GridCell[,] gridData;  // Changed from GridCell to GridComputeManager.GridCell

    private void Start()
    {
        gridManager = GetComponent<GridComputeManager>();
        if (gridManager.gridBuffer == null)
        {
            gridManager.InitializeCompute();
        }
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        // Get the grid data
        GridComputeManager.GridCell[] rawGridData = new GridComputeManager.GridCell[gridManager.gridWidth * gridManager.gridHeight];
        gridManager.GetGridData(out rawGridData);
        
        // Convert to 2D array for easier access
        gridData = new GridComputeManager.GridCell[gridManager.gridWidth, gridManager.gridHeight];
        
        for (int y = 0; y < gridManager.gridHeight; y++)
        {
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                // Consistent coordinate calculation
                Vector3Int tilePosition = new Vector3Int(x - gridManager.gridWidth/2, y - gridManager.gridHeight/2, 0);
                GridComputeManager.GridCell cell = rawGridData[y * gridManager.gridWidth + x];
                gridData[x, y] = cell;
                
                // Set the base tile type
                TileBase tileToUse = null;
                switch (cell.groundType)
                {
                    case 0: // Water
                        tileToUse = waterTile;
                        break;
                    case 1: // Sand
                        tileToUse = sandTile;
                        break;
                    case 2: // Grass
                        tileToUse = grassTile;
                        break;
                    case 3: // Rock
                        tileToUse = rockTile;
                        break;
                }
                
                if (tileToUse != null)
                {
                    tilemap.SetTile(tilePosition, tileToUse);
                    
                    // If there's a resource, tint the tile
                    if (cell.resourceType > 0 && cell.resourceAmount > 0)
                    {
                        Color tintColor = GetResourceColor(cell.resourceType);
                        tilemap.SetTileFlags(tilePosition, TileFlags.None);
                        tilemap.SetColor(tilePosition, Color.Lerp(Color.white, tintColor, cell.resourceAmount * 0.7f));
                    }
                }
            }
        }
    }

    private Color GetResourceColor(int resourceType)
    {
        switch (resourceType)
        {
            case 1: // Coal
                return new Color(0.2f, 0.2f, 0.2f);
            case 2: // Iron
                return new Color(0.8f, 0.4f, 0.2f);
            case 3: // Copper
                return new Color(0.8f, 0.5f, 0.2f);
            case 4: // Stone
                return new Color(0.5f, 0.5f, 0.5f);
            default:
                return Color.white;
        }
    }

    public void UpdateGrid()
    {
        GridComputeManager.GridCell[] rawGridData = new GridComputeManager.GridCell[gridManager.gridWidth * gridManager.gridHeight];
        gridManager.GetGridData(out rawGridData);

        for (int y = 0; y < gridManager.gridHeight; y++)
        {
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                Vector3Int tilePosition = new Vector3Int(x - gridManager.gridWidth/2, y - gridManager.gridHeight/2, 0);
                GridComputeManager.GridCell cell = rawGridData[y * gridManager.gridWidth + x];
                gridData[x, y] = cell;

                // Update tile color for resource changes
                if (cell.resourceType > 0)
                {
                    Color tintColor = GetResourceColor(cell.resourceType);
                    tilemap.SetTileFlags(tilePosition, TileFlags.None);
                    tilemap.SetColor(tilePosition, Color.Lerp(Color.white, tintColor, cell.resourceAmount * 0.7f));
                }
            }
        }
    }
}