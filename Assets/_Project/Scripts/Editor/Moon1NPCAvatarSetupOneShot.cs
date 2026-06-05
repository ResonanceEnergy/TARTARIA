// Moon1NPCAvatarSetupOneShot.cs — 2026-06-04 NPC ARMATURE PIPELINE STAGE A
//
// Auto-fires once on next Unity Editor launch to:
//   1. Walk the 4 Moon 1 NPC FBXs (AnastasiaPrincess, LiraelGuardian,
//      CassianCarter, BobInnkeeper) under Assets/_Project/Models/Blender/Moon1/
//   2. Force animationType = Human + avatarSetup = CreateFromThisModel so the
//      auto-mapping AvatarBuilder runs against the new 21-bone humanoid
//      armature emitted by tools/blender/_common.make_humanoid_armature.
//   3. Trigger reimport so the Avatar sub-asset materializes.
//   4. Mark EditorPref so it doesn't re-fire.
//
// Once it runs successfully it self-disables. NATRIX can delete the file
// after verifying. If something goes wrong, EditorPref reset via
// Tartaria/8 Fix/Reset NPC Avatar Setup OneShot.
//
// Idempotent: re-running clears the flag, walks the files again, applies
// settings, reimports. No destructive ops.

using System.IO;
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor.OneShots
{
    [InitializeOnLoad]
    public static class Moon1NPCAvatarSetupOneShot
    {
        // Stage B (2026-06-04): re-fire after the 25-bone UpperChest/Eyes/Jaw
        // armature lands. The Avatar mapping must re-import to pick up the new
        // bone hierarchy.
        const string PREF_KEY = "Tartaria.OneShot.NPCAvatarSetup.2026-06-04-StageB";

        // The 4 Moon 1 humanoid NPCs whose FBXs are emitted by tools/blender
        // with Stage A armatures. Keys ARE the FBX basenames under Moon1/.
        static readonly string[] NPC_FBXS = new[]
        {
            "Assets/_Project/Models/Blender/Moon1/AnastasiaPrincess.fbx",
            "Assets/_Project/Models/Blender/Moon1/LiraelGuardian.fbx",
            "Assets/_Project/Models/Blender/Moon1/CassianCarter.fbx",
            "Assets/_Project/Models/Blender/Moon1/BobInnkeeper.fbx",
        };

        static Moon1NPCAvatarSetupOneShot()
        {
            if (EditorPrefs.GetBool(PREF_KEY, false)) return;
            EditorApplication.delayCall += () => Run();
        }

        [MenuItem("Tartaria/8 Fix/Reset NPC Avatar Setup OneShot", priority = 997)]
        static void ResetFlag()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            Debug.Log("[NPCAvatarSetupOneShot] Flag cleared. Will fire again on next domain reload.");
        }

        [MenuItem("Tartaria/8 Fix/Run NPC Avatar Setup NOW", priority = 996)]
        static void RunNow() => Run();

        static void Run()
        {
            if (EditorPrefs.GetBool(PREF_KEY, false))
            {
                Debug.Log("[NPCAvatarSetupOneShot] Already ran this session. Skip.");
                return;
            }

            int configured = 0;
            int missing = 0;
            int failed = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var path in NPC_FBXS)
                {
                    string abs = Path.Combine(Directory.GetCurrentDirectory(), path);
                    if (!File.Exists(abs))
                    {
                        Debug.LogWarning("[NPCAvatarSetupOneShot] Missing FBX, skip: " + path);
                        missing++;
                        continue;
                    }

                    var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer == null)
                    {
                        Debug.LogWarning("[NPCAvatarSetupOneShot] No ModelImporter at: " + path);
                        failed++;
                        continue;
                    }

                    // Force Humanoid + auto-create avatar. The .meta already has
                    // autoGenerateAvatarMappingIfUnspecified=1 so Unity will map
                    // our 21 Unity-canonical bone names to HumanBodyBones without
                    // needing a manual transform map.
                    importer.animationType = ModelImporterAnimationType.Human;
                    importer.avatarSetup   = ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.useFileScale  = false;
                    importer.globalScale   = 1.0f;
                    importer.bakeAxisConversion = true;
                    importer.importBlendShapes  = false;
                    importer.importVisibility   = false;
                    importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;

                    importer.SaveAndReimport();
                    Debug.Log("[NPCAvatarSetupOneShot] Configured Humanoid + reimported: " + path);
                    configured++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            if (configured > 0 && failed == 0)
            {
                EditorPrefs.SetBool(PREF_KEY, true);
                Debug.Log($"[NPCAvatarSetupOneShot] Complete. configured={configured} missing={missing} failed={failed}. Flag set; will not re-fire.");
            }
            else
            {
                Debug.LogWarning($"[NPCAvatarSetupOneShot] Partial result configured={configured} missing={missing} failed={failed}. Flag NOT set; will retry next launch.");
            }
        }
    }
}
