using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6: The Molten Forge - Ambient Creatures
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns lava-themed ambient creatures: fire sprites, lava worms, ember drakes, magma beetles
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon6AmbientCreatures : MonoBehaviour
    {
        [Header("Lava Creatures")]
        [SerializeField] int fireSpriteCount = 16;
        [SerializeField] int lavaWormCount = 8;
        [SerializeField] int emberDrakeCount = 6;
        [SerializeField] int magmaBeetleCount = 12;

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // Fire sprites (floating embers)
            for (int i = 0; i < fireSpriteCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(2f, 10f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Fire_Sprite_{i}", pos, new Vector3(0.5f, 0.5f, 0.5f), new Color(1f, 0.5f, 0f), 3f);
            }

            // Lava worms (surfacing from ground)
            for (int i = 0; i < lavaWormCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Lava_Worm_{i}", pos, new Vector3(0.6f, 0.4f, 2f), new Color(0.8f, 0.2f, 0f), 2f);
            }

            // Ember drakes (flying creatures of fire)
            for (int i = 0; i < emberDrakeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(10f, 20f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Ember_Drake_{i}", pos, new Vector3(1.2f, 0.8f, 1.5f), new Color(1f, 0.3f, 0f), 6f);
            }

            // Magma beetles (armored ground creatures)
            for (int i = 0; i < magmaBeetleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.3f,
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Magma_Beetle_{i}", pos, new Vector3(0.5f, 0.4f, 0.7f), new Color(0.5f, 0.2f, 0f), 1.5f);
            }

            Debug.Log($"🔥 LAVA CREATURES: {creatures.Count} ambient creatures adding life to the molten forge");
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
            mat.SetFloat("_Metallic", 0.4f);
            mat.SetFloat("_Smoothness", 0.5f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 1.5f);
            renderer.material = mat;

            CreaturePatrol patrol = creature.AddComponent<CreaturePatrol>();
            patrol.patrolSpeed = speed;
            patrol.patrolRadius = Random.Range(10f, 25f);
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
