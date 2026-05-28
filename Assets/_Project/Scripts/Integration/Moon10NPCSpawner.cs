using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10 NPC Spawner — The Temporal Rift
    /// Spawns time keepers, chrono-mages, and paradox entities
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon10NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int timeKeeperCount = 4;
        [SerializeField] int chronoMageCount = 5;
        [SerializeField] int paradoxEntityCount = 3;

        List<GameObject> spawnedNPCs = new List<GameObject>();

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log($"[Moon10NPCSpawner] 🧑‍🤝‍🧑 Spawning {timeKeeperCount + chronoMageCount + paradoxEntityCount} NPCs...");

            // Time Keepers - at temporal anchor points
            for (int i = 0; i < timeKeeperCount; i++)
            {
                float angle = i * (360f / timeKeeperCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 80f,
                    2f,
                    Mathf.Sin(angle) * 80f
                );
                CreateNPC($"TimeKeeper_{i}", pos, new Color(0.7f, 0.7f, 0.8f));
            }

            // Chrono-Mages - studying temporal layers
            for (int i = 0; i < chronoMageCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    2f,
                    Random.Range(-70f, 70f)
                );
                CreateNPC($"ChronoMage_{i}", pos, new Color(0.6f, 0.7f, 0.9f));
            }

            // Paradox Entities - near vortex
            for (int i = 0; i < paradoxEntityCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-20f, 20f),
                    2f,
                    Random.Range(-20f, 20f)
                );
                CreateNPC($"ParadoxEntity_{i}", pos, new Color(0.9f, 0.95f, 1f));
            }

            Debug.Log($"[Moon10NPCSpawner] ✅ Spawned {spawnedNPCs.Count} temporal NPCs in the rift");
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
