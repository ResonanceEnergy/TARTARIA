using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    [DefaultExecutionOrder(-51)]
    public class Moon3WeatherSystem : MonoBehaviour
    {
        [Header("Moon 3: Jungle Weather")]
        [SerializeField] int rainZoneCount = 8;
        [SerializeField] int fogBankCount = 6;
        [SerializeField] int windGustCount = 5;
        [SerializeField] int mistLayerCount = 7;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Rain Zones - falling rain particles
            for (int i = 0; i < rainZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(8f, 15f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"RainZone_{i}", pos, new Vector3(20f, 3f, 20f), "Rain", new Color(0.7f, 0.8f, 0.9f, 0.3f), 50);
            }

            // Fog Banks - thick jungle fog
            for (int i = 0; i < fogBankCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(2f, 5f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"FogBank_{i}", pos, new Vector3(25f, 8f, 25f), "Fog", new Color(0.8f, 0.9f, 0.8f, 0.2f), 30);
            }

            // Wind Gusts - leaf particles in wind
            for (int i = 0; i < windGustCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(4f, 10f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"WindGust_{i}", pos, new Vector3(18f, 6f, 18f), "WindGust", new Color(0.3f, 0.6f, 0.2f, 0.4f), 40);
            }

            // Mist Layers - ground-level mist
            for (int i = 0; i < mistLayerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    1f,
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"MistLayer_{i}", pos, new Vector3(22f, 4f, 22f), "Mist", new Color(0.9f, 0.95f, 0.9f, 0.15f), 35);
            }

            Debug.Log($"🌧️ Moon3WeatherSystem spawned {weatherEffects.Count} weather effects");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "Rain" ? 0.1f : (weatherType == "WindGust" ? 0.3f : 0.5f);
            main.startLifetime = weatherType == "Rain" ? 2f : 5f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = scale;

            if (weatherType == "Rain")
            {
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.y = -5f;
            }
            else if (weatherType == "WindGust")
            {
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.x = Random.Range(-3f, 3f);
                velocity.z = Random.Range(-3f, 3f);
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
