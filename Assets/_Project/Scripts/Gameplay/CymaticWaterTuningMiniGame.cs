using UnityEngine;
using System;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Cymatic Water Tuning Mini-Game (Echohaven Fountain) — Moon 1 vertical slice.
    /// Minimal stub for clean Moon 1 build.
    /// </summary>
    public class CymaticWaterTuningMiniGame : MonoBehaviour
    {
        public static CymaticWaterTuningMiniGame Instance { get; private set; }

        [Header("Config")]
        public float timeLimit = 45f;
        public int difficulty = 1;

        float _bestAccuracy = 0f;
        int _completions = 0;
        bool _goldTierForFountain;
        bool _permanentEffectsActive;
        bool _active;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartMiniGame(float customTime = -1) { _active = true; Debug.Log("[Cymatic] Mini-game started (Moon 1 stub)."); }
        public void EndMiniGame(bool success) { _active = false; if (success) _completions++; }
        public void OnTuningInput(float freq, float amp) { }
        public float GetCurrentAccuracy() => _bestAccuracy;
        public void ForceFullCymaticVisualReapply() { EnsurePermanentCymaticVisuals(); }
        [System.Serializable]
        public class CymaticSaveData
        {
            public float bestCymaticAccuracy;
            public int cymaticCompletions;
            public bool goldTierUnlockedForFountain;
            public bool permanentEffectsActive;
        }
        public CymaticSaveData GetSaveData()
        {
            return new CymaticSaveData
            {
                bestCymaticAccuracy = _bestAccuracy,
                cymaticCompletions = _completions,
                goldTierUnlockedForFountain = _goldTierForFountain,
                permanentEffectsActive = _permanentEffectsActive
            };
        }
        public void LoadSaveData(CymaticSaveData data)
        {
            if (data == null) return;
            _bestAccuracy = data.bestCymaticAccuracy;
            _completions = data.cymaticCompletions;
            _goldTierForFountain = data.goldTierUnlockedForFountain;
            _permanentEffectsActive = data.permanentEffectsActive;
            EnsurePermanentCymaticVisuals();
        }
        public void EnsurePermanentCymaticVisuals() { }
        void PulseFountainCrystals(float strength) { }
        void FinishCymatic() { }
        void UpdateAccuracy() { }
        void UpdateCymaticPattern() { }
        void HandleInput() { }

        void Update()
        {
            if (!_active) return;
        }

        [Serializable]
        public class CymaticConfig
        {
            public float timeLimit = 45f;
            public int difficulty = 1;
            public int patternType = -1;
            public static CymaticConfig Default() => new CymaticConfig();
            public static CymaticConfig Easy() => new CymaticConfig();
            public static CymaticConfig Advanced() => new CymaticConfig();
        }
    }
}
