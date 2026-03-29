using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Achievement System — 52 achievements across 8 categories.
    /// Tracks player milestones, awards aether/RS bonuses, surfaces via HUD.
    ///
    /// Categories:
    ///   R = Restoration (R01-R08)
    ///   C = Combat (C01-C08)
    ///   E = Exploration (E01-E06)
    ///   L = Lore (L01-L06)
    ///   K = Campaign (K01-K13)
    ///   S = Social (S01-S05)
    ///   M = Mini-Games (M01-M06)
    ///   H = Hidden (H01-H12, not shown until unlocked)
    ///
    /// Cross-ref: docs/28_ACHIEVEMENTS.md
    /// </summary>
    [DisallowMultipleComponent]
    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }

        // ─── Achievement Definition ──────────────────

        public enum AchievementCategory : byte
        {
            Restoration = 0,
            Combat = 1,
            Exploration = 2,
            Lore = 3,
            Campaign = 4,
            Social = 5,
            MiniGames = 6,
            Hidden = 7
        }

        [Serializable]
        public class AchievementDef
        {
            public string id;
            public string title;
            public string description;
            public AchievementCategory category;
            public bool hidden;
            public int aetherReward;
            public float rsReward;
        }

        // ─── State ───────────────────────────────────

        readonly Dictionary<string, bool> _unlocked = new();
        readonly Dictionary<string, float> _progress = new(); // 0..1 for progressive achievements
        readonly List<AchievementDef> _definitions = new();
        int _totalUnlocked;

        // ─── Events ─────────────────────────────────

        public event Action<AchievementDef> OnAchievementUnlocked;
        public event Action<string, float> OnProgressUpdated;

        // ─── Lifecycle ───────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            RegisterAllAchievements();
        }

        void Start()
        {
            // Wire to game events
            if (GameLoopController.Instance != null)
            {
                // Poll for achievements on save sync
            }

            var ana = AnastasiaController.Instance;
            if (ana != null)
                ana.OnMoteCollected += _ => CheckMoteAchievements();
        }

        // ─── Registration ────────────────────────────

        void RegisterAllAchievements()
        {
            // Restoration (R01-R08)
            Reg("R01", "First Light", "Restore your first building", AchievementCategory.Restoration, 50, 2f);
            Reg("R02", "Foundation Layer", "Restore 5 buildings", AchievementCategory.Restoration, 100, 5f);
            Reg("R03", "City Block", "Restore 13 buildings", AchievementCategory.Restoration, 200, 8f);
            Reg("R04", "Perfect Tuner", "Achieve 95%+ accuracy on all 3 nodes of one building", AchievementCategory.Restoration, 150, 5f);
            Reg("R05", "Golden Ratio Adept", "Hit golden-ratio frequency 10 times", AchievementCategory.Restoration, 100, 3f);
            Reg("R06", "Zone Maestro", "Reach RS 100 in any zone", AchievementCategory.Restoration, 250, 10f);
            Reg("R07", "Tri-Zone Restorer", "Restore 3 complete zones", AchievementCategory.Restoration, 400, 15f);
            Reg("R08", "World Restorer", "Restore all 13 zones", AchievementCategory.Restoration, 1000, 25f);

            // Combat (C01-C08)
            Reg("C01", "First Strike", "Defeat your first enemy", AchievementCategory.Combat, 50, 2f);
            Reg("C02", "Golem Slayer", "Defeat 25 mud golems", AchievementCategory.Combat, 100, 5f);
            Reg("C03", "Boss Hunter", "Defeat any boss enemy", AchievementCategory.Combat, 200, 8f);
            Reg("C04", "Perfect Dodge", "Dodge 50 attacks with frequency shield", AchievementCategory.Combat, 150, 5f);
            Reg("C05", "Resonance Warrior", "Defeat an enemy using only resonance pulse", AchievementCategory.Combat, 100, 3f);
            Reg("C06", "Giant Crusher", "Defeat 10 enemies in Giant Mode", AchievementCategory.Combat, 200, 8f);
            Reg("C07", "Reset Drones Down", "Defeat the Moon 8 Reset Drone swarm", AchievementCategory.Combat, 300, 10f);
            Reg("C08", "Final Guardian", "Defeat the Moon 13 final boss", AchievementCategory.Combat, 500, 20f);

            // Exploration (E01-E06)
            Reg("E01", "Curious Wanderer", "Discover 10 points of interest", AchievementCategory.Exploration, 50, 2f);
            Reg("E02", "Cartographer", "Reveal the full world map", AchievementCategory.Exploration, 200, 8f);
            Reg("E03", "Mote Collector", "Collect 7 golden motes", AchievementCategory.Exploration, 150, 5f);
            Reg("E04", "All Motes Found", "Collect all 13 golden motes", AchievementCategory.Exploration, 500, 15f);
            Reg("E05", "Skyward Bound", "Use all 3 airships in one session", AchievementCategory.Exploration, 100, 5f);
            Reg("E06", "Continental Rail", "Complete the full rail network", AchievementCategory.Exploration, 300, 10f);

            // Lore (L01-L06)
            Reg("L01", "First Whisper", "Hear Anastasia's first line", AchievementCategory.Lore, 50, 2f);
            Reg("L02", "Codex Reader", "Unlock 20 codex entries", AchievementCategory.Lore, 100, 5f);
            Reg("L03", "Prophet's Ear", "Activate 6 prophecy stones", AchievementCategory.Lore, 150, 5f);
            Reg("L04", "Zereth's Truth", "Witness Zereth's full revelation", AchievementCategory.Lore, 200, 8f);
            Reg("L05", "Korath's Legacy", "Unlock all Korath teachings", AchievementCategory.Lore, 200, 8f);
            Reg("L06", "World Memory", "Complete all dialogue trees", AchievementCategory.Lore, 400, 15f);

            // Campaign (K01-K13)
            for (int m = 1; m <= 13; m++)
            {
                int reward = m <= 4 ? 100 : m <= 8 ? 200 : m <= 12 ? 300 : 500;
                float rs = m <= 4 ? 3f : m <= 8 ? 5f : m <= 12 ? 8f : 20f;
                Reg($"K{m:D2}", $"Moon {m} Complete", $"Complete Moon {m} campaign", AchievementCategory.Campaign, reward, rs);
            }

            // Social (S01-S05)
            Reg("S01", "Milo's Friend", "Reach max trust with Milo", AchievementCategory.Social, 150, 5f);
            Reg("S02", "Lirael's Harmony", "Reach max trust with Lirael", AchievementCategory.Social, 150, 5f);
            Reg("S03", "Thorne's Bond", "Reach max trust with Thorne", AchievementCategory.Social, 150, 5f);
            Reg("S04", "Veritas's Accord", "Reach max trust with Veritas", AchievementCategory.Social, 150, 5f);
            Reg("S05", "Full Fellowship", "Max trust with all companions", AchievementCategory.Social, 500, 15f);

            // Mini-Games (M01-M06)
            Reg("M01", "Tuning Fork", "Complete 10 tuning mini-games", AchievementCategory.MiniGames, 100, 3f);
            Reg("M02", "Rock Cutter", "Score 500+ in harmonic rock cutting", AchievementCategory.MiniGames, 100, 3f);
            Reg("M03", "Bell Ringer", "Synchronize all 12 bell towers", AchievementCategory.MiniGames, 200, 8f);
            Reg("M04", "Stone Seer", "Activate all 12 prophecy stones", AchievementCategory.MiniGames, 200, 8f);
            Reg("M05", "Aquifer Purifier", "Complete the aquifer purge", AchievementCategory.MiniGames, 200, 8f);
            Reg("M06", "Cosmic Conductor", "Complete the cosmic convergence", AchievementCategory.MiniGames, 500, 15f);

            // Hidden (H01-H12)
            RegH("H01", "Day Out of Time", "Experience the secret 14th day", 500, 20f);
            RegH("H02", "Anastasia Solidified", "Witness Anastasia's full solidification", 300, 10f);
            RegH("H03", "Zereth Redeemed", "Choose to redeem Zereth", 300, 10f);
            RegH("H04", "Cassian's Secret", "Discover Cassian's true identity", 200, 8f);
            RegH("H05", "Orphan Train", "Find the Orphan Train memory", 200, 8f);
            RegH("H06", "White City Vision", "Witness the White City", 200, 8f);
            RegH("H07", "Trigger Room", "Discover Zereth's trigger room", 150, 5f);
            RegH("H08", "Giant's Path", "Use Building Lift on every zone landmark", 200, 8f);
            RegH("H09", "432 Hz Resonance", "Tune a building to exactly 432 Hz", 150, 5f);
            RegH("H10", "Speed Runner", "Complete Moon 1 in under 20 minutes", 150, 5f);
            RegH("H11", "Pacifist Moon", "Complete a Moon without combat", 200, 8f);
            RegH("H12", "True Ending", "Reach the True Ending with 100% RS", 1000, 40f);
        }

        void Reg(string id, string title, string desc, AchievementCategory cat, int aether, float rs)
        {
            var def = new AchievementDef
            {
                id = id, title = title, description = desc,
                category = cat, hidden = false,
                aetherReward = aether, rsReward = rs
            };
            _definitions.Add(def);
            _unlocked[id] = false;
            _progress[id] = 0f;
        }

        void RegH(string id, string title, string desc, int aether, float rs)
        {
            var def = new AchievementDef
            {
                id = id, title = title, description = desc,
                category = AchievementCategory.Hidden, hidden = true,
                aetherReward = aether, rsReward = rs
            };
            _definitions.Add(def);
            _unlocked[id] = false;
            _progress[id] = 0f;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Unlock an achievement by ID. Fires event, grants rewards.
        /// </summary>
        public void Unlock(string achievementId)
        {
            if (!_unlocked.ContainsKey(achievementId)) return;
            if (_unlocked[achievementId]) return;

            _unlocked[achievementId] = true;
            _progress[achievementId] = 1f;
            _totalUnlocked++;

            var def = _definitions.Find(d => d.id == achievementId);
            if (def == null) return;

            Debug.Log($"[Achievement] UNLOCKED: {def.title} ({def.id})");

            // Grant rewards
            GameLoopController.Instance?.QueueRSReward(def.rsReward, $"achievement_{def.id}");

            // HUD notification
            UI.HUDController.Instance?.ShowInteractionPrompt(
                $"ACHIEVEMENT UNLOCKED\n{def.title}");

            OnAchievementUnlocked?.Invoke(def);
            SaveManager.Instance?.MarkDirty();
        }

        /// <summary>
        /// Update progress on a progressive achievement (0..1).
        /// Auto-unlocks at 1.0.
        /// </summary>
        public void SetProgress(string achievementId, float progress)
        {
            if (!_progress.ContainsKey(achievementId)) return;
            if (_unlocked[achievementId]) return;

            _progress[achievementId] = Mathf.Clamp01(progress);
            OnProgressUpdated?.Invoke(achievementId, _progress[achievementId]);

            if (_progress[achievementId] >= 1f)
                Unlock(achievementId);
        }

        public bool IsUnlocked(string id) => _unlocked.TryGetValue(id, out bool v) && v;
        public float GetProgress(string id) => _progress.TryGetValue(id, out float v) ? v : 0f;
        public int TotalUnlocked => _totalUnlocked;
        public int TotalAchievements => _definitions.Count;
        public IReadOnlyList<AchievementDef> Definitions => _definitions;

        // ─── Event Checks ────────────────────────────

        public void CheckBuildingRestored(int totalRestored, bool allPerfect)
        {
            if (totalRestored >= 1)  Unlock("R01");
            if (totalRestored >= 5)  Unlock("R02");
            if (totalRestored >= 13) Unlock("R03");
            if (allPerfect)          Unlock("R04");
        }

        public void CheckEnemyDefeated(int totalDefeated, string enemyType, bool giantMode)
        {
            if (totalDefeated >= 1) Unlock("C01");
            if (enemyType == "mud_golem") SetProgress("C02", totalDefeated / 25f);
            if (giantMode) SetProgress("C06", totalDefeated / 10f);
        }

        public void CheckBossDefeated(string bossId)
        {
            Unlock("C03");
            if (bossId == "reset_drone_carrier") Unlock("C07");
            if (bossId == "final_guardian")       Unlock("C08");
        }

        public void CheckMoonCompleted(int moonNumber)
        {
            if (moonNumber >= 1 && moonNumber <= 13)
                Unlock($"K{moonNumber:D2}");
        }

        public void CheckZoneRS(float rs)
        {
            if (rs >= 100f) Unlock("R06");
        }

        void CheckMoteAchievements()
        {
            var ana = AnastasiaController.Instance;
            if (ana == null) return;
            int count = 0;
            for (int i = 0; i < 13; i++)
                if (ana.IsMoteCollected(i)) count++;
            SetProgress("E03", count / 7f);
            if (count >= 13) Unlock("E04");
        }

        public void CheckCompanionTrust(string companionId, float trust)
        {
            if (trust < 100f) return;
            switch (companionId)
            {
                case "milo":    Unlock("S01"); break;
                case "lirael":  Unlock("S02"); break;
                case "thorne":  Unlock("S03"); break;
                case "veritas": Unlock("S04"); break;
            }

            // Check all companions maxed
            if (IsUnlocked("S01") && IsUnlocked("S02") &&
                IsUnlocked("S03") && IsUnlocked("S04"))
                Unlock("S05");
        }

        public void CheckDayOutOfTime() => Unlock("H01");
        public void CheckSolidification() => Unlock("H02");
        public void CheckZerethRedeemed() => Unlock("H03");
        public void CheckTrueEnding() => Unlock("H12");

        // ─── Save/Load ──────────────────────────────

        public AchievementSaveData GetSaveData()
        {
            var ids = new List<string>();
            var prog = new List<float>();
            foreach (var kvp in _unlocked)
            {
                if (kvp.Value)
                    ids.Add(kvp.Key);
            }
            foreach (var kvp in _progress)
                prog.Add(kvp.Value);

            return new AchievementSaveData
            {
                unlockedIds = ids.ToArray(),
                progressValues = prog.ToArray(),
                progressKeys = new List<string>(_progress.Keys).ToArray(),
                totalUnlocked = _totalUnlocked
            };
        }

        public void LoadSaveData(AchievementSaveData data)
        {
            if (data.unlockedIds != null)
            {
                foreach (var id in data.unlockedIds)
                {
                    if (_unlocked.ContainsKey(id))
                        _unlocked[id] = true;
                }
            }
            if (data.progressKeys != null && data.progressValues != null)
            {
                int count = Mathf.Min(data.progressKeys.Length, data.progressValues.Length);
                for (int i = 0; i < count; i++)
                {
                    if (_progress.ContainsKey(data.progressKeys[i]))
                        _progress[data.progressKeys[i]] = data.progressValues[i];
                }
            }
            _totalUnlocked = data.totalUnlocked;
        }
    }

    [Serializable]
    public class AchievementSaveData
    {
        public string[] unlockedIds;
        public string[] progressKeys;
        public float[] progressValues;
        public int totalUnlocked;
    }
}
