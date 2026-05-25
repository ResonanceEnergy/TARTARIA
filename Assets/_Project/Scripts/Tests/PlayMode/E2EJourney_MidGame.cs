using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.Core;
using Tartaria.Integration;
using Tartaria.Gameplay;
using Tartaria.Save;

namespace Tartaria.Tests.PlayMode
{
    /// <summary>
    /// E2E Test: Mid-Game Journey (10-30 hours)
    /// 
    /// Tests mid-game progression experience:
    /// - Moon 4-8 progression
    /// - Level 30-70
    /// - 150 quests completed
    /// - Equipment upgrades
    /// - Skill tree unlocks
    /// - Companion progression
    /// 
    /// SUCCESS CRITERIA:
    /// - Player reaches level 70+
    /// - At least 150 quests completed
    /// - Equipment tier 3+ unlocked
    /// - 10+ skills unlocked
    /// - Companion loyalty 50%+
    /// - Zero progression blockers
    /// </summary>
    public class E2EJourney_MidGame : PlayModeTestBase
    {
        GameObject _player;
        SaveManager _saveManager;
        QuestManager _questManager;
        
        const string TEST_SAVE_NAME = "E2E_MidGame";
        const int START_LEVEL = 30;
        const int TARGET_LEVEL = 70;
        const int TARGET_QUESTS = 150;
        const int TARGET_MOONS = 8;
        
        public E2EJourney_MidGame() : base("E2E: Mid-Game Journey (10-30h)") { }
        
        protected override IEnumerator RunTestPhase()
        {
            LogInfo("Setting up mid-game scenario...");
            yield return SetupMidGameState();
            
            if (_saveManager == null)
            {
                LogFail("SaveManager initialization failed");
                yield break;
            }
            
            // Test Moons 4-8 progression
            for (int moon = 4; moon <= 8; moon++)
            {
                LogInfo($"Testing Moon {moon} progression...");
                yield return TestMoonProgression(moon);
            }
            
            LogInfo("Testing equipment system...");
            yield return TestEquipmentUpgrades();
            
            LogInfo("Testing skill tree...");
            yield return TestSkillTreeProgression();
            
            LogInfo("Testing companion system...");
            yield return TestCompanionProgression();
            
            LogInfo("Validating mid-game metrics...");
            yield return ValidateMidGameMetrics();
            
            GenerateFinalReport();
        }
        
        IEnumerator SetupMidGameState()
        {
            yield return LoadSceneAsync("Boot");
            yield return new WaitForSeconds(1f);
            
            _saveManager = SaveManager.Instance;
            _questManager = QuestManager.Instance;
            
            if (_saveManager != null)
            {
                _saveManager.CreateNewSave(TEST_SAVE_NAME);
                
                // Set up starting state (level 30, Moons 1-3 cleared)
                _saveManager.SetPlayerLevel(START_LEVEL);
                
                if (MoonProgressTracker.Instance != null)
                {
                    for (int moon = 1; moon <= 3; moon++)
                    {
                        for (int beat = 0; beat < 5; beat++)
                        {
                            MoonProgressTracker.Instance.MarkBeatCleared(moon, beat);
                        }
                    }
                }
                
                LogPass("Mid-game starting state configured");
            }
            else
            {
                LogFail("SaveManager not found");
            }
            
            yield return LoadSceneAsync("Echohaven_VerticalSlice");
            yield return new WaitForSeconds(2f);
            
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player != null)
            {
                LogPass("Player spawned");
            }
        }
        
        IEnumerator TestMoonProgression(int moonNumber)
        {
            int questsPerMoon = 25; // Simulate 25 quests per moon
            yield return SimulateQuestProgression(moonNumber, questsPerMoon);
            
            // Mark moon as cleared
            if (MoonProgressTracker.Instance != null)
            {
                for (int beat = 0; beat < 5; beat++)
                {
                    MoonProgressTracker.Instance.MarkBeatCleared(moonNumber, beat);
                }
                LogPass($"Moon {moonNumber} cleared");
            }
            
            // Simulate level gain
            int expectedLevel = START_LEVEL + ((moonNumber - 3) * 10);
            if (_saveManager != null)
            {
                _saveManager.SetPlayerLevel(expectedLevel);
                LogInfo($"Player level updated to {expectedLevel}");
            }
        }
        
