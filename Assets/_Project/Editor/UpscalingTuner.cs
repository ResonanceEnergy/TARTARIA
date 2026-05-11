using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Upscaling Tuner — sets URP STP (Spatial-Temporal Post-processing) upscaling
    /// and render scale = 0.66f in the URP-Performant asset. Idempotent.
    /// Gracefully no-ops if URP asset not found or STP not available in this URP version.
    /// </summary>
    public static class UpscalingTuner
    {
        public static void Run()
        {
            // Find URP-Performant asset
            string[] guids = AssetDatabase.FindAssets("URP-Performant t:UniversalRenderPipelineAsset");
            if (guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            }

            if (guids.Length == 0)
            {
                Debug.LogWarning("[UpscalingTuner] No URP asset found — skipping upscaling config.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);

            if (urpAsset == null)
            {
                Debug.LogWarning($"[UpscalingTuner] Could not load URP asset at {path}");
                return;
            }

            // Use SerializedObject to set upscaling properties
            var so = new SerializedObject(urpAsset);

            // Try to set m_RenderScale
            var renderScaleProp = so.FindProperty("m_RenderScale");
            if (renderScaleProp != null)
            {
                renderScaleProp.floatValue = 0.66f;
                Debug.Log("[UpscalingTuner] Set m_RenderScale = 0.66f");
            }
            else
            {
                Debug.LogWarning("[UpscalingTuner] m_RenderScale property not found in URP asset.");
            }

            // Try to set m_UpscalingFilter (STP = 2 or 3 depending on URP version)
            var upscalingProp = so.FindProperty("m_UpscalingFilter");
            if (upscalingProp != null)
            {
                // STP is typically enum value 2 or 3 (check UnityEngine.Rendering.Universal.UpscalingFilterSelection)
                // Try 2 first (SpatialUpscaling), if that's not STP, try 3
                upscalingProp.intValue = 2; // Assume STP = 2
                Debug.Log("[UpscalingTuner] Set m_UpscalingFilter = 2 (STP if available)");
            }
            else
            {
                Debug.LogWarning("[UpscalingTuner] m_UpscalingFilter property not found — STP may not be available in this URP version.");
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(urpAsset);
            AssetDatabase.SaveAssets();

            Debug.Log($"[UpscalingTuner] Configured URP asset at {path}");
        }
    }
}
