#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1MegaCleanup — single menu that fixes all 5 visible-mess items per NATRIX's
    /// end-of-session call:
    ///   1. Delete 3 placeholder hero buildings (StarDome_Placeholder, etc.)
    ///   2. Delete 6 wrong-Moon mini-game shells (LeyLineProphecyMiniGame, etc.)
    ///   3. Delete remnants of the old Moon1HeroBuildingSpawner spawn path
    ///   4. Re-tune cathedral interior balance (move Sephiroth/Zodiac to dome floor center)
    ///   5. Apply URP/Lit material to magenta Player capsule (runtime + scene)
    ///
    /// Run after the project exits Safe Mode. Idempotent.
    /// </summary>
    public static class Moon1MegaCleanup
    {
        // Tier-A targets
        static readonly string[] PlaceholderNames =
        {
            "CrystalSpire_Placeholder", "HarmonicFountain_Placeholder", "StarDome_Placeholder",
            "CrystalSpirePlaceholder", "HarmonicFountainPlaceholder", "StarDomePlaceholder"
        };

        static readonly string[] WrongMoonShells =
        {
            "LeyLineProphecyMiniGame", "AquiferPurgeMiniGame", "BellTowerSyncMiniGame",
            "CosmicConvergenceMiniGame", "RailAlignmentMiniGame", "HarmonicRockCutting"
        };

        // Old-spawner remnants — primitive cubes / spheres named after hero buildings
        static readonly string[] OldSpawnerRemnants =
        {
            "Echohaven_StarDome_Primitive", "Echohaven_HarmonicFountain_Primitive", "Echohaven_CrystalSpire_Primitive",
            "Moon1_HeroBuilding_PrimitiveRoot"
        };

        [MenuItem("Tartaria/8 Fix/Moon 1 MEGA Cleanup (5-item visible mess)", priority = 800)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Mega Cleanup", "No active scene.", "OK");
                return;
            }

            var summary = new System.Text.StringBuilder();
            int totalDeleted = 0;
            int playersFixed = 0;
            int interiorMoved = 0;

            // === STEP 1 — Delete placeholder hero buildings ===
            int step1 = DeleteByNames(PlaceholderNames, "placeholder hero buildings");
            summary.AppendLine($"Step 1 — Placeholders deleted: {step1}");
            totalDeleted += step1;

            // === STEP 2 — Delete wrong-Moon mini-game shells ===
            int step2 = DeleteByNames(WrongMoonShells, "wrong-Moon mini-game shells");
            summary.AppendLine($"Step 2 — Wrong-Moon shells deleted: {step2}");
            totalDeleted += step2;

            // === STEP 3 — Delete old Moon1HeroBuildingSpawner remnants ===
            int step3 = DeleteByNames(OldSpawnerRemnants, "old spawner remnants");
            summary.AppendLine($"Step 3 — Old spawner remnants deleted: {step3}");
            totalDeleted += step3;

            // === STEP 4 — Re-tune cathedral interior (move sacred geometry to dome floor) ===
            // Move Sephiroth/Zodiac/Pentagram to align with the now-built dome at stardome position
            var stardome = GameObject.Find("Building_echohaven_stardome");
            Vector3 domeCenter = stardome != null ? stardome.transform.position : new Vector3(-30, 0, 30);
            string[] interiorPieces = { "Pentagram_CathedralFloor", "Sephiroth_Fountain", "Zodiac_FountainFloor", "Lunar_SpireFloor" };
            foreach (var n in interiorPieces)
            {
                var p = GameObject.Find(n);
                if (p != null)
                {
                    Undo.RecordObject(p.transform, "Reposition interior piece");
                    var pos = domeCenter + new Vector3(0f, 0.05f, 0f);
                    p.transform.position = pos;
                    interiorMoved++;
                }
            }
            summary.AppendLine($"Step 4 — Interior pieces re-aligned to dome center: {interiorMoved}");

            // === STEP 5 — Apply URP/Lit material to Player ===
            // Scene-side: find any GameObject tagged Player and any GameObject named Player*
            var playerCandidates = new List<GameObject>();
            try
            {
                foreach (var go in GameObject.FindGameObjectsWithTag("Player")) playerCandidates.Add(go);
            }
            catch { /* tag not registered — skip silently */ }
            foreach (var go in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name.StartsWith("Player") && !playerCandidates.Contains(go)) playerCandidates.Add(go);
            }
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");
            foreach (var go in playerCandidates)
            {
                foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                {
                    var mats = r.sharedMaterials;
                    bool changed = false;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] == null || mats[i].shader == null || mats[i].shader.name.Contains("Hidden/InternalErrorShader"))
                        {
                            mats[i] = new Material(urpLit) { name = "Player_URPLit_Fallback" };
                            mats[i].SetColor("_BaseColor", new Color(0.85f, 0.78f, 0.65f, 1f));
                            mats[i].SetColor("_EmissionColor", new Color(0.1f, 0.08f, 0.05f));
                            changed = true;
                        }
                    }
                    if (changed) { r.sharedMaterials = mats; playersFixed++; }
                }
            }
            summary.AppendLine($"Step 5 — Player renderers fixed (URP/Lit fallback applied): {playersFixed}");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.DisplayDialog("Moon 1 MEGA Cleanup",
                $"Done. Total GameObjects deleted: {totalDeleted}\n\n{summary}\n\n" +
                "Save the scene (Ctrl+S) before hitting Play.",
                "OK");
        }

        static int DeleteByNames(string[] names, string label)
        {
            int count = 0;
            foreach (var n in names)
            {
                var go = GameObject.Find(n);
                if (go != null)
                {
                    Undo.DestroyObjectImmediate(go);
                    count++;
                    continue;
                }
                // case-insensitive substring fallback
                foreach (var candidate in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                {
                    if (candidate.name.IndexOf(n, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Undo.DestroyObjectImmediate(candidate);
                        count++;
                        break;
                    }
                }
            }
            return count;
        }
    }
}
#endif
