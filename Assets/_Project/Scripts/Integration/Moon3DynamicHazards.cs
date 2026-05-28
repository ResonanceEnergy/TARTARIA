using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    [DefaultExecutionOrder(-60)]
    public class Moon3DynamicHazards : MonoBehaviour
    {
        [Header("Moon 3: Jungle Hazards")]
        [SerializeField] int poisonPlantCount = 18;
        [SerializeField] int carnivorousVineCount = 12;
        [SerializeField] int thornyPatchCount = 20;
        [SerializeField] int toxicSporeCount = 15;

        List<GameObject> hazards = new List<GameObject>();

        void Start()
        {
            SpawnHazards();
        }

        void SpawnHazards()
        {
            // Poison Plants - slow damage over time
            for (int i = 0; i < poisonPlantCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    0.5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"PoisonPlant_{i}", pos, new Vector3(2f, 1f, 2f), new Color(0.3f, 0.7f, 0.2f), "Poison", 5f);
            }

            // Carnivorous Vines - high instant damage
            for (int i = 0; i < carnivorousVineCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    1.5f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"CarnivorousVine_{i}", pos, new Vector3(3f, 3f, 3f), new Color(0.5f, 0.15f, 0.05f), "Physical", 15f);
            }

            // Thorny Patches - movement slow + damage
            for (int i = 0; i < thornyPatchCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    0.3f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"ThornyPatch_{i}", pos, new Vector3(4f, 0.5f, 4f), new Color(0.4f, 0.25f, 0.1f), "Thorn", 3f);
            }

            // Toxic Spores - vision obscure + DOT
            for (int i = 0; i < toxicSporeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    2f,
                    Random.Range(-80f, 80f)
                );
                CreateHazard($"ToxicSpore_{i}", pos, new Vector3(5f, 4f, 5f), new Color(0.6f, 0.8f, 0.2f, 0.3f), "Toxin", 4f);
            }

            Debug.Log($"🌿 Moon3DynamicHazards spawned {hazards.Count} hazards: {poisonPlantCount} poison plants, {carnivorousVineCount} vines, {thornyPatchCount} thorny patches, {toxicSporeCount} toxic spores");
        }

        GameObject CreateHazard(string hazardName, Vector3 position, Vector3 scale, Color color, string damageType, float damageAmount)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hazard.name = hazardName;
            hazard.transform.position = position;
            hazard.transform.localScale = scale;

            // Create semi-transparent danger material
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Smoothness", 0.3f);
            
            // Enable transparency
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0); // Alpha blend
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            
            hazard.GetComponent<Renderer>().material = mat;

            // Replace collider with trigger box
            Destroy(hazard.GetComponent<Collider>());
            BoxCollider trigger = hazard.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            // Add hazard zone component
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

    // Hazard damage component
    public class HazardZone : MonoBehaviour
    {
        public float damageAmount = 5f;
        public string damageType = "Environmental";
        public float effectDuration = 1f;
        
        float lastDamageTime = 0f;

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && Time.time - lastDamageTime >= effectDuration)
            {
                // Apply damage to player (placeholder - actual damage system integration needed)
                Debug.Log($"⚠️ Player hit by {damageType} hazard for {damageAmount} damage!");
                lastDamageTime = Time.time;
                
                // TODO: Integrate with actual player health system when implemented
                // PlayerHealth health = other.GetComponent<PlayerHealth>();
                // if (health != null) health.TakeDamage(damageAmount, damageType);
            }
        }
    }
}
