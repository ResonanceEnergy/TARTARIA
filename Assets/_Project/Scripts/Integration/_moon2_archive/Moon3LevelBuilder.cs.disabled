using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Level Builder — The Verdant Labyrinth
    /// Overgrown jungle temple with living vines and ancient stone
    /// Theme: Nature reclaiming civilization, maze-like paths, vertical climbing
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon3LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 templeCenter = Vector3.zero;
        [SerializeField] float templeSize = 100f;

        const float PHI = 1.618033988749895f;

        void Start()
        {
            BuildTemple();
        }

        void BuildTemple()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 3: THE VERDANT LABYRINTH — BUILDING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon3_VerdantLabyrinth");

            // Create 4 temple sections
            CreateOuterWalls(parent);
            CreateMazeSection(parent, new Vector3(-40f, 0f, 40f));
            CreateMazeSection(parent, new Vector3(40f, 0f, 40f));
            CreateMazeSection(parent, new Vector3(-40f, 0f, -40f));
            CreateMazeSection(parent, new Vector3(40f, 0f, -40f));
            CreateCentralShrine(parent);
            AddVegetation(parent);

            Debug.Log("[Moon3LevelBuilder] ✅ Verdant Labyrinth complete!");
            Debug.Log($"  • Temple: {templeSize}m × {templeSize}m maze");
            Debug.Log("  • 4 quadrants + central shrine");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void CreateOuterWalls(GameObject parent)
        {
            var walls = new GameObject("Outer_Walls");
            walls.transform.SetParent(parent.transform);

            // 4 outer walls
            CreateWall(walls, new Vector3(0f, 5f, 50f), new Vector3(100f, 10f, 2f));
            CreateWall(walls, new Vector3(0f, 5f, -50f), new Vector3(100f, 10f, 2f));
            CreateWall(walls, new Vector3(50f, 5f, 0f), new Vector3(2f, 10f, 100f));
            CreateWall(walls, new Vector3(-50f, 5f, 0f), new Vector3(2f, 10f, 100f));
        }

        void CreateMazeSection(GameObject parent, Vector3 offset)
        {
            var section = new GameObject($"Maze_Section");
            section.transform.SetParent(parent.transform);
            section.transform.localPosition = offset;

            // Random maze walls
            for (int i = 0; i < 8; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-15f, 15f), 3f, Random.Range(-15f, 15f));
                Vector3 scale = Random.value > 0.5f ? new Vector3(15f, 6f, 1f) : new Vector3(1f, 6f, 15f);
                CreateWall(section, pos, scale);
            }
        }

        void CreateCentralShrine(GameObject parent)
        {
            var shrine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shrine.name = "Central_Shrine";
            shrine.transform.SetParent(parent.transform);
            shrine.transform.localPosition = new Vector3(0f, 8f, 0f);
            shrine.transform.localScale = new Vector3(15f, 8f, 15f);
        }

        void CreateWall(GameObject parent, Vector3 pos, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.SetParent(parent.transform);
            wall.transform.localPosition = pos;
            wall.transform.localScale = scale;
        }

        void AddVegetation(GameObject parent)
        {
            var vegetation = new GameObject("Vegetation");
            vegetation.transform.SetParent(parent.transform);

            // Scatter 50 trees/vines
            for (int i = 0; i < 50; i++)
            {
                var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tree.name = "Tree";
                tree.transform.SetParent(vegetation.transform);
                tree.transform.position = new Vector3(Random.Range(-45f, 45f), 4f, Random.Range(-45f, 45f));
                tree.transform.localScale = new Vector3(2f, 8f, 2f);
                Destroy(tree.GetComponent<Collider>());
            }
        }
    }
}
