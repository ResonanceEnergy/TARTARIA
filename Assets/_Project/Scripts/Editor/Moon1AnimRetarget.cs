#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Tartaria.EditorTools
{
    /// <summary>
    /// Moon1AnimRetarget --- creates the shared <c>EchohavenHumanoid.controller</c>
    /// AnimatorController used by Player + Milo + Cassian + Anastasia + Lirael.
    ///
    /// Per HANDOFFS 2026-06-01 22:30 -> Animation TD (mecanim-humanoid-retarget)
    /// and 2026-06-01 walk-blendtree follow-up (locomotion BlendTree).
    ///
    /// Output: <c>Assets/_Project/Animations/Echohaven/EchohavenHumanoid.controller</c>
    /// Layout:
    ///   - Parameter:  Speed     (Float)  -- drives the Locomotion BlendTree
    ///   - Parameter:  IsWalking (Bool)   -- retained for external consumers
    ///   - State:      Locomotion (default), motion = Simple1D BlendTree on Speed
    ///       child 0: Idle  @ threshold 0.0
    ///       child 1: Walk  @ threshold 0.4
    ///       child 2: Run   @ threshold 0.9
    ///
    /// Motion clips on each BlendTree child are intentionally left null ---
    /// Cowork assigns the KayKit humanoid clips inside Unity (drag-drop into
    /// the Motion slot for each BlendTree child). This keeps the .controller
    /// asset deterministic and avoids GUID drift.
    ///
    /// Idempotent: re-running upgrades a legacy Idle/Walk controller in place
    /// (legacy states are removed, the Locomotion state + BlendTree are
    /// (re)built, child slots are reset to thresholds 0.0 / 0.4 / 0.9 with
    /// null motions preserved if previously assigned).
    /// </summary>
    public static class Moon1AnimRetarget
    {
        const string ControllerDir = "Assets/_Project/Animations/Echohaven";
        const string ControllerPath = ControllerDir + "/EchohavenHumanoid.controller";
        const string SpeedParam = "Speed";
        const string IsWalkingParam = "IsWalking";
        const string LocomotionStateName = "Locomotion";
        const string BlendTreeName = "Locomotion";

        [MenuItem("Tartaria/6 Anim/Create or Refresh EchohavenHumanoid Controller", false, 60)]
        public static void CreateOrRefresh()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Animations");
            }
            if (!AssetDatabase.IsValidFolder(ControllerDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Animations", "Echohaven");
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            bool created = false;
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
                created = true;
            }

            EnsureParameter(controller, SpeedParam, AnimatorControllerParameterType.Float);
            EnsureParameter(controller, IsWalkingParam, AnimatorControllerParameterType.Bool);

            var rootLayer = controller.layers[0];
            var sm = rootLayer.stateMachine;

            // Remove legacy Idle/Walk states left over from the bool-driven controller.
            RemoveStateIfPresent(sm, "Idle");
            RemoveStateIfPresent(sm, "Walk");

            // Find or create the Locomotion state.
            AnimatorState locomotion = FindState(sm, LocomotionStateName);
            BlendTree tree;
            if (locomotion == null)
            {
                // CreateBlendTreeInController adds a state with a fresh BlendTree as its motion.
                tree = controller.CreateBlendTreeInController(LocomotionStateName, out locomotion, rootLayer.stateMachine.states.Length);
                locomotion.name = LocomotionStateName;
            }
            else
            {
                tree = locomotion.motion as BlendTree;
                if (tree == null)
                {
                    tree = new BlendTree { name = BlendTreeName, hideFlags = HideFlags.HideInHierarchy };
                    AssetDatabase.AddObjectToAsset(tree, controller);
                    locomotion.motion = tree;
                }
            }

            sm.defaultState = locomotion;

            // Locomotion is the only state --- no transitions needed.
            ClearTransitions(locomotion);

            // Configure the BlendTree --- Simple1D on Speed, three thresholds.
            tree.name = BlendTreeName;
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = SpeedParam;
            tree.useAutomaticThresholds = false;

            // Preserve previously-assigned Motion clips when refreshing (Cowork's wiring).
            Motion idleMotion = FindChildMotion(tree, 0.0f);
            Motion walkMotion = FindChildMotion(tree, 0.4f);
            Motion runMotion = FindChildMotion(tree, 0.9f);

            tree.children = new[]
            {
                new ChildMotion { motion = idleMotion, threshold = 0.0f, timeScale = 1f, directBlendParameter = SpeedParam },
                new ChildMotion { motion = walkMotion, threshold = 0.4f, timeScale = 1f, directBlendParameter = SpeedParam },
                new ChildMotion { motion = runMotion,  threshold = 0.9f, timeScale = 1f, directBlendParameter = SpeedParam },
            };

            EditorUtility.SetDirty(tree);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "EchohavenHumanoid Controller",
                (created ? "Created" : "Refreshed") + " at:\n" + ControllerPath +
                "\n\nLocomotion BlendTree on Speed param (Idle 0.0 / Walk 0.4 / Run 0.9).\n" +
                "Next: assign humanoid Idle / Walk / Run clips on each BlendTree child Motion slot, then drop this controller onto Player / Milo / Cassian / Anastasia / Lirael Animator components.",
                "OK");
            Selection.activeObject = controller;
        }

        static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            foreach (var p in controller.parameters)
            {
                if (p.name == name && p.type == type) return;
            }
            // If a parameter with the same name exists with a different type, remove it first.
            for (int i = controller.parameters.Length - 1; i >= 0; i--)
            {
                if (controller.parameters[i].name == name)
                {
                    controller.RemoveParameter(i);
                    break;
                }
            }
            controller.AddParameter(name, type);
        }

        static AnimatorState FindState(AnimatorStateMachine sm, string name)
        {
            foreach (var c in sm.states)
            {
                if (c.state != null && c.state.name == name) return c.state;
            }
            return null;
        }

        static void RemoveStateIfPresent(AnimatorStateMachine sm, string name)
        {
            var found = FindState(sm, name);
            if (found != null)
            {
                ClearTransitions(found);
                sm.RemoveState(found);
            }
        }

        static void ClearTransitions(AnimatorState state)
        {
            // Copy because removal mutates the array.
            var existing = state.transitions;
            for (int i = existing.Length - 1; i >= 0; i--)
            {
                state.RemoveTransition(existing[i]);
            }
        }

        static Motion FindChildMotion(BlendTree tree, float threshold)
        {
            if (tree == null || tree.children == null) return null;
            const float epsilon = 0.001f;
            foreach (var child in tree.children)
            {
                if (Mathf.Abs(child.threshold - threshold) <= epsilon) return child.motion;
            }
            return null;
        }
    }
}
#endif
