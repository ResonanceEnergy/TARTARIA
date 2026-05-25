// Asset Replacement Batch Runner
// Run from Unity menu: Tartaria > Asset Replacement > Run Full Pipeline

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor.AssetGen
{
    public class AssetReplacementPipeline
    {
        [MenuItem("Tartaria/Asset Replacement/RUN FULL PIPELINE (1-click)")]
        public static void RunFullPipeline()
        {
            Debug.Log("=== TARTARIA ASSET REPLACEMENT PIPELINE ===");
            Debug.Log("Starting full production asset generation...\n");

            // Step 1: Generate all prefabs
            Debug.Log("STEP 1/3: Generating prefabs from KayKit assets...");
            AssetReplacementGenerator.GenerateAllAssets();

            // Step 2: Generate prefab library
            Debug.Log("\nSTEP 2/3: Generating PrefabLibrary.cs...");
            PrefabWireupGenerator.GeneratePrefabLibrary();

            // Step 3: Update spawner scripts
            Debug.Log("\nSTEP 3/3: Updating spawner scripts...");
            PrefabWireupGenerator.ApplySpawnerUpdates();

            Debug.Log("\n=== PIPELINE COMPLETE ===");
            Debug.Log("✓ All production assets generated");
            Debug.Log("✓ Prefab library created");
            Debug.Log("✓ Spawner scripts updated");
            Debug.Log("\nNext: Build project to test (Ctrl+B or Tartaria > Build & Play)");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tartaria/Asset Replacement/1. Generate Assets Only")]
        public static void Step1_GenerateAssets()
        {
            Debug.Log("[Pipeline] Step 1: Generating prefabs...");
            AssetReplacementGenerator.GenerateAllAssets();
        }

        [MenuItem("Tartaria/Asset Replacement/2. Generate Library Only")]
        public static void Step2_GenerateLibrary()
        {
            Debug.Log("[Pipeline] Step 2: Generating library...");
            PrefabWireupGenerator.GeneratePrefabLibrary();
        }

        [MenuItem("Tartaria/Asset Replacement/3. Update Spawners Only")]
        public static void Step3_UpdateSpawners()
        {
            Debug.Log("[Pipeline] Step 3: Updating spawners...");
            PrefabWireupGenerator.ApplySpawnerUpdates();
        }
    }
}
#endif
