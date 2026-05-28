using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 Level Builder — The Molten Forge
    /// Volcanic chambers with lava flows and ancient smithing halls
    /// Theme: Fire resistance, forge puzzles, metal crafting
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon6LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 forgeCenter = Vector3.zero;
        [SerializeField] float forgeSize = 90f;

        void Start()
        {
            BuildForge();
        }

        void BuildForge()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 6: THE MOLTEN FORGE — BUILDING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon6_MoltenForge");

            // Central forge chamber
            var forge = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            forge.name = "Central_Forge";
            forge.transform.SetParent(parent.transform);
            forge.transform.position = new Vector3(0f, 5f, 0f);
            forge.transform.localScale = new Vector3(30f, 5f, 30f);

            // Lava pools
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(35f, 0.5f, 0f);
                var pool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pool.name = "Lava_Pool";
                pool.transform.SetParent(parent.transform);
                pool.transform.position = pos;
                pool.transform.localScale = new Vector3(10f, 0.5f, 10f);
            }

            // Anvil pedestals
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f + 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(20f, 2f, 0f);
                var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pedestal.name = "Anvil_Pedestal";
                pedestal.transform.SetParent(parent.transform);
                pedestal.transform.position = pos;
                pedestal.transform.localScale = new Vector3(4f, 4f, 4f);
            }

            // Volcanic pillars
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(45f, 8f, 0f);
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "Volcanic_Pillar";
                pillar.transform.SetParent(parent.transform);
                pillar.transform.position = pos;
                pillar.transform.localScale = new Vector3(3f, 8f, 3f);
            }

            Debug.Log("[Moon6LevelBuilder] ✅ Molten Forge complete!");
            Debug.Log("  • Central forge + 8 lava pools + 4 anvils + 12 pillars");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }
    }
}
