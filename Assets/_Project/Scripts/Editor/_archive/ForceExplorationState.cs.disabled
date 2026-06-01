using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Editor
{
    /// <summary>
    /// Emergency fix for stuck game state. Forces transition to Exploration
    /// if the game is stuck in Boot/Loading and won't accept input.
    ///
    /// Run via menu: Tartaria → FIX: Force Exploration State
    /// </summary>
    public static class ForceExplorationState
    {
        [UnityEditor.MenuItem("Tartaria/FIX: Force Exploration State")]
        static void ForceExploration()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[ForceExploration] Must be in Play mode!");
                UnityEditor.EditorUtility.DisplayDialog("Force Exploration",
                    "Enter Play mode first.", "OK");
                return;
            }

            var gsm = GameStateManager.Instance;
            if (gsm == null)
            {
                Debug.LogError("[ForceExploration] GameStateManager.Instance is NULL!");
                return;
            }

            var currentState = gsm.CurrentState;
            Debug.Log($"[ForceExploration] Current state: {currentState} → forcing Exploration");

            gsm.TransitionTo(GameState.Exploration);

            Debug.Log($"[ForceExploration] New state: {gsm.CurrentState}");
            Debug.Log($"[ForceExploration] IsPlaying: {gsm.IsPlaying}");

            if (gsm.IsPlaying)
            {
                Debug.Log("[ForceExploration] SUCCESS - input should now work!");
                UnityEditor.EditorUtility.DisplayDialog("Force Exploration",
                    "State changed to Exploration. Try moving now!", "OK");
            }
            else
            {
                Debug.LogWarning("[ForceExploration] State transition failed!");
            }
        }
    }
}
