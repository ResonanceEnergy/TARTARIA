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

            // Keep cascade count moderate to avoid runtime atlas pressure in dense scenes.
            SetInt(so, "m_ShadowCascadeCount", 2);

            // Slightly reduced shadow distance for stability/perf in Echohaven.
            SetFloat(so, "m_ShadowDistance", 60f);

            // Disable additional light shadows to prevent punctual shadow atlas downsizing spam.
            SetBool(so, "m_AdditionalLightShadowsSupported", false);

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

            // URP 17 upscaler: Spatial-Temporal Post-Processing (STP).
            // UpscalingFilterSelection enum: Auto=0, Linear=1, Point=2, FSR=3, STP=4.
            SetInt(so, "m_UpscalingFilter", 4);

            // GPU Resident Drawer + GPU occlusion culling.
            // GPUResidentDrawerMode enum: Disabled=0, InstancedDrawing=1.
            SetInt(so, "m_GPUResidentDrawerMode", 1);
            SetBool(so, "m_GPUResidentDrawerEnableOcclusionCullingInCameras", true);

            // APV baseline support for Moon 1 lighting scenario blending.
            // LightProbeSystem enum: LegacyLightProbes=0, ProbeVolumes=1.
            SetInt(so, "m_LightProbeSystem", 1);
            SetBool(so, "m_SupportProbeVolumeGPUStreaming", true);
            SetBool(so, "m_SupportProbeVolumeDiskStreaming", true);
            SetBool(so, "m_SupportProbeVolumeScenarios", true);
            SetBool(so, "m_SupportProbeVolumeScenarioBlending", true);

            // Required by GPU Resident Drawer to avoid BRG variant stripping during player builds.
            // batchRendererGroupShaderStrippingMode is read-only; write m_BrgStripping=2 (KeepAll) directly.
            var gfxSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (gfxSettings != null && gfxSettings.Length > 0 && gfxSettings[0] != null)
            {
                var gfxSo = new SerializedObject(gfxSettings[0]);
                var brgProp = gfxSo.FindProperty("m_BrgStripping");
                if (brgProp != null)
                {
                    brgProp.intValue = 2; // BatchRendererGroupStrippingMode.KeepAll
                    gfxSo.ApplyModifiedProperties();
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(pipelineAsset);

            // Ensure renderer stays on Forward+ (required by GPU Resident Drawer).
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData != null)
            {
                var rendererSo = new SerializedObject(rendererData);
                SetInt(rendererSo, "m_RenderingMode", 2); // Forward+
                rendererSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(rendererData);
            }

            AssetDatabase.SaveAssets();

            Debug.Log("[Tartaria] URP Quality upgraded: Forward+, GPU Resident Drawer + GPU occlusion culling, STP upscaler, APV scenarios/blending, MSAA 4x, soft shadows, 2 cascades, HDR grading, additional shadows disabled.");
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
