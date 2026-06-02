using UnityEngine;

namespace Tartaria.AI
{
    /// <summary>
    /// A ScriptableObject patrol route — a sequence of world-space waypoints
    /// consumed by patrolling AI (Reset Scout, future Bureau agents, etc).
    ///
    /// Authoring: create via <c>Tartaria/AI/Patrol Path</c> in the Project window,
    /// fill the <see cref="waypoints"/> array, decide whether to <see cref="loop"/>,
    /// and tune <see cref="waypointArriveRadius"/> for how tight the AI hugs each node.
    /// </summary>
    [CreateAssetMenu(fileName = "PatrolPath", menuName = "Tartaria/AI/Patrol Path")]
    public class PatrolPath : ScriptableObject
    {
        [Tooltip("World-space patrol nodes. The AI walks them in array order.")]
        public Vector3[] waypoints;

        [Tooltip("If true, the AI wraps from the last waypoint back to the first. " +
                 "If false, the AI clamps at the last waypoint and stops advancing.")]
        public bool loop = true;

        [Tooltip("Distance under which the AI considers a waypoint reached " +
                 "and advances to the next one.")]
        public float waypointArriveRadius = 1.5f;

        /// <summary>Number of waypoints on this path. 0 if unassigned.</summary>
        public int Count => waypoints?.Length ?? 0;

        /// <summary>
        /// Returns the waypoint at <paramref name="index"/>.
        /// Indices outside [0, Count) are wrapped (when <see cref="loop"/>) or
        /// clamped (when not). Returns <see cref="Vector3.zero"/> if the path
        /// has no waypoints.
        /// </summary>
        public Vector3 GetWaypoint(int index)
        {
            if (waypoints == null || waypoints.Length == 0)
                return Vector3.zero;

            int n = waypoints.Length;
            if (loop)
            {
                // Modulo that handles negative indices correctly.
                int wrapped = ((index % n) + n) % n;
                return waypoints[wrapped];
            }

            int clamped = Mathf.Clamp(index, 0, n - 1);
            return waypoints[clamped];
        }

        /// <summary>
        /// Advances <paramref name="currentIndex"/> by one along the path,
        /// respecting <see cref="loop"/>. Returns the same index when not
        /// looping and already at the final node.
        /// </summary>
        public int AdvanceIndex(int currentIndex)
        {
            if (Count == 0) return 0;
            int next = currentIndex + 1;
            if (loop) return ((next % Count) + Count) % Count;
            return Mathf.Min(next, Count - 1);
        }

        /// <summary>True when the AI is within arrive-radius of the given waypoint.</summary>
        public bool HasArrived(Vector3 worldPos, int waypointIndex)
        {
            if (Count == 0) return true;
            Vector3 target = GetWaypoint(waypointIndex);
            // Compare on the horizontal plane only so the AI doesn't fight gravity.
            Vector2 a = new Vector2(worldPos.x, worldPos.z);
            Vector2 b = new Vector2(target.x, target.z);
            return Vector2.Distance(a, b) <= waypointArriveRadius;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only gizmo helper — call from a MonoBehaviour's OnDrawGizmosSelected
        /// to visualize the path in the scene view.
        /// </summary>
        public void DrawGizmos()
        {
            if (waypoints == null || waypoints.Length == 0) return;
            Gizmos.color = new Color(0.85f, 0.25f, 0.25f, 0.9f);
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawSphere(waypoints[i], 0.25f);
                if (i + 1 < waypoints.Length)
                    Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
            }
            if (loop && waypoints.Length > 1)
                Gizmos.DrawLine(waypoints[waypoints.Length - 1], waypoints[0]);
        }
#endif
    }
}
