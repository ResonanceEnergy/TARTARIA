using System.IO;
using Tartaria.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Sprint 6 Lane 7 - Editor menu that authors the 3 canonical
    /// DifficultyProfile assets and mirrors them into Resources/ so
    /// DifficultyController can Resources.Load at runtime.
    ///
    /// Menu: Tartaria/Gameplay/Build Difficulty Profiles
    ///
    /// Spec values (CLAUDE.md mandate):
    ///   Story    (0): 0.6 / 0.6 / 1.3 / 1.5
    ///   Standard (1): 1.0 / 1.0 / 1.0 / 1.0
    ///   Hardened (2): 1.5 / 1.4 / 0.7 / 0.7
    /// </summary>
    public static class DifficultyProfileBuilder
    {
        const string DataFolder = "Assets/_Project/Data/Difficulty";
        const string ResourcesFolder = "Assets/_Project/Resources/Difficulty";

        [MenuItem("Tartaria/Gameplay/Build Difficulty Profiles")]
        public static void BuildAll()
        {
            EnsureFolder(DataFolder);
            EnsureFolder(ResourcesFolder);

            BuildOne("Story",    0, 0.6f, 0.6f, 1.3f, 1.5f);
            BuildOne("Standard", 1, 1.0f, 1.0f, 1.0f, 1.0f);
            BuildOne("Hardened", 2, 1.5f, 1.4f, 0.7f, 0.7f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DifficultyProfileBuilder] Wrote Story / Standard / Hardened profiles + Resources mirror.");
        }

        static void BuildOne(string displayName, int idx, float hp, float dmg, float forgive, float aether)
        {
            string dataPath = DataFolder + "/" + displayName + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<DifficultyProfile>(dataPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DifficultyProfile>();
                AssetDatabase.CreateAsset(asset, dataPath);
            }
            asset.EditorAuthor(displayName, idx, hp, dmg, forgive, aether);
            EditorUtility.SetDirty(asset);

            string resPath = ResourcesFolder + "/" + displayName + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<DifficultyProfile>(resPath);
            if (existing == null)
            {
                AssetDatabase.CopyAsset(dataPath, resPath);
            }
            else
            {
                existing.EditorAuthor(displayName, idx, hp, dmg, forgive, aether);
                EditorUtility.SetDirty(existing);
            }
        }

        static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
            string leaf = Path.GetFileName(folderPath);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
