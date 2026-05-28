using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 8: The Celestial Spires - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns sky-themed particles: cloud wisps, wind streaks, light rays, ethereal motes
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon8AmbientParticles : MonoBehaviour
    {
        [Header("Sky Atmosphere")]
        [SerializeField] int cloudWispCount = 10;
        [SerializeField] int windStreakCount = 12;
        [SerializeField] int lightRayCount = 8;
        [SerializeField] int etherealMoteCount = 15;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Cloud wisps (soft white drifting clouds)
            for (int i = 0; i < cloudWispCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(15f, 25f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Cloud_Wisp_{i}", pos, new Color(1f, 1f, 1f, 0.4f), 2f, 15, 10f, 0.5f);
            }

            // Wind streaks (fast-moving horizontal trails)
            for (int i = 0; i < windStreakCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(10f, 20f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Wind_Streak_{i}", pos, new Color(0.9f, 0.95f, 1f, 0.3f), 0.5f, 35, 4f, 6f);
            }

            // Light rays (vertical beams of sunlight)
            for (int i = 0; i < lightRayCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(25f, 35f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Light_Ray_{i}", pos, new Color(1f, 1f, 0.9f, 0.5f), 1.5f, 20, 8f, 1f);
            }

            // Ethereal motes (glowing sparkles floating gently)
            for (int i = 0; i < etherealMoteCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(5f, 15f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Ethereal_Mote_{i}", pos, new Color(0.9f, 0.95f, 1f), 0.15f, 25, 6f, 0.4f);
            }

            Debug.Log($"☁️ SKY PARTICLES: {particleSystems.Count} ambient particle systems active");
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
