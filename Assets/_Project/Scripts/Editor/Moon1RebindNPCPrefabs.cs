#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Sprint 9 Lane 6 — replace the primitive Lirael/Anastasia/Cassian prefabs in
    /// Echohaven_VerticalSlice.unity with the Blender-FBX prefab variants produced
    /// by Sprint 9 Lane 5.
    ///
    /// Source FBX expected at: Assets/_Project/Models/Blender/Moon1/&lt;NPC&gt;.fbx
    /// Target prefab variant at: Assets/_Project/Prefabs/Moon1/Blender/&lt;NPC&gt;.prefab
    /// (auto-created by BlenderImportPostprocessor.OnPostprocessAllAssets)
    ///
    /// Idempotent: if a scene GameObject is already an instance of the new variant
    /// (PrefabUtility.GetCorrespondingObjectFromSource matches), it is skipped.
    ///
    /// Old primitive .prefab files are NOT deleted — kept so we can roll back if
    /// the new FBX looks broken.
    /// </summary>
    public static class Moon1RebindNPCPrefabs
    {
        const string MENU_PATH = "Tartaria/Content/Rebind Moon 1 NPC Prefabs";
        const string SCENE_PATH = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string FBX_ROOT = "Assets/_Project/Models/Blender/Moon1";
        const string BLENDER_PREFAB_ROOT = "Assets/_Project/Prefabs/Moon1/Blender";
        const string OLD_PREFAB_ROOT = "Assets/_Project/Prefabs/Characters";

        /// <summary>
        /// One NPC's source FBX + target variant + scene-object identifiers.
        /// </summary>
        struct NPCBinding
        {
            // Short name used to match scene GameObject by name OR tag.
            public string NpcName;
            // Filename (no extension) of the Lane-5 Blender FBX + prefab variant.
            public string BlenderAssetName;

            public NPCBinding(string npcName, string blenderAssetName)
            {
                NpcName = npcName;
                BlenderAssetName = blenderAssetName;
            }

            public string FbxPath => $"{FBX_ROOT}/{BlenderAssetName}.fbx";
            public string NewPrefabPath => $"{BLENDER_PREFAB_ROOT}/{BlenderAssetName}.prefab";
            public string OldPrefabPath => $"{OLD_PREFAB_ROOT}/{NpcName}.prefab";
        }

        // The actual Lane 5 FBX names (verified on disk 2026-06-02) attach a
        // descriptive suffix to each NPC short-name (Guardian / Princess / Carter).
        // If Lane 5 renames its outputs, update this table.
        static readonly NPCBinding[] BINDINGS = new[]
        {
            new NPCBinding("Lirael",    "LiraelGuardian"),
            new NPCBinding("Anastasia", "AnastasiaPrincess"),
            new NPCBinding("Cassian",   "CassianCarter"),
        };

        [MenuItem(MENU_PATH)]
        public static void RebindNPCs()
        {
            int replaced = 0;
            int skipped = 0;
            var warnings = new List<string>();

            // Open the Echohaven scene single-mode so we operate on a clean slate
            // (matches Lane 6 spec — no leftover scenes in the hierarchy).
            if (!File.Exists(SCENE_PATH))
            {
                Debug.LogError($"[NPCRebind] Echohaven scene not found at {SCENE_PATH} — aborting.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[NPCRebind] Failed to open scene {SCENE_PATH} — aborting.");
                return;
            }

            Debug.Log($"[NPCRebind] Opened scene: {scene.path}. Bindings: {BINDINGS.Length}.");

            // Cache scene root list once; we re-collect per-NPC so destroyed objects
            // don't leak into later passes.
            foreach (var binding in BINDINGS)
            {
                // Gate 1 — source FBX must exist. If Lane 5 hasn't shipped yet,
                // log loudly + move to the next NPC (do NOT abort the whole run).
                if (!File.Exists(binding.FbxPath))
                {
                    string msg = $"[NPCRebind] {binding.NpcName}: FBX not yet generated at {binding.FbxPath} — skipping (Lane 5 in flight).";
                    Debug.LogError(msg);
                    warnings.Add(msg);
                    skipped++;
                    continue;
                }

                // Gate 2 — auto-created prefab variant must exist. If FBX landed
                // but BlenderImportPostprocessor hasn't run yet, force a reimport.
                var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(binding.NewPrefabPath);
                if (newPrefab == null)
                {
                    Debug.LogWarning($"[NPCRebind] {binding.NpcName}: prefab variant {binding.NewPrefabPath} missing — forcing FBX reimport to trigger BlenderImportPostprocessor.");
                    AssetDatabase.ImportAsset(binding.FbxPath, ImportAssetOptions.ForceUpdate);
                    AssetDatabase.Refresh();
                    newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(binding.NewPrefabPath);
                }

                if (newPrefab == null)
                {
                    string msg = $"[NPCRebind] {binding.NpcName}: prefab variant still missing at {binding.NewPrefabPath} after reimport — skipping.";
                    Debug.LogError(msg);
                    warnings.Add(msg);
                    skipped++;
                    continue;
                }

                int npcReplaced = ReplaceInScene(binding, newPrefab, warnings);
                if (npcReplaced == 0)
                {
                    string msg = $"[NPCRebind] {binding.NpcName}: no scene GameObject matched name or tag — nothing to replace.";
                    Debug.LogWarning(msg);
                    warnings.Add(msg);
                    skipped++;
                }
                else
                {
                    replaced += npcReplaced;
                }
            }

            // Persist scene changes if we touched anything.
            if (replaced > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                bool saved = EditorSceneManager.SaveScene(scene);
                if (!saved)
                {
                    Debug.LogError($"[NPCRebind] Failed to save scene {scene.path} after {replaced} replacements.");
                }
                else
                {
                    Debug.Log($"[NPCRebind] Saved scene {scene.path}.");
                }
            }

            Debug.Log($"[NPCRebind] Replaced {replaced}, skipped {skipped} (see warnings)");
        }

        /// <summary>
        /// Find scene GameObjects matching the NPC short-name (by name or tag) and
        /// swap them for the new Blender prefab variant. Idempotent: already-bound
        /// instances are skipped.
        /// </summary>
        static int ReplaceInScene(NPCBinding binding, GameObject newPrefab, List<string> warnings)
        {
            int replaced = 0;

            // Unity 6 API — FindObjectsByType returns ALL active+inactive objects
            // in loaded scenes, which is exactly what we want (some NPCs spawn in
            // disabled containers).
            var allObjects = Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            // Collect matches up front so we don't mutate during iteration.
            var matches = new List<GameObject>();
            foreach (var go in allObjects)
            {
                if (go == null) continue;
                bool nameMatch = go.name == binding.NpcName;
                bool tagMatch = false;
                // Tag lookup throws if the tag is undefined — guard it.
                try { tagMatch = go.CompareTag(binding.NpcName); }
                catch (UnityException) { tagMatch = false; }

                if (!nameMatch && !tagMatch) continue;

                // Idempotency — if this GameObject is already an instance of the
                // new variant prefab, skip it.
                var source = PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (source != null)
                {
                    string sourcePath = AssetDatabase.GetAssetPath(source);
                    if (sourcePath == binding.NewPrefabPath)
                    {
                        Debug.Log($"[NPCRebind] {binding.NpcName}: '{go.name}' already linked to {binding.NewPrefabPath} — idempotent skip.");
                        continue;
                    }
                }

                matches.Add(go);
            }

            foreach (var oldGO in matches)
            {
                Transform oldT = oldGO.transform;
                Transform parent = oldT.parent;
                Vector3 localPos = oldT.localPosition;
                Quaternion localRot = oldT.localRotation;
                Vector3 localScale = oldT.localScale;
                string oldName = oldGO.name;

                // What was the old GameObject linked to (for the log line)?
                var oldSource = PrefabUtility.GetCorrespondingObjectFromSource(oldGO);
                string oldPath = oldSource != null
                    ? AssetDatabase.GetAssetPath(oldSource)
                    : binding.OldPrefabPath; // best-effort fallback for naming-only matches

                // Instantiate the new prefab variant AS a prefab connection.
                var newInstance = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab, oldT.gameObject.scene);
                if (newInstance == null)
                {
                    string msg = $"[NPCRebind] {binding.NpcName}: PrefabUtility.InstantiatePrefab returned null for {binding.NewPrefabPath} — skipping {oldName}.";
                    Debug.LogError(msg);
                    warnings.Add(msg);
                    continue;
                }

                Undo.RegisterCreatedObjectUndo(newInstance, "Rebind NPC to Blender variant");

                // Re-parent + copy transform.
                newInstance.transform.SetParent(parent, worldPositionStays: false);
                newInstance.transform.localPosition = localPos;
                newInstance.transform.localRotation = localRot;
                newInstance.transform.localScale = localScale;

                // Preserve the original short-name so other systems that
                // GameObject.Find("Lirael") still resolve.
                newInstance.name = oldName;

                Debug.Log($"[NPCRebind] {binding.NpcName}: replaced primitive @ {oldPath} with Blender variant @ {binding.NewPrefabPath}");

                Undo.DestroyObjectImmediate(oldGO);
                replaced++;
            }

            return replaced;
        }
    }
}
#endif
