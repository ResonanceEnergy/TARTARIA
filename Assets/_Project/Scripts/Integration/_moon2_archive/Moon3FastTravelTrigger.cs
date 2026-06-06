using UnityEngine;
using Tartaria.Core;
using Tartaria.Audio;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3: Simple fast travel trigger for Continental Rail after successful escort.
    /// On trigger, unlocks fast travel and can load hub or show UI.
    /// F310 gamepad friendly (A to confirm).
    /// </summary>
    public class Moon3FastTravelTrigger : MonoBehaviour
    {
        public string targetZone = "ContinentalRail_Hub";

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && RailEscortController.Moon3ContinentalRailFastTravelUnlocked)
            {
                Debug.Log($"[Moon3] Fast travel to {targetZone} unlocked and triggered. The golden rails now connect the highlands to the wider network.");
                ServiceLocator.VFX?.SpawnGiantEchoRelease(transform.position + Vector3.up * 3f);
                ServiceLocator.VFX?.PlayResonancePulse(transform.position, 15f);
                ServiceLocator.VFX?.SpawnWindElectricReaction(transform.position, true, 1.5f);
                AudioManager.Instance?.PlaySFX2D("FastTravelActivate");
                HapticFeedbackManager.Instance?.PlayClimaxRumble();

                // Functional fast travel: "travel" the player forward along the rail with dramatic golden burst (simulates Continental Hub arrival)
                Vector3 forward = (RailEscortController.Instance != null) ? (RailEscortController.Instance.railEnd - RailEscortController.Instance.railStart).normalized : transform.forward;
                Vector3 newPos = other.transform.position + forward * 30f + Vector3.up * 3f;
                other.transform.position = newPos;

                // Big travel VFX burst + series of golden pulses along the path (dramatic "network expansion" feel)
                ServiceLocator.VFX?.SpawnGiantEchoRelease(newPos);
                ServiceLocator.VFX?.PlayResonancePulse(newPos, 30f);

                if (!Tartaria.UI.SettingsOverlay.IsReducedMotion)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        ServiceLocator.VFX?.SpawnOrphanLullabyGlow(newPos + forward * (8 + i * 6), 3, 1.5f);
                    }
                }

                // Final confirmation
                Debug.Log("[Moon3] Fast travel complete - Continental Rail network expanded. The highlands sing with golden resonance.");
            }
        }
    }
}