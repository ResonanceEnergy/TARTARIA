using UnityEditor;
using UnityEngine;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Custom Scene view gizmo drawing for Tartaria-specific objects:
    ///   - Discovery trigger radii (golden wireframe)
    ///   - Aether node fields (blue wireframe)
    ///   - Enemy spawn radii (red wireframe)
    ///   - Building restoration state labels
    /// </summary>
    public static class TartariaSceneGizmos
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawDiscoveryRadius(SphereCollider collider, GizmoType type)
        {
            if (!collider.isTrigger) return;
            if (!collider.gameObject.name.Contains("Discovery")) return;

            Gizmos.color = new Color(0.9f, 0.75f, 0.2f, 0.15f);
            Gizmos.DrawWireSphere(collider.transform.position, collider.radius);

            var style = new GUIStyle();
            style.normal.textColor = new Color(0.9f, 0.75f, 0.2f);
            style.fontSize = 10;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(collider.transform.position + Vector3.up * 2f,
                $"Discovery: {collider.radius:F0}m", style);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawEnemySpawnRadius(Transform transform, GizmoType type)
        {
            if (!transform.gameObject.name.Contains("GolemSpawn")) return;

            Gizmos.color = new Color(0.8f, 0.2f, 0.1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 5f);

            var style = new GUIStyle();
            style.normal.textColor = new Color(0.8f, 0.2f, 0.1f);
            style.fontSize = 10;
            style.alignment = TextAnchor.MiddleCenter;

            string label = transform.gameObject.name.Replace("GolemSpawn_", "Spawn @ ");
            Handles.Label(transform.position + Vector3.up * 3f, label, style);
        }
    }
}
