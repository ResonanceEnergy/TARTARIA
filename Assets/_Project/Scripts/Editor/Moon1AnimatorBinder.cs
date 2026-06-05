#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates Animator Controllers per rig from the KayKit_Character_Animations_1.1 pack
    /// and binds them to every character prefab in Assets/_Project/Prefabs/Characters/.
    /// 
    /// Per CLAUDE.md "no stubs" mandate — creates real AnimatorControllers with real
    /// states + transitions. Without this, every NPC T-poses on Play.
    ///
    /// Menu: Tartaria / Moon 1 / Bind KayKit Animators
    /// </summary>
    public static class Moon1AnimatorBinder
    {
        const string CTRL_DIR = "Assets/_Project/Animation/Controllers";
        const string KAYKIT_ANIM_ROOT = "Assets/KayKit_Character_Animations_1.1/KayKit_Character_Animations_1.1/Animations/fbx";

        // Rig anim files we care about (Medium + Large)
        static readonly string[] RigMediumFiles =
        {
            "Rig_Medium/Rig_Medium_General.fbx",
            "Rig_Medium/Rig_Medium_MovementBasic.fbx",
            "Rig_Medium/Rig_Medium_CombatMelee.fbx",
        };
        static readonly string[] RigLargeFiles =
        {
            "Rig_Large/Rig_Large_General.fbx",
            "Rig_Large/Rig_Large_MovementBasic.fbx",
            "Rig_Large/Rig_Large_CombatMelee.fbx",
        };

        [MenuItem("Tartaria/3 Wire/Bind KayKit Animators", priority = 320)]
        public static void Run()
        {
            EnsureDir(CTRL_DIR);

            var mediumCtrl = BuildController("Rig_Medium_Controller", RigMediumFiles);
            var largeCtrl  = BuildController("Rig_Large_Controller",  RigLargeFiles);

            if (mediumCtrl == null && largeCtrl == null)
            {
                EditorUtility.DisplayDialog("Animator Binder",
                    "Could not locate KayKit animation FBX. Expected at:\n" + KAYKIT_ANIM_ROOT, "OK");
                return;
            }

            int boundMedium = 0, boundLarge = 0;
            // Medium: most characters
            string[] mediumChars =
            {
                "Anastasia", "Cassian", "Lirael", "Korath", "Milo", "Player", "Thorne",
                "KayKit/Char_Knight", "KayKit/Char_Mage", "KayKit/Char_Ranger",
                "KayKit/Char_Rogue", "KayKit/Char_Rogue_Hooded"
            };
            foreach (var c in mediumChars)
                if (BindControllerTo("Assets/_Project/Prefabs/Characters/" + c + ".prefab", mediumCtrl)) boundMedium++;

            // Large: MudGolem + barbarian + skeletons
            string[] largeChars =
            {
                "MudGolem",
                "KayKit/Char_Barbarian",
                "KayKit/Mannequin/Char_Mannequin_Large"
            };
            foreach (var c in largeChars)
                if (BindControllerTo("Assets/_Project/Prefabs/Characters/" + c + ".prefab", largeCtrl)) boundLarge++;

            // Characters/MudGolem.prefab is already bound via largeChars loop above (canonical path post-2026-06-04).
            if (BindControllerTo("Assets/_Project/Prefabs/Enemies/ResetScout.prefab", mediumCtrl)) boundMedium++;

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Animator Binder",
                "Bound Rig_Medium to " + boundMedium + " prefabs, Rig_Large to " + boundLarge + " prefabs.", "OK");
        }

        static AnimatorController BuildController(string name, string[] fbxRelPaths)
        {
            // Gather valid animation clips
            var clips = new System.Collections.Generic.List<AnimationClip>();
            foreach (var rel in fbxRelPaths)
            {
                var full = KAYKIT_ANIM_ROOT + "/" + rel;
                var assets = AssetDatabase.LoadAllAssetsAtPath(full);
                foreach (var a in assets)
                {
                    if (a is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                        clips.Add(clip);
                }
            }
            if (clips.Count == 0) return null;

            string outPath = CTRL_DIR + "/" + name + ".controller";
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(outPath);
            var rootSM = ctrl.layers[0].stateMachine;
            ctrl.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Attack",    AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Die",       AnimatorControllerParameterType.Trigger);

            // Pick reasonable clips for Idle, Walk, Attack, Die
            AnimationClip idleClip   = PickClip(clips, "Idle")   ?? clips.First();
            AnimationClip walkClip   = PickClip(clips, "Walk")   ?? PickClip(clips, "Run") ?? idleClip;
            AnimationClip attackClip = PickClip(clips, "Attack") ?? PickClip(clips, "Punch") ?? PickClip(clips, "Slash") ?? idleClip;
            AnimationClip dieClip    = PickClip(clips, "Die")    ?? PickClip(clips, "Death") ?? PickClip(clips, "Hit") ?? idleClip;

            var sIdle   = rootSM.AddState("Idle",   new Vector3(200, 0,   0));
            var sWalk   = rootSM.AddState("Walk",   new Vector3(450, 0,   0));
            var sAttack = rootSM.AddState("Attack", new Vector3(450, 100, 0));
            var sDie    = rootSM.AddState("Die",    new Vector3(450, 200, 0));

            sIdle.motion   = idleClip;
            sWalk.motion   = walkClip;
            sAttack.motion = attackClip;
            sDie.motion    = dieClip;

            rootSM.defaultState = sIdle;

            // Idle <-> Walk via IsWalking bool
            var idleToWalk = sIdle.AddTransition(sWalk);
            idleToWalk.AddCondition(AnimatorConditionMode.If, 0f, "IsWalking");
            idleToWalk.duration = 0.15f;
            idleToWalk.hasExitTime = false;
            var walkToIdle = sWalk.AddTransition(sIdle);
            walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsWalking");
            walkToIdle.duration = 0.15f;
            walkToIdle.hasExitTime = false;

            // Attack/Die triggers from Any State
            var anyAttack = rootSM.AddAnyStateTransition(sAttack);
            anyAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            anyAttack.duration = 0.05f;
            anyAttack.hasExitTime = false;
            var anyDie = rootSM.AddAnyStateTransition(sDie);
            anyDie.AddCondition(AnimatorConditionMode.If, 0f, "Die");
            anyDie.duration = 0.05f;
            anyDie.hasExitTime = false;

            // After attack, return to idle
            var attackBack = sAttack.AddTransition(sIdle);
            attackBack.hasExitTime = true;
            attackBack.exitTime = 0.9f;
            attackBack.duration = 0.1f;

            AssetDatabase.SaveAssets();
            return ctrl;
        }

        static AnimationClip PickClip(System.Collections.Generic.List<AnimationClip> clips, string contains)
        {
            return clips.FirstOrDefault(c => c.name.IndexOf(contains, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        static bool BindControllerTo(string prefabPath, AnimatorController ctrl)
        {
            if (ctrl == null) return false;
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), prefabPath))) return false;
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null) return false;
            var anim = root.GetComponent<Animator>();
            if (anim == null) anim = root.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl;
            anim.applyRootMotion = false;
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            return true;
        }

        static void EnsureDir(string projectRelative)
        {
            string full = Path.Combine(Directory.GetCurrentDirectory(), projectRelative);
            if (!Directory.Exists(full)) Directory.CreateDirectory(full);
            AssetDatabase.Refresh();
        }
    }
}
#endif
