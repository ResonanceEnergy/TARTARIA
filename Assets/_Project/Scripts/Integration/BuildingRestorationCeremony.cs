using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// BuildingRestorationCeremony — listens to GameEvents.OnBuildingRestoredTyped
    /// and plays the per-building visual + audio flourishes that docs/03 Days 6–12
    /// calls for.
    ///
    /// Per the spec:
    ///   - Dome restored      → rose window cymatic projection on the floor
    ///   - Fountain restored  → pure water font trickling back to life (particles + audio)
    ///   - Spire restored     → spire placement ceremony (blue-white sparks climb)
    ///
    /// Spawn this as a singleton GameObject (`RestorationCeremony`) in the scene
    /// via the master bootstrap; one MonoBehaviour handles all 3 buildings.
    ///
    /// Everything is procedural so we don't need bespoke prefabs — just URP/Lit
    /// primitives + ParticleSystem + AudioSource. Replace with hand-authored
    /// VFX prefabs in Phase 2.
    /// </summary>
    public class BuildingRestorationCeremony : MonoBehaviour
    {
        const string DOME_ID     = "echohaven_stardome";
        const string FOUNTAIN_ID = "echohaven_harmonicfountain";
        const string SPIRE_ID    = "echohaven_crystalspire";

        void OnEnable()  { GameEvents.OnBuildingRestoredTyped += HandleRestored; }
        void OnDisable() { GameEvents.OnBuildingRestoredTyped -= HandleRestored; }

        void HandleRestored(BuildingRestoredEventArgs args)
        {
            if (args == null) return;
            switch (args.buildingId)
            {
                case DOME_ID:     StartCoroutine(DomeRoseWindowCymatic(args.position)); break;
                case FOUNTAIN_ID: StartCoroutine(FountainPureWater(args.position));     break;
                case SPIRE_ID:    StartCoroutine(SpirePlacementSparks(args.position));  break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Rose window cymatic projection
        // ─────────────────────────────────────────────────────────────────────
        IEnumerator DomeRoseWindowCymatic(Vector3 buildingPos)
        {
            Debug.Log("[Ceremony] Rose window cymatic projection — Dome");
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "RoseWindow_Cymatic_Decal";
            disc.transform.position = buildingPos + new Vector3(0f, 0.05f, 0f); // just above ground
            disc.transform.localScale = new Vector3(8f, 0.02f, 8f);
            Object.Destroy(disc.GetComponent<Collider>());

            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            var tex = GenerateCymaticTexture(256);
            var rend = disc.GetComponent<Renderer>();
            if (rend != null && urpLit != null)
            {
                var mat = new Material(urpLit);
                mat.SetTexture("_BaseMap", tex);
                mat.SetColor("_BaseColor", new Color(0.95f, 0.78f, 0.20f, 1f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.85f, 0.65f, 0.10f) * 1.5f);
                rend.sharedMaterial = mat;
            }

            // Spin slowly + ease-fade-in
            float life = 14f, t = 0f;
            while (t < life)
            {
                t += Time.deltaTime;
                disc.transform.Rotate(0f, 8f * Time.deltaTime, 0f, Space.World);
                yield return null;
            }

            // Linger then destroy
            Object.Destroy(disc);
        }

        Texture2D GenerateCymaticTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float cx = size * 0.5f, cy = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float r = Mathf.Sqrt(dx * dx + dy * dy) / (size * 0.5f);
                    float theta = Mathf.Atan2(dy, dx);
                    // Chladni-style figure: radial rings × 8-fold petal
                    float ring = Mathf.Sin(r * 18f);
                    float petals = Mathf.Cos(theta * 8f);
                    float v = Mathf.Abs(ring * petals) * Mathf.Exp(-r * 1.4f);
                    byte a = (byte)Mathf.Clamp(v * 255f * 1.6f, 0f, 255f);
                    pixels[y * size + x] = new Color32(255, 220, 120, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false);
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Pure water font trickle
        // ─────────────────────────────────────────────────────────────────────
        IEnumerator FountainPureWater(Vector3 buildingPos)
        {
            Debug.Log("[Ceremony] Pure water font — Fountain");
            var go = new GameObject("PureWaterFont_FX");
            go.transform.position = buildingPos + new Vector3(0f, 1.5f, 0f);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.6f;
            main.startSpeed = 3.5f;
            main.startSize = 0.08f;
            main.startColor = new Color(0.65f, 0.85f, 0.95f, 1f);
            main.maxParticles = 400;
            var em = ps.emission; em.rateOverTime = 60f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 12f; sh.radius = 0.1f;

            // Make the particle material URP-friendly
            var psr = go.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                var partShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (partShader != null) psr.sharedMaterial = new Material(partShader);
            }

            // Trickling water audio — generated sine sweep
            var src = go.AddComponent<AudioSource>();
            src.clip = GenerateWaterTrickleClip();
            src.loop = true;
            src.spatialBlend = 1f;
            src.minDistance = 2f;
            src.maxDistance = 18f;
            src.volume = 0.35f;
            src.Play();

            // Light flicker so the font feels alive
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.6f, 0.85f, 1.0f);
            light.intensity = 1.2f;
            light.range = 6f;

            // Hold the FX for 30s then fade
            float life = 30f, t = 0f;
            while (t < life)
            {
                t += Time.deltaTime;
                if (light != null) light.intensity = 0.8f + 0.6f * Mathf.PerlinNoise(t * 3f, 0f);
                yield return null;
            }

            // Fade volume + destroy
            float fade = 2f, ft = 0f;
            while (ft < fade)
            {
                ft += Time.deltaTime;
                if (src != null) src.volume = Mathf.Lerp(0.35f, 0f, ft / fade);
                yield return null;
            }
            Object.Destroy(go);
        }

        AudioClip GenerateWaterTrickleClip()
        {
            const int sr = 44100;
            const float dur = 2.5f;
            int samples = (int)(sr * dur);
            var clip = AudioClip.Create("WaterTrickle", samples, 1, sr, false);
            var data = new float[samples];
            var rng = new System.Random(20260530);
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                // White noise + low-freq tremolo
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.25f;
                float bandpass = Mathf.Sin(2f * Mathf.PI * 800f * t) * 0.08f;
                float trem = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 1.7f * t);
                data[i] = (noise + bandpass) * trem;
            }
            clip.SetData(data, 0);
            return clip;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Spire placement — blue-white sparks climb
        // ─────────────────────────────────────────────────────────────────────
        IEnumerator SpirePlacementSparks(Vector3 buildingPos)
        {
            Debug.Log("[Ceremony] Spire placement — blue-white sparks climb");
            var go = new GameObject("SpireSparks_FX");
            go.transform.position = buildingPos;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.2f;
            main.startSpeed = 4.5f;
            main.startSize = 0.12f;
            main.startColor = new Color(0.7f, 0.85f, 1.0f, 1f);
            main.maxParticles = 600;
            main.gravityModifier = -0.25f; // sparks rise

            var em = ps.emission; em.rateOverTime = 90f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 4f; sh.radius = 0.3f;
            sh.rotation = new Vector3(-90f, 0f, 0f); // emit upward

            var psr = go.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                var partShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (partShader != null) psr.sharedMaterial = new Material(partShader);
            }

            // Pulsing light at the base — first ley line lights up
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.7f, 0.85f, 1.0f);
            light.intensity = 2.5f;
            light.range = 10f;

            // 8-second climb
            float life = 8f, t = 0f;
            while (t < life)
            {
                t += Time.deltaTime;
                if (light != null) light.intensity = 1.5f + 1.5f * Mathf.Sin(t * 6f);
                go.transform.position = buildingPos + new Vector3(0f, Mathf.Lerp(0f, 6f, t / life), 0f);
                yield return null;
            }

            // Settle at apex, dim
            float settle = 4f, st = 0f;
            while (st < settle)
            {
                st += Time.deltaTime;
                if (light != null) light.intensity = Mathf.Lerp(2.5f, 0.4f, st / settle);
                yield return null;
            }
            Object.Destroy(go);

            // Final banner — first ley line lights up (per docs/03)
            ServiceLocator.HUD?.ShowBanner("LEY LINE ACTIVE", "A golden thread points toward something vast.", 6f);
        }
    }
}
