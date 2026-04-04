using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Workshop System -- manages building upgrades and crafting.
    /// Each restored building can be upgraded through 5 tiers,
    /// improving its Aether output and unlocking cosmetic changes.
    ///
    /// Upgrade tiers:
    ///   Tier 0: Restored (base Aether output)
    ///   Tier 1: Reinforced (+25% output, requires RS 30)
    ///   Tier 2: Harmonized (+50% output, requires RS 50, golden ratio bonus)
    ///   Tier 3: Resonant (+100% output, requires RS 70, visual change)
    ///   Tier 4: Ascended (+200% output, requires RS 90, full golden glow)
    ///   Tier 5: Perfected (max output, requires RS 100 + all buildings Tier 4)
    /// </summary>
    [DisallowMultipleComponent]
    public class WorkshopSystem : MonoBehaviour
    {
        public static WorkshopSystem Instance { get; private set; }

        [Header("Upgrade Requirements")]
        [SerializeField] UpgradeTier[] upgradeTiers;

        readonly Dictionary<string, int> _buildingTiers = new();
        readonly Dictionary<string, InteractableBuilding> _buildingCache = new();

        // Cached ECS references to avoid per-call EntityQuery allocation
        World _ecsWorld;
        EntityManager _em;
        EntityQuery _rsQuery;
        bool _rsQueryCreated;

        public event Action<string, int> OnBuildingUpgraded;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Default tier definitions if none assigned
            if (upgradeTiers == null || upgradeTiers.Length == 0)
                upgradeTiers = CreateDefaultTiers();
        }

        void OnDestroy()
        {
            if (_rsQueryCreated) _rsQuery.Dispose();
            if (Instance == this) Instance = null;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Check if a building can be upgraded to its next tier.
        /// </summary>
        public bool CanUpgrade(string buildingId)
        {
            int currentTier = GetTier(buildingId);
            if (currentTier >= upgradeTiers.Length) return false;

            var building = FindBuilding(buildingId);
            if (building == null || building.State != BuildingRestorationState.Active)
                return false;

            var tier = upgradeTiers[currentTier];
            float currentRS = GetCurrentRS();

            return currentRS >= tier.rsRequirement;
        }

        /// <summary>
        /// Attempt to upgrade a building to its next tier.
        /// Returns true if successful.
        /// </summary>
        public bool TryUpgrade(string buildingId)
        {
            if (!CanUpgrade(buildingId)) return false;

            int currentTier = GetTier(buildingId);
            int newTier = currentTier + 1;
            _buildingTiers[buildingId] = newTier;

            var tier = upgradeTiers[currentTier];

            // Apply benefits
            var building = FindBuilding(buildingId);
            if (building != null)
            {
                ApplyUpgradeEffects(building, newTier, tier);
            }

            OnBuildingUpgraded?.Invoke(buildingId, newTier);

            // Quest integration
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.RestoreBuilding, buildingId);

            HUDController.Instance?.ShowInteractionPrompt(
                $"{building?.Definition?.buildingName ?? buildingId} upgraded to Tier {newTier}!");

            Debug.Log($"[Workshop] {buildingId} upgraded to Tier {newTier}");
            return true;
        }

        /// <summary>
        /// Get the current upgrade tier for a building.
        /// </summary>
        public int GetTier(string buildingId)
        {
            return _buildingTiers.TryGetValue(buildingId, out int tier) ? tier : 0;
        }

        /// <summary>
        /// Get the Aether output multiplier for a building's current tier.
        /// </summary>
        public float GetOutputMultiplier(string buildingId)
        {
            int tier = GetTier(buildingId);
            if (tier <= 0) return 1f;
            if (tier > upgradeTiers.Length) return upgradeTiers[^1].outputMultiplier;
            return upgradeTiers[tier - 1].outputMultiplier;
        }

        /// <summary>
        /// Get upgrade info for display in UI.
        /// </summary>
        public UpgradeTier GetNextTierInfo(string buildingId)
        {
            int currentTier = GetTier(buildingId);
            if (currentTier >= upgradeTiers.Length)
                return default;
            return upgradeTiers[currentTier];
        }

        // ─── Internal ────────────────────────────────

        void ApplyUpgradeEffects(InteractableBuilding building, int newTier, UpgradeTier tierDef)
        {
            // Visual feedback
            VFXController.Instance?.PlayBuildingUpgrade(building.transform.position, newTier);
            Audio.AdaptiveMusicController.Instance?.PlayZoneShift();
            Input.HapticFeedbackManager.Instance?.PlayCombatHit();

            // Scale boost (subtle growth per tier)
            float scaleBoost = 1f + (newTier * 0.02f);
            building.transform.localScale *= scaleBoost / (1f + ((newTier - 1) * 0.02f));
        }

        InteractableBuilding FindBuilding(string buildingId)
        {
            if (_buildingCache.TryGetValue(buildingId, out var cached) && cached != null)
                return cached;

            var buildings = FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            foreach (var b in buildings)
            {
                _buildingCache[b.BuildingId] = b;
                if (b.BuildingId == buildingId) cached = b;
            }
            return cached;
        }

        float GetCurrentRS()
        {
            // Lazy-init cached ECS references
            if (_ecsWorld == null || !_ecsWorld.IsCreated)
            {
                _ecsWorld = World.DefaultGameObjectInjectionWorld;
                if (_ecsWorld == null) return 0f;
                _em = _ecsWorld.EntityManager;
                _rsQuery = _em.CreateEntityQuery(typeof(ResonanceScore));
                _rsQueryCreated = true;
            }
            if (!_rsQueryCreated || _rsQuery.CalculateEntityCount() == 0) return 0f;
            return _em.GetComponentData<ResonanceScore>(_rsQuery.GetSingletonEntity()).CurrentRS;
        }

        static UpgradeTier[] CreateDefaultTiers()
        {
            return new[]
            {
                new UpgradeTier
                {
                    tierName = "Reinforced",
                    rsRequirement = 30f,
                    outputMultiplier = 1.25f,
                    description = "Structural reinforcement -- Aether flow stabilized."
                },
                new UpgradeTier
                {
                    tierName = "Harmonized",
                    rsRequirement = 50f,
                    outputMultiplier = 1.5f,
                    description = "Golden ratio alignments restored -- harmonic resonance detected."
                },
                new UpgradeTier
                {
                    tierName = "Resonant",
                    rsRequirement = 70f,
                    outputMultiplier = 2.0f,
                    description = "Full frequency attunement -- building hums with ancient power."
                },
                new UpgradeTier
                {
                    tierName = "Ascended",
                    rsRequirement = 90f,
                    outputMultiplier = 3.0f,
                    description = "Celestial band connection -- golden auroras emanate from the structure."
                },
                new UpgradeTier
                {
                    tierName = "Perfected",
                    rsRequirement = 100f,
                    outputMultiplier = 4.0f,
                    description = "The ancients' original design -- fully restored to its former glory."
                }
            };
        }

        // ─── Save/Load ──────────────────────────────

        public Dictionary<string, int> GetTiersForSave()
        {
            return new Dictionary<string, int>(_buildingTiers);
        }

        public void RestoreFromSave(Dictionary<string, int> saved)
        {
            if (saved == null) return;
            foreach (var kvp in saved)
                _buildingTiers[kvp.Key] = kvp.Value;
        }
    }

    [Serializable]
    public struct UpgradeTier
    {
        public string tierName;
        public float rsRequirement;
        public float outputMultiplier;
        [TextArea(1, 3)]
        public string description;
    }
}
