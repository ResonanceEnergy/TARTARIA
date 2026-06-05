// File: Assets/_Project/Scripts/AI/AIMaterialHelper.cs
using UnityEngine;

namespace Tartaria.AI
{
    public static class AIMaterialHelper
    {
        /// <summary>
        /// URP-safe color set on a Renderer's material. Falls back to .color
        /// if the material's shader doesn't have _BaseColor (Built-in Standard).
        /// Safe on null renderer or null material.
        /// </summary>
        public static void SetColor(Renderer r, Color c) { /* ... */ }

        /// <summary>
        /// URP-safe color set on a Material directly.
        /// </summary>
        public static void SetColor(Material m, Color c) { /* ... */ }

        /// <summary>
        /// URP-safe emission setup. Enables _EMISSION keyword and sets the color.
        /// </summary>
        public static void SetEmission(Material m, Color c, float intensity = 1f) { /* ... */ }

        /// <summary>
        /// Build a URP/Lit material with given base + optional emission. Falls
        /// back to Standard if URP shader not found. Returns null only if both
        /// shaders are missing (Unity in a very broken state).
        /// </summary>
        public static Material BuildUrpLitMaterial(Color baseColor, Color? emissionColor = null, float emissionIntensity = 1.2f) { /* ... */ }
    }
}
