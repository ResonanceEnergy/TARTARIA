using System;

namespace Tartaria.Core
{
    /// <summary>
    /// Static event bus for cross-assembly decoupling.
    /// Avoids circular asmdef references between Input, UI, and Integration.
    /// </summary>
    public static class GameEvents
    {
        public static event Action OnToggleAetherVision;
        public static event Action OnTogglePause;
        public static event Action<string, float> OnRequestPurgeCorruption;
        public static event Action OnRequestActivateRSBuff;
        public static event Action<float> OnRSChanged;
        public static event Action<string> OnBuildingRestored;   // buildingId
        public static event Action<string, UnityEngine.Vector3> OnBuildingDiscovered; // buildingName, position

        public static void FireToggleAetherVision() => OnToggleAetherVision?.Invoke();
        public static void FireTogglePause() => OnTogglePause?.Invoke();
        public static void FireRequestPurgeCorruption(string buildingId, float amount) => OnRequestPurgeCorruption?.Invoke(buildingId, amount);
        public static void FireRequestActivateRSBuff() => OnRequestActivateRSBuff?.Invoke();
        public static void FireRSChange(float amount) => OnRSChanged?.Invoke(amount);
        public static void FireBuildingRestored(string buildingId) => OnBuildingRestored?.Invoke(buildingId);
        public static void FireBuildingDiscovered(string buildingName, UnityEngine.Vector3 position) => OnBuildingDiscovered?.Invoke(buildingName, position);
    }
}
