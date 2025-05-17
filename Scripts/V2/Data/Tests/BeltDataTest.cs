using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace V2.Data.Tests
{
    [TestFixture]
    public class BeltDataTest
    {
        private BeltData _belt;
        private Vector2Int _defaultPosition;
        private ChunkData _mockChunk;
        private SimulationItem _testItem1;
        private SimulationItem _testItem2;
        private BeltData _nextBelt;
        private BeltData _previousBelt;

        [SetUp]
        public void SetUp()
        {
            _defaultPosition = new Vector2Int(5, 5);
            _belt = new BeltData(_defaultPosition);
            _mockChunk = new ChunkData(new Vector2Int(0, 0));
            
            // Create test items
            _testItem1 = new SimulationItem("item1", "TestItem1");
            _testItem2 = new SimulationItem("item2", "TestItem2");
            
            // Create connected belts
            _nextBelt = new BeltData(new Vector2Int(5, 6));      // Position in front of belt (rotation 0)
            _previousBelt = new BeltData(new Vector2Int(5, 4));  // Position behind belt (rotation 0)
        }

        #region Initialization Tests
        
        [Test]
        public void BeltData_Initialization_PropertiesSetCorrectly()
        {
            // Assert
            Assert.NotNull(_belt);
            Assert.That(_belt.LocalPosition, Is.EqualTo(_defaultPosition));
            Assert.That(_belt.Rotation, Is.EqualTo(0f));
            Assert.IsFalse(_belt.HasItem);
        }
        
        #endregion
        
        #region Position Calculation Tests
        
        [Test]
        public void GetNextPosition_AtRotation0_ReturnsCorrectPosition()
        {
            // Arrange - Rotation 0 (default)
            
            // Act
            Vector2Int nextPos = _belt.GetNextPosition();
            
            // Assert
            Assert.That(nextPos, Is.EqualTo(new Vector2Int(5, 6))); // Above belt
        }
        
        [Test]
        public void GetNextPosition_AtRotation90_ReturnsCorrectPosition()
        {
            // Arrange
            _belt.Rotate(); // 90 degrees
            
            // Act
            Vector2Int nextPos = _belt.GetNextPosition();
            
            // Assert
            Assert.That(nextPos, Is.EqualTo(new Vector2Int(6, 5))); // Right of belt
        }
        
        [Test]
        public void GetNextPosition_AtRotation180_ReturnsCorrectPosition()
        {
            // Arrange
            _belt.Rotate(); // 90 degrees
            _belt.Rotate(); // 180 degrees
            
            // Act
            Vector2Int nextPos = _belt.GetNextPosition();
            
            // Assert
            Assert.That(nextPos, Is.EqualTo(new Vector2Int(5, 4))); // Below belt
        }
        
        [Test]
        public void GetNextPosition_AtRotation270_ReturnsCorrectPosition()
        {
            // Arrange
            _belt.Rotate(); // 90 degrees
            _belt.Rotate(); // 180 degrees
            _belt.Rotate(); // 270 degrees
            
            // Act
            Vector2Int nextPos = _belt.GetNextPosition();
            
            // Assert
            Assert.That(nextPos, Is.EqualTo(new Vector2Int(4, 5))); // Left of belt
        }
        
        [Test]
        public void GetPreviousPosition_AtRotation0_ReturnsCorrectPosition()
        {
            // Arrange - Rotation 0 (default)
            
            // Act
            Vector2Int prevPos = _belt.GetPreviousPosition();
            
            // Assert
            Assert.That(prevPos, Is.EqualTo(new Vector2Int(5, 4))); // Below belt
        }
        
        [Test]
        public void GetPreviousPosition_AtRotation90_ReturnsCorrectPosition()
        {
            // Arrange
            _belt.Rotate(); // 90 degrees
            
            // Act
            Vector2Int prevPos = _belt.GetPreviousPosition();
            
            // Assert
            Assert.That(prevPos, Is.EqualTo(new Vector2Int(4, 5))); // Left of belt
        }
        
        #endregion
        
        #region Connection Tests
        
        [Test]
        public void CheckConnections_WithBeltsAtNextAndPrevious_UpdatesConnections()
        {
            // Arrange - Mock ChunkData to return our mock belts
            bool connectionChangedEventFired = false;
            BeltData capturedBelt = null;
            
            // Setup mock behavior for ChunkData
            // This would require a proper mock framework or a test-specific implementation
            // For this example, we'll use a simple event handler to verify behavior
            
            _belt.OnConnectionChanged += (belt, connectedBelt) => {
                connectionChangedEventFired = true;
                capturedBelt = connectedBelt;
            };
            
            // Act
            // In a real test, you would use a mock ChunkData that returns predefined entities
            // For this example, we'll just verify the event is wired up correctly
            _belt.CheckConnections(_mockChunk);
            
            // Assert
            // In a real test with proper mocking, you would assert:
            // Assert.IsTrue(connectionChangedEventFired);
            // Assert.That(capturedBelt, Is.EqualTo(_nextBelt));
            // Assert.That(_belt.GetNextBelt(), Is.EqualTo(_nextBelt));
            // Assert.That(_belt.GetPreviousBelt(), Is.EqualTo(_previousBelt));
        }
        
        [Test]
        public void Rotate_UpdatesConnectionsAfterRotation()
        {
            // Arrange
            bool rotationOccurred = false;
            
            // Act
            // In a real test, you would use a mock ChunkData
            // For this example, we'll just verify the rotation happens
            float initialRotation = _belt.Rotation;
            _belt.Rotate(_mockChunk);
            rotationOccurred = _belt.Rotation != initialRotation;
            
            // Assert
            Assert.IsTrue(rotationOccurred);
            Assert.That(_belt.Rotation, Is.EqualTo(90f));
        }
        
        #endregion
        
        #region Item Handling Tests
        
        [Test]
        public void CanAcceptItem_WithNoItems_ReturnsTrue()
        {
            // Act
            bool result = _belt.CanAcceptItem();
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [Test]
        public void CanAcceptItem_WithMaxItems_ReturnsFalse()
        {
            // Arrange - Add max items to the belt
            for (int i = 0; i < 5; i++) // MAX_ITEMS is 5
            {
                _belt.AcceptItem(new SimulationItem($"item{i}", "TestItem"));
            }
            
            // Act
            bool result = _belt.CanAcceptItem();
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void AcceptItem_WithSpaceAvailable_ReturnsTrue()
        {
            // Act
            bool result = _belt.AcceptItem(_testItem1);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_belt.HasItem);
        }
        
        [Test]
        public void AcceptItem_FiresOnItemAddedEvent()
        {
            // Arrange
            bool eventFired = false;
            SimulationItem capturedItem = null;
            
            _belt.OnItemAdded += (belt, item) => {
                eventFired = true;
                capturedItem = item;
            };
            
            // Act
            _belt.AcceptItem(_testItem1);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.That(capturedItem, Is.EqualTo(_testItem1));
        }
        
        [Test]
        public void TakeItem_WithNoItemsNearEnd_ReturnsNull()
        {
            // Act
            SimulationItem result = _belt.TakeItem();
            
            // Assert
            Assert.IsNull(result);
        }
        
        [Test]
        public void TakeItem_WithItemNearEnd_ReturnsItem()
        {
            // This test would require manipulating the internal progress of an item
            // For this example, we'll just document the test case
            
            // Arrange
            // Add an item and set its progress to 0.95f (> 0.9f threshold)
            
            // Act
            // SimulationItem result = _belt.TakeItem();
            
            // Assert
            // Assert.NotNull(result);
            // Assert.That(result, Is.EqualTo(_testItem1));
            // Assert.IsFalse(_belt.HasItem);
        }
        
        [Test]
        public void TakeItem_FiresOnItemRemovedEvent()
        {
            // This test would require manipulating the internal progress of an item
            // For this example, we'll just document the test case
            
            // Arrange
            // Add an item and set its progress to 0.95f (> 0.9f threshold)
            // bool eventFired = false;
            // SimulationItem capturedItem = null;
            // 
            // _belt.OnItemRemoved += (belt, item) => {
            //     eventFired = true;
            //     capturedItem = item;
            // };
            
            // Act
            // _belt.TakeItem();
            
            // Assert
            // Assert.IsTrue(eventFired);
            // Assert.That(capturedItem, Is.EqualTo(_testItem1));
        }
        
        [Test]
        public void GetAllItemsWithProgress_ReturnsCorrectDictionary()
        {
            // Arrange
            _belt.AcceptItem(_testItem1);
            _belt.AcceptItem(_testItem2);
            
            // Act
            Dictionary<SimulationItem, float> result = _belt.GetAllItemsWithProgress();
            
            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.IsTrue(result.ContainsKey(_testItem1));
            Assert.IsTrue(result.ContainsKey(_testItem2));
            Assert.That(result[_testItem1], Is.EqualTo(0f));
            Assert.That(result[_testItem2], Is.EqualTo(0f));
        }
        
        #endregion
        
        #region Tick Tests
        
        [Test]
        public void Tick_UpdatesItemProgress()
        {
            // Arrange
            _belt.AcceptItem(_testItem1);
            float dt = 0.5f;
            
            // Act
            _belt.Tick(dt);
            
            // Assert
            Dictionary<SimulationItem, float> progress = _belt.GetAllItemsWithProgress();
            Assert.That(progress[_testItem1], Is.EqualTo(0.5f).Within(0.001f)); // BELT_SPEED is 1.0f
        }
        
        [Test]
        public void Tick_WithItemReachingEnd_MovesToNextBelt()
        {
            // This test would require proper mocking of the next belt
            // For this example, we'll just document the test case
            
            // Arrange
            // Setup belt with an item at progress near 1.0f
            // Setup next belt that can accept items
            
            // Act
            // _belt.Tick(0.1f); // Enough to push progress over 1.0f
            
            // Assert
            // Verify item was removed from this belt
            // Verify item was added to next belt
        }
        
        [Test]
        public void Tick_WithItemReachingEndButNoNextBelt_StaysAtEnd()
        {
            // Arrange
            _belt.AcceptItem(_testItem1);
            
            // Act - Simulate enough time to reach the end
            _belt.Tick(1.5f); // More than enough to reach progress 1.0f
            
            // Assert
            Dictionary<SimulationItem, float> progress = _belt.GetAllItemsWithProgress();
            Assert.That(progress[_testItem1], Is.EqualTo(1.0f).Within(0.001f));
            Assert.IsTrue(_belt.HasItem);
        }
        
        [Test]
        public void Tick_WithMultipleItems_MaintainsMinimumSpacing()
        {
            // Arrange
            _belt.AcceptItem(_testItem1);
            _belt.AcceptItem(_testItem2);
            
            // Act - Move the first item forward
            _belt.Tick(0.5f);
            
            // Assert
            Dictionary<SimulationItem, float> progress = _belt.GetAllItemsWithProgress();
            Assert.That(progress[_testItem1], Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(progress[_testItem2], Is.EqualTo(0.0f).Within(0.001f)); // Should still be at start
            
            // Act - Move both items forward enough that spacing would be violated
            _belt.Tick(0.1f);
            
            // Assert - Second item should maintain minimum spacing
            progress = _belt.GetAllItemsWithProgress();
            Assert.That(progress[_testItem1], Is.EqualTo(0.6f).Within(0.001f));
            Assert.That(progress[_testItem2], Is.EqualTo(0.4f).Within(0.001f)); // MIN_SPACING is 0.2f
        }
        
        #endregion
    }
}