using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13: The Aether Convergence - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns aether-themed interactables: 12 convergence monoliths (one per moon tribute), aether wells, final gateway, celestial archives
    /// FINAL LEVEL tribute system includes 12 monoliths representing each previous moon
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon13InteractiveObjects : MonoBehaviour
    {
        [Header("Aether Convergence Interactables")]
        [SerializeField] int convergenceMonolithCount = 12; // One per moon (3-13 tribute + moon 1&2 foundation)
        [SerializeField] int aetherWellCount = 8;
        [SerializeField] int celestialArchiveCount = 6;
        [SerializeField] int finalGatewayCount = 1; // The ultimate portal

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // 12 Convergence monoliths (tribute system - one per moon)
            Color[] moonColors = {
                new Color(0.4f, 0.6f, 0.3f),    // Moon 3: Jungle green
                new Color(0.9f, 0.7f, 0.4f),    // Moon 4: Desert gold
                new Color(0.6f, 0.85f, 1f),     // Moon 5: Ice blue
                new Color(1f, 0.4f, 0f),        // Moon 6: Lava orange
                new Color(0.1f, 0.4f, 0.7f),    // Moon 7: Deep ocean blue
                new Color(0.9f, 0.95f, 1f),     // Moon 8: Sky white
                new Color(0.4f, 0.15f, 0.5f),   // Moon 9: Corruption purple
                new Color(0.7f, 0.8f, 0.9f),    // Moon 10: Time gray-blue
                new Color(1f, 0.5f, 0f),        // Moon 11: Prismatic (orange as representative)
                new Color(0.05f, 0.05f, 0.1f),  // Moon 12: Shadow black
                new Color(0.3f, 0.5f, 0.3f),    // Moon 1: Tutorial green (foundation)
                new Color(0.5f, 0.5f, 0.6f)     // Moon 2: Training gray (foundation)
            };

            float radius = 100f;
            for (int i = 0; i < convergenceMonolithCount; i++)
            {
                float angle = (360f / convergenceMonolithCount) * i * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0.5f,
                    Mathf.Sin(angle) * radius
                );
                CreateInteractive($"Convergence_Monolith_Moon{i+1}", pos, new Vector3(2f, 6f, 2f), moonColors[i], "Shrine");
            }

            // Aether wells (energy condensation points)
            for (int i = 0; i < aetherWellCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.3f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Aether_Well_{i}", pos, new Vector3(2f, 0.5f, 2f), new Color(0.7f, 0.9f, 1f), "Collectible");
            }

            // Celestial archives (lore repositories)
            for (int i = 0; i < celestialArchiveCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    1f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Celestial_Archive_{i}", pos, new Vector3(2f, 3f, 1.5f), new Color(0.9f, 0.95f, 1f), "Rune");
            }

            // Final gateway (the ultimate convergence portal)
            CreateInteractive("FINAL_GATEWAY", Vector3.zero + Vector3.up * 2f, new Vector3(5f, 8f, 1f), new Color(0.8f, 1f, 1f), "Portal");

            Debug.Log($"✨ AETHER CONVERGENCE INTERACTIVES: {interactiveObjects.Count} objects ready - INCLUDING 12 MOON TRIBUTE MONOLITHS");
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
            mat.SetFloat("_Metallic", 0.7f);
            mat.SetFloat("_Smoothness", 0.9f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.8f);
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
