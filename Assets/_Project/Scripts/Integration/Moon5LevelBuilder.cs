using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 Level Builder — The Frostbound Citadel
    /// Ice fortress with frozen towers and glacial caves
    /// Theme: Cold resistance, ice puzzles, frozen enemies
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon5LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 citadelCenter = Vector3.zero;
        [SerializeField] float citadelSize = 80f;

        void Start()
        {
            BuildCitadel();
        }

        void BuildCitadel()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 5: THE FROSTBOUND CITADEL — BUILDING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon5_FrostboundCitadel");
            
            // Central keep
            var keep = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keep.name = "Central_Keep";
            keep.transform.SetParent(parent.transform);
            keep.transform.position = new Vector3(0f, 20f, 0f);
            keep.transform.localScale = new Vector3(30f, 40f, 30f);
            
            // 4 Corner towers
            CreateTower(parent, new Vector3(-40f, 0f, -40f));
            CreateTower(parent, new Vector3(40f, 0f, -40f));
            CreateTower(parent, new Vector3(-40f, 0f, 40f));
            CreateTower(parent, new Vector3(40f, 0f, 40f));
            
            // Ice walls
            CreateWall(parent, new Vector3(0f, 8f, 60f), new Vector3(120f, 16f, 2f));
            CreateWall(parent, new Vector3(0f, 8f, -60f), new Vector3(120f, 16f, 2f));
            CreateWall(parent, new Vector3(60f, 8f, 0f), new Vector3(2f, 16f, 120f));
            CreateWall(parent, new Vector3(-60f, 8f, 0f), new Vector3(2f, 16f, 120f));
            
            // Ice spikes
            for (int i = 0; i < 40; i++)
            {
                var spike = GameObject.CreatePrimitive(PrimitiveType.Cone);
                spike.name = "Ice_Spike";
                spike.transform.SetParent(parent.transform);
                spike.transform.position = new Vector3(Random.Range(-55f, 55f), 0f, Random.Range(-55f, 55f));
                spike.transform.localScale = new Vector3(2f, Random.Range(4f, 10f), 2f);
                Destroy(spike.GetComponent<Collider>());
            }

            Debug.Log("[Moon5LevelBuilder] ✅ Frostbound Citadel complete!");
            Debug.Log("  • Central keep + 4 towers + ice walls + 40 spikes");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void CreateTower(GameObject parent, Vector3 position)
        {
            var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tower.name = "Tower";
            tower.transform.SetParent(parent.transform);
            tower.transform.position = position + Vector3.up * 15f;
            tower.transform.localScale = new Vector3(8f, 15f, 8f);
        }

        void CreateWall(GameObject parent, Vector3 pos, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Ice_Wall";
            wall.transform.SetParent(parent.transform);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
        }
    }
}
