using UnityEngine;
using UnityEditor;
using Unity.AI.Navigation;
using UnityEngine.AI;

namespace Tartaria.Editor
{
    /// <summary>
    /// NavMesh Baker — ensures NavMesh is baked for AI navigation.
    /// Adds NavMeshSurface component to scene root if missing, then builds.
    /// Invoked as Phase 9j21 in OneClickBuild pipeline.
    /// </summary>
    public static class NavMeshBaker
    {
        [MenuItem("TARTARIA/Build/Bake NavMesh")]
        public static void BakeNavMesh()
        {
            Debug.Log("[NavMeshBaker] Baking NavMesh for Echohaven...");

            // Find or create NavMeshSurface on root
            var root = GameObject.Find("EchohavenTerrain");
            if (root == null)
            {
                Debug.LogWarning("[NavMeshBaker] EchohavenTerrain root not found — creating.");
                root = new GameObject("EchohavenTerrain");
            }

            var surface = root.GetComponent<NavMeshSurface>();
            if (surface == null)
            {
                surface = root.AddComponent<NavMeshSurface>();
                surface.collectObjects = CollectObjects.All;
                surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                surface.layerMask = ~0; // All layers
                Debug.Log("[NavMeshBaker] NavMeshSurface component added.");
            }

            // Build
            surface.BuildNavMesh();
            Debug.Log("[NavMeshBaker] ✓ NavMesh baked successfully.");

            // Verify
            var bounds = surface.navMeshData.sourceBounds;
            int triCount = NavMesh.CalculateTriangulation().vertices.Length / 3;
            Debug.Log($"[NavMeshBaker] NavMesh bounds: {bounds}, triangles: {triCount}");
        }

        /// <summary>
        /// Headless entry point for OneClickBuild pipeline (Phase 9j21).
        /// </summary>
        public static void BakeNavMeshHeadless()
        {
            if (!Application.isBatchMode)
            {
                Debug.LogWarning("[NavMeshBaker] BakeNavMeshHeadless called in Editor mode — use BakeNavMesh instead.");
            }
            BakeNavMesh();
        }
    }
}
