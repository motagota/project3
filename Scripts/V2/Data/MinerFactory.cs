using UnityEngine;

namespace V2.Data
{
    public static class MinerFactory
    {
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
        
        public static Miner CreateIronMiner(Vector2Int position)
        {
            return CreateMiner(position, "IronOre");
        }
        
        public static Miner CreateCopperMiner(Vector2Int position)
        {
            return CreateMiner(position, "CopperOre");
        }
        
        public static Miner CreateGoldMiner(Vector2Int position)
        {
            return CreateMiner(position, "GoldOre");
        }
        
        public static Miner CreateCoalMiner(Vector2Int position)
        {
            return CreateMiner(position, "CoalOre");
        }
        
        public static Miner CreateStoneMiner(Vector2Int position)
        {
            return CreateMiner(position, "StoneOre");
        }
    }
}