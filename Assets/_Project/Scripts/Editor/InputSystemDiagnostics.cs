using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Runtime Input System Diagnostics — checks EVERYTHING that could block gamepad movement:
    /// - GameState (must be Exploration/Tuning/Combat)
    /// - InputSystem device detection
    /// - PlayerInputHandler existence + enabled state
    /// - CharacterController existence
    /// - Move action binding
    /// - Gamepad stick values
    /// - Any blocking UI/dialogue
    ///
    /// Run this in Play mode via menu: Tartaria → DIAGNOSE: Input System
    /// </summary>
    public static class InputSystemDiagnostics
    {
        [UnityEditor.MenuItem("Tartaria/7 Diagnose/Input System", priority = 720)]
        static void DiagnoseInputSystem()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[InputDiagnostics] Must be in Play mode!");
                UnityEditor.EditorUtility.DisplayDialog("Input Diagnostics",
                    "Enter Play mode first, then run this diagnostic.", "OK");
                return;
            }

            var report = new System.Text.StringBuilder();
            report.AppendLine("\n========== INPUT SYSTEM DIAGNOSTICS ==========\n");

            int issueCount = 0;

            // 1. GameState check
            report.AppendLine("[1] GAME STATE CHECK");
            var gsm = GameStateManager.Instance;
            if (gsm == null)
            {
                report.AppendLine("  [FAIL] GameStateManager.Instance is NULL");
                issueCount++;
            }
            else
            {
                var state = gsm.CurrentState;
                bool isPlaying = gsm.IsPlaying;
                report.AppendLine($"  Current State: {state}");
                report.AppendLine($"  IsPlaying: {isPlaying}");

                if (!isPlaying)
                {
                    report.AppendLine($"  [FAIL] State must be Exploration/Tuning/Combat for input to work!");
                    report.AppendLine($"         Current state '{state}' blocks ALL input in PlayerInputHandler.Update()");
                    issueCount++;
                }
                else
                {
                    report.AppendLine("  [OK] State allows input");
                }
            }

            // 2. Input devices
            report.AppendLine("\n[2] INPUT DEVICE DETECTION");
            var keyboard = Keyboard.current;
            var gamepad = Gamepad.current;
            var mouse = Mouse.current;

            report.AppendLine($"  Keyboard: {(keyboard != null ? "DETECTED" : "NOT FOUND")}");
            report.AppendLine($"  Mouse: {(mouse != null ? "DETECTED" : "NOT FOUND")}");
            report.AppendLine($"  Gamepad: {(gamepad != null ? gamepad.name : "NOT FOUND")}");

            if (gamepad != null)
            {
                var leftStick = gamepad.leftStick.ReadValue();
                var rightStick = gamepad.rightStick.ReadValue();
                report.AppendLine($"  Left Stick: ({leftStick.x:F3}, {leftStick.y:F3})");
                report.AppendLine($"  Right Stick: ({rightStick.x:F3}, {rightStick.y:F3})");
                report.AppendLine($"  Buttons: A={gamepad.buttonSouth.isPressed} B={gamepad.buttonEast.isPressed} X={gamepad.buttonWest.isPressed} Y={gamepad.buttonNorth.isPressed}");

                if (gamepad.name.Contains("360") || gamepad.name.Contains("Xbox"))
                {
                    report.AppendLine("  [OK] Xbox controller detected (XInput mode)");
                }
                else if (gamepad.name.Contains("Logitech"))
                {
                    report.AppendLine("  [WARNING] Logitech detected - ensure F310 switch is in X position!");
                }
            }
            else
            {
                report.AppendLine("  [FAIL] NO GAMEPAD DETECTED");
                report.AppendLine("         - Unplug + replug USB");
                report.AppendLine("         - For F310: flip switch to X (XInput mode)");
                report.AppendLine("         - Check Windows: Win+R → joy.cpl");
                issueCount++;
            }

            // 3. PlayerInputHandler
            report.AppendLine("\n[3] PLAYER INPUT HANDLER");
            var handler = Object.FindFirstObjectByType<PlayerInputHandler>();
            if (handler == null)
            {
                report.AppendLine("  [FAIL] PlayerInputHandler NOT FOUND in scene");
                issueCount++;
            }
            else
            {
                report.AppendLine($"  Found: {handler.name}");
                report.AppendLine($"  Enabled: {handler.enabled}");
                report.AppendLine($"  GameObject Active: {handler.gameObject.activeInHierarchy}");

                if (!handler.enabled || !handler.gameObject.activeInHierarchy)
                {
                    report.AppendLine("  [FAIL] Handler exists but is DISABLED");
                    issueCount++;
                }
                else
                {
                    report.AppendLine("  [OK] Handler active");
                }

                // Check for CharacterController
                var controller = handler.GetComponent<CharacterController>();
                if (controller == null)
                {
                    report.AppendLine("  [FAIL] CharacterController NOT FOUND");
                    issueCount++;
                }
                else
                {
                    report.AppendLine($"  CharacterController: enabled={controller.enabled}");
                    if (!controller.enabled)
                    {
                        report.AppendLine("  [FAIL] CharacterController DISABLED");
                        issueCount++;
                    }
                }

                // Check input actions
                var actionsField = typeof(PlayerInputHandler).GetField("inputActions",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (actionsField != null)
                {
                    var actions = actionsField.GetValue(handler) as InputActionAsset;
                    if (actions == null)
                    {
                        report.AppendLine("  [WARNING] inputActions field is NULL");
                        report.AppendLine("            Fallback input will be used (keyboard/gamepad direct read)");
                    }
                    else
                    {
                        report.AppendLine($"  InputActions asset: {actions.name}");
                        var moveAction = actions.FindAction("Player/Move");
                        if (moveAction != null)
                        {
                            report.AppendLine($"  Move action bindings: {moveAction.bindings.Count}");
                            foreach (var binding in moveAction.bindings)
                            {
                                if (!binding.isComposite && !binding.isPartOfComposite)
                                    report.AppendLine($"    - {binding.path}");
                            }
                        }
                        else
                        {
                            report.AppendLine("  [FAIL] Move action NOT FOUND in InputActions");
                            issueCount++;
                        }
                    }
                }
            }

            // 4. Blocking UI/Dialogue
            report.AppendLine("\n[4] BLOCKING UI/DIALOGUE CHECK");
            var dialogueMgr = DialogueManager.Instance;
            if (dialogueMgr != null && dialogueMgr.IsPlaying)
            {
                report.AppendLine($"  [WARNING] DialogueManager is playing (duration: {dialogueMgr.CurrentLineDuration:F1}s)");
                report.AppendLine("            Movement should still work, but may be intended to pause");
            }
            else
            {
                report.AppendLine("  [OK] No dialogue blocking");
            }

            // Check for active UI panels that might block input
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                int activeUICount = 0;
                foreach (Transform child in canvas.transform)
                {
                    if (child.gameObject.activeInHierarchy)
                    {
                        var name = child.name.ToLower();
                        if (name.Contains("menu") || name.Contains("dialogue") || name.Contains("pause"))
                        {
                            report.AppendLine($"  [WARNING] Active UI panel: {child.name}");
                            activeUICount++;
                        }
                    }
                }
                if (activeUICount == 0)
                    report.AppendLine("  [OK] No blocking UI panels active");
            }

            // 5. Player GameObject check
            report.AppendLine("\n[5] PLAYER GAMEOBJECT");
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                report.AppendLine("  [FAIL] Player GameObject NOT FOUND (tag='Player')");
                issueCount++;
            }
            else
            {
                report.AppendLine($"  Found: {player.name}");
                report.AppendLine($"  Position: {player.transform.position}");
                report.AppendLine($"  Active: {player.activeInHierarchy}");

                if (!player.activeInHierarchy)
                {
                    report.AppendLine("  [FAIL] Player GameObject is INACTIVE");
                    issueCount++;
                }
            }

            // Summary
            report.AppendLine("\n========== SUMMARY ==========");
            if (issueCount == 0)
            {
                report.AppendLine("[OK] All systems operational!");
                report.AppendLine("If movement still doesn't work:");
                report.AppendLine("  1. Check gamepad physically (unplug/replug)");
                report.AppendLine("  2. Test in joy.cpl (Win+R)");
                report.AppendLine("  3. Exit Play mode and re-enter");
            }
            else
            {
                report.AppendLine($"[FOUND {issueCount} ISSUE(S)] - see above for details");
                report.AppendLine("\nQuick Fixes:");
                report.AppendLine("  - Wrong GameState → Exit Play, check GameBootstrap/SceneLoader");
                report.AppendLine("  - No gamepad → Unplug/replug, check F310 switch position");
                report.AppendLine("  - Handler disabled → Check PlayerSpawner spawned correctly");
            }
            report.AppendLine("\n========================================\n");

            string fullReport = report.ToString();
            Debug.Log(fullReport);

            // Show in dialog too for visibility
            UnityEditor.EditorUtility.DisplayDialog("Input System Diagnostics",
                issueCount == 0 ? "All systems operational! Check Console for details." :
                $"Found {issueCount} issue(s) - check Console for full report.", "OK");
        }
    }
}
