using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Save;
using Tartaria.UI;

namespace Tartaria.Integration
{
    // ============================================================
    // MOON 13: COSMIC MOON — "The Transcendence of Enduring"
    // Galactic Tone: Transcendence / Presence
    // Scene prefix: "CosmicMoon" | "EchoRealm" | "FinalNode"
    // 5-Beat: Discovery → Echo Realms → Zereth Encounter → Climax → True Timeline
    // Key: Three Echo Realms; Zereth was FIRST VICTIM not villain;
    //      resonance-dialogue combat; Lirael fully solid; 17th Hour final node;
    //      ALL companions present; grid 100%; True Timeline revealed
    // ============================================================

    public class Moon13CosmicArc : MonoBehaviour
    {
        public static Moon13CosmicArc Instance { get; private set; }

        private const int MOON_NUM = 13;

        private const int BEAT_DISCOVERY    = 0;
        private const int BEAT_ECHO_REALMS  = 1;
        private const int BEAT_ZERETH       = 2;
        private const int BEAT_CLIMAX       = 3;
        private const int BEAT_TRUE_TIMELINE= 4;

        [Header("Echo Realm Portals")]
        [SerializeField] private GameObject _echoRealm1Portal;     // Golden Age
        [SerializeField] private GameObject _echoRealm2Portal;     // Dissonant Timeline
        [SerializeField] private GameObject _echoRealm3Portal;     // The Moment of the Flood

        [Header("Echo Realm FX")]
        [SerializeField] private GameObject _goldenAgeFX;
        [SerializeField] private GameObject _dissonantTimelineFX;
        [SerializeField] private GameObject _momentOfFloodFX;

        [Header("Zereth")]
        [SerializeField] private GameObject _zerethEchoProxy;
        [SerializeField] private Transform  _zerethEncounterAnchor;

        [Header("17th Hour Node")]
        [SerializeField] private Transform  _finalNodeSocket;
        [SerializeField] private GameObject _finalNodeFX;
        [SerializeField] private GameObject _trueTimelineFX;

        [Header("Lirael")]
        [SerializeField] private GameObject _liraelFullySolidProxy;
        [SerializeField] private GameObject _liraelEchoProxy;     // echo form used in realm 1

        private bool _echoRealm1Visited;
        private bool _echoRealm2Visited;
        private bool _echoRealm3Visited;
        private bool _zerethHealed;
        private bool _finalNodeActivated;
        private bool _liraelFullySolid;
        private bool _moonCleared;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!scene.StartsWith("CosmicMoon") &&
                !scene.StartsWith("EchoRealm") &&
                !scene.StartsWith("FinalNode"))
                return;

            var go = new GameObject("Moon13CosmicArc");
            go.AddComponent<Moon13CosmicArc>();
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

            _echoRealm1Visited   = save.GetMoonFlag(MOON_NUM, "echo_realm_1_visited");
            _echoRealm2Visited   = save.GetMoonFlag(MOON_NUM, "echo_realm_2_visited");
            _echoRealm3Visited   = save.GetMoonFlag(MOON_NUM, "echo_realm_3_visited");
            _zerethHealed        = save.GetMoonFlag(MOON_NUM, "zereth_healed");
            _finalNodeActivated  = save.GetMoonFlag(MOON_NUM, "final_node_activated");
            _liraelFullySolid    = save.GetMoonFlag(MOON_NUM, "lirael_fully_solid");
            _moonCleared         = save.GetMoonFlag(MOON_NUM, "moon_cleared");

