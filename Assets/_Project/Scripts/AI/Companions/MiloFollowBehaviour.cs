using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// MiloFollowBehaviour — Phase 1 NavMesh-driven companion follow + intro hook.
    ///
    /// Lives as a sibling component to MiloController. Auto-adds NavMeshAgent if missing.
    /// Subscribes to GameEvents.OnBuildingDiscovered → triggers MiloController.Introduce()
    /// the first time a building is discovered (Echohaven first-dome moment per
    /// docs/15_MVP_BUILD_SPEC.md § 5).
    ///
    /// Harvested concepts (followDistance=3, maxFollowDistance=10, teleport-if-stuck)
    /// from MiloControllerComplete.cs.disabled — without dragging in its 1500+ lines
    /// of trust-arc / quest / appraisal dependencies.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MiloFollowBehaviour : MonoBehaviour
    {
        [Header("Follow Tuning")]
        [SerializeField] private float followDistance = 3f;        // hold this gap when player is moving
        [SerializeField] private float maxFollowDistance = 12f;    // beyond this, teleport (handles getting stuck)
        [SerializeField] private float stoppingDistance = 1.8f;
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float runSpeed = 5.5f;
        [SerializeField] private float runIfFurtherThan = 6f;

        [Header("Idle Behaviour")]
        [SerializeField] private float idleChatterIntervalMin = 18f;
        [SerializeField] private float idleChatterIntervalMax = 35f;

        [Header("Auto-Find Player")]
        [SerializeField] private Transform playerTransform;

        private NavMeshAgent _agent;
        private float _nextChatterAt;
        private bool _introTriggered;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = walkSpeed;
            _agent.stoppingDistance = stoppingDistance;
            _agent.autoBraking = true;
            _agent.angularSpeed = 360f;
            _agent.acceleration = 16f;
        }

        void OnEnable()
        {
            GameEvents.OnBuildingDiscovered += HandleBuildingDiscovered;
            ScheduleNextChatter();
        }

        void OnDisable()
        {
            GameEvents.OnBuildingDiscovered -= HandleBuildingDiscovered;
        }

        void Start()
        {
            if (playerTransform == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) playerTransform = p.transform;
            }
        }

        void Update()
        {
            // Try once more if Start() didn't find player (player may spawn after Milo)
            if (playerTransform == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p == null) return;
                playerTransform = p.transform;
            }

            float distance = Vector3.Distance(transform.position, playerTransform.position);

            // Teleport-if-stuck — if Milo is way too far, snap to a point near the player
            if (distance > maxFollowDistance)
            {
                Vector3 snapTo = playerTransform.position - playerTransform.forward * followDistance;
                if (NavMesh.SamplePosition(snapTo, out var hit, 4f, NavMesh.AllAreas))
                {
                    _agent.Warp(hit.position);
                    Debug.Log($"[Milo] Teleport-snap (was {distance:F1}m away).");
                }
            }
            else if (distance > followDistance)
            {
                // Approach player. Run if player has run ahead.
                _agent.speed = (distance > runIfFurtherThan) ? runSpeed : walkSpeed;
                Vector3 target = playerTransform.position - (playerTransform.position - transform.position).normalized * followDistance * 0.6f;
                _agent.SetDestination(target);
            }
            else
            {
                // Within follow distance — stop and idle
                if (!_agent.isStopped && _agent.hasPath)
                {
                    _agent.ResetPath();
                }
                MaybeChatter();
            }
        }

        void MaybeChatter()
        {
            if (Time.time < _nextChatterAt) return;
            ScheduleNextChatter();
            var milo = MiloController.Instance;
            if (milo != null && milo.HasIntroduced)
            {
                milo.RequestBanter();
            }
        }

        void ScheduleNextChatter()
        {
            _nextChatterAt = Time.time + UnityEngine.Random.Range(idleChatterIntervalMin, idleChatterIntervalMax);
        }

        // ─── Building-discovery → intro hook ───

        void HandleBuildingDiscovered(string buildingId, Vector3 position)
        {
            if (_introTriggered) return;
            _introTriggered = true;
            var milo = MiloController.Instance;
            if (milo != null)
            {
                milo.Introduce();
                Debug.Log($"[Milo] Intro triggered on first building discovery: {buildingId}");
            }
        }
    }
}
