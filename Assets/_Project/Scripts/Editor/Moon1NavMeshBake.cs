#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1NavMeshBake — one-click NavMesh bake + Save Scene + Save Project,
    /// so NATRIX can skip Window→AI→Navigation→Bake clicks.
    ///
    /// Bake uses the legacy NavMeshBuilder.BuildNavMeshAsync() entry point,
    /// which respects the current scene's NavMeshObstacles and bake settings.
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

            Debug.Log("[Moon1NavMeshBake] Starting NavMesh bake...");
            NavMeshBuilder.ClearAllNavMeshes();
            NavMeshBuilder.BuildNavMesh();
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
