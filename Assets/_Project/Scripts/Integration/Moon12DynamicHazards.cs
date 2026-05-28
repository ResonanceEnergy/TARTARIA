using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-60)]
    public class Moon12DynamicHazards : MonoBehaviour
    {
        [Header("Moon 12: Shadow Hazards")]
        [SerializeField] int shadowVoidCount = 14;
        [SerializeField] int darknessWaveCount = 16;
        [SerializeField] int umbralZoneCount = 18;
        [SerializeField] int voidStalkerCount = 12;

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            // Shadow Voids - drain life force
            for (int i = 0; i < shadowVoidCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    1f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"ShadowVoid_{i}", pos, new Vector3(5f, 5f, 5f), new Color(0.05f, 0.05f, 0.1f, 0.8f), "Void", 12f);
            }

            // Darkness Waves - vision obscure + DOT
            for (int i = 0; i < darknessWaveCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    2f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"DarknessWave_{i}", pos, new Vector3(7f, 4f, 7f), new Color(0.1f, 0.1f, 0.15f, 0.6f), "Darkness", 8f);
            }

            // Umbral Zones - shadow corruption
            for (int i = 0; i < umbralZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    0.5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"UmbralZone_{i}", pos, new Vector3(6f, 3f, 6f), new Color(0.08f, 0.08f, 0.12f, 0.7f), "Umbral", 9f);
            }

            // Void Stalkers - high damage traps
            for (int i = 0; i < voidStalkerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    3f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"VoidStalker_{i}", pos, new Vector3(3f, 6f, 3f), new Color(0.03f, 0.03f, 0.08f, 0.9f), "Stalker", 16f);
            }

            Debug.Log($"🌑 Moon12DynamicHazards spawned {hazards.Count} hazards");
        }

        GameObject CreateHazard(string hazardName, Vector3 position, Vector3 scale, Color color, string damageType, float damageAmount)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hazard.name = hazardName;
            hazard.transform.position = position;
            hazard.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.3f);
            mat.SetFloat("_Smoothness", 0.2f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            
            // Very dim dark purple emission
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.1f, 0.05f, 0.15f) * 0.2f);
            
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
