using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 Level Builder — The Abyssal Depths
    /// Underwater temple with pressure chambers and bioluminescent coral
    /// Theme: Swimming mechanics, water pressure, ancient marine life
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon7LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 templeCenter = Vector3.zero;
        [SerializeField] float templeSize = 100f;

        void Start()
        {
            BuildTemple();
        }

        void BuildTemple()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 7: THE ABYSSAL DEPTHS — BUILDING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon7_AbyssalDepths");

            // Main temple dome
            var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "Temple_Dome";
            dome.transform.SetParent(parent.transform);
            dome.transform.position = new Vector3(0f, -20f, 0f);
            dome.transform.localScale = new Vector3(60f, 60f, 60f);

            // 5 Pressure chambers (vertical stack)
            for (int i = 0; i < 5; i++)
            {
                var chamber = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                chamber.name = $"Pressure_Chamber_{i + 1}";
                chamber.transform.SetParent(parent.transform);
                chamber.transform.position = new Vector3(0f, -40f - (i * 15f), 0f);
                chamber.transform.localScale = new Vector3(25f, 7f, 25f);
            }

            // Coral formations
            for (int i = 0; i < 30; i++)
            {
                var coral = GameObject.CreatePrimitive(PrimitiveType.Cone);
                coral.name = "Coral";
                coral.transform.SetParent(parent.transform);
                coral.transform.position = new Vector3(Random.Range(-40f, 40f), Random.Range(-50f, 0f), Random.Range(-40f, 40f));
                coral.transform.localScale = new Vector3(Random.Range(2f, 5f), Random.Range(3f, 8f), Random.Range(2f, 5f));
                Destroy(coral.GetComponent<Collider>());
            }

            // Pillar supports
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(30f, -30f, 0f);
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "Support_Pillar";
                pillar.transform.SetParent(parent.transform);
                pillar.transform.position = pos;
                pillar.transform.localScale = new Vector3(4f, 30f, 4f);
            }

            Debug.Log("[Moon7LevelBuilder] ✅ Abyssal Depths complete!");
            Debug.Log("  • Temple dome + 5 pressure chambers + 30 coral + 8 pillars");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }
    }
}
