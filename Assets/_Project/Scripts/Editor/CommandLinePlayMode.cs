using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    public static class CommandLinePlayMode
    {
        public static void EnterPlayModeFromCommandLine()
        {
            Debug.Log("[CommandLinePlayMode] Entering Play mode...");

            var bootScene = "Assets/_Project/Scenes/Boot.unity";
            if (System.IO.File.Exists(bootScene))
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(bootScene);
                EditorApplication.isPlaying = true;
                Debug.Log("[CommandLinePlayMode] Play mode entered");
            }
            else
            {
                Debug.LogError("[CommandLinePlayMode] Boot scene not found");
            }
        }
    }
}
