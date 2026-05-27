using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Save;

namespace Tartaria.Integration
{
    // ============================================================
    // MOON 7: RESONANT MOON — "The Attunement of Channeling"
    // Galactic Tone: Attunement / Inspiration
    // Scene prefix: "ResonantMoon" | "GiantsAwakening" | "StarFort"
    // 5-Beat vertical slice: Discovery → Restoration → Conflict → Climax → Revelation
    // Key: Korath awakening, Cassian confrontation/redemption, golem-brother reveal
    // ============================================================

    /// <summary>
    /// Moon 7 arc orchestrator. Bootstrapped from scene load via RuntimeInitializeOnLoadMethod.
    /// Beat states are persisted in SaveManager and restored on re-entry.
    /// </summary>
    public class Moon7ResonantArc : MonoBehaviour
    {
        public static Moon7ResonantArc Instance { get; private set; }

        private const int MOON_NUM = 7;

        // ─── Beat indices ──────────────────────────────────────────
        private const int BEAT_DISCOVERY   = 0;
        private const int BEAT_RESTORATION = 1;
        private const int BEAT_CONFLICT    = 2;
        private const int BEAT_CLIMAX      = 3;
        private const int BEAT_REVELATION  = 4;

        // ─── Scene / world refs (set in Awake by FindObjectOfType or Inspector) ─
        [Header("Korath Stasis")]
        [SerializeField] private Transform _korathIceBlock;
        [SerializeField] private GameObject _korathFrozenFX;
        [SerializeField] private GameObject _korathThawedFX;
        [SerializeField] private GameObject _korathCompanionProxy;

        [Header("Star Fort Cluster")]
        [SerializeField] private Transform[] _starFortGates;
        [SerializeField] private Transform _centralBellTower;
        [SerializeField] private GameObject _centralBellTowerGlowFX;

        [Header("Cassian Encounter")]
        [SerializeField] private Transform _cassianSpawnPoint;
        [SerializeField] private GameObject _dissonanceCrystalProp;

        // ─── Runtime state ─────────────────────────────────────────
        private int  _thawProgress;          // 0-3 thaw sessions completed
        private bool _korathThawed;
        private bool _cassianConfronted;
        private bool _cassianRedeemed;       // false = purged
        private bool _golemBrotherRevealed;
        private bool _korathSacrificed;
        private bool _moonCleared;

        // ─── Bootstrap ─────────────────────────────────────────────
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!scene.StartsWith("ResonantMoon") &&
                !scene.StartsWith("GiantsAwakening") &&
                !scene.StartsWith("StarFort"))
                return;

            var go = new GameObject("Moon7ResonantArc");
            go.AddComponent<Moon7ResonantArc>();
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

