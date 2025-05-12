using System.Collections.Generic;
using UnityEngine;

namespace V2.Data
{
    /// <summary>
    /// A database class that stores and manages recipes for the simulation.
    /// </summary>
    public class RecipeDatabase
    {
        private static RecipeDatabase _instance;
        private Dictionary<string, Recipe> _recipes = new Dictionary<string, Recipe>();
        
        // Singleton pattern to ensure only one recipe database exists
        public static RecipeDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RecipeDatabase();
                    _instance.InitializeDefaultRecipes();
                }
                return _instance;
            }
        }
        
        private RecipeDatabase()
        {
            // Private constructor to enforce singleton pattern
        }
        
        /// <summary>
        /// Initialize the database with default recipes.
        /// </summary>
        private void InitializeDefaultRecipes()
        {
            // Basic processing recipe
            AddRecipe("IronOre_to_IronPlate", new Recipe(
                duration: 2.0f,
                outputItemType: "IronPlate",
                inputItemTypes: new List<string> { "IronOre" },
                inputItemCount: 1
            ));
            
            // Advanced processing recipe
            AddRecipe("IronPlate_to_IronGear", new Recipe(
                duration: 3.0f,
                outputItemType: "IronGear",
                inputItemTypes: new List<string> { "IronPlate" },
                inputItemCount: 2
            ));
            
            // Complex recipe with multiple inputs
            AddRecipe("Circuit", new Recipe(
                duration: 4.0f,
                outputItemType: "Circuit",
                inputItemTypes: new List<string> { "IronPlate", "CopperWire" },
                inputItemCount: 1
            ));
            
            // Add more recipes as needed
        }
        
        /// <summary>
        /// Add a recipe to the database.
        /// </summary>
        /// <param name="recipeId">Unique identifier for the recipe</param>
        /// <param name="recipe">The recipe to add</param>
        public void AddRecipe(string recipeId, Recipe recipe)
        {
            if (_recipes.ContainsKey(recipeId))
            {
                Debug.LogWarning($"Recipe with ID {recipeId} already exists and will be overwritten.");
            }
            
            _recipes[recipeId] = recipe;
        }
        
        /// <summary>
        /// Get a recipe by its ID.
        /// </summary>
        /// <param name="recipeId">The ID of the recipe to retrieve</param>
        /// <returns>The recipe if found, null otherwise</returns>
        public Recipe GetRecipe(string recipeId)
        {
            if (_recipes.TryGetValue(recipeId, out Recipe recipe))
            {
                return recipe;
            }
            
            Debug.LogWarning($"Recipe with ID {recipeId} not found.");
            return null;
        }
        
        /// <summary>
        /// Get all recipes that produce a specific output item type.
        /// </summary>
        /// <param name="outputItemType">The output item type to search for</param>
        /// <returns>A list of recipes that produce the specified output</returns>
        public List<Recipe> GetRecipesByOutput(string outputItemType)
        {
            List<Recipe> matchingRecipes = new List<Recipe>();
            
            foreach (var recipe in _recipes.Values)
            {
                if (recipe.OutputItemType == outputItemType)
                {
                    matchingRecipes.Add(recipe);
                }
            }
            
            return matchingRecipes;
        }
        
        /// <summary>
        /// Get all recipes that require a specific input item type.
        /// </summary>
        /// <param name="inputItemType">The input item type to search for</param>
        /// <returns>A list of recipes that require the specified input</returns>
        public List<Recipe> GetRecipesByInput(string inputItemType)
        {
            List<Recipe> matchingRecipes = new List<Recipe>();
            
            foreach (var recipe in _recipes.Values)
            {
                if (recipe.RequiresItemType(inputItemType))
                {
                    matchingRecipes.Add(recipe);
                }
            }
            
            return matchingRecipes;
        }
        
        /// <summary>
        /// Get all recipe IDs in the database.
        /// </summary>
        /// <returns>A list of all recipe IDs</returns>
        public List<string> GetAllRecipeIds()
        {
            return new List<string>(_recipes.Keys);
        }
        
        /// <summary>
        /// Get all recipes in the database.
        /// </summary>
        /// <returns>A list of all recipes</returns>
        public List<Recipe> GetAllRecipes()
        {
            return new List<Recipe>(_recipes.Values);
        }
        
        /// <summary>
        /// Clear all recipes from the database.
        /// </summary>
        public void ClearRecipes()
        {
            _recipes.Clear();
        }
    }
}