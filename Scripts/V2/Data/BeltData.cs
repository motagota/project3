using System;
using System.Collections.Generic;
using UnityEngine;

namespace V2.Data
{
    public class BeltData : Entity
    {
        private BeltData _nextBelt;
        private BeltData _previousBelt;
        
        private List<SimulationItem> _items = new List<SimulationItem>();
        private Dictionary<SimulationItem, float> _itemProgress = new Dictionary<SimulationItem, float>();
        
        private const float BELT_SPEED = 1.0f;
        private const int MAX_ITEMS = 5;
        private const float MIN_SPACING = 0.2f; // Minimum spacing between items (0-1 scale)
        
        public bool HasItem => _items.Count > 0;
        
        public event Action<BeltData, SimulationItem> OnItemAdded;
        public event Action<BeltData, SimulationItem> OnItemRemoved;
        public event Action<BeltData, BeltData> OnConnectionChanged;
        
        public BeltData(Vector2Int localPosition) : base(localPosition)
        {
        }
        
        public override void Tick(float dt)
        {
            base.Tick(dt);
           
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                SimulationItem item = _items[i];
                float progress = _itemProgress[item];
                
                progress += BELT_SPEED * dt;
               
                if (progress >= 1.0f)
                {
                    if (_nextBelt != null && _nextBelt.CanAcceptItem())
                    {
                        _nextBelt.AcceptItem(item);
                        _items.RemoveAt(i);
                        _itemProgress.Remove(item);
                        OnItemRemoved?.Invoke(this, item);
                    }
                    else
                    {
                        _itemProgress[item] = 1.0f;
                    }
                }
                else
                {
                    if (i > 0 && progress + MIN_SPACING > _itemProgress[_items[i-1]])
                    {
                        progress = _itemProgress[_items[i-1]] - MIN_SPACING;
                    }
                    _itemProgress[item] = progress;
                }
            }
        }
        
        public bool CanAcceptItem()
        {
            if (_items.Count >= MAX_ITEMS)
                return false;
                
            if (_items.Count > 0 && _itemProgress[_items[_items.Count - 1]] < MIN_SPACING)
                return false;
                
            return true;
        }
        
        public bool AcceptItem(SimulationItem item)
        {
            if (!CanAcceptItem())
                return false;
                
            _items.Add(item);
            _itemProgress[item] = 0f;
            OnItemAdded?.Invoke(this, item);
            return true;
        }
        
        public SimulationItem TakeItem()
        {
            // Take the first item if it's near the end
            if (_items.Count > 0 && _itemProgress[_items[0]] >= 0.9f)
            {
                SimulationItem item = _items[0];
                _items.RemoveAt(0);
                _itemProgress.Remove(item);
                OnItemRemoved?.Invoke(this, item);
                return item;
            }
            
            return null;
        }
        
       
        public Dictionary<SimulationItem, float> GetAllItemsWithProgress()
        {
            // Return a copy of the progress dictionary
            return new Dictionary<SimulationItem, float>(_itemProgress);
        }
        
        public Vector2Int GetNextPosition()
        {
            switch ((int)Rotation)
            {
                case 0:  // Forward (Z+)
                    return LocalPosition + new Vector2Int(0, 1);
                case 90:  // Right (X+)
                    return LocalPosition + new Vector2Int(1, 0);
                case 180:  // Backward (Z-)
                    return LocalPosition + new Vector2Int(0, -1);
                case 270:  // Left (X-)
                    return LocalPosition + new Vector2Int(-1, 0);
                default:
                    return LocalPosition;
            }
        }
        
        public Vector2Int GetPreviousPosition()
        {
            switch ((int)Rotation)
            {
                case 0:  // Forward (Z+)
                    return LocalPosition + new Vector2Int(0, -1);
                case 90:  // Right (X+)
                    return LocalPosition + new Vector2Int(-1, 0);
                case 180:  // Backward (Z-)
                    return LocalPosition + new Vector2Int(0, 1);
                case 270:  // Left (X-)
                    return LocalPosition + new Vector2Int(1, 0);
                default:
                    return LocalPosition;
            }
        }
        
        public void CheckConnections(ChunkData chunk)
        {
            Vector2Int nextPos = GetNextPosition();
            Entity nextEntity = chunk.GetBeltAt(nextPos);
            
            if (nextEntity is BeltData nextBelt && nextBelt != _nextBelt)
            {
                _nextBelt = nextBelt;
                OnConnectionChanged?.Invoke(this, nextBelt);
            }
            else if (nextEntity == null && _nextBelt != null)
            {
                _nextBelt = null;
                OnConnectionChanged?.Invoke(this, null);
            }
            
        
            Vector2Int prevPos = GetPreviousPosition();
            Entity prevEntity = chunk.GetBeltAt(prevPos);
            
            if (prevEntity is BeltData prevBelt && prevBelt != _previousBelt)
            {
                _previousBelt = prevBelt;
                OnConnectionChanged?.Invoke(this, prevBelt);
            }
            else if (prevEntity == null && _previousBelt != null)
            {
                _previousBelt = null;
                OnConnectionChanged?.Invoke(this, null);
            }
        }
        
        public void Rotate(ChunkData chunk)
        {
            base.Rotate();
            CheckConnections(chunk);
        }
        
        public BeltData GetNextBelt()
        {
            return _nextBelt;
        }
        
        public BeltData GetPreviousBelt()
        {
            return _previousBelt;
        }
    }
}