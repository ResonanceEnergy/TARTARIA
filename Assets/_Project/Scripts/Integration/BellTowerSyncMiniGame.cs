using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12 (Crystal Moon) mini-game — Bell Tower Synchronization.
    /// 12 towers must be tuned to Earth's Schumann resonance (7.83 Hz fundamental).
    /// Each tower has a unique harmonic offset; the player must tune all 12
    /// within a narrow window to trigger the planetary resonance cascade.
    ///
    /// Cross-ref: docs/03C_MOON_MECHANICS_DETAILED.md §Moon 12, docs/13_MINI_GAMES.md
    /// </summary>
    [DisallowMultipleComponent]
    public class BellTowerSyncMiniGame : MonoBehaviour
    {
        public static BellTowerSyncMiniGame Instance { get; private set; }

        // ─── Constants ───────────────────────────────

        public const int TotalTowers = 12;
        public const float SchumannFrequency = 7.83f;
        public const float TuningTolerance = 0.15f;       // Hz tolerance for "in tune"
        public const float PerfectTolerance = 0.05f;       // Hz for "perfect" rating
        public const float TuningDecayPerSecond = 0.02f;   // towers drift out of tune
        public const float CascadeThreshold = 0.85f;       // 85% sync required for cascade

        // ─── Tower Data ──────────────────────────────

        [Serializable]
        public class BellTowerDef
        {
            public string towerId;
            public Vector3 worldPosition;
            public float harmonicOffset;   // unique offset from Schumann base
            public float driftRate;        // how fast this tower drifts (difficulty)
        }

        [SerializeField] BellTowerDef[] towerDefinitions;

        // ─── State ───────────────────────────────────

        readonly float[] _towerFrequencies = new float[TotalTowers];
        readonly bool[] _towerSynced = new bool[TotalTowers];
        readonly float[] _towerAccuracy = new float[TotalTowers];  // 0..1
        int _towersSynced;
        float _planetaryResonanceScore;
        bool _miniGameActive;
        bool _cascadeTriggered;

        // ─── Events ─────────────────────────────────

        public event Action<int, float> OnTowerTuned;        // towerIndex, accuracy
        public event Action<int> OnTowerSynced;              // towerIndex
        public event Action<int> OnTowerDesynced;            // towerIndex
        public event Action<float> OnResonanceScoreChanged;  // new score
        public event Action OnCascadeTriggered;

        // ─── Lifecycle ───────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            // Initialize towers to random frequencies (detuned)
            for (int i = 0; i < TotalTowers; i++)
            {
                float offset = towerDefinitions != null && i < towerDefinitions.Length
                    ? towerDefinitions[i].harmonicOffset
                    : UnityEngine.Random.Range(-2f, 2f);
                _towerFrequencies[i] = SchumannFrequency + offset;
            }
        }

        void Update()
        {
            if (!_miniGameActive || _cascadeTriggered) return;

            ApplyDrift();
            RecalculateSyncState();
            UpdateResonanceScore();
        }

        // ─── Public API ──────────────────────────────

        public void StartMiniGame()
        {
            _miniGameActive = true;
            _cascadeTriggered = false;
            Debug.Log("[BellTowerSync] Mini-game started. Synchronize 12 towers to Schumann resonance.");
        }

        public void StopMiniGame()
        {
            _miniGameActive = false;
        }

        /// <summary>
        /// Adjust a tower's frequency toward Schumann. Called by player interaction.
        /// </summary>
        public void TuneTower(int towerIndex, float adjustment)
        {
            if (!_miniGameActive) return;
            if (towerIndex < 0 || towerIndex >= TotalTowers) return;

            _towerFrequencies[towerIndex] += adjustment;

            float diff = Mathf.Abs(_towerFrequencies[towerIndex] - SchumannFrequency);
            float accuracy = 1f - Mathf.Clamp01(diff / 2f);
            _towerAccuracy[towerIndex] = accuracy;

            OnTowerTuned?.Invoke(towerIndex, accuracy);
        }

        /// <summary>
        /// Get the current frequency of a tower.
        /// </summary>
        public float GetTowerFrequency(int towerIndex)
        {
            if (towerIndex < 0 || towerIndex >= TotalTowers) return 0f;
            return _towerFrequencies[towerIndex];
        }

        /// <summary>
        /// Check if a specific tower is in sync.
        /// </summary>
        public bool IsTowerSynced(int towerIndex)
        {
            if (towerIndex < 0 || towerIndex >= TotalTowers) return false;
            return _towerSynced[towerIndex];
        }

        public int TowersSynced => _towersSynced;
        public float PlanetaryResonanceScore => _planetaryResonanceScore;
        public bool IsCascadeTriggered => _cascadeTriggered;

        // ─── Internal ────────────────────────────────

        void ApplyDrift()
        {
            for (int i = 0; i < TotalTowers; i++)
            {
                float drift = towerDefinitions != null && i < towerDefinitions.Length
                    ? towerDefinitions[i].driftRate
                    : TuningDecayPerSecond;

                // Towers drift away from Schumann unless recently tuned
                float direction = _towerFrequencies[i] > SchumannFrequency ? 1f : -1f;
                _towerFrequencies[i] += direction * drift * Time.deltaTime;
            }
        }

        void RecalculateSyncState()
        {
            int previousSynced = _towersSynced;
            _towersSynced = 0;

            for (int i = 0; i < TotalTowers; i++)
            {
                float diff = Mathf.Abs(_towerFrequencies[i] - SchumannFrequency);
                bool wasSync = _towerSynced[i];
                _towerSynced[i] = diff <= TuningTolerance;
                _towerAccuracy[i] = 1f - Mathf.Clamp01(diff / 2f);

                if (_towerSynced[i])
                    _towersSynced++;

                // Sync/desync events
                if (_towerSynced[i] && !wasSync)
                    OnTowerSynced?.Invoke(i);
                else if (!_towerSynced[i] && wasSync)
                    OnTowerDesynced?.Invoke(i);
            }

            // Progress quest whenever a tower newly syncs
            if (_towersSynced > previousSynced)
            {
                QuestManager.Instance?.ProgressByType(
                    QuestObjectiveType.CompleteMiniGame, "bell_tower_sync");
            }
        }

        void UpdateResonanceScore()
        {
            float totalAccuracy = 0f;
            for (int i = 0; i < TotalTowers; i++)
                totalAccuracy += _towerAccuracy[i];

            float newScore = totalAccuracy / TotalTowers;
            if (Mathf.Abs(newScore - _planetaryResonanceScore) > 0.001f)
            {
                _planetaryResonanceScore = newScore;
                OnResonanceScoreChanged?.Invoke(_planetaryResonanceScore);
            }

            // Cascade trigger
            if (!_cascadeTriggered && _planetaryResonanceScore >= CascadeThreshold && _towersSynced == TotalTowers)
            {
                _cascadeTriggered = true;
                OnCascadeTriggered?.Invoke();
                QuestManager.Instance?.ProgressByType(
                    QuestObjectiveType.CompleteMiniGame, "bell_tower_sync_game");
                Debug.Log($"[BellTowerSync] PLANETARY RESONANCE CASCADE! Score: {_planetaryResonanceScore:F3}");
            }
        }

        // ─── Save/Load ──────────────────────────────

        public BellTowerSaveData GetSaveData()
        {
            return new BellTowerSaveData
            {
                towerFrequencies = (float[])_towerFrequencies.Clone(),
                towersSynced = _towersSynced,
                resonanceScore = _planetaryResonanceScore,
                miniGameActive = _miniGameActive,
                cascadeTriggered = _cascadeTriggered
            };
        }

        public void LoadSaveData(BellTowerSaveData data)
        {
            if (data.towerFrequencies != null)
            {
                int count = Mathf.Min(data.towerFrequencies.Length, TotalTowers);
                for (int i = 0; i < count; i++)
                    _towerFrequencies[i] = data.towerFrequencies[i];
            }
            _towersSynced = data.towersSynced;
            _planetaryResonanceScore = data.resonanceScore;
            _miniGameActive = data.miniGameActive;
            _cascadeTriggered = data.cascadeTriggered;
        }
    }

    [Serializable]
    public class BellTowerSaveData
    {
        public float[] towerFrequencies;
        public int towersSynced;
        public float resonanceScore;
        public bool miniGameActive;
        public bool cascadeTriggered;
    }
}
