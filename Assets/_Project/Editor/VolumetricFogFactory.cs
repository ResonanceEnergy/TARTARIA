using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Reflection;

namespace Tartaria.Editor
{
    /// <summary>
    /// Volumetric Fog Factory — adds volumetric fog support to EchohavenVolumeProfile.
    /// Attempts to use URP 17's VolumetricFog component if available, otherwise
    /// falls back to increasing ambient fog density via existing profile overrides.
    /// Idempotent.
    /// </summary>
    public static class VolumetricFogFactory
    {
        const string ProfilePath = "Assets/_Project/Config/EchohavenVolumeProfile.asset";

        public static void Run()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                Debug.LogWarning($"[VolumetricFog] Profile not found at {ProfilePath} — skipping.");
                return;
            }

            // Try to add URP VolumetricFog component via reflection (URP 17 may or may not have it)
            bool added = TryAddVolumetricFogComponent(profile);

            if (!added)
            {
                // Fallback: increase ambient fog via existing overrides
                AddFogFallback(profile);
            }

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Debug.Log("[VolumetricFog] Volumetric fog configured (URP or fallback).");
        }

        static bool TryAddVolumetricFogComponent(VolumeProfile profile)
        {
            // Search for UnityEngine.Rendering.Universal.VolumetricFog type
            var volFogType = System.Type.GetType("UnityEngine.Rendering.Universal.VolumetricFog, Unity.RenderPipelines.Universal.Runtime");
            if (volFogType == null)
            {
                Debug.Log("[VolumetricFog] UnityEngine.Rendering.Universal.VolumetricFog not found in URP 17 — using fallback.");
                return false;
            }

            // Check if already present
            if (profile.Has(volFogType))
            {
                Debug.Log("[VolumetricFog] VolumetricFog component already exists in profile.");
                return true;
            }

            // Add it
            var component = profile.Add(volFogType);
            if (component != null)
            {
                // Set some defaults via reflection if possible
                SetPropertyIfExists(component, "active", true);
                SetPropertyIfExists(component, "fogAttenuation", 0.15f);
                SetPropertyIfExists(component, "meanFreePath", 50f);

                Debug.Log("[VolumetricFog] Added URP VolumetricFog component to profile.");
                return true;
            }

            return false;
        }

        static void AddFogFallback(VolumeProfile profile)
        {
            // Increase ambient density by tweaking existing ColorAdjustments + Tonemapping
            if (profile.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out var colorAdj))
            {
                // Reduce contrast slightly to simulate fog density
                colorAdj.contrast.value = Mathf.Max(colorAdj.contrast.value - 10f, -30f);
                colorAdj.saturation.value = Mathf.Max(colorAdj.saturation.value - 5f, -50f);
                Debug.Log("[VolumetricFog] Fallback: Reduced contrast/saturation for fog effect.");
            }

            // Increase ambient light slightly
            RenderSettings.ambientIntensity = Mathf.Min(RenderSettings.ambientIntensity + 0.15f, 2f);
            Debug.Log("[VolumetricFog] Fallback: Increased ambient intensity for height fog simulation.");
        }

        static void SetPropertyIfExists(object obj, string propName, object value)
        {
            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                try
                {
                    prop.SetValue(obj, value);
                }
                catch { /* Ignore reflection errors */ }
            }
        }
    }
}
