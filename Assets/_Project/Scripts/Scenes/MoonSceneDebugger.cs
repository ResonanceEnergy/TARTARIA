using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Scenes
{
    /// <summary>
    /// R285 — runtime debug binding for MoonSceneLoader.
    /// Hold LeftCtrl + press number key 1-9 to call MoonSceneLoader.Instance.LoadMoon(n).
    /// Hold LeftCtrl + LeftShift + press 1-4 to load Moons 10-13.
    /// Editor-only (or DEBUG build) — does nothing in release.
    /// </summary>
    public class MoonSceneDebugger : MonoBehaviour
    {
        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (!kb.leftCtrlKey.isPressed) return;
            if (MoonSceneLoader.Instance == null) return;

            bool shifted = kb.leftShiftKey.isPressed;

            for (int i = 0; i < 9; i++)
            {
                if (kb[Key.Digit1 + i].wasPressedThisFrame)
                {
                    int moonNum = shifted ? (10 + i) : (1 + i);
                    if (moonNum >= 1 && moonNum <= 13)
                    {
                        Debug.Log($"[MoonSceneDebugger] LeftCtrl{(shifted ? "+Shift" : "")}+{(i + 1)} -> LoadMoon({moonNum})");
                        MoonSceneLoader.Instance.LoadMoon(moonNum);
                    }
                }
            }
        }
    }
}
