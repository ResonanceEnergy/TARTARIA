using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 11: The Prismatic Nexus - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns prismatic-themed particles: rainbow particles, light refractions, spectrum sparkles, color beams
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon11AmbientParticles : MonoBehaviour
    {
        [Header("Prismatic Atmosphere")]
        [SerializeField] int rainbowParticleCount = 15;
        [SerializeField] int lightRefractionCount = 12;
        [SerializeField] int spectrumSparkleCount = 18;
        [SerializeField] int colorBeamCount = 10;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Rainbow particles (full spectrum colored motes)
            Color[] rainbowColors = { Color.red, new Color(1f, 0.5f, 0f), Color.yellow, Color.green, Color.cyan, Color.blue, new Color(0.5f, 0f, 1f) };
            for (int i = 0; i < rainbowParticleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(2f, 15f),
                    Random.Range(-180f, 180f)
                );
                Color color = rainbowColors[i % rainbowColors.Length];
                CreateParticleSystem($"Rainbow_Particle_{i}", pos, color, 0.3f, 25, 6f, 0.8f);
            }

            // Light refractions (prismatic splitting particles)
            for (int i = 0; i < lightRefractionCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(5f, 20f),
                    Random.Range(-180f, 180f)
                );
                Color color = rainbowColors[Random.Range(0, rainbowColors.Length)];
                CreateParticleSystem($"Light_Refraction_{i}", pos, color * 0.8f, 0.5f, 20, 5f, 1f);
            }

            // Spectrum sparkles (multi-colored glittering)
            for (int i = 0; i < spectrumSparkleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(1f, 12f),
                    Random.Range(-180f, 180f)
                );
                Color color = rainbowColors[Random.Range(0, rainbowColors.Length)];
                CreateParticleSystem($"Spectrum_Sparkle_{i}", pos, color, 0.15f, 30, 4f, 0.5f);
            }

            // Color beams (vertical rays of pure color)
            for (int i = 0; i < colorBeamCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(10f, 25f),
                    Random.Range(-180f, 180f)
                );
                Color color = rainbowColors[i % rainbowColors.Length];
                CreateParticleSystem($"Color_Beam_{i}", pos, color * 0.9f, 1f, 20, 7f, 1.5f);
            }

            Debug.Log($"🌈 PRISMATIC PARTICLES: {particleSystems.Count} ambient particle systems active");
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
            shape.radius = 4.5f;

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
