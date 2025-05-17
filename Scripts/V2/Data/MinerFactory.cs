using UnityEngine;

namespace V2.Data
{
    /// <summary>
    /// Factory class for creating different types of miners.
    /// </summary>
    public static class MinerFactory
    {
        /// <summary>
        /// Creates a miner for the specified ore type at the given position.
        /// </summary>
        /// <param name="position">The position to place the miner</param>
        /// <param name="oreType">The type of ore to mine (e.g., "IronOre", "CopperOre")</param>
        /// <returns>A new Miner instance configured for the specified ore type</returns>
        public static Miner CreateMiner(Vector2Int position, string oreType)
        {
            // Validate that the ore type exists in the database
            if (!ItemDatabase.Instance.HasItem(oreType))
            {
                Debug.LogWarning($"Ore type '{oreType}' not found in ItemDatabase. Using default IronOre instead.");
                oreType = "IronOre";
            }
            
            return new Miner(position, oreType);
        }
        
        /// <summary>
        /// Creates an iron ore miner at the specified position.
        /// </summary>
        public static Miner CreateIronMiner(Vector2Int position)
        {
            return CreateMiner(position, "IronOre");
        }
        
        /// <summary>
        /// Creates a copper ore miner at the specified position.
        /// </summary>
        public static Miner CreateCopperMiner(Vector2Int position)
        {
            return CreateMiner(position, "CopperOre");
        }
        
        /// <summary>
        /// Creates a gold ore miner at the specified position.
        /// </summary>
        public static Miner CreateGoldMiner(Vector2Int position)
        {
            return CreateMiner(position, "GoldOre");
        }
        
        /// <summary>
        /// Creates a coal ore miner at the specified position.
        /// </summary>
        public static Miner CreateCoalMiner(Vector2Int position)
        {
            return CreateMiner(position, "CoalOre");
        }
        
        /// <summary>
        /// Creates a stone miner at the specified position.
        /// </summary>
        public static Miner CreateStoneMiner(Vector2Int position)
        {
            return CreateMiner(position, "StoneOre");
        }
    }
}