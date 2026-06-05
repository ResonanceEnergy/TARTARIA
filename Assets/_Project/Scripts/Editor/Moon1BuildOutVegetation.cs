#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1BuildOutVegetation — Stage 3 vegetation scatter for Echohaven.
    /// Per docs/15_MVP_BUILD_SPEC.md §7: "Sparse modern weeds on mud layer;
    /// lush Tartarian plants revealed by restoration".
    ///
    /// Scatters KayKit forest pack grass + bushes around the play area, avoiding
    /// building obstacles + the player spawn platform. Parented under
    /// "Echohaven_Vegetation" group, scaled small (single bushes/grass clumps,
    /// not trees) so the player can walk through them.
    /// </summary>
    public static class Moon1BuildOutVegetation
    {
        const string FOREST_DIR = "Assets/_Project/Prefabs/Props/KayKit/Forest/";

        // Variants — pulling from the 8 grass + 22 bush prefabs that exist
        static readonly string[] GRASS_PREFABS = new string[]
        {
            "Prop_Grass_1_A_Color1.prefab",
            "Prop_Grass_1_B_Color1.prefab",
            "Prop_Grass_1_C_Color1.prefab",
            "Prop_Grass_1_D_Color1.prefab",
            "Prop_Grass_2_A_Color1.prefab",
            "Prop_Grass_2_B_Color1.prefab",
            "Prop_Grass_2_C_Color1.prefab",
            "Prop_Grass_2_D_Color1.prefab",
        };

        static readonly string[] BUSH_PREFABS = new string[]
        {
            "Prop_Bush_1_A_Color1.prefab",
            "Prop_Bush_1_C_Color1.prefab",
            "Prop_Bush_1_E_Color1.prefab",
            "Prop_Bush_2_A_Color1.prefab",
            "Prop_Bush_2_C_Color1.prefab",
            "Prop_Bush_2_E_Color1.prefab",
            "Prop_Bush_3_A_Color1.prefab",
            "Prop_Bush_4_A_Color1.prefab",
        };

        const int GRASS_COUNT = 80;
        const int BUSH_COUNT = 40;
        const float SCATTER_RADIUS = 65f; // Echohaven plaza area
        const float SCATTER_MIN_RADIUS = 8f; // don't crowd the spawn platform
        const float NO_GO_RADIUS_PLAYER_SPAWN = 7f;

        // Don't scatter inside building footprints — keep this list aligned with
        // Moon1BuildOutBuildings.SPECS positions.
        static readonly Vector3[] BUILDING_AVOID_POINTS = new Vector3[]
        {
            new Vector3(35f, 0f, 25f),   // Spire
            new Vector3(-30f, 0f, 30f),  // Dome
            new Vector3(5f, 0f, 50f),    // Fountain
        };
        const float BUILDING_AVOID_RADIUS = 10f;
        static readonly Vector3 PLAYER_SPAWN = new Vector3(0f, 2f, -10f);

        [MenuItem("Tartaria/1 Build/Moon 1 — Vegetation (Grass + Bushes)", priority = 130)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Vegetation", "No active scene.", "OK");
                return;
            }

            var parent = GameObject.Find("Echohaven_Vegetation");
            if (parent == null)
            {
                parent = new GameObject("Echohaven_Vegetation");
                Undo.RegisterCreatedObjectUndo(parent, "Create Vegetation group");
            }

            // Wipe old scatter so re-runs don't accumulate
            for (int i = parent.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.transform.GetChild(i).gameObject);
            }

            // Load prefabs once
            var grassPrefabs = LoadPrefabs(FOREST_DIR, GRASS_PREFABS);
            var bushPrefabs  = LoadPrefabs(FOREST_DIR, BUSH_PREFABS);
            if (grassPrefabs.Count == 0 || bushPrefabs.Count == 0)
            {
                EditorUtility.DisplayDialog("Vegetation",
                    $"Could not load prefabs. grass={grassPrefabs.Count}, bush={bushPrefabs.Count}", "OK");
                return;
            }

            // Deterministic seed so re-runs look the same
            var rng = new System.Random(20260530);

            int placed = 0;
            placed += ScatterAround(rng, grassPrefabs, parent.transform, GRASS_COUNT, scaleMin: 0.7f, scaleMax: 1.4f, nameTag: "Grass");
            placed += ScatterAround(rng, bushPrefabs,  parent.transform, BUSH_COUNT,  scaleMin: 0.5f, scaleMax: 1.1f, nameTag: "Bush");

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = parent;

            Debug.Log($"[Moon1BuildOutVegetation] {placed} foliage objects placed under Echohaven_Vegetation.");
            EditorUtility.DisplayDialog("Vegetation",
                $"{placed} grass + bush instances scattered.\n\n" +
                $"Avoiding building footprints (radius 10m around each of 3 buildings) and player spawn (radius 7m).\n\n" +
                "Re-running this menu wipes + re-scatters with the same seed.",
                "OK");
        }

        static List<GameObject> LoadPrefabs(string pathRoot, string[] names)
        {
            var list = new List<GameObject>();
            foreach (var name in names)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(pathRoot + name);
                if (p != null) list.Add(p);
                else Debug.LogWarning($"[Moon1BuildOutVegetation] Missing: {pathRoot + name}");
            }
            return list;
        }

        static int ScatterAround(System.Random rng, List<GameObject> prefabs, Transform parent, int count, float scaleMin, float scaleMax, string nameTag)
        {
            int placed = 0;
            int attempts = 0;
            int maxAttempts = count * 8;

            while (placed < count && attempts < maxAttempts)
            {
                attempts++;
                // Polar scatter, avoiding the inner spawn area
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                float radius = SCATTER_MIN_RADIUS + (float)rng.NextDouble() * (SCATTER_RADIUS - SCATTER_MIN_RADIUS);
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                var pos = new Vector3(x, 0f, z);

                // No-go zone: player spawn
                if (Vector3.Distance(pos, PLAYER_SPAWN) < NO_GO_RADIUS_PLAYER_SPAWN) continue;

                // No-go zone: building footprints
                bool inBuilding = false;
                foreach (var b in BUILDING_AVOID_POINTS)
                {
                    if (Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(b.x, 0, b.z)) < BUILDING_AVOID_RADIUS)
                    {
                        inBuilding = true;
                        break;
                    }
                }
                if (inBuilding) continue;

                var prefab = prefabs[rng.Next(prefabs.Count)];
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                float s = Mathf.Lerp(scaleMin, scaleMax, (float)rng.NextDouble());
                go.transform.localScale = new Vector3(s, s, s);
                go.name = $"{nameTag}_{placed:000}";
                placed++;
            }

            if (placed < count) Debug.LogWarning($"[Moon1BuildOutVegetation] Placed {placed}/{count} {nameTag} (hit max attempts).");
            return placed;
        }
    }
}
#endif
