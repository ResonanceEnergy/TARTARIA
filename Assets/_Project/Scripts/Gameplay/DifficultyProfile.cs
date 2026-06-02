using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Sprint 6 Lane 7 - asset-driven difficulty profile.
    ///
    /// A ScriptableObject carrying the four multipliers wired into
    /// real apply sites by DifficultyController:
    ///   - EnemyHpMultiplier        --> MudLordBoss hp/maxHp init
    ///   - EnemyDamageMultiplier    --> MudGolemAI meleeDamage scale
    ///   - MiniGameForgiveness      --> TuningMiniGame tolerance window
    ///   - AetherStaminaMultiplier  --> AetherVisionOverlay drainPerSec (divided)
    ///
    /// Author 3 instances under Assets/_Project/Data/Difficulty/ via the
    /// editor menu Tartaria/Gameplay/Build Difficulty Profiles:
    ///   Story.asset    (0.6 / 0.6 / 1.3 / 1.5)
    ///   Standard.asset (1.0 / 1.0 / 1.0 / 1.0)
    ///   Hardened.asset (1.5 / 1.4 / 0.7 / 0.7)
    ///
    /// 2026-06-02 no-debt compliance:
    ///   - No banned namespace (Tartaria.Gameplay is fine).
    ///   - No invented APIs - readers call DifficultyController only.
    ///   - Multipliers are clamped at the consumer; the asset author still gets full freedom
    ///     in the Inspector to express the profile and the runtime defends against malformed
    ///     values per CLAUDE.md mandate (rule: "Use Mathf.Clamp on every multiplier read").
    /// </summary>
    [CreateAssetMenu(
        fileName = "Difficulty",
        menuName = "Tartaria/Gameplay/Difficulty Profile",
        order = 50)]
    public class DifficultyProfile : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name shown in settings UI ('Story', 'Standard', 'Hardened').")]
        [SerializeField] string displayName = "Standard";

        [Tooltip("Index baked into PlayerPrefs 'TARTARIA_Difficulty' (0=Story, 1=Standard, 2=Hardened).")]
        [SerializeField, Range(0, 2)] int difficultyIndex = 1;

        [Header("Multipliers")]
        [Tooltip("Enemy HP scaling. 1.0 = canonical, <1 = easier, >1 = harder. Applied to MudLordBoss hp/maxHp.")]
        [SerializeField, Range(0.1f, 5f)] float _enemyHpMultiplier = 1f;

        [Tooltip("Enemy damage output scaling. Applied to MudGolemAI melee damage.")]
        [SerializeField, Range(0.1f, 5f)] float _enemyDamageMultiplier = 1f;

        [Tooltip("Mini-game tolerance widening. 1.0 = canonical, 1.3 = +30% bigger window, 0.7 = -30% tighter. Applied to TuningMiniGame tolerance.")]
        [SerializeField, Range(0.1f, 5f)] float _miniGameForgiveness = 1f;

        [Tooltip("Aether stamina scaling. drainPerSec is DIVIDED by this (>1 = drains slower). Applied to AetherVisionOverlay.")]
        [SerializeField, Range(0.1f, 5f)] float _aetherStaminaMultiplier = 1f;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public int DifficultyIndex => Mathf.Clamp(difficultyIndex, 0, 2);

        public float EnemyHpMultiplier        => Mathf.Clamp(_enemyHpMultiplier,       0.1f, 5f);
        public float EnemyDamageMultiplier    => Mathf.Clamp(_enemyDamageMultiplier,   0.1f, 5f);
        public float MiniGameForgiveness      => Mathf.Clamp(_miniGameForgiveness,     0.1f, 5f);
        public float AetherStaminaMultiplier  => Mathf.Clamp(_aetherStaminaMultiplier, 0.1f, 5f);

        public void EditorAuthor(string displayNameIn, int indexIn,
            float hpMul, float dmgMul, float miniGameMul, float aetherMul)
        {
            displayName = displayNameIn;
            difficultyIndex = Mathf.Clamp(indexIn, 0, 2);
            _enemyHpMultiplier       = Mathf.Clamp(hpMul,       0.1f, 5f);
            _enemyDamageMultiplier   = Mathf.Clamp(dmgMul,      0.1f, 5f);
            _miniGameForgiveness     = Mathf.Clamp(miniGameMul, 0.1f, 5f);
            _aetherStaminaMultiplier = Mathf.Clamp(aetherMul,   0.1f, 5f);
        }
    }
}
