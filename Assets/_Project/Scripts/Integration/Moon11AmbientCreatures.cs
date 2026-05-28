using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 11: The Prismatic Nexus - Ambient Creatures
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns prismatic-themed ambient creatures: light sprites, rainbow butterflies, color wisps, spectrum birds
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon11AmbientCreatures : MonoBehaviour
    {
        [Header("Prismatic Creatures")]
        [SerializeField] int lightSpriteCount = 21; // 3 per color
        [SerializeField] int rainbowButterflyCount = 14;
        [SerializeField] int colorWispCount = 28; // 4 per color
        [SerializeField] int spectrumBirdCount = 7; // 1 per color

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // Light sprites (one for each color × 3)
            Color[] colors = GetRainbowColors();
            for (int c = 0; c < colors.Length; c++)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 pos = new Vector3(
                        Random.Range(-160f, 160f),
                        Random.Range(3f, 12f),
                        Random.Range(-160f, 160f)
                    );
                    CreateCreature($"Light_Sprite_{colors[c]}_{i}", pos, new Vector3(0.5f, 0.5f, 0.5f), colors[c], 4f);
                }
            }

            // Rainbow butterflies (mixed colors)
            for (int i = 0; i < rainbowButterflyCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(2f, 8f),
                    Random.Range(-160f, 160f)
                );
                Color mixedColor = colors[Random.Range(0, colors.Length)];
                CreateCreature($"Rainbow_Butterfly_{i}", pos, new Vector3(0.6f, 0.3f, 0.8f), mixedColor, 5f);
            }

            // Color wisps (4 per color)
            for (int c = 0; c < colors.Length; c++)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 pos = new Vector3(
                        Random.Range(-160f, 160f),
                        Random.Range(1f, 6f),
                        Random.Range(-160f, 160f)
                    );
                    CreateCreature($"Color_Wisp_{colors[c]}_{i}", pos, new Vector3(0.4f, 0.4f, 0.4f), colors[c], 3f);
                }
            }

            // Spectrum birds (1 per color, large and majestic)
            for (int c = 0; c < colors.Length; c++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(15f, 30f),
                    Random.Range(-100f, 100f)
                );
                CreateCreature($"Spectrum_Bird_{colors[c]}", pos, new Vector3(1f, 0.6f, 1.2f), colors[c], 7f);
            }

            Debug.Log($"🌈 PRISMATIC CREATURES: {creatures.Count} ambient creatures adding life to the prismatic nexus");
        }

        Color[] GetRainbowColors()
        {
            return new Color[]
            {
                Color.red,
                new Color(1f, 0.5f, 0f), // Orange
                Color.yellow,
                Color.green,
                Color.cyan,
                Color.blue,
                new Color(0.5f, 0f, 1f) // Violet
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
            mat.SetFloat("_Metallic", 0.3f);
            mat.SetFloat("_Smoothness", 0.9f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 1f);
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
