using UnityEngine;
using UnityEditor;
using Tartaria.Data;

namespace Tartaria.Editor
{
    /// <summary>
    /// One-shot creator for GameBalanceConfig ScriptableObject asset.
    /// Run via Menu: Tartaria → Create Game Balance Config
    /// </summary>
    public static class CreateGameBalanceConfig
    {
        [MenuItem("Tartaria/5 Asset Database/Game Balance Config", priority = 540)]
        static void CreateAsset()
        {
            // Check if already exists
            var existing = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (existing != null)
            {
                Debug.Log("[CreateGameBalanceConfig] Asset already exists at Resources/GameBalanceConfig.asset");
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            // Create instance with default values
            var config = ScriptableObject.CreateInstance<GameBalanceConfig>();

            // Ensure Resources folder exists
            var resourcesPath = "Assets/_Project/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                var projectPath = "Assets/_Project";
                if (!AssetDatabase.IsValidFolder(projectPath))
                {
                    AssetDatabase.CreateFolder("Assets", "_Project");
                }
                AssetDatabase.CreateFolder(projectPath, "Resources");
            }

            // Create asset
            var assetPath = $"{resourcesPath}/GameBalanceConfig.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CreateGameBalanceConfig] Created asset at {assetPath}");
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        [InitializeOnLoadMethod]
        static void AutoCreateOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                var existing = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
                if (existing == null)
                {
                    Debug.Log("[CreateGameBalanceConfig] Auto-creating missing GameBalanceConfig asset...");
                    CreateAsset();
                }
            };
        }
    }
}
