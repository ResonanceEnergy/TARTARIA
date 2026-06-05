using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// Moon 5 Overtone HUD — STUB.
    ///
    /// Original implementation referenced WhiteCityAmplificationController (in Tartaria.Integration),
    /// which creates a circular asmdef dependency (Tartaria.Integration → Tartaria.UI → Tartaria.Integration).
    ///
    /// The Moon 5 amplification status is now surfaced via HUDController.ShowObjective() calls
    /// from WhiteCityAmplificationController directly. This stub preserves the singleton so any
    /// scene/prefab references still resolve, but renders nothing on its own.
    ///
    /// DO NOT re-add `using Tartaria.Integration;` here — it will break the build.
    /// </summary>
    public class Moon5AmplificationHUD : MonoBehaviour
    {
        public static Moon5AmplificationHUD Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
