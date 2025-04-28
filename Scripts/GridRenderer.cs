using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GridComputeManager))]
public class GridRenderer : MonoBehaviour
{
    private GridComputeManager _gridManager;
   
    private GridComputeManager.GridCell[,] _gridData;  // Changed from GridCell to GridComputeManager.GridCell

    private void Start()
    {
        _gridManager = GetComponent<GridComputeManager>();
        if (_gridManager.GridBuffer == null)
        {
            _gridManager.InitializeCompute();
        }
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        // Get the grid data
        GridComputeManager.GridCell[] rawGridData = new GridComputeManager.GridCell[_gridManager.gridWidth * _gridManager.gridHeight];
        _gridManager.GetGridData(out rawGridData);
        
        // Convert to 2D array for easier access
        _gridData = new GridComputeManager.GridCell[_gridManager.gridWidth, _gridManager.gridHeight];
        
        for (int y = 0; y < _gridManager.gridHeight; y++)
        {
            for (int x = 0; x < _gridManager.gridWidth; x++)
            {
                // Consistent coordinate calculation
                Vector3Int tilePosition = new Vector3Int(x - _gridManager.gridWidth/2, y - _gridManager.gridHeight/2, 0);
                GridComputeManager.GridCell cell = rawGridData[y * _gridManager.gridWidth + x];
                _gridData[x, y] = cell;
                
             
           
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
        GridComputeManager.GridCell[] rawGridData = new GridComputeManager.GridCell[_gridManager.gridWidth * _gridManager.gridHeight];
        _gridManager.GetGridData(out rawGridData);

        for (int y = 0; y < _gridManager.gridHeight; y++)
        {
            for (int x = 0; x < _gridManager.gridWidth; x++)
            {
                Vector3Int tilePosition = new Vector3Int(x - _gridManager.gridWidth/2, y - _gridManager.gridHeight/2, 0);
                GridComputeManager.GridCell cell = rawGridData[y * _gridManager.gridWidth + x];
                _gridData[x, y] = cell;

                // Update tile color for resource changes
                if (cell.resourceType > 0)
                {
                    Color tintColor = GetResourceColor(cell.resourceType);
                   
                }
            }
        }
    }

  
}