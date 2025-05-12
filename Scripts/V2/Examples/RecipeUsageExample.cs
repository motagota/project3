using UnityEngine;
using V2.Data;

namespace V2.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the RecipeDatabase with machines in the simulation.
    /// </summary>
    public class RecipeUsageExample : MonoBehaviour
    {
        // Reference to a machine in the scene
        public GameObject machineObject;
        private Machine _machine;
        
        // Recipe selection
        private string _currentRecipeId = "IronOre_to_IronPlate";
        
        private void Start()
        {
            // Get or create machine component
            _machine = machineObject.GetComponent<Machine>();
            if (_machine == null)
            {
                Debug.LogError("Machine component not found on the specified GameObject.");
                return;
            }
            
            // Set the machine's recipe from the database
            SetMachineRecipe(_currentRecipeId);
            
            // Log available recipes for debugging
            LogAvailableRecipes();
        }
        
        /// <summary>
        /// Set a machine's recipe using the recipe database
        /// </summary>
        public void SetMachineRecipe(string recipeId)
        {
            Recipe recipe = RecipeDatabase.Instance.GetRecipe(recipeId);
            if (recipe != null)
            {
                _machine.CurrentRecipe = recipe;
                Debug.Log($"Set machine recipe to {recipeId}: {recipe.OutputItemType} (Duration: {recipe.Duration}s)");
            }
            else
            {
                Debug.LogError($"Failed to set recipe: {recipeId} not found in database.");
            }
        }
        
        /// <summary>
        /// Example method to log all available recipes
        /// </summary>
        private void LogAvailableRecipes()
        {
            Debug.Log("Available Recipes in Database:");
            foreach (string recipeId in RecipeDatabase.Instance.GetAllRecipeIds())
            {
                Recipe recipe = RecipeDatabase.Instance.GetRecipe(recipeId);
                string inputs = string.Join(", ", recipe.InputItemTypes);
                Debug.Log($"- {recipeId}: {inputs} → {recipe.OutputItemType} (Takes {recipe.Duration}s)");
            }
        }
        
        /// <summary>
        /// Example method to find recipes that can process a specific item
        /// </summary>
        public void FindRecipesForItem(string itemType)
        {
            Debug.Log($"Recipes that can process {itemType}:");
            var recipes = RecipeDatabase.Instance.GetRecipesByInput(itemType);
            
            foreach (var recipe in recipes)
            {
                Debug.Log($"- {recipe.OutputItemType} (Takes {recipe.Duration}s)");
            }
        }
        
        /// <summary>
        /// Example method to cycle through available recipes
        /// </summary>
        public void CycleToNextRecipe()
        {
            var recipeIds = RecipeDatabase.Instance.GetAllRecipeIds();
            if (recipeIds.Count == 0) return;
            
            int currentIndex = recipeIds.IndexOf(_currentRecipeId);
            int nextIndex = (currentIndex + 1) % recipeIds.Count;
            _currentRecipeId = recipeIds[nextIndex];
            
            SetMachineRecipe(_currentRecipeId);
        }
    }
}