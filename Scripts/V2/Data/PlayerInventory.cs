using System;
using System.Collections.Generic;
using UnityEngine;

namespace V2.Data
{
    public class PlayerInventory
    {
        private static PlayerInventory _instance;
        
        // Singleton pattern to ensure only one player inventory exists
        public static PlayerInventory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayerInventory();
                }
                return _instance;
            }
        }
        
        private List<InventorySlot> _slots = new List<InventorySlot>();
        private int _maxSlots = 20;
        
        public event Action<PlayerInventory, SimulationItem> OnItemAdded;
        public event Action<PlayerInventory, SimulationItem> OnItemRemoved;
        
        private PlayerInventory(int maxSlots = 20)
        {
            _maxSlots = maxSlots;
            // Initialize empty slots
            for (int i = 0; i < _maxSlots; i++)
            {
                _slots.Add(new InventorySlot());
            }
        }
        
        public bool AddItem(SimulationItem item)
        {
            // First try to add to existing slots with same item type
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.ItemType == item.ItemType && !slot.IsFull)
                {
                    if (slot.AddItem(item, ItemDatabase.Instance))
                    {
                        OnItemAdded?.Invoke(this, item);
                        return true;
                    }
                }
            }
            
            // Then try to add to empty slots
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty)
                {
                    if (slot.AddItem(item, ItemDatabase.Instance))
                    {
                        OnItemAdded?.Invoke(this, item);
                        return true;
                    }
                }
            }
            
            // Inventory is full
            Debug.LogWarning("Player inventory is full, couldn't add item: " + item.ItemType);
            return false;
        }
        
        public SimulationItem TakeItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count)
                return null;
                
            SimulationItem item = _slots[slotIndex].TakeItem();
            if (item != null)
            {
                OnItemRemoved?.Invoke(this, item);
            }
            return item;
        }
        
        public List<InventorySlot> GetSlots()
        {
            return _slots;
        }
        
        public int GetItemCount(string itemType)
        {
            int count = 0;
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.ItemType == itemType)
                {
                    count += slot.Count;
                }
            }
            return count;
        }
    }
}