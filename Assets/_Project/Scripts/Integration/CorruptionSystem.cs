using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Corruption System -- manages the 3-stage purge mechanic across zones.
    ///
    /// Corruption spreads from Moon 2 onward as Fractal Wraiths drain Aether
    /// from buildings. Left unchecked, corruption spreads to adjacent buildings
    /// and eventually locks zones into a degraded state.
    ///
    /// 3-Stage Purge Protocol:
    ///   Stage 1: IDENTIFY  -- Use Dissonance Lens to reveal corruption
    ///   Stage 2: ISOLATE   -- Bell Tower purification to contain spread
    ///   Stage 3: PURIFY    -- Micro-Giant Mode or direct Resonance Pulse
    ///
    /// Failed purge: corruption jumps to an adjacent zone building.
    /// Successful purge: RS bonus + Aether burst.
    /// </summary>
    [DisallowMultipleComponent]
    public class CorruptionSystem : MonoBehaviour
    {
        public static CorruptionSystem Instance { get; private set; }

        [Header("Corruption Settings")]
        [SerializeField] float spreadRatePerSecond = 0.5f;
        [SerializeField] float spreadRadius = 30f;
        [SerializeField] float maxCorruption = 100f;
        [SerializeField] float identifyThreshold = 0.1f;
        [SerializeField] float isolateThreshold = 0.5f;

        [Header("Timing")]
        [SerializeField] float spreadCheckInterval = 5f;
        [SerializeField] float corruptionTickInterval = 1f;

        float _spreadCheckTimer;
        float _tickTimer;

        readonly Dictionary<string, CorruptionState> _states = new();

        public event System.Action<string, float> OnCorruptionChanged;
        public event System.Action<string> OnCorruptionPurged;
        public event System.Action<string, string> OnCorruptionSpread;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            _tickTimer += Time.deltaTime;
            if (_tickTimer >= corruptionTickInterval)
            {
                _tickTimer = 0f;
                TickCorruption();
            }

            _spreadCheckTimer += Time.deltaTime;
            if (_spreadCheckTimer >= spreadCheckInterval)
            {
                _spreadCheckTimer = 0f;
                CheckSpread();
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Apply corruption to a building (called by FractalWraith drain).
        /// </summary>
        public void ApplyCorruption(string buildingId, float amount)
        {
            if (!_states.TryGetValue(buildingId, out var state))
            {
                state = new CorruptionState { buildingId = buildingId };
                _states[buildingId] = state;
            }

            state.corruptionLevel = Mathf.Min(maxCorruption,
                state.corruptionLevel + amount);
            state.stage = DetermineStage(state.corruptionLevel);
            _states[buildingId] = state;

            OnCorruptionChanged?.Invoke(buildingId, state.corruptionLevel);
        }

        /// <summary>
        /// Purge corruption from a building. Amount is removed.
        /// If corruption reaches 0, building is fully purged.
        /// </summary>
        public void PurgeCorruption(string buildingId, float amount)
        {
            if (!_states.TryGetValue(buildingId, out var state)) return;

            state.corruptionLevel = Mathf.Max(0f, state.corruptionLevel - amount);
            state.stage = DetermineStage(state.corruptionLevel);

            if (state.corruptionLevel <= 0f)
            {
                state.purged = true;
                _states[buildingId] = state;

                // RS bonus for purge
                GameLoopController.Instance?.QueueRSReward(3f, $"purge_{buildingId}");

                VFXController.Instance?.PlayTuningSuccess(
                    GetBuildingPosition(buildingId), true);
                HapticFeedbackManager.Instance?.PlayPerfectTune();

                Debug.Log($"[Corruption] Building {buildingId} fully purged!");
                OnCorruptionPurged?.Invoke(buildingId);
            }
            else
            {
                _states[buildingId] = state;
            }

            OnCorruptionChanged?.Invoke(buildingId, state.corruptionLevel);
        }

        /// <summary>
        /// Mark a building as identified (Stage 1 complete).
        /// </summary>
        public void MarkIdentified(string buildingId)
        {
            if (!_states.TryGetValue(buildingId, out var state)) return;
            state.identified = true;
            _states[buildingId] = state;
            Debug.Log($"[Corruption] {buildingId} identified via Dissonance Lens");
        }

        /// <summary>
        /// Mark a building as isolated (Stage 2 complete via Bell Tower).
        /// Prevents corruption from spreading to adjacent buildings.
        /// </summary>
        public void MarkIsolated(string buildingId)
        {
            if (!_states.TryGetValue(buildingId, out var state)) return;
            state.isolated = true;
            _states[buildingId] = state;
            Debug.Log($"[Corruption] {buildingId} isolated via Bell Tower shield");
        }

        /// <summary>
        /// Get corruption level for a building (0-100).
        /// </summary>
        public float GetCorruptionLevel(string buildingId)
        {
            return _states.TryGetValue(buildingId, out var state)
                ? state.corruptionLevel : 0f;
        }

        /// <summary>
        /// Get the current purge stage of a building.
        /// </summary>
        public PurgeStage GetPurgeStage(string buildingId)
        {
            return _states.TryGetValue(buildingId, out var state)
                ? state.stage : PurgeStage.Clean;
        }

        /// <summary>
        /// Get all corrupted building states for save persistence.
        /// </summary>
        public List<CorruptionState> GetAllStates()
        {
            var list = new List<CorruptionState>();
            foreach (var kvp in _states)
                list.Add(kvp.Value);
            return list;
        }

        /// <summary>
        /// Restore corruption states from save data.
        /// </summary>
        public void RestoreFromSave(List<CorruptionState> saved)
        {
            _states.Clear();
            foreach (var state in saved)
                _states[state.buildingId] = state;
        }

        // ─── Internal ────────────────────────────────

        void TickCorruption()
        {
            // Skip ticking if corruption is disabled for this moon
            var mods = MoonModifierProvider.Active;
            if (!mods.enableCorruption) return;

            // Active corruption grows slowly on non-isolated buildings
            // Rate is modulated by Moon modifier corruptionSpreadRate
            float rate = spreadRatePerSecond * mods.corruptionSpreadRate;

            var keys = new List<string>(_states.Keys);
            foreach (var key in keys)
            {
                var state = _states[key];
                if (state.purged || state.isolated) continue;
                if (state.corruptionLevel <= 0f) continue;

                state.corruptionLevel = Mathf.Min(maxCorruption,
                    state.corruptionLevel + rate * corruptionTickInterval);
                state.stage = DetermineStage(state.corruptionLevel);
                _states[key] = state;

                OnCorruptionChanged?.Invoke(key, state.corruptionLevel);
            }
        }

        void CheckSpread()
        {
            // Skip spreading if corruption is disabled for this moon
            var mods = MoonModifierProvider.Active;
            if (!mods.enableCorruption) return;

            float rate = spreadRatePerSecond * mods.corruptionSpreadRate;

            var keys = new List<string>(_states.Keys);
            foreach (var key in keys)
            {
                var state = _states[key];
                // Only spread from non-isolated, heavily corrupted buildings
                if (state.isolated || state.purged) continue;
                if (state.corruptionLevel < maxCorruption * 0.75f) continue;

                // Find adjacent buildings within spreadRadius
                Vector3 origin = GetBuildingPosition(key);
                if (origin == Vector3.zero) continue;

                var buildings = FindObjectsByType<InteractableBuilding>(
                    FindObjectsSortMode.None);
                foreach (var b in buildings)
                {
                    if (b.BuildingId == key) continue;
                    float dist = Vector3.Distance(origin, b.transform.position);

                    if (dist <= spreadRadius
                        && GetCorruptionLevel(b.BuildingId) < identifyThreshold * maxCorruption)
                    {
                        ApplyCorruption(b.BuildingId, rate * 2f);
                        Debug.Log($"[Corruption] Spread from {key} to {b.BuildingId}");
                        OnCorruptionSpread?.Invoke(key, b.BuildingId);
                    }
                }
            }
        }

        static PurgeStage DetermineStage(float level)
        {
            if (level <= 0f) return PurgeStage.Clean;
            if (level < 30f) return PurgeStage.Identify;
            if (level < 70f) return PurgeStage.Isolate;
            return PurgeStage.Purify;
        }

        Vector3 GetBuildingPosition(string buildingId)
        {
            var buildings = FindObjectsByType<InteractableBuilding>(
                FindObjectsSortMode.None);
            foreach (var b in buildings)
                if (b.BuildingId == buildingId) return b.transform.position;
            return Vector3.zero;
        }
    }

    // ─── Data Types ──────────────────────────────

    public enum PurgeStage : byte
    {
        Clean    = 0,  // No corruption
        Identify = 1,  // Stage 1: use Dissonance Lens
        Isolate  = 2,  // Stage 2: Bell Tower containment
        Purify   = 3   // Stage 3: direct purge (Micro-Giant or Resonance)
    }

    [System.Serializable]
    public struct CorruptionState
    {
        public string buildingId;
        public float corruptionLevel;
        public PurgeStage stage;
        public bool identified;
        public bool isolated;
        public bool purged;
    }
}
