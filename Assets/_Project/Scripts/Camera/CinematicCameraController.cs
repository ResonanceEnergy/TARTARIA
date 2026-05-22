using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Tartaria.Camera
{
    /// <summary>
    /// Cinematic Camera Controller — smooth camera paths for cutscenes.
    /// Attach to cinematic camera GameObject, define waypoints in editor OR load from CinematicWaypointSequences.
    /// Integrates with Moon content spawners for story moments.
    /// </summary>
    public class CinematicCameraController : MonoBehaviour
    {
        [Header("Waypoints (Editor or Programmatic)")]
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

// Runtime waypoints loaded externally (from Integration tier via PlaySequence)
    List<Vector3> _runtimePositions;
    List<Vector3> _runtimeLookAts;
    List<float> _runtimeDurations;

    /// <summary>
    /// Play cinematic sequence with provided waypoint data.
    /// Called by Integration tier with CinematicWaypointSequences data.
    /// </summary>
    public void PlaySequence(CinematicWaypoint[] waypoints)
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning($"[CinematicCamera] Provided sequence has <2 waypoints");
                return;
            }

            // Load runtime waypoints
            _runtimePositions = new List<Vector3>(waypoints.Length);
            _runtimeLookAts = new List<Vector3>(waypoints.Length);
            _runtimeDurations = new List<float>(waypoints.Length);

            foreach (var wp in waypoints)
            {
                _runtimePositions.Add(wp.position);
                _runtimeLookAts.Add(wp.lookAt);
                _runtimeDurations.Add(wp.duration);
            }

            _currentWaypoint = 0;
            _segmentProgress = 0f;
            _isPlaying = true;

            Debug.Log($"[CinematicCamera] Playing sequence ({waypoints.Length} waypoints)");
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
            if (!_isPlaying) return;

            // Determine source: runtime waypoints or editor Transform waypoints
            int waypointCount = (_runtimePositions != null && _runtimePositions.Count > 0)
                ? _runtimePositions.Count
                : (waypoints != null ? waypoints.Length : 0);

            if (waypointCount < 2) return;

            int nextWaypoint = _currentWaypoint + 1;
            if (nextWaypoint >= waypointCount)
            {
                // Sequence complete
                _isPlaying = false;
                _runtimePositions = null;  // Clear runtime data
                _runtimeLookAts = null;
                _runtimeDurations = null;
                Debug.Log("[CinematicCamera] Sequence complete");
                return;
            }

            // Get current segment duration
            float duration = 3f;  // Default
            if (_runtimeDurations != null && _currentWaypoint < _runtimeDurations.Count)
            {
                duration = _runtimeDurations[_currentWaypoint];
            }
            else if (waypointDurations != null && _currentWaypoint < waypointDurations.Length)
            {
                duration = waypointDurations[_currentWaypoint];
            }

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

                Vector3 startPos, endPos, lookTarget;

                if (_runtimePositions != null && _runtimePositions.Count > 0)
                {
                    // Runtime waypoints
                    startPos = _runtimePositions[_currentWaypoint];
                    endPos = _runtimePositions[nextWaypoint];
                    lookTarget = Vector3.Lerp(_runtimeLookAts[_currentWaypoint], _runtimeLookAts[nextWaypoint], t);
                }
                else
                {
                    // Editor Transform waypoints
                    startPos = waypoints[_currentWaypoint].position;
                    endPos = waypoints[nextWaypoint].position;

                    if (lookAtTarget && lookAtTransform != null)
                    {
                        lookTarget = lookAtTransform.position;
                    }
                    else
                    {
                        // Interpolate rotation from Transforms
                        Quaternion targetRot = Quaternion.Slerp(
                            waypoints[_currentWaypoint].rotation,
                            waypoints[nextWaypoint].rotation,
                            t
                        );
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothTime);
                        transform.position = Vector3.SmoothDamp(transform.position, Vector3.Lerp(startPos, endPos, t), ref _velocity, smoothTime);
                        return;  // Skip lookAt handling below
                    }
                }

                Vector3 targetPos = Vector3.Lerp(startPos, endPos, t);
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);

                // Look at target
                Vector3 lookDirection = lookTarget - transform.position;
                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothTime);
                }
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

    /// <summary>
    /// Cinematic waypoint data structure (passed from Integration tier).
    /// </summary>
    public struct CinematicWaypoint
    {
        public Vector3 position;
        public Vector3 lookAt;
        public float duration;

        public CinematicWaypoint(Vector3 pos, Vector3 look, float dur)
        {
            position = pos;
            lookAt = look;
            duration = dur;
        }
    }
}
