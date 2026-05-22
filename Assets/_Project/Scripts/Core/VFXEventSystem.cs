using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Event args for VFX requests.
    /// </summary>
    public class VFXEventArgs
    {
        public VFXEffect Effect { get; set; }
        public Vector3 Position { get; set; }
    }

    /// <summary>
    /// Cross-assembly VFX event system.
    /// AI/Gameplay fire events → Integration's VFXController subscribes and plays effects.
    /// Architecture: Decouples Tartaria.AI from Tartaria.Integration (prevents circular dependency).
    /// </summary>
    public static class VFXEventSystem
    {
        /// <summary>
        /// Fired when any system requests a VFX effect.
        /// Subscribe from VFXController in Integration assembly.
        /// </summary>
        public static event System.Action<VFXEventArgs> OnVFXRequested;

        /// <summary>
        /// Request a VFX effect at a world position.
        /// Called from AI/Gameplay scripts in any assembly.
        /// </summary>
        public static void RequestVFX(VFXEffect effect, Vector3 position)
        {
            OnVFXRequested?.Invoke(new VFXEventArgs 
            { 
                Effect = effect, 
                Position = position 
            });
        }
    }
}
