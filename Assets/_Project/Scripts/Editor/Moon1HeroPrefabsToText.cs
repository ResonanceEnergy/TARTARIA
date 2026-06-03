// Hammer Lane 8 — Convert hero building prefabs from binary/corrupt to text serialization.
//
// Sprint 11 audit flagged 3 hero prefabs as "binary-serialized so PrefabUtility.LoadPrefabContents
// fails". Empirical inspection in Unity 6000.3.6f1 revealed they were not merely binary text — they
// were SerializedFile blobs with NO root GameObject (Unity reports them as BrokenPrefabAsset with
// the message ".prefab file is not a prefab. There are no GameObjects in the file."). The 130-byte
// LFS-pointer hypothesis from the audit also doesn't apply — the files were 213-229 KB on disk.
// Best guess: they were emitted by an earlier authoring step (BuildingSpawner runtime serializer
// or a prior agent's PrefabUtility misuse) that wrote primitive Detail_* meshes as raw asset
// records without a GameObject hierarchy wrapper.
//
// Because both Options A (LoadPrefabContents) and B (LoadAssetAtPath<GameObject>+InstantiatePrefab)
// fail on a no-root SerializedFile, the only working path is to rebuild from scratch:
//   1. Build a minimal GameObject in memory with the building's name and identity transform.
//   2. SaveAsPrefabAsset to a path under the project (writes %YAML 1.1 text because Force Text).
//   3. The newly-written prefab has GameObject + Transform root, ready for P5.L3 mesh-replace
//      to call NewKitParent() + InstantiateKitChild() to populate Cathedral kit pieces.
//
// The original .meta file's GUID is preserved (we overwrite the .prefab payload only, not the
// .meta), so any scene references to these prefabs remain intact.
//
// Files written:
//   Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_CrystalSpire.prefab
//   Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_StarDome.prefab
//   Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_HarmonicFountain.prefab
//
// Menu: Tartaria/8 Fix/Convert Hero Prefabs to Text Mode (priority 850)
//
// NOTE FOR P5.L3 (Moon1HeroBuildingMeshReplace): after this menu runs, each prefab has root
// GameObject {name} + Transform with childCount = 0. Your CountAndRemoveDetails iterates
// children — it will count 0 primitives, remove nothing. Then NewKitParent attaches the
// kit container. That's the intended interaction.

using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TartariaEditor.Sprint11
{
    public static class Moon1HeroPrefabsToText
    {
        const string CrystalSpirePath     = "Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_CrystalSpire.prefab";
        const string StarDomePath         = "Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_StarDome.prefab";
        const string HarmonicFountainPath = "Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_HarmonicFountain.prefab";

        static readonly (string path, string rootName)[] Targets = new (string, string)[]
        {
            (CrystalSpirePath,     "Echohaven_CrystalSpire"),
            (StarDomePath,         "Echohaven_StarDome"),
            (HarmonicFountainPath, "Echohaven_HarmonicFountain"),
        };

        [MenuItem("Tartaria/8 Fix/Convert Hero Prefabs to Text Mode", priority = 850)]
        public static void Run()
        {
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                Debug.LogError(
                    "[Moon1HeroPrefabsToText] EditorSettings.serializationMode is " +
                    EditorSettings.serializationMode +
                    " — needs ForceText (project setting m_SerializationMode: 2). Aborting.");
                return;
            }

            int converted = 0;
            int alreadyHealthy = 0;
            int failed = 0;
            var sb = new StringBuilder();
            sb.AppendLine("[Moon1HeroPrefabsToText] Results:");

            foreach (var t in Targets)
            {
                string path = t.path;
                string rootName = t.rootName;

                if (!File.Exists(path))
                {
                    sb.AppendLine("  MISSING: " + path);
                    failed++;
                    continue;
                }

                long beforeBytes = new FileInfo(path).Length;

                // Health check: a healthy text prefab loads as GameObject via AssetDatabase.
                bool isTextYaml = StartsWithYamlPreamble(path);
                bool hasRootGameObject = false;
                if (isTextYaml)
                {
                    var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    hasRootGameObject = existing != null;
                }

                if (isTextYaml && hasRootGameObject)
                {
                    sb.AppendLine($"  HEALTHY: {Path.GetFileName(path)} ({beforeBytes} bytes) — text YAML, root GameObject loads. Skipping.");
                    alreadyHealthy++;
                    continue;
                }

                try
                {
                    // Rebuild: minimal GameObject + Transform; SaveAsPrefabAsset emits text YAML.
                    var root = new GameObject(rootName);
                    root.transform.localPosition = Vector3.zero;
                    root.transform.localRotation = Quaternion.identity;
                    root.transform.localScale    = Vector3.one;

                    try
                    {
                        var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
                        if (saved == null)
                        {
                            sb.AppendLine("  FAIL: SaveAsPrefabAsset returned null for " + path);
                            failed++;
                            continue;
                        }
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(root);
                    }

                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                    long afterBytes = new FileInfo(path).Length;
                    bool nowText = StartsWithYamlPreamble(path);
                    bool nowLoads = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
                    if (nowText && nowLoads)
                    {
                        sb.AppendLine($"  REBUILT: {Path.GetFileName(path)} {beforeBytes} -> {afterBytes} bytes (text YAML, root GameObject ok)");
                        converted++;
                    }
                    else
                    {
                        sb.AppendLine($"  FAIL: {Path.GetFileName(path)} {beforeBytes} -> {afterBytes} bytes (nowText={nowText}, nowLoads={nowLoads})");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  FAIL ({ex.GetType().Name}): {Path.GetFileName(path)}: {ex.Message}");
                    failed++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            sb.AppendLine($"Totals: rebuilt={converted}, alreadyHealthy={alreadyHealthy}, failed={failed}");
            if (failed > 0) Debug.LogError(sb.ToString());
            else Debug.Log(sb.ToString());
        }

        static bool StartsWithYamlPreamble(string path)
        {
            try
            {
                using (var fs = File.OpenRead(path))
                {
                    var buf = new byte[5];
                    int read = fs.Read(buf, 0, 5);
                    if (read < 5) return false;
                    return buf[0] == (byte)'%' && buf[1] == (byte)'Y' && buf[2] == (byte)'A' && buf[3] == (byte)'M' && buf[4] == (byte)'L';
                }
            }
            catch
            {
                return fal