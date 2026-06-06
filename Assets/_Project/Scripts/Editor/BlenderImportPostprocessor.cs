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
        // 2026-06-04 STAGE A CLOSED: Blender NPC scripts now emit a 21-bone
        // humanoid armature with Unity HumanBodyBones canonical names. The
        // animationType is upgraded to Humanoid in ConfigureNpcImporter()
        // below. Auto-mapping is fed by autoGenerateAvatarMappingIfUnspecified
        // in the .meta files. Stage B will refine vertex weights + add
        // humanDescription mass/center metadata.
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
            "BobInnkeeper.fbx",
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
            // 2026-06-04 NPC armature pipeline Stage A: the Blender NPC scripts
            // now emit a 21-bone Unity-canonical humanoid armature (Hips/Spine/
            // Chest/Neck/Head + 4-bone arms + 3-bone legs) bound via auto-weights.
            // Set animationType = Humanoid so Unity's AvatarBuilder runs against
            // the canonical bone names. The .meta files already carry
            // autoGenerateAvatarMappingIfUnspecified=1, so the Avatar
            // sub-asset materializes on reimport without manual mapping.
            //
            // Stage B will introduce proper humanDescription mass/centerOfMass
            // and per-bone twist limits. For now the auto-mapping is acceptable.
            importer.animationType  = ModelImporterAnimationType.Human;
            importer.avatarSetup    = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.globalScale    = 1.0f;
            importer.useFileScale   = false;
            importer.bakeAxisConversion = true;
            importer.importBlendShapes  = false;   // none authored
            importer.importVisibility   = false;   // hidden Blender objects shouldn't carry over
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;

            // 2026-06-05 ROOT-CAUSE MAGENTA FIX: URP/Lit's skinned-mesh variant requires the
            // Standard (4-bones-per-vertex) skin weight pipeline. Default `OneBone` ships a
            // shader keyword combo that no project URP variant collection compiles, so the
            // skinned mesh renders magenta even though the material/shader look correct.
            // Per Unity 6 manual: ModelImporter.maxBonesPerVertex + skinWeights together
            // determine which URP skinning variant is required. Standard = 4 = URP default.
            importer.skinWeights      = ModelImporterSkinWeights.Standard;
            importer.maxBonesPerVertex = 4;

            Debug.Log("[BlenderImport] Configured NPC as Humanoid (Stage A armature) for " + importer.assetPath);
        }

        // After import, convert materials to URP/Lit
        void OnPostprocessMaterial(Material material)
        {
            if (!assetPath.StartsWith(BLENDER_FBX_ROOT)) return;
            var urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp == null) return;
            material.shader = urp;

            // 2026-06-05 ALBEDO BRIGHTEN: Blender's default export produces _BaseColor in
            // 0.05–0.10 linear range for "black" materials (e.g. CassianCarter_robe = (0.08,
            // 0.06, 0.05)). At runtime with 1.0 sun intensity that yields a near-black render
            // — Cassian appears as silhouette. Auto-brighten any non-skin/iris material whose
            // RGB is all below 0.30 so the player character is legible in screenshots.
            if (material.HasProperty("_BaseColor"))
            {
                string n = material.name.ToLower();
                bool skipBrighten = n.Contains("skin") || n.Contains("iris") || n.Contains("eye") || n.Contains("hair");
                if (!skipBrighten)
                {
                    var c = material.GetColor("_BaseColor");
                    if (c.r < 0.30f && c.g < 0.30f && c.b < 0.30f)
                    {
                        // Preserve hue, lift luminance to ~0.55
                        var bright = new Color(
                            Mathf.Max(c.r * 3.5f, 0.40f),
                            Mathf.Max(c.g * 3.5f, 0.38f),
                            Mathf.Max(c.b * 3.5f, 0.35f), c.a);
                        material.SetColor("_BaseColor", bright);
                        Debug.Log($"[BlenderImport] Brightened {material.name}: {c} → {bright}");
                    }
                }
            }

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

            // 2026-06-04 REORG-5: also skip if the prefab exists in any category subfolder
            // (post-C.L1 migration moved 347 prefabs into Architecture/Audio/NPCs/Plates/Props/VFX).
            // Without this guard, every FBX re-import regenerates a duplicate root copy with a
            // fresh GUID, leaving the original tracked subfolder copy plus an untracked shadow.
            string[] categories = { "Architecture", "Audio", "NPCs", "Plates", "Props", "VFX" };
            foreach (var cat in categories)
            {
                string subPath = prefabDir + "/" + cat + "/" + baseName + ".prefab";
                if (File.Exists(subPath)) return;
            }

            // Instantiate the FBX and save it as a prefab variant for direct scene use.
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            if (instance == null) return;
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            Debug.Log($"[BlenderImport] Generated prefab variant: {prefabPath}");
        }
    }
}
#endif