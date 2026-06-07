#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1AddDialogueRunner — fixes live-Play console warning:
    ///   "[YarnTutorialBinding] No DialogueRunner in scene — cannot play node
    ///    milo_tutorial_step_1_brazier for speaker Milo"
    ///
    /// YarnTutorialBinding.cs uses FindFirstObjectByType&lt;DialogueRunner&gt;.
    /// Echohaven_VerticalSlice has no DialogueRunner GameObject in scene YAML.
    ///
    /// REFLECTION-BASED (R120 fix): the Tartaria.Editor asmdef does NOT reference
    /// the YarnSpinner.Unity asmdef, so we cannot `using Yarn.Unity;` here.
    /// We resolve the DialogueRunner Type at runtime instead. The same pattern is
    /// used in MainMenuOverlay.cs to call Tartaria.Integration without a hard ref.
    ///
    /// Per CLAUDE.md 2026-06-07 NO BSING mandate: this menu only ADDS the component.
    /// It does NOT claim to have verified runtime dialogue playback — the human running
    /// Play mode must confirm the warning stops firing.
    ///
    /// Idempotent. Re-run safely. Adds a child GameObject "DialogueRunner" under
    /// "Moon1_Systems" if a runner is not already in the active scene.
    /// </summary>
    public static class Moon1AddDialogueRunner
    {
        const string MenuPath = "Tartaria/8 Fix/Add DialogueRunner To Scene";
        const string HostObjectName = "DialogueRunner";
        const string ParentName = "Moon1_Systems";
        const string DialogueRunnerTypeName = "Yarn.Unity.DialogueRunner, YarnSpinner.Unity";

        [MenuItem(MenuPath, false, 800)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("[Moon1AddDialogueRunner] No active scene loaded — open Echohaven_VerticalSlice first.");
                return;
            }

            Type runnerType = Type.GetType(DialogueRunnerTypeName);
            if (runnerType == null)
            {
                // Fall back: probe loaded assemblies for the type by full name (handles assembly rename / version churn).
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    runnerType = asm.GetType("Yarn.Unity.DialogueRunner");
                    if (runnerType != null) break;
                }
            }
            if (runnerType == null)
            {
                Debug.LogError("[Moon1AddDialogueRunner] Could not resolve Type 'Yarn.Unity.DialogueRunner' in any loaded assembly. " +
                               "YarnSpinner package may not be installed — check Packages/manifest.json.");
                return;
            }

            // Idempotency: bail if a DialogueRunner already exists in scene.
            var existing = UnityEngine.Object.FindFirstObjectByType(runnerType, FindObjectsInactive.Include) as Component;
            if (existing != null)
            {
                Debug.Log($"[Moon1AddDialogueRunner] DialogueRunner already exists on '{existing.gameObject.name}'. No-op.");
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                return;
            }

            // Find or create the parent.
            GameObject parent = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == ParentName) { parent = root; break; }
            }
            if (parent == null)
            {
                parent = new GameObject(ParentName);
                Undo.RegisterCreatedObjectUndo(parent, "Create Moon1_Systems");
                Debug.LogWarning($"[Moon1AddDialogueRunner] Created missing '{ParentName}' root GameObject.");
            }

            // Reuse a child of that name if it already exists; otherwise create one.
            Transform child = parent.transform.Find(HostObjectName);
            GameObject host;
            if (child != null)
            {
                host = child.gameObject;
            }
            else
            {
                host = new GameObject(HostObjectName);
                Undo.RegisterCreatedObjectUndo(host, "Create DialogueRunner host");
                host.transform.SetParent(parent.transform, false);
            }

            var runner = host.GetComponent(runnerType);
            if (runner == null)
            {
                runner = Undo.AddComponent(host, runnerType);
                Debug.Log($"[Moon1AddDialogueRunner] Added DialogueRunner to '{parent.name}/{host.name}'. " +
                          "NOTE: YarnProject must be assigned in Inspector for nodes to actually exist; " +
                          "without one, NodeExists returns false and YarnTutorialBinding will still skip lines, " +
                          "but the 'No DialogueRunner in scene' warning will stop.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Selection.activeGameObject = host;
            EditorGUIUtility.PingObject(host);
        }
    }
}
#endif
