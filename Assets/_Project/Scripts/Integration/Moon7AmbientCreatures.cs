using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7: The Abyssal Depths - Ambient Creatures
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns underwater-themed ambient creatures: fish schools, jellyfish, rays, anglerfish
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon7AmbientCreatures : MonoBehaviour
    {
        [Header("Underwater Creatures")]
        [SerializeField] int fishSchoolSize = 25;
        [SerializeField] int jellyfishCount = 15;
        [SerializeField] int rayCount = 8;
        [SerializeField] int anglerfishCount = 6;

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // Fish schools (small swimming clusters)
            for (int i = 0; i < fishSchoolSize; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(5f, 20f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Fish_{i}", pos, new Vector3(0.3f, 0.2f, 0.5f), new Color(0.6f, 0.7f, 0.8f), 4f);
            }

            // Jellyfish (graceful floating)
            for (int i = 0; i < jellyfishCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(8f, 18f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Jellyfish_{i}", pos, new Vector3(0.8f, 1.2f, 0.8f), new Color(0.4f, 0.6f, 0.9f, 0.7f), 1.5f);
            }

            // Rays (gliding majestically)
            for (int i = 0; i < rayCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(10f, 25f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Ray_{i}", pos, new Vector3(2f, 0.3f, 1.5f), new Color(0.5f, 0.6f, 0.7f), 3f);
            }

            // Anglerfish (deep lurkers with bio-glow)
            for (int i = 0; i < anglerfishCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(2f, 8f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Anglerfish_{i}", pos, new Vector3(0.8f, 0.6f, 1f), new Color(0.2f, 0.3f, 0.4f), 2f);
            }

            Debug.Log($"🌊 UNDERWATER CREATURES: {creatures.Count} ambient creatures adding life to the abyssal depths");
        }

        void CreateCreature(string name, Vector3 position, Vector3 scale, Color color, float speed)
        {
            GameObject creature = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            creature.name = name;
            creature.transform.position = position;
            creature.transform.localScale = scale;
            creature.transform.parent = transform;
            creature.tag = "Creature";

            Renderer renderer = creature.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.3f);
            mat.SetFloat("_Smoothness", 0.8f);
            if (name.Contains("Anglerfish") || name.Contains("Jellyfish"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 0.5f);
            }
            renderer.material = mat;

            CreaturePatrol patrol = creature.AddComponent<CreaturePatrol>();
            patrol.patrolSpeed = speed;
            patrol.patrolRadius = Random.Range(15f, 30f);
            patrol.startPosition = position;

            Destroy(creature.GetComponent<SphereCollider>());

            creatures.Add(creature);
        }

        void OnDestroy()
        {
            foreach (var creature in creatures)
            {
                if (creature != null) Destroy(creature);
            }
            creatures.Clear();
        }
    }
}
