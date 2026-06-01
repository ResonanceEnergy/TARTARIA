#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 cathedral ASSEMBLY builder.
    ///
    /// Distinct from Moon1CathedralKitDressing (which dresses the scene with the
    /// kit pieces directly). This tool builds a single composite prefab named
    /// "CathedralAssembly" with 6 named child group transforms in restoration
    /// order: Foundation, Walls, Roof, Buttresses, RoseWindow, Spire.
    ///
    /// Each KayKit cathedral piece found under
    /// Assets/_Project/Prefabs/Moon1/Cathedral/ is instantiated as a child of
    /// the matching group based on a case-insensitive filename substring match.
    /// Every part is given a MeshCollider so the runtime restoration system can
    /// raycast individual pieces and dissolve them group-by-group.
    ///
    /// Menus:
    ///   Tartaria/7 Level/Build Cathedral Assembly    -- builds + saves prefab
    ///   Tartaria/7 Level/Place Cathedral In Scene    -- swaps scene Cathedral
    ///
    /// Idempotent: rebuilding overwrites the prefab asset in-place.
    /// </summary>
    public static class Moon1CathedralAssembler
    {
        private const string KitFolder = "Assets/_Project/Prefabs/Moon1/Cathedral";
        private const string OutputFolder = "Assets/_Project/Prefabs/Moon1/Cathedral";
        private const string OutputAssetPath = "Assets/_Project/Prefabs/Moon1/Cathedral/CathedralAssembly.prefab";

        // Six restoration groups, ordered bottom-up so vertical stacking is
        // straightforward and matches the lore-driven "rebuild from ground to
        // spire" narrative beat in docs/15_MVP_BUILD_SPEC.md.
        private static readonly string[] GroupNames =
        {
            "Foundation",
            "Walls",
            "Roof",
            "Buttresses",
            "RoseWindow",
            "Spire",
        };

        // Group-relative base height for the first piece placed in that group.
        // Pieces within a group are arranged horizontally (ring or row); the
        // group's local Y sets its vertical band on the cathedral silhouette.
        private static readonly Dictionary<string, float> GroupBaseHeight = new Dictionary<string, float>
        {
            { "Foundation", 0.0f },
            { "Walls",      2.0f },
            { "Roof",       6.0f },
            { "Buttresses", 2.0f },
            { "RoseWindow", 5.5f },
            { "Spire",      9.0f },
        };

        // Substring classifier (case-insensitive). First match wins, so order
        // matters: more specific substrings come before generic ones.
        // "buttress" before "wall" prevents a hypothetical "Wall_Buttress.prefab"
        // from being mis-grouped into Walls.
        private static readonly (string substring, string group)[] Classifier =
        {
            ("foundation", "Foundation"),
            ("buttress",   "Buttresses"),
            ("rose",       "RoseWindow"),
            ("spire",      "Spire"),
            ("dome",       "Roof"),
            ("roof",       "Roof"),
            ("window",     "RoseWindow"),
            ("wall",       "Walls"),
            ("column",     "Walls"),
            ("door",       "Walls"),
            ("arch",       "Walls"),
        };

        [MenuItem("Tartaria/7 Level/Build Cathedral Assembly", false, 70)]
        public static void BuildCathedralAssembly()
        {
            // ---- Discover kit prefabs --------------------------------------
            if (!AssetDatabase.IsValidFolder(KitFolder))
            {
                string msg = $"Kit folder not found at '{KitFolder}'. Cannot build CathedralAssembly.";
                Debug.LogError($"[Moon1CathedralAssembler] {msg}");
                EditorUtility.DisplayDialog("Cathedral Assembly", msg, "OK");
                return;
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { KitFolder });
            List<string> kitPaths = new List<string>();
            foreach (string guid in prefabGuids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                // Skip our own output asset to avoid recursion.
                if (p.Replace('\\', '/').Equals(OutputAssetPath, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                kitPaths.Add(p);
            }

            if (kitPaths.Count == 0)
            {
                string msg = $"No kit prefabs found under '{KitFolder}'. Cannot build CathedralAssembly.";
                Debug.LogError($"[Moon1CathedralAssembler] {msg}");
                EditorUtility.DisplayDialog("Cathedral Assembly", msg, "OK");
                return;
            }

            // ---- Bucket kit prefabs by group -------------------------------
            Dictionary<string, List<string>> buckets = new Dictionary<string, List<string>>();
            foreach (string g in GroupNames)
            {
                buckets[g] = new List<string>();
            }

            List<string> unclassified = new List<string>();
            foreach (string p in kitPaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(p).ToLowerInvariant();
                string group = null;
                foreach (var entry in Classifier)
                {
                    if (fileName.Contains(entry.substring))
                    {
                        group = entry.group;
                        break;
                    }
                }
                if (group != null)
                {
                    buckets[group].Add(p);
                }
                else
                {
                    unclassified.Add(p);
                }
            }

            // ---- Build the assembly root in a temp scene-less hierarchy ---
            GameObject root = new GameObject("CathedralAssembly");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;

            List<string> missingGroups = new List<string>();
            int totalPlaced = 0;

            foreach (string groupName in GroupNames)
            {
                GameObject groupGo = new GameObject(groupName);
                groupGo.transform.SetParent(root.transform, false);
                groupGo.transform.localPosition = new Vector3(0f, GroupBaseHeight[groupName], 0f);
                groupGo.transform.localRotation = Quaternion.identity;

                List<string> members = buckets[groupName];
                if (members.Count == 0)
                {
                    missingGroups.Add(groupName);
                    Debug.LogWarning(
                        $"[Moon1CathedralAssembler] Group '{groupName}' has 0 kit prefabs " +
                        $"matching its classifier substrings. Empty group transform created so " +
                        $"future asset drops slot in cleanly.");
                    continue;
                }

                // Arrange members in a circle around the group's local origin so
                // groups with multiple pieces (Walls, Roof dome segments) form a
                // visually plausible ring. Single-member groups (Foundation,
                // RoseWindow, Spire base) sit at the origin.
                int count = members.Count;
                float radius = (count > 1) ? RadiusForGroup(groupName) : 0f;
                for (int i = 0; i < count; i++)
                {
                    string path = members[i];
                    GameObject src = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (src == null)
                    {
                        Debug.LogWarning($"[Moon1CathedralAssembler] Failed to load prefab at '{path}'.");
                        continue;
                    }

                    GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(src, groupGo.transform);
                    if (inst == null)
                    {
                        Debug.LogWarning($"[Moon1CathedralAssembler] InstantiatePrefab returned null for '{path}'.");
                        continue;
                    }

                    Vector3 localPos;
                    Quaternion localRot;
                    if (count > 1)
                    {
                        float angleDeg = (360f / count) * i;
                        float angleRad = angleDeg * Mathf.Deg2Rad;
                        localPos = new Vector3(
                            Mathf.Sin(angleRad) * radius,
                            VerticalStackOffset(groupName, i),
                            Mathf.Cos(angleRad) * radius);
                        // Face outward from ring center.
                        Vector3 outward = localPos;
                        outward.y = 0f;
                        localRot = (outward.sqrMagnitude > 0.0001f)
                            ? Quaternion.LookRotation(-outward.normalized, Vector3.up)
                            : Quaternion.identity;
                    }
                    else
                    {
                        localPos = new Vector3(0f, VerticalStackOffset(groupName, i), 0f);
                        localRot = Quaternion.identity;
                    }
                    inst.transform.localPosition = localPos;
                    inst.transform.localRotation = localRot;

                    EnsureMeshColliders(inst);
                    totalPlaced++;
                }
            }

            // ---- Save as prefab asset (idempotent overwrite) --------------
            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
                AssetDatabase.Refresh();
            }

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, OutputAssetPath, out bool success);
            Object.DestroyImmediate(root);

            if (!success || savedPrefab == null)
            {
                string msg = $"PrefabUtility.SaveAsPrefabAsset failed for '{OutputAssetPath}'.";
                Debug.LogError($"[Moon1CathedralAssembler] {msg}");
                EditorUtility.DisplayDialog("Cathedral Assembly", msg, "OK");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // ---- Report ----------------------------------------------------
            string summary =
                $"Cathedral assembly built.\n\n" +
                $"Prefab: {OutputAssetPath}\n" +
                $"Pieces placed: {totalPlaced}\n" +
                $"Groups empty: {(missingGroups.Count == 0 ? "(none)" : string.Join(", ", missingGroups))}\n" +
                $"Unclassified prefabs: {(unclassified.Count == 0 ? "(none)" : string.Join(", ", unclassified.Select(Path.GetFileName)))}";

            Debug.Log("[Moon1CathedralAssembler] " + summary.Replace("\n", " | "));

            if (missingGroups.Count > 0 || unclassified.Count > 0)
            {
                EditorUtility.DisplayDialog("Cathedral Assembly (with warnings)", summary, "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Cathedral Assembly", summary, "OK");
            }

            // Ping the prefab in the Project window so it's easy to find.
            EditorGUIUtility.PingObject(savedPrefab);
        }

        [MenuItem("Tartaria/7 Level/Place Cathedral In Scene", false, 71)]
        public static void PlaceCathedralInScene()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OutputAssetPath);
            if (prefab == null)
            {
                string msg =
                    $"CathedralAssembly prefab not found at '{OutputAssetPath}'. " +
                    $"Run 'Tartaria/7 Level/Build Cathedral Assembly' first.";
                Debug.LogError($"[Moon1CathedralAssembler] {msg}");
                EditorUtility.DisplayDialog("Place Cathedral", msg, "OK");
                return;
            }

            // ---- Find existing Cathedral in scene (case-insensitive name match) ----
            GameObject existing = FindCathedralInActiveScene();
            Vector3 targetPos = Vector3.zero;
            Quaternion targetRot = Quaternion.identity;
            Transform parent = null;

            if (existing != null)
            {
                targetPos = existing.transform.position;
                targetRot = existing.transform.rotation;
                parent = existing.transform.parent;
                Debug.Log(
                    $"[Moon1CathedralAssembler] Replacing existing '{existing.name}' at {targetPos}.");
                Undo.DestroyObjectImmediate(existing);
            }
            else
            {
                Debug.LogWarning(
                    "[Moon1CathedralAssembler] No 'Cathedral' GameObject found in active scene. " +
                    "Placing CathedralAssembly at world origin.");
            }

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (inst == null)
            {
                Debug.LogError("[Moon1CathedralAssembler] InstantiatePrefab returned null for CathedralAssembly.");
                return;
            }
            Undo.RegisterCreatedObjectUndo(inst, "Place Cathedral Assembly");
            inst.transform.SetParent(parent, worldPositionStays: false);
            inst.transform.position = targetPos;
            inst.transform.rotation = targetRot;

            EditorSceneManager.MarkSceneDirty(inst.scene);
            Selection.activeGameObject = inst;
            EditorGUIUtility.PingObject(inst);

            Debug.Log($"[Moon1CathedralAssembler] CathedralAssembly placed in scene at {targetPos}.");
        }

        // ----------------------------------------------------------------- helpers

        private static GameObject FindCathedralInActiveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            foreach (GameObject rootGo in scene.GetRootGameObjects())
            {
                GameObject hit = FindByNameContains(rootGo.transform, "cathedral");
                if (hit != null)
                {
                    return hit;
                }
            }
            return null;
        }

        private static GameObject FindByNameContains(Transform t, string needle)
        {
            if (t.name.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return t.gameObject;
            }
            for (int i = 0; i < t.childCount; i++)
            {
                GameObject hit = FindByNameContains(t.GetChild(i), needle);
                if (hit != null)
                {
                    return hit;
                }
            }
            return null;
        }

        private static float RadiusForGroup(string groupName)
        {
            switch (groupName)
            {
                case "Walls":       return 7.0f;
                case "Roof":        return 6.0f;
                case "Buttresses":  return 8.5f;
                case "RoseWindow":  return 0.0f;  // single piece centered
                case "Spire":       return 0.0f;  // stacked vertically
                default:            return 4.0f;
            }
        }

        private static float VerticalStackOffset(string groupName, int index)
        {
            // Spire pieces stack vertically (Base, Mid, Top) at 2m intervals.
            if (groupName == "Spire")
            {
                return index * 2.0f;
            }
            return 0f;
        }

        private static void EnsureMeshColliders(GameObject root)
        {
            // Adds a MeshCollider to every MeshFilter under this instance that
            // doesn't already have ANY Collider. Per-piece colliders enable
            // raycast hit-testing for piecewise restoration.
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            foreach (MeshFilter mf in filters)
            {
                if (mf == null || mf.sharedMesh == null)
                {
                    continue;
                }
                Collider existing = mf.GetComponent<Collider>();
                if (existing != null)
                {
                    continue;
                }
                MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = false;
            }
        }
    }
}
#endif
