using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 — Lirael first contact on entering the initial purge site volume.
    /// Lightweight trigger for the vertical slice. Full companion logic lives in LiraelController + Moon2LunarContentSpawner.
    /// </summary>
    public class Moon2LiraelIntroTrigger : MonoBehaviour
    {
        bool _triggered;

        void OnTriggerEnter(Collider other)
        {
            if (_triggered || !other.CompareTag("Player")) return;
            _triggered = true;

            var lirael = FindAnyObjectByType<LiraelController>();
            if (lirael != null)
            {
                lirael.IntroduceMoon2FirstPurgeSite();
            }

            // Ambient stinger for entering the shadow caverns
            // Moon2LunarContentSpawner.Instance?.PlayEntryStinger();

            HUDController.Instance?.ShowContextPrompt("THE CAVERNS REMEMBER... LISTEN");
            Destroy(gameObject, 1.5f);
        }
    }
}