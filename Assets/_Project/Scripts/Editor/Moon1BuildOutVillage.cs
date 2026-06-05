#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1BuildOutVillage — places the 11 real Blender-FBX-backed village prefabs
    /// from Assets/_Project/Prefabs/Moon1/Blender/ into a believable village layout
    /// around (0, 0, 50). Per CLAUDE.md 2026-05-30 LATE-NIGHT MANDATE rule #4 and #6:
    /// no primitives, no stubs — load the real prefab via AssetDatabase and keep the
    /// prefab link via PrefabUtility.InstantiatePrefab.
    ///
    /// Idempotent: if an instance with the prefab's name already exists under the
    /// Village_Buildings parent, it is left in place and counted as "skipped".
    /// Y is snapped to Terrain.activeTerrain.SampleHeight when a terrain exists.
    /// </summary>
    public static class Moon1BuildOutVillage
    {
        const string PREFAB_DIR = "Assets/_Project/Prefabs/Moon1/Blender/";
        const string PARENT_NAME = "Village_Buildings";
        static readonly Vector3 VILLAGE_CENTER = new Vector3(0f, 0f, 50f);

        struct Placement
        {
            public string prefabName;     // file base name (no .prefab)
            public string category;       // Blender sub-folder (Architecture, NPCs, Props, etc.) — post 2026-06-03 migration
            public Vector3 position;      // world XYZ (Y will be terrain-snapped if possible)
            public bool faceCenter;       // rotate to look toward VILLAGE_CENTER
            public Placement(string name, string cat, Vector3 pos, bool face = true)
            {
                prefabName = name;
                category = cat;
                position = pos;
                faceCenter = face;
            }
        }

        // Spec-locked coordinates from the gap-fix ticket. TownHall is the centerpiece;
        // inn/bakery flank the south approach; well is the central plaza landmark;
        // mill/smithy hold the rear flanks; three cottages form a back row; watchtower
        // east; signpost at the southern entrance.
        static readonly Placement[] PLACEMENTS = new Placement[]
        {
            new Placement("TownHall",         "Architecture", new Vector3(  0f, 0f, 50f), face: false),
            new Placement("VillageInn",       "Architecture", new Vector3(-25f, 0f, 35f)),
            new Placement("VillageBakery",    "Architecture", new Vector3( 25f, 0f, 35f)),
            new Placement("Apothecary",       "Architecture", new Vector3(-40f, 0f, 45f)),
            new Placement("VillageWell",      "Architecture", new Vector3(  0f, 0f, 40f), face: false),
            new Placement("VillageMill",      "Architecture", new Vector3(-30f, 0f, 65f)),
            new Placement("VillageSmithy",    "Architecture", new Vector3( 30f, 0f, 65f)),
            new Placement("VillageCottageA",  "Architecture", new Vector3(-20f, 0f, 80f)),
            new Placement("VillageCottageB",  "Architecture", new Vector3(  0f, 0f, 80f)),
            new Placement("VillageCottageC",  "Architecture", new Vector3( 20f, 0f, 80f)),
            new Placement("Watchtower",       "Architecture", new Vector3( 40f, 0f, 55f)),
            new Placement("VillagerSignpost", "NPCs",         new Vector3(  0f, 0f, 25f), face: true),
        };

        [MenuItem("Tartaria/1 Build/Build Out Moon 1 Village (9 Buildings)", priority = 102)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Village", "No active scene.", "OK");
                return;
            }

            // Find or create the parent. Idempotent — re-runs reuse the same node.
            var parent = GameObject.Find(PARENT_NAME);
            if (parent == null)
            {
                parent = new GameObject(PARENT_NAME);
                Undo.RegisterCreatedObjectUndo(parent, "Create " + PARENT_NAME);
            }

            var terrain = Terrain.activeTerrain;

            int placed = 0;
            int skipped = 0;
            var missing = new List<string>();
            var placedNames = new List<string>();

            foreach (var p in PLACEMENTS)
            {
                // Idempotency check by instance name under parent.
                Transform existing = parent.transform.Find(p.prefabName);
                if (existing != null)
                {
                    skipped++;
                    continue;
                }

                string assetPath = PREFAB_DIR + p.category + "/" + p.prefabName + ".prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null)
                {
                    Debug.LogWarning("[Moon1BuildOutVillage] Missing prefab: " + assetPath);
                    missing.Add(p.prefabName);
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
                if (instance == null)
                {
                    Debug.LogWarning("[Moon1BuildOutVillage] InstantiatePrefab returned null for " + assetPath);
                    missing.Add(p.prefabName);
                    continue;
                }

                instance.name = p.prefabName;

                // Terrain-snap Y if a terrain is in the scene.
                float y = p.position.y;
                if (terrain != null)
                {
                    // SampleHeight is in world space; add terrain transform Y back in.
                    y = terrain.SampleHeight(new Vector3(p.position.x, 0f, p.position.z))
                        + terrain.transform.position.y;
                }
                instance.transform.position = new Vector3(p.position.x, y, p.position.z);

                // Face the village center (TownHall area) so doors look inward. TownHall
                // and the central well keep their authored rotation.
                if (p.faceCenter)
                {
                    Vector3 lookTarget = new Vector3(VILLAGE_CENTER.x, instance.transform.position.y, VILLAGE_CENTER.z);
                    Vector3 dir = lookTarget - instance.transform.position;
                    if (dir.sqrMagnitude > 0.0001f)
                    {
                        instance.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    }
                }
                else
                {
                    instance.transform.rotation = Quaternion.identity;
                }

                Undo.RegisterCreatedObjectUndo(instance, "Place " + p.prefabName);
                placed++;
                placedNames.Add(p.prefabName);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = parent;

            var sb = new StringBuilder();
            sb.Append("Placed ").Append(placed).Append(" of ").Append(PLACEMENTS.Length);
            sb.Append(", skipped ").Append(skipped).Append(" existing");
            sb.Append(", missing ").Append(missing.Count).Append(" prefabs");
            if (missing.Count > 0)
            {
                sb.Append(" (").Append(string.Join(", ", missing)).Append(")");
            }
            string summary = sb.ToString();

            Debug.Log("[Moon1BuildOutVillage] " + summary +
                (placedNames.Count > 0 ? "\n  Placed: " + string.Join(", ", placedNames) : ""));

            EditorUtility.DisplayDialog(
                "Build Out Moon 1 Village",
                summary +
                "\n\nParent: " + PARENT_NAME +
                "\nNext: Window > AI > Navigation > Bake to refresh NavMesh.",
                "OK");
        }
    }
}
#endif
