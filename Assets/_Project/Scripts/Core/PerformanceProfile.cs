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

        // ─── Phase 3 Round 4: Full Hardware Tier Profiles ──────────────────────
        public enum HardwareTier
        {
            Low = 0,      // GTX 1050 / 4GB / integrated — 30fps target, aggressive culling
            Medium = 1,   // GTX 1070 / 8GB — 60fps balanced (baseline)
            High = 2,     // RTX 3060 / 12GB+ — 60fps high fidelity
            Ultra = 3     // RTX 4070+ / 16GB+ — max settings, minimal culling
        }

        [Header("Hardware Tier (Round 4)")]
        public HardwareTier tier = HardwareTier.Medium;
        public bool autoFallbackEnabled = true;
        public int fallbackCount = 0; // persisted count of quality drops

        /// <summary>
        /// Applies tier-specific overrides to this profile (call at bootstrap / fallback).
        /// </summary>
        public void ApplyTierDefaults(HardwareTier newTier)
        {
            tier = newTier;
            switch (newTier)
            {
                case HardwareTier.Low:
                    targetFrameRate = 30;
                    renderScale = 0.75f;
                    enableFSR = true;
                    shadowCascades = 2;
                    shadowDistance = 60f;
                    lodBias = 0.6f;
                    maxDrawDistance = 250f;
                    maxActiveParticleSystems = 16;
                    aetherGridX = 32; aetherGridY = 32; aetherGridZ = 16;
                    texturesBudget = 1024; meshesBudget = 300;
                    break;
                case HardwareTier.Medium:
                    targetFrameRate = 60;
                    renderScale = 1.0f;
                    enableFSR = true;
                    shadowCascades = 4;
                    shadowDistance = 120f;
                    lodBias = 1.0f;
                    maxDrawDistance = 500f;
                    maxActiveParticleSystems = 48;
                    aetherGridX = 64; aetherGridY = 64; aetherGridZ = 32;
                    texturesBudget = 2048; meshesBudget = 600;
                    break;
                case HardwareTier.High:
                    targetFrameRate = 60;
                    renderScale = 1.0f;
                    enableFSR = false;
                    shadowCascades = 4;
                    shadowDistance = 200f;
                    lodBias = 1.2f;
                    maxDrawDistance = 800f;
                    maxActiveParticleSystems = 96;
                    aetherGridX = 96; aetherGridY = 96; aetherGridZ = 48;
                    texturesBudget = 3072; meshesBudget = 900;
                    break;
                case HardwareTier.Ultra:
                    targetFrameRate = 60;
                    renderScale = 1.0f;
                    enableFSR = false;
                    shadowCascades = 4;
                    shadowDistance = 300f;
                    lodBias = 1.5f;
                    maxDrawDistance = 1200f;
                    maxActiveParticleSystems = 160;
                    aetherGridX = 128; aetherGridY = 128; aetherGridZ = 64;
                    texturesBudget = 4096; meshesBudget = 1200;
                    break;
            }
            Debug.Log($"[PerfProfile] Applied HardwareTier={newTier} (renderScale={renderScale}, particles={maxActiveParticleSystems})");
        }

        public string GetTierSummary() => $"Tier: {tier} | Fallbacks: {fallbackCount} | FPS Target: {targetFrameRate}";
    }
}
