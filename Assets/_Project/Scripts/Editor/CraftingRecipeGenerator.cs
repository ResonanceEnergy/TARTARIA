using UnityEngine;
using UnityEditor;
using Tartaria.Data;
using Tartaria.Core;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor utility to generate example crafting recipe ScriptableObject assets.
    /// Menu: Tartaria → Crafting → Generate Example Recipes
    /// 
    /// Creates 8 recipes covering all MaterialTiers (Common → Mythic) and the CraftingRecipeDatabase.
    /// </summary>
    public static class CraftingRecipeGenerator
    {
        const string RecipePath = "Assets/_Project/Resources/Recipes";
        const string DatabasePath = "Assets/_Project/Resources";

        [MenuItem("Tartaria/4 Generate Art/Crafting Recipes", false, 100)]
        public static void GenerateExampleRecipes()
        {
            // Ensure directories exist
            if (!Directory.Exists(RecipePath))
                Directory.CreateDirectory(RecipePath);
            if (!Directory.Exists(DatabasePath))
                Directory.CreateDirectory(DatabasePath);

            Debug.Log("[CraftingRecipeGenerator] Generating example recipes...");

            var recipes = new CraftingRecipeData[]
            {
                CreateRepairKit(),
                CreateAetherLens(),
                CreateResonanceAmplifier(),
                CreateHealthPotion(),
                CreateGolemHeart(),
                CreateHarmonicBlade(),
                CreateVoidAnchor(),
                CreateTruthResonator()
            };

            int created = 0;
            foreach (var recipe in recipes)
            {
                string path = $"{RecipePath}/Recipe_{recipe.recipeId}.asset";
                
                // Only create if doesn't exist
                if (File.Exists(path))
                {
                    Debug.Log($"[CraftingRecipeGenerator] Skipping existing: {recipe.recipeId}");
                    continue;
                }

                AssetDatabase.CreateAsset(recipe, path);
                created++;
                Debug.Log($"[CraftingRecipeGenerator] Created: {path}");
            }

            // Create or update database
            CreateOrUpdateDatabase(recipes);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CraftingRecipeGenerator] Complete! Created {created} new recipes.");
            EditorUtility.DisplayDialog(
                "Recipe Generation Complete",
                $"Created {created} new recipes.\nDatabase: {DatabasePath}/CraftingRecipeDatabase.asset",
                "OK"
            );
        }

        static void CreateOrUpdateDatabase(CraftingRecipeData[] recipes)
        {
            string dbPath = $"{DatabasePath}/CraftingRecipeDatabase.asset";
            var database = AssetDatabase.LoadAssetAtPath<CraftingRecipeDatabase>(dbPath);

            if (database == null)
            {
                database = ScriptableObject.CreateInstance<CraftingRecipeDatabase>();
                AssetDatabase.CreateAsset(database, dbPath);
                Debug.Log($"[CraftingRecipeGenerator] Created new database: {dbPath}");
            }

            // Add all recipes to database (avoiding duplicates)
            foreach (var recipe in recipes)
            {
                if (!database.recipes.Contains(recipe))
                {
                    database.recipes.Add(recipe);
                }
            }

            EditorUtility.SetDirty(database);
            Debug.Log($"[CraftingRecipeGenerator] Updated database with {database.GetRecipeCount()} recipes");
        }

        // ─── Recipe Definitions ───────────────────────────────────────

        static CraftingRecipeData CreateRepairKit()
        {
            var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipe.recipeId = "repair_kit";
            recipe.displayName = "Repair Kit";
            recipe.description = "Restores 30 HP to the nearest damaged building. Essential for maintaining Echohaven infrastructure.";
            recipe.requiredTier = MaterialTier.Common;
            recipe.requiredMoonNumber = 1;
            recipe.outputItemId = "repair_kit";
            recipe.outputCount = 1;
            recipe.costs = new CraftingCostEntry[]
            {
                new CraftingCostEntry { currency = CurrencyType.AetherShards, amount = 30 }
            };
            return recipe;
        }

        static CraftingRecipeData CreateAetherLens()
        {
            var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipe.recipeId = "aether_lens";
            recipe.displayName = "Aether Lens";
            recipe.description = "Reveals hidden excavation sites on the current moon. Critical for discovering ancient knowledge.";
            recipe.requiredTier = MaterialTier.Uncommon;
            recipe.requiredMoonNumber = 2;
            recipe.outputItemId = "echo_lens"; // Note: UseItem case uses "echo_lens"
            recipe.outputCount = 1;
            recipe.costs = new CraftingCostEntry[]
            {
                new CraftingCostEntry { currency = CurrencyType.AetherShards, amount = 100 },
                new CraftingCostEntry { currency = CurrencyType.HarmonicFragments, amount = 5 }
            };
            return recipe;
        }

        static CraftingRecipeData CreateResonanceAmplifier()
        {
            var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipe.recipeId = "resonance_amplifier";
            recipe.displayName = "Resonance Amplifier";
            recipe.description = "Boosts Resonance Score gain by 25% for 60 seconds. Accelerates progression through difficult moons.";
            recipe.requiredTier = MaterialTier.Uncommon;
            recipe.requiredMoonNumber = 3;
            recipe.outputItemId = "resonance_amplifier";
            recipe.outputCount = 1;
            recipe.costs = new CraftingCostEntry[]
            {
                new CraftingCostEntry { currency = CurrencyType.AetherShards, amount = 100 },
                new CraftingCostEntry { currency = CurrencyType.HarmonicFragments, amount = 5 }
            };
            return recipe;
        }

        static CraftingRecipeData CreateHealthPotion()
        {
            var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipe.recipeId = "health_potion";
            recipe.displayName = "Aether Potion";
            recipe.description = "Refills 50 Aether charge to the player. Life-saving in combat-heavy moons.";
            recipe.requiredTier = MaterialTier.Common;
            recipe.requiredMoonNumber = 1;
            recipe.outputItemId = "aether_potion"; // Note: UseItem case uses "aether_potion"
            recipe.outputCount = 1;
            recipe.costs = new CraftingCostEntry[]
            {
                new CraftingCostEntry { currency = CurrencyType.AetherShards, amount = 50 }
            };
            return recipe;
        }

        static CraftingRecipeData CreateGolemHeart()
        {
            var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipe.recipeId = "golem_heart";
            recipe.displayName = "Golem Heart";
            recipe.description = "Crafted from crystalline essence. Required for advanced golem research and Moon 4 progression.";
            recipe.requiredTier = MaterialTier.Rare;
            recipe.requiredMoonNumber = 4;
            recipe.outputItemId = "golem_heart";
            recipe.outputCount = 1;
            recipe.costs = new CraftingCostEntry[]
            {
                new CraftingCostEntry { currency = CurrencyType.ResonanceCrystals, amount = 15 },
                new CraftingCostEntry { currency = CurrencyType.CrystallineDust, amount = 10 }
            };
            return recipe;
        }

        static CraftingRecipeData CreateHarmonicBlade()
        {
            var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipe.recipeId = "harmonic_blade";
            recipe.displayName = "Harmonic Blade";
            recipe.description = "Weapon upgrade for high-tier combat. Increases damage output by 40%.";
            recipe.requiredTier = MaterialTier.Epic;
            recipe.requiredMoonNumber = 7;
            recipe.outputItemId = "harmonic_blade";
            recipe.outputCount = 1;
            recipe.costs = new CraftingCostEntry[]
            {
                new CraftingCostEntry { currency = CurrencyType.ResonanceCrystals, amount = 25 },
                new CraftingCostEntry { currency = CurrencyType.CrystallineDust, amount = 10 },
                new CraftingCostEntry { currency = CurrencyType.ForgeTokens, amount = 5 }
            };
            return recipe;
        }

        static CraftingRecipeData CreateVoidAnchor()
        {
            var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipe.recipeId = "void_anchor";
            recipe.displayName = "Void Anchor";
            recipe.description = "Stabilizes temporal anomalies. Required for Moon 10-12 navigation and boss encounters.";
            recipe.requiredTier = MaterialTier.Legendary;
            recipe.requiredMoonNumber = 10;
            recipe.outputItemId = "void_anchor";
            recipe.outputCount = 1;
            recipe.costs = new CraftingCostEntry[]
            {
                new CraftingCostEntry { currency = CurrencyType.StarFragments, amount = 5 },
                new CraftingCostEntry { currency = CurrencyType.CrystallineDust, amount = 20 },
                new CraftingCostEntry { currency = CurrencyType.ForgeTokens, amount = 15 }
            };
            return recipe;
        }

        static CraftingRecipeData CreateTruthResonator()
        {
            var recipe = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipe.recipeId = "truth_resonator";
            recipe.displayName = "Truth Resonator";
            recipe.description = "Unlocks the path to Moon 13 finale. Amplifies the player's connection to the Eternal Frequency.";
            recipe.requiredTier = MaterialTier.Ascendant;
            recipe.requiredMoonNumber = 13;
            recipe.outputItemId = "truth_resonator";
            recipe.outputCount = 1;
            recipe.costs = new CraftingCostEntry[]
            {
                new CraftingCostEntry { currency = CurrencyType.StarFragments, amount = 15 },
                new CraftingCostEntry { currency = CurrencyType.HarmonicFragments, amount = 50 },
                new CraftingCostEntry { currency = CurrencyType.ForgeTokens, amount = 25 }
            };
            return recipe;
        }
    }
}
