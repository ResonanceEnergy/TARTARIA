using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 9: The Blighted Wastes - Ambient Creatures
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns corruption-themed ambient creatures: corruption wisps, void crawlers, shadow creatures, blighted moths
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon9AmbientCreatures : MonoBehaviour
    {
        [Header("Corruption Creatures")]
        [SerializeField] int corruptionWispCount = 20;
        [SerializeField] int voidCrawlerCount = 10;
        [SerializeField] int shadowCreatureCount = 12;
        [SerializeField] int blightedMothCount = 14;

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // Corruption wisps (floating dark energy)
            for (int i = 0; i < corruptionWispCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(1f, 8f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Corruption_Wisp_{i}", pos, new Vector3(0.5f, 0.5f, 0.5f), new Color(0.5f, 0f, 0.5f), 3f);
            }

            // Void crawlers (ground skittering)
            for (int i = 0; i < voidCrawlerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.4f,
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Void_Crawler_{i}", pos, new Vector3(0.7f, 0.4f, 0.9f), new Color(0.3f, 0f, 0.3f), 2.5f);
            }

            // Shadow creatures (mid-level lurkers)
            for (int i = 0; i < shadowCreatureCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(3f, 12f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Shadow_Creature_{i}", pos, new Vector3(0.8f, 1.2f, 0.8f), new Color(0.2f, 0f, 0.4f), 4f);
            }

            // Blighted moths (corrupted flyers)
            for (int i = 0; i < blightedMothCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(4f, 15f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Blighted_Moth_{i}", pos, new Vector3(0.6f, 0.3f, 0.8f), new Color(0.4f, 0.1f, 0.4f), 5f);
            }

            Debug.Log($"☠️ CORRUPTION CREATURES: {creatures.Count} ambient creatures adding life to the blighted wastes");
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
            mat.SetFloat("_Metallic", 0.6f);
            mat.SetFloat("_Smoothness", 0.7f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.8f);
            renderer.material = mat;

            CreaturePatrol patrol = creature.AddComponent<CreaturePatrol>();
            patrol.patrolSpeed = speed;
            patrol.patrolRadius = Random.Range(10f, 25f);
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
