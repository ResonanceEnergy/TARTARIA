using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-57)]
    public class Moon5VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 5: Ice Landmarks")]
        [SerializeField] int icePalaceSpireCount = 2;
        [SerializeField] int frozenGuardianCount = 3;
        [SerializeField] int crystallineObeliskCount = 5;
        [SerializeField] int glacialPillarCount = 7;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Ice Palace Spires - majestic structures
            for (int i = 0; i < icePalaceSpireCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"IcePalaceSpire_{i}", pos, new Vector3(10f, 32f, 10f), new Color(0.85f, 0.92f, 1f), new Color(0.7f, 0.9f, 1f));
            }

            // Frozen Guardians - giant ice statues
            for (int i = 0; i < frozenGuardianCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"FrozenGuardian_{i}", pos, new Vector3(12f, 25f, 12f), new Color(0.8f, 0.9f, 1f), new Color(0.6f, 0.85f, 1f));
            }

            // Crystalline Obelisks - sharp ice formations
            for (int i = 0; i < crystallineObeliskCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    0f,
                    Random.Range(-60f, 60f)
                );
                CreateLandmark($"CrystallineObelisk_{i}", pos, new Vector3(5f, 22f, 5f), new Color(0.9f, 0.95f, 1f), new Color(0.8f, 0.9f, 1f));
            }

            // Glacial Pillars - ice columns
            for (int i = 0; i < glacialPillarCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"GlacialPillar_{i}", pos, new Vector3(4f, 18f, 4f), new Color(0.88f, 0.93f, 1f), new Color(0.75f, 0.88f, 1f));
            }

            Debug.Log($"❄️ Moon5VisualLandmarks spawned {landmarks.Count} landmarks");
        }

        GameObject CreateLandmark(string name, Vector3 position, Vector3 scale, Color baseColor, Color accentColor)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position + new Vector3(0f, scale.y / 2f, 0f);
            landmark.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.4f);
            mat.SetFloat("_Smoothness", 0.9f);
            
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
