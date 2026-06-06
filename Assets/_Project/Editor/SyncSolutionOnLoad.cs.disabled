using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Forces Unity to regenerate .sln / .csproj files on domain reload.
    /// Ensures VS Code C# extension can find the solution for IntelliSense.
    /// </summary>
    [InitializeOnLoad]
    static class SyncSolutionOnLoad
    {
        static SyncSolutionOnLoad()
        {
            // Only sync once per editor session
            const string key = "Tartaria_SolutionSynced";
            if (SessionState.GetBool(key, false)) return;
            SessionState.SetBool(key, true);
            Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();
        }
    }
}
