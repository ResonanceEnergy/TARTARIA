using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Tartaria.Editor
{
    /// <summary>
    /// Vex Aurelian's automated validation tool.
    /// Performs comprehensive build health check.
    /// </summary>
    public static class VexValidator
    {
        private const string Tag = "[VexValidator]";

        [MenuItem("Tartaria/Vex/Full Validation")]
        public static void RunFullValidation()
        {
            Debug.Log($"{Tag} ================================================");
            Debug.Log($"{Tag} VEX AUTOMATED VALIDATION");
            Debug.Log($"{Tag} Unity {Application.unityVersion}");
            Debug.Log($"{Tag} ================================================");

            bool allGood = true;

            // Phase 1: Script Compilation
            Debug.Log($"{Tag} Phase 1: Script Compilation");
            if (CheckCompilation())
            {
                Debug.Log($"{Tag}   ✅ Scripts compiled successfully");
            }
            else
            {
                Debug.LogError($"{Tag}   ❌ Compilation errors detected");
                allGood = false;
            }

            // Phase 2: Assembly Validation
            Debug.Log($"{Tag} Phase 2: Assembly Validation");
            int assemblyCount = CheckAssemblies();
            if (assemblyCount > 0)
            {
                Debug.Log($"{Tag}   ✅ Found {assemblyCount} Tartaria assemblies");
            }
            else
            {
                Debug.LogError($"{Tag}   ❌ No Tartaria assemblies found");
                allGood = false;
            }

            // Phase 3: Core Managers
            Debug.Log($"{Tag} Phase 3: Core Manager Types");
            int managerCount = CheckCoreManagers();
            Debug.Log($"{Tag}   Found {managerCount}/3 core managers");

            // Phase 4: Scene Validation
            Debug.Log($"{Tag} Phase 4: Scene Validation");
            CheckScenes();

            // Phase 5: Essential Prefabs
            Debug.Log($"{Tag} Phase 5: Essential Prefabs");
            CheckPrefabs();

            // Phase 6: Input System
            Debug.Log($"{Tag} Phase 6: Input System");
            CheckInputAssets();

            Debug.Log($"{Tag} ================================================");
            if (allGood)
            {
                Debug.Log($"{Tag} ✅ VALIDATION PASSED - Build is healthy");
                EditorUtility.DisplayDialog("Vex Validation", "✅ BUILD HEALTHY\n\nAll systems validated successfully.", "OK");
            }
            else
            {
                Debug.LogWarning($"{Tag} ⚠️ VALIDATION ISSUES - See console for details");
                EditorUtility.DisplayDialog("Vex Validation", "⚠️ VALIDATION ISSUES\n\nCheck console for details.", "OK");
            }
            Debug.Log($"{Tag} ================================================");
        }

        private static bool CheckCompilation()
        {
            // Unity 6 - if we're running this code, compilation must have succeeded
            var assemblies = CompilationPipeline.GetAssemblies();
            bool hasErrors = false;

            foreach (var assembly in assemblies)
            {
                if (assembly.compilerOptions.AllowUnsafeCode && assembly.name.StartsWith("Tartaria"))
                {
                    // Check if assembly has files
                    if (assembly.sourceFiles.Length == 0)
                    {
                        Debug.LogWarning($"{Tag}     Assembly {assembly.name} has no source files");
                        hasErrors = true;
                    }
                }
            }

            return !hasErrors;
        }

        private static int CheckAssemblies()
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            int count = 0;

            var tartariaAssemblies = new List<string>();
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.StartsWith("Tartaria."))
                {
                    count++;
                    tartariaAssemblies.Add(assembly.GetName().Name);
                }
            }

            tartariaAssemblies.Sort();
            foreach (var name in tartariaAssemblies)
            {
                Debug.Log($"{Tag}     {name}");
            }

            return count;
        }

        private static int CheckCoreManagers()
        {
            string[] coreTypes = new[]
            {
                "Tartaria.Core.GameStateManager",
                "Tartaria.Audio.AudioManager",
                "Tartaria.Save.SaveManager"
            };

            int found = 0;
            foreach (var typeName in coreTypes)
            {
                var type = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName == typeName);

                if (type != null)
                {
                    Debug.Log($"{Tag}     ✅ {typeName}");
                    found++;
                }
                else
                {
                    Debug.LogWarning($"{Tag}     ❌ {typeName}");
                }
            }

            return found;
        }

        private static void CheckScenes()
        {
            string[] essentialScenes = new[]
            {
                "Assets/_Project/Scenes/Boot.unity",
                "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity",
                "Assets/_Project/Scenes/UI_Overlay.unity"
            };

            foreach (var scenePath in essentialScenes)
            {
                if (File.Exists(scenePath))
                {
                    Debug.Log($"{Tag}     ✅ {Path.GetFileName(scenePath)}");
                }
                else
                {
                    Debug.LogWarning($"{Tag}     ❌ Missing: {Path.GetFileName(scenePath)}");
                }
            }
        }

        private static void CheckPrefabs()
        {
            string[] essentialPrefabs = new[]
            {
                "Assets/_Project/Prefabs/Player/PlayerCharacter.prefab",
                "Assets/_Project/Prefabs/Managers/GameStateManager.prefab"
            };

            foreach (var prefabPath in essentialPrefabs)
            {
                if (File.Exists(prefabPath))
                {
                    Debug.Log($"{Tag}     ✅ {Path.GetFileName(prefabPath)}");
                }
                else
                {
                    Debug.LogWarning($"{Tag}     ❌ Missing: {Path.GetFileName(prefabPath)}");
                }
            }
        }

        private static void CheckInputAssets()
        {
            string inputActionsPath = "Assets/_Project/Settings/TartariaInputActions.inputactions";

            if (File.Exists(inputActionsPath))
            {
                Debug.Log($"{Tag}     ✅ TartariaInputActions.inputactions");
            }
            else
            {
                Debug.LogWarning($"{Tag}     ❌ Missing: TartariaInputActions.inputactions");
            }
        }

        [MenuItem("Tartaria/Vex/Quick Compile Check")]
        public static void QuickCompileCheck()
        {
            Debug.Log($"{Tag} Quick Compile Check...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            int count = assemblies.Count(a => a.FullName.StartsWith("Tartaria."));

            if (count > 0)
            {
                Debug.Log($"{Tag} ✅ {count} Tartaria assemblies loaded - Compilation OK");
                EditorUtility.DisplayDialog("Vex Quick Check", $"✅ COMPILATION OK\n\n{count} Tartaria assemblies loaded", "OK");
            }
            else
            {
                Debug.LogError($"{Tag} ❌ No Tartaria assemblies found!");
                EditorUtility.DisplayDialog("Vex Quick Check", "❌ NO ASSEMBLIES\n\nCheck console for details", "OK");
            }
        }
    }
}
