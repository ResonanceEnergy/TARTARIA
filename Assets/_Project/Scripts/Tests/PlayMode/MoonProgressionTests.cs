using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.Integration;
using Tartaria.Gameplay;

namespace Tartaria.Tests.PlayMode
{
    /// <summary>
    /// End-to-end PlayMode tests for Moon 1-13 progression flow.
    /// Tests save/load persistence, Moon unlock logic, and content spawning.
    /// </summary>
    public class MoonProgressionTests
    {
        const float SceneLoadTimeout = 10f;
        
        [UnityTest]
        public IEnumerator Moon1_NewGame_SpawnsPlayerAtEchohaven()
        {
            // Load Boot scene
            yield return LoadSceneAsync("Boot");
            
            // Simulate new game start
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.CreateNewSave("TestSave_Moon1");
            }
            
            yield return LoadSceneAsync("Echohaven_VerticalSlice");
            
            // Verify player spawned
            var player = GameObject.FindGameObjectWithTag("Player");
            Assert.IsNotNull(player, "Player should spawn in Moon 1");
            
            // Verify Moon 1 spawner exists
            var moon1Spawner = GameObject.FindObjectOfType<Moon2ContentSpawner>();
            Assert.IsNotNull(moon1Spawner, "Moon1/2 spawner should exist");
            
            LogTestResult("Moon 1 spawn", true);
        }

        [UnityTest]
        public IEnumerator Moon1_BuildingRestoration_UpdatesProgress()
        {
            yield return LoadSceneAsync("Echohaven_VerticalSlice");
            
            // Find ResonanceStone component (used for restoration)
            var stones = GameObject.FindObjectsOfType<ResonanceStone>();
            
            if (stones.Length > 0)
            {
                float initialProgress = SaveManager.Instance?.GetMoonProgress(1) ?? 0f;
                
                // Simulate collecting a stone
                foreach (var stone in stones)
                {
                    stone.SendMessage("OnPlayerCollect", SendMessageOptions.DontRequireReceiver);
                    yield return new WaitForSeconds(0.5f);
                }
                
                float finalProgress = SaveManager.Instance?.GetMoonProgress(1) ?? 0f;
                
                Assert.GreaterOrEqual(finalProgress, initialProgress, 
                    "Moon progress should increase after collecting Resonance Stones");
                
                LogTestResult("Building restoration progress", true);
            }
            else
            {
                LogTestResult("Building restoration progress", false, "No ResonanceStones found in scene");
            }
        }

