using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace V2.Data.DB
{
    public class ItemDatabaseManager : MonoBehaviour
    {
        [SerializeField] private string _itemsFilePath = "Items/items.json";

        [System.Serializable]
        private class SerializableItem
        {
            public string id;
            public string itemType;
            public string name;
            public string description;
            public string spritePath;
        }

        [System.Serializable]
        private class ItemCollection
        {
            public List<SerializableItem> items = new List<SerializableItem>();
        }

        private void Awake()
        {
            LoadItemsFromFile();
        }


        public void LoadItemsFromFile()
        {
            string fullPath = Path.Combine(Application.dataPath , _itemsFilePath);

            if (!File.Exists(fullPath))
            {
                Debug.Log($"Item file not found at {fullPath}. Using default items.");
                return;
            }

            try
            {


                string json = File.ReadAllText(fullPath);
                ItemCollection collection = JsonUtility.FromJson<ItemCollection>(json);

                // Clear existing items and add loaded ones
                ItemDatabase.Instance.ClearItems();

                foreach (SerializableItem serializableItem in collection.items)
                {
                    ItemDefinition item = new ItemDefinition(
                        serializableItem.id,
                        serializableItem.name,
                        serializableItem.description,
                        serializableItem.description
                    );

                    ItemDatabase.Instance.AddItem(item.Id, item);
                }

                Debug.Log($"Loaded {collection.items.Count} items from {fullPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading items: {e.Message}");
            }
        }
    }
}
