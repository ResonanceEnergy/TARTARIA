using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    [DefaultExecutionOrder(-51)]
    public class Moon5WeatherSystem : MonoBehaviour
    {
        [Header("Moon 5: Ice Weather")]
        [SerializeField] int blizzardCount = 7;
        [SerializeField] int snowDriftCount = 6;
        [SerializeField] int iceFogCount = 5;
        [SerializeField] int frostCloudCount = 8;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Blizzards - heavy snowfall
            for (int i = 0; i < blizzardCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(8f, 15f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"Blizzard_{i}", pos, new Vector3(26f, 8f, 26f), "Blizzard", new Color(0.95f, 0.95f, 1f, 0.5f), 70);
            }

            // Snow Drifts - swirling ground snow
            for (int i = 0; i < snowDriftCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0.5f,
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"SnowDrift_{i}", pos, new Vector3(22f, 2f, 22f), "SnowDrift", new Color(0.9f, 0.95f, 1f, 0.4f), 50);
            }

            // Ice Fog - thick freezing fog
            for (int i = 0; i < iceFogCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(2f, 6f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"IceFog_{i}", pos, new Vector3(24f, 10f, 24f), "IceFog", new Color(0.85f, 0.9f, 1f, 0.25f), 35);
            }

            // Frost Clouds - elevated ice crystals
            for (int i = 0; i < frostCloudCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(5f, 12f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"FrostCloud_{i}", pos, new Vector3(18f, 6f, 18f), "FrostCloud", new Color(0.9f, 0.95f, 1f, 0.3f), 45);
            }

            Debug.Log($"❄️ Moon5WeatherSystem spawned {weatherEffects.Count} weather effects");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "IceFog" ? 0.6f : 0.15f;
            main.startLifetime = weatherType == "Blizzard" ? 3f : 4f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = scale;

            if (weatherType == "Blizzard")
            {
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.x = Random.Range(-2f, 2f);
                velocity.y = -4f;
            }
            else if (weatherType == "SnowDrift")
            {
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.x = Random.Range(-3f, -1f);
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
