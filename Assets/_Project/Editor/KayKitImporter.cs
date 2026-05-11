using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Imports the KayKit "Adventurers" FREE pack (chibi humanoids + shared
    /// Rig_Medium animation rigs) into the project's standard asset tree:
    ///   Models      → Assets/_Project/Models/Characters/KayKit/
    ///   Materials   → Assets/_Project/Materials/KayKit/
    ///   Prefabs     → Assets/_Project/Prefabs/Characters/KayKit/
    ///
    /// KayKit characters do NOT use a Mixamo-compatible humanoid rig — they share
    /// a custom "Rig_Medium" skeleton, so we configure them as Generic and skip
    /// HumanoidAutoBinder. Idempotent — silently no-ops when the source pack
    /// folder is missing or assets are already imported.
    /// </summary>
    public static class KayKitImporter
    {
        const string SrcRoot    = "Assets/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE";
        const string DstModels  = "Assets/_Project/Models/Characters/KayKit";
        const string DstMats    = "Assets/_Project/Materials/KayKit";
        const string DstPrefabs = "Assets/_Project/Prefabs/Characters/KayKit";

        static readonly string[] Characters = { "Knight", "Mage", "Rogue", "Ranger", "Barbarian" };

        [MenuItem("TARTARIA/Integration/Import KayKit Adventurers")]
        public static void ImportAllMenu() => ImportAll();

        public static void ImportAll()
        {
            if (!Directory.Exists(SrcRoot))
            {
                Debug.Log($"[KayKit] Source pack not present at {SrcRoot} — skipping.");
                return;
            }

            EnsureFolder(DstModels);
            EnsureFolder(DstMats);
            EnsureFolder(DstPrefabs);

            int copied = 0;

            // 1. Copy character FBXes + per-character textures.
            foreach (var c in Characters)
            {
                copied += CopyIfNew($"{SrcRoot}/Characters/fbx/{c}.fbx",
                                    $"{DstModels}/{c}.fbx");
                copied += CopyIfNew($"{SrcRoot}/Characters/fbx/{c.ToLowerInvariant()}_texture.png",
                                    $"{DstModels}/{c.ToLowerInvariant()}_texture.png");
            }
            // Hooded rogue variant.
            copied += CopyIfNew($"{SrcRoot}/Characters/fbx/Rogue_Hooded.fbx",
                                $"{DstModels}/Rogue_Hooded.fbx");

            // 2. Copy shared Rig_Medium animation FBXes.
            copied += CopyIfNew($"{SrcRoot}/Animations/fbx/Rig_Medium/Rig_Medium_General.fbx",
                                $"{DstModels}/Rig_Medium_General.fbx");
            copied += CopyIfNew($"{SrcRoot}/Animations/fbx/Rig_Medium/Rig_Medium_MovementBasic.fbx",
                                $"{DstModels}/Rig_Medium_MovementBasic.fbx");

            if (copied > 0)
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }

            // 3. Configure FBX importers (Generic rig, no embedded materials).
            foreach (var c in Characters) ConfigureCharacterFbx($"{DstModels}/{c}.fbx", isAnim: false);
            ConfigureCharacterFbx($"{DstModels}/Rogue_Hooded.fbx", isAnim: false);
            ConfigureCharacterFbx($"{DstModels}/Rig_Medium_General.fbx",       isAnim: true);
            ConfigureCharacterFbx($"{DstModels}/Rig_Medium_MovementBasic.fbx", isAnim: true);

            // 4. Build URP/Lit materials per character.
            foreach (var c in Characters)
            {
                CreateLitMaterial($"{DstMats}/M_KayKit_{c}.mat",
                                  $"{DstModels}/{c.ToLowerInvariant()}_texture.png");
            }

            // 5. Build display prefabs (FBX instance + material assigned).
            foreach (var c in Characters) BuildPrefab(c, materialName: c);
            BuildPrefab("Rogue_Hooded", materialName: "Rogue");

            AssetDatabase.SaveAssets();
            Debug.Log($"[KayKit] Imported {Characters.Length} adventurers + 2 anim rigs " +
                      $"(copied {copied} new files).");
        }

        // ─────────────────────────────────────────────────────────────────────
        // FBX import settings
        // ─────────────────────────────────────────────────────────────────────

        static void ConfigureCharacterFbx(string path, bool isAnim)
        {
            if (!File.Exists(path)) return;
            var imp = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) return;
            bool dirty = false;
            if (imp.animationType != ModelImporterAnimationType.Generic)
            {
                imp.animationType = ModelImporterAnimationType.Generic;
                dirty = true;
            }
            if (imp.materialImportMode != ModelImporterMaterialImportMode.None)
            {
                imp.materialImportMode = ModelImporterMaterialImportMode.None;
                dirty = true;
            }
            if (imp.importAnimation != isAnim)
            {
                imp.importAnimation = isAnim;
                dirty = true;
            }
            if (dirty) AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Material + prefab construction
        // ─────────────────────────────────────────────────────────────────────

        static void CreateLitMaterial(string matPath, string texPath)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) return;

            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            }
            EditorUtility.SetDirty(mat);
        }

        static void BuildPrefab(string charName, string materialName)
        {
            var fbxPath    = $"{DstModels}/{charName}.fbx";
            var matPath    = $"{DstMats}/M_KayKit_{materialName}.mat";
            var prefabPath = $"{DstPrefabs}/Char_{charName}.prefab";

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbx == null) return;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            try
            {
                instance.name = $"Char_{charName}";
                if (mat != null)
                {
                    foreach (var smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                        smr.sharedMaterials = Repeat(mat, smr.sharedMaterials.Length);
                    foreach (var mr in instance.GetComponentsInChildren<MeshRenderer>(true))
                        mr.sharedMaterials = Repeat(mat, mr.sharedMaterials.Length);
                }
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        static Material[] Repeat(Material mat, int count)
        {
            if (count <= 0) count = 1;
            var arr = new Material[count];
            for (int i = 0; i < arr.Length; i++) arr[i] = mat;
            return arr;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)!.Replace('\\', '/');
            var leaf   = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        static int CopyIfNew(string src, string dst)
        {
            if (!File.Exists(src))
            {
                Debug.LogWarning($"[KayKit] Missing source: {src}");
                return 0;
            }
            if (File.Exists(dst)) return 0;
            File.Copy(src, dst, false);
            return 1;
        }
    }
}
