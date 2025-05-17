using UnityEngine;
using V2.Data;


namespace V2.UI
{
    /// <summary>
    /// Handles the selection of machines in the game world and displays their UI.
    /// This class works with WindowManager to show machine UIs when machines are clicked.
    /// </summary>
    public class MachineSelectionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimulationManagerV2 simulationManager;
        
        private static MachineSelectionManager _instance;
        
        public static MachineSelectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MachineSelectionManager>();
                    if (_instance == null)
                    {
                        GameObject managerObject = new GameObject("MachineSelectionManager");
                        _instance = managerObject.AddComponent<MachineSelectionManager>();
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            
            // Find simulation manager if not assigned
            if (simulationManager == null)
            {
                simulationManager = FindObjectOfType<SimulationManagerV2>();
            }
        }
        
        private void Update()
        {
            // Check for machine selection on mouse click
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                CheckForMachineSelection();
            }
            
            // Close machine UI on escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // This will close all windows, but you could modify to only close machine windows
                WindowManager.Instance.CloseAllWindows();
            }
        }
        
        /// <summary>
        /// Checks if the pointer is over a UI element to prevent selecting machines through UI.
        /// </summary>
        private bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null && 
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        
        /// <summary>
        /// Checks if a machine is at the current mouse position and selects it if found.
        /// </summary>
        private void CheckForMachineSelection()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; // Distance from the camera to the grid plane
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2Int gridPos = GridUtility.SnapToGrid(worldPos);
            
            // Use a fixed chunk coordinate (0,0) since we're working with a single chunk
            Vector2Int chunkCoord = new Vector2Int(0, 0);
            ChunkData chunk = simulationManager.GetChunk(chunkCoord);
            
            if (chunk != null)
            {
                Machine machine = chunk.GetMachineAt(gridPos);
                if (machine != null)
                {
                    SelectMachine(machine);
                }
            }
        }
        
        /// <summary>
        /// Selects a machine and displays its UI.
        /// </summary>
        /// <param name="machine">The machine to select</param>
        public void SelectMachine(Machine machine)
        {
            // Use WindowManager to create/show the machine UI
            WindowManager.Instance.CreateMachineWindow(machine);
        }
        
        /// <summary>
        /// Gets the window ID for a machine based on its position.
        /// </summary>
        /// <param name="machine">The machine</param>
        /// <returns>The window ID string</returns>
        public static string GetMachineWindowId(Machine machine)
        {
            return $"Machine_{machine.LocalPosition.x}_{machine.LocalPosition.y}";
        }
    }
}