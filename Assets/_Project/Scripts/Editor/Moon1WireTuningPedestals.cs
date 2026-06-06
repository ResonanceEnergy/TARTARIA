#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Tartaria.Integration;
using Tartaria.Gameplay;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1WireTuningPedestals — converts the 9 decorative TuningPedestal_0..8
    /// GameObjects in the scene into live tuning interactables.
    ///
    /// Per docs/15 §9 difficulty escalation:
    ///   Node 0 → Variant A (Frequency Slider, 15s ±8%)
    ///   Node 1 → Variant B (Waveform Trace, 20s ±5%)
    ///   Node 2 → Variant C (Harmonic Pattern, 10s ±3%)
    ///
    /// Pedestals 0,3,6 → bound to StarDome      (3 tuning nodes per building)
    /// Pedestals 1,4,7 → bound to HarmonicFountain
    /// Pedestals 2,5,8 → bound to CrystalSpire
    /// </summary>
    public static class Moon1WireTuningPedestals
    {
        [MenuItem("Tartaria/1 Build/Wire Tuning Pedestals (9 → 3 hero buildings)", priority = 107)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Wire Tuning Pedestals", "No active scene.", "OK");
                return;
            }

            string[] heroIds = { "echohaven_stardome", "echohaven_harmonicfountain", "echohaven_crystalspire" };
            int wired = 0;
            int missing = 0;
            var report = new System.Text.StringBuilder();

            for (int i = 0; i < 9; i++)
            {
                var pedestal = GameObject.Find("TuningPedestal_" + i);
                if (pedestal == null)
                {
                    missing++;
                    report.AppendLine("MISSING: TuningPedestal_" + i);
                    continue;
                }

                // Determine target building + node index
                int buildingIdx = i % 3; // 0,1,2 → stardome/fountain/spire
                int nodeIdx = i / 3;     // 0,1,2 → which variant per pedestal in that building
                string buildingId = heroIds[buildingIdx];

                // Ensure SphereCollider trigger for player E-prompt
                var col = pedestal.GetComponent<Collider>();
                if (col == null)
                {
                    var sphere = pedestal.AddComponent<SphereCollider>();
                    sphere.radius = 2.5f;
                    sphere.isTrigger = true;
                }
                else
                {
                    col.isTrigger = true;
                }

                // Attach a TuningPedestalLink component carrying the building + node assignment
                var link = pedestal.GetComponent<TuningPedestalLink>();
                if (link == null) link = pedestal.AddComponent<TuningPedestalLink>();
                link.buildingId = buildingId;
                link.nodeIndex = nodeIdx;
                link.assignedVariant = nodeIdx == 0 ? TuningVariant.FrequencySlider
                                       : nodeIdx == 1 ? TuningVariant.WaveformTrace
                                                       : TuningVariant.HarmonicPattern;

                EditorUtility.SetDirty(pedestal);
                wired++;
                report.AppendLine("WIRED TuningPedestal_" + i + " → " + buildingId + " node " + nodeIdx + " (" + link.assignedVariant + ")");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.DisplayDialog("Wire Tuning Pedestals",
                $"Wired: {wired}\nMissing: {missing}\n\n{report}\n\nSave the scene (Ctrl+S).",
                "OK");
        }
    }
}
#endif
