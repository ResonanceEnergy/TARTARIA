using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 6: The Molten Forge - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns lava-themed particles: embers, ash, volcanic smoke, heat distortion
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon6AmbientParticles : MonoBehaviour
    {
        [Header("Lava Atmosphere")]
        [SerializeField] int emberClusterCount = 15;
        [SerializeField] int ashFallCount = 8;
        [SerializeField] int volcanicSmokeCount = 6;
        [SerializeField] int heatDistortionCount = 10;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Ember clusters (glowing orange particles floating upward)
            for (int i = 0; i < emberClusterCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(0.5f, 3f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Ember_Cluster_{i}", pos, new Color(1f, 0.4f, 0f), 0.2f, 35, 5f, 2f);
            }

            // Ash fall (gray particles drifting down from smoke)
            for (int i = 0; i < ashFallCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(20f, 30f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Ash_Fall_{i}", pos, new Color(0.3f, 0.3f, 0.3f, 0.6f), 0.25f, 30, 8f, 1f);
            }

            // Volcanic smoke (dark billowing clouds)
            for (int i = 0; i < volcanicSmokeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(2f, 8f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Volcanic_Smoke_{i}", pos, new Color(0.2f, 0.2f, 0.2f, 0.5f), 2f, 20, 6f, 1.5f);
            }

            // Heat distortion (shimmering near lava)
            for (int i = 0; i < heatDistortionCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Heat_Distortion_{i}", pos, new Color(1f, 0.6f, 0.2f, 0.3f), 1.2f, 25, 3f, 0.4f);
            }

            Debug.Log($"🔥 LAVA PARTICLES: {particleSystems.Count} ambient particle systems active");
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
            shape.radius = 3.5f;

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
