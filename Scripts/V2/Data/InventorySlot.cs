using System.Collections.Generic;
using UnityEngine;

namespace V2.Data
{
    public class InventorySlot
    {
        private List<SimulationItem> _items = new List<SimulationItem>();
        private string _itemType;
        private int _maxStackSize = 99;
        private bool _isStackable = true;
        
        public bool IsEmpty => _items.Count == 0;
        public int Count => _items.Count;
        public string ItemType => _itemType;
        public bool IsFull => _isStackable ? _items.Count >= _maxStackSize : _items.Count >= 1;
        
        public InventorySlot()
        {
            _items = new List<SimulationItem>();
        }
        
        public bool CanAcceptItem(SimulationItem item, ItemDatabase itemDatabase)
        {
            // If slot is empty, we can accept any item
            if (IsEmpty)
            {
                // Set up the slot based on the item definition
                ItemDefinition itemDef = itemDatabase.GetItem(item.ItemType);
                if (itemDef != null)
                {
                    _isStackable = itemDef.Stackable;
                    _maxStackSize = itemDef.MaxStackSize;
                }
                return true;
            }
            
            // If slot is not empty, we can only accept items of the same type
            // and only if the slot is not full
            return item.ItemType == _itemType && !IsFull;
        }
        
        public bool AddItem(SimulationItem item, ItemDatabase itemDatabase)
        {
            if (!CanAcceptItem(item, itemDatabase))
                return false;
                
            if (IsEmpty)
            {
                _itemType = item.ItemType;
                
                // Set up the slot based on the item definition
                ItemDefinition itemDef = itemDatabase.GetItem(item.ItemType);
                if (itemDef != null)
                {
                    _isStackable = itemDef.Stackable;
                    _maxStackSize = itemDef.MaxStackSize;
                }
            }
            
            _items.Add(item);
            return true;
        }
        
        public SimulationItem TakeItem()
        {
            if (IsEmpty)
                return null;
                
            SimulationItem item = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);
            
            // Reset the slot if it's now empty
            if (IsEmpty)
            {
                _itemType = null;
                _isStackable = true;
                _maxStackSize = 99;
            }
            
            return item;
        }
        
        public SimulationItem PeekItem()
        {
            if (IsEmpty)
                return null;
                
            return _items[_items.Count - 1];
        }
        
        public void Clear()
        {
            _items.Clear();
            _itemType = null;
            _isStackable = true;
            _maxStackSize = 99;
        }
    }
}