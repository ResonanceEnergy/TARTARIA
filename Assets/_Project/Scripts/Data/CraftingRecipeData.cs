using System;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Data
{
    /// <summary>
    /// ScriptableObject representing a single crafting recipe.
    /// Design per GDD §19 (Economy) — externalized recipe data for maintainability.
    /// </summary>
    [CreateAssetMenu(fileName = "Recipe_", menuName = "Tartaria/Crafting/Recipe", order = 1)]
    public class CraftingRecipeData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this recipe (e.g., 'repair_kit')")]
        public string recipeId;
        
        [Tooltip("Display name shown in crafting UI")]
        public string displayName;
        
        [TextArea(3, 5)]
        [Tooltip("Description of what the crafted item does")]
        public string description;

        [Header("Requirements")]
        [Tooltip("Minimum material tier required to unlock this recipe")]
        public MaterialTier requiredTier;
        
        [Tooltip("Specific Moon unlock requirement (0 = available from start)")]
        [Range(0, 13)]
        public int requiredMoonNumber;
        
        [Tooltip("Quest ID that must be completed to unlock (empty = no quest requirement)")]
        public string requiredQuestId;

        [Header("Costs")]
        [Tooltip("Currency costs to craft this recipe")]
        public CraftingCostEntry[] costs;

        [Header("Output")]
        [Tooltip("Item ID of the crafted output")]
        public string outputItemId;
        
        [Tooltip("Number of items produced per craft")]
        [Range(1, 99)]
        public int outputCount = 1;

        [Header("Optional: Icon")]
        [Tooltip("Icon sprite for UI display")]
        public Sprite icon;

        void OnValidate()
        {
            // Auto-generate recipeId from displayName if empty
            if (string.IsNullOrEmpty(recipeId) && !string.IsNullOrEmpty(displayName))
            {
                recipeId = displayName.ToLower().Replace(" ", "_");
            }
        }
    }

    /// <summary>
    /// Serializable cost entry for ScriptableObject inspector.
    /// </summary>
    [Serializable]
    public struct CraftingCostEntry
    {
        public CurrencyType currency;
        [Range(1, 10000)]
        public int amount;
    }
}
