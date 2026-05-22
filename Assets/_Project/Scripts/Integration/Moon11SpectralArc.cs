using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Save;
using Tartaria.UI;

namespace Tartaria.Integration
{
    // ============================================================
    // MOON 11: SPECTRAL MOON — "The Liberation of Releasing"
    // Galactic Tone: Liberation / Release
    // Scene prefix: "SpectralMoon" | "AquiferSanctum" | "FountainRing"
    // 5-Beat: Discovery → Restoration → Conflict → Climax → Revelation
    // Key: Ancient aquifer excavation; planetary fountain chain activation;
    //      continent-wide aurora veils; Lirael becomes semi-solid; prophecy stones 10-11
    // ============================================================

    public class Moon11SpectralArc : MonoBehaviour
    {
        public static Moon11SpectralArc Instance { get; private set; }

        private const int MOON_NUM = 11;

        private const int BEAT_DISCOVERY   = 0;
        private const int BEAT_RESTORATION = 1;
        private const int BEAT_CONFLICT    = 2;
        private const int BEAT_CLIMAX      = 3;
        private const int BEAT_REVELATION  = 4;

        [Header("Aquifer")]
        [SerializeField] private Transform   _aquiferExcavationZone;
        [SerializeField] private GameObject  _aquiferRevealFX;
        [SerializeField] private GameObject  _pureWaterFlowFX;

        [Header("Fountain Chain")]
        [SerializeField] private GameObject[] _fountainChainFX;      // one per zone
        [SerializeField] private GameObject   _planetaryAuroraVeilFX;

        [Header("Lirael")]
        [SerializeField] private GameObject _liraelSemiSolidProxy;

        private bool _aquiferFound;
        private int  _fountainsActivated;   // 0 to _fountainChainFX.Length
        private bool _sludgeTendrilsDefeated;
        private bool _auroraVeilAppeared;
        private bool _liraelSemiSolid;
        private bool _moonCleared;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!scene.StartsWith("SpectralMoon") &&
                !scene.StartsWith("AquiferSanctum") &&
                !scene.StartsWith("FountainRing"))
                return;

