using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Crafting System — 7-tier recipe-based crafting for tools, consumables, and upgrades.
    ///
    /// Design per GDD §19 (Economy), §06 (Combat Progression):
    ///   - Recipes gated by MaterialTier (Common through Mythic)
    ///   - Workshop bench interaction to craft
    ///   - Requires specific currency amounts per recipe
    ///   - Crafted items feed into combat, building restoration, and exploration
    ///   - Auto-discovers recipes when player reaches a new Moon tier
    ///
    /// Performance budget: minimal (event-driven, no per-frame cost).
    /// </summary>
    [DisallowMultipleComponent]
    public class CraftingSystem : MonoBehaviour
    {
        public static CraftingSystem Instance { get; private set; }

        // ─── Events ───
        public event Action<string> OnRecipeDiscovered;       // recipeId
        public event Action<string> OnItemCrafted;            // recipeId
        public event Action<string, string> OnCraftFailed;    // recipeId, reason
        public event Action<string> OnItemUsed;               // itemId
        public event Action<string, int> OnItemCollected;     // itemId, amount

        readonly Dictionary<string, CraftingRecipe> _recipes = new();
        readonly HashSet<string> _discoveredRecipes = new();
        readonly Dictionary<string, int> _inventory = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            RegisterDefaultRecipes();
        }

        // ─── Recipe Management ───

        public void RegisterRecipe(CraftingRecipe recipe)
        {
            if (recipe == null || string.IsNullOrEmpty(recipe.recipeId)) return;
            _recipes[recipe.recipeId] = recipe;
        }

        public void DiscoverRecipe(string recipeId)
        {
            if (_discoveredRecipes.Contains(recipeId)) return;
            if (!_recipes.ContainsKey(recipeId)) return;

            _discoveredRecipes.Add(recipeId);
            OnRecipeDiscovered?.Invoke(recipeId);
            Debug.Log($"[Crafting] Recipe discovered: {recipeId}");
        }

        public void DiscoverRecipesForTier(MaterialTier tier)
        {
            foreach (var kvp in _recipes)
            {
                if (kvp.Value.requiredTier <= tier)
                    DiscoverRecipe(kvp.Key);
            }
        }

        public bool IsRecipeDiscovered(string recipeId) => _discoveredRecipes.Contains(recipeId);

        public CraftingRecipe GetRecipe(string recipeId)
        {
            return _recipes.TryGetValue(recipeId, out var r) ? r : null;
        }

        public List<CraftingRecipe> GetDiscoveredRecipes()
        {
            var list = new List<CraftingRecipe>();
            foreach (var id in _discoveredRecipes)
            {
                if (_recipes.TryGetValue(id, out var r))
                    list.Add(r);
            }
            return list;
        }

        // ─── Crafting ───

        public bool CanCraft(string recipeId)
        {
            if (!_discoveredRecipes.Contains(recipeId)) return false;
            if (!_recipes.TryGetValue(recipeId, out var recipe)) return false;

            var economy = EconomySystem.Instance;
            if (economy == null) return false;

            foreach (var cost in recipe.costs)
            {
                if (!economy.CanAfford(cost.currency, cost.amount))
                    return false;
            }
            return true;
        }

        public bool Craft(string recipeId)
        {
            if (!CanCraft(recipeId))
            {
                OnCraftFailed?.Invoke(recipeId, "Insufficient resources");
                return false;
            }

            var recipe = _recipes[recipeId];
            var economy = EconomySystem.Instance;

            // Spend resources
            foreach (var cost in recipe.costs)
            {
                if (!economy.SpendCurrency(cost.currency, cost.amount))
                {
                    OnCraftFailed?.Invoke(recipeId, "Currency spend failed");
                    return false;
                }
            }

            // Add to inventory
            if (!_inventory.ContainsKey(recipe.outputItemId))
                _inventory[recipe.outputItemId] = 0;
            _inventory[recipe.outputItemId] += recipe.outputCount;

            OnItemCrafted?.Invoke(recipeId);
            Debug.Log($"[Crafting] Crafted: {recipe.outputItemId} x{recipe.outputCount}");
            return true;
        }

        // ─── Inventory ───

        public int GetItemCount(string itemId)
        {
            return _inventory.TryGetValue(itemId, out int count) ? count : 0;
        }

        public bool ConsumeItem(string itemId, int amount = 1)
        {
            if (!_inventory.TryGetValue(itemId, out int count) || count < amount)
                return false;
            _inventory[itemId] -= amount;
            if (_inventory[itemId] <= 0)
                _inventory.Remove(itemId);
            return true;
        }

        public void AddItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return;
            if (!_inventory.ContainsKey(itemId))
                _inventory[itemId] = 0;
            _inventory[itemId] += amount;
            OnItemCollected?.Invoke(itemId, amount);
        }

        /// <summary>
        /// Use a consumable item, applying its gameplay effect and consuming 1 from inventory.
        /// Returns true if the item was used successfully.
        /// </summary>
        public bool UseItem(string itemId)
        {
            if (GetItemCount(itemId) <= 0) return false;

            bool applied = false;
            switch (itemId)
            {
                case "repair_kit":
                    // Restore 30 HP to nearest damaged building via CorruptionSystem
                    var corruption = Integration.CorruptionSystem.Instance;
                    if (corruption != null)
                    {
                        corruption.PurgeCorruption("nearest", 30f);
                        applied = true;
                    }
                    break;

                case "aether_potion":
                    // Refill 50 Aether charge to player
                    if (AetherFieldManager.Instance != null)
                    {
                        AetherFieldManager.Instance.AddFieldEnergy(50f);
                        applied = true;
                    }
                    break;

                case "resonance_amplifier":
                    // Boost RS gain by 25% for 60 seconds
                    var loop = Integration.GameLoopController.Instance;
                    if (loop != null)
                    {
                        loop.ActivateRSBuff();
                        applied = true;
                    }
                    break;

                case "echo_lens":
                    // Reveal hidden excavation sites on the current moon
                    var excavation = ExcavationSystem.Instance;
                    if (excavation != null)
                    {
                        excavation.RevealHiddenSites();
                        applied = true;
                    }
                    break;

                default:
                    Debug.LogWarning($"[Crafting] Unknown consumable: {itemId}");
                    return false;
            }

            if (applied)
            {
                ConsumeItem(itemId);
                OnItemUsed?.Invoke(itemId);
                Debug.Log($"[Crafting] Used item: {itemId}");
            }
            return applied;
        }

        // ─── Default Recipes ───

        void RegisterDefaultRecipes()
        {
            // Common tier (Moon 1)
            RegisterRecipe(new CraftingRecipe
            {
                recipeId = "repair_kit",
                displayName = "Repair Kit",
                requiredTier = MaterialTier.Common,
                outputItemId = "repair_kit",
                outputCount = 1,
                costs = new[] { new CraftingCost(CurrencyType.AetherShards, 30) }
            });
            RegisterRecipe(new CraftingRecipe
            {
                recipeId = "aether_potion",
                displayName = "Aether Potion",
                requiredTier = MaterialTier.Common,
                outputItemId = "aether_potion",
                outputCount = 1,
                costs = new[] { new CraftingCost(CurrencyType.AetherShards, 50) }
            });

            // Uncommon tier (Moon 2-3)
            RegisterRecipe(new CraftingRecipe
            {
                recipeId = "resonance_amplifier",
                displayName = "Resonance Amplifier",
                requiredTier = MaterialTier.Uncommon,
                outputItemId = "resonance_amplifier",
                outputCount = 1,
                costs = new[]
                {
                    new CraftingCost(CurrencyType.AetherShards, 100),
                    new CraftingCost(CurrencyType.HarmonicFragments, 5)
                }
            });

            // Rare tier (Moon 4-6)
            RegisterRecipe(new CraftingRecipe
            {
                recipeId = "echo_lens",
                displayName = "Echo Lens",
                requiredTier = MaterialTier.Rare,
                outputItemId = "echo_lens",
                outputCount = 1,
                costs = new[]
                {
                    new CraftingCost(CurrencyType.ResonanceCrystals, 10),
                    new CraftingCost(CurrencyType.EchoMemories, 3)
                }
            });

            // Epic tier (Moon 7-9)
            RegisterRecipe(new CraftingRecipe
            {
                recipeId = "harmonic_blade",
                displayName = "Harmonic Blade",
                requiredTier = MaterialTier.Epic,
                outputItemId = "harmonic_blade",
                outputCount = 1,
                costs = new[]
                {
                    new CraftingCost(CurrencyType.ResonanceCrystals, 25),
                    new CraftingCost(CurrencyType.CrystallineDust, 10),
                    new CraftingCost(CurrencyType.ForgeTokens, 5)
                }
            });

            // Legendary tier (Moon 10-12)
            RegisterRecipe(new CraftingRecipe
            {
                recipeId = "void_anchor",
                displayName = "Void Anchor",
                requiredTier = MaterialTier.Legendary,
                outputItemId = "void_anchor",
                outputCount = 1,
                costs = new[]
                {
                    new CraftingCost(CurrencyType.StarFragments, 5),
                    new CraftingCost(CurrencyType.CrystallineDust, 20),
                    new CraftingCost(CurrencyType.ForgeTokens, 15)
                }
            });

            // Ascendant tier (Moon 13)
            RegisterRecipe(new CraftingRecipe
            {
                recipeId = "truth_resonator",
                displayName = "Truth Resonator",
                requiredTier = MaterialTier.Ascendant,
                outputItemId = "truth_resonator",
                outputCount = 1,
                costs = new[]
                {
                    new CraftingCost(CurrencyType.StarFragments, 15),
                    new CraftingCost(CurrencyType.HarmonicFragments, 50),
                    new CraftingCost(CurrencyType.ForgeTokens, 25)
                }
            });

            // Mythic tier (Day Out of Time)
            RegisterRecipe(new CraftingRecipe
            {
                recipeId = "eternal_frequency_key",
                displayName = "Eternal Frequency Key",
                requiredTier = MaterialTier.Mythic,
                outputItemId = "eternal_frequency_key",
                outputCount = 1,
                costs = new[]
                {
                    new CraftingCost(CurrencyType.StarFragments, 50),
                    new CraftingCost(CurrencyType.EchoMemories, 30),
                    new CraftingCost(CurrencyType.CrystallineDust, 40),
                    new CraftingCost(CurrencyType.ForgeTokens, 30)
                }
            });
        }

        // ─── Save / Load ───

        public CraftingSaveData GetSaveData()
        {
            var discoveredList = new List<string>(_discoveredRecipes);
            var inventoryList = new List<CraftingInventoryEntry>();
            foreach (var kvp in _inventory)
                inventoryList.Add(new CraftingInventoryEntry { itemId = kvp.Key, count = kvp.Value });

            return new CraftingSaveData
            {
                discoveredRecipes = discoveredList,
                inventory = inventoryList
            };
        }

        public void LoadSaveData(CraftingSaveData data)
        {
            _discoveredRecipes.Clear();
            _inventory.Clear();
            if (data == null) return;

            if (data.discoveredRecipes != null)
                foreach (var id in data.discoveredRecipes)
                    _discoveredRecipes.Add(id);

            if (data.inventory != null)
                foreach (var entry in data.inventory)
                    _inventory[entry.itemId] = entry.count;
        }
    }

    // ─── Data Types ──────────────────────────────

    [Serializable]
    public class CraftingRecipe
    {
        public string recipeId;
        public string displayName;
        public MaterialTier requiredTier;
        public string outputItemId;
        public int outputCount;
        public CraftingCost[] costs;
    }

    [Serializable]
    public struct CraftingCost
    {
        public CurrencyType currency;
        public int amount;

        public CraftingCost(CurrencyType currency, int amount)
        {
            this.currency = currency;
            this.amount = amount;
        }
    }

    [Serializable]
    public class CraftingSaveData
    {
        public List<string> discoveredRecipes;
        public List<CraftingInventoryEntry> inventory;
    }

    [Serializable]
    public class CraftingInventoryEntry
    {
        public string itemId;
        public int count;
    }
}
