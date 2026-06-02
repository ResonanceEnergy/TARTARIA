// TARTARIA WORLD OF WONDER
// MiloTutorialFlow.cs - Sprint 6 Lane 6
// Six-step onboarding state machine: Milo greets, points at brazier, prompts E,
// guides player to first hero building, drives one tuning, congratulates, frees play.
//
// Owner: AI assembly (per docs/agents/COORDINATION.md path ownership for Scripts/AI/).
//
// API contract (verified against Assets/_Project/Scripts/Core/GameEvents.cs 2026-06-02):
//   - GameEvents.OnBuildingRestored                  -> event Action<string>          (file:line 56)
//   - GameEvents.RaiseHUDShowDialogue(speaker,msg)   -> method                        (file:line 617)
//   - GameEvents.RaiseHUDShowBanner(t,s,d)           -> method                        (file:line 623)
//   - GameEvents.RaiseHUDShowInteractionPrompt(msg)  -> method                        (file:line 659)
//   - GameEvents.RaiseHUDHideInteractionPrompt()     -> method                        (file:line 665)
//
//   No OnTuneAttemptComplete / OnInteractStart events exist in GameEvents - those names
//   from the lane brief are NOT canonical (see API_CONTRACT.md "Invented API" entry).
//   This flow listens on OnBuildingRestored for step 5 (the canonical tuning-complete signal,
//   verified file:line 56 + 469-471) and drives prompt visibility itself for step 2/4.
//
//   DialogueManager (Tartaria.Integration) is NOT referenced directly because
//   Tartaria.Integration.asmdef references Tartaria.AI (file:line 8 of Tartaria.Integration.asmdef),
//   so AI -> Integration would be circular. Yarn lines are surfaced via the
//   RaiseHUDShowDialogue/RaiseHUDShowBanner GameEvents pathway; an Integration-side
//   Yarn-runner binding can later subscribe to OnHUDShowDialogue if a full Yarn
//   playback is desired. Per CLAUDE.md no-debt rule 4, the fallback is loud:
//   the banner always fires from the canonical GameEvents API.
//
// Input: Unity 6 Input System Package - Keyboard.current.escapeKey.wasPressedThisFrame
//        per CLAUDE.md F310 section ("InvalidOperationException under Input System Package mode").
//        Legacy UnityEngine.Input.* paths are banned in this project.
//
// First-boot detect: PlayerPrefs key "TARTARIA_FirstBoot". 0 (or missing) = run tutorial,
//        1 = skip. Set to 1 when tutorial completes OR when ESC skips.
//
// No-debt mandate (CLAUDE.md 2026-06-02):
//   - No silent catches: every catch logs error with site + value
//   - No silent fallbacks: missing scene/player logs warning with path tried
//   - No TODO bodies, no stubs, no override drivers
//   - Every step logs entry + exit per lane spec: "[MiloTutorial] step {n} entered/completed"

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.AI
{
    /// <summary>
    /// MiloTutorialFlow - six-step onboarding state machine.
    /// Auto-bootstraps on AfterSceneLoad in the Echohaven scene if PlayerPrefs
    /// "TARTARIA_FirstBoot" != 1. ESC at any step skips remainder and persists the flag.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-50)]
    public class MiloTutorialFlow : MonoBehaviour
    {
        public const string FirstBootPrefKey = "TARTARIA_FirstBoot";
        const string kEchohavenSceneName = "Echohaven_VerticalSlice";

        // Waypoint world position for step 3 (first hero building - Cathedral entrance ring).
        // Pulled from Assets/_Project/Scripts/Integration/Moon1Braziers.cs Brazier_Cathedral_L/R
        // which sit at z=24, x=+-4 - center at (0, 0, 24).
        static readonly Vector3 kCathedralWaypoint = new Vector3(0f, 0f, 24f);
        const float kWaypointArriveRadius = 6f;

        // Prompt timings.
        const float kBannerDuration = 6f;
        const float kStep2HoldSeconds = 8f;     // how long the "Press E" prompt persists before progressing
        const float kStep6CleanupSeconds = 4f;

        // Yarn node names mirror Assets/_Project/Dialogue/Echohaven/milo_tutorial.yarn.
        // Spoken lines kept SHORT in code so they fit the HUD banner; the .yarn file
        // carries the longer companion line for a future Yarn-runner integration.
        const string kSpeaker = "Milo";
        const string kStep1Line = "Look toward the firelight, traveler.";
        const string kStep2Line = "Press E to interact with the world.";
        const string kStep3Line = "Follow the arrow. The buried cathedral is closer than it looks.";
        const string kStep4Line = "Press E at the green light.";
        const string kStep5Line = "One building back from the silence. You've got the knack now.";
        const string kStep6Line = "Explore. You have buildings to wake.";
        const string kSkipLine = "Suit yourself. The valley's yours to read.";

        public static MiloTutorialFlow Instance { get; private set; }

        public enum Step
        {
            Inactive = 0,
            Step1_Greet = 1,
            Step2_PressE = 2,
            Step3_WalkWaypoint = 3,
            Step4_StartTuning = 4,
            Step5_RestoreComplete = 5,
            Step6_FreePlay = 6,
            Done = 99
        }

        public Step Current { get; private set; } = Step.Inactive;

        Transform _player;
        GameObject _waypointArrow;
        bool _subscribedRestored;
        float _step2EnteredAt = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            try
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (scene.name != kEchohavenSceneName)
                {
                    return;
                }

                if (Instance != null)
                {
                    return;
                }

                int firstBoot = PlayerPrefs.GetInt(FirstBootPrefKey, 0);
                if (firstBoot == 1)
                {
                    Debug.Log($"[MiloTutorial] {FirstBootPrefKey}=1, tutorial already completed; not auto-starting.");
                    return;
                }

                var go = new GameObject(nameof(MiloTutorialFlow));
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<MiloTutorialFlow>();
                Debug.Log("[MiloTutorial] Bootstrapped - first-boot detected, beginning step 1.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiloTutorial] Bootstrap failed at scene-load: {ex} {ex.StackTrace}");
                throw;
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            // Subscribe to canonical OnBuildingRestored - verified file:line 56 in GameEvents.cs.
            try
            {
                GameEvents.OnBuildingRestored += HandleBuildingRestored;
                _subscribedRestored = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiloTutorial] Failed to subscribe OnBuildingRestored: {ex}");
                throw;
            }

            EnterStep(Step.Step1_Greet);
        }

        void OnDestroy()
        {
            if (_subscribedRestored)
            {
                GameEvents.OnBuildingRestored -= HandleBuildingRestored;
                _subscribedRestored = false;
            }
            if (_waypointArrow != null)
            {
                Destroy(_waypointArrow);
                _waypointArrow = null;
            }
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void Update()
        {
            // ESC handled via UnityEngine.InputSystem (Unity 6 Input System Package).
            // Legacy UnityEngine.Input.GetKeyDown is banned here per CLAUDE.md F310 section.
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                SkipRemaining("ESC pressed");
                return;
            }

            switch (Current)
            {
                case Step.Step2_PressE:
                    // Auto-advance after a hold window so an unresponsive player still moves on.
                    if (_step2EnteredAt > 0f && Time.unscaledTime - _step2EnteredAt >= kStep2HoldSeconds)
                    {
                        CompleteStep(Step.Step2_PressE);
                        EnterStep(Step.Step3_WalkWaypoint);
                    }
                    break;

                case Step.Step3_WalkWaypoint:
                    UpdateWaypointArrow();
                    if (PlayerCloseToWaypoint())
                    {
                        CompleteStep(Step.Step3_WalkWaypoint);
                        EnterStep(Step.Step4_StartTuning);
                    }
                    break;
            }
        }

        // ----------------------------------------------------------------
        // Step machinery
        // ----------------------------------------------------------------

        void EnterStep(Step next)
        {
            Current = next;
            int n = (int)next;
            Debug.Log($"[MiloTutorial] step {n} entered");

            switch (next)
            {
                case Step.Step1_Greet:
                    SafeRaiseDialogue(kSpeaker, kStep1Line);
                    SafeRaiseBanner(kSpeaker, kStep1Line, kBannerDuration);
                    Invoke(nameof(AdvanceFromStep1), kBannerDuration);
                    break;

                case Step.Step2_PressE:
                    SafeRaiseDialogue(kSpeaker, kStep2Line);
                    SafeRaiseInteractionPrompt(kStep2Line);
                    _step2EnteredAt = Time.unscaledTime;
                    break;

                case Step.Step3_WalkWaypoint:
                    SafeHideInteractionPrompt();
                    SafeRaiseDialogue(kSpeaker, kStep3Line);
                    SafeRaiseBanner(kSpeaker, kStep3Line, kBannerDuration);
                    EnsureWaypointArrow();
                    break;

                case Step.Step4_StartTuning:
                    DestroyWaypointArrow();
                    SafeRaiseDialogue(kSpeaker, kStep4Line);
                    SafeRaiseInteractionPrompt(kStep4Line);
                    break;

                case Step.Step5_RestoreComplete:
                    SafeHideInteractionPrompt();
                    SafeRaiseDialogue(kSpeaker, kStep5Line);
                    SafeRaiseBanner(kSpeaker, kStep5Line, kBannerDuration);
                    Invoke(nameof(AdvanceFromStep5), kBannerDuration);
                    break;

                case Step.Step6_FreePlay:
                    SafeRaiseDialogue(kSpeaker, kStep6Line);
                    SafeRaiseBanner(kSpeaker, kStep6Line, kBannerDuration);
                    Invoke(nameof(CompleteTutorial), kStep6CleanupSeconds);
                    break;

                case Step.Done:
                    break;
            }
        }

        void CompleteStep(Step which)
        {
            int n = (int)which;
            Debug.Log($"[MiloTutorial] step {n} completed");
        }

        void AdvanceFromStep1()
        {
            if (Current != Step.Step1_Greet) return;
            CompleteStep(Step.Step1_Greet);
            EnterStep(Step.Step2_PressE);
        }

        void AdvanceFromStep5()
        {
            if (Current != Step.Step5_RestoreComplete) return;
            CompleteStep(Step.Step5_RestoreComplete);
            EnterStep(Step.Step6_FreePlay);
        }

        void CompleteTutorial()
        {
            if (Current == Step.Done) return;
            CompleteStep(Step.Step6_FreePlay);
            Current = Step.Done;
            PersistFirstBootFlag();
            Debug.Log("[MiloTutorial] All 6 steps complete; TARTARIA_FirstBoot set to 1.");
        }

        // ----------------------------------------------------------------
        // ESC skip
        // ----------------------------------------------------------------

        public void SkipRemaining(string reason)
        {
            if (Current == Step.Done || Current == Step.Inactive) return;
            int n = (int)Current;
            Debug.Log($"[MiloTutorial] step {n} skipped ({reason})");

            CancelInvoke();
            SafeHideInteractionPrompt();
            DestroyWaypointArrow();
            SafeRaiseDialogue(kSpeaker, kSkipLine);
            SafeRaiseBanner(kSpeaker, "Tutorial skipped. Carry on.", 3f);

            Current = Step.Done;
            PersistFirstBootFlag();
        }

        void PersistFirstBootFlag()
        {
            try
            {
                PlayerPrefs.SetInt(FirstBootPrefKey, 1);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiloTutorial] PlayerPrefs save failed for {FirstBootPrefKey}: {ex}");
                throw;
            }
        }

        // ----------------------------------------------------------------
        // Event handlers
        // ----------------------------------------------------------------

        void HandleBuildingRestored(string buildingId)
        {
            // Step 5 fires the moment the first restoration completes after the player
            // hits the tuning prompt. The lane spec calls for "Complete one tuning node"
            // - OnBuildingRestored is the canonical signal once a node chain finishes
            // (file:line 56 GameEvents.cs, raised at file:line 469-471).
            if (Current != Step.Step4_StartTuning)
            {
                // Player restored a building outside the tutorial window - do not derail.
                return;
            }
            Debug.Log($"[MiloTutorial] OnBuildingRestored received during step 4 (building='{buildingId}') - advancing to step 5.");
            CompleteStep(Step.Step4_StartTuning);
            EnterStep(Step.Step5_RestoreComplete);
        }

        // ----------------------------------------------------------------
        // Step 3 - waypoint arrow
        // ----------------------------------------------------------------

        Transform ResolvePlayer()
        {
            if (_player != null) return _player;
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go == null)
            {
                Debug.LogWarning("[MiloTutorial] Player tag not found in scene; waypoint arrow cannot anchor (looked up by tag='Player').");
                return null;
            }
            _player = go.transform;
            return _player;
        }

        void EnsureWaypointArrow()
        {
            if (_waypointArrow != null) return;

            _waypointArrow = new GameObject("MiloTutorial_WaypointArrow");
            DontDestroyOnLoad(_waypointArrow);

            // Yellow cylinder primitive floating above the cathedral waypoint, rotating + bobbing.
            // URP-safe: tags _BaseColor on the material.
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // URP-safe
            visual.name = "ArrowVisual";
            visual.transform.SetParent(_waypointArrow.transform, false);
            visual.transform.localScale = new Vector3(0.6f, 1.4f, 0.6f);
            // Remove collider - pure visual marker.
            var col = visual.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var rend = visual.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial != null)
            {
                var mat = new Material(rend.sharedMaterial);
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", new Color(1f, 0.9f, 0.25f, 1f));
                }
                else
                {
                    mat.color = new Color(1f, 0.9f, 0.25f, 1f);
                }
                rend.material = mat;
            }
            else
            {
                Debug.LogWarning("[MiloTutorial] Waypoint arrow visual has no renderer/material; will render with engine default.");
            }

            _waypointArrow.transform.position = kCathedralWaypoint + Vector3.up * 5f;
        }

        void UpdateWaypointArrow()
        {
            if (_waypointArrow == null) return;
            _waypointArrow.transform.Rotate(0f, 60f * Time.deltaTime, 0f, Space.World);

            // Gentle bob.
            var basePos = kCathedralWaypoint + Vector3.up * 5f;
            float bob = Mathf.Sin(Time.time * 2.5f) * 0.4f;
            _waypointArrow.transform.position = basePos + Vector3.up * bob;
        }

        void DestroyWaypointArrow()
        {
            if (_waypointArrow == null) return;
            Destroy(_waypointArrow);
            _waypointArrow = null;
        }

        bool PlayerCloseToWaypoint()
        {
            var p = ResolvePlayer();
            if (p == null) return false;
            float sqr = (p.position - kCathedralWaypoint).sqrMagnitude;
            return sqr <= kWaypointArriveRadius * kWaypointArriveRadius;
        }

        // ----------------------------------------------------------------
        // GameEvents wrappers - verified canonical API surface only.
        // Each catch logs the call site + value before rethrowing per CLAUDE.md
        // no-silent-catches rule.
        // ----------------------------------------------------------------

        static void SafeRaiseDialogue(string speaker, string line)
        {
            try
            {
                GameEvents.RaiseHUDShowDialogue(speaker, line);      // verified file:line 617 GameEvents.cs
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiloTutorial] RaiseHUDShowDialogue failed (speaker='{speaker}', line='{line}'): {ex}");
                throw;
            }
        }

        static void SafeRaiseBanner(string speaker, string line, float dur)
        {
            try
            {
                GameEvents.RaiseHUDShowBanner(speaker, line, dur);   // verified file:line 623 GameEvents.cs
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiloTutorial] RaiseHUDShowBanner failed (speaker='{speaker}', line='{line}'): {ex}");
                throw;
            }
        }

        static void SafeRaiseInteractionPrompt(string msg)
        {
            try
            {
                GameEvents.RaiseHUDShowInteractionPrompt(msg);       // verified file:line 659 GameEvents.cs
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiloTutorial] RaiseHUDShowInteractionPrompt failed (msg='{msg}'): {ex}");
                throw;
            }
        }

        static void SafeHideInteractionPrompt()
        {
            try
            {
                GameEvents.RaiseHUDHideInteractionPrompt();          // verified file:line 665 GameEvents.cs
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiloTutorial] RaiseHUDHideInteractionPrompt failed: {ex}");
                throw;
            }
        }

        // ----------------------------------------------------------------
        // Public test hooks (for editor / harness invocation)
        // ----------------------------------------------------------------

        /// <summary>Editor / test hook - force-restart the tutorial from step 1.</summary>
        public void DebugRestart()
        {
            CancelInvoke();
            DestroyWaypointArrow();
            SafeHideInteractionPrompt();
            Current = Step.Inactive;
            EnterStep(Step.Step1_Greet);
        }

        /// <summary>Editor / test hook - clear PlayerPrefs first-boot flag.</summary>
        public static void DebugClearFirstBoot()
        {
            PlayerPrefs.DeleteKey(FirstBootPrefKey);
            PlayerPrefs.Save();
            Debug.Log($"[MiloTutorial] Cleared {FirstBootPrefKey} from PlayerPrefs.");
        }
    }
}
