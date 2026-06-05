// Moon1PlayerVisualWireOneShot.cs
// One-shot Editor script — nests CassianCarter.fbx under Player.prefab's _CharacterVisual
// so the Player has a real rendered humanoid mesh (instead of an empty Transform).
//
// 2026-06-04 (HAMMER) — Player.prefab nests Char_Knight.prefab as _CharacterVisual but
// Char_Knight has no MeshRenderer/SkinnedMeshRenderer (KayKit FBX import never extracted
// mesh). CassianCarter.fbx is a real Blender-baked 1.8m humanoid with armature + 7
// URP/Lit mats already assigned. Use it as the Player visual until a dedicated
// PlayerHero.fbx is authored.
//
// TRADE-OFF (honest): This is a Stage A bridge. Cassian's body becomes the Player visual
// while CassianCarter ALSO still exists as an NPC spawned by EchohavenContentSpawner.
// In play, the world will contain TWO Cassians (one is Player, one is NPC). That is
// acceptable for build-phase visibility verification, NOT for ship. The Stage B
// follow-up is to author a dedicated PlayerHero.fbx and re-wire this slot. The Player
// will retarget Cassian's humanoid rig to AC_KayKit_Medium via the existing Animator
// Controller (assigned in Player.prefab as fileID 9100000 guid 78734b5564ec49d4bade3f0b1c74f6d9).
//
// Idempotent — guarded by EditorPrefs flag. Run-NOW menu + Reset menu under Tartaria/8 Fix/.
// Per CLAUDE.md mandates: no silent catches, no // TODO, no Resources.Load on
// non-existent paths, full context logging.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    [InitializeOnLoad]
    internal static class Moon1PlayerVisualWireOneShot
    {
        private const string PrefKey         = "Tartaria.OneShot.PlayerVisualWire.2026-06-04";
        private const string PlayerPath      = "Assets/_Project/Prefabs/Characters/Player.prefab";
        private const string CassianFbxPath  = "Assets/_Project/Models/Blender/Moon1/CassianCarter.fbx";
        private const string VisualChildName = "_CharacterVisual";
        private const string NestedName      = "PlayerVisual_Cassian";

        static Moon1PlayerVisualWireOneShot()
        {
            if (EditorPrefs.GetBool(PrefKey, false)) return;
            EditorApplication.delayCall += Run;
        }

        [MenuItem("Tartaria/8 Fix/Reset Player Visual Wire OneShot", priority = 995)]
        private static void ResetFlag()
        {
            EditorPrefs.DeleteKey(PrefKey);
            Debug.Log("[PlayerVisualWire] Flag cleared. Will run again on next reload (or via Run NOW menu).");
        }

        [MenuItem("Tartaria/8 Fix/Run Player Visual Wire NOW", priority = 994)]
        private static void RunNow()
        {
            EditorPrefs.DeleteKey(PrefKey);
            Run();
        }

        private static void Run()
        {
            if (EditorPrefs.GetBool(PrefKey, false))
            {
                return;
            }

            if (!File.Exists(PlayerPath))
            {
                Debug.LogError($"[PlayerVisualWire] Player prefab not found at '{PlayerPath}'. Abort.");
                return;
            }
            if (!File.Exists(CassianFbxPath))
            {
                Debug.LogError($"[PlayerVisualWire] Cassian FBX not found at '{CassianFbxPath}'. Abort.");
                return;
            }

            var playerPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
            var cassianFbx    = AssetDatabase.LoadAssetAtPath<GameObject>(CassianFbxPath);
            if (playerPrefab == null)
            {
                Debug.LogError($"[PlayerVisualWire] Failed to load Player prefab as GameObject (asset import bad?). Abort.");
                return;
            }
            if (cassianFbx == null)
            {
                Debug.LogError($"[PlayerVisualWire] Failed to load Cassian FBX as GameObject (asset import bad?). Abort.");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            if (instance == null)
            {
                Debug.LogError("[PlayerVisualWire] Failed to instantiate Player prefab for edit. Abort.");
                return;
            }

            try
            {
                var visualXform = instance.transform.Find(VisualChildName);
                if (visualXform == null)
                {
                    Debug.Log($"[PlayerVisualWire] No '{VisualChildName}' child on Player root — creating one.");
                    var v = new GameObject(VisualChildName);
                    v.transform.SetParent(instance.transform, false);
                    visualXform = v.transform;
                }

                // Clear any existing children of _CharacterVisual.
                // This nukes prior placeholder capsule (Moon1PlayerVisualPlaceholderOneShot.cs leftover) or stale Char_Knight nesting.
                int removed = 0;
                for (int i = visualXform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(visualXform.GetChild(i).gameObject);
                    removed++;
                }
                if (removed > 0)
                {
                    Debug.Log($"[PlayerVisualWire] Cleared {removed} existing child(ren) under '{VisualChildName}' before nesting Cassian.");
                }

                // Nest Cassian FBX as visual child (keeps Cassian's prefab-instance link to the FBX, so future re-bakes propagate).
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(cassianFbx);
                if (visual == null)
                {
                    Debug.LogError("[PlayerVisualWire] Failed to instantiate Cassian FBX. Abort (Player unchanged).");
                    return;
                }
                visual.name = NestedName;
                visual.transform.SetParent(visualXform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale    = Vector3.one;

                // Save back to the Player prefab.
                var saved = PrefabUtility.SaveAsPrefabAsset(instance, PlayerPath);
                if (saved == null)
                {
                    Debug.LogError($"[PlayerVisualWire] PrefabUtility.SaveAsPrefabAsset returned null for '{PlayerPath}'. Abort.");
                    return;
                }

                Debug.Log($"[PlayerVisualWire] OK Cassian visual nested under Player._CharacterVisual.{NestedName} (source FBX: {CassianFbxPath}).");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorPrefs.SetBool(PrefKey, true);
            Debug.Log("[PlayerVisualWire] OneShot complete. Flag set. (Re-run via Tartaria/8 Fix/Run Player Visual Wire NOW.)");
        }
    }
}
