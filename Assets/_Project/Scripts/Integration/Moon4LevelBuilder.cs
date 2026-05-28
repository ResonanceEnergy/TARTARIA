using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 Level Builder — The Sunscorched Oasis
    /// Desert ruins with sandstone temples and hidden water sources
    /// Theme: Survival, heat mirages, ancient solar technology
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon4LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 oasisCenter = Vector3.zero;
        [SerializeField] float desertRadius = 150f;

        void Start()
        {
            BuildOasis();
        }

        void BuildOasis()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 4: THE SUNSCORCHED OASIS — BUILDING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon4_SunscorchedOasis");
            
            // Central oasis
            var oasis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            oasis.name = "Oasis_Pool";
            oasis.transform.SetParent(parent.transform);
            oasis.transform.position = new Vector3(0f, 0.1f, 0f);
            oasis.transform.localScale = new Vector3(20f, 0.1f, 20f);
            
            // 3 Desert temples
            CreateTemple(parent, new Vector3(-50f, 0f, -50f), "North Temple");
            CreateTemple(parent, new Vector3(50f, 0f, -50f), "East Temple");
            CreateTemple(parent, new Vector3(0f, 0f, 50f), "South Temple");
            
            // Sand dunes
            for (int i = 0; i < 30; i++)
            {
                var dune = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dune.name = "Dune";
                dune.transform.SetParent(parent.transform);
                dune.transform.position = new Vector3(Random.Range(-desertRadius, desertRadius), -2f, Random.Range(-desertRadius, desertRadius));
                dune.transform.localScale = new Vector3(Random.Range(15f, 30f), Random.Range(5f, 10f), Random.Range(15f, 30f));
                Destroy(dune.GetComponent<Collider>());
            }

            Debug.Log("[Moon4LevelBuilder] ✅ Sunscorched Oasis complete!");
            Debug.Log("  • Oasis pool + 3 desert temples + 30 dunes");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void CreateTemple(GameObject parent, Vector3 position, string name)
        {
            var temple = GameObject.CreatePrimitive(PrimitiveType.Cube);
            temple.name = name;
            temple.transform.SetParent(parent.transform);
            temple.transform.position = position;
            temple.transform.localScale = new Vector3(25f, 15f, 25f);
        }
    }
}
