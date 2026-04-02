using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Creates placeholder AudioClip assets for the vertical slice.
    /// Silent mode: 1-sample clips (serialize-field assignment only).
    /// Tone mode: actual sine-wave tones at 432 Hz base (Tartarian harmonic).
    /// Menu: Tartaria > Build Assets > Audio Stubs
    /// </summary>
    public static class AudioStubFactory
    {
        const string MusicPath = "Assets/_Project/Audio/Music";
        const string SFXPath = "Assets/_Project/Audio/SFX";
        const int SampleRate = 44100;
        const float BaseFreq = 432f; // Tartarian harmonic base

        [MenuItem("Tartaria/Build Assets/Audio Stubs", false, 22)]
        public static void BuildAudioStubs()
        {
            BuildAudibleStubs();
        }

        [MenuItem("Tartaria/Build Assets/Audio Stubs (Audible Tones)", false, 23)]
        public static void BuildAudibleStubs()
        {
            EnsureDirectories();

            // Music beds — slow pads at harmonic intervals
            CreateToneClip(MusicPath, "MusicBed_Exploration_Low", 5f,
                BaseFreq * 0.5f, 0.15f, ToneShape.Pad);
            CreateToneClip(MusicPath, "MusicBed_Exploration_High", 5f,
                BaseFreq, 0.2f, ToneShape.Pad);
            CreateToneClip(MusicPath, "MusicBed_Combat_Low", 5f,
                BaseFreq * 0.75f, 0.25f, ToneShape.Pulse);
            CreateToneClip(MusicPath, "MusicBed_Combat_High", 5f,
                BaseFreq * 1.5f, 0.3f, ToneShape.Pulse);

            // Stingers — short melodic hits
            CreateToneClip(SFXPath, "Stinger_Discovery", 2f,
                BaseFreq * 1.618f, 0.4f, ToneShape.Chime); // φ interval
            CreateToneClip(SFXPath, "Stinger_Restoration", 3f,
                BaseFreq * 2f, 0.35f, ToneShape.Chime);
            CreateToneClip(SFXPath, "Stinger_CombatStart", 1.5f,
                BaseFreq * 0.667f, 0.5f, ToneShape.Hit);
            CreateToneClip(SFXPath, "Stinger_ZoneShift", 2f,
                BaseFreq * 1.25f, 0.3f, ToneShape.Chime);
            CreateToneClip(SFXPath, "Stinger_AetherWake", 2f,
                BaseFreq * 1.618f, 0.4f, ToneShape.Pad);

            // SFX — short distinctive tones
            CreateToneClip(SFXPath, "SFX_ResonancePulse", 0.5f,
                BaseFreq, 0.6f, ToneShape.Ping);
            CreateToneClip(SFXPath, "SFX_HarmonicStrike", 0.5f,
                BaseFreq * 2f, 0.7f, ToneShape.Hit);
            CreateToneClip(SFXPath, "SFX_FrequencyShield", 0.8f,
                BaseFreq * 1.5f, 0.3f, ToneShape.Pad);
            CreateToneClip(SFXPath, "SFX_GolemSpawn", 1f,
                BaseFreq * 0.5f, 0.5f, ToneShape.Hit);
            CreateToneClip(SFXPath, "SFX_GolemDeath", 1f,
                BaseFreq * 0.333f, 0.4f, ToneShape.Chime);
            CreateToneClip(SFXPath, "SFX_TuningSuccess", 0.5f,
                BaseFreq * 1.618f, 0.5f, ToneShape.Ping);
            CreateToneClip(SFXPath, "SFX_TuningFail", 0.5f,
                BaseFreq * 0.8f, 0.4f, ToneShape.Hit);
            CreateToneClip(SFXPath, "SFX_BuildingEmerge", 3f,
                BaseFreq * 0.75f, 0.3f, ToneShape.Pad);
            CreateToneClip(SFXPath, "SFX_MoteCollect", 0.3f,
                BaseFreq * 3f, 0.4f, ToneShape.Ping);
            CreateToneClip(SFXPath, "SFX_Footstep", 0.2f,
                80f, 0.3f, ToneShape.Hit);
            CreateToneClip(SFXPath, "SFX_Anastasia_Whisper", 1f,
                BaseFreq * 2f, 0.1f, ToneShape.Pad);
            CreateToneClip(SFXPath, "SFX_Solidification", 4f,
                BaseFreq * 0.5f, 0.2f, ToneShape.Pad);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AudioStubs] All audio clips created with audible tones (432 Hz base).");
        }

        enum ToneShape { Ping, Hit, Pad, Pulse, Chime }

        static void CreateToneClip(string folder, string clipName, float duration,
            float freq, float volume, ToneShape shape)
        {
            string path = $"{folder}/{clipName}.asset";
            if (AssetDatabase.LoadAssetAtPath<AudioClip>(path) != null) return;

            int samples = Mathf.Max(1, (int)(SampleRate * duration));
            var clip = AudioClip.Create(clipName, samples, 1, SampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float normTime = (float)i / samples; // 0→1

                // Base oscillator
                float osc;
                switch (shape)
                {
                    case ToneShape.Ping:
                        osc = Mathf.Sin(2f * Mathf.PI * freq * t);
                        osc += 0.3f * Mathf.Sin(2f * Mathf.PI * freq * 2.618f * t); // φ² harmonic
                        osc *= Mathf.Exp(-t * 8f); // Fast decay
                        break;
                    case ToneShape.Hit:
                        osc = Mathf.Sin(2f * Mathf.PI * freq * t * (1f - normTime * 0.5f)); // Pitch drop
                        osc *= Mathf.Exp(-t * 4f);
                        break;
                    case ToneShape.Pad:
                        osc = Mathf.Sin(2f * Mathf.PI * freq * t);
                        osc += 0.5f * Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t);
                        float env = Mathf.Min(normTime * 10f, 1f) * Mathf.Min((1f - normTime) * 5f, 1f);
                        osc *= env;
                        break;
                    case ToneShape.Pulse:
                        osc = Mathf.Sin(2f * Mathf.PI * freq * t);
                        osc *= (0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 2f * t)); // 2 Hz tremolo
                        float pEnv = Mathf.Min(normTime * 8f, 1f) * Mathf.Min((1f - normTime) * 4f, 1f);
                        osc *= pEnv;
                        break;
                    case ToneShape.Chime:
                        osc = Mathf.Sin(2f * Mathf.PI * freq * t);
                        osc += 0.6f * Mathf.Sin(2f * Mathf.PI * freq * 1.618f * t); // φ harmonic
                        osc += 0.3f * Mathf.Sin(2f * Mathf.PI * freq * 2.618f * t); // φ² harmonic
                        osc *= Mathf.Exp(-t * 2f);
                        break;
                    default:
                        osc = 0f;
                        break;
                }

                data[i] = Mathf.Clamp(osc * volume, -1f, 1f);
            }

            clip.SetData(data, 0);
            AssetDatabase.CreateAsset(clip, path);
        }

        static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Audio"))
                AssetDatabase.CreateFolder("Assets/_Project", "Audio");
            if (!AssetDatabase.IsValidFolder(MusicPath))
                AssetDatabase.CreateFolder("Assets/_Project/Audio", "Music");
            if (!AssetDatabase.IsValidFolder(SFXPath))
                AssetDatabase.CreateFolder("Assets/_Project/Audio", "SFX");
        }
    }
}
