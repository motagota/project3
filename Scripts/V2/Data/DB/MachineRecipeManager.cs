using UnityEngine;
using System.Collections.Generic;

namespace V2.Data
{
    /// <summary>
    /// Manages the assignment of recipes to machines in the simulation.
    /// Acts as a bridge between the RecipeDatabase and Machine instances.
    /// </summary>
    public class MachineRecipeManager : MonoBehaviour
    {
        [SerializeField] private bool _initializeOnStart = true;
        [SerializeField] private string _defaultRecipeId = "";
        
        private Dictionary<Machine, string> _machineRecipeAssignments = new Dictionary<Machine, string>();
        
        private void Start()
        {
            if (_initializeOnStart)
            {
                InitializeMachines();
            }
        }
        
        /// <summary>
        /// Find all machines in the scene and initialize them with recipes.
        /// </summary>
        public void InitializeMachines()
        {
            // Find all machines in the scene
           // Machine[] machines = FindObjectsOfType<Machine>();
           Machine[] machines = {};
            foreach (Machine machine in machines)
            {
                // If we have a previous assignment for this machine, use it
                if (_machineRecipeAssignments.TryGetValue(machine, out string recipeId))
                {
                    AssignRecipeToMachine(machine, recipeId);
                }
                // Otherwise, use the default recipe if specified
                else if (!string.IsNullOrEmpty(_defaultRecipeId))
                {
                    AssignRecipeToMachine(machine, _defaultRecipeId);
                }
            }
            
            Debug.Log($"Initialized {machines.Length} machines with recipes");
        }
        
        /// <summary>
        /// Assign a recipe to a specific machine by recipe ID.
        /// </summary>
        /// <param name="machine">The machine to assign the recipe to</param>
        /// <param name="recipeId">The ID of the recipe in the database</param>
        /// <returns>True if assignment was successful, false otherwise</returns>
        public bool AssignRecipeToMachine(Machine machine, string recipeId)
        {
            if (machine == null)
            {
                Debug.LogError("Cannot assign recipe to null machine");
                return false;
            }
            
            Recipe recipe = RecipeDatabase.Instance.GetRecipe(recipeId);
            if (recipe == null)
            {
                Debug.LogError($"Recipe with ID '{recipeId}' not found in database");
                return false;
            }
            
            machine.CurrentRecipe = recipe;
            _machineRecipeAssignments[machine] = recipeId;
            
            Debug.Log($"Assigned recipe '{recipeId}' to machine at {machine.LocalPosition}");
            return true;
        }
        
        /// <summary>
        /// Get the current recipe ID assigned to a machine.
        /// </summary>
        /// <param name="machine">The machine to check</param>
        /// <returns>The recipe ID, or empty string if none assigned</returns>
        public string GetMachineRecipeId(Machine machine)
        {
            if (machine == null || !_machineRecipeAssignments.ContainsKey(machine))
            {
                return "";
            }
            
            return _machineRecipeAssignments[machine];
        }
        
        /// <summary>
        /// Find all machines using a specific recipe.
        /// </summary>
        /// <param name="recipeId">The recipe ID to search for</param>
        /// <returns>A list of machines using the specified recipe</returns>
        public List<Machine> FindMachinesUsingRecipe(string recipeId)
        {
            List<Machine> result = new List<Machine>();
            
            foreach (var pair in _machineRecipeAssignments)
            {
                if (pair.Value == recipeId)
                {
                    result.Add(pair.Key);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Update all machines using a specific recipe when that recipe changes.
        /// </summary>
        /// <param name="recipeId">The ID of the recipe that changed</param>
        public void UpdateMachinesUsingRecipe(string recipeId)
        {
            Recipe updatedRecipe = RecipeDatabase.Instance.GetRecipe(recipeId);
            if (updatedRecipe == null) return;
            
            List<Machine> affectedMachines = FindMachinesUsingRecipe(recipeId);
            foreach (Machine machine in affectedMachines)
            {
                machine.CurrentRecipe = updatedRecipe;
            }
            
            Debug.Log($"Updated {affectedMachines.Count} machines using recipe '{recipeId}'");
        }
        
        /// <summary>
        /// Clear the recipe assignment for a specific machine.
        /// </summary>
        /// <param name="machine">The machine to clear the recipe from</param>
        public void ClearMachineRecipe(Machine machine)
        {
            if (machine == null) return;
            
            // Set to a minimal default recipe
            machine.CurrentRecipe = new Recipe(1.0f);
            _machineRecipeAssignments.Remove(machine);
        }
    }
}