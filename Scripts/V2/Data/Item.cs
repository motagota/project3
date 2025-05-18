using UnityEngine;

namespace V2.Data
{
    public class SimulationItem
    {
        public string ItemType { get; private set; }
        public string id;
        public Color ItemColor { get; private set; }
        public SimulationItem(string id, string itemType)
        {
            this.id = id;
            ItemType = itemType;
            ItemColor = Color.white; // Default color

            // Try to get color from ItemDefinition if available using the singleton pattern
            var definition = ItemDatabase.Instance.GetItem(itemType);
            if (definition != null)
            {
                ItemColor = definition.ItemColor;
            }
        }
        
          
    // Constructor with explicit color
    public SimulationItem(string id, string itemType, Color color)
    {
        this.id = id;
        ItemType = itemType;
        ItemColor = color;
    }
        public override string ToString()
        {
            return ItemType;
        }
    }
}