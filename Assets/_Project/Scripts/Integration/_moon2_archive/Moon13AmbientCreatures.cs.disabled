using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 13: The Aether Convergence - Ambient Creatures (FINAL LEVEL)
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns aether-themed ambient creatures representing ALL 12 previous moons converging
    /// 12 tribute spirit guardians + convergence essence + celestial observers
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon13AmbientCreatures : MonoBehaviour
    {
        [Header("Aether Convergence Creatures")]
        [SerializeField] int tributeSpiritCount = 12; // One per moon
        [SerializeField] int convergenceEssenceCount = 20;
        [SerializeField] int celestialObserverCount = 8;
        [SerializeField] int aetherPhoenixCount = 3; // Rare majestic creatures

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // 12 Tribute spirits (one representing each moon, circling the center)
            Color[] moonColors = GetMoonTributeColors();
            string[] moonNames = { "Jungle", "Desert", "Ice", "Lava", "Underwater", "Sky", "Corruption", "Time", "Prismatic", "Shadow", "Crystal", "Void" };
            
            for (int i = 0; i < tributeSpiritCount; i++)
            {
                float angle = (i / 12f) * 2f * Mathf.PI;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 80f,
                    15f + Mathf.Sin(Time.time * 0.5f + i) * 5f,
                    Mathf.Sin(angle) * 80f
                );
                CreateCreature($"Tribute_Spirit_{moonNames[i]}", pos, new Vector3(1.2f, 1.2f, 1.2f), moonColors[i], 3f);
            }

            // Convergence essence (aether energy floating everywhere)
            for (int i = 0; i < convergenceEssenceCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(5f, 25f),
                    Random.Range(-100f, 100f)
                );
                CreateCreature($"Convergence_Essence_{i}", pos, new Vector3(0.6f, 0.6f, 0.6f), new Color(0.9f, 0.85f, 1f), 4f);
            }

            // Celestial observers (high floating watchers)
            for (int i = 0; i < celestialObserverCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-120f, 120f),
                    Random.Range(30f, 50f),
                    Random.Range(-120f, 120f)
                );
                CreateCreature($"Celestial_Observer_{i}", pos, new Vector3(1.5f, 1.5f, 1.5f), new Color(1f, 1f, 0.95f, 0.7f), 2f);
            }

            // Aether phoenixes (rare majestic)
            for (int i = 0; i < aetherPhoenixCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    Random.Range(25f, 40f),
                    Random.Range(-80f, 80f)
                );
                CreateCreature($"Aether_Phoenix_{i}", pos, new Vector3(2f, 1.5f, 2.5f), new Color(1f, 0.9f, 0.95f), 8f);
            }

            Debug.Log($"✨ AETHER CONVERGENCE CREATURES: {creatures.Count} ambient creatures - 12 tribute spirits representing all moons!");
        }

        Color[] GetMoonTributeColors()
        {
            return new Color[]
            {
                new Color(0.2f, 0.7f, 0.3f),   // Moon 3: Jungle green
                new Color(0.9f, 0.7f, 0.4f),   // Moon 4: Desert tan
                new Color(0.7f, 0.9f, 1f),     // Moon 5: Ice blue
                new Color(1f, 0.3f, 0f),       // Moon 6: Lava red
                new Color(0.3f, 0.6f, 0.9f),   // Moon 7: Underwater blue
                new Color(0.95f, 0.95f, 1f),   // Moon 8: Sky white
                new Color(0.5f, 0f, 0.5f),     // Moon 9: Corruption purple
                new Color(0.6f, 0.7f, 0.8f),   // Moon 10: Time gray-blue
                new Color(1f, 0.5f, 0.8f),     // Moon 11: Prismatic (pink)
                new Color(0.1f, 0.1f, 0.2f),   // Moon 12: Shadow dark
                new Color(0.8f, 0.9f, 1f),     // Moon 1: Ethereal (start)
                new Color(0.9f, 0.8f, 0.95f)   // Moon 2: Mystic (progression)
            };
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
            mat.SetFloat("_Metallic", 0.4f);
            mat.SetFloat("_Smoothness", 0.95f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 1.2f);
            renderer.material = mat;

            CreaturePatrol patrol = creature.AddComponent<CreaturePatrol>();
            patrol.patrolSpeed = speed;
            patrol.patrolRadius = name.Contains("Tribute") ? 20f : Random.Range(15f, 35f);
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
