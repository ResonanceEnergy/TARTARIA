using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3: The Verdant Labyrinth - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns jungle-themed interactables: ancient rune stones, treasure vines, crystal flowers, hidden caches
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon3InteractiveObjects : MonoBehaviour
    {
        [Header("Jungle Interactables")]
        [SerializeField] int runeStoneCount = 8;
        [SerializeField] int treasureVineCount = 6;
        [SerializeField] int crystalFlowerCount = 12;
        [SerializeField] int hiddenCacheCount = 5;

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // Ancient rune stones (lore/puzzle elements)
            for (int i = 0; i < runeStoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Rune_Stone_{i}", pos, new Vector3(1f, 2f, 1f), new Color(0.4f, 0.6f, 0.3f), "Rune");
            }

            // Treasure vines (collectible access points)
            for (int i = 0; i < treasureVineCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(3f, 8f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Treasure_Vine_{i}", pos, new Vector3(0.5f, 4f, 0.5f), new Color(0.3f, 0.5f, 0.2f), "Treasure");
            }

            // Crystal flowers (collectible resources)
            for (int i = 0; i < crystalFlowerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.3f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Crystal_Flower_{i}", pos, new Vector3(0.6f, 0.6f, 0.6f), new Color(0.2f, 0.9f, 0.3f), "Collectible");
            }

            // Hidden caches (treasure chests)
            for (int i = 0; i < hiddenCacheCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Hidden_Cache_{i}", pos, new Vector3(1f, 0.8f, 1f), new Color(0.6f, 0.5f, 0.3f), "Chest");
            }

            Debug.Log($"🌿 JUNGLE INTERACTIVES: {interactiveObjects.Count} objects ready for player interaction");
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
            renderer.material = mat;

            // Add collider for interaction
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
