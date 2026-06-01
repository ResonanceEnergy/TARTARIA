#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1MasterBootstrap — wires the Moon1*.cs Integration systems
    /// into the scene as a single "Moon1_Systems" GameObject.
    ///
    /// 2026-05-31 cleanup pass per docs/audits/MOON1_BUILD_AUDIT_2026-05-31.md:
    /// REMOVED 6 conflicting / stub components from auto-attach:
    ///   - Moon1HeroBuildingSpawner   → Moon1BuildOutBuildings menu is canonical
    ///   - Moon1LevelBuilder          → Moon1BuildOutVillage menu is canonical
    ///   - Moon1MaterialSetup         → 31-line TODO stub (CLAUDE.md rule#1)
    ///   - Moon1AmbientCreatures      → 31-line TODO stub (CLAUDE.md rule#1)
    ///   - Moon1NPCSpawner            → Moon1BuildOutNPCs menu is canonical
    ///   - Moon1BuildingPrefabCreator → Editor-only asset-gen, no scene attach needed
    /// 2026-05-31 Agent 3 cleanup: Moon1PostProcessing also removed (duplicate of Moon1PostProcessingPreset).
    ///
    /// Idempotent — re-runs reuse the Moon1_Systems GameObject and skip components
    /// that are already present.
    /// </summary>
    public static class Moon1MasterBootstrap
    {
        [MenuItem("Tartaria/0 ★ MASTER/Bootstrap All Moon 1 Systems", priority = 30)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Bootstrap", "No active scene.", "OK");
                return;
            }

            var parent = GameObject.Find("Moon1_Systems");
            if (parent == null)
            {
                parent = new GameObject("Moon1_Systems");
                Undo.RegisterCreatedObjectUndo(parent, "Create Moon1_Systems");
            }

            int added = 0;
            int reused = 0;

            // Core systems (run in DefaultExecutionOrder, lowest number first)
            added += AddIfMissing<Moon1QuestTriggers>(parent, ref reused);          // -81
            added += AddIfMissing<Moon1ExcavationSites>(parent, ref reused);        // -79
            added += AddIfMissing<Moon1PlayerSetup>(parent, ref reused);            // -78
            // Moon1PostProcessing REMOVED 2026-05-31 — duplicate of Moon1PostProcessingPreset bootstrap.
            added += AddIfMissing<Moon1LightingSetup>(parent, ref reused);          // -75

            // 2026-05-30 second-wave components built from docs/03 Moon 1 spec:
            added += AddIfMissing<TartarianHourCycle>(parent, ref reused);          // 17-hour day, drives lighting + fires OnSeventeenthHour
            added += AddIfMissing<Moon1NarrativeBeats>(parent, ref reused);          // Cathedral eruption + skeleton hum prophecy + Giant skeleton key #1
            added += AddIfMissing<Moon1DialogueBindings>(parent, ref reused);        // Wires 3 yarn files to in-game events

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = parent;

            string summary =
                $"Moon1_Systems bootstrapped.\n" +
                $"  Components added this run: {added}\n" +
                $"  Components already present: {reused}\n\n" +
                "Pruned per audit: 7 stub/conflicting auto-attaches removed.\n" +
                "Run the 'Build Out Moon 1 *' menus + 'Place Blender Prefabs' + 'Place New Assets' for the canonical scene build.";
            EditorUtility.DisplayDialog("Bootstrap All Moon 1 Systems",
                summary + "\n\nNext: Tartaria → Ready Check (Audit + Bake + Save), then Play.",
                "OK");
        }

        static int AddIfMissing<T>(GameObject host, ref int reused) where T : Component
        {
            if (host.GetComponent<T>() != null) { reused++; return 0; }
            Undo.AddComponent<T>(host);
            Debug.Log($"[Moon1MasterBootstrap] + {typeof(T).Name}");
            return 1;
        }
    }
}
#endif
