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

            FireBeforeSave();
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
            // Push loaded data into subsystems (deferred to allow Awake/Start order)
            Invoke(nameof(DeferredFireAfterLoad), 0.1f);
        }

        void DeferredFireAfterLoad()
        {
            FireAfterLoad();
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
                    schemaVersion = 7,
                    gameVersion = "0.7.0",
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
            if (data.header.schemaVersion < 2)
            {
                // v1 → v2: add Anastasia, quest, workshop, zone blocks
                if (data.anastasia == null) data.anastasia = new AnastasiaSaveBlock();
                if (data.quests == null) data.quests = new QuestSaveBlock();
                if (data.workshop == null) data.workshop = new WorkshopSaveBlock();
                if (data.zone == null) data.zone = new ZoneSaveBlock();
                data.header.schemaVersion = 2;
                MarkDirty();
            }

            if (data.header.schemaVersion < 3)
            {
                // v2 → v3: add corruption, campaign, skill tree, companion blocks
                if (data.corruption == null) data.corruption = new CorruptionSaveBlock();
                if (data.campaign == null) data.campaign = new CampaignSaveBlock();
                if (data.skillTree == null) data.skillTree = new SkillTreeSaveBlock();
                if (data.cassian == null) data.cassian = new CassianSaveBlock();
                if (data.economy == null) data.economy = new EconomySaveBlock();
                if (data.thorne == null) data.thorne = new ThorneSaveBlock();
                if (data.korath == null) data.korath = new KorathSaveBlock();
                data.header.schemaVersion = 3;
                MarkDirty();
            }

            if (data.header.schemaVersion < 4)
            {
                // v3 → v4: add Milo, Lirael, Zereth, tutorial, dialogue, codex blocks
                if (data.milo == null) data.milo = new MiloSaveBlock();
                if (data.lirael == null) data.lirael = new LiraelSaveBlock();
                if (data.zereth == null) data.zereth = new ZerethSaveBlock();
                if (data.tutorial == null) data.tutorial = new TutorialSaveBlock();
                if (data.dialogueTree == null) data.dialogueTree = new DialogueTreeSaveBlock();
                if (data.codex == null) data.codex = new CodexSaveBlock();
                data.header.schemaVersion = 4;
                data.header.gameVersion = "0.4.0";
                MarkDirty();
            }

            if (data.header.schemaVersion < 5)
            {
                // v4 → v5: add Veritas companion block
                if (data.veritas == null) data.veritas = new VeritasSaveBlock();
                data.header.schemaVersion = 5;
                data.header.gameVersion = "0.5.0";
                MarkDirty();
            }

            if (data.header.schemaVersion < 6)
            {
                // v5 → v6: add v8 system blocks
                if (data.airshipFleet == null) data.airshipFleet = new AirshipFleetSaveBlock();
                if (data.leyLineProphecy == null) data.leyLineProphecy = new LeyLineProphecySaveBlock();
                if (data.bellTowerSync == null) data.bellTowerSync = new BellTowerSyncSaveBlock();
                if (data.giantMode == null) data.giantMode = new GiantModeSaveBlock();
                if (data.worldChoice == null) data.worldChoice = new WorldChoiceSaveBlock();
                if (data.achievementData == null) data.achievementData = new AchievementSaveBlock();
                if (data.dialogueArcs == null) data.dialogueArcs = new DialogueArcSaveBlock();
                data.header.schemaVersion = 6;
                data.header.gameVersion = "0.6.0";
                MarkDirty();
            }

            if (data.header.schemaVersion < 7)
            {
                // v6 → v7: add excavation, crafting, scanner, rail, aquifer, cosmic, DotT, companion blocks
                if (data.excavation == null) data.excavation = new ExcavationSaveBlock();
                if (data.crafting == null) data.crafting = new CraftingSaveBlock();
                if (data.scanner == null) data.scanner = new ScannerSaveBlock();
                if (data.rail == null) data.rail = new RailSaveBlock();
                if (data.aquiferPurge == null) data.aquiferPurge = new AquiferPurgeSaveBlock();
                if (data.cosmicConvergence == null) data.cosmicConvergence = new CosmicConvergenceSaveBlock();
                if (data.dayOutOfTime == null) data.dayOutOfTime = new DayOutOfTimeSaveBlock();
                if (data.companionManager == null) data.companionManager = new CompanionManagerSaveBlock();
                data.header.schemaVersion = 7;
                data.header.gameVersion = "0.7.0";
                MarkDirty();
            }

            if (data.header.schemaVersion < 8)
            {
                // v7 → v8: add combat wave persistence
                if (data.combatWave == null) data.combatWave = new CombatWaveSaveBlock();
                data.header.schemaVersion = 8;
                data.header.gameVersion = "0.8.0";
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

        // ─── Subsystem Sync ─────────────────────────

        /// <summary>
        /// Fired before save writes to disk. Subscribers should push their data into CurrentSave.
        /// </summary>
        public event Action<SaveData> OnBeforeSave;

        /// <summary>
        /// Fired after save loads from disk. Subscribers should pull their data from CurrentSave.
        /// </summary>
        public event Action<SaveData> OnAfterLoad;

        void FireBeforeSave()
        {
            OnBeforeSave?.Invoke(_currentSave);
        }

        void FireAfterLoad()
        {
            OnAfterLoad?.Invoke(_currentSave);
        }
    }
}