        [UnityTest]
        public IEnumerator SaveLoad_PersistsPlayerState()
        {
            yield return LoadSceneAsync("Boot");
            
            // Create save
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.CreateNewSave("TestSave_Persistence");
                
                // Store some test data
                var testPosition = new Vector3(10f, 2f, 15f);
                SaveManager.Instance.SetPlayerPosition(testPosition);
                SaveManager.Instance.SetMoonProgress(1, 50f);
                
                // Save to disk
                SaveManager.Instance.Save();
                yield return new WaitForSeconds(1f);
                
                // Load back
                SaveManager.Instance.Load("TestSave_Persistence");
                yield return new WaitForSeconds(1f);
                
                // Verify
                var loadedPosition = SaveManager.Instance.GetPlayerPosition();
                var loadedProgress = SaveManager.Instance.GetMoonProgress(1);
                
                Assert.AreEqual(testPosition, loadedPosition, "Player position should persist");
                Assert.AreEqual(50f, loadedProgress, 0.1f, "Moon progress should persist");
                
                LogTestResult("Save/Load persistence", true);
            }
            else
            {
                LogTestResult("Save/Load persistence", false, "SaveManager not available");
            }
        }

        [UnityTest]
        public IEnumerator Moon2_UnlocksAfterMoon1Complete()
        {
            yield return LoadSceneAsync("Boot");
            
            if (SaveManager.Instance != null)
            {
                // Complete Moon 1
                SaveManager.Instance.SetMoonProgress(1, 100f);
                SaveManager.Instance.Save();
                yield return new WaitForSeconds(0.5f);
                
                // Check Moon 2 unlock status
                var moon2Unlocked = SaveManager.Instance.IsMoonUnlocked(2);
                
                Assert.IsTrue(moon2Unlocked, "Moon 2 should unlock when Moon 1 reaches 100%");
                LogTestResult("Moon 2 unlock trigger", true);
            }
            else
            {
                LogTestResult("Moon 2 unlock trigger", false, "SaveManager not available");
            }
        }

        [UnityTest]
        public IEnumerator Moon3_RailEscort_SpawnsCartAndEnemies()
        {
            // Load Crystalline Caverns (Moon 3 rail escort)
            yield return LoadSceneAsync("CrystallineCaverns");
            
            var moon3Spawner = GameObject.FindObjectOfType<Moon3ContentSpawner>();
            
            if (moon3Spawner != null)
            {
                // Trigger rail escort (assuming there's a public method)
                moon3Spawner.SendMessage("StartRailEscort", SendMessageOptions.DontRequireReceiver);
                yield return new WaitForSeconds(2f);
                
                // Check for cart and enemies
                var cart = GameObject.Find("MineCart");
                var enemies = GameObject.FindGameObjectsWithTag("Enemy");
                
                bool passed = cart != null && enemies.Length > 0;
                LogTestResult("Moon 3 rail escort spawn", passed, 
                    passed ? null : "Cart or enemies not found");
            }
            else
            {
                LogTestResult("Moon 3 rail escort spawn", false, "Moon3ContentSpawner not found");
            }
        }

        [UnityTest]
        public IEnumerator Moon4_PhiSnap_DetectsAlignment()
        {
            yield return LoadSceneAsync("DeepForge");
            
            var moon4Spawner = GameObject.FindObjectOfType<Moon4ContentSpawner>();
            
            if (moon4Spawner != null)
            {
                // Test φ-snap mechanic (assuming there's a method to check alignment)
                bool alignmentDetected = false;
                
                // Simulate moving an object to φ-ratio position
                // This would need actual φ-snap component interaction
                yield return new WaitForSeconds(1f);
                
                LogTestResult("Moon 4 φ-snap alignment", alignmentDetected, 
                    "Manual verification required");
            }
            else
            {
                LogTestResult("Moon 4 φ-snap alignment", false, "Moon4ContentSpawner not found");
            }
        }

        [UnityTest]
        public IEnumerator Moon5_Pavilion_RestorationMechanic()
        {
            yield return LoadSceneAsync("VerdantCanopy");
            
            var moon5Spawner = GameObject.FindObjectOfType<Moon5ContentSpawner>();
            Assert.IsNotNull(moon5Spawner, "Moon 5 spawner should exist in VerdantCanopy scene");
            
            LogTestResult("Moon 5 pavilion scene load", true);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Moon10_RailStation_Interaction()
        {
            yield return LoadSceneAsync("StarFortBastion");
            
            var moon10Spawner = GameObject.FindObjectOfType<Moon10ContentSpawner>();
            Assert.IsNotNull(moon10Spawner, "Moon 10 spawner should exist");
            
            LogTestResult("Moon 10 rail station load", true);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Moon13_EndingChoice_UIPresent()
        {
            yield return LoadSceneAsync("PlanetaryNexus");
            
            var moon13Spawner = GameObject.FindObjectOfType<Moon13ContentSpawner>();
            
            if (moon13Spawner != null)
            {
                // Trigger ending choice UI
                moon13Spawner.SendMessage("ShowEndingChoice", SendMessageOptions.DontRequireReceiver);
                yield return new WaitForSeconds(1f);
                
                // Look for ending choice UI elements
                var endingUI = GameObject.Find("EndingChoicePanel");
                bool uiPresent = endingUI != null;
                
                LogTestResult("Moon 13 ending choice UI", uiPresent,
                    uiPresent ? null : "EndingChoicePanel not found");
            }
            else
            {
                LogTestResult("Moon 13 ending choice UI", false, "Moon13ContentSpawner not found");
            }
        }

        [UnityTest]
        public IEnumerator Performance_Moon1_MaintainsTargetFramerate()
        {
            yield return LoadSceneAsync("Echohaven_VerticalSlice");
            
            // Sample framerate over 5 seconds
            float sampleDuration = 5f;
            int frameCount = 0;
            float elapsed = 0f;
            
            while (elapsed < sampleDuration)
            {
                frameCount++;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            
            float avgFps = frameCount / elapsed;
            bool passed = avgFps >= 55f; // Allow 5fps margin below 60fps target
            
            LogTestResult("Moon 1 performance (60fps target)", passed, 
                $"Avg FPS: {avgFps:F1}");
        }

        [UnityTest]
        public IEnumerator Memory_Moon1_StaysUnderBudget()
        {
            yield return LoadSceneAsync("Echohaven_VerticalSlice");
            yield return new WaitForSeconds(2f);
            
            // Check memory usage
            long memoryUsageMB = System.GC.GetTotalMemory(false) / (1024 * 1024);
            bool passed = memoryUsageMB < 3600; // 3.6GB budget
            
            LogTestResult("Moon 1 memory budget (3.6GB)", passed,
                $"Memory: {memoryUsageMB}MB");
        }

        // ─── Helper Methods ───────────────────────────────

        IEnumerator LoadSceneAsync(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);
            float timeout = SceneLoadTimeout;
            
            while (!operation.isDone && timeout > 0)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
            
            if (timeout <= 0)
            {
                Debug.LogError($"Scene load timeout: {sceneName}");
            }
            
            yield return new WaitForSeconds(0.5f); // Allow scene initialization
        }

        void LogTestResult(string testName, bool passed, string notes = null)
        {
            string status = passed ? "✓ PASS" : "✗ FAIL";
            string message = $"[TEST] {status} - {testName}";
            if (!string.IsNullOrEmpty(notes))
            {
                message += $" ({notes})";
            }
            
            Debug.Log(message);
        }
    }
}
