using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-60)]
    public class Moon7DynamicHazards : MonoBehaviour
    {
        [Header("Moon 7: Underwater Hazards")]
        [SerializeField] int whirlpoolCount = 10;
        [SerializeField] int pressureZoneCount = 12;
        [SerializeField] int electricEelCount = 14;
        [SerializeField] int acidicCurrentCount = 16;

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            // Whirlpools - pull + drowning damage
            for (int i = 0; i < whirlpoolCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    -2f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"Whirlpool_{i}", pos, new Vector3(8f, 6f, 8f), new Color(0.2f, 0.4f, 0.7f, 0.4f), "Whirlpool", 12f);
            }

            // Pressure Zones - crushing damage
            for (int i = 0; i < pressureZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    -5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"PressureZone_{i}", pos, new Vector3(6f, 8f, 6f), new Color(0.1f, 0.2f, 0.5f, 0.5f), "Pressure", 10f);
            }

            // Electric Eels - shock damage
            for (int i = 0; i < electricEelCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    1f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"ElectricEel_{i}", pos, new Vector3(3f, 2f, 3f), new Color(0.3f, 0.5f, 0.9f, 0.6f), "Electric", 14f);
            }

            // Acidic Currents - corrosion DOT
            for (int i = 0; i < acidicCurrentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    0f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"AcidicCurrent_{i}", pos, new Vector3(5f, 3f, 5f), new Color(0.4f, 0.7f, 0.3f, 0.4f), "Acid", 7f);
            }

            Debug.Log($"🌊 Moon7DynamicHazards spawned {hazards.Count} hazards");
        }

        GameObject CreateHazard(string hazardName, Vector3 position, Vector3 scale, Color color, string damageType, float damageAmount)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hazard.name = hazardName;
            hazard.transform.position = position;
            hazard.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.3f);
            mat.SetFloat("_Smoothness", 0.8f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            
            if (damageType == "Electric")
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.3f, 0.5f, 1f) * 0.8f);
            }
            
            hazard.GetComponent<Renderer>().material = mat;

            Destroy(hazard.GetComponent<Collider>());
            SphereCollider trigger = hazard.AddComponent<SphereCollider>();
            trigger.isTrigger = true;

            HazardZone zone = hazard.AddComponent<HazardZone>();
            zone.damageAmount = damageAmount;
            zone.damageType = damageType;
            zone.effectDuration = 1f;

            hazards.Add(hazard);
            return hazard;
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
