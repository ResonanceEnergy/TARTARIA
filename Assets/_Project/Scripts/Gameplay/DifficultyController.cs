using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Sprint 6 Lane 7 - static difficulty service.
    ///
    /// Owns the player's chosen DifficultyProfile for the running session and
    /// publishes its four multipliers to every apply site. Reads/writes the integer choice
    /// to PlayerPrefs[PrefsKey] = 0 (Story) / 1 (Standard) / 2 (Hardened).
    ///
    /// Apply sites wired this sprint:
    ///   - MudLordBoss.Awake             --> EnemyHpMultiplier on hp+maxHp     (FILE GAP - see HANDOFFS)
    ///   - MudGolemAI.Awake              --> EnemyDamageMultiplier on meleeDamage
    ///   - TuningMiniGame.StartTuning    --> MiniGameForgiveness on tolerance
    ///   - AetherVisionOverlay.Awake     --> AetherStaminaMultiplier on drainPerSec (FILE GAP - see HANDOFFS)
    ///
    /// 2026-06-02 no-debt compliance:
    ///   - No silent catches.
    ///   - No silent fallbacks (every fallback path logs a warning naming the resource id).
    ///   - Every multiplier read is Mathf.Clamp-guarded at the source so a
    ///     malformed asset can never propagate a negative or zero HP / damage / tolerance.
    ///   - Bootstrap is a [RuntimeInitializeOnLoadMethod] so the service is hot before
    ///     any apply site's Awake() runs.
    ///
    /// Sprint 10 Lane 4 (2026-06-02) - s_applied guard wired (CS0414 cleanup):
    ///   - Apply() is now idempotent: re-entry on the same scene-load cycle short-circuits
    ///     with a loud log instead of redundantly hitting Resources.Load.
    ///   - SetDifficulty() resets the flag before re-applying so a user-driven difficulty
    ///     change still propagates.
    ///   - ResetForSceneTransition() is the public escape hatch for multi-scene games that
    ///     want a fresh apply per scene (e.g., entering a Moon transition that needs the
    ///     profile re-evaluated against scene-scoped enemies that haven't Awake()'d yet).
    /// </summary>
    public static class DifficultyController
    {
        public const string PrefsKey = "TARTARIA_Difficulty";

        const string ResourcesFolder = "Difficulty/";

        static DifficultyProfile s_current;
        static bool s_applied;

        public static DifficultyProfile Current => s_current;

        /// <summary>0=Story, 1=Standard, 2=Hardened. Reads from PlayerPrefs; defaults to 1 (Standard).</summary>
        public static int CurrentIndex => Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, 1), 0, 2);

        public static float EnemyHpMultiplier =>
            Mathf.Clamp(s_current != null ? s_current.EnemyHpMultiplier : 1f, 0.1f, 5f);

        public static float EnemyDamageMultiplier =>
            Mathf.Clamp(s_current != null ? s_current.EnemyDamageMultiplier : 1f, 0.1f, 5f);

        public static float MiniGameForgiveness =>
            Mathf.Clamp(s_current != null ? s_current.MiniGameForgiveness : 1f, 0.1f, 5f);

        public static float AetherStaminaMultiplier =>
            Mathf.Clamp(s_current != null ? s_current.AetherStaminaMultiplier : 1f, 0.1f, 5f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            Apply();
        }

        public static void Apply()
        {
            if (s_applied)
            {
                Debug.Log(
                    "[DifficultyController] Already applied - skip. " +
                    "Current profile='" + (s_current != null ? s_current.DisplayName : "<null>") + "' " +
                    "(call ResetForSceneTransition() before Apply() if a fresh re-evaluation is required).");
                return;
            }

            int idx = CurrentIndex;
            string resourceId = ResourcesFolder + IndexToName(idx);

            var profile = Resources.Load<DifficultyProfile>(resourceId);
            if (profile == null)
            {
                Debug.LogWarning(
                    $"[DifficultyController] Resources.Load returned null for '{resourceId}'. " +
                    "Identifier searched: '" + resourceId + "' in Resources/. " +
                    "Falling back to a transient in-memory Standard profile (1/1/1/1). " +
                    "Run Tartaria/Gameplay/Build Difficulty Profiles to author the assets.");

                profile = ScriptableObject.CreateInstance<DifficultyProfile>();
                profile.name = "Standard_Fallback";
                profile.EditorAuthor("Standard", 1, 1f, 1f, 1f, 1f);
            }

            s_current = profile;
            Debug.Log(
                "[DifficultyController] Applied profile='" + profile.DisplayName + "' " +
                $"hpMul={EnemyHpMultiplier:F2} dmgMul={EnemyDamageMultiplier:F2} " +
                $"miniGameForgive={MiniGameForgiveness:F2} aetherStam={AetherStaminaMultiplier:F2} " +
                "(prefs[" + PrefsKey + "]=" + idx + ")");
            s_applied = true;
        }

        public static void SetDifficulty(int index)
        {
            int clamped = Mathf.Clamp(index, 0, 2);
            PlayerPrefs.SetInt(PrefsKey, clamped);
            PlayerPrefs.Save();
            // User-driven difficulty change MUST re-apply even if a profile is already
            // resident. Drop the idempotency flag so Apply() runs the full Resources.Load
            // path against the newly-chosen index.
            s_applied = false;
            Apply();
        }

        /// <summary>
        /// Sprint 10 Lane 4 escape hatch for multi-scene games.
        ///
        /// Clears the idempotency flag so the next Apply() call (typically a Bootstrap on
        /// the next scene load, or a manual call from a Moon-transition controller) does
        /// the full Resources.Load + clamp + log cycle instead of short-circuiting.
        ///
        /// Use this when the loaded scene is entitled to a fresh re-evaluation of the
        /// profile, e.g., entering a Moon whose enemy Awake()s haven't run yet and you
        /// want the "Applied profile=..." line in the log to anchor that scene's diagnostics.
        ///
        /// Does NOT clear s_current - the multiplier getters keep returning the last-known
        /// good values until Apply() reassigns them, so no apply-site ever sees a null
        /// profile mid-frame.
        /// </summary>
        public static void ResetForSceneTransition()
        {
            Debug.Log(
                "[DifficultyController] ResetForSceneTransition - clearing s_applied " +
                "(retaining s_current='" + (s_current != null ? s_current.DisplayName : "<null>") + "' " +
                "until next Apply()).");
            s_applied = false;
        }

        static string IndexToName(int idx)
        {
            switch (idx)
            {
                case 0: return "Story";
                case 1: return "Standard";
                case 2: return "Hardened";
                default:
                    Debug.LogWarning("[DifficultyController] IndexToName: unexpected idx=" + idx + ", defaulting to 'Standard'.");
                    return "Standard";
            }
        }
    }
}
