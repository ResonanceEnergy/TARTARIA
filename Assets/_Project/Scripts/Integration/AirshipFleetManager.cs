using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 8 (Galactic Moon) — Airship Fleet Manager.
    /// Manages 3 airships: mercury orb tuning, aerial construction,
    /// megalith transport, and fleet combat formations.
    ///
    /// Cross-ref: docs/03C_MOON_MECHANICS_DETAILED.md §Moon 8
    /// </summary>
    [DisallowMultipleComponent]
    public class AirshipFleetManager : MonoBehaviour
    {
        public static AirshipFleetManager Instance { get; private set; }

        // ─── Constants ───────────────────────────────

        public const int MaxAirships = 3;
        public const int MercuryOrbsPerShip = 4;
        public const float LiftCapacityBase = 1000f;         // kg per airship base
        public const float MercuryTuningBonusPct = 0.25f;    // 25% lift per tuned orb
        public const float RepairRatePerSecond = 5f;

        // ─── Enums ───────────────────────────────────

        public enum AirshipState
        {
            Grounded = 0,
            Repairing = 1,
            Idle = 2,
            InFlight = 3,
            InCombat = 4,
            Transporting = 5
        }

        public enum FleetFormation
        {
            Scatter = 0,
            Vanguard = 1,
            Shield = 2,
            Transport = 3
        }

        // ─── Airship Data ────────────────────────────

        [Serializable]
        public class AirshipDef
        {
            public string shipId;
            public string displayName;
            public float maxHealth = 100f;
            public float baseSpeed = 20f;
        }

        [Serializable]
        public class AirshipRuntime
        {
            public string shipId;
            public AirshipState state;
            public float health;
            public float maxHealth;
            public float speed;
            public int mercuryOrbsTuned;
            public float liftCapacity;
            public bool restored;

            public float HealthPct => maxHealth > 0f ? health / maxHealth : 0f;
        }

        [SerializeField] AirshipDef[] shipDefinitions;

        // ─── State ───────────────────────────────────

        readonly AirshipRuntime[] _ships = new AirshipRuntime[MaxAirships];
        FleetFormation _currentFormation = FleetFormation.Scatter;
        int _shipsRestored;
        int _totalMercuryOrbsTuned;
        bool _fleetOperational;

        // ─── Events ─────────────────────────────────

        public event Action<int> OnAirshipRestored;          // shipIndex
        public event Action<int, int> OnMercuryOrbTuned;     // shipIndex, orbCount
        public event Action<FleetFormation> OnFormationChanged;
        public event Action OnFleetOperational;

        // ─── Lifecycle ───────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            for (int i = 0; i < MaxAirships; i++)
            {
                var def = shipDefinitions != null && i < shipDefinitions.Length
                    ? shipDefinitions[i]
                    : null;

                _ships[i] = new AirshipRuntime
                {
                    shipId = def?.shipId ?? $"airship_{i}",
                    state = AirshipState.Grounded,
                    health = 0f,
                    maxHealth = def?.maxHealth ?? 100f,
                    speed = def?.baseSpeed ?? 20f,
                    mercuryOrbsTuned = 0,
                    liftCapacity = 0f,
                    restored = false
                };
            }
        }

        void Update()
        {
            for (int i = 0; i < MaxAirships; i++)
            {
                if (_ships[i].state == AirshipState.Repairing)
                    RepairTick(i);
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Restore a grounded airship (player quest action).
        /// </summary>
        public bool RestoreAirship(int shipIndex)
        {
            if (shipIndex < 0 || shipIndex >= MaxAirships) return false;
            if (_ships[shipIndex].restored) return false;

            _ships[shipIndex].restored = true;
            _ships[shipIndex].state = AirshipState.Repairing;
            _ships[shipIndex].health = _ships[shipIndex].maxHealth * 0.5f;
            _shipsRestored++;

            RecalculateLift(shipIndex);
            OnAirshipRestored?.Invoke(shipIndex);

            QuestManager.Instance?.ProgressByType(
                QuestObjectiveType.RestoreBuilding, _ships[shipIndex].shipId);

            Debug.Log($"[AirshipFleet] Ship {_ships[shipIndex].shipId} restored ({_shipsRestored}/{MaxAirships})");

            if (_shipsRestored >= MaxAirships && !_fleetOperational)
            {
                _fleetOperational = true;
                OnFleetOperational?.Invoke();
                Debug.Log("[AirshipFleet] FLEET OPERATIONAL — all 3 airships restored!");
            }

            return true;
        }

        /// <summary>
        /// Tune a mercury orb on a specific airship — increases lift capacity.
        /// </summary>
        public bool TuneMercuryOrb(int shipIndex)
        {
            if (shipIndex < 0 || shipIndex >= MaxAirships) return false;
            if (!_ships[shipIndex].restored) return false;
            if (_ships[shipIndex].mercuryOrbsTuned >= MercuryOrbsPerShip) return false;

            _ships[shipIndex].mercuryOrbsTuned++;
            _totalMercuryOrbsTuned++;
            RecalculateLift(shipIndex);

            OnMercuryOrbTuned?.Invoke(shipIndex, _ships[shipIndex].mercuryOrbsTuned);

            QuestManager.Instance?.ProgressByType(
                QuestObjectiveType.CompleteTuning, "mercury_orb");

            Debug.Log($"[AirshipFleet] Mercury orb tuned on {_ships[shipIndex].shipId} ({_ships[shipIndex].mercuryOrbsTuned}/{MercuryOrbsPerShip})");
            return true;
        }

        /// <summary>
        /// Set fleet formation — affects combat and transport efficiency.
        /// </summary>
        public void SetFormation(FleetFormation formation)
        {
            if (_currentFormation == formation) return;
            _currentFormation = formation;
            OnFormationChanged?.Invoke(formation);
            Debug.Log($"[AirshipFleet] Formation changed to {formation}");
        }

        /// <summary>
        /// Launch an airship for flight.
        /// </summary>
        public bool LaunchAirship(int shipIndex)
        {
            if (shipIndex < 0 || shipIndex >= MaxAirships) return false;
            if (!_ships[shipIndex].restored) return false;
            if (_ships[shipIndex].state != AirshipState.Idle) return false;
            if (_ships[shipIndex].HealthPct < 0.3f) return false;

            _ships[shipIndex].state = AirshipState.InFlight;
            return true;
        }

        /// <summary>
        /// Land an airship.
        /// </summary>
        public bool LandAirship(int shipIndex)
        {
            if (shipIndex < 0 || shipIndex >= MaxAirships) return false;
            var state = _ships[shipIndex].state;
            if (state != AirshipState.InFlight && state != AirshipState.Transporting) return false;

            _ships[shipIndex].state = AirshipState.Idle;
            return true;
        }

        /// <summary>
        /// Get runtime state of an airship.
        /// </summary>
        public AirshipRuntime GetShipState(int shipIndex)
        {
            if (shipIndex < 0 || shipIndex >= MaxAirships) return null;
            return _ships[shipIndex];
        }

        public int ShipsRestored => _shipsRestored;
        public int TotalMercuryOrbsTuned => _totalMercuryOrbsTuned;
        public bool IsFleetOperational => _fleetOperational;
        public FleetFormation CurrentFormation => _currentFormation;

        // ─── Internal ────────────────────────────────

        void RepairTick(int shipIndex)
        {
            var ship = _ships[shipIndex];
            ship.health = Mathf.Min(ship.health + RepairRatePerSecond * Time.deltaTime, ship.maxHealth);
            if (ship.health >= ship.maxHealth)
                ship.state = AirshipState.Idle;
        }

        void RecalculateLift(int shipIndex)
        {
            float bonus = 1f + (_ships[shipIndex].mercuryOrbsTuned * MercuryTuningBonusPct);
            _ships[shipIndex].liftCapacity = LiftCapacityBase * bonus;
        }

        // ─── Save/Load ──────────────────────────────

        public AirshipFleetSaveData GetSaveData()
        {
            var shipStates = new AirshipShipSave[MaxAirships];
            for (int i = 0; i < MaxAirships; i++)
            {
                shipStates[i] = new AirshipShipSave
                {
                    shipId = _ships[i].shipId,
                    state = (int)_ships[i].state,
                    health = _ships[i].health,
                    mercuryOrbsTuned = _ships[i].mercuryOrbsTuned,
                    restored = _ships[i].restored
                };
            }
            return new AirshipFleetSaveData
            {
                ships = shipStates,
                formation = (int)_currentFormation,
                shipsRestored = _shipsRestored,
                totalMercuryOrbsTuned = _totalMercuryOrbsTuned,
                fleetOperational = _fleetOperational
            };
        }

        public void LoadSaveData(AirshipFleetSaveData data)
        {
            if (data.ships != null)
            {
                int count = Mathf.Min(data.ships.Length, MaxAirships);
                for (int i = 0; i < count; i++)
                {
                    _ships[i].shipId = data.ships[i].shipId;
                    _ships[i].state = (AirshipState)data.ships[i].state;
                    _ships[i].health = data.ships[i].health;
                    _ships[i].mercuryOrbsTuned = data.ships[i].mercuryOrbsTuned;
                    _ships[i].restored = data.ships[i].restored;
                    RecalculateLift(i);
                }
            }
            _currentFormation = (FleetFormation)data.formation;
            _shipsRestored = data.shipsRestored;
            _totalMercuryOrbsTuned = data.totalMercuryOrbsTuned;
            _fleetOperational = data.fleetOperational;
        }
    }

    [Serializable]
    public class AirshipShipSave
    {
        public string shipId;
        public int state;
        public float health;
        public int mercuryOrbsTuned;
        public bool restored;
    }

    [Serializable]
    public class AirshipFleetSaveData
    {
        public AirshipShipSave[] ships;
        public int formation;
        public int shipsRestored;
        public int totalMercuryOrbsTuned;
        public bool fleetOperational;
    }
}
