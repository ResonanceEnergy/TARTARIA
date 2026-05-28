using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-57)]
    public class Moon9VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 9: Corruption Landmarks")]
        [SerializeField] int corruptionSpireCount = 3;
        [SerializeField] int voidPortalCount = 2;
        [SerializeField] int blightedMonumentCount = 4;
        [SerializeField] int shadowObeliskCount = 6;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Corruption Spires - twisted structures
            for (int i = 0; i < corruptionSpireCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"CorruptionSpire_{i}", pos, new Vector3(10f, 28f, 10f), new Color(0.3f, 0.1f, 0.4f), new Color(0.5f, 0.1f, 0.6f));
            }

            // Void Portals - massive rifts
            for (int i = 0; i < voidPortalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"VoidPortal_{i}", pos, new Vector3(16f, 24f, 16f), new Color(0.2f, 0.05f, 0.3f), new Color(0.6f, 0.1f, 0.7f));
            }

            // Blighted Monuments - corrupted ancient structures
            for (int i = 0; i < blightedMonumentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    0f,
                    Random.Range(-60f, 60f)
                );
                CreateLandmark($"BlightedMonument_{i}", pos, new Vector3(12f, 20f, 12f), new Color(0.25f, 0.08f, 0.35f), new Color(0.55f, 0.1f, 0.65f));
            }

            // Shadow Obelisks - dark markers
            for (int i = 0; i < shadowObeliskCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"ShadowObelisk_{i}", pos, new Vector3(4f, 16f, 4f), new Color(0.28f, 0.09f, 0.38f), new Color(0.5f, 0.1f, 0.6f));
            }

            Debug.Log($"☠️ Moon9VisualLandmarks spawned {landmarks.Count} landmarks");
        }

        GameObject CreateLandmark(string name, Vector3 position, Vector3 scale, Color baseColor, Color accentColor)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position + new Vector3(0f, scale.y / 2f, 0f);
            landmark.transform.localScale = scale;
            landmark.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-10f, 10f));

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Smoothness", 0.4f);
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", accentColor * 0.5f);
            
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
