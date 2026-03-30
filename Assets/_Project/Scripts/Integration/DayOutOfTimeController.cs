using System;
using System.Collections;
using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Day Out of Time (DotT) — The secret 14th zone that exists outside the 13-moon calendar.
    /// Triggered after completing Moon 13's Planetary Nexus and collecting all 13 golden motes.
    ///
    /// From doc 17 (Day Out of Time):
    ///   - A single 10-minute real-time event
    ///   - All restored buildings pulse in sync
    ///   - Anastasia delivers her final lines before solidification
    ///   - The world transforms: fog lifts, full golden palette, 432 Hz drone
    ///   - Player walks through all 13 zones in a compressed memory corridor
    ///   - Ends with Anastasia's final solidification and the True Ending
    ///
    /// Anastasia DotT lines (from doc 18):
    ///   ID 102: "I can feel the ground remembering what it was."
    ///   ID 103: "Ten seconds. After a thousand years, ten seconds is all it takes."
    /// </summary>
    [DisallowMultipleComponent]
    public class DayOutOfTimeController : MonoBehaviour
    {
        public static DayOutOfTimeController Instance { get; private set; }

        [Header("DotT Configuration")]
        [SerializeField] float eventDurationSeconds = 600f; // 10 minutes
#pragma warning disable CS0414
        [SerializeField] float memoryCorridorSpeed = 12f;
#pragma warning restore CS0414
        [SerializeField] Color dottSkyColor = new(0.95f, 0.85f, 0.5f, 1f);
        [SerializeField] Color dottFogColor = new(0.9f, 0.8f, 0.4f, 1f);
        [SerializeField] float dottFogDensity = 0.005f;

        bool _eventActive;
        bool _eventCompleted;
        float _eventTimer;
        int _currentMemoryZone;

        public bool IsEventActive => _eventActive;
        public bool IsEventCompleted => _eventCompleted;
        public float EventProgress => _eventActive ? Mathf.Clamp01(_eventTimer / eventDurationSeconds) : 0f;

        // ─── Events ─────────────────────────────────
        public event Action OnEventCompleted;
#pragma warning disable CS0067
        public event Action<int> OnMemoryZoneChanged;
#pragma warning restore CS0067

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Check if DotT can be triggered (all 13 motes collected + Moon 13 complete).
        /// </summary>
        public bool CanTrigger()
        {
            if (_eventCompleted || _eventActive) return false;

            var ana = AnastasiaController.Instance;
            if (ana == null) return false;

            // Need all 13 golden motes
            for (int i = 0; i < 13; i++)
            {
                if (!ana.IsMoteCollected(i)) return false;
            }

            // Need to be in Moon 13 or beyond
            return ana.CurrentMoon >= 13;
        }

        /// <summary>
        /// Begin the Day Out of Time event.
        /// </summary>
        public void TriggerEvent()
        {
            if (!CanTrigger()) return;

            _eventActive = true;
            _eventTimer = 0f;
            _currentMemoryZone = 0;

            GameStateManager.Instance?.TransitionTo(GameState.Cinematic);
            Debug.Log("[DotT] Day Out of Time has begun.");

            StartCoroutine(DotTSequence());
        }

        IEnumerator DotTSequence()
        {
            // Phase 1: World transformation (first 30 seconds)
            yield return StartCoroutine(WorldTransformation());

            // Phase 2: Anastasia's first DotT line
            AnastasiaController.Instance?.TryDeliverLine("dott_start");
            yield return new WaitForSeconds(8f);

            // Phase 3: Memory corridor — walk through 13 compressed zones
            for (int i = 0; i < 13; i++)
            {
                _currentMemoryZone = i + 1;
                yield return StartCoroutine(MemoryZoneFlash(i));
            }

            // Phase 4: Anastasia's final line
            AnastasiaController.Instance?.TryDeliverLine("dott_final");
            yield return new WaitForSeconds(10f);

            // Phase 5: Solidification
            var ana = AnastasiaController.Instance;
            if (ana != null && ana.CurrentSolidPhase == SolidificationPhase.NotTriggered)
            {
                ana.TriggerSolidification();
                // Wait for solidification to complete (~30s based on AnastasiaController)
                yield return new WaitUntil(() =>
                    ana.CurrentSolidPhase == SolidificationPhase.Return ||
                    ana.CurrentSolidPhase == SolidificationPhase.NotTriggered);
                yield return new WaitForSeconds(5f);
            }

            // Phase 5b: Companion Performances
            yield return StartCoroutine(CompanionPerformances());

            // Phase 6: True Ending
            _eventActive = false;
            _eventCompleted = true;
            OnEventCompleted?.Invoke();

            HUDController.Instance?.ShowInteractionPrompt(
                "THE DAY OUT OF TIME\nResonance Restored. The world remembers.");

            yield return new WaitForSeconds(10f);
            HUDController.Instance?.HideInteractionPrompt();

            GameStateManager.Instance?.TransitionTo(GameState.Exploration);
            SaveManager.Instance?.MarkDirty();

            Debug.Log("[DotT] Day Out of Time complete. True Ending reached.");
        }

        IEnumerator WorldTransformation()
        {
            float duration = 30f;
            float elapsed = 0f;

            Color startFog = RenderSettings.fogColor;
            Color startAmbient = RenderSettings.ambientLight;
            float startDensity = RenderSettings.fogDensity;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                RenderSettings.fogColor = Color.Lerp(startFog, dottFogColor, t);
                RenderSettings.ambientLight = Color.Lerp(startAmbient, dottSkyColor, t);
                RenderSettings.fogDensity = Mathf.Lerp(startDensity, dottFogDensity, t);

                _eventTimer += Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator MemoryZoneFlash(int zoneIndex)
        {
            // Each zone gets ~35 seconds of the corridor (13 zones * ~35s = ~7.5 minutes)
            float zoneDuration = (eventDurationSeconds - 150f) / 13f; // minus transformation + ending time

            HUDController.Instance?.ShowInteractionPrompt(
                $"Memory {zoneIndex + 1} of 13");

            // Zone atmosphere flash
            float elapsed = 0f;
            while (elapsed < zoneDuration)
            {
                elapsed += Time.deltaTime;
                _eventTimer += Time.deltaTime;
                yield return null;
            }

            HUDController.Instance?.HideInteractionPrompt();
        }

        // ─── Companion Performances ──────────────────

        /// <summary>
        /// 6 companion performances during the DotT event.
        /// Each companion delivers a unique tribute to the restored world.
        /// </summary>
        IEnumerator CompanionPerformances()
        {
            Debug.Log("[DotT] Companion performances begin.");

            // 1. Lirael's Concert — crystal resonance song reverberates through all 13 zones
            yield return StartCoroutine(LiraelConcert());

            // 2. Thorne's Flyover — militia airship salute over the restored skyline
            yield return StartCoroutine(ThorneFlyover());

            // 3. Korath's Celestial Symphony — star patterns align, constellation map projected
            yield return StartCoroutine(KorathSymphony());

            // 4. Veritas's Organ Finale — 12 bell towers ring in perfect Schumann harmony
            yield return StartCoroutine(VeritasOrganFinale());

            // 5. Milo's Commerce Festival — the fox leads a parade of restored shopkeepers
            yield return StartCoroutine(MiloCommerceFestival());

            // 6. Anastasia's Solidification Celebration — golden motes converge, she becomes fully real
            yield return StartCoroutine(AnastasiaSolidificationCelebration());

            Debug.Log("[DotT] All companion performances complete.");
        }

        IEnumerator LiraelConcert()
        {
            HUDController.Instance?.ShowInteractionPrompt(
                "Lirael sings the Song of Restoration.\nCrystal harmonics ripple through all 13 zones.");
            DialogueManager.Instance?.PlayContextDialogue("dott_lirael_concert");
            LiraelController.Instance?.NotifyZoneComplete(); // triggers celebration state

            Tartaria.Audio.AdaptiveMusicController.Instance?.PlayRestoration();
            yield return new WaitForSeconds(12f);
            CompanionManager.Instance?.AddTrust("lirael", 25);
            HUDController.Instance?.HideInteractionPrompt();
        }

        IEnumerator ThorneFlyover()
        {
            HUDController.Instance?.ShowInteractionPrompt(
                "Thorne leads the militia fleet in a skyward salute.\n\"For those who stood when the world knelt.\"");
            DialogueManager.Instance?.PlayContextDialogue("dott_thorne_flyover");

            var fleet = AirshipFleetManager.Instance;
            if (fleet != null)
                fleet.SetFormation(AirshipFleetManager.FleetFormation.Vanguard);

            yield return new WaitForSeconds(10f);
            CompanionManager.Instance?.AddTrust("thorne", 25);
            HUDController.Instance?.HideInteractionPrompt();
        }

        IEnumerator KorathSymphony()
        {
            HUDController.Instance?.ShowInteractionPrompt(
                "Korath projects the true star map onto the sky.\nConstellations the old world once navigated by.");
            DialogueManager.Instance?.PlayContextDialogue("dott_korath_symphony");

            yield return new WaitForSeconds(10f);
            CompanionManager.Instance?.AddTrust("korath", 25);
            HUDController.Instance?.HideInteractionPrompt();
        }

        IEnumerator VeritasOrganFinale()
        {
            HUDController.Instance?.ShowInteractionPrompt(
                "Veritas rings all 12 bell towers simultaneously.\n7.83 Hz -- the Earth's heartbeat restored.");
            DialogueManager.Instance?.PlayContextDialogue("dott_veritas_organ");

            VFXController.Instance?.SpawnPlanetaryBellRing(Vector3.zero);
            yield return new WaitForSeconds(12f);
            CompanionManager.Instance?.AddTrust("veritas", 25);
            HUDController.Instance?.HideInteractionPrompt();
        }

        IEnumerator MiloCommerceFestival()
        {
            HUDController.Instance?.ShowInteractionPrompt(
                "Milo darts between stalls, tail blazing gold.\n\"Best deals in a thousand years! Everything must go!\"");
            DialogueManager.Instance?.PlayContextDialogue("dott_milo_festival");

            yield return new WaitForSeconds(8f);
            CompanionManager.Instance?.AddTrust("milo", 25);
            HUDController.Instance?.HideInteractionPrompt();
        }

        IEnumerator AnastasiaSolidificationCelebration()
        {
            HUDController.Instance?.ShowInteractionPrompt(
                "Golden motes spiral inward. Anastasia steps forward, fully solid.\n\"I remember everything now. Every stone. Every name.\"");
            DialogueManager.Instance?.PlayContextDialogue("dott_anastasia_solid");

            VFXController.Instance?.SpawnAnastasiaSolidificationEffect(Vector3.zero);
            yield return new WaitForSeconds(15f);
            CompanionManager.Instance?.AddTrust("anastasia", 50);
            HUDController.Instance?.HideInteractionPrompt();

            AchievementSystem.Instance?.CheckSolidification();
        }

        // ─── Sandbox Mode ────────────────────────────

        bool _sandboxActive;
        public bool IsSandboxActive => _sandboxActive;

        /// <summary>
        /// Enter sandbox mode after DotT completion — free exploration of all 13 restored zones.
        /// No timers, no combat, all buildings accessible.
        /// </summary>
        public void EnterSandbox()
        {
            if (!_eventCompleted)
            {
                Debug.LogWarning("[DotT] Sandbox requires DotT completion.");
                return;
            }

            _sandboxActive = true;
            GameStateManager.Instance?.TransitionTo(GameState.Exploration);

            // Disable enemy spawns, remove fog entirely
            RenderSettings.fog = false;
            RenderSettings.ambientLight = dottSkyColor;

            HUDController.Instance?.ShowInteractionPrompt(
                "SANDBOX MODE\nExplore the restored world freely. No enemies, no timers.");

            Debug.Log("[DotT] Sandbox mode activated.");
        }

        public void ExitSandbox()
        {
            _sandboxActive = false;
            HUDController.Instance?.HideInteractionPrompt();
            Debug.Log("[DotT] Sandbox mode deactivated.");
        }

        // ─── Challenge Modes ─────────────────────────

        DotTChallengeMode _activeChallenge = DotTChallengeMode.None;
        float _challengeTimer;
        float _challengeTimeLimit;
        int _challengeScore;

        public DotTChallengeMode ActiveChallenge => _activeChallenge;
        public float ChallengeTimer => _challengeTimer;
        public int ChallengeScore => _challengeScore;

        /// <summary>
        /// Start a challenge mode — replayable content after DotT completion.
        /// </summary>
        public bool StartChallenge(DotTChallengeMode mode)
        {
            if (!_eventCompleted) return false;
            if (_activeChallenge != DotTChallengeMode.None) return false;

            _activeChallenge = mode;
            _challengeTimer = 0f;
            _challengeScore = 0;

            switch (mode)
            {
                case DotTChallengeMode.Speedrun:
                    // Re-run the memory corridor under 5 minutes
                    _challengeTimeLimit = 300f;
                    break;
                case DotTChallengeMode.Pacifist:
                    // Complete all 13 memory zones without triggering any combat
                    _challengeTimeLimit = 900f; // 15 min generous limit
                    break;
                case DotTChallengeMode.Creative:
                    // Build/restore freely with unlimited resources — no time limit
                    _challengeTimeLimit = float.MaxValue;
                    break;
                default:
                    return false;
            }

            GameStateManager.Instance?.TransitionTo(GameState.Cinematic);
            Debug.Log($"[DotT] Challenge started: {mode}");
            StartCoroutine(ChallengeLoop());
            return true;
        }

        IEnumerator ChallengeLoop()
        {
            HUDController.Instance?.ShowInteractionPrompt(
                $"CHALLENGE: {_activeChallenge}\nTime limit: {(_challengeTimeLimit < float.MaxValue ? $"{_challengeTimeLimit}s" : "None")}");
            yield return new WaitForSeconds(3f);
            HUDController.Instance?.HideInteractionPrompt();

            GameStateManager.Instance?.TransitionTo(GameState.Exploration);

            // Tick timer
            while (_activeChallenge != DotTChallengeMode.None)
            {
                _challengeTimer += Time.deltaTime;

                if (_challengeTimer >= _challengeTimeLimit)
                {
                    EndChallenge(false);
                    yield break;
                }

                yield return null;
            }
        }

        public void AddChallengeScore(int points)
        {
            if (_activeChallenge == DotTChallengeMode.None) return;
            _challengeScore += points;
        }

        public void EndChallenge(bool success)
        {
            if (_activeChallenge == DotTChallengeMode.None) return;

            var mode = _activeChallenge;
            _activeChallenge = DotTChallengeMode.None;

            string result = success ? "COMPLETE" : "FAILED";
            HUDController.Instance?.ShowInteractionPrompt(
                $"Challenge {result}: {mode}\nScore: {_challengeScore} | Time: {_challengeTimer:F1}s");

            // Award festival currency on success
            if (success)
            {
                int festivalReward = Mathf.RoundToInt(_challengeScore * 0.1f + 50f);
                AddFestivalCurrency(festivalReward);
            }

            Debug.Log($"[DotT] Challenge {mode} {result}: score={_challengeScore}, time={_challengeTimer:F1}s");
            SaveManager.Instance?.MarkDirty();
        }

        // ─── Festival Economy ────────────────────────

        int _festivalCurrency;
        public int FestivalCurrency => _festivalCurrency;

        public void AddFestivalCurrency(int amount)
        {
            if (amount <= 0) return;
            _festivalCurrency += amount;
            Debug.Log($"[DotT] Festival currency +{amount} (total: {_festivalCurrency})");
        }

        public bool SpendFestivalCurrency(int amount)
        {
            if (amount <= 0 || _festivalCurrency < amount) return false;
            _festivalCurrency -= amount;
            return true;
        }

        /// <summary>
        /// Festival shop — exclusive cosmetics and items available only during/after DotT.
        /// </summary>
        public bool PurchaseFestivalItem(string itemId)
        {
            int cost = GetFestivalItemCost(itemId);
            if (cost <= 0) return false;

            if (!SpendFestivalCurrency(cost)) return false;

            // Grant the item via CraftingSystem inventory
            Gameplay.CraftingSystem.Instance?.ConsumeItem(itemId, -1); // negative consume = add
            Debug.Log($"[DotT] Festival purchase: {itemId} for {cost} festival currency");
            return true;
        }

        static int GetFestivalItemCost(string itemId)
        {
            return itemId switch
            {
                "golden_cloak" => 100,
                "resonance_crown" => 250,
                "dott_banner" => 50,
                "celestial_lantern" => 150,
                "memory_crystal" => 75,
                _ => 0
            };
        }

        // ─── Save / Load ─────────────────────────────

        public DotTSavePayload GetSaveData()
        {
            return new DotTSavePayload
            {
                eventCompleted = _eventCompleted,
                festivalCurrency = _festivalCurrency,
                currentMemoryZone = _currentMemoryZone,
                bestChallengeScore = _challengeScore
            };
        }

        public void LoadSaveData(DotTSavePayload data)
        {
            if (data == null) return;
            _eventCompleted = data.eventCompleted;
            _festivalCurrency = data.festivalCurrency;
            _currentMemoryZone = data.currentMemoryZone;
            _challengeScore = (int)data.bestChallengeScore;
        }

        public class DotTSavePayload
        {
            public bool eventCompleted;
            public int festivalCurrency;
            public int currentMemoryZone;
            public float bestChallengeScore;
        }
    }

    public enum DotTChallengeMode : byte
    {
        None = 0,
        Speedrun = 1,
        Pacifist = 2,
        Creative = 3
    }
}
