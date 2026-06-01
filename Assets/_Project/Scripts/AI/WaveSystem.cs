using UnityEngine;

namespace Tartaria.AI
{
    /// <summary>
    /// WaveSystem --- thin public facade over MudGolemSpawner for non-AI callers
    /// (gameplay scripts, debug menus, encounter triggers). Exists so the wave
    /// contract has a stable seam that survives spawner refactors.
    ///
    /// Per agent/ai/wave-spawner-tuning the actual spawn logic, cap enforcement,
    /// and corpse cleanup live in MudGolemSpawner --- this file only forwards.
    /// </summary>
    public static class WaveSystem
    {
        public static int AliveCount => MudGolemSpawner.Instance != null ? MudGolemSpawner.Instance.AliveCount : 0;
        public static int WaveNumber => MudGolemSpawner.Instance != null ? MudGolemSpawner.Instance.WaveNumber : 0;
        public static int RestoredCount => MudGolemSpawner.Instance != null ? MudGolemSpawner.Instance.RestoredCount : 0;
        public static int MaxAlive => MudGolemSpawner.MAX_ALIVE;

        /// <summary>
        /// Request a wave of <paramref name="size"/> golems. The spawner will clamp
        /// against the live cap (max 3 alive) and return how many actually spawned.
        /// </summary>
        public static int RequestWave(int size)
        {
            EnsureSpawner();
            return MudGolemSpawner.Instance != null ? MudGolemSpawner.Instance.SpawnWave(size) : 0;
        }

        /// <summary>
        /// Register a golem instantiated outside the spawner so it counts toward
        /// the live cap and gets the 10s corpse cleanup.
        /// </summary>
        public static void RegisterExternalGolem(MudGolemHealth health)
        {
            EnsureSpawner();
            MudGolemSpawner.Instance?.Register(health);
        }

        private static void EnsureSpawner()
        {
            if (MudGolemSpawner.Instance != null) return;
            var go = new GameObject("[MudGolemSpawner]");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<MudGolemSpawner>();
        }
    }
}
