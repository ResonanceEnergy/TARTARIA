using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Performance profile ScriptableObject — per-quality-tier settings.
    /// Recommended: RTX 3060 → 60 FPS @ 1080p–1440p
    /// Minimum:     GTX 1070 → 30 FPS @ 720p–1080p
    /// </summary>
    [CreateAssetMenu(fileName = "PerformanceProfile", menuName = "Tartaria/Performance Profile")]
    public class PerformanceProfile : ScriptableObject
    {
        [Header("Rendering")]
        public int targetFrameRate = 60;
        public float renderScale = 1.0f;
        public bool enableFSR = true;
        public bool enableDLSS = false;
        public int shadowCascades = 4;
        public float shadowDistance = 100f;

        [Header("Aether Field")]
        public int aetherGridX = 64;
        public int aetherGridY = 64;
        public int aetherGridZ = 32;
        public float aetherCellSize = 2.0f;
        public bool aetherGPUCompute = true;

        [Header("Memory Budget (MB)")]
        public int texturesBudget = 2048;
        public int meshesBudget = 600;
        public int audioBudget = 200;
        public int ecsBudget = 400;
        public int uiBudget = 200;

        [Header("LOD & Culling")]
        public float lodBias = 1.0f;
        public float maxDrawDistance = 500f;
        public int maxActiveParticleSystems = 32;

        [Header("Frame Budget (ms)")]
        public float renderBudget = 8.0f;
        public float aetherBudget = 2.0f;
        public float physicsBudget = 1.5f;
        public float aiBudget = 1.0f;
        public float audioBudgetMs = 0.5f;
        public float inputBudget = 0.3f;
        public float uiFrameBudget = 0.5f;

        public int TotalMemoryBudgetMB =>
            texturesBudget + meshesBudget + audioBudget + ecsBudget + uiBudget + 600; // +overhead
    }
}
