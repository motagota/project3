using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using V2.Data.DB;

namespace V2.Data.Editor
{
    public class ItemDatabaseEditor : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showItemList = true;
        private bool _showAddItem = false;
        
        // Fields for adding a new item
        private string _newItemId = "";
        private string _newItemName = "";
        private string _newItemDescription = "";
        private string _newItemCategory = "";
        private bool _newItemStackable = true;
        private int _newItemMaxStack = 99;
        private float _newItemMass = 1.0f;
        private Vector3 _newItemSize = Vector3.one;
        private float _newItemValue = 1.0f;
        private bool _newItemConsumable = false;
        private Color _newItemColor = Color.white;
        
        // Selected item for editing
        private string _selectedItemId = null;
        private ItemDefinition _selectedItem = null;
        private bool _showEditItem = false;
        
        [MenuItem("Tools/Item Database Editor")]
        public static void ShowWindow()
        {
            GetWindow<ItemDatabaseEditor>("Item Database");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Item Database Editor", EditorStyles.boldLabel);
            
            // Buttons for saving/loading
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save to JSON"))
            {
                string path = EditorUtility.SaveFilePanel("Save Item Database", "", "ItemDatabase.json", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    GameObject tempGO = new GameObject("TempItemDatabaseManager");
                    ItemDatabaseManager manager = tempGO.AddComponent<ItemDatabaseManager>();
                    DestroyImmediate(tempGO);
                }
            }
            
            if (GUILayout.Button("Load from JSON"))
            {
                string path = EditorUtility.OpenFilePanel("Load Item Database", "", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    GameObject tempGO = new GameObject("TempItemDatabaseManager");
                    ItemDatabaseManager manager = tempGO.AddComponent<ItemDatabaseManager>();
                    DestroyImmediate(tempGO);
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
        
            _showItemList = EditorGUILayout.Foldout(_showItemList, "Item List");
            _showAddItem = EditorGUILayout.Foldout(_showAddItem, "Add New Item");
            
            EditorGUILayout.Space();
            
       
            if (_showItemList)
            {
                DrawItemList();
            }
            
            if (_showAddItem)
            {
                DrawAddItemSection();
            }
            
            if (_showEditItem && _selectedItem != null)
            {
                DrawEditItemSection();
            }
        }
        
        private void DrawItemList()
        {
            EditorGUILayout.LabelField("Available Items", EditorStyles.boldLabel);
            
            Dictionary<string, ItemDefinition> allItems = ItemDatabase.Instance.GetAllItems();
            
            if (allItems.Count == 0)
            {
                EditorGUILayout.HelpBox("No items in the database.", MessageType.Info);
                return;
            }
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            foreach (var pair in allItems)
            {
                EditorGUILayout.BeginHorizontal("box");
                
                // Item info
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"ID: {pair.Key}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Name: {pair.Value.DisplayName}");
                EditorGUILayout.LabelField($"Category: {pair.Value.Category}");
                EditorGUILayout.EndVertical();
                
                // Buttons
                EditorGUILayout.BeginVertical(GUILayout.Width(100));
                if (GUILayout.Button("Edit"))
                {
                    _selectedItemId = pair.Key;
                    _selectedItem = pair.Value;
                    _showEditItem = true;
                }
                
                if (GUILayout.Button("Delete"))
                {
                    if (EditorUtility.DisplayDialog("Delete Item", 
                        $"Are you sure you want to delete the item '{pair.Value.DisplayName}'?", 
                        "Delete", "Cancel"))
                    {
                        // Create a new dictionary without the item to delete
                        Dictionary<string, ItemDefinition> updatedItems = new Dictionary<string, ItemDefinition>();
                        foreach (var item in allItems)
                        {
                            if (item.Key != pair.Key)
                            {
                                updatedItems.Add(item.Key, item.Value);
                            }
                        }
                        
                        ItemDatabase.Instance.ClearItems();
                        foreach (var item in updatedItems)
                        {
                            ItemDatabase.Instance.AddItem(item.Key, item.Value);
                        }
                      
                        if (_selectedItemId == pair.Key)
                        {
                            _selectedItemId = null;
                            _selectedItem = null;
                            _showEditItem = false;
                        }
              
                        Repaint();
                        break;
                    }
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawAddItemSection()
        {
            EditorGUILayout.LabelField("Add New Item", EditorStyles.boldLabel);
            
            _newItemId = EditorGUILayout.TextField("Item ID", _newItemId);
            _newItemName = EditorGUILayout.TextField("Display Name", _newItemName);
            _newItemDescription = EditorGUILayout.TextField("Description", _newItemDescription);
            _newItemCategory = EditorGUILayout.TextField("Category", _newItemCategory);
            _newItemStackable = EditorGUILayout.Toggle("Stackable", _newItemStackable);
            
            if (_newItemStackable)
            {
                _newItemMaxStack = EditorGUILayout.IntField("Max Stack Size", _newItemMaxStack);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Physical Properties", EditorStyles.boldLabel);
            _newItemMass = EditorGUILayout.FloatField("Mass", _newItemMass);
            _newItemSize = EditorGUILayout.Vector3Field("Size", _newItemSize);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gameplay Properties", EditorStyles.boldLabel);
            _newItemValue = EditorGUILayout.FloatField("Value", _newItemValue);
            _newItemConsumable = EditorGUILayout.Toggle("Consumable", _newItemConsumable);
            _newItemColor = EditorGUILayout.ColorField("Icon Color", _newItemColor);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Add Item"))
            {
                if (string.IsNullOrEmpty(_newItemId))
                {
                    EditorUtility.DisplayDialog("Error", "Item ID cannot be empty.", "OK");
                    return;
                }
                
                if (string.IsNullOrEmpty(_newItemName))
                {
                    EditorUtility.DisplayDialog("Error", "Display Name cannot be empty.", "OK");
                    return;
                }
                
                if (ItemDatabase.Instance.HasItem(_newItemId))
                {
                    EditorUtility.DisplayDialog("Error", $"Item with ID '{_newItemId}' already exists.", "OK");
                    return;
                }
                
                // Create the new item
                ItemDefinition newItem = new ItemDefinition(
                    id: _newItemId,
                    displayName: _newItemName,
                    description: _newItemDescription,
                    category: _newItemCategory,
                    stackable: _newItemStackable,
                    maxStackSize: _newItemMaxStack
                );
                
                
                ItemDatabase.Instance.AddItem(_newItemId, newItem);
                ResetNewItemFields();
                Repaint();
            }
        }
        
        private void DrawEditItemSection()
        {
            EditorGUILayout.LabelField($"Edit Item: {_selectedItemId}", EditorStyles.boldLabel);
            
            string displayName = EditorGUILayout.TextField("Display Name", _selectedItem.DisplayName);
            string description = EditorGUILayout.TextField("Description", _selectedItem.Description);
            string category = EditorGUILayout.TextField("Category", _selectedItem.Category);
            bool stackable = EditorGUILayout.Toggle("Stackable", _selectedItem.Stackable);
            
            int maxStackSize = _selectedItem.MaxStackSize;
            if (stackable)
            {
                maxStackSize = EditorGUILayout.IntField("Max Stack Size", _selectedItem.MaxStackSize);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Physical Properties", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gameplay Properties", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Save Changes"))
            {
                if (string.IsNullOrEmpty(displayName))
                {
                    EditorUtility.DisplayDialog("Error", "Display Name cannot be empty.", "OK");
                    return;
                }
                
                // Update the item
                ItemDefinition updatedItem = new ItemDefinition(
                    id: _selectedItemId,
                    displayName: displayName,
                    description: description,
                    category: category,
                    stackable: stackable,
                    maxStackSize: maxStackSize
                );
                
                ItemDatabase.Instance.AddItem(_selectedItemId, updatedItem);
                
                _selectedItem = updatedItem;
                Repaint();
            }
            
            if (GUILayout.Button("Cancel"))
            {
                _selectedItemId = null;
                _selectedItem = null;
                _showEditItem = false;
            }
        }
        
        private void ResetNewItemFields()
        {
            _newItemId = "";
            _newItemName = "";
            _newItemDescription = "";
            _newItemCategory = "";
            _newItemStackable = true;
            _newItemMaxStack = 99;
            _newItemMass = 1.0f;
            _newItemSize = Vector3.one;
            _newItemValue = 1.0f;
            _newItemConsumable = false;
            _newItemColor = Color.white;
        }
    }
}