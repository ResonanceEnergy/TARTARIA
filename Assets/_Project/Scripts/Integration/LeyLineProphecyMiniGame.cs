using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 9 (Solar Moon) mini-game — Ley Line Prophecy.
    /// 12 prophecy stones scattered across the zone, each triggering a temporal echo vision.
    /// The 17-hour Dreamspell clock determines which stones can be activated at which time.
    ///
    /// Cross-ref: docs/03C_MOON_MECHANICS_DETAILED.md §Moon 9, docs/13_MINI_GAMES.md
    /// </summary>
    [DisallowMultipleComponent]
    public class LeyLineProphecyMiniGame : MonoBehaviour
    {
        public static LeyLineProphecyMiniGame Instance { get; private set; }

        // ─── Constants ───────────────────────────────

        public const int TotalProphecyStones = 12;
        public const float DreamspellHours = 17f;
        public const float StoneActivationRadius = 3f;
        public const float VisionDurationSeconds = 8f;
        public const float CooldownBetweenStones = 30f;

        // ─── State ───────────────────────────────────

        readonly bool[] _stonesActivated = new bool[TotalProphecyStones];
        readonly float[] _stoneCooldowns = new float[TotalProphecyStones];
        int _stonesCompleted;
        float _dreamspellClock;          // 0..17 cycling
        bool _miniGameActive;
        bool _visionPlaying;
        float _visionTimer;
        int _currentVisionStone = -1;

        // ─── Events ─────────────────────────────────

        public event Action<int> OnStoneActivated;           // stoneIndex
        public event Action<int, string> OnVisionStarted;    // stoneIndex, visionId
        public event Action OnVisionEnded;
        public event Action OnAllStonesComplete;

        // ─── Prophecy Stone Data ─────────────────────

        [Serializable]
        public class ProphecyStoneDef
        {
            public Vector3 worldPosition;
            public float requiredDreamspellHour;  // 0-16, which hour activates this stone
            public string visionId;
            public string prophecyText;
        }

        [SerializeField] ProphecyStoneDef[] stoneDefinitions;

        // ─── Lifecycle ───────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            if (!_miniGameActive) return;

            AdvanceDreamspellClock();
            UpdateCooldowns();

            if (_visionPlaying)
                UpdateVision();
        }

        // ─── Public API ──────────────────────────────

        public void StartMiniGame()
        {
            _miniGameActive = true;
            _dreamspellClock = 0f;
            Debug.Log("[LeyLineProphecy] Mini-game started. 12 prophecy stones to activate.");
        }

        public void StopMiniGame()
        {
            _miniGameActive = false;
            _visionPlaying = false;
        }

        /// <summary>
        /// Player attempts to activate a prophecy stone.
        /// Only works if the Dreamspell clock is on the correct hour.
        /// </summary>
        public bool TryActivateStone(int stoneIndex)
        {
            if (!_miniGameActive) return false;
            if (stoneIndex < 0 || stoneIndex >= TotalProphecyStones) return false;
            if (_stonesActivated[stoneIndex]) return false;
            if (_stoneCooldowns[stoneIndex] > 0f) return false;
            if (_visionPlaying) return false;

            // Check Dreamspell hour alignment
            if (stoneDefinitions != null && stoneIndex < stoneDefinitions.Length)
            {
                float required = stoneDefinitions[stoneIndex].requiredDreamspellHour;
                float currentHour = Mathf.Floor(_dreamspellClock);
                if (Mathf.Abs(currentHour - required) > 0.5f)
                {
                    Debug.Log($"[LeyLineProphecy] Stone {stoneIndex} requires Dreamspell hour {required}, current: {currentHour}");
                    return false;
                }
            }

            ActivateStone(stoneIndex);
            return true;
        }

        /// <summary>
        /// Get how many stones the player has activated.
        /// </summary>
        public int StonesCompleted => _stonesCompleted;

        /// <summary>
        /// Get the current Dreamspell clock hour (0-16).
        /// </summary>
        public float CurrentDreamspellHour => _dreamspellClock;

        /// <summary>
        /// Check if any vision is currently playing.
        /// </summary>
        public bool IsVisionActive => _visionPlaying;

        /// <summary>
        /// Check if a specific stone has been activated.
        /// </summary>
        public bool IsStoneActivated(int index)
        {
            if (index < 0 || index >= TotalProphecyStones) return false;
            return _stonesActivated[index];
        }

        // ─── Internal ────────────────────────────────

        void ActivateStone(int stoneIndex)
        {
            _stonesActivated[stoneIndex] = true;
            _stonesCompleted++;

            OnStoneActivated?.Invoke(stoneIndex);

            // Start temporal echo vision
            string visionId = stoneDefinitions != null && stoneIndex < stoneDefinitions.Length
                ? stoneDefinitions[stoneIndex].visionId
                : $"vision_{stoneIndex}";

            StartVision(stoneIndex, visionId);

            // Progress quest
            QuestManager.Instance?.ProgressByType(
                QuestObjectiveType.CompleteMiniGame, "prophecy_stone");

            Debug.Log($"[LeyLineProphecy] Stone {stoneIndex} activated ({_stonesCompleted}/{TotalProphecyStones}). Vision: {visionId}");

            if (_stonesCompleted >= TotalProphecyStones)
            {
                OnAllStonesComplete?.Invoke();
                Debug.Log("[LeyLineProphecy] ALL 12 STONES ACTIVATED — Prophecy complete!");
            }
        }

        void StartVision(int stoneIndex, string visionId)
        {
            _visionPlaying = true;
            _visionTimer = VisionDurationSeconds;
            _currentVisionStone = stoneIndex;

            OnVisionStarted?.Invoke(stoneIndex, visionId);

            // Show prophecy text via HUD
            if (stoneDefinitions != null && stoneIndex < stoneDefinitions.Length)
            {
                string text = stoneDefinitions[stoneIndex].prophecyText;
                if (!string.IsNullOrEmpty(text))
                    UI.HUDController.Instance?.ShowInteractionPrompt(text);
            }
        }

        void UpdateVision()
        {
            _visionTimer -= Time.deltaTime;
            if (_visionTimer <= 0f)
            {
                _visionPlaying = false;
                _currentVisionStone = -1;
                UI.HUDController.Instance?.HideInteractionPrompt();
                OnVisionEnded?.Invoke();
            }
        }

        void AdvanceDreamspellClock()
        {
            // 1 Dreamspell hour = 60 real seconds (adjustable)
            _dreamspellClock += Time.deltaTime / 60f;
            if (_dreamspellClock >= DreamspellHours)
                _dreamspellClock -= DreamspellHours;
        }

        void UpdateCooldowns()
        {
            for (int i = 0; i < TotalProphecyStones; i++)
            {
                if (_stoneCooldowns[i] > 0f)
                    _stoneCooldowns[i] -= Time.deltaTime;
            }
        }

        // ─── Save/Load ──────────────────────────────

        public LeyLineSaveData GetSaveData()
        {
            return new LeyLineSaveData
            {
                stonesActivated = (bool[])_stonesActivated.Clone(),
                stonesCompleted = _stonesCompleted,
                dreamspellClock = _dreamspellClock,
                miniGameActive = _miniGameActive
            };
        }

        public void LoadSaveData(LeyLineSaveData data)
        {
            if (data.stonesActivated != null)
            {
                int count = Mathf.Min(data.stonesActivated.Length, TotalProphecyStones);
                for (int i = 0; i < count; i++)
                    _stonesActivated[i] = data.stonesActivated[i];
            }
            _stonesCompleted = data.stonesCompleted;
            _dreamspellClock = data.dreamspellClock;
            _miniGameActive = data.miniGameActive;
        }
    }

    [Serializable]
    public class LeyLineSaveData
    {
        public bool[] stonesActivated;
        public int stonesCompleted;
        public float dreamspellClock;
        public bool miniGameActive;
    }
}
