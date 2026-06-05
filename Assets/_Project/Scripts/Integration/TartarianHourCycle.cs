using System;
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// TartarianHourCycle — drives the 17-hour visual day cycle of Tartaria.
    ///
    /// Hours run 0..16 (17 hour positions). Hour 0 = dawn, hour ~8.5 = noon,
    /// hour 16 → 0 = night wrap. The 17th-hour event fires once per cycle when
    /// the clock crosses from hour 16 back to hour 0.
    ///
    /// Sun: rotates Sun_GoldenHour (preferred) or RenderSettings.sun or any
    /// directional Light. Azimuth = hourFloat/17 * 360°; elevation pivots on
    /// horizon at hour 0/17 and peaks ~70° at hour 8.5.
    ///
    /// Subscribers: Moon1NarrativeBeats (Cathedral Light Eruption on 17th hour),
    /// Moon1CinematicMoments (wide pan), Moon1LightingSetup (ambient tweak).
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-50)]
    public class TartarianHourCycle : MonoBehaviour
    {
        public const int HOURS_PER_DAY = 17;

        /// <summary>Real seconds per in-game hour. 30s × 17 = 8.5 minutes per cycle.</summary>
        [SerializeField] float secondsPerHour = 30f;

        /// <summary>Starting hour for new sessions (0..16). 6 = mid-morning.</summary>
        [SerializeField] int startHour = 6;

        public int CurrentHour { get; private set; }
        public float CurrentHourPhase { get; private set; } // 0..1 within the hour
        public float HourFloat => CurrentHour + CurrentHourPhase;

        /// <summary>Raised once per 17-hour cycle when the clock wraps to hour 0.</summary>
        public static event Action OnSeventeenthHour;

        /// <summary>Raised at each hour transition. Payload: new hour (0..16).</summary>
        public static event Action<int> OnHourChanged;

        Light _sunLight;
        bool _seventeenthFiredThisCycle;
        float _accumSeconds;

        void Awake()
        {
            CurrentHour = Mathf.Clamp(startHour, 0, HOURS_PER_DAY - 1);
            CurrentHourPhase = 0f;
            ResolveSunLight();
        }

        void Start()
        {
            // Late re-resolve in case the sun was spawned post-Awake.
            if (_sunLight == null) ResolveSunLight();
        }

        void ResolveSunLight()
        {
            var named = GameObject.Find("Sun_GoldenHour");
            if (named != null) _sunLight = named.GetComponent<Light>();
            if (_sunLight == null) _sunLight = RenderSettings.sun;
            if (_sunLight == null)
            {
                foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                {
                    if (l.type == LightType.Directional) { _sunLight = l; break; }
                }
            }
        }

        void Update()
        {
            if (secondsPerHour <= 0f) return;

            _accumSeconds += Time.deltaTime;
            CurrentHourPhase = _accumSeconds / secondsPerHour;

            if (_accumSeconds >= secondsPerHour)
            {
                _accumSeconds -= secondsPerHour;
                CurrentHour++;
                if (CurrentHour >= HOURS_PER_DAY)
                {
                    CurrentHour = 0;
                    if (!_seventeenthFiredThisCycle)
                    {
                        _seventeenthFiredThisCycle = true;
                        try { OnSeventeenthHour?.Invoke(); }
                        catch (Exception ex) { Debug.LogError($"[TartarianHourCycle] OnSeventeenthHour subscriber threw: {ex}"); }
                        // Mirror to canonical Tartaria.Core.GameEvents.OnSeventeenthHour so cross-assembly
                        // subscribers (Sprint 12 P2.L1 canonical surface) stay in sync. Local event is
                        // retained for legacy Integration-assembly subscribers (Moon1NarrativeBeats,
                        // Moon1CinematicMoments) until they migrate to the GameEvents surface.
                        try { Tartaria.Core.GameEvents.FireSeventeenthHour(); }
                        catch (Exception ex) { Debug.LogError($"[TartarianHourCycle] GameEvents.OnSeventeenthHour subscriber threw: {ex}"); }
                    }
                }
                else if (CurrentHour == 1)
                {
                    // Re-arm for next cycle
                    _seventeenthFiredThisCycle = false;
                }
                try { OnHourChanged?.Invoke(CurrentHour); }
                catch (Exception ex) { Debug.LogError($"[TartarianHourCycle] OnHourChanged subscriber threw: {ex}"); }
            }

            UpdateSun();
        }

        /// <summary>
        /// Restore current hour + intra-hour phase from a save snapshot.
        /// Called by Moon1SaveCoordinator OnAfterLoad. Clamps + re-arms the 17th-hour
        /// fire flag so the beat fires correctly after a mid-cycle load.
        /// </summary>
        public void SetHourFromSave(int hour, float hourPhase01)
        {
            CurrentHour = Mathf.Clamp(hour, 0, HOURS_PER_DAY - 1);
            CurrentHourPhase = Mathf.Clamp01(hourPhase01);
            _accumSeconds = CurrentHourPhase * secondsPerHour;
            // If we loaded into hour 0 again, the 17th-hour beat for the next wrap is fresh.
            // If we loaded into hour 1+, the beat already fired this cycle.
            _seventeenthFiredThisCycle = CurrentHour >= 1;
            ResolveSunLight();
            UpdateSun();
        }

        void UpdateSun()
        {
            if (_sunLight == null) return;
            float hourFloat = HourFloat;
            float azimuth = (hourFloat / HOURS_PER_DAY) * 360f;
            // elevation: sin curve, 0 at hour 0 and hour 17, peak 70° at hour 8.5
            float elevation = Mathf.Sin(Mathf.PI * hourFloat / HOURS_PER_DAY) * 70f - 10f;
            float elevNorm = Mathf.Clamp01((elevation + 10f) / 80f);
            _sunLight.transform.rotation = Quaternion.Euler(elevation, azimuth, 0f);
            _sunLight.color = Color.Lerp(new Color(1f, 0.6f, 0.3f), new Color(1f, 0.95f, 0.85f), elevNorm);
            _sunLight.intensity = Mathf.Lerp(0.15f, 1.3f, elevNorm);
        }
    }
}
