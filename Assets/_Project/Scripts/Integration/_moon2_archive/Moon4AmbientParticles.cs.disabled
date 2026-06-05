using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 4: The Sunscorched Oasis - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns desert-themed particles: sand swirls, heat waves, dust devils
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon4AmbientParticles : MonoBehaviour
    {
        [Header("Desert Atmosphere")]
        [SerializeField] int sandSwirlCount = 10;
        [SerializeField] int heatWaveCount = 8;
        [SerializeField] int dustDevilCount = 4;
        [SerializeField] int sandStreamCount = 6;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Sand swirls (tan particles blowing across ground)
            for (int i = 0; i < sandSwirlCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(0.2f, 1f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Sand_Swirl_{i}", pos, new Color(0.9f, 0.8f, 0.6f), 0.3f, 40, 3f, 2f);
            }

            // Heat waves (distortion effect, shimmer near ground)
            for (int i = 0; i < heatWaveCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Heat_Wave_{i}", pos, new Color(1f, 0.9f, 0.7f, 0.2f), 1.5f, 15, 4f, 0.5f);
            }

            // Dust devils (spiraling sand columns)
            for (int i = 0; i < dustDevilCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    0f,
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Dust_Devil_{i}", pos, new Color(0.85f, 0.75f, 0.55f, 0.6f), 0.5f, 60, 6f, 4f);
            }

            // Sand streams (wind-blown sand trails)
            for (int i = 0; i < sandStreamCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(1f, 5f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Sand_Stream_{i}", pos, new Color(0.95f, 0.85f, 0.65f), 0.2f, 30, 5f, 3f);
            }

            Debug.Log($"🏜️ DESERT PARTICLES: {particleSystems.Count} ambient particle systems active");
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
