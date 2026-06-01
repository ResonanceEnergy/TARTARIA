using UnityEngine;
using System.Collections;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Save;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Core.Enums;

namespace Tartaria.Integration
{
    /// <summary>
    /// Echohaven (Moon 1) Progression & Save Compatibility System.
    ///
    /// Mission: Make early progression, Skill Tree, and save system work cleanly in the starting hub (Echohaven).
    ///
    /// - Adds meaningful early progression hooks and permanent player changes from restoring the 3 core hub buildings:
    ///   Harmonic Fountain, StarDome, CrystalSpire.
    /// - Each restoration grants a free permanent "Echohaven Blessing" Skill Tree node (600+ ids).
    ///   These are 0-cost auto-unlocks that provide lasting modifiers (tuning, RS, combat) from the very first 5-10 minutes.
    /// - Full hub restoration (all 3) grants capstone "Echohaven Fully Awakened" with global RS multiplier.
    ///   This makes restoring the hub feel like a true permanent power spike and world change that carries forward.
    /// - Full save/load roundtrip via EchohavenSaveBlock (v14 schema) + re-application on load (no lost progress).
    /// - Fixes persistence: restored hub state re-applies blessings/skills/modifiers silently on reload so progression and side-effects survive sessions.
    /// - Wires cleanly into GameEvents.OnBuildingRestored, SkillTreeSystem (via ForceUnlock), GameLoop save hooks, InteractableBuilding restore path.
    /// - AGENT4_BUG3_FIX: Giant Mode tutorial prompt on first Crystal Spire restoration (BUILD_NOTES.md Bug 3).
    ///
    /// Zero scope creep beyond Moon 1 Echohaven starting area progression + save compatibility.
    /// </summary>
    [DisallowMultipleComponent]
    public class EchohavenProgressionSystem : MonoBehaviour
    {
        const string PP_GIANT_TUTORIAL_SEEN = "TARTARIA_GiantModeTutorialSeen_v1";

        public static EchohavenProgressionSystem Instance { get; private set; }

        private bool _fountainRestored;
        private bool _domeRestored;
        private bool _spireRestored;
        private bool _hubFullyRestored;
        private int _hubRestorations;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("EchohavenProgressionSystem");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<EchohavenProgressionSystem>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Hook building restores for Echohaven hub progression (idempotent)
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
        }

        void HandleBuildingRestored(string buildingIdOrName)
        {
            if (string.IsNullOrEmpty(buildingIdOrName)) return;
            string key = buildingIdOrName.ToLowerInvariant();

            bool granted = false;

            if ((key.Contains("fountain") || key.Contains("harmonic")) && !_fountainRestored)
            {
                _fountainRestored = true;
                _hubRestorations++;
                SkillTreeSystem.Instance?.ForceUnlockSkill(SkillId.E_FountainEcho);
                granted = true;
                Debug.Log("[EchohavenProg] Fountain restored — E_FountainEcho blessing granted (permanent early tuning power).");
                GameEvents.FireCriticalSaveTrigger("echohaven_fountain_restored");
            }
            else if (key.Contains("dome") && !_domeRestored)
            {
                _domeRestored = true;
                _hubRestorations++;
                SkillTreeSystem.Instance?.ForceUnlockSkill(SkillId.E_DomeInsight);
                granted = true;
                Debug.Log("[EchohavenProg] Dome restored — E_DomeInsight blessing granted (permanent early discovery/RS power).");
            }
            else if (key.Contains("spire") && !_spireRestored)
            {
                _spireRestored = true;
                _hubRestorations++;
                SkillTreeSystem.Instance?.ForceUnlockSkill(SkillId.E_SpireResonance);
                granted = true;
                Debug.Log("[EchohavenProg] Spire restored — E_SpireResonance blessing granted (permanent early combat power).");

                // AGENT4_BUG3_FIX: Show Giant Mode tutorial prompt on first Crystal Spire restoration
                if (PlayerPrefs.GetInt(PP_GIANT_TUTORIAL_SEEN, 0) == 0)
                {
                    StartCoroutine(ShowGiantModeTutorial());
                }
            }

            if (granted && _fountainRestored && _domeRestored && _spireRestored && !_hubFullyRestored)
            {
                _hubFullyRestored = true;
                SkillTreeSystem.Instance?.ForceUnlockSkill(SkillId.E_HubAwakened);
                // Meaningful permanent change: one-time early RS surge representing the hub singing as one
                AetherFieldManager.Instance?.AddResonanceScore(30f);
                Debug.Log("[EchohavenProg] HUB FULLY RESTORED — E_HubAwakened capstone +30 RS surge. Permanent +8% RS multiplier active for entire journey. Early progression complete; world permanently changed.");
                GameEvents.FireCriticalSaveTrigger("echohaven_hub_restored");
            }

            if (granted)
            {
                SaveManager.Instance?.MarkDirty();
            }
        }

