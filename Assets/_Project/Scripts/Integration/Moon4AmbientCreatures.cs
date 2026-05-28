using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4: The Sunscorched Oasis - Ambient Creatures
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns desert-themed ambient creatures: scorpions, vultures, sand snakes, desert beetles
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon4AmbientCreatures : MonoBehaviour
    {
        [Header("Desert Creatures")]
        [SerializeField] int scorpionCount = 12;
        [SerializeField] int vultureCount = 8;
        [SerializeField] int sandSnakeCount = 10;
        [SerializeField] int desertBeetleCount = 15;

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // Scorpions (ground scuttlers)
            for (int i = 0; i < scorpionCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.3f,
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Scorpion_{i}", pos, new Vector3(0.6f, 0.3f, 0.8f), new Color(0.6f, 0.4f, 0.2f), 2f);
            }

            // Vultures (high circling)
            for (int i = 0; i < vultureCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(20f, 35f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Vulture_{i}", pos, new Vector3(1f, 0.4f, 1.2f), new Color(0.3f, 0.2f, 0.2f), 6f);
            }

            // Sand snakes (slithering ground)
            for (int i = 0; i < sandSnakeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.1f,
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Sand_Snake_{i}", pos, new Vector3(0.3f, 0.2f, 1.2f), new Color(0.8f, 0.7f, 0.5f), 3f);
            }

            // Desert beetles (small ground insects)
            for (int i = 0; i < desertBeetleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.2f,
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Desert_Beetle_{i}", pos, new Vector3(0.3f, 0.2f, 0.4f), new Color(0.2f, 0.2f, 0.1f), 1.5f);
            }

            Debug.Log($"🦂 DESERT CREATURES: {creatures.Count} ambient creatures adding life to the desert");
        }

        void CreateCreature(string name, Vector3 position, Vector3 scale, Color color, float speed)
        {
            GameObject creature = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            creature.name = name;
            creature.transform.position = position;
            creature.transform.localScale = scale;
            creature.transform.parent = transform;
            creature.tag = "Creature";

            Renderer renderer = creature.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Smoothness", 0.4f);
            renderer.material = mat;

            CreaturePatrol patrol = creature.AddComponent<CreaturePatrol>();
            patrol.patrolSpeed = speed;
            patrol.patrolRadius = Random.Range(15f, 35f);
            patrol.startPosition = position;

            Destroy(creature.GetComponent<CapsuleCollider>());

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
