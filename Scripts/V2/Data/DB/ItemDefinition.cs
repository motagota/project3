using System.Security.Cryptography;
using UnityEngine;

namespace V2.Data
{
    /// <summary>
    /// Defines the properties and behavior of an item type in the simulation.
    /// </summary>
    [System.Serializable]
    public class ItemDefinition
    {
        // Basic properties
        public string Id;
        public string DisplayName;
        public string Description;
        public string Category;
        
        // Inventory properties
        public bool Stackable;
        public int MaxStackSize;
        
        // Visual properties
        public string IconPath;
        public Color IconColor = Color.white;
        
        // Physical properties
        public float Mass = 1.0f;
        public Vector3 Size = Vector3.one;
        
        // Gameplay properties
        public float Value = 1.0f;
        public bool Consumable = false;
        public float ConsumptionEffect = 0.0f;
        
        // Custom properties
        public System.Collections.Generic.Dictionary<string, object> CustomProperties = new System.Collections.Generic.Dictionary<string, object>();
        
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
        /// Set a custom property value.
        /// </summary>
        /// <param name="key">The property key</param>
        /// <param name="value">The property value</param>
        public void SetCustomProperty(string key, object value)
        {
            CustomProperties[key] = value;
        }
        
        /// <summary>
        /// Get a custom property value.
        /// </summary>
        /// <typeparam name="T">The expected type of the property</typeparam>
        /// <param name="key">The property key</param>
        /// <param name="defaultValue">Default value to return if the property doesn't exist</param>
        /// <returns>The property value, or defaultValue if not found</returns>
        public T GetCustomProperty<T>(string key, T defaultValue = default)
        {
            if (CustomProperties.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }
            
            return defaultValue;
        }
        
        /// <summary>
        /// Set the visual properties of the item.
        /// </summary>
        /// <param name="iconPath">Path to the icon resource</param>
        /// <param name="iconColor">Tint color for the icon</param>
        public void SetVisualProperties(string iconPath, Color iconColor)
        {
            IconPath = iconPath;
            IconColor = iconColor;
        }
        
        /// <summary>
        /// Set the physical properties of the item.
        /// </summary>
        /// <param name="mass">Mass of the item in kg</param>
        /// <param name="size">Size of the item in world units</param>
        public void SetPhysicalProperties(float mass, Vector3 size)
        {
            Mass = mass;
            Size = size;
        }
        
        /// <summary>
        /// Set the gameplay properties of the item.
        /// </summary>
        /// <param name="value">Value/cost of the item</param>
        /// <param name="consumable">Whether the item can be consumed</param>
        /// <param name="consumptionEffect">Effect value when consumed</param>
        public void SetGameplayProperties(float value, bool consumable, float consumptionEffect = 0.0f)
        {
            Value = value;
            Consumable = consumable;
            ConsumptionEffect = consumptionEffect;
        }
        
        /// <summary>
        /// Create a copy of this item definition.
        /// </summary>
        /// <returns>A new ItemDefinition with the same properties</returns>
        public ItemDefinition Clone()
        {
            ItemDefinition clone = new ItemDefinition("1",DisplayName, Description, Category, Stackable, MaxStackSize);
            
            // Copy visual properties
            clone.IconPath = IconPath;
            clone.IconColor = IconColor;
            
            // Copy physical properties
            clone.Mass = Mass;
            clone.Size = Size;
            
            // Copy gameplay properties
            clone.Value = Value;
            clone.Consumable = Consumable;
            clone.ConsumptionEffect = ConsumptionEffect;
            
            // Copy custom properties
            foreach (var pair in CustomProperties)
            {
                clone.CustomProperties[pair.Key] = pair.Value;
            }
            
            return clone;
        }
    }
}