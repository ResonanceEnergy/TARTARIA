#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/Art/Run Next-150 Blender Batch
    /// Spawns one Blender process per gen-script across 11 new categories:
    /// vehicles+mounts, instruments, cooking/alchemy, weapons, armor,
    /// flora trees, flora small, fauna, containers, arch details, ritual sigils,
    /// extras pack 2. Expected ~141 FBX produced.
    /// </summary>
    public static class Next150BlenderBatch
    {
        const string BLENDER_5 = @"C:\Program Files\Blender Foundation\Blender 5.0\blender.exe";
        const string BLENDER_45 = @"C:\Program Files\Blender Foundation\Blender 4.5\blender.exe";
        const string BLENDER_44 = @"C:\Program Files\Blender Foundation\Blender 4.4\blender.exe";
        const string BLENDER_36 = @"C:\Program Files\Blender Foundation\Blender 3.6\blender.exe";

        static readonly string[] Scripts = {
            "gen_vehicles.py",
            "gen_instruments.py",
            "gen_cooking_alchemy.py",
            "gen_weapons.py",
            "gen_armor.py",
            "gen_flora_trees.py",
            "gen_flora_small.py",
            "gen_fauna.py",
            "gen_containers.py",
            "gen_arch_details.py",
            "gen_ritual_sigils.py",
            "gen_extras_pack2.py",
        };

        [MenuItem("Tartaria/4 Generate Art/Blender — Next 150 (148 assets)", priority = 440)]
        public static void Run()
        {
            string blender = FindBlender();
            if (blender == null)
            {
                EditorUtility.DisplayDialog("Blender not found",
                    "Couldn't locate blender.exe.\nTried Blender 5.0 / 4.5 / 4.4 / 3.6 default install paths.", "OK");
                return;
            }
            string repo = Directory.GetCurrentDirectory();
            string scriptDir = Path.Combine(repo, "tools", "blender");
            int ok = 0, fail = 0;
            foreach (var s in Scripts)
            {
                string scriptPath = Path.Combine(scriptDir, s).Replace("\\", "/");
                if (!File.Exists(scriptPath))
                {
                    Debug.LogWarning("[Next150BlenderBatch] missing: " + s);
                    fail++;
                    continue;
                }
                Debug.Log("[Next150BlenderBatch] Launching: " + s);
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
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                if (proc.ExitCode == 0) ok++; else fail++;
                if (!string.IsNullOrEmpty(stderr) && stderr.Contains("Error"))
                    Debug.LogWarning("[" + s + "] STDERR\n" + stderr);
            }
            AssetDatabase.Refresh();
            string summary = $"Next-150 batch complete.\n\nOK: {ok}\nFailed: {fail}\n\n" +
                             $"FBX → Assets/_Project/Models/Blender/\nPrefab variants auto-created.";
            Debug.Log("[Next150BlenderBatch] " + summary);
            EditorUtility.DisplayDialog("Next-150 Batch", summary, "OK");
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
