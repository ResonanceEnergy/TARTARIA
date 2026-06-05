using UnityEngine;
using UnityEditor;
using Tartaria.Integration;
using UnityEngine.InputSystem;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// Runtime diagnostic tool - checks all spawners, buildings, NPCs, and controller status.
    /// Menu: Tartaria > DIAGNOSE: Check Runtime State
    /// </summary>
    public static class DiagnoseRuntime
    {
        [MenuItem("Tartaria/7 Diagnose/Runtime State", priority = 730)]
        static void CheckRuntimeState()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[Diagnose] Must be in Play mode!");
                return;
            }

            Debug.Log("\n═══════════════════════════════════════");
            Debug.Log("   TARTARIA RUNTIME DIAGNOSTIC REPORT");
            Debug.Log("═══════════════════════════════════════\n");

            // Check spawners
            var buildingSpawner = Object.FindFirstObjectByType<BuildingSpawner>();
            var contentSpawner = Object.FindFirstObjectByType<EchohavenContentSpawner>();

            Debug.Log("--- SPAWNERS ---");
            Debug.Log($"BuildingSpawner: {(buildingSpawner != null ? "FOUND [OK]" : "MISSING [!]")}");
            Debug.Log($"EchohavenContentSpawner: {(contentSpawner != null ? "FOUND [OK]" : "MISSING [!]")}");

            // Check buildings
            Debug.Log("\n--- BUILDINGS ---");
            var dome = GameObject.Find("Echohaven_StarDome") ?? GameObject.Find("Building_dome");
            var fountain = GameObject.Find("Echohaven_HarmonicFountain") ?? GameObject.Find("Building_fountain");
            var spire = GameObject.Find("Echohaven_CrystalSpire") ?? GameObject.Find("Building_spire");

            Debug.Log($"Star Dome: {(dome != null ? $"FOUND at {dome.transform.position} [OK]" : "MISSING [!]")}");
            Debug.Log($"Harmonic Fountain: {(fountain != null ? $"FOUND at {fountain.transform.position} [OK]" : "MISSING [!]")}");
            Debug.Log($"Crystal Spire: {(spire != null ? $"FOUND at {spire.transform.position} [OK]" : "MISSING [!]")}");

            // Check NPCs
            Debug.Log("\n--- NPCs ---");
            var milo = GameObject.Find("Milo");
            var cassian = GameObject.Find("Cassian");
            var lirael = GameObject.Find("Lirael");

            Debug.Log($"Milo: {(milo != null ? $"FOUND at {milo.transform.position}" : "MISSING [!]")}");
            Debug.Log($"Cassian: {(cassian != null ? $"FOUND at {cassian.transform.position}" : "MISSING [!]")}");
            Debug.Log($"Lirael: {(lirael != null ? $"FOUND at {lirael.transform.position}" : "MISSING [!]")}");

            // Count all KayKit characters
            var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int kayKitCount = 0;
            foreach (var go in allObjects)
            {
                if (go.name.Contains("KayKit") || go.name.Contains("Character"))
                    kayKitCount++;
            }
            Debug.Log($"Total KayKit/Character GameObjects: {kayKitCount}");

            // Check controller
            Debug.Log("\n--- INPUT SYSTEM ---");
            var gamepad = Gamepad.current;
            Debug.Log($"Gamepad detected: {(gamepad != null ? $"YES - {gamepad.name} [OK]" : "NO [!]")}");

            if (gamepad != null)
            {
                Debug.Log($"Left stick: {gamepad.leftStick.ReadValue()}");
                Debug.Log($"Buttons pressed: {gamepad.allControls.Count(c => c is UnityEngine.InputSystem.Controls.ButtonControl btn && btn.isPressed)}");
            }

            // Check player
            Debug.Log("\n--- PLAYER ---");
            var player = GameObject.Find("Player");
            Debug.Log($"Player GameObject: {(player != null ? $"FOUND at {player.transform.position} [OK]" : "MISSING [!]")}");

            if (player != null)
            {
                var cc = player.GetComponent<CharacterController>();
                var input = player.GetComponent<Tartaria.Input.PlayerInputHandler>();
                Debug.Log($"  CharacterController: {(cc != null ? "[OK]" : "[!]")}");
                Debug.Log($"  PlayerInputHandler: {(input != null ? "[OK]" : "[!]")}");
            }

            Debug.Log("\n═══════════════════════════════════════\n");
        }
    }
}
