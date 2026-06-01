#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/Build Out Moon 1 VFX (Cathedral / Spire / Giant / 17th-Hour)
    ///
    /// Generates the 4 climactic VFX prefabs required by docs/03 Moon 1:
    ///   1. VFX_CathedralLightEruption — vertical white beam + radial ground pulse + lens flare
    ///   2. VFX_SpirePlacementSparks — blue-white sparks climbing the spire collider
    ///   3. VFX_GiantModeBurst — ground crack + outward shockwave + character scale tween marker
    ///   4. VFX_SeventeenthHourBeam — golden cathedral-interior beam, vertical light shaft
    ///
    /// Per CLAUDE.md no-stubs mandate: every prefab is a real ParticleSystem with real
    /// curves, sized emission, colored over lifetime. No empty GO placeholders.
    /// </summary>
    public static class Moon1ClimacticVFX
    {
        const string OUT_DIR = "Assets/_Project/Prefabs/VFX/Moon1";

        [MenuItem("Tartaria/1 Build/Moon 1 — VFX (Cathedral + Spire + Giant + 17th-Hour)", priority = 185)]
        public static void Run()
        {
            if (!Directory.Exists(OUT_DIR)) Directory.CreateDirectory(OUT_DIR);

            int built = 0;
            built += SaveAs(BuildCathedralEruption(), "VFX_CathedralLightEruption") ? 1 : 0;
            built += SaveAs(BuildSpireSparks(),       "VFX_SpirePlacementSparks")    ? 1 : 0;
            built += SaveAs(BuildGiantModeBurst(),    "VFX_GiantModeBurst")          ? 1 : 0;
            built += SaveAs(BuildSeventeenthHour(),   "VFX_SeventeenthHourBeam")     ? 1 : 0;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            string msg = $"Generated {built}/4 VFX prefabs at:\n{OUT_DIR}\n\n" +
                         "1. VFX_CathedralLightEruption  — Days 19-24 climax\n" +
                         "2. VFX_SpirePlacementSparks    — Days 6-12 spire ceremony\n" +
                         "3. VFX_GiantModeBurst          — Days 13-18 giant transformation\n" +
                         "4. VFX_SeventeenthHourBeam     — Days 19-24 alignment cathedral light";
            Debug.Log("[Moon1ClimacticVFX] " + msg);
            EditorUtility.DisplayDialog("Climactic VFX", msg, "OK");
        }

        // ─── 1. Cathedral light eruption ────────────────────────────────────
        static GameObject BuildCathedralEruption()
        {
            var root = new GameObject("VFX_CathedralLightEruption");

            // Vertical beam (white shaft)
            var beam = new GameObject("Beam");
            beam.transform.SetParent(root.transform, false);
            beam.transform.localPosition = new Vector3(0, 30f, 0);
            var beamPS = beam.AddComponent<ParticleSystem>();
            ConfigBeam(beamPS, new Color(1f, 0.95f, 0.80f, 1f), 60f, 5f);

            // Radial ground pulse
            var pulse = new GameObject("RadialPulse");
            pulse.transform.SetParent(root.transform, false);
            var pulsePS = pulse.AddComponent<ParticleSystem>();
            ConfigShockwave(pulsePS, new Color(1f, 0.90f, 0.40f, 1f), 40f, 2.5f);

            // Lens flare (point light)
            var glow = new GameObject("Glow");
            glow.transform.SetParent(root.transform, false);
            glow.transform.localPosition = new Vector3(0, 25f, 0);
            var pt = glow.AddComponent<Light>();
            pt.type = LightType.Point;
            pt.range = 80f;
            pt.intensity = 8f;
            pt.color = new Color(1f, 0.90f, 0.65f, 1f);

            return root;
        }

        // ─── 2. Spire placement sparks ──────────────────────────────────────
        static GameObject BuildSpireSparks()
        {
            var root = new GameObject("VFX_SpirePlacementSparks");
            var sparks = new GameObject("Sparks");
            sparks.transform.SetParent(root.transform, false);
            var ps = sparks.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 3.0f;
            main.startSpeed = 8f;
            main.startSize = 0.15f;
            main.startColor = new Color(0.55f, 0.75f, 1.0f, 1f);
            main.maxParticles = 600;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 120f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f;
            shape.radius = 0.5f;
            shape.position = new Vector3(0, 0, 0);
            shape.rotation = new Vector3(-90, 0, 0); // upward
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.55f, 0.75f, 1.0f), 0f),
                        new GradientColorKey(new Color(0.95f, 0.95f, 1.0f), 0.6f),
                        new GradientColorKey(new Color(0.20f, 0.40f, 0.85f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            // Renderer
            var r = sparks.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.lengthScale = 4f;
            return root;
        }

        // ─── 3. Giant Mode burst ────────────────────────────────────────────
        static GameObject BuildGiantModeBurst()
        {
            var root = new GameObject("VFX_GiantModeBurst");

            // Ground crack — radial cube splatter
            var crack = new GameObject("GroundCrack");
            crack.transform.SetParent(root.transform, false);
            var crackPS = crack.AddComponent<ParticleSystem>();
            var main = crackPS.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 12f;
            main.startSize = 0.40f;
            main.startColor = new Color(0.60f, 0.45f, 0.28f, 1f);
            main.maxParticles = 200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = crackPS.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 80) });
            var shape = crackPS.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.5f;
            var col = crackPS.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.60f, 0.45f, 0.28f), 0f),
                        new GradientColorKey(new Color(0.32f, 0.22f, 0.14f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            // Outward shockwave
            var wave = new GameObject("Shockwave");
            wave.transform.SetParent(root.transform, false);
            var wavePS = wave.AddComponent<ParticleSystem>();
            ConfigShockwave(wavePS, new Color(1f, 0.65f, 0.20f, 1f), 25f, 1.8f);

            // Vertical golden pillar (gives the player a frame of reference for scale)
            var pillar = new GameObject("ScalePillar");
            pillar.transform.SetParent(root.transform, false);
            pillar.transform.localPosition = new Vector3(0, 7f, 0);
            var pillarPS = pillar.AddComponent<ParticleSystem>();
            ConfigBeam(pillarPS, new Color(0.95f, 0.85f, 0.30f, 1f), 15f, 1.5f);

            return root;
        }

        // ─── 4. 17th-Hour alignment beam ────────────────────────────────────
        static GameObject BuildSeventeenthHour()
        {
            var root = new GameObject("VFX_SeventeenthHourBeam");

            // Long vertical golden shaft (cathedral interior)
            var shaft = new GameObject("GoldenShaft");
            shaft.transform.SetParent(root.transform, false);
            shaft.transform.localPosition = new Vector3(0, 12f, 0);
            var shaftPS = shaft.AddComponent<ParticleSystem>();
            ConfigBeam(shaftPS, new Color(0.95f, 0.78f, 0.25f, 1f), 24f, 3f);

            // Floating motes (dust catching the beam)
            var motes = new GameObject("Motes");
            motes.transform.SetParent(root.transform, false);
            var motesPS = motes.AddComponent<ParticleSystem>();
            var main = motesPS.main;
            main.startLifetime = 6f;
            main.startSpeed = 0.30f;
            main.startSize = 0.06f;
            main.startColor = new Color(0.98f, 0.92f, 0.65f, 0.85f);
            main.maxParticles = 300;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = motesPS.emission;
            emission.rateOverTime = 40f;
            var shape = motesPS.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(8f, 18f, 8f);
            shape.position = new Vector3(0, 9f, 0);

            // Warm point light
            var glow = new GameObject("Glow");
            glow.transform.SetParent(root.transform, false);
            glow.transform.localPosition = new Vector3(0, 8f, 0);
            var pt = glow.AddComponent<Light>();
            pt.type = LightType.Point;
            pt.range = 30f;
            pt.intensity = 4.5f;
            pt.color = new Color(0.95f, 0.78f, 0.30f, 1f);

            return root;
        }

        // ─── Shared helpers ─────────────────────────────────────────────────
        static void ConfigBeam(ParticleSystem ps, Color color, float length, float widthRadius)
        {
            var main = ps.main;
            main.startLifetime = 4f;
            main.startSpeed = 1f;
            main.startSize = widthRadius * 0.4f;
            main.startColor = color;
            main.maxParticles = 400;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 80f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(widthRadius * 2f, length, widthRadius * 2f);
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f),
                        new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
        }

        static void ConfigShockwave(ParticleSystem ps, Color color, float radius, float lifetime)
        {
            var main = ps.main;
            main.startLifetime = lifetime;
            main.startSpeed = radius / lifetime;
            main.startSize = 1.2f;
            main.startColor = color;
            main.maxParticles = 600;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 220) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            shape.arc = 360f;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color * 0.7f, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
        }

        static bool SaveAs(GameObject go, string assetName)
        {
            string path = OUT_DIR + "/" + assetName + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab != null;
        }
    }
}
#endif
