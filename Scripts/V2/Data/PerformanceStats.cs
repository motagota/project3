using System;
using System.Collections.Generic;
using UnityEngine;

namespace V2.Data
{
    [Serializable]
    public class ComponentStats
    {
        public float MinTime = float.MaxValue;
        public float MaxTime = float.MinValue;
        public float TotalTime = 0f;
        public int SampleCount = 0;
        public float AverageTime => SampleCount > 0 ? TotalTime / SampleCount : 0f;
        
        public Queue<float> _recentTimes = new Queue<float>();
        private const int MaxSamples = 60; // Keep last second of samples (assuming 60 ticks per second)
        
        public void RecordTime(float time)
        {
            MinTime = Mathf.Min(MinTime, time);
            MaxTime = Mathf.Max(MaxTime, time);
            TotalTime += time;
            SampleCount++;
            
            // Track recent times for per-second average
            _recentTimes.Enqueue(time);
            if (_recentTimes.Count > MaxSamples)
            {
                _recentTimes.Dequeue();
            }
        }
        
        public float GetRecentAverage()
        {
            if (_recentTimes.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (var time in _recentTimes)
            {
                sum += time;
            }
            return sum / _recentTimes.Count;
        }
        
        public void Reset()
        {
            MinTime = float.MaxValue;
            MaxTime = float.MinValue;
            TotalTime = 0f;
            SampleCount = 0;
            _recentTimes.Clear();
        }
    }
    
    public class PerformanceStats
    {
        public ComponentStats MachineStats { get; private set; } = new ComponentStats();
        public ComponentStats ConnectorStats { get; private set; } = new ComponentStats();
        public ComponentStats BeltStats { get; private set; } = new ComponentStats();
        public ComponentStats TotalStats { get; private set; } = new ComponentStats();
        
        public void Reset()
        {
            MachineStats.Reset();
            ConnectorStats.Reset();
            BeltStats.Reset();
            TotalStats.Reset();
        }
    }
}