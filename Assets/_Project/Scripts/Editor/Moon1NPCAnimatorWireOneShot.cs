// Moon1NPCAnimatorWireOneShot.cs — 2026-06-04 NPC ARMATURE PIPELINE STAGE D
//
// Wires `Animator.runtimeAnimatorController = AC_KayKit_Medium` (guid
// 78734b5564ec49d4bade3f0b1c74f6d9) + `Animator.avatar = <FBX-imported Avatar
// sub-asset>` on the 4 Moon 1 NPC prefabs (Milo, Anastasia, Lirael, Cassian).
//
// CRITICAL ORDERING: This one-shot MUST run AFTER Moon1NPCAvatarSetupOneShot
// has completed. That earlier one-shot sets `ModelImporter.animationType =
// Human` + `avatarSetup = CreateFromThisModel` and forces a reimport, which
// causes Unity to materialize an `Avatar` sub-asset inside each FBX. Without
// the Avatar present, the `animator.avatar` assignment here would be null
// and the humanoid retarget against AC_KayKit_Medium would fail at runtime.
//
// InitializeOnLoad delayCall ordering between two [InitializeOnLoad] static
// constructors is implementation-defined — Unity guarantees BOTH run after
// domain reload, but not the order. We resolve that by:
//   1. Checking for non-null Avatar sub-asset on the FBX before wiring; if
//      the Avatar is missing we BAIL WITHOUT setting the success flag, so
//      this one-shot re-fires on the next launch (by which point the avatar
//      setup will have re-imported the FBX).
//   2. Manual "Run NOW" menu lets NATRIX force the wire after a fresh open.
//
// Idempotent — guarded by EditorPrefs flag. Reset + Run-NOW menus under
// Tartaria/8 Fix/. Per CLAUDE.md mandates: no silent catches, no // TODO,
// no Resources.Load on non-existent paths, full context logging.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor.OneShots
{
    [InitializeOnLoad]
    public static class Moon1NPCAnimatorWireOneShot
    {
        const string PREF_KEY = "Tartaria.OneShot.NPCAnimatorWire.2026-06-04";
        const string KAYKIT_CONTROLLER_GUID = "78734b5564ec49d4bade3f0b1c74f6d9";

        struct NPCMapping
        {
            public string PrefabPath;
            public string FbxPath;
        }

        // Per task spec — 4 NPCs (Milo, Anastasia, Lirael, Cassian).
        // BobInnkeeper is intentionally omitted from this Stage D wire (Stage A
        // included him in the Avatar setup, but the Moon 1 prefab list per
        // CLAUDE.md punch-list is these four).
        static readonly NPCMapping[] NPCs = new[]
        {
            new NPCMapping { PrefabPath = "Assets/_Project/Prefabs/Characters/Milo.prefab",      FbxPath = "Assets/_Project/Models/Blender/Moon1/MiloBoy.fbx" },
            new NPCMapping { PrefabPath = "Assets/_Project/Prefabs/Characters/Anastasia.prefab", FbxPath = "Assets/_Project/Models/Blender/Moon1/AnastasiaPrincess.fbx" },
            new NPCMapping { PrefabPath = "Assets/_Project/Prefabs/Characters/Lirael.prefab",    FbxPath = "Assets/_Project/Models/Blender/Moon1/LiraelGuardian.fbx" },
            new NPCMapping { PrefabPath = "Assets/_Project/Prefabs/Characters/Cassian.prefab",   FbxPath = "Assets/_Project/Models/Blender/Moon1/CassianCarter.fbx" }
        };

        static Moon1NPCAnimatorWireOneShot()
        {
            if (EditorPrefs.GetBool(PREF_KEY, false)) return;
            EditorApplication.delayCall += Run;
        }

        [MenuItem("Tartaria/8 Fix/Reset NPC Animator Wire OneShot", priority = 993)]
        static void Reset()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            Debug.Log("[NPCAnimatorWire] Flag cleared. Will fire again on next domain reload (or via Run NOW menu).");
        }

        [MenuItem("Tartaria/8 Fix/Run NPC Animator Wire NOW", priority = 992)]
        static void RunNow()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            Run();
        }

        static void Run()
        {
            if (EditorPrefs.GetBool(PREF_KEY, false))
            {
                Debug.Log("[NPCAnimatorWire] Already ran this session. Skip.");
                return;
            }

            var controllerPath = AssetDatabase.GUIDToAssetPath(KAYKIT_CONTROLLER_GUID);
            if (string.IsNullOrEmpty(controllerPath))
            {
                Debug.LogError($"[NPCAnimatorWire] AC_KayKit_Medium not found by GUID '{KAYKIT_CONTROLLER_GUID}'. Abort.");
                return;
            }
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller == null)
            {
                Debug.LogError($"[NPCAnimatorWire] AC_KayKit_Medium failed to load at '{controllerPath}'. Abort.");
                return;
            }

            int wired = 0;
            int skipped = 0;
            int failed = 0;
            bool avatarMissingDeferral = false;

            foreach (var npc in NPCs)
            {
                if (!File.Exists(npc.PrefabPath))
                {
                    Debug.LogWarning($"[NPCAnimatorWire] Prefab missing on disk, skip: {npc.PrefabPath}");
                    skipped++;
                    continue;
                }
                if (!File.Exists(npc.FbxPath))
                {
                    Debug.LogWarning($"[NPCAnimatorWire] FBX missing on disk, skip: {npc.FbxPath}");
                    skipped++;
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(npc.PrefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"[NPCAnimatorWire] Failed to load prefab as GameObject: {npc.PrefabPath}");
                    failed++;
                    continue;
                }

                // Walk FBX sub-assets to find the Avatar sub-asset that Unity
                // materialized after Moon1NPCAvatarSetupOneShot set animationType
                // = Human. If absent, we BAIL the whole run without setting the
                // success flag so we retry on the next domain reload.
                Avatar avatar = null;
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(npc.FbxPath);
                foreach (var a in subAssets)
                {
                    if (a is Avatar av)
                    {
                        avatar = av;
                        break;
                    }
                }
                if (avatar == null)
                {
                    Debug.LogWarning($"[NPCAnimatorWire] Avatar sub-asset not found in '{npc.FbxPath}' — Humanoid import has not completed. Will retry next launch.");
                    avatarMissingDeferral = true;
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (instance == null)
                {
                    Debug.LogError($"[NPCAnimatorWire] Failed to instantiate prefab for edit: {npc.PrefabPath}");
                    failed++;
                    continue;
                }

                try
                {
                    var animator = instance.GetComponentInChildren<Animator>();
                    if (animator == null)
                    {
                        Debug.LogWarning($"[NPCAnimatorWire] No Animator found on or under '{npc.PrefabPath}'. Skip.");
                        skipped++;
                        continue;
                    }

                    animator.runtimeAnimatorController = controller;
                    animator.avatar = avatar;

                    var saved = PrefabUtility.SaveAsPrefabAsset(instance, npc.PrefabPath);
                    if (saved == null)
                    {
                        Debug.LogError($"[NPCAnimatorWire] PrefabUtility.SaveAsPrefabAsset returned null for '{npc.PrefabPath}'.");
                        failed++;
                        continue;
                    }

                    wired++;
                    Debug.Log($"[NPCAnimatorWire] OK '{npc.PrefabPath}' ctrl=AC_KayKit_Medium avatar={avatar.name}");
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (avatarMissingDeferral)
            {
                Debug.LogWarning($"[NPCAnimatorWire] Partial result wired={wired} skipped={skipped} failed={failed} (avatar-missing deferral). Flag NOT set; will retry next launch.");
                return;
            }

            if (wired == NPCs.Length)
            {
                EditorPrefs.SetBool(PREF_KEY, true);
                Debug.Log($"[NPCAnimatorWire] Complete. Wired {wired}/{NPCs.Length}. Flag set; will not re-fire.");
            }
            else
            {
                Debug.LogWarning($"[NPCAnimatorWire] Did not wire all 4 NPCs. wired={wired} skipped={skipped} failed={failed}. Flag NOT set; will retry next launch.");
            }
        }
    }
}
