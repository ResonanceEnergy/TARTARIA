using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 3 NPC Spawner — The Verdant Labyrinth
    /// Spawns jungle explorers, archaeologists, and nature guardians
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon3NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int explorerCount = 5;
        [SerializeField] int guardianCount = 3;
        [SerializeField] int archaeologistCount = 2;

        List<GameObject> spawnedNPCs = new List<GameObject>();

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log($"[Moon3NPCSpawner] 🧑‍🤝‍🧑 Spawning {explorerCount + guardianCount + archaeologistCount} NPCs...");

            // Jungle Explorers - scattered through maze
            for (int i = 0; i < explorerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    2f,
                    Random.Range(-80f, 80f)
                );
                CreateNPC($"JungleExplorer_{i}", pos, new Color(0.4f, 0.6f, 0.3f));
            }

            // Nature Guardians - near outer walls
            for (int i = 0; i < guardianCount; i++)
            {
                float angle = i * (360f / guardianCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 90f,
                    2f,
                    Mathf.Sin(angle) * 90f
                );
                CreateNPC($"NatureGuardian_{i}", pos, new Color(0.2f, 0.7f, 0.2f));
            }

            // Archaeologists - near central shrine
            for (int i = 0; i < archaeologistCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-15f, 15f),
                    2f,
                    Random.Range(-15f, 15f)
                );
                CreateNPC($"Archaeologist_{i}", pos, new Color(0.7f, 0.6f, 0.4f));
            }

            Debug.Log($"[Moon3NPCSpawner] ✅ Spawned {spawnedNPCs.Count} NPCs in the jungle labyrinth");
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
