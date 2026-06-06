using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Tartaria.EditorTools
{
    /// <summary>
    /// Rebuilds Assets/_Project/Animations/PlayerAnimatorController.controller
    /// with a single Base Layer + a default empty state, ensuring required
    /// parameters (Speed/IsGrounded/Jump/Attack) exist exactly once.
    ///
    /// Kills the recurring Unity warning:
    ///   "Statemachine for layer 'Base Layer' is missing."
    ///
    /// Idempotent. Run as part of the OneClickBuild pipeline.
    /// </summary>
    public static class PlayerAnimatorControllerFactory
    {
        const string ControllerPath = "Assets/_Project/Animations/PlayerAnimatorController.controller";

        [MenuItem("Tartaria/Fix/Rebuild Player Animator Controller")]
        public static void Run()
        {
            // Ensure folder
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations"))
                AssetDatabase.CreateFolder("Assets/_Project", "Animations");

            // Always recreate to wipe stale duplicate parameters / empty layers.
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
                AssetDatabase.DeleteAsset(ControllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            // CreateAnimatorControllerAtPath gives us one Base Layer with an empty state machine.
            var sm = controller.layers[0].stateMachine;
            var idle = sm.AddState("Idle");
            sm.defaultState = idle;

            // Real motions are bound at runtime by KayKitDeepIntegrator / MixamoAnimatorBinder
            // when humanoid clips are imported. The empty Idle state keeps the controller valid
            // and silences the "Statemachine for layer 'Base Layer' is missing" warning.

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[PlayerAnimatorControllerFactory] Rebuilt PlayerAnimatorController with Base Layer + Idle default state.");
        }
    }
}
