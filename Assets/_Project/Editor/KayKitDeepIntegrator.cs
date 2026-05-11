#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Second-pass deep integration for the 5 KayKit FREE packs:
    ///   • Imports adventurer weapon/gear FBXes that the first-pass importer skipped.
    ///   • Imports the Mannequin reference characters from Character_Animations.
    ///   • Configures every anim FBX (Adventurers + Skeletons + Character_Animations)
    ///     for clip extraction with sensible loop flags.
    ///   • Builds two shared AnimatorControllers (Medium for adventurers/skeletons,
    ///     Large for mannequin) wired to a "Locomotion" blend tree using clips
    ///     extracted from the Character_Animations master FBXes.
    ///   • Assigns the controller to every existing character prefab and attaches
    ///     a class-appropriate weapon under each right-hand bone.
    ///
    /// Idempotent: safe to run repeatedly; existing prefabs / controllers are
    /// updated rather than duplicated.
    /// </summary>
    public static class KayKitDeepIntegrator
    {
        // ─── Sources ──────────────────────────────────────────────────────────
        const string AdvRoot     = "Assets/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE";
        const string SkelRoot    = "Assets/KayKit_Skeletons_1.1_FREE/KayKit_Skeletons_1.1_FREE";
        const string AnimRoot    = "Assets/KayKit_Character_Animations_1.1/KayKit_Character_Animations_1.1";

        // ─── Destinations ─────────────────────────────────────────────────────
        const string DstAdvGearMod = "Assets/_Project/Models/Props/KayKit_AdventurerGear";
        const string DstAdvGearPre = "Assets/_Project/Prefabs/Props/KayKit/AdventurerGear";
        const string DstMannequinMod = "Assets/_Project/Models/Characters/KayKit/Mannequin";
        const string DstMannequinPre = "Assets/_Project/Prefabs/Characters/KayKit/Mannequin";
        const string DstAnimsDir   = "Assets/_Project/Animations/KayKit";
        const string DstControllers = "Assets/_Project/Animations/KayKit/Controllers";
        const string DstMatsDir     = "Assets/_Project/Materials/KayKit";

        const string ControllerMedium = "AC_KayKit_Medium.controller";
        const string ControllerLarge  = "AC_KayKit_Large.controller";

        [MenuItem("TARTARIA/Integration/KayKit Deep Integration (anims, gear, weapons)")]
        public static void RunMenu() => Run();

        public static void Run()
        {
            EnsureFolder(DstAdvGearMod);
            EnsureFolder(DstAdvGearPre);
            EnsureFolder(DstMannequinMod);
            EnsureFolder(DstMannequinPre);
            EnsureFolder(DstAnimsDir);
            EnsureFolder(DstControllers);

            try
            {
                AssetDatabase.StartAssetEditing();
                CopyAdventurerGearFbx();
                CopyMannequinFbx();
                CopyAllAnimFbx();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Now FBXes exist on disk and are imported — configure + build prefabs.
            ConfigureAdventurerGear();
            ConfigureMannequin();
            ConfigureAllAnimFbx();

            BuildAdventurerGearPrefabs();
            BuildMannequinPrefabs();

            var medium = BuildController(ControllerMedium, /*large*/ false);
            var large  = BuildController(ControllerLarge,  /*large*/ true);

            AssignControllerToPrefabs(medium, large);
            AttachWeaponsToCharacters();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[KayKitDeep] ✓ Deep integration pass complete.");
        }

        // ═════════════════════════════════════════════════════════════════════
        // 1. ADVENTURER WEAPONS / GEAR
        // ═════════════════════════════════════════════════════════════════════

        static void CopyAdventurerGearFbx()
        {
            string srcDir = $"{AdvRoot}/Assets/fbx";
            if (!AssetDatabase.IsValidFolder(srcDir)) return;
            foreach (var srcGuid in AssetDatabase.FindAssets("t:Model", new[] { srcDir }))
            {
                var srcPath = AssetDatabase.GUIDToAssetPath(srcGuid);
                if (!srcPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
                string dstFbx = $"{DstAdvGearMod}/{Path.GetFileName(srcPath)}";
                if (!File.Exists(dstFbx)) AssetDatabase.CopyAsset(srcPath, dstFbx);
            }
        }

        static void ConfigureAdventurerGear()
        {
            if (!AssetDatabase.IsValidFolder(DstAdvGearMod)) return;
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { DstAdvGearMod }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                    ConfigureStaticFbx(path);
            }
        }

        static void BuildAdventurerGearPrefabs()
        {
            if (!AssetDatabase.IsValidFolder(DstAdvGearMod)) return;
            var sharedMat = EnsureMaterial("M_KayKit_AdventurerGear",
                $"{AdvRoot}/Textures/knight_texture.png");

            int count = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { DstAdvGearMod }))
            {
                var fbxPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!fbxPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
                string baseName = Path.GetFileNameWithoutExtension(fbxPath);
                BuildStaticPrefab(fbxPath, $"{DstAdvGearPre}/Prop_{baseName}.prefab", sharedMat);
                count++;
            }
            Debug.Log($"[KayKitDeep] Adventurer gear: {count} prefabs.");
        }

        // ═════════════════════════════════════════════════════════════════════
        // 2. MANNEQUIN CHARACTERS
        // ═════════════════════════════════════════════════════════════════════

        static void CopyMannequinFbx()
        {
            string srcDir = $"{AnimRoot}/Mannequin Character/characters";
            if (!AssetDatabase.IsValidFolder(srcDir)) return;
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { srcDir }))
            {
                var srcPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!srcPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
                string dstFbx = $"{DstMannequinMod}/{Path.GetFileName(srcPath)}";
                if (!File.Exists(dstFbx)) AssetDatabase.CopyAsset(srcPath, dstFbx);
            }
        }

        static void ConfigureMannequin()
        {
            if (!AssetDatabase.IsValidFolder(DstMannequinMod)) return;
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { DstMannequinMod }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                    ConfigureCharacterFbx(path);
            }
        }

        static void BuildMannequinPrefabs()
        {
            if (!AssetDatabase.IsValidFolder(DstMannequinMod)) return;
            var mat = EnsureMaterial("M_KayKit_Mannequin",
                $"{AnimRoot}/Mannequin Character/Textures/mannequin_texture.png");

            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { DstMannequinMod }))
            {
                var fbxPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!fbxPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
                string baseName = Path.GetFileNameWithoutExtension(fbxPath);
                BuildSkinnedPrefab(fbxPath, $"{DstMannequinPre}/Char_{baseName}.prefab", mat);
            }
            Debug.Log("[KayKitDeep] Mannequin character prefabs built.");
        }

        // ═════════════════════════════════════════════════════════════════════
        // 3. ANIM FBX CONFIGURATION
        // ═════════════════════════════════════════════════════════════════════

        static readonly string[] LoopHints = { "idle", "walk", "run", "sneak" };

        static void CopyAllAnimFbx()
        {
            CopyAnimFbxTree($"{AdvRoot}/Animations/fbx",  DstAnimsDir + "/Adventurers");
            CopyAnimFbxTree($"{SkelRoot}/Animations/fbx", DstAnimsDir + "/Skeletons");
            CopyAnimFbxTree($"{AnimRoot}/Animations/fbx", DstAnimsDir + "/Library");
        }

        static void ConfigureAllAnimFbx()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { DstAnimsDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;

                var imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp == null) continue;

                imp.animationType        = ModelImporterAnimationType.Generic;
                imp.importAnimation      = true;
                imp.materialImportMode   = ModelImporterMaterialImportMode.None;
                imp.importVisibility     = false;
                imp.importCameras        = false;
                imp.importLights         = false;
                imp.useFileScale         = true;

                var clips = imp.defaultClipAnimations;
                bool dirty = false;
                for (int i = 0; i < clips.Length; i++)
                {
                    bool shouldLoop = LoopHints.Any(h =>
                        clips[i].name.IndexOf(h, System.StringComparison.OrdinalIgnoreCase) >= 0);
                    if (clips[i].loopTime != shouldLoop)
                    {
                        clips[i].loopTime = shouldLoop;
                        dirty = true;
                    }
                }
                if (dirty) imp.clipAnimations = clips;
                imp.SaveAndReimport();
            }
        }

        static void CopyAnimFbxTree(string src, string dst)
        {
            if (!AssetDatabase.IsValidFolder(src)) return;
            EnsureFolder(dst);

            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { src }))
            {
                var srcPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!srcPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;

                // Preserve sub-folder layout (Rig_Medium / Rig_Large).
                string rel = srcPath.Substring(src.Length).TrimStart('/');
                string dstPath = $"{dst}/{rel}";
                EnsureFolder(Path.GetDirectoryName(dstPath).Replace('\\', '/'));
                if (!File.Exists(dstPath)) AssetDatabase.CopyAsset(srcPath, dstPath);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // 4. ANIMATOR CONTROLLER
        // ═════════════════════════════════════════════════════════════════════

        static AnimatorController BuildController(string fileName, bool large)
        {
            string path = $"{DstControllers}/{fileName}";

            // Always rebuild from scratch so re-runs reflect fresh clip pickings.
            AssetDatabase.DeleteAsset(path);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

            // Locate the master library FBX for this rig.
            string libDir = $"{DstAnimsDir}/Library/{(large ? "Rig_Large" : "Rig_Medium")}";
            var clips = LoadClipsFromDir(libDir).ToList();
            if (clips.Count == 0)
            {
                Debug.LogWarning($"[KayKitDeep] No clips found in {libDir} for {fileName}.");
                return ctrl;
            }

            // Pick a default idle / walk / run.
            AnimationClip idle = PickClip(clips, "idle");
            AnimationClip walk = PickClip(clips, "walk") ?? PickClip(clips, "run");
            AnimationClip run  = PickClip(clips, "run")  ?? walk;

            var sm = ctrl.layers[0].stateMachine;
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);

            if (idle != null && walk != null)
            {
                ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float); // no-op if already added
                // Build a 1D blend tree on Speed.
                var blendState = sm.AddState("Locomotion");
                var tree = new BlendTree
                {
                    name           = "Locomotion",
                    blendType       = BlendTreeType.Simple1D,
                    blendParameter = "Speed",
                    useAutomaticThresholds = false,
                };
                tree.AddChild(idle, 0f);
                tree.AddChild(walk, 0.5f);
                if (run != null && run != walk) tree.AddChild(run, 1f);
                AssetDatabase.AddObjectToAsset(tree, ctrl);
                blendState.motion = tree;
                sm.defaultState = blendState;
            }
            else if (idle != null)
            {
                var s = sm.AddState("Idle");
                s.motion = idle;
                sm.defaultState = s;
            }
            else
            {
                // Fallback — first available clip.
                var s = sm.AddState(clips[0].name);
                s.motion = clips[0];
                sm.defaultState = s;
            }

            EditorUtility.SetDirty(ctrl);
            Debug.Log($"[KayKitDeep] Built controller {fileName} with {clips.Count} clips (idle={idle?.name}, walk={walk?.name}, run={run?.name}).");
            return ctrl;
        }

        static IEnumerable<AnimationClip> LoadClipsFromDir(string dir)
        {
            if (!AssetDatabase.IsValidFolder(dir)) yield break;
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { dir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                foreach (var sub in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (sub is AnimationClip c && !c.name.StartsWith("__"))
                        yield return c;
                }
            }
        }

        static AnimationClip PickClip(List<AnimationClip> clips, string keyword)
        {
            return clips.FirstOrDefault(c =>
                c.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        // ═════════════════════════════════════════════════════════════════════
        // 5. ASSIGN CONTROLLER TO ALL CHARACTER PREFABS
        // ═════════════════════════════════════════════════════════════════════

        static void AssignControllerToPrefabs(AnimatorController medium, AnimatorController large)
        {
            string[] charDirs =
            {
                "Assets/_Project/Prefabs/Characters/KayKit",
                "Assets/_Project/Prefabs/Characters/KayKit/Skeletons",
                "Assets/_Project/Prefabs/Characters/KayKit/Mannequin",
            };

            int updated = 0;
            foreach (var dir in charDirs)
            {
                if (!AssetDatabase.IsValidFolder(dir)) continue;
                foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { dir }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var go = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        var animator = go.GetComponentInChildren<Animator>(true);
                        if (animator == null)
                        {
                            animator = go.AddComponent<Animator>();
                        }
                        bool isLarge = path.IndexOf("Mannequin_Large", System.StringComparison.OrdinalIgnoreCase) >= 0;
                        animator.runtimeAnimatorController = isLarge ? large : medium;
                        animator.applyRootMotion = false;
                        PrefabUtility.SaveAsPrefabAsset(go, path);
                        updated++;
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents(go);
                    }
                }
            }
            Debug.Log($"[KayKitDeep] Assigned animator controller to {updated} character prefabs.");
        }

        // ═════════════════════════════════════════════════════════════════════
        // 6. AUTO-ATTACH WEAPONS TO RIGHT HAND
        // ═════════════════════════════════════════════════════════════════════

        // Class-appropriate default weapon prefab (must match Adventurer-gear file names).
        static readonly Dictionary<string, string> WeaponMap = new()
        {
            { "Knight",         "Prop_sword_1handed" },
            { "Barbarian",      "Prop_axe_2handed" },
            { "Rogue",          "Prop_dagger" },
            { "Rogue_Hooded",   "Prop_dagger" },
            { "Mage",           "Prop_staff" },
            { "Ranger",         "Prop_bow_withString" },
            { "Skeleton_Warrior","Prop_sword_1handed" },
            { "Skeleton_Mage",   "Prop_staff" },
            { "Skeleton_Rogue",  "Prop_dagger" },
            { "Skeleton_Minion", "Prop_axe_1handed" },
        };

        static void AttachWeaponsToCharacters()
        {
            string[] charDirs =
            {
                "Assets/_Project/Prefabs/Characters/KayKit",
                "Assets/_Project/Prefabs/Characters/KayKit/Skeletons",
            };

            int attached = 0;
            foreach (var dir in charDirs)
            {
                if (!AssetDatabase.IsValidFolder(dir)) continue;
                foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { dir }))
                {
                    var path     = AssetDatabase.GUIDToAssetPath(guid);
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    string charKey = fileName.StartsWith("Char_") ? fileName.Substring(5) : fileName;
                    if (!WeaponMap.TryGetValue(charKey, out var weaponPrefabName)) continue;

                    var weaponPrefab = LoadFirst<GameObject>(
                        $"{DstAdvGearPre}/{weaponPrefabName}.prefab",
                        $"Assets/_Project/Prefabs/Props/KayKit/Skeletons/{weaponPrefabName}.prefab");
                    if (weaponPrefab == null) continue;

                    var go = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        // Skip if a weapon is already attached.
                        var existing = go.transform.Find("Weapon_R");
                        if (existing != null) Object.DestroyImmediate(existing.gameObject);

                        Transform hand = FindRightHandBone(go.transform);
                        if (hand == null) continue;

                        var weapon = (GameObject)PrefabUtility.InstantiatePrefab(weaponPrefab, hand);
                        weapon.name = "Weapon_R";
                        weapon.transform.localPosition = Vector3.zero;
                        weapon.transform.localRotation = Quaternion.identity;
                        // Strip colliders on the held weapon — gameplay code will add proper hit volumes.
                        foreach (var col in weapon.GetComponentsInChildren<Collider>(true))
                            Object.DestroyImmediate(col);

                        PrefabUtility.SaveAsPrefabAsset(go, path);
                        attached++;
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents(go);
                    }
                }
            }
            Debug.Log($"[KayKitDeep] Attached weapons to {attached} character prefabs.");
        }

        static readonly string[] HandHints = { "handslot.r", "hand.r", "hand_r", "righthand", "wrist.r", "wrist_r" };

        static Transform FindRightHandBone(Transform root)
        {
            // BFS through children, match against known KayKit naming variants.
            var queue = new Queue<Transform>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var t = queue.Dequeue();
                string n = t.name.ToLowerInvariant();
                if (HandHints.Any(h => n.Contains(h))) return t;
                foreach (Transform c in t) queue.Enqueue(c);
            }
            return null;
        }

        // ═════════════════════════════════════════════════════════════════════
        // HELPERS
        // ═════════════════════════════════════════════════════════════════════

        static T LoadFirst<T>(params string[] paths) where T : Object
        {
            foreach (var p in paths)
            {
                var a = AssetDatabase.LoadAssetAtPath<T>(p);
                if (a != null) return a;
            }
            return null;
        }

        static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static void ConfigureCharacterFbx(string fbxPath)
        {
            var imp = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (imp == null) return;
            imp.animationType      = ModelImporterAnimationType.Generic;
            imp.importAnimation    = true;
            imp.materialImportMode = ModelImporterMaterialImportMode.None;
            imp.importVisibility   = false;
            imp.importCameras      = false;
            imp.importLights       = false;
            imp.SaveAndReimport();
        }

        static void ConfigureStaticFbx(string fbxPath)
        {
            var imp = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (imp == null) return;
            imp.animationType      = ModelImporterAnimationType.None;
            imp.importAnimation    = false;
            imp.materialImportMode = ModelImporterMaterialImportMode.None;
            imp.addCollider        = true;
            imp.importVisibility   = false;
            imp.importCameras      = false;
            imp.importLights       = false;
            imp.SaveAndReimport();
        }

        static Material EnsureMaterial(string name, string srcTexturePath)
        {
            EnsureFolder(DstMatsDir);
            string matPath = $"{DstMatsDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            // Copy texture into project if needed.
            if (!string.IsNullOrEmpty(srcTexturePath) && File.Exists(srcTexturePath))
            {
                string texDst = $"{DstMatsDir}/T_{name}.png";
                if (!File.Exists(texDst)) AssetDatabase.CopyAsset(srcTexturePath, texDst);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texDst);
                if (tex != null && mat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", tex);
                else if (tex != null && mat.HasProperty("_MainTex"))
                    mat.SetTexture("_MainTex", tex);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void BuildStaticPrefab(string fbxPath, string prefabPath, Material mat)
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (src == null) return;

            EnsureFolder(Path.GetDirectoryName(prefabPath).Replace('\\', '/'));

            var go = (GameObject)PrefabUtility.InstantiatePrefab(src);
            try
            {
                foreach (var rend in go.GetComponentsInChildren<Renderer>(true))
                {
                    var arr = new Material[rend.sharedMaterials.Length];
                    for (int i = 0; i < arr.Length; i++) arr[i] = mat;
                    rend.sharedMaterials = arr;
                }
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        static void BuildSkinnedPrefab(string fbxPath, string prefabPath, Material mat)
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (src == null) return;

            EnsureFolder(Path.GetDirectoryName(prefabPath).Replace('\\', '/'));

            var go = (GameObject)PrefabUtility.InstantiatePrefab(src);
            try
            {
                foreach (var rend in go.GetComponentsInChildren<Renderer>(true))
                {
                    var arr = new Material[rend.sharedMaterials.Length];
                    for (int i = 0; i < arr.Length; i++) arr[i] = mat;
                    rend.sharedMaterials = arr;
                }
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
#endif
