#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Auto-processes FBX files dropped into Assets/_Project/Models/Blender/
    /// with the correct settings for URP + 1m-scale + no animation embedding.
    ///
    /// When NATRIX runs `blender --background --python tools/blender/run_all_moon1.py`,
    /// the resulting FBX files land here; Unity imports them with proper materials.
    ///
    /// Also generates Unity .prefab variants per .fbx for direct use in scenes.
    ///
    /// Sprint 10 Lane 6 addition: special-cases the Moon 1 NPC FBX files
    /// (Lirael / Anastasia / Cassian / Milo variants) so they import as
    /// Generic Mecanim rigs ready for animation retargeting in a later sprint.
    /// </summary>
    public class BlenderImportPostprocessor : AssetPostprocessor
    {
        const string BLENDER_FBX_ROOT = "Assets/_Project/Models/Blender";
        const string MOON1_FBX_ROOT   = "Assets/_Project/Models/Blender/Moon1";

        // Moon 1 NPC FBX filenames that need rig configuration. Both the
        // short "name" form and the Sprint 9 Lane 5 "RoleSuffix" forms are
        // listed so either set imports correctly.
        //
        // Sprint 11 TODO: once the Blender NPC scripts emit a real armature
        // (hips/spine/head/limbs hierarchy with weighted skin), flip these
        // from Generic to Humanoid + autoconfigure the AvatarMask. The
        // current Blender gen_* scripts produce static joined meshes with
        // no bones, so Humanoid mapping would silently fail.
        static readonly string[] NPC_FILENAMES = new[]
        {
            "Lirael.fbx",
            "Anastasia.fbx",
            "Cassian.fbx",
            "Milo.fbx",
            "LiraelGuardian.fbx",
            "AnastasiaPrincess.fbx",
            "CassianCarter.fbx",
            "MiloBoy.fbx",
        };

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(BLENDER_FBX_ROOT)) return;
            var importer = (ModelImporter)assetImporter;

            // Scale + axis — Blender's export uses 1.0 unit = 1m which matches Unity
            importer.useFileScale = false;
            importer.globalScale = 1.0f;
            importer.bakeAxisConversion = true;

            // Materials — embed and extract for tweaking
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;

            // Mesh
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.weldVertices = true;

            // Animation — default is static prop, no anim
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;

            // NPC-specific overrides for Moon 1 character FBX files.
            if (IsMoon1NpcAsset(assetPath))
            {
                ConfigureNpcImporter(importer);
            }

            Debug.Log("[BlenderImportPostprocessor] Configured " + assetPath);
        }

        static bool IsMoon1NpcAsset(string path)
        {
            if (!path.StartsWith(MOON1_FBX_ROOT)) return false;
            string fileName = Path.GetFileName(path);
            for (int i = 0; i < NPC_FILENAMES.Length; i++)
            {
                if (string.Equals(fileName, NPC_FILENAMES[i], System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        static void ConfigureNpcImporter(ModelImporter importer)
        {
            // Generic rig — safer first pass than Humanoid because the Sprint 9
            // Blender NPC scripts join all meshes and do not emit a bone
            // hierarchy. Humanoid retargeting needs Hips/Spine/Head and the
            // limb chain; without bones Unity would log "AvatarBuilder failed"
            // and leave the avatar invalid. Sprint 11 will upgrade to
            // Humanoid once the Blender pipeline emits a real armature.
            importer.animationType  = ModelImporterAnimationType.Generic;
            importer.globalScale    = 1.0f;
            importer.useFileScale   = false;
            importer.bakeAxisConversion = true;
            importer.importBlendShapes  = false;   // none authored
            importer.importVisibility   = false;   // hidden Blender objects shouldn't carry over
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;

            Debug.Log("[BlenderImport] Configured NPC humanoid presets for " + importer.assetPath);
        }

        // After import, convert materials to URP/Lit
        void OnPostprocessMaterial(Material material)
        {
            if (!assetPath.StartsWith(BLENDER_FBX_ROOT)) return;
            var urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp == null) return;
            material.shader = urp;
            Debug.Log("[BlenderImportPostprocessor] " + material.name + " -> URP/Lit");
        }

        // After whole-asset import, auto-generate a prefab variant for direct use
        static void OnPostprocessAllAssets(string[] importedAssets, string[] _, string[] __, string[] ___)
        {
            foreach (var path in importedAssets)
            {
                if (!path.StartsWith(BLENDER_FBX_ROOT)) continue;
                if (!path.EndsWith(".fbx")) continue;
                GeneratePrefabVariant(path);
            }
        }

        static void GeneratePrefabVariant(string fbxPath)
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbx == null) return;

            string baseName = Path.GetFileNameWithoutExtension(fbxPath);
            string prefabDir = "Assets/_Project/Prefabs/Moon1/Blender";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);
            string prefabPath = prefabDir + "/" + baseName + ".prefab";

            // Skip if prefab already exists (so we don't overwrite manual tweaks)
            if (File.Exists(prefabPath)) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            AssetDatabase.SaveAssets();
            Debug.Log("[BlenderImportPostprocessor] Created prefab variant: " + prefabPath);
        }

        // ----- Editor menu: force-reimport the Moon 1 NPC FBX set -----
        // Used after the Sprint 10 import-config change to re-run
        // OnPreprocessModel with the new Generic-rig settings.
        [MenuItem("Tartaria/Content/Reimport Moon 1 NPC FBX")]
        public static void ReimportMoon1NpcFbx()
        {
            int found = 0;
            int missing = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var fileName in NPC_FILENAMES)
                {
                    string assetPath = MOON1_FBX_ROOT + "/" + fileName;
                    string absolute  = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                    if (!File.Exists(absolute))
                    {
                        Debug.LogWarning("[BlenderImport] Reimport skipped — not on disk: " + assetPath);
                        missing++;
                        continue;
                    }
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    Debug.Log("[BlenderImport] Force-reimported NPC FBX: " + assetPath);
                    found++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
            Debug.Log($"[BlenderImport] Reimport Moon 1 NPC FBX complete. Reimported={found} Missing={missing}");
        }
    }
}
#endif
