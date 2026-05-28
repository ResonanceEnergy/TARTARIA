using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-57)]
    public class Moon10VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 10: Time Landmarks")]
        [SerializeField] int timeRiftPillarCount = 3;
        [SerializeField] int temporalNexusCount = 2;
        [SerializeField] int chronoBeaconCount = 4;
        [SerializeField] int paradoxMonolithCount = 5;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Time Rift Pillars - distorted structures
            for (int i = 0; i < timeRiftPillarCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"TimeRiftPillar_{i}", pos, new Vector3(8f, 26f, 8f), new Color(0.5f, 0.6f, 0.7f), new Color(0.6f, 0.7f, 0.8f));
            }

            // Temporal Nexus - massive time focal points
            for (int i = 0; i < temporalNexusCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"TemporalNexus_{i}", pos, new Vector3(14f, 28f, 14f), new Color(0.45f, 0.55f, 0.65f), new Color(0.65f, 0.75f, 0.85f));
            }

            // Chrono Beacons - time markers
            for (int i = 0; i < chronoBeaconCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    0f,
                    Random.Range(-60f, 60f)
                );
                CreateLandmark($"ChronoBeacon_{i}", pos, new Vector3(6f, 20f, 6f), new Color(0.48f, 0.58f, 0.68f), new Color(0.62f, 0.72f, 0.82f));
            }

            // Paradox Monoliths - impossible geometry
            for (int i = 0; i < paradoxMonolithCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"ParadoxMonolith_{i}", pos, new Vector3(5f, 18f, 5f), new Color(0.52f, 0.62f, 0.72f), new Color(0.6f, 0.7f, 0.8f));
            }

            Debug.Log($"⏰ Moon10VisualLandmarks spawned {landmarks.Count} landmarks");
        }

        GameObject CreateLandmark(string name, Vector3 position, Vector3 scale, Color baseColor, Color accentColor)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position + new Vector3(0f, scale.y / 2f, 0f);
            landmark.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.7f);
            mat.SetFloat("_Smoothness", 0.8f);
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", accentColor * 0.25f);
            
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
