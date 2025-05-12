using System.Collections.Generic;

namespace V2.Data
{
    public class Recipe
    {
        public float Duration;
        public string OutputItemType;
        public List<string> InputItemTypes; 
        public int InputItemCount; 
        
        public Recipe(float duration, string outputItemType = "Default", List<string> inputItemTypes = null, int inputItemCount = 0)
        {
            Duration = duration;
            OutputItemType = outputItemType;
            InputItemTypes = inputItemTypes ?? new List<string>();
            InputItemCount = inputItemCount;
        }
        
        public bool RequiresItemType(string itemType)
        {
            return InputItemTypes.Contains(itemType);
        }
    }
}