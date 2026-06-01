using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 10: The Temporal Rift - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns time-themed particles: temporal distortions, clock particles, time ripples, chrono-sparks
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon10AmbientParticles : MonoBehaviour
    {
        [Header("Temporal Atmosphere")]
        [SerializeField] int temporalDistortionCount = 10;
        [SerializeField] int clockParticleCount = 12;
        [SerializeField] int timeRippleCount = 8;
        [SerializeField] int chronoSparkCount = 15;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Temporal distortions (warped space-time particles)
            for (int i = 0; i < temporalDistortionCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(3f, 12f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Temporal_Distortion_{i}", pos, new Color(0.6f, 0.6f, 0.7f, 0.4f), 1f, 20, 5f, 0.5f);
            }

            // Clock particles (floating clock face fragments)
            for (int i = 0; i < clockParticleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(2f, 10f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Clock_Particle_{i}", pos, new Color(0.8f, 0.8f, 0.9f), 0.3f, 15, 7f, 0.3f);
            }

            // Time ripples (expanding waves of temporal energy)
            for (int i = 0; i < timeRippleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(5f, 15f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Time_Ripple_{i}", pos, new Color(0.5f, 0.7f, 0.9f, 0.5f), 1.2f, 18, 6f, 1f);
            }

            // Chrono-sparks (glittering temporal anomalies)
            for (int i = 0; i < chronoSparkCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(1f, 12f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Chrono_Spark_{i}", pos, new Color(0.7f, 0.8f, 1f), 0.15f, 25, 4f, 0.6f);
            }

            Debug.Log($"⏱️ TIME PARTICLES: {particleSystems.Count} ambient particle systems active");
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
