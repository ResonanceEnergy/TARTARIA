using UnityEngine;

namespace Tartaria.Integration
    #pragma warning disable CS0414, CS0219 // Placeholder fields/vars for planned features
{
    /// <summary>
    /// Moon 9 Level Builder — The Blighted Wastes
    /// Corrupted landscape with twisted architecture and dark energy
    /// Theme: Corruption cleansing, enemy stronghold, dark aether
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon9LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 wastelandCenter = Vector3.zero;
        [SerializeField] float wastelandSize = 140f;

        void Start()
        {
            BuildWasteland();
        }

        void BuildWasteland()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 9: THE BLIGHTED WASTES — BUILDING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon9_BlightedWastes");

            // Central corruption nexus
            var nexus = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nexus.name = "Corruption_Nexus";
            nexus.transform.SetParent(parent.transform);
            nexus.transform.position = new Vector3(0f, 10f, 0f);
            nexus.transform.localScale = new Vector3(25f, 25f, 25f);

            // Twisted spires (5)
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(50f, 0f, 0f);
                var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spire.name = "Twisted_Spire";
                spire.transform.SetParent(parent.transform);
                spire.transform.position = pos;
                spire.transform.localScale = new Vector3(6f, 20f, 6f);
                spire.transform.Rotate(Random.Range(-15f, 15f), 0f, Random.Range(-15f, 15f)); // Tilt
            }

            // Corrupted monoliths (12)
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(Random.Range(30f, 70f), 0f, 0f);
                var monolith = GameObject.CreatePrimitive(PrimitiveType.Cube);
                monolith.name = "Corrupted_Monolith";
                monolith.transform.SetParent(parent.transform);
                monolith.transform.position = pos + Vector3.up * 8f;
                monolith.transform.localScale = new Vector3(3f, 16f, 3f);
                monolith.transform.Rotate(0f, Random.Range(0f, 360f), Random.Range(-10f, 10f));
            }

            // Blight craters
            for (int i = 0; i < 20; i++)
            {
                var crater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crater.name = "Blight_Crater";
                crater.transform.SetParent(parent.transform);
                crater.transform.position = new Vector3(Random.Range(-60f, 60f), -1f, Random.Range(-60f, 60f));
                crater.transform.localScale = new Vector3(Random.Range(8f, 15f), 1f, Random.Range(8f, 15f));
                Destroy(crater.GetComponent<Collider>());
            }

            Debug.Log("[Moon9LevelBuilder] ✅ Blighted Wastes complete!");
            Debug.Log("  • Corruption nexus + 5 twisted spires + 12 monoliths + 20 craters");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }
    }
}
