using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Editor
{
    /// <summary>
    /// Creates and assigns a URP Pipeline Asset if none exists.
    /// Called from OneClickBuild Phase 1 (Directories) or standalone via menu.
    /// </summary>
    public static class URPSetup
    {
        const string PipelineAssetPath = "Assets/_Project/Config/TartariaURP.asset";
        const string RendererDataPath  = "Assets/_Project/Config/TartariaURP_Renderer.asset";

        [MenuItem("Tartaria/Setup URP Pipeline", false, 50)]
        public static void EnsureURPPipeline()
        {
            // Check if already assigned
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                Debug.Log("[Tartaria] URP Pipeline already assigned — skipping.");
                return;
            }

            EnsureDirectory("Assets/_Project/Config");

            // Create renderer data first
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RendererDataPath);
                Debug.Log($"[Tartaria] Created URP Renderer: {RendererDataPath}");
            }

            // Create pipeline asset
            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipelineAsset == null)
            {
                // Create() allocates internal renderer list and reloads shader resources
                pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
                Debug.Log($"[Tartaria] Created URP Pipeline Asset: {PipelineAssetPath}");
            }

            // Assign to Graphics Settings
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            Debug.Log("[Tartaria] Assigned URP to GraphicsSettings.defaultRenderPipeline");

            // Assign to all Quality levels (must switch active level to set each one)
            int currentLevel = QualitySettings.GetQualityLevel();
            int levelCount = QualitySettings.names.Length;
            for (int i = 0; i < levelCount; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipelineAsset;
            }
            QualitySettings.SetQualityLevel(currentLevel, false);
            Debug.Log($"[Tartaria] Assigned URP to QualitySettings ({levelCount} levels)");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Tartaria] URP Pipeline setup complete.");
        }

        static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
