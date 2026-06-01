using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.AI
{
    /// <summary>
    /// MaterialBank — pooled color-setter for renderers that frequently swap colors
    /// (damage flashes, state tints, freeze stuns). Avoids per-frame material
    /// instantiation by routing every color change through a shared
    /// <see cref="MaterialPropertyBlock"/>.
    ///
    /// Background: Unity's <c>Renderer.material</c> getter clones the shared
    /// material on first access. A boss arena with 5+ golems each flashing on
    /// every hit produced dozens of dangling material instances per minute
    /// (visible as "Suboptimal memory type used for buffer" log spam in URP).
    ///
    /// Usage:
    ///   <code>
    ///   // hot path (replaces r.material.color = c or r.material.SetColor("_BaseColor", c))
    ///   MaterialBank.ApplyColor(_renderer, _damageFlashColor);
    ///   </code>
    /// 
    /// Property block is stateless beyond the color so a single shared instance is
    /// reused — no per-renderer pool needed. URP/Lit prefers <c>_BaseColor</c>;
    /// fallback to the built-in <c>_Color</c> for legacy shaders.
    /// </summary>
    public static class MaterialBank
    {
        // Single reusable property block for the whole game. Safe because we
        // overwrite all relevant keys on every Apply call before pushing.
        static readonly MaterialPropertyBlock s_block = new MaterialPropertyBlock();
        static readonly int s_baseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int s_legacyColorId = Shader.PropertyToID("_Color");

        // Track which renderers we've ever touched so reset() can flush them all.
        static readonly HashSet<Renderer> s_tracked = new HashSet<Renderer>();

        /// <summary>
        /// Tint a renderer to <paramref name="c"/> without instantiating its material.
        /// Safe to call every frame.
        /// </summary>
        public static void ApplyColor(Renderer r, Color c)
        {
            if (r == null) return;
            // Pull the existing block so we don't clobber unrelated keys other
            // systems may have stashed.
            r.GetPropertyBlock(s_block);
            s_block.SetColor(s_baseColorId, c);
            s_block.SetColor(s_legacyColorId, c);
            r.SetPropertyBlock(s_block);
            s_tracked.Add(r);
        }

        /// <summary>
        /// Clear any property-block tint we applied to <paramref name="r"/>, falling
        /// back to the shared material's authored color. Call when an entity dies
        /// or returns to a pool.
        /// </summary>
        public static void ClearColor(Renderer r)
        {
            if (r == null) return;
            r.GetPropertyBlock(s_block);
            // Setting the block to default is the cheapest way to drop our override.
            r.SetPropertyBlock(null);
            s_tracked.Remove(r);
        }

        /// <summary>
        /// Flush every renderer we've tinted. Use sparingly — currently only the
        /// scene-unload bootstrap path needs this.
        /// </summary>
        public static void ResetAll()
        {
            foreach (var r in s_tracked)
            {
                if (r != null) r.SetPropertyBlock(null);
            }
            s_tracked.Clear();
        }
    }
}
