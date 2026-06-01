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
    /// </summary>
    public class BlenderImportPostprocessor : AssetPostprocessor
    {
        const string BLENDER_FBX_ROOT = "Assets/_Project/Models/Blender";

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

            // Animation — these are static props, no anim
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;

            Debug.Log("[BlenderImportPostprocessor] Configured " + assetPath);
        }

        // After import, convert materials to URP/Lit
        void OnPostprocessMaterial(Material material)
        {
            if (!assetPath.StartsWith(BLENDER_FBX_ROOT)) return;
            var urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp == null) return;
            material.shader = urp;
            Debug.Log("[BlenderImportPostprocessor] " + material.name + " → URP/Lit");
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
    }
}
#endif
