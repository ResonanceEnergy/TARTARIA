using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Imports the additional KayKit FREE packs into the project's standard
    /// asset tree. Each pack is handled idempotently and silently no-ops
    /// when the corresponding source folder is missing:
    ///
    ///   • RPG Tools &amp; Bits (static props, 1 shared atlas)
    ///         → Assets/_Project/Models/Props/KayKit_Tools/
    ///   • Forest Nature Pack (static props, 1 shared atlas)
    ///         → Assets/_Project/Models/Props/KayKit_Forest/
    ///   • Skeletons (4 chibi character FBXes + props + 2 Rig_Medium anims)
    ///         → Assets/_Project/Models/Characters/KayKit/Skeletons/
    ///         → Assets/_Project/Models/Props/KayKit_Skeletons/
    ///   • Character Animations (Rig_Large + Rig_Medium FBX anim libraries)
    ///         → Assets/_Project/Models/Characters/KayKit/Animations/
    ///
    /// All FBXes are configured as Generic rigs (KayKit's custom skeletons
    /// are not Mixamo-humanoid compatible) and HumanoidAutoBinder is told
    /// to skip the entire /KayKit/ subtree.
    /// </summary>
    public static class KayKitPacksImporter
    {
        const string DstProps      = "Assets/_Project/Models/Props";
        const string DstChars      = "Assets/_Project/Models/Characters/KayKit";
        const string DstMats       = "Assets/_Project/Materials/KayKit";
        const string DstPrefabsP   = "Assets/_Project/Prefabs/Props/KayKit";
        const string DstPrefabsC   = "Assets/_Project/Prefabs/Characters/KayKit";

        [MenuItem("TARTARIA/Integration/Import KayKit Packs (Tools/Forest/Skeletons/Anims)")]
        public static void ImportAllMenu() => ImportAll();

        public static void ImportAll()
        {
            EnsureFolder(DstMats);
            EnsureFolder(DstPrefabsP);
            EnsureFolder(DstPrefabsC);

            int copied = 0;
            copied += ImportPropPack(
                srcRoot:    "Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE",
                fbxSubdir:  "Assets/fbx",
                texSubpath: "Textures/tools_bits_texture.png",
                destModels: $"{DstProps}/KayKit_Tools",
                destPrefabs:$"{DstPrefabsP}/Tools",
                matName:    "M_KayKit_Tools");

            copied += ImportPropPack(
                srcRoot:    "Assets/KayKit_Forest_Nature_Pack_1.0_FREE/KayKit_Forest_Nature_Pack_1.0_FREE",
                fbxSubdir:  "Assets/fbx",
                texSubpath: "Textures/forest_texture.png",
                destModels: $"{DstProps}/KayKit_Forest",
                destPrefabs:$"{DstPrefabsP}/Forest",
                matName:    "M_KayKit_Forest");

            copied += ImportSkeletonsPack();
            copied += ImportCharacterAnimationsPack();

            if (copied > 0) AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.SaveAssets();
            Debug.Log($"[KayKitPacks] Import complete (copied {copied} new files).");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Static prop packs (Tools, Forest)
        // ─────────────────────────────────────────────────────────────────────

        static int ImportPropPack(string srcRoot, string fbxSubdir, string texSubpath,
                                  string destModels, string destPrefabs,
                                  string matName)
        {
            if (!Directory.Exists(srcRoot))
            {
                Debug.Log($"[KayKitPacks] {srcRoot} not present — skipping.");
                return 0;
            }
            EnsureFolder(destModels);
            EnsureFolder(destPrefabs);

            int copied = 0;
            var srcFbxDir = $"{srcRoot}/{fbxSubdir}";
            var srcTex    = $"{srcRoot}/{texSubpath}";
            var dstTex    = $"{destModels}/{Path.GetFileName(texSubpath)}";

            copied += CopyIfNew(srcTex, dstTex);

            var copiedFbxes = new List<string>();
            if (Directory.Exists(srcFbxDir))
            {
                foreach (var f in Directory.GetFiles(srcFbxDir, "*.fbx"))
                {
                    var dst = $"{destModels}/{Path.GetFileName(f)}";
                    copied += CopyIfNew(f, dst);
                    copiedFbxes.Add(dst);
                }
            }
            if (copied > 0) AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (var f in copiedFbxes) ConfigureStaticFbx(f);

            var mat = CreateLitMaterial($"{DstMats}/{matName}.mat", dstTex);

            foreach (var f in copiedFbxes) BuildStaticPrefab(f, mat, destPrefabs);
            return copied;
        }

        static void ConfigureStaticFbx(string path)
        {
            if (!File.Exists(path)) return;
            var imp = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) return;
            bool dirty = false;
            if (imp.animationType != ModelImporterAnimationType.None)
            { imp.animationType = ModelImporterAnimationType.None; dirty = true; }
            if (imp.materialImportMode != ModelImporterMaterialImportMode.None)
            { imp.materialImportMode = ModelImporterMaterialImportMode.None; dirty = true; }
            if (imp.importAnimation)
            { imp.importAnimation = false; dirty = true; }
            if (!imp.addCollider)
            { imp.addCollider = true; dirty = true; }
            if (dirty) AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }

        static void BuildStaticPrefab(string fbxPath, Material mat, string prefabDir)
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbx == null) return;
            var name = Path.GetFileNameWithoutExtension(fbxPath);
            var prefabPath = $"{prefabDir}/Prop_{name}.prefab";

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            try
            {
                instance.name = $"Prop_{name}";
                if (mat != null)
                {
                    foreach (var mr in instance.GetComponentsInChildren<MeshRenderer>(true))
                        mr.sharedMaterials = Repeat(mat, mr.sharedMaterials.Length);
                }
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Skeletons pack
        // ─────────────────────────────────────────────────────────────────────

        static int ImportSkeletonsPack()
        {
            const string srcRoot = "Assets/KayKit_Skeletons_1.1_FREE/KayKit_Skeletons_1.1_FREE";
            if (!Directory.Exists(srcRoot))
            {
                Debug.Log($"[KayKitPacks] {srcRoot} not present — skipping.");
                return 0;
            }

            var dstChars  = $"{DstChars}/Skeletons";
            var dstProps  = $"{DstProps}/KayKit_Skeletons";
            var dstAnims  = $"{DstChars}/Animations";
            var dstPrefC  = $"{DstPrefabsC}/Skeletons";
            var dstPrefP  = $"{DstPrefabsP}/Skeletons";
            EnsureFolder(dstChars); EnsureFolder(dstProps);
            EnsureFolder(dstAnims); EnsureFolder(dstPrefC); EnsureFolder(dstPrefP);

            int copied = 0;
            var sharedTex = $"{dstChars}/skeleton_texture.png";
            copied += CopyIfNew($"{srcRoot}/texture/skeleton_texture.png", sharedTex);

            // Characters
            string[] characters = { "Skeleton_Mage", "Skeleton_Minion", "Skeleton_Rogue", "Skeleton_Warrior" };
            var charFbxes = new List<string>();
            foreach (var c in characters)
            {
                var dst = $"{dstChars}/{c}.fbx";
                copied += CopyIfNew($"{srcRoot}/characters/fbx/{c}.fbx", dst);
                charFbxes.Add(dst);
            }

            // Equipment props
            var propFbxes = new List<string>();
            var srcPropDir = $"{srcRoot}/assets/fbx";
            if (Directory.Exists(srcPropDir))
            {
                foreach (var f in Directory.GetFiles(srcPropDir, "*.fbx"))
                {
                    var dst = $"{dstProps}/{Path.GetFileName(f)}";
                    copied += CopyIfNew(f, dst);
                    propFbxes.Add(dst);
                }
            }

            // Anims (only 2 here; full library is in CharacterAnimations pack)
            copied += CopyIfNew($"{srcRoot}/Animations/fbx/Rig_Medium/Rig_Medium_General.fbx",
                                $"{dstAnims}/Skeletons_Rig_Medium_General.fbx");
            copied += CopyIfNew($"{srcRoot}/Animations/fbx/Rig_Medium/Rig_Medium_MovementBasic.fbx",
                                $"{dstAnims}/Skeletons_Rig_Medium_MovementBasic.fbx");

            if (copied > 0) AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (var f in charFbxes) ConfigureCharacterFbx(f, isAnim: false);
            foreach (var f in propFbxes) ConfigureStaticFbx(f);
            ConfigureCharacterFbx($"{dstAnims}/Skeletons_Rig_Medium_General.fbx",       isAnim: true);
            ConfigureCharacterFbx($"{dstAnims}/Skeletons_Rig_Medium_MovementBasic.fbx", isAnim: true);

            var mat = CreateLitMaterial($"{DstMats}/M_KayKit_Skeletons.mat", sharedTex);

            foreach (var f in charFbxes) BuildSkinnedPrefab(f, mat, dstPrefC, "Char_");
            foreach (var f in propFbxes) BuildStaticPrefab(f, mat, dstPrefP);
            return copied;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Character Animations pack
        // ─────────────────────────────────────────────────────────────────────

        static int ImportCharacterAnimationsPack()
        {
            const string srcRoot = "Assets/KayKit_Character_Animations_1.1/KayKit_Character_Animations_1.1";
            if (!Directory.Exists(srcRoot))
            {
                Debug.Log($"[KayKitPacks] {srcRoot} not present — skipping.");
                return 0;
            }
            var dstAnims = $"{DstChars}/Animations";
            EnsureFolder(dstAnims);

            int copied = 0;
            var copiedFbxes = new List<string>();
            foreach (var rig in new[] { "Rig_Large", "Rig_Medium" })
            {
                var srcDir = $"{srcRoot}/Animations/fbx/{rig}";
                if (!Directory.Exists(srcDir)) continue;
                foreach (var f in Directory.GetFiles(srcDir, "*.fbx"))
                {
                    var dst = $"{dstAnims}/{Path.GetFileName(f)}";
                    copied += CopyIfNew(f, dst);
                    copiedFbxes.Add(dst);
                }
            }

            if (copied > 0) AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var f in copiedFbxes) ConfigureCharacterFbx(f, isAnim: true);
            return copied;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Shared helpers
        // ─────────────────────────────────────────────────────────────────────

        static void ConfigureCharacterFbx(string path, bool isAnim)
        {
            if (!File.Exists(path)) return;
            var imp = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) return;
            bool dirty = false;
            if (imp.animationType != ModelImporterAnimationType.Generic)
            { imp.animationType = ModelImporterAnimationType.Generic; dirty = true; }
            if (imp.materialImportMode != ModelImporterMaterialImportMode.None)
            { imp.materialImportMode = ModelImporterMaterialImportMode.None; dirty = true; }
            if (imp.importAnimation != isAnim)
            { imp.importAnimation = isAnim; dirty = true; }
            if (dirty) AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }

        static Material CreateLitMaterial(string matPath, string texPath)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) return null;

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
            return mat;
        }

        static void BuildSkinnedPrefab(string fbxPath, Material mat, string prefabDir, string prefix)
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbx == null) return;
            var name = Path.GetFileNameWithoutExtension(fbxPath);
            var prefabPath = $"{prefabDir}/{prefix}{name}.prefab";

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            try
            {
                instance.name = $"{prefix}{name}";
                if (mat != null)
                {
                    foreach (var smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                        smr.sharedMaterials = Repeat(mat, smr.sharedMaterials.Length);
                    foreach (var mr in instance.GetComponentsInChildren<MeshRenderer>(true))
                        mr.sharedMaterials = Repeat(mat, mr.sharedMaterials.Length);
                }
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            }
            finally { Object.DestroyImmediate(instance); }
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
                Debug.LogWarning($"[KayKitPacks] Missing source: {src}");
                return 0;
            }
            if (File.Exists(dst)) return 0;
            File.Copy(src, dst, false);
            return 1;
        }
    }
}
