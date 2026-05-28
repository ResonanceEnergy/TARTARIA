using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-60)]
    public class Moon13DynamicHazards : MonoBehaviour
    {
        [Header("Moon 13: Aether Convergence Hazards")]
        [SerializeField] int convergenceRiftCount = 10;
        [SerializeField] int aetherStormCount = 12;
        [SerializeField] int realityWarpCount = 14;
        [SerializeField] int tributeHazardCount = 12; // One per moon type

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            // Convergence Rifts - mixed elemental damage
            for (int i = 0; i < convergenceRiftCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    4f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"ConvergenceRift_{i}", pos, new Vector3(6f, 8f, 6f), new Color(0.8f, 0.9f, 1f, 0.5f), "Convergence", 15f);
            }

            // Aether Storms - reality damage
            for (int i = 0; i < aetherStormCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    6f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"AetherStorm_{i}", pos, new Vector3(9f, 10f, 9f), new Color(0.9f, 0.95f, 1f, 0.4f), "Aether", 18f);
            }

            // Reality Warps - existence disruption
            for (int i = 0; i < realityWarpCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    3f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"RealityWarp_{i}", pos, new Vector3(5f, 7f, 5f), new Color(0.7f, 0.8f, 0.95f, 0.6f), "Reality", 14f);
            }

            // Tribute Hazards - 12 hazards representing each moon's signature danger
            Color[] moonColors = GetMoonTributeColors();
            string[] tributeTypes = new string[] 
            { 
                "Jungle", "Desert", "Ice", "Lava", "Underwater", "Sky", 
                "Corruption", "Time", "Prismatic", "Shadow", "Memory", "Dream" 
            };
            
            float radius = 90f;
            for (int i = 0; i < tributeHazardCount; i++)
            {
                float angle = (i * 360f / tributeHazardCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    2f,
                    Mathf.Sin(angle) * radius
                );
                Color color = moonColors[i];
                color.a = 0.6f;
                CreateHazard($"TributeHazard_{tributeTypes[i]}", pos, new Vector3(4f, 6f, 4f), color, $"Tribute{tributeTypes[i]}", 13f);
            }

            Debug.Log($"✨ Moon13DynamicHazards spawned {hazards.Count} hazards (including 12 tribute hazards in circle formation)");
        }

        GameObject CreateHazard(string hazardName, Vector3 position, Vector3 scale, Color color, string damageType, float damageAmount)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hazard.name = hazardName;
            hazard.transform.position = position;
            hazard.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Smoothness", 0.9f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            
            // Strong white-blue emission for aether effects
            mat.EnableKeyword("_EMISSION");
            Color emissionColor = new Color(color.r, color.g, color.b, 1f);
            mat.SetColor("_EmissionColor", emissionColor * 1.5f);
            
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

        Color[] GetMoonTributeColors()
        {
            return new Color[]
            {
                new Color(0.2f, 0.6f, 0.2f),   // Moon3: Jungle - Green
                new Color(0.8f, 0.7f, 0.3f),   // Moon4: Desert - Sand
                new Color(0.7f, 0.9f, 1f),     // Moon5: Ice - Ice Blue
                new Color(1f, 0.3f, 0f),       // Moon6: Lava - Orange-Red
                new Color(0.2f, 0.4f, 0.7f),   // Moon7: Underwater - Deep Blue
                new Color(0.8f, 0.9f, 1f),     // Moon8: Sky - Light Blue
                new Color(0.4f, 0.1f, 0.5f),   // Moon9: Corruption - Purple
                new Color(0.5f, 0.6f, 0.7f),   // Moon10: Time - Gray-Blue
                new Color(1f, 1f, 1f),         // Moon11: Prismatic - White (rainbow)
                new Color(0.1f, 0.1f, 0.2f),   // Moon12: Shadow - Near-Black
                new Color(0.6f, 0.5f, 0.7f),   // Moon1: Memory - Soft Purple
                new Color(0.4f, 0.6f, 0.8f)    // Moon2: Dream - Soft Blue
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
