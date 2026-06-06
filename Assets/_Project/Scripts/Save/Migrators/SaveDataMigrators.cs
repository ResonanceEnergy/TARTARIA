using UnityEngine;

namespace Tartaria.Save
{
    /// <summary>
    /// Save Data Migrator: V17 → V18
    /// 
    /// Changes:
    ///   - Added schemaVersion field to all data classes
    ///   - Added migration system infrastructure
    ///   - No breaking data changes (purely additive)
    /// 
    /// Migration strategy:
    ///   - Stamp schemaVersion = 18 on SaveData
    ///   - Initialize provider schema versions
    ///   - Preserve all existing data
    /// </summary>
    public class SaveDataMigrator_V17_to_V18 : IDataMigrator<SaveData, SaveData>
    {
        public int FromVersion => 17;
        public int ToVersion => 18;

        public SaveData Migrate(SaveData input)
        {
            if (input == null)
            {
                Debug.LogError("[SaveDataMigrator_V17_to_V18] Input is null!");
                return null;
            }

            // Clone input (don't modify original)
            var output = CloneSaveData(input);

            // Stamp new version
            output.version = SchemaVersion.SAVE_V18;

            // Initialize new schema fields (if any were added in v18)
            // For v18, this is purely infrastructure - no new data fields

            Debug.Log("[SaveDataMigrator_V17_to_V18] Migration complete: added schema versioning system");
            return output;
        }

        public bool Validate(SaveData input)
        {
            if (input == null) return false;
            if (input.version != 17)
            {
                Debug.LogWarning($"[SaveDataMigrator_V17_to_V18] Expected version 17, got {input.version}");
                return false;
            }
            return true;
        }

        public string GetChangeDescription()
        {
            return "V17→V18: Added schema versioning system (infrastructure only, no data changes)";
        }

        /// <summary>
        /// Deep clone SaveData (Unity's JsonUtility.FromJson(ToJson) is simplest for serializable data).
        /// </summary>
        SaveData CloneSaveData(SaveData original)
        {
            string json = JsonUtility.ToJson(original);
            return JsonUtility.FromJson<SaveData>(json);
        }
    }

    /// <summary>
    /// Save Data Migrator: V2 → V17 (Large Jump)
    /// 
    /// Changes over 15 versions:
    ///   V3: Economy, Codex, Thorne, Korath, Tutorial, DialogueTree blocks
    ///   V4: Milo, Lirael, Zereth blocks
    ///   V5: Veritas block
    ///   V6: Airship, Ley Line, Bell Tower, Giant Mode, World Choice, Achievements, Dialogue Arcs
    ///   V7: Excavation, Crafting, Scanner, Rail, Aquifer Purge, Cosmic Convergence, Day Out of Time, Companion Manager
    ///   V8: Combat Wave block
    ///   V9: Archive block
    ///   V10: Moon2, Moon3, Moon5 blocks
    ///   V14: Echohaven block
    ///   V15: MoonFlags, MoonFlagsInt, globalFlags
    ///   V17: ProviderData
    /// 
    /// Migration strategy:
    ///   - All new blocks auto-initialize via C# constructors
    ///   - Only need to bump version field
    /// </summary>
    public class SaveDataMigrator_V2_to_V17 : IDataMigrator<SaveData, SaveData>
    {
        public int FromVersion => 2;
        public int ToVersion => 17;

        public SaveData Migrate(SaveData input)
        {
            if (input == null) return null;

            var output = CloneSaveData(input);
            output.version = SchemaVersion.SAVE_V17;

            // All new blocks initialize via their default constructors
            // No manual field copying needed (additive schema)

            Debug.Log("[SaveDataMigrator_V2_to_V17] Migration complete: initialized all v3-v17 blocks");
            return output;
        }

        public bool Validate(SaveData input)
        {
            return input != null && input.version == 2;
        }

        public string GetChangeDescription()
        {
            return "V2→V17: Added 35+ save blocks (economy, companions, moons, provider data)";
        }

        SaveData CloneSaveData(SaveData original)
        {
            string json = JsonUtility.ToJson(original);
            return JsonUtility.FromJson<SaveData>(json);
        }
    }
}
