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
                rendererData.renderingMode = RenderingMode.ForwardPlus;
                rendererData.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(
                    "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
                AssetDatabase.CreateAsset(rendererData, RendererDataPath);
                Debug.Log($"[Tartaria] Created URP Renderer (Forward+): {RendererDataPath}");
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

        /// <summary>
        /// Upgrades URP pipeline settings for better visual quality:
        /// MSAA 4x, soft shadows, 4 shadow cascades, HDR color grading,
        /// additional light shadows, and assigns the Echohaven volume profile.
        /// Called from OneClickBuild or standalone via menu.
        /// </summary>
        [MenuItem("Tartaria/Setup/Upgrade URP Quality", false, 51)]
        public static void UpgradeURPQuality()
        {
            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipelineAsset == null)
            {
                Debug.LogWarning("[Tartaria] No URP pipeline asset found — run Setup URP Pipeline first.");
                return;
            }

            var so = new SerializedObject(pipelineAsset);

            // MSAA 4x (1=off, 2=2x, 4=4x, 8=8x)
            SetInt(so, "m_MSAA", 4);

            // Soft shadows
            SetBool(so, "m_SoftShadowsSupported", true);

            // 4 shadow cascades for smoother shadow transitions
            SetInt(so, "m_ShadowCascadeCount", 4);

            // Shadow distance — increase for open world
            SetFloat(so, "m_ShadowDistance", 80f);

            // Additional light shadows (point/spot lights cast shadows)
            SetBool(so, "m_AdditionalLightShadowsSupported", true);

            // HDR color grading for richer tonemapping
            SetInt(so, "m_ColorGradingMode", 1); // 0=LDR, 1=HDR

            // Assign Echohaven volume profile if it exists
            var volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(
                "Assets/_Project/Config/EchohavenVolumeProfile.asset");
            if (volumeProfile != null)
            {
                var vpProp = so.FindProperty("m_VolumeProfile");
                if (vpProp != null) vpProp.objectReferenceValue = volumeProfile;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(pipelineAsset);
            AssetDatabase.SaveAssets();

            Debug.Log("[Tartaria] URP Quality upgraded: MSAA 4x, soft shadows, 4 cascades, HDR grading, additional shadows.");
        }

        static void SetInt(SerializedObject so, string prop, int value)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.intValue = value;
        }

        static void SetFloat(SerializedObject so, string prop, float value)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.floatValue = value;
        }

        static void SetBool(SerializedObject so, string prop, bool value)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.boolValue = value;
        }
    }
}
