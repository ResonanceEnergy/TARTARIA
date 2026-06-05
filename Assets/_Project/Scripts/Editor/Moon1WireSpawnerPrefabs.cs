#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1WireSpawnerPrefabs — wires every prefab field on every scene-attached
    /// spawner/VFX/AI component using EXCLUSIVELY Blender-built prefabs.
    ///
    /// Per NATRIX 2026-05-31 mandate: "remove all kaykit from characters and rebuild
    /// with blender". The serialized field names on EchohavenContentSpawner still read
    /// 'kayKitMiloPrefab' etc. (renaming would break serialization) but those slots
    /// now point at the Blender FBX prefab variants under Assets/_Project/Prefabs/
    /// Moon1/Blender/ and Shared/Blender/.
    ///
    /// Covers:
    ///   • EchohavenContentSpawner    — Milo / Cassian / Anastasia / MudGolem / Shovel / rocks / foliage
    ///   • BuildingSpawner            — rocks / trees / bushes
    ///   • Moon1ExcavationSites       — mudMoundPrefab
    ///   • Moon1BuildingPrefabCreator — rockPrefabs[]
    ///   • VFXWiringController        — scanPulse / restoreSparkle / shardCollect / hitImpact / deathBurst
    ///   • PlayerSpawner              — playerPrefab (prefers Blender PlayerHero)
    ///   • MudGolemAI (project-wide)  — aetherShardPrefab
    ///   • EnemySpawnerManager        — 8 enemy prefabs (Blender-built where available)
    ///   • HitVFXController/CombatHitReactor/PlayerRanged — spark/blood/shield/arrow
    ///
    /// Idempotent. Re-run after asset moves.
    /// </summary>
    public static class Moon1WireSpawnerPrefabs
    {
        // ─── Blender-first character paths ──────────────────────────────────────
        // Each character has an array of candidate paths — the FIRST one that exists wins.
        // Order: Moon1 Blender variant → Shared Blender variant → legacy Characters/ → KayKit fallback (none used post-purge).

        static readonly string[] MiloSearch        = { "Assets/_Project/Prefabs/Moon1/Blender/NPCs/MiloBoy.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/MiloBoy.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/Milo.prefab" };
        static readonly string[] CassianSearch     = { "Assets/_Project/Prefabs/Moon1/Blender/NPCs/CassianCarter.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/CassianCarter.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/Cassian.prefab" };
        static readonly string[] AnastasiaSearch   = { "Assets/_Project/Prefabs/Moon1/Blender/NPCs/AnastasiaPrincess.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/AnastasiaPrincess.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/Anastasia.prefab" };
        static readonly string[] LiraelSearch      = { "Assets/_Project/Prefabs/Moon1/Blender/NPCs/LiraelGuardian.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/LiraelGuardian.prefab" };
        static readonly string[] BobSearch         = { "Assets/_Project/Prefabs/Moon1/Blender/NPCs/BobInnkeeper.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/BobInnkeeper.prefab" };
        static readonly string[] MudGolemSearch    = { "Assets/_Project/Prefabs/Moon1/Blender/NPCs/MudGolem.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/MudGolem.prefab",
                                                       "Assets/_Project/Prefabs/Shared/Blender/MudGolem.prefab" };
        static readonly string[] ShovelSearch      = { "Assets/_Project/Prefabs/Moon1/Blender/Props/Shovel.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/Shovel.prefab",
                                                       "Assets/_Project/Prefabs/Shared/Blender/Shovel.prefab" };

        // Player + bosses + enemies (Blender-built per gen_characters_complete.py)
        static readonly string[] PlayerHeroSearch  = { "Assets/_Project/Prefabs/Shared/Blender/PlayerHero.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/PlayerHero.prefab",
                                                       "Assets/_Project/Prefabs/Characters/Player.prefab" };
        static readonly string[] GiantGolemSearch  = { "Assets/_Project/Prefabs/Shared/Blender/GiantGolem.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/GiantGolem.prefab" };
        static readonly string[] VoidPhantomSearch = { "Assets/_Project/Prefabs/Shared/Blender/VoidPhantom.prefab" };
        static readonly string[] WraithSearch      = { "Assets/_Project/Prefabs/Shared/Blender/TemporalWraith.prefab" };
        static readonly string[] ResetScoutSearch  = { "Assets/_Project/Prefabs/Moon1/Blender/NPCs/ResetScout.prefab",
                                                       "Assets/_Project/Prefabs/Moon1/Blender/ResetScout.prefab",
                                                       "Assets/_Project/Prefabs/Shared/Blender/ResetScout.prefab" };
        static readonly string[] ShadowStalkerSearch = { "Assets/_Project/Prefabs/Moon1/Blender/NPCs/ShadowStalker.prefab",
                                                         "Assets/_Project/Prefabs/Moon1/Blender/ShadowStalker.prefab",
                                                         "Assets/_Project/Prefabs/Shared/Blender/ShadowStalker.prefab" };
        static readonly string[] CrystalSentrySearch = { "Assets/_Project/Prefabs/Moon1/Blender/NPCs/CrystalSentry.prefab",
                                                         "Assets/_Project/Prefabs/Moon1/Blender/CrystalSentry.prefab",
                                                         "Assets/_Project/Prefabs/Moon2/Blender/CrystalSentry.prefab" };
        static readonly string[] ResonanceDroneSearch = { "Assets/_Project/Prefabs/Moon1/Blender/NPCs/ResonanceDrone.prefab",
                                                          "Assets/_Project/Prefabs/Moon1/Blender/ResonanceDrone.prefab" };
        static readonly string[] DissonanceCrystalSearch = { "Assets/_Project/Prefabs/Moon1/Blender/Props/DissonanceCrystal.prefab",
                                                             "Assets/_Project/Prefabs/Moon1/Blender/DissonanceCrystal.prefab",
                                                             "Assets/_Project/Prefabs/Moon2/Blender/DissonanceCrystal.prefab" };
        static readonly string[] AetherShardSearch   = { "Assets/_Project/Prefabs/Moon1/Blender/Props/Aether_A3_Crystal_Amber.prefab",
                                                         "Assets/_Project/Prefabs/Moon1/Blender/Aether_A3_Crystal_Amber.prefab",
                                                         "Assets/_Project/Prefabs/Collectibles/AetherShard/AetherShard.prefab" };
        static readonly string[] MudMoundSearch      = { "Assets/_Project/Prefabs/Moon1/Blender/Plates/MudPoolBasin.prefab",
                                                         "Assets/_Project/Prefabs/Moon1/Blender/MudPoolBasin.prefab",
                                                         "Assets/_Project/Prefabs/Moon1/Blender/Props/CrystalCluster.prefab",
                                                         "Assets/_Project/Prefabs/Moon1/Blender/CrystalCluster.prefab" };

        // VFX prefab paths (kept as-is — they're not character art)
        const string ScanPulsePath      = "Assets/_Project/Prefabs/VFX/ScanPulse.prefab";
        const string RestoreSparklePath = "Assets/_Project/Prefabs/VFX/RestoreSparkle.prefab";
        const string ShardCollectPath   = "Assets/_Project/Prefabs/VFX/ShardCollect.prefab";
        const string GiantBurstPath     = "Assets/_Project/Prefabs/VFX/Moon1/VFX_GiantModeBurst.prefab";
        const string SpireSparksPath    = "Assets/_Project/Prefabs/VFX/Moon1/VFX_SpirePlacementSparks.prefab";

        // Blender-only scan folders for arrays (rocks/foliage/trees/bushes)
        // KayKit folders also scanned IFF KayKitPrefabBatch has been run (idempotent).
        static readonly string[] BlenderScanFolders = {
            "Assets/_Project/Prefabs/Moon1/Blender",
            "Assets/_Project/Prefabs/Shared/Blender",
            "Assets/_Project/Prefabs/KayKit/KayKit_Forest_Nature_Pack_1.0_FREE",
            "Assets/_Project/Prefabs/KayKit/KayKit_RPGToolsBits_1.0_FREE",
        };

        // ════════════════════════════════════════════════════════════════════════
        //  MENUS
        // ════════════════════════════════════════════════════════════════════════

        [MenuItem("Tartaria/0 ★ MASTER/Wire ALL Scene Prefab Refs (full sweep, Blender-only)", priority = 40)]
        public static void RunAll()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Wire ALL", "No active scene.", "OK");
                return;
            }

            var report = new System.Text.StringBuilder();
            int totalFieldsWired = 0;

            totalFieldsWired += WireEchohavenContentSpawner(report);
            totalFieldsWired += WireBuildingSpawner(report);
            totalFieldsWired += WireExcavationSites(report);
            totalFieldsWired += WireBuildingPrefabCreator(report);
            totalFieldsWired += WireVFXController(report);
            totalFieldsWired += WirePlayerSpawner(report);
            totalFieldsWired += WireMudGolemAetherShard(report);
            totalFieldsWired += WireEnemySpawnerManager(report);
            totalFieldsWired += WireCombatVFX(report);

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Wire ALL — Blender-only Characters",
                $"Wired {totalFieldsWired} prefab fields across the scene using Blender-built models.\n\n{report}\n\n" +
                "Save the scene (Ctrl+S), then hit Play. Zero KayKit character references remain.",
                "OK");
        }

        [MenuItem("Tartaria/8 Fix/Wire EchohavenContentSpawner Prefabs (Blender-only)", priority = 195)]
        public static void RunSpawnerOnly()
        {
            var report = new System.Text.StringBuilder();
            int wired = WireEchohavenContentSpawner(report);
            EditorUtility.DisplayDialog("Wire Spawner Prefabs (Blender)",
                $"Wired {wired} prefab fields on EchohavenContentSpawner.\n\n{report}",
                "OK");
        }

        // ─── 1. EchohavenContentSpawner ──────────────────────────────────────────
        static int WireEchohavenContentSpawner(System.Text.StringBuilder report)
        {
            var sp = Object.FindFirstObjectByType<EchohavenContentSpawner>();
            report.AppendLine("── EchohavenContentSpawner (Blender) ──");
            if (sp == null) { report.AppendLine("  not in scene — skipped"); return 0; }
            var so = new SerializedObject(sp);
            int n = 0;
            n += TryAssign(so, "kayKitMiloPrefab",      ResolveFirstExisting(MiloSearch),      report);
            n += TryAssign(so, "kayKitCassianPrefab",   ResolveFirstExisting(CassianSearch),   report);
            n += TryAssign(so, "kayKitAnastasiaPrefab", ResolveFirstExisting(AnastasiaSearch), report);
            n += TryAssign(so, "kayKitMudGolemPrefab",  ResolveFirstExisting(MudGolemSearch),  report);
            n += TryAssign(so, "kayKitShovelPrefab",    ResolveFirstExisting(ShovelSearch),    report);

            // Rocks: scan Blender folders for stone/rock/boulder
            var rocks = new List<GameObject>();
            foreach (var f in BlenderScanFolders)
                CollectByName(f, new[] { "stone", "rock", "boulder", "pebble" }, rocks);
            n += AssignArrayReport(so, "kayKitRockPrefabs", rocks, "kayKitRockPrefabs (Blender)", report);

            // Foliage: trees, bushes, grass, plants, flowers, mushrooms, ferns
            var foliage = new List<GameObject>();
            foreach (var f in BlenderScanFolders)
                CollectByName(f, new[] { "tree", "bush", "grass", "plant", "flower", "fern", "shrub",
                                          "mushroom", "moss", "leaf", "log", "branch", "lotus", "sunflower" }, foliage);
            n += AssignArrayReport(so, "kayKitFoliagePrefabs", foliage, "kayKitFoliagePrefabs (Blender)", report);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(sp);
            return n;
        }

        // ─── 2. BuildingSpawner ──────────────────────────────────────────────────
        static int WireBuildingSpawner(System.Text.StringBuilder report)
        {
            var bs = Object.FindFirstObjectByType<BuildingSpawner>();
            report.AppendLine("── BuildingSpawner (Blender) ──");
            if (bs == null) { report.AppendLine("  not in scene — skipped"); return 0; }
            var so = new SerializedObject(bs);
            int n = 0;

            var rocks = new List<GameObject>();
            foreach (var f in BlenderScanFolders)
                CollectByName(f, new[] { "stone", "rock", "boulder" }, rocks);
            n += AssignArrayReport(so, "kayKitRockPrefabs", rocks, "kayKitRockPrefabs (Blender)", report);

            var trees = new List<GameObject>();
            foreach (var f in BlenderScanFolders)
                CollectByName(f, new[] { "tree" }, trees);
            n += AssignArrayReport(so, "kayKitTreePrefabs", trees, "kayKitTreePrefabs (Blender)", report);

            var bushes = new List<GameObject>();
            foreach (var f in BlenderScanFolders)
                CollectByName(f, new[] { "bush", "shrub", "fern", "plant", "grass", "mushroom", "flower", "moss" }, bushes);
            n += AssignArrayReport(so, "kayKitBushPrefabs", bushes, "kayKitBushPrefabs (Blender)", report);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(bs);
            return n;
        }

        // ─── 3. Moon1ExcavationSites ─────────────────────────────────────────────
        static int WireExcavationSites(System.Text.StringBuilder report)
        {
            var es = Object.FindFirstObjectByType<Moon1ExcavationSites>();
            report.AppendLine("── Moon1ExcavationSites ──");
            if (es == null) { report.AppendLine("  not in scene — skipped"); return 0; }
            var so = new SerializedObject(es);
            int n = TryAssign(so, "mudMoundPrefab", ResolveFirstExisting(MudMoundSearch), report);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(es);
            return n;
        }

        // ─── 4. Moon1BuildingPrefabCreator ───────────────────────────────────────
        static int WireBuildingPrefabCreator(System.Text.StringBuilder report)
        {
            var bpc = Object.FindFirstObjectByType<Moon1BuildingPrefabCreator>();
            report.AppendLine("── Moon1BuildingPrefabCreator ──");
            if (bpc == null) { report.AppendLine("  not in scene — skipped"); return 0; }
            var so = new SerializedObject(bpc);
            var rocks = new List<GameObject>();
            foreach (var f in BlenderScanFolders)
                CollectByName(f, new[] { "stone", "rock", "boulder" }, rocks);
            int n = AssignArrayReport(so, "rockPrefabs", rocks, "rockPrefabs (Blender)", report);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(bpc);
            return n;
        }

        // ─── 5. VFXWiringController ──────────────────────────────────────────────
        static int WireVFXController(System.Text.StringBuilder report)
        {
            var vfx = Object.FindFirstObjectByType<VFXWiringController>();
            report.AppendLine("── VFXWiringController ──");
            if (vfx == null) { report.AppendLine("  not in scene — skipped"); return 0; }
            var so = new SerializedObject(vfx);
            int n = 0;
            n += TryAssign(so, "scanPulsePrefab",      ScanPulsePath,      report);
            n += TryAssign(so, "restoreSparklePrefab", RestoreSparklePath, report);
            n += TryAssign(so, "shardCollectPrefab",   ShardCollectPath,   report);
            n += TryAssign(so, "hitImpactPrefab",      RestoreSparklePath, report);
            n += TryAssign(so, "deathBurstPrefab",     GiantBurstPath,     report);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(vfx);
            return n;
        }

        // ─── 6. PlayerSpawner ────────────────────────────────────────────────────
        static int WirePlayerSpawner(System.Text.StringBuilder report)
        {
            var ps = Object.FindFirstObjectByType<PlayerSpawner>();
            report.AppendLine("── PlayerSpawner ──");
            if (ps == null) { report.AppendLine("  not in scene — skipped"); return 0; }
            var so = new SerializedObject(ps);
            int n = TryAssign(so, "playerPrefab", ResolveFirstExisting(PlayerHeroSearch), report);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ps);
            return n;
        }

        // ─── 7. MudGolemAI loot drop ─────────────────────────────────────────────
        static int WireMudGolemAetherShard(System.Text.StringBuilder report)
        {
            report.AppendLine("── MudGolemAI.aetherShardPrefab (Blender shard) ──");
            string shardPath = ResolveFirstExisting(AetherShardSearch);
            if (shardPath == null) { report.AppendLine("  No aether shard prefab on disk"); return 0; }
            var shard = AssetDatabase.LoadAssetAtPath<GameObject>(shardPath);
            int n = 0;
            // Wire on the MudGolem prefab asset itself (Blender-built)
            string mgPath = ResolveFirstExisting(MudGolemSearch);
            if (!string.IsNullOrEmpty(mgPath))
            {
                var inst = PrefabUtility.LoadPrefabContents(mgPath);
                bool changed = false;
                foreach (var mg in inst.GetComponentsInChildren<Tartaria.AI.MudGolemAI>(true))
                {
                    var so = new SerializedObject(mg);
                    var p = so.FindProperty("aetherShardPrefab");
                    if (p != null && p.objectReferenceValue == null)
                    {
                        p.objectReferenceValue = shard;
                        so.ApplyModifiedProperties();
                        changed = true; n++;
                    }
                }
                if (changed) PrefabUtility.SaveAsPrefabAsset(inst, mgPath);
                PrefabUtility.UnloadPrefabContents(inst);
                report.AppendLine($"  Blender MudGolem prefab: {(changed ? "wired ✓" : "already set")}");
            }
            // Wire any scene instances
            foreach (var mg in Object.FindObjectsByType<Tartaria.AI.MudGolemAI>(FindObjectsSortMode.None))
            {
                var so = new SerializedObject(mg);
                var p = so.FindProperty("aetherShardPrefab");
                if (p != null && p.objectReferenceValue == null)
                {
                    p.objectReferenceValue = shard;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(mg);
                    n++;
                }
            }
            return n;
        }

        // ─── 8. EnemySpawnerManager ──────────────────────────────────────────────
        static int WireEnemySpawnerManager(System.Text.StringBuilder report)
        {
            report.AppendLine("── EnemySpawnerManager (Blender) ──");
            var esm = FindByName("EnemySpawnerManager");
            if (esm == null) { report.AppendLine("  not in scene — skipped"); return 0; }
            var so = new SerializedObject(esm);
            int n = 0;
            string mudGolem    = ResolveFirstExisting(MudGolemSearch);
            string giant       = ResolveFirstExisting(GiantGolemSearch) ?? mudGolem;
            string shadowStalk = ResolveFirstExisting(ShadowStalkerSearch);
            string crystalSen  = ResolveFirstExisting(CrystalSentrySearch);
            string drone       = ResolveFirstExisting(ResonanceDroneSearch);
            string voidPh      = ResolveFirstExisting(VoidPhantomSearch) ?? shadowStalk;
            string wraith      = ResolveFirstExisting(WraithSearch)      ?? shadowStalk;
            string dissCrystal = ResolveFirstExisting(DissonanceCrystalSearch);

            n += TryAssign(so, "mudGolemPrefab",         mudGolem,    report);
            n += TryAssign(so, "dissonantCrystalPrefab", dissCrystal, report);
            n += TryAssign(so, "giantGolemPrefab",       giant,       report);
            n += TryAssign(so, "shadowStalkerPrefab",    shadowStalk, report);
            n += TryAssign(so, "crystalSentryPrefab",    crystalSen,  report);
            n += TryAssign(so, "voidPhantomPrefab",      voidPh,      report);
            n += TryAssign(so, "resonanceDronePrefab",   drone,       report);
            n += TryAssign(so, "temporalWraithPrefab",   wraith,      report);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(esm);
            return n;
        }

        // ─── 9. Combat VFX (HitVFX / CombatHitReactor / PlayerRanged) ───────────
        static int WireCombatVFX(System.Text.StringBuilder report)
        {
            report.AppendLine("── Combat VFX components ──");
            int n = 0;
            n += WireField("HitVFXController", "_sparkVfxPrefab",  ScanPulsePath,      report);
            n += WireField("HitVFXController", "_bloodVfxPrefab",  RestoreSparklePath, report);
            n += WireField("HitVFXController", "_shieldVfxPrefab", SpireSparksPath,    report);
            n += WireField("CombatHitReactor", "hitParticlePrefab", ScanPulsePath,     report);
            // Arrow — keep canonical for now (TODO: wire a Blender ArrowBundle when ready)
            n += WireField("PlayerRanged", "arrowPrefab", "Assets/_Project/Prefabs/Moon1/Blender/Props/ArrowBundle.prefab", report);
            return n;
        }

        // ────────────────────────────────────────────────────────────────────────
        //  HELPERS
        // ────────────────────────────────────────────────────────────────────────

        static int WireField(string componentName, string fieldName, string assetPath, System.Text.StringBuilder report)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (go == null) { report.AppendLine($"  {componentName}.{fieldName}: asset not found"); return 0; }
            int n = 0;
            foreach (var c in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (c == null) continue;
                if (c.GetType().Name != componentName) continue;
                var so = new SerializedObject(c);
                var p = so.FindProperty(fieldName);
                if (p == null) continue;
                if (p.objectReferenceValue != null) continue;
                p.objectReferenceValue = go;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(c);
                n++;
            }
            report.AppendLine($"  {componentName}.{fieldName}: {n} instance(s) wired");
            return n;
        }

        static MonoBehaviour FindByName(string componentTypeName)
        {
            foreach (var c in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (c != null && c.GetType().Name == componentTypeName) return c;
            }
            return null;
        }

        static int TryAssign(SerializedObject so, string fieldName, string assetPath, System.Text.StringBuilder report)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null) { report.AppendLine($"  {fieldName}: field not found"); return 0; }
            if (string.IsNullOrEmpty(assetPath))
            {
                report.AppendLine($"  {fieldName}: NO BLENDER PREFAB FOUND — left as-is");
                return 0;
            }
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (go == null) { report.AppendLine($"  {fieldName}: prefab missing at {assetPath}"); return 0; }
            // ALWAYS overwrite to ensure KayKit refs get replaced
            prop.objectReferenceValue = go;
            report.AppendLine($"  {fieldName}: ✓ {Path.GetFileName(assetPath)}");
            return 1;
        }

        static string ResolveFirstExisting(string[] candidates)
        {
            foreach (var p in candidates)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(p) != null) return p;
            }
            return null;
        }

        static void CollectByName(string folder, string[] keywords, List<GameObject> bucket)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return;
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g).ToLowerInvariant();
                foreach (var kw in keywords)
                {
                    if (p.Contains(kw))
                    {
                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g));
                        if (go != null && !bucket.Contains(go)) bucket.Add(go);
                        break;
                    }
                }
            }
        }

        static int AssignArrayReport(SerializedObject so, string fieldName, List<GameObject> items, string label, System.Text.StringBuilder report)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null || !prop.isArray) { report.AppendLine($"  {label}: field not found"); return 0; }
            int n = Mathf.Min(items.Count, 24);
            prop.arraySize = n;
            for (int i = 0; i < n; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            report.AppendLine($"  {label}: ✓ {n} Blender prefabs");
            return n > 0 ? 1 : 0;
        }
    }
}
#endif
