using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace V2.Data.Tests
{
    [TestFixture]
    public class MinerTest
    {
        private Miner _ironMiner;
        private Miner _goldMiner;
        private Vector2Int _defaultPosition;
        
        [SetUp]
        public void SetUp()
        {
            _defaultPosition = new Vector2Int(0, 0);
            _ironMiner = new Miner(_defaultPosition, "IronOre");
            _goldMiner = new Miner(new Vector2Int(1, 1), "GoldOre");
        }
        
        [Test]
        public void MinerIsCreatedWithCorrectOreType()
        {
            // Assert
            Assert.That(_ironMiner.GetOreType(), Is.EqualTo("IronOre"));
            Assert.That(_goldMiner.GetOreType(), Is.EqualTo("GoldOre"));
        }
        
        [Test]
        public void MinerHasCorrectRecipeWithNoInputs()
        {
            // Assert
            Assert.NotNull(_ironMiner.CurrentRecipe);
            Assert.That(_ironMiner.CurrentRecipe.OutputItemType, Is.EqualTo("IronOre"));
            Assert.That(_ironMiner.CurrentRecipe.InputItemCount, Is.EqualTo(0));
            Assert.That(_ironMiner.CurrentRecipe.InputItemTypes.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void MinerDoesNotAcceptItems()
        {
            // Arrange
            SimulationItem testItem = new SimulationItem("test1", "IronOre");
            
            // Act & Assert
            Assert.False(_ironMiner.CanAcceptItem(testItem));
            Assert.False(_ironMiner.GiveItem(testItem));
        }
        
        [Test]
        public void MinerProducesCorrectOreAfterDuration()
        {
            // Arrange - Get the duration from the recipe
            float duration = _ironMiner.CurrentRecipe.Duration;
            
            // Act - Tick for the full duration
            _ironMiner.Tick(duration);
            
            // Assert - Check that an item was produced
            SimulationItem producedItem = _ironMiner.TakeItem();
            Assert.NotNull(producedItem);
            Assert.That(producedItem.ItemType, Is.EqualTo("IronOre"));
        }
        
        [Test]
        public void MinerFactoryCreatesCorrectMinerTypes()
        {
            // Act
            Miner ironMiner = MinerFactory.CreateIronMiner(new Vector2Int(0, 0));
            Miner copperMiner = MinerFactory.CreateCopperMiner(new Vector2Int(1, 0));
            Miner goldMiner = MinerFactory.CreateGoldMiner(new Vector2Int(2, 0));
            Miner coalMiner = MinerFactory.CreateCoalMiner(new Vector2Int(3, 0));
            Miner stoneMiner = MinerFactory.CreateStoneMiner(new Vector2Int(4, 0));
            
            // Assert
            Assert.That(ironMiner.GetOreType(), Is.EqualTo("IronOre"));
            Assert.That(copperMiner.GetOreType(), Is.EqualTo("CopperOre"));
            Assert.That(goldMiner.GetOreType(), Is.EqualTo("GoldOre"));
            Assert.That(coalMiner.GetOreType(), Is.EqualTo("CoalOre"));
            Assert.That(stoneMiner.GetOreType(), Is.EqualTo("StoneOre"));
        }
        
        [Test]
        public void MinerContinuesProducingAfterItemIsTaken()
        {
            // Arrange
            float duration = _ironMiner.CurrentRecipe.Duration;
            
            // Act - First cycle
            _ironMiner.Tick(duration);
            SimulationItem firstItem = _ironMiner.TakeItem();
            
            // Act - Second cycle
            _ironMiner.Tick(duration);
            SimulationItem secondItem = _ironMiner.TakeItem();
            
            // Assert
            Assert.NotNull(firstItem);
            Assert.NotNull(secondItem);
            Assert.That(firstItem.ItemType, Is.EqualTo("IronOre"));
            Assert.That(secondItem.ItemType, Is.EqualTo("IronOre"));
        }
    }
}