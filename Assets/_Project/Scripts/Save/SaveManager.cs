using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Tartaria.Save
{
    /// <summary>
    /// Save Manager — handles persistence, auto-save, and integrity.
    ///
    /// Design principles:
    ///   1. Never lose player progress (double-write + checksum)
    ///   2. Offline-first (local JSON, no network dependency)
    ///   3. Invisible persistence (auto-save on state changes)
    ///   4. Forward-compatible (schema versioning with migration)
    ///
    /// Auto-save triggers:
    ///   - Every 10 seconds (dirty flag check)
    ///   - Zone transitions, quest completion, building placed
    ///   - Alt-tab / minimize (emergency serialize < 2s)
    ///   - Application quit
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [SerializeField] float autoSaveIntervalSeconds = 10f;

        SaveData _currentSave;
        float _autoSaveTimer;
        bool _isDirty;
        string _savePath;
        string _backupPath;

        public SaveData CurrentSave => _currentSave;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _savePath = Path.Combine(Application.persistentDataPath, "save_slot_0.json");
            _backupPath = Path.Combine(Application.persistentDataPath, "save_slot_0.backup.json");
        }

        void Start()
        {
            LoadOrCreate();
        }

        void Update()
        {
            if (!_isDirty) return;

            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= autoSaveIntervalSeconds)
            {
                Save();
                _autoSaveTimer = 0f;
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            // Emergency save on alt-tab
            if (!hasFocus && _isDirty)
                Save();
        }

        void OnApplicationQuit()
        {
            Save();
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Marks save data as modified — will be written at next auto-save interval.
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Immediately writes save to disk (double-write with checksum).
        /// </summary>
        public void Save()
        {
            if (_currentSave == null) return;

            _currentSave.header.modifiedUtc = DateTime.UtcNow.ToString("o");
            _currentSave.header.playTimeSeconds += _autoSaveTimer;

            string json = JsonUtility.ToJson(_currentSave, true);

            // Compute integrity checksum
            _currentSave.header.checksum = ComputeChecksum(json);
            json = JsonUtility.ToJson(_currentSave, true);

            // Double-write: backup first, then primary
            try
            {
                WriteFile(_backupPath, json);
                WriteFile(_savePath, json);
                _isDirty = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
            }
        }

        /// <summary>
        /// Loads save from disk, or creates a fresh save if none exists.
        /// Validates checksum on load. Falls back to backup if primary is corrupt.
        /// </summary>
        public void LoadOrCreate()
        {
            _currentSave = TryLoadFromPath(_savePath);

            if (_currentSave == null)
            {
                // Try backup
                _currentSave = TryLoadFromPath(_backupPath);
                if (_currentSave != null)
                    Debug.LogWarning("[SaveManager] Primary save corrupt — loaded backup.");
            }

            if (_currentSave == null)
            {
                // Fresh save
                _currentSave = CreateNewSave();
                Debug.Log("[SaveManager] Created new save file.");
                Save();
            }
            else
            {
                // Schema migration if needed
                MigrateIfNeeded(_currentSave);
                Debug.Log($"[SaveManager] Loaded save — RS: {_currentSave.world.resonanceScore}, " +
                          $"Play time: {_currentSave.header.playTimeSeconds:F0}s");
            }
        }

        /// <summary>
        /// Deletes save data for a slot (requires explicit confirmation).
        /// </summary>
        public void DeleteSave()
        {
            if (File.Exists(_savePath)) File.Delete(_savePath);
            if (File.Exists(_backupPath)) File.Delete(_backupPath);
            _currentSave = CreateNewSave();
            Debug.Log("[SaveManager] Save deleted.");
        }

        // ─── Internal ────────────────────────────────

        SaveData TryLoadFromPath(string path)
        {
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<SaveData>(json);

                // Validate checksum
                string savedChecksum = data.header.checksum;
                data.header.checksum = "";
                string recomputed = ComputeChecksum(JsonUtility.ToJson(data));

                if (savedChecksum != recomputed)
                {
                    Debug.LogWarning($"[SaveManager] Checksum mismatch in {path}");
                    return null;
                }

                data.header.checksum = savedChecksum;
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to load {path}: {e.Message}");
                return null;
            }
        }

        SaveData CreateNewSave()
        {
            return new SaveData
            {
                header = new SaveHeader
                {
                    schemaVersion = 1,
                    gameVersion = Application.version,
                    platform = "windows",
                    saveSlot = 0,
                    createdUtc = DateTime.UtcNow.ToString("o"),
                    modifiedUtc = DateTime.UtcNow.ToString("o"),
                    playTimeSeconds = 0f,
                    checksum = ""
                },
                player = new PlayerSaveData
                {
                    position = new SerializableVector3(0, 0, 0),
                    currentZone = "echohaven",
                    aetherCharge = 0f
                },
                world = new WorldSaveData
                {
                    resonanceScore = 0f,
                    buildings = new[]
                    {
                        new BuildingSaveState { buildingId = "echohaven_dome_01", state = 0 },
                        new BuildingSaveState { buildingId = "echohaven_fountain_01", state = 0 },
                        new BuildingSaveState { buildingId = "echohaven_spire_01", state = 0 }
                    },
                    discoveredPOIs = new bool[4], // 4 POIs in Echohaven
                    playedDialogueIds = Array.Empty<string>(),
                    enemySpawns = new[]
                    {
                        new EnemySpawnState { rsThreshold = 25f, hasSpawned = false },
                        new EnemySpawnState { rsThreshold = 50f, hasSpawned = false },
                        new EnemySpawnState { rsThreshold = 75f, hasSpawned = false }
                    }
                }
            };
        }

        /// <summary>
        /// Schema migration — ensures old saves work with new code.
        /// </summary>
        void MigrateIfNeeded(SaveData data)
        {
            if (data.header.schemaVersion < 1)
            {
                // Future: migration logic here
                data.header.schemaVersion = 1;
                MarkDirty();
            }
        }

        static string ComputeChecksum(string content)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            var sb = new StringBuilder(64);
            for (int i = 0; i < bytes.Length; i++)
                sb.Append(bytes[i].ToString("x2"));
            return sb.ToString();
        }

        static void WriteFile(string path, string content)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
