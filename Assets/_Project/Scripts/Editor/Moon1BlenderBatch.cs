#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/Moon 1/Run Blender Batch (Generate All Moon 1 Assets)
    /// Launches Blender in background mode and runs run_all_moon1.py.
    /// </summary>
    public static class Moon1BlenderBatch
    {
        const string BLENDER_DEFAULT_PATHS_64 = @"C:\Program Files\Blender Foundation\Blender 5.0\blender.exe";
        const string BLENDER_DEFAULT_PATHS_4 = @"C:\Program Files\Blender Foundation\Blender 4.4\blender.exe";
        const string BLENDER_DEFAULT_PATHS_3 = @"C:\Program Files\Blender Foundation\Blender 3.6\blender.exe";

        [MenuItem("Tartaria/4 Generate Art/Blender — Moon 1 (original 42 assets)", priority = 420)]
        public static void Run()
        {
            string blender = FindBlender();
            if (blender == null)
            {
                EditorUtility.DisplayDialog("Blender not found",
                    "Couldn't locate blender.exe at expected paths.\nPaste your path here and re-run.\nExpected:\n" +
                    BLENDER_DEFAULT_PATHS_64 + "\n" +
                    BLENDER_DEFAULT_PATHS_4 + "\n" +
                    BLENDER_DEFAULT_PATHS_3, "OK");
                return;
            }

            string repo = Directory.GetCurrentDirectory();
            string script = Path.Combine(repo, "tools", "blender", "run_all_moon1.py").Replace("\\", "/");

            var psi = new ProcessStartInfo
            {
                FileName = blender,
                Arguments = "--background --python \"" + script + "\"",
                WorkingDirectory = repo,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            Debug.Log("[Moon1BlenderBatch] Launching: " + psi.FileName + " " + psi.Arguments);
            var proc = Process.Start(psi);
            string stdout = proc.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            Debug.Log("[Moon1BlenderBatch] === STDOUT ===\n" + stdout);
            if (!string.IsNullOrEmpty(stderr)) Debug.LogWarning("[Moon1BlenderBatch] === STDERR ===\n" + stderr);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Blender Batch",
                "Done. FBX files should now appear in Assets/_Project/Models/Blender/Moon1/\n" +
                "Auto-imported as URP-ready prefabs in Assets/_Project/Prefabs/Moon1/Blender/.\n\n" +
                "Exit code: " + proc.ExitCode, "OK");
        }

        static string FindBlender()
        {
            foreach (var p in new[] { BLENDER_DEFAULT_PATHS_64, BLENDER_DEFAULT_PATHS_4, BLENDER_DEFAULT_PATHS_3 })
                if (File.Exists(p)) return p;
            return null;
        }
    }
}
#endif
