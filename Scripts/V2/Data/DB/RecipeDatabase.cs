using System.Collections.Generic;
using UnityEngine;

namespace V2.Data
{

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
        
        private void InitializeDefaultRecipes()
        {
            // Mining recipes (for Miner machines)
            AddRecipe("Mine_IronOre", new Recipe(
                duration: 1.0f,
                outputItemType: "IronOre",
                inputItemTypes: new List<string>(),
                inputItemCount: 0
            ));
            
            AddRecipe("Mine_CopperOre", new Recipe(
                duration: 1.0f,
                outputItemType: "CopperOre",
                inputItemTypes: new List<string>(),
                inputItemCount: 0
            ));
            
            AddRecipe("Mine_GoldOre", new Recipe(
                duration: 1.0f, // Gold takes longer to mine
                outputItemType: "GoldOre",
                inputItemTypes: new List<string>(),
                inputItemCount: 0
            ));
            
            AddRecipe("Mine_CoalOre", new Recipe(
                duration: 1.0f, // Coal is faster to mine
                outputItemType: "CoalOre",
                inputItemTypes: new List<string>(),
                inputItemCount: 0
            ));
            
            AddRecipe("Mine_StoneOre", new Recipe(
                duration: 1.0f, // Stone is the fastest to mine
                outputItemType: "StoneOre",
                inputItemTypes: new List<string>(),
                inputItemCount: 0
            ));
            
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
        
        public void AddRecipe(string recipeId, Recipe recipe)
        {
            if (_recipes.ContainsKey(recipeId))
            {
                Debug.LogWarning($"Recipe with ID {recipeId} already exists and will be overwritten.");
            }
            
            _recipes[recipeId] = recipe;
        }
        
        public Recipe GetRecipe(string recipeId)
        {
            if (_recipes.TryGetValue(recipeId, out Recipe recipe))
            {
                return recipe;
            }
            
            Debug.LogWarning($"Recipe with ID {recipeId} not found.");
            return null;
        }
        
  
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
        
        public List<string> GetAllRecipeIds()
        {
            return new List<string>(_recipes.Keys);
        }
        
        public List<Recipe> GetAllRecipes()
        {
            return new List<Recipe>(_recipes.Values);
        }
     
        public void ClearRecipes()
        {
            _recipes.Clear();
        }
    }
}