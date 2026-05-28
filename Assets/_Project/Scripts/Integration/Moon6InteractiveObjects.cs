using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6: The Molten Forge - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns lava-themed interactables: forge anvils, magma cores, obsidian tablets, volcanic vents
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon6InteractiveObjects : MonoBehaviour
    {
        [Header("Lava Interactables")]
        [SerializeField] int forgeAnvilCount = 6;
        [SerializeField] int magmaCoreCount = 8;
        [SerializeField] int obsidianTabletCount = 10;
        [SerializeField] int volcanicVentCount = 7;

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // Forge anvils (crafting stations)
            for (int i = 0; i < forgeAnvilCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Forge_Anvil_{i}", pos, new Vector3(1.5f, 1f, 1f), new Color(0.3f, 0.3f, 0.3f), "Craft");
            }

            // Magma cores (power source collectibles)
            for (int i = 0; i < magmaCoreCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Magma_Core_{i}", pos, new Vector3(1f, 1f, 1f), new Color(1f, 0.3f, 0f), "Collectible");
            }

            // Obsidian tablets (lore inscriptions)
            for (int i = 0; i < obsidianTabletCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    1f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Obsidian_Tablet_{i}", pos, new Vector3(1f, 1.5f, 0.3f), new Color(0.1f, 0.1f, 0.15f), "Rune");
            }

            // Volcanic vents (puzzle pressure points)
            for (int i = 0; i < volcanicVentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.2f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Volcanic_Vent_{i}", pos, new Vector3(2f, 0.5f, 2f), new Color(0.6f, 0.2f, 0f), "Puzzle");
            }

            Debug.Log($"🔥 LAVA INTERACTIVES: {interactiveObjects.Count} objects ready for player interaction");
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
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Smoothness", 0.4f);
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
