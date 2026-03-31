using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10 (Planetary Moon) — Continental Rail System.
    /// 12 stations connected by a megalithic rail network that must be restored.
    /// Uses A* pathfinding on a weighted graph; rail segments must be repaired
    /// before trains can traverse them.
    ///
    /// Mechanics:
    ///   - 12 stations (one per zone, plus hub station in Aurora City)
    ///   - Rail segments connect stations (directed graph)
    ///   - Each segment has a repair cost (aether) and corruption level
    ///   - Restored segments allow fast travel between zones
    ///   - Full network completion triggers Continental Aurora VFX
    ///   - Rail Wraith enemies patrol corrupted segments
    ///   - Rail Leviathan boss guards the final hub connection
    ///
    /// Cross-ref: docs/03C_MOON_MECHANICS_DETAILED.md §Moon 10, docs/13_MINI_GAMES.md
    /// </summary>
    [DisallowMultipleComponent]
    public class ContinentalRailSystem : MonoBehaviour
    {
        public static ContinentalRailSystem Instance { get; private set; }

        // ─── Constants ───────────────────────────────

        public const int TotalStations = 12;
        public const int MaxSegments = 24;
        public const float RepairTimePerSegment = 15f;     // seconds
        public const int BaseRepairCostAether = 50;
        public const float TrainSpeedUnitsPerSecond = 30f;
        public const float RSRewardPerSegment = 3f;
        public const float RSRewardNetworkComplete = 15f;

        // ─── Station & Segment Definitions ───────────

        [Serializable]
        public class StationDef
        {
            public string stationId;
            public string displayName;
            public int zoneIndex;
            public Vector3 worldPosition;
            public bool isHub;
        }

        [Serializable]
        public class RailSegment
        {
            public int fromStation;
            public int toStation;
            public float distance;         // world units
            public float corruptionLevel;  // 0=clear, 1=fully corrupted
            public int repairCost;
            public bool restored;
            public bool hasBoss;           // Rail Leviathan guards this segment
        }

        [SerializeField] StationDef[] stationDefs;

        // ─── State ───────────────────────────────────

        readonly List<RailSegment> _segments = new();
        readonly bool[] _stationsDiscovered = new bool[TotalStations];
        int _segmentsRestored;
        bool _networkComplete;
        bool _trainActive;
        int _trainCurrentStation = -1;
        int _trainTargetStation = -1;
        readonly List<int> _trainPath = new();
        float _trainProgress;

        // ─── Events ─────────────────────────────────

        public event Action<int, int> OnSegmentRestored;     // fromStation, toStation
        public event Action<int> OnStationDiscovered;        // stationIndex
        public event Action OnNetworkComplete;
        public event Action<int, int> OnTrainDeparted;       // from, to
        public event Action<int> OnTrainArrived;             // stationIndex
        public event Action OnRailLeviathan;

        // ─── Lifecycle ───────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildDefaultNetwork();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (_trainActive)
                UpdateTrainMovement();
        }

        // ─── Network Construction ────────────────────

        void BuildDefaultNetwork()
        {
            // Build a ring of 12 stations + hub spokes
            _segments.Clear();

            // Ring connections (each station to next)
            for (int i = 0; i < TotalStations; i++)
            {
                int next = (i + 1) % TotalStations;
                float dist = stationDefs != null && i < stationDefs.Length && next < stationDefs.Length
                    ? Vector3.Distance(stationDefs[i].worldPosition, stationDefs[next].worldPosition)
                    : 100f + i * 10f;

                _segments.Add(new RailSegment
                {
                    fromStation = i,
                    toStation = next,
                    distance = dist,
                    corruptionLevel = 0.5f + 0.05f * i,
                    repairCost = BaseRepairCostAether + i * 10,
                    restored = false,
                    hasBoss = i == 11   // Last segment has Rail Leviathan
                });
            }

            // Hub spokes (station 0 = Aurora City hub, connect to stations 3, 6, 9)
            int[] hubTargets = { 3, 6, 9 };
            foreach (int t in hubTargets)
            {
                float dist = stationDefs != null && t < stationDefs.Length
                    ? Vector3.Distance(stationDefs[0].worldPosition, stationDefs[t].worldPosition)
                    : 200f;

                _segments.Add(new RailSegment
                {
                    fromStation = 0,
                    toStation = t,
                    distance = dist,
                    corruptionLevel = 0.7f,
                    repairCost = BaseRepairCostAether * 2,
                    restored = false,
                    hasBoss = false
                });
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Discover a station (called when player enters a zone).
        /// </summary>
        public void DiscoverStation(int stationIndex)
        {
            if (stationIndex < 0 || stationIndex >= TotalStations) return;
            if (_stationsDiscovered[stationIndex]) return;

            _stationsDiscovered[stationIndex] = true;
            OnStationDiscovered?.Invoke(stationIndex);
            Debug.Log($"[ContinentalRail] Station discovered: {stationIndex}");
        }

        /// <summary>
        /// Attempt to repair a rail segment. Returns true on success.
        /// </summary>
        public bool TryRepairSegment(int fromStation, int toStation)
        {
            var seg = FindSegment(fromStation, toStation);
            if (seg == null || seg.restored) return false;

            // Boss check first — don't spend currency if boss blocks repair
            if (seg.hasBoss)
            {
                OnRailLeviathan?.Invoke();
                BossEncounterSystem.Instance?.SpawnBoss("rail_leviathan");
                Debug.Log("[ContinentalRail] Rail Leviathan guards this segment!");
                return false; // Boss must be defeated first
            }

            // Check aether cost
            var econ = Core.EconomySystem.Instance;
            if (econ == null) return false;
            if (econ.GetBalance(Core.CurrencyType.AetherShards) < seg.repairCost) return false;
            econ.SpendCurrency(Core.CurrencyType.AetherShards, seg.repairCost);

            seg.restored = true;
            seg.corruptionLevel = 0f;
            _segmentsRestored++;

            GameLoopController.Instance?.QueueRSReward(RSRewardPerSegment, "rail_segment");
            OnSegmentRestored?.Invoke(fromStation, toStation);

            Debug.Log($"[ContinentalRail] Segment restored: {fromStation} → {toStation}");

            CheckNetworkComplete();
            SaveManager.Instance?.MarkDirty();
            return true;
        }

        /// <summary>
        /// Mark a boss segment as cleared (called after Rail Leviathan defeated).
        /// </summary>
        public void ClearBossSegment(int fromStation, int toStation)
        {
            var seg = FindSegment(fromStation, toStation);
            if (seg != null)
            {
                seg.hasBoss = false;
                seg.restored = true;
                seg.corruptionLevel = 0f;
                _segmentsRestored++;
                CheckNetworkComplete();
            }
        }

        /// <summary>
        /// Begin train travel from one station to another using A* path.
        /// </summary>
        public bool TravelToStation(int fromStation, int toStation)
        {
            if (_trainActive) return false;
            if (!_stationsDiscovered[fromStation] || !_stationsDiscovered[toStation]) return false;

            var path = FindPath(fromStation, toStation);
            if (path == null || path.Count < 2) return false;

            _trainPath.Clear();
            _trainPath.AddRange(path);
            _trainCurrentStation = fromStation;
            _trainTargetStation = toStation;
            _trainProgress = 0f;
            _trainActive = true;

            OnTrainDeparted?.Invoke(fromStation, toStation);
            Debug.Log($"[ContinentalRail] Train departed: {fromStation} → {toStation} ({path.Count} stops)");
            return true;
        }

        // ─── A* Pathfinding ─────────────────────────

        List<int> FindPath(int from, int to)
        {
            var open = new SortedList<float, int>();
            var cameFrom = new Dictionary<int, int>();
            var gScore = new Dictionary<int, float>();
            var closed = new HashSet<int>();

            gScore[from] = 0;
            open.Add(0, from);

            while (open.Count > 0)
            {
                int current = open.Values[0];
                open.RemoveAt(0);

                if (current == to)
                    return ReconstructPath(cameFrom, current);

                closed.Add(current);

                foreach (var seg in _segments)
                {
                    if (!seg.restored) continue;

                    int neighbor = -1;
                    if (seg.fromStation == current) neighbor = seg.toStation;
                    else if (seg.toStation == current) neighbor = seg.fromStation;
                    if (neighbor < 0 || closed.Contains(neighbor)) continue;

                    float tentG = gScore[current] + seg.distance;
                    if (!gScore.ContainsKey(neighbor) || tentG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentG;
                        float fScore = tentG + EstimateDistance(neighbor, to);
                        // Avoid duplicate keys in SortedList
                        while (open.ContainsKey(fScore)) fScore += 0.001f;
                        open.Add(fScore, neighbor);
                    }
                }
            }

            return null; // No path found
        }

        float EstimateDistance(int stationA, int stationB)
        {
            if (stationDefs != null && stationA < stationDefs.Length && stationB < stationDefs.Length)
                return Vector3.Distance(stationDefs[stationA].worldPosition, stationDefs[stationB].worldPosition);
            return Mathf.Abs(stationA - stationB) * 100f;
        }

        List<int> ReconstructPath(Dictionary<int, int> cameFrom, int current)
        {
            var path = new List<int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        // ─── Train Movement ──────────────────────────

        void UpdateTrainMovement()
        {
            if (_trainPath.Count < 2)
            {
                _trainActive = false;
                return;
            }

            int fromIdx = Mathf.FloorToInt(_trainProgress);
            if (fromIdx >= _trainPath.Count - 1)
            {
                // Arrived
                _trainActive = false;
                _trainCurrentStation = _trainTargetStation;
                OnTrainArrived?.Invoke(_trainCurrentStation);

                // Zone transition
                ZoneTransitionSystem.Instance?.TransitionToZone(
                    stationDefs != null && _trainCurrentStation < stationDefs.Length
                        ? stationDefs[_trainCurrentStation].zoneIndex
                        : _trainCurrentStation);

                Debug.Log($"[ContinentalRail] Train arrived at station {_trainCurrentStation}");
                return;
            }

            var seg = FindSegment(_trainPath[fromIdx], _trainPath[fromIdx + 1]);
            float segDist = seg?.distance ?? 100f;
            float speed = TrainSpeedUnitsPerSecond / segDist;

            _trainProgress += speed * Time.deltaTime;
        }

        // ─── Helpers ─────────────────────────────────

        RailSegment FindSegment(int from, int to)
        {
            foreach (var seg in _segments)
            {
                if ((seg.fromStation == from && seg.toStation == to) ||
                    (seg.fromStation == to && seg.toStation == from))
                    return seg;
            }
            return null;
        }

        void CheckNetworkComplete()
        {
            if (_networkComplete) return;

            foreach (var seg in _segments)
            {
                if (!seg.restored) return;
            }

            _networkComplete = true;
            GameLoopController.Instance?.QueueRSReward(RSRewardNetworkComplete, "rail_network_complete");
            VFXController.Instance?.SpawnContinentalTrainAurora(transform.position);
            AchievementSystem.Instance?.Unlock("E06");
            OnNetworkComplete?.Invoke();

            Debug.Log("[ContinentalRail] Full network restored! Continental Aurora triggered.");
        }

        // ─── Queries ─────────────────────────────────

        public bool IsNetworkComplete => _networkComplete;
        public int SegmentsRestored => _segmentsRestored;
        public int TotalSegments => _segments.Count;
        public bool IsTrainActive => _trainActive;
        public bool IsStationDiscovered(int idx) =>
            idx >= 0 && idx < TotalStations && _stationsDiscovered[idx];

        // ─── Save / Load ─────────────────────────────

        public RailSavePayload GetSaveData()
        {
            var payload = new RailSavePayload
            {
                segmentRestored = new bool[_segments.Count],
                segmentHasBoss = new bool[_segments.Count],
                segmentCorruption = new float[_segments.Count],
                stationsDiscovered = (bool[])_stationsDiscovered.Clone(),
                segmentsRestored = _segmentsRestored,
                networkComplete = _networkComplete,
                trainActive = _trainActive,
                trainCurrentStation = _trainCurrentStation
            };
            for (int i = 0; i < _segments.Count; i++)
            {
                payload.segmentRestored[i] = _segments[i].restored;
                payload.segmentHasBoss[i] = _segments[i].hasBoss;
                payload.segmentCorruption[i] = _segments[i].corruptionLevel;
            }
            return payload;
        }

        public void LoadSaveData(RailSavePayload data)
        {
            if (data == null) return;
            int segCount = Mathf.Min(_segments.Count, data.segmentRestored?.Length ?? 0);
            for (int i = 0; i < segCount; i++)
            {
                _segments[i].restored = data.segmentRestored[i];
                _segments[i].hasBoss = data.segmentHasBoss != null && i < data.segmentHasBoss.Length && data.segmentHasBoss[i];
                _segments[i].corruptionLevel = data.segmentCorruption != null && i < data.segmentCorruption.Length ? data.segmentCorruption[i] : 0f;
            }
            if (data.stationsDiscovered != null)
            {
                int stationCount = Mathf.Min(TotalStations, data.stationsDiscovered.Length);
                for (int i = 0; i < stationCount; i++)
                    _stationsDiscovered[i] = data.stationsDiscovered[i];
            }
            _segmentsRestored = data.segmentsRestored;
            _networkComplete = data.networkComplete;
            _trainActive = data.trainActive;
            _trainCurrentStation = data.trainCurrentStation;
        }

        public class RailSavePayload
        {
            public bool[] segmentRestored;
            public bool[] segmentHasBoss;
            public float[] segmentCorruption;
            public bool[] stationsDiscovered;
            public int segmentsRestored;
            public bool networkComplete;
            public bool trainActive;
            public int trainCurrentStation;
        }
    }
}
