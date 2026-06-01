using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-51)]
    public class Moon11WeatherSystem : MonoBehaviour
    {
        [Header("Moon 11: Prismatic Weather")]
        [SerializeField] int rainbowShowerCount = 8;
        [SerializeField] int crystalDustCount = 7;
        [SerializeField] int spectrumWaveCount = 6;
        [SerializeField] int prismBeamCount = 9;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Rainbow Showers - multi-colored particles
            Color[] rainbowColors = new Color[]
            {
                new Color(1f, 0f, 0f, 0.5f),      // Red
                new Color(1f, 0.5f, 0f, 0.5f),    // Orange
                new Color(1f, 1f, 0f, 0.5f),      // Yellow
                new Color(0f, 1f, 0f, 0.5f),      // Green
                new Color(0f, 0.5f, 1f, 0.5f),    // Blue
                new Color(0.5f, 0f, 1f, 0.5f),    // Indigo
                new Color(1f, 0f, 1f, 0.5f),      // Violet
                new Color(1f, 0.75f, 0.8f, 0.5f)  // Pink
            };

            for (int i = 0; i < rainbowShowerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(10f, 18f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"RainbowShower_{i}", pos, new Vector3(20f, 6f, 20f), "RainbowShower", rainbowColors[i % rainbowColors.Length], 52);
            }

            // Crystal Dust - shimmering crystalline particles
            for (int i = 0; i < crystalDustCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(3f, 12f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"CrystalDust_{i}", pos, new Vector3(22f, 8f, 22f), "CrystalDust", new Color(0.9f, 0.9f, 1f, 0.6f), 45);
            }

            // Spectrum Waves - shifting color waves
            for (int i = 0; i < spectrumWaveCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(4f, 10f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"SpectrumWave_{i}", pos, new Vector3(26f, 8f, 26f), "SpectrumWave", rainbowColors[i % rainbowColors.Length], 48);
            }

            // Prism Beams - light ray particles
            for (int i = 0; i < prismBeamCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(6f, 14f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"PrismBeam_{i}", pos, new Vector3(8f, 12f, 8f), "PrismBeam", rainbowColors[i % rainbowColors.Length], 38);
            }

            Debug.Log($"🌈 Moon11WeatherSystem spawned {weatherEffects.Count} weather effects");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "CrystalDust" ? 0.15f : (weatherType == "PrismBeam" ? 0.3f : 0.25f);
            main.startLifetime = 3.5f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2f;

            var shape = ps.shape;
            shape.shapeType = weatherType == "PrismBeam" ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Box;
            shape.scale = scale;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            
            if (weatherType == "RainbowShower")
            {
                velocity.y = -4f;
            }
            else if (weatherType == "PrismBeam")
            {
                velocity.y = Random.Range(-3f, 3f);
            }
            else if (weatherType == "SpectrumWave")
            {
                velocity.x = Random.Range(-2f, 2f);
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
