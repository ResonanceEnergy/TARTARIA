using UnityEngine;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Applies a movement slow while the player is inside muddy terrain.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class MudZone : MonoBehaviour
    {
        [SerializeField, Range(0.2f, 1f)] float moveMultiplier = 0.6f;

        static int s_activeMudZones;

        void Awake()
        {
            if (TryGetComponent<Collider>(out var col))
                col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            s_activeMudZones++;
            PlayerInputHandler.Instance?.SetExternalMoveMultiplier(moveMultiplier);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            s_activeMudZones = Mathf.Max(0, s_activeMudZones - 1);
            if (s_activeMudZones == 0)
                PlayerInputHandler.Instance?.SetExternalMoveMultiplier(1f);
        }
    }
}