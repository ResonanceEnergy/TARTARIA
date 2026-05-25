using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Input;

namespace Tartaria.Editor
{
    /// <summary>
    /// Auto-assigns the TartariaInputActions asset to PlayerInputHandler in the scene.
    /// Menu: Tartaria > Setup > Assign Input Actions
    /// </summary>
    public static class InputActionsAssigner
    {
        const string InputActionsPath = "Assets/_Project/Input/TartariaInputActions.inputactions";

        [MenuItem("Tartaria/Setup/Assign Input Actions")]
        public static void AssignInputActions()
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (asset == null)
            {
                Debug.LogError($"[InputAssigner] InputActionAsset not found at {InputActionsPath}");
                return;
            }

            var handlers = Object.FindObjectsByType<PlayerInputHandler>(FindObjectsSortMode.None);
            if (handlers.Length == 0)
            {
                Debug.LogWarning("[InputAssigner] No PlayerInputHandler found in scene.");
                return;
            }

            int assigned = 0;
            foreach (var handler in handlers)
            {
                var so = new SerializedObject(handler);
                var prop = so.FindProperty("inputActions");
                if (prop != null && prop.objectReferenceValue == null)
                {
                    prop.objectReferenceValue = asset;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(handler);
                    assigned++;
                }
            }

            Debug.Log($"[InputAssigner] Assigned InputActions to {assigned} handler(s).");
        }
    }
}
