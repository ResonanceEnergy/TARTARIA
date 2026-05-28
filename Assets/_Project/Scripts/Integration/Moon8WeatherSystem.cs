using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-51)]
    public class Moon8WeatherSystem : MonoBehaviour
    {
        [Header("Moon 8: Sky Weather")]
        [SerializeField] int windStreamCount = 8;
        [SerializeField] int cloudDriftCount = 6;
        [SerializeField] int lightningStormCount = 4;
        [SerializeField] int auraBurstCount = 7;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Wind Streams - horizontal air currents
            for (int i = 0; i < windStreamCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(8f, 18f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"WindStream_{i}", pos, new Vector3(30f, 6f, 30f), "WindStream", new Color(0.8f, 0.85f, 0.9f, 0.2f), 45);
            }

            // Cloud Drifts - moving cloud formations
            for (int i = 0; i < cloudDriftCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(10f, 16f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"CloudDrift_{i}", pos, new Vector3(26f, 8f, 26f), "CloudDrift", new Color(0.9f, 0.9f, 0.95f, 0.35f), 55);
            }

            // Lightning Storms - electrical discharge zones
            for (int i = 0; i < lightningStormCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(12f, 20f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"LightningStorm_{i}", pos, new Vector3(18f, 10f, 18f), "LightningStorm", new Color(0.8f, 0.9f, 1f, 0.6f), 40);
            }

            // Aura Bursts - magical energy bursts
            for (int i = 0; i < auraBurstCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(6f, 14f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"AuraBurst_{i}", pos, new Vector3(14f, 6f, 14f), "AuraBurst", new Color(0.7f, 0.8f, 1f, 0.5f), 35);
            }

            Debug.Log($"⚡ Moon8WeatherSystem spawned {weatherEffects.Count} weather effects");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "CloudDrift" ? 0.6f : (weatherType == "LightningStorm" ? 0.15f : 0.3f);
            main.startLifetime = weatherType == "WindStream" ? 3f : 4f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = scale;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            
            if (weatherType == "WindStream")
            {
                velocity.x = Random.Range(-5f, 5f);
            }
            else if (weatherType == "CloudDrift")
            {
                velocity.x = Random.Range(-2f, 2f);
            }
            else if (weatherType == "LightningStorm")
            {
                velocity.y = Random.Range(-8f, -4f);
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
