using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 4 NPC Spawner — The Sunscorched Oasis
    /// Spawns desert nomads, merchants, and sun worshippers
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon4NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int nomadCount = 6;
        [SerializeField] int merchantCount = 3;
        [SerializeField] int worshipperCount = 4;

        List<GameObject> spawnedNPCs = new List<GameObject>();

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log($"[Moon4NPCSpawner] 🧑‍🤝‍🧑 Spawning {nomadCount + merchantCount + worshipperCount} NPCs...");

            // Desert Nomads - wandering the dunes
            for (int i = 0; i < nomadCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    2f,
                    Random.Range(-100f, 100f)
                );
                CreateNPC($"DesertNomad_{i}", pos, new Color(0.9f, 0.8f, 0.6f));
            }

            // Merchants - near oasis
            for (int i = 0; i < merchantCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-20f, 20f),
                    2f,
                    Random.Range(-20f, 20f)
                );
                CreateNPC($"OasisMerchant_{i}", pos, new Color(0.7f, 0.5f, 0.3f));
            }

            // Sun Worshippers - at sun temples
            for (int i = 0; i < worshipperCount; i++)
            {
                float angle = i * (360f / worshipperCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 70f,
                    2f,
                    Mathf.Sin(angle) * 70f
                );
                CreateNPC($"SunWorshipper_{i}", pos, new Color(1f, 0.9f, 0.5f));
            }

            Debug.Log($"[Moon4NPCSpawner] ✅ Spawned {spawnedNPCs.Count} NPCs in the desert");
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
