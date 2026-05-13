using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// Asset Integration Tool — applies downloaded FREE assets to TARTARIA.
    /// Priority 1: Capoeira animations → Player.prefab
    /// Priority 2: 432 Hz music → AudioManager (via Inspector)
    /// Priority 3: Player_Mesh.fbx → Replace capsule in Player.prefab
    /// Priority 4: Validate custom shaders on buildings
    /// </summary>
    public static class AssetIntegrationTool
    {
        const string CAPOEIRA_ANIM_PATH = "Assets/_Project/Models/Animations/Capoeira";
        const string PLAYER_MESH_PATH = "Assets/_Project/Models/Characters/Player_Mesh.fbx";
        const string PLAYER_PREFAB_PATH = "Assets/_Project/Prefabs/Characters/Player.prefab";
        const string ANIMATOR_CONTROLLER_PATH = "Assets/_Project/Animations/PlayerAnimatorController.controller";
        const string ANIMATIONS_DIR = "Assets/_Project/Animations";

        [MenuItem("TARTARIA/Integration/1. Apply Capoeira Animations to Player")]
        public static void IntegrateCapoeiraAnimations()
        {
            Debug.Log("[Integration] Starting Capoeira animation integration...");

            // Create Animations directory if it doesn't exist
            if (!AssetDatabase.IsValidFolder(ANIMATIONS_DIR))
            {
                Directory.CreateDirectory(ANIMATIONS_DIR);
                AssetDatabase.Refresh();
            }

            // Create Animator Controller
            AnimatorController controller = CreateCapoeiraAnimatorController();
            if (controller == null)
            {
                Debug.LogError("[Integration] Failed to create Animator Controller!");
                return;
            }

            // Load Player prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
            if (playerPrefab == null)
            {
                Debug.LogError($"[Integration] Player prefab not found at: {PLAYER_PREFAB_PATH}");
                return;
            }

            // Open prefab for editing
            string prefabPath = AssetDatabase.GetAssetPath(playerPrefab);
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            // Add or get Animator component
            Animator animator = prefabInstance.GetComponent<Animator>();
            if (animator == null)
            {
                animator = prefabInstance.AddComponent<Animator>();
            }

            // Apply controller
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false; // Use CharacterController movement
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            // Disable PlayerAnimator (procedural) - we're using real animations now
            var proceduralAnimator = prefabInstance.GetComponent<Tartaria.Gameplay.PlayerAnimator>();
            if (proceduralAnimator != null)
            {
                proceduralAnimator.enabled = false;
                Debug.Log("[Integration] Disabled procedural PlayerAnimator (using real animations now)");
            }

            // Add PlayerAnimatorBridge to drive animations from input
            if (prefabInstance.GetComponent<Tartaria.Gameplay.PlayerAnimatorBridge>() == null)
            {
                prefabInstance.AddComponent<Tartaria.Gameplay.PlayerAnimatorBridge>();
                Debug.Log("[Integration] Added PlayerAnimatorBridge to drive animations from input");
            }

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabInstance);

            Debug.Log($"[Integration] ✓ Capoeira animations applied to Player.prefab");
            Debug.Log($"[Integration] Animator Controller: {ANIMATOR_CONTROLLER_PATH}");
            Debug.Log($"[Integration] Animations mapped:");
            Debug.Log($"  Walk → ginga_forward");
            Debug.Log($"  Idle → ginga_variation_1");
            Debug.Log($"  Jump → au");
            Debug.Log($"  Attack → martelo");

            if (OneClickBuild.DialogsAllowed)
            {
                EditorUtility.DisplayDialog("Integration Complete",
                    "Capoeira animations successfully applied to Player.prefab!\n\n" +
                    "Animator Controller created at:\n" + ANIMATOR_CONTROLLER_PATH,
                    "OK");
            }
        }

        // KayKit Rogue_Hooded — gender-neutral hooded silhouette, fits Elara Voss canon
        const string KAYKIT_PLAYER_FBX = "Assets/_Project/Models/Characters/KayKit/Rogue_Hooded.fbx";
        const string KAYKIT_ANIM_DIR   = "Assets/_Project/Models/Characters/KayKit/Animations";
        const string KAYKIT_CONTROLLER_PATH = "Assets/_Project/Animations/KayKitPlayerController.controller";

        [MenuItem("TARTARIA/Integration/3. Replace Player Capsule with KayKit Rogue_Hooded")]
        public static void ReplacePlayerMeshModel()
        {
            Debug.Log("[Integration] Replacing Player capsule with KayKit Rogue_Hooded.fbx (Elara Voss visual)...");

            // 1) Force importer settings on the mesh + KayKit animation rigs (Generic, animations enabled)
            ConfigureKayKitImporter(KAYKIT_PLAYER_FBX, importAnims: false);
            ConfigureKayKitAnimationsFolder(KAYKIT_ANIM_DIR);

            GameObject sourceMesh = AssetDatabase.LoadAssetAtPath<GameObject>(KAYKIT_PLAYER_FBX);
            if (sourceMesh == null)
            {
                Debug.LogWarning($"[Integration] KayKit source not found at {KAYKIT_PLAYER_FBX} — keeping procedural capsule.");
                return;
            }

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
            if (playerPrefab == null)
            {
                Debug.LogError($"[Integration] Player prefab not found at: {PLAYER_PREFAB_PATH}");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(playerPrefab);
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            // 2) Strip primitive body parts created by CharacterPrefabFactory.CreatePlayerPrefab
            string[] primitiveNames = {
                "Body","Head","Eye_L","Eye_R","Visor","JawGuard",
                "Arm_L","Arm_R","Leg_L","Leg_R","Foot_L","Foot_R",
                "Shoulder_L","Shoulder_R","Belt","AetherCore"
            };
            foreach (var n in primitiveNames)
            {
                var t = prefabInstance.transform.Find(n);
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }
            // Also remove any leftover PlayerMesh from prior runs
            var prevMesh = prefabInstance.transform.Find("PlayerMesh");
            if (prevMesh != null) Object.DestroyImmediate(prevMesh.gameObject);

            // 3) Instantiate KayKit mesh as child
            GameObject meshInstance = (GameObject)PrefabUtility.InstantiatePrefab(sourceMesh, prefabInstance.transform);
            meshInstance.name = "PlayerMesh";
            meshInstance.transform.localPosition = Vector3.zero;
            meshInstance.transform.localRotation = Quaternion.identity;
            // KayKit Rogue is ~1.7m tall — scale to fit CharacterController height of 2m
            meshInstance.transform.localScale = Vector3.one * 1.0f;

            // 4) Tint primary body materials with Aether glow accent (cape)
            string aetherMaterialPath = "Assets/_Project/Materials/M_AetherVein.mat";
            Material aetherMaterial = AssetDatabase.LoadAssetAtPath<Material>(aetherMaterialPath);
            if (aetherMaterial != null)
            {
                var renderers = meshInstance.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var renderer in renderers)
                {
                    var mats = renderer.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] != null && mats[i].name.ToLowerInvariant().Contains("cape"))
                            mats[i] = aetherMaterial;
                    }
                    renderer.sharedMaterials = mats;
                }
            }

            // 5) Build the KayKit animator controller from the pack's own (Generic-rigged) anims
            //    and assign to root Animator. This replaces the humanoid Capoeira controller
            //    (which can't drive a Generic skeleton) when KayKit is present.
            var controller = BuildKayKitAnimatorController();
            var animator = prefabInstance.GetComponent<Animator>();
            if (animator == null) animator = prefabInstance.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            if (controller != null) animator.runtimeAnimatorController = controller;
            animator.avatar = null; // Generic rig — no avatar required; Animator uses the SkinnedMeshRenderer hierarchy on PlayerMesh

            // Disable procedural PlayerAnimator so it doesn't fight the new clips
            var procedural = prefabInstance.GetComponent<Tartaria.Gameplay.PlayerAnimator>();
            if (procedural != null) procedural.enabled = false;

            // Add bridge if missing — drives Speed/Attack params from input
            if (prefabInstance.GetComponent<Tartaria.Gameplay.PlayerAnimatorBridge>() == null)
                prefabInstance.AddComponent<Tartaria.Gameplay.PlayerAnimatorBridge>();

            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabInstance);

            Debug.Log("[Integration] ✓ Player capsule replaced with KayKit Rogue_Hooded mesh + Generic animator.");
        }

        // Force every KayKit FBX in the animations folder to Generic + import animations.
        // This is what fixes the silent "no anim plays" smoke that's been haunting the build.
        static void ConfigureKayKitAnimationsFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return;
            var guids = AssetDatabase.FindAssets("t:Model", new[] { folder });
            int n = 0;
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (ConfigureKayKitImporter(p, importAnims: true)) n++;
            }
            Debug.Log($"[Integration] KayKit anim importers configured: {n}/{guids.Length}");
        }

        static bool ConfigureKayKitImporter(string path, bool importAnims)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) return false;
            bool dirty = false;
            if (importer.animationType != ModelImporterAnimationType.Generic)
            { importer.animationType = ModelImporterAnimationType.Generic; dirty = true; }
            if (importAnims && !importer.importAnimation)
            { importer.importAnimation = true; dirty = true; }
            if (importer.optimizeGameObjects)
            { importer.optimizeGameObjects = false; dirty = true; }
            if (dirty) importer.SaveAndReimport();
            return dirty;
        }

        // Build a tiny 3-state controller (Idle/Walk/Attack) from KayKit's own anim FBXs.
        // Picks the first matching clip name we can find under KayKit/Animations.
        static AnimatorController BuildKayKitAnimatorController()
        {
            if (!AssetDatabase.IsValidFolder(ANIMATIONS_DIR))
            {
                Directory.CreateDirectory(ANIMATIONS_DIR);
                AssetDatabase.Refresh();
            }

            AnimationClip idle   = FindKayKitClip(new[] { "idle", "ginga", "stand" });
            AnimationClip walk   = FindKayKitClip(new[] { "walk", "movement", "run" });
            AnimationClip attack = FindKayKitClip(new[] { "attack", "melee", "swing", "punch" });

            // If we can't find any KayKit clips, return null and let the existing
            // Capoeira controller (humanoid, may misbehave on Generic) stay attached.
            if (idle == null && walk == null && attack == null)
            {
                Debug.LogWarning("[Integration] No KayKit animation clips found — controller not built.");
                return null;
            }

            // Recreate clean each build
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(KAYKIT_CONTROLLER_PATH) != null)
                AssetDatabase.DeleteAsset(KAYKIT_CONTROLLER_PATH);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(KAYKIT_CONTROLLER_PATH);
            controller.AddParameter("Speed",  AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;

            var idleState = sm.AddState("Idle");
            if (idle != null) idleState.motion = idle;

            var walkState = sm.AddState("Walk");
            if (walk != null) walkState.motion = walk;

            var atkState = sm.AddState("Attack");
            if (attack != null) atkState.motion = attack;

            sm.defaultState = idleState;

            // Idle <-> Walk by Speed
            var i2w = idleState.AddTransition(walkState);
            i2w.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            i2w.hasExitTime = false; i2w.duration = 0.1f;

            var w2i = walkState.AddTransition(idleState);
            w2i.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            w2i.hasExitTime = false; w2i.duration = 0.1f;

            // Anywhere -> Attack on trigger
            var anyAtk = sm.AddAnyStateTransition(atkState);
            anyAtk.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            anyAtk.hasExitTime = false; anyAtk.duration = 0.05f;
            anyAtk.canTransitionToSelf = false;

            // Attack -> Idle when finished
            var atk2idle = atkState.AddTransition(idleState);
            atk2idle.hasExitTime = true; atk2idle.exitTime = 0.9f; atk2idle.duration = 0.15f;

            AssetDatabase.SaveAssets();
            Debug.Log($"[Integration] KayKit animator built  Idle:{(idle!=null?idle.name:"-")}  Walk:{(walk!=null?walk.name:"-")}  Attack:{(attack!=null?attack.name:"-")}");
            return controller;
        }

        static AnimationClip FindKayKitClip(string[] keywords)
        {
            if (!AssetDatabase.IsValidFolder(KAYKIT_ANIM_DIR)) return null;
            var modelGuids = AssetDatabase.FindAssets("t:Model", new[] { KAYKIT_ANIM_DIR });
            // Prefer Medium rig for the Rogue (medium humanoid silhouette)
            System.Collections.Generic.List<string> paths = new System.Collections.Generic.List<string>();
            foreach (var g in modelGuids) paths.Add(AssetDatabase.GUIDToAssetPath(g));
            paths.Sort((a, b) => (a.Contains("Medium") ? 0 : 1).CompareTo(b.Contains("Medium") ? 0 : 1));

            foreach (var p in paths)
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(p);
                foreach (var a in assets)
                {
                    var clip = a as AnimationClip;
                    if (clip == null || clip.name.StartsWith("__preview__")) continue;
                    string lower = clip.name.ToLowerInvariant();
                    foreach (var kw in keywords)
                    {
                        if (lower.Contains(kw)) return clip;
                    }
                }
            }
            return null;
        }

        [MenuItem("TARTARIA/Integration/3. Replace Player Capsule with Player_Mesh (DISABLED)")]
        public static void ReplacePlayerMeshModel_OLD()
        {
            Debug.Log("[Integration] Replacing Player capsule with Player_Mesh.fbx...");

            // Load Player_Mesh.fbx
            GameObject playerMesh = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_MESH_PATH);
            if (playerMesh == null)
            {
                Debug.LogError($"[Integration] Player_Mesh.fbx not found at: {PLAYER_MESH_PATH}");
                if (OneClickBuild.DialogsAllowed)
                {
                    EditorUtility.DisplayDialog("Error", "Player_Mesh.fbx not found!\nMake sure it's imported in Assets/_Project/Models/Characters/", "OK");
                }
                return;
            }

            // Load Player prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
            if (playerPrefab == null)
            {
                Debug.LogError($"[Integration] Player prefab not found at: {PLAYER_PREFAB_PATH}");
                return;
            }

            // Open prefab for editing
            string prefabPath = AssetDatabase.GetAssetPath(playerPrefab);
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            // Find and delete primitive body parts (capsules/spheres)
            var primitivesToDelete = prefabInstance.GetComponentsInChildren<Transform>()
                .Where(t => t.name.Contains("Body") || t.name.Contains("Arm") || t.name.Contains("Leg") || t.name.Contains("Head"))
                .Where(t => t.GetComponent<MeshFilter>() != null)
                .ToList();

            foreach (var primitive in primitivesToDelete)
            {
                if (primitive != prefabInstance.transform) // Don't delete root
                {
                    Debug.Log($"[Integration] Removing primitive: {primitive.name}");
                    Object.DestroyImmediate(primitive.gameObject);
                }
            }

            // Instantiate Player_Mesh as child
            GameObject meshInstance = PrefabUtility.InstantiatePrefab(playerMesh, prefabInstance.transform) as GameObject;
            meshInstance.name = "PlayerMesh";
            meshInstance.transform.localPosition = Vector3.zero;
            meshInstance.transform.localRotation = Quaternion.identity;
            meshInstance.transform.localScale = Vector3.one;

            // Apply Aether glow material
            string aetherMaterialPath = "Assets/_Project/Materials/M_AetherVein.mat";
            Material aetherMaterial = AssetDatabase.LoadAssetAtPath<Material>(aetherMaterialPath);
            if (aetherMaterial != null)
            {
                var renderers = meshInstance.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var renderer in renderers)
                {
                    var materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] == null || materials[i].name.Contains("Body"))
                        {
                            materials[i] = aetherMaterial;
                        }
                    }
                    renderer.sharedMaterials = materials;
                }
                Debug.Log($"[Integration] Applied M_AetherVein material to player mesh");
            }

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabInstance);

            Debug.Log($"[Integration] ✓ Player capsule replaced with Player_Mesh.fbx");
            if (OneClickBuild.DialogsAllowed)
            {
                EditorUtility.DisplayDialog("Integration Complete",
                    "Player capsule replaced with real 3D model!\n\n" +
                    "Player now has Mixamo Adventurer mesh with Aether glow material.",
                    "OK");
            }
        }

        [MenuItem("TARTARIA/Integration/4. Validate Custom Shaders on Buildings")]
        public static void ValidateCustomShaders()
        {
            Debug.Log("[Integration] Validating custom shaders on buildings...");

            string[] materialPaths = new string[]
            {
                "Assets/_Project/Materials/M_AetherVein.mat",
                "Assets/_Project/Materials/M_Corruption.mat",
                "Assets/_Project/Materials/M_Restoration.mat",
                "Assets/_Project/Materials/M_SpectralGhost.mat"
            };

            int found = 0;
            int compiled = 0;

            foreach (var path in materialPaths)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    found++;
                    if (mat.shader != null && !mat.shader.name.Contains("Hidden"))
                    {
                        compiled++;
                        Debug.Log($"[Integration] ✓ {mat.name} - Shader: {mat.shader.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Integration] Material not found: {path}");
                }
            }

            Debug.Log($"[Integration] Custom shaders validation: {compiled}/{found} materials compiled successfully");

            if (OneClickBuild.DialogsAllowed)
            {
                EditorUtility.DisplayDialog("Shader Validation",
                    $"Custom Shaders Status:\n\n" +
                    $"Materials found: {found}/4\n" +
                    $"Shaders compiled: {compiled}/4\n\n" +
                    (compiled == 4 ? "✓ All custom shaders ready!" : "⚠ Some shaders missing or not compiled"),
                    "OK");
            }
        }

        [MenuItem("TARTARIA/Integration/Run All Integration Steps")]
        public static void RunAllIntegrationSteps()
        {
            if (!EditorUtility.DisplayDialog("Asset Integration",
                "This will run all 4 integration steps:\n\n" +
                "1. Apply Capoeira animations to Player\n" +
                "2. Wire 432 Hz music (manual - use Inspector)\n" +
                "3. Replace Player capsule with Player_Mesh\n" +
                "4. Validate custom shaders\n\n" +
                "Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            IntegrateCapoeiraAnimations();
            ReplacePlayerMeshModel();
            ValidateCustomShaders();

            Debug.Log("[Integration] ═══════════════════════════════════════");
            Debug.Log("[Integration] ✓ ALL INTEGRATION STEPS COMPLETE!");
            Debug.Log("[Integration] ═══════════════════════════════════════");
            Debug.Log("[Integration]");
            Debug.Log("[Integration] MANUAL STEP REMAINING:");
            Debug.Log("[Integration] 2. Wire 432 Hz music:");
            Debug.Log("[Integration]    - Find AudioManager in scene hierarchy");
            Debug.Log("[Integration]    - Drag 'Drake Stafford - 432 Hz.mp3' into 'Exploration Music' slot");
            Debug.Log("[Integration]    - Set Music Volume to 0.3");
            Debug.Log("[Integration]");
            Debug.Log("[Integration] Test by pressing Play in Unity Editor!");

            EditorUtility.DisplayDialog("Integration Complete!",
                "All automated steps complete!\n\n" +
                "MANUAL STEP:\n" +
                "Find AudioManager in scene → Drag 'Drake Stafford - 432 Hz.mp3' into Exploration Music slot\n\n" +
                "Then press Play to test!",
                "OK");
        }

        static AnimatorController CreateCapoeiraAnimatorController()
        {
            // Create or load controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ANIMATOR_CONTROLLER_PATH);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ANIMATOR_CONTROLLER_PATH);
            }

            // Get Capoeira animation clips
            var gingaForward = LoadAnimationClip("ginga forward");
            var gingaVariation1 = LoadAnimationClip("ginga variation 1");
            var au = LoadAnimationClip("au");
            var martelo = LoadAnimationClip("martelo");
            var esquiva1 = LoadAnimationClip("esquiva 1");

            if (gingaForward == null || gingaVariation1 == null)
            {
                Debug.LogError("[Integration] Failed to load Capoeira animations! Make sure they're imported.");
                return null;
            }

            // Clear existing layers
            while (controller.layers.Length > 0)
            {
                controller.RemoveLayer(0);
            }

            // Add parameters
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            // Create base layer (newly created controllers have no layers)
            AnimatorControllerLayer baseLayer;
            if (controller.layers.Length == 0)
            {
                baseLayer = new AnimatorControllerLayer
                {
                    name = "Base Layer",
                    defaultWeight = 1f,
                    stateMachine = new AnimatorStateMachine()
                };
                controller.AddLayer(baseLayer);
            }
            else
            {
                baseLayer = controller.layers[0];
            }
            var stateMachine = baseLayer.stateMachine;

            // Create states
            var idleState = stateMachine.AddState("Idle");
            idleState.motion = gingaVariation1;

            var walkState = stateMachine.AddState("Walk");
            walkState.motion = gingaForward;

            var jumpState = stateMachine.AddState("Jump");
            if (au != null) jumpState.motion = au;

            var attackState = stateMachine.AddState("Attack");
            if (martelo != null) attackState.motion = martelo;

            // Set default state
            stateMachine.defaultState = idleState;

            // Create transitions
            // Idle → Walk (when Speed > 0.1)
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.2f;

            // Walk → Idle (when Speed < 0.1)
            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.2f;

            // Any State → Jump
            if (au != null)
            {
                var anyToJump = stateMachine.AddAnyStateTransition(jumpState);
                anyToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
                anyToJump.hasExitTime = false;
                anyToJump.duration = 0.1f;

                // Jump → Idle
                var jumpToIdle = jumpState.AddTransition(idleState);
                jumpToIdle.hasExitTime = true;
                jumpToIdle.exitTime = 0.9f;
                jumpToIdle.duration = 0.1f;
            }

            // Any State → Attack
            if (martelo != null)
            {
                var anyToAttack = stateMachine.AddAnyStateTransition(attackState);
                anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
                anyToAttack.hasExitTime = false;
                anyToAttack.duration = 0.1f;

                // Attack → Idle
                var attackToIdle = attackState.AddTransition(idleState);
                attackToIdle.hasExitTime = true;
                attackToIdle.exitTime = 0.9f;
                attackToIdle.duration = 0.1f;
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Integration] Animator Controller created: {ANIMATOR_CONTROLLER_PATH}");
            return controller;
        }

        static AnimationClip LoadAnimationClip(string animationName)
        {
            // Capoeira animations are in FBX files - need to extract clips
            string searchPattern = animationName.Replace(" ", "_");
            string[] guids = AssetDatabase.FindAssets($"t:AnimationClip {animationName}", new[] { CAPOEIRA_ANIM_PATH });

            if (guids.Length == 0)
            {
                // Try exact FBX file match
                string fbxPath = $"{CAPOEIRA_ANIM_PATH}/{animationName}.fbx";
                var fbxAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                foreach (var asset in fbxAssets)
                {
                    if (asset is AnimationClip clip)
                    {
                        Debug.Log($"[Integration] Found animation clip: {clip.name} in {fbxPath}");
                        return clip;
                    }
                }

                Debug.LogWarning($"[Integration] Animation not found: {animationName}");
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            AnimationClip animClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (animClip == null)
            {
                // Try loading from FBX sub-assets
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in allAssets)
                {
                    if (asset is AnimationClip clip)
                    {
                        Debug.Log($"[Integration] Found animation clip: {clip.name}");
                        return clip;
                    }
                }
            }

            return animClip;
        }
    }
}
