#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Tartaria.AI;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1CombatVerify — audits the combat pipeline before Play:
    ///   1. EchohavenCombatArena present in scene (3-wave timed combat)
    ///   2. EchohavenContentSpawner present (RS-threshold waves at 25/50/75)
    ///   3. MudGolem prefab has MudGolemHealth + MudGolemAI components
    ///   4. Player has PlayerCombat / PlayerAbilityController
    ///
    /// Logs a checklist + auto-adds missing systems where possible.
    /// </summary>
    public static class Moon1CombatVerify
    {
        const string MUD_GOLEM_PREFAB = "Assets/_Project/Prefabs/Characters/MudGolem.prefab";

        [MenuItem("Tartaria/6 Scene Tools/Combat Verify (Moon 1)", priority = 650)]
        public static void Run()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Moon 1 Combat Audit ===");

            // 1. EchohavenCombatArena
            var arena = UnityEngine.Object.FindFirstObjectByType<EchohavenCombatArena>(FindObjectsInactive.Include);
            if (arena == null)
            {
                report.AppendLine("  [WARN] No EchohavenCombatArena in scene. Adding to GAMEPLAY_SYSTEMS.");
                var systemsGO = GameObject.Find("--- GAMEPLAY SYSTEMS ---") ?? GameObject.Find("GAMEPLAY_SYSTEMS") ?? new GameObject("--- GAMEPLAY SYSTEMS ---");
                var arenaGO = new GameObject("EchohavenCombatArena");
                arenaGO.transform.SetParent(systemsGO.transform);
                arena = arenaGO.AddComponent<EchohavenCombatArena>();
                Undo.RegisterCreatedObjectUndo(arenaGO, "Add CombatArena");
            }
            report.AppendLine($"  [OK]   EchohavenCombatArena: {arena.gameObject.name}, waves={string.Join(",", arena.waveSizes)}");

            // 2. EchohavenContentSpawner (RS-threshold golem spawner)
            var spawner = UnityEngine.Object.FindFirstObjectByType<EchohavenContentSpawner>(FindObjectsInactive.Include);
            if (spawner == null)
                report.AppendLine("  [WARN] No EchohavenContentSpawner. Run Tartaria → Wire Echohaven Content Spawner.");
            else
                report.AppendLine($"  [OK]   EchohavenContentSpawner present (Instance ready for RS waves at 25/50/75)");

            // 3. MudGolem prefab health + AI
            var golemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MUD_GOLEM_PREFAB);
            if (golemPrefab == null)
            {
                report.AppendLine($"  [FAIL] MudGolem prefab missing at {MUD_GOLEM_PREFAB}");
            }
            else
            {
                bool hasHealth = golemPrefab.GetComponent<MudGolemHealth>() != null;
                bool hasAI     = golemPrefab.GetComponent<MudGolemAI>() != null;
                report.AppendLine($"  [{(hasHealth ? "OK" : "WARN")}] MudGolem prefab: MudGolemHealth={hasHealth}, MudGolemAI={hasAI}");
                if (!hasHealth || !hasAI)
                    report.AppendLine($"          (EchohavenContentSpawner.SpawnMudGolem auto-adds these at runtime so combat still works.)");
            }

            // 4. Player combat components — check Player.prefab + scene player
            var playerInScene = GameObject.FindWithTag("Player");
            if (playerInScene != null)
            {
                var combat = playerInScene.GetComponent<Tartaria.Gameplay.PlayerCombat>();
                var ability = playerInScene.GetComponent<Tartaria.Gameplay.PlayerAbilityController>();
                report.AppendLine($"  [{(combat != null ? "OK" : "WARN")}] Player.PlayerCombat = {(combat != null ? "present" : "MISSING")}");
                report.AppendLine($"  [{(ability != null ? "OK" : "WARN")}] Player.PlayerAbilityController = {(ability != null ? "present" : "MISSING")}");
            }
            else
            {
                report.AppendLine("  [INFO] No Player in scene (spawns at Play time via PlayerSpawner).");
            }

            // 5. Tag/Layer setup for golem detection
            string golemTag = "Enemy";
            bool tagOk = true;
            try
            {
                var test = GameObject.CreatePrimitive(PrimitiveType.Cube);
                test.tag = golemTag;
                UnityEngine.Object.DestroyImmediate(test);
            }
            catch (UnityException)
            {
                tagOk = false;
            }
            report.AppendLine($"  [{(tagOk ? "OK" : "WARN")}] 'Enemy' tag available (used by golem spawn).");

            report.AppendLine("=== End of audit ===");
            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Combat Verify",
                "Audit logged to Console.\n\nKey runtime flow:\n" +
                " - EchohavenCombatArena runs 3 timed waves on scene start (4s delay)\n" +
                " - EchohavenContentSpawner fires extra waves at RS 25/50/75\n" +
                " - Both call EchohavenContentSpawner.SpawnMudGolem → real prefab or primitive fallback\n" +
                " - MudGolemHealth.TakeDamage(damage, instigator) handles player hits", "OK");
        }
    }
}
#endif
