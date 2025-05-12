using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using V2.Data;

namespace V2.Editor
{
    public class ItemDatabaseEditor : EditorWindow
    {
        private List<string> _allItemIds = new List<string>();
        
        [MenuItem("Tools/Item Database Editor")]
        public static void ShowWindow()
        {
            GetWindow<ItemDatabaseEditor>("Item Database");
        }
        
        private void OnEnable(){}

        private void RefreshItemList()
        {
            _allItemIds = ItemDatabase.Instance.GetAllItemIds(); 
        }

        private void OnGUI()
        {
            GUILayout.Label("Recipe Database Editor", EditorStyles.boldLabel);
            
        }
    }
}
