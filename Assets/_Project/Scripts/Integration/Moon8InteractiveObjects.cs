using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 8: The Celestial Spires - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns sky-themed interactables: wind chimes, cloud platforms, star fragments, celestial gates
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon8InteractiveObjects : MonoBehaviour
    {
        [Header("Sky Interactables")]
        [SerializeField] int windChimeCount = 10;
        [SerializeField] int cloudPlatformCount = 8;
        [SerializeField] int starFragmentCount = 15;
        [SerializeField] int celestialGateCount = 4;

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // Wind chimes (audio puzzle elements)
            for (int i = 0; i < windChimeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(8f, 15f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Wind_Chime_{i}", pos, new Vector3(0.5f, 2f, 0.5f), new Color(0.9f, 0.9f, 1f), "Puzzle");
            }

            // Cloud platforms (traversal/rest points)
            for (int i = 0; i < cloudPlatformCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(10f, 20f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Cloud_Platform_{i}", pos, new Vector3(3f, 0.5f, 3f), new Color(1f, 1f, 1f, 0.8f), "Platform");
            }

            // Star fragments (collectible sky shards)
            for (int i = 0; i < starFragmentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(5f, 25f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Star_Fragment_{i}", pos, new Vector3(0.6f, 0.6f, 0.6f), new Color(1f, 1f, 0.9f), "Collectible");
            }

            // Celestial gates (portal/teleport points)
            for (int i = 0; i < celestialGateCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(12f, 18f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Celestial_Gate_{i}", pos, new Vector3(2f, 4f, 0.5f), new Color(0.8f, 0.9f, 1f), "Portal");
            }

            Debug.Log($"☁️ SKY INTERACTIVES: {interactiveObjects.Count} objects ready for player interaction");
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
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Smoothness", 0.9f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.3f);
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
