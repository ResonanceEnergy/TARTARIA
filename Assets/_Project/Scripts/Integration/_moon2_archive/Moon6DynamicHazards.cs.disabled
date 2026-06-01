using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-60)]
    public class Moon6DynamicHazards : MonoBehaviour
    {
        [Header("Moon 6: Lava Hazards")]
        [SerializeField] int lavaFlowCount = 15;
        [SerializeField] int emberGeyserCount = 12;
        [SerializeField] int heatVentCount = 18;
        [SerializeField] int magmaPoolCount = 10;

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            // Lava Flows - extreme fire DOT
            for (int i = 0; i < lavaFlowCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    0.3f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"LavaFlow_{i}", pos, new Vector3(6f, 1f, 6f), new Color(1f, 0.3f, 0f, 0.8f), "Lava", 25f);
            }

            // Ember Geysers - burst fire damage + knockback
            for (int i = 0; i < emberGeyserCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    2f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"EmberGeyser_{i}", pos, new Vector3(3f, 5f, 3f), new Color(1f, 0.5f, 0f, 0.6f), "Fire", 15f);
            }

            // Heat Vents - radiant damage
            for (int i = 0; i < heatVentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    0.5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"HeatVent_{i}", pos, new Vector3(4f, 2f, 4f), new Color(1f, 0.6f, 0.2f, 0.5f), "Heat", 8f);
            }

            // Magma Pools - instant death
            for (int i = 0; i < magmaPoolCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    -0.5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"MagmaPool_{i}", pos, new Vector3(7f, 1.5f, 7f), new Color(1f, 0.2f, 0f, 0.9f), "Magma", 100f);
            }

            Debug.Log($"🌋 Moon6DynamicHazards spawned {hazards.Count} hazards");
        }

        GameObject CreateHazard(string hazardName, Vector3 position, Vector3 scale, Color color, string damageType, float damageAmount)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hazard.name = hazardName;
            hazard.transform.position = position;
            hazard.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Smoothness", 0.4f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            
            // Strong emission for lava/fire hazards
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f) * 2f);
            
            hazard.GetComponent<Renderer>().material = mat;

            Destroy(hazard.GetComponent<Collider>());
            BoxCollider trigger = hazard.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            HazardZone zone = hazard.AddComponent<HazardZone>();
            zone.damageAmount = damageAmount;
            zone.damageType = damageType;
            zone.effectDuration = 0.5f; // Faster damage tick for fire

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
