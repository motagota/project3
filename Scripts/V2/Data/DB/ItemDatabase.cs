using System.Collections.Generic;
using UnityEngine;

namespace V2.Data
{
    public class ItemDatabase
    {
        private static ItemDatabase _instance;
        private Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();
        
        // Singleton pattern to ensure only one item database exists
        public static ItemDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ItemDatabase();
                    _instance.InitializeDefaultItems();
                }
                return _instance;
            }
        }
        
        private ItemDatabase()
        {
            // Private constructor to enforce singleton pattern
        }
        
        private void InitializeDefaultItems()
        {
            // Raw materials
            AddItem("IronOre", new ItemDefinition(
                id: "1",
                displayName: "Iron Ore",
                description: "Raw iron ore extracted from the ground.",
                category: "RawMaterial",
                stackable: true,
                maxStackSize: 50,
                itemColor: new Color32(90, 90, 90, 255)
               // 🪨
            ));
            
            AddItem("CopperOre", new ItemDefinition(
                id: "2",
                displayName: "Copper Ore",
                description: "Raw copper ore extracted from the ground.",
                category: "RawMaterial",
                stackable: true,
                maxStackSize: 50,
                itemColor: new Color32(184, 115, 51, 255)
                //🧲
            ));
            
            AddItem("GoldOre", new ItemDefinition(
                id: "7",
                displayName: "Gold Ore",
                description: "Precious gold ore extracted from deep mines.",
                category: "RawMaterial",
                stackable: true,
                maxStackSize: 50,
                itemColor: new Color32(255, 215, 0, 255)
                //💎
            ));
            
            AddItem("CoalOre", new ItemDefinition(
                id: "8",
                displayName: "Coal Ore",
                description: "Combustible coal ore used for fuel and processing.",
                category: "RawMaterial",
                stackable: true,
                maxStackSize: 50,
                itemColor: new Color32(28, 28, 28, 255)
                //🔥
            ));
            
            AddItem("StoneOre", new ItemDefinition(
                id: "9",
                displayName: "Stone",
                description: "Basic stone material used for construction.",
                category: "RawMaterial",
                stackable: true,
                maxStackSize: 100,
                itemColor: new Color32(122, 111, 100, 255)
                //🪨
            ));
            
            // Processed materials
            AddItem("IronPlate", new ItemDefinition(
                id: "3",
                displayName: "Iron Plate",
                description: "Processed iron plate used in manufacturing.",
                category: "ProcessedMaterial",
                stackable: true,
                maxStackSize: 100
            ));
            
            AddItem("CopperPlate", new ItemDefinition(
                id: "3",
                displayName: "Copper Plate",
                description: "Processed iron plate used in manufacturing.",
                category: "ProcessedMaterial",
                stackable: true,
                maxStackSize: 100,
                itemColor: new Color32(200, 90, 51, 255)
            ));
            
            AddItem("CopperWire", new ItemDefinition(
                id: "4",
                displayName: "Copper Wire",
                description: "Processed copper wire used in electronics.",
                category: "ProcessedMaterial",
                stackable: true,
                maxStackSize: 200
            ));
            
            // Components
            AddItem("Circuit", new ItemDefinition(
                id: "5",
                displayName: "Basic Circuit",
                description: "A basic electronic circuit.",
                category: "Component",
                stackable: true,
                maxStackSize: 50
            ));
            
            AddItem("IronGear", new ItemDefinition(
                id: "6",
                displayName: "Iron Gear",
                description: "A mechanical gear made of iron.",
                category: "Component",
                stackable: true,
                maxStackSize: 50
            ));
            
        }
        
        public void AddItem(string itemId, ItemDefinition itemDefinition)
        {
            if (_items.ContainsKey(itemId))
            {
                Debug.LogWarning($"Item with ID {itemId} already exists and will be overwritten.");
            }
            
            _items[itemId] = itemDefinition;
        }
       
        public ItemDefinition GetItem(string itemId)
        {
            if (_items.TryGetValue(itemId, out ItemDefinition item))
            {
                return item;
            }
            
            Debug.LogWarning($"Item with ID {itemId} not found.");
            return null;
        }
        
        public List<KeyValuePair<string, ItemDefinition>> GetItemsByCategory(string category)
        {
            List<KeyValuePair<string, ItemDefinition>> result = new List<KeyValuePair<string, ItemDefinition>>();
            
            foreach (var pair in _items)
            {
                if (pair.Value.Category == category)
                {
                    result.Add(pair);
                }
            }
            
            return result;
        }
        
        public bool HasItem(string itemId)
        {
            return _items.ContainsKey(itemId);
        }
        
        public List<string> GetAllItemIds()
        {
            return new List<string>(_items.Keys);
        }
        
        public Dictionary<string, ItemDefinition> GetAllItems()
        {
            return new Dictionary<string, ItemDefinition>(_items);
        }
        
        public void ClearItems()
        {
            _items.Clear();
        }
    }
}