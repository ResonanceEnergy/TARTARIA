using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 Auto-Wire — **LEGACY** one-click wire of InteractableBuilding components
    /// to the 3 cathedral-kit placeholders. Wires the OLD scene placeholder GameObjects
    /// (CrystalSpire_Placeholder, StarDome_Placeholder, HarmonicFountain_Placeholder)
    /// in-place rather than instantiating fresh prefabs.
    ///
    /// **SUPERSEDED by `Moon1BuildOutBuildings.cs`** (menu: `Tartaria/Build Out Moon 1
    /// Buildings (3 Hero)`), which uses the real `Echohaven_CrystalSpire.prefab` /
    /// `Echohaven_StarDome.prefab` / `Echohaven_HarmonicFountain.prefab` (~225 KB each)
    /// and applies the docs/15 §7 burial depths.
    ///
    /// Keep this menu as a fallback for scenes that still have the placeholder
    /// GameObjects; it's now in the `Legacy/` submenu so the canonical workflow stays
    /// at the top of `Tartaria/`.
    /// </summary>
    public static class Moon1AutoWire
    {
        // SUPERSEDED 2026-05-31 — use Tartaria/1 Build/Build Out Moon 1 NPCs (Milo + Anastasia + Lirael + Cassian)
        // [MenuItem("Tartaria/_ Legacy/Auto-Wire Moon 1 Buildings (placeholders)", false, 950)]
        public static void WireMoon1Buildings()
        {
            int wired = 0, skipped = 0;
            var msgs = new System.Text.StringBuilder();

            // Each entry: (GameObject name in scene, building id, display name)
            var targets = new (string sceneName, string id, string display)[]
            {
                ("CrystalSpire_Placeholder",      "crystal_spire",     "The First Note"),
                ("StarDome_Placeholder",          "star_dome",         "The Listeners' Hall"),
                ("HarmonicFountain_Placeholder",  "harmonic_fountain", "Thread of Memory"),
                // Fallbacks if the user renamed them already
                ("Echohaven_CrystalSpire",        "crystal_spire",     "The First Note"),
                ("Echohaven_StarDome",            "star_dome",         "The Listeners' Hall"),
                ("Echohaven_HarmonicFountain",    "harmonic_fountain", "Thread of Memory"),
            };

            foreach (var t in targets)
            {
                var go = GameObject.Find(t.sceneName);
                if (go == null)
                {
                    msgs.AppendLine($"  [skip] '{t.sceneName}' not in scene");
                    skipped++;
                    continue;
                }

                // Check if already has the component
                var existing = go.GetComponent<InteractableBuilding>();
                if (existing != null)
                {
                    msgs.AppendLine($"  [skip] '{t.sceneName}' already has InteractableBuilding");
                    skipped++;
                    continue;
                }

                // Need a SphereCollider for the trigger (InteractableBuilding [RequireComponent])
                var col = go.GetComponent<SphereCollider>();
                if (col == null)
                {
                    col = go.AddComponent<SphereCollider>();
                    col.isTrigger = true;
                    col.radius = 5f;
                }

                var ib = go.AddComponent<InteractableBuilding>();

                // Use SerializedObject to set private [SerializeField] values
                var so = new SerializedObject(ib);
                so.FindProperty("buildingId").stringValue = t.id;
                so.FindProperty("displayName").stringValue = t.display;
                so.FindProperty("nodeCount").intValue = 3;
                so.FindProperty("interactRadius").floatValue = 5f;
                so.FindProperty("restorationRsReward").floatValue = 50f;
                so.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(go);
                msgs.AppendLine($"  [WIRED] '{t.sceneName}' → id={t.id}, nodes=3, radius=5");
                wired++;
            }

            // Mark scene dirty + save
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            string summary = "Moon 1 Auto-Wire: " + wired + " building(s) wired, " + skipped + " skipped.\n\n" + msgs;
            Debug.Log("[Moon1AutoWire]\n" + summary);
            EditorUtility.DisplayDialog("Moon 1 Auto-Wire", summary, "OK");
        }
    }
}
