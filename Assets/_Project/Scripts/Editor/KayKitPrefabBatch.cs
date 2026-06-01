#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// KayKitPrefabBatch — walks all KayKit vendor FBX packs and generates a prefab
    /// variant for each one under Assets/_Project/Prefabs/KayKit/{Pack}/{relative}/{Name}.prefab
    /// preserving folder structure.
    ///
    /// Vendor packs contain 426 raw .fbx files but ZERO .prefab files. The
    /// EchohavenContentSpawner / BuildingSpawner / Moon1WireSpawnerPrefabs arrays
    /// need .prefab assets to wire via SerializedObject. This tool closes that gap.
    ///
    /// Idempotent — skips FBX whose .prefab variant already exists, unless -Force.
    /// </summary>
    public static class KayKitPrefabBatch
    {
        static readonly string[] KayKitFolders = {
            "Assets/KayKit_Adventurers_2.0_FREE",
            "Assets/KayKit_Skeletons_1.1_FREE",
            "Assets/KayKit_Forest_Nature_Pack_1.0_FREE",
            "Assets/KayKit_RPGToolsBits_1.0_FREE",
        };

        const string OutputRoot = "Assets/_Project/Prefabs/KayKit";

        [MenuItem("Tartaria/1 Build/Generate KayKit Prefab Variants (all 426 FBX)", priority = 60)]
        public static void GenerateAll() => Generate(force: false);

        [MenuItem("Tartaria/1 Build/Regenerate KayKit Prefab Variants (FORCE overwrite)", priority = 61)]
        public static void RegenerateAll() => Generate(force: true);

        static void Generate(bool force)
        {
            int created = 0, skipped = 0, failed = 0;
            var perPack = new Dictionary<string, int>();

            EnsureFolder(OutputRoot);

            foreach (var pack in KayKitFolders)
            {
                if (!AssetDatabase.IsValidFolder(pack))
                {
                    Debug.LogWarning($"[KayKitPrefabBatch] Pack missing on disk: {pack}");
                    continue;
                }

                string packName = Path.GetFileName(pack); // e.g. "KayKit_Adventurers_2.0_FREE"
                perPack[packName] = 0;

                // Find every FBX under this pack
                var fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { pack });
                int total = fbxGuids.Length;
                int idx = 0;

                foreach (var guid in fbxGuids)
                {
                    idx++;
                    string fbxPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!fbxPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;

                    EditorUtility.DisplayProgressBar(
                        "KayKit Prefab Batch",
                        $"{packName}  ({idx}/{total})  {Path.GetFileName(fbxPath)}",
                        (float)idx / Mathf.Max(1, total));

                    // Build output path mirroring the FBX's relative folder structure
                    string rel = fbxPath.Substring(pack.Length).TrimStart('/', '\\');
                    string relFolder = Path.GetDirectoryName(rel)?.Replace('\\', '/') ?? "";
                    string fbxName = Path.GetFileNameWithoutExtension(fbxPath);

                    string outFolder = $"{OutputRoot}/{packName}";
                    if (!string.IsNullOrEmpty(relFolder)) outFolder += "/" + relFolder;
                    string outPath = $"{outFolder}/{fbxName}.prefab";

                    if (File.Exists(outPath) && !force) { skipped++; continue; }

                    EnsureFolderRecursive(outFolder);

                    var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                    if (fbx == null) { failed++; continue; }

                    // Instantiate, save as flat prefab (not a variant — so spawner arrays
                    // can hold a stable GameObject ref independent of FBX reimport).
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
                    if (go == null) { failed++; continue; }

                    PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    go.name = fbxName;

                    var saved = PrefabUtility.SaveAsPrefabAsset(go, outPath, out bool success);
                    Object.DestroyImmediate(go);

                    if (success && saved != null) { created++; perPack[packName]++; }
                    else { failed++; }
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var lines = new List<string> { $"Created: {created}", $"Skipped (already exist): {skipped}", $"Failed: {failed}", "" };
            foreach (var kv in perPack) lines.Add($"  {kv.Key}: {kv.Value} new");

            EditorUtility.DisplayDialog("KayKit Prefab Batch",
                string.Join("\n", lines) + "\n\nOutput root:\n" + OutputRoot,
                "OK");
        }

        // ─── folder helpers ─────────────────────────────────────────────────────

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string leaf = Path.GetFileName(folder);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf)) return;
            EnsureFolderRecursive(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        static void EnsureFolderRecursive(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string leaf = Path.GetFileName(folder);
            if (string.IsNullOrEmpty(parent)) return;
            EnsureFolderRecursive(parent);
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
