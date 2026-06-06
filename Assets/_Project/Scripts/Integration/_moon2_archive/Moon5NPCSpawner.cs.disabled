using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 5 NPC Spawner — The Frostbound Citadel
    /// Spawns ice mages, frozen knights, and frost priests
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon5NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int mageCount = 4;
        [SerializeField] int knightCount = 5;
        [SerializeField] int priestCount = 3;

        List<GameObject> spawnedNPCs = new List<GameObject>();

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log($"[Moon5NPCSpawner] 🧑‍🤝‍🧑 Spawning {mageCount + knightCount + priestCount} NPCs...");

            // Ice Mages - in towers
            for (int i = 0; i < mageCount; i++)
            {
                float angle = i * (360f / mageCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 80f,
                    15f, // Elevated in towers
                    Mathf.Sin(angle) * 80f
                );
                CreateNPC($"IceMage_{i}", pos, new Color(0.7f, 0.85f, 1f));
            }

            // Frozen Knights - guarding walls
            for (int i = 0; i < knightCount; i++)
            {
                float angle = i * (360f / knightCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 60f,
                    2f,
                    Mathf.Sin(angle) * 60f
                );
                CreateNPC($"FrozenKnight_{i}", pos, new Color(0.5f, 0.6f, 0.7f));
            }

            // Frost Priests - at crystal shrine
            for (int i = 0; i < priestCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-10f, 10f),
                    2f,
                    Random.Range(-10f, 10f)
                );
                CreateNPC($"FrostPriest_{i}", pos, new Color(0.9f, 0.95f, 1f));
            }

            Debug.Log($"[Moon5NPCSpawner] ✅ Spawned {spawnedNPCs.Count} NPCs in the frozen citadel");
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
