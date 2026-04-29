using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Builds a procedural gradient skybox material (zenith → horizon → ground)
    /// using the URP/Skybox/Cubemap shader fallback to Skybox/Procedural so we
    /// don't need a cubemap asset. Also tunes RenderSettings ambient lighting
    /// and sets the directional Sun light to a warm late-afternoon angle.
    /// </summary>
    public static class SkyboxFactory
    {
        const string MatPath = "Assets/_Project/Materials/M_Skybox_Echohaven.mat";

        public static void BuildAndApply()
        {
            var shader = Shader.Find("Skybox/Procedural");
            if (shader == null)
            {
                Debug.LogWarning("[Tartaria] Skybox/Procedural shader missing — skipping skybox.");
                return;
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
            if (mat == null)
            {
                mat = new Material(shader) { name = "M_Skybox_Echohaven" };
                System.IO.Directory.CreateDirectory("Assets/_Project/Materials");
                AssetDatabase.CreateAsset(mat, MatPath);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            // Procedural skybox params — moody late afternoon with cyan zenith
            mat.SetFloat("_SunSize", 0.04f);
            mat.SetFloat("_SunSizeConvergence", 8f);
            mat.SetFloat("_AtmosphereThickness", 1.15f);
            mat.SetColor("_SkyTint", new Color(0.45f, 0.55f, 0.75f));
            mat.SetColor("_GroundColor", new Color(0.18f, 0.16f, 0.14f));
            mat.SetFloat("_Exposure", 1.15f);

            EditorUtility.SetDirty(mat);
            RenderSettings.skybox = mat;

            // Ambient — trilight tuned to scene
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.35f, 0.42f, 0.55f);
            RenderSettings.ambientEquatorColor = new Color(0.30f, 0.27f, 0.24f);
            RenderSettings.ambientGroundColor = new Color(0.10f, 0.09f, 0.08f);
            RenderSettings.ambientIntensity = 1.0f;
            RenderSettings.reflectionIntensity = 0.6f;

            // Fog — gives depth + masks the terrain edge
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.40f, 0.45f, 0.55f);
            RenderSettings.fogDensity = 0.0045f;

            // Position the sun (directional Light) at warm late-afternoon angle
            var sun = FindBestDirectionalLight();
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(35f, -55f, 0f);
                sun.color = new Color(1.00f, 0.92f, 0.78f);
                sun.intensity = 1.35f;
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = 0.85f;
                RenderSettings.sun = sun;
            }

            DynamicGI.UpdateEnvironment();
            Debug.Log("[Tartaria] Procedural skybox + ambient + fog applied.");
        }

        static Light FindBestDirectionalLight()
        {
#if UNITY_2023_1_OR_NEWER
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
            var lights = Object.FindObjectsOfType<Light>();
#endif
            Light best = null;
            foreach (var l in lights)
            {
                if (l.type != LightType.Directional) continue;
                if (best == null || l.intensity > best.intensity) best = l;
            }
            return best;
        }
    }
}
