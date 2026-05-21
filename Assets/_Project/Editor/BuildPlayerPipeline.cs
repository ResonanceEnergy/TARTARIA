using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Tartaria.EditorTools
{
    /// <summary>
    /// Standalone (Windows x64) build pipeline.
    /// Output: Build/Windows/Tartaria.exe
    /// Invoke from menu or batch: -executeMethod Tartaria.EditorTools.BuildPlayerPipeline.BuildWindows
    /// </summary>
    public static class BuildPlayerPipeline
    {
        const string OutputDir  = "Build/Windows";
        const string OutputExe  = "Build/Windows/Tartaria.exe";

        [MenuItem("Tartaria/Build Standalone (Windows x64)")]
        public static void BuildWindows()
        {
            // Ensure output dir
            if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);

            // Use the EditorBuildSettings scene list (already populated by pipeline).
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildPlayerPipeline] No enabled scenes in EditorBuildSettings.");
                EditorApplication.Exit(2);
                return;
            }

            var opts = new BuildPlayerOptions
            {
                scenes           = scenes,
                locationPathName = OutputExe,
                target           = BuildTarget.StandaloneWindows64,
                targetGroup      = BuildTargetGroup.Standalone,
                options          = BuildOptions.None,
            };

            Debug.Log($"[BuildPlayerPipeline] Building {scenes.Length} scenes â†’ {OutputExe}");
            BuildReport report = BuildPipeline.BuildPlayer(opts);

            var summary = report.summary;
            Debug.Log($"[BuildPlayerPipeline] Result: {summary.result} | Size: {summary.totalSize / (1024 * 1024)} MB | Warnings: {summary.totalWarnings} | Errors: {summary.totalErrors}");

            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[BuildPlayerPipeline] BUILD FAILED: {summary.result}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"[BuildPlayerPipeline] BUILD SUCCEEDED â†’ {Path.GetFullPath(OutputExe)}");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        [MenuItem("Tartaria/Reveal Build Output")]
        public static void Reveal()
        {
            var full = Path.GetFullPath(OutputDir);
            if (Directory.Exists(full))
                EditorUtility.RevealInFinder(full);
            else
                Debug.LogWarning($"[BuildPlayerPipeline] Output dir does not exist yet: {full}");
        [MenuItem("Tartaria/Build Development Standalone (Windows x64) — Moon 1 (Echohaven first)")]
        public static void BuildWindowsDevMoon1()
        {
            // Configure for clean Moon 1 direct launch
            MoonScenesFactory.ConfigureMoon1DevBuildSettings();
            OneClickBuild.ConfigureRecommendedPlayerSettings(true);

            if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildPlayerPipeline] No enabled scenes in EditorBuildSettings.");
                if (Application.isBatchMode) EditorApplication.Exit(2);
                return;
            }

            var opts = new BuildPlayerOptions
            {
                scenes           = scenes,
                locationPathName = OutputExe,
                target           = BuildTarget.StandaloneWindows64,
                targetGroup      = BuildTargetGroup.Standalone,
                options          = BuildOptions.DevelopmentBuild | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler | BuildOptions.DetailedBuildReport,
            };

            Debug.Log($"[BuildPlayerPipeline] DEV BUILD (Moon 1): {scenes.Length} scenes — Echohaven_VerticalSlice FIRST ? {OutputExe}");
            BuildReport report = BuildPipeline.BuildPlayer(opts);

            var summary = report.summary;
            Debug.Log($"[BuildPlayerPipeline] Result: {summary.result} | Size: {summary.totalSize / (1024 * 1024)} MB | Warnings: {summary.totalWarnings} | Errors: {summary.totalErrors}");

            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[BuildPlayerPipeline] DEV BUILD FAILED: {summary.result}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"[BuildPlayerPipeline] DEV BUILD SUCCEEDED ? {Path.GetFullPath(OutputExe)}  (Launch the exe for Moon 1 direct!)");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
    }
}
