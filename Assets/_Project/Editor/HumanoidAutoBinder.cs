using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Closes §8 items 5 + 8 of the Visual/Audio Architecture deep-dive:
    /// when a humanoid FBX is present in <c>Assets/_Project/Models/Characters/</c>,
    /// auto-configure it as Humanoid, generate Avatar, and replace the capsule
    /// body parts on Player.prefab with one SkinnedMeshRenderer.
    ///
    /// Silently no-ops when the drop-zone is empty so the build pipeline stays
    /// green pre-asset-acquisition. Idempotent — safe to run on every build.
    /// </summary>
    public static class HumanoidAutoBinder
    {
        const string CharactersDir = "Assets/_Project/Models/Characters";
        const string PlayerPrefabPath = "Assets/_Project/Prefabs/Characters/Player.prefab";
        const string LocomotionControllerPath = "Assets/_Project/Models/Animations/PlayerLocomotion.controller";
        const string AetherMaterialPath = "Assets/_Project/Materials/M_AetherVein.mat";

        // Filename preference for ambiguous drops.
        static readonly string[] PreferredKeywords =
        {
            "Player_Mesh", "Female", "Eve", "Kachujin", "Liam", "Elara", "Mremireh"
        };

        [MenuItem("TARTARIA/Integration/Bind Humanoid Mesh")]
        public static void BindMenu()
        {
            int n = BindIfAvailable();
            if (n == 0)
            {
                Debug.Log($"[HumanoidAutoBinder] No FBX found in {CharactersDir}. " +
                          "Drop a Mixamo female humanoid FBX (T-pose, no anim) and re-run.");
            }
        }

        /// <summary>
        /// Build-pipeline entry point. Returns number of meshes bound (0 = skipped).
        /// Never throws — logs warnings and returns 0 on failure.
        /// </summary>
        public static int BindIfAvailable()
        {
            EnsureDir(CharactersDir);

            string fbxAssetPath = PickPreferredFbx();
            if (fbxAssetPath == null)
            {
                // Empty drop-zone — silent skip.
                return 0;
            }

            Debug.Log($"[HumanoidAutoBinder] Binding humanoid mesh: {fbxAssetPath}");

            if (!ConfigureHumanoid(fbxAssetPath))
            {
                Debug.LogWarning($"[HumanoidAutoBinder] Could not configure {fbxAssetPath} as Humanoid.");
                return 0;
            }

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxAssetPath);
            if (fbx == null)
            {
                Debug.LogWarning($"[HumanoidAutoBinder] Failed to load FBX after import: {fbxAssetPath}");
                return 0;
            }

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogWarning($"[HumanoidAutoBinder] Player.prefab missing at {PlayerPrefabPath}. " +
                                 "Earlier phases must scaffold it before 9j2.");
                return 0;
            }

            var instance = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                StripCapsuleBodyParts(instance);
                AttachMesh(instance, fbx);
                WireAnimator(instance, fbxAssetPath);
                ApplyAetherTint(instance);

                PrefabUtility.SaveAsPrefabAsset(instance, PlayerPrefabPath);
                Debug.Log($"[HumanoidAutoBinder] ✓ Player.prefab now uses humanoid SkinnedMeshRenderer.");
                return 1;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(instance);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // FBX selection + import config
        // ─────────────────────────────────────────────────────────────────────

        static string PickPreferredFbx()
        {
            if (!AssetDatabase.IsValidFolder(CharactersDir)) return null;

            var guids = AssetDatabase.FindAssets("t:Model", new[] { CharactersDir });
            var fbxes = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                // KayKit chibi rigs are not Mixamo-compatible humanoids — handled
                // separately by KayKitImporter and configured as Generic.
                .Where(p => p.IndexOf("/KayKit/", System.StringComparison.OrdinalIgnoreCase) < 0)
                .OrderBy(p => p, System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (fbxes.Count == 0) return null;

            foreach (var keyword in PreferredKeywords)
            {
                var hit = fbxes.FirstOrDefault(p =>
                    Path.GetFileNameWithoutExtension(p)
                        .IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0);
                if (hit != null) return hit;
            }
            return fbxes[0];
        }

        static bool ConfigureHumanoid(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null) return false;

            bool dirty = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                dirty = true;
            }
            if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportStandard)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                dirty = true;
            }
            if (importer.meshCompression != ModelImporterMeshCompression.Off)
            {
                importer.meshCompression = ModelImporterMeshCompression.Off;
                dirty = true;
            }
            if (dirty)
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Prefab surgery
        // ─────────────────────────────────────────────────────────────────────

        static void StripCapsuleBodyParts(GameObject root)
        {
            // Remove any prior PlayerMesh child (idempotency).
            var prior = root.transform.Find("PlayerMesh");
            if (prior != null)
            {
                Object.DestroyImmediate(prior.gameObject);
            }

            // Remove primitive body-part renderers seeded by PlayerVisualBuilder /
            // Phase 6 character scaffolding.
            string[] partNames = { "Body", "Head", "ArmL", "ArmR", "LegL", "LegR", "Arm", "Leg" };
            var toDelete = root.GetComponentsInChildren<MeshRenderer>(true)
                .Select(r => r.gameObject)
                .Where(go => go != root)
                .Where(go => partNames.Any(n =>
                    go.name.IndexOf(n, System.StringComparison.OrdinalIgnoreCase) >= 0))
                .Distinct()
                .ToList();

            foreach (var go in toDelete)
            {
                Object.DestroyImmediate(go);
            }
        }

        static void AttachMesh(GameObject root, GameObject fbx)
        {
            var meshGO = (GameObject)PrefabUtility.InstantiatePrefab(fbx, root.transform);
            meshGO.name = "PlayerMesh";
            meshGO.transform.localPosition = Vector3.zero;
            meshGO.transform.localRotation = Quaternion.identity;
            meshGO.transform.localScale = Vector3.one;
        }

        static void WireAnimator(GameObject root, string fbxAssetPath)
        {
            var animator = root.GetComponent<Animator>() ?? root.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            // Avatar lives as a sub-asset of the FBX once imported as Human.
            var avatar = AssetDatabase
                .LoadAllAssetsAtPath(fbxAssetPath)
                .OfType<Avatar>()
                .FirstOrDefault();
            if (avatar != null)
            {
                animator.avatar = avatar;
            }

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(LocomotionControllerPath);
            if (controller != null && animator.runtimeAnimatorController == null)
            {
                animator.runtimeAnimatorController = controller;
            }

            // Disable procedural capsule animator if present — real bones now drive pose.
            var procedural = root.GetComponent<Tartaria.Gameplay.PlayerAnimator>();
            if (procedural != null) procedural.enabled = false;

            // Bridge real-rig pose from input.
            if (root.GetComponent<Tartaria.Gameplay.PlayerAnimatorBridge>() == null)
            {
                root.AddComponent<Tartaria.Gameplay.PlayerAnimatorBridge>();
            }
        }

        static void ApplyAetherTint(GameObject root)
        {
            var aether = AssetDatabase.LoadAssetAtPath<Material>(AetherMaterialPath);
            if (aether == null) return;

            var mesh = root.transform.Find("PlayerMesh");
            if (mesh == null) return;

            foreach (var smr in mesh.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var mats = smr.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null)
                    {
                        mats[i] = aether;
                        changed = true;
                    }
                }
                if (changed) smr.sharedMaterials = mats;
            }
        }

        static void EnsureDir(string assetDir)
        {
            if (AssetDatabase.IsValidFolder(assetDir)) return;
            string sysPath = Path.Combine(Application.dataPath, "..", assetDir);
            Directory.CreateDirectory(sysPath);
            AssetDatabase.Refresh();
        }
    }
}
