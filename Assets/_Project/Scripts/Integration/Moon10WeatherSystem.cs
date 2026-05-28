using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    [DefaultExecutionOrder(-51)]
    public class Moon10WeatherSystem : MonoBehaviour
    {
        [Header("Moon 10: Time Weather")]
        [SerializeField] int temporalFluxCount = 6;
        [SerializeField] int chronoParticleCount = 7;
        [SerializeField] int timeRippleCount = 5;
        [SerializeField] int pastEchoCount = 8;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Temporal Flux - distorted time streams
            for (int i = 0; i < temporalFluxCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(4f, 12f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"TemporalFlux_{i}", pos, new Vector3(24f, 10f, 24f), "TemporalFlux", new Color(0.6f, 0.7f, 0.9f, 0.4f), 48);
            }

            // Chrono Particles - time crystals floating
            for (int i = 0; i < chronoParticleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(2f, 10f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"ChronoParticle_{i}", pos, new Vector3(18f, 8f, 18f), "ChronoParticle", new Color(0.7f, 0.8f, 1f, 0.6f), 42);
            }

            // Time Ripples - expanding temporal waves
            for (int i = 0; i < timeRippleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(3f, 9f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"TimeRipple_{i}", pos, new Vector3(22f, 6f, 22f), "TimeRipple", new Color(0.5f, 0.6f, 0.8f, 0.35f), 38);
            }

            // Past Echoes - ghost images from past
            for (int i = 0; i < pastEchoCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(1f, 8f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"PastEcho_{i}", pos, new Vector3(16f, 8f, 16f), "PastEcho", new Color(0.8f, 0.85f, 0.95f, 0.25f), 35);
            }

            Debug.Log($"⏰ Moon10WeatherSystem spawned {weatherEffects.Count} weather effects");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "TimeRipple" ? 0.5f : 0.25f;
            main.startLifetime = weatherType == "TemporalFlux" ? 5f : 3.5f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = scale;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = Random.Range(-2f, 2f);
            velocity.y = Random.Range(-2f, 2f);
            velocity.z = Random.Range(-2f, 2f);

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
