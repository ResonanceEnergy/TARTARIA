using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12: The Umbral Sanctum - Ambient Creatures
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns shadow-themed ambient creatures: shadow bats, umbral wraiths, darkness wisps, void stalkers
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon12AmbientCreatures : MonoBehaviour
    {
        [Header("Shadow Creatures")]
        [SerializeField] int shadowBatCount = 16;
        [SerializeField] int umbralWraithCount = 10;
        [SerializeField] int darknessWispCount = 20;
        [SerializeField] int voidStalkerCount = 8;

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // Shadow bats (flying darkness)
            for (int i = 0; i < shadowBatCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(8f, 20f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Shadow_Bat_{i}", pos, new Vector3(0.6f, 0.4f, 0.8f), new Color(0.1f, 0.1f, 0.15f), 6f);
            }

            // Umbral wraiths (ethereal shadow entities)
            for (int i = 0; i < umbralWraithCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(3f, 12f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Umbral_Wraith_{i}", pos, new Vector3(0.9f, 1.5f, 0.9f), new Color(0.15f, 0.1f, 0.2f, 0.6f), 3f);
            }

            // Darkness wisps (small shadow essence)
            for (int i = 0; i < darknessWispCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(1f, 6f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Darkness_Wisp_{i}", pos, new Vector3(0.3f, 0.3f, 0.3f), new Color(0.1f, 0.05f, 0.15f), 2.5f);
            }

            // Void stalkers (ground shadow predators)
            for (int i = 0; i < voidStalkerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Void_Stalker_{i}", pos, new Vector3(1f, 0.7f, 1.2f), new Color(0.05f, 0.05f, 0.1f), 4f);
            }

            Debug.Log($"🌑 SHADOW CREATURES: {creatures.Count} ambient creatures adding life to the umbral sanctum");
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
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Smoothness", 0.6f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.1f, 0.05f, 0.15f) * 0.2f);
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
