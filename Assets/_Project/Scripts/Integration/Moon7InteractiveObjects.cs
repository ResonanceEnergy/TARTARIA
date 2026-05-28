using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7: The Abyssal Depths - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns underwater-themed interactables: pearl clams, ancient shipwrecks, coral altars, abyssal orbs
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon7InteractiveObjects : MonoBehaviour
    {
        [Header("Underwater Interactables")]
        [SerializeField] int pearlClamCount = 12;
        [SerializeField] int ancientShipwreckCount = 5;
        [SerializeField] int coralAltarCount = 8;
        [SerializeField] int abyssalOrbCount = 10;

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // Pearl clams (collectible treasure)
            for (int i = 0; i < pearlClamCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.3f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Pearl_Clam_{i}", pos, new Vector3(0.8f, 0.5f, 0.8f), new Color(0.9f, 0.85f, 0.95f), "Collectible");
            }

            // Ancient shipwrecks (exploration loot)
            for (int i = 0; i < ancientShipwreckCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Ancient_Shipwreck_{i}", pos, new Vector3(3f, 2f, 5f), new Color(0.4f, 0.3f, 0.2f), "Chest");
            }

            // Coral altars (underwater shrines)
            for (int i = 0; i < coralAltarCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Coral_Altar_{i}", pos, new Vector3(1.5f, 2f, 1.5f), new Color(0.8f, 0.4f, 0.6f), "Shrine");
            }

            // Abyssal orbs (deep sea energy sources)
            for (int i = 0; i < abyssalOrbCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(3f, 10f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Abyssal_Orb_{i}", pos, new Vector3(1f, 1f, 1f), new Color(0.1f, 0.4f, 0.8f), "Collectible");
            }

            Debug.Log($"🌊 UNDERWATER INTERACTIVES: {interactiveObjects.Count} objects ready for player interaction");
        }

        void CreateInteractive(string name, Vector3 position, Vector3 scale, Color color, string tag)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = name;
            obj.transform.position = position;
            obj.transform.localScale = scale;
            obj.transform.parent = transform;
            obj.tag = tag;

            Renderer renderer = obj.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.4f);
            mat.SetFloat("_Smoothness", 0.8f);
            renderer.material = mat;

            SphereCollider collider = obj.GetComponent<SphereCollider>();
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
