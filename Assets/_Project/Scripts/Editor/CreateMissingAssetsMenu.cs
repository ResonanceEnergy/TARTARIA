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
        [MenuItem("Tartaria/Create Missing Assets/Performance Profile")]
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

        [MenuItem("Tartaria/Create Missing Assets/Crafting Recipe Database")]
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

        [MenuItem("Tartaria/Create Missing Assets/Archive Database")]
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

        [MenuItem("Tartaria/Create Missing Assets/All Missing Assets")]
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
