# TICKET: KayKit Forest Nature Pack — generate Editor menu that creates prefab wrappers

## Output destination
`Assets/_Project/Scripts/Editor/KayKit_GenerateForestPrefabs.cs`

## Acceptance criteria
- Namespace: `Tartaria.Editor`
- Compiles against Unity 6 LTS, Editor-only assembly
- One C# file, one Editor class, brace-balanced
- Uses `[MenuItem("Tartaria/Asset Wrangling/Generate KayKit Forest Prefabs")]`
- When invoked, scans `Assets/KayKit_Forest_Nature_Pack_1.0_FREE/KayKit_Forest_Nature_Pack_1.0_FREE/Assets/fbx/`
- For each `.fbx` file:
  1. Create a prefab at `Assets/_Project/Prefabs/Vegetation/KayKit_<filename-without-extension>.prefab`
  2. Inside the prefab, instantiate the FBX as a child
  3. Apply a URP/Lit material with reasonable defaults (foliage = green-tinted, rocks = grey, mushrooms = warm tone — heuristic by filename keyword)
  4. Add a MeshCollider to the prefab root with `convex = false`
- Skip files that already have a prefab created (idempotent)
- Show progress bar via `EditorUtility.DisplayProgressBar`
- Report completion via `EditorUtility.DisplayDialog` with count of new prefabs created

## Spec

The FBX files exist at:
```
Assets/KayKit_Forest_Nature_Pack_1.0_FREE/KayKit_Forest_Nature_Pack_1.0_FREE/Assets/fbx/Bush_1_A_Color1.fbx
Assets/KayKit_Forest_Nature_Pack_1.0_FREE/KayKit_Forest_Nature_Pack_1.0_FREE/Assets/fbx/Bush_1_B_Color1.fbx
... 210 total
```

Filename keywords for material selection:
- contains "Bush" or "Tree" or "Branch" or "Plant" → green foliage material `_BaseColor = (0.30, 0.50, 0.20)`
- contains "Rock" or "Stone" or "Boulder" → grey rock material `_BaseColor = (0.55, 0.52, 0.50)`
- contains "Mushroom" → warm tone `_BaseColor = (0.62, 0.40, 0.30)`
- contains "Wood" or "Log" or "Stump" → brown wood `_BaseColor = (0.42, 0.28, 0.18)`
- contains "Grass" → bright green `_BaseColor = (0.35, 0.55, 0.18)`
- otherwise → neutral grey-green `_BaseColor = (0.45, 0.50, 0.40)`

Each prefab structure:
```
KayKit_Bush_1_A_Color1 (root GameObject)
├── [PrefabAsset reference to the .fbx imported model]
├── MeshCollider (convex=false)
```

## Sample skeleton

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Tartaria.Editor
{
    public static class KayKit_GenerateForestPrefabs
    {
        const string SOURCE_DIR = "Assets/KayKit_Forest_Nature_Pack_1.0_FREE/KayKit_Forest_Nature_Pack_1.0_FREE/Assets/fbx";
        const string DEST_DIR = "Assets/_Project/Prefabs/Vegetation";

        [MenuItem("Tartaria/Asset Wrangling/Generate KayKit Forest Prefabs")]
        public static void Run()
        {
            if (!Directory.Exists(SOURCE_DIR))
            {
                EditorUtility.DisplayDialog("Forest Pack Missing", $"Source dir not found: {SOURCE_DIR}", "OK");
                return;
            }
            if (!Directory.Exists(DEST_DIR)) Directory.CreateDirectory(DEST_DIR);

            var fbxFiles = Directory.GetFiles(SOURCE_DIR, "*.fbx");
            int created = 0, skipped = 0;
            try
            {
                for (int i = 0; i < fbxFiles.Length; i++)
                {
                    var fbxPath = fbxFiles[i].Replace("\\", "/");
                    var name = Path.GetFileNameWithoutExtension(fbxPath);
                    var prefabPath = $"{DEST_DIR}/KayKit_{name}.prefab";

                    EditorUtility.DisplayProgressBar("KayKit Forest", $"{name}", (float)i / fbxFiles.Length);

                    if (File.Exists(prefabPath)) { skipped++; continue; }

                    var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                    if (fbxAsset == null) continue;

                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
                    instance.name = $"KayKit_{name}";

                    // Add MeshCollider on the root if not already
                    if (instance.GetComponent<MeshCollider>() == null && instance.GetComponentInChildren<MeshFilter>() != null)
                    {
                        var mc = instance.AddComponent<MeshCollider>();
                        mc.convex = false;
                    }

                    // Apply URP material based on filename
                    ApplyMaterial(instance, name);

                    PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                    Object.DestroyImmediate(instance);
                    created++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("KayKit Forest Prefabs",
                $"Created: {created}\nSkipped (already existed): {skipped}\nDest: {DEST_DIR}", "OK");
        }

        static void ApplyMaterial(GameObject go, string name)
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) return;
            var c = new Color(0.45f, 0.50f, 0.40f);
            string lower = name.ToLowerInvariant();
            if (lower.Contains("bush") || lower.Contains("tree") || lower.Contains("branch") || lower.Contains("plant")) c = new Color(0.30f, 0.50f, 0.20f);
            else if (lower.Contains("rock") || lower.Contains("stone") || lower.Contains("boulder")) c = new Color(0.55f, 0.52f, 0.50f);
            else if (lower.Contains("mushroom")) c = new Color(0.62f, 0.40f, 0.30f);
            else if (lower.Contains("wood") || lower.Contains("log") || lower.Contains("stump")) c = new Color(0.42f, 0.28f, 0.18f);
            else if (lower.Contains("grass")) c = new Color(0.35f, 0.55f, 0.18f);

            var mat = new Material(urpLit);
            mat.SetColor("_BaseColor", c);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
        }
    }
}
```

## Do NOT
- Do not delete the FBX source files.
- Do not modify any non-Editor `.cs` file.
- Do not regenerate prefabs that already exist (the `File.Exists(prefabPath)` skip is required).
- Do not require the FBX to be re-imported (we use the existing import settings).
