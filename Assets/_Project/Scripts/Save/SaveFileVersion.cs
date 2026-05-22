using UnityEngine;

namespace Tartaria.Save
{
    /// <summary>
    /// Save File Version Helper — handles save data format migrations.
    /// Ensures backward compatibility when save schema changes.
    /// </summary>
    public static class SaveFileVersion
    {
        public const int CURRENT_VERSION = 2;  // Increment when schema changes

        /// <summary>
        /// Migrate save data from old version to current.
        /// </summary>
        public static bool MigrateSaveData(ref SaveData data)
        {
            if (data == null) return false;

            int fromVersion = data.version;
            if (fromVersion == CURRENT_VERSION) return true;  // Already current

            Debug.Log($"[SaveFileVersion] Migrating save from v{fromVersion} to v{CURRENT_VERSION}");

            // Version-specific migrations
            if (fromVersion < 1)
            {
                // V0 → V1: Added moon progress tracking
                Debug.Log("[SaveFileVersion] V0→V1: Initializing moon progress");
                // (data would be modified here if needed)
            }

            if (fromVersion < 2)
            {
                // V1 → V2: Added achievement tracking
                Debug.Log("[SaveFileVersion] V1→V2: Initializing achievements");
                // (data would be modified here if needed)
            }

            // Update version
            data.version = CURRENT_VERSION;

            Debug.Log($"[SaveFileVersion] Migration complete to v{CURRENT_VERSION}");
            return true;
        }

        /// <summary>
        /// Check if save file is compatible.
        /// </summary>
        public static bool IsCompatible(int saveVersion)
        {
            // Accept saves up to 2 versions old
            return saveVersion >= (CURRENT_VERSION - 2);
        }

        /// <summary>
        /// Get version changelog for display to player.
        /// </summary>
        public static string GetChangelog(int fromVersion, int toVersion)
        {
            if (fromVersion == toVersion) return "No changes";

            string changelog = $"Save file updated v{fromVersion} → v{toVersion}:\n";

            if (fromVersion < 1 && toVersion >= 1)
            {
                changelog += "- Added: Moon progress tracking\n";
            }

            if (fromVersion < 2 && toVersion >= 2)
            {
                changelog += "- Added: Achievement system\n";
            }

            return changelog;
        }
    }
}
