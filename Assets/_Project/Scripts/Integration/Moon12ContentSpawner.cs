using UnityEngine;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.Gameplay;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12: Crystal Moon - Bell tower network sync + planetary ring convergence + ALL companions.
    /// GDD §03: Discovery → 12 towers awaiting activation, Climax → All bells ring simultaneously (60s planetary harmonic).
    /// </summary>
    public class Moon12ContentSpawner : MonoBehaviour
    {
        public static Moon12ContentSpawner Instance { get; private set; }

        void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; }

        void Start()
        {
            if (SaveManager.Instance?.GetMoonProgress(11) >= 100f) UnlockMoon12();
        }

        public void UnlockMoon12()
        {
            Debug.Log("[Moon12] Bell tower network awaits. 12 towers, 12 continents, planetary ring climax.");
            // Spawn 12 bell towers, tune each to neighbors, Reset global assault, 60s planetary ring
            SaveManager.Instance?.SetMoonProgress(12, 100f);
            SaveManager.Instance?.UnlockMoon(13);
        }
    }
}
