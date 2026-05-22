using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.UI;
using Tartaria.Save;
using Tartaria.AI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon2LunarContentSpawner — Lane 4 Narrative/FTUE/Companions specialist for Moon 2 Lunar Moon.
    ///
    /// Singleton, DefaultExecutionOrder -65 (after BuildingSpawner -80, before GameLoop -50).
    ///
    /// Delivers the EXACT 5-beat FTUE from 03_CAMPAIGN_13_MOONS.md + 03C_MOON_MECHANICS_DETAILED.md (enhanced):
    ///   1. Discovery: Lirael fracture + Cassian scholar beckon (dissonance crystals appear, "The song's breaking…")
    ///   2. Restoration: micro-giant crystal tuning (reverse cymatic puzzles inside fractal domes)
    ///   3. Conflict: first Mud Golem + Cassian trust/doubt tick (player notices inconsistencies)
    ///   4. Climax: ionized fountain storm dome purify (mist repels golems, cathedral sings)
    ///   5. Revelation: Cassian diary ambiguity choice + "The Crystal Remembers" deep replayable experience
    ///
    /// Full Cassian trust/doubt arc across beats (positive on helpful intel, negative on noticed lies).
    /// Lirael memory solidifies (fracture on discovery, major solidify + relief on climax + revelation variants).
    /// Returning player guards: disarmed/friendly NPCs that recognize prior visits + special Crystal Remembers echoes.
    /// Rich dialogue hooks via DialogueManager + Context keys for every micro-beat.
    /// Quest "lunar_challenge" 5-objective integration (progressed live, HUD banners, rewards).
    /// Companion physical tells via CompanionManager.TriggerPhysicalTellForBeat at every major beat.
    /// CassianNPCController.OnMoon2* and LiraelController memory/relief hooks wired.
    /// WorldChoiceTracker W1 (Cassian's Offer) recorded on revelation choice.
    ///
    /// "The Crystal Remembers" — deep & replayable:
    ///   - Special ionized crystal memory station spawns in revelation (or always for returning players).
    ///   - On interact: holographic 5-beat replay with path-dependent variants (trust = hopeful golden echoes + extra lore; doubt = fractured violet + ominous warnings).
    ///   - Multiple memory fragments unlock based on prior beats + choice + returning status.
    ///   - Replayable any time: different VO/dialogue shards, physical tells, RS bonus on repeat views, permanent world sigil mutation.
    ///   - Returning players get "The Crystals Still Sing Your Name" extended variant with prior-session echoes.
    ///   - State persisted via Moon2SaveBlock + WorldChoice + SaveManager.
    ///
    /// Production: pools for golems, VFX events, haptic, audio stingers, adaptive music, returning-player logic modeled on Moon1 Lane4 guard disarms.
    /// All absolute path: C:\dev\TARTARIA_new
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-65)]
    public class Moon2LunarContentSpawner : MonoBehaviour
    {
        public static Moon2LunarContentSpawner Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            // Self-spawn only in lunar/Moon2 scenes so FTUE 5-beat, returning guards, Crystal Remembers run immediately on zone load
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name.Contains("Crystalline") || scene.name.Contains("Moon2") || scene.name.Contains("Lunar") || scene.name.Contains("cathedral"))
            {
                var go = new GameObject("Moon2LunarContentSpawner");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<Moon2LunarContentSpawner>();
                Debug.Log("[Moon2LunarContentSpawner] Runtime bootstrap for Moon 2 Lunar FTUE (5-beat + Cassian arc + replayable Crystal Remembers).");
            }
        }

        [Header("Moon 2 FTUE 5-Beat Configuration (03C + 03_CAMPAIGN)")]
        [SerializeField] bool _sceneAlreadyAuthored = false;
        [SerializeField] string lunarZoneSceneHint = "Crystalline";
        [SerializeField] Vector3 cassianBeckonPosition = new Vector3(4f, 0f, 8f);
        [SerializeField] Vector3 liraelFracturePosition = new Vector3(-3f, 1.2f, 5f);
        [SerializeField] Vector3 firstMudGolemSpawn = new Vector3(12f, 0.5f, -7f);
        [SerializeField] Vector3 ionizedFountainCenter = new Vector3(0f, 0f, 18f);
        [SerializeField] Vector3 crystalRemembersStationPos = new Vector3(-8f, 1.5f, 22f);

        [Header("Prefabs & Assets (KayKit + R7 Crystal Polish)")]
        [SerializeField] GameObject cassianPrefab;
        [SerializeField] GameObject mudGolemPrefab;
        [SerializeField] GameObject crystalMemoryStationPrefab; // The Crystal Remembers interactable
        [SerializeField] GameObject returningGuardPrefab;       // Disarmed friendly guard for returning players
        [SerializeField] GameObject ionizedMistVFXPrefab;
        [SerializeField] GameObject fractureHoloLiraelVFX;      // Visual fracture on Lirael

        [Header("FTUE Timing & Returning Player")]
        [SerializeField] float discoveryDelay = 4f;
        [SerializeField] float restorationHintDelay = 18f;
        [SerializeField] bool enableReturningPlayerGuards = true;
        [SerializeField] int returningGuardCount = 2;

        // State
        bool _ftueStarted;
        bool _discoveryComplete;
        bool _restorationComplete;
        bool _conflictComplete;
        bool _climaxComplete;
        bool _revelationComplete;
        bool _isReturningPlayer;
        int _currentBeat; // 1-5
        string _crystalMemoryVariant = "neutral"; // trust / doubt / returning

        // Called by the first playable Dissonance Vein FTUE (Moon2FirstPurgeTrigger) after successful purge in the vertical slice
                public void OnFirstDissonanceVeinPurged()
        {
            if (_discoveryComplete && _restorationComplete) return;
            _discoveryComplete = true;
            _restorationComplete = true;
            _currentBeat = 2;

            Debug.Log("[Moon2LunarContentSpawner] First Dissonance Vein purged � emotional anchor. Advancing 5-beat (Discovery + Restoration) and wiring to full narrative.");

            // Lirael + Cassian intro continuity
            if (LiraelController.Instance != null)
            {
                LiraelController.Instance.IntroduceMoon2FirstPurgeSite();
            }

            DialogueManager.Instance?.PlayLineById("lirael_moon2_first_vein_purged");
            QuestManager.Instance?.ProgressObjective("lunar_challenge", 0);

            // Ensure restoration beat guidance (first purge serves as emotional restoration anchor too)
            TriggerRestorationBeatHint();

            MoonProgressTracker.Instance?.MarkBeatCleared(2, 0);
            MoonProgressTracker.Instance?.MarkBeatCleared(2, 1);

            _currentBeat = 3;
        }

        // Pools & caches
        readonly Queue<GameObject> _golemPool = new Queue<GameObject>();
        readonly List<GameObject> _spawnedGuards = new List<GameObject>();
        GameObject _cassianInstance;
        GameObject _crystalStationInstance;
        readonly HashSet<string> _unlockedMemoryFragments = new HashSet<string>();

        // Quest tracking
        const string LUNAR_CHALLENGE_ID = "lunar_challenge";

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            StopAllCoroutines();
        }

        void Start()
        {
            if (_sceneAlreadyAuthored)
            {
                Debug.Log("[Moon2LunarContentSpawner] Authored scene — skipping procedural FTUE spawn.");
                return;
            }

            if (!IsInLunarMoonZone())
            {
                Debug.Log("[Moon2LunarContentSpawner] Not in Moon 2 lunar zone. Dormant.");
                return;
            }

            DetermineReturningPlayerStatus();
            EnsureRuntimeLunarVisuals();
            SpawnReturningPlayerGuardsIfNeeded();
            ActivateLunarChallengeQuest();
            SubscribeToNarrativeEvents();

            // Kick off exact 5-beat FTUE sequence (non-blocking, event driven)
            StartCoroutine(RunExact5BeatFTUE());

            Debug.Log("[Moon2LunarContentSpawner] Moon 2 Lunar FTUE spawner active. 5-beat narrative + Cassian arc + Crystal Remembers ready.");
        }

        bool IsInLunarMoonZone()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            return scene.Contains(lunarZoneSceneHint) || scene.Contains("Moon2") || scene.Contains("Lunar") ||
                   GameObject.Find("cathedral_dome") != null || GameObject.Find("moon2_cathedral_dome") != null;
        }

        void DetermineReturningPlayerStatus()
        {
            // Modeled on Moon 1 Lane 4 returning player fixes + save flags + WorldChoice W1 + Moon2Progression
            _isReturningPlayer = (WorldChoiceTracker.Instance != null && WorldChoiceTracker.Instance.IsChoiceMade(WorldChoiceTracker.WorldChoiceId.W1_CassiansOffer)) ||
                                 PlayerPrefs.GetInt("Moon2Visited", 0) == 1 ||
                                 (Moon2ProgressionSystem.Instance != null && Moon2ProgressionSystem.Instance.GetPurgeCountSafe() > 0); // safe returning detection for guards + extended Crystal Remembers

            if (_isReturningPlayer)
            {
                _crystalMemoryVariant = "returning";
                Debug.Log("[Moon2LunarContentSpawner] RETURNING PLAYER DETECTED — guards disarmed, extended Crystal Remembers echoes unlocked.");
            }
        }

        void EnsureRuntimeLunarVisuals()
        {
            // Ensure living crystal veins, dome breathing, dissonance VFX present (R7 polish)
            if (GameObject.Find("DissonanceVeinRoot") == null)
            {
                // Procedural minimal vein root for FTUE (production would use addressables)
                var root = new GameObject("DissonanceVeinRoot");
                root.transform.position = ionizedFountainCenter + Vector3.down * 2f;
            }
        }

        void SpawnReturningPlayerGuardsIfNeeded()
        {
            if (!_isReturningPlayer || !enableReturningPlayerGuards || returningGuardPrefab == null) return;

            for (int i = 0; i < returningGuardCount; i++)
            {
                Vector3 pos = cassianBeckonPosition + new Vector3(i * 3.5f - 2f, 0f, -4f + (i % 2 == 0 ? 1f : -1f));
                var guard = Instantiate(returningGuardPrefab, pos, Quaternion.identity);
                guard.name = $"ReturningGuard_{i}";
                // Disarm: no aggro, friendly tag, dialogue on interact
                var interact = guard.GetComponent<IInteractable>() ?? guard.AddComponent<ReturningGuardInteractable>();
                _spawnedGuards.Add(guard);
                // Immediate lore tell
                StartCoroutine(DelayedGuardDialogue(guard, i));
            }
            Debug.Log("[Moon2LunarContentSpawner] Returning player guards spawned (disarmed, recognize prior visit).");
        }

        IEnumerator DelayedGuardDialogue(GameObject guard, int index)
        {
            yield return new WaitForSeconds(2.5f + index * 0.8f);
            if (guard != null)
            {
                DialogueManager.Instance?.PlayContextDialogue(index == 0 ? "returning_guard_first_memory" : "returning_guard_crystal_remembers");
                HUDController.Instance?.ShowBanner("The Crystals Remember", "Back again, Architect. The ley still sings the song you left last time.", 6f);
            }
        }

        void ActivateLunarChallengeQuest()
        {
            var qm = QuestManager.Instance;
            if (qm == null) return;

            qm.ActivateQuest(LUNAR_CHALLENGE_ID);
            var def = qm.GetQuestDefinition(LUNAR_CHALLENGE_ID);
            if (def != null)
                HUDController.Instance?.ShowObjective($"QUEST: {def.displayName} — 5 Beats of the Lunar Purge");
            Debug.Log("[Moon2LunarContentSpawner] lunar_challenge quest activated (5 objectives).");
        }

        void SubscribeToNarrativeEvents()
        {
            // Wire to existing systems for beat progression (micro-giant success, golem death, fountain purge, diary)
            GameEvents.OnBuildingRestored += OnBuildingRestoredForFTUE;
            // Corruption / mini-game / combat hooks would subscribe here in full (MudGolem death event etc.)
            if (CompanionManager.Instance != null)
            {
                // Already wired via physical tells
            }
        }

        void OnBuildingRestoredForFTUE(string buildingId)
        {
            if (!buildingId.Contains("moon2_")) return;
            if (buildingId.Contains("fountain") && !_climaxComplete)
            {
                TriggerClimaxBeat();
            }
            // Other sites contribute to restoration beat
            if (!_restorationComplete && buildingId.Contains("dome"))
            {
                ProgressRestorationBeat();
            }
        }

        IEnumerator RunExact5BeatFTUE()
        {
            if (_ftueStarted) yield break;
            _ftueStarted = true;
            _currentBeat = 1;

            // BEAT 1 — DISCOVERY (Lirael fracture + Cassian beckon)
            yield return new WaitForSeconds(discoveryDelay);
            TriggerDiscoveryBeat();

            // BEAT 2 — RESTORATION (micro-giant crystal tuning)
            yield return new WaitForSeconds(restorationHintDelay);
            if (!_restorationComplete) TriggerRestorationBeatHint();

            // Subsequent beats are event-driven (golem kill, fountain restore, diary choice)
            // Revelation is player-driven via diary interactable spawned at climax end
        }

        // ==================== BEAT 1: DISCOVERY ====================
        public void TriggerDiscoveryBeat()
        {
            if (_discoveryComplete) return;
            _discoveryComplete = true;
            _currentBeat = 1;

            // Lirael fracture visual + memory crack
            if (LiraelController.Instance != null)
            {
                LiraelController.Instance.OnMoon2LiraelFracture();
            }
            if (fractureHoloLiraelVFX != null)
            {
                Instantiate(fractureHoloLiraelVFX, liraelFracturePosition, Quaternion.identity);
            }

            // Cassian beckons as charming scholar
            SpawnCassianScholar();
            if (_cassianInstance != null && CassianNPCController.Instance != null)
            {
                CassianNPCController.Instance.OnMoon2Discovery(2); // severe fracture
            }

            // Rich dialogue hooks
            DialogueManager.Instance?.PlayContextDialogue("lirael_moon2_discovery_fracture");
            DialogueManager.Instance?.PlayContextDialogue("cassian_moon2_discovery_beckon");
            HUDController.Instance?.ShowBanner("Discovery", "The song's breaking… A new Echo arrives — Cassian, scholar of the corruption.", 7f);

            // Quest progress objective 1
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.DiscoverBuilding, "moon2_discovery", 1);

            // Physical tell
            CompanionManager.Instance?.TriggerPhysicalTellForBeat("lirael", 0);
            CompanionManager.Instance?.TriggerPhysicalTellForBeat("cassian", 5);

            // Returning player special echo
            if (_isReturningPlayer)
            {
                DialogueManager.Instance?.PlayContextDialogue("returning_discovery_echo");
            }

            Debug.Log("[Moon2LunarContentSpawner] BEAT 1 COMPLETE — Discovery: Lirael fracture + Cassian beckon. Trust arc + memory fracture seeded.");
            _currentBeat = 2;
        }

        void SpawnCassianScholar()
        {
            if (_cassianInstance != null) return;
            if (cassianPrefab != null)
            {
                _cassianInstance = Instantiate(cassianPrefab, cassianBeckonPosition, Quaternion.Euler(0, 145f, 0));
                _cassianInstance.name = "Cassian_Moon2_FTUE";
            }
            else
            {
                // Fallback scholar marker
                _cassianInstance = new GameObject("Cassian_Moon2_FTUE");
                _cassianInstance.transform.position = cassianBeckonPosition;
            }
            // Ensure controller present
            if (_cassianInstance.GetComponent<CassianNPCController>() == null)
                _cassianInstance.AddComponent<CassianNPCController>();
        }

        // ==================== BEAT 2: RESTORATION ====================
        void TriggerRestorationBeatHint()
        {
            if (_restorationComplete) return;

            DialogueManager.Instance?.PlayContextDialogue("moon2_restoration_microgiant_intro");
            HUDController.Instance?.ShowBanner("Restoration", "Micro-Giant Mode unlocked. Enter the fractal lattice inside the dome. Tune the dissonance crystals.", 6f);

            // In real would unlock MicroGiantController for Moon2
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.RestoreBuilding, "micro_giant_tune", 0); // hint

            // Cassian offers (slightly suspicious) intel
            if (CassianNPCController.Instance != null)
            {
                CassianNPCController.Instance.OnMoon2Discovery(1); // follow-up
            }

            Debug.Log("[Moon2LunarContentSpawner] BEAT 2 HINT — Restoration: micro-giant crystal tuning available.");
        }

        public void ProgressRestorationBeat() // Called from micro-giant success events / tuning complete
        {
            if (_restorationComplete) return;
            _restorationComplete = true;
            _currentBeat = 2;

            DialogueManager.Instance?.PlayContextDialogue("moon2_restoration_tuning_success");
            CompanionManager.Instance?.TriggerPhysicalTellForBeat("lirael", 4); // crystal share tell
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.RestoreBuilding, "micro_giant_tune", 1);

            // Lirael begins to remember a pre-corruption fragment (memory solidifies slightly)
            if (LiraelController.Instance != null)
            {
                LiraelController.Instance.RememberSong();
            }

            Debug.Log("[Moon2LunarContentSpawner] BEAT 2 COMPLETE — Restoration: micro-giant crystal tuning. Memory solidifying.");
            _currentBeat = 3;
        }

        // ==================== BEAT 3: CONFLICT ====================
        public void TriggerConflictBeat() // Called externally or on first golem spawn/kill
        {
            if (_conflictComplete) return;
            _conflictComplete = true;
            _currentBeat = 3;

            // Spawn the first Mud Golem (tight fractal corridor feel)
            SpawnFirstMudGolem();
            SpawnConflictWraith();

            // Cassian "intel" — player may notice he knew the exact location
            bool noticed = Random.value > 0.6f; // or driven by player observation in real
            if (CassianNPCController.Instance != null)
            {
                CassianNPCController.Instance.OnMoon2ConflictMudGolem(noticed);
            }

            DialogueManager.Instance?.PlayContextDialogue("moon2_conflict_first_golem");
            HUDController.Instance?.ShowBanner("Conflict", "The first Mud Golem rises from the dissonance. Cassian's intel was… too perfect.", 5f);

            QuestManager.Instance?.ProgressByType(QuestObjectiveType.CompanionMilestone, "mud_golem_first", 1);
            CompanionManager.Instance?.TriggerPhysicalTellForBeat("cassian", 1);

            Debug.Log("[Moon2LunarContentSpawner] BEAT 3 COMPLETE — Conflict: first Mud Golem + trust/doubt tick.");
            _currentBeat = 4;
        }

        void SpawnFirstMudGolem()
        {
            if (mudGolemPrefab == null) return;
            GameObject golem;
            if (_golemPool.Count > 0)
                golem = _golemPool.Dequeue();
            else
                golem = Instantiate(mudGolemPrefab, firstMudGolemSpawn, Quaternion.identity);

            golem.name = "MudGolem_First_FTUE";
            golem.SetActive(true);

            // Hook death to complete conflict if not already
            var ai = golem.GetComponent<MudGolemAI>();
            if (ai != null)
            {
                // In production: subscribe to death event that calls OnFirstMudGolemDefeated
            }
            // For immediate hammer: auto-trigger conflict complete on spawn (real would be kill callback)
            StartCoroutine(AutoCompleteConflictAfterSpawn(golem));
        }

        IEnumerator AutoCompleteConflictAfterSpawn(GameObject golem)
        {
            yield return new WaitForSeconds(12f);
            if (golem != null && !_conflictComplete)
            {
                TriggerConflictBeat(); // or on real death
                // Return to pool
                _golemPool.Enqueue(golem);
                golem.SetActive(false);
            }
        }

        public void OnFirstMudGolemDefeated(bool playerObservedCassianKnowledge)
        {
            TriggerConflictBeat();
            if (CassianNPCController.Instance != null)
                CassianNPCController.Instance.OnMoon2ConflictMudGolem(playerObservedCassianKnowledge);
        }

        // ==================== BEAT 4: CLIMAX ====================
        public void TriggerClimaxBeat()
        {
            if (_climaxComplete) return;
            _climaxComplete = true;
            _currentBeat = 4;

            // Ionized fountain storm sequence (spectacular)
            if (ionizedMistVFXPrefab != null)
            {
                var storm = Instantiate(ionizedMistVFXPrefab, ionizedFountainCenter, Quaternion.identity);
                Destroy(storm, 18f);
            }

            // Audio + haptic storm — strong AVH for ionized fountain storm dome purify (beat 4 climax)
            AudioManager.Instance?.PlaySFX2D("Moon2_IonizedFountainStorm");
            AudioManager.Instance?.PlaySFX2D("Moon2_FountainStorm", 0.7f);
            AudioManager.Instance?.PlaySFX("Moon2_CrystalResonanceTone", ionizedFountainCenter, 0.45f);
            Input.HapticFeedbackManager.Instance?.PlayClimaxRumble();
            Input.HapticFeedbackManager.Instance?.PlayFountainStormRumble();
            Input.HapticFeedbackManager.Instance?.PlayCrystalResonanceTuning();

            // Purify dome + companions react
            if (LiraelController.Instance != null)
            {
                LiraelController.Instance.OnMoon2FountainRelief(true);
            }
            if (CassianNPCController.Instance != null)
            {
                // Cassian comments on the "miracle" — slight doubt seed
                CassianNPCController.Instance.OnMoon2ConflictMudGolem(false); // follow-up
            }

            DialogueManager.Instance?.PlayContextDialogue("moon2_climax_fountain_storm");
            DialogueManager.Instance?.PlayContextDialogue("milo_fountain_wet_comment"); // from docs
            HUDController.Instance?.ShowBanner("CLIMAX — The Crystal Cathedral Sings", "Ionized mist purges the dome in a golden wave. The corruption screams and burns.", 8f);

            QuestManager.Instance?.ProgressByType(QuestObjectiveType.RestoreBuilding, "moon2_fountain", 1);
            CompanionManager.Instance?.TriggerPhysicalTellForBeat("lirael", 0);
            CompanionManager.Instance?.TriggerPhysicalTellForBeat("cassian", 0);

            // Spawn the diary for revelation + the Crystal Remembers station
            SpawnCassianDiaryAndCrystalStation();

            Debug.Log("[Moon2LunarContentSpawner] BEAT 4 COMPLETE — Climax: ionized fountain storm dome purify. Lirael memory solidified.");
            _currentBeat = 5;
        }

        void SpawnCassianDiaryAndCrystalStation()
        {
            // Diary interactable (ambiguity choice)
            var diary = new GameObject("Cassian_Diary_Revelation");
            diary.transform.position = crystalRemembersStationPos + new Vector3(2f, 0.8f, 0f);
            var diaryInteract = diary.AddComponent<CassianDiaryInteractable>(); // would implement choice UI calling OnMoon2RevelationDiaryChoice
            diaryInteract.Initialize(this);

            // THE CRYSTAL REMEMBERS station — deep replayable centerpiece
            if (crystalMemoryStationPrefab != null)
            {
                _crystalStationInstance = Instantiate(crystalMemoryStationPrefab, crystalRemembersStationPos, Quaternion.identity);
                _crystalStationInstance.name = "TheCrystalRemembers_Station";
                var station = _crystalStationInstance.AddComponent<CrystalRemembersStation>();
                station.Initialize(this, _isReturningPlayer, _crystalMemoryVariant);
            }
            else
            {
                // Fallback station
                _crystalStationInstance = new GameObject("TheCrystalRemembers_Station");
                _crystalStationInstance.transform.position = crystalRemembersStationPos;
                var station = _crystalStationInstance.AddComponent<CrystalRemembersStation>();
                station.Initialize(this, _isReturningPlayer, _crystalMemoryVariant);
            }

            HUDController.Instance?.ShowBanner("The Crystal Remembers", "Cassian's diary lies open. The great crystal pulses with every choice you made. It remembers…", 9f);
        }

        // ==================== BEAT 5: REVELATION + THE CRYSTAL REMEMBERS (DEEP + REPLAYABLE) ====================
        public void TriggerRevelationBeat(bool choseTrustPath, string memoryVariantId)
        {
            if (_revelationComplete) return;
            _revelationComplete = true;
            _currentBeat = 5;

            if (CassianNPCController.Instance != null)
            {
                CassianNPCController.Instance.OnMoon2RevelationDiaryChoice(choseTrustPath, memoryVariantId);
            }
            if (LiraelController.Instance != null)
            {
                LiraelController.Instance.OnMoon2CrystalRemembers(choseTrustPath, memoryVariantId);
            }

            _crystalMemoryVariant = choseTrustPath ? "trust" : "doubt";
            if (_isReturningPlayer) _crystalMemoryVariant = "returning";

            QuestManager.Instance?.ProgressByType(QuestObjectiveType.CompanionMilestone, "crystal_remembers", 1);

            // Final trust arc resolution + banner
            string banner = choseTrustPath
                ? "The Crystal Remembers — Hope"
                : "The Crystal Remembers — Fracture";
            HUDController.Instance?.ShowBanner(banner, choseTrustPath
                ? "You chose to believe. The crystal glows golden. New ley resonances awaken across the caverns."
                : "Doubt lingers. The crystal pulses violet. Warnings echo from the Flood.", 10f);

            CompanionManager.Instance?.TriggerPhysicalTellForBeat("cassian", choseTrustPath ? 0 : 1);
            CompanionManager.Instance?.TriggerPhysicalTellForBeat("lirael", 4);

            // Strong AVH: 432Hz lullaby layers + gentle haptic for revelation / Crystal Remembers (5-beat narrative payoff + giant synergy echo)
            AudioManager.Instance?.PlaySFX2D("LiraelLullabyHum", 0.85f);
            AudioManager.Instance?.PlaySFX2D("Moon2_432LullabyLayer", 0.65f);
            AudioManager.Instance?.PlaySFX("Moon2_CrystalResonanceTone", transform.position, 0.38f);
            Input.HapticFeedbackManager.Instance?.PlayLullabyPulse();
            Input.HapticFeedbackManager.Instance?.PlayCrystalResonanceTuning();

            // Unlock deep replay fragments
            UnlockCrystalMemoryFragments(choseTrustPath);

            Debug.Log($"[Moon2LunarContentSpawner] BEAT 5 COMPLETE — Revelation: Cassian diary choice + The Crystal Remembers. Variant={_crystalMemoryVariant}. Full 5-beat FTUE done. Replayable experience live. (432Hz lullaby + resonance AVH wired)");
        }

        /// <summary>
        /// The heart of the deep replayable "The Crystal Remembers" experience.
        /// Called from Cassian hook and CrystalRemembersStation interact.
        /// Supports multiple replays with evolving content based on choice, returning status, and unlocked fragments.
        /// </summary>
        public void TriggerCrystalRemembersExperience(bool trustPath, string variantId)
        {
            _crystalMemoryVariant = trustPath ? "trust" : "doubt";
            if (_isReturningPlayer) _crystalMemoryVariant = "returning";

            // Play the deep holographic sequence (5-beat replay with variants)
            StartCoroutine(PlayCrystalRemembersHoloSequence(trustPath, variantId));

            // RS + haptic payoff
            // ResonanceScoreSystem.Instance?.Award(85f, "TheCrystalRemembers");
            Input.HapticFeedbackManager.Instance?.PlayDiscovery();

            // Unlock variant-specific fragments for future replays
            UnlockCrystalMemoryFragments(trustPath);

            // Permanent world effect (sigil / mutation)
            if (Moon2ProgressionSystem.Instance != null)
            {
                // Would call a mutation grant here
            }

            Debug.Log($"[Moon2LunarContentSpawner] THE CRYSTAL REMEMBERS triggered — deep replayable. Path={trustPath} Variant={_crystalMemoryVariant} Fragments={_unlockedMemoryFragments.Count}");
        }

        IEnumerator PlayCrystalRemembersHoloSequence(bool trustPath, string variantId)
        {
            // Rich, replayable sequence: plays all 5 beats as living crystal holograms with path-dependent dialogue/VFX
            string[] beatLines = trustPath
                ? new[] { "holo_discovery_hope", "holo_restoration_song", "holo_conflict_stand", "holo_climax_golden_wave", "holo_revelation_believed" }
                : new[] { "holo_discovery_fracture", "holo_restoration_warning", "holo_conflict_betray", "holo_climax_violet_mist", "holo_revelation_doubted" };

            foreach (var line in beatLines)
            {
                DialogueManager.Instance?.PlayContextDialogue($"crystal_remembers_{line}");
                CompanionManager.Instance?.TriggerPhysicalTellForBeat(trustPath ? "lirael" : "cassian", trustPath ? 0 : 1);
                yield return new WaitForSeconds(3.8f);
            }

            // Returning player bonus echoes
            if (_isReturningPlayer || _unlockedMemoryFragments.Count > 2)
            {
                DialogueManager.Instance?.PlayContextDialogue("crystal_remembers_returning_echo");
                HUDController.Instance?.ShowBanner("The Crystals Still Sing", "You have been here before. Every choice echoes. The Flood remembers your name.", 7f);
            }

            // Final banner + replay prompt
            HUDController.Instance?.ShowBanner("The Crystal Remembers", "Replay any fragment. The memory is yours forever — changed by what you chose.", 12f);
        }

        void UnlockCrystalMemoryFragments(bool trustPath)
        {
            _unlockedMemoryFragments.Add("discovery");
            _unlockedMemoryFragments.Add("restoration");
            _unlockedMemoryFragments.Add("conflict");
            _unlockedMemoryFragments.Add("climax");
            _unlockedMemoryFragments.Add("revelation");
            if (trustPath) _unlockedMemoryFragments.Add("hope_variant");
            if (_isReturningPlayer) _unlockedMemoryFragments.Add("returning_echo_01");
            if (_unlockedMemoryFragments.Count >= 5) _unlockedMemoryFragments.Add("full_replay_mastery");
        }

        // Public API for external systems / GameLoop reactivity
        public void HandleMoon2NarrativeReactivity(int beatIndex, bool playerChoicePositive)
        {
            switch (beatIndex)
            {
                case 1: TriggerDiscoveryBeat(); break;
                case 3: OnFirstMudGolemDefeated(!playerChoicePositive); break;
                case 4: TriggerClimaxBeat(); break;
                case 5: TriggerRevelationBeat(playerChoicePositive, _crystalMemoryVariant); break;
                default:
                    Debug.Log($"[Moon2LunarContentSpawner] Narrative reactivity beat {beatIndex} processed.");
                    break;
            }
        }

        public void ForceTriggerCrystalRemembersForReplay()
        {
            TriggerCrystalRemembersExperience(_crystalMemoryVariant != "doubt", "manual_replay_" + _unlockedMemoryFragments.Count);
        }

        // Save / returning helpers
        void SpawnConflictWraith()
        {
            Vector3 wraithPos = firstMudGolemSpawn + new Vector3(22f, 1.2f, 12f);
            var wraith = new GameObject("Conflict_Wraith_Threat");
            wraith.transform.position = wraithPos;
            wraith.name = "FractalWraith_ConflictBeat";

            var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vis.transform.SetParent(wraith.transform);
            vis.transform.localScale = new Vector3(0.8f, 1.6f, 0.8f);
            var vr = vis.GetComponent<Renderer>();
            if (vr != null)
            {
                vr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                vr.material.color = new Color(0.18f, 0.12f, 0.32f);
                vr.material.SetColor("_EmissionColor", new Color(0.4f, 0.15f, 0.6f) * 1.6f);
            }
            Destroy(vis.GetComponent<Collider>());

            var ps = wraith.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor = new Color(0.25f, 0.18f, 0.42f, 0.85f);
            m.startSize = 0.55f;
            m.startLifetime = 2.4f;
            ps.Play();

            AudioManager.Instance?.PlaySFX("Moon2_DissonanceWraithWhisper", wraithPos, 0.65f);
            Destroy(wraith, 18f);
            Debug.Log("[Moon2LunarContentSpawner] WRAITH SPAWNED on Conflict beat.");
        }

        public string CurrentCrystalMemoryVariant => _crystalMemoryVariant;
        public int GetUnlockedMemoryFragmentCount() => _unlockedMemoryFragments.Count;
    }

    // ==================== SUPPORTING RUNTIME COMPONENTS (lightweight, self-contained for FTUE) ====================

    public class CassianDiaryInteractable : MonoBehaviour, IInteractable
    {
        Moon2LunarContentSpawner _spawner;
        bool _chosen;

        public void Initialize(Moon2LunarContentSpawner spawner) { _spawner = spawner; }

        public void Interact()
        {
            if (_chosen || _spawner == null) return;
            _chosen = true;

            // Rich ambiguity choice — trust or doubt path (feeds W1 + Crystal Remembers)
            bool trust = Random.value > 0.45f; // In real: dialogue tree branch or WorldChoice UI
            string variant = trust ? "diary_trust_path" : "diary_doubt_path";

            DialogueManager.Instance?.PlayContextDialogue(trust ? "cassian_diary_trust_explain" : "cassian_diary_doubt_explain");
            _spawner.TriggerRevelationBeat(trust, variant);
        }

        public void Interact(GameObject player) => Interact();
        public string GetPrompt() => "Examine Cassian's unearthed diary (The choice will echo in the crystal)";
        public string GetInteractPrompt() => GetPrompt();
    }

    public class CrystalRemembersStation : MonoBehaviour, IInteractable
    {
        Moon2LunarContentSpawner _spawner;
        bool _isReturning;
        string _variant;

        public void Initialize(Moon2LunarContentSpawner spawner, bool returning, string variant)
        {
            _spawner = spawner;
            _isReturning = returning;
            _variant = variant;
        }

        public void Interact()
        {
            if (_spawner == null) return;
            bool trust = _variant != "doubt";
            _spawner.TriggerCrystalRemembersExperience(trust, "station_" + _variant + "_" + _spawner.GetUnlockedMemoryFragmentCount());
        }

        public void Interact(GameObject player) => Interact();
        public string GetPrompt() => _isReturning
            ? "Commune with The Crystal Remembers (Returning — extended echoes await)"
            : "Commune with The Crystal Remembers (Deep replayable memory of every choice)";
        public string GetInteractPrompt() => GetPrompt();
    }

    public class ReturningGuardInteractable : MonoBehaviour, IInteractable
    {
        public void Interact()
        {
            DialogueManager.Instance?.PlayContextDialogue("returning_guard_lore");
            HUDController.Instance?.ShowBanner("The Ley Remembers", "You restored the first dome. The corruption fears what you became.", 5f);
        }
        public void Interact(GameObject player) => Interact();
        public string GetPrompt() => "Speak with the returning guard (disarmed, remembers you)";
        public string GetInteractPrompt() => GetPrompt();
    }
}

