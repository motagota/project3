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
        public event Action<Machine, Recipe> OnRecipeChanged;

        public int CompletedRecipes { get; private set; } = 0;
        private InventorySlot _outputSlot = new InventorySlot();
        private InventorySlot _inputSlot = new InventorySlot();
        
        // Accessor methods for input and output slots
        public InventorySlot InputSlot => _inputSlot;
        public InventorySlot OutputSlot => _outputSlot;
        
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
            CurrentRecipe = null;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
           
            // 1. Check if the machine is enabled
            if (!IsEnabled)
                return;

            // 2. Check if we have a recipe
            if (CurrentRecipe == null) return;
            
            // If we're already cooking (Progress > 0), continue the process
            if (Progress > 0)
            {
                // Process the recipe for the required duration
                Progress += dt;
                
                // When complete, create the output item
                if (Progress >= CurrentRecipe.Duration)
                {
                    // Only try to create output if the output slot isn't full
                    if (!_outputSlot.IsFull)
                    {
                        CompletedRecipes++;
                        SimulationItem newItem = new SimulationItem("1", CurrentRecipe.OutputItemType);
                        
                        // Add the item to the output slot
                        if (_outputSlot.AddItem(newItem, ItemDatabase.Instance))
                        {
                            // Notify listeners and log success
                            OnRecipeCompleted?.Invoke(this);
                            Debug.Log("Machine finished and produced: " + newItem);
                            
                            // Reset progress to start a new cycle
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
                // Continue cooking, no need to check inputs again
                return;
            }
            
            // 3. If we're not cooking yet (Progress == 0), check if we can start
            if (HasRequiredInputItems() && !_outputSlot.IsFull)
            {
                // Only consume items if the recipe requires them
                if (CurrentRecipe.InputItemCount > 0)
                {
                    ConsumeItems();
                    Debug.Log("Starting production with consumed items");
                }
                
                // Start the cooking process
                Progress += dt;
                Debug.Log("Starting cooking process");
            }
        }
        
        private bool HasRequiredInputItems()
        {
            if (CurrentRecipe.InputItemCount == 0 || CurrentRecipe.InputItemTypes.Count == 0)
                return true;
            
            // Check if we have enough items in the input slot
            if (_inputSlot.IsEmpty || _inputSlot.Count < CurrentRecipe.InputItemCount)
                return false;
                
            // Check if the item type matches what the recipe requires
            if (!CurrentRecipe.InputItemTypes.Contains(_inputSlot.ItemType))
                return false;
                
            return true;
        }
        
        private void ConsumeItems()
        {
            if (!HasRequiredInputItems())
                return;
            
            // Consume the required number of items
            for (int i = 0; i < CurrentRecipe.InputItemCount; i++)
            {
                SimulationItem consumedItem = _inputSlot.TakeItem();
                if (consumedItem != null)
                {
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
            if (CurrentRecipe == null) return false;
            
            // Check if the item type is required by the recipe
            if (!CurrentRecipe.RequiresItemType(item.ItemType))
                return false;
                
            // Check if the input slot can accept this item
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
            // Don't do anything if the recipe is the same
            if (CurrentRecipe == newRecipe) return;
          
            List<SimulationItem> itemsToGivePlayer = new List<SimulationItem>();
            
            // Clear output slot and give items to player
            while (!_outputSlot.IsEmpty)
            {
                SimulationItem item = _outputSlot.TakeItem();
                if (item != null)
                {
                    itemsToGivePlayer.Add(item);
                }
            }
            
            // Clear input slot and give items to player
            while (!_inputSlot.IsEmpty)
            {
                SimulationItem item = _inputSlot.TakeItem();
                if (item != null)
                {
                    itemsToGivePlayer.Add(item);
                }
            }
            
            // Set the new recipe
            Recipe oldRecipe = CurrentRecipe;
            CurrentRecipe = newRecipe;
            Progress = 0;
            
            // Give collected items to player
            if (PlayerInventory.Instance != null)
            {
                foreach (var item in itemsToGivePlayer)
                {
                    PlayerInventory.Instance.AddItem(item);
                }
            }
            
            // Notify listeners about recipe change
            OnRecipeChanged?.Invoke(this, oldRecipe);
            Debug.Log($"Machine {ID} recipe changed from {oldRecipe?.OutputItemType ?? "none"} to {newRecipe?.OutputItemType ?? "none"}");
        }
    }
}