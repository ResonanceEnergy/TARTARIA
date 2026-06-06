#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1WireMilo — **LEGACY** standalone Milo placer that instantiates the
    /// Milo prefab with `MiloController` + `MiloFollowBehaviour`.
    ///
    /// **SUPERSEDED by `Moon1BuildOutNPCs.cs`** (menu: `Tartaria/Build Out Moon 1
    /// NPCs (Milo + Anastasia + Lirael + Cassian)`), which places all 4 Moon 1
    /// characters at canonical positions with URP/Lit material override + the
    /// conditional-reveal `NPCConditionalSpawn` for Anastasia.
    ///
    /// Keep this menu for the rare case where you want Milo alone without
    /// touching the rest of the cast (e.g. when debugging the follow loop).
    /// Moved to `Tartaria/Legacy/` so the canonical workflow stays clean.
    /// </summary>
    public static class Moon1WireMilo
    {
        const string MILO_PREFAB_PATH = "Assets/_Project/Prefabs/Characters/Milo.prefab";
        static readonly Vector3 SPAWN_OFFSET = new Vector3(2.5f, 0f, -2f); // slightly behind+right of player spawn

        // SUPERSEDED 2026-05-31 — use Tartaria/1 Build/Build Out Moon 1 NPCs (Milo + Anastasia + Lirael + Cassian)
        // [MenuItem("Tartaria/_ Legacy/Spawn Milo Only", priority = 9010)]
        public static void SpawnMilo()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Spawn Milo", "No active scene. Open Echohaven_VerticalSlice.unity first.", "OK");
                return;
            }

            // Idempotent: re-use existing Milo if there is one
            var existing = UnityEngine.Object.FindFirstObjectByType<MiloController>();
            GameObject miloGO;

            if (existing != null)
            {
                miloGO = existing.gameObject;
                Debug.Log($"[Moon1WireMilo] Found existing Milo: {miloGO.name}");
            }
            else
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MILO_PREFAB_PATH);
                if (prefab == null)
                {
                    EditorUtility.DisplayDialog("Spawn Milo",
                        $"Could not find prefab at {MILO_PREFAB_PATH}", "OK");
                    return;
                }

                // Decide spawn position: near PlayerSpawner if it exists, else origin
                Vector3 spawnPos = SPAWN_OFFSET;
                var playerSpawnerGO = GameObject.Find("PlayerSpawner");
                if (playerSpawnerGO != null)
                {
                    spawnPos = playerSpawnerGO.transform.position + SPAWN_OFFSET;
                }

                miloGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                miloGO.transform.position = spawnPos;
                miloGO.name = "Milo";
                Undo.RegisterCreatedObjectUndo(miloGO, "Spawn Milo");
                Debug.Log($"[Moon1WireMilo] Spawned Milo at {spawnPos}");
            }

            // Ensure MiloController + NavMeshAgent + MiloFollowBehaviour
            if (miloGO.GetComponent<MiloController>() == null)
            {
                miloGO.AddComponent<MiloController>();
                Debug.Log("[Moon1WireMilo] Added MiloController");
            }
            if (miloGO.GetComponent<NavMeshAgent>() == null)
            {
                var agent = miloGO.AddComponent<NavMeshAgent>();
                agent.radius = 0.4f;
                agent.height = 1.8f;
                Debug.Log("[Moon1WireMilo] Added NavMeshAgent");
            }
            if (miloGO.GetComponent<MiloFollowBehaviour>() == null)
            {
                miloGO.AddComponent<MiloFollowBehaviour>();
                Debug.Log("[Moon1WireMilo] Added MiloFollowBehaviour");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorGUIUtility.PingObject(miloGO);
            Selection.activeGameObject = miloGO;
            Debug.Log("[Moon1WireMilo] Done. Bake NavMesh and Play.");
        }
    }
}
#endif
