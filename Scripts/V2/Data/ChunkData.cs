using System;
using System.Collections.Generic;
using UnityEngine;

namespace V2.Data
{
    public class ChunkData
    {
        public readonly Vector2Int Coords;
        List<Machine> _machines;
        List<Connector> _connectors;
        List<BeltData> _belts;
        
        public const int ChunkSize = 32;
        
        public bool isDirty;

        // Events for machine tracking
        public event Action<Machine> OnMachineAdded;
        public event Action<Machine> OnMachineRemoved;
        
        //Events for Connector trackiong
        public event Action<Connector> OnConnectorAdded;
        public event Action<Connector> OnConnectorRemoved;
        
        //Events for Belt trackiong
        public event Action<BeltData> OnBeltAdded;
         public event Action<BeltData> OnBeltRemoved;
        
        // Property still useful for direct access when needed
        public int MachineCount => _machines.Count;
        
        public ChunkData(Vector2Int coords)
        {
            Coords = coords;
            _machines = new List<Machine>();
            _connectors = new List<Connector>();
            _belts = new List<BeltData>();
        }

        public void AddConnector(Connector connector)
        {
            _connectors.Add(connector);
            MarkDirty();
            OnConnectorAdded?.Invoke(connector);
            connector.CheckForConnection(this);
        }

        public void AddBelt(BeltData belt)
        {
            _belts.Add(belt);
            MarkDirty();
            OnBeltAdded?.Invoke(belt);
        }
        public void AddMachine(Machine machine)
        {
            _machines.Add(machine);
            MarkDirty();
            OnMachineAdded?.Invoke(machine);
        }
        
        public bool RemoveMachine(Machine machine)
        {
            bool removed = _machines.Remove(machine);
            if (removed)
            {
                MarkDirty();
                OnMachineRemoved?.Invoke(machine);
            }
            return removed;
        }

        private void MarkDirty()
        {
            isDirty = true;
        }

        public void ProcessTick(float dt)
        {
            foreach (var m in _machines)
            {
                m.Tick(dt);
              
            }  
            MarkDirty();
        }

        public IEnumerable<Vector2Int> GetNeighborsThatChanged()
        {
            yield break;
        }

        public List<Machine> GetMachines()
        {
            return _machines;
        }
        
        public List<Connector> GetConnectors()
        {
            return _connectors;
        }

        public List<BeltData> GetBelts()
        {
            return _belts;
        }

        public Machine GetMachineAt(Vector2Int pos)
        {
            foreach (var m in _machines)
            {
                if (m.LocalPostion == pos)
                {
                    return m;
                }
            }

            return null;
        }
        
        public BeltData GetBeltAt(Vector2Int pos)
        {
            foreach (var b in _belts) 
            {
                if (b.LocalPostion == pos)
                {
                    return b;
                }
            }

            return null;
        }
    }
}