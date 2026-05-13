using UnityEngine;
using UnityEditor;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Builds the Aurora.prefab procedurally via Editor menu.
    /// Menu: Tartaria → VFX → Build Aurora
    /// </summary>
    public static class BuildAuroraVFX
    {
        const string AuroraPrefabPath = "Assets/_Project/Prefabs/VFX/Aurora.prefab";
        const string AuroraMaterialPath = "Assets/_Project/Materials/VFX/AuroraMaterial.mat";
        const string AuroraShaderPath = "Tartaria/VFX/Aurora";

        [MenuItem("Tartaria/VFX/Build Aurora")]
        public static void BuildAurora()
        {
            Debug.Log("[BuildAurora] Creating Aurora.prefab procedurally...");

            // 1. Create root GameObject
            var root = new GameObject("Aurora");

            // 2. Add Particle System
            var ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 60f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 12f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(20f, 40f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.3f, 0.8f, 0.6f, 0.4f),
                new Color(0.5f, 0.9f, 0.7f, 0.6f)
            );
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // 3. Emission
            var emission = ps.emission;
            emission.rateOverTime = 2f;

            // 4. Shape (hemisphere, upward flow)
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 100f;
            shape.radiusThickness = 0.8f;

            // 5. Velocity over Lifetime (drift upward + lateral wave)
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-2f, 2f); // Lateral drift
            vel.y = new ParticleSystem.MinMaxCurve(1f, 3f);  // Upward
            vel.z = new ParticleSystem.MinMaxCurve(-2f, 2f);

            // 6. Color over Lifetime (fade in/out)
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 0.5f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.2f),
                    new GradientAlphaKey(1f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = new ParticleSystem.MinMaxGradient(gradient);

            // 7. Size over Lifetime (expand)
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1.5f));

            // 8. Renderer
            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = GetOrCreateAuroraMaterial();

            // 9. Ensure prefab directory exists
            var dir = Path.GetDirectoryName(AuroraPrefabPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // 10. Save as prefab
            PrefabUtility.SaveAsPrefabAsset(root, AuroraPrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"[BuildAurora] Created {AuroraPrefabPath}");
        }

        static Material GetOrCreateAuroraMaterial()
        {
            // Try to load existing material
            var mat = AssetDatabase.LoadAssetAtPath<Material>(AuroraMaterialPath);
            if (mat != null)
                return mat;

            // Create new material
            var shader = Shader.Find(AuroraShaderPath) ?? Shader.Find("Universal Render Pipeline/Particles/Unlit");
            mat = new Material(shader);
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0);   // Alpha
            mat.SetColor("_BaseColor", new Color(0.5f, 0.9f, 0.7f, 0.5f));

            // Ensure directory exists
            var dir = Path.GetDirectoryName(AuroraMaterialPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(mat, AuroraMaterialPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[BuildAurora] Created {AuroraMaterialPath}");

            return mat;
        }
    }
}
