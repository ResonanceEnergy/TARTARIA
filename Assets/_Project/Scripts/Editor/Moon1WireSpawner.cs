#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1WireSpawner — Editor menu that creates an EchohavenContentSpawner
    /// GameObject in the active scene and assigns the MudGolem prefab via
    /// reflection (private SerializeField). Idempotent.
    /// </summary>
    public static class Moon1WireSpawner
    {
        const string MUD_GOLEM_PREFAB_PATH = "Assets/_Project/Prefabs/Characters/MudGolem.prefab";
        const string MUD_GOLEM_FALLBACK_PATH = "Assets/_Project/Resources/Enemies/MudGolem.prefab";

        [MenuItem("Tartaria/3 Wire/Echohaven Content Spawner", priority = 300)]
        public static void Wire()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Wire Spawner", "No active scene. Open Echohaven_VerticalSlice.unity first.", "OK");
                return;
            }

            // Idempotent: re-use existing spawner if present
            var existing = UnityEngine.Object.FindFirstObjectByType<EchohavenContentSpawner>();
            EchohavenContentSpawner spawner;

            if (existing != null)
            {
                spawner = existing;
                Debug.Log($"[Moon1WireSpawner] Found existing spawner on {existing.gameObject.name}");
            }
            else
            {
                var go = new GameObject("EchohavenContentSpawner");
                spawner = go.AddComponent<EchohavenContentSpawner>();
                Undo.RegisterCreatedObjectUndo(go, "Wire Echohaven Content Spawner");
                Debug.Log("[Moon1WireSpawner] Created EchohavenContentSpawner GameObject");
            }

            // Locate MudGolem prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MUD_GOLEM_PREFAB_PATH);
            if (prefab == null)
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MUD_GOLEM_FALLBACK_PATH);

            if (prefab == null)
            {
                Debug.LogWarning($"[Moon1WireSpawner] No MudGolem prefab found at {MUD_GOLEM_PREFAB_PATH} or {MUD_GOLEM_FALLBACK_PATH}. Spawner will use primitive fallback.");
            }
            else
            {
                // Assign private SerializeField via SerializedObject (avoids reflection brittleness)
                var so = new SerializedObject(spawner);
                var prop = so.FindProperty("mudGolemPrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = prefab;
                    so.ApplyModifiedProperties();
                    Debug.Log($"[Moon1WireSpawner] Assigned MudGolem prefab: {prefab.name}");
                }
                else
                {
                    Debug.LogWarning("[Moon1WireSpawner] Could not find mudGolemPrefab field on EchohavenContentSpawner.");
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorGUIUtility.PingObject(spawner.gameObject);
            Selection.activeGameObject = spawner.gameObject;
            Debug.Log("[Moon1WireSpawner] Done. RS thresholds: 25 → 1 golem, 50 → 2, 75 → 3.");
        }
    }
}
#endif
