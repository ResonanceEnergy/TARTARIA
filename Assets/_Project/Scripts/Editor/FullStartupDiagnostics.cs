using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// COMPREHENSIVE STARTUP DIAGNOSTICS — Checks EVERYTHING:
    /// - Game state progression (Boot → Loading → Exploration)
    /// - Player spawning and components
    /// - Camera setup and target tracking
    /// - NPC spawning and AI setup
    /// - Building spawning
    /// - NavMesh baking status
    /// - Input system readiness
    ///
    /// Run this WHILE IN PLAY MODE via menu: Tartaria → DIAGNOSE: Full Startup Audit
    /// </summary>
    public static class FullStartupDiagnostics
    {
        [UnityEditor.MenuItem("Tartaria/DIAGNOSE: Full Startup Audit")]
        static void DiagnoseFullStartup()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[StartupDiag] Must be in Play mode!");
                UnityEditor.EditorUtility.DisplayDialog("Startup Diagnostics",
                    "Enter Play mode first, then run this diagnostic.", "OK");
                return;
            }

            var report = new System.Text.StringBuilder();
            report.AppendLine("\n╔══════════════════════════════════════════════════════╗");
            report.AppendLine("║       FULL STARTUP DIAGNOSTICS                      ║");
            report.AppendLine("╚══════════════════════════════════════════════════════╝\n");

            int criticalIssues = 0;
            int warnings = 0;

            // ═══════════════════════════════════════════════════
            // 1. GAME STATE
            // ═══════════════════════════════════════════════════
            report.AppendLine("─────────────────────────────────────────────────────");
            report.AppendLine("[1] GAME STATE");
            report.AppendLine("─────────────────────────────────────────────────────");

            var gsm = GameStateManager.Instance;
            if (gsm == null)
            {
                report.AppendLine("  [CRITICAL] GameStateManager.Instance is NULL!");
                criticalIssues++;
            }
            else
            {
                var state = gsm.CurrentState;
                var prevState = gsm.PreviousState;
                var isPlaying = gsm.IsPlaying;

                report.AppendLine($"  Current State: {state}");
                report.AppendLine($"  Previous State: {prevState}");
                report.AppendLine($"  IsPlaying: {isPlaying}");

                if (state != GameState.Exploration && state != GameState.Combat && state != GameState.Tuning)
                {
                    report.AppendLine($"  [CRITICAL] State is '{state}' — should be Exploration!");
                    report.AppendLine("             This blocks ALL input in PlayerInputHandler!");
                    report.AppendLine("             Run: Tartaria → FIX: Force Exploration State");
                    criticalIssues++;
                }
                else
                {
                    report.AppendLine("  [OK] State allows gameplay");
                }
            }

            // ═══════════════════════════════════════════════════
            // 2. PLAYER
            // ═══════════════════════════════════════════════════
            report.AppendLine("\n─────────────────────────────────────────────────────");
            report.AppendLine("[2] PLAYER");
            report.AppendLine("─────────────────────────────────────────────────────");

            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                report.AppendLine("  [CRITICAL] Player NOT FOUND (tag='Player')");
                report.AppendLine("             PlayerSpawner may have failed!");
                criticalIssues++;
            }
            else
            {
                report.AppendLine($"  GameObject: {player.name}");
                report.AppendLine($"  Position: {player.transform.position}");
                report.AppendLine($"  Active: {player.activeInHierarchy}");

                // Check components
                var handler = player.GetComponent<PlayerInputHandler>();
                var controller = player.GetComponent<CharacterController>();

                if (handler == null)
                {
                    report.AppendLine("  [CRITICAL] PlayerInputHandler NOT FOUND!");
                    criticalIssues++;
                }
                else
                {
                    report.AppendLine($"  PlayerInputHandler: {(handler.enabled ? "ENABLED" : "DISABLED")}");
                    if (!handler.enabled)
                    {
                        report.AppendLine("  [CRITICAL] Handler is DISABLED!");
                        criticalIssues++;
                    }
                }

                if (controller == null)
                {
                    report.AppendLine("  [CRITICAL] CharacterController NOT FOUND!");
                    criticalIssues++;
                }
                else
                {
                    report.AppendLine($"  CharacterController: {(controller.enabled ? "ENABLED" : "DISABLED")}");
                    if (!controller.enabled)
                    {
                        report.AppendLine("  [WARNING] CharacterController is DISABLED!");
                        warnings++;
                    }
                }

                report.AppendLine("  [OK] Player exists");
            }

            // ═══════════════════════════════════════════════════
            // 3. CAMERA
            // ═══════════════════════════════════════════════════
            report.AppendLine("\n─────────────────────────────────────────────────────");
            report.AppendLine("[3] CAMERA");
            report.AppendLine("─────────────────────────────────────────────────────");

            var mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                report.AppendLine("  [CRITICAL] Camera.main is NULL!");
                criticalIssues++;
            }
            else
            {
                report.AppendLine($"  GameObject: {mainCam.name}");
                report.AppendLine($"  Position: {mainCam.transform.position}");
                report.AppendLine($"  Forward: {mainCam.transform.forward}");

                // Find CameraController using type name (avoid assembly reference issues)
                MonoBehaviour camController = null;
                foreach (var comp in mainCam.GetComponents<MonoBehaviour>())
                {
                    if (comp != null && comp.GetType().Name == "CameraController")
                    {
                        camController = comp;
                        break;
                    }
                }

                if (camController == null && mainCam.transform.parent != null)
                {
                    foreach (var comp in mainCam.transform.parent.GetComponents<MonoBehaviour>())
                    {
                        if (comp != null && comp.GetType().Name == "CameraController")
                        {
                            camController = comp;
                            break;
                        }
                    }
                }

                if (camController == null)
                {
                    report.AppendLine("  [WARNING] CameraController NOT FOUND!");
                    report.AppendLine("            Camera may not follow player!");
                    warnings++;
                }
                else
                {
                    report.AppendLine($"  CameraController: FOUND on {camController.name}");

                    // Check if it has followTarget using reflection
                    var followTargetField = camController.GetType().GetField("followTarget",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (followTargetField != null)
                    {
                        var followTarget = followTargetField.GetValue(camController) as Transform;
                        if (followTarget == null)
                        {
                            report.AppendLine("  [WARNING] followTarget is NULL!");
                            report.AppendLine("            Camera searching for Player tag...");
                            warnings++;
                        }
                        else
                        {
                            report.AppendLine($"  followTarget: {followTarget.name} at {followTarget.position}");
                            report.AppendLine("  [OK] Camera has target");
                        }
                    }
                }

                // Check camera distance
                if (player != null)
                {
                    float dist = Vector3.Distance(mainCam.transform.position, player.transform.position);
                    report.AppendLine($"  Distance to player: {dist:F1}m");
                    if (dist > 20f)
                    {
                        report.AppendLine("  [WARNING] Camera is VERY FAR from player!");
                        report.AppendLine("            Expected: 6-12m, Actual: " + dist.ToString("F1") + "m");
                        warnings++;
                    }
                    else if (dist < 2f)
                    {
                        report.AppendLine("  [WARNING] Camera is TOO CLOSE to player!");
                        warnings++;
                    }
                }
            }

            // ═══════════════════════════════════════════════════
            // 4. SPAWNERS
            // ═══════════════════════════════════════════════════
            report.AppendLine("\n─────────────────────────────────────────────────────");
            report.AppendLine("[4] SPAWNERS");
            report.AppendLine("─────────────────────────────────────────────────────");

            var buildingSpawner = Object.FindFirstObjectByType<BuildingSpawner>();
            if (buildingSpawner == null)
            {
                report.AppendLine("  [WARNING] BuildingSpawner NOT FOUND");
                report.AppendLine("            RuntimeSpawnerInsurance should have created it");
                warnings++;
            }
            else
            {
                report.AppendLine($"  BuildingSpawner: FOUND on {buildingSpawner.name}");
            }

            var contentSpawner = Object.FindFirstObjectByType<EchohavenContentSpawner>();
            if (contentSpawner == null)
            {
                report.AppendLine("  [WARNING] EchohavenContentSpawner NOT FOUND");
                warnings++;
            }
            else
            {
                report.AppendLine($"  EchohavenContentSpawner: FOUND on {contentSpawner.name}");
            }

            var playerSpawner = Object.FindFirstObjectByType<PlayerSpawner>();
            if (playerSpawner == null)
            {
                report.AppendLine("  [WARNING] PlayerSpawner NOT FOUND");
                warnings++;
            }
            else
            {
                report.AppendLine($"  PlayerSpawner: FOUND on {playerSpawner.name}");
            }

            // ═══════════════════════════════════════════════════
            // 5. BUILDINGS
            // ═══════════════════════════════════════════════════
            report.AppendLine("\n─────────────────────────────────────────────────────");
            report.AppendLine("[5] BUILDINGS");
            report.AppendLine("─────────────────────────────────────────────────────");

            var buildings = Object.FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            report.AppendLine($"  Found {buildings.Length} InteractableBuilding components");

            if (buildings.Length == 0)
            {
                report.AppendLine("  [WARNING] NO buildings found!");
                report.AppendLine("            BuildingSpawner.Start() may not have run");
                warnings++;
            }
            else
            {
                foreach (var b in buildings)
                {
                    report.AppendLine($"    - {b.name} at {b.transform.position}");
                }
            }

            // ═══════════════════════════════════════════════════
            // 6. NPCs
            // ═══════════════════════════════════════════════════
            report.AppendLine("\n─────────────────────────────────────────────────────");
            report.AppendLine("[6] NPCs & AI");
            report.AppendLine("─────────────────────────────────────────────────────");

            // Find all NPCs (by common names)
            var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var npcs = new System.Collections.Generic.List<GameObject>();
            foreach (var obj in allObjects)
            {
                var name = obj.name.ToLower();
                if (name.Contains("milo") || name.Contains("cassian") || name.Contains("lirael") ||
                    name.Contains("kaykit") || name.Contains("character") || name.Contains("npc"))
                {
                    npcs.Add(obj);
                }
            }

            report.AppendLine($"  Found {npcs.Count} potential NPC objects");

            if (npcs.Count == 0)
            {
                report.AppendLine("  [WARNING] NO NPCs found!");
                report.AppendLine("            EchohavenContentSpawner may not have run");
                warnings++;
            }
            else
            {
                int withAI = 0;
                int withNavMesh = 0;
                int withAnimator = 0;

                foreach (var npc in npcs)
                {
                    // Find components by type name to avoid assembly reference issues
                    MonoBehaviour ai = null;
                    foreach (var comp in npc.GetComponents<MonoBehaviour>())
                    {
                        if (comp != null && comp.GetType().Name == "NPCAIBehavior")
                        {
                            ai = comp;
                            break;
                        }
                    }

                    var nav = npc.GetComponent<NavMeshAgent>();
                    var anim = npc.GetComponent<Animator>();

                    if (ai != null) withAI++;
                    if (nav != null) withNavMesh++;
                    if (anim != null) withAnimator++;

                    report.AppendLine($"    - {npc.name}:");
                    report.AppendLine($"        Position: {npc.transform.position}");
                    report.AppendLine($"        NPCAIBehavior: {(ai != null ? "YES" : "MISSING")}");
                    report.AppendLine($"        NavMeshAgent: {(nav != null ? "YES" : "MISSING")}");
                    report.AppendLine($"        Animator: {(anim != null ? "YES" : "MISSING")}");

                    if (nav != null)
                    {
                        report.AppendLine($"        NavMesh enabled: {nav.enabled}");
                        report.AppendLine($"        NavMesh speed: {nav.speed}");
                        report.AppendLine($"        NavMesh on mesh: {nav.isOnNavMesh}");

                        if (!nav.isOnNavMesh)
                        {
                            report.AppendLine("        [WARNING] Agent NOT on NavMesh!");
                            warnings++;
                        }
                    }
                }

                report.AppendLine($"\n  Summary:");
                report.AppendLine($"    NPCs with AI: {withAI}/{npcs.Count}");
                report.AppendLine($"    NPCs with NavMesh: {withNavMesh}/{npcs.Count}");
                report.AppendLine($"    NPCs with Animator: {withAnimator}/{npcs.Count}");

                if (withAI < npcs.Count)
                {
                    report.AppendLine($"  [WARNING] {npcs.Count - withAI} NPCs missing NPCAIBehavior!");
                    report.AppendLine("            EnableNPCAI() may not have run");
                    warnings++;
                }

                if (withNavMesh < npcs.Count)
                {
                    report.AppendLine($"  [WARNING] {npcs.Count - withNavMesh} NPCs missing NavMeshAgent!");
                    warnings++;
                }
            }

            // ═══════════════════════════════════════════════════
            // 7. NAVMESH
            // ═══════════════════════════════════════════════════
            report.AppendLine("\n─────────────────────────────────────────────────────");
            report.AppendLine("[7] NAVMESH");
            report.AppendLine("─────────────────────────────────────────────────────");

            // Find NavMeshSurface by type name (avoid package dependency issues)
            MonoBehaviour navMeshSurface = null;
            var allComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var comp in allComponents)
            {
                if (comp != null && comp.GetType().Name == "NavMeshSurface")
                {
                    navMeshSurface = comp;
                    break;
                }
            }

            if (navMeshSurface == null)
            {
                report.AppendLine("  [CRITICAL] NavMeshSurface NOT FOUND in scene!");
                report.AppendLine("             NPCs cannot navigate without baked NavMesh!");
                report.AppendLine("             Fix: Window → AI → Navigation → Bake");
                criticalIssues++;
            }
            else
            {
                report.AppendLine($"  NavMeshSurface: FOUND on {navMeshSurface.name}");

                // Check if NavMesh has actual data
                var triangulation = NavMesh.CalculateTriangulation();
                if (triangulation.vertices.Length == 0)
                {
                    report.AppendLine("  [CRITICAL] NavMesh has NO BAKED DATA!");
                    report.AppendLine("             Window → AI → Navigation → Bake");
                    criticalIssues++;
                }
                else
                {
                    report.AppendLine($"  NavMesh vertices: {triangulation.vertices.Length}");
                    report.AppendLine($"  NavMesh triangles: {triangulation.indices.Length / 3}");
                    report.AppendLine("  [OK] NavMesh is baked");
                }
            }

            // ═══════════════════════════════════════════════════
            // 8. INPUT
            // ═══════════════════════════════════════════════════
            report.AppendLine("\n─────────────────────────────────────────────────────");
            report.AppendLine("[8] INPUT");
            report.AppendLine("─────────────────────────────────────────────────────");

            var gamepad = Gamepad.current;
            report.AppendLine($"  Gamepad: {(gamepad != null ? gamepad.name : "NOT DETECTED")}");

            if (gamepad != null)
            {
                var leftStick = gamepad.leftStick.ReadValue();
                report.AppendLine($"  Left stick: ({leftStick.x:F3}, {leftStick.y:F3})");

                if (leftStick.sqrMagnitude > 0.01f)
                {
                    report.AppendLine("  [INFO] Left stick currently being moved");
                }
            }

            // ═══════════════════════════════════════════════════
            // SUMMARY
            // ═══════════════════════════════════════════════════
            report.AppendLine("\n╔══════════════════════════════════════════════════════╗");
            report.AppendLine("║       SUMMARY                                         ║");
            report.AppendLine("╚══════════════════════════════════════════════════════╝\n");

            report.AppendLine($"  CRITICAL Issues: {criticalIssues}");
            report.AppendLine($"  Warnings: {warnings}");

            if (criticalIssues == 0 && warnings == 0)
            {
                report.AppendLine("\n  [OK] All systems operational!");
            }
            else
            {
                report.AppendLine("\n  TOP FIXES:");

                if (gsm != null && gsm.CurrentState != GameState.Exploration &&
                    gsm.CurrentState != GameState.Combat && gsm.CurrentState != GameState.Tuning)
                {
                    report.AppendLine("    1. Fix game state: Tartaria → FIX: Force Exploration State");
                }

                if (NavMesh.CalculateTriangulation().vertices.Length == 0)
                {
                    report.AppendLine("    2. Bake NavMesh: Window → AI → Navigation → Bake");
                }

                if (mainCam != null && player != null)
                {
                    float dist = Vector3.Distance(mainCam.transform.position, player.transform.position);
                    if (dist > 20f)
                    {
                        report.AppendLine("    3. Camera too far - check CameraController followTarget");
                    }
                }

                if (player == null)
                {
                    report.AppendLine("    4. Player not spawned - check PlayerSpawner logs");
                }
            }

            report.AppendLine("\n╚══════════════════════════════════════════════════════╝\n");

            string fullReport = report.ToString();
            Debug.Log(fullReport);

            // Show dialog
            string dialogMsg = criticalIssues == 0 && warnings == 0
                ? "All systems operational! Check Console for full details."
                : $"Found {criticalIssues} critical issue(s) and {warnings} warning(s).\n\nCheck Console for full report.";

            UnityEditor.EditorUtility.DisplayDialog("Startup Diagnostics", dialogMsg, "OK");
        }
    }
}
