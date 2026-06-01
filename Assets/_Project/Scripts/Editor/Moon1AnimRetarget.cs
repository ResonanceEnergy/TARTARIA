#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Tartaria.EditorTools
{
    /// <summary>
    /// Moon1AnimRetarget — creates the shared <c>EchohavenHumanoid.controller</c>
    /// AnimatorController used by Player + Milo + Cassian + Anastasia + Lirael.
    ///
    /// Per HANDOFFS 2026-06-01 22:30 → Animation TD (mecanim-humanoid-retarget).
    ///
    /// Output: <c>Assets/_Project/Animations/Echohaven/EchohavenHumanoid.controller</c>
    /// Layout:
    ///   - Parameter:  IsWalking (Bool)
    ///   - State:      Idle (default)
    ///   - State:      Walk
    ///   - Transition: Idle → Walk when IsWalking == true   (no exit time, 0.1s)
    ///   - Transition: Walk → Idle when IsWalking == false  (no exit time, 0.1s)
    ///
    /// Motion clips are intentionally left null — Cowork assigns the KayKit
    /// humanoid clips inside Unity (drag-drop into Idle/Walk state Motion slot).
    /// This keeps the .controller asset deterministic and avoids GUID drift.
    /// </summary>
    public static class Moon1AnimRetarget
    {
        const string ControllerDir = "Assets/_Project/Animations/Echohaven";
        const string ControllerPath = ControllerDir + "/EchohavenHumanoid.controller";
        const string ParamName = "IsWalking";

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

            EnsureParameter(controller, ParamName, AnimatorControllerParameterType.Bool);

            var rootLayer = controller.layers[0];
            var sm = rootLayer.stateMachine;

            AnimatorState idle = FindState(sm, "Idle") ?? sm.AddState("Idle", new Vector3(250f, 100f, 0f));
            AnimatorState walk = FindState(sm, "Walk") ?? sm.AddState("Walk", new Vector3(500f, 100f, 0f));

            sm.defaultState = idle;

            ClearTransitions(idle);
            ClearTransitions(walk);

            var idleToWalk = idle.AddTransition(walk);
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.1f;
            idleToWalk.AddCondition(AnimatorConditionMode.If, 0f, ParamName);

            var walkToIdle = walk.AddTransition(idle);
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.1f;
            walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamName);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "EchohavenHumanoid Controller",
                (created ? "Created" : "Refreshed") + " at:\n" + ControllerPath +
                "\n\nNext: assign Idle / Walk humanoid clips on the AnimatorState Motion slots, then drop this controller onto Player / Milo / Cassian / Anastasia / Lirael Animator components.",
                "OK");
            Selection.activeObject = controller;
        }

        static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            foreach (var p in controller.parameters)
            {
                if (p.name == name && p.type == type) return;
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

        static void ClearTransitions(AnimatorState state)
        {
            // Copy because removal mutates the array.
            var existing = state.transitions;
            for (int i = existing.Length - 1; i >= 0; i--)
            {
                state.RemoveTransition(existing[i]);
            }
        }
    }
}
#endif
