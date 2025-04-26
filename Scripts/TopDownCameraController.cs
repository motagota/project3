using UnityEngine;
using UnityEngine.EventSystems;

public class TopDownCameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;
    // Adjust these values for a smaller grid
    public float minZoom = 5f; // Closer zoom for smaller grid
    public float maxZoom = 20f;
    
    [Header("Movement Settings")]
    public float moveSpeed = 20f;
    public float fastMoveSpeed = 40f;
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;
    
    [Header("References")]
    public Transform gridCenter;
    
    private Vector3 _lastMousePosition;
    private Camera _cam;
    private float _currentMoveSpeed;
    private Vector2Int? _hoveredTileCoords = null;
    private UITooltip _tooltip;
    
    private void Start()
    {
        _cam = GetComponent<Camera>();
        
        // Initialize camera position and rotation
        if (gridCenter != null)
        {
            transform.position = gridCenter.position + new Vector3(0, maxZoom, 0);
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        
        // Find or create tooltip - Replace FindObjectOfType with FindAnyObjectByType
        _tooltip = FindAnyObjectByType<UITooltip>();
        if (_tooltip == null)
        {
            GameObject tooltipObj = new GameObject("TileTooltip");
            _tooltip = tooltipObj.AddComponent<UITooltip>();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();
        HandleTileHover();
    }

    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string scrollAxis = "Mouse ScrollWheel";
    public KeyCode fastMoveKey = KeyCode.LeftShift;
    public KeyCode rotateKey = KeyCode.Mouse1;

    private void HandleMovement()
    {
        _currentMoveSpeed = Input.GetKey(fastMoveKey) ? fastMoveSpeed : moveSpeed;
        Vector3 movement = new Vector3(
            Input.GetAxisRaw(horizontalAxis), 
            0, 
            Input.GetAxisRaw(verticalAxis)
        ).normalized;
        
        Vector3 rotatedMovement = Quaternion.Euler(0, transform.eulerAngles.y, 0) * movement;
        
        if (movement != Vector3.zero)
        {
            transform.position += rotatedMovement * _currentMoveSpeed * Time.deltaTime;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis(scrollAxis);
        if (scroll != 0)
        {
            float newSize = _cam.orthographicSize - scroll * zoomSpeed;
            _cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            _lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1) && gridCenter != null)
        {
            Vector3 delta = Input.mousePosition - _lastMousePosition;
            transform.RotateAround(gridCenter.position, Vector3.up, delta.x * rotationSpeed * Time.deltaTime);
            _lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleTileHover()
    {
        // Skip if hovering over UI
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        // Get the mouse position and convert it to a ray
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Cast ray against all colliders
        if (Physics.Raycast(ray, out hit))
        {
            // Check if we hit a GridTile
            GridTile tile = hit.collider.GetComponent<GridTile>();
            if (tile != null)
            {
                Vector2Int coords = tile.GetGridCoordinates();
                
                // Only update if we're hovering over a new tile
                if (!_hoveredTileCoords.HasValue || _hoveredTileCoords.Value != coords)
                {
                    _hoveredTileCoords = coords;
                    
                    // Show tooltip
                    if (_tooltip != null)
                    {
                        GridComputeManager.GridCell cellData = tile.GetCellData();
                        string groundType = tile.GetGroundTypeName(cellData.groundType);
                        string resourceType = tile.GetResourceTypeName(cellData.resourceType);
                        
                        string tooltipText = $"Tile ({coords.x}, {coords.y})\nGround: {groundType}";
                        if (!string.IsNullOrEmpty(resourceType))
                        {
                            tooltipText += $"\nResource: {resourceType}";
                        }
                        else
                        {
                            tooltipText += "\nResource: None";
                        }
                        
                        _tooltip.Show(tooltipText); 
                    }
                }
                return;
            }
        }
        
        // If we get here, we're not hovering over any tile
        if (_hoveredTileCoords.HasValue)
        {
            _hoveredTileCoords = null;
            _tooltip.Hide();
        }
    }
}