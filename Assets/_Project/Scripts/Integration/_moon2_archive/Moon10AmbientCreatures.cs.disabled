using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 10: The Temporal Rift - Ambient Creatures
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns time-themed ambient creatures: temporal echoes, time anomalies, chrono wisps, phase shifters
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon10AmbientCreatures : MonoBehaviour
    {
        [Header("Time Creatures")]
        [SerializeField] int temporalEchoCount = 14;
        [SerializeField] int timeAnomalyCount = 10;
        [SerializeField] int chronoWispCount = 18;
        [SerializeField] int phaseShifterCount = 8;

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // Temporal echoes (ghostly repeating patterns)
            for (int i = 0; i < temporalEchoCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(2f, 10f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Temporal_Echo_{i}", pos, new Vector3(0.7f, 1f, 0.7f), new Color(0.6f, 0.7f, 0.8f, 0.5f), 4f);
            }

            // Time anomalies (distorted space-time pockets)
            for (int i = 0; i < timeAnomalyCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(5f, 15f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Time_Anomaly_{i}", pos, new Vector3(1f, 1f, 1f), new Color(0.5f, 0.6f, 0.7f, 0.6f), 2f);
            }

            // Chrono wisps (time essence floating)
            for (int i = 0; i < chronoWispCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(1f, 8f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Chrono_Wisp_{i}", pos, new Vector3(0.4f, 0.4f, 0.4f), new Color(0.7f, 0.8f, 0.9f), 3.5f);
            }

            // Phase shifters (blinking in/out of time)
            for (int i = 0; i < phaseShifterCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(3f, 12f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Phase_Shifter_{i}", pos, new Vector3(0.8f, 0.8f, 0.8f), new Color(0.5f, 0.7f, 0.9f, 0.7f), 6f);
            }

            Debug.Log($"⏰ TIME CREATURES: {creatures.Count} ambient creatures adding life to the temporal rift");
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
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Smoothness", 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.6f);
            renderer.material = mat;

            CreaturePatrol patrol = creature.AddComponent<CreaturePatrol>();
            patrol.patrolSpeed = speed;
            patrol.patrolRadius = Random.Range(12f, 30f);
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
