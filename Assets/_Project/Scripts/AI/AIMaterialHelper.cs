using UnityEngine;

namespace Tartaria.AI
{
    /// <summary>
    /// URP-safe material helpers used across Tartaria.AI enemy classes.
    /// Per CLAUDE.md "no stubs" mandate — every method has a real implementation,
    /// not an empty body.
    /// </summary>
    public static class AIMaterialHelper
    {
        public static void SetColor(Renderer r, Color c)
        {
            if (r == null) return;
            var m = Application.isPlaying ? r.material : r.sharedMaterial;
            SetColor(m, c);
        }

        public static void SetColor(Material m, Color c)
        {
            if (m == null) return;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            else m.color = c;
        }

        public static void SetEmission(Material m, Color c, float intensity = 1f)
        {
            if (m == null) return;
            m.EnableKeyword("_EMISSION");
            var emission = c * Mathf.Max(0f, intensity);
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emission);
        }

        public static Material BuildUrpLitMaterial(Color baseColor, Color? emissionColor = null, float emissionIntensity = 1.2f)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return null;
            var mat = new Material(shader);
            SetColor(mat, baseColor);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.30f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);
            if (emissionColor.HasValue) SetEmission(mat, emissionColor.Value, emissionIntensity);
            return mat;
        }
    }
}
