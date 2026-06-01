#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor menu: Tartaria/Input/Fix Run In Background + Input Focus
    ///
    /// Bakes "Run In Background" into the PlayerSettings asset AND configures the
    /// Input System for background-friendly play. After running this once, the
    /// settings stick to disk — no script in the scene required to enable input
    /// when the Unity Editor loses OS focus.
    /// </summary>
    public static class FixRunInBackground
    {
        [MenuItem("Tartaria/8 Fix/Run In Background + Input Focus", priority = 860)]
        public static void Run()
        {
            PlayerSettings.runInBackground = true;
            PlayerSettings.visibleInBackground = true;

#if ENABLE_INPUT_SYSTEM
            InputSystem.settings.backgroundBehavior =
                InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            EditorUtility.SetDirty(InputSystem.settings);
#endif

            AssetDatabase.SaveAssets();
            Debug.Log("[FixRunInBackground] PlayerSettings.runInBackground = true. visibleInBackground = true. " +
                      "InputSystem.backgroundBehavior = IgnoreFocus. " +
                      "EditorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView. " +
                      "Settings saved.");
            EditorUtility.DisplayDialog("Run In Background fixed",
                "PlayerSettings + Input System updated and saved.\n\n" +
                "• runInBackground = true\n" +
                "• visibleInBackground = true\n" +
                "• backgroundBehavior = IgnoreFocus\n" +
                "• editorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView\n\n" +
                "Game view will now keep ticking when the Editor loses focus (weather widget, etc).",
                "OK");
        }
    }
}
#endif
