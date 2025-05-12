using System;
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
                    return LocalPostion + new Vector2Int(-1,0);
                case 90 :
                    return LocalPostion + new Vector2Int(0,-1);
                case 180 :
                    return LocalPostion + new Vector2Int(1,0);
                case 270 :
                    return LocalPostion + new Vector2Int(0,1);
                default:
                    return LocalPostion;
            }
            
        }
        
        public Vector2Int GetBackPosition()
        {
            switch ((int)Rotation)
            {
                case 0 :
                    return LocalPostion + new Vector2Int(1,0);
                case 90 :
                    return LocalPostion + new Vector2Int(0,1);
                case 180 :
                    return LocalPostion + new Vector2Int(-1,0);
                case 270 :
                    return LocalPostion + new Vector2Int(0,-1);
                default:
                    return LocalPostion;
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
    
            // Check front connection
            Vector2Int frontPos = GetFrontPosition();
            Entity frontConnection = GetEntityAt(chunk, frontPos);
    
            if (frontConnection != _inputConnector)
            {
                _inputConnector = frontConnection;
                OnConnectionChanged?.Invoke(this, frontConnection);
                connectionsChanged = true;
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
        
        private void TryPickUpItem()
        {
            if (_inputConnector is Machine inputMachine)
            {
                SimulationItem item = inputMachine.TakeItem();
                if (item != null)
                {
                    _inputHeldItem = item;
                    OnItemPickedUp?.Invoke(this, item);
                    Debug.Log($"Connector {ID} picked up {item} from machine {inputMachine.ID}");
                }
            }
            else if (_inputConnector is BeltData inputBelt)
            {
                SimulationItem item = inputBelt.TakeItem();
                if (item != null)
                {
                    _inputHeldItem = item;
                    OnItemPickedUp?.Invoke(this, item);
                    Debug.Log($"Connector {ID} picked up {item} from belt {inputBelt.ID}");
                }
            }
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
                    Debug.Log($"Connector {ID} dropped {droppedItem} to machine {outputMachine.ID}");
                    canDrop = true;
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
                    Debug.Log($"Connector {ID} dropped {droppedItem} to belt {outputBelt.ID}");
                    canDrop = true;
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
            }
        }
 
        public SimulationItem GetHeldItem()
        {
            return _inputHeldItem;
        }
    }
}