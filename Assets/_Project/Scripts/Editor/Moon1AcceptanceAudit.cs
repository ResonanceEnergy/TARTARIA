#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEditor;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 acceptance audit — walks through the CLAUDE.md "no stubs" mandate
    /// checklist and reports green/red for each criterion. Run before declaring
    /// Moon 1 done. Editor menu: Tartaria / Moon 1 / Acceptance Audit.
    ///
    /// Per CLAUDE.md "no stubs" — every check actually inspects scene state,
    /// prefabs on disk, or class presence. No placeholder "TODO check this" lines.
    /// </summary>
    public static class Moon1AcceptanceAudit
    {
        [MenuItem("Tartaria/6 Scene Tools/Acceptance Audit (Moon 1)", priority = 640)]
        public static void Run()
        {
            var report = new StringBuilder();
            int passed = 0;
            int total = 0;

            // 1. Hero buildings — real prefabs on disk
            Check(report, ref total, ref passed, "3 hero building prefabs on disk",
                AssetExists("Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_StarDome.prefab") &&
                AssetExists("Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_HarmonicFountain.prefab") &&
                AssetExists("Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_CrystalSpire.prefab"));

            // 2. Cathedral kit pieces on disk
            Check(report, ref total, ref passed, "Cathedral kit pieces on disk",
                AssetExists("Assets/_Project/Prefabs/Moon1/Cathedral/Foundation_16x16m.prefab") &&
                AssetExists("Assets/_Project/Prefabs/Moon1/Cathedral/Wall_4x4m_Stone.prefab") &&
                AssetExists("Assets/_Project/Prefabs/Moon1/Cathedral/Column_Ornate_6.5m.prefab"));

            // 3. KayKit RPG props pack present
            Check(report, ref total, ref passed, "KayKit RPG Tools pack imported",
                AssetExists("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/anvil.fbx"));

            // 4. New auto-bootstrap component classes present
            Check(report, ref total, ref passed, "Moon1VillageProps script",
                ClassExists("Tartaria.Editor.Moon1VillagePropScatter, Assembly-CSharp-Editor"));
            Check(report, ref total, ref passed, "Moon1EnvironmentDetail script",
                ClassExists("Tartaria.Integration.Moon1EnvironmentDetail, Tartaria.Integration"));
            Check(report, ref total, ref passed, "Moon1Braziers script",
                ClassExists("Tartaria.Integration.Moon1Braziers, Tartaria.Integration"));
            Check(report, ref total, ref passed, "Moon1MudPoolPuzzle script",
                ClassExists("Tartaria.Integration.Moon1MudPoolPuzzle, Tartaria.Integration"));
            Check(report, ref total, ref passed, "Moon1AnastasiaRocker script",
                ClassExists("Tartaria.Integration.Moon1AnastasiaRocker, Tartaria.Integration"));
            Check(report, ref total, ref passed, "Moon1VillagerAmbient script",
                ClassExists("Tartaria.Integration.Moon1VillagerAmbient, Tartaria.Integration"));
            Check(report, ref total, ref passed, "Moon1CombatDirector script",
                ClassExists("Tartaria.Integration.Moon1CombatDirector, Tartaria.Integration"));
            Check(report, ref total, ref passed, "Moon1AudioAtmosphere script",
                ClassExists("Tartaria.Integration.Moon1AudioAtmosphere, Tartaria.Integration"));
            Check(report, ref total, ref passed, "Moon1CinematicMoments script",
                ClassExists("Tartaria.Integration.Moon1CinematicMoments, Tartaria.Integration"));
            Check(report, ref total, ref passed, "Moon1ProgressPersistence script",
                ClassExists("Tartaria.Integration.Moon1ProgressPersistence, Tartaria.Integration"));
            Check(report, ref total, ref passed, "QuestObjectiveTrackerUI fleshed out (>100 lines)",
                LineCount("Assets/_Project/Scripts/UI/QuestObjectiveTrackerUI.cs") > 100);

            // 5. CLAUDE.md mandate section present
            Check(report, ref total, ref passed, "CLAUDE.md late-night no-stubs mandate present",
                FileContains("CLAUDE.md", "LATE-NIGHT MANDATE"));

            // 6. No remaining .candidate files in scripts
            int candidateCount = CountFilesWithExtension("Assets/_Project/Scripts", ".candidate");
            Check(report, ref total, ref passed, "Zero unresolved .candidate files (have " + candidateCount + ")",
                candidateCount == 0);

            // 7. Moon 1 village builder uses Cathedral kit
            //    (Runtime Moon1LevelBuilder.cs was archived 2026-05-31 / Task D1 — pure duplicate.
            //     The Editor canonical Moon1BuildOutVillage.cs owns village build-out + Cathedral kit usage now.)
            Check(report, ref total, ref passed, "Moon1BuildOutVillage uses Cathedral kit prefabs",
                FileContains("Assets/_Project/Scripts/Editor/Moon1BuildOutVillage.cs", "Cathedral")
                || FileContains("Assets/_Project/Scripts/Editor/Moon1BuildOutBuildings.cs", "Cathedral"));

            // 8. Moon1HeroBuildingSpawner loads real prefabs
            Check(report, ref total, ref passed, "Moon1HeroBuildingSpawner uses LoadHeroPrefab",
                FileContains("Assets/_Project/Scripts/Integration/Moon1HeroBuildingSpawner.cs", "LoadHeroPrefab"));

            // 9. Scene exists
            Check(report, ref total, ref passed, "Echohaven_VerticalSlice scene exists",
                AssetExists("Assets/_Project/Scenes/Echohaven_VerticalSlice.unity"));

            // 10. GameEvents has expected events
            Check(report, ref total, ref passed, "GameEvents.OnBuildingRestored event",
                FileContains("Assets/_Project/Scripts/Core/GameEvents.cs", "OnBuildingRestored"));

            string header = "═══ MOON 1 ACCEPTANCE AUDIT ═══\n  " + passed + " / " + total + " checks PASSED\n\n";
            Debug.Log(header + report.ToString());

            string ok = passed == total ? "ALL CHECKS PASSED" : (passed + " / " + total + " passed — see Console");
            EditorUtility.DisplayDialog("Moon 1 Acceptance Audit", header + report.ToString(), "OK");
        }

        // ───────────── helpers ─────────────

        static void Check(StringBuilder report, ref int total, ref int passed, string label, bool ok)
        {
            total++;
            if (ok) passed++;
            report.AppendLine((ok ? "[PASS] " : "[FAIL] ") + label);
        }

        static bool AssetExists(string path)
        {
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null;
        }

        static bool ClassExists(string assemblyQualifiedName)
        {
            return System.Type.GetType(assemblyQualifiedName) != null
                || System.Type.GetType(assemblyQualifiedName.Split(',')[0]) != null;
        }

        static bool FileContains(string relPath, string needle)
        {
            string full = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), relPath);
            if (!System.IO.File.Exists(full)) return false;
            return System.IO.File.ReadAllText(full).Contains(needle);
        }

        static int LineCount(string relPath)
        {
            string full = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), relPath);
            if (!System.IO.File.Exists(full)) return 0;
            return System.IO.File.ReadAllLines(full).Length;
        }

        static int CountFilesWithExtension(string root, string ext)
        {
            string full = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), root);
            if (!System.IO.Directory.Exists(full)) return 0;
            return System.IO.Directory.GetFiles(full, "*" + ext, System.IO.SearchOption.AllDirectories).Length;
       