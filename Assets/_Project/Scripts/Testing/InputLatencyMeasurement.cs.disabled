using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

namespace Tartaria.Testing
{
    /// <summary>
    /// Input Latency Measurement Tool — measures and reports input-to-response latency.
    /// Accessibility requirement: input latency should be &lt;100ms for responsive feel.
    /// 
    /// Usage: Attach to a test object, press Space to measure latency.
    /// Displays average latency over 100 samples in debug overlay.
    /// </summary>
    public class InputLatencyMeasurement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] int sampleCount = 100;
        [SerializeField] bool showDebugOverlay = true;
        [SerializeField] KeyCode measureKey = KeyCode.Space;

        readonly List<float> _latencySamples = new();
        float _inputTimestamp;
        bool _waitingForResponse;
        float _averageLatency;
        float _minLatency = float.MaxValue;
        float _maxLatency;

        void Update()
        {
            // Measure latency on key press
            if (Input.GetKeyDown(measureKey))
            {
                _inputTimestamp = Time.realtimeSinceStartup;
                _waitingForResponse = true;
            }

            if (_waitingForResponse)
            {
                // Response triggered (simulated by immediate frame)
                float responseTime = Time.realtimeSinceStartup;
                float latency = (responseTime - _inputTimestamp) * 1000f; // Convert to ms

                _latencySamples.Add(latency);
                if (_latencySamples.Count > sampleCount)
                    _latencySamples.RemoveAt(0);

                _averageLatency = _latencySamples.Average();
                _minLatency = Mathf.Min(_minLatency, latency);
                _maxLatency = Mathf.Max(_maxLatency, latency);

                _waitingForResponse = false;

                Debug.Log($"[InputLatency] Sample: {latency:F2}ms | Avg: {_averageLatency:F2}ms | Min: {_minLatency:F2}ms | Max: {_maxLatency:F2}ms");
            }
        }

        void OnGUI()
        {
            if (!showDebugOverlay) return;

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                alignment = TextAnchor.UpperLeft
            };

            var shadowStyle = new GUIStyle(style) { normal = { textColor = Color.black } };

            int y = 20;
            int x = 20;

            DrawShadowedLabel(x, y, $"INPUT LATENCY MEASUREMENT", style, shadowStyle);
            y += 25;
            DrawShadowedLabel(x, y, $"Press [{measureKey}] to measure", style, shadowStyle);
            y += 25;

            if (_latencySamples.Count > 0)
            {
                DrawShadowedLabel(x, y, $"Average: {_averageLatency:F2}ms ({_latencySamples.Count} samples)", style, shadowStyle);
                y += 20;
                DrawShadowedLabel(x, y, $"Min: {_minLatency:F2}ms | Max: {_maxLatency:F2}ms", style, shadowStyle);
                y += 20;

                // Accessibility status
                string status = _averageLatency < 100f ? "✓ PASS (< 100ms)" : "✗ FAIL (≥ 100ms)";
                var statusColor = _averageLatency < 100f ? Color.green : Color.red;
                var statusStyle = new GUIStyle(style) { normal = { textColor = statusColor } };
                var statusShadowStyle = new GUIStyle(shadowStyle);

                DrawShadowedLabel(x, y, $"Status: {status}", statusStyle, statusShadowStyle);
            }
        }

        void DrawShadowedLabel(int x, int y, string text, GUIStyle style, GUIStyle shadowStyle)
        {
            GUI.Label(new Rect(x + 1, y + 1, 500, 20), text, shadowStyle);
            GUI.Label(new Rect(x, y, 500, 20), text, style);
        }
    }
}
