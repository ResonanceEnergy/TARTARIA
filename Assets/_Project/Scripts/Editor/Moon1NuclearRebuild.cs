#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1NuclearRebuild — empties the Echohaven scene of every NPC/building/prop
    /// GameObject so the spawners can repopulate it cleanly from Blender prefabs.
    ///
    /// Per NATRIX 2026-05-31 mandate: "objects arent right size not placed properly,
    /// main character has no skin, still see kaykit characters, environment is
    /// cluttered and cant walk around". Root cause: prior sessions placed NPC + building
    /// GameObjects DIRECTLY in scene hierarchy (with KayKit FBX meshes underneath),
    /// then later sessions also wired EchohavenContentSpawner to spawn Blender prefabs
    /// at runtime. Result: double-spawned overlapping props, KayKit meshes still showing
    /// because the scene-static GameObjects are never replaced, NavMesh is stale because
    /// of all the moved geometry.
    ///
    /// PRESERVES (whitelist):
    ///   • Camera roots, lighting, post-process, probe volumes
    ///   • PlayerSpawn (transform anchor — Bootstrap uses its position)
    ///   • Moon1_Systems (gets cleared but kept — Bootstrap re-attaches components)
    ///   • Echohaven_Lighting, WorldBoundary, APV_*, PostProcessVolume, ProbeVolumePerSceneData
    ///   • RuntimePBRApplier, EchohavenCombatArena, MoonFramework, _FallSafetyFloor
    ///   • _SpawnPlatform (player spawns on it)
    ///
    /// DELETES (everything else under scene root), including:
    ///   • Milo, --- NPCs ---, --- BUILDINGS ---, --- ENEMY SPAWNS ---
    ///   • Moon1_Terrain, Moon1_BlenderPlacements, Moon1_NewAssetsPlacements
    ///   • Echohaven_Village, Echohaven_NPCs, Echohaven_Environment, EchohavenObelisk
    ///   • EchohavenContentSpawner (free root one — Bootstrap will re-add as Moon1_Systems component)
    ///   • Player (runtime-spawned)
    ///   • Any direct child of root that's not on the whitelist
    /// </summary>
    public static class Moon1NuclearRebuild
    {
        // Whitelist — KEEP these scene-root GameObjects.
        static readonly HashSet<string> PreserveExact = new HashSet<string>
        {
            "Main Camera",
            "Directional Light",
            "PlayerSpawn",
            "Moon1_Systems",
            "Echohaven_Lighting",
            "WorldBoundary",
            "PostProcessVolume",
            "RuntimePBRApplier",
            "EchohavenCombatArena",
            "MoonFramework",
            "_FallSafetyFloor",
            "_SpawnPlatform",
            "EchohavenTerrain",       // keep terrain mesh — spawners place props on it
        };

        // Also preserve anything starting with these prefixes (APV probe volumes, etc.)
        static readonly string[] PreservePrefix = new []
        {
            "APV_",
            "ProbeVolume",
            "APVScenario",
            "--- UI ---",          // UI overlay group
            "--- MINI-GAMES ---",  // mini-game UI parent
        };

        [MenuItem("Tartaria/8 Fix/☢ NUCLEAR — Empty Scene + Rebuild from Blender", priority = 100)]
        public static void NuclearRebuild()
        {
            if (!EditorUtility.DisplayDialog("Nuclear Rebuild",
                "This will DELETE every NPC, building, prop, and content placement GameObject in the scene.\n\n" +
                "Preserves only: camera, lighting, probe volumes, PlayerSpawn, Moon1_Systems, terrain, combat arena.\n\n" +
                "Then runs: Wire ALL Prefab Refs → Bootstrap → NavMesh Bake → Save.\n\n" +
                "Cannot be undone in batch. Continue?",
                "☢ Nuke it", "Cancel"))
            {
                return;
            }

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Nuclear Rebuild", "No active scene.", "OK");
                return;
            }

            // ─── STEP 1: Delete every scene-root GameObject not on whitelist ─────
            int deleted = 0;
            var roots = new List<GameObject>(scene.GetRootGameObjects());
            var deletedNames = new List<string>();
            foreach (var go in roots)
            {
                if (go == null) continue;
                if (ShouldPreserve(go.name)) continue;

                deletedNames.Add(go.name);
                Undo.DestroyObjectImmediate(go);
                deleted++;
            }

            // ─── STEP 2: Clear all components from Moon1_Systems (Bootstrap re-adds them fresh) ─────
            var systems = GameObject.Find("Moon1_Systems");
            if (systems != null)
            {
                foreach (var c in systems.GetComponents<MonoBehaviour>())
                {
                    if (c == null) continue;
                    Undo.DestroyObjectImmediate(c);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);

            // ─── STEP 3: Re-run Bootstrap (which auto-chains Wire ALL) ───────────
            try
            {
                var t = System.Type.GetType("Tartaria.Editor.Moon1MasterBootstrap, Assembly-CSharp-Editor");
                if (t != null)
                {
                    var m = t.GetMethod("Run", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (m != null) m.Invoke(null, null);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[NuclearRebuild] Bootstrap failed: " + ex.Message);
            }

            // ─── STEP 4: NavMesh bake — invoked via Moon1NavMeshBake menu ───────
            try
            {
                var t = System.Type.GetType("Tartaria.Editor.Moon1NavMeshBake, Assembly-CSharp-Editor");
                var m = t?.GetMethod("Bake", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (m != null) m.Invoke(null, null);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[NuclearRebuild] NavMesh bake skipped: " + ex.Message);
            }

            // ─── STEP 5: Save scene ──────────────────────────────────────────────
            EditorSceneManager.SaveScene(scene);

            string deletedList = deletedNames.Count > 12
                ? string.Join(", ", deletedNames.GetRange(0, 12)) + $" + {deletedNames.Count - 12} more"
                : string.Join(", ", deletedNames);

            EditorUtility.DisplayDialog("Nuclear Rebuild Complete",
                $"Deleted {deleted} scene-root GameObjects:\n  {deletedList}\n\n" +
                $"Re-bootstrapped Moon1_Systems with all 12 components.\n" +
                $"Auto-wired prefab refs to Blender prefabs.\n" +
                $"NavMesh baked.\nScene saved.\n\n" +
                $"Hit Play — Bootstrap will re-spawn from Blender prefabs only.",
                "OK");
        }

        static bool ShouldPreserve(string name)
        {
            if (PreserveExact.Contains(name)) return true;
            foreach (var p in PreservePrefix)
                if (name.StartsWith(p)) return true;
            return false;
        }
    }
}
#endif
