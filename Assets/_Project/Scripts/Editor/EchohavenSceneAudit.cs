using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Tartaria.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor-mode (not Play-mode) audit of Echohaven_VerticalSlice.unity.
    /// Runs WITHOUT entering Play mode so blockers can be detected and fixed before pressing Play.
    /// Menu: Tartaria → Scene Audit: Echohaven
    /// Batchmode entry: Tartaria.Editor.EchohavenSceneAudit.AuditFromBatchmode
    ///
    /// Diagnoses (per STATUS.md § 2 blocker list):
    ///   1. Echohaven scene exists and opens
    ///   2. PlayerSpawner GameObject present with assigned playerPrefab
    ///   3. NavMesh bake status (any walkable surfaces baked?)
    ///   4. 3 Moon 1 buildings present (StarDome, HarmonicFountain, CrystalSpire)
    ///   5. EchohavenContentSpawner has real prefab refs (not primitive-capsule fallbacks)
    ///   6. Missing-script references (broken Component refs after refactors)
    ///   7. MainCamera in scene
    ///   8. Directional Light in scene
    /// </summary>
    public static class EchohavenSceneAudit
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        private const string REPORT_PATH_RELATIVE = "Logs/echohaven_audit_report.txt";

        // Counters shared across run
        private static int _passCount;
        private static int _failCount;
        private static int _warnCount;
        private static StringBuilder _report;

        [MenuItem("Tartaria/6 Scene Tools/Audit Echohaven", priority = 600)]
        public static void AuditFromMenu()
        {
            RunAudit(writeReportFile: true, openSceneIfClosed: true);
            if (_failCount > 0)
            {
                EditorUtility.DisplayDialog(
                    "Echohaven Scene Audit",
                    $"Audit FAILED: {_failCount} blocker(s), {_warnCount} warning(s), {_passCount} check(s) passed.\n\nSee Unity Console for full report. Detailed log at:\n{REPORT_PATH_RELATIVE}",
                    "OK");
            }
            else if (_warnCount > 0)
            {
                EditorUtility.DisplayDialog(
                    "Echohaven Scene Audit",
                    $"Audit PASSED with warnings: {_warnCount} warning(s), {_passCount} check(s) passed.\n\nScene is playable but has gaps. See Console.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Echohaven Scene Audit",
                    $"Audit PASSED ALL CHECKS: {_passCount} pass(es).\n\nEchohaven is ready for Play Mode.",
                    "OK");
            }
        }

        /// <summary>
        /// Batchmode entry — usable via Unity -executeMethod.
        /// Writes report to Logs/echohaven_audit_report.txt, exits Unity with code 0 (pass+warn) or 1 (fail).
        /// </summary>
        public static void AuditFromBatchmode()
        {
            RunAudit(writeReportFile: true, openSceneIfClosed: true);
            int exitCode = _failCount > 0 ? 1 : 0;
            Debug.Log($"[EchohavenSceneAudit] Batchmode complete. Exit code {exitCode} (pass={_passCount}, warn={_warnCount}, fail={_failCount})");
            EditorApplication.Exit(exitCode);
        }

        private static void RunAudit(bool writeReportFile, bool openSceneIfClosed)
        {
            _passCount = 0;
            _failCount = 0;
            _warnCount = 0;
            _report = new StringBuilder();

            WriteLine("");
            WriteLine("═══════════════════════════════════════════════════════════════");
            WriteLine("   ECHOHAVEN_VERTICAL_SLICE.UNITY — SCENE AUDIT");
            WriteLine($"   {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            WriteLine("═══════════════════════════════════════════════════════════════");
            WriteLine("");

            // CHECK 1: Scene file exists on disk
            if (!File.Exists(SCENE_PATH))
            {
                Fail("SCENE FILE", $"Scene file not found at {SCENE_PATH}");
                FinalizeReport(writeReportFile);
                return;
            }
            Pass("SCENE FILE", $"Found at {SCENE_PATH}");

            // CHECK 2: Open scene (or verify it's loaded)
            // Play Mode locks scene I/O — fall back to inspecting the currently
            // active scene in that case rather than failing the audit outright.
            Scene scene;
            try
            {
                bool inPlayMode = EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying;
                var active = EditorSceneManager.GetActiveScene();
                bool activeIsTarget = active.path == SCENE_PATH;

                if (inPlayMode)
                {
                    if (!activeIsTarget)
                    {
                        Fail("SCENE LOAD", $"Play Mode is active and the loaded scene ('{active.name}') is not Echohaven. Stop Play Mode and run the audit again.");
                        FinalizeReport(writeReportFile);
                        return;
                    }
                    scene = active;
                    Pass("SCENE LOAD", $"Inspecting live Play-mode scene '{scene.name}' ({scene.rootCount} root GameObjects)");
                }
                else if (openSceneIfClosed || !activeIsTarget)
                {
                    scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
                    Pass("SCENE LOAD", $"Loaded '{scene.name}' ({scene.rootCount} root GameObjects)");
                }
                else
                {
                    scene = active;
                    Pass("SCENE LOAD", $"Using already-open scene '{scene.name}' ({scene.rootCount} root GameObjects)");
                }
            }
            catch (Exception ex)
            {
                Fail("SCENE LOAD", $"Could not open scene: {ex.Message}");
                FinalizeReport(writeReportFile);
                return;
            }

            WriteLine("");
            WriteLine("--- CRITICAL BLOCKERS (must pass to enter Play Mode) ---");

            // CHECK 3: PlayerSpawner GameObject
            var playerSpawner = UnityEngine.Object.FindFirstObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
            if (playerSpawner == null)
            {
                Fail("PLAYER SPAWNER",
                    "No PlayerSpawner found in scene.\n" +
                    "  FIX: Create empty GameObject 'PlayerSpawner' at (0,1,0), add PlayerSpawner component,\n" +
                    "       assign Assets/_Project/Prefabs/Characters/Player.prefab to playerPrefab field.");
            }
            else
            {
                var so = new SerializedObject(playerSpawner);
                var prefabProp = so.FindProperty("playerPrefab");
                if (prefabProp == null || prefabProp.objectReferenceValue == null)
                {
                    Fail("PLAYER SPAWNER", "PlayerSpawner exists but playerPrefab is null. " +
                        "Assign Assets/_Project/Prefabs/Characters/Player.prefab in Inspector.");
                }
                else
                {
                    Pass("PLAYER SPAWNER", $"Present at {playerSpawner.transform.position}, prefab assigned: {prefabProp.objectReferenceValue.name}");
                }
            }

            // CHECK 4: NavMesh bake status
            var navData = NavMesh.CalculateTriangulation();
            if (navData.vertices == null || navData.vertices.Length == 0)
            {
                Fail("NAVMESH",
                    "No NavMesh baked. NPCs (Milo, MudGolem) will be frozen.\n" +
                    "  FIX: Window → AI → Navigation → Bake tab → mark terrain/floor as Walkable → click Bake.");
            }
            else
            {
                Pass("NAVMESH", $"Baked. {navData.vertices.Length} vertices, {navData.indices.Length / 3} triangles.");
            }

            // CHECK 5: MainCamera
            var mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                Fail("MAIN CAMERA", "No Camera tagged MainCamera in scene. Player will see nothing.");
            }
            else
            {
                Pass("MAIN CAMERA", $"Found: {mainCam.name} at {mainCam.transform.position}");
            }

            // CHECK 6: Directional Light
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var directional = lights.FirstOrDefault(l => l.type == LightType.Directional);
            if (directional == null)
            {
                Fail("DIRECTIONAL LIGHT", "No Directional Light. Scene will render unlit.");
            }
            else
            {
                Pass("DIRECTIONAL LIGHT", $"Found: {directional.name}, intensity {directional.intensity:F2}");
            }

            WriteLine("");
            WriteLine("--- MOON 1 CONTENT (3 buildings + spawner) ---");

            // CHECK 7: 3 Moon 1 buildings
            var domeNames = new[] { "Echohaven_StarDome", "StarDome", "Building_dome", "Dome" };
            var fountainNames = new[] { "Echohaven_HarmonicFountain", "HarmonicFountain", "Building_fountain", "Fountain" };
            var spireNames = new[] { "Echohaven_CrystalSpire", "CrystalSpire", "Building_spire", "Spire" };

            CheckBuilding("STARDOME", domeNames);
            CheckBuilding("HARMONIC FOUNTAIN", fountainNames);
            CheckBuilding("CRYSTAL SPIRE", spireNames);

            // CHECK 8: EchohavenContentSpawner
            var contentSpawner = UnityEngine.Object.FindFirstObjectByType<EchohavenContentSpawner>(FindObjectsInactive.Include);
            if (contentSpawner == null)
            {
                Warn("CONTENT SPAWNER", "No EchohavenContentSpawner in scene. Milo/Anastasia/MudGolem will not be auto-spawned.");
            }
            else
            {
                Pass("CONTENT SPAWNER", $"Present at {contentSpawner.transform.position}");
                CheckSpawnerPrefabRefs(contentSpawner);
            }

            WriteLine("");
            WriteLine("--- INTEGRITY CHECKS (warnings, not blockers) ---");

            // CHECK 9: Missing-script references
            int missingScriptCount = CountMissingScripts(scene);
            if (missingScriptCount > 0)
            {
                Warn("MISSING SCRIPTS", $"{missingScriptCount} Component(s) reference deleted scripts. " +
                    "Use Tools → Tartaria → Clean Missing Scripts or accept Unity scene recovery on next open.");
            }
            else
            {
                Pass("MISSING SCRIPTS", "Zero missing script references.");
            }

            // CHECK 10: Root GameObject count (sanity: scene shouldn't be near-empty or 1000+ objects)
            if (scene.rootCount < 3)
            {
                Warn("SCENE POPULATED", $"Only {scene.rootCount} root GameObject(s). Scene is suspiciously empty.");
            }
            else if (scene.rootCount > 500)
            {
                Warn("SCENE POPULATED", $"{scene.rootCount} root GameObjects. Possibly stub-spawner pollution.");
            }
            else
            {
                Pass("SCENE POPULATED", $"{scene.rootCount} root GameObjects (reasonable).");
            }

            FinalizeReport(writeReportFile);
        }

        private static void CheckBuilding(string label, string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                var go = GameObject.Find(name);
                if (go != null)
                {
                    Pass(label, $"Found as '{go.name}' at {go.transform.position}");
                    return;
                }
            }
            Warn(label, $"Not found. Tried names: {string.Join(", ", candidateNames)}");
        }

        private static void CheckSpawnerPrefabRefs(EchohavenContentSpawner spawner)
        {
            // Reflect-inspect serialized fields ending in "Prefab" for null assignments
            var so = new SerializedObject(spawner);
            var prop = so.GetIterator();
            int nullPrefabFields = 0;
            int totalPrefabFields = 0;
            List<string> nullFieldNames = new List<string>();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                        prop.name.EndsWith("Prefab", StringComparison.OrdinalIgnoreCase))
                    {
                        totalPrefabFields++;
                        if (prop.objectReferenceValue == null)
                        {
                            nullPrefabFields++;
                            nullFieldNames.Add(prop.name);
                        }
                    }
                } while (prop.NextVisible(false));
            }
            if (totalPrefabFields == 0)
            {
                Warn("SPAWNER PREFAB FIELDS", "EchohavenContentSpawner has no *Prefab SerializedFields (or this is a code-only spawner).");
            }
            else if (nullPrefabFields == totalPrefabFields)
            {
                Fail("SPAWNER PREFAB FIELDS",
                    $"All {totalPrefabFields} *Prefab fields are null. Spawner will fall back to primitive capsules.\n" +
                    "  FIX: Drag Player/Milo/Anastasia/MudGolem prefabs from Assets/_Project/Prefabs/Characters/ to Inspector.");
            }
            else if (nullPrefabFields > 0)
            {
                Warn("SPAWNER PREFAB FIELDS",
                    $"{nullPrefabFields}/{totalPrefabFields} *Prefab fields are null: {string.Join(", ", nullFieldNames)}");
            }
            else
            {
                Pass("SPAWNER PREFAB FIELDS", $"All {totalPrefabFields} *Prefab fields assigned.");
            }
        }

        private static int CountMissingScripts(Scene scene)
        {
            int count = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                count += CountMissingInHierarchy(root);
            }
            return count;
        }

        private static int CountMissingInHierarchy(GameObject go)
        {
            int count = 0;
            var components = go.GetComponents<Component>();
            foreach (var c in components)
            {
                if (c == null) count++; // null Component slot = missing script
            }
            foreach (Transform child in go.transform)
            {
                count += CountMissingInHierarchy(child.gameObject);
            }
            return count;
        }

        private static void Pass(string label, string detail)
        {
            _passCount++;
            WriteLine($"  [PASS] {label}: {detail}");
        }

        private static void Warn(string label, string detail)
        {
            _warnCount++;
            WriteLine($"  [WARN] {label}: {detail}");
        }

        private static void Fail(string label, string detail)
        {
            _failCount++;
            WriteLine($"  [FAIL] {label}: {detail}");
        }

        private static void WriteLine(string line)
        {
            _report.AppendLine(line);
            Debug.Log(line);
        }

        private static void FinalizeReport(bool writeReportFile)
        {
            WriteLine("");
            WriteLine("═══════════════════════════════════════════════════════════════");
            WriteLine($"  AUDIT SUMMARY  PASS={_passCount}  WARN={_warnCount}  FAIL={_failCount}");
            if (_failCount > 0)
                WriteLine("  → BLOCKER(S) PRESENT. Cannot Play Mode the scene successfully.");
            else if (_warnCount > 0)
                WriteLine("  → PLAYABLE WITH WARNINGS. Review WARN entries.");
            else
                WriteLine("  → CLEAN. Ready for Play Mode.");
            WriteLine("═══════════════════════════════════════════════════════════════");

            if (writeReportFile)
            {
                try
                {
                    System.IO.Directory.CreateDirectory("Logs");
                    System.IO.File.WriteAllText("Logs/echohaven_audit_report.txt", _report.ToString());
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[EchohavenSceneAudit] Could not write report file: " + ex.Message);
                }
            }
        }
    }
}
