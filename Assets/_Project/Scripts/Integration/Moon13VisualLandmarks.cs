using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-57)]
    public class Moon13VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 13: Aether Convergence Landmarks")]
        [SerializeField] int convergenceSpireCount = 1; // Tallest structure
        [SerializeField] int moonTributeTowerCount = 12; // One per moon
        [SerializeField] int aetherGatewayCount = 4;
        [SerializeField] int realityPillarCount = 8;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Convergence Spire - THE TALLEST STRUCTURE (center)
            CreateLandmark("ConvergenceSpire_FINAL", Vector3.zero, new Vector3(20f, 40f, 20f), new Color(0.9f, 0.95f, 1f), new Color(1f, 1f, 1f));

            // Moon Tribute Towers - 12 towers in circle (one per moon)
            Color[] moonColors = GetMoonTributeColors();
            string[] moonNames = new string[] 
            { 
                "Memory", "Dream", "Jungle", "Desert", "Ice", "Lava", 
                "Underwater", "Sky", "Corruption", "Time", "Prismatic", "Shadow" 
            };
            
            float radius = 100f;
            for (int i = 0; i < moonTributeTowerCount; i++)
            {
                float angle = (i * 360f / moonTributeTowerCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );
                CreateLandmark($"TributeTower_{moonNames[i]}", pos, new Vector3(8f, 25f, 8f), moonColors[i], moonColors[i]);
            }

            // Aether Gateways - portal-like structures
            for (int i = 0; i < aetherGatewayCount; i++)
            {
                float angle = (i * 90f) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 50f,
                    0f,
                    Mathf.Sin(angle) * 50f
                );
                CreateLandmark($"AetherGateway_{i}", pos, new Vector3(12f, 28f, 12f), new Color(0.85f, 0.9f, 1f), new Color(0.95f, 0.98f, 1f));
            }

            // Reality Pillars - stabilizing structures
            for (int i = 0; i < realityPillarCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"RealityPillar_{i}", pos, new Vector3(5f, 20f, 5f), new Color(0.88f, 0.92f, 1f), new Color(0.9f, 0.95f, 1f));
            }

            Debug.Log($"✨ Moon13VisualLandmarks spawned {landmarks.Count} landmarks (including 12-moon tribute tower circle at radius 100f and FINAL convergence spire at height 40m)");
        }

        GameObject CreateLandmark(string name, Vector3 position, Vector3 scale, Color baseColor, Color accentColor)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position + new Vector3(0f, scale.y / 2f, 0f);
            landmark.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Smoothness", 0.9f);
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", accentColor * 1.2f);
            
            landmark.GetComponent<Renderer>().material = mat;

            landmarks.Add(landmark);
            return landmark;
        }

        Color[] GetMoonTributeColors()
        {
            return new Color[]
            {
                new Color(0.6f, 0.5f, 0.7f),   // Moon1: Memory - Soft Purple
                new Color(0.4f, 0.6f, 0.8f),   // Moon2: Dream - Soft Blue
                new Color(0.2f, 0.6f, 0.2f),   // Moon3: Jungle - Green
                new Color(0.8f, 0.7f, 0.3f),   // Moon4: Desert - Sand
                new Color(0.7f, 0.9f, 1f),     // Moon5: Ice - Ice Blue
                new Color(1f, 0.3f, 0f),       // Moon6: Lava - Orange-Red
                new Color(0.2f, 0.4f, 0.7f),   // Moon7: Underwater - Deep Blue
                new Color(0.8f, 0.9f, 1f),     // Moon8: Sky - Light Blue
                new Color(0.4f, 0.1f, 0.5f),   // Moon9: Corruption - Purple
                new Color(0.5f, 0.6f, 0.7f),   // Moon10: Time - Gray-Blue
                new Color(1f, 1f, 1f),         // Moon11: Prismatic - White (rainbow)
                new Color(0.1f, 0.1f, 0.2f)    // Moon12: Shadow - Near-Black
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
