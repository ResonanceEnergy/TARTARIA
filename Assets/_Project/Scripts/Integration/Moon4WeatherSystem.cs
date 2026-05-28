using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-51)]
    public class Moon4WeatherSystem : MonoBehaviour
    {
        [Header("Moon 4: Desert Weather")]
        [SerializeField] int sandstormCount = 6;
        [SerializeField] int heatWaveCount = 5;
        [SerializeField] int dustDevilCount = 4;
        [SerializeField] int sandDriftCount = 8;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Sandstorms - swirling sand particles
            for (int i = 0; i < sandstormCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(4f, 12f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"Sandstorm_{i}", pos, new Vector3(28f, 10f, 28f), "Sandstorm", new Color(0.8f, 0.7f, 0.5f, 0.4f), 60);
            }

            // Heat Waves - shimmering air particles
            for (int i = 0; i < heatWaveCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(1f, 3f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"HeatWave_{i}", pos, new Vector3(22f, 5f, 22f), "HeatWave", new Color(1f, 0.9f, 0.7f, 0.2f), 25);
            }

            // Dust Devils - vertical sand spirals
            for (int i = 0; i < dustDevilCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(2f, 8f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"DustDevil_{i}", pos, new Vector3(6f, 12f, 6f), "DustDevil", new Color(0.75f, 0.65f, 0.45f, 0.5f), 45);
            }

            // Sand Drifts - ground-level sand movement
            for (int i = 0; i < sandDriftCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0.5f,
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"SandDrift_{i}", pos, new Vector3(20f, 2f, 20f), "SandDrift", new Color(0.85f, 0.75f, 0.55f, 0.3f), 40);
            }

            Debug.Log($"🌪️ Moon4WeatherSystem spawned {weatherEffects.Count} weather effects");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "HeatWave" ? 0.8f : 0.2f;
            main.startLifetime = weatherType == "Sandstorm" ? 4f : 3f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2f;

            var shape = ps.shape;
            shape.shapeType = weatherType == "DustDevil" ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Box;
            shape.scale = scale;

            if (weatherType == "Sandstorm" || weatherType == "SandDrift")
            {
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.x = Random.Range(-4f, -2f);
                velocity.y = weatherType == "Sandstorm" ? Random.Range(-1f, 1f) : 0f;
            }
            else if (weatherType == "DustDevil")
            {
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.y = 3f;
                
                var velocityModule = ps.velocityOverLifetime;
                velocityModule.space = ParticleSystemSimulationSpace.Local;
            }

            weatherEffects.Add(effect);
            return effect;
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
