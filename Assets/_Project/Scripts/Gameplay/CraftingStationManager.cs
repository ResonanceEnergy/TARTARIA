using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// CraftingStationManager — manages crafting stations (workbench, forge, alchemy table).
    /// Player interacts with station → opens crafting UI → selects recipe → consumes materials → produces item.
    /// Recipes unlock via quests, discoveries, skill level.
    /// 
    /// Station Types:
    /// - Workbench → basic tools, building materials
    /// - Forge → weapons, armor, metal refinement
    /// - AlchemyTable → potions, buffs, resonance consumables
    /// 
    /// Recipe Format:
    /// - ID: "iron_sword"
    /// - Name: "Iron Sword"
    /// - Ingredients: { "iron_ore": 5, "wood": 2 }
    /// - Output: "iron_sword" × 1
    /// - Unlock: QuestID or SkillLevel
    /// 
    /// Usage:
    /// - Place crafting station in world
    /// - Define recipes in inspector or ScriptableObject
    /// - Player interacts via IInteractable
    /// - Integrates with InventorySystem for materials + outputs
    /// 
    /// GDD refs: §07 (Crafting System), §02 (Aether Economy)
    /// </summary>
    public class CraftingStationManager : MonoBehaviour
    {
        public static CraftingStationManager Instance { get; private set; }

        [Header("Recipe Database")]
        [SerializeField] CraftingRecipe[] allRecipes;

        Dictionary<string, CraftingRecipe> _recipesByID = new();
        HashSet<string> _unlockedRecipes = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Index recipes
            if (allRecipes != null)
            {
                foreach (var recipe in allRecipes)
                {
                    _recipesByID[recipe.recipeID] = recipe;
                }
            }

            Debug.Log($"[CraftingStation] Initialized with {_recipesByID.Count} recipes");
        }

        /// <summary>
        /// Get all recipes for a specific station type.
        /// </summary>
        public CraftingRecipe[] GetRecipesForStation(StationType stationType)
        {
            return _recipesByID.Values
                .Where(r => r.requiredStation == stationType)
                .ToArray();
        }

        /// <summary>
        /// Get unlocked recipes for a station.
        /// </summary>
        public CraftingRecipe[] GetUnlockedRecipesForStation(StationType stationType)
        {
            return _recipesByID.Values
                .Where(r => r.requiredStation == stationType && IsRecipeUnlocked(r.recipeID))
                .ToArray();
        }

        /// <summary>
        /// Check if player has materials for recipe.
        /// </summary>
        public bool CanCraftRecipe(string recipeID)
        {
            if (!_recipesByID.TryGetValue(recipeID, out var recipe))
            {
                return false;
            }

            if (!IsRecipeUnlocked(recipeID))
            {
                return false;
            }

            // Check inventory for each ingredient
            foreach (var ingredient in recipe.ingredients)
            {
                int available = InventorySystem.Instance?.GetItemCount(ingredient.itemID) ?? 0;
                if (available < ingredient.quantity)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Craft recipe, consume materials, add output to inventory.
        /// </summary>
        public bool CraftRecipe(string recipeID)
        {
            if (!CanCraftRecipe(recipeID))
            {
                Debug.LogWarning($"[CraftingStation] Cannot craft {recipeID} (materials or unlock missing)");
                return false;
            }

            var recipe = _recipesByID[recipeID];

            // Consume ingredients
            foreach (var ingredient in recipe.ingredients)
            {
                InventorySystem.Instance?.RemoveItem(ingredient.itemID, ingredient.quantity);
            }

            // Add output
            InventorySystem.Instance?.AddItem(recipe.outputItemID, recipe.outputQuantity);

            // Play crafting SFX
            Audio.AudioManager.Instance?.PlaySFX("craft_success", Vector3.zero);

            Debug.Log($"[CraftingStation] Crafted {recipe.recipeName} ({recipe.outputQuantity}x {recipe.outputItemID})");

            return true;
        }

        /// <summary>
        /// Unlock recipe by ID.
        /// </summary>
        public void UnlockRecipe(string recipeID)
        {
            if (_unlockedRecipes.Contains(recipeID))
            {
                return;
            }

            _unlockedRecipes.Add(recipeID);

            Debug.Log($"[CraftingStation] Unlocked recipe: {recipeID}");

            // TODO: Show UI notification
        }

        /// <summary>
        /// Check if recipe is unlocked.
        /// </summary>
        public bool IsRecipeUnlocked(string recipeID)
        {
            if (!_recipesByID.TryGetValue(recipeID, out var recipe))
            {
                return false;
            }

            // Auto-unlock if no requirement
            if (string.IsNullOrEmpty(recipe.unlockCondition))
            {
                return true;
            }

            return _unlockedRecipes.Contains(recipeID);
        }

        /// <summary>
        /// Get recipe by ID.
        /// </summary>
        public CraftingRecipe GetRecipe(string recipeID)
        {
            _recipesByID.TryGetValue(recipeID, out var recipe);
            return recipe;
        }

        [System.Serializable]
        public class CraftingRecipe
        {
            public string recipeID;
            public string recipeName;
            public string description;
            public StationType requiredStation;
            public CraftingIngredient[] ingredients;
            public string outputItemID;
            public int outputQuantity = 1;
            public string unlockCondition;  // QuestID or "SkillLevel:10"
        }

        [System.Serializable]
        public struct CraftingIngredient
        {
            public string itemID;
            public int quantity;
        }

        public enum StationType : byte
        {
            Workbench = 0,
            Forge = 1,
            AlchemyTable = 2
        }
    }

    /// <summary>
    /// CraftingStation component — attach to station GameObjects in world.
    /// </summary>
    public class CraftingStation : MonoBehaviour, IInteractable
    {
        [Header("Station Settings")]
        [SerializeField] CraftingStationManager.StationType stationType;
        [SerializeField] string stationName = "Workbench";

        public CraftingStationManager.StationType Type => stationType;

        public void Interact(GameObject player)
        {
            Debug.Log($"[CraftingStation] {player.name} interacted with {stationName}");

            // TODO: Open crafting UI panel
            // CraftingUI.Instance?.OpenCraftingMenu(stationType);

            // For now, list available recipes
            var recipes = CraftingStationManager.Instance?.GetUnlockedRecipesForStation(stationType);
            if (recipes != null && recipes.Length > 0)
            {
                Debug.Log($"[CraftingStation] Available recipes at {stationName}:");
                foreach (var recipe in recipes)
                {
                    bool canCraft = CraftingStationManager.Instance.CanCraftRecipe(recipe.recipeID);
                    Debug.Log($"  - {recipe.recipeName} {(canCraft ? "[Can Craft]" : "[Missing Materials]")}");
                }
            }
            else
            {
                Debug.Log($"[CraftingStation] No recipes available at {stationName}");
            }
        }

        public string GetInteractPrompt()
        {
            return $"Use {stationName}";
        }
    }
}
