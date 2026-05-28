using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 1 Excavation Sites — Creates 5 excavation points around village buildings
    /// Each site has a mud mound visual and requires scanning to reveal the building
    /// Integrates with ExcavationSystem for gameplay mechanics
    /// </summary>
    [DefaultExecutionOrder(-79)] // After ambient audio (-80)
    public class Moon1ExcavationSites : MonoBehaviour
    {
        [Header("Excavation Configuration")]
        [SerializeField] int siteCount = 5;
        [SerializeField] float siteRadius = 70f;
        [SerializeField] GameObject mudMoundPrefab;

        [Header("Materials")]
        [SerializeField] Material mudMaterial;
        [SerializeField] Material dirtMaterial;

        void Start()
        {
            CreateExcavationSites();
        }

        void CreateExcavationSites()
        {
            Debug.Log("[Moon1ExcavationSites] Creating excavation sites...");

            LoadMaterials();

            var sitesParent = new GameObject("Excavation_Sites");
            sitesParent.transform.position = Vector3.zero;

            // Create sites in circular pattern
            float angleStep = 360f / siteCount;
            for (int i = 0; i < siteCount; i++)
            {
                float angle = i * angleStep;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(siteRadius, 0f, 0f);
                Vector3 position = Vector3.zero + offset;

                CreateExcavationSite(sitesParent, position, i);
            }

            Debug.Log($"[Moon1ExcavationSites] ✅ Created {siteCount} excavation sites");
        }

        void LoadMaterials()
        {
            if (mudMaterial == null)
                mudMaterial = Resources.Load<Material>("Materials/M_Mud_Fresh");
            if (dirtMaterial == null)
                dirtMaterial = Resources.Load<Material>("Materials/PBR/Ground037");

            // Fallback
            if (mudMaterial == null)
            {
                mudMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mudMaterial.color = new Color(0.4f, 0.3f, 0.2f);
            }
        }

        void CreateExcavationSite(GameObject parent, Vector3 position, int index)
        {
            var site = new GameObject($"Excavation_Site_{index + 1}");
            site.transform.SetParent(parent.transform);
            site.transform.position = position;

            // Mud mound visual
            var mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mound.name = "Mud_Mound";
            mound.transform.SetParent(site.transform);
            mound.transform.localPosition = Vector3.zero;
            mound.transform.localScale = new Vector3(6f, 3f, 6f);
            mound.GetComponent<Renderer>().material = mudMaterial;

            // Dirt base
            var base_ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            base_ground.name = "Dirt_Base";
            base_ground.transform.SetParent(site.transform);
            base_ground.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            base_ground.transform.localScale = new Vector3(8f, 0.1f, 8f);
            base_ground.GetComponent<Renderer>().material = dirtMaterial;
            Destroy(base_ground.GetComponent<Collider>());

            // Add interaction collider
            var collider = site.AddComponent<SphereCollider>();
            collider.isTrigger = false;
            collider.radius = 4f;
            collider.center = Vector3.up * 1.5f;

            // Add excavation site component
            var excavationSite = site.AddComponent<ExcavationSiteMarker>();
            excavationSite.siteId = $"moon1_site_{index + 1}";
            excavationSite.requiresScan = true;
            excavationSite.mudMoundVisual = mound;

            Debug.Log($"  ✓ Site {index + 1} at {position}");
        }
    }

    /// <summary>
    /// Excavation Site Marker — Marks a location as scannable/excavatable
    /// </summary>
    public class ExcavationSiteMarker : MonoBehaviour
    {
        public string siteId;
        public bool requiresScan = true;
        public GameObject mudMoundVisual;
        private bool isExcavated = false;

        public void MarkAsExcavated()
        {
            if (isExcavated) return;

            isExcavated = true;

            // Hide mud mound
            if (mudMoundVisual != null)
            {
                mudMoundVisual.SetActive(false);
            }

            Debug.Log($"[ExcavationSiteMarker] {siteId} excavated!");

            // Notify ExcavationSystem
            if (ExcavationSystem.Instance != null)
            {
                // DISABLED: ExcavationSystem.Instance.RegisterExcavation(siteId);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = isExcavated ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 4f);
        }
    }
}
