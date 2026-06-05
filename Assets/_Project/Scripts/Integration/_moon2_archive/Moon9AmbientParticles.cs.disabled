using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 9: The Blighted Wastes - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns corruption-themed particles: void wisps, dark energy, corruption tendrils, toxic smoke
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon9AmbientParticles : MonoBehaviour
    {
        [Header("Corruption Atmosphere")]
        [SerializeField] int voidWispCount = 12;
        [SerializeField] int darkEnergyCount = 10;
        [SerializeField] int corruptionTendrilCount = 8;
        [SerializeField] int toxicSmokeCount = 6;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Void wisps (dark purple drifting particles)
            for (int i = 0; i < voidWispCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(2f, 10f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Void_Wisp_{i}", pos, new Color(0.3f, 0.1f, 0.4f), 0.3f, 25, 6f, 0.8f);
            }

            // Dark energy (pulsing corruption particles)
            for (int i = 0; i < darkEnergyCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(1f, 8f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Dark_Energy_{i}", pos, new Color(0.5f, 0f, 0.5f), 0.4f, 30, 5f, 1f);
            }

            // Corruption tendrils (snaking dark trails)
            for (int i = 0; i < corruptionTendrilCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(0.5f, 5f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Corruption_Tendril_{i}", pos, new Color(0.2f, 0f, 0.3f, 0.7f), 0.5f, 20, 7f, 1.5f);
            }

            // Toxic smoke (poisonous clouds near ground)
            for (int i = 0; i < toxicSmokeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(0.5f, 3f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Toxic_Smoke_{i}", pos, new Color(0.3f, 0.2f, 0.4f, 0.6f), 1.5f, 25, 6f, 0.6f);
            }

            Debug.Log($"💀 CORRUPTION PARTICLES: {particleSystems.Count} ambient particle systems active");
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