        /// <summary>
        /// Called by GameLoopController during OnAfterLoad to restore persisted hub progression state and re-apply blessings.
        /// Ensures skills/modifiers are active even if loaded after a fresh boot.
        /// </summary>
        public void RestoreFromSaveBlock(EchohavenSaveBlock block)
        {
            if (block == null) return;

            _fountainRestored = block.fountainRestored;
            _domeRestored = block.domeRestored;
            _spireRestored = block.spireRestored;
            _hubFullyRestored = block.hubFullyRestored;
            _hubRestorations = block.hubRestorations;

            // Re-apply permanent blessings (ForceUnlock is safe if already set)
            if (_fountainRestored)
                SkillTreeSystem.Instance?.ForceUnlockSkill(SkillId.E_FountainEcho);
            if (_domeRestored)
                SkillTreeSystem.Instance?.ForceUnlockSkill(SkillId.E_DomeInsight);
            if (_spireRestored)
                SkillTreeSystem.Instance?.ForceUnlockSkill(SkillId.E_SpireResonance);
            if (_hubFullyRestored)
                SkillTreeSystem.Instance?.ForceUnlockSkill(SkillId.E_HubAwakened);

            Debug.Log($"[EchohavenProg] Restored from save — fountain={_fountainRestored}, dome={_domeRestored}, spire={_spireRestored}, hubFull={_hubFullyRestored}. Blessings re-applied for clean save/load.");
        }

        /// <summary>
        /// Called by GameLoopController OnBeforeSave.
        /// </summary>
        public EchohavenSaveBlock GetSaveData()
        {
            return new EchohavenSaveBlock
            {
                fountainRestored = _fountainRestored,
                domeRestored = _domeRestored,
                spireRestored = _spireRestored,
                hubFullyRestored = _hubFullyRestored,
                hubRestorations = _hubRestorations
            };
        }

        public bool IsHubFullyRestored() => _hubFullyRestored;
        /// <summary>
        /// Called from InteractableBuilding restore path for re-application of side effects if needed.
        /// Safe to call multiple times.
        /// </summary>
        public void NotifyBuildingRestoredFromLoad(string buildingIdOrName)
        {
            // Re-run the grant logic (flags prevent double effects, ForceUnlock is safe)
            HandleBuildingRestored(buildingIdOrName);
        }
        public int GetHubRestorationCount() => _hubRestorations;

        /// <summary>
        /// AGENT4_BUG3_FIX: Display first-time Giant Mode activation tutorial.
        /// Triggered after Crystal Spire restoration. Shows multi-stage tutorial with:
        /// - Ability unlock notification
        /// - Input prompt with animation hint
        /// - Gameplay context (when to use, benefits)
        /// Only shows once per player (tracked via PlayerPrefs).
        /// </summary>
        IEnumerator ShowGiantModeTutorial()
        {
            // Mark tutorial as seen immediately to prevent double-trigger
            PlayerPrefs.SetInt(PP_GIANT_TUTORIAL_SEEN, 1);
            PlayerPrefs.Save();

            // Wait for restoration cinematic to complete
            yield return new WaitForSeconds(3.5f);

            // Stage 1: Ability Unlock Notification
            GameEvents.RaiseHUDShowBanner(
                "GIANT MODE UNLOCKED",
                "Harness the power of the Tartarian titans — grow to colossal scale.",
                6f
            );

            // Accessibility caption
            UI.AccessibilityManager.Instance?.PostSFXCaption(
                "Giant Mode",
                "New ability unlocked: Giant Mode. Transform to titan scale."
            );

            // Haptic + SFX feedback
            Audio.AudioManager.Instance?.PlaySFX2D("TutorialDone");
            HapticFeedbackManager.Instance?.PlayPerfectTune();

            yield return new WaitForSeconds(6.5f);

            // Stage 2: Input Prompt with Clear Instructions
            string inputPrompt = GetGiantModeInputPrompt();
            GameEvents.RaiseHUDShowBanner(
                "HOW TO ACTIVATE GIANT MODE",
                inputPrompt,
                8f
            );

            UI.AccessibilityManager.Instance?.PostSFXCaption(
                "Tutorial",
                "Hold the Giant Mode button for 3 seconds to transform. " + inputPrompt
            );

            Audio.AudioManager.Instance?.PlaySFX2D("TutorialStep");

            yield return new WaitForSeconds(8.5f);

            // Stage 3: Gameplay Context & Benefits
            GameEvents.RaiseHUDShowBanner(
                "GIANT MODE BENEFITS",
                "Clear massive rubble • Lift buildings • Combat giant foes • Reshape the world at titan scale.",
                7f
            );

            UI.AccessibilityManager.Instance?.PostSFXCaption(
                "Giant Mode",
                "Use Giant Mode to clear rubble, lift buildings, and battle colossal enemies."
            );

            Audio.AudioManager.Instance?.PlaySFX2D("TutorialStep");

            Debug.Log("[EchohavenProg AGENT4_BUG3_FIX] Giant Mode tutorial complete. PlayerPrefs key set: " + PP_GIANT_TUTORIAL_SEEN);
        }

        /// <summary>
        /// Generate input-device-specific prompt for Giant Mode activation.
        /// Detects active input (gamepad vs KB+M) and returns appropriate instruction.
        /// </summary>
        string GetGiantModeInputPrompt()
        {
            // Check for active gamepad
            var gamepad = UnityEngine.InputSystem.Gamepad.current;
            bool hasGamepad = gamepad != null && gamepad.wasUpdatedThisFrame;

            if (hasGamepad)
            {
                // DualSense / Xbox: RT / Right Trigger
                return "Hold RIGHT TRIGGER (RT) for 3 seconds to grow to giant scale. Watch for the golden aura.";
            }
            else
            {
                // Keyboard + Mouse
                return "Hold G KEY for 3 seconds to grow to giant scale. Watch for the golden aura and camera pull-back.";
            }
        }
    }
}
