using UnityEngine;

namespace Tartaria.Integration
    #pragma warning disable CS0414, CS0219 // Placeholder fields/vars for planned features
{
    /// <summary>
    /// Moon 12 Level Builder — The Umbral Sanctum
    /// Shadow realm with void architecture and negative space
    /// Theme: Shadow manipulation, light/dark balance, hidden paths
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon12LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 sanctumCenter = Vector3.zero;
        [SerializeField] float sanctumSize = 120f;

        void Start()
        {
            BuildSanctum();
        }

        void BuildSanctum()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 12: THE UMBRAL SANCTUM — BUILDING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon12_UmbralSanctum");

            // Central void sphere
            var voidSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            voidSphere.name = "Void_Core";
            voidSphere.transform.SetParent(parent.transform);
            voidSphere.transform.position = new Vector3(0f, 15f, 0f);
            voidSphere.transform.localScale = new Vector3(35f, 35f, 35f);

            // Shadow spires (6) - forming hexagram
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(60f, 0f, 0f);
                var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spire.name = "Shadow_Spire";
                spire.transform.SetParent(parent.transform);
                spire.transform.position = pos;
                spire.transform.localScale = new Vector3(8f, 25f, 8f);

                // Add inverted pyramid cap
                var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cap.name = "Spire_Cap";
                cap.transform.SetParent(spire.transform);
                cap.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                cap.transform.localScale = new Vector3(2f, 1f, 2f);
                cap.transform.Rotate(45f, 0f, 0f);
            }

            // Void bridges (6)
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                float nextAngle = ((i + 1) % 6) * 60f;
                Vector3 start = Quaternion.Euler(0f, angle, 0f) * new Vector3(60f, 20f, 0f);
                Vector3 end = Quaternion.Euler(0f, nextAngle, 0f) * new Vector3(60f, 20f, 0f);
                CreateBridge(parent, start, end);
            }

            // Shadow obelisks (12)
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(Random.Range(30f, 50f), 0f, 0f);
                var obelisk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obelisk.name = "Shadow_Obelisk";
                obelisk.transform.SetParent(parent.transform);
                obelisk.transform.position = pos + Vector3.up * 10f;
                obelisk.transform.localScale = new Vector3(4f, 20f, 4f);
                obelisk.transform.Rotate(0f, Random.Range(0f, 360f), Random.Range(-10f, 10f));
            }

            // Void rifts (floating)
            for (int i = 0; i < 15; i++)
            {
                var rift = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rift.name = "Void_Rift";
                rift.transform.SetParent(parent.transform);
                rift.transform.position = new Vector3(Random.Range(-70f, 70f), Random.Range(10f, 30f), Random.Range(-70f, 70f));
                rift.transform.localScale = new Vector3(Random.Range(0.5f, 2f), Random.Range(8f, 15f), Random.Range(0.5f, 2f));
                rift.transform.Rotate(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                Destroy(rift.GetComponent<Collider>());
            }

            Debug.Log("[Moon12LevelBuilder] ✅ Umbral Sanctum complete!");
            Debug.Log("  • Void core + 6 shadow spires + 6 bridges + 12 obelisks + 15 rifts");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void CreateBridge(GameObject parent, Vector3 start, Vector3 end)
        {
            var bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bridge.name = "Void_Bridge";
            bridge.transform.SetParent(parent.transform);
            bridge.transform.position = (start + end) / 2f;
            float length = Vector3.Distance(start, end);
            bridge.transform.localScale = new Vector3(3f, 0.5f, length);
            bridge.transform.LookAt(end);
        }
    }
}
