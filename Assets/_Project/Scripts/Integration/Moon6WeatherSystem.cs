using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    [DefaultExecutionOrder(-51)]
    public class Moon6WeatherSystem : MonoBehaviour
    {
        [Header("Moon 6: Lava Weather")]
        [SerializeField] int emberShowerCount = 7;
        [SerializeField] int ashCloudCount = 6;
        [SerializeField] int heatDistortionCount = 5;
        [SerializeField] int smokeColumnCount = 8;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Ember Showers - falling fire particles
            for (int i = 0; i < emberShowerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(10f, 18f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"EmberShower_{i}", pos, new Vector3(20f, 4f, 20f), "EmberShower", new Color(1f, 0.5f, 0.1f, 0.7f), 55);
            }

            // Ash Clouds - dark volcanic ash
            for (int i = 0; i < ashCloudCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(5f, 10f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"AshCloud_{i}", pos, new Vector3(26f, 8f, 26f), "AshCloud", new Color(0.2f, 0.15f, 0.1f, 0.4f), 45);
            }

            // Heat Distortion - shimmering air near lava
            for (int i = 0; i < heatDistortionCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(1f, 4f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"HeatDistortion_{i}", pos, new Vector3(22f, 6f, 22f), "HeatDistortion", new Color(1f, 0.7f, 0.3f, 0.15f), 30);
            }

            // Smoke Columns - rising volcanic smoke
            for (int i = 0; i < smokeColumnCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(2f, 8f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"SmokeColumn_{i}", pos, new Vector3(8f, 15f, 8f), "SmokeColumn", new Color(0.3f, 0.25f, 0.2f, 0.5f), 50);
            }

            Debug.Log($"🔥 Moon6WeatherSystem spawned {weatherEffects.Count} weather effects");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "HeatDistortion" ? 0.7f : (weatherType == "EmberShower" ? 0.1f : 0.4f);
            main.startLifetime = weatherType == "SmokeColumn" ? 5f : 3f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2.5f;

            var shape = ps.shape;
            shape.shapeType = weatherType == "SmokeColumn" ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Box;
            shape.scale = scale;

            if (weatherType == "EmberShower")
            {
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.y = -6f;
            }
            else if (weatherType == "SmokeColumn")
            {
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.y = 4f;
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
