using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Consequence Visuals — applies visual changes to the world based on
    /// World Choice W1-W6 decisions. Each choice modifies zone atmosphere,
    /// material palettes, and ambient particles.
    ///
    /// Uses RenderSettings + material property blocks to avoid instantiating
    /// new materials per-choice.
    ///
    /// Cross-ref: docs/12_VIVID_VISUALS.md, docs/22_DIALOGUE_BRANCHING.md
    /// </summary>
    [DisallowMultipleComponent]
    public class ConsequenceVisuals : MonoBehaviour
    {
        public static ConsequenceVisuals Instance { get; private set; }

        [Header("Palette Overrides")]
        [SerializeField] Color militarizedTint = new(0.7f, 0.5f, 0.4f, 1f);
        [SerializeField] Color sanctifiedTint = new(0.85f, 0.9f, 1f, 1f);
        [SerializeField] Color openGatesTint = new(0.95f, 0.85f, 0.5f, 1f);
        [SerializeField] Color sealedCityTint = new(0.4f, 0.5f, 0.7f, 1f);
        [SerializeField] Color allianceTint = new(0.8f, 0.65f, 0.9f, 1f);
        [SerializeField] Color independenceTint = new(0.6f, 0.85f, 0.65f, 1f);
        [SerializeField] Color sacrificeGlow = new(0.95f, 0.6f, 0.3f, 1f);
        [SerializeField] Color mercyGlow = new(0.7f, 0.9f, 0.95f, 1f);
        [SerializeField] Color redemptionGlow = new(0.9f, 0.95f, 0.8f, 1f);
        [SerializeField] Color condemnationGlow = new(0.6f, 0.3f, 0.3f, 1f);
        [SerializeField] Color restorationFinal = new(0.95f, 0.82f, 0.35f, 1f);
        [SerializeField] Color transcendenceFinal = new(0.6f, 0.8f, 1f, 1f);

        [Header("Atmosphere")]
        [SerializeField] float allianceFogDensity = 0.01f;
        [SerializeField] float independenceFogDensity = 0.007f;
        [SerializeField] float militarizedFogDensity = 0.015f;
        [SerializeField] float sanctifiedFogDensity = 0.005f;
        [SerializeField] float openGatesFogDensity = 0.008f;
        [SerializeField] float sealedFogDensity = 0.02f;

        // Current applied state
        Color _currentTintOverride = Color.white;
        float _currentFogOverride;
        bool _hasOverride;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Apply any choices already made on load
            var wc = WorldChoiceTracker.Instance;
            if (wc == null) return;

            wc.OnChoiceMade += OnWorldChoiceChanged;

            foreach (var def in wc.Definitions)
            {
                var opt = wc.GetChoice(def.id);
                if (opt != WorldChoiceTracker.ChoiceOption.NotChosen)
                    OnWorldChoiceChanged(def.id, opt);
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (WorldChoiceTracker.Instance != null)
                WorldChoiceTracker.Instance.OnChoiceMade -= OnWorldChoiceChanged;
        }

        /// <summary>
        /// Called by WorldChoiceTracker when a choice is made or loaded.
        /// Applies the visual consequence.
        /// </summary>
        public void OnWorldChoiceChanged(WorldChoiceTracker.WorldChoiceId id,
            WorldChoiceTracker.ChoiceOption option)
        {
            switch (id)
            {
                case WorldChoiceTracker.WorldChoiceId.W1_CassiansOffer:
                    ApplyZonePalette(
                        option == WorldChoiceTracker.ChoiceOption.OptionA
                            ? allianceTint : independenceTint,
                        option == WorldChoiceTracker.ChoiceOption.OptionA
                            ? allianceFogDensity : independenceFogDensity);
                    break;

                case WorldChoiceTracker.WorldChoiceId.W2_StarFort:
                    ApplyZonePalette(
                        option == WorldChoiceTracker.ChoiceOption.OptionA
                            ? militarizedTint : sanctifiedTint,
                        option == WorldChoiceTracker.ChoiceOption.OptionA
                            ? militarizedFogDensity : sanctifiedFogDensity);
                    break;

                case WorldChoiceTracker.WorldChoiceId.W4_AuroraCity:
                    ApplyZonePalette(
                        option == WorldChoiceTracker.ChoiceOption.OptionA
                            ? openGatesTint : sealedCityTint,
                        option == WorldChoiceTracker.ChoiceOption.OptionA
                            ? openGatesFogDensity : sealedFogDensity);
                    break;

                case WorldChoiceTracker.WorldChoiceId.W3_KorathSacrifice:
                    ApplyAmbientGlow(
                        option == WorldChoiceTracker.ChoiceOption.OptionA
                            ? sacrificeGlow : mercyGlow);
                    break;

                case WorldChoiceTracker.WorldChoiceId.W5_ZerethPlea:
                    ApplyAmbientGlow(
                        option == WorldChoiceTracker.ChoiceOption.OptionA
                            ? redemptionGlow : condemnationGlow);
                    break;

                case WorldChoiceTracker.WorldChoiceId.W6_FinalAlignment:
                    ApplyFinalPalette(
                        option == WorldChoiceTracker.ChoiceOption.OptionA
                            ? restorationFinal : transcendenceFinal);
                    break;
            }

            Debug.Log($"[ConsequenceVisuals] Applied visual for {id} = {option}");
        }

        // ─── Visual Application ──────────────────────

        void ApplyZonePalette(Color tint, float fogDensity)
        {
            _currentTintOverride = tint;
            _currentFogOverride = fogDensity;
            _hasOverride = true;

            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, tint, 0.4f);
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, tint, 0.3f);
        }

        void ApplyAmbientGlow(Color glow)
        {
            _currentTintOverride = glow;
            _hasOverride = true;

            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, glow, 0.5f);
        }

        void ApplyFinalPalette(Color palette)
        {
            _currentTintOverride = palette;
            _hasOverride = true;

            RenderSettings.fogColor = palette;
            RenderSettings.ambientLight = palette;
            RenderSettings.ambientSkyColor = palette;
        }

        // ─── Queries ─────────────────────────────────

        public bool HasOverride => _hasOverride;
        public Color CurrentTint => _currentTintOverride;
    }
}
