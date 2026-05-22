using UnityEngine;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.Gameplay;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13: Cosmic Moon - Final Node + Echo Realms + Zereth confrontation + THE CHOICE.
    /// GDD §03: 3 Echo Realms (Golden Age, Dissonant Timeline, Moment of Flood), Zereth redemption, 3 endings.
    /// Endings: Harmony (Mud Flood reverses), Echo (parallel timelines), Reset (controlled distribution).
    /// </summary>
    public class Moon13ContentSpawner : MonoBehaviour
    {
        public static Moon13ContentSpawner Instance { get; private set; }

        void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; }

        void Start()
        {
            if (SaveManager.Instance?.GetMoonProgress(12) >= 100f) UnlockMoon13();
        }

        public void UnlockMoon13()
        {
            Debug.Log("[Moon13] 13th Moon rises. 17th Hour approaches. Final Node beneath New Chicago. Zereth awaits.");
            // Spawn 3 Echo Realms, Zereth resonance dialogue, THE CHOICE (Harmony/Echo/Reset), finale
            // Harmony ending: Mud recedes, buildings rise, giants walk, Lirael sings, Korath whispers
            SaveManager.Instance?.SetMoonProgress(13, 100f);
            Debug.Log("[Moon13] Campaign complete. The Aether resumes.");
        }
    }
}
