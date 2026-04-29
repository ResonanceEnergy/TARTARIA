using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// VFX Factory — creates gameplay VFX prefabs using Unity ParticleSystems.
    /// Feature 3: ScanPulse, RestoreSparkle, ShardCollect
    /// 
    /// Menu: Tartaria/Build Assets/VFX Prefabs
    /// </summary>
    public static class VFXFactory
    {
        const string PrefabPath = "Assets/_Project/Prefabs/VFX/";

        [MenuItem("Tartaria/Build Assets/VFX Prefabs")]
        public static void BuildAllVFX()
        {
            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder(PrefabPath);

            BuildScanPulse();
            BuildRestoreSparkle();
            BuildShardCollect();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VFX] Built 3 VFX prefabs");
        }

        static void BuildScanPulse()
        {
            // Radial wave, cyan glow, 1s duration
            GameObject go = new GameObject("ScanPulse");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = 1f;
            main.startSpeed = 0f;
            main.startSize = 0.5f;
            main.startColor = new Color(0.2f, 0.8f, 1f, 0.7f); // cyan
            main.maxParticles = 100;

            // Emission: burst at start
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 80)
            });

            // Shape: sphere expanding outward
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            // Size over lifetime: grow from 0.5 → 5m
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 5f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Color over lifetime: fade out
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.2f, 0.8f, 1f), 0f),
                    new GradientColorKey(new Color(0.2f, 0.8f, 1f), 0.5f),
                    new GradientColorKey(new Color(0.1f, 0.4f, 0.6f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.7f, 0f),
                    new GradientAlphaKey(0.5f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            // Renderer: additive material
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = CreateAdditiveMaterial(new Color(0.2f, 0.8f, 1f));

            SavePrefab(go, "ScanPulse");
        }

        static void BuildRestoreSparkle()
        {
            // Golden particles spiral upward, 2s
            GameObject go = new GameObject("RestoreSparkle");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 2f;
            main.loop = false;
            main.startLifetime = 1.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = new Color(1f, 0.85f, 0.3f, 0.9f); // golden
            main.gravityModifier = -0.3f; // slight upward drift
            main.maxParticles = 300;

            // Emission: continuous stream
            var emission = ps.emission;
            emission.rateOverTime = 150f;

            // Shape: cone upward
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.5f;
            shape.rotation = new Vector3(-90f, 0f, 0f); // point upward

            // Velocity over lifetime: spiral (approximate with noise)
            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, -1f, 1f, 1f));
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, -1f));

            // Color over lifetime: sparkle fade
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0f),
                    new GradientColorKey(new Color(1f, 0.7f, 0.2f), 0.5f),
                    new GradientColorKey(new Color(0.8f, 0.5f, 0.1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0.7f, 0.3f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            // Renderer: additive
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = CreateAdditiveMaterial(new Color(1f, 0.85f, 0.3f));

            SavePrefab(go, "RestoreSparkle");
        }

        static void BuildShardCollect()
        {
            // Shimmer burst at pickup point, 0.5s
            GameObject go = new GameObject("ShardCollect");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.4f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.1f);
            main.startColor = new Color(0.8f, 1f, 0.9f, 1f); // pale shimmer
            main.maxParticles = 50;

            // Emission: single burst
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 40)
            });

            // Shape: sphere outward
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;

            // Size over lifetime: shrink
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Color over lifetime: fade
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 1f, 1f), 0f),
                    new GradientColorKey(new Color(0.5f, 0.8f, 0.6f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            // Renderer: additive
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = CreateAdditiveMaterial(new Color(0.8f, 1f, 0.9f));

            SavePrefab(go, "ShardCollect");
        }

        static Material CreateAdditiveMaterial(Color tint)
        {
            // Use URP/Particles/Unlit with additive blending
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
                shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null)
                shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 1);   // Additive
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_ZWrite", 0);
            mat.renderQueue = 3000; // Transparent queue
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_BLENDMODE_ADD");

            return mat;
        }

        static void SavePrefab(GameObject go, string name)
        {
            string path = $"{PrefabPath}{name}.prefab";

            // Delete existing
            if (System.IO.File.Exists(path))
                AssetDatabase.DeleteAsset(path);

            // Save new
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            Debug.Log($"[VFX] Created {path}");
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = System.IO.Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
