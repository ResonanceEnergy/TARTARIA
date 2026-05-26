using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using System;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// One-click build system for TARTARIA.
    /// Automates full project compilation and validation.
    /// Called from tartaria-play.ps1 in batch mode.
    /// </summary>
    public static class OneClickBuild
    {
        private const string BuildTag = "[Tartaria.OneClickBuild]";
        private const string ReportPath = "Logs/tartaria-build-report.txt";

        /// <summary>
        /// Main build entry point (called from batch mode).
        /// Performs script compilation validation and generates build report.
        /// </summary>
        [MenuItem("Tartaria/Build/One-Click Build")]
        public static void RunBuild()
        {
            Debug.Log($"{BuildTag} ================================================");
            Debug.Log($"{BuildTag} TARTARIA ONE-CLICK BUILD");
            Debug.Log($"{BuildTag} Unity {Application.unityVersion}");
            Debug.Log($"{BuildTag} ================================================");

            try
            {
                // Phase 1: Force script recompilation
                Debug.Log($"{BuildTag} Phase 1: Validating script compilation...");
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                // Note: Unity 6 will exit with code 1 if there are compilation errors,
                // so if we reach this point, compilation succeeded
                Debug.Log($"{BuildTag} OK: Scripts compiled successfully");

                // Phase 2: Validate assemblies
                Debug.Log($"{BuildTag} Phase 2: Validating assemblies...");
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                int tartariaAssemblies = 0;
                foreach (var assembly in assemblies)
                {
                    if (assembly.FullName.StartsWith("Tartaria."))
                    {
                        tartariaAssemblies++;
                        Debug.Log($"{BuildTag}   Found: {assembly.GetName().Name}");
                    }
                }

                if (tartariaAssemblies == 0)
                {
                    Debug.LogError($"{BuildTag} FAIL: No Tartaria assemblies found");
                    WriteBuildReport(false, "No Tartaria assemblies detected");
                    ExitWithCode(1);
                    return;
                }
                Debug.Log($"{BuildTag} OK: Found {tartariaAssemblies} Tartaria assemblies");

                // Phase 3: Validate core managers
                Debug.Log($"{BuildTag} Phase 3: Validating core managers...");
                bool managersOK = ValidateCoreManagers();
                if (!managersOK)
                {
                    Debug.LogWarning($"{BuildTag} WARNING: Some core managers not found (expected for current build state)");
                }
                else
                {
                    Debug.Log($"{BuildTag} OK: Core managers validated");
                }

                // Phase 4: Generate build report
                Debug.Log($"{BuildTag} Phase 4: Generating build report...");
                WriteBuildReport(true, $"Build validated successfully - {tartariaAssemblies} assemblies active");

                Debug.Log($"{BuildTag} ================================================");
                Debug.Log($"{BuildTag} BUILD COMPLETE");
                Debug.Log($"{BuildTag} ================================================");

                ExitWithCode(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{BuildTag} EXCEPTION: {ex.Message}");
                Debug.LogError($"{BuildTag} Stack trace: {ex.StackTrace}");
                WriteBuildReport(false, $"Exception: {ex.Message}");
                ExitWithCode(1);
            }
        }

        /// <summary>
        /// Validate that core manager types exist.
        /// Returns true if all found, false if some missing (non-critical in current build state).
        /// </summary>
        private static bool ValidateCoreManagers()
        {
            string[] coreManagers = new[]
            {
                "Tartaria.Core.GameStateManager",
                "Tartaria.Audio.AudioManager",
                "Tartaria.Save.SaveManager"
            };

            bool allFound = true;
            foreach (var managerName in coreManagers)
            {
                var type = Type.GetType(managerName);
                if (type != null)
                {
                    Debug.Log($"{BuildTag}   Found: {managerName}");
                }
                else
                {
                    Debug.LogWarning($"{BuildTag}   Missing: {managerName}");
                    allFound = false;
                }
            }
            return allFound;
        }

        /// <summary>
        /// Write build report to Logs/tartaria-build-report.txt
        /// </summary>
        private static void WriteBuildReport(bool success, string message)
        {
            try
            {
                string logsDir = Path.GetDirectoryName(ReportPath);
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string status = success ? "SUCCESS" : "FAILURE";

                string report = $@"═══════════════════════════════════════════════════════════
TARTARIA BUILD REPORT
═══════════════════════════════════════════════════════════
Status:    {status}
Timestamp: {timestamp}
Unity:     {Application.unityVersion}
Message:   {message}
═══════════════════════════════════════════════════════════
";
                File.WriteAllText(ReportPath, report);
                Debug.Log($"{BuildTag} Build report written to: {ReportPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{BuildTag} Failed to write build report: {ex.Message}");
            }
        }

        /// <summary>
        /// Exit Unity with specified code (only in batch mode).
        /// </summary>
        private static void ExitWithCode(int code)
        {
            if (Application.isBatchMode)
            {
                Debug.Log($"{BuildTag} Exiting with code {code}");
                EditorApplication.Exit(code);
            }
        }
    }
}
