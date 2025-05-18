using System.Collections.Generic;
using UnityEngine;

namespace V2.Data
{
    public class StorageBox : Entity
    {
        private const int DEFAULT_SLOT_COUNT = 12;
        
        private List<InventorySlot> _slots;
        private bool _isEnabled = true;
        
        public int SlotCount => _slots.Count;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set 
            { 
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnEnabledStateChanged?.Invoke(this);
                }
            }
        }
        
        public event System.Action<V2.Data.StorageBox> OnEnabledStateChanged;
        public event System.Action<V2.Data.StorageBox, SimulationItem> OnItemAdded;
        public event System.Action<V2.Data.StorageBox, SimulationItem> OnItemRemoved;
        
        public StorageBox(Vector2Int localPosition, int slotCount = DEFAULT_SLOT_COUNT) : base(localPosition)
        {
            _slots = new List<InventorySlot>();
            
            // Initialize slots
            for (int i = 0; i < slotCount; i++)
            {
                _slots.Add(new InventorySlot());
            }
        }
        
        public override void Tick(float dt)
        {
            base.Tick(dt);
            
            // Any time-based logic for the storage box can go here
        }
        
        // Check if the storage has any items
        public bool HasItem
        {
            get
            {
                foreach (var slot in _slots)
                {
                    if (!slot.IsEmpty)
                        return true;
                }
                return false;
            }
        }
        
        // Get the first available item for output
        public SimulationItem TakeItem()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsEmpty)
                {
                    SimulationItem item = _slots[i].TakeItem();
                    if (item != null)
                    {
                        OnItemRemoved?.Invoke(this, item);
                        return item;
                    }
                }
            }
            return null;
        }
        
        // Try to take a specific item type
        public SimulationItem TakeItem(string itemType)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsEmpty && _slots[i].ItemType == itemType)
                {
                    SimulationItem item = _slots[i].TakeItem();
                    if (item != null)
                    {
                        OnItemRemoved?.Invoke(this, item);
                        return item;
                    }
                }
            }
            return null;
        }
        
        // Check if the storage can accept an item
        public bool CanAcceptItem(SimulationItem item)
        {
            if (item == null)
                return false;
                
            // First check if we can add to existing slots with the same item type
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.ItemType == item.ItemType && !slot.IsFull)
                {
                    return true;
                }
            }
            
            // Then check for empty slots
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        // Add an item to the storage
        public bool GiveItem(SimulationItem item)
        {
            if (item == null)
                return false;
                
            // First try to add to existing slots with the same item type
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
            
            return false;
        }
        
        // Get all slots for UI display
        public List<InventorySlot> GetSlots()
        {
            return _slots;
        }
        
        // Get total count of a specific item type
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