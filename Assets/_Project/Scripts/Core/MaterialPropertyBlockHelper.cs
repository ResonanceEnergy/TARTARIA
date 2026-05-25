using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Core
{
    /// <summary>
    /// AGENT 6: MaterialPropertyBlock helper to optimize per-renderer property changes.
    /// 
    /// Why MaterialPropertyBlock?
    /// - Avoids creating unique material instances (saves memory)
    /// - Preserves GPU instancing (critical for batching)
    /// - Zero GC allocations for property changes
    /// 
    /// Without MPB: Each Renderer.material creates a clone (memory leak + breaks instancing)
    /// With MPB: Property changes applied without breaking shared materials
    /// 
    /// Performance impact:
    /// - 100 objects with unique materials: 100 draw calls
    /// - 100 objects with MPB color variations: 1-2 draw calls (instanced)
    /// </summary>
    public static class MaterialPropertyBlockHelper
    {
        // Shared MPB instance to eliminate allocations
        static readonly MaterialPropertyBlock _sharedBlock = new MaterialPropertyBlock();

        // Property name IDs (cached for performance)
        static readonly int _colorID = Shader.PropertyToID("_BaseColor");
        static readonly int _emissionID = Shader.PropertyToID("_EmissionColor");
        static readonly int _metallicID = Shader.PropertyToID("_Metallic");
        static readonly int _smoothnessID = Shader.PropertyToID("_Smoothness");
        static readonly int _mainTexID = Shader.PropertyToID("_BaseMap");

        /// <summary>
        /// Set color on a renderer using MaterialPropertyBlock.
        /// Preserves GPU instancing and avoids material cloning.
        /// </summary>
        public static void SetColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;

            renderer.GetPropertyBlock(_sharedBlock);
            _sharedBlock.SetColor(_colorID, color);
            renderer.SetPropertyBlock(_sharedBlock);
        }

        /// <summary>
        /// Set emission color on a renderer using MaterialPropertyBlock.
        /// </summary>
        public static void SetEmissionColor(Renderer renderer, Color emissionColor)
        {
            if (renderer == null) return;

            renderer.GetPropertyBlock(_sharedBlock);
            _sharedBlock.SetColor(_emissionID, emissionColor);
            renderer.SetPropertyBlock(_sharedBlock);
        }

        /// <summary>
        /// Set multiple properties at once to minimize MPB updates.
        /// </summary>
        public static void SetProperties(Renderer renderer, Color? color = null, 
            Color? emission = null, float? metallic = null, float? smoothness = null)
        {
            if (renderer == null) return;

            renderer.GetPropertyBlock(_sharedBlock);

            if (color.HasValue)
                _sharedBlock.SetColor(_colorID, color.Value);

            if (emission.HasValue)
                _sharedBlock.SetColor(_emissionID, emission.Value);

            if (metallic.HasValue)
                _sharedBlock.SetFloat(_metallicID, metallic.Value);

            if (smoothness.HasValue)
                _sharedBlock.SetFloat(_smoothnessID, smoothness.Value);

            renderer.SetPropertyBlock(_sharedBlock);
        }

        /// <summary>
        /// Clear all MaterialPropertyBlock overrides on a renderer.
        /// Restores original material properties.
        /// </summary>
        public static void ClearProperties(Renderer renderer)
        {
            if (renderer == null) return;
            renderer.SetPropertyBlock(null);
        }

        /// <summary>
        /// Set color on multiple renderers efficiently.
        /// </summary>
        public static void SetColorBatch(Renderer[] renderers, Color color)
        {
            if (renderers == null || renderers.Length == 0) return;

            _sharedBlock.Clear();
            _sharedBlock.SetColor(_colorID, color);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].SetPropertyBlock(_sharedBlock);
            }
        }

        /// <summary>
        /// Animate color over time using MaterialPropertyBlock.
        /// Call from Update/FixedUpdate for smooth color transitions.
        /// </summary>
        public static void AnimateColor(Renderer renderer, Color from, Color to, float t)
        {
            if (renderer == null) return;

            Color lerpedColor = Color.Lerp(from, to, t);
            renderer.GetPropertyBlock(_sharedBlock);
            _sharedBlock.SetColor(_colorID, lerpedColor);
            renderer.SetPropertyBlock(_sharedBlock);
        }

        /// <summary>
        /// Set texture on a renderer using MaterialPropertyBlock.
        /// Useful for sprite variations without creating material clones.
        /// </summary>
        public static void SetTexture(Renderer renderer, Texture texture)
        {
            if (renderer == null || texture == null) return;

            renderer.GetPropertyBlock(_sharedBlock);
            _sharedBlock.SetTexture(_mainTexID, texture);
            renderer.SetPropertyBlock(_sharedBlock);
        }

        /// <summary>
        /// GPU Instancing validation helper.
        /// Checks if a material supports GPU instancing.
        /// </summary>
        public static bool SupportsInstancing(Material material)
        {
            if (material == null) return false;
            return material.enableInstancing;
        }

        /// <summary>
        /// Enable GPU instancing on a material.
        /// Required for MaterialPropertyBlock batching benefits.
        /// </summary>
        public static void EnableInstancing(Material material)
        {
            if (material == null) return;
            material.enableInstancing = true;
        }

        /// <summary>
        /// Enable GPU instancing on all materials in a renderer.
        /// </summary>
        public static void EnableInstancingOnRenderer(Renderer renderer)
        {
            if (renderer == null) return;

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                    materials[i].enableInstancing = true;
            }
        }
    }
}
