using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Sprint 11 Lane 5 — ROOT-CAUSE permanent fix for the persistent
    /// "[CleanMissingScripts] Removing missing script from: Moon1_Systems"
    /// console spam.
    ///
    /// The orphan classes (Moon1NPCSpawner, Moon1AmbientCreatures,
    /// Moon1MaterialSetup, Moon1HeroBuildingSpawner) were deleted from the
    /// Tartaria.Integration assembly, but four "!u!114 MonoBehaviour" entries
    /// on the Moon1_Systems GameObject (and four matching "!u!115 MonoScript"
    /// stubs *embedded inside the scene YAML*) survived.
    ///
    /// CleanMissingScripts.cs only treats the !u!114 side, and only when
    /// Unity reports the runtime Component as null. When the inline !u!115
    /// stub is present, Unity sometimes resolves the m_Script fileID at
    /// YAML-load time, so the !u!114 ref is not flagged as missing and
    /// nothing gets stripped — but the class still doesn't exist so the
    /// "missing script" warning fires every domain reload anyway.
    ///
    /// This menu item performs three passes:
    ///   1. Open every prefab under Assets/_Project/Prefabs/Moon1 in
    ///      PrefabUtility, run GameObjectUtility.RemoveMonoBehavioursWithMissingScript
    ///      on every transform, and save dirty prefabs.
    ///   2. Open the currently active scene (or Echohaven if none),
    ///      do the same to every root + descendant.
    ///   3. Read the saved scene/prefab YAML on disk and surgically strip
    ///      any residual orphan blocks whose embedded MonoScript references
    ///      one of the dead Moon1 classes by name. This catches the inline
    ///      !u!115 case that the managed API can't reach.
    ///
    /// Menu: Tartaria/8 Fix/Deep-Clean Moon1_Systems Prefab
    /// </summary>
    public static class Moon1SystemsPrefabDeepClean
    {
        // The four classes whose definitions were deleted but whose
        // references survive in scene + prefab YAML.
        static readonly string[] OrphanClassNames =
        {
            "Moon1NPCSpawner",
            "Moon1AmbientCreatures",
            "Moon1MaterialSetup",
            "Moon1HeroBuildingSpawner",
        };

        const string Moon1PrefabRoot = "Assets/_Project/Prefabs/Moon1";
        const string EchohavenScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";

        [MenuItem("Tartaria/8 Fix/Deep-Clean Moon1_Systems Prefab", priority = 820)]
        public static void DeepCleanAll()
        {
            int totalManagedRemoved = 0;
            int totalYamlBlocksRemoved = 0;
            int prefabsTouched = 0;
            int scenesTouched = 0;

            Debug.Log("[Moon1SystemsDeepClean] === START ===");

            // ---- PASS 1: Prefabs under Moon1/ ----
            string[] prefabGuids = AssetDatabase.FindAssets(
                "t:Prefab",
                new[] { Moon1PrefabRoot });

            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;

                int managedRemoved = CleanPrefabManaged(path);
                int yamlRemoved = CleanYamlOrphans(path);
                int subTotal = managedRemoved + yamlRemoved;

                if (subTotal > 0)
                {
                    Debug.Log($"[Moon1SystemsDeepClean] Removed {subTotal} orphans from {path} (managed={managedRemoved}, yaml={yamlRemoved})");
                    prefabsTouched++;
                    totalManagedRemoved += managedRemoved;
                    totalYamlBlocksRemoved += yamlRemoved;
                }
            }

            // ---- PASS 2: Active scene + Echohaven ----
            // Capture currently-open scene path so we can restore later.
            var activeScene = SceneManager.GetActiveScene();
            string activeScenePath = activeScene.IsValid() ? activeScene.path : null;

            scenesTouched += CleanSceneAtPath(EchohavenScenePath,
                out int echoManaged, out int echoYaml);
            totalManagedRemoved += echoManaged;
            totalYamlBlocksRemoved += echoYaml;

            // If active scene is different and loaded, clean it too.
            if (!string.IsNullOrEmpty(activeScenePath)
                && activeScenePath != EchohavenScenePath
                && File.Exists(activeScenePath))
            {
                scenesTouched += CleanSceneAtPath(activeScenePath,
                    out int aManaged, out int aYaml);
                totalManagedRemoved += aManaged;
                totalYamlBlocksRemoved += aYaml;
            }

            // ---- Final summary ----
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Moon1SystemsDeepClean] === DONE === " +
                $"prefabsTouched={prefabsTouched} " +
                $"scenesTouched={scenesTouched} " +
                $"managedRemoved={totalManagedRemoved} " +
                $"yamlBlocksRemoved={totalYamlBlocksRemoved}");

            if (totalManagedRemoved == 0 && totalYamlBlocksRemoved == 0)
            {
                Debug.Log("[Moon1SystemsDeepClean] 0 fixed — repo is clean. (Idempotent run.)");
            }
        }

        // -----------------------------------------------------------------
        // PASS 1 helpers — managed-API prefab cleanup
        // -----------------------------------------------------------------

        static int CleanPrefabManaged(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null) return 0;

            int removed = 0;
            try
            {
                removed = RemoveMissingFromHierarchy(root);
                if (removed > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return removed;
        }

        static int RemoveMissingFromHierarchy(GameObject root)
        {
            int total = 0;
            var stack = new Stack<Transform>();
            stack.Push(root.transform);
            while (stack.Count > 0)
            {
                var t = stack.Pop();
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                if (count > 0)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                    total += count;
                }
                foreach (Transform child in t)
                {
                    stack.Push(child);
                }
            }
            return total;
        }

        // -----------------------------------------------------------------
        // PASS 2 helpers — scene cleanup (managed + YAML)
        // -----------------------------------------------------------------

        static int CleanSceneAtPath(string scenePath, out int managedRemoved, out int yamlRemoved)
        {
            managedRemoved = 0;
            yamlRemoved = 0;

            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[Moon1SystemsDeepClean] Scene not found: {scenePath}");
                return 0;
            }

            // Prompt-save any unsaved edits before we switch scenes.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Moon1SystemsDeepClean] User cancelled save prompt — aborting scene pass.");
                return 0;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[Moon1SystemsDeepClean] Failed to open scene: {scenePath}");
                return 0;
            }

            foreach (var go in scene.GetRootGameObjects())
            {
                managedRemoved += RemoveMissingFromHierarchy(go);
            }

            if (managedRemoved > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Moon1SystemsDeepClean] Removed {managedRemoved} orphans (managed) from scene {scenePath}");
            }

            // YAML pass on the saved-to-disk file.
            yamlRemoved = CleanYamlOrphans(scenePath);
            if (yamlRemoved > 0)
            {
                // YAML mutated on disk — force reimport so Unity reparses.
                AssetDatabase.ImportAsset(scenePath, ImportAssetOptions.ForceUpdate);
                Debug.Log($"[Moon1SystemsDeepClean] Removed {yamlRemoved} orphan YAML blocks from scene {scenePath}");
            }

            return (managedRemoved + yamlRemoved) > 0 ? 1 : 0;
        }

        // -----------------------------------------------------------------
        // YAML surgery — works on .unity scenes and .prefab files
        // -----------------------------------------------------------------
        // Strategy:
        //   1. Read file. If it doesn't contain any orphan class name, skip.
        //   2. Parse into YAML "documents" — each --- !u!N &fileID delimited block.
        //   3. Find all !u!115 MonoScript blocks whose m_ClassName is one of
        //      the orphan classes. Record their fileIDs.
        //   4. Find all !u!114 MonoBehaviour blocks whose m_Script.fileID is
        //      one of the orphan MonoScript fileIDs. Record their fileIDs.
        //   5. Remove the !u!115 and !u!114 blocks.
        //   6. For every !u!1 GameObject block, prune m_Component lines that
        //      reference removed fileIDs.
        //   7. Write back. Idempotent — second run finds nothing.
        // -----------------------------------------------------------------

        static int CleanYamlOrphans(string filePath)
        {
            if (!File.Exists(filePath)) return 0;

            string text = File.ReadAllText(filePath);

            bool mentionsOrphan = false;
            foreach (var name in OrphanClassNames)
            {
                if (text.Contains("m_ClassName: " + name))
                {
                    mentionsOrphan = true;
                    break;
                }
            }
            if (!mentionsOrphan) return 0;

            // Split into header + documents
            // The first two lines are typically "%YAML 1.1" + "%TAG !u! ..."
            // Documents start with "--- !u!NN &FILEID"
            var lines = text.Replace("\r\n", "\n").Split('\n');
            var headerLines = new List<string>();
            var documents = new List<YamlDoc>();
            YamlDoc current = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("--- !u!"))
                {
                    if (current != null) documents.Add(current);
                    current = new YamlDoc { Header = line, Body = new List<string>() };
                }
                else
                {
                    if (current == null) headerLines.Add(line);
                    else current.Body.Add(line);
                }
            }
            if (current != null) documents.Add(current);

            // Step 1: find orphan MonoScript fileIDs (!u!115)
            var orphanScriptFileIds = new HashSet<string>();
            var headerRe = new Regex(@"^--- !u!(\d+) &(\d+)");
            foreach (var doc in documents)
            {
                var m = headerRe.Match(doc.Header);
                if (!m.Success) continue;
                string typeId = m.Groups[1].Value;
                string fileId = m.Groups[2].Value;
                if (typeId != "115") continue;
                // Look for m_ClassName in body
                foreach (var b in doc.Body)
                {
                    string trimmed = b.Trim();
                    foreach (var orphan in OrphanClassNames)
                    {
                        if (trimmed == "m_ClassName: " + orphan)
                        {
                            orphanScriptFileIds.Add(fileId);
                            break;
                        }
                    }
                }
            }

            // Step 2: find orphan MonoBehaviour fileIDs (!u!114) whose
            // m_Script.fileID is in orphanScriptFileIds.
            var orphanMbFileIds = new HashSet<string>();
            var scriptRe = new Regex(@"m_Script:\s*\{fileID:\s*(-?\d+)");
            foreach (var doc in documents)
            {
                var m = headerRe.Match(doc.Header);
                if (!m.Success) continue;
                string typeId = m.Groups[1].Value;
                string fileId = m.Groups[2].Value;
                if (typeId != "114") continue;
                foreach (var b in doc.Body)
                {
                    var sm = scriptRe.Match(b);
                    if (sm.Success && orphanScriptFileIds.Contains(sm.Groups[1].Value))
                    {
                        orphanMbFileIds.Add(fileId);
                        break;
                    }
                }
            }

            if (orphanScriptFileIds.Count == 0 && orphanMbFileIds.Count == 0)
                return 0;

            var allRemovedFileIds = new HashSet<string>(orphanScriptFileIds);
            allRemovedFileIds.UnionWith(orphanMbFileIds);

            // Step 3: drop the documents themselves.
            var keptDocs = new List<YamlDoc>();
            int removedDocCount = 0;
            foreach (var doc in documents)
            {
                var m = headerRe.Match(doc.Header);
                if (!m.Success)
                {
                    keptDocs.Add(doc);
                    continue;
                }
                string fileId = m.Groups[2].Value;
                if (allRemovedFileIds.Contains(fileId))
                {
                    removedDocCount++;
                    continue;
                }
                keptDocs.Add(doc);
            }

            // Step 4: prune m_Component lines that point at removed fileIDs.
            var componentRe = new Regex(@"^(\s*)- component:\s*\{fileID:\s*(-?\d+)\}\s*$");
            foreach (var doc in keptDocs)
            {
                for (int i = doc.Body.Count - 1; i >= 0; i--)
                {
                    var cm = componentRe.Match(doc.Body[i]);
                    if (cm.Success && allRemovedFileIds.Contains(cm.Groups[2].Value))
                    {
                        doc.Body.RemoveAt(i);
                    }
                }
            }

            // Step 5: rebuild and write.
            var sb = new StringBuilder();
            for (int i = 0; i < headerLines.Count; i++)
            {
                sb.Append(headerLines[i]);
                if (i < headerLines.Count - 1) sb.Append('\n');
            }
            // ensure a newline before first doc if there were header lines
            if (headerLines.Count > 0 && !sb.ToString().EndsWith("\n"))
            {
                sb.Append('\n');
            }
            for (int d = 0; d < keptDocs.Count; d++)
            {
                sb.Append(keptDocs[d].Header).Append('\n');
                for (int i = 0; i < keptDocs[d].Body.Count; i++)
                {
                    sb.Append(keptDocs[d].Body[i]);
                    sb.Append('\n');
                }
            }

            // Write atomically.
            string tempPath = filePath + ".moon1deepclean.tmp";
            File.WriteAllText(tempPath, sb.ToString(), new UTF8Encoding(false));
            File.Copy(tempPath, filePath, overwrite: true);
            File.Delete(tempPath);

            return removedDocCount;
        }

        class YamlDoc
        {
            public string Header;
            public List<string> Body;
        }
    }
}
