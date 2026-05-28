using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 5: The Frostbound Citadel - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns ice-themed particles: snow, frost sparkles, aurora shimmer
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon5AmbientParticles : MonoBehaviour
    {
        [Header("Ice Atmosphere")]
        [SerializeField] int snowfallZoneCount = 10;
        [SerializeField] int frostSparkleCount = 15;
        [SerializeField] int auroraShimmerCount = 8;
        [SerializeField] int iceWindCount = 6;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Snowfall zones (gentle falling snow)
            for (int i = 0; i < snowfallZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(20f, 30f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Snowfall_{i}", pos, Color.white, 0.3f, 50, 10f, 1f);
            }

            // Frost sparkles (glittering ice crystals in air)
            for (int i = 0; i < frostSparkleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(2f, 8f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Frost_Sparkle_{i}", pos, new Color(0.8f, 0.9f, 1f), 0.15f, 20, 4f, 0.5f);
            }

            // Aurora shimmer (ethereal light particles high above)
            for (int i = 0; i < auroraShimmerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(25f, 35f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Aurora_Shimmer_{i}", pos, new Color(0.4f, 0.8f, 1f, 0.5f), 1f, 15, 6f, 0.3f);
            }

            // Ice wind (horizontal blowing ice particles)
            for (int i = 0; i < iceWindCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(1f, 5f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Ice_Wind_{i}", pos, new Color(0.9f, 0.95f, 1f, 0.6f), 0.2f, 35, 5f, 4f);
            }

            Debug.Log($"❄️ ICE PARTICLES: {particleSystems.Count} ambient particle systems active");
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
