using UnityEngine;

namespace Tartaria.Core.Data
{
    /// <summary>
    /// Quality-tiered material variant. Lets the renderer pick lower-cost
    /// shaders on weak hardware without touching scene assignments.
    /// QualityApplicator reads this and patches Renderer.sharedMaterial at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Visual/Material Variant Set", fileName = "MV_Material")]
    public class MaterialVariantSet : ScriptableObject
    {
        public string variantId = "stone";

        [Tooltip("Used on Quality 0 (Performance / Very Low / Low). URP/Simple Lit recommended.")]
        public Material lowQuality;

        [Tooltip("Used on Quality 1-2 (Balanced / Medium). URP/Lit baseline.")]
        public Material standard;

        [Tooltip("Used on Quality 3+ (High / Ultra). May enable detail maps, parallax, custom shaders.")]
        public Material highQuality;

        public Material Pick(int qualityLevel)
        {
            if (qualityLevel <= 0 && lowQuality != null) return lowQuality;
            if (qualityLevel >= 3 && highQuality != null) return highQuality;
            return standard != null ? standard : (highQuality ?? lowQuality);
        }
    }
}
