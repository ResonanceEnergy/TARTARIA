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
    // MOON 12: CRYSTAL MOON — "The Cooperation of Dedicating"
    // Galactic Tone: Cooperation / Dedication
    // Scene prefix: "CrystalMoon" | "BellNetwork" | "BellTower"
    // 5-Beat: Discovery → Restoration → Conflict → Climax → Revelation
    // Key: 12 bell towers across 12 continents; sync using ALL prior mechanics;
    //      global Reset assault; 60-second Planetary Ring; Stone of Promise; 95% grid
    // ============================================================

    public class Moon12CrystalArc : MonoBehaviour
    {
        public static Moon12CrystalArc Instance { get; private set; }

        private const int MOON_NUM = 12;

        private const int BEAT_DISCOVERY   = 0;
        private const int BEAT_RESTORATION = 1;
        private const int BEAT_CONFLICT    = 2;
        private const int BEAT_CLIMAX      = 3;
        private const int BEAT_REVELATION  = 4;

        private const int TOTAL_BELL_TOWERS = 12;

        [Header("Bell Towers")]
        [SerializeField] private Moon12BellTowerPoint[] _bellTowers;
        [SerializeField] private GameObject              _bellNetworkActiveFX;

        [Header("Planetary Ring")]
        [SerializeField] private GameObject _planetaryRingFX;
        [SerializeField] private float      _planetaryRingDuration = 60f;

        [Header("Stone of Promise")]
        [SerializeField] private Transform _stoneOfPromiseSocket;
        [SerializeField] private GameObject _stoneOfPromisePrefab;

        private int  _bellTowersSynced;
        private bool _resetAssaultBegun;
        private bool _resetAssaultRepelled;
        private bool _planetaryRingFired;
        private bool _moonCleared;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!scene.StartsWith("CrystalMoon") &&
                !scene.StartsWith("BellNetwork") &&
                !scene.StartsWith("BellTower"))
                return;

            var go = new GameObject("Moon12CrystalArc");
            go.AddComponent<Moon12CrystalArc>();
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

            _bellTowersSynced    = save.GetMoonFlag(MOON_NUM, "bells_synced_int", 0);
            _resetAssaultBegun   = save.GetMoonFlag(MOON_NUM, "reset_assault_begun");
            _resetAssaultRepelled= save.GetMoonFlag(MOON_NUM, "reset_assault_repelled");
            _planetaryRingFired  = save.GetMoonFlag(MOON_NUM, "planetary_ring_fired");
            _moonCleared         = save.GetMoonFlag(MOON_NUM, "moon_cleared");

            if (_moonCleared) ApplyPersistentWorldState();
        }

        private void ApplyPersistentWorldState()
        {
            if (_bellNetworkActiveFX != null) _bellNetworkActiveFX.SetActive(true);

            for (int i = 0; i < _bellTowersSynced && _bellTowers != null && i < _bellTowers.Length; i++)
                _bellTowers[i]?.SetSyncedVisual();

            if (_moonCleared)
                HUDController.Instance?.ShowObjective(
                    "<b>CRYSTAL MOON — THE GRID SINGS</b>\n" +
                    "All 12 bell towers synchronized. The Planetary Ring is complete. Grid at 95%.");
        }

        private IEnumerator RunArc()
        {
            if (_moonCleared) yield break;
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) yield break;

            if (_bellTowersSynced < 1)       yield return StartCoroutine(Beat1_Discovery());
            if (_bellTowersSynced < TOTAL_BELL_TOWERS) yield return StartCoroutine(Beat2_Restoration());
            if (!_resetAssaultRepelled)      yield return StartCoroutine(Beat3_Conflict());
            if (!_planetaryRingFired)        yield return StartCoroutine(Beat4_Climax());
            if (!_moonCleared)               yield return StartCoroutine(Beat5_Revelation());
        }

        // ─── Beat 1: Discovery ─────────────────────────────────────
        private IEnumerator Beat1_Discovery()
        {
            HUDController.Instance?.ShowBanner(
                "CRYSTAL MOON — DISCOVERY",
                "The final pre-endgame moon. 12 bell towers — one on each continent — are visible from the airship. " +
                "Each must be synchronized using every mechanic mastered across the prior 11 moons.",
                10f);
            HUDController.Instance?.ShowObjective(
                "Reach and synchronize the first bell tower.");

            AudioManager.Instance?.PlaySFX2D("bell_tower_distant_peal");

            HUDController.Instance?.ShowBanner(
                "KORATH ECHO (resonant, strong)",
                "\"We built these towers to speak to the stars. When they sing together — the stars remember us.\"",
                9f);
            AudioManager.Instance?.PlaySFX2D("korath_echo_bell_vo");

            if (_bellTowers != null)
                foreach (var bt in _bellTowers)
                    bt.OnSynced += OnBellTowerSynced;

            yield return new WaitUntil(() => _bellTowersSynced >= 1);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_DISCOVERY);
            GameEvents.FireCriticalSaveTrigger("moon12_discovery_complete");
        }

        // ─── Beat 2: Restoration ───────────────────────────────────
        private IEnumerator Beat2_Restoration()
        {
            HUDController.Instance?.ShowObjective(
                $"Synchronize all 12 bell towers using every learned mechanic. [{_bellTowersSynced}/{TOTAL_BELL_TOWERS}]");

            AudioManager.Instance?.PlaySFX2D("bell_sync_progress_ambient");

            yield return new WaitUntil(() => _bellTowersSynced >= TOTAL_BELL_TOWERS);

            HUDController.Instance?.ShowBanner(
                "12 TOWERS SYNCHRONIZED",
                "Every bell tower tuned. Korath echoes ring from all 12. The entire continent network hums as one instrument.",
                9f);
            AudioManager.Instance?.PlaySFX2D("twelve_towers_synced_chord");

            if (_bellNetworkActiveFX != null) _bellNetworkActiveFX.SetActive(true);
            GameEvents.FireCriticalSaveTrigger("all_12_bell_towers_synced");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_RESTORATION);
            GameEvents.FireCriticalSaveTrigger("moon12_restoration_complete");
        }

        private void OnBellTowerSynced(int towerIndex)
        {
            _bellTowersSynced = Mathf.Max(_bellTowersSynced, towerIndex + 1);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "bells_synced_int", _bellTowersSynced);
            HUDController.Instance?.ShowObjective($"Sync bell towers: [{_bellTowersSynced}/{TOTAL_BELL_TOWERS}]");
            AudioManager.Instance?.PlaySFX2D($"bell_sync_{towerIndex + 1}");
            GameEvents.FireCriticalSaveTrigger($"moon12_bell_{towerIndex + 1}_synced");
        }

        // ─── Beat 3: Conflict — global Reset assault ───────────────
        private IEnumerator Beat3_Conflict()
        {
            _resetAssaultBegun = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "reset_assault_begun", true);

            HUDController.Instance?.ShowBanner(
                "RESET GLOBAL ASSAULT — MAXIMUM FORCE",
                "The Reset deploys their complete remaining arsenal: Dissonance Cannons targeting all 12 towers simultaneously. " +
                "Every companion is deployed. Every ally called. This is the final war.",
                11f);
            AudioManager.Instance?.PlaySFX2D("reset_global_assault_alarm");
            HUDController.Instance?.ShowObjective(
                "Defend all 12 bell towers. Coordinate every companion and mechanic.");

            // All companions get deployment callouts
            HUDController.Instance?.ShowBanner("MILO — DEPLOYED", "Frequency dampeners on towers 1-4.", 5f);
            AudioManager.Instance?.PlaySFX2D("milo_deploy_vo");
            yield return new WaitForSeconds(1f);

            HUDController.Instance?.ShowBanner("THORNE — DEPLOYED", "Airship covering towers 5-8.", 5f);
            AudioManager.Instance?.PlaySFX2D("thorne_deploy_vo");
            yield return new WaitForSeconds(1f);

            HUDController.Instance?.ShowBanner("LIRAEL — DEPLOYED", "Resonance shield weaving around towers 9-12.", 5f);
            AudioManager.Instance?.PlaySFX2D("lirael_deploy_vo");
            yield return new WaitForSeconds(1f);

            bool cassianRedeemed = SaveManager.Instance?.CurrentSave?.GetMoonFlag(0, "cassian_fate_redeemed") ?? false;
            if (cassianRedeemed)
            {
                HUDController.Instance?.ShowBanner("CASSIAN (redeemed) — DEPLOYED",
                    "Feeding Reset tactical data — jamming their targeting systems.", 5f);
                AudioManager.Instance?.PlaySFX2D("cassian_redeemed_deploy_vo");
                yield return new WaitForSeconds(1f);
            }

            var combatSystem = FindObjectOfType<BossEncounterSystem>();
            if (combatSystem != null)
                combatSystem.SpawnBoss("moon12_reset_global_assault");

            yield return new WaitUntil(() =>
                SaveManager.Instance?.CurrentSave?.GetMoonFlag(MOON_NUM, "reset_assault_repelled") ?? false);

            _resetAssaultRepelled = true;
            HUDController.Instance?.ShowBanner(
                "RESET REPELLED — FOREVER",
                "Their final commander falls. The Reset as an organization is shattered. " +
                "Remnants retreat to the Dissonant timeline.",
                9f);
            AudioManager.Instance?.PlaySFX2D("reset_final_defeat_sting");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CONFLICT);
            GameEvents.FireCriticalSaveTrigger("reset_permanently_shattered");
            GameEvents.FireCriticalSaveTrigger("moon12_conflict_complete");
        }

        // ─── Beat 4: Climax — 60-second Planetary Ring ─────────────
        private IEnumerator Beat4_Climax()
        {
            yield return new WaitForSeconds(2f);

            HUDController.Instance?.ShowBanner(
                "THE PLANETARY RING",
                "All 12 bells ring simultaneously. For 60 seconds — maximum resonance. " +
                "The Aether grid burns gold from orbit. Every restored zone pulses with light.",
                12f);
            HUDController.Instance?.ShowObjective(
                "Stand at the central bell and receive the Planetary Ring.");

            AudioManager.Instance?.PlaySFX2D("planetary_ring_60s_peal");

            if (_planetaryRingFX != null)
                _planetaryRingFX.SetActive(true);

            yield return new WaitForSeconds(_planetaryRingDuration);

            if (_planetaryRingFX != null)
                _planetaryRingFX.SetActive(false);

            HUDController.Instance?.ShowBanner(
                "KORATH ECHO — FINAL BELL",
                "\"We always knew you would come. We left the bells ready. " +
                "One more door. The last moon. Go — finish what we started together.\"",
                11f);
            AudioManager.Instance?.PlaySFX2D("korath_final_bell_vo");

            _planetaryRingFired = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "planetary_ring_fired", true);
            GameEvents.FireCriticalSaveTrigger("planetary_ring_complete");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CLIMAX);
            GameEvents.FireCriticalSaveTrigger("moon12_climax_complete");
        }

        // ─── Beat 5: Revelation ────────────────────────────────────
        private IEnumerator Beat5_Revelation()
        {
            yield return new WaitForSeconds(2f);

            HUDController.Instance?.ShowBanner(
                "STONE OF PROMISE",
                "The 12th prophecy stone appears — Stone of Promise. " +
                "Vision: two shadows standing at the edge of the grid at full resonance. " +
                "One is enormous. One is human-sized. They are holding hands.",
                12f);
            AudioManager.Instance?.PlaySFX2D("stone_of_promise_vision");

            if (_stoneOfPromisePrefab != null && _stoneOfPromiseSocket != null)
                Instantiate(_stoneOfPromisePrefab, _stoneOfPromiseSocket.position, _stoneOfPromiseSocket.rotation);

            GameEvents.FireCriticalSaveTrigger("stone_of_promise_received");

            yield return new WaitForSeconds(5f);

            HUDController.Instance?.ShowBanner(
                "GRID AT 95%",
                "One moon remains. The final 5% requires reaching the origin point — " +
                "the 17th Hour node at the heart of the first star fort ever built.",
                10f);
            AudioManager.Instance?.PlaySFX2D("grid_95_percent_tone");
            GameEvents.FireCriticalSaveTrigger("grid_at_95_percent");

            // Crossover seeds — all companions confirmed for Moon 13
            GameEvents.FireCriticalSaveTrigger("moon12_seed_all_zone_bell_buffs");
            GameEvents.FireCriticalSaveTrigger("moon12_seed_all_companions_moon13");
            GameEvents.FireCriticalSaveTrigger("moon12_seed_grid_95_percent");

            _moonCleared = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "moon_cleared", true);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_REVELATION);
            MoonProgressTracker.Instance?.MarkCleared(MOON_NUM);
            GameEvents.FireCriticalSaveTrigger("moon12_complete");

            HUDController.Instance?.ShowBanner(
                "CRYSTAL MOON — COMPLETE",
                "All 12 bells sing. The Reset is broken. One door remains. Enter the Cosmic Moon.",
                9f);
            AudioManager.Instance?.PlaySFX2D("moon12_completion_sting");

            ApplyPersistentWorldState();
        }
    }

    // ─── Bell Tower Sync Point Helper ──────────────────────────────────
    public class Moon12BellTowerPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _towerIndex;
        public event System.Action<int> OnSynced;
        private bool _synced;

        [SerializeField] private GameObject _syncedVisualIndicator;

        public void SetSyncedVisual()
        {
            if (_syncedVisualIndicator != null) _syncedVisualIndicator.SetActive(true);
            _synced = true;
        }

        public void Interact(GameObject interactor)
        {
            if (_synced) return;
            SetSyncedVisual();
            OnSynced?.Invoke(_towerIndex);
        }

        public string GetInteractPrompt() => $"Synchronize Bell Tower {_towerIndex + 1} of 12";
        public bool CanInteract(GameObject interactor) => !_synced;
    }
}
