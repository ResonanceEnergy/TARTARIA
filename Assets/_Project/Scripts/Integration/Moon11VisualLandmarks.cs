using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-57)]
    public class Moon11VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 11: Prismatic Landmarks")]
        [SerializeField] int prismTowerCount = 3;
        [SerializeField] int crystalCathedralCount = 2;
        [SerializeField] int spectrumMonumentCount = 5;
        [SerializeField] int rainbowObeliskCount = 7;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Prism Towers - rainbow structures
            Color[] rainbowColors = GetRainbowColors();
            for (int i = 0; i < prismTowerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                Color color = rainbowColors[i % rainbowColors.Length];
                CreateLandmark($"PrismTower_{i}", pos, new Vector3(10f, 32f, 10f), color, color);
            }

            // Crystal Cathedrals - massive prismatic structures
            for (int i = 0; i < crystalCathedralCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                Color color = rainbowColors[(i * 3) % rainbowColors.Length];
                CreateLandmark($"CrystalCathedral_{i}", pos, new Vector3(16f, 30f, 16f), color, color);
            }

            // Spectrum Monuments - colorful markers
            for (int i = 0; i < spectrumMonumentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    0f,
                    Random.Range(-60f, 60f)
                );
                Color color = rainbowColors[i % rainbowColors.Length];
                CreateLandmark($"SpectrumMonument_{i}", pos, new Vector3(8f, 22f, 8f), color, color);
            }

            // Rainbow Obelisks - tall prisms
            for (int i = 0; i < rainbowObeliskCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                Color color = rainbowColors[i % rainbowColors.Length];
                CreateLandmark($"RainbowObelisk_{i}", pos, new Vector3(4f, 18f, 4f), color, color);
            }

            Debug.Log($"🌈 Moon11VisualLandmarks spawned {landmarks.Count} landmarks");
        }

        GameObject CreateLandmark(string name, Vector3 position, Vector3 scale, Color baseColor, Color accentColor)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position + new Vector3(0f, scale.y / 2f, 0f);
            landmark.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Smoothness", 1f);
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", accentColor * 0.8f);
            
            landmark.GetComponent<Renderer>().material = mat;

            landmarks.Add(landmark);
            return landmark;
        }

        Color[] GetRainbowColors()
        {
            return new Color[]
            {
                new Color(1f, 0f, 0f),      // Red
                new Color(1f, 0.5f, 0f),    // Orange
                new Color(1f, 1f, 0f),      // Yellow
                new Color(0f, 1f, 0f),      // Green
                new Color(0f, 0.5f, 1f),    // Blue
                new Color(0.3f, 0f, 0.5f),  // Indigo
                new Color(0.5f, 0f, 1f)     // Violet
            };
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
