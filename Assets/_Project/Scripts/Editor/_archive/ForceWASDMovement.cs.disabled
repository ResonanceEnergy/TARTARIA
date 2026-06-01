using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Editor
{
    /// <summary>
    /// NUCLEAR OPTION: Bypasses ALL systems and directly moves player with WASD.
    /// Use when even the emergency fix doesn't work.
    /// Run ONCE in Play Mode: Tartaria → NUCLEAR: Force WASD Movement
    /// </summary>
    public static class ForceWASDMovement
    {
        private static bool _active = false;
        private static GameObject _player;
        private static CharacterController _controller;

        [UnityEditor.MenuItem("Tartaria/NUCLEAR: Force WASD Movement", false, 1001)]
        public static void ActivateDirectMovement()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[ForceWASD] Must be in Play Mode!");
                return;
            }

            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null)
            {
                Debug.LogError("[ForceWASD] NO PLAYER FOUND!");
                UnityEditor.EditorUtility.DisplayDialog(
                    "No Player",
                    "Can't find Player GameObject.\n\nMake sure player spawned first.",
                    "OK"
                );
                return;
            }

            _controller = _player.GetComponent<CharacterController>();
            if (_controller == null)
            {
                Debug.LogError("[ForceWASD] Player has no CharacterController!");
                return;
            }

            _active = true;
            UnityEditor.EditorApplication.update += DirectUpdate;

            Debug.Log("=== NUCLEAR WASD MOVEMENT ACTIVE ===");
            Debug.Log("W/A/S/D = Move (ignores ALL game systems)");
            Debug.Log("This runs in Editor update loop - bypasses EVERYTHING");

            UnityEditor.EditorUtility.DisplayDialog(
                "Nuclear Movement Active",
                "WASD movement is now DIRECTLY controlling the player.\n\n" +
                "This bypasses ALL game systems.\n\n" +
                "Press W/A/S/D to move.\n\n" +
                "Stop Play Mode to deactivate.",
                "OK"
            );
        }

        [UnityEditor.MenuItem("Tartaria/STOP Nuclear Movement", false, 1002)]
        public static void DeactivateDirectMovement()
        {
            _active = false;
            UnityEditor.EditorApplication.update -= DirectUpdate;
            Debug.Log("[ForceWASD] Deactivated");
        }

        private static void DirectUpdate()
        {
            if (!_active || !Application.isPlaying || _player == null || _controller == null)
            {
                DeactivateDirectMovement();
                return;
            }

            var kb = Keyboard.current;
            if (kb == null) return;

            Vector3 move = Vector3.zero;
            if (kb.wKey.isPressed) move += Vector3.forward;
            if (kb.sKey.isPressed) move += Vector3.back;
            if (kb.aKey.isPressed) move += Vector3.left;
            if (kb.dKey.isPressed) move += Vector3.right;

            if (move.sqrMagnitude > 0.01f)
            {
                move = move.normalized * 6f * Time.deltaTime;
                _controller.Move(move);
                Debug.Log($"[ForceWASD] Moved to {_player.transform.position}");
            }
        }
    }
}
