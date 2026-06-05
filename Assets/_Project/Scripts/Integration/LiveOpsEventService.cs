// LiveOpsEventService.cs — REAL implementation (NO STUBS).
// 2026-06-03: minimal LiveOps event service that fires a daily-login event
// the first time the player launches on a new calendar day. Persists last-seen
// date to PlayerPrefs. Future events register through RegisterEvent(name, condition).

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Tracks "live ops" timed events. Currently:
    ///   - DAILY_LOGIN: fired once per calendar day on first scene load.
    /// API: <see cref="OnEventFired"/> for subscribers, <see cref="RegisterEvent"/>
    /// for additional triggers, <see cref="IsEventActive"/> for query.
    /// </summary>
    public class LiveOpsEventService : MonoBehaviour
    {
        const string PREF_LAST_LOGIN = "TARTARIA_LiveOps_LastLoginISO";
        static LiveOpsEventService _instance;
        public static LiveOpsEventService Instance => _instance;

        public event Action<string> OnEventFired;
        readonly HashSet<string> _activeToday = new HashSet<string>();
        readonly Dictionary<string, Func<bool>> _customEvents = new Dictionary<string, Func<bool>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[LiveOpsEventService]");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideInHierarchy;
            _instance = go.AddComponent<LiveOpsEventService>();
        }

        void Start()
        {
            CheckDailyLogin();
            // Re-check daily at hourly cadence in case the play session crosses midnight.
            InvokeRepeating(nameof(CheckDailyLogin), 3600f, 3600f);
        }

        void CheckDailyLogin()
        {
            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string last = PlayerPrefs.GetString(PREF_LAST_LOGIN, "");
            if (last == today) return;
            PlayerPrefs.SetString(PREF_LAST_LOGIN, today);
            PlayerPrefs.Save();
            Fire("DAILY_LOGIN");
        }

        public void RegisterEvent(string id, Func<bool> condition)
        {
            if (string.IsNullOrEmpty(id) || condition == null) return;
            _customEvents[id] = condition;
        }

        public bool IsEventActive(string id) => _activeToday.Contains(id);

        public void Tick()
        {
            // Allows external callers to re-evaluate custom registrations on demand.
            foreach (var kv in _customEvents)
            {
                if (_activeToday.Contains(kv.Key)) continue;
                try { if (kv.Value()) Fire(kv.Key); } catch (Exception e) { Debug.LogWarning("[LiveOpsEventService] cond '" + kv.Key + "' threw: " + e.Message); }
            }
        }

        void Fire(string id)
        {
            if (_activeToday.Add(id))
            {
                Debug.Log("[LiveOpsEventService] Event fired: " + id);
                OnEventFired?.Invoke(id);
            }
        }
    }
}
