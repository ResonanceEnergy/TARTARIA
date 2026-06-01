using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// B1 — Single reward funnel for the 5-beat Moon Framework v2.
    /// Idempotent per (moon, beat) so award is paid at most once.
    /// Splits the moon's total RS across beats and pays achievements / unlocks
    /// at the Revelation beat (= moon fully cleared).
    /// </summary>
    public static class MoonRewardService
    {
        // RS payout weights across the 5 beats; sums to 1.0
        static readonly float[] BeatWeights = { 0.05f, 0.10f, 0.40f, 0.25f, 0.20f };

        static readonly HashSet<long> _paid = new HashSet<long>();

        static long Key(int moon, int beat) => ((long)moon << 8) | (long)beat;

        public static void AwardBeat(MoonDefinition def, MoonBeatRunner.Beat beat)
        {
            if (def == null) return;
            int moon = def.number;
            int b = (int)beat;
            var k = Key(moon, b);
            if (_paid.Contains(k)) return;
            _paid.Add(k);

            float total = def.rewardRS > 0f ? def.rewardRS : 15f + moon * 2f;
            float share = total * BeatWeights[Mathf.Clamp(b, 0, BeatWeights.Length - 1)];

            // RS payout — uses existing GameLoopController funnel
            try { GameLoopController.Instance?.QueueRSReward(share, $"moon{moon:D2}_beat{b}"); }
            catch (System.Exception ex) { Debug.LogWarning($"[MoonReward] RS payout failed: {ex.Message}"); }

            // Final-beat extras: achievement + cross-moon unlock
            if (beat == MoonBeatRunner.Beat.Revelation)
            {
                try { AchievementSystem.Instance?.Unlock($"K{moon:D2}"); }
                catch (System.Exception ex) { Debug.LogWarning($"[MoonReward] Achievement unlock failed: {ex.Message}"); }

                if (def.unlockMoonId > 0)
                    Debug.Log($"[MoonReward] Moon {moon:D2} ✓ → Moon {def.unlockMoonId:D2} unlocked.");
                Debug.Log($"[MoonReward] Moon {moon:D2} full clear — +{Mathf.RoundToInt(total)} RS total.");
            }
            else
            {
                Debug.Log($"[MoonReward] Moon {moon:D2} beat {beat} → +{Mathf.RoundToInt(share)} RS.");
            }
        }

        /// <summary>Test/dev hook — clears paid memo so awards can fire again.</summary>
        public static void ResetMemo() => _paid.Clear();

        /// <summary>Returns true if a given (moon, beat) has already been paid this session.</summary>
        public static bool IsPaid(int moon, int beat) => _paid.Contains(Key(moon, beat));
    }
}
