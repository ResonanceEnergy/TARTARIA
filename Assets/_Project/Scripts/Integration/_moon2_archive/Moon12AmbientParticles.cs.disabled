using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 12: The Umbral Sanctum - Ambient Particle Systems
    /// Execution order: -68 (after EnvironmentDecorator -70)
    /// Spawns shadow-themed particles: shadow wisps, void particles, darkness tendrils, umbral motes
    /// </summary>
    [DefaultExecutionOrder(-68)]
    public class Moon12AmbientParticles : MonoBehaviour
    {
        [Header("Shadow Atmosphere")]
        [SerializeField] int shadowWispCount = 14;
        [SerializeField] int voidParticleCount = 12;
        [SerializeField] int darknessTendrilCount = 8;
        [SerializeField] int umbralMoteCount = 10;

        List<GameObject> particleSystems = new List<GameObject>();

        void Start()
        {
            SpawnAmbientParticles();
        }

        void SpawnAmbientParticles()
        {
            // Shadow wisps (near-black drifting particles)
            for (int i = 0; i < shadowWispCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(2f, 10f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Shadow_Wisp_{i}", pos, new Color(0.05f, 0.05f, 0.1f), 0.4f, 25, 6f, 0.6f);
            }

            // Void particles (light-absorbing darkness)
            for (int i = 0; i < voidParticleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(1f, 8f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Void_Particle_{i}", pos, new Color(0.02f, 0.02f, 0.05f, 0.8f), 0.5f, 20, 5f, 0.4f);
            }

            // Darkness tendrils (snaking shadow trails)
            for (int i = 0; i < darknessTendrilCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(0.5f, 5f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Darkness_Tendril_{i}", pos, new Color(0.1f, 0.05f, 0.1f, 0.7f), 0.6f, 18, 7f, 1f);
            }

            // Umbral motes (faint dark sparkles)
            for (int i = 0; i < umbralMoteCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-180f, 180f),
                    Random.Range(3f, 12f),
                    Random.Range(-180f, 180f)
                );
                CreateParticleSystem($"Umbral_Mote_{i}", pos, new Color(0.15f, 0.1f, 0.15f), 0.2f, 22, 5f, 0.5f);
            }

            Debug.Log($"🌑 SHADOW PARTICLES: {particleSystems.Count} ambient particle systems active");
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
