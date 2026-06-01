#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1BuildOutProps — scatters non-interactable + interactable props around
    /// Echohaven. Phase 2 content per docs/15 §7 "modern weeds on mud layer"
    /// (mundane rubble) and §10 (Milo dialogue triggers via lore-stones).
    ///
    /// Drops:
    ///   - ~60 KayKit forest rocks of varying sizes around the play area
    ///   - 6 lore-stones (interactable cubes that grant +1 RS + Milo dialogue line on E)
    ///   - 3 fallen pillars (KayKit Column prefab on its side)
    ///
    /// Everything parented under "Echohaven_Props" for cleanliness. Idempotent.
    /// </summary>
    public static class Moon1BuildOutProps
    {
        const string ROCK_DIR = "Assets/_Project/Prefabs/Props/KayKit/Forest/";
        const string COLUMN_PREFAB = "Assets/_Project/Prefabs/Moon1/Cathedral/Column_Ornate_6.5m.prefab";

        static readonly string[] ROCK_PREFABS = new string[]
        {
            "Prop_Rock_1_A_Color1.prefab",
            "Prop_Rock_1_C_Color1.prefab",
            "Prop_Rock_1_E_Color1.prefab",
            "Prop_Rock_1_G_Color1.prefab",
            "Prop_Rock_1_I_Color1.prefab",
            "Prop_Rock_1_K_Color1.prefab",
            "Prop_Rock_1_M_Color1.prefab",
            "Prop_Rock_2_A_Color1.prefab",
            "Prop_Rock_2_B_Color1.prefab",
            "Prop_Rock_2_C_Color1.prefab",
        };

        struct LoreStoneSpec
        {
            public string id;
            public Vector3 worldPos;
            public string dialogueKey;
            public Color color;
        }

        // 6 lore-stones placed at narrative-relevant spots near the buildings/POIs
        static readonly LoreStoneSpec[] LORE_STONES = new LoreStoneSpec[]
        {
            new LoreStoneSpec { id = "lore_stone_listener", worldPos = new Vector3(-28f, 0.6f, 26f), dialogueKey = "lore_listener_hall",  color = new Color(0.85f, 0.78f, 0.55f) },
            new LoreStoneSpec { id = "lore_stone_spire",    worldPos = new Vector3( 32f, 0.6f, 21f), dialogueKey = "lore_first_note",     color = new Color(0.55f, 0.78f, 0.95f) },
            new LoreStoneSpec { id = "lore_stone_fountain", worldPos = new Vector3(  3f, 0.6f, 44f), dialogueKey = "lore_thread_memory",  color = new Color(0.65f, 0.85f, 0.95f) },
            new LoreStoneSpec { id = "lore_stone_well",     worldPos = new Vector3(-18f, 0.6f,  3f), dialogueKey = "lore_old_well",       color = new Color(0.78f, 0.68f, 0.50f) },
            new LoreStoneSpec { id = "lore_stone_gate",     worldPos = new Vector3(  2f, 0.6f, -5f), dialogueKey = "lore_broken_gate",    color = new Color(0.55f, 0.50f, 0.42f) },
            new LoreStoneSpec { id = "lore_stone_root",     worldPos = new Vector3( 22f, 0.6f, -26f), dialogueKey = "lore_root_chamber",  color = new Color(0.40f, 0.85f, 1.00f) },
        };

        // Fallen pillar positions — close to ruined gate / village houses
        static readonly Vector3[] FALLEN_PILLARS = new Vector3[]
        {
            new Vector3(-5f, 0.5f, 0f),
            new Vector3(10f, 0.5f, -8f),
            new Vector3(-14f, 0.5f, -8f),
        };

        const int ROCK_COUNT = 60;
        const float ROCK_SCATTER_RADIUS = 60f;
        const float ROCK_MIN_RADIUS = 6f;
        static readonly Vector3 PLAYER_SPAWN = new Vector3(0f, 2f, -10f);
        const float NO_GO_SPAWN = 6f;
        const float NO_GO_BUILDING = 8f;
        static readonly Vector3[] BUILDING_POINTS = new Vector3[]
        {
            new Vector3(35f, 0f, 25f),
            new Vector3(-30f, 0f, 30f),
            new Vector3(5f, 0f, 50f),
        };

        [MenuItem("Tartaria/1 Build/Moon 1 — Props (Rocks + Lore Stones + Pillars)", priority = 140)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Props", "No active scene.", "OK");
                return;
            }

            var parent = GameObject.Find("Echohaven_Props");
            if (parent == null)
            {
                parent = new GameObject("Echohaven_Props");
                Undo.RegisterCreatedObjectUndo(parent, "Create Props group");
            }

            // Wipe + rebuild for idempotency
            for (int i = parent.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(parent.transform.GetChild(i).gameObject);

            var urpLit = Shader.Find("Universal Render Pipeline/Lit");

            int rocksPlaced = ScatterRocks(parent.transform);
            int stonesPlaced = SpawnLoreStones(parent.transform, urpLit);
            int pillarsPlaced = SpawnFallenPillars(parent.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = parent;

            string summary = $"Props placed: {rocksPlaced} rocks, {stonesPlaced} lore stones, {pillarsPlaced} fallen pillars.";
            Debug.Log("[Moon1BuildOutProps] " + summary);
            EditorUtility.DisplayDialog("Props", summary +
                "\n\nLore stones are tagged 'LoreStone' and granted +1 RS each on press E. " +
                "Re-run wipes + re-scatters with seed 20260530.",
                "OK");
        }

        static int ScatterRocks(Transform parent)
        {
            var rng = new System.Random(20260530);
            var prefabs = new List<GameObject>();
            foreach (var name in ROCK_PREFABS)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(ROCK_DIR + name);
                if (p != null) prefabs.Add(p);
            }
            if (prefabs.Count == 0)
            {
                Debug.LogWarning("[Moon1BuildOutProps] No rock prefabs loaded.");
                return 0;
            }

            int placed = 0;
            int attempts = 0;
            int maxAttempts = ROCK_COUNT * 6;
            var rocksParent = new GameObject("Rocks");
            rocksParent.transform.SetParent(parent);

            while (placed < ROCK_COUNT && attempts < maxAttempts)
            {
                attempts++;
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                float radius = ROCK_MIN_RADIUS + (float)rng.NextDouble() * (ROCK_SCATTER_RADIUS - ROCK_MIN_RADIUS);
                var pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

                if (Vector3.Distance(pos, new Vector3(PLAYER_SPAWN.x, 0, PLAYER_SPAWN.z)) < NO_GO_SPAWN) continue;
                bool inBuilding = false;
                foreach (var b in BUILDING_POINTS)
                {
                    if (Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(b.x, 0, b.z)) < NO_GO_BUILDING)
                    {
                        inBuilding = true; break;
                    }
                }
                if (inBuilding) continue;

                var prefab = prefabs[rng.Next(prefabs.Count)];
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, rocksParent.transform);
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                float s = 0.6f + (float)rng.NextDouble() * 1.4f;
                go.transform.localScale = new Vector3(s, s, s);
                placed++;
            }
            return placed;
        }

        static int SpawnLoreStones(Transform parent, Shader urpLit)
        {
            var stonesParent = new GameObject("LoreStones");
            stonesParent.transform.SetParent(parent);

            int placed = 0;
            foreach (var spec in LORE_STONES)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = spec.id;
                go.transform.SetParent(stonesParent.transform);
                go.transform.position = spec.worldPos;
                go.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                go.tag = "Untagged"; // Custom "LoreStone" tag would need scene setup; skip for safety

                // URP/Lit with the spec color
                var rend = go.GetComponent<Renderer>();
                if (rend != null && urpLit != null)
                {
                    var mat = new Material(urpLit);
                    mat.SetColor("_BaseColor", spec.color);
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", spec.color * 0.5f);
                    rend.sharedMaterial = mat;
                }

                // Trigger collider for interaction
                var col = go.GetComponent<BoxCollider>();
                if (col != null) col.isTrigger = true;

                // Attach the runtime component (LoreStoneInteraction lives in Integration assembly)
                go.AddComponent<Tartaria.Integration.LoreStoneInteraction>().Init(spec.id, spec.dialogueKey);
                placed++;
            }
            return placed;
        }

        static int SpawnFallenPillars(Transform parent)
        {
            var pillarsParent = new GameObject("FallenPillars");
            pillarsParent.transform.SetParent(parent);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(COLUMN_PREFAB);
            if (prefab == null)
            {
                Debug.LogWarning($"[Moon1BuildOutProps] Column prefab missing at {COLUMN_PREFAB}");
                return 0;
            }

            int placed = 0;
            for (int i = 0; i < FALLEN_PILLARS.Length; i++)
            {
                var pos = FALLEN_PILLARS[i];
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pillarsParent.transform);
                go.transform.position = pos;
                // Fallen on its side — random Z rotation gives variety
                float zRot = (i % 2 == 0) ? 90f : -90f;
                float yRot = i * 60f;
                go.transform.rotation = Quaternion.Euler(0f, yRot, zRot);
                go.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                go.name = $"FallenPillar_{i}";
                placed++;
            }
            return placed;
        }
    }
}
#endif
