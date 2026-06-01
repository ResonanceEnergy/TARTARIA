#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor menu: Tartaria/Moon 1/Place New Assets (vehicles, weapons, flora, fauna...)
    ///
    /// Auto-places the 250+ new Blender FBX/prefab assets generated in the
    /// 102-asset + 163-asset batches at thematic positions around Echohaven.
    ///
    /// Idempotent — rebuilds the placement root. Falls back from prefab to FBX
    /// per existing Moon1BlenderPrefabPlacer pattern. Per CLAUDE.md no-stubs
    /// mandate every placement is intentional.
    /// </summary>
    public static class Moon1NewAssetsPlacer
    {
        const string ROOT_NAME = "Moon1_NewAssetsPlacements";
        const string PREFAB_DIR = "Assets/_Project/Prefabs/Moon1/Blender";
        const string FBX_DIR_BASE = "Assets/_Project/Models/Blender";

        [MenuItem("Tartaria/2 Place/Moon 1 — New Assets (vehicles, weapons, flora, fauna)", priority = 210)]
        public static void Run()
        {
            var existing = GameObject.Find(ROOT_NAME);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Rebuild new-asset placements?",
                    ROOT_NAME + " exists. Destroy + rebuild?", "Rebuild", "Cancel")) return;
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(ROOT_NAME);
            Undo.RegisterCreatedObjectUndo(root, "Create New Asset Placements");
            int placed = 0;

            // ─── CHARACTERS — Moon 1 cast positions ───
            placed += Place(root, "MiloBoy",          new Vector3( 1.5f, 0f, -6f),  Quaternion.Euler(0,  10f, 0), 1f, "Milo_NearSpawn");
            placed += Place(root, "AnastasiaPrincess", new Vector3( 2.5f, 0f, 21f), Quaternion.Euler(0,180f, 0), 1f, "Anastasia_Cathedral");
            placed += Place(root, "LiraelGuardian",   new Vector3(-32f, 0f,  0f),   Quaternion.Euler(0, 90f, 0), 1f, "Lirael_AtFountain");
            placed += Place(root, "CassianCarter",    new Vector3( 32f, 0f,  0f),   Quaternion.Euler(0,270f, 0), 1f, "Cassian_AtSpire");
            placed += Place(root, "BobInnkeeper",     new Vector3( 0f,  0f,-52f),   Quaternion.Euler(0,  0f, 0), 1f, "Bob_AtInn");
            placed += Place(root, "Villager_GenericA",new Vector3(-2f,  0f,-19f),   Quaternion.Euler(0, 60f, 0), 1f, "Villager_AtWell");

            // ─── ENEMIES — Moon 1 patrol points ───
            placed += Place(root, "MudGolem",     new Vector3(-48f, 0f, 38f),  Quaternion.identity, 1f, "MudGolem_AtPool_NW");
            placed += Place(root, "MudGolem",     new Vector3( 53f, 0f, 28f),  Quaternion.Euler(0, 120f, 0), 1f, "MudGolem_AtPool_NE");
            placed += Place(root, "ResetScout",   new Vector3( 18f, 0f, -28f), Quaternion.Euler(0, 200f, 0), 1f, "ResetScout_Patrol_S");
            placed += Place(root, "ResetScout",   new Vector3(-22f, 0f, -32f), Quaternion.Euler(0,  40f, 0), 1f, "ResetScout_Patrol_SW");
            placed += Place(root, "CathedralChoirSpirit", new Vector3(0f, 1f, 28f), Quaternion.identity, 1f, "CathedralChoirSpirit_Inside");

            // ─── VEHICLES + MOUNTS — village edge ───
            placed += Place(root, "Wagon",        new Vector3( 36f, 0f, 38f),  Quaternion.Euler(0, 45f, 0), 1f, "Wagon_Blacksmith");
            placed += Place(root, "CartFull",     new Vector3(-38f, 0f, -1f),  Quaternion.Euler(0,  0f, 0), 1f, "Cart_Scholar");
            placed += Place(root, "Sailboat",     new Vector3( 0f, 0f, 68f),   Quaternion.identity, 1f, "Sailboat_Lake");
            placed += Place(root, "Rowboat",      new Vector3(15f, 0f, 60f),   Quaternion.Euler(0, 30f, 0), 1f, "Rowboat_Lake");
            placed += Place(root, "Raft",         new Vector3(-15f, 0f, 60f),  Quaternion.Euler(0,-30f, 0), 1f, "Raft_Lake");
            placed += Place(root, "Horse",        new Vector3( 38f, 0f, 35f),  Quaternion.Euler(0, 60f, 0), 1f, "Horse_Smithy");
            placed += Place(root, "Donkey",       new Vector3(-37f, 0f, -3f),  Quaternion.Euler(0, 90f, 0), 1f, "Donkey_Scholar");
            placed += Place(root, "Ox",           new Vector3(-2f, 0f, -42f),  Quaternion.Euler(0,180f, 0), 1f, "Ox_Market");
            placed += Place(root, "Wolf",         new Vector3(-60f, 0f, -20f), Quaternion.Euler(0, 90f, 0), 1f, "Wolf_Forest");
            placed += Place(root, "Eagle",        new Vector3( 40f, 8f, -40f), Quaternion.Euler(0,200f, 0), 1f, "Eagle_Overlook");
            placed += Place(root, "Sled",         new Vector3(-42f, 0f, -45f), Quaternion.identity, 1f, "Sled_NearStump");
            placed += Place(root, "BalloonBasket",new Vector3( 45f, 0f, -45f), Quaternion.identity, 1f, "Balloon_Outskirts");
            placed += Place(root, "Palanquin",    new Vector3(  4f, 0f, 20f),  Quaternion.Euler(0,180f, 0), 1f, "Palanquin_Cathedral");

            // ─── WEAPONS — blacksmith zone ───
            placed += Place(root, "LongSword",    new Vector3(40f, 0.95f, 38f), Quaternion.Euler(0, 30f, 0), 1f, "LongSword_OnTable");
            placed += Place(root, "Dagger",       new Vector3(41f, 0.95f, 38.4f),Quaternion.Euler(0, 60f, 0), 1f, "Dagger_OnTable");
            placed += Place(root, "WarHammer",    new Vector3(39f, 0.0f,  37.5f),Quaternion.Euler(90, 0, 0), 1f, "Hammer_AtAnvil");
            placed += Place(root, "Mace",         new Vector3(39.5f, 0.0f, 38.5f),Quaternion.Euler(90,0,30),1f, "Mace_AtAnvil");
            placed += Place(root, "BattleAxe",    new Vector3(42f, 0.95f, 37f), Quaternion.Euler(0, 90f, 0), 1f, "Axe_OnTable");
            placed += Place(root, "Bow",          new Vector3(43f, 0.95f, 39f), Quaternion.Euler(0,120f, 0), 1f, "Bow_OnTable");
            placed += Place(root, "Quiver",       new Vector3(43.5f, 0f, 39.5f),Quaternion.identity, 1f, "Quiver_OnGround");
            placed += Place(root, "Crossbow",     new Vector3(38f, 0.95f, 40f), Quaternion.Euler(0, 0f, 0), 1f, "Crossbow_OnTable");
            placed += Place(root, "RoundShield",  new Vector3(37f, 0.4f, 38f),  Quaternion.Euler(90,0,0), 1f, "Shield_Leaning");
            placed += Place(root, "KiteShield",   new Vector3(38f, 0.4f, 39f),  Quaternion.Euler(90,0,0), 1f, "KiteShield_Leaning");

            // ─── ARMOR — same blacksmith zone (3 helmets on table, breastplate on stand) ───
            placed += Place(root, "HelmKnight",   new Vector3(39f, 1.0f, 38f),  Quaternion.identity, 1f, "Helm_Knight_OnTable");
            placed += Place(root, "HelmRoman",    new Vector3(40f, 1.0f, 38f),  Quaternion.identity, 1f, "Helm_Roman_OnTable");
            placed += Place(root, "BreastplateFull", new Vector3(41f, 0f, 36f), Quaternion.Euler(0, 90f, 0), 1f, "Breastplate_OnStand");

            // ─── INSTRUMENTS — scholar/cathedral zone ───
            placed += Place(root, "Lute",         new Vector3(-38f, 0.85f, 1f), Quaternion.Euler(0, 90f, 0), 1f, "Lute_OnTable");
            placed += Place(root, "Harp",         new Vector3(-42f, 0f, 1f),    Quaternion.Euler(0, 90f, 0), 1f, "Harp_OnFloor");
            placed += Place(root, "Flute",        new Vector3(-37f, 0.82f, -1f),Quaternion.Euler(0,  0f, 90f),1f, "Flute_OnLectern");
            placed += Place(root, "Tambourine",   new Vector3(-39f, 0.82f, 0f), Quaternion.Euler(0, 30f, 0), 1f, "Tambourine_OnTable");
            placed += Place(root, "HandDrum",     new Vector3(-40f, 0f, 2f),    Quaternion.identity, 1f, "HandDrum_OnFloor");
            placed += Place(root, "Gong",         new Vector3(  4f, 0f, 28f),   Quaternion.Euler(0,180f, 0), 1f, "Gong_Cathedral");

            // ─── COOKING + ALCHEMY — bakery + apothecary zones ───
            placed += Place(root, "Stove",        new Vector3(  3f, 0f, -33f),  Quaternion.Euler(0,  0f, 0), 1f, "Stove_Bakery");
            placed += Place(root, "Kettle",       new Vector3(  3f, 0.78f,-32f),Quaternion.identity, 1f, "Kettle_OnStove");
            placed += Place(root, "FryingPan",    new Vector3(  3f, 0.78f,-31f),Quaternion.identity, 1f, "Pan_OnStove");
            placed += Place(root, "BrewingRack",  new Vector3( 30f, 0f, -40f),  Quaternion.Euler(0,  0f, 0), 1f, "Brewing_Apothecary");
            placed += Place(root, "DistillationTower", new Vector3(33f, 0f, -40f), Quaternion.identity, 1f, "Distillation_Apothecary");
            placed += Place(root, "Alembic",      new Vector3( 31f, 0.85f, -38f), Quaternion.identity, 1f, "Alembic_OnTable");
            placed += Place(root, "Retort",       new Vector3( 32f, 0.85f, -38f), Quaternion.identity, 1f, "Retort_OnTable");
            placed += Place(root, "BeakerSmall",  new Vector3( 30f, 0.85f, -38f), Quaternion.identity, 1f, "Beaker1");
            placed += Place(root, "BeakerMed",    new Vector3( 30.3f, 0.85f, -38f), Quaternion.identity, 1f, "Beaker2");
            placed += Place(root, "BeakerLarge",  new Vector3( 30.7f, 0.85f, -38f), Quaternion.identity, 1f, "Beaker3");
            placed += Place(root, "SpiceRack",    new Vector3(28f, 0.85f, -39f), Quaternion.Euler(0, 90f, 0), 1f, "SpiceRack_Apothecary");
            placed += Place(root, "Cauldron",     new Vector3( 27f, 0f, -42f),  Quaternion.identity, 1f, "Cauldron_Apothecary");

            // ─── CONTAINERS — village + market ───
            placed += Place(root, "CrateLarge",   new Vector3(-7f, 0f, -36f), Quaternion.identity, 1f, "CrateLarge_Market");
            placed += Place(root, "CrateMed",     new Vector3( 7f, 0f, -36f), Quaternion.identity, 1f, "CrateMed_Market");
            placed += Place(root, "CrateSmall",   new Vector3( 8f, 0.3f, -36f),Quaternion.identity, 1f, "CrateSmall_OnCrateMed");
            placed += Place(root, "BarrelLarge",  new Vector3(-9f, 0f, -38f), Quaternion.identity, 1f, "BarrelLarge");
            placed += Place(root, "BarrelSmall",  new Vector3( 9f, 0f, -38f), Quaternion.identity, 1f, "BarrelSmall");
            placed += Place(root, "SackBurlap",   new Vector3(-3f, 0f, -44f), Quaternion.identity, 1f, "Sack_Burlap");
            placed += Place(root, "SackCanvas",   new Vector3( 3f, 0f, -44f), Quaternion.identity, 1f, "Sack_Canvas");
            placed += Place(root, "BasketWoven",  new Vector3( 6f, 0f, -40f), Quaternion.identity, 1f, "Basket_Market");
            placed += Place(root, "JarClay",      new Vector3(-6f, 0f, -40f), Quaternion.identity, 1f, "Jar_Market");
            placed += Place(root, "LockedStrongbox", new Vector3(43f, 0f, 41f), Quaternion.identity, 1f, "Strongbox_Smithy");

            // ─── FLORA TREES — perimeter forest ring ───
            var trees = new[] { "OakTree", "PineTree", "BirchTree", "WillowTree", "DeadOak", "Cypress", "Magnolia", "HawthornTree" };
            int treeCount = 18;
            for (int i = 0; i < treeCount; i++)
            {
                float a = i * (Mathf.PI * 2f / treeCount);
                float r = 65f + (i % 4) * 2.5f;
                var t = trees[i % trees.Length];
                placed += Place(root, t, new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r), Quaternion.Euler(0, i * 17f, 0), 1f + (i%3)*0.1f, $"Perimeter_{t}_{i}");
            }
            placed += Place(root, "AncientSequoia",   new Vector3(-70f, 0f,  70f), Quaternion.identity, 1f, "AncientSequoia_NW");
            placed += Place(root, "WorldTreeSmall",   new Vector3(  0f, 0f, -75f), Quaternion.identity, 1f, "WorldTree_S");
            placed += Place(root, "BigMushroomTree",  new Vector3( 70f, 0f, -65f), Quaternion.identity, 1f, "BigMushroom_SE");
            placed += Place(root, "PalmTree",         new Vector3(  0f, 0f,  72f), Quaternion.identity, 1f, "Palm_LakeShore");

            // ─── FLORA SMALL — scattered ground detail ───
            string[] smallFlora = { "MushroomCluster", "Fern", "Sunflower", "LilyPad", "LotusFlower", "IvyVine", "HangingMoss", "LeafPile", "SnowDrift", "CrystalCluster", "CattailReed", "GlowingFlowerPatch" };
            var rand = new System.Random(42);
            for (int i = 0; i < 24; i++)
            {
                float a = (float)rand.NextDouble() * Mathf.PI * 2f;
                float r = 25f + (float)rand.NextDouble() * 35f;
                var sf = smallFlora[i % smallFlora.Length];
                placed += Place(root, sf, new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r), Quaternion.Euler(0, (float)rand.NextDouble() * 360f, 0), 1f, $"Small_{sf}_{i}");
            }

            // ─── FAUNA — birds + small creatures ───
            placed += Place(root, "Owl",       new Vector3(  3f,  3.5f,  21f), Quaternion.identity, 1f, "Owl_Cathedral_Beam");
            placed += Place(root, "Raven",     new Vector3(-2f,   3f,   22f),  Quaternion.identity, 1f, "Raven_Cathedral");
            placed += Place(root, "Sparrow",   new Vector3(-1f,   2.5f, -20f), Quaternion.identity, 1f, "Sparrow_Well");
            placed += Place(root, "Butterfly", new Vector3( 10f,  1f,    10f), Quaternion.identity, 1f, "Butterfly_Garden");
            placed += Place(root, "Dragonfly", new Vector3(-50f,  1f,    35f), Quaternion.identity, 1f, "Dragonfly_MudPool");
            placed += Place(root, "Frog",      new Vector3(-48f,  0f,    37f), Quaternion.identity, 1f, "Frog_MudPool");
            placed += Place(root, "Turtle",    new Vector3( 10f,  0f,    62f), Quaternion.identity, 1f, "Turtle_Lake");
            placed += Place(root, "FishKoi",   new Vector3(  0f,  0f,    65f), Quaternion.Euler(0, 45f, 0), 1f, "FishKoi_Lake");
            placed += Place(root, "FishTrout", new Vector3(  8f,  0f,    63f), Quaternion.Euler(0, 90f, 0), 1f, "FishTrout_Lake");
            placed += Place(root, "FishBass",  new Vector3( -6f,  0f,    66f), Quaternion.Euler(0, 30f, 0), 1f, "FishBass_Lake");

            // ─── ARCHITECTURAL DETAILS — village + cathedral ───
            placed += Place(root, "Archway",       new Vector3(0f, 0f, 5f),    Quaternion.identity, 1f, "VillageArch_Entry");
            placed += Place(root, "DoorwayWithDoor", new Vector3(0f, 0f, 23.5f), Quaternion.identity, 1f, "Cathedral_Door");
            placed += Place(root, "WindowStainedGlass", new Vector3(0f, 4f, 33f), Quaternion.identity, 1.5f, "Cathedral_RoseWindow");
            placed += Place(root, "Staircase",     new Vector3( 4f, 0f, 22f),  Quaternion.identity, 1f, "Cathedral_Stairs");
            placed += Place(root, "Gargoyle",      new Vector3( 5f, 4f, 33f),  Quaternion.Euler(0,180f, 0), 1f, "Gargoyle_Cathedral_E");
            placed += Place(root, "Gargoyle",      new Vector3(-5f, 4f, 33f),  Quaternion.Euler(0,180f, 0), 1f, "Gargoyle_Cathedral_W");
            placed += Place(root, "PillarDoric",   new Vector3(-2.5f, 0f, 24f), Quaternion.identity, 1f, "Pillar_Cathedral_L");
            placed += Place(root, "PillarDoric",   new Vector3( 2.5f, 0f, 24f), Quaternion.identity, 1f, "Pillar_Cathedral_R");
            placed += Place(root, "WeatherVane",   new Vector3( 0f, 6f, 33f),   Quaternion.identity, 1f, "WeatherVane_Cathedral");
            placed += Place(root, "Finial",        new Vector3( -30f, 5.5f, 0f),Quaternion.identity, 1f, "Finial_Fountain");
            placed += Place(root, "BalconyRail",   new Vector3(  0f, 2f, -52f), Quaternion.identity, 1f, "Balcony_BobsInn");
            placed += Place(root, "Dormer",        new Vector3(  0f, 3f, -55f), Quaternion.identity, 1f, "Dormer_BobsInn");

            // ─── RITUAL SIGILS — Aether resonance circles ───
            placed += Place(root, "StoneCircle",        new Vector3( 40f, 0f, -40f), Quaternion.identity, 1f, "StoneCircle_CarvedStone");
            placed += Place(root, "PentagramFloor",     new Vector3(  0f, 0.01f, 28f), Quaternion.identity, 1f, "Pentagram_CathedralFloor");
            placed += Place(root, "ZodiacWheel",        new Vector3(-30f, 0.01f, 0f), Quaternion.identity, 1f, "Zodiac_FountainFloor");
            placed += Place(root, "LunarPhaseWheel",    new Vector3( 30f, 0.01f, 0f), Quaternion.identity, 1f, "Lunar_SpireFloor");
            placed += Place(root, "TriskeleTile",       new Vector3( 0f, 0.01f, -10f),Quaternion.identity, 1f, "Triskele_SpawnGate");
            placed += Place(root, "VesicaPiscisFloor",  new Vector3( 0f, 0.01f, 10f), Quaternion.identity, 1f, "VesicaPiscis_BetweenSpawn");
            placed += Place(root, "AnkhWallPlaque",     new Vector3(-5f, 1.5f, 24f),  Quaternion.identity, 1f, "Ankh_CathedralWall");
            placed += Place(root, "EyeOfProvidenceRelief", new Vector3(5f, 1.5f, 24f), Quaternion.identity, 1f, "Eye_CathedralWall");
            placed += Place(root, "OuroborosRingLarge", new Vector3(40f, 0.01f, 38f), Quaternion.identity, 1f, "Ouroboros_Smithy");
            placed += Place(root, "SephirothPillarTrio",new Vector3(-40f, 0f, 0f),    Quaternion.identity, 1f, "Sephiroth_Fountain");

            // ─── EXTRAS — village edge infrastructure ───
            placed += Place(root, "FencePanel", new Vector3(  8f, 0f, -10f), Quaternion.identity, 1f, "Fence_NearSpawn_E");
            placed += Place(root, "FencePanel", new Vector3( -8f, 0f, -10f), Quaternion.identity, 1f, "Fence_NearSpawn_W");
            placed += Place(root, "Gate",       new Vector3(  0f, 0f, -10f), Quaternion.identity, 1f, "Gate_NearSpawn");
            placed += Place(root, "WellBucket", new Vector3(  0f, 1.5f,-20f),Quaternion.identity, 1f, "Bucket_AtWell");
            placed += Place(root, "HangingChain", new Vector3(0f, 4f, 28f),  Quaternion.identity, 1f, "Chain_CathedralCeiling");
            placed += Place(root, "RopeCoil",   new Vector3( 36f, 0f, 40f),  Quaternion.identity, 1f, "RopeCoil_Smithy");
            placed += Place(root, "BrickPile",  new Vector3( 35f, 0f, 36f),  Quaternion.identity, 1f, "Brick_Smithy");
            placed += Place(root, "ScaffoldPiece", new Vector3(37f, 0f, 35f), Quaternion.identity, 1f, "Scaffold_Smithy");
            placed += Place(root, "LadderFolded", new Vector3(38f, 0f, 36f), Quaternion.identity, 1f, "LadderFolded_Smithy");
            placed += Place(root, "WoodenSign", new Vector3(  6f, 0f, -8f),  Quaternion.Euler(0, 30f, 0), 1f, "WoodenSign_Path");
            placed += Place(root, "BannerPole", new Vector3( -6f, 0f, -8f),  Quaternion.identity, 1f, "BannerPole_Path");
            placed += Place(root, "WallBanner", new Vector3(-4f, 2f, 24f),   Quaternion.identity, 1f, "WallBanner_Cathedral_L");
            placed += Place(root, "WallBanner", new Vector3( 4f, 2f, 24f),   Quaternion.identity, 1f, "WallBanner_Cathedral_R");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = root;

            string summary = $"Placed {placed} new Blender prefabs.\n\nRoot: {ROOT_NAME}\n\n" +
                             $"Categories: Moon 1 cast (6), enemies (5), vehicles+mounts (13), " +
                             $"weapons (10), armor (3), instruments (6), cooking+alchemy (12), " +
                             $"containers (10), trees (~22), small flora (24), fauna (10), " +
                             $"arch details (12), ritual sigils (10), extras (13).\n\n" +
                             $"Next: Save scene (Ctrl+S), then Play.";
            EditorUtility.DisplayDialog("New Asset Placement", summary, "OK");
            Debug.Log("[Moon1NewAssetsPlacer] " + summary);
        }

        static int Place(GameObject root, string assetName, Vector3 pos, Quaternion rot, float scale, string childName)
        {
            var asset = LoadAsset(assetName);
            if (asset == null) { Debug.LogWarning("[Moon1NewAssetsPlacer] missing: " + assetName); return 0; }
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
            if (inst == null) inst = Object.Instantiate(asset, root.transform);
            inst.name = childName;
            inst.transform.position = pos;
            inst.transform.rotation = rot;
            inst.transform.localScale = Vector3.one * scale;
            return 1;
        }

        static readonly string[] FBX_SUBDIRS = { "Moon1", "Shared", "Moon2", "Moon3", "Moon4", "Moon5", "Moon6", "Moon7", "Moon8", "Moon9", "Moon10", "Moon11", "Moon12", "Moon13" };

        static GameObject LoadAsset(string name)
        {
            // 1. Prefab variant first
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_DIR + "/" + name + ".prefab");
            if (p != null) return p;
            // 2. FBX in any of the moon subdirs
            foreach (var sub in FBX_SUBDIRS)
            {
                var f = AssetDatabase.LoadAssetAtPath<GameObject>(FBX_DIR_BASE + "/" + sub + "/" + name + ".fbx");
                if (f != null) return f;
            }
            return null;
        }
    }
}
#endif