            var go = new GameObject("Moon11SpectralArc");
            go.AddComponent<Moon11SpectralArc>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            RestoreStateFromSave();
            StartCoroutine(RunArc());
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void RestoreStateFromSave()
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) return;

            _aquiferFound            = save.GetMoonFlag(MOON_NUM, "aquifer_found");
            _fountainsActivated      = save.GetMoonFlag(MOON_NUM, "fountains_activated_int", 0);
            _sludgeTendrilsDefeated  = save.GetMoonFlag(MOON_NUM, "sludge_tendrils_defeated");
            _auroraVeilAppeared      = save.GetMoonFlag(MOON_NUM, "aurora_veil_appeared");
            _liraelSemiSolid         = save.GetMoonFlag(MOON_NUM, "lirael_semi_solid");
            _moonCleared             = save.GetMoonFlag(MOON_NUM, "moon_cleared");

            if (_moonCleared) ApplyPersistentWorldState();
        }

        private void ApplyPersistentWorldState()
        {
            if (_liraelSemiSolid && _liraelSemiSolidProxy != null)
                _liraelSemiSolidProxy.SetActive(true);

            if (_pureWaterFlowFX != null) _pureWaterFlowFX.SetActive(true);

            for (int i = 0; i < _fountainsActivated && _fountainChainFX != null && i < _fountainChainFX.Length; i++)
                if (_fountainChainFX[i] != null) _fountainChainFX[i].SetActive(true);

            if (_moonCleared)
                HUDController.Instance?.ShowObjective(
                    "<b>SPECTRAL MOON — THE AQUIFER SINGS</b>\n" +
                    "Planetary fountains active. Lirael walks more solidly now. The world heals.");
        }

        private IEnumerator RunArc()
        {
            if (_moonCleared) yield break;
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) yield break;

            if (!_aquiferFound)                                         yield return StartCoroutine(Beat1_Discovery());
            if (_fountainsActivated < GetTotalFountains())             yield return StartCoroutine(Beat2_Restoration());
            if (!_sludgeTendrilsDefeated)                              yield return StartCoroutine(Beat3_Conflict());
            if (!_auroraVeilAppeared)                                  yield return StartCoroutine(Beat4_Climax());
            if (!_liraelSemiSolid)                                     yield return StartCoroutine(Beat5_Revelation());
        }

        private int GetTotalFountains() =>
            (_fountainChainFX != null) ? _fountainChainFX.Length : 8;

        // ─── Beat 1: Discovery ─────────────────────────────────────
        private IEnumerator Beat1_Discovery()
        {
            HUDController.Instance?.ShowBanner(
                "SPECTRAL MOON — DISCOVERY",
                "Beneath the oldest star fort lies an ancient aquifer — the source of all Tartarian pure water. " +
                "Corrupted by centuries of Mud Flood sludge.",
                9f);
            HUDController.Instance?.ShowObjective(
                "Excavate the ancient aquifer beneath the star fort.");

            HUDController.Instance?.ShowBanner(
                "LIRAEL (sensing the water)",
                "\"The water remembers what it tasted like before the mud. It wants to come home.\"",
                8f);
            AudioManager.Instance?.PlaySFX2D("lirael_aquifer_vo");
            AudioManager.Instance?.PlaySFX2D("aquifer_hum_underground");

            yield return new WaitForSeconds(4f);

            if (_aquiferRevealFX != null) _aquiferRevealFX.SetActive(true);

            _aquiferFound = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "aquifer_found", true);
            GameEvents.FireCriticalSaveTrigger("ancient_aquifer_discovered");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_DISCOVERY);
            GameEvents.FireCriticalSaveTrigger("moon11_discovery_complete");
        }

        // ─── Beat 2: Restoration ───────────────────────────────────
        private IEnumerator Beat2_Restoration()
        {
            int total = GetTotalFountains();
            HUDController.Instance?.ShowObjective(
                $"Channel purified water through precision-cut tunnels. Activate fountain chain. [{_fountainsActivated}/{total}]");

            AudioManager.Instance?.PlaySFX2D("water_tunnel_ambient");
            if (_pureWaterFlowFX != null) _pureWaterFlowFX.SetActive(true);

            var fountainPoints = FindObjectsOfType<Moon11FountainActivatePoint>();
            foreach (var fp in fountainPoints)
                fp.OnActivated += OnFountainActivated;

            yield return new WaitUntil(() => _fountainsActivated >= total);

            HUDController.Instance?.ShowBanner(
                "FOUNTAIN CHAIN ACTIVE",
                "Surface fountains activate in chain reaction — one after another, city by city. " +
                "Ionized mist heals everything in its radius.",
                9f);
            AudioManager.Instance?.PlaySFX2D("fountain_chain_activation_cascade");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_RESTORATION);
            GameEvents.FireCriticalSaveTrigger("planetary_fountain_chain_active");
            GameEvents.FireCriticalSaveTrigger("moon11_restoration_complete");
        }

        private void OnFountainActivated(int fountainIndex)
        {
            _fountainsActivated = Mathf.Max(_fountainsActivated, fountainIndex + 1);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "fountains_activated_int", _fountainsActivated);

            if (_fountainChainFX != null && fountainIndex < _fountainChainFX.Length && _fountainChainFX[fountainIndex] != null)
                _fountainChainFX[fountainIndex].SetActive(true);

            HUDController.Instance?.ShowObjective($"Activate fountain chain: [{_fountainsActivated}/{GetTotalFountains()}]");
            AudioManager.Instance?.PlaySFX2D("fountain_activate_splash");
            GameEvents.FireCriticalSaveTrigger($"moon11_fountain_{fountainIndex + 1}_active");
        }

        // ─── Beat 3: Conflict ──────────────────────────────────────
        private IEnumerator Beat3_Conflict()
        {
            HUDController.Instance?.ShowBanner(
                "SPECTRAL MOON — CONFLICT",
                "Corrupted water sources fight back — sentient black-sludge tendrils (Mud Flood remnants with vestigial intelligence) clog pipes and attack.",
                9f);
            HUDController.Instance?.ShowObjective(
                "Cleanse sludge tendrils with 6-band resonance + fountain water counter-pressure.");

            AudioManager.Instance?.PlaySFX2D("sludge_tendril_encounter");

            HUDController.Instance?.ShowBanner(
                "MILO (knee-deep in sludge)",
                "\"I've sold mud, built on mud, lived in mud — and I STILL hate this stuff.\"",
                7f);
            AudioManager.Instance?.PlaySFX2D("milo_sludge_vo");

            var combatSystem = FindObjectOfType<BossEncounterSystem>();
            if (combatSystem != null)
                combatSystem.SpawnBoss("moon11_sludge_tendril_swarm");

            yield return new WaitUntil(() =>
                SaveManager.Instance?.CurrentSave?.GetMoonFlag(MOON_NUM, "sludge_tendrils_defeated") ?? false);

            _sludgeTendrilsDefeated = true;
            HUDController.Instance?.ShowBanner(
                "SLUDGE CLEANSED",
                "The pipes run pure. The fountains breathe freely. Even the mud pulls back.",
                6f);
            AudioManager.Instance?.PlaySFX2D("sludge_cleanse_tone");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CONFLICT);
            GameEvents.FireCriticalSaveTrigger("moon11_conflict_complete");
        }

        // ─── Beat 4: Climax ────────────────────────────────────────
        private IEnumerator Beat4_Climax()
        {
            HUDController.Instance?.ShowBanner(
                "PLANETARY FOUNTAIN ACTIVATION",
                "Every fountain on every continent sprays simultaneously. " +
                "Ionized mist creates continent-wide aurora veils — shimmering prismatic curtains visible from the airship.",
                11f);
            HUDController.Instance?.ShowObjective(
                "Board the airship to witness the planetary fountain activation.");

            AudioManager.Instance?.PlaySFX2D("planetary_fountain_surge");

            if (_planetaryAuroraVeilFX != null)
                _planetaryAuroraVeilFX.SetActive(true);

            yield return new WaitForSeconds(5f);

            HUDController.Instance?.ShowBanner(
                "THE WORLD HEALS",
                "The global map transforms: gray zones turn green, then golden.",
                7f);

            HUDController.Instance?.ShowBanner(
                "THORNE (from the airship bridge)",
                "\"The old world had a word for this. Kairos. The moment when everything aligns and the universe exhales.\"",
                10f);
            AudioManager.Instance?.PlaySFX2D("thorne_kairos_vo");

            _auroraVeilAppeared = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "aurora_veil_appeared", true);
            GameEvents.FireCriticalSaveTrigger("aurora_veil_continent_wide");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CLIMAX);
            GameEvents.FireCriticalSaveTrigger("moon11_climax_complete");
        }

        // ─── Beat 5: Revelation ────────────────────────────────────
        private IEnumerator Beat5_Revelation()
        {
            yield return new WaitForSeconds(2f);

            HUDController.Instance?.ShowBanner(
                "LORE REVELATION — THE AQUIFER'S TRUE ROLE",
                "Pure water was the lifeblood of the empire — not just for drinking but for conducting Aether, " +
                "healing cellular damage, and maintaining the resonance sensitivity that allowed human-giant cooperation. " +
                "The Reset's first strategic target was the aquifer system.",
                14f);
            AudioManager.Instance?.PlaySFX2D("moon11_lore_reveal_vo");

            yield return new WaitForSeconds(6f);

            // Lirael becomes semi-solid
            _liraelSemiSolid = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "lirael_semi_solid", true);
            if (_liraelSemiSolidProxy != null) _liraelSemiSolidProxy.SetActive(true);

            HUDController.Instance?.ShowBanner(
                "LIRAEL — MORE PRESENT",
                "The fountain network's ionized mist strengthens Lirael's resonance anchor. " +
                "She walks more solidly now. Her voice clearer. Her form more real.",
                9f);
            AudioManager.Instance?.PlaySFX2D("lirael_solidifies_vo");
            GameEvents.FireCriticalSaveTrigger("lirael_semi_solid");

            // Prophecy stones 10-11
            yield return new WaitForSeconds(3f);
            HUDController.Instance?.ShowBanner(
                "PROPHECY STONES 10-11 APPEAR",
                "Stone of Healing: fountain water restoring cellular memory. " +
                "Stone of Warning: the first tremors — and THREE FIGURES at the trigger device.",
                10f);
            AudioManager.Instance?.PlaySFX2D("prophecy_stones_warning_chime");
            GameEvents.FireCriticalSaveTrigger("moon9_stones_10_11_spawned");
            GameEvents.FireCriticalSaveTrigger("three_figures_warning_stone");

            // Crossover seeds
            GameEvents.FireCriticalSaveTrigger("moon11_seed_fountains_heal_companions");
            GameEvents.FireCriticalSaveTrigger("moon11_seed_thorne_ship_ionized_air");
            GameEvents.FireCriticalSaveTrigger("moon11_seed_purification_prereq_moon12");
            GameEvents.FireCriticalSaveTrigger("moon11_seed_warning_vision_moon13");

            _moonCleared = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "moon_cleared", true);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_REVELATION);
            MoonProgressTracker.Instance?.MarkCleared(MOON_NUM);
            GameEvents.FireCriticalSaveTrigger("moon11_complete");

            HUDController.Instance?.ShowBanner(
                "SPECTRAL MOON — COMPLETE",
                "The aquifer flows. The fountains sing. Lirael steps forward. The final arc begins.",
                8f);
            AudioManager.Instance?.PlaySFX2D("moon11_completion_sting");

            ApplyPersistentWorldState();
        }
    }

    // ─── Fountain Activate Point Helper ────────────────────────────────
    public class Moon11FountainActivatePoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _fountainIndex;
        public event System.Action<int> OnActivated;
        private bool _activated;

        public void Interact(GameObject interactor)
        {
            if (_activated) return;
            _activated = true;
            OnActivated?.Invoke(_fountainIndex);
            gameObject.SetActive(false);
        }

        public string GetInteractPrompt() => "Channel Water — Activate Fountain";
        public bool CanInteract(GameObject interactor) => !_activated;
    }
}
