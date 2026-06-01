#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/Art/Run Next-100 Blender Batch
    ///
    /// Spawns one Blender process per gen-script for the 8 new asset batches:
    /// characters (humanoid + enemies), buildings (village + special), props
    /// (tools + furniture2 + ritual), and minigame props.
    ///
    /// Each gen script ships 6-13 FBX → expected total ≥80 FBX produced under
    /// Assets/_Project/Models/Blender/. BlenderImportPostprocessor auto-creates
    /// the URP/Lit prefab variants on import.
    /// </summary>
    public static class Next100BlenderBatch
    {
        const string BLENDER_5 = @"C:\Program Files\Blender Foundation\Blender 5.0\blender.exe";
        const string BLENDER_45 = @"C:\Program Files\Blender Foundation\Blender 4.5\blender.exe";
        const string BLENDER_44 = @"C:\Program Files\Blender Foundation\Blender 4.4\blender.exe";
        const string BLENDER_36 = @"C:\Program Files\Blender Foundation\Blender 3.6\blender.exe";

        static readonly string[] Scripts = {
            "gen_characters_humanoid.py",
            "gen_characters_enemies.py",
            "gen_buildings_village.py",
            "gen_buildings_special.py",
            "gen_props_tools.py",
            "gen_props_furniture_set2.py",
            "gen_props_ritual.py",
            "gen_minigame_props.py",
            "gen_extras_utility.py",
        };

        [MenuItem("Tartaria/4 Generate Art/Blender — Next 100 (78 assets)", priority = 430)]
        public static void Run()
        {
            string blender = FindBlender();
            if (blender == null)
            {
                EditorUtility.DisplayDialog("Blender not found",
                    "Couldn't locate blender.exe. Tried:\n" +
                    BLENDER_5 + "\n" + BLENDER_45 + "\n" + BLENDER_44 + "\n" + BLENDER_36, "OK");
                return;
            }

            string repo = Directory.GetCurrentDirectory();
            string scriptDir = Path.Combine(repo, "tools", "blender");
            int ok = 0, fail = 0;
            var allOutput = new System.Text.StringBuilder();

            foreach (var s in Scripts)
            {
                string scriptPath = Path.Combine(scriptDir, s).Replace("\\", "/");
                if (!File.Exists(scriptPath)) { Debug.LogWarning("[Next100BlenderBatch] missing: " + s); continue; }
                Debug.Log("[Next100BlenderBatch] Launching: " + s);

                var psi = new ProcessStartInfo
                {
                    FileName = blender,
                    Arguments = "--background --python \"" + scriptPath + "\"",
                    WorkingDirectory = repo,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                var proc = Process.Start(psi);
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                allOutput.AppendLine($"=== {s} === exit {proc.ExitCode}");
                if (proc.ExitCode == 0) ok++; else fail++;
                if (!string.IsNullOrEmpty(stderr) && stderr.Contains("Error"))
                    Debug.LogWarning("[" + s + "] STDERR\n" + stderr);
            }

            AssetDatabase.Refresh();
            string summary = $"Next-100 batch complete.\n\nOK: {ok}\nFailed: {fail}\n\n" +
                             $"FBX output → Assets/_Project/Models/Blender/\n" +
                             $"Prefab variants → Assets/_Project/Prefabs/Moon1/Blender/ (auto-created).";
            Debug.Log("[Next100BlenderBatch] " + summary);
            EditorUtility.DisplayDialog("Next-100 Batch", summary, "OK");
        }

        static string FindBlender()
        {
            foreach (var p in new[] { BLENDER_5, BLENDER_45, BLENDER_44, BLENDER_36 })
                if (File.Exists(p)) return p;
            return null;
        }
    }
}
#endif
