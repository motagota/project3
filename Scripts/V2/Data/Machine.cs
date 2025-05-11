using UnityEngine;
using System;

namespace V2.Data
{
    public class Machine : Entity
    {
        public Recipe CurrentRecipe;
        public float Progress;
        
        // Add event for recipe completion
        public event Action<Machine> OnRecipeCompleted;
        
        // Track number of completed recipes
        public int CompletedRecipes { get; private set; } = 0;
        
        public Machine(Vector2Int localPosition) : base(localPosition)
        {
            CurrentRecipe = new Recipe(1);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            Progress += dt;
            if (Progress >= CurrentRecipe.Duration)
            {
                Progress = 0;
                CompletedRecipes++;
                
                // Trigger the event when recipe completes
                OnRecipeCompleted?.Invoke(this);
                
                Debug.Log("Machine finished");
            }
        }
    }
}