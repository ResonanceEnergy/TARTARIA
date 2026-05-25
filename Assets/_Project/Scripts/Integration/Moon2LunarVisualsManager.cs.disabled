using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// STUB: Manages visual feedback for Moon 2 lunar alignment mechanics.
    /// Displays phase transitions, lunar resonance effects, and moon-phase-dependent environmental visuals.
    /// TODO: Implement full lunar cycle visualization system.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon2LunarVisualsManager : MonoBehaviour
    {
        public static Moon2LunarVisualsManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Sets the visual representation of the current lunar phase (0-7).</summary>
        public void SetPhaseVisual(int phase)
        {
            // TODO: Implement lunar phase visual updates
            Debug.Log($"[Moon2LunarVisualsManager] SetPhaseVisual({phase}) - STUB");
        }

        /// <summary>Plays the transition animation between lunar phases.</summary>
        public void PlayPhaseTransition()
        {
            // TODO: Implement phase transition animation
            Debug.Log("[Moon2LunarVisualsManager] PlayPhaseTransition() - STUB");
        }

        /// <summary>Plays lunar shadow purge transformation VFX (golden burn effect).</summary>
        public void PlayLunarShadowPurgeCathedralTransformation(Vector3 position, float duration)
        {
            Debug.Log($"[Moon2LunarVisualsManager] PlayLunarShadowPurgeCathedralTransformation at {position} for {duration}s - STUB");
            // TODO: Implement golden purge VFX
        }
    }
}
