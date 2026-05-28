using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12: The Umbral Sanctum - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns shadow-themed interactables: shadow keystones, void wells, umbral shrines, darkness essence
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon12InteractiveObjects : MonoBehaviour
    {
        [Header("Shadow Interactables")]
        [SerializeField] int shadowKeystoneCount = 8;
        [SerializeField] int voidWellCount = 6;
        [SerializeField] int umbralShrineCount = 5;
        [SerializeField] int darknessEssenceCount = 14;

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // Shadow keystones (dark power anchors)
            for (int i = 0; i < shadowKeystoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Shadow_Keystone_{i}", pos, new Vector3(1f, 3f, 1f), new Color(0.05f, 0.05f, 0.1f), "Rune");
            }

            // Void wells (shadow dimension portals)
            for (int i = 0; i < voidWellCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.2f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Void_Well_{i}", pos, new Vector3(2.5f, 0.5f, 2.5f), new Color(0.02f, 0.02f, 0.05f), "Portal");
            }

            // Umbral shrines (shadow altars)
            for (int i = 0; i < umbralShrineCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Umbral_Shrine_{i}", pos, new Vector3(1.8f, 2.5f, 1.8f), new Color(0.08f, 0.08f, 0.12f), "Shrine");
            }

            // Darkness essence (shadow collectibles)
            for (int i = 0; i < darknessEssenceCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(0.5f, 4f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Darkness_Essence_{i}", pos, new Vector3(0.7f, 0.7f, 0.7f), new Color(0.1f, 0.1f, 0.15f), "Collectible");
            }

            Debug.Log($"🌑 SHADOW INTERACTIVES: {interactiveObjects.Count} objects ready for player interaction");
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
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Smoothness", 0.3f);
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
