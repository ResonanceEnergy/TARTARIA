// MixamoAnimatorBinder.cs
// Builds an AnimatorController from any Mixamo .fbx clips fetched into
// Assets/_Project/Models/Animations/ and wires it onto the Player prefab.
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Tartaria.Editor
{
    public static class MixamoAnimatorBinder
    {
        const string AnimDir          = "Assets/_Project/Models/Animations";
        const string ControllerPath   = "Assets/_Project/Models/Animations/PlayerLocomotion.controller";

        [MenuItem("Tartaria/Setup/Bind Mixamo Animator", false, 61)]
        public static void BuildController()
        {
            if (!AssetDatabase.IsValidFolder(AnimDir))
            {
                Debug.Log("[Tartaria][Mixamo] No animations folder — run OpenClaw-FetchAssets.ps1 with $env:MIXAMO_TOKEN first.");
                return;
            }

            var fbxFiles = Directory.GetFiles(AnimDir, "*.fbx").Select(p => p.Replace("\\", "/")).ToArray();
            if (fbxFiles.Length == 0)
            {
                Debug.Log("[Tartaria][Mixamo] No .fbx clips present — skipping animator wiring.");
                return;
            }

            // Force humanoid import on each clip so they can share an avatar.
            foreach (var fbx in fbxFiles)
            {
                var imp = AssetImporter.GetAtPath(fbx) as ModelImporter;
                if (imp == null) continue;
                bool dirty = false;
                if (imp.animationType != ModelImporterAnimationType.Human)
                { imp.animationType = ModelImporterAnimationType.Human; dirty = true; }
                if (!imp.importAnimation) { imp.importAnimation = true; dirty = true; }
                if (dirty) imp.SaveAndReimport();
            }

            // Create / replace the controller.
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Jump",  AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dig",   AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Attack",AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;

            AnimationClip Find(string name) =>
                fbxFiles.Select(f => AssetDatabase.LoadAllAssetsAtPath(f)
                                    .OfType<AnimationClip>()
                                    .FirstOrDefault(c => !c.name.StartsWith("__preview") &&
                                                          c.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0))
                          .FirstOrDefault(c => c != null);

            var idle   = Find("Idle");
            var walk   = Find("Walk");
            var run    = Find("Run");
            var jump   = Find("Jump");
            var dig    = Find("Dig");
            var attack = Find("Slash") ?? Find("Attack");

            AnimatorState idleState   = idle   != null ? sm.AddState("Idle",   new Vector3(300, 0,   0))  : null;
            AnimatorState walkState   = walk   != null ? sm.AddState("Walk",   new Vector3(500, 100, 0))  : null;
            AnimatorState runState    = run    != null ? sm.AddState("Run",    new Vector3(700, 200, 0))  : null;
            AnimatorState jumpState   = jump   != null ? sm.AddState("Jump",   new Vector3(300, 200, 0))  : null;
            AnimatorState digState    = dig    != null ? sm.AddState("Dig",    new Vector3(100, 100, 0))  : null;
            AnimatorState attackState = attack != null ? sm.AddState("Attack", new Vector3(100, 200, 0))  : null;

            if (idleState   != null) { idleState.motion   = idle;   sm.defaultState = idleState; }
            if (walkState   != null) walkState.motion     = walk;
            if (runState    != null) runState.motion      = run;
            if (jumpState   != null) jumpState.motion     = jump;
            if (digState    != null) digState.motion      = dig;
            if (attackState != null) attackState.motion   = attack;

            void Trans(AnimatorState from, AnimatorState to, string param,
                       AnimatorConditionMode mode, float value)
            {
                if (from == null || to == null) return;
                var t = from.AddTransition(to);
                t.AddCondition(mode, value, param);
                t.duration = 0.15f;
                t.hasExitTime = false;
            }

            Trans(idleState, walkState, "Speed", AnimatorConditionMode.Greater, 0.1f);
            Trans(walkState, runState,  "Speed", AnimatorConditionMode.Greater, 4.0f);
            Trans(runState,  walkState, "Speed", AnimatorConditionMode.Less,    4.0f);
            Trans(walkState, idleState, "Speed", AnimatorConditionMode.Less,    0.1f);
            Trans(idleState, jumpState, "Jump", AnimatorConditionMode.If, 0);
            Trans(idleState, digState,  "Dig",  AnimatorConditionMode.If, 0);
            Trans(idleState, attackState, "Attack", AnimatorConditionMode.If, 0);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            // Bind to Player prefab if it exists.
            var playerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
            if (playerPrefab != null)
            {
                var go = PrefabUtility.LoadPrefabContents(playerPrefabPath);
                var anim = go.GetComponent<Animator>();
                if (anim == null) anim = go.AddComponent<Animator>();
                anim.runtimeAnimatorController = controller;
                anim.applyRootMotion = false;
                PrefabUtility.SaveAsPrefabAsset(go, playerPrefabPath);
                PrefabUtility.UnloadPrefabContents(go);
                Debug.Log($"[Tartaria][Mixamo] Animator wired onto {playerPrefabPath}");
            }

            Debug.Log($"[Tartaria][Mixamo] Controller built: {ControllerPath} ({fbxFiles.Length} clips)");
        }
    }
}
