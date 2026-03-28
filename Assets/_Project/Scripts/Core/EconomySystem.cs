using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Economy System — manages Tartarian resource currencies and building output.
    ///
    /// Design per GDD §19 (Economy Balance):
    ///   - Primary currency: Aether Shards (earned via tuning, combat, quests)
    ///   - Secondary: Resonance Crystals (rare, from zone completions / climaxes)
    ///   - Tertiary: Star Fragments (premium, from Day-Out-of-Time events)
    ///   - Buildings produce passive Aether income when restored
    ///   - RS level multiplies all income
    ///   - MoonModifiers.rsGainMultiplier affects earning rates
    ///
    /// Singleton MonoBehaviour — lives on GameLoop object.
    /// </summary>
    public class EconomySystem : MonoBehaviour
    {
        public static EconomySystem Instance { get; private set; }

        // ─── Events ───
        public event Action<CurrencyType, int, int> OnCurrencyChanged; // type, oldAmt, newAmt
        public event Action<string, int> OnBuildingIncomeCollected;    // buildingId, amount

        // ─── Balances ───
        int _aetherShards;
        int _resonanceCrystals;
        int _starFragments;

        // ─── Building Income ───
        readonly Dictionary<string, BuildingIncome> _buildings = new();
        float _incomeTimer;
        const float INCOME_TICK = 10f; // seconds between passive income ticks

        // ─── RS Multiplier ───
        float _rsMultiplier = 1f;
        float _moonMultiplier = 1f;

        // ─── Getters ───
        public int AetherShards => _aetherShards;
        public int ResonanceCrystals => _resonanceCrystals;
        public int StarFragments => _starFragments;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            _incomeTimer += Time.deltaTime;
            if (_incomeTimer >= INCOME_TICK)
            {
                _incomeTimer -= INCOME_TICK;
                CollectAllBuildingIncome();
            }
        }

        // ─── Currency Operations ───

        public void AddCurrency(CurrencyType type, int amount)
        {
            if (amount <= 0) return;
            int scaled = Mathf.RoundToInt(amount * _rsMultiplier * _moonMultiplier);
            int old;
            switch (type)
            {
                case CurrencyType.AetherShards:
                    old = _aetherShards;
                    _aetherShards += scaled;
                    OnCurrencyChanged?.Invoke(type, old, _aetherShards);
                    break;
                case CurrencyType.ResonanceCrystals:
                    old = _resonanceCrystals;
                    _resonanceCrystals += scaled;
                    OnCurrencyChanged?.Invoke(type, old, _resonanceCrystals);
                    break;
                case CurrencyType.StarFragments:
                    old = _starFragments;
                    _starFragments += scaled; // star fragments unaffected by multiplier typically, but kept for consistency
                    OnCurrencyChanged?.Invoke(type, old, _starFragments);
                    break;
            }
        }

        public bool SpendCurrency(CurrencyType type, int amount)
        {
            if (amount <= 0) return false;
            switch (type)
            {
                case CurrencyType.AetherShards:
                    if (_aetherShards < amount) return false;
                    int oldA = _aetherShards;
                    _aetherShards -= amount;
                    OnCurrencyChanged?.Invoke(type, oldA, _aetherShards);
                    return true;
                case CurrencyType.ResonanceCrystals:
                    if (_resonanceCrystals < amount) return false;
                    int oldR = _resonanceCrystals;
                    _resonanceCrystals -= amount;
                    OnCurrencyChanged?.Invoke(type, oldR, _resonanceCrystals);
                    return true;
                case CurrencyType.StarFragments:
                    if (_starFragments < amount) return false;
                    int oldS = _starFragments;
                    _starFragments -= amount;
                    OnCurrencyChanged?.Invoke(type, oldS, _starFragments);
                    return true;
                default:
                    return false;
            }
        }

        public bool CanAfford(CurrencyType type, int amount)
        {
            return type switch
            {
                CurrencyType.AetherShards => _aetherShards >= amount,
                CurrencyType.ResonanceCrystals => _resonanceCrystals >= amount,
                CurrencyType.StarFragments => _starFragments >= amount,
                _ => false
            };
        }

        public int GetBalance(CurrencyType type)
        {
            return type switch
            {
                CurrencyType.AetherShards => _aetherShards,
                CurrencyType.ResonanceCrystals => _resonanceCrystals,
                CurrencyType.StarFragments => _starFragments,
                _ => 0
            };
        }

        // ─── RS / Moon Multiplier ───

        public void SetRSMultiplier(float rs)
        {
            // RS 0-100 maps to 1.0-2.0× income
            _rsMultiplier = 1f + (Mathf.Clamp01(rs / 100f));
        }

        public void SetMoonMultiplier(float mult)
        {
            _moonMultiplier = Mathf.Max(0.1f, mult);
        }

        // ─── Building Income ───

        public void RegisterBuilding(string buildingId, int baseIncome, CurrencyType outputType = CurrencyType.AetherShards)
        {
            _buildings[buildingId] = new BuildingIncome
            {
                buildingId = buildingId,
                baseIncome = baseIncome,
                outputType = outputType,
                level = 1,
                active = true
            };
        }

        public void UpgradeBuilding(string buildingId)
        {
            if (!_buildings.TryGetValue(buildingId, out var b)) return;
            b.level++;
            _buildings[buildingId] = b;
        }

        public void SetBuildingActive(string buildingId, bool active)
        {
            if (!_buildings.TryGetValue(buildingId, out var b)) return;
            b.active = active;
            _buildings[buildingId] = b;
        }

        void CollectAllBuildingIncome()
        {
            foreach (var kvp in _buildings)
            {
                var b = kvp.Value;
                if (!b.active) continue;

                // Income = base × level × 0.5 (level scaling: diminishing)
                int income = Mathf.RoundToInt(b.baseIncome * (1f + (b.level - 1) * 0.5f));
                AddCurrency(b.outputType, income);
                OnBuildingIncomeCollected?.Invoke(b.buildingId, income);
            }
        }

        // ─── Pricing Table ───

        /// <summary>
        /// Standard costs for common actions.
        /// </summary>
        public static class Prices
        {
            // Building restoration
            public const int RESTORE_BUILDING_TIER1 = 50;
            public const int RESTORE_BUILDING_TIER2 = 150;
            public const int RESTORE_BUILDING_TIER3 = 400;

            // Workshop upgrades
            public const int WORKSHOP_UPGRADE_1 = 100;
            public const int WORKSHOP_UPGRADE_2 = 250;
            public const int WORKSHOP_UPGRADE_3 = 500;

            // Skill unlocks
            public const int SKILL_TIER1 = 75;
            public const int SKILL_TIER2 = 200;
            public const int SKILL_TIER3 = 350;
            public const int SKILL_TIER4 = 500;
            public const int SKILL_TIER5 = 750;

            // Consumables
            public const int REPAIR_KIT = 30;
            public const int AETHER_POTION = 50;
            public const int RS_BOOSTER = 80;
        }

        // ─── Save / Load ───

        public EconomySaveData GetSaveData()
        {
            var buildings = new List<BuildingIncomeSave>();
            foreach (var kvp in _buildings)
            {
                var b = kvp.Value;
                buildings.Add(new BuildingIncomeSave
                {
                    buildingId = b.buildingId,
                    baseIncome = b.baseIncome,
                    outputType = (int)b.outputType,
                    level = b.level,
                    active = b.active
                });
            }

            return new EconomySaveData
            {
                aetherShards = _aetherShards,
                resonanceCrystals = _resonanceCrystals,
                starFragments = _starFragments,
                buildings = buildings
            };
        }

        public void LoadSaveData(EconomySaveData data)
        {
            _aetherShards = data.aetherShards;
            _resonanceCrystals = data.resonanceCrystals;
            _starFragments = data.starFragments;

            _buildings.Clear();
            if (data.buildings != null)
            {
                foreach (var b in data.buildings)
                {
                    _buildings[b.buildingId] = new BuildingIncome
                    {
                        buildingId = b.buildingId,
                        baseIncome = b.baseIncome,
                        outputType = (CurrencyType)b.outputType,
                        level = b.level,
                        active = b.active
                    };
                }
            }
        }

        // ─── Internal Types ───

        struct BuildingIncome
        {
            public string buildingId;
            public int baseIncome;
            public CurrencyType outputType;
            public int level;
            public bool active;
        }
    }

    // ─── Public Types ───

    public enum CurrencyType : byte
    {
        AetherShards = 0,
        ResonanceCrystals = 1,
        StarFragments = 2
    }

    [Serializable]
    public class EconomySaveData
    {
        public int aetherShards;
        public int resonanceCrystals;
        public int starFragments;
        public List<BuildingIncomeSave> buildings;
    }

    [Serializable]
    public class BuildingIncomeSave
    {
        public string buildingId;
        public int baseIncome;
        public int outputType;
        public int level;
        public bool active;
    }
}
