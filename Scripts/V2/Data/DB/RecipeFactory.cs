using System.Collections.Generic;

namespace V2.Data
{
    
    public static class RecipeFactory
    {
        public static Recipe CreateBasicProcessingRecipe(string inputType, string outputType, float duration, int inputCount = 1)
        {
            return new Recipe(
                duration: duration,
                outputItemType: outputType,
                inputItemTypes: new List<string> { inputType },
                inputItemCount: inputCount
            );
        }

        public static Recipe CreateMultiInputRecipe(List<string> inputTypes, string outputType, float duration, int inputCount = 1)
        {
            return new Recipe(
                duration: duration,
                outputItemType: outputType,
                inputItemTypes: inputTypes,
                inputItemCount: inputCount
            );
        }

        public static Recipe CreateGeneratorRecipe(string outputType, float duration)
        {
            return new Recipe(
                duration: duration,
                outputItemType: outputType,
                inputItemTypes: new List<string>(),
                inputItemCount: 0
            );
        }

        public static Recipe CreateRecyclingRecipe(string inputType, string outputType, float duration)
        {
            return new Recipe(
                duration: duration,
                outputItemType: outputType,
                inputItemTypes: new List<string> { inputType },
                inputItemCount: 1
            );
        }
    }
}