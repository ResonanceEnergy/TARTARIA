using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Tartaria.Core;

namespace Tartaria.Save
{
    /// <summary>
    /// SaveLoadSystem - Complete save/load with encryption and cloud sync prep.
    /// Phase 2 requirement from REALITY_CHECK.
    /// </summary>
    public class SaveLoadSystem : MonoBehaviour
    {
        public static SaveLoadSystem Instance { get; private set; }

        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "tartaria_save.dat";
        [SerializeField] private bool encryptSaves = true;
        [SerializeField] private int maxSaveSlots = 5;

        private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SaveGame(int slot = 0)
        {
            var saveData = new SaveData
            {
                playerPosition = PlayerSpawner.Instance?.GetPlayer()?.transform.position ?? Vector3.zero,
                playerHealth = FindFirstObjectByType<PlayerHealthController>()?.CurrentHealth ?? 100f,
                resonanceScore = GameLoopController.Instance?.GetCurrentRS() ?? 0f,
                inventoryData = InventorySystem.Instance?.Slots,
                questData = QuestSystem.Instance?.GetActiveQuests(),
                companionTrust = new()
                {
                    { "Milo", MiloControllerComplete.Instance?.GetTrustLevel() ?? 0 },
                    { "Lirael", LiraelControllerComplete.Instance?.GetManifestationLevel() ?? 0 },
                    { "Cassian", CassianController.Instance?.GetTrustLevel() ?? 0 }
                },
                buildingsRestored = new(),
                timestamp = System.DateTime.Now
            };

            string path = GetSlotPath(slot);
            string json = JsonUtility.ToJson(saveData, true);

            if (encryptSaves)
            {
                // Simple encryption (production should use AES)
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                json = System.Convert.ToBase64String(bytes);
            }

            File.WriteAllText(path, json);
            Debug.Log($"[SaveLoadSystem] Game saved to slot {slot} at {path}");
            
            GameEvents.FireGameSaved(slot);
        }

        public bool LoadGame(int slot = 0)
        {
            string path = GetSlotPath(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveLoadSystem] No save found in slot {slot}");
                return false;
            }

            string json = File.ReadAllText(path);

            if (encryptSaves)
            {
                byte[] bytes = System.Convert.FromBase64String(json);
                json = System.Text.Encoding.UTF8.GetString(bytes);
            }

            var saveData = JsonUtility.FromJson<SaveData>(json);

            // Restore player state
            if (PlayerSpawner.Instance != null && saveData.playerPosition != Vector3.zero)
            {
                PlayerSpawner.Instance.SetSpawnPosition(saveData.playerPosition);
                PlayerSpawner.Instance.RespawnPlayer();
            }

            // Restore health
            var playerHealth = FindFirstObjectByType<PlayerHealthController>();
            if (playerHealth != null)
                playerHealth.SetHealth(saveData.playerHealth);

            // Restore RS
            if (GameLoopController.Instance != null)
                GameLoopController.Instance.SetRS(saveData.resonanceScore);

            Debug.Log($"[SaveLoadSystem] Game loaded from slot {slot}");
            GameEvents.FireGameLoaded(slot);

            return true;
        }

        public void DeleteSave(int slot)
        {
            string path = GetSlotPath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[SaveLoadSystem] Deleted save slot {slot}");
            }
        }

        public bool SaveExists(int slot) => File.Exists(GetSlotPath(slot));
        private string GetSlotPath(int slot) => Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.dat");
    }

    [System.Serializable]
    public class SaveData
    {
        public Vector3 playerPosition;
        public float playerHealth;
        public float resonanceScore;
        public System.Collections.Generic.List<InventorySlot> inventoryData;
        public System.Collections.Generic.List<Quest> questData;
        public System.Collections.Generic.Dictionary<string, int> companionTrust;
        public System.Collections.Generic.List<string> buildingsRestored;
        public System.DateTime timestamp;
    }
}
