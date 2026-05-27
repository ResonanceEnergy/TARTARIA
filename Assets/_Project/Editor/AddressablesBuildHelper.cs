using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Helper to build Addressables content from batch mode or menu.
    /// Generates Library/com.unity.addressables/aa/Windows/settings.json runtime data.
    /// </summary>
    public static class AddressablesBuildHelper
    {
        [MenuItem("Tartaria/Build/Build Addressables Content")]
        public static void BuildAddressablesContent()
        {
            Debug.Log("[AddressablesBuildHelper] Starting Addressables build...");
            
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("[AddressablesBuildHelper] No Addressables settings found. Creating default settings...");
                settings = AddressableAssetSettings.Create(
                    AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                    AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName,
                    true,
                    true
                );
                AddressableAssetSettingsDefaultObject.Settings = settings;
            }
            
            // Build content using default build script
            AddressableAssetSettings.BuildPlayerContent(out var result);
            
            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError($"[AddressablesBuildHelper] Build failed: {result.Error}");
            }
            else
            {
                Debug.Log($"[AddressablesBuildHelper] Build complete. Duration: {result.Duration:F2}s");
            }
        }
        
        /// <summary>
        /// Batch mode entry point. Call from command line with:
        /// Unity.exe -batchmode -quit -projectPath "." -executeMethod "Tartaria.Editor.AddressablesBuildHelper.BuildFromCommandLine"
        /// </summary>
        public static void BuildFromCommandLine()
        {
            Debug.Log("[AddressablesBuildHelper] Building Addressables from command line...");
            BuildAddressablesContent();
            
            // Exit code 0 = success
            if (AddressableAssetSettingsDefaultObject.Settings != null)
            {
                EditorApplication.Exit(0);
            }
            else
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
