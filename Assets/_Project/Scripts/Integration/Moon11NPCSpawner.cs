using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 11 NPC Spawner — The Prismatic Nexus
    /// Spawns light weavers, spectrum mages, and prism guardians
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon11NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int lightWeaverCount = 7; // One per color
        [SerializeField] int spectrumMageCount = 5;
        [SerializeField] int prismGuardianCount = 3;

        List<GameObject> spawnedNPCs = new List<GameObject>();

        // Spectrum colors
        Color[] spectrumColors = new Color[]
        {
            new Color(1f, 0f, 0f),      // Red
            new Color(1f, 0.5f, 0f),    // Orange
            new Color(1f, 1f, 0f),      // Yellow
            new Color(0f, 1f, 0f),      // Green
            new Color(0f, 1f, 1f),      // Cyan
            new Color(0f, 0f, 1f),      // Blue
            new Color(0.5f, 0f, 1f)     // Violet
        };

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log($"[Moon11NPCSpawner] 🧑‍🤝‍🧑 Spawning {lightWeaverCount + spectrumMageCount + prismGuardianCount} NPCs...");

            // Light Weavers - one in each color chamber
            for (int i = 0; i < lightWeaverCount; i++)
            {
                float angle = i * (360f / lightWeaverCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 70f,
                    2f,
                    Mathf.Sin(angle) * 70f
                );
                CreateNPC($"LightWeaver_{i}", pos, spectrumColors[i]);
            }

            // Spectrum Mages - studying refraction
            for (int i = 0; i < spectrumMageCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    2f,
                    Random.Range(-80f, 80f)
                );
                int colorIndex = Random.Range(0, spectrumColors.Length);
                CreateNPC($"SpectrumMage_{i}", pos, spectrumColors[colorIndex]);
            }

            // Prism Guardians - at central nexus
            for (int i = 0; i < prismGuardianCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-15f, 15f),
                    2f,
                    Random.Range(-15f, 15f)
                );
                CreateNPC($"PrismGuardian_{i}", pos, Color.white);
            }

            Debug.Log($"[Moon11NPCSpawner] ✅ Spawned {spawnedNPCs.Count} prismatic NPCs in the nexus");
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
