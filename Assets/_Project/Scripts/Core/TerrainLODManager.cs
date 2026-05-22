using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Terrain LOD Manager — dynamically adjusts terrain detail distance based on performance.
    /// Reduces grass/tree draw distance when FPS drops below threshold.
    /// Attach to Terrain GameObject or create as singleton.
    /// </summary>
    public class TerrainLODManager : MonoBehaviour
    {
        [Header("Performance Targets")]
        [SerializeField] float targetFPS = 60f;
        [SerializeField] float minDetailDistance = 50f;
        [SerializeField] float maxDetailDistance = 250f;

        [Header("Settings")]
        [SerializeField] Terrain[] terrains;
        [SerializeField] float adjustInterval = 2f;  // Check every 2 seconds
        [SerializeField] bool enableDynamicLOD = true;

        float _checkTimer;
        float _currentDetailDistance;
        float[] _fpssamples = new float[30];
        int _sampleIndex;

        void Awake()
        {
            if (terrains == null || terrains.Length == 0)
            {
                terrains = FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            }

            _currentDetailDistance = maxDetailDistance;
            ApplyDetailDistance();
        }

        void Update()
        {
            if (!enableDynamicLOD) return;

            // Sample FPS
            _fpssamples[_sampleIndex] = 1f / Time.deltaTime;
            _sampleIndex = (_sampleIndex + 1) % _fpssamples.Length;

            _checkTimer += Time.deltaTime;
            if (_checkTimer >= adjustInterval)
            {
                _checkTimer = 0f;
                AdjustLOD();
            }
        }

        void AdjustLOD()
        {
            // Calculate average FPS
            float avgFPS = 0f;
            for (int i = 0; i < _fpssamples.Length; i++)
            {
                avgFPS += _fpssamples[i];
            }
            avgFPS /= _fpssamples.Length;

            // Adjust detail distance based on FPS
            if (avgFPS < targetFPS - 5f)
            {
                // FPS too low, reduce detail
                _currentDetailDistance = Mathf.Max(minDetailDistance, _currentDetailDistance * 0.9f);
                ApplyDetailDistance();
                Debug.Log($"[TerrainLOD] FPS {avgFPS:F1} < target, reducing detail to {_currentDetailDistance:F0}m");
            }
            else if (avgFPS > targetFPS + 10f && _currentDetailDistance < maxDetailDistance)
            {
                // FPS comfortable, increase detail
                _currentDetailDistance = Mathf.Min(maxDetailDistance, _currentDetailDistance * 1.1f);
                ApplyDetailDistance();
                Debug.Log($"[TerrainLOD] FPS {avgFPS:F1} > target, increasing detail to {_currentDetailDistance:F0}m");
            }
        }

        void ApplyDetailDistance()
        {
            if (terrains == null) return;

            foreach (var terrain in terrains)
            {
                if (terrain != null)
                {
                    terrain.detailObjectDistance = _currentDetailDistance;
                    terrain.treeDistance = _currentDetailDistance * 2f;  // Trees visible farther
                }
            }
        }

        public void SetTargetFPS(float fps)
        {
            targetFPS = Mathf.Clamp(fps, 30f, 120f);
        }

        public void SetDynamicLODEnabled(bool enabled)
        {
            enableDynamicLOD = enabled;
            if (!enabled)
            {
                // Reset to max detail
                _currentDetailDistance = maxDetailDistance;
                ApplyDetailDistance();
            }
        }
    }
}
