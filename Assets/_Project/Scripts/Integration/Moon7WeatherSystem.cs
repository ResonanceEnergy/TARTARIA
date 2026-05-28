using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-51)]
    public class Moon7WeatherSystem : MonoBehaviour
    {
        [Header("Moon 7: Underwater Weather")]
        [SerializeField] int currentFlowCount = 7;
        [SerializeField] int bubbleStreamCount = 8;
        [SerializeField] int bioluminescenceWaveCount = 5;
        [SerializeField] int planktonCloudCount = 6;

        List<GameObject> weatherEffects = new List<GameObject>();

        void Start()
        {
            SpawnWeatherSystems();
        }

        void SpawnWeatherSystems()
        {
            // Current Flows - directional water movement
            for (int i = 0; i < currentFlowCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(2f, 12f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"CurrentFlow_{i}", pos, new Vector3(24f, 8f, 24f), "CurrentFlow", new Color(0.4f, 0.6f, 0.8f, 0.25f), 40);
            }

            // Bubble Streams - rising air bubbles
            for (int i = 0; i < bubbleStreamCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(0f, 8f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"BubbleStream_{i}", pos, new Vector3(6f, 12f, 6f), "BubbleStream", new Color(0.8f, 0.9f, 1f, 0.4f), 50);
            }

            // Bioluminescence Waves - glowing plankton
            for (int i = 0; i < bioluminescenceWaveCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(1f, 6f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"BioluminescenceWave_{i}", pos, new Vector3(22f, 6f, 22f), "BioluminescenceWave", new Color(0.2f, 0.8f, 0.9f, 0.6f), 45);
            }

            // Plankton Clouds - drifting particles
            for (int i = 0; i < planktonCloudCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(3f, 10f),
                    Random.Range(-70f, 70f)
                );
                CreateWeatherEffect($"PlanktonCloud_{i}", pos, new Vector3(20f, 8f, 20f), "PlanktonCloud", new Color(0.6f, 0.75f, 0.8f, 0.3f), 35);
            }

            Debug.Log($"🌊 Moon7WeatherSystem spawned {weatherEffects.Count} weather effects");
        }

        GameObject CreateWeatherEffect(string name, Vector3 position, Vector3 scale, string weatherType, Color particleColor, int particleCount)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;

            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = particleColor;
            main.startSize = weatherType == "BubbleStream" ? 0.2f : (weatherType == "BioluminescenceWave" ? 0.15f : 0.3f);
            main.startLifetime = 4f;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.rateOverTime = particleCount / 2f;

            var shape = ps.shape;
            shape.shapeType = weatherType == "BubbleStream" ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Box;
            shape.scale = scale;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            
            if (weatherType == "CurrentFlow")
            {
                velocity.x = Random.Range(-3f, 3f);
            }
            else if (weatherType == "BubbleStream")
            {
                velocity.y = 3f;
            }
            else if (weatherType == "BioluminescenceWave")
            {
                velocity.x = Random.Range(-1f, 1f);
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
