using UnityEngine;
using UnityEngine.UI;
using TMPro;
using V2.Data;

namespace V2.UI
{
    public class MachineUI : MonoBehaviour
    {
        // These fields will be set by WindowManager when UI is created
        [HideInInspector] public GameObject uiPanel;
        [HideInInspector] public TextMeshProUGUI titleText;
        [HideInInspector] public TextMeshProUGUI recipeNameText;
        [HideInInspector] public TextMeshProUGUI inputItemsText;
        [HideInInspector] public TextMeshProUGUI outputItemText;
        [HideInInspector] public TextMeshProUGUI completedRecipesText;
        [HideInInspector] public Slider progressBar;
        [HideInInspector] public Toggle enabledToggle;
        
        private Machine _currentMachine;
        private SimulationManagerV2 _simulationManager;
        
        private void Awake()
        {
            _simulationManager = FindObjectOfType<SimulationManagerV2>();
        }
        
        private void Start()
        {
            // Add listener to the toggle if it exists
            if (enabledToggle != null)
            {
                enabledToggle.onValueChanged.AddListener(OnToggleChanged);
            }
        }
        
        private void Update()
        {
            if (_currentMachine != null)
            {
                UpdateProgressBar();
                UpdateCompletedRecipes();
            }
            
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                CheckForMachineSelection();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) && _currentMachine != null)
            {
                HideUI();
            }
        }
        
        private bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null && 
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        
        private void CheckForMachineSelection()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2Int gridPos = GridUtility.SnapToGrid(worldPos);
            Vector2Int chunkCoord = new Vector2Int(0, 0);
            ChunkData chunk = _simulationManager.GetChunk(chunkCoord);
            
            if (chunk != null)
            {
                Machine machine = chunk.GetMachineAt(gridPos);
                if (machine != null)
                {
                    // Use WindowManager to create/show the machine UI
                    WindowManager.Instance.CreateMachineWindow(machine);
                }
            }
        }
        
        public void SelectMachine(Machine machine)
        {
            _currentMachine = machine;
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (_currentMachine == null) return;
            
            string machineType = _currentMachine.GetType().Name;
            if (titleText != null)
            {
                titleText.text = machineType;
            }
            
            Recipe recipe = _currentMachine.CurrentRecipe;
            if (recipeNameText != null)
            {
                recipeNameText.text = $"Recipe: {recipe.OutputItemType}";
            }
            
            if (inputItemsText != null)
            {
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
            }
            
            if (outputItemText != null)
            {
                outputItemText.text = $"Output: {recipe.OutputItemType}";
            }
            
            UpdateCompletedRecipes();
            UpdateProgressBar();
            
            if (enabledToggle != null)
            {
                enabledToggle.isOn = _currentMachine.IsEnabled;
            }
        }
        
        private void UpdateProgressBar()
        {
            
            if (_currentMachine == null || progressBar == null) return;
            if(_currentMachine.CurrentRecipe == null) return;
            
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
        
        private void HideUI()
        {
            if (_currentMachine != null)
            {
                string windowId = MachineSelectionManager.GetMachineWindowId(_currentMachine);
                WindowManager.Instance.CloseWindow(windowId);
                _currentMachine = null;
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event listeners when the UI is destroyed
            if (enabledToggle != null)
            {
                enabledToggle.onValueChanged.RemoveListener(OnToggleChanged);
            }
        }
    }
}