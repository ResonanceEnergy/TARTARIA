#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/MASTER: Tier 1 (FBX + Terrain + Splats + Lighting)
    ///
    /// One-click sequencer for the Tier 1 asset gap close-out per
    /// docs/audits/MOON1_BUILD_AUDIT_2026-05-31.md. Runs in order:
    ///   1. Next-100 Blender batch (78 FBX: characters/enemies/buildings/tools/furniture/ritual/minigame/extras)
    ///   2. Next-150 Blender batch (148 FBX: vehicles/instruments/cooking/weapons/armor/flora/fauna/containers/arch/sigils)
    ///   3. Terrain heightmap generation (500m + central depression + south ridge)
    ///   4. 4-layer PBR splat painting (Mud/Stone/Grass/Tartarian Tile)
    ///   5. Golden-hour lighting setup + (optional) bake trigger
    ///
    /// Stops on first failure. Each step idempotent.
    /// </summary>
    public static class Moon1Tier1Master
    {
        [MenuItem("Tartaria/0 ★ MASTER/Tier 1 — FBX + Terrain + Splats + Lighting", priority = 20)]
        public static void Run()
        {
            if (!EditorUtility.DisplayDialog("Tier 1 Master",
                "This runs 5 actions in sequence:\n\n" +
                "  1. Blender Next-100 batch (≈ 78 FBX)\n" +
                "  2. Blender Next-150 batch (≈ 148 FBX)\n" +
                "  3. Terrain heightmap (500m + depression)\n" +
                "  4. 4 PBR splat layers\n" +
                "  5. Golden-hour lighting + (optional) bake\n\n" +
                "Blender steps need Blender 4.4 / 4.5 / 5.0 installed at default Windows paths.\n" +
                "Total time ≈ 3-8 minutes (Blender batches dominate).\n\n" +
                "Proceed?", "Run All", "Cancel")) return;

            int stepsOk = 0;
            var failures = new System.Collections.Generic.List<string>();

            // Step 1: Next-100 Blender batch
            if (Step("[1/5] Next-100 Blender batch", () => InvokeMenuOrReport("Tartaria/4 Generate Art/Blender — Next 100 (78 assets)")))
                stepsOk++;
            else failures.Add("Next-100 Blender batch");

            // Step 2: Next-150 Blender batch
            if (Step("[2/5] Next-150 Blender batch", () => InvokeMenuOrReport("Tartaria/4 Generate Art/Blender — Next 150 (148 assets)")))
                stepsOk++;
            else failures.Add("Next-150 Blender batch");

            // Force asset refresh so new FBX import + prefab variants spawn
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            // Step 3: Terrain
            if (Step("[3/5] Terrain heightmap", () => InvokeMenuOrReport("Tartaria/1 Build/Moon 1 — Terrain (500m + Depression)")))
                stepsOk++;
            else failures.Add("Terrain heightmap");

            // Step 4: Splats
            if (Step("[4/5] 4 PBR splat layers", () => InvokeMenuOrReport("Tartaria/1 Build/Moon 1 — Splats (4 PBR layers)")))
                stepsOk++;
            else failures.Add("Splat layers");

            // Step 5: Lighting bake
            if (Step("[5/5] Golden-hour lighting", () => InvokeMenuOrReport("Tartaria/1 Build/Moon 1 — Lighting Bake (Golden Hour)")))
                stepsOk++;
            else failures.Add("Lighting bake");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string summary = $"Tier 1 Master finished.\n\n" +
                             $"Steps OK: {stepsOk}/5\n" +
                             (failures.Count > 0 ? $"Failures: {string.Join(", ", failures)}\n\n" : "") +
                             $"Next: run the canonical 'Build Out Moon 1 *' menus + 'Place Blender Prefabs' + 'Place New Assets'.\n" +
                             $"Then ask Claude for the Tier 2 (VFX) batch.";
            Debug.Log("[Moon1Tier1Master] " + summary);
            EditorUtility.DisplayDialog("Tier 1 Master", summary, "OK");
        }

        static bool Step(string label, System.Func<bool> action)
        {
            Debug.Log("[Moon1Tier1Master] " + label + " — starting");
            try
            {
                bool ok = action();
                Debug.Log("[Moon1Tier1Master] " + label + (ok ? " — OK" : " — FAILED"));
                return ok;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Moon1Tier1Master] " + label + " threw: " + e.Message);
                return false;
            }
        }

        static bool InvokeMenuOrReport(string menuPath)
        {
            bool fired = EditorApplication.ExecuteMenuItem(menuPath);
            if (!fired) Debug.LogWarning("[Moon1Tier1Master] Menu not found / not invocable: " + menuPath);
            return fired;
        }
    }
}
#endif
