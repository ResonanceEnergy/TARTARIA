using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 11 (Spectral Moon) mini-game — Aquifer Purge.
    /// The Great Aquifer beneath the Spectral zone has 3 corruption layers
    /// that must be purged in sequence using fractal resonance patterns.
    ///
    /// Mechanics:
    ///   - 3 corruption layers (surface scum, mid-depth sludge, deep core)
    ///   - Each layer has a fractal pattern the player must trace on screen
    ///   - Accuracy determines purge effectiveness (golden ratio bonus)
    ///   - Sludge Leviathan spawns at layer 3 (boss encounter)
    ///   - Fountain chain VFX triggers when all 3 layers purged
    ///
    /// Cross-ref: docs/03C_MOON_MECHANICS_DETAILED.md §Moon 11, docs/13_MINI_GAMES.md
    /// </summary>
    [DisallowMultipleComponent]
    public class AquiferPurgeMiniGame : MonoBehaviour
    {
        public static AquiferPurgeMiniGame Instance { get; private set; }

        // ─── Constants ───────────────────────────────

        public const int TotalCorruptionLayers = 3;
        public const float GoldenRatio = 1.618034f;
        public const float TracingTolerance = 0.12f;        // normalized distance
        public const float PerfectTracingTolerance = 0.04f;
        public const float LayerPurgeDuration = 30f;         // seconds per layer
        public const float CorruptionRegrowthRate = 0.01f;   // per second if idle
        public const float RSRewardPerLayer = 8f;
        public const float RSBonusPerfectPurge = 5f;

        // ─── Fractal Pattern ─────────────────────────

        [Serializable]
        public class FractalPattern
        {
            public Vector2[] controlPoints;   // normalized 0..1 screen space
            public float scale;               // golden-ratio scaled size
            public int iterations;            // fractal depth
        }

        // ─── Layer State ─────────────────────────────

        enum LayerState : byte
        {
            Corrupted = 0,
            Purging = 1,
            Purged = 2
        }

        // ─── State ───────────────────────────────────

        readonly LayerState[] _layerStates = new LayerState[TotalCorruptionLayers];
        readonly float[] _layerPurity = new float[TotalCorruptionLayers];   // 0=corrupt, 1=pure
        readonly float[] _layerAccuracy = new float[TotalCorruptionLayers];
        int _currentLayer;
        bool _miniGameActive;
        bool _tracingActive;
        float _traceTimer;
        float _traceAccuracySum;
        int _traceSamples;
        readonly List<Vector2> _playerTrace = new();
        FractalPattern[] _patterns;

        // ─── Events ─────────────────────────────────

        public event Action<int, float> OnLayerPurged;       // layerIndex, accuracy
        public event Action OnAllLayersPurged;
        public event Action OnBossSpawned;

        // ─── Lifecycle ───────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            GenerateFractalPatterns();
        }

        void Update()
        {
            if (!_miniGameActive) return;

            if (_tracingActive)
            {
                UpdateTracing();
            }
            else
            {
                // Corruption regrowth on incomplete layers
                for (int i = 0; i < TotalCorruptionLayers; i++)
                {
                    if (_layerStates[i] == LayerState.Corrupted && _layerPurity[i] > 0f)
                    {
                        _layerPurity[i] = Mathf.Max(0f, _layerPurity[i] - CorruptionRegrowthRate * Time.deltaTime);
                    }
                }
            }
        }

        // ─── Public API ──────────────────────────────

        public void StartMiniGame()
        {
            _miniGameActive = true;
            _currentLayer = 0;
            for (int i = 0; i < TotalCorruptionLayers; i++)
            {
                _layerStates[i] = LayerState.Corrupted;
                _layerPurity[i] = 0f;
                _layerAccuracy[i] = 0f;
            }
            Debug.Log("[AquiferPurge] Mini-game started. 3 corruption layers to purge.");
        }

        public void StopMiniGame()
        {
            _miniGameActive = false;
            _tracingActive = false;
        }

        /// <summary>
        /// Begin tracing the fractal pattern for the current layer.
        /// </summary>
        public void BeginLayerTrace()
        {
            if (!_miniGameActive) return;
            if (_currentLayer >= TotalCorruptionLayers) return;
            if (_layerStates[_currentLayer] == LayerState.Purged) return;

            _tracingActive = true;
            _layerStates[_currentLayer] = LayerState.Purging;
            _traceTimer = 0f;
            _traceAccuracySum = 0f;
            _traceSamples = 0;
            _playerTrace.Clear();

            // Spawn boss at layer 3
            if (_currentLayer == 2)
            {
                OnBossSpawned?.Invoke();
                BossEncounterSystem.Instance?.SpawnBoss("sludge_leviathan");
                Debug.Log("[AquiferPurge] Layer 3 — Sludge Leviathan spawned!");
            }

            Debug.Log($"[AquiferPurge] Tracing layer {_currentLayer + 1}");
        }

        // ─── Fractal Generation ──────────────────────

        void GenerateFractalPatterns()
        {
            _patterns = new FractalPattern[TotalCorruptionLayers];
            for (int layer = 0; layer < TotalCorruptionLayers; layer++)
            {
                int pointCount = 8 + layer * 4; // more complex per layer
                var points = new Vector2[pointCount];
                float scale = 1f / Mathf.Pow(GoldenRatio, layer);

                for (int i = 0; i < pointCount; i++)
                {
                    float angle = i * Mathf.PI * 2f / pointCount * GoldenRatio;
                    float radius = scale * (0.3f + 0.2f * Mathf.Sin(angle * (layer + 1)));
                    points[i] = new Vector2(
                        0.5f + radius * Mathf.Cos(angle),
                        0.5f + radius * Mathf.Sin(angle));
                }

                _patterns[layer] = new FractalPattern
                {
                    controlPoints = points,
                    scale = scale,
                    iterations = layer + 2
                };
            }
        }

        // ─── Tracing Logic ───────────────────────────

        void UpdateTracing()
        {
            _traceTimer += Time.deltaTime;

            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 mousePos = mouse.position.ReadValue();
            Vector2 normalized = new(
                mousePos.x / Screen.width,
                mousePos.y / Screen.height);

            if (mouse.leftButton.isPressed)
            {
                _playerTrace.Add(normalized);

                // Compare against fractal pattern
                float dist = GetNearestPatternDistance(normalized, _currentLayer);
                float accuracy = 1f - Mathf.Clamp01(dist / TracingTolerance);
                _traceAccuracySum += accuracy;
                _traceSamples++;

                // Update purity based on accuracy
                float purgeRate = accuracy / LayerPurgeDuration;
                _layerPurity[_currentLayer] = Mathf.Clamp01(
                    _layerPurity[_currentLayer] + purgeRate * Time.deltaTime);
            }

            // Layer complete check
            if (_layerPurity[_currentLayer] >= 1f || _traceTimer >= LayerPurgeDuration)
            {
                CompleteLayer();
            }
        }

        float GetNearestPatternDistance(Vector2 point, int layerIndex)
        {
            if (_patterns == null || layerIndex >= _patterns.Length) return 1f;
            var pattern = _patterns[layerIndex];
            float minDist = float.MaxValue;

            for (int i = 0; i < pattern.controlPoints.Length; i++)
            {
                int next = (i + 1) % pattern.controlPoints.Length;
                float dist = DistanceToSegment(point, pattern.controlPoints[i], pattern.controlPoints[next]);
                if (dist < minDist) minDist = dist;
            }
            return minDist;
        }

        static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab));
            Vector2 closest = a + t * ab;
            return Vector2.Distance(p, closest);
        }

        void CompleteLayer()
        {
            float avgAccuracy = _traceSamples > 0 ? _traceAccuracySum / _traceSamples : 0f;
            _layerAccuracy[_currentLayer] = avgAccuracy;
            _layerStates[_currentLayer] = LayerState.Purged;
            _tracingActive = false;

            float reward = RSRewardPerLayer;
            if (avgAccuracy >= 0.95f) reward += RSBonusPerfectPurge;

            GameLoopController.Instance?.OnMiniGameCompleted(reward, "aquifer_purge");
            OnLayerPurged?.Invoke(_currentLayer, avgAccuracy);

            Debug.Log($"[AquiferPurge] Layer {_currentLayer + 1} purged! Accuracy: {avgAccuracy:P0}");

            _currentLayer++;
            if (_currentLayer >= TotalCorruptionLayers)
            {
                OnAllLayersPurged?.Invoke();
                VFXController.Instance?.SpawnAquiferPurificationCascade(transform.position);

                // Achievement
                AchievementSystem.Instance?.Unlock("M05");
                Debug.Log("[AquiferPurge] All layers purged! Fountain chain activated.");
            }
        }

        // ─── Queries ─────────────────────────────────

        public bool IsActive => _miniGameActive;
        public int CurrentLayer => _currentLayer;
        public float GetLayerPurity(int layer) =>
            layer >= 0 && layer < TotalCorruptionLayers ? _layerPurity[layer] : 0f;
        public float GetLayerAccuracy(int layer) =>
            layer >= 0 && layer < TotalCorruptionLayers ? _layerAccuracy[layer] : 0f;
        public bool AllLayersPurged => _currentLayer >= TotalCorruptionLayers;
    }
}
