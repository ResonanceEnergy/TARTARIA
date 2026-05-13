using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using System.Linq;
using Tartaria.Core;

namespace Tartaria.Editor
{
    /// <summary>
    /// Aether Fog Installer — adds AetherFogRendererFeature to TartariaURP_Renderer
    /// if not already present. Menu: Tartaria → Rendering → Install Aether Fog Feature.
    /// Auto-installs on Editor load. Idempotent: safe to run multiple times.
    /// </summary>
    [InitializeOnLoad]
    public static class AetherFogInstaller
    {
        const string kRendererAssetPath = "Assets/_Project/Config/TartariaURP_Renderer.asset";

        static AetherFogInstaller()
        {
            // Auto-install on Editor startup (idempotent check inside)
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying && !EditorApplication.isCompiling)
                    InstallAetherFogFeatureInternal();
            };
        }

        [MenuItem("Tartaria/Rendering/Install Aether Fog Feature")]
        public static void InstallAetherFogFeature()
        {
            InstallAetherFogFeatureInternal();
        }

        /// <summary>
        /// Batch-safe installer (can be called via -executeMethod).
        /// </summary>
        public static void InstallAetherFogFeatureInternal()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(kRendererAssetPath);
            if (rendererData == null)
            {
                Debug.LogError($"[AetherFogInstaller] Renderer asset not found at {kRendererAssetPath}");
                return;
            }

            // Check if feature already exists
            var existingFeature = rendererData.rendererFeatures
                .OfType<AetherFogRendererFeature>()
                .FirstOrDefault();

            if (existingFeature != null)
            {
                Debug.Log("[AetherFogInstaller] AetherFogRendererFeature already installed.");
                
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Aether Fog Installer", 
                        "AetherFogRendererFeature is already present in TartariaURP_Renderer.", 
                        "OK");
                }
                return;
            }

            // Add the feature
            var feature = ScriptableObject.CreateInstance<AetherFogRendererFeature>();
            feature.name = "AetherFog";
            
            // Use reflection to add the feature (rendererFeatures is internal in Unity 6)
            var addMethod = typeof(UniversalRendererData).GetMethod("AddRendererFeature",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (addMethod != null)
            {
                addMethod.Invoke(rendererData, new object[] { feature });
            }
            else
            {
                // Fallback: direct manipulation (less safe but works)
                var featuresField = typeof(UniversalRendererData).GetField("m_RendererFeatures",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (featuresField != null)
                {
                    var featuresList = featuresField.GetValue(rendererData) as System.Collections.Generic.List<ScriptableRendererFeature>;
                    if (featuresList != null)
                    {
                        featuresList.Add(feature);
                    }
                }
            }

            AssetDatabase.AddObjectToAsset(feature, rendererData);
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[AetherFogInstaller] AetherFogRendererFeature installed successfully.");
            
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Aether Fog Installer", 
                    "AetherFogRendererFeature has been added to TartariaURP_Renderer.\n\n" +
                    "The feature will raymarch the Aether voxel field at AfterRenderingTransparents.", 
                    "OK");
            }
        }

        [MenuItem("Tartaria/Rendering/Remove Aether Fog Feature")]
        public static void RemoveAetherFogFeature()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(kRendererAssetPath);
            if (rendererData == null)
            {
                Debug.LogError($"[AetherFogInstaller] Renderer asset not found at {kRendererAssetPath}");
                return;
            }

            var feature = rendererData.rendererFeatures
                .OfType<AetherFogRendererFeature>()
                .FirstOrDefault();

            if (feature == null)
            {
                Debug.Log("[AetherFogInstaller] AetherFogRendererFeature not found (already removed or never installed).");
                return;
            }

            // Remove via reflection
            var featuresField = typeof(UniversalRendererData).GetField("m_RendererFeatures",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (featuresField != null)
            {
                var featuresList = featuresField.GetValue(rendererData) as System.Collections.Generic.List<ScriptableRendererFeature>;
                if (featuresList != null)
                {
                    featuresList.Remove(feature);
                }
            }

            AssetDatabase.RemoveObjectFromAsset(feature);
            Object.DestroyImmediate(feature, true);
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[AetherFogInstaller] AetherFogRendererFeature removed.");
        }
    }
}
