using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5: The Frostbound Citadel - Interactive Objects
    /// Execution order: -65 (after AmbientParticles -68)
    /// Spawns ice-themed interactables: frozen relics, ice puzzle blocks, aurora crystals, treasure glaciers
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class Moon5InteractiveObjects : MonoBehaviour
    {
        [Header("Ice Interactables")]
        [SerializeField] int frozenRelicCount = 9;
        [SerializeField] int icePuzzleBlockCount = 12;
        [SerializeField] int auroraCrystalCount = 10;
        [SerializeField] int treasureGlacierCount = 5;

        List<GameObject> interactiveObjects = new List<GameObject>();

        void Start()
        {
            SpawnInteractives();
        }

        void SpawnInteractives()
        {
            // Frozen relics (ancient artifacts in ice)
            for (int i = 0; i < frozenRelicCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(0.5f, 3f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Frozen_Relic_{i}", pos, new Vector3(0.8f, 1.2f, 0.8f), new Color(0.6f, 0.8f, 1f, 0.7f), "Rune");
            }

            // Ice puzzle blocks (movable frozen blocks)
            for (int i = 0; i < icePuzzleBlockCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.5f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Ice_Puzzle_Block_{i}", pos, new Vector3(2f, 2f, 2f), new Color(0.8f, 0.9f, 1f, 0.8f), "Puzzle");
            }

            // Aurora crystals (collectible sky shards)
            for (int i = 0; i < auroraCrystalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(0.5f, 4f),
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Aurora_Crystal_{i}", pos, new Vector3(0.6f, 1.8f, 0.6f), new Color(0.4f, 0.8f, 1f), "Collectible");
            }

            // Treasure glaciers (hidden ice vaults)
            for (int i = 0; i < treasureGlacierCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    1f,
                    Random.Range(-160f, 160f)
                );
                CreateInteractive($"Treasure_Glacier_{i}", pos, new Vector3(2f, 2f, 2f), new Color(0.7f, 0.85f, 0.95f, 0.6f), "Chest");
            }

            Debug.Log($"❄️ ICE INTERACTIVES: {interactiveObjects.Count} objects ready for player interaction");
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
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Smoothness", 0.9f);
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
