using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-60)]
    public class Moon5DynamicHazards : MonoBehaviour
    {
        [Header("Moon 5: Ice Hazards")]
        [SerializeField] int iceStormCount = 12;
        [SerializeField] int avalancheZoneCount = 8;
        [SerializeField] int frozenSpikeCount = 20;
        [SerializeField] int frostbiteFieldCount = 14;

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            // Ice Storms - freezing DOT + movement slow
            for (int i = 0; i < iceStormCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    4f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"IceStorm_{i}", pos, new Vector3(7f, 8f, 7f), new Color(0.7f, 0.9f, 1f, 0.3f), "Freezing", 7f);
            }

            // Avalanche Zones - crushing damage
            for (int i = 0; i < avalancheZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"AvalancheZone_{i}", pos, new Vector3(10f, 3f, 10f), new Color(0.9f, 0.95f, 1f, 0.5f), "Crushing", 20f);
            }

            // Frozen Spikes - piercing damage
            for (int i = 0; i < frozenSpikeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    1f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"FrozenSpike_{i}", pos, new Vector3(1.5f, 3f, 1.5f), new Color(0.6f, 0.8f, 0.95f, 0.7f), "Piercing", 10f);
            }

            // Frostbite Fields - gradual freeze + damage
            for (int i = 0; i < frostbiteFieldCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    0.5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"FrostbiteField_{i}", pos, new Vector3(6f, 2f, 6f), new Color(0.5f, 0.7f, 0.9f, 0.4f), "Frostbite", 5f);
            }

            Debug.Log($"❄️ Moon5DynamicHazards spawned {hazards.Count} hazards");
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
            
            if (damageType == "Freezing" || damageType == "Frostbite")
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.3f, 0.5f, 0.7f) * 0.3f);
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
