using UnityEngine;
using Tartaria.Integration;

namespace Tartaria.Integration
{
    /// <summary>
    /// Simple trigger to start the Moon 3 rail escort for easy playtesting after running the scaffold populate.
    /// Walk into the volume near the departure platform → full escort experience launches.
    /// </summary>
    public class Moon3StartEscortTrigger : MonoBehaviour
    {
        public RailEscortController controller;
        public int adoptedChildren = 3;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && controller != null && !controller.IsActive)
            {
                controller.StartEscort(adoptedChildren);
                Debug.Log("[Moon3] Escort started via trigger volume. Full 11-min Compassion & Rails experience active.");
                // Optional: destroy the trigger after use
                Destroy(gameObject, 1f);
            }
        }
    }
}