using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    [DefaultExecutionOrder(-60)]
    public class Moon9DynamicHazards : MonoBehaviour
    {
        [Header("Moon 9: Corruption Hazards")]
        [SerializeField] int corruptionPoolCount = 16;
        [SerializeField] int voidRiftCount = 10;
        [SerializeField] int blightZoneCount = 18;
        [SerializeField] int shadowTendrilCount = 14;

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            // Corruption Pools - corruption DOT + debuff
            for (int i = 0; i < corruptionPoolCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    -0.5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"CorruptionPool_{i}", pos, new Vector3(6f, 1.5f, 6f), new Color(0.4f, 0.1f, 0.5f, 0.7f), "Corruption", 9f);
            }

            // Void Rifts - instant high damage + teleport
            for (int i = 0; i < voidRiftCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    3f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"VoidRift_{i}", pos, new Vector3(4f, 6f, 4f), new Color(0.1f, 0f, 0.2f, 0.6f), "Void", 22f);
            }

            // Blight Zones - gradual corruption spread
            for (int i = 0; i < blightZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    1f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"BlightZone_{i}", pos, new Vector3(8f, 3f, 8f), new Color(0.3f, 0.15f, 0.3f, 0.5f), "Blight", 6f);
            }

            // Shadow Tendrils - grapple + drain
            for (int i = 0; i < shadowTendrilCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    2f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"ShadowTendril_{i}", pos, new Vector3(2f, 5f, 2f), new Color(0.2f, 0.05f, 0.3f, 0.8f), "Shadow", 11f);
            }

            Debug.Log($"💀 Moon9DynamicHazards spawned {hazards.Count} hazards");
        }

        GameObject CreateHazard(string hazardName, Vector3 position, Vector3 scale, Color color, string damageType, float damageAmount)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hazard.name = hazardName;
            hazard.transform.position = position;
            hazard.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.6f);
            mat.SetFloat("_Smoothness", 0.3f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            
            // Purple/dark emission for corruption
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.4f, 0.1f, 0.5f) * 0.6f);
            
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
