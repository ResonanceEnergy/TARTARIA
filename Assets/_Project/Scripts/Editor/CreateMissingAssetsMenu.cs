using UnityEditor;
using UnityEngine;
using System.IO;
using Tartaria.Data;
using Tartaria.Core;

namespace Tartaria.Editor
{
    /// <summary>
    /// Simple menu items to create missing ScriptableObject assets.
    /// </summary>
    public static class CreateMissingAssetsMenu
    {
        [MenuItem("Tartaria/5 Asset Database/Performance Profile", priority = 530)]
        static void CreatePerformanceProfile()
        {
            string path = "Assets/_Project/Resources";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            if (Resources.Load<PerformanceProfile>("PerformanceProfile") != null)
            {
                Debug.LogWarning("PerformanceProfile already exists");
                return;
            }
            
            var asset = ScriptableObject.CreateInstance<PerformanceProfile>();
            string assetPath = $"{path}/PerformanceProfile.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created {assetPath}");
        }

        [MenuItem("Tartaria/5 Asset Database/Crafting Recipe Database", priority = 520)]
        static void CreateCraftingRecipeDatabase()
        {
            string path = "Assets/_Project/Resources";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            if (Resources.Load<CraftingRecipeDatabase>("CraftingRecipeDatabase") != null)
            {
                Debug.LogWarning("CraftingRecipeDatabase already exists");
                return;
            }
            
            var asset = ScriptableObject.CreateInstance<CraftingRecipeDatabase>();
            string assetPath = $"{path}/CraftingRecipeDatabase.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created {assetPath}");
        }

        [MenuItem("Tartaria/5 Asset Database/Archive Database", priority = 510)]
        static void CreateArchiveDatabase()
        {
            string path = "Assets/_Project/Resources";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            if (Resources.Load<ArchiveDatabase>("ArchiveDatabase") != null)
            {
                Debug.LogWarning("ArchiveDatabase already exists");
                return;
            }
            
            var asset = ScriptableObject.CreateInstance<ArchiveDatabase>();
            string assetPath = $"{path}/ArchiveDatabase.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created {assetPath}");
        }

        [MenuItem("Tartaria/5 Asset Database/Create All Missing", priority = 500)]
        static void CreateAllMissingAssets()
        {
            CreatePerformanceProfile();
            CreateCraftingRecipeDatabase();
            CreateArchiveDatabase();
            AssetDatabase.Refresh();
            Debug.Log("[CreateMissingAssets] All missing assets created");
        }
    }
}
