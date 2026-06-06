using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Data
{
    /// <summary>
    /// ScriptableObject database holding all crafting recipes for the game.
    /// Design per GDD §19 (Economy) — single source of truth for recipe data.
    /// 
    /// Usage:
    /// 1. Create asset: Assets → Create → Tartaria → Crafting → Recipe Database
    /// 2. Add recipe ScriptableObjects to the recipes list
    /// 3. CraftingSystem loads this from Resources/CraftingRecipeDatabase.asset
    /// </summary>
    [CreateAssetMenu(fileName = "CraftingRecipeDatabase", menuName = "Tartaria/Crafting/Recipe Database", order = 0)]
    public class CraftingRecipeDatabase : ScriptableObject
    {
        [Header("All Crafting Recipes")]
        [Tooltip("Complete list of all crafting recipes in the game")]
        public List<CraftingRecipeData> recipes = new List<CraftingRecipeData>();

        [Header("Database Info")]
        [TextArea(2, 3)]
        public string notes = "Recipes organized by MaterialTier progression (Common → Mythic).";

        /// <summary>
        /// Get recipe by ID.
        /// </summary>
        public CraftingRecipeData GetRecipeById(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId)) return null;
            foreach (var recipe in recipes)
            {
                if (recipe != null && recipe.recipeId == recipeId)
                    return recipe;
            }
            return null;
        }

        /// <summary>
        /// Get all recipes for a specific tier.
        /// </summary>
        public List<CraftingRecipeData> GetRecipesByTier(MaterialTier tier)
        {
            var result = new List<CraftingRecipeData>();
            foreach (var recipe in recipes)
            {
                if (recipe != null && recipe.requiredTier == tier)
                    result.Add(recipe);
            }
            return result;
        }

        /// <summary>
        /// Get all recipes unlockable up to and including the specified tier.
        /// </summary>
        public List<CraftingRecipeData> GetRecipesUpToTier(MaterialTier tier)
        {
            var result = new List<CraftingRecipeData>();
            foreach (var recipe in recipes)
            {
                if (recipe != null && recipe.requiredTier <= tier)
                    result.Add(recipe);
            }
            return result;
        }

        /// <summary>
        /// Get total recipe count.
        /// </summary>
        public int GetRecipeCount()
        {
            int count = 0;
            foreach (var recipe in recipes)
            {
                if (recipe != null) count++;
            }
            return count;
        }

        void OnValidate()
        {
            // Remove null entries
            recipes.RemoveAll(r => r == null);

            // Check for duplicate recipe IDs
            var ids = new HashSet<string>();
            foreach (var recipe in recipes)
            {
                if (recipe == null || string.IsNullOrEmpty(recipe.recipeId)) continue;
                if (ids.Contains(recipe.recipeId))
                {
                    Debug.LogWarning($"[CraftingRecipeDatabase] Duplicate recipe ID: {recipe.recipeId}", this);
                }
                ids.Add(recipe.recipeId);
            }

            Debug.Log($"[CraftingRecipeDatabase] Validated {GetRecipeCount()} recipes");
        }
    }
}
