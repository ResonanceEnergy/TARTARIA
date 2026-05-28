using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-60)]
    public class Moon11DynamicHazards : MonoBehaviour
    {
        [Header("Moon 11: Prismatic Hazards")]
        [SerializeField] int prismTrapCount = 14;
        [SerializeField] int refractionBurnCount = 16;
        [SerializeField] int colorDrainCount = 12;
        [SerializeField] int spectrumOverloadCount = 10;

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            Color[] rainbowColors = GetRainbowColors();

            // Prism Traps - refraction damage with 7 color variants
            for (int i = 0; i < prismTrapCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    2f,
                    Random.Range(-80f, 80f)
                );
                Color color = rainbowColors[i % 7];
                color.a = 0.5f;
                CreateHazard($"PrismTrap_{i}", pos, new Vector3(3f, 4f, 3f), color, "Prism", 11f);
            }

            // Refraction Burns - focused light damage
            for (int i = 0; i < refractionBurnCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    1f,
                    Random.Range(-80f, 80f)
                );
                Color color = rainbowColors[i % 7];
                color.a = 0.6f;
                CreateHazard($"RefractionBurn_{i}", pos, new Vector3(2f, 6f, 2f), color, "Refraction", 13f);
            }

            // Color Drains - debuff zones
            for (int i = 0; i < colorDrainCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    0.5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"ColorDrain_{i}", pos, new Vector3(6f, 2f, 6f), new Color(0.5f, 0.5f, 0.5f, 0.3f), "Drain", 7f);
            }

            // Spectrum Overload - rainbow burst damage
            for (int i = 0; i < spectrumOverloadCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    3f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"SpectrumOverload_{i}", pos, new Vector3(5f, 5f, 5f), new Color(1f, 1f, 1f, 0.4f), "Spectrum", 18f);
            }

            Debug.Log($"🌈 Moon11DynamicHazards spawned {hazards.Count} hazards");
        }

        GameObject CreateHazard(string hazardName, Vector3 position, Vector3 scale, Color color, string damageType, float damageAmount)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hazard.name = hazardName;
            hazard.transform.position = position;
            hazard.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Smoothness", 1.0f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            
            // Bright emission for prismatic effects
            mat.EnableKeyword("_EMISSION");
            Color emissionColor = new Color(color.r, color.g, color.b, 1f);
            mat.SetColor("_EmissionColor", emissionColor * 1.2f);
            
            hazard.GetComponent<Renderer>().material = mat;

            Destroy(hazard.GetComponent<Collider>());
            BoxCollider trigger = hazard.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            HazardZone zone = hazard.AddComponent<HazardZone>();
            zone.damageAmount = damageAmount;
            zone.damageType = damageType;
            zone.effectDuration = 1f;

            hazards.Add(hazard);
            return hazard;
        }

        Color[] GetRainbowColors()
        {
            return new Color[]
            {
                new Color(1f, 0f, 0f),     // Red
                new Color(1f, 0.5f, 0f),   // Orange
                new Color(1f, 1f, 0f),     // Yellow
                new Color(0f, 1f, 0f),     // Green
                new Color(0f, 0.5f, 1f),   // Blue
                new Color(0.3f, 0f, 0.5f), // Indigo
                new Color(0.5f, 0f, 1f)    // Violet
            };
        }

        void OnDestroy()
        {
            foreach (GameObject hazard in hazards)
            {
                if (hazard != null) Destroy(hazard);
            }
            hazards.Clear();
        }
    }
}