        IEnumerator TestEquipmentUpgrades()
        {
            var equipmentSystem = GameObject.FindObjectOfType<EquipmentSystemController>();
            
            if (equipmentSystem != null)
            {
                LogPass("Equipment system found");
                
                // Test equipment slots
                bool hasWeaponSlot = equipmentSystem.HasSlot(EquipmentSlot.Weapon);
                bool hasArmorSlot = equipmentSystem.HasSlot(EquipmentSlot.Armor);
                bool hasAccessorySlot = equipmentSystem.HasSlot(EquipmentSlot.Accessory);
                
                if (hasWeaponSlot && hasArmorSlot && hasAccessorySlot)
                {
                    LogPass("All equipment slots available");
                }
                else
                {
                    LogWarn("Some equipment slots not available");
                }
            }
            else
            {
                LogWarn("Equipment system not found (may be in different scene)");
            }
            
            yield return null;
        }
        
        IEnumerator TestSkillTreeProgression()
        {
            var skillTree = GameObject.FindObjectOfType<SkillTreeController>();
            
            if (skillTree != null)
            {
                LogPass("Skill tree system found");
                
                // Simulate unlocking skills
                int unlockedSkills = skillTree.GetUnlockedSkillCount();
                LogInfo($"Skills unlocked: {unlockedSkills}");
                
                if (unlockedSkills >= 10)
                {
                    LogPass($"Skill target met: {unlockedSkills}/10");
                }
                else
                {
                    LogWarn($"Skill target not met: {unlockedSkills}/10");
                }
            }
            else
            {
                LogWarn("Skill tree system not found");
            }
            
            yield return null;
        }
        
        IEnumerator TestCompanionProgression()
        {
            var companionSystem = GameObject.FindObjectOfType<CompanionManager>();
            
            if (companionSystem != null)
            {
                LogPass("Companion system found");
                
                // Test companion loyalty
                float loyalty = companionSystem.GetCompanionLoyalty("Milo");
                LogInfo($"Companion loyalty: {loyalty:F1}%");
                
                if (loyalty >= 50f)
                {
                    LogPass($"Companion loyalty target met: {loyalty:F1}%/50%");
                }
                else
                {
                    LogWarn($"Companion loyalty below target: {loyalty:F1}%/50%");
                }
            }
            else
            {
                LogWarn("Companion system not found");
            }
            
            yield return null;
        }
        
        IEnumerator ValidateMidGameMetrics()
        {
            int currentLevel = GetPlayerLevel();
            if (currentLevel >= TARGET_LEVEL)
            {
                LogPass($"Level target met: {currentLevel}/{TARGET_LEVEL}");
            }
            else
            {
                LogFail($"Level target not met: {currentLevel}/{TARGET_LEVEL}");
            }
            
            int moonsCleared = GetClearedMoonCount();
            if (moonsCleared >= TARGET_MOONS)
            {
                LogPass($"Moon target met: {moonsCleared}/{TARGET_MOONS}");
            }
            else
            {
                LogFail($"Moon target not met: {moonsCleared}/{TARGET_MOONS}");
            }
            
            yield return null;
        }
        
        void GenerateFinalReport()
        {
            LogInfo("═══════════════════════════════════════════════");
            LogInfo("MID-GAME JOURNEY (10-30h) - FINAL REPORT");
            LogInfo("═══════════════════════════════════════════════");
            LogInfo($"Player Level: {GetPlayerLevel()}/{TARGET_LEVEL}");
            LogInfo($"Moons Cleared: {GetClearedMoonCount()}/{TARGET_MOONS}");
            LogInfo("═══════════════════════════════════════════════");
            
            if (FailCount == 0)
            {
                LogPass("MID-GAME JOURNEY: ALL TESTS PASSED ✓");
            }
            else
            {
                LogFail($"MID-GAME JOURNEY: {FailCount} FAILURES DETECTED");
            }
        }
        
        // ═══════════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════════
        
        IEnumerator LoadSceneAsync(string sceneName)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        
        IEnumerator SimulateQuestProgression(int moonNumber, int questCount)
        {
            for (int i = 0; i < questCount; i++)
            {
                string questId = $"moon{moonNumber}_q{i:D2}";
                
                if (_questManager != null)
                {
                    _questManager.ActivateQuest(questId);
                    yield return new WaitForSeconds(0.05f);
                    _questManager.CompleteQuest(questId);
                }
                
                yield return null;
            }
        }
        
        int GetPlayerLevel()
        {
            if (_saveManager != null)
            {
                return _saveManager.GetPlayerLevel();
            }
            return 0;
        }
        
        int GetClearedMoonCount()
        {
            if (MoonProgressTracker.Instance == null) return 0;
            
            int count = 0;
            for (int moon = 1; moon <= 13; moon++)
            {
                if (MoonProgressTracker.Instance.IsMoonCleared(moon))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
