using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Adds all missing singleton managers to the active scene.
    /// Run after ProjectSetupWizard to fill in systems not covered by the
    /// vertical-slice scaffold.
    ///
    /// Menu: Tartaria > Scaffold All Managers
    /// </summary>
    public static class MasterSceneScaffold
    {
        [MenuItem("Tartaria/Scaffold All Managers", false, 2)]
        public static void ScaffoldAll()
        {
            int added = 0;
            added += ScaffoldCoreSystems();
            added += ScaffoldGameplaySystems();
            added += ScaffoldUISystems();
            added += ScaffoldIntegrationSystems();
            added += ScaffoldNPCs();
            added += ScaffoldMiniGames();

            Debug.Log($"[Tartaria] MasterSceneScaffold complete — {added} managers added.");
            if (added > 0)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // ─── Helpers ─────────────────────────────────

        static GameObject FindOrCreateParent(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) go = new GameObject(name);
            return go;
        }

        static bool Ensure<T>(Transform parent, string name = null) where T : Component
        {
            if (Object.FindFirstObjectByType<T>() != null) return false;
            var go = new GameObject(name ?? typeof(T).Name);
            go.transform.SetParent(parent);
            go.AddComponent<T>();
            return true;
        }

        // ─── Core ────────────────────────────────────

        static int ScaffoldCoreSystems()
        {
            var parent = FindOrCreateParent("--- GAME MANAGERS ---").transform;
            int n = 0;
            if (Ensure<Core.GameBootstrap>(parent)) n++;
            if (Ensure<Core.AetherFieldManager>(parent)) n++;
            if (Ensure<Core.EconomySystem>(parent)) n++;
            if (Ensure<Core.PerformanceGuard>(parent)) n++;
            // GameStateManager is Lazy<T>, not MonoBehaviour — no GO needed
            // LeyLineManager is nested in LeyLineSystem file, added under Gameplay
            return n;
        }

        // ─── Gameplay ────────────────────────────────

        static int ScaffoldGameplaySystems()
        {
            var parent = FindOrCreateParent("--- GAMEPLAY SYSTEMS ---").transform;
            int n = 0;
            if (Ensure<Gameplay.SkillTreeSystem>(parent)) n++;
            if (Ensure<Gameplay.CraftingSystem>(parent)) n++;
            if (Ensure<Gameplay.ExcavationSystem>(parent)) n++;
            if (Ensure<Gameplay.ResonanceScannerSystem>(parent)) n++;
            if (Ensure<Core.LeyLineManager>(parent)) n++;
            return n;
        }

        // ─── UI ──────────────────────────────────────

        static int ScaffoldUISystems()
        {
            var parent = FindOrCreateParent("--- UI ---").transform;
            int n = 0;
            if (Ensure<UI.UIManager>(parent)) n++;
            if (Ensure<UI.HUDController>(parent, "HUDController")) n++;
            if (Ensure<UI.AccessibilityManager>(parent)) n++;
            if (Ensure<UI.DissonanceLensOverlay>(parent)) n++;
            if (Ensure<UI.WorldMapUI>(parent)) n++;
            if (Ensure<UI.WorkshopUIPanel>(parent)) n++;
            if (Ensure<UI.QuestLogUI>(parent)) n++;
            if (Ensure<UI.SkillTreeUI>(parent)) n++;
            return n;
        }

        // ─── Integration (core systems) ──────────────

        static int ScaffoldIntegrationSystems()
        {
            var parent = FindOrCreateParent("--- GAME MANAGERS ---").transform;
            int n = 0;
            if (Ensure<Integration.GameLoopController>(parent)) n++;
            if (Ensure<Integration.CombatBridge>(parent)) n++;
            if (Ensure<Integration.CombatWaveManager>(parent)) n++;
            if (Ensure<Integration.DialogueManager>(parent)) n++;
            if (Ensure<Integration.DialogueTreeRunner>(parent)) n++;
            if (Ensure<Integration.ZoneController>(parent)) n++;
            if (Ensure<Integration.ZoneTransitionSystem>(parent)) n++;
            if (Ensure<Integration.VFXController>(parent)) n++;
            if (Ensure<Integration.ParticleEffectLibrary>(parent)) n++;
            if (Ensure<Integration.DebugOverlay>(parent)) n++;
            if (Ensure<Integration.AchievementSystem>(parent)) n++;
            if (Ensure<Integration.CampaignFlowController>(parent)) n++;
            if (Ensure<Integration.ClimaxSequenceSystem>(parent)) n++;
            if (Ensure<Integration.CompanionManager>(parent)) n++;
            if (Ensure<Integration.CompanionDialogueArcs>(parent)) n++;
            if (Ensure<Integration.ConsequenceVisuals>(parent)) n++;
            if (Ensure<Integration.CorruptionSystem>(parent)) n++;
            if (Ensure<Integration.ContinentalRailSystem>(parent)) n++;
            if (Ensure<Integration.AirshipFleetManager>(parent)) n++;
            if (Ensure<Integration.DayOutOfTimeController>(parent)) n++;
            if (Ensure<Integration.GiantModeController>(parent)) n++;
            if (Ensure<Integration.MicroGiantController>(parent)) n++;
            if (Ensure<Integration.QuestManager>(parent)) n++;
            if (Ensure<Integration.TutorialSystem>(parent)) n++;
            if (Ensure<Integration.WorkshopSystem>(parent)) n++;
            if (Ensure<Integration.WorldChoiceTracker>(parent)) n++;
            if (Ensure<Integration.BossEncounterSystem>(parent)) n++;
            // Audio
            if (Ensure<Audio.AudioManager>(parent)) n++;
            if (Ensure<Audio.AdaptiveMusicController>(parent)) n++;
            // Input
            if (Ensure<Input.HapticFeedbackManager>(parent)) n++;
            // Save
            if (Ensure<Save.SaveManager>(parent)) n++;
            return n;
        }

        // ─── NPCs ────────────────────────────────────

        static int ScaffoldNPCs()
        {
            var parent = FindOrCreateParent("--- NPCs ---").transform;
            int n = 0;
            if (Ensure<Integration.AnastasiaController>(parent)) n++;
            if (Ensure<Integration.CassianNPCController>(parent)) n++;
            if (Ensure<Integration.KorathController>(parent)) n++;
            if (Ensure<Integration.LiraelController>(parent)) n++;
            if (Ensure<Integration.MiloController>(parent)) n++;
            if (Ensure<Integration.ThorneController>(parent)) n++;
            if (Ensure<Integration.VeritasController>(parent)) n++;
            if (Ensure<Integration.ZerethController>(parent)) n++;
            return n;
        }

        // ─── Mini-Games ──────────────────────────────

        static int ScaffoldMiniGames()
        {
            var parent = FindOrCreateParent("--- MINI-GAMES ---").transform;
            int n = 0;
            if (Ensure<Gameplay.HarmonicRockCutting>(parent)) n++;
            if (Ensure<Gameplay.RailAlignmentMiniGame>(parent)) n++;
            if (Ensure<Gameplay.PipeOrganMiniGame>(parent)) n++;
            if (Ensure<Integration.AquiferPurgeMiniGame>(parent)) n++;
            if (Ensure<Integration.BellTowerSyncMiniGame>(parent)) n++;
            if (Ensure<Integration.CosmicConvergenceMiniGame>(parent)) n++;
            if (Ensure<Integration.LeyLineProphecyMiniGame>(parent)) n++;
            return n;
        }
    }
}
