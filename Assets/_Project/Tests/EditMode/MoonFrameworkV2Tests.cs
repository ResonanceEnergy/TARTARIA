using NUnit.Framework;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Integration;

namespace Tartaria.Tests.EditMode
{
    /// <summary>
    /// B1 — EditMode tests for the Moon Framework v2 spine.
    /// Tests the persistence + reward funnel without entering Play mode.
    /// MoonBeatRunner's coroutine behavior is exercised via the bits it writes,
    /// not by running it — coroutines need Play mode.
    /// </summary>
    public class MoonFrameworkV2Tests
    {
        const int TestMoon = 12; // Use a high moon number unlikely to be set in dev prefs.

        [SetUp]
        public void Setup()
        {
            // Wipe any prior bits for the test moon so each test is hermetic.
            PlayerPrefs.DeleteKey($"TARTARIA_MoonCleared_{TestMoon}");
            for (int b = 0; b < 5; b++)
                PlayerPrefs.DeleteKey($"TARTARIA_MoonBeat_{TestMoon}_{b}");
            PlayerPrefs.Save();

            EnsureTracker();
            MoonProgressTracker.Instance.ResetAll();
            MoonRewardService.ResetMemo();
        }

        [TearDown]
        public void Teardown()
        {
            if (MoonProgressTracker.Instance != null)
                MoonProgressTracker.Instance.ResetAll();
            MoonRewardService.ResetMemo();
        }

        static void EnsureTracker()
        {
            if (MoonProgressTracker.Instance != null) return;
            // Invoke the private static Bootstrap() — it creates the singleton GO,
            // sets DontDestroyOnLoad, and writes the Instance property.
            var bootstrap = typeof(MoonProgressTracker).GetMethod(
                "Bootstrap",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(bootstrap, "MoonProgressTracker.Bootstrap private static method not found.");
            bootstrap.Invoke(null, null);
            Assert.IsNotNull(MoonProgressTracker.Instance);
        }

        [Test]
        public void MarkBeatCleared_PersistsBitAndIsIdempotent()
        {
            var tr = MoonProgressTracker.Instance;
            Assert.IsFalse(tr.IsBeatCleared(TestMoon, 0));
            tr.MarkBeatCleared(TestMoon, 0);
            Assert.IsTrue(tr.IsBeatCleared(TestMoon, 0));
            // Idempotent — second call must not throw and bit remains set.
            tr.MarkBeatCleared(TestMoon, 0);
            Assert.IsTrue(tr.IsBeatCleared(TestMoon, 0));
            Assert.AreEqual(1, tr.BeatsCleared(TestMoon));
        }

        [Test]
        public void FinalBeat_ImpliesWholeMoonClear()
        {
            var tr = MoonProgressTracker.Instance;
            Assert.IsFalse(tr.IsCleared(TestMoon));
            tr.MarkBeatCleared(TestMoon, MoonProgressTracker.BeatCount - 1);
            Assert.IsTrue(tr.IsBeatCleared(TestMoon, MoonProgressTracker.BeatCount - 1));
            Assert.IsTrue(tr.IsCleared(TestMoon), "Marking the Revelation beat must imply a whole-moon clear.");
        }

        [Test]
        public void OutOfRangeArgs_AreRejectedSafely()
        {
            var tr = MoonProgressTracker.Instance;
            Assert.DoesNotThrow(() => tr.MarkBeatCleared(0, 0));
            Assert.DoesNotThrow(() => tr.MarkBeatCleared(TestMoon, -1));
            Assert.DoesNotThrow(() => tr.MarkBeatCleared(TestMoon, 99));
            Assert.IsFalse(tr.IsBeatCleared(0, 0));
            Assert.IsFalse(tr.IsBeatCleared(TestMoon, 99));
        }

        [Test]
        public void RewardService_IsIdempotentPerBeat()
        {
            var def = ScriptableObject.CreateInstance<MoonDefinition>();
            def.number   = TestMoon;
            def.rewardRS = 100f;
            try
            {
                Assert.IsFalse(MoonRewardService.IsPaid(TestMoon, 0));
                MoonRewardService.AwardBeat(def, MoonBeatRunner.Beat.Discovery);
                Assert.IsTrue(MoonRewardService.IsPaid(TestMoon, 0));
                // Second call is a no-op.
                MoonRewardService.AwardBeat(def, MoonBeatRunner.Beat.Discovery);
                Assert.IsTrue(MoonRewardService.IsPaid(TestMoon, 0));
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void RewardService_LegacyComputeWhenRewardRSZero()
        {
            var def = ScriptableObject.CreateInstance<MoonDefinition>();
            def.number   = TestMoon;
            def.rewardRS = 0f;
            try
            {
                // Must not throw even when GameLoopController.Instance is null.
                Assert.DoesNotThrow(() => MoonRewardService.AwardBeat(def, MoonBeatRunner.Beat.Revelation));
                Assert.IsTrue(MoonRewardService.IsPaid(TestMoon, (int)MoonBeatRunner.Beat.Revelation));
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void MoonDefinition_DefaultBeatArraysHaveCorrectShape()
        {
            var def = ScriptableObject.CreateInstance<MoonDefinition>();
            try
            {
                Assert.IsNotNull(def.beatHeadlines);
                Assert.AreEqual(5, def.beatHeadlines.Length);
                Assert.IsNotNull(def.beatDurations);
                Assert.AreEqual(5, def.beatDurations.Length);
                Assert.AreEqual(0f, def.rewardRS);
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }
    }
}
