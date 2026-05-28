using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    [DefaultExecutionOrder(-57)]
    public class Moon7VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 7: Underwater Landmarks")]
        [SerializeField] int sunkenColossusCount = 2;
        [SerializeField] int underwaterRuinCount = 4;
        [SerializeField] int ancientAnchorCount = 3;
        [SerializeField] int coralTowerCount = 6;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Sunken Colossi - massive underwater statues
            for (int i = 0; i < sunkenColossusCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    5f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"SunkenColossus_{i}", pos, new Vector3(14f, 26f, 14f), new Color(0.3f, 0.5f, 0.6f), new Color(0.2f, 0.4f, 0.7f));
            }

            // Underwater Ruins - ancient structures
            for (int i = 0; i < underwaterRuinCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    3f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"UnderwaterRuin_{i}", pos, new Vector3(10f, 16f, 10f), new Color(0.4f, 0.5f, 0.6f), new Color(0.25f, 0.45f, 0.65f));
            }

            // Ancient Anchors - giant nautical artifacts
            for (int i = 0; i < ancientAnchorCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    2f,
                    Random.Range(-60f, 60f)
                );
                CreateLandmark($"AncientAnchor_{i}", pos, new Vector3(8f, 18f, 8f), new Color(0.35f, 0.45f, 0.55f), new Color(0.2f, 0.4f, 0.6f));
            }

            // Coral Towers - natural formations
            for (int i = 0; i < coralTowerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    1f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"CoralTower_{i}", pos, new Vector3(5f, 14f, 5f), new Color(0.5f, 0.3f, 0.4f), new Color(0.3f, 0.5f, 0.7f));
            }

            Debug.Log($"🌊 Moon7VisualLandmarks spawned {landmarks.Count} landmarks");
        }

        GameObject CreateLandmark(string name, Vector3 position, Vector3 scale, Color baseColor, Color accentColor)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            landmark.name = name;
            landmark.transform.position = position + new Vector3(0f, scale.y / 2f, 0f);
            landmark.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.3f);
            mat.SetFloat("_Smoothness", 0.6f);
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", accentColor * 0.12f);
            
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
