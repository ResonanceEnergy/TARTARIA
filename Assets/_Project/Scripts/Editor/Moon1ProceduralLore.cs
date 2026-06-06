#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/Build Out Moon 1 Audio Lore (Lullaby / Skeleton Hum / Stinger / Taunts)
    ///
    /// Generates the 5 narrative audio clips required by docs/03 Moon 1:
    ///   1. Lirael_Lullaby_432Hz.wav        — 30s 432 Hz sine pad + harmonic 4ths
    ///   2. Skeleton_Hum_Prophecy.wav       — 18s low 80 Hz drone + breath modulation
    ///   3. Cathedral_Restoration_Stinger.wav — 6s orchestral swell, golden chord
    ///   4. Reset_Scout_Taunt.wav           — 2s clipped descending "no" warning beep
    ///   5. Milo_Blimey_Chime.wav           — 1.5s rising bell triad
    ///
    /// Per CLAUDE.md no-stubs mandate: actual procedural waveform synthesis
    /// (sine + harmonic stack + envelope), written to disk as WAV PCM-16 mono 44.1 kHz.
    /// </summary>
    public static class Moon1ProceduralLore
    {
        const int SR = 44100;
        const string OUT_DIR = "Assets/_Project/Audio/Moon1_Lore";

        [MenuItem("Tartaria/1 Build/Moon 1 — Audio Lore (Lullaby + Hum + Stinger + Taunt + Chime)", priority = 190)]
        public static void Run()
        {
            if (!Directory.Exists(OUT_DIR)) Directory.CreateDirectory(OUT_DIR);

            int built = 0;
            built += WriteWav("Lirael_Lullaby_432Hz",        GenLullaby432(30f))    ? 1 : 0;
            built += WriteWav("Skeleton_Hum_Prophecy",        GenSkeletonHum(18f))   ? 1 : 0;
            built += WriteWav("Cathedral_Restoration_Stinger", GenCathedralStinger(6f)) ? 1 : 0;
            built += WriteWav("Reset_Scout_Taunt",            GenResetScoutTaunt(2f))  ? 1 : 0;
            built += WriteWav("Milo_Blimey_Chime",            GenMiloChime(1.5f))     ? 1 : 0;

            AssetDatabase.Refresh();
            string msg = $"Generated {built}/5 audio clips at:\n{OUT_DIR}\n\n" +
                         "All are PCM-16 mono 44.1 kHz WAV.\n" +
                         "Lirael lullaby loops cleanly (3-cycle 432 Hz pad).\n" +
                         "Cathedral stinger fires on building restoration.\n" +
                         "Reset Scout + Milo chimes for combat barks.";
            Debug.Log("[Moon1ProceduralLore] " + msg);
            EditorUtility.DisplayDialog("Procedural Lore Audio", msg, "OK");
        }

        // ───────────── Generators ─────────────

        // Lirael 432 Hz lullaby: pure pad + 4th harmonic + slow envelope
        static float[] GenLullaby432(float seconds)
        {
            int n = (int)(seconds * SR);
            var s = new float[n];
            float f0 = 432f;
            float f1 = 432f * 1.5f;          // perfect fifth
            float f2 = 432f * 4f / 3f;       // perfect fourth
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SR;
                float lfo = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.15f * t);
                float pad =
                    0.40f * Mathf.Sin(2f * Mathf.PI * f0 * t) +
                    0.20f * Mathf.Sin(2f * Mathf.PI * f1 * t) * lfo +
                    0.10f * Mathf.Sin(2f * Mathf.PI * f2 * t) * (1f - lfo);
                // Gentle fade-in + fade-out
                float env = Mathf.Min(1f, t / 2f) * Mathf.Min(1f, (seconds - t) / 2f);
                s[i] = pad * env * 0.6f;
            }
            return s;
        }

        // Low rumbling skeleton hum
        static float[] GenSkeletonHum(float seconds)
        {
            int n = (int)(seconds * SR);
            var s = new float[n];
            float f0 = 80f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SR;
                float breath = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.20f * t);
                float drone =
                    0.45f * Mathf.Sin(2f * Mathf.PI * f0 * t) * breath +
                    0.20f * Mathf.Sin(2f * Mathf.PI * f0 * 1.5f * t) * (1f - breath) +
                    0.12f * Mathf.Sin(2f * Mathf.PI * f0 * 2.01f * t);
                // Modulated noise (whispered prophecy)
                float noise = (Mathf.PerlinNoise(t * 30f, 0.5f) - 0.5f) * 0.10f;
                float env = Mathf.Min(1f, t / 1.5f) * Mathf.Min(1f, (seconds - t) / 1.5f);
                s[i] = (drone + noise) * env * 0.55f;
            }
            return s;
        }

        // Cathedral restoration stinger: ascending major triad → octave swell
        static float[] GenCathedralStinger(float seconds)
        {
            int n = (int)(seconds * SR);
            var s = new float[n];
            // Major triad in C5: 523.25, 659.25, 783.99 Hz
            float[] freqs = { 523.25f, 659.25f, 783.99f, 1046.50f };
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SR;
                float sum = 0f;
                for (int k = 0; k < freqs.Length; k++)
                {
                    float kStart = k * 0.4f;
                    float gain = Mathf.Clamp01((t - kStart) / 0.3f);
                    sum += 0.20f * Mathf.Sin(2f * Mathf.PI * freqs[k] * t) * gain;
                }
                // Long swell envelope
                float env = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 2f)) *
                            Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((seconds - t) / 1.5f));
                s[i] = sum * env;
            }
            return s;
        }

        // Reset Scout taunt: descending blip-blip-warning
        static float[] GenResetScoutTaunt(float seconds)
        {
            int n = (int)(seconds * SR);
            var s = new float[n];
            // 3 descending sine pulses 880 → 660 → 440 Hz, ~0.2s each + decay
            float[] freqs = { 880f, 660f, 440f };
            float pulseDur = 0.30f;
            for (int p = 0; p < 3; p++)
            {
                int startSamp = (int)(p * 0.50f * SR);
                int endSamp = Mathf.Min(n, startSamp + (int)(pulseDur * SR));
                for (int i = startSamp; i < endSamp; i++)
                {
                    float local = (i - startSamp) / (float)SR;
                    float env = Mathf.Exp(-local * 8f);
                    s[i] += 0.35f * Mathf.Sin(2f * Mathf.PI * freqs[p] * local) * env;
                }
            }
            return s;
        }

        // Milo "blimey" rising chime: three-note triad up
        static float[] GenMiloChime(float seconds)
        {
            int n = (int)(seconds * SR);
            var s = new float[n];
            float[] freqs = { 587.33f, 740.00f, 880.00f }; // D5 F#5 A5
            for (int k = 0; k < 3; k++)
            {
                int startSamp = (int)(k * 0.18f * SR);
                int endSamp = Mathf.Min(n, startSamp + (int)(0.50f * SR));
                for (int i = startSamp; i < endSamp; i++)
                {
                    float local = (i - startSamp) / (float)SR;
                    float env = Mathf.Exp(-local * 4f);
                    // Bell partials
                    s[i] += (0.30f * Mathf.Sin(2f * Mathf.PI * freqs[k] * local)
                          + 0.15f * Mathf.Sin(2f * Mathf.PI * freqs[k] * 2.41f * local)
                          + 0.08f * Mathf.Sin(2f * Mathf.PI * freqs[k] * 4.83f * local)) * env;
                }
            }
            return s;
        }

        // ───────────── WAV writer (PCM-16 mono) ─────────────
        static bool WriteWav(string name, float[] samples)
        {
            string path = OUT_DIR + "/" + name + ".wav";
            try
            {
                using (var fs = File.Create(path))
                using (var bw = new BinaryWriter(fs))
                {
                    int byteRate = SR * 2;
                    int dataSize = samples.Length * 2;
                    bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                    bw.Write(36 + dataSize);
                    bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                    bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                    bw.Write(16);          // sub-chunk size
                    bw.Write((short)1);    // PCM
                    bw.Write((short)1);    // mono
                    bw.Write(SR);
                    bw.Write(byteRate);
                    bw.Write((short)2);    // block align
                    bw.Write((short)16);   // bits per sample
                    bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                    bw.Write(dataSize);
                    for (int i = 0; i < samples.Length; i++)
                    {
                        float c = Mathf.Clamp(samples[i], -1f, 1f);
                        short pcm = (short)(c * 32760f);
                        bw.Write(pcm);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[Moon1ProceduralLore] WAV write failed for " + name + ": " + e.Message);
                return false;
            }
        }
    }
}
#endif
