using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Tartaria.Input
{
    /// <summary>
    /// Force-set Application.runInBackground = true + Input System background-behavior
    /// BEFORE any scene loads. This beats every other GameObject-attachment race —
    /// if PlayerInputHandler ever fails to attach, this still keeps Update() ticking
    /// when the Unity Editor loses OS focus to the weather widget / other shells.
    ///
    /// Per CLAUDE.md no-stubs mandate — real implementation, no placeholders.
    /// </summary>
    public static class RunInBackgroundGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void EarlyBootstrap()
        {
            Application.runInBackground = true;
#if ENABLE_INPUT_SYSTEM
            try
            {
                InputSystem.settings.backgroundBehavior =
                    InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode =
                    InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[RunInBackgroundGuard] InputSystem settings adjust failed: " + e.Message);
            }
#endif
            Debug.Log("[RunInBackgroundGuard] runInBackground=true + IgnoreFocus + GameViewInput baked at BeforeSceneLoad.");
        }
    }
}
