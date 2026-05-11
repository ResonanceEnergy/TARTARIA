using UnityEngine;
using System.Reflection;

namespace Tartaria.World
{
    /// <summary>
    /// Day/Night APV Blender — blends Adaptive Probe Volume scenarios (if available)
    /// or falls back to RenderSettings ambient light lerp for day/night cycle.
    /// Reads DayNightCycleController.TimeOfDay (normalized 0=midnight, 0.5=noon).
    /// 
    /// TODO: Real APV scenario blending requires baked scenarios (Day + Night).
    ///       This implementation provides runtime ambient lighting as a placeholder.
    /// </summary>
    [DisallowMultipleComponent]
    public class DayNightAPVBlender : MonoBehaviour
    {
        [Header("Ambient Colors")]
        [SerializeField] Color dayColor = new(0.66f, 0.78f, 1.0f, 1f); // #A8C8FF
        [SerializeField] Color nightColor = new(0.1f, 0.1f, 0.23f, 1f); // #1A1A3A

        [Header("Ambient Intensity")]
        [SerializeField, Range(0f, 2f)] float dayIntensity = 1.0f;
        [SerializeField, Range(0f, 2f)] float nightIntensity = 0.3f;

        object _dayNightController;
        PropertyInfo _timeOfDayProp;
        object _apvInstance;
        PropertyInfo _blendingFactorProp;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var go = new GameObject("DayNightAPVBlender");
            DontDestroyOnLoad(go);
            go.AddComponent<DayNightAPVBlender>();
        }

        void Awake()
        {
            // Reflect into DayNightCycleController.TimeOfDay
            var dnType = System.Type.GetType("Tartaria.Gameplay.DayNightCycleController, Tartaria.Gameplay");
            if (dnType != null)
            {
                var instances = FindObjectsByType(dnType, FindObjectsSortMode.None);
                if (instances.Length > 0)
                {
                    _dayNightController = instances[0];
                    _timeOfDayProp = dnType.GetProperty("TimeOfDay", BindingFlags.Public | BindingFlags.Instance);
                }
            }

            if (_timeOfDayProp == null)
            {
                Debug.LogWarning("[DayNightAPVBlender] Could not find DayNightCycleController.TimeOfDay — using fixed time 0.5 (noon).");
            }

            // Try to reflect into ProbeReferenceVolume.instance.BlendingFactor (APV API)
            var apvType = System.Type.GetType("UnityEngine.Rendering.ProbeReferenceVolume, Unity.RenderPipelines.Core.Runtime");
            if (apvType != null)
            {
                var instanceProp = apvType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp != null)
                {
                    _apvInstance = instanceProp.GetValue(null);
                    if (_apvInstance != null)
                    {
                        _blendingFactorProp = apvType.GetProperty("BlendingFactor", BindingFlags.Public | BindingFlags.Instance);
                    }
                }
            }

            if (_blendingFactorProp == null)
            {
                Debug.Log("[DayNightAPVBlender] APV scenario blending API not available — using RenderSettings ambient lerp fallback.");
            }
        }

        void Update()
        {
            float t = GetTimeOfDay();

            if (_apvInstance != null && _blendingFactorProp != null)
            {
                // APV scenario blending (if baked scenarios exist)
                try
                {
                    _blendingFactorProp.SetValue(_apvInstance, t);
                }
                catch
                {
                    // APV not initialized or scenarios not baked — fall through to ambient fallback
                }
            }

            // Always drive RenderSettings as fallback (works without APV)
            Color ambientColor = Color.Lerp(nightColor, dayColor, t);
            float ambientIntensity = Mathf.Lerp(nightIntensity, dayIntensity, t);

            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = ambientIntensity;
        }

        float GetTimeOfDay()
        {
            if (_dayNightController == null || _timeOfDayProp == null)
                return 0.5f; // Noon default

            try
            {
                return (float)_timeOfDayProp.GetValue(_dayNightController);
            }
            catch
            {
                return 0.5f;
            }
        }
    }
}
