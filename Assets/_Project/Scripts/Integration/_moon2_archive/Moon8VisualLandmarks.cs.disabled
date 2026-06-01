using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-57)]
    public class Moon8VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 8: Sky Landmarks")]
        [SerializeField] int celestialTowerCount = 2;
        [SerializeField] int floatingIslandCount = 5;
        [SerializeField] int cloudTempleCount = 3;
        [SerializeField] int windSpireCount = 7;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Celestial Towers - tallest structures
            for (int i = 0; i < celestialTowerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    10f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"CelestialTower_{i}", pos, new Vector3(12f, 35f, 12f), new Color(0.9f, 0.95f, 1f), new Color(0.8f, 0.9f, 1f));
            }

            // Floating Islands - suspended platforms
            for (int i = 0; i < floatingIslandCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(8f, 15f),
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"FloatingIsland_{i}", pos, new Vector3(15f, 8f, 15f), new Color(0.7f, 0.8f, 0.9f), new Color(0.85f, 0.9f, 1f));
            }

            // Cloud Temples - ethereal structures
            for (int i = 0; i < cloudTempleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    12f,
                    Random.Range(-60f, 60f)
                );
                CreateLandmark($"CloudTemple_{i}", pos, new Vector3(14f, 22f, 14f), new Color(0.85f, 0.9f, 1f), new Color(0.9f, 0.95f, 1f));
            }

            // Wind Spires - tall slim towers
            for (int i = 0; i < windSpireCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    6f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"WindSpire_{i}", pos, new Vector3(3f, 18f, 3f), new Color(0.88f, 0.92f, 1f), new Color(0.85f, 0.9f, 1f));
            }

            Debug.Log($"☁️ Moon8VisualLandmarks spawned {landmarks.Count} landmarks");
        }

        GameObject CreateLandmark(string name, Vector3 position, Vector3 scale, Color baseColor, Color accentColor)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position + new Vector3(0f, scale.y / 2f, 0f);
            landmark.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Smoothness", 0.7f);
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", accentColor * 0.2f);
            
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
