using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 8: The Celestial Spires - Ambient Creatures
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns sky-themed ambient creatures: sky birds, cloud spirits, wind elementals, star moths
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon8AmbientCreatures : MonoBehaviour
    {
        [Header("Sky Creatures")]
        [SerializeField] int skyBirdCount = 20;
        [SerializeField] int cloudSpiritCount = 12;
        [SerializeField] int windElementalCount = 8;
        [SerializeField] int starMothCount = 15;

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // Sky birds (high soaring)
            for (int i = 0; i < skyBirdCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(20f, 40f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Sky_Bird_{i}", pos, new Vector3(0.7f, 0.4f, 1f), new Color(0.9f, 0.9f, 1f), 8f);
            }

            // Cloud spirits (ethereal floating)
            for (int i = 0; i < cloudSpiritCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(15f, 30f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Cloud_Spirit_{i}", pos, new Vector3(1.2f, 1.2f, 1.2f), new Color(1f, 1f, 1f, 0.5f), 2f);
            }

            // Wind elementals (fast swirling)
            for (int i = 0; i < windElementalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(10f, 25f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Wind_Elemental_{i}", pos, new Vector3(0.8f, 1.5f, 0.8f), new Color(0.85f, 0.95f, 1f, 0.6f), 10f);
            }

            // Star moths (glowing night flyers)
            for (int i = 0; i < starMothCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(12f, 22f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Star_Moth_{i}", pos, new Vector3(0.6f, 0.3f, 0.8f), new Color(1f, 1f, 0.9f), 5f);
            }

            Debug.Log($"☁️ SKY CREATURES: {creatures.Count} ambient creatures adding life to the celestial spires");
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
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Smoothness", 0.9f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.4f);
            renderer.material = mat;

            CreaturePatrol patrol = creature.AddComponent<CreaturePatrol>();
            patrol.patrolSpeed = speed;
            patrol.patrolRadius = Random.Range(20f, 40f);
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