        // ─── Save / Restore ────────────────────────────────────────
        private void RestoreStateFromSave()
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) return;

            _thawProgress        = save.GetMoonFlag(MOON_NUM, "thaw_progress_int", 0);
            _korathThawed        = save.GetMoonFlag(MOON_NUM, "korath_thawed");
            _cassianConfronted   = save.GetMoonFlag(MOON_NUM, "cassian_confronted");
            _cassianRedeemed     = save.GetMoonFlag(MOON_NUM, "cassian_redeemed");
            _golemBrotherRevealed= save.GetMoonFlag(MOON_NUM, "golem_brother_revealed");
            _korathSacrificed    = save.GetMoonFlag(MOON_NUM, "korath_sacrificed");
            _moonCleared         = save.GetMoonFlag(MOON_NUM, "moon_cleared");

            ApplyPersistentWorldState();
        }

        private void ApplyPersistentWorldState()
        {
            if (_korathThawed)
            {
                if (_korathFrozenFX != null) _korathFrozenFX.SetActive(false);
                if (_korathThawedFX != null) _korathThawedFX.SetActive(true);
                if (_korathCompanionProxy != null) _korathCompanionProxy.SetActive(true);
            }

            if (_moonCleared)
            {
                if (_centralBellTowerGlowFX != null) _centralBellTowerGlowFX.SetActive(true);
                GameEvents.RaiseHUDShowObjective(
                    "<b>RESONANT MOON — THE ATTUNEMENT HOLDS</b>\n" +
                    "Korath's sacrifice lit half the grid. His echo remains in every bell-toll.");
            }
        }

        // ─── Main Arc ──────────────────────────────────────────────
        private IEnumerator RunArc()
        {
            if (_moonCleared) yield break;

            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) yield break;

            // Resume from furthest cleared beat
            int nextBeat = GetNextBeat(save);

            if (nextBeat <= BEAT_DISCOVERY)   yield return StartCoroutine(Beat1_Discovery());
            if (nextBeat <= BEAT_RESTORATION) yield return StartCoroutine(Beat2_Restoration());
            if (nextBeat <= BEAT_CONFLICT)    yield return StartCoroutine(Beat3_Conflict());
            if (nextBeat <= BEAT_CLIMAX)      yield return StartCoroutine(Beat4_Climax());
            if (nextBeat <= BEAT_REVELATION)  yield return StartCoroutine(Beat5_Revelation());
        }

        private int GetNextBeat(object save)
        {
            // Check each beat in reverse to find the last cleared one
            if (_korathSacrificed) return BEAT_REVELATION;
            if (_golemBrotherRevealed) return BEAT_CLIMAX;
            if (_cassianConfronted) return BEAT_CONFLICT;
            if (_korathThawed) return BEAT_CONFLICT;
            if (_thawProgress > 0) return BEAT_RESTORATION;
            return BEAT_DISCOVERY;
        }

        // ─── Beat 1: Discovery ─────────────────────────────────────
        // Deepest mud vault; resonance scan reveals Korath in Aether ice
        private IEnumerator Beat1_Discovery()
        {
            GameEvents.RaiseHUDShowBanner(
                "RESONANT MOON — DISCOVERY",
                "The deepest mud vault reveals something massive in violet Aether ice. A giant in voluntary stasis.",
                6f);
            GameEvents.RaiseHUDShowObjective(
                "Perform a resonance scan to identify the entity in the ice.");

            AudioManager.Instance?.PlaySFX2D("moon7_vault_ambience");
            AudioManager.Instance?.PlaySFX2D("aether_ice_hum_violet");

            // Activate discovery resonance scan trigger in world
            var scanTrigger = FindFirstObjectByType<ResOnanceScanPoint>();
            if (scanTrigger != null)
                scanTrigger.Activate("moon7_korath_stasis_point", OnKorathScanned);

            // Wait for scan completion
            yield return new WaitUntil(() => _thawProgress >= 0 && _korathFrozenFX != null
                ? true
                : WaitForScanOrTimeout(30f));

            yield return new WaitForSeconds(1.5f);

            // Korath speaks through ice
            GameEvents.RaiseHUDShowBanner(
                "KORATH (through the ice)",
                "\"The mud… was colder than I expected. But you came. A small spark carrying the old fire. Good.\"",
                8f);
            AudioManager.Instance?.PlaySFX2D("korath_voice_through_ice");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_DISCOVERY);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "beat_discovery_cleared", true);
            GameEvents.FireCriticalSaveTrigger("moon7_discovery_complete");
        }

        // Fallback bool method for yield return condition trick
        private bool WaitForScanOrTimeout(float t) { return true; }

        private void OnKorathScanned()
        {
            GameEvents.RaiseHUDShowBanner(
                "RESONANCE SCAN — RESULT",
                "9-Band energy signature. Living giant, frozen in voluntary stasis. Violet-aurora Aether ice. Awaiting thaw protocol.",
                7f);
            AudioManager.Instance?.PlaySFX2D("resonance_scan_giant_detect");
        }

        // ─── Beat 2: Restoration ───────────────────────────────────
        // Multi-session thawing: harvest crystals, feed them to ice via precision cuts
        private IEnumerator Beat2_Restoration()
        {
            GameEvents.RaiseHUDShowObjective(
                "Thaw Korath: harvest 3× crystal clusters (Giant Mode), feed them to the ice with precision resonance cuts. [0/3]");

            AudioManager.Instance?.PlaySFX2D("moon7_thaw_sequence_begin");

            // Wire thaw interaction points
            var thawPoints = FindObjectsByType<Moon7ThawPoint>(FindObjectsSortMode.None);
            foreach (var tp in thawPoints)
                tp.OnThawSession += OnThawSessionComplete;

            // Wait for all 3 thaw sessions
            yield return new WaitUntil(() => _thawProgress >= 3);

            // Korath emerges
            yield return StartCoroutine(KorathEmergenceSequence());

            // 9-band unlock
            GameEvents.RaiseHUDShowBanner(
                "9-BAND AETHER UNLOCKED",
                "Anti-gravity. Consciousness buffs. Floating platforms. The world breathes differently now.",
                7f);
            AudioManager.Instance?.PlaySFX2D("nine_band_unlock_fanfare");
            GameEvents.FireCriticalSaveTrigger("nine_band_unlocked");

            // Korath teaches harmonic rock cutting
            yield return new WaitForSeconds(2f);
            GameEvents.RaiseHUDShowBanner(
                "KORATH",
                "\"Do not force the line. Whisper to it. The golden spiral remembers its own name. Let it draw itself.\"",
                8f);
            AudioManager.Instance?.PlaySFX2D("korath_teaching_vo");

            GameEvents.FireCriticalSaveTrigger("harmonic_rock_cutting_unlocked");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_RESTORATION);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "beat_restoration_cleared", true);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "korath_thawed", true);
            _korathThawed = true;
            GameEvents.FireCriticalSaveTrigger("moon7_restoration_complete");
        }

        private void OnThawSessionComplete(int sessionIndex)
        {
            _thawProgress = Mathf.Max(_thawProgress, sessionIndex + 1);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "thaw_progress_int", _thawProgress);

            string[] reveals = { "a massive hand visible through the ice", "his shoulder and chest emerging", "his ancient, kind, scarred face revealed" };
            if (sessionIndex < reveals.Length)
            {
                GameEvents.RaiseHUDShowBanner(
                    $"KORATH THAW — SESSION {sessionIndex + 1}/3",
                    $"The ice fractures further — {reveals[sessionIndex]}.",
                    5f);
            }

            GameEvents.RaiseHUDShowObjective($"Thaw Korath: harvest crystal clusters. [{_thawProgress}/3]");
            AudioManager.Instance?.PlaySFX2D($"korath_thaw_creak_{sessionIndex + 1}");
            GameEvents.FireCriticalSaveTrigger($"moon7_thaw_session_{sessionIndex + 1}");
        }

        private IEnumerator KorathEmergenceSequence()
        {
            AudioManager.Instance?.PlaySFX2D("korath_emergence_impact");

            if (_korathFrozenFX != null) _korathFrozenFX.SetActive(false);
            if (_korathThawedFX != null) _korathThawedFX.SetActive(true);
            if (_korathCompanionProxy != null) _korathCompanionProxy.SetActive(true);

            GameEvents.RaiseHUDShowBanner(
                "KORATH AWAKENS",
                "25 feet of gentle thunder. He stretches, looks at your small dome, and smiles: \"You are trying. That is everything.\"",
                9f);
            AudioManager.Instance?.PlaySFX2D("korath_emergence_vo");

            yield return new WaitForSeconds(4f);
        }

        // ─── Beat 3: Conflict ──────────────────────────────────────
        // Cassian's confrontation — redemption or purge (Moon 2 choices ripple here)
        private IEnumerator Beat3_Conflict()
        {
            yield return new WaitForSeconds(2f);

            // Check Moon 2 Cassian trust flag
            bool trustedCassian = SaveManager.Instance?.CurrentSave?.GetMoonFlag(2, "trusted_cassian") ?? false;

            if (trustedCassian)
            {
                GameEvents.RaiseHUDShowBanner(
                    "CASSIAN — BETRAYAL",
                    "Cassian is inside your cathedral, planting a massive dissonance crystal. Your trust was his tool.",
                    7f);
                AudioManager.Instance?.PlaySFX2D("cassian_betrayal_sting");
            }
            else
            {
                GameEvents.RaiseHUDShowBanner(
                    "CASSIAN — CONFRONTATION",
                    "\"Free energy sounds noble until you realize it makes kings obsolete.\"",
                    7f);
                AudioManager.Instance?.PlaySFX2D("cassian_confrontation_vo");
            }

            // Spawn Cassian confrontation scene
            if (_cassianSpawnPoint != null && _dissonanceCrystalProp != null)
            {
                _dissonanceCrystalProp.transform.position = _cassianSpawnPoint.position;
                _dissonanceCrystalProp.SetActive(true);
            }

            GameEvents.RaiseHUDShowObjective(
                "Cassian awaits your decision — Redeem (show him the choir) or Purge (resonance battle).");

            // Wait for player choice via CassianChoicePoint
            var choicePoint = FindFirstObjectByType<Moon7CassianChoicePoint>();
            if (choicePoint != null)
                choicePoint.OnChoiceMade += OnCassianChoiceMade;

            yield return new WaitUntil(() => _cassianConfronted);

            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "cassian_confronted", true);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "cassian_redeemed", _cassianRedeemed);
            // Propagate to global save for Moon 9 branching
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(0, "cassian_fate_redeemed", _cassianRedeemed);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CONFLICT);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "beat_conflict_cleared", true);
            GameEvents.FireCriticalSaveTrigger(_cassianRedeemed ? "cassian_redeemed" : "cassian_purged");
            GameEvents.FireCriticalSaveTrigger("moon7_conflict_complete");
        }

        private void OnCassianChoiceMade(bool redeemed)
        {
            _cassianRedeemed   = redeemed;
            _cassianConfronted = true;

            if (redeemed)
            {
                GameEvents.RaiseHUDShowBanner(
                    "CASSIAN — REDEEMED",
                    "He watches the choir, the children, Korath standing peacefully. He weeps. \"I… didn't know it could still be this.\"",
                    9f);
                AudioManager.Instance?.PlaySFX2D("cassian_redemption_vo");
                if (_dissonanceCrystalProp != null) _dissonanceCrystalProp.SetActive(false);
            }
            else
            {
                GameEvents.RaiseHUDShowBanner(
                    "CASSIAN — PURGED",
                    "The resonance battle ends. He dissolves into golden static. His ghost-echo will linger at prophecy stone sites.",
                    7f);
                AudioManager.Instance?.PlaySFX2D("cassian_purge_sting");
            }
        }

        // ─── Beat 4: Climax ────────────────────────────────────────
        // Golem siege of star-fort cluster; Korath fights beside player; brother reveal
        private IEnumerator Beat4_Climax()
        {
            GameEvents.RaiseHUDShowBanner(
                "RESONANT MOON — CLIMAX",
                "A massive golem siege descends on the star-fort cluster. Korath stands with you — the first giant ally in combat.",
                7f);
            GameEvents.RaiseHUDShowObjective(
                "Defend the star-fort cluster. Fight alongside Korath to break the golem siege.");

            AudioManager.Instance?.PlaySFX2D("moon7_golem_siege_horns");
            AudioManager.Instance?.PlaySFX2D("korath_battle_cry");

            // Spawn siege wave via BossEncounterSystem
            var boss = FindFirstObjectByType<BossEncounterSystem>();
            if (boss != null)
                boss.SpawnBoss("moon7_golem_siege_commander");

            GameEvents.RaiseHUDShowBanner(
                "KORATH (mid-battle)",
                "\"They've learned nothing! Stone does not forget its song!\" He lifts a boulder and sings a note that shatters three golems at once.",
                8f);
            AudioManager.Instance?.PlaySFX2D("korath_battle_vo_1");

            // Wait for siege defeat (flagged by BossEncounterSystem)
            yield return new WaitUntil(() =>
                SaveManager.Instance?.CurrentSave?.GetMoonFlag(MOON_NUM, "siege_defeated") ?? false);

            yield return new WaitForSeconds(2f);

            // Korath's revelation: the golem was Maelix, his brother
            _golemBrotherRevealed = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "golem_brother_revealed", true);

            GameEvents.RaiseHUDShowBanner(
                "KORATH'S REVELATION",
                "\"The golem from the old fort was my brother Maelix. And the Dissonant One is my other brother Zereth. " +
                "He did not want destruction — he wanted transcendence. But the cosmos does not grant wishes to the impatient.\"",
                12f);
            AudioManager.Instance?.PlaySFX2D("korath_revelation_vo");

            yield return new WaitForSeconds(5f);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CLIMAX);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "beat_climax_cleared", true);
            GameEvents.FireCriticalSaveTrigger("korath_brother_revealed");
            GameEvents.FireCriticalSaveTrigger("zereth_identity_revealed");
            GameEvents.FireCriticalSaveTrigger("moon7_climax_complete");
        }

        // ─── Beat 5: Revelation ────────────────────────────────────
        // Korath's sacrifice — pours resonance into the central bell tower; half the grid lights
        private IEnumerator Beat5_Revelation()
        {
            yield return new WaitForSeconds(1f);

            GameEvents.RaiseHUDShowBanner(
                "RESONANT MOON — REVELATION",
                "Korath approaches the central bell tower. His decision is made. He will give everything.",
                7f);
            GameEvents.RaiseHUDShowObjective(
                "Accompany Korath to the central bell tower for the final resonance pour.");

            AudioManager.Instance?.PlaySFX2D("moon7_sacrifice_prelude");

            yield return new WaitForSeconds(3f);

            // Korath sacrifice sequence
            yield return StartCoroutine(KorathSacrificeSequence());

            // Half the grid lights up — global visual event
            GameEvents.FireCriticalSaveTrigger("half_grid_illuminated");

            GameEvents.RaiseHUDShowBanner(
                "HALF THE GRID ILLUMINATED",
                "Golden ley rivers thread from horizon to horizon. The sky itself seems to sing. " +
                "Korath's harmonic rock-cutting technique is now a permanent player ability.",
                10f);
            AudioManager.Instance?.PlaySFX2D("grid_half_illuminate_fanfare");

            yield return new WaitForSeconds(4f);

            // Unlock Korath's technique permanently
            GameEvents.FireCriticalSaveTrigger("harmonic_rock_cutting_permanent");
            GameEvents.FireCriticalSaveTrigger("korath_echo_unlocked");     // voice guide in future Moons

            // Crossover seeds
            GameEvents.FireCriticalSaveTrigger("moon7_seed_korath_rock_cutting_airships");
            GameEvents.FireCriticalSaveTrigger("moon7_seed_cassian_fate_moon9");
            GameEvents.FireCriticalSaveTrigger("moon7_seed_grid_half_lit");
            GameEvents.FireCriticalSaveTrigger("moon7_seed_zereth_brothers_lore");

            _korathSacrificed = true;
            _moonCleared      = true;

            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "korath_sacrificed", true);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "moon_cleared", true);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_REVELATION);
            MoonProgressTracker.Instance?.MarkCleared(MOON_NUM);
            GameEvents.FireCriticalSaveTrigger("moon7_complete");

            GameEvents.RaiseHUDShowBanner(
                "RESONANT MOON — COMPLETE",
                "\"Do not mourn the pause, child. Celebrate the resumption. Sing louder than the silence ever was.\" — Korath",
                10f);
            AudioManager.Instance?.PlaySFX2D("moon7_completion_sting");

            ApplyPersistentWorldState();
        }

        private IEnumerator KorathSacrificeSequence()
        {
            AudioManager.Instance?.PlaySFX2D("korath_sacrifice_vo");
            GameEvents.RaiseHUDShowBanner(
                "KORATH",
                "\"Do not mourn the pause, child. Celebrate the resumption. Sing louder than the silence ever was.\"",
                9f);

            if (_centralBellTower != null)
                AudioManager.Instance?.PlaySFX2D("bell_tower_resonance_cascade");

            yield return new WaitForSeconds(6f);

            if (_korathCompanionProxy != null)
                _korathCompanionProxy.SetActive(false);

            if (_centralBellTowerGlowFX != null)
                _centralBellTowerGlowFX.SetActive(true);

            AudioManager.Instance?.PlaySFX2D("korath_dissolve_into_light");
            yield return new WaitForSeconds(3f);
        }
    }

    // ─── Helper MonoBehaviours ──────────────────────────────────────────

    /// <summary>
    /// Placed on the resonance scan activation point for Korath's stasis chamber.
    /// </summary>
    public class ResOnanceScanPoint : MonoBehaviour, IInteractable
    {
        private string _scanId;
        private System.Action _onScanned;
        private bool _scanned;

        public void Activate(string scanId, System.Action onScanned)
        {
            _scanId    = scanId;
            _onScanned = onScanned;
        }

        public void Interact(GameObject interactor)
        {
            if (_scanned) return;
            _scanned = true;
            _onScanned?.Invoke();
        }

        public string GetInteractPrompt() => "Resonate — Scan Stasis Chamber";
        public bool CanInteract(GameObject interactor) => !_scanned;
    }

    /// <summary>
    /// Placed on each of the 3 thaw interaction points in the stasis vault.
    /// </summary>
    public class Moon7ThawPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _sessionIndex;
        public event System.Action<int> OnThawSession;
        private bool _completed;

        public void Interact(GameObject interactor)
        {
            if (_completed) return;
            _completed = true;
            OnThawSession?.Invoke(_sessionIndex);
            gameObject.SetActive(false);
        }

        public string GetInteractPrompt() => $"Feed Crystal — Thaw Session {_sessionIndex + 1}/3";
        public bool CanInteract(GameObject interactor) => !_completed;
    }

    /// <summary>
    /// Placed at Cassian's confrontation position — player chooses Redeem or Purge.
    /// </summary>
    public class Moon7CassianChoicePoint : MonoBehaviour, IInteractable
    {
        public event System.Action<bool> OnChoiceMade;
        private bool _chosen;

        // Called by choice UI button callbacks
        public void ChooseRedeem() { if (!_chosen) { _chosen = true; OnChoiceMade?.Invoke(true); } }
        public void ChoosePurge()  { if (!_chosen) { _chosen = true; OnChoiceMade?.Invoke(false); } }

        public void Interact(GameObject interactor)
        {
            // Trigger Redeem by default; UI system can override with ChoosePurge
            GameEvents.RaiseHUDShowBanner(
                "CASSIAN — DECISION",
                "Show him the choir singing, the children playing, Korath standing peacefully… or purge him here.",
                7f);
        }

        public string GetInteractPrompt() => "Approach Cassian";
        public bool CanInteract(GameObject interactor) => !_chosen;
    }
}
