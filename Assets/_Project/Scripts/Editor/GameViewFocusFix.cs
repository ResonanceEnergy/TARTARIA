#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// GameViewFocusFix — works around Unity 6 known bug where the Game view's
    /// "Play Focused" toggle desyncs and the Game view never receives input even
    /// though Application.isFocused is true.
    ///
    /// Refs:
    ///   - https://issuetracker.unity3d.com/issues/game-view-focused-toggle-not-functioning-when-entering-play-mode
    ///   - https://issuetracker.unity3d.com/issues/game-stops-accepting-input-when-the-game-view-undocked-and-re-docked-during-play-mode-using-the-input-system
    ///
    /// On EnteredPlayMode this script:
    ///   1. Finds the GameView via reflection (type is internal to UnityEditor.dll)
    ///   2. Calls Focus() so input routes there
    ///   3. Calls Repaint() to kick the render loop
    /// </summary>
    [InitializeOnLoad]
    public static class GameViewFocusFix
    {
        static GameViewFocusFix()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Defer one frame so the Game view has time to register with the editor.
                EditorApplication.delayCall += FocusGameView;
            }
        }

        [MenuItem("Tartaria/9 Debug/Force Focus Game View", priority = 950)]
        public static void FocusGameView()
        {
            try
            {
                var asm = typeof(EditorWindow).Assembly;
                var gameViewType = asm.GetType("UnityEditor.GameView");
                if (gameViewType == null)
                {
                    Debug.LogWarning("[GameViewFocusFix] UnityEditor.GameView type not found via reflection.");
                    return;
                }
                var window = EditorWindow.GetWindow(gameViewType, false, null, true);
                if (window == null)
                {
                    Debug.LogWarning("[GameViewFocusFix] GameView window null — opening one.");
                    window = EditorWindow.GetWindow(gameViewType);
                }
                window.Focus();
                window.Repaint();
                Debug.Log("[GameViewFocusFix] Game view focused + repainted.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameViewFocusFix] {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
#endif
