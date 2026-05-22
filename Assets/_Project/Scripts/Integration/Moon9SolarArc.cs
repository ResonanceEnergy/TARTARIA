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
    // MOON 9: SOLAR MOON — "The Intention of Intention"
    // Galactic Tone: Intention / Realization
    // Scene prefix: "SolarMoon" | "ProphecyStone" | "CrossContinental"
    // 5-Beat: Discovery → Restoration → Conflict → Climax → Revelation
    // Key: Prophecy stone collection; timeline echo visions; Zereth speaks directly;
    //      floating aurora city; 6th stone timestamp mystery; 17-hour clock tower
    // ============================================================

    public class Moon9SolarArc : MonoBehaviour
    {
        public static Moon9SolarArc Instance { get; private set; }

        private const int MOON_NUM = 9;

        private const int BEAT_DISCOVERY   = 0;
        private const int BEAT_RESTORATION = 1;
        private const int BEAT_CONFLICT    = 2;
        private const int BEAT_CLIMAX      = 3;
        private const int BEAT_REVELATION  = 4;

        [Header("Prophecy Stones")]
        [SerializeField] private Moon9ProphecyStone[] _stones;        // 6 stones for this moon

        [Header("Aurora City")]
        [SerializeField] private GameObject _floatingAuroraCityFX;
        [SerializeField] private Transform  _auroraCityAnchor;

        [Header("Clock Tower")]
        [SerializeField] private Transform _clockTowerSocket;
        [SerializeField] private GameObject _clockMechanismPrefab;

        private int  _stonesCollected;   // 0-6
        private bool _zerethSpoke;
        private bool _auroraCityAppeared;
        private bool _clockInstalled;
        private bool _moonCleared;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!scene.StartsWith("SolarMoon") &&
                !scene.StartsWith("ProphecyStone") &&
                !scene.StartsWith("CrossContinental"))
                return;

            var go = new GameObject("Moon9SolarArc");
            go.AddComponent<Moon9SolarArc>();
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

            _stonesCollected   = save.GetMoonFlag(MOON_NUM, "stones_collected_int", 0);
            _zerethSpoke       = save.GetMoonFlag(MOON_NUM, "zereth_spoke");
            _auroraCityAppeared= save.GetMoonFlag(MOON_NUM, "aurora_city_appeared");
            _clockInstalled    = save.GetMoonFlag(MOON_NUM, "clock_installed");
            _moonCleared       = save.GetMoonFlag(MOON_NUM, "moon_cleared");

            if (_moonCleared) ApplyPersistentWorldState();
        }

        private void ApplyPersistentWorldState()
        {
            if (_clockInstalled && _clockMechanismPrefab != null && _clockTowerSocket != null)
            {
                Instantiate(_clockMechanismPrefab, _clockTowerSocket.position, _clockTowerSocket.rotation);
            }
            if (_moonCleared)
                HUDController.Instance?.ShowObjective(
                    "<b>SOLAR MOON — THE PROPHECY UNFOLDS</b>\n" +
                    "Six stones aligned. The aurora city spoke. The clock ticks on the 17th Hour.");
        }

        private IEnumerator RunArc()
        {
            if (_moonCleared) yield break;
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) yield break;

            if (_stonesCollected < 1)     yield return StartCoroutine(Beat1_Discovery());
            if (_stonesCollected < 6)     yield return StartCoroutine(Beat2_Restoration());
            if (!_zerethSpoke)            yield return StartCoroutine(Beat3_Conflict());
            if (!_auroraCityAppeared)     yield return StartCoroutine(Beat4_Climax());
            if (!_clockInstalled)         yield return StartCoroutine(Beat5_Revelation());
        }

        // ─── Beat 1: Discovery ─────────────────────────────────────
        private IEnumerator Beat1_Discovery()
        {
            HUDController.Instance?.ShowBanner(
                "SOLAR MOON — DISCOVERY",
                "Prophecy stones — ancient crystals inscribed with golden-ratio patterns — appear across the grid as floating golden markers at ley-line intersections.",
                8f);
            HUDController.Instance?.ShowObjective(
                "Locate the first prophecy stone via airship or train.");

            AudioManager.Instance?.PlaySFX2D("prophecy_stone_hum_ambient");

            // Check Cassian fate for discovery assist
            bool cassianRedeemed = SaveManager.Instance?.CurrentSave?.GetMoonFlag(0, "cassian_fate_redeemed") ?? false;
            if (cassianRedeemed)
            {
                HUDController.Instance?.ShowBanner(
                    "CASSIAN (redeemed)",
                    "He provides coded translations from his Reset contact days, speeding prophecy stone discovery.",
                    6f);
                AudioManager.Instance?.PlaySFX2D("cassian_redeemed_assist_vo");
            }
            else
            {
                HUDController.Instance?.ShowBanner(
                    "CASSIAN'S GHOST-ECHO",
                    "His ghost-echo haunts stone locations, offering cryptic directions from beyond.",
                    6f);
                AudioManager.Instance?.PlaySFX2D("cassian_ghost_echo_vo");
            }

            // Subscribe to stone collection
            if (_stones != null)
            {
                foreach (var stone in _stones)
                    stone.OnCollected += OnStoneCollected;
            }

            yield return new WaitUntil(() => _stonesCollected >= 1);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_DISCOVERY);
            GameEvents.FireCriticalSaveTrigger("moon9_discovery_complete");
        }

        // ─── Beat 2: Restoration — collect all 6 stones, each triggers vision ───
        private IEnumerator Beat2_Restoration()
        {
            HUDController.Instance?.ShowObjective(
                $"Collect and align all 6 prophecy stones. [{_stonesCollected}/6]");

            yield return new WaitUntil(() => _stonesCollected >= 6);

            HUDController.Instance?.ShowBanner(
                "SIX STONES ALIGNED",
                "Each stone triggers a Prophecy Vision when held during the 17th Hour. " +
                "Giants and humans in communal song. Water fountains feeding ionized mist. Sound waves parting granite.",
                10f);
            AudioManager.Instance?.PlaySFX2D("stones_alignment_chime");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_RESTORATION);
            GameEvents.FireCriticalSaveTrigger("moon9_six_stones_aligned");
            GameEvents.FireCriticalSaveTrigger("moon9_restoration_complete");
        }

        private static readonly string[] _stoneNames = {
            "Stone of Dawn",  "Stone of Flow", "Stone of Craft",
            "Stone of Flight","Stone of Song",  "Stone of Stars"
        };
        private static readonly string[] _stoneVisions = {
            "Giants and humans greeting the 17-hour sunrise with communal song.",
            "Pure water fountains feeding ionized mist through golden streets.",
            "Sound waves parting granite — precision cutting at continental scale.",
            "Airships lifting megaliths through aurora night.",
            "Pipe organs thundering while cymatic gardens bloom.",
            "Bell towers ringing in cosmic alignment. Timestamp: Rhythmic Moon, 17th Hour. But the bells ring in PERFECT HARMONY. Nothing is wrong."
        };

        private void OnStoneCollected(int stoneIndex)
        {
            _stonesCollected = Mathf.Max(_stonesCollected, stoneIndex + 1);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "stones_collected_int", _stonesCollected);
            HUDController.Instance?.ShowObjective($"Collect prophecy stones: [{_stonesCollected}/6]");

            if (stoneIndex < _stoneNames.Length)
            {
                HUDController.Instance?.ShowBanner(
                    $"PROPHECY VISION — {_stoneNames[stoneIndex]}",
                    _stoneVisions[stoneIndex],
                    9f);
            }

            AudioManager.Instance?.PlaySFX2D($"prophecy_vision_{stoneIndex + 1}");
            GameEvents.FireCriticalSaveTrigger($"moon9_stone_{stoneIndex + 1}_collected");
        }

        // ─── Beat 3: Conflict — Zereth speaks directly; Reset attacks ─────────
        private IEnumerator Beat3_Conflict()
        {
            yield return new WaitForSeconds(1f);

            HUDController.Instance?.ShowBanner(
                "THE DISSONANT ONE SPEAKS",
                "Zereth's echo appears as a dark shimmer at the edge of each prophecy vision — and speaks directly to you for the first time.",
                8f);
            AudioManager.Instance?.PlaySFX2D("zereth_first_direct_vo");

            HUDController.Instance?.ShowBanner(
                "ZERETH (distorted, agonized)",
                "\"You see paradise. I saw a cage. They called it harmony. I called it submission. " +
                "One note — one frequency — forever? I wanted MORE.\"",
                11f);
            AudioManager.Instance?.PlaySFX2D("zereth_monologue_vo");

            _zerethSpoke = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "zereth_spoke", true);
            GameEvents.FireCriticalSaveTrigger("zereth_first_contact");

            // Reset attacks on prophecy sites
            HUDController.Instance?.ShowBanner(
                "RESET AGENTS — INTENSIFIED ASSAULT",
                "Reset forces are targeting prophecy stone sites specifically. Defend the stones.",
                7f);
            AudioManager.Instance?.PlaySFX2D("reset_attack_alarm");

            var combatSystem = FindObjectOfType<BossEncounterSystem>();
            if (combatSystem != null)
                combatSystem.SpawnBoss("moon9_reset_prophecy_assault");

            yield return new WaitUntil(() =>
                SaveManager.Instance?.CurrentSave?.GetMoonFlag(MOON_NUM, "reset_assault_repelled") ?? false);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CONFLICT);
            GameEvents.FireCriticalSaveTrigger("moon9_conflict_complete");
        }

        // ─── Beat 4: Climax — floating aurora city manifests for 3 minutes ────
        private IEnumerator Beat4_Climax()
        {
            yield return new WaitForSeconds(1f);

            HUDController.Instance?.ShowBanner(
                "ALL SIX STONES ALIGNED — AURORA CITY",
                "A temporary floating aurora city appears above you — a complete Golden Age district: " +
                "domes, spires, fountains, trains, all in perfect operation. For 3 minutes.",
                10f);
            AudioManager.Instance?.PlaySFX2D("aurora_city_manifestation");

            if (_floatingAuroraCityFX != null)
                _floatingAuroraCityFX.SetActive(true);

            HUDController.Instance?.ShowBanner(
                "MILO (staring up)",
                "\"That's real, isn't it? Not a sales pitch. Not a postcard. That's what we were supposed to have.\"",
                9f);
            AudioManager.Instance?.PlaySFX2D("milo_aurora_city_vo");

            // 3-minute window
            yield return new WaitForSeconds(180f);

            // It fades — the loss hits
            if (_floatingAuroraCityFX != null)
                _floatingAuroraCityFX.SetActive(false);

            HUDController.Instance?.ShowBanner(
                "THE CITY FADES",
                "It's gone. The loss hits harder than any boss fight. But now you know what you're building toward.",
                9f);
            AudioManager.Instance?.PlaySFX2D("aurora_city_fade");

            _auroraCityAppeared = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "aurora_city_appeared", true);
            GameEvents.FireCriticalSaveTrigger("aurora_city_witnessed");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CLIMAX);
            GameEvents.FireCriticalSaveTrigger("moon9_climax_complete");
        }

        // ─── Beat 5: Revelation — Stone of Stars timestamp mystery; 17hr clock ─
        private IEnumerator Beat5_Revelation()
        {
            yield return new WaitForSeconds(2f);

            HUDController.Instance?.ShowBanner(
                "STONE OF STARS — ANOMALY",
                "The 6th stone shows a timestamp: the vision is dated to the Rhythmic Moon, 17th Hour — " +
                "the SAME time Zereth allegedly triggered the Flood. But in the vision, the bells ring in PERFECT HARMONY. Nothing is wrong.",
                13f);
            AudioManager.Instance?.PlaySFX2D("stone_of_stars_anomaly_sting");

            yield return new WaitForSeconds(5f);

            HUDController.Instance?.ShowBanner(
                "THE CENTRAL MYSTERY DEEPENS",
                "The Mud Flood happened AFTER the vision ends. What happened between the ringing bells and the cataclysm? " +
                "Three figures at a trigger device. One giant. Two humans.",
                12f);
            AudioManager.Instance?.PlaySFX2D("mystery_deepen_vo");

            GameEvents.FireCriticalSaveTrigger("mud_flood_timeline_questioned");

            // Install 17-hour clock
            yield return new WaitForSeconds(3f);
            HUDController.Instance?.ShowObjective(
                "Install the 17-hour clock mechanism in the bell tower.");

            yield return new WaitUntil(() => FindObjectOfType<Moon9ClockInstallPoint>()?.IsInstalled ?? true);

            _clockInstalled = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "clock_installed", true);

            HUDController.Instance?.ShowBanner(
                "17-HOUR CLOCK INSTALLED",
                "Permanent time-bend ability unlocked. During the 17th Hour, resonance sensitivity is doubled " +
                "and Aether yields from all structures increase.",
                9f);
            AudioManager.Instance?.PlaySFX2D("clock_install_chime");
            GameEvents.FireCriticalSaveTrigger("seventeen_hour_clock_unlocked");

            // Crossover seeds
            GameEvents.FireCriticalSaveTrigger("moon9_seed_stones_7_to_12_seeded");
            GameEvents.FireCriticalSaveTrigger("moon9_seed_zereth_confession_seeded");
            GameEvents.FireCriticalSaveTrigger("moon9_seed_aurora_city_live_ops");
            GameEvents.FireCriticalSaveTrigger("moon9_seed_prophecy_zones_upgraded");

            _moonCleared = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "moon_cleared", true);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_REVELATION);
            MoonProgressTracker.Instance?.MarkCleared(MOON_NUM);
            GameEvents.FireCriticalSaveTrigger("moon9_complete");

            HUDController.Instance?.ShowBanner(
                "SOLAR MOON — COMPLETE",
                "The prophecy stones remember. Zereth's voice lingers. The truth approaches.",
                8f);
            AudioManager.Instance?.PlaySFX2D("moon9_completion_sting");

            ApplyPersistentWorldState();
        }
    }

    // ─── Prophecy Stone Helper ──────────────────────────────────────────
    public class Moon9ProphecyStone : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _stoneIndex;
        public event System.Action<int> OnCollected;
        private bool _collected;

        public void Interact(GameObject interactor)
        {
            if (_collected) return;
            _collected = true;
            OnCollected?.Invoke(_stoneIndex);
            gameObject.SetActive(false);
        }

        public string GetInteractPrompt() => $"Collect Prophecy Stone — {_stoneIndex + 1} of 6";
        public bool CanInteract(GameObject interactor) => !_collected;
    }

    /// <summary>17-hour clock installation socket in a bell tower.</summary>
    public class Moon9ClockInstallPoint : MonoBehaviour, IInteractable
    {
        public bool IsInstalled { get; private set; }

        public void Interact(GameObject interactor)
        {
            if (IsInstalled) return;
            IsInstalled = true;
            AudioManager.Instance?.PlaySFX2D("clock_install_chime");
            gameObject.SetActive(false);
        }

        public string GetInteractPrompt() => "Install 17-Hour Clock Mechanism";
        public bool CanInteract(GameObject interactor) => !IsInstalled;
    }
}
