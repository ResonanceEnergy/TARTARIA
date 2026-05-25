using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Tartaria.Integration;

namespace Tartaria.Tests.EditMode
{
    /// <summary>
    /// Yarn Spinner smoke tests — verify dialogue data loading and variable storage.
    /// Wave 3: 4 tests added (19 → 23 total EditMode tests).
    /// </summary>
    public class YarnSmokeTests
    {
        [Test]
        public void Milo_Intro_Yarn_Loads()
        {
            // Load .yarn file as TextAsset
            var yarnText = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Project/Data/Dialogue/Milo_Intro.yarn");
            Assert.IsNotNull(yarnText, "Milo_Intro.yarn should exist in Assets/_Project/Data/Dialogue/");
            
            // Verify it contains the title node
            string content = yarnText.text;
            Assert.IsTrue(content.Contains("title: Milo_Intro"), "Yarn file should contain 'title: Milo_Intro' node");
            Assert.IsTrue(content.Contains("Milo: Welcome to Tartaria"), "Yarn file should contain Milo's greeting");
        }

        [Test]
        public void TartariaVariableStorage_Returns_Zero_RS_Initially()
        {
            var storage = new GameObject("TestStorage").AddComponent<TartariaVariableStorage>();
            
            // $rs should return 0 when no ECS world exists
            bool found = storage.TryGetValue<float>("$rs", out var rs);
            
            Assert.IsTrue(found, "$rs should be found in variable storage");
            Assert.AreEqual(0f, rs, "$rs should return 0 when no ResonanceScore entity exists");
            
            Object.DestroyImmediate(storage.gameObject);
        }

        [Test]
        public void TartariaVariableStorage_Moon_Returns_Zero_Initially()
        {
            var storage = new GameObject("TestStorage").AddComponent<TartariaVariableStorage>();
            
            // $moon should return 0 when MoonProgressTracker.Instance is null
            bool found = storage.TryGetValue<float>("$moon", out var moon);
            
            Assert.IsTrue(found, "$moon should be found in variable storage");
            Assert.AreEqual(0f, moon, "$moon should return 0 when MoonProgressTracker is not initialized");
            
            Object.DestroyImmediate(storage.gameObject);
        }

        [Test]
        public void TartariaVariableStorage_Companion_Returns_Milo()
        {
            var storage = new GameObject("TestStorage").AddComponent<TartariaVariableStorage>();
            
            // $companion should return "Milo" (default companion)
            bool found = storage.TryGetValue<string>("$companion", out var companion);
            
            Assert.IsTrue(found, "$companion should be found in variable storage");
            Assert.AreEqual("Milo", companion, "$companion should default to 'Milo'");
            
            Object.DestroyImmediate(storage.gameObject);
        }
    }
}
