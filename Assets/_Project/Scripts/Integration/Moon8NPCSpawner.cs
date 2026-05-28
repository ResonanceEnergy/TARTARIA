using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 8 NPC Spawner — The Celestial Spires
    /// Spawns sky priests, cloud walkers, and celestial scholars
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon8NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int priestCount = 5;
        [SerializeField] int walkerCount = 6;
        [SerializeField] int scholarCount = 3;

        List<GameObject> spawnedNPCs = new List<GameObject>();

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log($"[Moon8NPCSpawner] 🧑‍🤝‍🧑 Spawning {priestCount + walkerCount + scholarCount} NPCs...");

            // Sky Priests - at temples
            for (int i = 0; i < priestCount; i++)
            {
                float angle = i * (360f / priestCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 60f,
                    20f, // Elevated on platforms
                    Mathf.Sin(angle) * 60f
                );
                CreateNPC($"SkyPriest_{i}", pos, new Color(1f, 1f, 1f));
            }

            // Cloud Walkers - wandering the clouds
            for (int i = 0; i < walkerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    Random.Range(10f, 25f), // Floating
                    Random.Range(-90f, 90f)
                );
                CreateNPC($"CloudWalker_{i}", pos, new Color(0.9f, 0.95f, 1f));
            }

            // Celestial Scholars - studying stars
            for (int i = 0; i < scholarCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-30f, 30f),
                    15f,
                    Random.Range(-30f, 30f)
                );
                CreateNPC($"CelestialScholar_{i}", pos, new Color(0.8f, 0.9f, 1f));
            }

            Debug.Log($"[Moon8NPCSpawner] ✅ Spawned {spawnedNPCs.Count} NPCs in the celestial spires");
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
