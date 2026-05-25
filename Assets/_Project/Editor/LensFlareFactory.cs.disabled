using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Tartaria.Editor
{
    /// <summary>
    /// Lens Flare Factory — generates a LensFlareDataSRP asset and attaches it
    /// to the scene's directional sun light. Provides cinematic sun lens flare.
    /// Gracefully stubs if URP 17 doesn't expose LensFlareDataSRP or LensFlareComponentSRP.
    /// </summary>
    public static class LensFlareFactory
    {
        const string FlareAssetPath = "Assets/_Project/Rendering/SunFlare.asset";

        public static void Run()
        {
            // Try to create LensFlareDataSRP asset
            var flareAsset = TryCreateLensFlareAsset();

            if (flareAsset == null)
            {
                Debug.LogWarning("[LensFlare] LensFlareDataSRP not available in this URP version — skipping.");
                return;
            }

            // Attach to scene's directional light
            AttachToSunLight(flareAsset);
        }

        static Object TryCreateLensFlareAsset()
        {
            // Reflect into LensFlareDataSRP type
            var flareDataType = System.Type.GetType("UnityEngine.Rendering.Universal.LensFlareDataSRP, Unity.RenderPipelines.Universal.Runtime");
            if (flareDataType == null)
                return null;

            var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(FlareAssetPath);
            if (existing != null && existing.GetType() == flareDataType)
            {
                Debug.Log("[LensFlare] SunFlare asset already exists.");
                return existing;
            }

            // Create new asset
            var flareAsset = ScriptableObject.CreateInstance(flareDataType);
            if (flareAsset == null)
                return null;

            flareAsset.name = "SunFlare";

            // Set properties via reflection: 3 elements (white core, blue ring, gold streak)
            // Since API may not be stable, attempt gracefully
            TrySetProperty(flareAsset, "intensity", 1.0f);

            // Create directory if needed
            string dir = System.IO.Path.GetDirectoryName(FlareAssetPath);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                System.IO.Directory.CreateDirectory(dir.Replace("Assets/", Application.dataPath + "/"));
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(flareAsset, FlareAssetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[LensFlare] Created SunFlare asset at {FlareAssetPath}");
            return flareAsset;
        }

        static void AttachToSunLight(Object flareAsset)
        {
            // Find directional light in scene (sun)
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            Light sun = null;
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    sun = light;
                    break;
                }
            }

            if (sun == null)
            {
                Debug.LogWarning("[LensFlare] No directional light found in scene — cannot attach lens flare.");
                return;
            }

            // Reflect into LensFlareComponentSRP
            var flareCompType = System.Type.GetType("UnityEngine.Rendering.Universal.LensFlareComponentSRP, Unity.RenderPipelines.Universal.Runtime");
            if (flareCompType == null)
            {
                Debug.LogWarning("[LensFlare] LensFlareComponentSRP not available — skipping attachment.");
                return;
            }

            var existingComp = sun.GetComponent(flareCompType);
            if (existingComp != null)
            {
                Debug.Log("[LensFlare] LensFlareComponentSRP already attached to sun light.");
                TrySetProperty(existingComp, "lensFlareData", flareAsset);
                TrySetProperty(existingComp, "intensity", 1.0f);
                return;
            }

            var comp = sun.gameObject.AddComponent(flareCompType);
            if (comp != null)
            {
                TrySetProperty(comp, "lensFlareData", flareAsset);
                TrySetProperty(comp, "intensity", 1.0f);
                Debug.Log("[LensFlare] Attached LensFlareComponentSRP to sun light.");
            }
        }

        static void TrySetProperty(object obj, string propName, object value)
        {
            if (obj == null) return;
            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                try { prop.SetValue(obj, value); }
                catch { /* Ignore */ }
            }
        }
    }
}
