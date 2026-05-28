using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 12 NPC Spawner — The Umbral Sanctum
    /// Spawns shadow priests, void walkers, and umbral sentinels
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon12NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int shadowPriestCount = 4;
        [SerializeField] int voidWalkerCount = 6;
        [SerializeField] int sentinelCount = 5;

        List<GameObject> spawnedNPCs = new List<GameObject>();

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log($"[Moon12NPCSpawner] 🧑‍🤝‍🧑 Spawning {shadowPriestCount + voidWalkerCount + sentinelCount} NPCs...");

            // Shadow Priests - near obelisks
            for (int i = 0; i < shadowPriestCount; i++)
            {
                float angle = i * (360f / shadowPriestCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 60f,
                    2f,
                    Mathf.Sin(angle) * 60f
                );
                CreateNPC($"ShadowPriest_{i}", pos, new Color(0.1f, 0.1f, 0.15f));
            }

            // Void Walkers - wandering darkness
            for (int i = 0; i < voidWalkerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    2f,
                    Random.Range(-90f, 90f)
                );
                CreateNPC($"VoidWalker_{i}", pos, new Color(0.05f, 0.05f, 0.1f));
            }

            // Umbral Sentinels - guarding sanctum
            for (int i = 0; i < sentinelCount; i++)
            {
                float angle = i * (360f / sentinelCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 40f,
                    2f,
                    Mathf.Sin(angle) * 40f
                );
                CreateNPC($"UmbralSentinel_{i}", pos, new Color(0.08f, 0.08f, 0.12f));
            }

            Debug.Log($"[Moon12NPCSpawner] ✅ Spawned {spawnedNPCs.Count} shadow NPCs in the umbral sanctum");
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
