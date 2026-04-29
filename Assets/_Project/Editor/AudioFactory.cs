using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates procedural ambient audio (wind drone + harmonic music + footstep)
    /// as .wav assets. Then attaches AudioSources to the scene that play them.
    /// 100% generated in code — no external audio dependencies.
    /// </summary>
    public static class AudioFactory
    {
        const string AudioDir = "Assets/_Project/Audio";
        const int Sample = 44100;

        public static void BuildAudioAssets()
        {
            EnsureDir(AudioDir);
            WriteWav($"{AudioDir}/Ambient_Wind.wav", BuildWindLoop(20f));
            WriteWav($"{AudioDir}/Ambient_HarmonicChoir.wav", BuildHarmonicChoir(32f));
            WriteWav($"{AudioDir}/Footstep.wav", BuildFootstep(0.18f));
            WriteWav($"{AudioDir}/Building_Hum.wav", BuildBuildingHum(8f));
            AssetDatabase.Refresh();
            Debug.Log($"[Tartaria] 4 procedural audio clips written to {AudioDir}");
        }

        public static void AddAmbienceToScene()
        {
            var existing = GameObject.Find("AudioAmbience");
            if (existing != null) Object.DestroyImmediate(existing);

            var root = new GameObject("AudioAmbience");
            AddLoop(root, "Wind",  $"{AudioDir}/Ambient_Wind.wav", 0.35f, 1.0f);
            AddLoop(root, "Choir", $"{AudioDir}/Ambient_HarmonicChoir.wav", 0.28f, 0.98f);
            Debug.Log("[Tartaria] Audio ambience (wind + choir loops) attached to scene.");
        }

        static void AddLoop(GameObject parent, string name, string clipPath, float volume, float pitch)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            if (clip == null) { Debug.LogWarning($"[Tartaria] Missing clip: {clipPath}"); return; }
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.loop = true;
            src.volume = volume;
            src.pitch = pitch;
            src.spatialBlend = 0f; // 2D
            src.playOnAwake = true;
        }

        // ── Synthesis ─────────────────────────────────────────────────────────
        static float[] BuildWindLoop(float seconds)
        {
            int n = (int)(seconds * Sample);
            var samples = new float[n];
            var rng = new System.Random(8675);
            // Brown noise + slow LFO for "gust" envelope
            float prev = 0f;
            for (int i = 0; i < n; i++)
            {
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                prev = (prev + 0.02f * white) * 0.985f; // brown noise
                float t = (float)i / Sample;
                float lfo = 0.5f + 0.5f * Mathf.Sin(t * 0.4f) * Mathf.Sin(t * 0.13f + 1.7f);
                samples[i] = prev * 4.0f * lfo;
            }
            CrossfadeLoop(samples, 0.3f);
            Normalize(samples, 0.85f);
            return samples;
        }

        static float[] BuildHarmonicChoir(float seconds)
        {
            int n = (int)(seconds * Sample);
            var samples = new float[n];
            // Pad chord (C minor 9): C, Eb, G, Bb, D
            float[] freqs = { 130.81f, 155.56f, 196.00f, 233.08f, 293.66f };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Sample;
                float s = 0f;
                for (int v = 0; v < freqs.Length; v++)
                {
                    float det = 1f + Mathf.Sin(t * 0.7f + v) * 0.0015f; // gentle detune
                    float voice =
                        Mathf.Sin(2 * Mathf.PI * freqs[v] * det * t)
                      + 0.4f * Mathf.Sin(2 * Mathf.PI * freqs[v] * 2f * det * t)
                      + 0.2f * Mathf.Sin(2 * Mathf.PI * freqs[v] * 3f * det * t);
                    s += voice / freqs.Length;
                }
                // Slow swell envelope
                float env = 0.6f + 0.4f * Mathf.Sin(t * 0.25f);
                samples[i] = s * 0.35f * env;
            }
            CrossfadeLoop(samples, 1.0f);
            Normalize(samples, 0.7f);
            return samples;
        }

        static float[] BuildFootstep(float seconds)
        {
            int n = (int)(seconds * Sample);
            var samples = new float[n];
            var rng = new System.Random(12);
            // Burst of filtered noise with quick decay
            float prev = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Sample;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                prev = prev * 0.7f + white * 0.3f; // simple low-pass
                float env = Mathf.Exp(-t * 30f);
                samples[i] = prev * env * 0.9f;
            }
            return samples;
        }

        static float[] BuildBuildingHum(float seconds)
        {
            int n = (int)(seconds * Sample);
            var samples = new float[n];
            float baseFreq = 110f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Sample;
                float s = Mathf.Sin(2 * Mathf.PI * baseFreq * t)
                       + 0.5f * Mathf.Sin(2 * Mathf.PI * baseFreq * 1.5f * t)
                       + 0.25f * Mathf.Sin(2 * Mathf.PI * baseFreq * 2.0f * t);
                samples[i] = s * 0.25f;
            }
            CrossfadeLoop(samples, 0.5f);
            return samples;
        }

        static void CrossfadeLoop(float[] s, float fadeSec)
        {
            int fade = Mathf.Min(s.Length / 3, (int)(fadeSec * Sample));
            for (int i = 0; i < fade; i++)
            {
                float a = (float)i / fade;
                int tail = s.Length - fade + i;
                float v = s[i] * a + s[tail] * (1f - a);
                s[i] = v;
                s[tail] = v;
            }
        }

        static void Normalize(float[] s, float target)
        {
            float peak = 0f;
            for (int i = 0; i < s.Length; i++)
                peak = Mathf.Max(peak, Mathf.Abs(s[i]));
            if (peak < 1e-6f) return;
            float gain = target / peak;
            for (int i = 0; i < s.Length; i++) s[i] *= gain;
        }

        // ── WAV writer ────────────────────────────────────────────────────────
        static void WriteWav(string path, float[] samples)
        {
            int byteCount = samples.Length * 2;
            using var fs = new FileStream(path, FileMode.Create);
            using var bw = new BinaryWriter(fs);
            // RIFF header
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + byteCount);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            // fmt chunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);            // PCM chunk size
            bw.Write((short)1);      // PCM
            bw.Write((short)1);      // mono
            bw.Write(Sample);
            bw.Write(Sample * 2);    // byte rate
            bw.Write((short)2);      // block align
            bw.Write((short)16);     // bits per sample
            // data chunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(byteCount);
            for (int i = 0; i < samples.Length; i++)
            {
                short s = (short)Mathf.Clamp(samples[i] * short.MaxValue, short.MinValue, short.MaxValue);
                bw.Write(s);
            }
        }

        static void EnsureDir(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    string parentParent = Path.GetDirectoryName(parent).Replace("\\", "/");
                    AssetDatabase.CreateFolder(parentParent, Path.GetFileName(parent));
                }
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
