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
    // MOON 10: PLANETARY MOON — "The Manifestation of Producing"
    // Galactic Tone: Manifestation / Production
    // Scene prefix: "PlanetaryMoon" | "ContinentalRail" | "MegaStation"
    // 5-Beat: Discovery → Restoration → Conflict → Climax → Revelation
    // Key: Continental train network; children as junior engineers;
    //      hidden Mud Flood trigger room (3 fingerprints); prophecy stones 7-9
    // ============================================================

    public class Moon10PlanetaryArc : MonoBehaviour
    {
        public static Moon10PlanetaryArc Instance { get; private set; }

        private const int MOON_NUM = 10;

        private const int BEAT_DISCOVERY   = 0;
        private const int BEAT_RESTORATION = 1;
        private const int BEAT_CONFLICT    = 2;
        private const int BEAT_CLIMAX      = 3;
        private const int BEAT_REVELATION  = 4;

        [Header("Rail Network")]
        [SerializeField] private Transform[] _railSegmentSockets;
        [SerializeField] private GameObject  _trainPrefab;
        [SerializeField] private Transform   _trainSpawnPoint;
        [SerializeField] private Transform[] _stationTransforms;

        [Header("Trigger Room")]
        [SerializeField] private GameObject _triggerRoomRevealFX;
        [SerializeField] private Transform  _triggerDeviceProp;

        private int  _railSegmentsLaid;    // 0 to railSegmentSockets.Length
        private bool _triggerRoomFound;
        private bool _trainJourneyComplete;
        private bool _moonCleared;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!scene.StartsWith("PlanetaryMoon") &&
                !scene.StartsWith("ContinentalRail") &&
                !scene.StartsWith("MegaStation"))
                return;

            var go = new GameObject("Moon10PlanetaryArc");
            go.AddComponent<Moon10PlanetaryArc>();
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

            _railSegmentsLaid   = save.GetMoonFlag(MOON_NUM, "rail_segments_int", 0);
            _triggerRoomFound   = save.GetMoonFlag(MOON_NUM, "trigger_room_found");
            _trainJourneyComplete= save.GetMoonFlag(MOON_NUM, "train_journey_complete");
            _moonCleared        = save.GetMoonFlag(MOON_NUM, "moon_cleared");

            if (_moonCleared) ApplyPersistentWorldState();
        }

        private void ApplyPersistentWorldState()
        {
            if (_moonCleared)
                HUDController.Instance?.ShowObjective(
                    "<b>PLANETARY MOON — THE RAIL SINGS</b>\n" +
                    "Continental train network complete. The children keep the trains running. " +
                    "The trigger room evidence is secured.");
        }

        private IEnumerator RunArc()
        {
            if (_moonCleared) yield break;
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) yield break;

            if (!_triggerRoomFound && _railSegmentsLaid == 0)
                yield return StartCoroutine(Beat1_Discovery());
            if (_railSegmentsLaid < GetTotalRailSegments())
                yield return StartCoroutine(Beat2_Restoration());
            if (save.GetMoonFlag(MOON_NUM, "dissonant_rail_cleared") == false)
                yield return StartCoroutine(Beat3_Conflict());
            if (!_trainJourneyComplete)
                yield return StartCoroutine(Beat4_Climax());
            if (!_moonCleared)
                yield return StartCoroutine(Beat5_Revelation());
        }

        private int GetTotalRailSegments() =>
            (_railSegmentSockets != null) ? _railSegmentSockets.Length : 12;

        // ─── Beat 1: Discovery ─────────────────────────────────────
        private IEnumerator Beat1_Discovery()
        {
            HUDController.Instance?.ShowBanner(
                "PLANETARY MOON — DISCOVERY",
                "The Aether grid nears 80%. Buried train stations surface as mud recedes. " +
                "Inside: precision-cut platforms, copper-inlaid waiting halls, Aether-powered engine bays.",
                9f);
            HUDController.Instance?.ShowObjective(
                "Excavate the main continental station to reveal the hidden trigger room.");

            AudioManager.Instance?.PlaySFX2D("rail_station_emerge_rumble");

            yield return new WaitForSeconds(3f);

            // Hidden trigger room discovery
            if (_triggerRoomRevealFX != null) _triggerRoomRevealFX.SetActive(true);

            HUDController.Instance?.ShowBanner(
                "HIDDEN ROOM FOUND",
                "One station has a sealed chamber with the original Mud Flood trigger device — " +
                "a massive dissonance amplifier pointed at the star fort network. " +
                "Three sets of fingerprints on the control panel.",
                12f);
            AudioManager.Instance?.PlaySFX2D("trigger_room_discovery_sting");

            _triggerRoomFound = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "trigger_room_found", true);
            GameEvents.FireCriticalSaveTrigger("mud_flood_trigger_device_found");
            GameEvents.FireCriticalSaveTrigger("three_fingerprints_mystery_seeded");

            yield return new WaitForSeconds(2f);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_DISCOVERY);
            GameEvents.FireCriticalSaveTrigger("moon10_discovery_complete");
        }

        // ─── Beat 2: Restoration ───────────────────────────────────
        private IEnumerator Beat2_Restoration()
        {
            int total = GetTotalRailSegments();
            HUDController.Instance?.ShowObjective(
                $"Build the continental rail network: cut and lay precision rail segments. [{_railSegmentsLaid}/{total}]");

            AudioManager.Instance?.PlaySFX2D("rail_construction_ambient");

            var railPoints = FindObjectsOfType<Moon10RailLayPoint>();
            foreach (var rp in railPoints)
                rp.OnLaid += OnRailSegmentLaid;

            yield return new WaitUntil(() => _railSegmentsLaid >= total);

            HUDController.Instance?.ShowBanner(
                "CONTINENTAL RAIL COMPLETE",
                "Every restored zone connected. The children from Moon 3 — now skilled junior engineers — operate the trains.",
                8f);

            HUDController.Instance?.ShowBanner(
                "JUNIOR ENGINEER (grinning)",
                "\"Korath said the rails should sing. I tuned this one myself — listen!\" " +
                "The rail hums a perfect 432 Hz note.",
                8f);
            AudioManager.Instance?.PlaySFX2D("rail_432hz_hum");

            GameEvents.FireCriticalSaveTrigger("continental_rail_network_complete");
            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_RESTORATION);
            GameEvents.FireCriticalSaveTrigger("moon10_restoration_complete");
        }

        private void OnRailSegmentLaid(int segIndex)
        {
            _railSegmentsLaid = Mathf.Max(_railSegmentsLaid, segIndex + 1);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "rail_segments_int", _railSegmentsLaid);
            HUDController.Instance?.ShowObjective(
                $"Build rail network: [{_railSegmentsLaid}/{GetTotalRailSegments()}]");
            AudioManager.Instance?.PlaySFX2D("rail_segment_click");
        }

        // ─── Beat 3: Conflict ──────────────────────────────────────
        private IEnumerator Beat3_Conflict()
        {
            HUDController.Instance?.ShowBanner(
                "DISSONANT RAILS",
                "Zereth's old frequency experiments left pockets of inverted resonance in the ley-line network. " +
                "Elite golems spawn on corrupted track segments.",
                8f);
            HUDController.Instance?.ShowObjective(
                "Purify corrupted rail segments with fountain water + tuning. Defeat elite ley-line golems.");

            AudioManager.Instance?.PlaySFX2D("dissonant_rail_alarm");

            var combatSystem = FindObjectOfType<BossEncounterSystem>();
            if (combatSystem != null)
                combatSystem.SpawnBoss("moon10_ley_golem_elite");

            yield return new WaitUntil(() =>
                SaveManager.Instance?.CurrentSave?.GetMoonFlag(MOON_NUM, "dissonant_rail_cleared") ?? false);

            HUDController.Instance?.ShowBanner(
                "RAILS PURIFIED",
                "Every track segment hums at the correct frequency. The ley-line network flows clean.",
                6f);
            AudioManager.Instance?.PlaySFX2D("rail_purified_chime");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CONFLICT);
            GameEvents.FireCriticalSaveTrigger("moon10_conflict_complete");
        }

        // ─── Beat 4: Climax ────────────────────────────────────────
        private IEnumerator Beat4_Climax()
        {
            HUDController.Instance?.ShowBanner(
                "FULL CONTINENTAL JOURNEY",
                "All zones connected. Every bell tower rings as the train passes. " +
                "Children wave from platforms. Pure water fountains spray ionized arches over the tracks like liquid rainbows.",
                10f);
            HUDController.Instance?.ShowObjective(
                "Board the train for the full continental journey under the 13th Moon.");

            AudioManager.Instance?.PlaySFX2D("moon10_train_journey_music");

            // Spawn train
            if (_trainPrefab != null && _trainSpawnPoint != null)
                Instantiate(_trainPrefab, _trainSpawnPoint.position, _trainSpawnPoint.rotation);

            yield return new WaitForSeconds(12f);

            HUDController.Instance?.ShowBanner(
                "THORNE (in the oversized caboose)",
                "\"In my day, the trains ran at the speed of song. Yours are slower. But they've got more heart.\"",
                8f);
            AudioManager.Instance?.PlaySFX2D("thorne_train_vo");

            _trainJourneyComplete = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "train_journey_complete", true);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CLIMAX);
            GameEvents.FireCriticalSaveTrigger("moon10_climax_complete");
        }

        // ─── Beat 5: Revelation ────────────────────────────────────
        private IEnumerator Beat5_Revelation()
        {
            yield return new WaitForSeconds(2f);

            HUDController.Instance?.ShowBanner(
                "THE TRIGGER ROOM — ANALYSIS",
                "Three operators present when the Mud Flood device activated. " +
                "One giant (Zereth's prints). Two humans — size matching Parasite Cabal proportions. " +
                "The truth is more complex than 'one villain.'",
                13f);
            AudioManager.Instance?.PlaySFX2D("trigger_room_analysis_vo");

            yield return new WaitForSeconds(5f);

            GameEvents.FireCriticalSaveTrigger("two_humans_at_trigger_revealed");
            GameEvents.FireCriticalSaveTrigger("parasite_cabal_implicated");

            // Prophecy stones 7-9 appear
            HUDController.Instance?.ShowBanner(
                "PROPHECY STONES 7-9 APPEAR",
                "Stone of Giants: Korath building. Stone of Children: learning and laughing. " +
                "Stone of Rail: trains connecting the globe.",
                9f);
            AudioManager.Instance?.PlaySFX2D("prophecy_stone_trio_chime");

            GameEvents.FireCriticalSaveTrigger("moon9_stones_7_8_9_spawned");

            // Crossover seeds
            GameEvents.FireCriticalSaveTrigger("moon10_seed_trains_carry_fountain_water");
            GameEvents.FireCriticalSaveTrigger("moon10_seed_trigger_room_evidence_moon13");
            GameEvents.FireCriticalSaveTrigger("moon10_seed_full_transport_network_endgame");

            _moonCleared = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "moon_cleared", true);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_REVELATION);
            MoonProgressTracker.Instance?.MarkCleared(MOON_NUM);
            GameEvents.FireCriticalSaveTrigger("moon10_complete");

            HUDController.Instance?.ShowBanner(
                "PLANETARY MOON — COMPLETE",
                "The continental train network lives. The trigger room evidence is secured. The truth awaits Moon 13.",
                8f);
            AudioManager.Instance?.PlaySFX2D("moon10_completion_sting");

            ApplyPersistentWorldState();
        }
    }

    // ─── Rail Lay Point Helper ──────────────────────────────────────────
    public class Moon10RailLayPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _segmentIndex;
        public event System.Action<int> OnLaid;
        private bool _laid;

        public void Interact(GameObject interactor)
        {
            if (_laid) return;
            _laid = true;
            OnLaid?.Invoke(_segmentIndex);
            gameObject.SetActive(false);
        }

        public string GetInteractPrompt() => "Lay Rail Segment (Precision Cut + Tune)";
        public bool CanInteract(GameObject interactor) => !_laid;
    }
}
