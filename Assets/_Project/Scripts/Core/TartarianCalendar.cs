using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Core
{
    // Minimal stubs for live-ops types (full definitions live in Integration/Gameplay for Phase 3+)
    // These allow Core to compile standalone for Echohaven/Moon 1 vertical slice testing.
    public enum MoonBeat { Discovery, Restoration, Conflict, Climax, Revelation }

    [System.Serializable]
    public struct FairEntry
    {
        public string Name;
        public string Builder;
        public int Votes;
        public int RsInvested;
        public string SubmitTimeIso;
    }

    /// <summary>
    /// TartarianCalendar — minimal implementation for Moon 1 Echohaven vertical slice playtesting.
    /// Full live-ops (Milo deals, World's Fair, companion trust schedules, 17th Hour) are Phase 3+.
    /// Provides day/moon tracking, basic beat, save hooks. Safe no-op for advanced calls.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-60)]
    public class TartarianCalendar : MonoBehaviour
    {
        public static TartarianCalendar Instance { get; private set; }

        [Header("Moon 1 Defaults")]
        [SerializeField] int startingMoon = 1; // Magnetic Moon for Echohaven
        [SerializeField] int startingDay = 1;

        int _currentMoon = 1;
        int _currentMoonDay = 1;
        float _dayProgress; // 0-1 within current day

        // Events for basic integration
        public event Action<int> OnDayAdvanced;
        public event Action<MoonBeat> OnBeatAdvanced;
#pragma warning disable CS0067 // Event never used - future integration
        public event Action OnSeventeenthHour;
#pragma warning restore CS0067

        // Live ops stubs (no-op for Moon 1 readiness)
        public int FairEntryCount => 0;
        public bool IsWorldsFairActive => false;
        public bool CanVoteInFairToday => false;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            _currentMoon = startingMoon;
            _currentMoonDay = startingDay;
            Debug.Log($"[TartarianCalendar] Moon 1 Echohaven slice: Day {_currentMoonDay} of Moon {_currentMoon}");
        }

        void Update()
        {
            // Simple day progression for playtest (real time scaled)
            _dayProgress += Time.deltaTime / 120f; // ~2 min real day for testing
            if (_dayProgress >= 1f)
            {
                _dayProgress = 0f;
                AdvanceDay();
            }

            // 17th hour stub (hour 17 of 24)
            if (Mathf.FloorToInt(_dayProgress * 24) == 17)
            {
                // fire once per day
            }
        }

        void AdvanceDay()
        {
            _currentMoonDay++;
            if (_currentMoonDay > 28)
            {
                _currentMoonDay = 1;
                // _currentMoon ++; keep at 1 for slice
            }
            OnDayAdvanced?.Invoke(_currentMoonDay);

            // Sprint 12 P2.L1 — canonical day-change event for cross-assembly subscribers
            // (Moon1LiraelDay25Gate-style consumers that bind through GameEvents instead
            // of the calendar singleton). Mirrors the local OnDayAdvanced fan-out so a
            // future migration can deprecate the calendar event without losing coverage.
            GameEvents.RaiseDayChanged(_currentMoonDay);

            var beat = GetBeatForDay(_currentMoonDay);
            OnBeatAdvanced?.Invoke(beat);

            Debug.Log($"[Calendar] Day advanced to {_currentMoonDay} (beat: {beat})");
        }

        public MoonBeat CurrentBeat => GetBeatForDay(_currentMoonDay);

        public static MoonBeat GetBeatForDay(int day)
        {
            if (day <= 5) return MoonBeat.Discovery;
            if (day <= 12) return MoonBeat.Restoration;
            if (day <= 18) return MoonBeat.Conflict;
            if (day <= 24) return MoonBeat.Climax;
            return MoonBeat.Revelation;
        }

        public int CurrentMoon => _currentMoon;
        public int CurrentMoonDay => _currentMoonDay;
        public float DayProgress => _dayProgress;

        // Save support for GameLoop / SaveManager (minimal)
        public void SetDay(int day, int moon = 1)
        {
            _currentMoonDay = Mathf.Clamp(day, 1, 28);
            _currentMoon = moon;
            _dayProgress = 0f;
        }

        // Live-ops stubs (safe for Moon 1 playtest — no crash, just log)
        public object GetTodaysMiloDailyDeal() => null;
        public void SubmitPavilionToFair(string name, int rs) { Debug.Log("[Calendar] Fair stub: submission ignored (Moon 1 slice)"); }
        public bool VoteInFair(int index) { return false; }
        public List<FairEntry> GetFairLeaderboard() => new List<FairEntry>();
        public void ScheduleEvent(string key, Action cb) { /* no-op */ }
        public void ForceSetMoonDay(int moon, int day) { SetDay(day, moon); }

        // For GameLoop save/load wiring
        public CalendarSaveData GetSaveData() => new CalendarSaveData { currentMoon = _currentMoon, currentDay = _currentMoonDay };
        public void LoadSaveData(CalendarSaveData data)
        {
            if (data != null) SetDay(data.currentDay, data.currentMoon);
        }

        [Serializable]
        public class CalendarSaveData
        {
            public int currentMoon = 1;
            public int currentDay = 1;
        }
    }
}
