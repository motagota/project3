using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace V2.Data.Tests
{
    [TestFixture]
    public class MachineTest
    {
        private Machine _machine;
        private Vector2Int _defaultPosition;
        private Recipe _defaultRecipe;
        private Recipe _customRecipe;
        private SimulationItem _validItem;
        private SimulationItem _invalidItem;

        [SetUp]
        public void SetUp()
        {
            _defaultPosition = new Vector2Int(0, 0);
            _machine = new Machine(_defaultPosition);
            
            // Create a custom recipe for testing
            _customRecipe = new Recipe(
                duration: 2.0f,
                outputItemType: "ProcessedItem",
                inputItemTypes: new List<string> { "RawMaterial" },
                inputItemCount: 2
            );
            
            // Create test items
            _validItem = new SimulationItem("item1", "RawMaterial");
            _invalidItem = new SimulationItem("item2", "WrongMaterial");
        }

        #region Initialization Tests
        
        [Test]
        public void MachineIsCreatedWithDefaultRecipe()
        {
            // Assert
            Assert.NotNull(_machine);
            Assert.NotNull(_machine.CurrentRecipe);
            Assert.That(_machine.CurrentRecipe.Duration, Is.EqualTo(1.0f));
            Assert.That(_machine.CurrentRecipe.OutputItemType, Is.EqualTo("Default"));
        }
        
        [Test]
        public void MachineInitializedWithCorrectPosition()
        {
            // Assert
            Assert.That(_machine.LocalPostion, Is.EqualTo(_defaultPosition));
            Assert.That(_machine.Rotation, Is.EqualTo(0f));
        }
        
        [Test]
        public void MachineInitializedWithZeroProgress()
        {
            // Assert
            Assert.That(_machine.Progress, Is.EqualTo(0f));
            Assert.That(_machine.CompletedRecipes, Is.EqualTo(0));
            Assert.That(_machine.HasItem, Is.False);
        }
        
        #endregion
        
        #region Item Acceptance Tests
        
        [Test]
        public void CanAcceptItem_WithValidItemType_ReturnsTrue()
        {
            // Arrange
            _machine.CurrentRecipe = _customRecipe;
            
            // Act
            bool result = _machine.CanAcceptItem(_validItem);
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [Test]
        public void CanAcceptItem_WithInvalidItemType_ReturnsFalse()
        {
            // Arrange
            _machine.CurrentRecipe = _customRecipe;
            
            // Act
            bool result = _machine.CanAcceptItem(_invalidItem);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void GiveItem_WithValidItemType_ReturnsTrue()
        {
            // Arrange
            _machine.CurrentRecipe = _customRecipe;
            
            // Act
            bool result = _machine.GiveItem(_validItem);
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [Test]
        public void GiveItem_WithInvalidItemType_ReturnsFalse()
        {
            // Arrange
            _machine.CurrentRecipe = _customRecipe;
            
            // Act
            bool result = _machine.GiveItem(_invalidItem);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        #endregion
        
        #region Recipe Processing Tests
        
        [Test]
        public void Tick_WithNoInputRequirements_ProducesOutputAfterDuration()
        {
            // Arrange
            Recipe simpleRecipe = new Recipe(0.5f, "SimpleOutput");
            _machine.CurrentRecipe = simpleRecipe;
            
            // Act - Simulate time passing
            _machine.Tick(0.6f); // More than the recipe duration
            
            // Assert
            Assert.IsTrue(_machine.HasItem);
            Assert.That(_machine.CompletedRecipes, Is.EqualTo(1));
        }
        
        [Test]
        public void Tick_WithInsufficientTime_DoesNotCompleteRecipe()
        {
            // Arrange
            Recipe simpleRecipe = new Recipe(1.0f, "SimpleOutput");
            _machine.CurrentRecipe = simpleRecipe;
            
            // Act - Simulate time passing, but not enough
            _machine.Tick(0.5f);
            
            // Assert
            Assert.IsFalse(_machine.HasItem);
            Assert.That(_machine.Progress, Is.EqualTo(0.5f));
            Assert.That(_machine.CompletedRecipes, Is.EqualTo(0));
        }
        
        [Test]
        public void Tick_WithRequiredInputs_ProcessesRecipe()
        {
            // Arrange
            _machine.CurrentRecipe = _customRecipe;
            
            // Add required inputs
            _machine.GiveItem(_validItem);
            _machine.GiveItem(_validItem); // Add two as required by recipe
            
            // Act - First tick to consume items
            _machine.Tick(0.4f); // More than CONSUMPTION_RATE (1/3)
            
            // Second tick to complete recipe
            _machine.Tick(2.0f); // Enough to complete recipe
            
            // Assert
            Assert.IsTrue(_machine.HasItem);
            Assert.That(_machine.CompletedRecipes, Is.EqualTo(1));
        }
        
        [Test]
        public void Tick_WithoutRequiredInputs_DoesNotProcessRecipe()
        {
            // Arrange
            _machine.CurrentRecipe = _customRecipe;
            
            // Act - Simulate time passing without providing inputs
            _machine.Tick(3.0f); // More than enough time if inputs were available
            
            // Assert
            Assert.IsFalse(_machine.HasItem);
            Assert.That(_machine.CompletedRecipes, Is.EqualTo(0));
        }
        
        #endregion
        
        #region Output Handling Tests
        
        [Test]
        public void TakeItem_WithNoOutput_ReturnsNull()
        {
            // Act
            SimulationItem result = _machine.TakeItem();
            
            // Assert
            Assert.IsNull(result);
        }
        
        [Test]
        public void TakeItem_WithOutput_ReturnsAndRemovesItem()
        {
            // Arrange - Create output
            Recipe simpleRecipe = new Recipe(0.5f, "SimpleOutput");
            _machine.CurrentRecipe = simpleRecipe;
            _machine.Tick(0.6f); // Complete recipe
            
            // Act
            SimulationItem result = _machine.TakeItem();
            
            // Assert
            Assert.NotNull(result);
            Assert.That(result.ItemType, Is.EqualTo("SimpleOutput"));
            Assert.IsFalse(_machine.HasItem); // Item should be removed
        }
        
        #endregion
        
        #region Event Tests
        
        [Test]
        public void OnRecipeCompleted_EventIsFired_WhenRecipeCompletes()
        {
            // Arrange
            bool eventFired = false;
            Recipe simpleRecipe = new Recipe(0.5f, "SimpleOutput");
            _machine.CurrentRecipe = simpleRecipe;
            
            _machine.OnRecipeCompleted += (machine) => {
                eventFired = true;
                Assert.That(machine, Is.EqualTo(_machine));
            };
            
            // Act
            _machine.Tick(0.6f); // Complete recipe
            
            // Assert
            Assert.IsTrue(eventFired);
        }
        
        [Test]
        public void OnItemConsumed_EventIsFired_WhenItemIsConsumed()
        {
            // Arrange
            bool eventFired = false;
            SimulationItem consumedItem = null;
            
            _machine.CurrentRecipe = _customRecipe;
            _machine.GiveItem(_validItem);
            
            _machine.OnItemConsumed += (machine, item) => {
                eventFired = true;
                consumedItem = item;
                Assert.That(machine, Is.EqualTo(_machine));
            };
            
            // Act
            _machine.Tick(0.4f); // More than CONSUMPTION_RATE (1/3)
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.NotNull(consumedItem);
            Assert.That(consumedItem.ItemType, Is.EqualTo("RawMaterial"));
        }
        
        #endregion
        
        #region Multiple Recipe Cycles Tests
        
        [Test]
        public void Machine_CanProcessMultipleRecipeCycles()
        {
            // Arrange
            Recipe simpleRecipe = new Recipe(0.5f, "SimpleOutput");
            _machine.CurrentRecipe = simpleRecipe;
            
            // Act - First cycle
            _machine.Tick(0.6f);
            SimulationItem firstItem = _machine.TakeItem();
            
            // Second cycle
            _machine.Tick(0.6f);
            SimulationItem secondItem = _machine.TakeItem();
            
            // Assert
            Assert.NotNull(firstItem);
            Assert.NotNull(secondItem);
            Assert.That(_machine.CompletedRecipes, Is.EqualTo(2));
        }
        
        #endregion
    }
}