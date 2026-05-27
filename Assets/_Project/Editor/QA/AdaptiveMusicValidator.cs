using UnityEngine;
using UnityEditor;
using Tartaria.Audio;

namespace Tartaria.Editor.QA
{
    /// <summary>
    /// Validation script for AdaptiveMusicController RS 50 crossfade fix.
    /// Tests layer blending at critical RS thresholds (especially 48-55).
    /// </summary>
    public class AdaptiveMusicValidator : EditorWindow
    {
        float _testRS = 45f;
        bool _autoSweep = false;
        float _sweepSpeed = 2f;

        [MenuItem("TARTARIA/QA/Adaptive Music Validator")]
        static void ShowWindow()
        {
            GetWindow<AdaptiveMusicValidator>("Music Validator");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("ADAPTIVE MUSIC RS 50 CROSSFADE VALIDATOR", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This tool validates the RS 50 crossfade fix:\n" +
                "• Layer 1 should fade OUT from 50-55\n" +
                "• Schumann should fade IN from 48-100\n" +
                "• No silence gaps or volume spikes at RS 50",
                MessageType.Info
            );

            EditorGUILayout.Space();

            // Manual RS slider
            _testRS = EditorGUILayout.Slider("Test RS Value", _testRS, 0f, 100f);

            if (GUILayout.Button("Set RS (Runtime Only)"))
            {
                if (Application.isPlaying && AdaptiveMusicController.Instance != null)
                {
                    AdaptiveMusicController.Instance.SetResonanceScore(_testRS);
                    Debug.Log($"[MusicValidator] Set RS to {_testRS:F1}");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Enter Play Mode first!", "OK");
                }
            }

            EditorGUILayout.Space();

            // Auto-sweep controls
            EditorGUILayout.LabelField("AUTO-SWEEP TEST", EditorStyles.boldLabel);
            _autoSweep = EditorGUILayout.Toggle("Enable Auto-Sweep", _autoSweep);
            _sweepSpeed = EditorGUILayout.Slider("Sweep Speed", _sweepSpeed, 0.5f, 10f);

            if (_autoSweep && Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Auto-sweep active: RS 45 → 55 → 45 (looping)", MessageType.Warning);
            }

            EditorGUILayout.Space();

            // Critical threshold tests
            EditorGUILayout.LabelField("QUICK THRESHOLD TESTS", EditorStyles.boldLabel);

            if (GUILayout.Button("Test RS 48 (Schumann start)"))
                SetRS(48f);

            if (GUILayout.Button("Test RS 49 (Before transition)"))
                SetRS(49f);

            if (GUILayout.Button("Test RS 50 (CRITICAL POINT)"))
                SetRS(50f);

            if (GUILayout.Button("Test RS 51 (After transition)"))
                SetRS(51f);

            if (GUILayout.Button("Test RS 55 (L1 fully faded out)"))
                SetRS(55f);

            EditorGUILayout.Space();

            // Volume analysis
            if (Application.isPlaying && AdaptiveMusicController.Instance != null)
            {
                EditorGUILayout.LabelField("LIVE VOLUME ANALYSIS", EditorStyles.boldLabel);

                var controller = AdaptiveMusicController.Instance;
                var l0 = GetLayerVolume("Music_L0_Ambient");
                var l1 = GetLayerVolume("Music_L1_Melodic");
                var l2 = GetLayerVolume("Music_L2_Orchestral");
                var l3 = GetLayerVolume("Music_L3_Triumphant");
                var sch = GetLayerVolume("Music_Schumann");

                EditorGUILayout.LabelField($"L0 (Ambient):    {l0:F3}", GetStyle(l0));
                EditorGUILayout.LabelField($"L1 (Melodic):    {l1:F3}", GetStyle(l1));
                EditorGUILayout.LabelField($"L2 (Orchestral): {l2:F3}", GetStyle(l2));
                EditorGUILayout.LabelField($"L3 (Triumphant): {l3:F3}", GetStyle(l3));
                EditorGUILayout.LabelField($"Schumann:        {sch:F3}", GetStyle(sch));

                float totalVolume = l0 + l1 + l2 + l3 + sch;
                EditorGUILayout.LabelField($"TOTAL VOLUME:    {totalVolume:F3}",
                    totalVolume > 2.5f ? EditorStyles.boldLabel : EditorStyles.label);

                if (totalVolume > 2.5f)
                {
                    EditorGUILayout.HelpBox("⚠ Total volume > 2.5 — possible layer congestion!", MessageType.Warning);
                }

                // Crossfade validation at RS 48-55
                if (_testRS >= 48f && _testRS <= 55f)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("RS 48-55 CROSSFADE ZONE", EditorStyles.boldLabel);

                    if (l1 > 0.05f && sch > 0.05f)
                    {
                        float crossfadeSum = l1 + sch;
                        EditorGUILayout.LabelField($"L1 + Schumann:   {crossfadeSum:F3}");

                        if (crossfadeSum > 0.95f && crossfadeSum < 1.05f)
                        {
                            EditorGUILayout.HelpBox("✓ Crossfade is smooth (sum ≈ 1.0)", MessageType.Info);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see live volume analysis", MessageType.Info);
            }

            Repaint();
        }

        void Update()
        {
            if (_autoSweep && Application.isPlaying)
            {
                // Sweep RS from 45 to 55 and back
                float t = Mathf.PingPong(Time.realtimeSinceStartup * _sweepSpeed * 0.1f, 1f);
                _testRS = Mathf.Lerp(45f, 55f, t);

                if (AdaptiveMusicController.Instance != null)
                {
                    AdaptiveMusicController.Instance.SetResonanceScore(_testRS);
                }
            }
        }

        void SetRS(float rs)
        {
            if (Application.isPlaying && AdaptiveMusicController.Instance != null)
            {
                _testRS = rs;
                AdaptiveMusicController.Instance.SetResonanceScore(rs);
                Debug.Log($"[MusicValidator] Set RS to {rs:F1}");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Enter Play Mode first!", "OK");
            }
        }

        float GetLayerVolume(string layerName)
        {
            var controller = AdaptiveMusicController.Instance;
            if (controller == null) return 0f;

            var layerTransform = controller.transform.Find(layerName);
            if (layerTransform == null) return 0f;

            var audioSource = layerTransform.GetComponent<AudioSource>();
            return audioSource != null ? audioSource.volume : 0f;
        }

        GUIStyle GetStyle(float volume)
        {
            if (volume < 0.01f) return EditorStyles.label;
            if (volume < 0.5f) return EditorStyles.label;
            return EditorStyles.boldLabel;
        }
    }
}
