using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Companion Manager — tracks and coordinates all named companions.
    ///
    /// Companions (from GDD):
    ///   - Milo: Aether-sensitive fox (DOTS AI via CompanionBehaviorSystem)
    ///   - Lirael: Crystal singer (Moon 2+, healer/support)
    ///   - Thorne: Forge smith (Moon 4+, combat/crafting)
    ///   - Korath: Star reader (Moon 8+, navigation/lore)
    ///
    /// Each companion has:
    ///   - An unlock moon (they join the party at that zone)
    ///   - A trust level (0-100, increased by quests and gifts)
    ///   - A passive buff that scales with trust
    ///   - Unique dialogue contexts
    /// </summary>
    [DisallowMultipleComponent]
    public class CompanionManager : MonoBehaviour
    {
        public static CompanionManager Instance { get; private set; }

        [Header("Companion Data")]
        [SerializeField] CompanionData[] companions;

        readonly System.Collections.Generic.Dictionary<string, CompanionState> _states = new();
        static readonly float[] _trustMilestones = { 25f, 50f, 75f, 100f };

        public event System.Action<string, float> OnTrustChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (companions == null || companions.Length == 0)
                companions = CreateDefaultCompanions();

            foreach (var c in companions)
                _states[c.companionId] = new CompanionState { unlocked = false, trustLevel = 0f };
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Unlock a companion (called when player reaches their unlock moon).
        /// </summary>
        public void UnlockCompanion(string companionId)
        {
            if (!_states.TryGetValue(companionId, out var state)) return;
            if (state.unlocked) return;

            state.unlocked = true;
            _states[companionId] = state;

            DialogueManager.Instance?.PlayContextDialogue($"companion_join_{companionId}");
            Debug.Log($"[CompanionManager] {companionId} has joined the party!");
        }

        /// <summary>
        /// Add trust to a companion (from quests, gifts, exploration).
        /// </summary>
        public void AddTrust(string companionId, float amount)
        {
            if (!_states.TryGetValue(companionId, out var state)) return;
            if (!state.unlocked) return;

            float oldTrust = state.trustLevel;
            state.trustLevel = Mathf.Clamp(state.trustLevel + amount, 0f, 100f);
            _states[companionId] = state;
            OnTrustChanged?.Invoke(companionId, state.trustLevel);

            QuestManager.Instance?.ProgressByType(QuestObjectiveType.RaiseCompanionTrust, companionId);

            // Check milestone boundaries (25/50/75/100)
            foreach (float milestone in _trustMilestones)
            {
                if (oldTrust < milestone && state.trustLevel >= milestone)
                    QuestManager.Instance?.ProgressByType(QuestObjectiveType.CompanionMilestone, companionId);
            }
        }

        /// <summary>
        /// Get a companion's passive buff multiplier based on trust.
        /// </summary>
        public float GetBuffMultiplier(string companionId)
        {
            if (!_states.TryGetValue(companionId, out var state)) return 1f;
            if (!state.unlocked) return 1f;

            // Linear 1.0 → 1.5 over 0-100 trust
            return 1f + (state.trustLevel / 100f) * 0.5f;
        }

        public bool IsUnlocked(string companionId)
        {
            return _states.TryGetValue(companionId, out var state) && state.unlocked;
        }

        public float GetTrust(string companionId)
        {
            return _states.TryGetValue(companionId, out var state) ? state.trustLevel : 0f;
        }

        /// <summary>
        /// Check moon progression and auto-unlock companions.
        /// Called by GameLoopController when zone changes.
        /// </summary>
        public void CheckUnlocks(int currentMoon)
        {
            foreach (var c in companions)
            {
                if (currentMoon >= c.unlockMoon && !IsUnlocked(c.companionId))
                    UnlockCompanion(c.companionId);
            }
        }

        static CompanionData[] CreateDefaultCompanions()
        {
            return new[]
            {
                new CompanionData
                {
                    companionId = "milo",
                    displayName = "Milo",
                    description = "Aether-sensitive fox. Senses buried structures and hidden frequencies.",
                    unlockMoon = 1,
                    passiveBuffType = CompanionBuffType.DiscoveryRange,
                    passiveDescription = "Increases building discovery detection range."
                },
                new CompanionData
                {
                    companionId = "lirael",
                    displayName = "Lirael",
                    description = "Crystal singer from the Crystalline Caverns. Heals and amplifies tuning accuracy.",
                    unlockMoon = 2,
                    passiveBuffType = CompanionBuffType.TuningAccuracy,
                    passiveDescription = "Increases tuning mini-game accuracy window."
                },
                new CompanionData
                {
                    companionId = "thorne",
                    displayName = "Thorne",
                    description = "Forge smith from Star Fort Bastion. Strengthens combat and workshop upgrades.",
                    unlockMoon = 4,
                    passiveBuffType = CompanionBuffType.CombatDamage,
                    passiveDescription = "Increases Resonance Pulse and Harmonic Strike damage."
                },
                new CompanionData
                {
                    companionId = "korath",
                    displayName = "Korath",
                    description = "Star reader from the Verdant Canopy. Reveals hidden lore and golden mote locations.",
                    unlockMoon = 8,
                    passiveBuffType = CompanionBuffType.MoteDetection,
                    passiveDescription = "Reveals golden mote locations on the minimap."
                }
            };
        }

        // ─── Save / Load ─────────────────────────────

        public CompanionManagerSavePayload GetSaveData()
        {
            var ids = new System.Collections.Generic.List<string>();
            var unlocked = new System.Collections.Generic.List<bool>();
            var trust = new System.Collections.Generic.List<float>();
            foreach (var kvp in _states)
            {
                ids.Add(kvp.Key);
                unlocked.Add(kvp.Value.unlocked);
                trust.Add(kvp.Value.trustLevel);
            }
            return new CompanionManagerSavePayload
            {
                companionIds = ids.ToArray(),
                companionUnlocked = unlocked.ToArray(),
                companionTrust = trust.ToArray()
            };
        }

        public void LoadSaveData(CompanionManagerSavePayload data)
        {
            if (data == null || data.companionIds == null) return;
            _states.Clear();
            for (int i = 0; i < data.companionIds.Length; i++)
            {
                _states[data.companionIds[i]] = new CompanionState
                {
                    unlocked = data.companionUnlocked != null && i < data.companionUnlocked.Length && data.companionUnlocked[i],
                    trustLevel = data.companionTrust != null && i < data.companionTrust.Length ? data.companionTrust[i] : 0f
                };
            }
        }

        public class CompanionManagerSavePayload
        {
            public string[] companionIds;
            public bool[] companionUnlocked;
            public float[] companionTrust;
        }
    }

    [System.Serializable]
    public struct CompanionData
    {
        public string companionId;
        public string displayName;
        [TextArea(1, 3)]
        public string description;
        public int unlockMoon;
        public CompanionBuffType passiveBuffType;
        public string passiveDescription;
    }

    public enum CompanionBuffType : byte
    {
        DiscoveryRange = 0,
        TuningAccuracy = 1,
        CombatDamage = 2,
        MoteDetection = 3,
    }

    struct CompanionState
    {
        public bool unlocked;
        public float trustLevel;
    }
}
