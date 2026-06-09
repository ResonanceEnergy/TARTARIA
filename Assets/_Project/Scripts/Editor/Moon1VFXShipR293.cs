using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// R293-R295 — instantiate the 3 R171 VFX particle systems + dissolution material
    /// into the scene per CLAUDE.md "highest-leverage R171 actions" + docs/15 §8.
    ///
    /// Menu: Tartaria → 1 Build → Moon 1 VFX Ship R293
    /// </summary>
    public static class Moon1VFXShipR293
    {
        [MenuItem("Tartaria/1 Build/Moon 1 VFX Ship R293 (3 ParticleSystems + Dissolve mat)")]
        public static void Run()
        {
            int placed = 0;

            placed += CreateAetherSeamPulseAt(new Vector3(0, 5, 15), "VFX_AetherSeam_Dome");
            placed += CreateAetherSeamPulseAt(new Vector3(15, 0, 6), "VFX_AetherSeam_Fountain");
            placed += CreateAetherSeamPulseAt(new Vector3(-14, 2, 8), "VFX_AetherSeam_Spire");

            placed += CreateMudBubbleAt(new Vector3(28, 0.2f, -8), "VFX_MudBubble_Pool1");
            placed += CreateMudBubbleAt(new Vector3(-12, 0.2f, -22), "VFX_MudBubble_Pool2");
            placed += CreateMudBubbleAt(new Vector3(35, 0.2f, 22), "VFX_MudBubble_Pool3");

            placed += CreateRestorationBurstAt(new Vector3(0, 0, 15), "VFX_RestorationBurst_Dome");
            placed += CreateRestorationBurstAt(new Vector3(15, 0, 6), "VFX_RestorationBurst_Fountain");
            placed += CreateRestorationBurstAt(new Vector3(-14, 0, 8), "VFX_RestorationBurst_Spire");

            // Create the dissolution material asset
            CreateDissolutionMaterial();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog(
                "Moon 1 VFX Ship R293",
                $"Placed {placed} ParticleSystems + 1 dissolution material. Save scene (Ctrl+S).",
                "OK");
        }

        // === Aether-Gold seam pulse (slow rising golden particles) ===
        static int CreateAetherSeamPulseAt(Vector3 pos, string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject(name);
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = 4.5f;
            main.startSpeed = 0.4f;
            main.startSize = 0.08f;
            main.startColor = new Color(1f, 0.85f, 0.45f, 0.85f);  // Aether-Gold
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 12f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var velocityOverLife = ps.velocityOverLifetime;
            velocityOverLife.enabled = true;
            velocityOverLife.y = 0.3f;

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.85f, 0.45f), 0.0f),
                    new GradientColorKey(new Color(1f, 0.95f, 0.65f), 0.5f),
                    new GradientColorKey(new Color(1f, 0.7f, 0.3f), 1.0f),
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0.0f),
                    new GradientAlphaKey(1f, 0.2f),
                    new GradientAlphaKey(0.7f, 0.7f),
                    new GradientAlphaKey(0f, 1.0f),
                });
            colorOverLife.color = grad;

            // Use Particles/Unlit Additive shader for emissive glow
            var mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.SetFloat("_Surface", 1f);  // Transparent
            mat.SetFloat("_Blend", 0f);     // Additive (alpha = 0 for additive)
            mat.SetColor("_BaseColor", new Color(1f, 0.85f, 0.45f, 1));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.45f) * 3f);
            renderer.sharedMaterial = mat;

            Debug.Log($"[Moon1VFXShipR293] {name} placed @ {pos}");
            return 1;
        }

        // === Mud bubble (slow oily rises) ===
        static int CreateMudBubbleAt(Vector3 pos, string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject(name);
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = 2.5f;
            main.startSpeed = 0.15f;
            main.startSize = 0.25f;
            main.startColor = new Color(0.30f, 0.22f, 0.15f, 0.75f);
            main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 4f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.2f;

            var velocityOverLife = ps.velocityOverLifetime;
            velocityOverLife.enabled = true;
            velocityOverLife.y = 0.2f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            var curve = new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(0.6f, 1.0f),
                new Keyframe(1f, 0f));
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.SetColor("_BaseColor", new Color(0.30f, 0.22f, 0.15f, 1));
            renderer.sharedMaterial = mat;

            Debug.Log($"[Moon1VFXShipR293] {name} placed @ {pos}");
            return 1;
        }

        // === Restoration burst (one-shot golden burst at end of tuning) ===
        static int CreateRestorationBurstAt(Vector3 pos, string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject(name);
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            go.SetActive(false);  // Dormant - triggered by RestorationManager on tune complete

            var main = ps.main;
            main.duration = 3f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 2.5f;
            main.startSpeed = 4.0f;
            main.startSize = 0.3f;
            main.startColor = new Color(1f, 0.95f, 0.65f, 1f);
            main.maxParticles = 200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0.0f, 150) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1.5f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            var curve = new AnimationCurve(
                new Keyframe(0f, 1.0f),
                new Keyframe(1f, 0f));
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 1f, 0.8f), 0.0f),
                    new GradientColorKey(new Color(1f, 0.85f, 0.45f), 0.5f),
                    new GradientColorKey(new Color(1f, 0.7f, 0.3f), 1.0f),
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0.0f),
                    new GradientAlphaKey(0.8f, 0.4f),
                    new GradientAlphaKey(0f, 1.0f),
                });
            colorOverLife.color = grad;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.SetColor("_BaseColor", new Color(1f, 0.85f, 0.45f, 1));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.45f) * 5f);
            renderer.sharedMaterial = mat;

            Debug.Log($"[Moon1VFXShipR293] {name} placed @ {pos} (dormant — triggered on restoration)");
            return 1;
        }

        // === Dissolution material asset ===
        static void CreateDissolutionMaterial()
        {
            string path = "Assets/_Project/Materials/Building_MudDissolve.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) { Debug.Log("Building_MudDissolve.mat already exists"); return; }

            var shader = Shader.Find("Tartaria/Building/MudDissolve");
            if (shader == null) { Debug.LogWarning("Tartaria/Building/MudDissolve shader not found"); return; }

            var mat = new Material(shader);
            mat.SetColor("_StoneColor", new Color(0.62f, 0.52f, 0.40f));
            mat.SetColor("_MudColor", new Color(0.28f, 0.20f, 0.12f));
            mat.SetColor("_GoldenEmission", new Color(1.0f, 0.85f, 0.45f));
            mat.SetFloat("_DissolveProgress", 0f);
            mat.SetFloat("_BuildingBase", -8.0f);
            mat.SetFloat("_BuildingHeight", 18.0f);
            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            Debug.Log("[Moon1VFXShipR293] Building_MudDissolve.mat created at " + path);
        }
    }
}
