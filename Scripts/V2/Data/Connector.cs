using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace V2.Data
{
    public class Connector : Entity
    {
        private Entity _inputConnector;
        private Entity _outputConnector;
   
        private SimulationItem _inputHeldItem;
        private bool canDrop = true;
        private bool _shouldCheckForItems = true;
        private string _expectedOutputItemType;
        
        public bool HasInputItem => _inputHeldItem != null;
        public bool CanDropItem => canDrop;
        
        public event Action<Connector, Entity> OnConnectionChanged;
        public event Action<Connector, SimulationItem> OnItemPickedUp;
        public event Action<Connector, SimulationItem> OnItemDropped;
        
        public Connector(Vector2Int localPosition) : base(localPosition)
        {
        }

        public Vector2Int GetFrontPosition()
        {
            switch ((int)Rotation)
            {
                case 0 :
                    return LocalPosition + new Vector2Int(-1,0);
                case 90 :
                    return LocalPosition + new Vector2Int(0,-1);
                case 180 :
                    return LocalPosition + new Vector2Int(1,0);
                case 270 :
                    return LocalPosition + new Vector2Int(0,1);
                default:
                    return LocalPosition;
            }
            
        }
        
        public Vector2Int GetBackPosition()
        {
            switch ((int)Rotation)
            {
                case 0 :
                    return LocalPosition + new Vector2Int(1,0);
                case 90 :
                    return LocalPosition + new Vector2Int(0,1);
                case 180 :
                    return LocalPosition + new Vector2Int(-1,0);
                case 270 :
                    return LocalPosition + new Vector2Int(0,-1);
                default:
                    return LocalPosition;
            }
            
        }

        public void Rotate(ChunkData chunk)
        {
            base.Rotate();
            CheckForConnection(chunk);
        }

        public bool CheckForConnection(ChunkData chunk)
        {
            bool connectionsChanged = false;
    
            Vector2Int frontPos = GetFrontPosition();
            Entity frontConnection = GetEntityAt(chunk, frontPos);
    
            if (frontConnection != _inputConnector)
            {
                // Unsubscribe from old connection events
                if (_inputConnector is Machine oldInputMachine)
                {
                    oldInputMachine.OnRecipeCompleted -= HandleInputMachineRecipeCompleted;
                }
                
                _inputConnector = frontConnection;
                OnConnectionChanged?.Invoke(this, frontConnection);
                connectionsChanged = true;
                
                // Subscribe to new connection events
                if (_inputConnector is Machine inputMachine)
                {
                    inputMachine.OnRecipeCompleted += HandleInputMachineRecipeCompleted;
                     inputMachine.OnRecipeChanged += HandleInputMachineRecipeChanged;
                    // Special handling for Miner machines
                    if (_inputConnector is Miner miner)
                    {
                        _expectedOutputItemType = miner.GetOreType();
                        _shouldCheckForItems = true;
                    }
                }
            }
    
            // Check back connection
            Vector2Int backPos = GetBackPosition();
            Entity backConnection = GetEntityAt(chunk, backPos);
    
            if (backConnection != _outputConnector)
            {
                _outputConnector = backConnection;
                OnConnectionChanged?.Invoke(this, backConnection);
                connectionsChanged = true;
            }
            return connectionsChanged;
        }

        private Entity GetEntityAt(ChunkData chunk, Vector2Int position)
        {
            Entity entity = chunk.GetMachineAt(position);
    
            if (entity == null)
            {
                entity = chunk.GetBeltAt(position);
            }
            return entity;
        }

        public Entity GetInputConnectedMachine()
        {
            return _inputConnector;
        }
        
        public Entity GetOutputConnectedMachine()
        {
            return _outputConnector;
        }
        
        public override void Tick(float dt)
        {
            base.Tick(dt);
   
            if (_inputHeldItem == null)
            {
                TryPickUpItem();
            }
            else
            {
                TryDropItem();
            }
        }
        
        private void HandleInputMachineRecipeChanged(Machine machine, Recipe oldRecipe)
{
    _shouldCheckForItems = true;
}

        private void HandleInputMachineRecipeCompleted(Machine machine)
        {
            _shouldCheckForItems = true;
        }
        
        private void TryPickUpItem()
        {
            if (!_shouldCheckForItems)
                return;
            
            if (_inputConnector is Machine inputMachine)
            {
                if (inputMachine is Miner miner)
                {
                    if (inputMachine.HasItem)
                    {
                        SimulationItem peekItem = new SimulationItem("1", _expectedOutputItemType);
                        if (CanOutputAcceptItem(peekItem))
                        {
                            SimulationItem item = inputMachine.TakeItem();
                            if (item != null)
                            {
                                _inputHeldItem = item;
                                OnItemPickedUp?.Invoke(this, item);
                                _shouldCheckForItems = false; 
                            }
                        }
                        else
                        {
                           // Debug.Log($"Connector {ID} skipped item from miner {inputMachine.ID} because output cannot accept it");
                            _shouldCheckForItems = false; 
                        }
                    }
                }
                else if (inputMachine.HasItem)
                {
                    SimulationItem peekItem = inputMachine.TakeItem();
                    if (peekItem != null)
                    {
                        inputMachine.GiveItem(peekItem);
                     
                        if (CanOutputAcceptItem(peekItem))
                        {
                            SimulationItem item = inputMachine.TakeItem();
                            if (item != null)
                            {
                                _inputHeldItem = item;
                                OnItemPickedUp?.Invoke(this, item);
                                _shouldCheckForItems = false; 
                            }
                        }
                        else
                        {
                          //  Debug.Log($"Connector {ID} skipped item from machine {inputMachine.ID} because output cannot accept it");
                            _shouldCheckForItems = false; 
                        }
                    }
                }
            }
            else if (_inputConnector is BeltData inputBelt)
            {
                Dictionary<SimulationItem, float> items = inputBelt.GetAllItemsWithProgress();
                SimulationItem firstItem = null;
                float highestProgress = 0f;
                foreach (var kvp in items)
                {
                    if (kvp.Value > highestProgress)
                    {
                        highestProgress = kvp.Value;
                        firstItem = kvp.Key;
                    }
                }
                
                if (firstItem != null && highestProgress >= 0.9f)
                {
                    if (CanOutputAcceptItem(firstItem))
                    {
                        SimulationItem item = inputBelt.TakeItem();
                        if (item != null)
                        {
                            _inputHeldItem = item;
                            OnItemPickedUp?.Invoke(this, item);
                        }
                    }
                    else
                    {
                       // Debug.Log($"Connector {ID} skipped item from belt {inputBelt.ID} because output cannot accept it");
                    }
                }
            }
        }
        
        private bool CanOutputAcceptItem(SimulationItem item)
        {
            if (_outputConnector == null)
                return false;
         
            if (_outputConnector is BeltData)
                return true;
             
            if (_outputConnector is Machine outputMachine)
                return outputMachine.CanAcceptItem(item);
                
            return false;
        }
        
        private void TryDropItem()
        {
            if (_outputConnector is Machine outputMachine)
            {
                if (outputMachine.CanAcceptItem(_inputHeldItem) && outputMachine.GiveItem(_inputHeldItem))
                {
                    SimulationItem droppedItem = _inputHeldItem;
                    _inputHeldItem = null;
                    OnItemDropped?.Invoke(this, droppedItem);
                    canDrop = true;
                    _shouldCheckForItems = true; 
                }
                else
                {
                    canDrop = false;
                }
            }
            else if (_outputConnector is BeltData outputBelt)
            {
                if (outputBelt.AcceptItem(_inputHeldItem))
                {
                    SimulationItem droppedItem = _inputHeldItem;
                    _inputHeldItem = null;
                    OnItemDropped?.Invoke(this, droppedItem);
                    canDrop = true;
                    _shouldCheckForItems = true; 
                }
                else
                {
                    canDrop = false;
                }
            }
            else
            { 
                _inputHeldItem = null;
                canDrop = true;
                _shouldCheckForItems = true; 
            }
        }
 
        public SimulationItem GetHeldItem()
        {
            return _inputHeldItem;
        }
    }
}