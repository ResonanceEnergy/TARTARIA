// Sprint 9 Lane 3 — Lirael Day-25 gate.
// Subscribes to TartarianCalendar.OnDayAdvanced; when day reaches 25, it
// activates the Day-25 Lirael beat exactly once. Real-life smoke testing
// of this file's behaviour goes through the Editor menus in
// Assets/_Project/Scripts/Editor/Moon1DaySmokeMenus.cs (Sprint 10 Lane 7).
//
// NOTE: API_CONTRACT.md §2 mentions a GameEvents.OnDayChanged at line 461,
// but the canonical source of day-tick events in this branch is the calendar's
// own C# event (TartarianCalendar.OnDayAdvanced, TartarianCalendar.cs:41).
// We subscribe to the calendar directly to avoid inventing a GameEvents API
// that does not exist.

using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Fires the Day-25 Lirael narrative beat once per playthrough.
    /// Hook this MonoBehaviour somewhere persistent in the Echohaven scene
    /// (Moon1_Systems is the usual home).
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1LiraelDay25Gate : MonoBehaviour
    {
        [Header("Gate")]
        [Tooltip("In-game day on which the gate fires. Default 25 per Sprint 9 Lane 3.")]
        [SerializeField] private int activationDay = 25;

        [Tooltip("If true, the gate will only fire once per scene load.")]
        [SerializeField] private bool fireOnce = true;

        [Header("Hooks (optional)")]
        [Tooltip("Optional LiraelController to flag as fully manifested when the gate fires.")]
        [SerializeField] private LiraelController liraelController;

        private bool _hasFired;
        private TartarianCalendar _cal;

        private void OnEnable()
        {
            _cal = TartarianCalendar.Instance;
            if (_cal == null)
            {
                Debug.LogWarning("[Moon1LiraelDay25Gate] TartarianCalendar.Instance is null on enable. " +
                                 "Gate will retry in Start() but you should verify execution order if this persists.");
                return;
            }
            _cal.OnDayAdvanced += HandleDayAdvanced;
        }

        private void Start()
        {
            if (_cal == null)
            {
                _cal = TartarianCalendar.Instance;
                if (_cal != null) _cal.OnDayAdvanced += HandleDayAdvanced;
            }

            // Cover the case where the scene loads with currentDay already >= activationDay
            // (e.g. loading a save mid-Moon).
            if (_cal != null && _cal.CurrentMoonDay >= activationDay)
                HandleDayAdvanced(_cal.CurrentMoonDay);
        }

        private void OnDisable()
        {
            if (_cal != null) _cal.OnDayAdvanced -= HandleDayAdvanced;
        }

        private void HandleDayAdvanced(int newDay)
        {
            if (fireOnce && _hasFired) return;
            if (newDay < activationDay) return;
            Activate(newDay);
        }

        private void Activate(int day)
        {
            _hasFired = true;
            Debug.Log($"[Moon1LiraelDay25Gate] Activating Day-{activationDay} Lirael beat (current day={day}).");

            if (liraelController == null) liraelController = LiraelController.Instance;
            if (liraelController != null)
            {
                // Per LiraelController public API (LiraelController.cs:83/136/204), the Day-25 beat
                // is the convergence of song-remembered + manifested. We call the public methods that
                // already exist rather than poking private state.
                liraelController.RememberSong();
                liraelController.BeginLullabySupport();
            }
            else
            {
                Debug.LogWarning("[Moon1LiraelDay25Gate] No LiraelController found at activation. " +
                                 "Beat will fire as a log-only marker — narrative writers may want to hook this.");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Sprint 10 Lane 7 — last-mile QA. Bypasses the day check and runs the
        /// activation path directly. Wire from the component context menu in the
        /// Inspector (right-click on the component header).
        /// Editor-only: stripped from shipped builds so designers cannot bypass gating in production.
        /// </summary>
        [ContextMenu("Force Activate Now")]
        public void ForceActivateNow()
        {
            Debug.Log("[Moon1LiraelDay25Gate] Force Activate Now invoked from context menu. Bypassing day check.");
            _hasFired = false; // allow re-run even if previously fired
            Activate(_cal != null ? _cal.CurrentMoonDay : activationDay);
        }
#endif
    }
}
