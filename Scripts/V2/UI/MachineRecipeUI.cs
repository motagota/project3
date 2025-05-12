using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using V2.Data;

namespace V2.UI
{
    /// <summary>
    /// UI component for selecting and assigning recipes to machines in-game.
    /// </summary>
    public class MachineRecipeUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Dropdown _recipeDropdown;
        [SerializeField] private Button _assignButton;
        [SerializeField] private TextMeshProUGUI _recipeInfoText;
        [SerializeField] private Transform _inputItemsContainer;
        [SerializeField] private GameObject _inputItemPrefab;
        
        [Header("Configuration")]
        [SerializeField] private MachineRecipeManager _recipeManager;
        
        private Machine _selectedMachine;
        private List<string> _availableRecipeIds = new List<string>();
        
        private void Start()
        {
            // Find recipe manager if not assigned
            if (_recipeManager == null)
            {
                _recipeManager = FindObjectOfType<MachineRecipeManager>();
            }
            
            // Set up UI event handlers
            _recipeDropdown.onValueChanged.AddListener(OnRecipeSelected);
            _assignButton.onClick.AddListener(AssignSelectedRecipe);
            
            // Initially hide the UI until a machine is selected
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Show the UI for a specific machine.
        /// </summary>
        /// <param name="machine">The machine to configure</param>
        public void ShowForMachine(Machine machine)
        {
            if (machine == null) return;
            
            _selectedMachine = machine;
            gameObject.SetActive(true);
            
            // Populate recipe dropdown
            PopulateRecipeDropdown();
            
            // Select the current recipe if any
            string currentRecipeId = _recipeManager.GetMachineRecipeId(machine);
            if (!string.IsNullOrEmpty(currentRecipeId))
            {
                int index = _availableRecipeIds.IndexOf(currentRecipeId);
                if (index >= 0)
                {
                    _recipeDropdown.value = index;
                }
            }
            
            // Update recipe info display
            UpdateRecipeInfoDisplay();
        }
        
        /// <summary>
        /// Hide the UI.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            _selectedMachine = null;
        }
        
        /// <summary>
        /// Populate the recipe dropdown with available recipes.
        /// </summary>
        private void PopulateRecipeDropdown()
        {
            _recipeDropdown.ClearOptions();
            _availableRecipeIds = RecipeDatabase.Instance.GetAllRecipeIds();
            
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (string recipeId in _availableRecipeIds)
            {
                options.Add(new TMP_Dropdown.OptionData(recipeId));
            }
            
            _recipeDropdown.AddOptions(options);
        }
        
        /// <summary>
        /// Handle recipe selection from dropdown.
        /// </summary>
        private void OnRecipeSelected(int index)
        {
            UpdateRecipeInfoDisplay();
        }
        
        /// <summary>
        /// Assign the selected recipe to the current machine.
        /// </summary>
        private void AssignSelectedRecipe()
        {
            if (_selectedMachine == null || _recipeDropdown.value < 0 || 
                _recipeDropdown.value >= _availableRecipeIds.Count)
                return;
            
            string recipeId = _availableRecipeIds[_recipeDropdown.value];
            _recipeManager.AssignRecipeToMachine(_selectedMachine, recipeId);
            
            // Update the UI to reflect changes
            UpdateRecipeInfoDisplay();
        }
        
        /// <summary>
        /// Update the recipe information display.
        /// </summary>
        private void UpdateRecipeInfoDisplay()
        {
            if (_selectedMachine == null || _recipeDropdown.value < 0 || 
                _recipeDropdown.value >= _availableRecipeIds.Count)
            {
                _recipeInfoText.text = "No recipe selected";
                ClearInputItems();
                return;
            }
            
            string recipeId = _availableRecipeIds[_recipeDropdown.value];
            Recipe recipe = RecipeDatabase.Instance.GetRecipe(recipeId);
            
            if (recipe == null)
            {
                _recipeInfoText.text = "Recipe not found";
                ClearInputItems();
                return;
            }
            
            // Update recipe info text
            _recipeInfoText.text = $"Output: {recipe.OutputItemType}\n" +
                                  $"Duration: {recipe.Duration} seconds";
            
            // Update input items display
            UpdateInputItemsDisplay(recipe);
        }
        
        /// <summary>
        /// Update the input items display for a recipe.
        /// </summary>
        private void UpdateInputItemsDisplay(Recipe recipe)
        {
            // Clear existing items
            ClearInputItems();
            
            // If no input items, show a message
            if (recipe.InputItemTypes.Count == 0)
            {
                GameObject itemObj = Instantiate(_inputItemPrefab, _inputItemsContainer);
                TextMeshProUGUI itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                if (itemText != null)
                {
                    itemText.text = "No inputs required";
                }
                return;
            }
            
            // Create UI elements for each input item type
            foreach (string inputType in recipe.InputItemTypes)
            {
                GameObject itemObj = Instantiate(_inputItemPrefab, _inputItemsContainer);
                TextMeshProUGUI itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                if (itemText != null)
                {
                    itemText.text = $"{inputType} x{recipe.InputItemCount}";
                }
                
                // You could add an icon or other visual representation here
            }
        }
        
        /// <summary>
        /// Clear all input item UI elements.
        /// </summary>
        private void ClearInputItems()
        {
            foreach (Transform child in _inputItemsContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}