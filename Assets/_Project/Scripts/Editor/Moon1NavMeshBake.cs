#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
// 2026-06-02 Unity 6 API migration per docs/agents/API_CONTRACT.md:
// UnityEditor.AI.NavMeshBuilder is obsolete in Unity 6. The canonical replacement
// is the AI Navigation package's NavMeshSurface component (per-surface baking).
// Until the scene gains NavMeshSurface GameObjects (separate scene-authoring lane),
// we intentionally use the still-functional legacy editor entry points and
// SCOPE-SUPPRESS the warning with file:line + rationale per no-debt rule 4.
using LegacyNavMeshBuilder = UnityEditor.AI.NavMeshBuilder;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1NavMeshBake — one-click NavMesh bake + Save Scene + Save Project,
    /// so NATRIX can skip Window→AI→Navigation→Bake clicks.
    ///
    /// MIGRATION NOTE: Uses legacy UnityEditor.AI.NavMeshBuilder pending scene
    /// migration to NavMeshSurface (AI Navigation package). Tracked in HANDOFFS.md
    /// as "Level → Tools: NavMeshSurface migration". Until that lands, the legacy
    /// API is the only entry point that bakes without requiring per-surface
    /// components — and Unity 6 still SHIPS it (deprecated but functional).
    /// </summary>
    public static class Moon1NavMeshBake
    {
        [MenuItem("Tartaria/6 Scene Tools/Bake NavMesh", priority = 620)]
        public static void Bake()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("NavMesh Bake", "No active scene. Open Echohaven_VerticalSlice.unity first.", "OK");
                return;
            }

            Debug.Log("[Moon1NavMeshBake] Starting NavMesh bake (legacy entry point — see file header for Unity 6 migration plan)...");
#pragma warning disable CS0618 // legacy NavMeshBuilder — see file-header migration plan
            LegacyNavMeshBuilder.ClearAllNavMeshes();
            LegacyNavMeshBuilder.BuildNavMesh();
#pragma warning restore CS0618
            Debug.Log("[Moon1NavMeshBake] NavMesh bake complete.");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.DisplayDialog("NavMesh Bake",
                "NavMesh baked. Save the scene (Ctrl+S or Tartaria → Save Scene) and then Play.",
                "OK");
        }

        [MenuItem("Tartaria/6 Scene Tools/Save Scene", priority = 610)]
        public static void SaveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Save Scene", "No active scene.", "OK");
                return;
            }
            bool ok = EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Moon1NavMeshBake] Save scene '{scene.name}': {(ok ? "OK" : "FAILED")}");
        }

        [MenuItem("Tartaria/0 ★ MASTER/Ready Check (Audit + Bake + Save)", priority = 50)]
        public static void ReadyCheck()
        {
            // Run audit (will fail if Play mode active)
            EchohavenSceneAudit.AuditFromMenu();
            // Bake NavMesh
            Bake();
            // Save
            SaveScene();
            Debug.Log("[Moon1NavMeshBake] Ready check complete. Ready to Play.");
        }
    }
}
#endif
