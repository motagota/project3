using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using V2.Data;

namespace V2.Editor
{
    /// <summary>
    /// Custom editor window for managing recipes in the RecipeDatabase.
    /// </summary>
    public class RecipeDatabaseEditor : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _newRecipeId = "";
        private string _newOutputType = "";
        private float _newDuration = 2.0f;
        private List<string> _newInputTypes = new List<string> { "" };
        private int _newInputCount = 1;
        private int _selectedRecipeIndex = -1;
        private List<string> _allRecipeIds = new List<string>();
        
        [MenuItem("Tools/Recipe Database Editor")]
        public static void ShowWindow()
        {
            GetWindow<RecipeDatabaseEditor>("Recipe Database");
        }
        
        private void OnEnable()
        {
            RefreshRecipeList();
        }
        
        private void RefreshRecipeList()
        {
            _allRecipeIds = RecipeDatabase.Instance.GetAllRecipeIds();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Recipe Database Editor", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            DrawExistingRecipes();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            DrawAddNewRecipe();
        }
        
        private void DrawExistingRecipes()
        {
            GUILayout.Label("Existing Recipes", EditorStyles.boldLabel);
            
            if (_allRecipeIds.Count == 0)
            {
                EditorGUILayout.HelpBox("No recipes in database. Add a new recipe below.", MessageType.Info);
                return;
            }
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            for (int i = 0; i < _allRecipeIds.Count; i++)
            {
                string recipeId = _allRecipeIds[i];
                Recipe recipe = RecipeDatabase.Instance.GetRecipe(recipeId);
                
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(recipeId, EditorStyles.boldLabel);
                
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Recipe", 
                        $"Are you sure you want to delete the recipe '{recipeId}'?", "Delete", "Cancel"))
                    {
                        // We would need to add a RemoveRecipe method to RecipeDatabase
                        // For now, we'll just clear and re-add all except this one
                        Dictionary<string, Recipe> tempRecipes = new Dictionary<string, Recipe>();
                        foreach (string id in _allRecipeIds)
                        {
                            if (id != recipeId)
                            {
                                tempRecipes[id] = RecipeDatabase.Instance.GetRecipe(id);
                            }
                        }
                        
                        RecipeDatabase.Instance.ClearRecipes();
                        foreach (var pair in tempRecipes)
                        {
                            RecipeDatabase.Instance.AddRecipe(pair.Key, pair.Value);
                        }
                        
                        RefreshRecipeList();
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Output:", recipe.OutputItemType);
                EditorGUILayout.LabelField("Duration:", recipe.Duration.ToString() + "s");
                
                EditorGUILayout.LabelField("Inputs:");
                EditorGUI.indentLevel++;
                if (recipe.InputItemTypes.Count > 0)
                {
                    foreach (string inputType in recipe.InputItemTypes)
                    {
                        EditorGUILayout.LabelField(inputType, recipe.InputItemCount.ToString() + "x");
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("None (Generator)");
                }
                EditorGUI.indentLevel -= 2;
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawAddNewRecipe()
        {
            GUILayout.Label("Add New Recipe", EditorStyles.boldLabel);
            
            _newRecipeId = EditorGUILayout.TextField("Recipe ID:", _newRecipeId);
            _newOutputType = EditorGUILayout.TextField("Output Item Type:", _newOutputType);
            _newDuration = EditorGUILayout.FloatField("Duration (seconds):", _newDuration);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Input Items:");
            
            for (int i = 0; i < _newInputTypes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _newInputTypes[i] = EditorGUILayout.TextField("Type " + (i + 1) + ":", _newInputTypes[i]);
                
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    _newInputTypes.RemoveAt(i);
                    GUIUtility.ExitGUI();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Input Type", GUILayout.Width(120)))
            {
                _newInputTypes.Add("");
            }
            
            EditorGUILayout.Space();
            _newInputCount = EditorGUILayout.IntField("Input Count (per type):", _newInputCount);
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Add Recipe"))
            {
                if (string.IsNullOrEmpty(_newRecipeId))
                {
                    EditorUtility.DisplayDialog("Error", "Recipe ID cannot be empty.", "OK");
                    return;
                }
                
                if (string.IsNullOrEmpty(_newOutputType))
                {
                    EditorUtility.DisplayDialog("Error", "Output Item Type cannot be empty.", "OK");
                    return;
                }
                
                // Filter out empty input types
                List<string> validInputTypes = new List<string>();
                foreach (string inputType in _newInputTypes)
                {
                    if (!string.IsNullOrEmpty(inputType))
                    {
                        validInputTypes.Add(inputType);
                    }
                }
                
                Recipe newRecipe = new Recipe(
                    duration: _newDuration,
                    outputItemType: _newOutputType,
                    inputItemTypes: validInputTypes,
                    inputItemCount: _newInputCount
                );
                
                RecipeDatabase.Instance.AddRecipe(_newRecipeId, newRecipe);
                
                // Reset form
                _newRecipeId = "";
                _newOutputType = "";
                _newDuration = 2.0f;
                _newInputTypes = new List<string> { "" };
                _newInputCount = 1;
                
                RefreshRecipeList();
            }
        }
    }
}