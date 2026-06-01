using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-57)]
    public class Moon12VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 12: Shadow Landmarks")]
        [SerializeField] int shadowObeliskCount = 4;
        [SerializeField] int umbralCitadelCount = 2;
        [SerializeField] int voidThroneCount = 1;
        [SerializeField] int darknessPillarCount = 7;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Shadow Obelisks - near-black markers
            for (int i = 0; i < shadowObeliskCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"ShadowObelisk_{i}", pos, new Vector3(6f, 22f, 6f), new Color(0.05f, 0.05f, 0.1f), new Color(0.1f, 0.05f, 0.15f));
            }

            // Umbral Citadels - massive dark structures
            for (int i = 0; i < umbralCitadelCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"UmbralCitadel_{i}", pos, new Vector3(18f, 32f, 18f), new Color(0.03f, 0.03f, 0.08f), new Color(0.08f, 0.04f, 0.12f));
            }

            // Void Throne - single central monument
            CreateLandmark("VoidThrone", new Vector3(0f, 0f, 0f), new Vector3(20f, 28f, 20f), new Color(0.02f, 0.02f, 0.06f), new Color(0.06f, 0.03f, 0.1f));

            // Darkness Pillars - tall shadow markers
            for (int i = 0; i < darknessPillarCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"DarknessPillar_{i}", pos, new Vector3(4f, 18f, 4f), new Color(0.06f, 0.06f, 0.12f), new Color(0.1f, 0.05f, 0.15f));
            }

            Debug.Log($"🌑 Moon12VisualLandmarks spawned {landmarks.Count} landmarks");
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
            mat.SetFloat("_Smoothness", 0.1f);
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", accentColor * 0.15f);
            
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
