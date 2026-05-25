using NUnit.Framework;
using UnityEngine;
using Tartaria.Gameplay;
using Tartaria.Save;

namespace Tartaria.Tests.EditMode
{
    /// <summary>
    /// Unit tests for InventorySystem - AddItem, RemoveItem, capacity, persistence.
    /// Tests the 10-slot inventory system for items and consumables.
    /// </summary>
    [TestFixture]
    public class InventorySystemTests
    {
        GameObject _inventoryGO;
        InventorySystem _inventory;

        [SetUp]
        public void Setup()
        {
            // Create inventory instance
            _inventoryGO = new GameObject("TestInventory");
            _inventory = _inventoryGO.AddComponent<InventorySystem>();
            
            // Override singleton for testing
            var instanceField = typeof(InventorySystem).GetField("Instance", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            instanceField?.SetValue(null, _inventory);
        }

        [TearDown]
        public void Teardown()
        {
            if (_inventoryGO != null)
                Object.DestroyImmediate(_inventoryGO);
            
            // Clear singleton
            var instanceField = typeof(InventorySystem).GetField("Instance", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            instanceField?.SetValue(null, null);
        }

        // ═══════════════════════════════════════════════════════════════════
        // AddItem Tests
        // ═══════════════════════════════════════════════════════════════════

        [Test]
        public void AddItem_NewItem_AddsSuccessfully()
        {
            bool result = _inventory.AddItem("aether_shard", 5);
            
            Assert.IsTrue(result, "AddItem should succeed for new item");
            Assert.AreEqual(5, _inventory.GetItemCount("aether_shard"), "Item count should be 5");
        }

        [Test]
        public void AddItem_ExistingItem_StacksQuantity()
        {
            _inventory.AddItem("resonance_crystal", 3);
            _inventory.AddItem("resonance_crystal", 2);
            
            Assert.AreEqual(5, _inventory.GetItemCount("resonance_crystal"), 
                "Item should stack to 5");
        }

        [Test]
        public void AddItem_FullInventory_ReturnsFalse()
        {
            // Fill 10 unique slots
            for (int i = 0; i < 10; i++)
            {
                _inventory.AddItem($"item_{i}", 1);
            }
            
            // Attempt 11th unique item
            bool result = _inventory.AddItem("overflow_item", 1);
            
            Assert.IsFalse(result, "AddItem should fail when inventory full (10 unique items)");
            Assert.AreEqual(0, _inventory.GetItemCount("overflow_item"), 
                "Overflow item should not be added");
        }

        [Test]
        public void AddItem_NullItemId_ReturnsFalse()
        {
            bool result = _inventory.AddItem(null, 1);
            Assert.IsFalse(result, "AddItem should reject null itemId");
        }

        [Test]
        public void AddItem_ZeroCount_ReturnsFalse()
        {
            bool result = _inventory.AddItem("test_item", 0);
            Assert.IsFalse(result, "AddItem should reject count <= 0");
        }

        [Test]
        public void AddItem_NegativeCount_ReturnsFalse()
        {
            bool result = _inventory.AddItem("test_item", -5);
            Assert.IsFalse(result, "AddItem should reject negative count");
        }

        // ═══════════════════════════════════════════════════════════════════
        // RemoveItem Tests
        // ═══════════════════════════════════════════════════════════════════

        [Test]
        public void RemoveItem_ExistingItem_RemovesSuccessfully()
        {
            _inventory.AddItem("shovel", 3);
            bool result = _inventory.RemoveItem("shovel", 2);
            
            Assert.IsTrue(result, "RemoveItem should succeed");
            Assert.AreEqual(1, _inventory.GetItemCount("shovel"), "Should have 1 remaining");
        }

        [Test]
        public void RemoveItem_AllQuantity_DeletesItem()
        {
            _inventory.AddItem("tuning_fork", 5);
            _inventory.RemoveItem("tuning_fork", 5);
            
            Assert.AreEqual(0, _inventory.GetItemCount("tuning_fork"), 
                "Item should be removed from inventory");
            Assert.IsFalse(_inventory.HasItem("tuning_fork"), "HasItem should return false");
        }

        [Test]
        public void RemoveItem_InsufficientQuantity_ReturnsFalse()
        {
            _inventory.AddItem("repair_kit", 2);
            bool result = _inventory.RemoveItem("repair_kit", 5);
            
            Assert.IsFalse(result, "RemoveItem should fail when quantity insufficient");
            Assert.AreEqual(2, _inventory.GetItemCount("repair_kit"), 
                "Quantity should remain unchanged");
        }

        [Test]
        public void RemoveItem_NonexistentItem_ReturnsFalse()
        {
            bool result = _inventory.RemoveItem("phantom_item", 1);
            Assert.IsFalse(result, "RemoveItem should fail for nonexistent item");
        }

        // ═══════════════════════════════════════════════════════════════════
        // Query Tests
        // ═══════════════════════════════════════════════════════════════════

        [Test]
        public void GetItemCount_ExistingItem_ReturnsCorrectCount()
        {
            _inventory.AddItem("aether_potion", 7);
            Assert.AreEqual(7, _inventory.GetItemCount("aether_potion"));
        }

        [Test]
        public void GetItemCount_NonexistentItem_ReturnsZero()
        {
            Assert.AreEqual(0, _inventory.GetItemCount("missing_item"));
        }

        [Test]
        public void HasItem_ExistingItem_ReturnsTrue()
        {
            _inventory.AddItem("echo_lens", 1);
            Assert.IsTrue(_inventory.HasItem("echo_lens"));
        }

        [Test]
        public void HasItem_NonexistentItem_ReturnsFalse()
        {
            Assert.IsFalse(_inventory.HasItem("missing_item"));
        }

        [Test]
        public void HasItem_WithMinCount_ValidatesQuantity()
        {
            _inventory.AddItem("resonance_amplifier", 3);
            
            Assert.IsTrue(_inventory.HasItem("resonance_amplifier", 3), 
                "Should have exactly 3");
            Assert.IsFalse(_inventory.HasItem("resonance_amplifier", 5), 
                "Should not have 5");
        }

        // ═══════════════════════════════════════════════════════════════════
        // Event Tests
        // ═══════════════════════════════════════════════════════════════════

        [Test]
        public void AddItem_FiresOnItemAddedEvent()
        {
            string capturedItemId = null;
            int capturedCount = 0;

            _inventory.OnItemAdded += (itemId, count) =>
            {
                capturedItemId = itemId;
                capturedCount = count;
            };

            _inventory.AddItem("test_item", 5);

            Assert.AreEqual("test_item", capturedItemId, "Event should fire with correct itemId");
            Assert.AreEqual(5, capturedCount, "Event should fire with correct count");
        }

        [Test]
        public void RemoveItem_FiresOnItemRemovedEvent()
        {
            _inventory.AddItem("test_item", 10);

            string capturedItemId = null;
            int capturedRemaining = 0;

            _inventory.OnItemRemoved += (itemId, remaining) =>
            {
                capturedItemId = itemId;
                capturedRemaining = remaining;
            };

            _inventory.RemoveItem("test_item", 3);

            Assert.AreEqual("test_item", capturedItemId);
            Assert.AreEqual(7, capturedRemaining, "Should have 7 remaining after removing 3");
        }

        [Test]
        public void AddItem_FiresOnInventoryChangedEvent()
        {
            bool eventFired = false;
            _inventory.OnInventoryChanged += () => eventFired = true;

            _inventory.AddItem("test_item", 1);

            Assert.IsTrue(eventFired, "OnInventoryChanged should fire on AddItem");
        }

        // ═══════════════════════════════════════════════════════════════════
        // Clear Tests
        // ═══════════════════════════════════════════════════════════════════

        [Test]
        public void Clear_RemovesAllItems()
        {
            _inventory.AddItem("item1", 5);
            _inventory.AddItem("item2", 3);
            _inventory.AddItem("item3", 7);

            _inventory.Clear();

            Assert.AreEqual(0, _inventory.GetAllItems().Count, "Inventory should be empty");
            Assert.AreEqual(0, _inventory.GetItemCount("item1"));
            Assert.AreEqual(0, _inventory.GetItemCount("item2"));
            Assert.AreEqual(0, _inventory.GetItemCount("item3"));
        }

        // ═══════════════════════════════════════════════════════════════════
        // GetAllItems Tests
        // ═══════════════════════════════════════════════════════════════════

        [Test]
        public void GetAllItems_ReturnsAllItems()
        {
            _inventory.AddItem("item_a", 2);
            _inventory.AddItem("item_b", 5);
            _inventory.AddItem("item_c", 1);

            var allItems = _inventory.GetAllItems();

            Assert.AreEqual(3, allItems.Count, "Should have 3 unique items");
            Assert.IsTrue(allItems.ContainsKey("item_a"));
            Assert.IsTrue(allItems.ContainsKey("item_b"));
            Assert.IsTrue(allItems.ContainsKey("item_c"));
        }

        [Test]
        public void GetAllItems_EmptyInventory_ReturnsEmptyDictionary()
        {
            var allItems = _inventory.GetAllItems();
            Assert.AreEqual(0, allItems.Count, "Empty inventory should return empty dictionary");
        }
    }
}
