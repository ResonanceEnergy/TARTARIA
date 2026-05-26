using UnityEngine;
using Tartaria.Data;

namespace Tartaria.Save
{
    /// <summary>
    /// Item Data Migrator: V1 → V2 (Future Example)
    /// 
    /// EXAMPLE migration for future stat system refactor.
    /// NOT ACTIVE YET (CURRENT_ITEM = V1).
    /// 
    /// Hypothetical changes:
    ///   - Added: durability field
    ///   - Added: enchantmentSlots field
    ///   - Removed: customData field (migrated to structured data)
    ///   - Changed: weight from float to int (kg → g)
    /// 
    /// When this goes live:
    ///   1. Update SchemaVersion.CURRENT_ITEM = 2
    ///   2. Add [SerializeField] int schemaVersion = 1 to ItemData
    ///   3. Register this migrator in MigrationPipeline
    ///   4. Run batch migration tool on all item assets
    /// </summary>
    public class ItemDataMigrator_V1_to_V2 : IDataMigrator<ItemData, ItemData>
    {
        public int FromVersion => 1;
        public int ToVersion => 2;

        public ItemData Migrate(ItemData input)
        {
            if (input == null)
            {
                Debug.LogError("[ItemDataMigrator_V1_to_V2] Input is null!");
                return null;
            }

            // Clone via ScriptableObject.Instantiate
            var output = Object.Instantiate(input);

            // V2 CHANGES (example):
            // output.durability = 100f;           // Add new field with default
            // output.enchantmentSlots = 0;         // Add new field
            // output.weightGrams = (int)(input.weight * 1000); // Convert kg to grams
            // MigrateCustomData(input.customData, output); // Parse customData into structured fields

            Debug.Log($"[ItemDataMigrator_V1_to_V2] Migrated item: {input.itemID}");
            return output;
        }

        public bool Validate(ItemData input)
        {
            if (input == null) return false;
            // Check schemaVersion field once it's added
            // For now, all ItemData are v1
            return true;
        }

        public string GetChangeDescription()
        {
            return "V1→V2: Added durability/enchantment slots, converted weight kg→g (FUTURE)";
        }

        // Example: parse customData JSON into structured fields
        void MigrateCustomData(string customData, ItemData output)
        {
            if (string.IsNullOrEmpty(customData)) return;

            try
            {
                // var data = JsonUtility.FromJson<CustomItemData>(customData);
                // output.enchantmentSlots = data.slots;
                // output.durability = data.maxDurability;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ItemDataMigrator] Failed to parse customData: {ex.Message}");
            }
        }
    }
}
