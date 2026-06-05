using UnityEngine;
using Unity.AI.Navigation;

namespace Tartaria.Integration
{
    /// <summary>
    /// NavMeshBaker - Runtime navmesh baking for Echohaven.
    /// Wires one or more NavMeshSurface components and rebuilds them on demand.
    /// Phase 3 requirement from REALITY_CHECK.
    /// </summary>
    public class NavMeshBaker : MonoBehaviour
    {
        [Header("Bake Settings")]
        [Tooltip("Surfaces to bake. If empty, will auto-discover NavMeshSurface components on this GameObject and its children.")]
        [SerializeField] private NavMeshSurface[] surfaces;
        [SerializeField] private bool bakeOnStart = true;

        void Awake()
        {
            if (surfaces == null || surfaces.Length == 0)
            {
                surfaces = GetComponentsInChildren<NavMeshSurface>(includeInactive: true);
                if (surfaces == null || surfaces.Length == 0)
                {
                    var local = GetComponent<NavMeshSurface>();
                    if (local == null)
                    {
                        local = gameObject.AddComponent<NavMeshSurface>();
                    }
                    surfaces = new[] { local };
                }
            }
        }

        void Start()
        {
            if (bakeOnStart)
            {
                BakeAll();
            }
        }

        /// <summary>
        /// Rebuilds every assigned NavMeshSurface. Returns the count of surfaces baked.
        /// </summary>
        public int BakeAll()
        {
            int baked = 0;
            if (surfaces == null) return 0;
            for (int i = 0; i < surfaces.Length; i++)
            {
                var surface = surfaces[i];
                if (surface == null) continue;
                surface.BuildNavMesh();
                baked++;
            }
            Debug.Log($"[NavMeshBaker] Baked {baked} NavMeshSurface(s).");
            return baked;
        }

        /// <summary>
        /// Legacy alias preserved for OneClickBuild and any other callers expecting BakeNavMesh().
        /// </summary>
        public void BakeNavMesh() => BakeAll();
    }
}
