using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-57)]
    public class Moon6VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 6: Lava Landmarks")]
        [SerializeField] int volcanicForgeCount = 2;
        [SerializeField] int magmaTitanCount = 3;
        [SerializeField] int obsidianMonolithCount = 4;
        [SerializeField] int emberTowerCount = 6;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Volcanic Forges - massive industrial structures
            for (int i = 0; i < volcanicForgeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"VolcanicForge_{i}", pos, new Vector3(14f, 30f, 14f), new Color(0.3f, 0.15f, 0.1f), new Color(1f, 0.3f, 0f));
            }

            // Magma Titans - giant lava statues
            for (int i = 0; i < magmaTitanCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"MagmaTitan_{i}", pos, new Vector3(13f, 27f, 13f), new Color(0.4f, 0.2f, 0.1f), new Color(1f, 0.4f, 0f));
            }

            // Obsidian Monoliths - dark volcanic glass
            for (int i = 0; i < obsidianMonolithCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    0f,
                    Random.Range(-60f, 60f)
                );
                CreateLandmark($"ObsidianMonolith_{i}", pos, new Vector3(6f, 24f, 6f), new Color(0.15f, 0.1f, 0.1f), new Color(0.8f, 0.2f, 0f));
            }

            // Ember Towers - glowing volcanic spires
            for (int i = 0; i < emberTowerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"EmberTower_{i}", pos, new Vector3(4f, 20f, 4f), new Color(0.5f, 0.25f, 0.15f), new Color(1f, 0.35f, 0f));
            }

            Debug.Log($"🌋 Moon6VisualLandmarks spawned {landmarks.Count} landmarks");
        }

        GameObject CreateLandmark(string name, Vector3 position, Vector3 scale, Color baseColor, Color accentColor)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position + new Vector3(0f, scale.y / 2f, 0f);
            landmark.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.6f);
            mat.SetFloat("_Smoothness", 0.3f);
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", accentColor * 0.4f);
            
            landmark.GetComponent<Renderer>().material = mat;

            landmarks.Add(landmark);
            return landmark;
        }

        void OnDestroy()
        {
            foreach (GameObject landmark in landmarks)
            {
                if (landmark != null) Destroy(landmark);
            }
            landmarks.Clear();
        }
    }
}
