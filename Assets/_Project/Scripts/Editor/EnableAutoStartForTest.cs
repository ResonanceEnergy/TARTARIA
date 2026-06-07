#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// R100 Test Preparation — One-shot helper to enable Auto-Start Game mode.
    /// Run via Tartaria/9 Debug/Enable Auto-Start Game for Testing before hitting Play.
    /// </summary>
    public static class EnableAutoStartForTest
    {
        const string PrefKey = "Tartaria.Dev.AutoStartGame";
        const string MenuItem = "Tartaria/9 Debug/Enable Auto-Start Game for Testing";

        [MenuItem(MenuItem)]
        static void EnableAutoStart()
        {
            EditorPrefs.SetBool(PrefKey, true);
            EditorPrefs.Save();
            
            // Also update the checkmark on the main toggle
            Menu.SetChecked("Tartaria/9 Debug/Auto-Start Game (Skip Menu)", true);
            
            Debug.Log("[TartariaDevAutoStart] Auto-Start Game ENABLED. Now click Play to verify R99 fixes.");
            Debug.Log("[Verification Checklist]");
            Debug.Log("  ✓ Scene loads with 0 console errors");
            Debug.Log("  ✓ Player visible at spawn (no magenta, no T-pose)");
            Debug.Log("  ✓ NO golden mandala/eye blocking view (R97 fix)");
            Debug.Log("  ✓ Menu auto-skipped after 2-3 seconds (R99 fix)");
            Debug.Log("  ✓ HUD visible (Aether bands, compass, stats)");
            Debug.Log("  ✓ Camera follows player smoothly (no overlap)");
        }

        [MenuItem(MenuItem, true)]
        static bool ValidateAutoStart()
        {
            // Always available in editor
            return true;
        }
    }
}
#endif
