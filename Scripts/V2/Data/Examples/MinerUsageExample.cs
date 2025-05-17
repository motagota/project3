using UnityEngine;
using System.Collections.Generic;

namespace V2.Data.Examples
{
    /// <summary>
    /// Example demonstrating how to use the Miner class in a simulation.
    /// </summary>
    public class MinerUsageExample : MonoBehaviour
    {
        private List<Miner> _miners = new List<Miner>();
        private float _simulationTimer = 0f;
        private float _outputCheckInterval = 1f;
        
        void Start()
        {
            // Create different types of miners using the factory
            CreateMiners();
        }
        
        void Update()
        {
            // Simulate the miners
            float dt = Time.deltaTime;
            _simulationTimer += dt;
            
            // Tick all miners
            foreach (var miner in _miners)
            {
                miner.Tick(dt);
            }
            
            // Check for output items periodically
            if (_simulationTimer >= _outputCheckInterval)
            {
                _simulationTimer = 0f;
                CollectOutputFromMiners();
            }
        }
        
        private void CreateMiners()
        {
            // Create miners for different ore types at different positions
            _miners.Add(MinerFactory.CreateIronMiner(new Vector2Int(0, 0)));
            _miners.Add(MinerFactory.CreateCopperMiner(new Vector2Int(2, 0)));
            _miners.Add(MinerFactory.CreateGoldMiner(new Vector2Int(4, 0)));
            _miners.Add(MinerFactory.CreateCoalMiner(new Vector2Int(0, 2)));
            _miners.Add(MinerFactory.CreateStoneMiner(new Vector2Int(2, 2)));
            
            Debug.Log($"Created {_miners.Count} miners of different types");
        }
        
        private void CollectOutputFromMiners()
        {
            foreach (var miner in _miners)
            {
                SimulationItem item = miner.TakeItem();
                if (item != null)
                {
                    Debug.Log($"Collected {item.ItemType} from miner at position {miner.LocalPostion}");
                    // In a real implementation, you would add this item to an inventory or transport it
                }
            }
        }
        
        // Example of how to get information about a miner
        public void DisplayMinerInfo(Miner miner)
        {
            string oreType = miner.GetOreType();
            float progress = miner.Progress;
            int completedCycles = miner.CompletedRecipes;
            
            Debug.Log($"Miner Info - Type: {oreType}, Progress: {progress:F2}, Completed Cycles: {completedCycles}");
        }
    }
}