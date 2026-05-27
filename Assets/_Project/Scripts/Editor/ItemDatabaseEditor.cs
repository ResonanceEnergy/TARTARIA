#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Tartaria.Data.Editor
{
    /// <summary>
    /// Item Database Editor — helper utilities for managing ItemDatabase.
    /// Provides auto-population and validation tools.
    /// </summary>
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ItemDatabase db = (ItemDatabase)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Database Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Auto-Populate from Assets"))
            {
                AutoPopulateDatabase(db);
            }

            if (GUILayout.Button("Validate Item IDs"))
            {
                ValidateDatabase(db);
            }

            if (GUILayout.Button("Sort Items by ID"))
            {
                SortItemsById(db);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                $"Items in database: {db.GetAllItems().Count}\n" +
                "Use 'Auto-Populate' to find all ItemData assets in the project.",
                MessageType.Info);
        }

        void AutoPopulateDatabase(ItemDatabase db)
        {
            // Find all ItemData assets in the project
            string[] guids = AssetDatabase.FindAssets("t:ItemData");
            int added = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);

                if (item != null && !db.GetAllItems().Contains(item))
                {
                    db.AddItem(item);
                    added++;
                }
            }

            if (added > 0)
            {
                EditorUtility.SetDirty(db);
                AssetDatabase.SaveAssets();
                Debug.Log($"[ItemDatabase] Auto-populated {added} new items");
            }
            else
            {
                Debug.Log("[ItemDatabase] No new items found");
            }
        }

        void ValidateDatabase(ItemDatabase db)
        {
            var items = db.GetAllItems();
            int errors = 0;
            int warnings = 0;

            foreach (var item in items)
            {
                if (item == null)
                {
                    Debug.LogError("[ItemDatabase] Null item in database");
                    errors++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.itemID))
                {
                    Debug.LogError($"[ItemDatabase] Item '{item.name}' has empty itemID", item);
                    errors++;
                }

                if (string.IsNullOrWhiteSpace(item.displayName))
                {
                    Debug.LogWarning($"[ItemDatabase] Item '{item.itemID}' has empty displayName", item);
                    warnings++;
                }

                if (item.icon == null)
                {
                    Debug.LogWarning($"[ItemDatabase] Item '{item.itemID}' has no icon", item);
                    warnings++;
                }

                if (item.stackSize < 1)
                {
                    Debug.LogWarning($"[ItemDatabase] Item '{item.itemID}' has invalid stackSize {item.stackSize}", item);
                    warnings++;
                }
            }

            // Check for duplicates
            var duplicates = items
                .GroupBy(item => item?.itemID)
                .Where(g => g.Count() > 1 && !string.IsNullOrWhiteSpace(g.Key))
                .ToList();

            if (duplicates.Any())
            {
                foreach (var dup in duplicates)
                {
                    Debug.LogError($"[ItemDatabase] Duplicate itemID '{dup.Key}' found {dup.Count()} times");
                    errors++;
                }
            }

            if (errors == 0 && warnings == 0)
            {
                Debug.Log($"[ItemDatabase] Validation passed! {items.Count} items OK");
            }
            else
            {
                Debug.LogWarning($"[ItemDatabase] Validation complete: {errors} errors, {warnings} warnings");
            }
        }

        void SortItemsById(ItemDatabase db)
        {
            var items = db.GetAllItems().ToList();
            items = items.OrderBy(item => item?.itemID).ToList();

            // Rebuild the list
            foreach (var item in items)
            {
                db.RemoveItem(item);
            }

            foreach (var item in items)
            {
                db.AddItem(item);
            }

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ItemDatabase] Sorted {items.Count} items by ID");
        }
    }

    /// <summary>
    /// Item Database Creation Menu — helper to create ItemDatabase asset.
    /// </summary>
    public static class ItemDatabaseCreator
    {
        [MenuItem("Assets/Create/Tartaria/Setup ItemDatabase", priority = 98)]
        public static void CreateItemDatabaseInResources()
        {
            // Check if ItemDatabase already exists
            var existing = Resources.Load<ItemDatabase>("ItemDatabase");
            if (existing != null)
            {
                Debug.LogWarning("[ItemDatabase] ItemDatabase already exists at Resources/ItemDatabase.asset");
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            // Create Resources folder if it doesn't exist
            string resourcesPath = "Assets/_Project/Resources";
            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
            }

            // Create the database asset
            var db = ScriptableObject.CreateInstance<ItemDatabase>();
            string assetPath = $"{resourcesPath}/ItemDatabase.asset";
            
            AssetDatabase.CreateAsset(db, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = db;
            EditorGUIUtility.PingObject(db);

            Debug.Log($"[ItemDatabase] Created at {assetPath}");
        }

        [MenuItem("Assets/Create/Tartaria/Setup Example Items", priority = 97)]
        public static void CreateExampleItems()
        {
            string itemsPath = "Assets/_Project/Resources/Items";
            
            // Create Items folder if it doesn't exist
            if (!Directory.Exists(itemsPath))
            {
                Directory.CreateDirectory(itemsPath);
            }

            // Define example items
            var exampleItems = new[]
            {
                new { id = "aether_shard", name = "Aether Shard", desc = "A crystalline fragment pulsing with temporal energy. Essential for resonance rituals.", category = ItemCategory.Material, rarity = ItemRarity.Rare, value = 150, weight = 0.2f, stack = 50 },
                new { id = "golem_core", name = "Golem Core", desc = "The inert heart of a mud golem. Still warm with residual animating force.", category = ItemCategory.Material, rarity = ItemRarity.Uncommon, value = 85, weight = 3.5f, stack = 10 },
                new { id = "resonance_crystal", name = "Resonance Crystal", desc = "A perfectly tuned crystal that amplifies Resonance Shard collection. Rare and valuable.", category = ItemCategory.Material, rarity = ItemRarity.Epic, value = 500, weight = 0.5f, stack = 20 },
                new { id = "repair_kit", name = "Repair Kit", desc = "Contains tools and materials for emergency building repairs. One-time use.", category = ItemCategory.Consumable, rarity = ItemRarity.Common, value = 30, weight = 1.2f, stack = 5 },
                new { id = "health_potion", name = "Health Potion", desc = "A crimson elixir that restores vitality. Tastes of iron and honey.", category = ItemCategory.Consumable, rarity = ItemRarity.Common, value = 25, weight = 0.3f, stack = 10 }
            };

            int created = 0;
            foreach (var itemDef in exampleItems)
            {
                string assetPath = $"{itemsPath}/{itemDef.id}.asset";
                
                // Skip if already exists
                if (File.Exists(assetPath))
                {
                    Debug.Log($"[ItemDatabase] Skipping existing item: {itemDef.id}");
                    continue;
                }

                var item = ScriptableObject.CreateInstance<ItemData>();
                item.itemID = itemDef.id;
                item.displayName = itemDef.name;
                item.description = itemDef.desc;
                item.category = itemDef.category;
                item.rarity = itemDef.rarity;
                item.value = itemDef.value;
                item.weight = itemDef.weight;
                item.stackSize = itemDef.stack;

                AssetDatabase.CreateAsset(item, assetPath);
                created++;
            }

            if (created > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[ItemDatabase] Created {created} example items in {itemsPath}");
            }
            else
            {
                Debug.Log("[ItemDatabase] All example items already exist");
            }

            // Ping the folder
            var folder = AssetDatabase.LoadAssetAtPath<Object>(itemsPath);
            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
        }
    }
}
#endif
