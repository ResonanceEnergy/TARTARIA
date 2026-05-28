using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10: The Temporal Rift - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns time-themed interactables: time anchors, chrono crystals, temporal gates, broken clocks
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon10InteractiveObjects : MonoBehaviour
    {
        [Header("Temporal Interactables")]
        [SerializeField] int timeAnchorCount = 7;
        [SerializeField] int chronoCrystalCount = 14;
        [SerializeField] int temporalGateCount = 5;
        [SerializeField] int brokenClockCount = 9;

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // Time anchors (stabilization points)
            for (int i = 0; i < timeAnchorCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Time_Anchor_{i}", pos, new Vector3(1.2f, 3f, 1.2f), new Color(0.6f, 0.7f, 0.8f), "Shrine");
            }

            // Chrono crystals (time energy collectibles)
            for (int i = 0; i < chronoCrystalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(0.5f, 4f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Chrono_Crystal_{i}", pos, new Vector3(0.6f, 1.2f, 0.6f), new Color(0.7f, 0.8f, 1f), "Collectible");
            }

            // Temporal gates (time travel portals)
            for (int i = 0; i < temporalGateCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    1f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Temporal_Gate_{i}", pos, new Vector3(2.5f, 4f, 0.5f), new Color(0.5f, 0.7f, 0.9f, 0.7f), "Portal");
            }

            // Broken clocks (lore/puzzle pieces)
            for (int i = 0; i < brokenClockCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Broken_Clock_{i}", pos, new Vector3(1f, 1f, 0.3f), new Color(0.8f, 0.8f, 0.9f), "Rune");
            }

            Debug.Log($"⏱️ TIME INTERACTIVES: {interactiveObjects.Count} objects ready for player interaction");
        }

        void CreateInteractive(string name, Vector3 position, Vector3 scale, Color color, string tag)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.position = position;
            obj.transform.localScale = scale;
            obj.transform.parent = transform;
            obj.tag = tag;

            Renderer renderer = obj.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Smoothness", 0.7f);
            renderer.material = mat;

            BoxCollider collider = obj.GetComponent<BoxCollider>();
            collider.isTrigger = true;

            interactiveObjects.Add(obj);
        }

        void OnDestroy()
        {
            foreach (var obj in interactiveObjects)
            {
                if (obj != null) Destroy(obj);
            }
            interactiveObjects.Clear();
        }
    }
}
