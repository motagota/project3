using UnityEngine;
using UnityEngine.UI;
using TMPro;
using V2.Data;

namespace V2.UI
{
    public class MachineUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject uiPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI recipeNameText;
        [SerializeField] private TextMeshProUGUI inputItemsText;
        [SerializeField] private TextMeshProUGUI outputItemText;
        [SerializeField] private TextMeshProUGUI completedRecipesText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Toggle enabledToggle;
        
        private Machine _currentMachine;
        private SimulationManagerV2 _simulationManager;
        
        private void Awake()
        {
            _simulationManager = FindObjectOfType<SimulationManagerV2>();
            HideUI();
            
            // Add listener to the toggle
            if (enabledToggle != null)
            {
                enabledToggle.onValueChanged.AddListener(OnToggleChanged);
            }
        }
        
        private void Update()
        {
            // Update the UI if a machine is selected
            if (_currentMachine != null)
            {
                UpdateProgressBar();
                UpdateCompletedRecipes();
            }
            
            // Check for mouse click to select a machine
            if (Input.GetMouseButtonDown(0))
            {
                CheckForMachineSelection();
            }
            
            // Close UI with Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideUI();
            }
        }
        
        private void CheckForMachineSelection()
        {
            // Convert mouse position to world position
            Vector3 mousePos = Input.mousePosition;
            // Set z to the distance from the camera to the grid plane (typically 10 units in Unity)
            mousePos.z = 10f;
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            
            // Debug the positions
            Debug.Log($"Mouse Position: {mousePos}, World Position: {worldPos}, Grid Position: {GridUtility.SnapToGrid(worldPos)}");
            
            // Get the chunk at this position
            Vector2Int gridPos = GridUtility.SnapToGrid(worldPos);
            // Use a fixed chunk coordinate (0,0) since we're working with a single chunk in this simulation
            Vector2Int chunkCoord = new Vector2Int(0, 0);
            
            ChunkData chunk = _simulationManager.GetChunk(chunkCoord);
            if (chunk != null)
            {
                // Check if there's a machine at this position
                Machine machine = chunk.GetMachineAt(gridPos);
                if (machine != null)
                {
                    SelectMachine(machine);
                }
                else
                {
                    HideUI();
                }
            }
        }
        
        public void SelectMachine(Machine machine)
        {
            _currentMachine = machine;
            ShowUI();
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (_currentMachine == null) return;
            
            // Set machine title
            string machineType = _currentMachine.GetType().Name;
            titleText.text = machineType;
            
            // Set recipe information
            Recipe recipe = _currentMachine.CurrentRecipe;
            recipeNameText.text = $"Recipe: {recipe.OutputItemType}";
            
            // Set input items
            string inputItemsStr = "Inputs: ";
            if (recipe.InputItemTypes.Count == 0)
            {
                inputItemsStr += "None";
            }
            else
            {
                inputItemsStr += string.Join(", ", recipe.InputItemTypes);
                inputItemsStr += $" (x{recipe.InputItemCount})";
            }
            inputItemsText.text = inputItemsStr;
            
            // Set output item
            outputItemText.text = $"Output: {recipe.OutputItemType}";
            
            // Set completed recipes
            UpdateCompletedRecipes();
            
            // Set progress bar
            UpdateProgressBar();
            
            // Set enabled toggle
            enabledToggle.isOn = _currentMachine.IsEnabled;
        }
        
        private void UpdateProgressBar()
        {
            if (_currentMachine == null || progressBar == null) return;
            
            float progress = _currentMachine.Progress / _currentMachine.CurrentRecipe.Duration;
            progressBar.value = progress;
        }
        
        private void UpdateCompletedRecipes()
        {
            if (_currentMachine == null || completedRecipesText == null) return;
            
            completedRecipesText.text = $"Completed: {_currentMachine.CompletedRecipes}";
        }
        
        private void OnToggleChanged(bool isOn)
        {
            if (_currentMachine != null)
            {
                _currentMachine.IsEnabled = isOn;
            }
        }
        
        private void ShowUI()
        {
            if (uiPanel != null)
            {
                uiPanel.SetActive(true);
            }
        }
        
        private void HideUI()
        {
            if (uiPanel != null)
            {
                uiPanel.SetActive(false);
                _currentMachine = null;
            }
        }
    }
}