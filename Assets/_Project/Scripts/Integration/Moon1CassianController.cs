using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Cassian controller — narrative-driven walk + face logic for the boss intro beat.
    ///
    /// Owns the NavMeshAgent drive on the Cassian GameObject during the Moon 1
    /// completion cinematic. Driven externally by Tartaria.AI.CassianBossIntro
    /// (via reflection to keep the AI assembly free of Integration deps).
    ///
    /// Contract (CassianBossIntro relies on these):
    ///   - public Vector3 cathedralTarget;
    ///   - public void WalkToCathedral();
    ///
    /// 2026-06-02 no-debt mandate compliance:
    ///   - No silent fail on missing NavMeshAgent — logs error with hierarchy path
    ///   - No silent fallback on off-mesh — logs warning with position
    ///   - No TODO stub body; both paths (agent + transform-lerp) are real movement
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1CassianController : MonoBehaviour
    {
        [Header("Boss intro target")]
        [Tooltip("World-space position Cassian walks to during the Moon 1 boss intro beat. " +
                 "Set by Tartaria.AI.CassianBossIntro just before WalkToCathedral() is called.")]
        public Vector3 cathedralTarget;

        [Header("Movement tuning")]
        [SerializeField] float walkSpeed = 2.2f;
        [SerializeField] float stoppingDistance = 2.5f;
        [SerializeField] float fallbackLerpDuration = 2f;

        NavMeshAgent _agent;
        Coroutine _activeWalk;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// Drive Cassian toward <see cref="cathedralTarget"/>.
        /// Preferred path: NavMeshAgent.SetDestination. Fallback: coroutine that
        /// lerps Transform.position over <see cref="fallbackLerpDuration"/> seconds.
        /// </summary>
        public void WalkToCathedral()
        {
            if (cathedralTarget == Vector3.zero)
            {
                Debug.LogWarning($"[Moon1CassianController] WalkToCathedral called with cathedralTarget == Vector3.zero on '{BuildHierarchyPath(transform)}'. The caller (likely CassianBossIntro) did not set the target. Proceeding anyway — Cassian will walk toward world origin.");
            }

            // Stop any prior walk so the new one wins.
            if (_activeWalk != null)
            {
                StopCoroutine(_activeWalk);
                _activeWalk = null;
            }

            if (_agent == null) _agent = GetComponent<NavMeshAgent>();

            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.speed = walkSpeed;
                _agent.stoppingDistance = stoppingDistance;
                _agent.isStopped = false;
                bool ok = _agent.SetDestination(cathedralTarget);
                if (!ok)
                {
                    Debug.LogWarning($"[Moon1CassianController] NavMeshAgent.SetDestination({cathedralTarget}) returned false on '{BuildHierarchyPath(transform)}' — target may be off-mesh. Falling back to transform lerp.");
                    _activeWalk = StartCoroutine(LerpToTarget());
                }
                else
                {
                    Debug.Log($"[Moon1CassianController] NavMeshAgent driving '{name}' to {cathedralTarget} at speed {walkSpeed}.");
                }
                return;
            }

            // Agent missing or not on mesh — log loud and use the lerp fallback.
            if (_agent == null)
            {
                Debug.LogWarning($"[Moon1CassianController] No NavMeshAgent on '{BuildHierarchyPath(transform)}'. Using transform-lerp fallback toward {cathedralTarget}. Add a NavMeshAgent to the Cassian prefab to get proper steering.");
            }
            else
            {
                Debug.LogWarning($"[Moon1CassianController] NavMeshAgent on '{BuildHierarchyPath(transform)}' is not on the NavMesh (position {transform.position}). Bake NavMesh covering Cassian's spawn. Using transform-lerp fallback toward {cathedralTarget}.");
            }

            _activeWalk = StartCoroutine(LerpToTarget());
        }

        IEnumerator LerpToTarget()
        {
            Vector3 start = transform.position;
            Vector3 end = new Vector3(cathedralTarget.x, start.y, cathedralTarget.z); // keep Y stable
            Vector3 dir = end - start;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion startRot = transform.rotation;
                Quaternion endRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                float t = 0f;
                while (t < fallbackLerpDuration)
                {
                    t += Time.deltaTime;
                    float k = Mathf.Clamp01(t / fallbackLerpDuration);
                    transform.position = Vector3.Lerp(start, end, k);
                    transform.rotation = Quaternion.Slerp(startRot, endRot, Mathf.Clamp01(k * 2f));
                    yield return null;
                }
                transform.position = end;
                transform.rotation = endRot;
            }
            _activeWalk = null;
        }

        static string BuildHierarchyPath(Transform t)
        {
            if (t == null) return "<null>";
            var path = t.name;
            var p = t.parent;
            while (p != null) { path = p.name + "/" + path; p = p.parent; }
            return path;
        }
    }
}
