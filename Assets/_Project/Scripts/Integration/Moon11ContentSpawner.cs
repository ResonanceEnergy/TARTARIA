using UnityEngine;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.Gameplay;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 11: Spectral Aquifer - Pure water restoration + healing auras + Warning prophecy stone.
    /// GDD §03: Discovery → Aquifer chamber, Restoration → Purify water sources, Climax → Fountains heal ALL companions.
    /// </summary>
    public class Moon11ContentSpawner : MonoBehaviour
    {
        public static Moon11ContentSpawner Instance { get; private set; }

        void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; }

        void Start()
        {
            if (SaveManager.Instance?.GetMoonProgress(10) >= 100f) UnlockMoon11();
        }

        public void UnlockMoon11()
        {
            Debug.Log("[Moon11] Spectral Aquifer unlocked. Pure water lifeblood of empire.");
            // Spawn aquifer chamber, 12 purification fountains, Warning prophecy stones 10-11
            SaveManager.Instance?.SetMoonProgress(11, 100f);
            SaveManager.Instance?.UnlockMoon(12);
        }
    }
}
