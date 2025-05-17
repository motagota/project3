using UnityEngine;
using System.Collections.Generic;

namespace V2.Data
{
 
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
        
        public string GetMachineRecipeId(Machine machine)
        {
            if (machine == null || !_machineRecipeAssignments.ContainsKey(machine))
            {
                return "";
            }
            
            return _machineRecipeAssignments[machine];
        }
        
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
        
        public void ClearMachineRecipe(Machine machine)
        {
            if (machine == null) return;
            
            // Set to a minimal default recipe
            machine.CurrentRecipe = new Recipe(1.0f);
            _machineRecipeAssignments.Remove(machine);
        }
    }
}