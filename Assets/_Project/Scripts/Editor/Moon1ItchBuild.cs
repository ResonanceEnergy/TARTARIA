using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 itch.io build pipeline with profiler gate.
    /// Wraps the Moon1ProfilerBaseline runner (sprint-2 PR #20) + Unity BuildPipeline
    /// into one menu action so any build to itch.io fails when avg frame time regresses
    /// past the 16.6ms (60fps) threshold.
    ///
    /// Flow:
    ///   1. Reflectively invoke Tartaria.Editor.Moon1ProfilerBaseline.RunBaseline() if present
    ///   2. Parse Builds/itch_moon1/profile_report.md for avg frame time
    ///   3. Abort if &gt; 16.6 ms (perf regression gate)
    ///   4. BuildPipeline.BuildPlayer -&gt; StandaloneWindows64
    ///   5. Zip Builds/itch_moon1 -&gt; TARTARIA_Moon1_itch.zip
    ///   6. SHA256 the zip, log + dialog the hash
    ///
    /// If the profile report does not exist yet (PR #20 hasn't run), the build still proceeds
    /// but logs a warning that perf gating is bypassed this round.
    /// </summary>
    public static class Moon1ItchBuild
    {
        private const string Tag = "[Moon1ItchBuild]";
        private const string OutputDir = "Builds/itch_moon1";
        private const string OutputExe = "TARTARIA_Moon1.exe";
        private const string OutputZip = "TARTARIA_Moon1_itch.zip";
        private const string ProfileReportRelPath = "Builds/itch_moon1/profile_report.md";
        private const float FrameBudgetMs = 16.6f; // 60 fps target
        private const string ProfilerBaselineTypeName = "Tartaria.Editor.Moon1ProfilerBaseline";
        private const string ProfilerBaselineMethodName = "RunBaseline";

        [MenuItem("Tartaria/0 ★ MASTER/Build Windows itch.io (with profiler gate)", priority = 75)]
        public static void BuildItchWithProfilerGate()
        {
            Debug.Log($"{Tag} =================================================================");
            Debug.Log($"{Tag} TARTARIA Moon 1 -> itch.io build (with profiler gate)");
            Debug.Log($"{Tag} Unity {Application.unityVersion}");
            Debug.Log($"{Tag} =================================================================");

            try
            {
                // ---- Step 1: Run profiler baseline (if the type exists in this build) ----
                bool baselineRan = TryRunProfilerBaseline();

                // ---- Step 2: Parse profile_report.md and gate on frame time ----
                if (!CheckProfilerGate(baselineRan))
                {
                    // Gate refused the build; CheckProfilerGate already showed the dialog + logged.
                    return;
                }

                // ---- Ensure output dir exists & is clean of stale exe ----
                if (!Directory.Exists(OutputDir))
                {
                    Directory.CreateDirectory(OutputDir);
                }

                string exePath = Path.Combine(OutputDir, OutputExe);
                string zipPath = Path.Combine(OutputDir, OutputZip);
                if (File.Exists(zipPath))
                {
                    try { File.Delete(zipPath); } catch (Exception ex) { Debug.LogWarning($"{Tag} Could not delete stale zip: {ex.Message}"); }
                }

                // ---- Step 3: BuildPipeline.BuildPlayer ----
                var scenes = GetEnabledScenesFromBuildSettings();
                if (scenes.Length == 0)
                {
                    string msg = "No enabled scenes in Build Settings. Add at least Echohaven_VerticalSlice.unity and re-run.";
                    Debug.LogError($"{Tag} {msg}");
                    if (!Application.isBatchMode) EditorUtility.DisplayDialog("Build aborted", msg, "OK");
                    return;
                }

                Debug.Log($"{Tag} Building {scenes.Length} scene(s) -> {exePath}");
                foreach (var s in scenes) Debug.Log($"{Tag}   scene: {s}");

                var buildOptions = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = exePath,
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = BuildTargetGroup.Standalone,
                    options = BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
                BuildSummary summary = report.summary;
                Debug.Log($"{Tag} Build result: {summary.result}  ({summary.totalSize / 1024 / 1024} MB, {summary.totalTime})");

                if (summary.result != BuildResult.Succeeded)
                {
                    string msg = $"Unity BuildPipeline failed: {summary.result} (errors={summary.totalErrors})";
                    Debug.LogError($"{Tag} {msg}");
                    if (!Application.isBatchMode) EditorUtility.DisplayDialog("Build aborted", msg, "OK");
                    return;
                }

                // ---- Step 4: Zip the build folder ----
                Debug.Log($"{Tag} Zipping {OutputDir} -> {zipPath}");
                CreateZipExcludingSelf(OutputDir, zipPath);
                Debug.Log($"{Tag} Zip size: {new FileInfo(zipPath).Length / 1024 / 1024} MB");

                // ---- Step 5: SHA256 the zip + log + dialog ----
                string sha = ComputeSha256(zipPath);
                Debug.Log($"{Tag} SHA256({Path.GetFileName(zipPath)}) = {sha}");
                Debug.Log($"{Tag} =================================================================");
                Debug.Log($"{Tag} BUILD COMPLETE -> {zipPath}");
                Debug.Log($"{Tag} =================================================================");

                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "itch.io build complete",
                        $"Output:\n{zipPath}\n\nSHA256:\n{sha}\n\nReady to upload to itch.io.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Tag} EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Build aborted", $"Exception during build:\n{ex.Message}", "OK");
                }
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        // -----------------------------------------------------------------------
        // Step 1 helper: reflectively run the sprint-2 Moon1ProfilerBaseline.
        // We don't take a direct reference to it because that type is owned by PR #20
        // and may not be merged into this branch yet.
        // -----------------------------------------------------------------------
        private static bool TryRunProfilerBaseline()
        {
            try
            {
                Type type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
                    })
                    .FirstOrDefault(t => t != null && t.FullName == ProfilerBaselineTypeName);

                if (type == null)
                {
                    Debug.LogWarning($"{Tag} {ProfilerBaselineTypeName} not present in this build. " +
                                     "Perf gating will be bypassed (PR #20 not merged here).");
                    return false;
                }

                MethodInfo method = type.GetMethod(
                    ProfilerBaselineMethodName,
                    BindingFlags.Public | BindingFlags.Static);

                if (method == null)
                {
                    Debug.LogWarning($"{Tag} {ProfilerBaselineTypeName} found but no static method {ProfilerBaselineMethodName}(). " +
                                     "Perf gating will rely on whatever profile_report.md already exists.");
                    return false;
                }

                Debug.Log($"{Tag} Invoking {ProfilerBaselineTypeName}.{ProfilerBaselineMethodName}() via reflection...");
                method.Invoke(null, null);
                Debug.Log($"{Tag} Profiler baseline run complete.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{Tag} Profiler baseline invocation threw: {ex.Message}. " +
                                 "Continuing without fresh profile data; will check whatever file exists.");
                return false;
            }
        }

        // -----------------------------------------------------------------------
        // Step 2 helper: parse profile_report.md for avg frame time and gate.
        // Returns true if the build may proceed.
        // -----------------------------------------------------------------------
        private static bool CheckProfilerGate(bool baselineRan)
        {
            if (!File.Exists(ProfileReportRelPath))
            {
                string msg = $"No profile report at {ProfileReportRelPath}. " +
                             "Perf gating BYPASSED for this round. " +
                             "Run sprint-2 Moon1ProfilerBaseline once and re-build to enable the gate.";
                Debug.LogWarning($"{Tag} {msg}");
                // Per spec: still proceed when the report doesn't exist.
                return true;
            }

            float? avgFrameMs = ParseAvgFrameTimeMs(ProfileReportRelPath);
            if (!avgFrameMs.HasValue)
            {
                Debug.LogWarning($"{Tag} Could not parse avg frame time from {ProfileReportRelPath}. " +
                                 "Perf gating BYPASSED for this round.");
                return true;
            }

            Debug.Log($"{Tag} profile_report.md avg frame time = {avgFrameMs.Value:F2} ms  (budget {FrameBudgetMs:F2} ms / 60 fps)");

            if (avgFrameMs.Value > FrameBudgetMs)
            {
                string body =
                    $"Average frame time {avgFrameMs.Value:F2} ms exceeds budget {FrameBudgetMs:F2} ms (60 fps).\n\n" +
                    $"Source: {ProfileReportRelPath}\n" +
                    $"Baseline run this build: {(baselineRan ? "yes" : "no")}\n\n" +
                    "Fix perf regression before shipping to itch.io.";
                Debug.LogError($"{Tag} Build aborted -- perf regression: {avgFrameMs.Value:F2} ms > {FrameBudgetMs:F2} ms");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Build aborted — perf regression", body, "OK");
                }
                if (Application.isBatchMode) EditorApplication.Exit(2);
                return false;
            }

            Debug.Log($"{Tag} Perf gate PASS.");
            return true;
        }

        /// <summary>
        /// Scrape the first numeric "avg frame time" value from a markdown report.
        /// Tolerates a few formats the profiler script might emit:
        ///   - "avg frame time: 14.2 ms"
        ///   - "Avg frame time | 14.2 |"
        ///   - "average frame: 14.2ms"
        /// Returns null if none found.
        /// </summary>
        private static float? ParseAvgFrameTimeMs(string path)
        {
            try
            {
                foreach (var rawLine in File.ReadAllLines(path))
                {
                    var line = rawLine.ToLowerInvariant();
                    if (!(line.Contains("avg") && line.Contains("frame"))) continue;

                    // Pull the first number-with-decimal substring from the line
                    var sb = new StringBuilder();
                    bool sawDot = false;
                    foreach (char c in line)
                    {
                        if (char.IsDigit(c)) sb.Append(c);
                        else if (c == '.' && !sawDot && sb.Length > 0) { sb.Append(c); sawDot = true; }
                        else if (sb.Length > 0) break;
                    }

                    if (sb.Length > 0 && float.TryParse(sb.ToString(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float v))
                    {
                        return v;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{Tag} Failed to parse {path}: {ex.Message}");
            }
            return null;
        }

        // -----------------------------------------------------------------------
        // Step 3 helper: pull enabled scenes from Build Settings.
        // -----------------------------------------------------------------------
        private static string[] GetEnabledScenesFromBuildSettings()
        {
            return EditorBuildSettings.scenes
                .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToArray();
        }

        // -----------------------------------------------------------------------
        // Step 4 helper: zip the build folder (excluding the zip itself if it landed there).
        // ZipFile.CreateFromDirectory throws if the destination is inside the source, so we
        // stage a sibling temp dir, copy the contents, then zip that.
        // -----------------------------------------------------------------------
        private static void CreateZipExcludingSelf(string sourceDir, string zipPath)
        {
            string parent = Path.GetDirectoryName(Path.GetFullPath(sourceDir));
            string tempStage = Path.Combine(parent, $"_zipstage_{Path.GetFileName(sourceDir)}_{Guid.NewGuid():N}");

            try
            {
                Directory.CreateDirectory(tempStage);
                CopyDirectory(sourceDir, tempStage, excludeFullPath: Path.GetFullPath(zipPath));
                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(tempStage, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);
            }
            finally
            {
                try { if (Directory.Exists(tempStage)) Directory.Delete(tempStage, recursive: true); }
                catch (Exception ex) { Debug.LogWarning($"{Tag} Failed to clean up zip stage dir: {ex.Message}"); }
            }
        }

        private static void CopyDirectory(string srcDir, string dstDir, string excludeFullPath)
        {
            Directory.CreateDirectory(dstDir);
            foreach (string file in Directory.GetFiles(srcDir))
            {
                if (string.Equals(Path.GetFullPath(file), excludeFullPath, StringComparison.OrdinalIgnoreCase))
                    continue;
                File.Copy(file, Path.Combine(dstDir, Path.GetFileName(file)), overwrite: true);
            }
            foreach (string sub in Directory.GetDirectories(srcDir))
            {
                CopyDirectory(sub, Path.Combine(dstDir, Path.GetFileName(sub)), excludeFullPath);
            }
        }

        // -----------------------------------------------------------------------
        // Step 5 helper: SHA256 of a file as a lowercase hex string.
        // -----------------------------------------------------------------------
        private static string ComputeSha256(string filePath)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = sha.ComputeHash(stream);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
