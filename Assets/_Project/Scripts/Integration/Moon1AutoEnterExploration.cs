// Moon1AutoEnterExploration.cs
// 2026-06-03 — Fix: when Play is hit directly on Echohaven_VerticalSlice (skipping
// the MainMenu / Start-click flow), GameStateManager stays in Boot forever, which
// gates PlayerInputHandler.Update via the `IsPlaying` guard at line 252. Input
// fires (left stick, A button, etc.), but the player never moves because the
// state guard returns early every frame.
//
// This component auto-fires `TransitionTo(Exploration)` after a one-second grace
// period if the state hasn't already advanced. Honors the main-menu flow:
// if BeginGameplay() ran (via menu click) the state will already be past Boot,
// so this no-ops.
//
// The 1s delay lets EchohavenContentSpawner + Moon1MasterBootstrap + other
// `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` chains finish initializing
// before we flip the state machine.

using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.Core;

namespace Tartaria.Integration
{
    internal static class Moon1AutoEnterExploration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            // Fire UNCONDITIONALLY — earlier scene-name guard didn't match when the active
            // scene's name was "Untitled" or some bootloader stand-in. Host.EnsureExplorationState
            // is safe in any scene: it only flips Boot/Loading → Exploration, no-ops otherwise.
            var go = new GameObject("Moon1AutoEnterExploration");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            go.AddComponent<Host>();
            Debug.Log("[Moon1AutoEnterExploration] Bootstrapped (any scene). Will check state in " + Host.GRACE_SECONDS + "s.");
        }

        internal class Host : MonoBehaviour
        {
            internal const float GRACE_SECONDS = 1.0f;

            void Start()
            {
                Invoke(nameof(EnsureExplorationState), GRACE_SECONDS);
                // Re-check every 2s in case some system re-transitions back to Loading.
                InvokeRepeating(nameof(EnsureExplorationState), GRACE_SECONDS + 2f, 2f);
                // Camera-follow safety net — if Main Camera isn't following the player,
                // bind it once a player exists in scene.
                InvokeRepeating(nameof(EnsureCameraFollowsPlayer), GRACE_SECONDS + 0.5f, 1.5f);
            }

            void EnsureExplorationState()
            {
                var mgr = GameStateManager.Instance;
                if (mgr == null)
                {
                    Debug.LogWarning("[Moon1AutoEnterExploration] GameStateManager.Instance was null; cannot transition. Player input will stay gated.");
                    return;
                }

                var current = mgr.CurrentState;
                if (current == GameState.Exploration || current == GameState.Combat || current == GameState.Tuning)
                {
                    return; // already in a playing state; quietly no-op (don't spam log)
                }

                if (current == GameState.Boot || current == GameState.Loading)
                {
                    Debug.Log($"[Moon1AutoEnterExploration] State stuck in {current} — auto-transitioning to Exploration so PlayerInputHandler.Update can run.");
                    mgr.TransitionTo(GameState.Exploration);
                }
                // Paused / Menu / Cinematic / Dialogue / Dead — quietly skip; those are
                // legitimate non-playing states that systems or the player have opted into.
            }

            // Track whether we've already bound a follow target so we don't re-bind every tick.
            bool _cameraBound;
            void EnsureCameraFollowsPlayer()
            {
                if (_cameraBound) return;
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player == null) player = GameObject.Find("Player");
                if (player == null) return;
                var cam = Camera.main;
                if (cam == null) return;

                // Look for any "camera follow" style component on the camera and bind it via SendMessage.
                // Use SendMessage so we don't need a hard reference to Tartaria.Camera.CameraController.
                cam.SendMessage("SetTarget", player.transform, SendMessageOptions.DontRequireReceiver);
                cam.SendMessage("SetFollowTarget", player.transform, SendMessageOptions.DontRequireReceiver);
                cam.SendMessage("SetPlayer", player.transform, SendMessageOptions.DontRequireReceiver);

                // Fallback: if nothing exists on the camera to follow, position the camera behind
                // the player so it's at least pointing at them at startup. This makes the player
                // VISIBLE even if a real follow controller isn't present.
                if (cam.GetComponent("CameraController") == null && cam.GetComponent("CameraFollow") == null)
                {
                    cam.transform.position = player.transform.position + new Vector3(0f, 4f, -8f);
                    cam.transform.LookAt(player.transform.position + Vector3.up * 1.5f);
                    Debug.Log($"[Moon1AutoEnterExploration] No follow controller on Main Camera — positioned camera behind player at {cam.transform.position}.");
                }
                else
                {
                    Debug.Log("[Moon1AutoEnterExploration] Bound Main Camera follow target → Player.");
                }
                _cameraBound = true;
            }
        }
    }
}
