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
            s_applied = true;
            Debug.Log(
                "[DifficultyController] Applied profile='" + profile.DisplayName + "' " +
                $"hpMul={EnemyHpMultiplier:F2} dmgMul={EnemyDamageMultiplier:F2} " +
                $"miniGameForgive={MiniGameForgiveness:F2} aetherStam={AetherStaminaMultiplier:F2} " +
                "(prefs[" + PrefsKey + "]=" + idx + ")");
        }

        public static void SetDifficulty(int index)
        {
            int clamped = Mathf.Clamp(index, 0, 2);
            PlayerPrefs.SetInt(PrefsKey, clamped);
            PlayerPrefs.Save();
            Apply();
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
