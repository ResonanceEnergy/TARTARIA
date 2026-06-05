using Unity.Entities;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// DOTS tag component added by CombatBridge when a HarmonicCombatant's health
    /// reaches zero. Consumed by EnemyAISystem / CombatWaveManager to trigger
    /// death VFX, reward distribution, and wave-clear bookkeeping.
    /// </summary>
    public struct EnemyDeathTag : IComponentData { }
}
