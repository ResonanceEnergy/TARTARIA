using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    [DefaultExecutionOrder(-51)]
    public class Moon9WeatherSystem : MonoBehaviour
    {
        [Header("Moon 9: Corruption Weather")]
        [SerializeField] int blightFalloutCount = 7;
        [SerializeField] int voidMiasmaCount = 6;
        [SerializeField] int corruptionPulseCount = 5;
        [SerializeField] int shadowTendrilCount = 8;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Blight Fallout - falling corruption particles
            for (int i = 0; i < blightFalloutCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(10f, 16f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"BlightFallout_{i}", pos, new Vector3(22f, 6f, 22f), "BlightFallout", new Color(0.4f, 0.2f, 0.5f, 0.5f), 50);
            }

            // Void Miasma - thick corruptive fog
            for (int i = 0; i < voidMiasmaCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(2f, 8f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"VoidMiasma_{i}", pos, new Vector3(26f, 10f, 26f), "VoidMiasma", new Color(0.3f, 0.15f, 0.4f, 0.4f), 45);
            }

            // Corruption Pulses - rhythmic dark waves
            for (int i = 0; i < corruptionPulseCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(3f, 10f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"CorruptionPulse_{i}", pos, new Vector3(20f, 8f, 20f), "CorruptionPulse", new Color(0.5f, 0.2f, 0.6f, 0.6f), 40);
            }

            // Shadow Tendrils - rising dark wisps
            for (int i = 0; i < shadowTendrilCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(0f, 6f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"ShadowTendril_{i}", pos, new Vector3(8f, 12f, 8f), "ShadowTendril", new Color(0.2f, 0.1f, 0.3f, 0.7f), 55);
            }

            Debug.Log($"💀 Moon9WeatherSystem spawned {weatherEffects.Count} weather effects");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "VoidMiasma" ? 0.7f : (weatherType == "BlightFallout" ? 0.2f : 0.35f);
            main.startLifetime = 4f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2f;

            var shape = ps.shape;
            shape.shapeType = weatherType == "ShadowTendril" ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Box;
            shape.scale = scale;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            
            if (weatherType == "BlightFallout")
            {
                velocity.y = -3f;
            }
            else if (weatherType == "ShadowTendril")
            {
                velocity.y = 2f;
            }
            else if (weatherType == "CorruptionPulse")
            {
                velocity.x = Random.Range(-1f, 1f);
                velocity.y = Random.Range(-1f, 1f);
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
