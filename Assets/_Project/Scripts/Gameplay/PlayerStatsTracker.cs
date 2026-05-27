using UnityEngine;
using Tartaria.Save;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Player Stats Tracker — tracks kills, deaths, damage dealt, healing, RS earned.
    /// Singleton instance accessible from combat/healing/economy systems.
    /// Persists stats to save file on game save.
    /// </summary>
    public class PlayerStatsTracker : MonoBehaviour, ISaveDataProvider
    {
        public static PlayerStatsTracker Instance { get; private set; }

        [Header("Combat Stats")]
        public int TotalKills { get; private set; }
        public int TotalDeaths { get; private set; }
        public float TotalDamageDealt { get; private set; }
        public float TotalDamageTaken { get; private set; }
        public float TotalHealingReceived { get; private set; }

        [Header("Economy Stats")]
        public float TotalRSEarned { get; private set; }
        public float TotalRSSpent { get; private set; }

        [Header("Exploration Stats")]
        public int MoonsCompleted { get; private set; }
        public int QuestsCompleted { get; private set; }
        public int BuildingsRestored { get; private set; }
        public float TotalDistanceTraveled { get; private set; }

        Vector3 _lastPosition;
        float _sessionStartTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("PlayerStatsTracker");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<PlayerStatsTracker>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            _sessionStartTime = Time.time;
        }

        void Start()
        {
            // Load stats from save
            LoadStatsFromSave();

            // Track player position for distance
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _lastPosition = player.transform.position;
            }
        }

        void Update()
        {
            // Track distance traveled
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(player.transform.position, _lastPosition);
                if (distance > 0.1f)  // Threshold to ignore jitter
                {
                    TotalDistanceTraveled += distance;
                    _lastPosition = player.transform.position;
                }
            }
        }

        // === Public API ===

        public void RecordKill(string enemyType)
        {
            TotalKills++;
            Debug.Log($"[PlayerStats] Kill recorded: {enemyType} (Total: {TotalKills})");
        }

        public void RecordDeath()
        {
            TotalDeaths++;
            Debug.Log($"[PlayerStats] Death recorded (Total: {TotalDeaths})");
        }

        public void RecordDamageDealt(float amount)
        {
            TotalDamageDealt += amount;
        }

        public void RecordDamageTaken(float amount)
        {
            TotalDamageTaken += amount;
        }

        public void RecordHealing(float amount)
        {
            TotalHealingReceived += amount;
        }

        public void RecordRSEarned(float amount)
        {
            TotalRSEarned += amount;
        }

        public void RecordRSSpent(float amount)
        {
            TotalRSSpent += amount;
        }

        public void RecordMoonCompleted(int moonNumber)
        {
            MoonsCompleted++;
            Debug.Log($"[PlayerStats] Moon {moonNumber} completed (Total: {MoonsCompleted}/13)");
        }

        public void RecordQuestCompleted(string questId)
        {
            QuestsCompleted++;
            Debug.Log($"[PlayerStats] Quest completed: {questId} (Total: {QuestsCompleted})");
        }

        public void RecordBuildingRestored(string buildingName)
        {
            BuildingsRestored++;
            Debug.Log($"[PlayerStats] Building restored: {buildingName} (Total: {BuildingsRestored})");
        }

        public float GetPlayTime()
        {
            return Time.time - _sessionStartTime;
        }

        void LoadStatsFromSave()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.RegisterProvider(this);
                Debug.Log("[PlayerStats] Registered with SaveManager");
            }
        }

        public void SaveStats()
        {
            SaveManager.Instance?.Save();
        }

        // ISaveDataProvider implementation
        string ISaveDataProvider.GetProviderKey() => "PlayerStats";

        object ISaveDataProvider.GetSaveData()
        {
            return new PlayerStatsSaveData
            {
                totalKills = TotalKills,
                totalDeaths = TotalDeaths,
                totalDamageDealt = TotalDamageDealt,
                totalDamageTaken = TotalDamageTaken,
                totalHealingReceived = TotalHealingReceived,
                totalRSEarned = TotalRSEarned,
                totalRSSpent = TotalRSSpent,
                moonsCompleted = MoonsCompleted,
                questsCompleted = QuestsCompleted,
                buildingsRestored = BuildingsRestored,
                totalDistanceTraveled = TotalDistanceTraveled
            };
        }

        void ISaveDataProvider.RestoreSaveData(object data)
        {
            if (data is string json)
            {
                var saveData = JsonUtility.FromJson<PlayerStatsSaveData>(json);
                TotalKills = saveData.totalKills;
                TotalDeaths = saveData.totalDeaths;
                TotalDamageDealt = saveData.totalDamageDealt;
                TotalDamageTaken = saveData.totalDamageTaken;
                TotalHealingReceived = saveData.totalHealingReceived;
                TotalRSEarned = saveData.totalRSEarned;
                TotalRSSpent = saveData.totalRSSpent;
                MoonsCompleted = saveData.moonsCompleted;
                QuestsCompleted = saveData.questsCompleted;
                BuildingsRestored = saveData.buildingsRestored;
                TotalDistanceTraveled = saveData.totalDistanceTraveled;
                Debug.Log("[PlayerStats] Stats restored from save");
            }
        }

        [System.Serializable]
        class PlayerStatsSaveData
        {
            public int totalKills;
            public int totalDeaths;
            public float totalDamageDealt;
            public float totalDamageTaken;
            public float totalHealingReceived;
            public float totalRSEarned;
            public float totalRSSpent;
            public int moonsCompleted;
            public int questsCompleted;
            public int buildingsRestored;
            public float totalDistanceTraveled;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
