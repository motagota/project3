using UnityEngine;

namespace V2.Data
{
    public class SimulationItem
    {
        public string ItemType { get; private set; }
        public string id;
        public SimulationItem(string id, string itemType)
        {
            this.id = id;
            ItemType = itemType;
        }
        
        public override string ToString()
        {
            return ItemType;
        }
    }
}