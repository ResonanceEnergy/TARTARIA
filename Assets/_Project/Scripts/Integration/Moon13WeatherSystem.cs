using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    [DefaultExecutionOrder(-51)]
    public class Moon13WeatherSystem : MonoBehaviour
    {
        [Header("Moon 13: Aether Convergence Weather")]
        [SerializeField] int aetherStormCount = 5;
        [SerializeField] int convergenceFluxCount = 4;
        [SerializeField] int moonEchoCount = 12; // One per moon
        [SerializeField] int realityFragmentCount = 8;
        [SerializeField] int cosmicWindCount = 7;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Central Aether Storm - massive storm at center
            CreateWeatherEffect("CentralAetherStorm", Vector3.zero, new Vector3(40f, 20f, 40f), "AetherStorm", new Color(0.8f, 0.9f, 1f, 0.7f), 100);

            // Cardinal Aether Storms
            for (int i = 0; i < aetherStormCount - 1; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 55f,
                    Random.Range(8f, 16f),
                    Mathf.Sin(angle) * 55f
                );
                CreateWeatherEffect($"AetherStorm_{i}", pos, new Vector3(28f, 12f, 28f), "AetherStorm", new Color(0.75f, 0.85f, 0.95f, 0.6f), 65);
            }

            // Convergence Flux - energy streams toward center
            for (int i = 0; i < convergenceFluxCount; i++)
            {
                float angle = (i * 360f / convergenceFluxCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 40f,
                    Random.Range(5f, 12f),
                    Mathf.Sin(angle) * 40f
                );
                CreateWeatherEffect($"ConvergenceFlux_{i}", pos, new Vector3(20f, 10f, 20f), "ConvergenceFlux", new Color(0.6f, 0.8f, 1f, 0.55f), 48);
            }

            // Moon Echoes - 12 tribute weather effects in circle
            Color[] moonColors = GetMoonTributeColors();
            float radius = 85f;
            for (int i = 0; i < moonEchoCount; i++)
            {
                float angle = (i * 360f / moonEchoCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Random.Range(4f, 10f),
                    Mathf.Sin(angle) * radius
                );
                CreateWeatherEffect($"MoonEcho_{i + 1}", pos, new Vector3(18f, 8f, 18f), "MoonEcho", moonColors[i], 42);
            }

            // Reality Fragments - floating pieces of merged realities
            for (int i = 0; i < realityFragmentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    Random.Range(3f, 14f),
                    Random.Range(-60f, 60f)
                );
                CreateWeatherEffect($"RealityFragment_{i}", pos, new Vector3(16f, 8f, 16f), "RealityFragment", new Color(0.85f, 0.9f, 1f, 0.5f), 38);
            }

            // Cosmic Winds - high-altitude energy currents
            for (int i = 0; i < cosmicWindCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(10f, 20f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"CosmicWind_{i}", pos, new Vector3(32f, 8f, 32f), "CosmicWind", new Color(0.7f, 0.85f, 1f, 0.4f), 52);
            }

            Debug.Log($"✨ Moon13WeatherSystem spawned {weatherEffects.Count} weather effects (including 12-moon tribute circle at radius 85f and FINAL convergence storm)");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "AetherStorm" ? 0.4f : (weatherType == "CosmicWind" ? 0.3f : 0.25f);
            main.startLifetime = weatherType == "AetherStorm" ? 5f : 3.5f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = scale;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            
            if (weatherType == "ConvergenceFlux")
            {
                // Point toward center
                Vector3 toCenter = (Vector3.zero - position).normalized;
                velocity.x = toCenter.x * 2f;
                velocity.z = toCenter.z * 2f;
            }
            else if (weatherType == "AetherStorm")
            {
                velocity.x = Random.Range(-3f, 3f);
                velocity.y = Random.Range(-2f, 2f);
                velocity.z = Random.Range(-3f, 3f);
            }
            else if (weatherType == "CosmicWind")
            {
                velocity.x = Random.Range(-4f, 4f);
            }
            else
            {
                velocity.x = Random.Range(-1.5f, 1.5f);
                velocity.y = Random.Range(-1f, 1f);
            }

            weatherEffects.Add(effect);
            return effect;
        }

        Color[] GetMoonTributeColors()
        {
            return new Color[]
            {
                new Color(0.6f, 0.6f, 0.7f, 0.5f),    // Moon1: Memory (gray-blue)
                new Color(0.7f, 0.6f, 0.8f, 0.5f),    // Moon2: Dream (purple)
                new Color(0.3f, 0.7f, 0.3f, 0.5f),    // Moon3: Jungle (green)
                new Color(0.9f, 0.8f, 0.5f, 0.5f),    // Moon4: Desert (sand)
                new Color(0.8f, 0.9f, 1f, 0.5f),      // Moon5: Ice (blue-white)
                new Color(1f, 0.5f, 0.2f, 0.5f),      // Moon6: Lava (orange-red)
                new Color(0.3f, 0.6f, 0.9f, 0.5f),    // Moon7: Underwater (blue)
                new Color(0.7f, 0.8f, 0.95f, 0.5f),   // Moon8: Sky (light blue)
                new Color(0.5f, 0.3f, 0.6f, 0.5f),    // Moon9: Corruption (purple-dark)
                new Color(0.6f, 0.7f, 0.9f, 0.5f),    // Moon10: Time (blue-gray)
                new Color(0.9f, 0.7f, 0.9f, 0.5f),    // Moon11: Prismatic (rainbow)
                new Color(0.2f, 0.2f, 0.3f, 0.5f)     // Moon12: Shadow (dark)
            };
        }

        void OnDestroy()
        {
            foreach (GameObject effect in weatherEffects)
            {
                if (effect != null) Destroy(effect);
            }
            weatherEffects.Clear();
        }
    }
}
