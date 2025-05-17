using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        
        // Performance tracking
        public static PerformanceStats PerformanceStats { get; private set; } = new PerformanceStats();

        public event Action<Machine> OnMachineAdded;
        public event Action<Machine> OnMachineRemoved;

        public event Action<Connector> OnConnectorAdded;
        public event Action<Connector> OnConnectorRemoved;

        public event Action<BeltData> OnBeltAdded;
         public event Action<BeltData> OnBeltRemoved;
        
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
            Stopwatch totalStopwatch = new Stopwatch();
            Stopwatch componentStopwatch = new Stopwatch();
            float elapsedMs;
            
            // Start timing the entire process
            totalStopwatch.Start();
            
            // Time machines processing
            componentStopwatch.Restart();
            foreach (var m in _machines)
            {
                m.Tick(dt);
            }
            componentStopwatch.Stop();
            elapsedMs = componentStopwatch.ElapsedTicks / (float)Stopwatch.Frequency * 1000f;
            PerformanceStats.MachineStats.RecordTime(elapsedMs);
            
            // Time connectors processing
            componentStopwatch.Restart();
            foreach (var c in _connectors)
            {
                c.Tick(dt);
            }
            componentStopwatch.Stop();
            elapsedMs = componentStopwatch.ElapsedTicks / (float)Stopwatch.Frequency * 1000f;
            PerformanceStats.ConnectorStats.RecordTime(elapsedMs);
            
            // Time belts processing
            componentStopwatch.Restart();
            foreach (var b in _belts)
            {   
                b.Tick(dt);
            }
            componentStopwatch.Stop();
            elapsedMs = componentStopwatch.ElapsedTicks / (float)Stopwatch.Frequency * 1000f;
            PerformanceStats.BeltStats.RecordTime(elapsedMs);
            
            // Stop timing the entire process and record total time
            totalStopwatch.Stop();
            float totalElapsedMs = totalStopwatch.ElapsedTicks / (float)Stopwatch.Frequency * 1000f;
            PerformanceStats.TotalStats.RecordTime(totalElapsedMs);
            
            MarkDirty();
        }

        public IEnumerable<Vector2Int> GetNeighborsThatChanged()
        {
            yield break;
        }
        
        public Machine GetMachineAt(Vector2Int position)
        {
            foreach (var machine in _machines)
            {
                if (machine.LocalPosition == position)
                {
                    return machine;
                }
            }
            return null;
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
      
        public BeltData GetBeltAt(Vector2Int pos)
        {
            foreach (var b in _belts) 
            {
                if (b.LocalPosition == pos)
                {
                    return b;
                }
            }
            return null;
        }
    }
}