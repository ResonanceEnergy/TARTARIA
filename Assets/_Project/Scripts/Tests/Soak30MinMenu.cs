// Soak30MinMenu.cs — Editor menu entry for the 30-minute soak test.
// Owned by: QA Engineer agent.
//
// Adds "Tartaria/9 QA/Run 30-Min Soak Test" which calls Soak30Min.RunFromMenu().
// The whole file is gated behind UNITY_EDITOR so the Tests folder still compiles
// in player builds even though it does not have an Editor asmdef.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Tartaria.Tests
{
    /// <summary>
    /// Editor-only menu wrapper for the 30-minute soak test. Must be invoked
    /// while the editor is in Play mode — the soak test relies on coroutines,
    /// physics frame ticking, and Application.persistentDataPath, all of which
    /// behave normally in Play mode.
    /// </summary>
    public static class Soak30MinMenu
    {
        [MenuItem("Tartaria/9 QA/Run 30-Min Soak Test")]
        public static void RunSoakFromMenu()
        {
            if (!EditorApplication.isPlaying)
            {
                bool entered = EditorUtility.DisplayDialog(
                    "Soak30Min requires Play mode",
                    "The 30-minute soak test must run with the game in Play mode so coroutines and physics tick. Enter Play mode now? You will need to re-invoke the menu item after Play mode starts.",
                    "Enter Play mode",
                    "Cancel");
                if (entered)
                {
                    EditorApplication.isPlaying = true;
                }
                return;
            }

            Soak30Min.RunFromMenu();
            Debug.Log("[Soak30MinMenu] Soak controller dispatched via Tartaria/9 QA menu.");
        }
    }
}
#endif
