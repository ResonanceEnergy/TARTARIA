using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Editor
{
    /// <summary>
    /// EMERGENCY FIX: Force game into playable state with functional input.
    /// Run via Unity menu: Tartaria → EMERGENCY: Make Game Playable NOW
    /// Bypasses all startup checks and forces everything into working state.
    /// </summary>
    public static class EmergencyPlayableFix
    {
        [UnityEditor.MenuItem("Tartaria/EMERGENCY: Make Game Playable NOW", false, 999)]
        public static void ForcePlayableState()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[EmergencyFix] Must be in Play Mode! Press Ctrl+P first.");
                UnityEditor.EditorUtility.DisplayDialog(
                    "Not in Play Mode",
                    "You must enter Play Mode first (Ctrl+P), THEN run this menu item.",
                    "OK"
                );
                return;
            }

            Debug.Log("=== EMERGENCY PLAYABLE FIX START ===");
            int fixCount = 0;

            // 1. Force Exploration state
            if (GameStateManager.Instance != null)
            {
                Debug.Log("[EmergencyFix] Forcing Exploration state...");
                GameStateManager.Instance.TransitionTo(GameState.Exploration);
                fixCount++;
            }
            else
            {
                Debug.LogError("[EmergencyFix] GameStateManager not found!");
            }

            // 2. Find and verify player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[EmergencyFix] NO PLAYER FOUND!");

                // Try to force spawn
                var spawner = Object.FindFirstObjectByType<Tartaria.Integration.PlayerSpawner>();
                if (spawner != null)
                {
                    Debug.Log("[EmergencyFix] Found PlayerSpawner, forcing spawn...");
                    spawner.SendMessage("SpawnPlayer", SendMessageOptions.DontRequireReceiver);
                    player = GameObject.FindGameObjectWithTag("Player");
                }

                if (player == null)
                {
                    UnityEditor.EditorUtility.DisplayDialog(
                        "Player Not Found!",
                        "No Player GameObject found and couldn't spawn one.\n\n" +
                        "Check if PlayerSpawner exists in scene.\n" +
                        "Or manually place a Player prefab.",
                        "OK"
                    );
                    return;
                }
            }

            if (player != null)
            {
                Debug.Log($"[EmergencyFix] Player found: {player.name} at {player.transform.position}");

                // Verify CharacterController
                var cc = player.GetComponent<CharacterController>();
                if (cc == null)
                {
                    Debug.LogError("[EmergencyFix] Player missing CharacterController!");
                }
                else
                {
                    cc.enabled = true;
                    Debug.Log("[EmergencyFix] CharacterController enabled");
                    fixCount++;
                }

                // Verify PlayerInputHandler
                var inputHandler = player.GetComponent<Tartaria.Input.PlayerInputHandler>();
                if (inputHandler == null)
                {
                    Debug.LogError("[EmergencyFix] Player missing PlayerInputHandler!");
                }
                else
                {
                    inputHandler.enabled = true;
                    Debug.Log("[EmergencyFix] PlayerInputHandler enabled");
                    fixCount++;
                }

                // Check gamepad
                var gamepad = Gamepad.current;
                if (gamepad != null)
                {
                    Debug.Log($"[EmergencyFix] ✓ Gamepad detected: {gamepad.displayName}");
                    Debug.Log($"[EmergencyFix] Left Stick: {gamepad.leftStick.ReadValue()}");
                    fixCount++;
                }
                else
                {
                    Debug.LogWarning("[EmergencyFix] NO GAMEPAD DETECTED - keyboard only");
                }
            }
            else
            {
                Debug.LogError("[EmergencyFix] NO PLAYER FOUND! Check PlayerSpawner.");
            }

            // 3. Check camera
            var cam = Camera.main;
            if (cam != null)
            {
                Debug.Log($"[EmergencyFix] Main Camera: {cam.name} at {cam.transform.position}");

                // Find CameraController
                foreach (var comp in cam.GetComponents<MonoBehaviour>())
                {
                    if (comp != null && comp.GetType().Name == "CameraController")
                    {
                        comp.enabled = true;
                        var followTarget = comp.GetType().GetField("followTarget");
                        if (followTarget != null && player != null)
                        {
                            followTarget.SetValue(comp, player.transform);
                            Debug.Log("[EmergencyFix] Camera followTarget set to Player");
                            fixCount++;
                        }
                        break;
                    }
                }
            }

            // 4. Report status
            Debug.Log($"=== EMERGENCY FIX COMPLETE: {fixCount} systems fixed ===");
            Debug.Log("[EmergencyFix] Try moving with left stick or WASD now!");

            UnityEditor.EditorUtility.DisplayDialog(
                "Emergency Fix Applied",
                $"Fixed {fixCount} systems.\n\n" +
                $"State: {(GameStateManager.Instance != null ? GameStateManager.Instance.CurrentState.ToString() : "Unknown")}\n" +
                $"Player: {(player != null ? "Found" : "MISSING")}\n" +
                $"Gamepad: {(Gamepad.current != null ? Gamepad.current.displayName : "Not detected")}\n\n" +
                "Try moving with left stick or WASD!",
                "OK"
            );
        }

        [UnityEditor.MenuItem("Tartaria/DEBUG: Log Input Status", false, 1000)]
        public static void LogInputStatus()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Must be in Play Mode!");
                return;
            }

            Debug.Log("=== INPUT STATUS DEBUG ===");

            // Gamepad
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Debug.Log($"Gamepad: {gamepad.displayName}");
                Debug.Log($"  Left Stick: {gamepad.leftStick.ReadValue()}");
                Debug.Log($"  Right Stick: {gamepad.rightStick.ReadValue()}");
                Debug.Log($"  A Button: {gamepad.aButton.isPressed}");
                Debug.Log($"  B Button: {gamepad.bButton.isPressed}");
                Debug.Log($"  LT: {gamepad.leftTrigger.ReadValue()}");
                Debug.Log($"  RT: {gamepad.rightTrigger.ReadValue()}");
            }
            else
            {
                Debug.LogWarning("NO GAMEPAD");
            }

            // Keyboard
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                Debug.Log($"WASD: W={keyboard.wKey.isPressed} A={keyboard.aKey.isPressed} S={keyboard.sKey.isPressed} D={keyboard.dKey.isPressed}");
            }

            // GameState
            if (GameStateManager.Instance != null)
            {
                Debug.Log($"GameState: {GameStateManager.Instance.CurrentState}");
                Debug.Log($"IsPlaying: {GameStateManager.Instance.IsPlaying}");
            }

            // Player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log($"Player: {player.name} at {player.transform.position}");
                var handler = player.GetComponent<Tartaria.Input.PlayerInputHandler>();
                if (handler != null)
                {
                    Debug.Log($"  PlayerInputHandler: enabled={handler.enabled}, IsMoving={handler.IsMoving}");
                }
            }
            else
            {
                Debug.LogError("NO PLAYER FOUND");
            }
        }
    }
}