            if (_moonCleared) ApplyPersistentWorldState();
        }

        private void ApplyPersistentWorldState()
        {
            if (_liraelFullySolid && _liraelFullySolidProxy != null)
                _liraelFullySolidProxy.SetActive(true);

            if (_finalNodeActivated && _finalNodeFX != null)
                _finalNodeFX.SetActive(true);

            if (_trueTimelineFX != null && _moonCleared)
                _trueTimelineFX.SetActive(true);

            if (_moonCleared)
                HUDController.Instance?.ShowObjective(
                    "<b>TARTARIA — RESTORED</b>\n" +
                    "Grid: 100%. True Timeline unveiled. Zereth healed. The world remembers.");
        }

        private IEnumerator RunArc()
        {
            if (_moonCleared) yield break;
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) yield break;

            if (!_echoRealm1Visited)   yield return StartCoroutine(Beat1_Discovery());
            if (!_echoRealm3Visited)   yield return StartCoroutine(Beat2_EchoRealms());
            if (!_zerethHealed)        yield return StartCoroutine(Beat3_ZerethEncounter());
            if (!_finalNodeActivated)  yield return StartCoroutine(Beat4_Climax());
            if (!_moonCleared)         yield return StartCoroutine(Beat5_TrueTimeline());
        }

        // ─── Beat 1: Discovery — arrival at origin star fort ───────
        private IEnumerator Beat1_Discovery()
        {
            HUDController.Instance?.ShowBanner(
                "COSMIC MOON — THE ORIGIN POINT",
                "The first star fort ever built. The foundation of the entire grid. " +
                "Three glowing portals surround the central 17th-Hour node — sealed until now.",
                10f);
            HUDController.Instance?.ShowObjective(
                "Enter Echo Realm 1 — The Golden Age.");

            AudioManager.Instance?.PlaySFX2D("origin_star_fort_ambient");
            AudioManager.Instance?.PlaySFX2D("echo_realm_portals_hum");

            if (_echoRealm1Portal != null) _echoRealm1Portal.SetActive(true);

            yield return new WaitForSeconds(3f);

            // All companions gather
            HUDController.Instance?.ShowBanner(
                "ALL COMPANIONS — ASSEMBLED",
                "Milo, Thorne, Lirael, Cassian (if redeemed), the junior engineers, Korath's final echo — " +
                "everyone stands at the threshold. They know this is the end.",
                10f);
            AudioManager.Instance?.PlaySFX2D("companions_assembled_stinger");

            yield return new WaitUntil(() => _echoRealm1Visited);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_DISCOVERY);
            GameEvents.FireCriticalSaveTrigger("moon13_discovery_complete");
        }

        // ─── Beat 2: Echo Realms — all three ───────────────────────
        private IEnumerator Beat2_EchoRealms()
        {
            // Realm 1: Golden Age
            if (!_echoRealm1Visited)
            {
                if (_goldenAgeFX != null) _goldenAgeFX.SetActive(true);

                HUDController.Instance?.ShowBanner(
                    "ECHO REALM 1 — THE GOLDEN AGE",
                    "The city at full glory: thousands of giants and humans living together. " +
                    "Bell towers singing. Trains running. Fountains glowing with ionized light. " +
                    "Children playing beneath feet of laughing giants. It was real. It was REAL.",
                    14f);
                AudioManager.Instance?.PlaySFX2D("echo_realm_1_golden_age_music");

                if (_liraelEchoProxy != null) _liraelEchoProxy.SetActive(true);
                HUDController.Instance?.ShowBanner(
                    "LIRAEL (in the Golden Age vision)",
                    "\"This is where I'm from. I was a record-keeper. I documented every song, " +
                    "every collaboration, every breakthrough. I remember all of it.\"",
                    10f);
                AudioManager.Instance?.PlaySFX2D("lirael_golden_age_memory_vo");

                _echoRealm1Visited = true;
                SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "echo_realm_1_visited", true);
                GameEvents.FireCriticalSaveTrigger("echo_realm_1_golden_age_witnessed");

                if (_goldenAgeFX != null) _goldenAgeFX.SetActive(false);
                if (_echoRealm1Portal != null) _echoRealm1Portal.SetActive(false);
                if (_echoRealm2Portal != null) _echoRealm2Portal.SetActive(true);
            }

            yield return new WaitForSeconds(2f);

            // Realm 2: Dissonant Timeline
            if (!_echoRealm2Visited)
            {
                if (_dissonantTimelineFX != null) _dissonantTimelineFX.SetActive(true);

                HUDController.Instance?.ShowBanner(
                    "ECHO REALM 2 — THE DISSONANT TIMELINE",
                    "The world if the Reset had won completely: eternal mud, silence, grey skies forever. " +
                    "No bell towers. No giants. No children. No song. " +
                    "This is what was planned for Tartaria.",
                    12f);
                AudioManager.Instance?.PlaySFX2D("echo_realm_2_dissonant_music");

                HUDController.Instance?.ShowBanner(
                    "MILO (shaken)",
                    "\"I grew up thinking this was normal. Slightly worse than this, but — " +
                    "this was the direction. We were heading HERE.\"",
                    9f);
                AudioManager.Instance?.PlaySFX2D("milo_dissonant_realm_vo");

                _echoRealm2Visited = true;
                SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "echo_realm_2_visited", true);
                GameEvents.FireCriticalSaveTrigger("echo_realm_2_dissonant_witnessed");

                if (_dissonantTimelineFX != null) _dissonantTimelineFX.SetActive(false);
                if (_echoRealm2Portal != null) _echoRealm2Portal.SetActive(false);
                if (_echoRealm3Portal != null) _echoRealm3Portal.SetActive(true);
            }

            yield return new WaitForSeconds(2f);

            // Realm 3: The Moment of the Flood — Zereth was FIRST VICTIM
            if (!_echoRealm3Visited)
            {
                if (_momentOfFloodFX != null) _momentOfFloodFX.SetActive(true);

                HUDController.Instance?.ShowBanner(
                    "ECHO REALM 3 — THE MOMENT OF THE FLOOD",
                    "The 17th Hour. Zereth at the origin node — not the trigger device. " +
                    "He was conducting a 9-band transcendence experiment: an attempt to permanently anchor " +
                    "the grid at full resonance without further maintenance.",
                    14f);
                AudioManager.Instance?.PlaySFX2D("echo_realm_3_flood_music");

                yield return new WaitForSeconds(6f);

                HUDController.Instance?.ShowBanner(
                    "THE TRUTH — PARASITE CABAL REVERSAL",
                    "Two humans at the trigger device — the Parasite Cabal — " +
                    "reverse the polarity of Zereth's experiment. His own equipment becomes the weapon. " +
                    "The sonic shockwave hits him first. He is the first to fall.",
                    14f);
                AudioManager.Instance?.PlaySFX2D("flood_reversal_moment_vo");

                yield return new WaitForSeconds(6f);

                HUDController.Instance?.ShowBanner(
                    "ZERETH WAS THE FIRST VICTIM",
                    "He wasn't the villain. He was the inventor whose creation was weaponized against his people. " +
                    "He has been trapped in the moment of that betrayal ever since — " +
                    "in infinite pain, thinking HE caused the Flood.",
                    14f);
                AudioManager.Instance?.PlaySFX2D("zereth_truth_reveal_sting");

                _echoRealm3Visited = true;
                SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "echo_realm_3_visited", true);
                GameEvents.FireCriticalSaveTrigger("zereth_true_story_revealed");
                GameEvents.FireCriticalSaveTrigger("parasite_cabal_named_as_flood_cause");

                if (_momentOfFloodFX != null) _momentOfFloodFX.SetActive(false);
                if (_echoRealm3Portal != null) _echoRealm3Portal.SetActive(false);
            }

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_ECHO_REALMS);
            GameEvents.FireCriticalSaveTrigger("moon13_echo_realms_complete");
        }

        // ─── Beat 3: Zereth Encounter — resonance dialogue, not combat ──────
        private IEnumerator Beat3_ZerethEncounter()
        {
            yield return new WaitForSeconds(2f);

            if (_zerethEchoProxy != null) _zerethEchoProxy.SetActive(true);

            HUDController.Instance?.ShowBanner(
                "ZERETH — CONFRONTATION",
                "Zereth's full echo manifests: a massive, fractured giant-spirit made of dark resonance and ancient grief. " +
                "He speaks in discordant waves that shake the star fort foundations. " +
                "This is NOT a boss fight. This is a conversation.",
                12f);
            AudioManager.Instance?.PlaySFX2D("zereth_full_manifestation");
            HUDController.Instance?.ShowObjective(
                "Meet Zereth's anger with resonance harmony. Do not fight — respond.");

            yield return new WaitForSeconds(5f);

            HUDController.Instance?.ShowBanner(
                "ZERETH (in anguish)",
                "\"I MADE this. Every crack in the earth. Every buried city. " +
                "Every child who never heard a bell — I MADE THAT. " +
                "Do not restore me. I do not deserve restoration.\"",
                12f);
            AudioManager.Instance?.PlaySFX2D("zereth_anguish_wave_1");

            yield return new WaitForSeconds(5f);

            // Player responds with 9-band harmony
            HUDController.Instance?.ShowBanner(
                "RESPOND — 9-BAND RESONANCE HARMONY",
                "Play back the pattern Zereth himself designed: the 9-band transcendence chord. " +
                "Not as power — as recognition.",
                9f);
            AudioManager.Instance?.PlaySFX2D("player_zereth_harmony_attempt");

            yield return new WaitForSeconds(4f);

            HUDController.Instance?.ShowBanner(
                "ZERETH (destabilizing)",
                "\"That's... mine. You learned my chord. " +
                "You know what it was supposed to DO. You know I wasn't—\" [silence]",
                10f);
            AudioManager.Instance?.PlaySFX2D("zereth_recognition_stutter");

            yield return new WaitForSeconds(4f);

            // Lirael steps forward — the lullaby
            HUDController.Instance?.ShowBanner(
                "LIRAEL — STEPS FORWARD",
                "Lirael steps directly into Zereth's resonance field — becoming MORE solid, not less. " +
                "She sings the lullaby the giants sang to their children on the night of the Flood.",
                11f);
            AudioManager.Instance?.PlaySFX2D("lirael_zereth_lullaby");

            yield return new WaitForSeconds(8f);

            HUDController.Instance?.ShowBanner(
                "ZERETH (breaking)",
                "\"...I remember that song. My mother sang it. " +
                "She was singing it when the wave hit her.\" " +
                "[The dark resonance fractures. Gold light bleeds through.]",
                13f);
            AudioManager.Instance?.PlaySFX2D("zereth_breaking_open");

            yield return new WaitForSeconds(6f);

            HUDController.Instance?.ShowBanner(
                "THE TRUTH LANDS",
                "\"They used me. They used my chord. And I have spent eternity — " +
                "punishing a world that was also a victim. " +
                "I... I want to stop.\"",
                12f);
            AudioManager.Instance?.PlaySFX2D("zereth_surrender_vo");

            yield return new WaitForSeconds(5f);

            _zerethHealed = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "zereth_healed", true);
            GameEvents.FireCriticalSaveTrigger("zereth_healed");
            GameEvents.FireCriticalSaveTrigger("zereth_guilt_released");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_ZERETH);
            GameEvents.FireCriticalSaveTrigger("moon13_zereth_complete");
        }

        // ─── Beat 4: Climax — 17th Hour final node, all companions ─────────
        private IEnumerator Beat4_Climax()
        {
            yield return new WaitForSeconds(2f);

            // Lirael goes fully solid
            _liraelFullySolid = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "lirael_fully_solid", true);
            if (_liraelFullySolidProxy != null) _liraelFullySolidProxy.SetActive(true);
            if (_liraelEchoProxy != null)       _liraelEchoProxy.SetActive(false);

            HUDController.Instance?.ShowBanner(
                "LIRAEL — FULLY PRESENT",
                "The fountain network, the resonance healing, Zereth's release — it's enough. " +
                "Lirael is fully solid. She reaches out and takes your hand.",
                9f);
            AudioManager.Instance?.PlaySFX2D("lirael_fully_solid_moment");
            GameEvents.FireCriticalSaveTrigger("lirael_fully_solid");

            yield return new WaitForSeconds(4f);

            HUDController.Instance?.ShowBanner(
                "THE 17TH HOUR — FINAL NODE",
                "All companions stand at the origin node. " +
                "Zereth's healing consciousness flows into the grid as a permanent healing force. " +
                "The final 5% ignites.",
                11f);
            HUDController.Instance?.ShowObjective(
                "Activate the 17th Hour final node — together.");

            AudioManager.Instance?.PlaySFX2D("final_node_activation_buildup");

            if (_finalNodeFX != null) _finalNodeFX.SetActive(true);

            yield return new WaitForSeconds(10f);

            HUDController.Instance?.ShowBanner(
                "MILO",
                "\"I just want to say — I started this thinking there'd be profit in it. " +
                "There's not. It's better than that.\"",
                8f);
            AudioManager.Instance?.PlaySFX2D("milo_final_vo");
            yield return new WaitForSeconds(3f);

            HUDController.Instance?.ShowBanner(
                "THORNE",
                "\"A thousand years I've waited for this moment. It was worth every one.\"",
                7f);
            AudioManager.Instance?.PlaySFX2D("thorne_final_vo");
            yield return new WaitForSeconds(3f);

            HUDController.Instance?.ShowBanner(
                "KORATH ECHO (from every bell tower simultaneously)",
                "\"NOW. ALL OF YOU. TOGETHER. SING.\"",
                7f);
            AudioManager.Instance?.PlaySFX2D("korath_final_command_all_bells");

            yield return new WaitForSeconds(4f);

            _finalNodeActivated = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "final_node_activated", true);
            GameEvents.FireCriticalSaveTrigger("final_node_17th_hour_activated");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CLIMAX);
            GameEvents.FireCriticalSaveTrigger("moon13_climax_complete");
        }

        // ─── Beat 5: True Timeline — grid 100%, world restored ─────────────
        private IEnumerator Beat5_TrueTimeline()
        {
            yield return new WaitForSeconds(3f);

            AudioManager.Instance?.PlaySFX2D("grid_100_percent_surge");

            HUDController.Instance?.ShowBanner(
                "GRID — 100%",
                "The Aether grid ignites fully. The entire continental network burns gold.",
                8f);
            GameEvents.FireCriticalSaveTrigger("grid_at_100_percent");

            yield return new WaitForSeconds(5f);

            if (_trueTimelineFX != null) _trueTimelineFX.SetActive(true);

            HUDController.Instance?.ShowBanner(
                "THE TRUE TIMELINE — UNVEILED",
                "Layer by layer, the Reset's false history strips away. " +
                "The real timeline appears beneath: a thousand-year civilization of resonance, cooperation, and shared sky.",
                13f);
            AudioManager.Instance?.PlaySFX2D("true_timeline_reveal_music");

            yield return new WaitForSeconds(8f);

            HUDController.Instance?.ShowBanner(
                "TARTARIA — REMEMBERED",
                "Not a nation. Not an empire. A way of living. " +
                "Humans and giants building together. Sound as the universal language. " +
                "Water as Aether. Bells as memory. Children as the reason for everything.",
                14f);

            yield return new WaitForSeconds(7f);

            HUDController.Instance?.ShowBanner(
                "ZERETH — MERGED WITH THE GRID",
                "His consciousness disperses into the resonance network — not as punishment, not as prison. " +
                "As a permanent healing force. His nine-band chord plays softly now in every bell tower, forever.",
                12f);
            AudioManager.Instance?.PlaySFX2D("zereth_merge_final_chord");
            GameEvents.FireCriticalSaveTrigger("zereth_merged_with_grid");

            yield return new WaitForSeconds(6f);

            HUDController.Instance?.ShowBanner(
                "LIRAEL (fully present, smiling)",
                "\"I finished my record. Every song. Every collaboration. Every breakthrough. " +
                "Including this one. Especially this one.\"",
                10f);
            AudioManager.Instance?.PlaySFX2D("lirael_final_vo");

            yield return new WaitForSeconds(5f);

            // Final crossover seeds
            GameEvents.FireCriticalSaveTrigger("moon13_seed_true_timeline_unlocked");
            GameEvents.FireCriticalSaveTrigger("moon13_seed_zereth_healed");
            GameEvents.FireCriticalSaveTrigger("moon13_seed_grid_complete");
            GameEvents.FireCriticalSaveTrigger("moon13_seed_golden_age_restored");
            GameEvents.FireCriticalSaveTrigger("tartaria_fully_restored");

            _moonCleared = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "moon_cleared", true);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_TRUE_TIMELINE);
            MoonProgressTracker.Instance?.MarkCleared(MOON_NUM);
            GameEvents.FireCriticalSaveTrigger("moon13_complete");
            GameEvents.FireCriticalSaveTrigger("game_complete");   // GameCompleteOverlay subscribes here

            HUDController.Instance?.ShowBanner(
                "TARTARIA — COMPLETE",
                "All 13 moons. All 13 bells. The golden age begins again.",
                12f);
            AudioManager.Instance?.PlaySFX2D("game_complete_credits_theme");

            // Explicit Show() call as belt-and-suspenders in case event was missed
            Tartaria.UI.GameCompleteOverlay.Instance?.Show();

            ApplyPersistentWorldState();
        }
    }
}
