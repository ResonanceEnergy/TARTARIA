using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-57)]
    public class Moon4VisualLandmarks : MonoBehaviour
    {
        [Header("Moon 4: Desert Landmarks")]
        [SerializeField] int sandColossusCount = 2;
        [SerializeField] int driedOasisCount = 4;
        [SerializeField] int boneGraveyardCount = 3;
        [SerializeField] int sandstoneSpireCount = 6;

        List<GameObject> landmarks = new List<GameObject>();

        void Start()
        {
            SpawnLandmarks();
        }

        void SpawnLandmarks()
        {
            // Sand Colossi - massive statues
            for (int i = 0; i < sandColossusCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"SandColossus_{i}", pos, new Vector3(12f, 28f, 12f), new Color(0.8f, 0.7f, 0.5f), new Color(1f, 0.8f, 0.4f));
            }

            // Dried Oasis Monuments - ancient markers
            for (int i = 0; i < driedOasisCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"DriedOasis_{i}", pos, new Vector3(8f, 10f, 8f), new Color(0.7f, 0.6f, 0.4f), new Color(0.8f, 0.7f, 0.5f));
            }

            // Bone Graveyards - giant skeletal structures
            for (int i = 0; i < boneGraveyardCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    0f,
                    Random.Range(-60f, 60f)
                );
                CreateLandmark($"BoneGraveyard_{i}", pos, new Vector3(10f, 18f, 10f), new Color(0.9f, 0.9f, 0.8f), new Color(0.8f, 0.8f, 0.7f));
            }

            // Sandstone Spires - tall natural formations
            for (int i = 0; i < sandstoneSpireCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    0f,
                    Random.Range(-70f, 70f)
                );
                CreateLandmark($"SandstoneSpire_{i}", pos, new Vector3(4f, 16f, 4f), new Color(0.85f, 0.75f, 0.55f), new Color(0.9f, 0.8f, 0.6f));
            }

            Debug.Log($"🏜️ Moon4VisualLandmarks spawned {landmarks.Count} landmarks");
        }

        GameObject CreateLandmark(string name, Vector3 position, Vector3 scale, Color baseColor, Color accentColor)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position + new Vector3(0f, scale.y / 2f, 0f);
            landmark.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.05f);
            mat.SetFloat("_Smoothness", 0.15f);
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", accentColor * 0.05f);
            
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
