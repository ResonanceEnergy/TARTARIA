// File: Assets/_Project/Scripts/Editor/KayKit_GenerateForestPrefabs.cs
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
                    var fbxPath = fbxFiles[i].Replace("\\", "/"); var name = Path.GetFileNameWithoutExtension(fbxPath); var prefabPath = $"{DEST_DIR}/KayKit_{name}.prefab";  EditorUtility.DisplayProgressBar("KayKit Forest", $"{name}", (float)i / fbxFiles.Length); 

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
