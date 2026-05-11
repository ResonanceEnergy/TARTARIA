using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Day-3: persistent cross-moon progression spine.
    /// Records which moons have been cleared (via PlayerPrefs so it survives session restarts),
    /// fires events on clear, and exposes a query API for HUD/portal/obelisk gating.
    ///
    /// Singleton, DontDestroyOnLoad, self-bootstraps before scene load.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonProgressTracker : MonoBehaviour
    {
        const string PrefKeyPrefix = "TARTARIA_MoonCleared_";
        public const int MoonCount = 13;

        public static MoonProgressTracker Instance { get; private set; }

        public static event System.Action<int> OnMoonCleared;

        readonly HashSet<int> _cleared = new HashSet<int>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("MoonProgressTracker");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<MoonProgressTracker>();
            Instance.LoadFromPrefs();
        }

        void LoadFromPrefs()
        {
            _cleared.Clear();
            for (int n = 1; n <= MoonCount; n++)
            {
                if (PlayerPrefs.GetInt(PrefKeyPrefix + n, 0) == 1)
                    _cleared.Add(n);
            }
            Debug.Log($"[MoonProgress] Loaded {_cleared.Count}/{MoonCount} cleared moons.");
        }

        public bool IsCleared(int moonNumber) => _cleared.Contains(moonNumber);

        public int ClearedCount => _cleared.Count;

        public IEnumerable<int> ClearedMoons => _cleared;

        public void MarkCleared(int moonNumber)
        {
            if (moonNumber < 1 || moonNumber > MoonCount) return;
            if (_cleared.Add(moonNumber))
            {
                PlayerPrefs.SetInt(PrefKeyPrefix + moonNumber, 1);
                PlayerPrefs.Save();
                Debug.Log($"[MoonProgress] ✓ Moon {moonNumber:D2} marked cleared. Total: {_cleared.Count}/{MoonCount}");
                try { OnMoonCleared?.Invoke(moonNumber); } catch { /* swallow listener errors */ }
                // Achievements: per-moon K01…K13 + final True Ending H12 trigger when all cleared.
                AchievementSystem.Instance?.Unlock($"K{moonNumber:D2}");
                if (_cleared.Count >= MoonCount)
                    AchievementSystem.Instance?.Unlock("H12");
            }
        }

        /// <summary>Reset all progression — used by debug menu / new game.</summary>
        public void ResetAll()
        {
            for (int n = 1; n <= MoonCount; n++)
                PlayerPrefs.DeleteKey(PrefKeyPrefix + n);
            PlayerPrefs.Save();
            _cleared.Clear();
            Debug.Log("[MoonProgress] All progression reset.");
        }
    }
}
