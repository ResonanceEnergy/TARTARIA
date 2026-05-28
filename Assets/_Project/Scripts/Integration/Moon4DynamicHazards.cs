using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    [DefaultExecutionOrder(-60)]
    public class Moon4DynamicHazards : MonoBehaviour
    {
        [Header("Moon 4: Desert Hazards")]
        [SerializeField] int sandstormZoneCount = 10;
        [SerializeField] int quicksandPitCount = 14;
        [SerializeField] int scorchingVentCount = 16;
        [SerializeField] int mirageTrapCount = 8;

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            // Sandstorm Zones - vision obscure + DOT
            for (int i = 0; i < sandstormZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    3f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"SandstormZone_{i}", pos, new Vector3(8f, 6f, 8f), new Color(0.8f, 0.7f, 0.4f, 0.4f), "Sandstorm", 6f);
            }

            // Quicksand Pits - movement trap + drowning damage
            for (int i = 0; i < quicksandPitCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    -0.5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"QuicksandPit_{i}", pos, new Vector3(5f, 1f, 5f), new Color(0.7f, 0.6f, 0.3f, 0.6f), "Quicksand", 8f);
            }

            // Scorching Vents - burst fire damage
            for (int i = 0; i < scorchingVentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    0.5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"ScorchingVent_{i}", pos, new Vector3(3f, 2f, 3f), new Color(1f, 0.5f, 0.1f, 0.5f), "Heat", 12f);
            }

            // Mirage Traps - disorientation + teleport
            for (int i = 0; i < mirageTrapCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    1f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"MirageTrap_{i}", pos, new Vector3(6f, 4f, 6f), new Color(0.5f, 0.7f, 1f, 0.2f), "Mirage", 0f);
            }

            Debug.Log($"🏜️ Moon4DynamicHazards spawned {hazards.Count} hazards");
        }

        GameObject CreateHazard(string hazardName, Vector3 position, Vector3 scale, Color color, string damageType, float damageAmount)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hazard.name = hazardName;
            hazard.transform.position = position;
            hazard.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Smoothness", 0.3f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            
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
