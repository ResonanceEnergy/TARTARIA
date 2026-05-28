using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13 NPC Spawner — The Aether Convergence
    /// FINAL LEVEL - Spawns aether masters, convergence witnesses, and tribute bearers from all 12 moons
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon13NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int aetherMasterCount = 3;
        [SerializeField] int witnessCount = 8;
        [SerializeField] int tributeBearerCount = 12; // One per moon

        List<GameObject> spawnedNPCs = new List<GameObject>();

        // Tribute colors (one per moon)
        Color[] tributeColors = new Color[]
        {
            new Color(1f, 0.85f, 0.5f),    // Moon 1: Warm golden
            new Color(0.4f, 0.6f, 0.9f),   // Moon 2: Blue cavern
            new Color(0.3f, 0.6f, 0.3f),   // Moon 3: Green jungle
            new Color(0.9f, 0.8f, 0.5f),   // Moon 4: Desert gold
            new Color(0.7f, 0.85f, 1f),    // Moon 5: Ice blue
            new Color(1f, 0.4f, 0.1f),     // Moon 6: Lava orange
            new Color(0.3f, 0.6f, 0.8f),   // Moon 7: Deep water
            new Color(0.95f, 0.95f, 1f),   // Moon 8: Sky white
            new Color(0.6f, 0.3f, 0.8f),   // Moon 9: Corruption purple
            new Color(0.8f, 0.8f, 0.8f),   // Moon 10: Time neutral
            new Color(1f, 1f, 1f),         // Moon 11: Prismatic white
            new Color(0.1f, 0.1f, 0.15f)   // Moon 12: Shadow dark
        };

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log($"[Moon13NPCSpawner] 🧑‍🤝‍🧑 SPAWNING {aetherMasterCount + witnessCount + tributeBearerCount} FINAL LEVEL NPCs");
            Debug.Log("    ✨ AETHER CONVERGENCE - REPRESENTATIVES FROM ALL 12 MOONS ✨");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            // Aether Masters - at central convergence
            for (int i = 0; i < aetherMasterCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-10f, 10f),
                    5f, // Elevated
                    Random.Range(-10f, 10f)
                );
                CreateNPC($"AetherMaster_{i}", pos, new Color(0.95f, 1f, 1f));
            }

            // Convergence Witnesses - observing the ritual
            for (int i = 0; i < witnessCount; i++)
            {
                float angle = i * (360f / witnessCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 40f,
                    2f,
                    Mathf.Sin(angle) * 40f
                );
                CreateNPC($"ConvergenceWitness_{i}", pos, new Color(0.9f, 0.95f, 1f));
            }

            // Tribute Bearers - one from each moon at tribute platforms
            for (int i = 0; i < tributeBearerCount; i++)
            {
                float angle = i * (360f / tributeBearerCount) * Mathf.Deg2Rad;
                float phi = 1.618033988749895f; // Golden ratio
                float radius = 80f + 20f * Mathf.Sin(i * phi);
                
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    2f,
                    Mathf.Sin(angle) * radius
                );
                CreateNPC($"TributeBearer_Moon{i + 1}", pos, tributeColors[i]);
                Debug.Log($"  → Tribute Bearer from Moon {i + 1} placed at tribute platform");
            }

            Debug.Log($"[Moon13NPCSpawner] ✅ ALL {spawnedNPCs.Count} FINAL LEVEL NPCs CONVERGED!");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void CreateNPC(string name, Vector3 position, Color color)
        {
            var npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = name;
            npc.transform.position = position;
            npc.transform.SetParent(transform);

            var renderer = npc.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            renderer.material = mat;

            spawnedNPCs.Add(npc);
        }

        void OnDestroy()
        {
            foreach (var npc in spawnedNPCs)
            {
                if (npc != null) Destroy(npc);
            }
        }
    }
}
