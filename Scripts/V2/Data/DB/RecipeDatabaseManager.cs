using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace V2.Data
{
    /// <summary>
    /// Manages saving and loading recipes to/from JSON files.
    /// </summary>
    public class RecipeDatabaseManager : MonoBehaviour
    {
        [SerializeField] private string _recipesFilePath = "Recipes/recipes.json";
        
        [System.Serializable]
        private class SerializableRecipe
        {
            public string id;
            public float duration;
            public string outputItemType;
            public List<string> inputItemTypes;
            public int inputItemCount;
        }
        
        [System.Serializable]
        private class RecipeCollection
        {
            public List<SerializableRecipe> recipes = new List<SerializableRecipe>();
        }
        
        private void Awake()
        {
            LoadRecipesFromFile();
        }
        
        /// <summary>
        /// Save all recipes in the database to a JSON file.
        /// </summary>
        public void SaveRecipesToFile()
        {
            RecipeCollection collection = new RecipeCollection();
            List<string> recipeIds = RecipeDatabase.Instance.GetAllRecipeIds();
            
            foreach (string id in recipeIds)
            {
                Recipe recipe = RecipeDatabase.Instance.GetRecipe(id);
                SerializableRecipe serializableRecipe = new SerializableRecipe
                {
                    id = id,
                    duration = recipe.Duration,
                    outputItemType = recipe.OutputItemType,
                    inputItemTypes = recipe.InputItemTypes,
                    inputItemCount = recipe.InputItemCount
                };
                
                collection.recipes.Add(serializableRecipe);
            }
            
            string json = JsonUtility.ToJson(collection, true);
            string fullPath = Path.Combine(Application.dataPath, _recipesFilePath);
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(fullPath, json);
            Debug.Log($"Saved {collection.recipes.Count} recipes to {fullPath}");
        }
        
        /// <summary>
        /// Load recipes from a JSON file into the database.
        /// </summary>
        public void LoadRecipesFromFile()
        {
            string fullPath = Path.Combine(Application.dataPath, _recipesFilePath);
            
            if (!File.Exists(fullPath))
            {
                Debug.Log($"Recipe file not found at {fullPath}. Using default recipes.");
                return;
            }
            
            try
            {
                string json = File.ReadAllText(fullPath);
                RecipeCollection collection = JsonUtility.FromJson<RecipeCollection>(json);
                
                // Clear existing recipes and add loaded ones
                RecipeDatabase.Instance.ClearRecipes();
                
                foreach (SerializableRecipe serializableRecipe in collection.recipes)
                {
                    Recipe recipe = new Recipe(
                        duration: serializableRecipe.duration,
                        outputItemType: serializableRecipe.outputItemType,
                        inputItemTypes: serializableRecipe.inputItemTypes,
                        inputItemCount: serializableRecipe.inputItemCount
                    );
                    
                    RecipeDatabase.Instance.AddRecipe(serializableRecipe.id, recipe);
                }
                
                Debug.Log($"Loaded {collection.recipes.Count} recipes from {fullPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading recipes: {e.Message}");
            }
        }
        
        /// <summary>
        /// Export recipes to a specific JSON file path.
        /// </summary>
        public void ExportRecipes(string filePath)
        {
            _recipesFilePath = filePath;
            SaveRecipesToFile();
        }
        
        /// <summary>
        /// Import recipes from a specific JSON file path.
        /// </summary>
        public void ImportRecipes(string filePath)
        {
            _recipesFilePath = filePath;
            LoadRecipesFromFile();
        }
    }
}