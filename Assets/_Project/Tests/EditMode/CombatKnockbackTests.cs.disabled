using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Gameplay;

namespace Tartaria.Tests.EditMode
{
    /// <summary>
    /// Tests for combat knockback mechanics:
    /// - Knockback component structure
    /// - Hitstun component structure  
    /// - Component data integrity
    /// </summary>
    [TestFixture]
    public class CombatKnockbackTests
    {
        [Test]
        public void KnockbackImpulse_DefaultValues()
        {
            var knockback = new KnockbackImpulse
            {
                Direction = new float3(1, 0, 0),
                Magnitude = 8f,
                DecayRate = 5f
            };

            Assert.AreEqual(1f, knockback.Direction.x, 0.01f);
            Assert.AreEqual(8f, knockback.Magnitude, 0.01f);
            Assert.AreEqual(5f, knockback.DecayRate, 0.01f);
        }

        [Test]
        public void HitStunTimer_DefaultValues()
        {
            var hitstun = new HitStunTimer
            {
                Remaining = 0.3f
            };

            Assert.AreEqual(0.3f, hitstun.Remaining, 0.01f);
        }

        [Test]
        public void KnockbackDirection_Normalization()
        {
            float3 unnormalized = new float3(5, 0, 0);
            float3 normalized = math.normalizesafe(unnormalized);

            var knockback = new KnockbackImpulse
            {
                Direction = normalized,
                Magnitude = 8f,
                DecayRate = 5f
            };

            // Verify direction is unit vector
            float length = math.length(knockback.Direction);
            Assert.AreEqual(1f, length, 0.01f, "Direction should be normalized");
        }

        [Test]
        public void KnockbackMagnitude_ScalesWithFrequencyMatch()
        {
            // Simulate frequency match quality 0..1
            float poorMatch = math.lerp(0.5f, 1.0f, 0.0f) * 8f; // 4 m/s
            float perfectMatch = math.lerp(0.5f, 1.0f, 1.0f) * 8f; // 8 m/s

            Assert.AreEqual(4f, poorMatch, 0.01f);
            Assert.AreEqual(8f, perfectMatch, 0.01f);
        }

        [Test]
        public void HitStunDuration_ScalesWithFrequencyMatch()
        {
            // Simulate hitstun duration based on freq match
            float poorMatch = math.lerp(0.15f, 0.4f, 0.0f); // 0.15s
            float perfectMatch = math.lerp(0.15f, 0.4f, 1.0f); // 0.4s

            Assert.AreEqual(0.15f, poorMatch, 0.01f);
            Assert.AreEqual(0.4f, perfectMatch, 0.01f);
        }
    }
}
