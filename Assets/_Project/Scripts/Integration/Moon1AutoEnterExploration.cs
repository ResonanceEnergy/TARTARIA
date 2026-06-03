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
            // Only fire in the Moon 1 gameplay scene. Main menu / loading / Moon 2-13 scenes
            // should run their own boot chain.
            var active = SceneManager.GetActiveScene();
            if (active == null || !active.IsValid()) return;
            string name = active.name;
            if (string.IsNullOrEmpty(name)) return;
            if (!(name.Contains("Echohaven") || name.Contains("Moon1") || name.Contains("VerticalSlice"))) return;

            // Create a host GameObject so we can schedule the delayed transition via coroutine.
            var go = new GameObject("Moon1AutoEnterExploration");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            go.AddComponent<Host>();
        }

        private class Host : MonoBehaviour
        {
            const float GRACE_SECONDS = 1.0f;

            void Start()
            {
                Invoke(nameof(EnsureExplorationState), GRACE_SECONDS);
            }

            void EnsureExplorationState()
            {
                var mgr = GameStateManager.Instance;
                if (mgr == null)
                {
                    Debug.LogWarning("[Moon1AutoEnterExploration] GameStateManager.Instance was null after grace period; cannot transition. Player input will stay gated.");
                    return;
                }

                var current = mgr.CurrentState;
                if (current == GameState.Exploration || current == GameState.Combat || current == GameState.Tuning)
                {
                    Debug.Log($"[Moon1AutoEnterExploration] State already advanced to {current}; no transition needed.");
                    return;
                }

                if (current == GameState.Boot || current == GameState.Loading)
                {
                    Debug.Log($"[Moon1AutoEnterExploration] State stuck in {current} after {GRACE_SECONDS:F1}s — auto-transitioning to Exploration so PlayerInputHandler.Update can run.");
                    mgr.TransitionTo(GameState.Exploration);
                }
                else
                {
                    // Paused / Menu / Cinematic / Dialogue / Dead — leave alone, those are
                    // legitimate non-playing states the player or systems have opted into.
                    Debug.Log($"[Moon1AutoEnterExploration] State is {current} (not Boot/Loading); leaving in place.");
                }
            }
        }
    }
}
