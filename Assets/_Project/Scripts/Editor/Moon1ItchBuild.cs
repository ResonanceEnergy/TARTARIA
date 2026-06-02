using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 itch.io build entry. Produces a Standalone Win64 player at
    /// Builds/Win64/TARTARIA_Moon1.exe and zips it to Builds/itch_assets/TARTARIA_Moon1.zip
    /// for upload to itch.io via butler.
    ///
    /// Menu:           Tartaria/Marketing/Build itch Win64
    /// Batchmode:      Tartaria.Editor.Moon1ItchBuild.BuildWin64
    ///
    /// Sprint 7 Lane 9 — paired with Moon1ItchScreenshotCapture for the marketing
    /// smoke-test pipeline (scripts/dev/itch-smoke-test.ps1).
    /// </summary>
    public static class Moon1ItchBuild
    {
        private const string Tag = "[Moon1ItchBuild]";
        private const string BuildDirRelative = "Builds/Win64";
        private const string ItchAssetsDirRelative = "Builds/itch_assets";
        private const string ExeName = "TARTARIA_Moon1.exe";
        private const string ZipName = "TARTARIA_Moon1.zip";
        private const string MainScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        private const string BootScenePath = "Assets/_Project/Scenes/Boot.unity";

        [MenuItem("Tartaria/Marketing/Build itch Win64", priority = 810)]
        public static void BuildWin64FromMenu()
        {
            int code = BuildWin64Internal();
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Itch Win64 Build",
                    code == 0 ? "Build succeeded." : $"Build FAILED with exit code {code}. See Console.",
                    "OK");
            }
        }

        /// <summary>
        /// Batchmode entry — exits the editor with the build's exit code so the
        /// PS smoke-test wrapper can capture pass/fail without parsing logs.
        /// </summary>
        public static void BuildWin64()
        {
            Debug.Log($"{Tag} BuildWin64 batchmode entry — Unity {Application.unityVersion}");
            int code = BuildWin64Internal();
            Debug.Log($"{Tag} Build complete with internal exit code {code}.");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(code);
            }
        }

        private static int BuildWin64Internal()
        {
            try
            {
                string repoRoot = Path.GetDirectoryName(Application.dataPath) ?? Directory.GetCurrentDirectory();
                string buildDirAbs = Path.Combine(repoRoot, BuildDirRelative);
                string itchAssetsAbs = Path.Combine(repoRoot, ItchAssetsDirRelative);
                string exeAbs = Path.Combine(buildDirAbs, ExeName);
                string zipAbs = Path.Combine(itchAssetsAbs, ZipName);

                Directory.CreateDirectory(buildDirAbs);
                Directory.CreateDirectory(itchAssetsAbs);

                // Resolve scenes — prefer Boot.unity + Echohaven if both exist,
                // otherwise just Echohaven. This matches the smoke-test contract.
                var scenes = new System.Collections.Generic.List<string>();
                if (File.Exists(BootScenePath)) scenes.Add(BootScenePath);
                if (File.Exists(MainScenePath)) scenes.Add(MainScenePath);
                if (scenes.Count == 0)
                {
                    Debug.LogError($"{Tag} FAIL: no scenes available to build (looked for {BootScenePath}, {MainScenePath}).");
                    return 2;
                }

                Debug.Log($"{Tag} Build target: StandaloneWindows64");
                Debug.Log($"{Tag} Output:       {exeAbs}");
                Debug.Log($"{Tag} Scenes:       {string.Join(", ", scenes)}");

                var opts = new BuildPlayerOptions
                {
                    scenes = scenes.ToArray(),
                    locationPathName = exeAbs,
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = BuildTargetGroup.Standalone,
                    options = BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(opts);
                BuildSummary summary = report.summary;

                Debug.Log($"{Tag} BuildReport: result={summary.result} totalErrors={summary.totalErrors} totalWarnings={summary.totalWarnings} totalSize={summary.totalSize} duration={summary.totalTime}");

                if (summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError($"{Tag} FAIL: build result {summary.result} ({summary.totalErrors} errors).");
                    return 3;
                }

                if (!File.Exists(exeAbs))
                {
                    Debug.LogError($"{Tag} FAIL: exe missing at {exeAbs} after build succeeded.");
                    return 4;
                }

                // Zip the entire build dir for itch upload.
                if (File.Exists(zipAbs))
                {
                    Debug.Log($"{Tag} Removing stale zip {zipAbs}");
                    File.Delete(zipAbs);
                }

                Debug.Log($"{Tag} Compressing {buildDirAbs} -> {zipAbs}");
                System.IO.Compression.ZipFile.CreateFromDirectory(
                    buildDirAbs,
                    zipAbs,
                    System.IO.Compression.CompressionLevel.Optimal,
                    includeBaseDirectory: false);

                var zipInfo = new FileInfo(zipAbs);
                Debug.Log($"{Tag} Zip written: {zipAbs} ({zipInfo.Length / (1024 * 1024)} MB)");

                // Sidecar manifest for downstream tools.
                var manifest = new StringBuilder();
                manifest.AppendLine($"build_target=StandaloneWindows64");
                manifest.AppendLine($"unity_version={Application.unityVersion}");
                manifest.AppendLine($"build_time_utc={DateTime.UtcNow:O}");
                manifest.AppendLine($"exe_path={exeAbs}");
                manifest.AppendLine($"zip_path={zipAbs}");
                manifest.AppendLine($"zip_size_bytes={zipInfo.Length}");
                manifest.AppendLine($"build_result={summary.result}");
                manifest.AppendLine($"errors={summary.totalErrors}");
                manifest.AppendLine($"warnings={summary.totalWarnings}");
                manifest.AppendLine($"duration_seconds={(int)summary.totalTime.TotalSeconds}");
                manifest.AppendLine($"scenes={string.Join(";", scenes)}");
                File.WriteAllText(Path.Combine(itchAssetsAbs, "build_manifest.txt"), manifest.ToString());

                Debug.Log($"{Tag} SUCCESS — itch package ready at {zipAbs}");
                return 0;
            }
            catch (Exception ex)
            {
                // No silent catches per NO-DEBT MANDATE.
                Debug.LogError($"{Tag} FAIL: unhandled {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                return 9;
            }
        }
    }
}
