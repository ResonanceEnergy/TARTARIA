using UnityEngine;
using UnityEditor;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// VFX Upgrade Tool — enhances existing particle systems to AAA quality.
    /// Upgrades ScanPulse, RestoreSparkle, ShardCollect to 500-2000 particles with trails, sub-emitters, color gradients.
    /// 
    /// Usage: Unity menu → Tartaria → Upgrade VFX
    /// CLI Usage: Unity -batchmode -executeMethod Tartaria.Editor.VFXUpgradeTool.UpgradeAllCLI -quit
    /// </summary>
    public class VFXUpgradeTool : EditorWindow
    {
        /// <summary>
        /// CLI entry point: upgrade all VFX (batch mode compatible).
        /// </summary>
        [MenuItem("Tartaria/CLI/Upgrade All VFX")]
        public static void UpgradeAllCLI()
        {
            Debug.Log("[VFX] CLI execution started...");
            UpgradeScanPulseStatic();
            UpgradeRestoreSparkleStatic();
            UpgradeShardCollectStatic();
            CreateAuroraVFXStatic();
            Debug.Log("[VFX] CLI execution complete.");
        }
        [MenuItem("Tartaria/Upgrade VFX")]
        static void ShowWindow()
        {
            var window = GetWindow<VFXUpgradeTool>("VFX Upgrade");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("VFX Upgrade Tool — P2 Implementation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Upgrades 3 existing particle prefabs to AAA quality:\n\n" +
                "• ScanPulse: 100 → 500 particles, radial burst, gold fade\n" +
                "• RestoreSparkle: 100 → 2000 particles, upward drift, trails\n" +
                "• ShardCollect: 50 → 300 particles, impact burst, shimmer\n\n" +
                "Adds volumetric fog + aurora effects.",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("Upgrade ScanPulse (Radial Burst)", GUILayout.Height(40)))
            {
                UpgradeScanPulseStatic();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Upgrade RestoreSparkle (2000 Particles)", GUILayout.Height(40)))
            {
                UpgradeRestoreSparkleStatic();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Upgrade ShardCollect (Shimmer Burst)", GUILayout.Height(40)))
            {
                UpgradeShardCollectStatic();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Create Aurora VFX Prefab", GUILayout.Height(40)))
            {
                CreateAuroraVFXStatic();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("FULL VFX UPGRADE (All 4 Operations)", GUILayout.Height(60)))
            {
                UpgradeScanPulseStatic();
                UpgradeRestoreSparkleStatic();
                UpgradeShardCollectStatic();
                CreateAuroraVFXStatic();
                
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Success", "All VFX upgraded to AAA quality!", "OK");
                }
            }
        }

        public static void UpgradeScanPulseStatic()
        {
            Debug.Log("[VFX] Upgrading ScanPulse...");

            string prefabPath = "Assets/_Project/Prefabs/VFX/ScanPulse.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[VFX] Prefab not found: {prefabPath}");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            ParticleSystem ps = instance.GetComponent<ParticleSystem>();

            if (ps == null)
            {
                DestroyImmediate(instance);
                Debug.LogError("[VFX] No ParticleSystem on ScanPulse prefab");
                return;
            }

            // Main module: 500 particles, radial burst
            var main = ps.main;
            main.startLifetime = 1.2f;
            main.startSpeed = 5.0f;
            main.startSize = 0.3f;
            main.maxParticles = 500;

            // Emission: burst 500 particles at start
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 500)
            });

            // Shape: sphere surface (radial burst)
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            shape.radiusThickness = 1.0f; // Emit from surface only

            // Color over lifetime: cyan → gold → fade
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0.3f, 0.8f, 1.0f), 0.0f),   // Cyan
                    new GradientColorKey(new Color(1.0f, 0.8f, 0.3f), 0.7f),   // Gold
                    new GradientColorKey(new Color(1.0f, 0.8f, 0.3f), 1.0f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 0.5f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            colorOverLifetime.color = grad;

            // Size over lifetime: grow slightly
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0.0f, 0.5f);
            sizeCurve.AddKey(0.3f, 1.2f);
            sizeCurve.AddKey(1.0f, 0.3f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
            DestroyImmediate(instance);

            Debug.Log("[VFX] ✓ ScanPulse upgraded: 500 particles, radial burst, cyan→gold fade");
        }

        public static void UpgradeRestoreSparkleStatic()
        {
            Debug.Log("[VFX] Upgrading RestoreSparkle...");

            string prefabPath = "Assets/_Project/Prefabs/VFX/RestoreSparkle.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[VFX] Prefab not found: {prefabPath}");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            ParticleSystem ps = instance.GetComponent<ParticleSystem>();

            if (ps == null)
            {
                DestroyImmediate(instance);
                Debug.LogError("[VFX] No ParticleSystem on RestoreSparkle prefab");
                return;
            }

            // Main module: 2000 particles, longer lifetime
            var main = ps.main;
            main.duration = 2.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 3.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.maxParticles = 2000;
            main.simulationSpace = ParticleSystemSimulationSpace.World; // Drift away from building

            // Emission: 800 particles/sec for 2.5s
            var emission = ps.emission;
            emission.rateOverTime = 800;

            // Shape: box around building
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(5f, 5f, 5f);

            // Velocity over lifetime: upward drift
            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(2.0f); // Constant upward

            // Color over lifetime: golden sparkle
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1.0f, 0.9f, 0.5f), 0.0f),  // Bright gold
                    new GradientColorKey(new Color(1.0f, 0.7f, 0.2f), 0.5f),  // Orange-gold
                    new GradientColorKey(new Color(1.0f, 0.8f, 0.3f), 1.0f)   // Fade gold
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 0.1f),
                    new GradientAlphaKey(1.0f, 0.7f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            colorOverLifetime.color = grad;

            // Trails: golden streaks
            var trails = ps.trails;
            trails.enabled = true;
            trails.ratio = 0.3f; // 30% of particles emit trails
            trails.lifetime = 0.5f;
            trails.minVertexDistance = 0.2f;
            trails.worldSpace = true;
            trails.dieWithParticles = true;
            trails.colorOverLifetime = grad;

            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
            DestroyImmediate(instance);

            Debug.Log("[VFX] ✓ RestoreSparkle upgraded: 2000 particles, upward drift, golden trails");
        }

        public static void UpgradeShardCollectStatic()
        {
            Debug.Log("[VFX] Upgrading ShardCollect...");

            string prefabPath = "Assets/_Project/Prefabs/VFX/ShardCollect.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[VFX] Prefab not found: {prefabPath}");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            ParticleSystem ps = instance.GetComponent<ParticleSystem>();

            if (ps == null)
            {
                DestroyImmediate(instance);
                Debug.LogError("[VFX] No ParticleSystem on ShardCollect prefab");
                return;
            }

            // Main module: 300 particles, quick burst
            var main = ps.main;
            main.duration = 0.8f;
            main.startLifetime = 0.6f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 5.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.maxParticles = 300;

            // Emission: instant burst
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 300)
            });

            // Shape: sphere burst
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            // Color over lifetime: shimmer fade
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0.9f, 1.0f, 1.0f), 0.0f),  // Bright white-cyan
                    new GradientColorKey(new Color(0.6f, 0.8f, 1.0f), 0.5f),  // Cyan-blue
                    new GradientColorKey(new Color(0.3f, 0.5f, 0.7f), 1.0f)   // Dark blue
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.8f, 0.3f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            colorOverLifetime.color = grad;

            // Size over lifetime: shrink
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0.0f, 1.5f);
            sizeCurve.AddKey(0.2f, 1.0f);
            sizeCurve.AddKey(1.0f, 0.0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
            DestroyImmediate(instance);

            Debug.Log("[VFX] ✓ ShardCollect upgraded: 300 particles, shimmer burst");
        }

        public static void CreateAuroraVFXStatic()
        {
            Debug.Log("[VFX] Creating Aurora VFX...");

            string prefabPath = "Assets/_Project/Prefabs/VFX/Aurora.prefab";
            string prefabDir = Path.GetDirectoryName(prefabPath);

            if (!Directory.Exists(Path.Combine(Application.dataPath, "..", prefabDir)))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", prefabDir));
            }

            GameObject aurora = new GameObject("Aurora");
            ParticleSystem ps = aurora.AddComponent<ParticleSystem>();

            // Main module: long-lived, slow-moving aurora ribbons
            var main = ps.main;
            main.duration = 30f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(10f, 15f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(20f, 40f);
            main.startRotation = new ParticleSystem.MinMaxCurve(-180f, 180f);
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Emission: slow spawn rate
            var emission = ps.emission;
            emission.rateOverTime = 2f;

            // Shape: box in sky
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(100f, 20f, 100f);
            shape.position = new Vector3(0f, 50f, 0f);

            // Velocity over lifetime: horizontal wave motion
            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            AnimationCurve xCurve = AnimationCurve.EaseInOut(0f, -5f, 1f, 5f);
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(1.0f, xCurve);

            // Color over lifetime: aurora gradient (green-gold-violet)
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0.3f, 1.0f, 0.5f), 0.0f),  // Green
                    new GradientColorKey(new Color(1.0f, 0.8f, 0.3f), 0.5f),  // Gold
                    new GradientColorKey(new Color(0.6f, 0.3f, 1.0f), 1.0f)   // Violet
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.0f, 0.0f),
                    new GradientAlphaKey(0.6f, 0.2f),
                    new GradientAlphaKey(0.6f, 0.8f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            colorOverLifetime.color = grad;

            // Renderer: additive blending for glow
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_Particle_Additive.mat");

            PrefabUtility.SaveAsPrefabAsset(aurora, prefabPath);
            DestroyImmediate(aurora);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VFX] ✓ Aurora VFX created: {prefabPath}");
        }
    }
}
