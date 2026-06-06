#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/MASTER: ALL TIERS (1+2+3+4) — Run Everything
    ///
    /// Sequences every Tier in one click per docs/audits/MOON1_BUILD_AUDIT_2026-05-31.md:
    ///   TIER 1: FBX gen + Terrain + Splats + Lighting (5 sub-steps)
    ///   TIER 2: 4 climactic VFX prefabs (Cathedral / Spire / Giant / 17th-Hour)
    ///   TIER 3: 5 procedural audio clips (Lirael / Skeleton / Stinger / Scout / Milo)
    ///   TIER 4: UI widgets — Ley Line minimap + Aether Band HUD auto-bootstrap at scene load
    ///           (no menu — they self-attach via [RuntimeInitializeOnLoadMethod])
    ///
    /// Total estimated runtime: 4-9 minutes (Blender batches dominate).
    /// Each step is idempotent. Stops on any unhandled exception but tracks per-tier OK/FAIL.
    /// </summary>
    public static class Moon1AllTiersMaster
    {
        // SUPERSEDED by Moon1MasterBootstrap per Sprint 11 L8 50ff78ea (Hammer Lane 6 consolidation 2026-06-02).
        // "Run ALL Tiers" is an asset-pipeline + VFX/Audio gen sequencer, not a scene bootstrap. It also fired the
        // superseded "Tier 1 — FBX + Terrain + Splats + Lighting" menu (now hidden), so this menu would silently
        // fail its first step. Run logic preserved; menu hidden. Re-fire via Moon1AllTiersMaster.Run() if needed.
        // [MenuItem("Tartaria/0 ★ MASTER/Run ALL Tiers (Everything)", priority = 10)]
        public static void Run()
        {
            if (!EditorUtility.DisplayDialog("Moon 1 — Run ALL Tiers",
                "Runs every staged asset action for Moon 1:\n\n" +
                "  TIER 1 — FBX batches + Terrain + Splats + Lighting\n" +
                "  TIER 2 — 4 climactic VFX prefabs\n" +
                "  TIER 3 — 5 procedural audio clips\n" +
                "  TIER 4 — Ley Line + Aether Band HUD (auto-bootstrap)\n\n" +
                "Estimated runtime: 4-9 minutes (Blender batches dominate).\n" +
                "Make sure Blender 4.4 / 4.5 / 5.0 is installed.\n\n" +
                "Proceed?", "Run All Tiers", "Cancel")) return;

            int total = 0, ok = 0;
            var failures = new System.Collections.Generic.List<string>();

            // TIER 1 — single dispatch (the Tier 1 master sequences 5 sub-steps internally)
            total++;
            if (Fire("Tartaria/0 ★ MASTER/Tier 1 — FBX + Terrain + Splats + Lighting"))
                ok++;
            else failures.Add("Tier 1 Master");

            // TIER 2 — VFX
            total++;
            if (Fire("Tartaria/1 Build/Moon 1 — VFX (Cathedral + Spire + Giant + 17th-Hour)"))
                ok++;
            else failures.Add("Tier 2 VFX");

            // TIER 3 — Audio
            total++;
            if (Fire("Tartaria/1 Build/Moon 1 — Audio Lore (Lullaby + Hum + Stinger + Taunt + Chime)"))
                ok++;
            else failures.Add("Tier 3 Audio");

            // TIER 4 — UI widgets auto-bootstrap at runtime via [RuntimeInitializeOnLoadMethod].
            // No menu fire needed — they create themselves on next Play.
            total++;
            ok++; // auto-counts as OK because no fire required
            Debug.Log("[Moon1AllTiersMaster] Tier 4 UI widgets (LeyLineMinimap + AetherBandHUD) will auto-bootstrap on next Play.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string summary = $"ALL TIERS finished.\n\n" +
                             $"OK: {ok}/{total}\n" +
                             (failures.Count > 0 ? $"Failures: {string.Join(", ", failures)}\n\n" : "All steps succeeded.\n\n") +
                             "Next:\n" +
                             "  1. Place Blender Prefabs (Echohaven Scene Dressing)\n" +
                             "  2. Place New Assets (vehicles, weapons, flora, fauna...)\n" +
                             "  3. Acceptance Audit + Combat Verify + Ready Check\n" +
                             "  4. Hit Play. LeyLineMinimap + AetherBandHUD will auto-attach.";
            Debug.Log("[Moon1AllTiersMaster] " + summary);
            EditorUtility.DisplayDialog("ALL TIERS Complete", summary, "OK");
        }

        static bool Fire(string menuPath)
        {
            Debug.Log("[Moon1AllTiersMaster] Firing: " + menuPath);
            try
            {
                bool ok = EditorApplication.ExecuteMenuItem(menuPath);
                if (!ok) Debug.LogWarning("[Moon1AllTiersMaster] Menu not invocable: " + menuPath);
                return ok;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Moon1AllTiersMaster] " + menuPath + " threw: " + e.Message);
                return false;
            }
        }
    }
}
#endif
