using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Day-3: persistent cross-moon progression spine.
    /// Records which moons have been cleared (via PlayerPrefs so it survives session restarts),
    /// fires events on clear, and exposes a query API for HUD/portal/obelisk gating.
    ///
    /// Singleton, DontDestroyOnLoad, self-bootstraps before scene load.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonProgressTracker : MonoBehaviour, Tartaria.Core.IMoonProgressService
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
            Tartaria.Core.ServiceLocator.MoonProgress = Instance;
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
            // B1 — load per-beat bits
            _beatCleared.Clear();
            for (int n = 1; n <= MoonCount; n++)
                for (int b = 0; b < BeatCount; b++)
                    if (PlayerPrefs.GetInt(BeatKey(n, b), 0) == 1)
                        _beatCleared.Add(BeatPackedKey(n, b));
            Debug.Log($"[MoonProgress] Loaded {_cleared.Count}/{MoonCount} cleared moons; {_beatCleared.Count} beat bits.");
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
                try { OnMoonCleared?.Invoke(moonNumber); } catch (System.Exception ex) { Debug.LogWarning($"[MoonProgress] OnMoonCleared listener failed: {ex.Message}"); }
                Tartaria.Core.GameEvents.FireMoonCleared(moonNumber);
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
            for (int n = 1; n <= MoonCount; n++)
                for (int b = 0; b < BeatCount; b++)
                    PlayerPrefs.DeleteKey(BeatKey(n, b));
            PlayerPrefs.Save();
            _cleared.Clear();
            _beatCleared.Clear();
            Debug.Log("[MoonProgress] All progression reset.");
        }

        // ─── B1 Moon Framework v2: per-beat persistence ───
        public const int BeatCount = 5;
        const string BeatKeyPrefix = "TARTARIA_MoonBeat_";
        readonly HashSet<long> _beatCleared = new HashSet<long>();
        public static event System.Action<int /*moon*/, int /*beat*/> OnBeatCleared;

        static string BeatKey(int moon, int beat) => $"{BeatKeyPrefix}{moon}_{beat}";
        static long   BeatPackedKey(int moon, int beat) => ((long)moon << 8) | (long)beat;

        public bool IsBeatCleared(int moon, int beat)
        {
            if (moon < 1 || moon > MoonCount || beat < 0 || beat >= BeatCount) return false;
            return _beatCleared.Contains(BeatPackedKey(moon, beat));
        }

        public void MarkBeatCleared(int moon, int beat)
        {
            if (moon < 1 || moon > MoonCount || beat < 0 || beat >= BeatCount) return;
            var k = BeatPackedKey(moon, beat);
            if (_beatCleared.Add(k))
            {
                PlayerPrefs.SetInt(BeatKey(moon, beat), 1);
                PlayerPrefs.Save();
                Debug.Log($"[MoonProgress] beat {beat} on Moon {moon:D2} cleared.");
                try { OnBeatCleared?.Invoke(moon, beat); } catch (System.Exception ex) { Debug.LogWarning($"[MoonProgress] OnBeatCleared listener failed: {ex.Message}"); }

                // Final beat (Revelation = 4) implies whole-moon clear.
                if (beat == BeatCount - 1 && !IsCleared(moon))
                    MarkCleared(moon);
            }
        }

        public int BeatsCleared(int moon)
        {
            if (moon < 1 || moon > MoonCount) return 0;
            int n = 0;
            for (int b = 0; b < BeatCount; b++)
                if (_beatCleared.Contains(BeatPackedKey(moon, b))) n++;
            return n;
        }

        // ─── Moon 3 Specific Payoff Hooks (Continental Rail fast travel, golden rails, post-escort) ───
        const string Moon3FastTravelKey = "TARTARIA_Moon3_ContinentalRailFastTravel";
        const string Moon3GoldenRailsKey = "TARTARIA_Moon3_GoldenRailsPermanent";

        public bool IsContinentalRailFastTravelUnlocked =>
            PlayerPrefs.GetInt(Moon3FastTravelKey, 0) == 1 ||
            (Tartaria.Save.SaveManager.Instance?.CurrentSave?.moon3?.continentalFastTravelUnlocked ?? false);

        public bool HasGoldenRailsPermanent =>
            PlayerPrefs.GetInt(Moon3GoldenRailsKey, 0) == 1 ||
            (Tartaria.Save.SaveManager.Instance?.CurrentSave?.moon3?.goldenRailsPermanent ?? false);

        public void MarkMoon3ContinentalRailUnlocked()
        {
            PlayerPrefs.SetInt(Moon3FastTravelKey, 1);
            PlayerPrefs.SetInt(Moon3GoldenRailsKey, 1);
            PlayerPrefs.Save();
            Debug.Log("[MoonProgress] Moon 3 Continental Rail fast travel + golden rails permanent world changes unlocked (Campaign + SpectralOrphan payoff).");
            // Fire any listeners for rail UI / zone portals
        }

        /// <summary>Resets Moon 3 specific progression (debug/new game support).</summary>
        public void ResetMoon3Progress()
        {
            PlayerPrefs.DeleteKey(Moon3FastTravelKey);
            PlayerPrefs.DeleteKey(Moon3GoldenRailsKey);
            PlayerPrefs.Save();
        }
    }
}
