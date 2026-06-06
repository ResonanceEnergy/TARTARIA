using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System;

namespace Tartaria.Editor.QA
{
    /// <summary>
    /// Custom test runner for Moon 1-13 end-to-end testing.
    /// Provides menu items to run automated and manual test suites.
    /// </summary>
    public class MoonTestRunner : EditorWindow
    {
        static readonly string TestReportPath = "Logs/moon-test-report.txt";
        
        Vector2 _scrollPos;
        string _testLog = "";
        
        [MenuItem("Tartaria/QA/Moon Test Runner")]
        static void ShowWindow()
        {
            var window = GetWindow<MoonTestRunner>("Moon Test Runner");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("TARTARIA - Moon 1-13 Test Suite", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This window provides tools for automated and manual testing of Moon progression. " +
                "Tests cover save/load, Moon unlocks, content spawning, and performance.",
                MessageType.Info);

            GUILayout.Space(10);

            // Automated Tests Section
            GUILayout.Label("Automated Tests (PlayMode)", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Run Moon 1-4 Regression Tests", GUILayout.Height(30)))
            {
                RunPlayModeTests("Moon1|Moon2|Moon3|Moon4|SaveLoad");
            }

            if (GUILayout.Button("Run Moon 5-10 Smoke Tests", GUILayout.Height(30)))
            {
                RunPlayModeTests("Moon5|Moon10");
            }

            if (GUILayout.Button("Run Moon 11-13 Finale Tests", GUILayout.Height(30)))
            {
                RunPlayModeTests("Moon13");
            }

            if (GUILayout.Button("Run Performance Tests", GUILayout.Height(30)))
            {
                RunPlayModeTests("Performance|Memory");
            }

            if (GUILayout.Button("Run ALL Tests", GUILayout.Height(40)))
            {
                RunPlayModeTests("");
            }

            GUILayout.Space(10);

            // Manual Test Checklist
            GUILayout.Label("Manual Test Utilities", EditorStyles.boldLabel);

            if (GUILayout.Button("Open Manual Test Checklist", GUILayout.Height(25)))
            {
                OpenManualChecklist();
            }

            if (GUILayout.Button("Generate Test Report", GUILayout.Height(25)))
            {
                GenerateTestReport();
            }

            GUILayout.Space(10);

            // Test Log Display
            GUILayout.Label("Test Log:", EditorStyles.boldLabel);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(200));
            GUILayout.TextArea(_testLog, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            if (GUILayout.Button("Clear Log"))
            {
                _testLog = "";
            }
        }

        void RunPlayModeTests(string filter)
        {
            _testLog += $"\n[{DateTime.Now:HH:mm:ss}] Starting PlayMode tests (filter: {filter})...\n";
            
            // Use Unity Test Runner API
            var testRunnerApi = ScriptableObject.CreateInstance<UnityEditor.TestTools.TestRunner.Api.TestRunnerApi>();
            
            var filterSettings = new UnityEditor.TestTools.TestRunner.Api.Filter
            {
                testMode = UnityEditor.TestTools.TestRunner.Api.TestMode.PlayMode,
                categoryNames = string.IsNullOrEmpty(filter) ? null : new[] { filter }
            };

            testRunnerApi.Execute(new UnityEditor.TestTools.TestRunner.Api.ExecutionSettings(filterSettings));
            
            _testLog += "Test execution started. Check Test Runner window for results.\n";
            EditorUtility.DisplayDialog("Tests Started", 
                "PlayMode tests are running. Open Window > General > Test Runner to see results.",
                "OK");
        }

        void OpenManualChecklist()
        {
            string checklistPath = "Assets/_Project/Docs/MANUAL_TEST_CHECKLIST.md";
            
            if (!File.Exists(checklistPath))
            {
                CreateManualChecklist(checklistPath);
            }
            
            // Open in default editor
            System.Diagnostics.Process.Start(checklistPath);
            _testLog += $"Opened manual test checklist: {checklistPath}\n";
        }

        void CreateManualChecklist(string path)
        {
            var content = @"# TARTARIA - Manual Test Checklist
## Session: {DATE}
## Tester: {NAME}

---

## Moon 1-4 Regression Test (30 min)

### Moon 1: Echohaven Restoration
- [ ] Boot to Main Menu loads without errors
- [ ] New Game creates save and loads Echohaven scene
- [ ] Player spawns at expected position (near main plaza)
- [ ] Star Dome building can be restored
  - [ ] Resonance Stones appear and can be collected
  - [ ] Building restoration progress updates (HUD/Quest)
  - [ ] Audio/VFX play on restoration complete
- [ ] Excavation site interaction works
  - [ ] Dig prompt appears when near site
  - [ ] Shard collected and added to inventory
  - [ ] Inventory UI shows shard
- [ ] Save game function works (Pause Menu > Save)
- [ ] Quit to Main Menu
- [ ] Load saved game
  - [ ] Player position restored correctly
  - [ ] Building restoration state persists
  - [ ] Inventory contents persist (shard present)

**Notes:**
_______________________________________________

### Moon 2: Crystalline Caverns
- [ ] Moon 2 unlocks after completing Moon 1
- [ ] Cathedral scene loads
- [ ] Dissonance crystals visible and interactable
- [ ] Cassian companion NPC appears
- [ ] Shrink mechanic (micro-giant mode) works
- [ ] Bell tower repair interaction available
- [ ] Mud Golems spawn as enemies

**Notes:**
_______________________________________________

### Moon 3: Rail Escort
- [ ] Mine cart spawns on rails
- [ ] Enemies spawn during escort
- [ ] Cart moves along rail for at least 1 minute
- [ ] Player can defend cart from enemies
- [ ] Escort progress tracked (HUD/Quest)

**Notes:**
_______________________________________________

### Moon 4: φ-Snap Alignment
- [ ] Deep Forge scene loads
- [ ] φ-snap mechanic tutorial appears
- [ ] Objects snap to φ-ratio positions
- [ ] Visual/audio feedback on correct alignment
- [ ] At least one forge puzzle completable

**Notes:**
_______________________________________________

---

## Moon 5-10 Smoke Test (30 min)

### Moon 5: Verdant Pavilion
- [ ] Scene loads
- [ ] At least 1 pavilion restoration mechanic works

### Moon 6: Organ Pipes
- [ ] Scene loads
- [ ] At least 1 pipe interaction works (sound/puzzle)

### Moon 7: Ice Thaw
- [ ] Scene loads
- [ ] At least 1 ice patch can be thawed

### Moon 8: Airship
- [ ] Scene loads
- [ ] Airship is interactable

### Moon 9: Golden Codex
- [ ] Scene loads
- [ ] Codex is interactable

### Moon 10: Rail Station
- [ ] Scene loads
- [ ] Rail station interaction works

**Notes:**
_______________________________________________

---

## Moon 11-13 Finale Test (20 min)

### Moon 11: Fountain Healing
- [ ] Scene loads
- [ ] At least 1 fountain provides healing

### Moon 12: Bell Tower Sync
- [ ] Scene loads
- [ ] At least 1 bell tower is interactable

### Moon 13: Echo Gate + Ending
- [ ] Scene loads
- [ ] At least 1 echo gate interaction works
- [ ] Ending choice UI appears
- [ ] All 3 ending branches visible in UI:
  - [ ] Harmony (restore balance)
  - [ ] Domination (control the power)
  - [ ] Transcendence (ascend beyond)
- [ ] Chose Harmony ending
- [ ] Ending cinematic plays without errors

**Notes:**
_______________________________________________

---

## Performance Metrics

### Moon 1 Profiling
- [ ] Unity Profiler run on Moon 1 for 5 minutes
- Average FPS: ______
- Lowest FPS: ______
- Frame spikes > 16.67ms: ______ (count)
- Memory usage: ______ MB
- Meets 60fps target: YES / NO
- Stays under 3.6GB budget: YES / NO

**Notes:**
_______________________________________________

---

## Blocker Issues Found

### Critical (blocks testing)
1. _______________________________________________
2. _______________________________________________

### Major (impacts experience)
1. _______________________________________________
2. _______________________________________________

### Minor (cosmetic/polish)
1. _______________________________________________
2. _______________________________________________

---

## Overall Test Result

- [ ] PASS - All critical tests passed, ready for next phase
- [ ] PASS WITH ISSUES - Core functionality works, minor issues found
- [ ] FAIL - Critical issues block progression

**Summary:**
_______________________________________________
_______________________________________________
_______________________________________________

**Time Spent:** ______ minutes
**Tested By:** __________________
**Date:** __________________
";

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
        }

        void GenerateTestReport()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("TARTARIA - Moon 1-13 Test Report");
            sb.AppendLine($"Generated: {DateTime.Now}");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine();
            
            // Check for PlayMode test results
            sb.AppendLine("AUTOMATED TEST RESULTS:");
            sb.AppendLine("-".PadRight(80, '-'));
            sb.AppendLine("Check Unity Test Runner window for detailed PlayMode test results.");
            sb.AppendLine("Window > General > Test Runner > PlayMode");
            sb.AppendLine();
            
            // Scene inventory
            sb.AppendLine("SCENE INVENTORY:");
            sb.AppendLine("-".PadRight(80, '-'));
            
            string[] moonScenes = {
                "Boot.unity",
                "Echohaven_VerticalSlice.unity (Moon 1)",
                "CrystallineCaverns.unity (Moon 2/3)",
                "DeepForge.unity (Moon 4)",
                "VerdantCanopy.unity (Moon 5)",
                "WindsweptHighlands.unity (Moon 6)",
                "TidalArchive.unity (Moon 7)",
                "SunkenColosseum.unity (Moon 8)",
                "LivingLibrary.unity (Moon 9)",
                "StarFortBastion.unity (Moon 10)",
                "ClockworkCitadel.unity (Moon 11)",
                "AuroralSpire.unity (Moon 12)",
                "PlanetaryNexus.unity (Moon 13)"
            };
            
            foreach (var scene in moonScenes)
            {
                string scenePath = $"Assets/_Project/Scenes/{scene}";
                bool exists = File.Exists(scenePath) || File.Exists($"Assets/_Project/Scenes/Moons/{scene}");
                sb.AppendLine($"  {(exists ? "✓" : "✗")} {scene}");
            }
            sb.AppendLine();
            
            // Content Spawner inventory
            sb.AppendLine("CONTENT SPAWNER INVENTORY:");
            sb.AppendLine("-".PadRight(80, '-'));
            
            for (int i = 2; i <= 13; i++)
            {
                string spawnerPath = $"Assets/_Project/Scripts/Integration/Moon{i}ContentSpawner.cs";
                bool exists = File.Exists(spawnerPath);
                sb.AppendLine($"  {(exists ? "✓" : "✗")} Moon{i}ContentSpawner.cs");
            }
            sb.AppendLine();
            
            // Performance requirements
            sb.AppendLine("PERFORMANCE REQUIREMENTS:");
            sb.AppendLine("-".PadRight(80, '-'));
            sb.AppendLine("  Target: 60 FPS (16.67ms per frame)");
            sb.AppendLine("  Memory Budget: 3.6 GB");
            sb.AppendLine("  Run Unity Profiler for detailed metrics.");
            sb.AppendLine();
            
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("END OF REPORT");
            sb.AppendLine("=".PadRight(80, '='));
            
            // Write to file
            Directory.CreateDirectory("Logs");
            File.WriteAllText(TestReportPath, sb.ToString());
            
            _testLog += $"\nTest report generated: {TestReportPath}\n";
            _testLog += sb.ToString();
            
            EditorUtility.DisplayDialog("Report Generated", 
                $"Test report saved to:\n{TestReportPath}", 
                "OK");
        }
    }
}
