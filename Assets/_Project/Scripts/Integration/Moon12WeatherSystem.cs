using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-51)]
    public class Moon12WeatherSystem : MonoBehaviour
    {
        [Header("Moon 12: Shadow Weather")]
        [SerializeField] int darknessFallCount = 7;
        [SerializeField] int umbralMistCount = 6;
        [SerializeField] int voidPocketCount = 5;
        [SerializeField] int shadowWaveCount = 8;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Darkness Fall - descending shadow particles
            for (int i = 0; i < darknessFallCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(10f, 16f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"DarknessFall_{i}", pos, new Vector3(22f, 8f, 22f), "DarknessFall", new Color(0.1f, 0.1f, 0.15f, 0.6f), 50);
            }

            // Umbral Mist - thick shadow fog
            for (int i = 0; i < umbralMistCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(2f, 8f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"UmbralMist_{i}", pos, new Vector3(28f, 10f, 28f), "UmbralMist", new Color(0.15f, 0.15f, 0.2f, 0.45f), 42);
            }

            // Void Pockets - areas of absolute darkness
            for (int i = 0; i < voidPocketCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(3f, 10f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"VoidPocket_{i}", pos, new Vector3(16f, 12f, 16f), "VoidPocket", new Color(0.05f, 0.05f, 0.1f, 0.8f), 36);
            }

            // Shadow Waves - rippling darkness
            for (int i = 0; i < shadowWaveCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(1f, 7f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"ShadowWave_{i}", pos, new Vector3(24f, 6f, 24f), "ShadowWave", new Color(0.12f, 0.12f, 0.18f, 0.55f), 45);
            }

            Debug.Log($"🌑 Moon12WeatherSystem spawned {weatherEffects.Count} weather effects");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "UmbralMist" ? 0.7f : (weatherType == "VoidPocket" ? 0.6f : 0.3f);
            main.startLifetime = 4f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = scale;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            
            if (weatherType == "DarknessFall")
            {
                velocity.y = -3f;
            }
            else if (weatherType == "ShadowWave")
            {
                velocity.x = Random.Range(-1.5f, 1.5f);
                velocity.y = Random.Range(-0.5f, 0.5f);
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
