#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.EditorTools
{
    /// <summary>
    /// itch.io build pipeline --- one-shot Windows Standalone build for Moon 1 drop.
    /// Menu entry: Tartaria/0 ★ MASTER/Build Windows itch.io
    /// Headless entry: Tartaria.EditorTools.ItchBuildPipeline.BuildItchWindowsHeadless
    /// (callable from Unity -executeMethod; returns 0 on success, non-zero on failure
    ///  via EditorApplication.Exit).
    ///
    /// Output:
    ///   Builds/itch_moon1/TARTARIA.exe         (player + data folders)
    ///   Builds/itch_moon1.zip                  (whole folder, zipped)
    ///
    /// Prints SHA256 of the zip on success.
    /// </summary>
    public static class ItchBuildPipeline
    {
        private const string Tag = "[ItchBuildPipeline]";
        private const string OutputFolder = "Builds/itch_moon1";
        private const string OutputExe = "Builds/itch_moon1/TARTARIA.exe";
        private const string ZipPath = "Builds/itch_moon1.zip";

        [MenuItem("Tartaria/0 \u2605 MASTER/Build Windows itch.io", false, 0)]
        public static void BuildItchWindows()
        {
            int code = BuildItchWindowsHeadless();
            if (code == 0)
            {
                EditorUtility.DisplayDialog(
                    "itch.io Build",
                    $"Build succeeded.\n\nExe: {OutputExe}\nZip: {ZipPath}",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "itch.io Build",
                    $"Build FAILED with exit code {code}. See Console / Editor.log.",
                    "OK");
            }
        }

        /// <summary>
        /// Headless entry point. Returns 0 on success, non-zero on failure.
        /// When invoked via -executeMethod, also calls EditorApplication.Exit(code)
        /// so the Unity process exits with the build status.
        /// </summary>
        public static int BuildItchWindowsHeadless()
        {
            int exitCode;
            try
            {
                exitCode = RunBuild();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Tag} Unhandled exception: {ex}");
                exitCode = 99;
            }

            // If invoked via -executeMethod, exit the process with the build status
            // so PowerShell / CI captures it via $LASTEXITCODE.
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
            return exitCode;
        }

        private static int RunBuild()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string absOutputFolder = Path.Combine(projectRoot, OutputFolder);
            string absOutputExe = Path.Combine(projectRoot, OutputExe);
            string absZipPath = Path.Combine(projectRoot, ZipPath);

            Debug.Log($"{Tag} Project root: {projectRoot}");
            Debug.Log($"{Tag} Output folder: {absOutputFolder}");

            // Ensure output folder is clean
            if (Directory.Exists(absOutputFolder))
            {
                Debug.Log($"{Tag} Cleaning existing output folder.");
                try { Directory.Delete(absOutputFolder, recursive: true); }
                catch (Exception ex) { Debug.LogWarning($"{Tag} Could not clean output folder: {ex.Message}"); }
            }
            Directory.CreateDirectory(absOutputFolder);

            if (File.Exists(absZipPath))
            {
                try { File.Delete(absZipPath); }
                catch (Exception ex) { Debug.LogWarning($"{Tag} Could not delete existing zip: {ex.Message}"); }
            }

            // Resolve scene list
            string[] scenes = ResolveScenes();
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError($"{Tag} No scenes resolved for build. Aborting.");
                return 2;
            }

            Debug.Log($"{Tag} Scenes ({scenes.Length}):");
            for (int i = 0; i < scenes.Length; i++)
            {
                Debug.Log($"{Tag}   [{i}] {scenes[i]}");
            }

            // Configure build options
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = absOutputExe,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };

            Debug.Log($"{Tag} Starting BuildPipeline.BuildPlayer ...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            Debug.Log(
                $"{Tag} BuildResult={summary.result} totalErrors={summary.totalErrors} " +
                $"totalWarnings={summary.totalWarnings} size={summary.totalSize} bytes " +
                $"duration={summary.totalTime}");

            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"{Tag} Build did not succeed.");
                return 1;
            }

            if (!File.Exists(absOutputExe))
            {
                Debug.LogError($"{Tag} Build reported success but exe missing at: {absOutputExe}");
                return 3;
            }

            Debug.Log($"{Tag} Build succeeded at: {absOutputExe}");

            // Zip the folder
            try
            {
                Debug.Log($"{Tag} Creating zip: {absZipPath}");
                ZipFile.CreateFromDirectory(
                    absOutputFolder,
                    absZipPath,
                    System.IO.Compression.CompressionLevel.Optimal,
                    includeBaseDirectory: true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Tag} Zip failed: {ex}");
                return 4;
            }

            if (!File.Exists(absZipPath))
            {
                Debug.LogError($"{Tag} Zip reported success but file missing: {absZipPath}");
                return 5;
            }

            long zipBytes = new FileInfo(absZipPath).Length;
            string sha256 = ComputeSha256(absZipPath);

            Debug.Log($"{Tag} Zip OK: {absZipPath} ({zipBytes:N0} bytes)");
            Debug.Log($"{Tag} SHA256: {sha256}");
            Debug.Log($"{Tag} DONE.");

            return 0;
        }

        private static string[] ResolveScenes()
        {
            // EditorBuildSettings.scenes, enabled-only.
            var enabled = EditorBuildSettings.scenes
                .Where(s => s != null && s.enabled && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToArray();

            if (enabled.Length > 0)
            {
                return enabled;
            }

            Debug.LogWarning(
                $"{Tag} EditorBuildSettings.scenes has no enabled entries. " +
                "Falling back to the currently-open scene.");

            // Fall back to the active scene in the editor.
            // In batch mode with no scene loaded this will be empty -> caller aborts.
            Scene active = EditorSceneManager.GetActiveScene();
            if (active.IsValid() && !string.IsNullOrEmpty(active.path))
            {
                return new[] { active.path };
            }

            Debug.LogError(
                $"{Tag} No active scene path available. " +
                "Open a scene in the Editor or populate EditorBuildSettings before building.");
            return Array.Empty<string>();
        }

        private static string ComputeSha256(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(stream);
            var sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
#endif
