using UnityEngine;
using System;
using System.Collections.Generic;

namespace V2.Data
{
    public class Machine : Entity
    {
        public Recipe CurrentRecipe;
        public float Progress;
        public event Action<Machine> OnRecipeCompleted;
        public event Action<Machine, SimulationItem> OnItemConsumed;
        public event Action<Machine> OnEnabledStateChanged;

        public int CompletedRecipes { get; private set; } = 0;
        private InventorySlot _outputSlot = new InventorySlot();
        private Dictionary<string, List<SimulationItem>> _inputItems = new Dictionary<string, List<SimulationItem>>();
        
        public bool HasItem => !_outputSlot.IsEmpty;
        
        private bool _isEnabled = true;
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
        
        private float _consumptionTimer = 0f;
        private const float CONSUMPTION_RATE = 1f / 3f; 
        
        public Machine(Vector2Int localPosition) : base(localPosition)
        {
            CurrentRecipe = new Recipe(1);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
           
            if (!IsEnabled)
                return;
        
            if (CurrentRecipe.InputItemCount > 0)
            {
                _consumptionTimer += dt;
                if (_consumptionTimer >= CONSUMPTION_RATE)
                {
                    _consumptionTimer -= CONSUMPTION_RATE;
                    ConsumeItems();
                }
            }
            
            // Only produce if the output slot can accept more items
            if (!_outputSlot.IsFull)
            {
                if (HasRequiredInputItems())
                {
                    Progress += dt;
                    if (Progress >= CurrentRecipe.Duration)
                    {
                        Progress = 0;
                        CompletedRecipes++;
                        SimulationItem newItem = new SimulationItem("1", CurrentRecipe.OutputItemType);
                        
                        // Add the item to the output slot
                        if (_outputSlot.AddItem(newItem, ItemDatabase.Instance))
                        {
                            OnRecipeCompleted?.Invoke(this);
                            Debug.Log("Machine finished and produced: " + newItem);
                        }
                        else
                        {
                            Debug.LogWarning("Machine could not add produced item to output slot");
                        }
                    }
                }
            }
        }
        
        private bool HasRequiredInputItems()
        {
            if (CurrentRecipe.InputItemCount == 0 || CurrentRecipe.InputItemTypes.Count == 0)
                return true;
            
            foreach (string itemType in CurrentRecipe.InputItemTypes)
            {
                if (!_inputItems.ContainsKey(itemType) || 
                    _inputItems[itemType].Count < CurrentRecipe.InputItemCount)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private void ConsumeItems()
        {
        
            if (!HasRequiredInputItems())
                return;
             
            foreach (string itemType in CurrentRecipe.InputItemTypes)
            {
                if (_inputItems.ContainsKey(itemType) && _inputItems[itemType].Count > 0)
                {
                    SimulationItem consumedItem = _inputItems[itemType][0];
                    _inputItems[itemType].RemoveAt(0);
                    OnItemConsumed?.Invoke(this, consumedItem);
                    Debug.Log("Machine consumed: " + consumedItem);
                }
            }
        }
        
        public SimulationItem TakeItem()
        {
            return _outputSlot.TakeItem();
        }
        
        public virtual bool CanAcceptItem(SimulationItem item)
        {
            return CurrentRecipe.RequiresItemType(item.ItemType);
        }
 
        public virtual bool GiveItem(SimulationItem item)
        {
            if (!CanAcceptItem(item))
                return false;
          
            if (!_inputItems.ContainsKey(item.ItemType))
            {
                _inputItems[item.ItemType] = new List<SimulationItem>();
            }
            _inputItems[item.ItemType].Add(item);
            return true;
        }
        
    }
}