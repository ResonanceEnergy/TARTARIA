using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;

namespace Tartaria.AI
{
    /// <summary>
    /// Patrol + line-of-sight behavior for Reset Scout antagonists in Moon 1
    /// (Echohaven). Walks a <see cref="PatrolPath"/> at <see cref="walkSpeed"/>,
    /// runs a sight check every <see cref="sightCheckInterval"/> seconds, and
    /// raises a HUD banner when it spots the player.
    ///
    /// Combat hookup (attacks, damage, death) lives in the sibling
    /// <see cref="ResetScout"/> component — this script is patrol-only so the
    /// two concerns can be iterated independently. Both can live on the same
    /// GameObject; this one drives motion, that one handles aggro + attacks.
    ///
    /// Movement uses <see cref="NavMeshAgent"/> when present (preferred so the
    /// scout respects baked NavMesh borders around Echohaven), and falls back
    /// to <see cref="Transform.Translate"/> when no agent is attached.
    /// </summary>
    [DisallowMultipleComponent]
    public class ResetScoutAI : MonoBehaviour
    {
        [Header("Patrol")]
        [SerializeField] private PatrolPath patrolPath;
        [SerializeField] private float walkSpeed = 2.5f;
        [Tooltip("How tightly to face the next waypoint. Degrees per second.")]
        [SerializeField] private float turnSpeed = 240f;

        [Header("Detection")]
        [SerializeField] private float sightRange = 15f;
        [Tooltip("Forward field-of-view, in degrees. 360 = omnidirectional.")]
        [SerializeField, Range(30f, 360f)] private float sightFOV = 110f;
        [Tooltip("Seconds between line-of-sight checks. 0.5s keeps load low.")]
        [SerializeField] private float sightCheckInterval = 0.5f;
        [Tooltip("Local-space offset from the scout's root to the 'eyes' — " +
                 "used as the raycast origin so a head-height obstacle still " +
                 "blocks vision.")]
        [SerializeField] private Vector3 headOffset = new Vector3(0f, 1.7f, 0f);
        [Tooltip("Layers that count as occluders for the sight raycast. " +
                 "Leave at Default | Environment to ignore the player layer " +
                 "(otherwise the player's own collider blocks the check).")]
        [SerializeField] private LayerMask occlusionMask = ~0;
        [Tooltip("Once the scout spots the player, suppress new banner alerts " +
                 "for this many seconds so the HUD doesn't spam.")]
        [SerializeField] private float spotCooldown = 6f;

        [Header("Banner")]
        [SerializeField] private string spotTitle = "Reset Scout spotted you!";
        [SerializeField] private string spotSubtitle =
            "A Bureau agent has cataloged your anomaly. Move.";
        [SerializeField] private float spotBannerDuration = 2f;

        // --- runtime state ---------------------------------------------------
        private NavMeshAgent _agent;
        private CharacterController _cc;
        private Transform _player;
        private int _waypointIndex;
        private float _nextSightCheckAt;
        private float _spotSuppressedUntil;
        private bool _hasSpottedPlayer;

        /// <summary>True after the scout has raised a spot banner at least once.</summary>
        public bool HasSpottedPlayer => _hasSpottedPlayer;

        /// <summary>Currently-targeted waypoint index on the patrol path.</summary>
        public int CurrentWaypointIndex => _waypointIndex;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _cc = GetComponent<CharacterController>();
            if (_agent != null)
            {
                _agent.speed = walkSpeed;
                _agent.angularSpeed = turnSpeed;
                _agent.autoBraking = false;
            }
        }

        void Start()
        {
            // Snap onto the closest waypoint at boot so we don't trek across
            // the village to reach node 0 from a random spawn position.
            if (patrolPath != null && patrolPath.Count > 0)
                _waypointIndex = FindNearestWaypoint(transform.position);
        }

        void Update()
        {
            UpdatePatrol();
            UpdateSight();
        }

        // --- patrol ----------------------------------------------------------

        private void UpdatePatrol()
        {
            if (patrolPath == null || patrolPath.Count == 0) return;

            Vector3 target = patrolPath.GetWaypoint(_waypointIndex);

            if (patrolPath.HasArrived(transform.position, _waypointIndex))
            {
                _waypointIndex = patrolPath.AdvanceIndex(_waypointIndex);
                target = patrolPath.GetWaypoint(_waypointIndex);
            }

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.speed = walkSpeed;
                _agent.SetDestination(target);
                return;
            }

