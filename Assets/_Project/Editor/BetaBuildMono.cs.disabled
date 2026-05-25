using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Tartaria.EditorTools
{
    /// <summary>
    /// Beta build with Mono backend (no IL2CPP required).
    /// Invoke: -executeMethod Tartaria.EditorTools.BetaBuildMono.BuildWindows64
    /// </summary>
    public static class BetaBuildMono
    {
        const string OutputDir = "Builds/TARTARIA_Beta_v0.9_Mono";
        const string OutputExe = "Builds/TARTARIA_Beta_v0.9_Mono/TARTARIA.exe";

        public static void BuildWindows64()
        {
            Debug.Log("[BetaBuildMono] Starting beta build with Mono backend...");
            
            // Force Mono2x backend (no IL2CPP required)
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
            
            // Ensure output dir
            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);

            // Get enabled scenes
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[BetaBuildMono] No enabled scenes in EditorBuildSettings.");
                if (Application.isBatchMode)
                    EditorApplication.Exit(2);
                return;
            }

            Debug.Log($"[BetaBuildMono] Building {scenes.Length} scenes to {OutputExe}");
            Debug.Log($"[BetaBuildMono] Scripting Backend: Mono2x");

            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputExe,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);
            var summary = report.summary;

            Debug.Log($"[BetaBuildMono] Result: {summary.result}");
            Debug.Log($"[BetaBuildMono] Size: {summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"[BetaBuildMono] Warnings: {summary.totalWarnings}");
            Debug.Log($"[BetaBuildMono] Errors: {summary.totalErrors}");

            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[BetaBuildMono] BUILD FAILED: {summary.result}");
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"[BetaBuildMono] ✓ BUILD SUCCEEDED");
            Debug.Log($"[BetaBuildMono] Executable: {Path.GetFullPath(OutputExe)}");
            
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }
    }
}
