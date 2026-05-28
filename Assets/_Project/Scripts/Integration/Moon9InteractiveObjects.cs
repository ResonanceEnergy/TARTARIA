using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 9: The Blighted Wastes - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns corruption-themed interactables: corruption obelisks, void rifts, blighted shrines, purification crystals
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon9InteractiveObjects : MonoBehaviour
    {
        [Header("Corruption Interactables")]
        [SerializeField] int corruptionObeliskCount = 8;
        [SerializeField] int voidRiftCount = 6;
        [SerializeField] int blightedShrineCount = 5;
        [SerializeField] int purificationCrystalCount = 12;

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // Corruption obelisks (dark power sources)
            for (int i = 0; i < corruptionObeliskCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Corruption_Obelisk_{i}", pos, new Vector3(1f, 4f, 1f), new Color(0.3f, 0.1f, 0.4f), "Rune");
            }

            // Void rifts (portal to corruption dimension)
            for (int i = 0; i < voidRiftCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(2f, 6f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Void_Rift_{i}", pos, new Vector3(2f, 3f, 0.3f), new Color(0.2f, 0f, 0.3f, 0.8f), "Portal");
            }

            // Blighted shrines (corrupted altars)
            for (int i = 0; i < blightedShrineCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Blighted_Shrine_{i}", pos, new Vector3(1.5f, 2f, 1.5f), new Color(0.4f, 0.2f, 0.4f), "Shrine");
            }

            // Purification crystals (cleansing collectibles)
            for (int i = 0; i < purificationCrystalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(0.5f, 3f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Purification_Crystal_{i}", pos, new Vector3(0.7f, 1.5f, 0.7f), new Color(0.9f, 0.95f, 1f), "Collectible");
            }

            Debug.Log($"💀 CORRUPTION INTERACTIVES: {interactiveObjects.Count} objects ready for player interaction");
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
            mat.SetFloat("_Metallic", 0.3f);
            mat.SetFloat("_Smoothness", 0.6f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.4f);
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
