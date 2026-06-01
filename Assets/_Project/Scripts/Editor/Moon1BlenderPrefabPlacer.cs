#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor menu: Tartaria/Moon 1/Place Blender Prefabs (Echohaven Scene Dressing)
    ///
    /// Instantiates the Blender-generated FBX prefab variants at lore-appropriate
    /// positions in the Echohaven scene — replacing procedural primitives with
    /// real authored geometry. Idempotent: re-running rebuilds the placement root.
    ///
    /// Per CLAUDE.md "no stubs" mandate — every placement uses a real Asset.LoadAssetAtPath,
    /// every position is intentional, no TODO bodies.
    /// </summary>
    public static class Moon1BlenderPrefabPlacer
    {
        const string PLACEMENT_ROOT_NAME = "Moon1_BlenderPlacements";
        const string FBX_DIR = "Assets/_Project/Models/Blender/Moon1";
        const string PREFAB_DIR = "Assets/_Project/Prefabs/Moon1/Blender";

        [MenuItem("Tartaria/2 Place/Moon 1 — Blender Prefabs (Echohaven Scene Dressing)", priority = 200)]
        public static void Run()
        {
            var existing = GameObject.Find(PLACEMENT_ROOT_NAME);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Rebuild scene placements?",
                    PLACEMENT_ROOT_NAME + " already in scene. Destroy and re-create?",
                    "Rebuild", "Cancel")) return;
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(PLACEMENT_ROOT_NAME);
            Undo.RegisterCreatedObjectUndo(root, "Create Blender Placements");

            int placed = 0;

            // ── 1. Hero building props ──
            placed += Place(root, "PipeOrganCathedral",   new Vector3(  0f, 0.5f, 28f),  Quaternion.identity, 1.4f, "Cathedral_Interior");
            placed += Place(root, "PureWaterFont",        new Vector3(-30f, 0.5f,  0f),  Quaternion.identity, 1.3f, "Fountain_Interior");
            placed += Place(root, "MercuryBallSpireHero", new Vector3( 30f, 0.5f,  0f),  Quaternion.Euler(0f,180f,0f), 1.0f, "Spire_Crown");
            placed += Place(root, "RoseWindowCymatic",    new Vector3(  0f, 6f,   33f),  Quaternion.Euler(0f,180f,0f), 1.5f, "Cathedral_Facade");

            // ── 2. Echohaven Brazier ring around hero buildings (8 perimeter at radius 12) ──
            for (int i = 0; i < 8; i++)
            {
                float a = i * (Mathf.PI / 4f);
                var pos = new Vector3(Mathf.Cos(a) * 12f, 0f, Mathf.Sin(a) * 12f);
                placed += Place(root, "EchohavenBrazier", pos, Quaternion.identity, 1f, $"Brazier_{i}");
            }

            // ── 3. Mud Pool basins replacing primitives (3 pools) ──
            placed += Place(root, "MudPoolBasin", new Vector3(-50f, 0f,  35f), Quaternion.identity, 1f, "MudPool_NW");
            placed += Place(root, "MudPoolBasin", new Vector3( 55f, 0f,  30f), Quaternion.identity, 1f, "MudPool_NE");
            placed += Place(root, "MudPoolBasin", new Vector3(-45f, 0f, -45f), Quaternion.identity, 1f, "MudPool_SW");

            // ── 4. Aether crystals (3 per pool, 120° apart at radius 3.5) ──
            string[] crystals = { "Aether_E3_Crystal_BlueIce", "Aether_A3_Crystal_Amber", "Aether_D4_Crystal_PaleGreen" };
            Vector3[] poolCenters = {
                new Vector3(-50f, 0f,  35f),
                new Vector3( 55f, 0f,  30f),
                new Vector3(-45f, 0f, -45f),
            };
            foreach (var pc in poolCenters)
            {
                for (int i = 0; i < 3; i++)
                {
                    float a = i * (2f * Mathf.PI / 3f);
                    var off = new Vector3(Mathf.Cos(a) * 3.5f, 1.5f, Mathf.Sin(a) * 3.5f);
                    placed += Place(root, crystals[i], pc + off, Quaternion.Euler(0f, i * 60f, 0f), 0.8f, $"Crystal_{i}");
                }
            }

            // ── 5. Anastasia at Cathedral entrance ──
            placed += Place(root, "AnastasiaRockingChair", new Vector3(3f, 0f, 22f), Quaternion.Euler(0f, 195f, 0f), 1f, "Anastasia_Chair");

            // ── 6. Tuning pedestals (3 per hero building at burial-resonance points) ──
            Vector3[] tuningPos = {
                // StarDome triplet
                new Vector3(-5f, 0f, 25f), new Vector3(5f, 0f, 25f), new Vector3(0f, 0f, 35f),
                // HarmonicFountain triplet
                new Vector3(-35f, 0f,  5f), new Vector3(-35f, 0f, -5f), new Vector3(-25f, 0f, 0f),
                // CrystalSpire triplet
                new Vector3( 35f, 0f,  5f), new Vector3( 35f, 0f, -5f), new Vector3( 25f, 0f, 0f),
            };
            for (int i = 0; i < tuningPos.Length; i++)
                placed += Place(root, "TuningPedestal", tuningPos[i], Quaternion.identity, 1f, $"TuningPedestal_{i}");

            // ── 7. Bob's Inn ── (Moon 1 completion / Moon 2 transition spot)
            placed += Place(root, "BobsInn", new Vector3(0f, 0f, -55f), Quaternion.Euler(0f, 180f, 0f), 1.2f, "BobsInn");

            // ── 8. Carved Stone POI ──
            placed += Place(root, "CarvedStoneObelisk", new Vector3(40f, 0f, -40f), Quaternion.identity, 1.5f, "CarvedStone");
            placed += Place(root, "SkeletonRemains", new Vector3(42f, 0f, -42f), Quaternion.Euler(0f, 45f, 0f), 1f, "SkeletonAtCarvedStone");
            placed += Place(root, "GiantSkeletonKey", new Vector3(38f, 0.3f, -38f), Quaternion.Euler(0f, 30f, 0f), 1f, "GiantKeyClaw");

            // ── 9. Hanging lanterns on village posts (6 placed at ring positions) ──
            for (int i = 0; i < 6; i++)
            {
                float a = i * (Mathf.PI / 3f);
                var pos = new Vector3(Mathf.Cos(a) * 50f, 3f, Mathf.Sin(a) * 50f);
                placed += Place(root, "HangingLantern", pos, Quaternion.identity, 1f, $"VillageLantern_{i}");
            }

            // ── 10. Wall sconces near hero building doors ──
            placed += Place(root, "WallSconceIron", new Vector3(-3f, 1.5f, 23f), Quaternion.Euler(0f, 180f, 0f), 1f, "Sconce_Cathedral_L");
            placed += Place(root, "WallSconceIron", new Vector3( 3f, 1.5f, 23f), Quaternion.Euler(0f, 180f, 0f), 1f, "Sconce_Cathedral_R");
            placed += Place(root, "WallSconceIron", new Vector3(-23f, 1.5f, -3f), Quaternion.Euler(0f,  90f, 0f), 1f, "Sconce_Fountain_L");
            placed += Place(root, "WallSconceIron", new Vector3(-23f, 1.5f,  3f), Quaternion.Euler(0f,  90f, 0f), 1f, "Sconce_Fountain_R");

            // ── 11. Torches on posts at village outskirts ──
            for (int i = 0; i < 4; i++)
            {
                float a = i * (Mathf.PI / 2f) + Mathf.PI / 4f;
                var pos = new Vector3(Mathf.Cos(a) * 65f, 0f, Mathf.Sin(a) * 65f);
                placed += Place(root, "TorchOnPost", pos, Quaternion.identity, 1f, $"TorchPost_{i}");
            }

            // ── 12. Village well (centerpoint) ──
            placed += Place(root, "VillageWell", new Vector3(0f, 0f, -20f), Quaternion.identity, 1f, "VillageWell");
            placed += Place(root, "VillagerSignpost", new Vector3(-5f, 0f, -22f), Quaternion.Euler(0f, 45f, 0f), 1f, "VillagerSignpost");

            // ── 13. Furniture inside village buildings (around (40, 0, 40) blacksmith zone) ──
            placed += Place(root, "RoundTable",   new Vector3(38f, 0f, 38f), Quaternion.identity, 1f, "BlacksmithTable");
            placed += Place(root, "PeasantChair", new Vector3(37f, 0f, 38f), Quaternion.Euler(0f,  90f, 0f), 1f, "ChairA");
            placed += Place(root, "PeasantChair", new Vector3(39f, 0f, 38f), Quaternion.Euler(0f, 270f, 0f), 1f, "ChairB");
            placed += Place(root, "StorageChest", new Vector3(40f, 0f, 41f), Quaternion.Euler(0f,  45f, 0f), 1f, "BlacksmithChest");
            placed += Place(root, "FireplaceHearth", new Vector3(43f, 0f, 39f), Quaternion.Euler(0f, 270f, 0f), 1f, "BlacksmithHearth");

            // ── 14. Long dining table (scholar zone west at -40,0,0) ──
            placed += Place(root, "LongDiningTable", new Vector3(-40f, 0f, 0f), Quaternion.identity, 1f, "ScholarTable");
            placed += Place(root, "LongBench",       new Vector3(-40f, 0f, -1f), Quaternion.identity, 1f, "ScholarBench");
            placed += Place(root, "Bookshelf",       new Vector3(-42f, 0f,  2f), Quaternion.Euler(0f,90f,0f), 1f, "ScholarBookshelf");
            placed += Place(root, "WoodenLectern",   new Vector3(-39f, 0f,  1f), Quaternion.identity, 1f, "ScholarLectern");
            placed += Place(root, "CandelabraTriple", new Vector3(-40f, 0.78f, 0f), Quaternion.identity, 1f, "ScholarCandles");

            // ── 15. Market stalls (south at 0,0,-38) ──
            placed += Place(root, "WoodenBarrel", new Vector3(-3f, 0f, -36f), Quaternion.identity, 1f, "MarketBarrel_A");
            placed += Place(root, "WoodenBarrel", new Vector3( 3f, 0f, -36f), Quaternion.identity, 1f, "MarketBarrel_B");
            placed += Place(root, "WoodenCrate",  new Vector3(-6f, 0f, -38f), Quaternion.identity, 1f, "MarketCrate_A");
            placed += Place(root, "WoodenCrate",  new Vector3( 6f, 0f, -38f), Quaternion.identity, 1f, "MarketCrate_B");
            placed += Place(root, "ClayUrn",      new Vector3(-4f, 0f, -40f), Quaternion.identity, 1f, "MarketUrn_A");
            placed += Place(root, "ClayUrn",      new Vector3( 4f, 0f, -40f), Quaternion.identity, 1f, "MarketUrn_B");
            placed += Place(root, "GrainSack",    new Vector3(-2f, 0f, -42f), Quaternion.identity, 1f, "MarketSack_A");
            placed += Place(root, "GrainSack",    new Vector3( 2f, 0f, -42f), Quaternion.identity, 1f, "MarketSack_B");
            placed += Place(root, "MetalBucket",  new Vector3( 0f, 0f, -36f), Quaternion.identity, 1f, "MarketBucket");

            // ── 16. Anastasia interior (small room near her chair) ──
            placed += Place(root, "WoodenBed",   new Vector3(8f, 0f, 24f), Quaternion.Euler(0f, 195f, 0f), 1f, "AnastasiaBed");
            placed += Place(root, "NightStand",  new Vector3(7f, 0f, 22f), Quaternion.identity, 1f, "AnastasiaNightStand");
            placed += Place(root, "TableLantern",new Vector3(7f, 0.6f, 22f), Quaternion.identity, 1f, "AnastasiaLantern");
            placed += Place(root, "RugWoven",    new Vector3(6f, 0f, 23f), Quaternion.identity, 1f, "AnastasiaRug");
            placed += Place(root, "ThreeLeggedStool", new Vector3(4f, 0f, 21f), Quaternion.identity, 1f, "AnastasiaStool");

            // ── 17. Lore artifact scrolls (3 collectibles scattered) ──
            placed += Place(root, "LoreArtifactScroll", new Vector3(-55f, 0f, 35f), Quaternion.identity, 1f, "Lore_MudPool_NW");
            placed += Place(root, "LoreArtifactScroll", new Vector3( 60f, 0f, 30f), Quaternion.identity, 1f, "Lore_MudPool_NE");
            placed += Place(root, "LoreArtifactScroll", new Vector3( 40f, 0.3f, -40f), Quaternion.Euler(0f, 45f, 0f), 1f, "Lore_CarvedStone");

            // ── 18. Milo satchel as pickup near spawn ──
            placed += Place(root, "MiloSatchelAndLantern", new Vector3(2f, 0f, -8f), Quaternion.identity, 1f, "MiloSatchelPickup");

            // ── 19. Shared utility props scattered for environment detail ──
            placed += Place(root, "BoulderLarge",      new Vector3( 60f, 0f,  20f), Quaternion.identity, 1f, "Boulder_NE_1");
            placed += Place(root, "BoulderMed",        new Vector3(-62f, 0f, -25f), Quaternion.identity, 1f, "Boulder_SW_1");
            placed += Place(root, "BoulderMed",        new Vector3( 55f, 0f, -50f), Quaternion.identity, 1f, "Boulder_SE_1");
            placed += Place(root, "BoulderSmall",      new Vector3( 12f, 0f, -32f), Quaternion.identity, 1f, "Boulder_S_1");
            placed += Place(root, "MushroomRed",       new Vector3(-15f, 0f,  40f), Quaternion.identity, 1f, "Shroom_N_1");
            placed += Place(root, "MushroomBlueGlow",  new Vector3(-43f, 0f,  20f), Quaternion.identity, 1f, "Shroom_W_1");
            placed += Place(root, "FallenLog",         new Vector3( 50f, 0f,  10f), Quaternion.Euler(0f, 30f, 0f), 1f, "FallenLog_E_1");
            placed += Place(root, "TreeStump",         new Vector3(-22f, 0f, -42f), Quaternion.identity, 1f, "Stump_SW_1");
            placed += Place(root, "RuinedColumn",      new Vector3(  8f, 0f, -50f), Quaternion.identity, 1f, "RuinedColumn_S_1");
            placed += Place(root, "AncientStoneSign",  new Vector3( -5f, 0f, -55f), Quaternion.Euler(0f, 180f, 0f), 1f, "AncientStoneSign_S");
            placed += Place(root, "CrackedFlagstone",  new Vector3(  0f, 0f, -10f), Quaternion.identity, 1f, "Flagstone_Spawn");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = root;

            string summary = $"Placed {placed} Blender prefabs in scene.\n\n" +
                             $"Root: {PLACEMENT_ROOT_NAME}\n" +
                             $"Categories: hero props, brazier ring, mud pool basins, " +
                             $"Aether crystals, Anastasia chair + interior, tuning pedestals, " +
                             $"Bob's Inn, Carved Stone POI + lore items, lanterns, sconces, " +
                             $"torches, village well + signpost, blacksmith zone, scholar zone, " +
                             $"market stalls, scattered environment detail.\n\n" +
                             $"Next: Save scene (Ctrl+S), then Play.";
            EditorUtility.DisplayDialog("Blender Prefab Placement", summary, "OK");
            Debug.Log("[Moon1BlenderPrefabPlacer] " + summary);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Loader helpers — try prefab variant first, fall back to FBX
        // ─────────────────────────────────────────────────────────────────────────

        static int Place(GameObject root, string assetName, Vector3 worldPos, Quaternion worldRot, float scale, string childName)
        {
            var asset = LoadAsset(assetName);
            if (asset == null)
            {
                Debug.LogWarning("[Moon1BlenderPrefabPlacer] missing: " + assetName);
                return 0;
            }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
            if (instance == null) instance = Object.Instantiate(asset, root.transform);
            instance.name = string.IsNullOrEmpty(childName) ? assetName : childName;
            instance.transform.position = worldPos;
            instance.transform.rotation = worldRot;
            instance.transform.localScale = Vector3.one * scale;
            return 1;
        }

        static GameObject LoadAsset(string name)
        {
            // 1. Try the auto-created prefab variant first
            string prefabPath = PREFAB_DIR + "/" + name + ".prefab";
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (p != null) return p;

            // 2. Fall back to raw FBX
            string fbxPath = FBX_DIR + "/" + name + ".fbx";
            var f = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (f != null) return f;

            return null;
        }
    }
}
#endif
