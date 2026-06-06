// PBRResourceCopier.cs
// Mirrors Assets/_Project/Materials/PBR/*.mat into Assets/_Project/Resources/PBR/
// so RuntimePBRApplier can load them at runtime via Resources.LoadAll<Material>("PBR").
// Also adds a RuntimePBRApplier component to the active scene.
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    public static class PBRResourceCopier
    {
        const string SrcDir = "Assets/_Project/Materials/PBR";
        const string DstDir = "Assets/_Project/Resources/PBR";

        public static void MirrorAndAttach()
        {
            if (!AssetDatabase.IsValidFolder(SrcDir)) return;
            EnsureFolder(DstDir);

            int copied = 0;
            foreach (var src in Directory.GetFiles(SrcDir, "*.mat"))
            {
                var name = Path.GetFileName(src);
                var dst = $"{DstDir}/{name}";
                if (!File.Exists(dst))
                {
                    AssetDatabase.CopyAsset(src.Replace("\\", "/"), dst);
                    copied++;
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[PBRResourceCopier] Mirrored {copied} new mats into {DstDir}.");

            // Attach RuntimePBRApplier to active scene root.
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid()) return;

            bool found = false;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.GetComponentInChildren<RuntimePBRApplier>() != null) { found = true; break; }
            }
            if (!found)
            {
                var host = new GameObject("RuntimePBRApplier");
                host.AddComponent<RuntimePBRApplier>();
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log("[PBRResourceCopier] Added RuntimePBRApplier to scene.");
            }
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
