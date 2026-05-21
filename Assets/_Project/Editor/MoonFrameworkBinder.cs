using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Integration;
using Tartaria.UI;

namespace Tartaria.EditorTools
{
    /// <summary>
    /// B1 Moon Framework v2 — editor pass.
    /// For every Moon scene (and Echohaven), adds a "MoonFramework" GO root
    /// carrying a MoonBeatRunner wired to the matching MoonDefinition asset
    /// and ensures a MoonHUDBanner singleton exists. Idempotent.
    ///
    /// Registered in OneClickBuild as Phase 9j21, after MoonDefinitionsFactory (9j8)
    /// has authored the MoonDefinition SOs and the MoonRuntimeBootstrapper /
    /// MoonMechanicActivator attachments.
    /// </summary>
    public static class MoonFrameworkBinder
    {
        const string MoonsConfigFolder = "Assets/_Project/Config/Moons";
        const string FrameworkRootName = "MoonFramework";

        // (sceneRelativePath, moonNumber) pairs we attach to.
        static readonly (string scenePath, int moonNumber)[] Targets = new (string, int)[]
        {
            ("Assets/_Project/Scenes/Echohaven_VerticalSlice.unity",         1),
            ("Assets/_Project/Scenes/Moons/CrystallineCaverns.unity",        2),
            ("Assets/_Project/Scenes/Moons/TidalArchive.unity",              3),
            ("Assets/_Project/Scenes/Moons/VerdantCanopy.unity",             4),
            ("Assets/_Project/Scenes/Moons/WindsweptHighlands.unity",        5),
            ("Assets/_Project/Scenes/Moons/ClockworkCitadel.unity",          6),
            ("Assets/_Project/Scenes/Moons/SunkenColosseum.unity",           7),
            ("Assets/_Project/Scenes/Moons/DeepForge.unity",                 8),
            ("Assets/_Project/Scenes/Moons/LivingLibrary.unity",             9),
            ("Assets/_Project/Scenes/Moons/AuroralSpire.unity",             10),
            ("Assets/_Project/Scenes/Moons/StarFortBastion.unity",          11),
            ("Assets/_Project/Scenes/Moons/CelestialObservatory.unity",     12),
            ("Assets/_Project/Scenes/Moons/PlanetaryNexus.unity",           13),
        };

        public static void AttachAll()
        {
            int attached = 0, skipped = 0;
            foreach (var (scenePath, moon) in Targets)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    Debug.Log($"[MoonFramework] Skip Moon {moon:D2} — scene not found: {scenePath}");
                    skipped++;
                    continue;
                }

                var def = LoadMoonDefinition(moon);
                if (def == null)
                {
                    Debug.LogWarning($"[MoonFramework] Skip Moon {moon:D2} — MoonDefinition asset missing in {MoonsConfigFolder}.");
                    skipped++;
                    continue;
                }

                // Populate sensible default beat headlines if author has not.
                if (EnsureDefaultBeatHeadlines(def))
                    EditorUtility.SetDirty(def);

                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                var root = GameObject.Find(FrameworkRootName);
                if (root == null)
                    root = new GameObject(FrameworkRootName);

                var runner = root.GetComponent<MoonBeatRunner>();
                if (runner == null) runner = root.AddComponent<MoonBeatRunner>();
                runner.definition = def;
                runner.autoStart  = true;

                // Banner is created at runtime by Show(); no need to add a component here,
                // but we add one so play-mode discovery in editor is obvious.
                if (root.GetComponent<MoonHUDBanner>() == null)
                    root.AddComponent<MoonHUDBanner>();

                EditorSceneManager.MarkSceneDirty(root.scene);
                EditorSceneManager.SaveOpenScenes();
                attached++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[MoonFramework] Attached MoonBeatRunner on {attached} scene(s); skipped {skipped}.");
        }

        static MoonDefinition LoadMoonDefinition(int moon)
        {
            // MoonDefinitionsFactory.Run authors files like "Moon02_CrystallineCaverns.asset"
            // — match by leading "Moon{NN}_" prefix.
            string prefix = $"Moon{moon:D2}_";
            if (!System.IO.Directory.Exists(MoonsConfigFolder))
                return null;
            var guids = AssetDatabase.FindAssets("t:MoonDefinition", new[] { MoonsConfigFolder });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var fname = System.IO.Path.GetFileNameWithoutExtension(path);
                if (fname.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                    return AssetDatabase.LoadAssetAtPath<MoonDefinition>(path);
            }
            // Fallback — load any MoonDefinition with matching .number
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var d = AssetDatabase.LoadAssetAtPath<MoonDefinition>(path);
                if (d != null && d.number == moon) return d;
            }
            return null;
        }

        static bool EnsureDefaultBeatHeadlines(MoonDefinition def)
        {
            if (def.beatHeadlines != null && def.beatHeadlines.Length == 5 && !string.IsNullOrEmpty(def.beatHeadlines[0]))
                return false;
            def.beatHeadlines = new string[]
            {
                $"Discovery — {def.zoneName} stirs.",
                $"Restoration — tune the architecture.",
                $"Conflict — face the {def.mechanic}.",
                $"Climax — hold the resonance.",
                $"Revelation — the {def.zoneName} reveals its secret.",
            };
            if (def.beatDurations == null || def.beatDurations.Length != 5)
                def.beatDurations = new float[] { 4f, 6f, 12f, 8f, 6f };
            return true;
        }
    }
}
