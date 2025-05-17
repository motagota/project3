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
      
      
        public ItemDefinition(string id, string displayName, string description, string category, bool stackable = true, int maxStackSize = 99)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Category = category;
            Stackable = stackable;
            MaxStackSize = maxStackSize;
        }
        public ItemDefinition Clone()
        {
            ItemDefinition clone = new ItemDefinition("1",DisplayName, Description, Category, Stackable, MaxStackSize);
            
            return clone;
        }
    }
}