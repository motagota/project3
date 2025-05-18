using UnityEngine;
using System.Collections.Generic;

namespace V2.Data
{
    public class Miner : Machine
    {
        private string _oreType;
        
        public Miner(Vector2Int localPosition, string oreType) : base(localPosition)
        {
            _oreType = oreType;
            
            // Set up a generator recipe that doesn't require inputs
            CurrentRecipe = new Recipe(
                duration: 0.10f,  // Takes 3 seconds to mine one ore
                outputItemType: _oreType,
                inputItemTypes: new List<string>(),  // No inputs required
                inputItemCount: 0
            );
        }
        
        public override bool CanAcceptItem(SimulationItem item)
        {
            // Miners don't accept any items
            return false;
        }
        
        public override bool GiveItem(SimulationItem item)
        {
            // Miners don't accept any items
            return false;
        }
        
        public string GetOreType()
        {
            return _oreType;
        }
    }
}