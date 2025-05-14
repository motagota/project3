using System.Security.Cryptography;
using UnityEngine;

namespace V2.Data
{
    
    [System.Serializable]
    public class ItemDefinition
    {
        // Basic properties
        public string Id;
        public string DisplayName;
        public string Description;
        public string Category;
   
        public bool Stackable;
        public int MaxStackSize;
      
        /// <summary>
        /// Create a new item definition with basic properties.
        /// </summary>
        /// <param name="displayName">Human-readable name of the item</param>
        /// <param name="description">Description of the item</param>
        /// <param name="category">Category the item belongs to (e.g., RawMaterial, Component)</param>
        /// <param name="stackable">Whether the item can be stacked in inventory</param>
        /// <param name="maxStackSize">Maximum number of items in a stack (if stackable)</param>
        public ItemDefinition(string id, string displayName, string description, string category, bool stackable = true, int maxStackSize = 99)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Category = category;
            Stackable = stackable;
            MaxStackSize = maxStackSize;
        }
    
        /// <summary>
        /// Create a copy of this item definition.
        /// </summary>
        /// <returns>A new ItemDefinition with the same properties</returns>
        public ItemDefinition Clone()
        {
            ItemDefinition clone = new ItemDefinition("1",DisplayName, Description, Category, Stackable, MaxStackSize);
            
            return clone;
        }
    }
}