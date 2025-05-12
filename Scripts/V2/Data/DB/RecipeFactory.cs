using System.Collections.Generic;

namespace V2.Data
{
    /// <summary>
    /// Factory class for creating common recipe types with predefined parameters.
    /// This complements the RecipeDatabase by providing a programmatic way to create recipes.
    /// </summary>
    public static class RecipeFactory
    {
        /// <summary>
        /// Creates a basic processing recipe that converts one input item into one output item.
        /// </summary>
        /// <param name="inputType">The type of input item required</param>
        /// <param name="outputType">The type of output item produced</param>
        /// <param name="duration">The time in seconds to complete the recipe</param>
        /// <param name="inputCount">The number of input items required (default: 1)</param>
        /// <returns>A new Recipe instance</returns>
        public static Recipe CreateBasicProcessingRecipe(string inputType, string outputType, float duration, int inputCount = 1)
        {
            return new Recipe(
                duration: duration,
                outputItemType: outputType,
                inputItemTypes: new List<string> { inputType },
                inputItemCount: inputCount
            );
        }

        /// <summary>
        /// Creates a multi-input recipe that requires multiple different input types to produce one output.
        /// </summary>
        /// <param name="inputTypes">List of input item types required</param>
        /// <param name="outputType">The type of output item produced</param>
        /// <param name="duration">The time in seconds to complete the recipe</param>
        /// <param name="inputCount">The number of each input item required (default: 1)</param>
        /// <returns>A new Recipe instance</returns>
        public static Recipe CreateMultiInputRecipe(List<string> inputTypes, string outputType, float duration, int inputCount = 1)
        {
            return new Recipe(
                duration: duration,
                outputItemType: outputType,
                inputItemTypes: inputTypes,
                inputItemCount: inputCount
            );
        }

        /// <summary>
        /// Creates a generator recipe that produces an output without requiring any inputs.
        /// Useful for resource generators or starting machines.
        /// </summary>
        /// <param name="outputType">The type of output item produced</param>
        /// <param name="duration">The time in seconds to complete the recipe</param>
        /// <returns>A new Recipe instance</returns>
        public static Recipe CreateGeneratorRecipe(string outputType, float duration)
        {
            return new Recipe(
                duration: duration,
                outputItemType: outputType,
                inputItemTypes: new List<string>(),
                inputItemCount: 0
            );
        }

        /// <summary>
        /// Creates a recycling recipe that breaks down an input into multiple outputs.
        /// Note: The current Recipe class doesn't support multiple outputs directly,
        /// so this is a placeholder for future expansion.
        /// </summary>
        /// <param name="inputType">The type of input item to recycle</param>
        /// <param name="outputType">The primary output type (for now)</param>
        /// <param name="duration">The time in seconds to complete the recipe</param>
        /// <returns>A new Recipe instance</returns>
        public static Recipe CreateRecyclingRecipe(string inputType, string outputType, float duration)
        {
            // Currently limited to one output type due to Recipe class limitations
            // This could be expanded in the future
            return new Recipe(
                duration: duration,
                outputItemType: outputType,
                inputItemTypes: new List<string> { inputType },
                inputItemCount: 1
            );
        }
    }
}