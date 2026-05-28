using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Path Generator — Creates dirt/stone paths connecting buildings
    /// Uses medieval hexagon tiles or simple ground plane strips
    /// Paths radiate from village center in golden spiral pattern
    /// </summary>
    [DefaultExecutionOrder(-83)] // After Moon1EnvironmentDecorator (-84)
    public class Moon1PathGenerator : MonoBehaviour
    {
        [Header("Path Configuration")]
        [SerializeField] Vector3 villageCenter = Vector3.zero;
        [SerializeField] float pathWidth = 3f;
        [SerializeField] int pathSegments = 8; // Radiating paths from center

        [Header("Path Materials")]
        [SerializeField] Material pathMaterial;
        [SerializeField] Material dirtMaterial;

        [Header("Medieval Hex Tiles (Optional)")]
        [SerializeField] GameObject[] hexRoadTiles;
        [SerializeField] bool useHexTiles = false;

        const float PHI = 1.618033988749895f;

        void Start()
        {
            GeneratePaths();
        }

        void GeneratePaths()
        {
            Debug.Log("[Moon1PathGenerator] Generating village paths...");

            var pathsParent = new GameObject("Village_Paths");
            pathsParent.transform.position = villageCenter;

            LoadMaterials();

            if (useHexTiles && hexRoadTiles != null && hexRoadTiles.Length > 0)
            {
                GenerateHexTilePaths(pathsParent);
            }
            else
            {
                GenerateSimplePaths(pathsParent);
            }

            // Add central plaza
            CreateCentralPlaza(pathsParent);

            Debug.Log($"[Moon1PathGenerator] Generated {pathSegments} paths + central plaza");
        }

        void LoadMaterials()
        {
            if (pathMaterial == null)
                pathMaterial = Resources.Load<Material>("Materials/PBR/PavingStones150");
            if (dirtMaterial == null)
                dirtMaterial = Resources.Load<Material>("Materials/PBR/Ground037");

            // Fallback
            if (pathMaterial == null)
            {
                pathMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                pathMaterial.color = new Color(0.5f, 0.45f, 0.4f);
            }
        }

        void GenerateSimplePaths(GameObject parent)
        {
            float angleStep = 360f / pathSegments;

            for (int i = 0; i < pathSegments; i++)
            {
                float angle = i * angleStep;
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                
                CreatePathStrip(parent, villageCenter, direction * 60f, pathWidth, $"Path_{i}");
            }
        }

        void CreatePathStrip(GameObject parent, Vector3 start, Vector3 end, float width, string pathName)
        {
            var pathObj = new GameObject(pathName);
            pathObj.transform.SetParent(parent.transform);
            pathObj.transform.position = start;

            // Create path mesh
            var meshFilter = pathObj.AddComponent<MeshFilter>();
            var meshRenderer = pathObj.AddComponent<MeshRenderer>();

            Vector3 direction = (end - start).normalized;
            float length = Vector3.Distance(start, end);
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

            // Simple quad strip
            var vertices = new List<Vector3>
            {
                perpendicular * -width * 0.5f,
                perpendicular * width * 0.5f,
                direction * length + perpendicular * -width * 0.5f,
                direction * length + perpendicular * width * 0.5f
            };

            var triangles = new int[] { 0, 2, 1, 1, 2, 3 };
            var uvs = new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, length / width),
                new Vector2(1f, length / width)
            };

            var mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles,
                uv = uvs
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
            meshRenderer.material = pathMaterial;
        }

        void GenerateHexTilePaths(GameObject parent)
        {
            // TODO: Implement hex tile path generation
            // For now, fall back to simple paths
            Debug.LogWarning("[Moon1PathGenerator] Hex tile paths not yet implemented, using simple paths");
            GenerateSimplePaths(parent);
        }

        void CreateCentralPlaza(GameObject parent)
        {
            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "Central_Plaza";
            plaza.transform.SetParent(parent.transform);
            plaza.transform.position = villageCenter + Vector3.up * 0.05f;
            plaza.transform.localScale = new Vector3(15f, 0.1f, 15f);
            plaza.GetComponent<Renderer>().material = pathMaterial;
            Destroy(plaza.GetComponent<Collider>());

            Debug.Log("[Moon1PathGenerator] Created central plaza (15m diameter)");
        }
    }
}
