using UnityEditor;
using UnityEngine;
using System.IO;
using Tartaria.Data;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.Editor
{
    /// <summary>
    /// Auto-creates missing critical ScriptableObject assets on domain reload.
    /// Prevents runtime errors from missing Resources.Load calls.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoCreateMissingAssets
    {
        private static bool hasRun = false;

        static AutoCreateMissingAssets()
        {
            EditorApplication.delayCall += CheckAndCreateMissingAssets;
        }

        static void CheckAndCreateMissingAssets()
        {
            // Only run once per session to avoid duplicate manager warnings
            if (hasRun) return;
            hasRun = true;

            string resourcesPath = "Assets/_Project/Resources";
            bool needsRefresh = false;

            // Ensure Resources folder exists
            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
                needsRefresh = true;
            }

            // Check and create ItemDatabase
            if (Resources.Load<ItemDatabase>("ItemDatabase") == null)
            {
                Debug.Log("[AutoCreate] Creating ItemDatabase...");
                var db = ScriptableObject.CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(db, $"{resourcesPath}/ItemDatabase.asset");
                needsRefresh = true;
            }

            // Check and create PerformanceProfile
            if (Resources.Load<PerformanceProfile>("PerformanceProfile") == null)
            {
                Debug.Log("[AutoCreate] Creating PerformanceProfile...");
                var profile = ScriptableObject.CreateInstance<PerformanceProfile>();
                AssetDatabase.CreateAsset(profile, $"{resourcesPath}/PerformanceProfile.asset");
                needsRefresh = true;
            }

            // Check and create CraftingRecipeDatabase
            if (Resources.Load<CraftingRecipeDatabase>("CraftingRecipeDatabase") == null)
            {
                Debug.Log("[AutoCreate] Creating CraftingRecipeDatabase...");
                var craftingDB = ScriptableObject.CreateInstance<CraftingRecipeDatabase>();
                AssetDatabase.CreateAsset(craftingDB, $"{resourcesPath}/CraftingRecipeDatabase.asset");
                needsRefresh = true;
            }

            // Check and create ArchiveDatabase
            if (Resources.Load<ArchiveDatabase>("ArchiveDatabase") == null)
            {
                Debug.Log("[AutoCreate] Creating ArchiveDatabase...");
                var archiveDB = ScriptableObject.CreateInstance<ArchiveDatabase>();
                AssetDatabase.CreateAsset(archiveDB, $"{resourcesPath}/ArchiveDatabase.asset");
                needsRefresh = true;
            }

            // Only refresh once if anything was created
            if (needsRefresh)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}
