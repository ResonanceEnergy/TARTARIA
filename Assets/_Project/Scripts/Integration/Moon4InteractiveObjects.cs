using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4: The Sunscorched Oasis - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns desert-themed interactables: sun idols, buried treasures, oasis shrines, mirage crystals
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon4InteractiveObjects : MonoBehaviour
    {
        [Header("Desert Interactables")]
        [SerializeField] int sunIdolCount = 7;
        [SerializeField] int buriedTreasureCount = 8;
        [SerializeField] int oasisShrineCount = 4;
        [SerializeField] int mirageCrystalCount = 10;

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // Sun idols (puzzle/worship elements)
            for (int i = 0; i < sunIdolCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Sun_Idol_{i}", pos, new Vector3(1.2f, 3f, 1.2f), new Color(0.9f, 0.7f, 0.3f), "Rune");
            }

            // Buried treasures (dig/excavate points)
            for (int i = 0; i < buriedTreasureCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.2f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Buried_Treasure_{i}", pos, new Vector3(0.8f, 0.5f, 0.8f), new Color(0.8f, 0.6f, 0.4f), "Treasure");
            }

            // Oasis shrines (power-up/rest points)
            for (int i = 0; i < oasisShrineCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Oasis_Shrine_{i}", pos, new Vector3(1.5f, 2.5f, 1.5f), new Color(0.4f, 0.7f, 0.9f), "Shrine");
            }

            // Mirage crystals (collectible illusion fragments)
            for (int i = 0; i < mirageCrystalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Mirage_Crystal_{i}", pos, new Vector3(0.5f, 1.5f, 0.5f), new Color(1f, 0.9f, 0.7f, 0.6f), "Collectible");
            }

            Debug.Log($"🏜️ DESERT INTERACTIVES: {interactiveObjects.Count} objects ready for player interaction");
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
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Smoothness", 0.5f);
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
