using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 7 NPC Spawner — The Abyssal Depths
    /// Spawns mer-folk, deep sea explorers, and abyssal guardians
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon7NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int merfolkCount = 7;
        [SerializeField] int explorerCount = 4;
        [SerializeField] int guardianCount = 3;

        List<GameObject> spawnedNPCs = new List<GameObject>();

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log($"[Moon7NPCSpawner] 🧑‍🤝‍🧑 Spawning {merfolkCount + explorerCount + guardianCount} NPCs...");

            // Mer-folk - swimming between coral
            for (int i = 0; i < merfolkCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(5f, 15f), // Floating at various depths
                    Random.Range(-100f, 100f)
                );
                CreateNPC($"Merfolk_{i}", pos, new Color(0.3f, 0.6f, 0.8f));
            }

            // Deep Sea Explorers - near trenches
            for (int i = 0; i < explorerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    2f,
                    Random.Range(-80f, 80f)
                );
                CreateNPC($"DeepExplorer_{i}", pos, new Color(0.2f, 0.4f, 0.6f));
            }

            // Abyssal Guardians - protecting depths
            for (int i = 0; i < guardianCount; i++)
            {
                float angle = i * (360f / guardianCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 70f,
                    2f,
                    Mathf.Sin(angle) * 70f
                );
                CreateNPC($"AbyssalGuardian_{i}", pos, new Color(0.1f, 0.2f, 0.3f));
            }

            Debug.Log($"[Moon7NPCSpawner] ✅ Spawned {spawnedNPCs.Count} NPCs in the abyssal depths");
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
