using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7: The Abyssal Depths - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns underwater-themed particles: bubbles, floating particles, bioluminescence
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon7AmbientParticles : MonoBehaviour
    {
        [Header("Underwater Atmosphere")]
        [SerializeField] int bubbleStreamCount = 12;
        [SerializeField] int floatingDebrisCount = 10;
        [SerializeField] int biolumSparkleCount = 18;
        [SerializeField] int currentDriftCount = 8;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Bubble streams (rising from seafloor)
            for (int i = 0; i < bubbleStreamCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Bubble_Stream_{i}", pos, new Color(0.8f, 0.9f, 1f, 0.4f), 0.2f, 40, 8f, 1.5f);
            }

            // Floating debris (tiny particles suspended in water)
            for (int i = 0; i < floatingDebrisCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(3f, 12f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Floating_Debris_{i}", pos, new Color(0.6f, 0.7f, 0.8f, 0.5f), 0.15f, 25, 6f, 0.3f);
            }

            // Bioluminescent sparkles (glowing plankton effect)
            for (int i = 0; i < biolumSparkleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(1f, 15f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Biolum_Sparkle_{i}", pos, new Color(0.2f, 0.8f, 1f), 0.1f, 30, 5f, 0.4f);
            }

            // Current drift (horizontal water movement particles)
            for (int i = 0; i < currentDriftCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(5f, 10f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Current_Drift_{i}", pos, new Color(0.5f, 0.6f, 0.8f, 0.3f), 0.3f, 20, 7f, 2f);
            }

            Debug.Log($"🌊 UNDERWATER PARTICLES: {particleSystems.Count} ambient particle systems active");
        }

        void CreateParticleSystem(string name, Vector3 position, Color color, float size, int emissionRate, float lifetime, float speed)
        {
            GameObject psObj = new GameObject(name);
            psObj.transform.position = position;
            psObj.transform.parent = transform;

            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startSize = size;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = emissionRate;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 4f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.material.color = color;

            particleSystems.Add(psObj);
        }

        void OnDestroy()
        {
            foreach (var ps in particleSystems)
            {
                if (ps != null) Destroy(ps);
            }
            particleSystems.Clear();
        }
    }
}
