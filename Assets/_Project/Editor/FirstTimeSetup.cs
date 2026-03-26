using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Runs once on first Unity open — detects if the project needs setup
    /// and prompts the user to run the ProjectSetupWizard.
    /// </summary>
    [InitializeOnLoad]
    public static class FirstTimeSetup
    {
        const string SetupCompleteKey = "Tartaria_SetupComplete_v1";

        static FirstTimeSetup()
        {
            // Only run once per project
            if (SessionState.GetBool(SetupCompleteKey, false)) return;
            if (EditorPrefs.GetBool(SetupCompleteKey, false)) return;

            // Delay to let Unity finish loading
            EditorApplication.delayCall += OnFirstLoad;
        }

        static void OnFirstLoad()
        {
            // Check if the scene already has our managers
            var bootstrap = Object.FindAnyObjectByType<Core.GameBootstrap>();
            if (bootstrap != null)
            {
                // Already set up
                EditorPrefs.SetBool(SetupCompleteKey, true);
                SessionState.SetBool(SetupCompleteKey, true);
                return;
            }

            bool run = EditorUtility.DisplayDialog(
                "TARTARIA: World of Wonder",
                "Welcome to the TARTARIA project!\n\n" +
                "It looks like the vertical slice scene hasn't been set up yet.\n\n" +
                "This will create:\n" +
                "  - Echohaven scene with all game managers\n" +
                "  - 3 building ScriptableObjects (Dome, Fountain, Spire)\n" +
                "  - Performance profiles & game constants\n" +
                "  - Test geometry with placeholder art\n" +
                "  - Player + Milo + enemy spawn points\n\n" +
                "You can also run this later from: Tartaria > Setup Vertical Slice",
                "Set Up Now", "Skip For Now");

            if (run)
            {
                ProjectSetupWizard.SetupVerticalSlice();
                EditorPrefs.SetBool(SetupCompleteKey, true);
            }

            SessionState.SetBool(SetupCompleteKey, true);
        }
    }
}
