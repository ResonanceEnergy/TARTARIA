using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 13: The Aether Convergence - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns final-level particles: aether sparkles, convergence energy, tribute beams (12 moon colors)
    /// THE CULMINATION OF ALL 12 MOONS - ULTIMATE PARTICLE SYMPHONY
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon13AmbientParticles : MonoBehaviour
    {
        [Header("Aether Convergence Atmosphere")]
        [SerializeField] int aetherSparkleCount = 20;
        [SerializeField] int convergenceEnergyCount = 15;
        [SerializeField] int tributeBeamCount = 12; // One per moon!
        [SerializeField] int celestialOrbCount = 10;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Aether sparkles (brilliant cyan-white particles everywhere)
            for (int i = 0; i < aetherSparkleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(2f, 25f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Aether_Sparkle_{i}", pos, new Color(0.8f, 0.95f, 1f), 0.2f, 35, 6f, 1f);
            }

            // Convergence energy (pulsing waves of power)
            for (int i = 0; i < convergenceEnergyCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(5f, 15f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Convergence_Energy_{i}", pos, new Color(0.6f, 0.9f, 1f, 0.7f), 1.2f, 25, 7f, 1.5f);
            }

            // Tribute beams (12 colored beams representing each moon)
            Color[] moonColors = {
                new Color(0.2f, 0.8f, 0.2f),    // Moon 3 - Jungle Green
                new Color(0.9f, 0.8f, 0.5f),    // Moon 4 - Desert Tan
                new Color(0.6f, 0.8f, 1f),      // Moon 5 - Ice Blue
                new Color(1f, 0.3f, 0f),        // Moon 6 - Lava Orange
                new Color(0.1f, 0.4f, 0.8f),    // Moon 7 - Deep Blue
                Color.white,                     // Moon 8 - Sky White
                new Color(0.5f, 0.1f, 0.5f),    // Moon 9 - Corruption Purple
                new Color(0.7f, 0.7f, 0.8f),    // Moon 10 - Time Gray
                new Color(1f, 0.5f, 0.8f),      // Moon 11 - Prismatic (pink variant)
                new Color(0.1f, 0.05f, 0.1f),   // Moon 12 - Shadow Black
                new Color(0.9f, 0.95f, 1f),     // Moon 13 - Aether Cyan
                Color.yellow                     // Bonus - Golden tribute
            };

            for (int i = 0; i < tributeBeamCount; i++)
            {
                Vector3 pos = new Vector3(
                    Mathf.Cos(i * Mathf.PI * 2f / 12f) * 150f,
                    Random.Range(15f, 30f),
                    Mathf.Sin(i * Mathf.PI * 2f / 12f) * 150f
                );
                CreateParticleSystem($"Tribute_Beam_Moon{i+3}", pos, moonColors[i], 0.8f, 30, 10f, 2f);
            }

            // Celestial orbs (floating spheres of light)
            for (int i = 0; i < celestialOrbCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(10f, 20f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Celestial_Orb_{i}", pos, new Color(1f, 1f, 0.9f), 0.5f, 20, 8f, 0.4f);
            }

            Debug.Log("═══════════════════════════════════════════════════════════");
            Debug.Log("  ✨ FINAL LEVEL PARTICLES ACTIVATED ✨");
            Debug.Log($"  {particleSystems.Count} particle systems honoring all 12 moons");
            Debug.Log("═══════════════════════════════════════════════════════════");
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
            shape.radius = 5f;

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
