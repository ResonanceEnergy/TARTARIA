using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 6 NPC Spawner — The Molten Forge
    /// Spawns fire elementals, blacksmiths, and forge masters
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon6NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int elementalCount = 6;
        [SerializeField] int blacksmithCount = 4;
        [SerializeField] int forgeMasterCount = 2;

        List<GameObject> spawnedNPCs = new List<GameObject>();

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log($"[Moon6NPCSpawner] 🧑‍🤝‍🧑 Spawning {elementalCount + blacksmithCount + forgeMasterCount} NPCs...");

            // Fire Elementals - near lava flows
            for (int i = 0; i < elementalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    2f,
                    Random.Range(-90f, 90f)
                );
                CreateNPC($"FireElemental_{i}", pos, new Color(1f, 0.3f, 0f));
            }

            // Blacksmiths - at forges
            for (int i = 0; i < blacksmithCount; i++)
            {
                float angle = i * (360f / blacksmithCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 50f,
                    2f,
                    Mathf.Sin(angle) * 50f
                );
                CreateNPC($"Blacksmith_{i}", pos, new Color(0.5f, 0.4f, 0.35f));
            }

            // Forge Masters - at central hearth
            for (int i = 0; i < forgeMasterCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-15f, 15f),
                    2f,
                    Random.Range(-15f, 15f)
                );
                CreateNPC($"ForgeMaster_{i}", pos, new Color(0.7f, 0.2f, 0.1f));
            }

            Debug.Log($"[Moon6NPCSpawner] ✅ Spawned {spawnedNPCs.Count} NPCs in the molten forge");
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
