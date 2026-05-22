using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Save
{
    /// <summary>
    /// Manages New Game Plus (NG+) system for replayability.
    /// Tracks NG+ cycles, carries over unlocks, and scales difficulty.
    /// </summary>
    public class NewGamePlusSystem : MonoBehaviour
    {
        [System.Serializable]
        public class NewGamePlusData
        {
            public int ngPlusCycle = 0;               // 0 = first playthrough, 1 = NG+1, etc.
            public bool isNewGamePlus = false;
            public float difficultyMultiplier = 1f;
            public bool[] permanentUnlocks = new bool[50]; // Persistent unlocks across cycles
            public int totalMoonsCleared = 0;
            public int totalPlaythroughs = 0;
        }

        [Header("NG+ Settings")]
        [SerializeField] private float difficultyIncreasePerCycle = 0.25f;
        [SerializeField] private float maxDifficultyMultiplier = 3f;
        [SerializeField] private bool carryOverEquipment = true;
        [SerializeField] private bool carryOverAbilities = true;
        [SerializeField] private bool carryOverResources = false;

        [Header("Current State")]
        [SerializeField] private NewGamePlusData currentNGPlusData;

        private static NewGamePlusSystem instance;
        public static NewGamePlusSystem Instance => instance;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (currentNGPlusData == null)
            {
                currentNGPlusData = new NewGamePlusData();
            }

            LoadNGPlusData();
        }

        /// <summary>
        /// Check if player has completed all 13 Moons and can start NG+.
        /// </summary>
        public bool CanStartNewGamePlus()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr == null) return false;

            for (int i = 1; i <= 13; i++)
            {
                if (saveMgr.GetMoonProgress(i) < 1f) return false;
            }

            return true;
        }

        /// <summary>
        /// Start a New Game Plus cycle, carrying over eligible data.
        /// </summary>
        public void StartNewGamePlus()
        {
            if (!CanStartNewGamePlus())
            {
                Debug.LogWarning("[NewGamePlus] Cannot start NG+ - not all Moons cleared");
                return;
            }

            Debug.Log($"[NewGamePlus] Starting NG+ cycle {currentNGPlusData.ngPlusCycle + 1}");

            // Backup player data to carry over
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null)
            {
                // Note: SaveManager.BackupPlayerData() API pending
                // var playerData = saveMgr.GetPlayerData();
                // BackupPlayerData(playerData);
            }

            // Increment cycle
            currentNGPlusData.ngPlusCycle++;
            currentNGPlusData.isNewGamePlus = true;
            currentNGPlusData.totalPlaythroughs++;
            currentNGPlusData.totalMoonsCleared += 13;

            // Calculate new difficulty
            float newDifficulty = 1f + (currentNGPlusData.ngPlusCycle * difficultyIncreasePerCycle);
            currentNGPlusData.difficultyMultiplier = Mathf.Min(newDifficulty, maxDifficultyMultiplier);

            // Reset world progress (clear all moon progress)
            if (saveMgr != null)
            {
                for (int i = 1; i <= 13; i++)
                {
                    saveMgr.SetMoonProgress(i, 0f);
                }
            }

            // Restore carried-over data
            RestoreCarryOverData();

            SaveNGPlusData();

            GameEvents.FireNewGamePlusStarted(currentNGPlusData.ngPlusCycle);
        }

        /// <summary>
        /// Reset to a fresh New Game (not NG+).
        /// </summary>
        public void StartFreshGame()
        {
            Debug.Log("[NewGamePlus] Starting fresh game (no carry-over)");

            currentNGPlusData = new NewGamePlusData();
            currentNGPlusData.totalPlaythroughs++;

            SaveNGPlusData();
        }

        private void BackupPlayerData(object playerData)
        {
            // Store carry-over data in PlayerPrefs or separate save file
            // Equipment, abilities, etc.
            if (carryOverEquipment)
            {
                PlayerPrefs.SetString("NGPlus_Equipment", JsonUtility.ToJson(playerData));
            }

            if (carryOverAbilities)
            {
                PlayerPrefs.SetString("NGPlus_Abilities", JsonUtility.ToJson(playerData));
            }

            PlayerPrefs.Save();
        }

        private void RestoreCarryOverData()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr == null) return;

            if (carryOverEquipment && PlayerPrefs.HasKey("NGPlus_Equipment"))
            {
                string equipmentJson = PlayerPrefs.GetString("NGPlus_Equipment");
                // Restore equipment from JSON
                Debug.Log("[NewGamePlus] Restoring equipment from previous cycle");
            }

            if (carryOverAbilities && PlayerPrefs.HasKey("NGPlus_Abilities"))
            {
                string abilitiesJson = PlayerPrefs.GetString("NGPlus_Abilities");
                // Restore abilities from JSON
                Debug.Log("[NewGamePlus] Restoring abilities from previous cycle");
            }
        }

        private void LoadNGPlusData()
        {
            if (PlayerPrefs.HasKey("NGPlusData"))
            {
                string json = PlayerPrefs.GetString("NGPlusData");
                currentNGPlusData = JsonUtility.FromJson<NewGamePlusData>(json);
                Debug.Log($"[NewGamePlus] Loaded NG+ cycle: {currentNGPlusData.ngPlusCycle}");
            }
            else
            {
                currentNGPlusData = new NewGamePlusData();
            }
        }

        private void SaveNGPlusData()
        {
            string json = JsonUtility.ToJson(currentNGPlusData);
            PlayerPrefs.SetString("NGPlusData", json);
            PlayerPrefs.Save();
            Debug.Log($"[NewGamePlus] Saved NG+ data, cycle: {currentNGPlusData.ngPlusCycle}");
        }

        /// <summary>
        /// Unlock a permanent NG+ reward (e.g. costumes, modes, cheats).
        /// </summary>
        public void UnlockPermanentReward(int rewardId)
        {
            if (rewardId < 0 || rewardId >= currentNGPlusData.permanentUnlocks.Length) return;

            if (!currentNGPlusData.permanentUnlocks[rewardId])
            {
                currentNGPlusData.permanentUnlocks[rewardId] = true;
                SaveNGPlusData();
                Debug.Log($"[NewGamePlus] Unlocked permanent reward: {rewardId}");
                GameEvents.FirePermanentUnlockEarned(rewardId);
            }
        }

        public bool IsPermanentRewardUnlocked(int rewardId)
        {
            if (rewardId < 0 || rewardId >= currentNGPlusData.permanentUnlocks.Length) return false;
            return currentNGPlusData.permanentUnlocks[rewardId];
        }

        public int GetNGPlusCycle() => currentNGPlusData.ngPlusCycle;
        public float GetDifficultyMultiplier() => currentNGPlusData.difficultyMultiplier;
        public bool IsNewGamePlus() => currentNGPlusData.isNewGamePlus;
        public int GetTotalMoonsCleared() => currentNGPlusData.totalMoonsCleared;
        public int GetTotalPlaythroughs() => currentNGPlusData.totalPlaythroughs;

        /// <summary>
        /// Apply difficulty scaling to an enemy's base health.
        /// </summary>
        public float ScaleEnemyHealth(float baseHealth)
        {
            return baseHealth * currentNGPlusData.difficultyMultiplier;
        }

        /// <summary>
        /// Apply difficulty scaling to an enemy's base damage.
        /// </summary>
        public float ScaleEnemyDamage(float baseDamage)
        {
            return baseDamage * currentNGPlusData.difficultyMultiplier;
        }

        /// <summary>
        /// Apply difficulty scaling to resource drops.
        /// </summary>
        public int ScaleResourceDrop(int baseAmount)
        {
            // Increased rewards in NG+ to compensate for difficulty
            float multiplier = 1f + (currentNGPlusData.ngPlusCycle * 0.15f);
            return Mathf.CeilToInt(baseAmount * multiplier);
        }
    }
}
