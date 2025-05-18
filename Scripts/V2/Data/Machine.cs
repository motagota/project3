using UnityEngine;
using System;
using System.Collections.Generic;

namespace V2.Data
{
    public class Machine : Entity
    {
        private const float CONSUMPTION_RATE = 1f / 3f;

        private float _consumptionTimer = 0f;
        private InventorySlot _inputSlot = new InventorySlot();

        private bool _isEnabled = true;
        private InventorySlot _outputSlot = new InventorySlot();
        public Recipe CurrentRecipe;
        public float Progress;

        public Machine(Vector2Int localPosition) : base(localPosition)
        {
            CurrentRecipe = null;
        }

        public int CompletedRecipes { get; private set; } = 0;

        // Accessor methods for input and output slots
        public InventorySlot InputSlot => _inputSlot;
        public InventorySlot OutputSlot => _outputSlot;

        public bool HasItem => !_outputSlot.IsEmpty;

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

        public event Action<Machine> OnRecipeCompleted;
        public event Action<Machine, SimulationItem> OnItemConsumed;
        public event Action<Machine> OnEnabledStateChanged;
        public event Action<Machine, Recipe> OnRecipeChanged;

        public override void Tick(float dt)
        {
            base.Tick(dt);
           
            // 1. Check if the machine is enabled
            if (!IsEnabled)
            {
                return;
            }

            if (CurrentRecipe == null) {return;}
            
            if (Progress > 0)
            {
                Progress += dt;
                if (Progress >= CurrentRecipe.Duration)
                {
                    if (!_outputSlot.IsFull)
                    {
                        CompletedRecipes++;
                        SimulationItem newItem = new SimulationItem("1", CurrentRecipe.OutputItemType);
                        
                        if (_outputSlot.AddItem(newItem, ItemDatabase.Instance))
                        {
                            OnRecipeCompleted?.Invoke(this);
                            Progress = 0;
                        }
                        else
                        {
                            Debug.LogWarning("Machine could not add produced item to output slot");
                            // Don't reset progress, we'll try again next tick
                        }
                    }
                    else
                    {
                        // Output slot is full, wait until it's emptied
                        Debug.Log("Machine production complete but output slot is full");
                        // Don't reset progress, we'll try again when output slot is available
                    }
                }
                return;
            }
            
            if (HasRequiredInputItems() && !_outputSlot.IsFull)
            {
                if (CurrentRecipe.InputItemCount > 0)
                {
                    ConsumeItems();
                }
                
                Progress += dt;
            }
        }

        private bool HasRequiredInputItems()
        {
            if (CurrentRecipe.InputItemCount == 0 || CurrentRecipe.InputItemTypes.Count == 0)
            {
                return true;
            }

            if (_inputSlot.IsEmpty || _inputSlot.Count < CurrentRecipe.InputItemCount){
                return false;
            }

            if (!CurrentRecipe.InputItemTypes.Contains(_inputSlot.ItemType))
            {
                return false;
            }

            return true;
        }

        private void ConsumeItems()
        {
            if (!HasRequiredInputItems())
                return;
            
            for (int i = 0; i < CurrentRecipe.InputItemCount; i++)
            {
                SimulationItem consumedItem = _inputSlot.TakeItem();
                if (consumedItem != null)
                {
                    OnItemConsumed?.Invoke(this, consumedItem);
                }
            }
        }

        public SimulationItem TakeItem()
        {
            return _outputSlot.TakeItem();
        }

        public virtual bool CanAcceptItem(SimulationItem item)
        {
            if (CurrentRecipe == null) return false;
            if (!CurrentRecipe.RequiresItemType(item.ItemType))
                return false;
             
            return _inputSlot.CanAcceptItem(item, ItemDatabase.Instance);
        }

        public virtual bool GiveItem(SimulationItem item)
        {
            if (!CanAcceptItem(item))
                return false;
            
            return _inputSlot.AddItem(item, ItemDatabase.Instance);
        }

        public void SetRecipe(Recipe newRecipe)
        {
            if (CurrentRecipe == newRecipe) return;
            List<SimulationItem> itemsToGivePlayer = new List<SimulationItem>();
            
            while (!_outputSlot.IsEmpty)
            {
                SimulationItem item = _outputSlot.TakeItem();
                if (item != null)
                {
                    itemsToGivePlayer.Add(item);
                }
            }
            while (!_inputSlot.IsEmpty)
            {
                SimulationItem item = _inputSlot.TakeItem();
                if (item != null)
                {
                    itemsToGivePlayer.Add(item);
                }
            }
            Recipe oldRecipe = CurrentRecipe;
            CurrentRecipe = newRecipe;
            Progress = 0;
            if (PlayerInventory.Instance != null)
            {
                foreach (var item in itemsToGivePlayer)
                {
                    PlayerInventory.Instance.AddItem(item);
                }
            }
            OnRecipeChanged?.Invoke(this, oldRecipe);
        }
    }
}