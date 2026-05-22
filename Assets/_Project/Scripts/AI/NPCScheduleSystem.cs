using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace Tartaria.AI
{
    /// <summary>
    /// NPCScheduleSystem — time-based NPC activity scheduling for day/night cycles.
    /// NPCs follow daily routines: work, socialize, sleep based on hour of day.
    /// Integrates with DayNightCycle for time queries.
    /// 
    /// Schedule Format:
    /// - 06:00-12:00 → Work (go to workplace waypoint)
    /// - 12:00-14:00 → Lunch (go to tavern/fountain)
    /// - 14:00-18:00 → Work
    /// - 18:00-22:00 → Socialize (wander town square)
    /// - 22:00-06:00 → Sleep (go home, disable pathfinding)
    /// 
    /// Usage:
    /// - Attach to NPC GameObject with NavMeshAgent
    /// - Define schedule entries in inspector
    /// - NPCScheduleSystem.Instance broadcasts time changes, NPCs react
    /// 
    /// GDD refs: §05 (Living World), §01 (Echo Haven Immersion)
    /// </summary>
    public class NPCScheduleSystem : MonoBehaviour
    {
        public static NPCScheduleSystem Instance { get; private set; }

        [Header("Time Settings")]
        [SerializeField] float updateInterval = 5f;  // Check schedule every 5s

        public event System.Action<int> OnHourChanged;  // Fired when game hour advances

        Gameplay.DayNightCycle _dayNightCycle;
        int _lastHour = -1;
        float _updateTimer;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            _dayNightCycle = Gameplay.DayNightCycle.Instance;
            if (_dayNightCycle == null)
            {
                Debug.LogWarning("[NPCSchedule] DayNightCycle not found, NPC schedules disabled");
                enabled = false;
                return;
            }

            Debug.Log("[NPCSchedule] NPC schedule system initialized");
        }

        void Update()
        {
            if (_dayNightCycle == null) return;

            _updateTimer += Time.deltaTime;

            if (_updateTimer >= updateInterval)
            {
                _updateTimer = 0f;
                CheckTimeUpdate();
            }
        }

        void CheckTimeUpdate()
        {
            int currentHour = _dayNightCycle.GetCurrentHour();

            if (currentHour != _lastHour)
            {
                _lastHour = currentHour;
                OnHourChanged?.Invoke(currentHour);

                Debug.Log($"[NPCSchedule] Hour changed to {currentHour:D2}:00");
            }
        }

        /// <summary>
        /// Get current game hour (0-23).
        /// </summary>
        public int GetCurrentHour()
        {
            return _dayNightCycle?.GetCurrentHour() ?? 12;
        }

        /// <summary>
        /// Check if current time is within a range (inclusive).
        /// </summary>
        public bool IsTimeBetween(int startHour, int endHour)
        {
            int current = GetCurrentHour();

            if (endHour < startHour)
            {
                // Wraps midnight (e.g. 22:00-06:00)
                return current >= startHour || current < endHour;
            }
            else
            {
                return current >= startHour && current < endHour;
            }
        }
    }

    /// <summary>
    /// NPCSchedule component — attach to NPCs for daily routine behavior.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCSchedule : MonoBehaviour
    {
        [Header("Schedule Entries")]
        [SerializeField] ScheduleEntry[] schedule;

        [Header("References")]
        [SerializeField] Transform homeWaypoint;
        [SerializeField] Transform workWaypoint;
        [SerializeField] Transform socialWaypoint;

        NavMeshAgent _agent;
        ScheduleEntry _currentActivity;
        Transform _currentDestination;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        void OnEnable()
        {
            if (NPCScheduleSystem.Instance != null)
            {
                NPCScheduleSystem.Instance.OnHourChanged += OnHourChanged;
            }
        }

        void OnDisable()
        {
            if (NPCScheduleSystem.Instance != null)
            {
                NPCScheduleSystem.Instance.OnHourChanged -= OnHourChanged;
            }
        }

        void Start()
        {
            // Initialize with current time
            if (NPCScheduleSystem.Instance != null)
            {
                OnHourChanged(NPCScheduleSystem.Instance.GetCurrentHour());
            }
        }

        void OnHourChanged(int newHour)
        {
            // Find matching schedule entry
            foreach (var entry in schedule)
            {
                if (NPCScheduleSystem.Instance.IsTimeBetween(entry.startHour, entry.endHour))
                {
                    if (_currentActivity.activity != entry.activity)
                    {
                        _currentActivity = entry;
                        ExecuteActivity(entry);
                    }
                    return;
                }
            }
        }

        void ExecuteActivity(ScheduleEntry entry)
        {
            Debug.Log($"[NPCSchedule] {gameObject.name} starting activity: {entry.activity} ({entry.startHour:D2}:00-{entry.endHour:D2}:00)");

            switch (entry.activity)
            {
                case NPCActivity.Work:
                    if (workWaypoint != null)
                    {
                        GoToWaypoint(workWaypoint);
                    }
                    break;

                case NPCActivity.Socialize:
                    if (socialWaypoint != null)
                    {
                        GoToWaypoint(socialWaypoint);
                    }
                    break;

                case NPCActivity.Sleep:
                    if (homeWaypoint != null)
                    {
                        GoToWaypoint(homeWaypoint);
                    }
                    // Disable pathfinding when sleeping (performance optimization)
                    _agent.isStopped = true;
                    break;

                case NPCActivity.Idle:
                    _agent.isStopped = true;
                    break;

                case NPCActivity.Wander:
                    // Continue wandering behavior from NPCAIBehavior
                    _agent.isStopped = false;
                    break;
            }
        }

        void GoToWaypoint(Transform waypoint)
        {
            if (_agent != null && waypoint != null)
            {
                _currentDestination = waypoint;
                _agent.isStopped = false;
                _agent.SetDestination(waypoint.position);
            }
        }

        void Update()
        {
            // Check if NPC reached destination and should idle
            if (_agent != null && _currentDestination != null)
            {
                if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude < 0.1f)
                    {
                        _agent.isStopped = true;
                        _currentDestination = null;

                        Debug.Log($"[NPCSchedule] {gameObject.name} reached destination, idling");
                    }
                }
            }
        }

        [System.Serializable]
        public struct ScheduleEntry
        {
            public int startHour;  // 0-23
            public int endHour;    // 0-23
            public NPCActivity activity;
        }

        public enum NPCActivity : byte
        {
            Idle = 0,
            Work = 1,
            Socialize = 2,
            Sleep = 3,
            Wander = 4
        }
    }
}
