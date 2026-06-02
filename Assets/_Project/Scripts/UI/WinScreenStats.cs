using System;
using System.Text;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// WinScreenStats — cross-playthrough counters fed into Moon1WinScreen.
    ///
    /// Subscribes to GameEvents in a RuntimeInitializeOnLoadMethod bootstrap so
    /// the counters start ticking from the first frame of the build, regardless
    /// of which scene loads first. Counters are static so the win card can read
    /// them no matter when in the playthrough OnMoonCompleted fires.
    ///
    /// Note on ElapsedSeconds: tracked by a tiny static MonoBehaviour helper
    /// (StatsTicker) that DontDestroyOnLoad-anchors itself in the bootstrap.
    /// </summary>
    public static class WinScreenStats
    {
        // ─── Tracked fields ───────────────────────────────────────────────
        public static float ElapsedSeconds;
        public static int BuildingsRestored;
        public static int GolemsDefeated;
        public static float MiniGameAccuracySum;
        public static int MiniGameAttempts;
        public static int RSAwarded;

        // ─── Internal: last RS observed so we can compute deltas ──────────
        static float _lastRS = 0f;
        static bool _haveRSBaseline = false;

        /// <summary>
        /// Multi-line stats summary for the Moon 1 win card. Padded out with
        /// blank line separators and includes only the fields with meaningful
        /// data (no "0 / 0 accuracy" garbage).
        /// </summary>
        public static string FormatStats()
        {
            int min = Mathf.FloorToInt(ElapsedSeconds / 60f);
            int sec = Mathf.FloorToInt(ElapsedSeconds % 60f);

            var sb = new StringBuilder();
            sb.AppendLine($"Time Played:          {min:00}:{sec:00}");
            sb.AppendLine($"Buildings Restored:   {BuildingsRestored}");
            sb.AppendLine($"Golems Defeated:      {GolemsDefeated}");

            if (MiniGameAttempts > 0)
            {
                float avg = Mathf.Clamp01(MiniGameAccuracySum / MiniGameAttempts) * 100f;
                sb.AppendLine($"Tuning Accuracy:      {avg:0.0}%  ({MiniGameAttempts} attempts)");
            }
            else
            {
                sb.AppendLine("Tuning Accuracy:      --");
            }

            sb.Append($"Resonance Earned:     +{RSAwarded} RS");
            return sb.ToString();
        }

        /// <summary>Resets counters — call between New Game starts.</summary>
        public static void ResetAll()
        {
            ElapsedSeconds = 0f;
            BuildingsRestored = 0;
            GolemsDefeated = 0;
            MiniGameAccuracySum = 0f;
            MiniGameAttempts = 0;
            RSAwarded = 0;
            _lastRS = 0f;
            _haveRSBaseline = false;
        }

        // ═════════════════════════════════════════════════════════════════
        // BOOTSTRAP — wires GameEvents subscriptions + spawns elapsed-time ticker
        // ═════════════════════════════════════════════════════════════════
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            // Reset on bootstrap so domain reload in editor doesn't carry stale data
            ResetAll();

            // Buildings — fires once per Tartarian building restoration completion.
            GameEvents.OnBuildingRestored += HandleBuildingRestored;

            // Enemies — Moon 1 mobs are golems, so any kill counts.
            GameEvents.OnEnemyKilled += HandleEnemyKilled;

            // Tuning mini-game completion accuracy.
            // BuildingRestoredEventArgs carries tuningAccuracy on the typed event,
            // so we tap that as the canonical "mini-game complete" signal.
            GameEvents.OnBuildingRestoredTyped += HandleBuildingRestoredTyped;

            // Resonance score — track positive deltas as "RS awarded this run".
            GameEvents.OnResonanceScoreChanged += HandleRSChanged;

            // Spawn the elapsed-time ticker on a hidden DontDestroyOnLoad GO.
            var tickerGo = new GameObject("WinScreenStatsTicker");
            UnityEngine.Object.DontDestroyOnLoad(tickerGo);
            tickerGo.hideFlags = HideFlags.HideAndDontSave;
            tickerGo.AddComponent<StatsTicker>();
        }

        static void HandleBuildingRestored(string buildingId)
        {
            BuildingsRestored++;
        }

        static void HandleEnemyKilled(EnemyKilledEventArgs args)
        {
            if (args == null) return;
            GolemsDefeated++;
        }

        static void HandleBuildingRestoredTyped(BuildingRestoredEventArgs args)
        {
            if (args == null) return;
            // tuningAccuracy is 0-1 per BuildingRestoredEventArgs schema.
            MiniGameAccuracySum += Mathf.Clamp01(args.tuningAccuracy);
            MiniGameAttempts++;
        }

        static void HandleRSChanged(float newRS)
        {
            if (!_haveRSBaseline)
            {
                _lastRS = newRS;
                _haveRSBaseline = true;
                return;
            }
            float delta = newRS - _lastRS;
            _lastRS = newRS;
            if (delta > 0f) RSAwarded += Mathf.RoundToInt(delta);
        }

        /// <summary>
        /// Hidden MonoBehaviour that increments ElapsedSeconds each unscaled
        /// frame so pause menus don't artificially inflate playtime.
        /// </summary>
        sealed class StatsTicker : MonoBehaviour
        {
            void Update()
            {
                ElapsedSeconds += Time.unscaledDeltaTime;
            }
        }
    }
}
