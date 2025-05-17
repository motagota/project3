using UnityEngine;
using System.Collections.Generic;

namespace V2.Data
{
    /// <summary>
    /// A specialized machine that produces ore items at regular intervals without requiring input items.
    /// </summary>
    public class Miner : Machine
    {
        private string _oreType;
        
        /// <summary>
        /// Creates a new Miner machine that produces the specified ore type.
        /// </summary>
        /// <param name="localPosition">The position of the miner on the grid</param>
        /// <param name="oreType">The type of ore this miner will produce</param>
        public Miner(Vector2Int localPosition, string oreType) : base(localPosition)
        {
            _oreType = oreType;
            
            // Set up a generator recipe that doesn't require inputs
            CurrentRecipe = new Recipe(
                duration: 3.0f,  // Takes 3 seconds to mine one ore
                outputItemType: _oreType,
                inputItemTypes: new List<string>(),  // No inputs required
                inputItemCount: 0
            );
        }
        
        /// <summary>
        /// Override the CanAcceptItem method to prevent items from being given to the miner.
        /// </summary>
        public override bool CanAcceptItem(SimulationItem item)
        {
            // Miners don't accept any items
            return false;
        }
        
        /// <summary>
        /// Override the GiveItem method to prevent items from being given to the miner.
        /// </summary>
        public override bool GiveItem(SimulationItem item)
        {
            // Miners don't accept any items
            return false;
        }
        
        /// <summary>
        /// Gets the type of ore this miner produces.
        /// </summary>
        public string GetOreType()
        {
            return _oreType;
        }
    }
}