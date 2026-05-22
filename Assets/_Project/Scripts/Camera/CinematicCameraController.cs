using UnityEngine;
using System.Collections;

namespace Tartaria.Camera
{
    /// <summary>
    /// Cinematic Camera Controller — smooth camera paths for cutscenes.
    /// Attach to cinematic camera GameObject, define waypoints in editor.
    /// Integrates with Moon content spawners for story moments.
    /// </summary>
    public class CinematicCameraController : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] Transform[] waypoints;
        [SerializeField] float[] waypointDurations;  // Seconds per segment

        [Header("Settings")]
        [SerializeField] float smoothTime = 0.3f;
        [SerializeField] AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] bool lookAtTarget;
        [SerializeField] Transform lookAtTransform;

        Vector3 _velocity;
        int _currentWaypoint;
        float _segmentProgress;
        bool _isPlaying;

        /// <summary>
        /// Play cinematic sequence from start to end.
        /// </summary>
        public void PlayCinematic()
        {
            if (waypoints == null || waypoints.Length < 2)
            {
                Debug.LogWarning("[CinematicCamera] Need at least 2 waypoints");
                return;
            }

            _currentWaypoint = 0;
            _segmentProgress = 0f;
            _isPlaying = true;

            Debug.Log($"[CinematicCamera] Playing {waypoints.Length} waypoint sequence");
        }

        /// <summary>
        /// Stop cinematic and return control.
        /// </summary>
        public void StopCinematic()
        {
            _isPlaying = false;
            Debug.Log("[CinematicCamera] Stopped");
        }

        void Update()
        {
            if (!_isPlaying || waypoints == null || waypoints.Length < 2) return;

            int nextWaypoint = _currentWaypoint + 1;
            if (nextWaypoint >= waypoints.Length)
            {
                // Sequence complete
                _isPlaying = false;
                Debug.Log("[CinematicCamera] Sequence complete");
                return;
            }

            // Get current segment duration
            float duration = (waypointDurations != null && _currentWaypoint < waypointDurations.Length)
                ? waypointDurations[_currentWaypoint]
                : 3f;  // Default 3 seconds per segment

            _segmentProgress += Time.deltaTime / duration;

            if (_segmentProgress >= 1f)
            {
                // Move to next segment
                _currentWaypoint = nextWaypoint;
                _segmentProgress = 0f;
            }
            else
            {
                // Interpolate position
                float t = moveCurve.Evaluate(_segmentProgress);
                Vector3 targetPos = Vector3.Lerp(waypoints[_currentWaypoint].position, waypoints[nextWaypoint].position, t);
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);

                // Interpolate rotation
                Quaternion targetRot;
                if (lookAtTarget && lookAtTransform != null)
                {
                    targetRot = Quaternion.LookRotation(lookAtTransform.position - transform.position);
                }
                else
                {
                    targetRot = Quaternion.Slerp(waypoints[_currentWaypoint].rotation, waypoints[nextWaypoint].rotation, t);
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothTime);
            }
        }

        /// <summary>
        /// Snap camera to waypoint (for testing).
        /// </summary>
        public void SnapToWaypoint(int index)
        {
            if (waypoints == null || index < 0 || index >= waypoints.Length) return;

            transform.position = waypoints[index].position;
            transform.rotation = waypoints[index].rotation;

            Debug.Log($"[CinematicCamera] Snapped to waypoint {index}");
        }

        void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            // Draw waypoint path
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);
                }
            }

            // Draw last waypoint
            if (waypoints[waypoints.Length - 1] != null)
            {
                Gizmos.DrawWireSphere(waypoints[waypoints.Length - 1].position, 0.5f);
            }
        }
    }
}
