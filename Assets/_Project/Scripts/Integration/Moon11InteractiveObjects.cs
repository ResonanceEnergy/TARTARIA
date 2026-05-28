using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 11: The Prismatic Nexus - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns prismatic-themed interactables: spectrum prisms, rainbow bridges, color essence orbs, refraction puzzles
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon11InteractiveObjects : MonoBehaviour
    {
        [Header("Prismatic Interactables")]
        [SerializeField] int spectrumPrismCount = 10;
        [SerializeField] int rainbowBridgeCount = 6;
        [SerializeField] int colorEssenceOrbCount = 21; // 3 per color (7 colors)
        [SerializeField] int refractionPuzzleCount = 8;

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // Spectrum prisms (light-splitting devices)
            for (int i = 0; i < spectrumPrismCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(1f, 5f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Spectrum_Prism_{i}", pos, new Vector3(1f, 2f, 1f), Color.white, "Puzzle");
            }

            // Rainbow bridges (traversal platforms)
            for (int i = 0; i < rainbowBridgeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(5f, 12f),
                    Random.Range(-160f, 160f)
                );
                Color bridgeColor = GetRainbowColor(i % 7);
                CreateInteractive($"Rainbow_Bridge_{i}", pos, new Vector3(5f, 0.3f, 2f), bridgeColor, "Platform");
            }

            // Color essence orbs (7-color collectible set)
            for (int i = 0; i < colorEssenceOrbCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(1f, 8f),
                    Random.Range(-160f, 160f)
                );
                Color orbColor = GetRainbowColor(i % 7);
                CreateInteractive($"Color_Essence_Orb_{i}", pos, new Vector3(0.8f, 0.8f, 0.8f), orbColor, "Collectible");
            }

            // Refraction puzzles (light-beam challenges)
            for (int i = 0; i < refractionPuzzleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    1f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Refraction_Puzzle_{i}", pos, new Vector3(1.5f, 1.5f, 1.5f), Color.white * 0.9f, "Puzzle");
            }

            Debug.Log($"🌈 PRISMATIC INTERACTIVES: {interactiveObjects.Count} objects ready for player interaction");
        }

        Color GetRainbowColor(int index)
        {
            Color[] rainbow = {
                Color.red,
                new Color(1f, 0.5f, 0f), // Orange
                Color.yellow,
                Color.green,
                Color.cyan,
                Color.blue,
                new Color(0.5f, 0f, 1f)  // Violet
            };
            return rainbow[index % rainbow.Length];
        }

        void CreateInteractive(string name, Vector3 position, Vector3 scale, Color color, string tag)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.position = position;
            obj.transform.localScale = scale;
            obj.transform.parent = transform;
            obj.tag = tag;

            Renderer renderer = obj.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Smoothness", 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.5f);
            renderer.material = mat;

            BoxCollider collider = obj.GetComponent<BoxCollider>();
            collider.isTrigger = true;

            interactiveObjects.Add(obj);
        }

        void OnDestroy()
        {
            foreach (var obj in interactiveObjects)
            {
                if (obj != null) Destroy(obj);
            }
            interactiveObjects.Clear();
        }
    }
}
