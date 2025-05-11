using System.Collections.Generic;
using UnityEngine;

namespace Simulation
{
    public class SimulationManager : MonoBehaviour
    {
        // Singleton instance
        public static SimulationManager Instance { get; private set; }
    
        // Systems
        private ConveyorSystem conveyorSystem;
        private MinerSystem minerSystem;
        private StorageSystem storageSystem;
    
        // Data collections
        public Dictionary<int, ConveyorBeltData> conveyorBelts = new Dictionary<int, ConveyorBeltData>();
        public Dictionary<int, ConveyorItemData> conveyorItems = new Dictionary<int, ConveyorItemData>();
        public Dictionary<int, MinerData> miners = new Dictionary<int, MinerData>();
        public Dictionary<int, StorageBoxData> storageBoxes = new Dictionary<int, StorageBoxData>();
    
        // ID counter for generating unique IDs
        private int nextId = 0;
    
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        
            // Initialize systems
            conveyorSystem = new ConveyorSystem();
            minerSystem = new MinerSystem();
            storageSystem = new StorageSystem();
        }
    
        private void Update()
        {
            float deltaTime = Time.deltaTime;
        
            // Update all systems
            conveyorSystem.Update(conveyorBelts, conveyorItems, deltaTime);
            minerSystem.Update(miners, conveyorItems,conveyorBelts, deltaTime);
            storageSystem.Update(storageBoxes, conveyorItems, deltaTime);
        }
    
        // Generate a unique ID
        public int GetNextId()
        {
            return nextId++;
        }
    
        // Add a new conveyor belt to the simulation
        public int AddConveyorBelt(Vector3 position, Quaternion rotation)
        {
            int id = GetNextId();
        
            ConveyorBeltData data = new ConveyorBeltData
            {
                id = id,
                position = position,
                rotation = rotation,
                speed = 1.0f,
                direction = 0,
                inputPointPosition = position + (rotation * new Vector3(0, 0.1f, -0.4f)),
                outputPointPosition = position + (rotation * new Vector3(0, 0.1f, 0.4f)),
                itemsOnFarLane = new List<int>(),
                itemsOnCloseLane = new List<int>(),
                connectedConveyors = new List<int>(),
                isActive = true
            };
        
            conveyorBelts[id] = data;
            return id;
        }
    
        // Add a new item to the simulation
        public int AddConveyorItem(Vector3 position, int resourceType, float resourceAmount)
        {
            int id = GetNextId();
        
            ConveyorItemData data = new ConveyorItemData
            {
                id = id,
                position = position,
                rotation = Quaternion.identity,
                resourceType = resourceType,
                resourceAmount = resourceAmount,
                currentConveyorId = -1,
                isOnFarLane = false
            };
        
            conveyorItems[id] = data;
            return id;
        }

        public int AddMiner(Vector3 position, Quaternion rotation)
        {
            int id = GetNextId();

            MinerData data = new MinerData
            {
                id = id,
                position = position,
                rotation = rotation,
                miningRate=1,
                miningTimer=1,
                resourceType=1,
                isActive = true
            };
            miners[id] = data;
            return id;
        }
        // Add a new storage box to the simulation
        public int AddStorageBox(Vector3 position, Quaternion rotation)
        {
            int id = GetNextId();
        
            StorageBoxData data = new StorageBoxData
            {
                id = id,
                position = position,
                rotation = rotation,
                storedResources = new Dictionary<int, float>(),
                maxCapacity = 100.0f,
                inputPointPosition = position + (rotation * new Vector3(0, 0.1f, -0.4f)),
                outputPointPosition = position + (rotation * new Vector3(0, 0.1f, 0.4f)),
                connectedConveyorIds = new List<int>(),
                isActive = true
            };
        
            storageBoxes[id] = data;
            return id;
        }
    
        // Similar methods for other entity types...
    }
}