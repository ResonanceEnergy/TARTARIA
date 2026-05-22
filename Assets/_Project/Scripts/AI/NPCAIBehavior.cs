using UnityEngine;
using UnityEngine.AI;

namespace Tartaria.AI
{
    /// <summary>
    /// Basic NPC AI Behavior — simple wander + idle patrol for ambient NPCs.
    /// Attach to NPC GameObjects for basic autonomous movement in zones.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCAIBehavior : MonoBehaviour
    {
        [Header("Patrol Config")]
        [SerializeField] float wanderRadius = 20f;
        [SerializeField] float idleTimeMin = 3f;
        [SerializeField] float idleTimeMax = 8f;
        [SerializeField] bool enableWander = true;

        NavMeshAgent _agent;
        float _idleTimer;
        bool _isIdle = true;
        Vector3 _homePosition;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _homePosition = transform.position;
        }

        void Start()
        {
            if (_agent != null && enableWander)
            {
                _idleTimer = Random.Range(idleTimeMin, idleTimeMax);
            }
        }

        void Update()
        {
            if (!enableWander || _agent == null) return;

            if (_isIdle)
            {
                _idleTimer -= Time.deltaTime;
                if (_idleTimer <= 0f)
                {
                    WanderToRandomPoint();
                }
            }
            else
            {
                // Check if reached destination
                if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                    {
                        // Arrived, start idle
                        _isIdle = true;
                        _idleTimer = Random.Range(idleTimeMin, idleTimeMax);
                    }
                }
            }
        }

        void WanderToRandomPoint()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += _homePosition;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                _isIdle = false;
            }
            else
            {
                // Failed to find valid point, wait and retry
                _idleTimer = 1f;
            }
        }

        public void SetWanderEnabled(bool enabled)
        {
            enableWander = enabled;
            if (!enabled && _agent != null)
            {
                _agent.ResetPath();
                _isIdle = true;
            }
        }

        public void SetHomePosition(Vector3 position)
        {
            _homePosition = position;
        }

        void OnDrawGizmosSelected()
        {
            // Draw wander radius
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(_homePosition, wanderRadius);
        }
    }
}
