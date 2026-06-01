#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Bulk procedural asset generation for Moon 1:
    ///  - SFX wav clips
    ///  - Particle texture PNGs (6 masks)
    ///  - Skybox material (procedural golden-hour gradient)
    ///  - Custom shader compile diagnostic
    ///  - Hero building persistent post-restoration markers
    ///
    /// Per CLAUDE.md "no stubs" — every menu writes real bytes to disk.
    /// </summary>
    public static class Moon1AssetGenerators
    {
        const string SFX_DIR     = "Assets/_Project/Audio/SFX_Generated";
        const string PARTICLE_DIR = "Assets/_Project/Textures/ParticleMasks";
        const string SKYBOX_PATH = "Assets/_Project/Materials/Moon1_GoldenHourSkybox.mat";

        // ─────────────────────────────────────────────────────────────
        // 5. SFX CLIPS — write real .wav files
        // ─────────────────────────────────────────────────────────────

        [MenuItem("Tartaria/4 Generate Art/SFX Library", priority = 460)]
        public static void GenerateSFX()
        {
            EnsureDir(SFX_DIR);
            int written = 0;
            written += WriteSFX("Tuning_Success",     GenerateChord(new[] { 523.25f, 659.25f, 783.99f }, 1.2f, 0.6f));
            written += WriteSFX("Tuning_Fail",        GenerateBuzz(110f, 0.7f, 0.5f));
            written += WriteSFX("Collect_Coin",       GenerateBlip(880f, 1318f, 0.18f, 0.5f));
            written += WriteSFX("Mud_Splat",          GenerateSplat(0.4f, 0.6f));
            written += WriteSFX("Crystal_Tune_Activate", GenerateBell(440f, 1.5f, 0.55f));
            written += WriteSFX("Brazier_Light",      GenerateWhoosh(0.7f, 0.4f));
            written += WriteSFX("Pause_Open",         GenerateBlip(330f, 220f, 0.15f, 0.35f));
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("SFX Library", "Wrote " + written + " WAV files to " + SFX_DIR, "OK");
        }

        static int WriteSFX(string name, float[] samples)
        {
            string path = SFX_DIR + "/" + name + ".wav";
            string full = Path.Combine(Directory.GetCurrentDirectory(), path);
            WriteWav(full, samples, 44100);
            return 1;
        }

        // ─────────────────────────────────────────────────────────────
        // Procedural SFX synthesis
        // ─────────────────────────────────────────────────────────────

        static float[] GenerateChord(float[] freqs, float dur, float vol)
        {
            int sr = 44100; int n = (int)(sr * dur);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float v = 0f;
                foreach (var f in freqs) v += Mathf.Sin(2f * Mathf.PI * f * t);
                float env = Mathf.Min(1f, t / 0.05f) * Mathf.Exp(-2.5f * t);
                data[i] = (v / freqs.Length) * vol * env;
            }
            return data;
        }

        static float[] GenerateBuzz(float baseHz, float dur, float vol)
        {
            int sr = 44100; int n = (int)(sr * dur);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * baseHz * t));
                float env = Mathf.Exp(-3.5f * t);
                data[i] = square * vol * env;
            }
            return data;
        }

        static float[] GenerateBlip(float startHz, float endHz, float dur, float vol)
        {
            int sr = 44100; int n = (int)(sr * dur);
            var data = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float k = t / dur;
                float hz = Mathf.Lerp(startHz, endHz, k);
                phase += 2f * Mathf.PI * hz / sr;
                float env = Mathf.Min(1f, k / 0.05f) * (1f - k);
                data[i] = Mathf.Sin(phase) * vol * env;
            }
            return data;
        }

        static float[] GenerateSplat(float dur, float vol)
        {
            int sr = 44100; int n = (int)(sr * dur);
            var data = new float[n];
            var rng = new System.Random(42);
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                lp = lp * 0.85f + noise * 0.15f;
                float env = Mathf.Exp(-6f * t);
                data[i] = lp * vol * env;
            }
            return data;
        }

        static float[] GenerateBell(float baseHz, float dur, float vol)
        {
            int sr = 44100; int n = (int)(sr * dur);
            var data = new float[n];
            float[] partials = { 1f, 2.01f, 2.99f, 4.01f };
            float[] gains    = { 0.6f, 0.3f, 0.15f, 0.08f };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float v = 0f;
                for (int p = 0; p < partials.Length; p++)
                    v += gains[p] * Mathf.Sin(2f * Mathf.PI * baseHz * partials[p] * t);
                float env = Mathf.Min(1f, t / 0.005f) * Mathf.Exp(-1.8f * t);
                data[i] = v * vol * env;
            }
            return data;
        }

        static float[] GenerateWhoosh(float dur, float vol)
        {
            int sr = 44100; int n = (int)(sr * dur);
            var data = new float[n];
            var rng = new System.Random(7);
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float cutoff = Mathf.Lerp(0.04f, 0.20f, t / dur);
                lp = lp * (1f - cutoff) + noise * cutoff;
                float env = Mathf.Sin(Mathf.PI * (t / dur));
                data[i] = lp * vol * env;
            }
            return data;
        }

        // WAV writer (PCM16 mono)
        static void WriteWav(string path, float[] samples, int sampleRate)
        {
            using (var fs = File.Create(path))
            using (var bw = new BinaryWriter(fs))
            {
                int dataBytes = samples.Length * 2;
                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(36 + dataBytes);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                bw.Write(16); bw.Write((short)1); bw.Write((short)1);
                bw.Write(sampleRate); bw.Write(sampleRate * 2);
                bw.Write((short)2); bw.Write((short)16);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                bw.Write(dataBytes);
                foreach (var s in samples)
                {
                    short v = (short)(Mathf.Clamp(s, -1f, 1f) * 32767f);
                    bw.Write(v);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 10. PARTICLE TEXTURES — write 6 PNG masks
        // ─────────────────────────────────────────────────────────────

        [MenuItem("Tartaria/4 Generate Art/Particle Textures", priority = 470)]
        public static void GenerateParticleTextures()
        {
            EnsureDir(PARTICLE_DIR);
            WriteMask("Flame",       (u, v) => SoftFlame(u, v));
            WriteMask("Smoke",       (u, v) => Smoke(u, v));
            WriteMask("Spark",       (u, v) => Spark(u, v));
            WriteMask("Dust",        (u, v) => DustMote(u, v));
            WriteMask("WaterDroplet",(u, v) => Droplet(u, v));
            WriteMask("MudSplat",    (u, v) => MudSplat(u, v));
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Particle Textures", "Wrote 6 PNG masks to " + PARTICLE_DIR, "OK");
        }

        delegate float MaskFn(float u, float v);

        static void WriteMask(string name, MaskFn fn)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size;
                float v = (y + 0.5f) / size;
                float a = Mathf.Clamp01(fn(u, v));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            tex.Apply();
            string path = PARTICLE_DIR + "/" + name + ".png";
            File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), path), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static float Radial(float u, float v) { float dx = u - 0.5f, dy = v - 0.5f; return Mathf.Sqrt(dx*dx+dy*dy) * 2f; }

        static float SoftFlame(float u, float v)
        {
            float r = Radial(u, v);
            float teardrop = Mathf.Clamp01(1f - r) * (0.4f + 0.6f * v);
            return teardrop * teardrop;
        }
        static float Smoke(float u, float v)
        {
            float r = Radial(u, v);
            return Mathf.Clamp01(1f - r) * 0.85f * Mathf.PerlinNoise(u * 4f, v * 4f);
        }
        static float Spark(float u, float v)
        {
            float r = Radial(u, v);
            return r < 0.08f ? 1f : Mathf.Max(0f, 0.2f - r) * 5f;
        }
        static float DustMote(float u, float v)
        {
            float r = Radial(u, v);
            return Mathf.SmoothStep(0.5f, 0f, r);
        }
        static float Droplet(float u, float v)
        {
            float r = Radial(u, v);
            float core = Mathf.SmoothStep(0.35f, 0.15f, r);
            float rim  = (r > 0.32f && r < 0.40f) ? 0.4f : 0f;
            return Mathf.Clamp01(core + rim);
        }
        static float MudSplat(float u, float v)
        {
            float r = Radial(u, v);
            float ang = Mathf.Atan2(v - 0.5f, u - 0.5f);
            float lobe = Mathf.Abs(Mathf.Sin(ang * 5f)) * 0.15f;
            return Mathf.Clamp01(0.7f - r + lobe);
        }

        // ─────────────────────────────────────────────────────────────
        // 6. SKYBOX — procedural golden-hour gradient material
        // ─────────────────────────────────────────────────────────────

        [MenuItem("Tartaria/4 Generate Art/Golden-Hour Skybox", priority = 480)]
        public static void GenerateSkybox()
        {
            var shader = Shader.Find("Skybox/Procedural");
            if (shader == null)
            {
                EditorUtility.DisplayDialog("Skybox", "Skybox/Procedural shader not found.", "OK");
                return;
            }
            var mat = new Material(shader);
            mat.SetColor("_SkyTint",     new Color(1.0f, 0.78f, 0.50f));
            mat.SetColor("_GroundColor", new Color(0.35f, 0.28f, 0.18f));
            mat.SetFloat("_AtmosphereThickness", 1.65f);
            mat.SetFloat("_Exposure",    1.25f);
            mat.SetFloat("_SunSize",     0.06f);
            mat.SetFloat("_SunSizeConvergence", 5f);
            AssetDatabase.CreateAsset(mat, SKYBOX_PATH);
            RenderSettings.skybox = mat;
            DynamicGI.UpdateEnvironment();
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Skybox",
                "Created procedural golden-hour skybox at " + SKYBOX_PATH + "\nAssigned to current scene.", "OK");
        }

        // ─────────────────────────────────────────────────────────────
        // 7. CUSTOM SHADER COMPILE DIAGNOSTIC
        // ─────────────────────────────────────────────────────────────

        [MenuItem("Tartaria/7 Diagnose/Custom Shaders (Tartaria/*)", priority = 770)]
        public static void DiagnoseShaders()
        {
            string[] shaderPaths =
            {
                "Assets/_Project/Shaders/AetherFlow.shader",
                "Assets/_Project/Shaders/AetherFog.shader",
                "Assets/_Project/Shaders/AetherVein.shader",
                "Assets/_Project/Shaders/AetherVeinStone.shader",
                "Assets/_Project/Shaders/ColorblindCorrection.shader",
            };
            var sb = new System.Text.StringBuilder();
            int ok = 0, bad = 0;
            foreach (var p in shaderPaths)
            {
                var s = AssetDatabase.LoadAssetAtPath<Shader>(p);
                if (s == null) { sb.AppendLine("[MISSING] " + p); bad++; continue; }
                if (!s.isSupported) { sb.AppendLine("[UNSUPPORTED] " + p); bad++; continue; }
                // Probe by trying a test material
                var testMat = new Material(s);
                if (testMat.shader == null || testMat.shader.name.Contains("Hidden/InternalErrorShader"))
                {
                    sb.AppendLine("[ERROR-FALLBACK] " + p);
                    bad++;
                }
                else
                {
                    sb.AppendLine("[OK] " + s.name + "  (" + p + ")");
                    ok++;
                }
                Object.DestroyImmediate(testMat);
            }
            string header = "Compile diag: " + ok + " ok, " + bad + " bad of " + shaderPaths.Length + ".\n\n";
            EditorUtility.DisplayDialog("Custom Shader Diag", header + sb.ToString(), "OK");
            Debug.Log("[Moon1AssetGenerators] " + header + sb.ToString());
        }

        // ─────────────────────────────────────────────────────────────
        // 9. HERO BUILDING PERSISTENT POST-RESTORATION MARKERS
        //    Adds a child GameObject to each Echohaven prefab that
        //    activates on Restored state via Animator parameter trigger.
        // ─────────────────────────────────────────────────────────────

        [MenuItem("Tartaria/3 Wire/Add Hero Post-State Markers", priority = 340)]
        public static void AddHeroPostStateMarkers()
        {
            int updated = 0;
            updated += AddMarker("Assets/_Project/Prefabs/Buildings/Echohaven_StarDome.prefab",
                "RoseWindowCymaticProjection",  new Color(0.95f, 0.78f, 0.30f), 4f);
            updated += AddMarker("Assets/_Project/Prefabs/Buildings/Echohaven_HarmonicFountain.prefab",
                "PureWaterColumn",              new Color(0.55f, 0.85f, 0.95f), 5f);
            updated += AddMarker("Assets/_Project/Prefabs/Buildings/Echohaven_CrystalSpire.prefab",
                "MercuryBallRotor",             new Color(0.85f, 0.85f, 0.90f), 6f);
            EditorUtility.DisplayDialog("Post-State Markers",
                "Added persistent post-restoration child markers to " + updated + " hero buildings.", "OK");
        }

        static int AddMarker(string prefabPath, string markerName, Color tint, float height)
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), prefabPath))) return 0;
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null) return 0;
            // Don't double-add
            if (root.transform.Find(markerName) != null)
            {
                PrefabUtility.UnloadPrefabContents(root);
                return 0;
            }
            var marker = new GameObject(markerName);
            marker.transform.SetParent(root.transform);
            marker.transform.localPosition = new Vector3(0f, height, 0f);
            marker.SetActive(false); // BuildingRestorationCeremony enables on restore

            // Visual: glow light + emissive disc
            var light = marker.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = tint;
            light.intensity = 2.5f;
            light.range = 12f;

            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "GlowDisc";
            disc.transform.SetParent(marker.transform);
            disc.transform.localPosition = Vector3.zero;
            disc.transform.localScale = new Vector3(2.5f, 0.05f, 2.5f);
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                var mat = new Material(shader);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.85f);
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", tint * 2.0f);
                disc.GetComponent<Renderer>().sharedMaterial = mat;
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            return 1;
        }

        static void EnsureDir(string projectRelative)
        {
            string full = Path.Combine(Directory.GetCurrentDirectory(), projectRelative);
            if (!Directory.Exists(full)) Directory.CreateDirectory(full);
            AssetDatabase.Refresh();
        }
    }
}
#endif
