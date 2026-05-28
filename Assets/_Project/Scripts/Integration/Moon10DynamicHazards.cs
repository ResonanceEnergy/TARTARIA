using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-60)]
    public class Moon10DynamicHazards : MonoBehaviour
    {
        [Header("Moon 10: Time Hazards")]
        [SerializeField] int timeAnomalyCount = 12;
        [SerializeField] int temporalLoopCount = 8;
        [SerializeField] int chronoDisplacementCount = 14;
        [SerializeField] int realityTearCount = 10;

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            // Time Anomalies - temporal disruption
            for (int i = 0; i < timeAnomalyCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    3f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"TimeAnomaly_{i}", pos, new Vector3(5f, 5f, 5f), new Color(0.5f, 0.6f, 0.7f, 0.4f), "Temporal", 10f);
            }

            // Temporal Loops - time slow + damage
            for (int i = 0; i < temporalLoopCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    1f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"TemporalLoop_{i}", pos, new Vector3(7f, 4f, 7f), new Color(0.4f, 0.5f, 0.6f, 0.5f), "TimeLoop", 8f);
            }

            // Chrono Displacement - teleport + confusion
            for (int i = 0; i < chronoDisplacementCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    2f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"ChronoDisplacement_{i}", pos, new Vector3(4f, 6f, 4f), new Color(0.6f, 0.7f, 0.8f, 0.3f), "Chrono", 12f);
            }

            // Reality Tears - existence damage
            for (int i = 0; i < realityTearCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    4f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"RealityTear_{i}", pos, new Vector3(3f, 8f, 3f), new Color(0.3f, 0.4f, 0.5f, 0.6f), "Reality", 16f);
            }

            Debug.Log($"⏰ Moon10DynamicHazards spawned {hazards.Count} hazards");
        }

        GameObject CreateHazard(string hazardName, Vector3 position, Vector3 scale, Color color, string damageType, float damageAmount)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hazard.name = hazardName;
            hazard.transform.position = position;
            hazard.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.7f);
            mat.SetFloat("_Smoothness", 0.8f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            
            // Gray-blue emission for time effects
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.4f, 0.5f, 0.6f) * 0.5f);
            
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
