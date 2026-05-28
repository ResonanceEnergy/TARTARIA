using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// NavMeshBaker - Runtime navmesh baking for Echohaven.
    /// Phase 3 requirement from REALITY_CHECK.
    /// </summary>
    public class NavMeshBaker : MonoBehaviour
    {
        [Header("Bake Settings")]
        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private bool bakeOnStart = true;
        [SerializeField] private LayerMask walkableLayers;

        void Start()
        {
            if (navMeshSurface == null)
            {
                navMeshSurface = GetComponent<NavMeshSurface>();
                if (navMeshSurface == null)
                {
                    navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
                }
            }

            if (bakeOnStart)
            {
                BakeNavMesh();
            }
        }

        public void BakeNavMesh()
        {
            if (navMeshSurface != null)
            {
                navMeshSurface.BuildNavMesh();
                Debug.Log("[NavMeshBaker] ✅ NavMesh baked successfully!");
            }
        }
    }
}
