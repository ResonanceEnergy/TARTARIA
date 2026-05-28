using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.LiveOps
{
    /// <summary>
    /// TelemetrySystem - Event tracking and analytics.
    /// Agent 2 requirement from Phase 5.
    /// </summary>
    public class TelemetrySystem : MonoBehaviour
    {
        public static TelemetrySystem Instance { get; private set; }

        [Header("Telemetry Stats")]
        [SerializeField] private int totalEvents = 0;
        [SerializeField] private Dictionary<string, int> eventCounts = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // Subscribe to game events
            GameEvents.OnBuildingRestored += (id) => TrackEvent("building_restored", new() { { "buildingId", id } });
            GameEvents.OnEnemyKilled += (args) => TrackEvent("enemy_killed", new() { { "enemyType", args.enemyType } });
            GameEvents.OnQuestCompleted += (id) => TrackEvent("quest_complete", new() { { "questId", id } });
        }

        public void TrackEvent(string eventName, Dictionary<string, string> parameters = null)
        {
            totalEvents++;
            
            if (!eventCounts.ContainsKey(eventName))
                eventCounts[eventName] = 0;
            eventCounts[eventName]++;

            string paramStr = parameters != null ? string.Join(", ", parameters) : "";
            Debug.Log($"[Telemetry] {eventName} ({paramStr})");
        }

        public int GetEventCount(string eventName) => eventCounts.GetValueOrDefault(eventName, 0);
        public Dictionary<string, int> GetAllEventCounts() => new Dictionary<string, int>(eventCounts);
    }
}
