using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Runtime VFX Setup — Creates placeholder VFX prefabs programmatically
    /// Runs on scene load to ensure VFXManager has valid prefab references
    /// </summary>
    [DefaultExecutionOrder(-90)] // After VFXManager (-100), before other systems
    public class RuntimeVFXSetup : MonoBehaviour
    {
        void Awake()
        {
            CreatePlaceholderVFXPrefabs();
        }

        void CreatePlaceholderVFXPrefabs()
        {
            // Create ScanPulse VFX
            var scanPulse = CreateVFXPrefab("ScanPulse",
                new Color(0.3f, 0.7f, 1f, 0.8f), // Cyan blue
                2f, // 2m radius
                1f); // 1s duration

            // Create RestoreSparkle VFX
            var restoreSparkle = CreateVFXPrefab("RestoreSparkle",
                new Color(1f, 0.9f, 0.4f, 0.9f), // Golden
                1f, // 1m radius
                2f); // 2s duration with burst

            // Create ShardCollect VFX
            var shardCollect = CreateVFXPrefab("ShardCollect",
                new Color(0.5f, 1f, 0.7f, 0.8f), // Aether green
                0.5f, // 0.5m radius
                0.8f); // 0.8s duration

            // Assign to VFXManager if it exists
            var vfxManager = VFXManager.Instance;
            if (vfxManager != null)
            {
                var vfxType = typeof(VFXManager);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

                var scanField = vfxType.GetField("scanPulseVFXPrefab", flags);
                scanField?.SetValue(vfxManager, scanPulse);

                var restoreField = vfxType.GetField("restoreSparkleVFXPrefab", flags);
                restoreField?.SetValue(vfxManager, restoreSparkle);

                var shardField = vfxType.GetField("shardCollectVFXPrefab", flags);
                shardField?.SetValue(vfxManager, shardCollect);

                Debug.Log("[RuntimeVFXSetup] Created and assigned 3 placeholder VFX prefabs");
            }
            else
            {
                Debug.LogWarning("[RuntimeVFXSetup] VFXManager not found, couldn't assign prefabs");
            }
        }

        GameObject CreateVFXPrefab(string name, Color color, float radius, float duration)
        {
            var prefab = new GameObject($"VFX_{name}");
            prefab.SetActive(false); // Prefabs should be inactive

            // Add particle system
            var ps = prefab.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startSize = radius;
            main.startLifetime = duration;
            main.startSpeed = 0.5f;
            main.maxParticles = 50;
            main.loop = false;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 30) // Burst 30 particles at start
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 0.5f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            // Add auto-destroy component
            var autoDestroy = prefab.AddComponent<AutoDestroyVFX>();
            autoDestroy.lifetime = duration + 0.5f;

            // Add light for extra visual pop
            var light = prefab.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = 2f;
            light.range = radius * 3f;
            light.enabled = true;

            DontDestroyOnLoad(prefab);
            return prefab;
        }
    }

    /// <summary>
    /// Auto-destroy component for VFX instances
    /// </summary>
    public class AutoDestroyVFX : MonoBehaviour
    {
        public float lifetime = 2f;
        float _timer;

        void OnEnable()
        {
            _timer = 0f;
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
