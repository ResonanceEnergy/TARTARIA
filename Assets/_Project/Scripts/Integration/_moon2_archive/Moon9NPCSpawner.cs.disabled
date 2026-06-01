using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 9 NPC Spawner — The Blighted Wastes
    /// Spawns corrupted cultists, void walkers, and twisted survivors
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon9NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int cultistCount = 8;
        [SerializeField] int voidWalkerCount = 5;
        [SerializeField] int survivorCount = 3;

        List<GameObject> spawnedNPCs = new List<GameObject>();

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log($"[Moon9NPCSpawner] 🧑‍🤝‍🧑 Spawning {cultistCount + voidWalkerCount + survivorCount} NPCs...");

            // Corrupted Cultists - around spires
            for (int i = 0; i < cultistCount; i++)
            {
                float angle = i * (360f / cultistCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 70f,
                    2f,
                    Mathf.Sin(angle) * 70f
                );
                CreateNPC($"CorruptedCultist_{i}", pos, new Color(0.5f, 0.2f, 0.7f));
            }

            // Void Walkers - near dark energy sources
            for (int i = 0; i < voidWalkerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    2f,
                    Random.Range(-90f, 90f)
                );
                CreateNPC($"VoidWalker_{i}", pos, new Color(0.3f, 0.15f, 0.4f));
            }

            // Twisted Survivors - scattered wasteland
            for (int i = 0; i < survivorCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    2f,
                    Random.Range(-60f, 60f)
                );
                CreateNPC($"TwistedSurvivor_{i}", pos, new Color(0.4f, 0.3f, 0.5f));
            }

            Debug.Log($"[Moon9NPCSpawner] ✅ Spawned {spawnedNPCs.Count} corrupted NPCs in the blighted wastes");
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
