using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-60)]
    public class Moon8DynamicHazards : MonoBehaviour
    {
        [Header("Moon 8: Sky Hazards")]
        [SerializeField] int lightningStormCount = 12;
        [SerializeField] int windGustCount = 16;
        [SerializeField] int turbulenceZoneCount = 14;
        [SerializeField] int fallingDebrisCount = 10;

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            // Lightning Storms - high electric burst damage
            for (int i = 0; i < lightningStormCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    10f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"LightningStorm_{i}", pos, new Vector3(4f, 12f, 4f), new Color(0.9f, 0.9f, 1f, 0.3f), "Lightning", 18f);
            }

            // Wind Gusts - knockback + movement disruption
            for (int i = 0; i < windGustCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    6f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"WindGust_{i}", pos, new Vector3(7f, 5f, 7f), new Color(0.8f, 0.9f, 1f, 0.2f), "Wind", 5f);
            }

            // Turbulence Zones - disorientation + damage
            for (int i = 0; i < turbulenceZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    8f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"TurbulenceZone_{i}", pos, new Vector3(9f, 6f, 9f), new Color(0.7f, 0.8f, 0.95f, 0.25f), "Turbulence", 8f);
            }

            // Falling Debris - crushing impact damage
            for (int i = 0; i < fallingDebrisCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    15f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"FallingDebris_{i}", pos, new Vector3(3f, 8f, 3f), new Color(0.5f, 0.5f, 0.6f, 0.6f), "Crushing", 20f);
            }

            Debug.Log($"☁️ Moon8DynamicHazards spawned {hazards.Count} hazards");
        }

        GameObject CreateHazard(string hazardName, Vector3 position, Vector3 scale, Color color, string damageType, float damageAmount)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hazard.name = hazardName;
            hazard.transform.position = position;
            hazard.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.4f);
            mat.SetFloat("_Smoothness", 0.7f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            
            if (damageType == "Lightning")
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.8f, 0.9f, 1f) * 1.5f);
            }
            
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
