using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 3: The Verdant Labyrinth - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns jungle-themed particles: fireflies, falling leaves, mist
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon3AmbientParticles : MonoBehaviour
    {
        [Header("Jungle Atmosphere")]
        [SerializeField] int fireflyClusterCount = 12;
        [SerializeField] int fallingLeafEmitterCount = 8;
        [SerializeField] int mistZoneCount = 6;
        [SerializeField] int sporeCloudCount = 5;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Firefly clusters (glowing yellow-green drifting particles)
            for (int i = 0; i < fireflyClusterCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(3f, 12f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Firefly_Cluster_{i}", pos, Color.yellow, 0.1f, 25, 5f, 0.8f);
            }

            // Falling leaves (green to brown gradient, slowly drifting down)
            for (int i = 0; i < fallingLeafEmitterCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(20f, 30f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Falling_Leaves_{i}", pos, new Color(0.2f, 0.6f, 0.1f), 0.4f, 15, 8f, 0.5f);
            }

            // Mist zones (low-lying fog, slow movement)
            for (int i = 0; i < mistZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Mist_Zone_{i}", pos, new Color(0.7f, 0.8f, 0.7f, 0.3f), 2f, 20, 4f, 0.3f);
            }

            // Spore clouds (floating upward, glowing slightly)
            for (int i = 0; i < sporeCloudCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(1f, 3f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Spore_Cloud_{i}", pos, new Color(0.8f, 1f, 0.6f), 0.3f, 30, 6f, 0.6f);
            }

            Debug.Log($"🌿 JUNGLE PARTICLES: {particleSystems.Count} ambient particle systems active");
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
            shape.radius = 3f;

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
