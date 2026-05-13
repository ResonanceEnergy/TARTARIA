using NUnit.Framework;
using Tartaria.Core;

namespace Tartaria.Tests.EditMode
{
    public class GoldenRatioValidatorTests
    {
        const float Phi = 1.6180339887f;

        [Test]
        public void GetMultiplier_PerfectPhi_ReturnsPhi()
        {
            float m = GoldenRatioValidator.GetMultiplier(Phi);
            Assert.AreEqual(Phi, m, 0.0001f);
        }

        [Test]
        public void GetMultiplier_FarFromPhi_ReturnsOne()
        {
            Assert.AreEqual(1f, GoldenRatioValidator.GetMultiplier(0.5f), 0.0001f);
            Assert.AreEqual(1f, GoldenRatioValidator.GetMultiplier(3.0f), 0.0001f);
        }

        [Test]
        public void GetMultiplier_SlightlyOff_ReturnsBetweenOneAndPhi()
        {
            float m = GoldenRatioValidator.GetMultiplier(Phi * 1.05f);
            Assert.That(m, Is.GreaterThan(1f).And.LessThan(Phi));
        }

        [TestCase(1f, Phi, 1f)]   // perfect 1:phi → score 1
        [TestCase(0f, 1f, 0f)]    // invalid input → 0
        [TestCase(1f, 1f, 0f)]    // 1:1 ratio → far from phi → 0 score
        public void ValidateBuildingProportion_KnownInputs(float w, float h, float expected)
        {
            float score = GoldenRatioValidator.ValidateBuildingProportion(w, h);
            Assert.AreEqual(expected, score, 0.05f);
        }

        [Test]
        public void IsGoldenSpiral_PhiPair_True()
        {
            Assert.IsTrue(GoldenRatioValidator.IsGoldenSpiral(1f, Phi, 0.02f));
        }

        [Test]
        public void IsGoldenSpiral_NonPhi_False()
        {
            Assert.IsFalse(GoldenRatioValidator.IsGoldenSpiral(1f, 2f, 0.02f));
        }

        [Test]
        public void GetGoldenPair_ReturnsBaseTimesPhi()
        {
            Assert.AreEqual(10f * Phi, GoldenRatioValidator.GetGoldenPair(10f), 0.0001f);
        }

        [Test]
        public void GetFrequencyAccuracy_432_ReturnsOne()
        {
            Assert.AreEqual(1f, GoldenRatioValidator.GetFrequencyAccuracy(432f), 0.0001f);
        }

        [Test]
        public void GetFrequencyAccuracy_DoubleTarget_ReturnsZero()
        {
            Assert.AreEqual(0f, GoldenRatioValidator.GetFrequencyAccuracy(864f), 0.0001f);
        }

        [Test]
        public void Constants_HaveExpectedValues()
        {
            Assert.AreEqual(Phi, GoldenRatioValidator.PHI, 0.0001f);
            Assert.AreEqual(1f / Phi, GoldenRatioValidator.PHI_INVERSE, 0.001f);
            Assert.AreEqual(Phi * Phi, GoldenRatioValidator.PHI_SQUARED, 0.001f);
        }
    }
}
