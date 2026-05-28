using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 5: The Frostbound Citadel - Ambient Creatures
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns ice-themed ambient creatures: ice spirits, frozen bats, snow owls, frost wisps
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon5AmbientCreatures : MonoBehaviour
    {
        [Header("Ice Creatures")]
        [SerializeField] int iceSpiritCount = 10;
        [SerializeField] int frozenBatCount = 12;
        [SerializeField] int snowOwlCount = 8;
        [SerializeField] int frostWispCount = 18;

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // Ice spirits (floating ethereal)
            for (int i = 0; i < iceSpiritCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(5f, 15f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Ice_Spirit_{i}", pos, new Vector3(0.6f, 1.2f, 0.6f), new Color(0.7f, 0.9f, 1f, 0.6f), 4f);
            }

            // Frozen bats (erratic flying)
            for (int i = 0; i < frozenBatCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(8f, 20f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Frozen_Bat_{i}", pos, new Vector3(0.5f, 0.3f, 0.7f), new Color(0.4f, 0.5f, 0.6f), 7f);
            }

            // Snow owls (majestic flying)
            for (int i = 0; i < snowOwlCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(12f, 25f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Snow_Owl_{i}", pos, new Vector3(0.8f, 0.6f, 1f), new Color(0.95f, 0.95f, 1f), 5f);
            }

            // Frost wisps (ground-level glowing)
            for (int i = 0; i < frostWispCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(0.5f, 3f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Frost_Wisp_{i}", pos, new Vector3(0.4f, 0.4f, 0.4f), new Color(0.6f, 0.8f, 1f), 2.5f);
            }

            Debug.Log($"❄️ ICE CREATURES: {creatures.Count} ambient creatures adding life to the frozen citadel");
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
            mat.SetFloat("_Metallic", 0.7f);
            mat.SetFloat("_Smoothness", 0.9f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.3f);
            renderer.material = mat;

            CreaturePatrol patrol = creature.AddComponent<CreaturePatrol>();
            patrol.patrolSpeed = speed;
            patrol.patrolRadius = Random.Range(12f, 28f);
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