            // Fallback path — direct steering for prefabs that ship without
            // a NavMeshAgent or land off-mesh.
            Vector3 toTarget = target - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            Vector3 dir = toTarget.normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, lookRot, turnSpeed * Time.deltaTime);

            Vector3 step = dir * walkSpeed * Time.deltaTime;
            if (_cc != null && _cc.enabled)
            {
                step.y = -9.81f * Time.deltaTime;
                _cc.Move(step);
            }
            else
            {
                transform.Translate(step, Space.World);
            }
        }

        private int FindNearestWaypoint(Vector3 from)
        {
            if (patrolPath == null || patrolPath.Count == 0) return 0;
            int best = 0;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < patrolPath.Count; i++)
            {
                float sqr = (patrolPath.GetWaypoint(i) - from).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = i; }
            }
            return best;
        }

        // --- sight -----------------------------------------------------------

        private void UpdateSight()
        {
            if (Time.time < _nextSightCheckAt) return;
            _nextSightCheckAt = Time.time + sightCheckInterval;

            if (_player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p == null) return;
                _player = p.transform;
            }

            Vector3 eye = transform.TransformPoint(headOffset);
            // Aim at roughly chest height so we don't raycast into the player's
            // foot collider.
            Vector3 target = _player.position + Vector3.up * 1.2f;
            Vector3 toPlayer = target - eye;
            float dist = toPlayer.magnitude;
            if (dist > sightRange) return;
            if (dist <= 0.001f) return;

            Vector3 dir = toPlayer / dist;

            // FOV gate — only "see" what's in front of us.
            if (sightFOV < 360f)
            {
                Vector3 flatForward = transform.forward; flatForward.y = 0f;
                Vector3 flatDir = dir; flatDir.y = 0f;
                if (flatForward.sqrMagnitude > 0.0001f && flatDir.sqrMagnitude > 0.0001f)
                {
                    float ang = Vector3.Angle(flatForward.normalized, flatDir.normalized);
                    if (ang > sightFOV * 0.5f) return;
                }
            }

            // Raycast for occluders. If nothing hits between eye and player,
            // OR the first hit IS the player, we have line-of-sight.
            if (Physics.Raycast(eye, dir, out RaycastHit hit, dist, occlusionMask,
                                QueryTriggerInteraction.Ignore))
            {
                if (!IsPlayerHit(hit)) return;
            }

            OnPlayerSpotted();
        }

        private bool IsPlayerHit(RaycastHit hit)
        {
            if (hit.collider == null) return false;
            if (_player == null) return false;
            if (hit.collider.transform == _player) return true;
            if (hit.collider.transform.IsChildOf(_player)) return true;
            if (hit.collider.CompareTag("Player")) return true;
            return false;
        }

        /// <summary>
        /// Fires when the scout confirms a clean line-of-sight to the player.
        /// Raises a HUD banner via <see cref="GameEvents.RaiseHUDShowBanner"/>.
        /// Combat / aggro escalation is intentionally not handled here — that's
        /// the sibling <see cref="ResetScout"/> component's job.
        /// </summary>
        protected virtual void OnPlayerSpotted()
        {
            if (Time.time < _spotSuppressedUntil) return;
            _spotSuppressedUntil = Time.time + spotCooldown;
            _hasSpottedPlayer = true;

            GameEvents.RaiseHUDShowBanner(spotTitle, spotSubtitle, spotBannerDuration);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            // Sight cone (flat).
            Gizmos.color = new Color(1f, 0.5f, 0.3f, 0.6f);
            Vector3 eye = Application.isPlaying
                ? transform.TransformPoint(headOffset)
                : transform.position + headOffset;
            Gizmos.DrawWireSphere(eye, 0.1f);

            Vector3 fwd = transform.forward * sightRange;
            Quaternion left = Quaternion.Euler(0, -sightFOV * 0.5f, 0);
            Quaternion right = Quaternion.Euler(0, sightFOV * 0.5f, 0);
            Gizmos.DrawLine(eye, eye + left * fwd);
            Gizmos.DrawLine(eye, eye + right * fwd);

            if (patrolPath != null) patrolPath.DrawGizmos();
        }
#endif
    }
}
