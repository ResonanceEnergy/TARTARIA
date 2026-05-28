using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-57)]
    public class Moon3VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 3: Jungle Landmarks")]
        [SerializeField] int ancientTreeCount = 3;
        [SerializeField] int stoneRuinCount = 5;
        [SerializeField] int overgrownTempleCount = 2;
        [SerializeField] int vineCoveredPillarCount = 8;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Ancient Trees - massive vertical structures
            for (int i = 0; i < ancientTreeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"AncientTree_{i}", pos, new Vector3(8f, 30f, 8f), new Color(0.3f, 0.2f, 0.1f), new Color(0.2f, 0.5f, 0.2f));
            }

            // Stone Ruins - ancient structures
            for (int i = 0; i < stoneRuinCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"StoneRuin_{i}", pos, new Vector3(10f, 12f, 10f), new Color(0.5f, 0.5f, 0.5f), new Color(0.3f, 0.5f, 0.3f));
            }

            // Overgrown Temples - massive structures
            for (int i = 0; i < overgrownTempleCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    0f,
                    Random.Range(-60f, 60f)
                );
                CreateLandmark($"OvergrownTemple_{i}", pos, new Vector3(15f, 20f, 15f), new Color(0.4f, 0.4f, 0.4f), new Color(0.2f, 0.6f, 0.2f));
            }

            // Vine Covered Pillars - tall markers
            for (int i = 0; i < vineCoveredPillarCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"VinePillar_{i}", pos, new Vector3(3f, 15f, 3f), new Color(0.5f, 0.5f, 0.5f), new Color(0.2f, 0.5f, 0.2f));
            }

            Debug.Log($"🌳 Moon3VisualLandmarks spawned {landmarks.Count} landmarks");
        }

        GameObject CreateLandmark(string name, Vector3 position, Vector3 scale, Color baseColor, Color accentColor)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position + new Vector3(0f, scale.y / 2f, 0f);
            landmark.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Smoothness", 0.2f);
            
            // Very subtle emission for visibility
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", accentColor * 0.1f);
            
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
