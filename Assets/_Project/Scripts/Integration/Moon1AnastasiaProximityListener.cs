using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Listens for player proximity and notifies the rocker parent.
    /// Authored onto the trigger child inside AnastasiaRocker.prefab; the parent ref
    /// is wired up by Moon1AnastasiaRocker at runtime (prefab-safe).
    /// Split into its own file (2026-06-06) so Unity generates a MonoScript asset —
    /// classes sharing a file can never be serialized into prefabs (m_Script fileID 0 bug).
    /// </summary>
    public class Moon1AnastasiaProximityListener : MonoBehaviour
    {
        public Moon1AnastasiaRocker parent;
        void OnTriggerEnter(Collider other)
        {
            if (parent == null) return;
            if (!other.CompareTag("Player") && other.GetComponentInParent<CharacterController>() == null) return;
            parent.NotifyPlayerNearby();
        }
    }
}
