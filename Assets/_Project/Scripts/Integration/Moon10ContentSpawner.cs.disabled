using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Save;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10: PLANETARY MOON — "The Manifestation of Producing"
    /// Continental train network restoration + mega-station construction.
    /// Adopted children (from Moon 3) become junior engineers. 80% grid completion.
    /// Discovery: resonance rail network reactivates, Mud Flood trigger room found.
    /// </summary>
    public class Moon10ContentSpawner : MonoBehaviour
    {
        [Header("Moon 10 State")]
        [SerializeField] bool moon10Unlocked;
        [SerializeField] bool railNetworkComplete;
        [SerializeField] bool orphanPuzzleSolved;
        [SerializeField] bool railLeviathanDefeated;

        [Header("Rail Network")]
        [SerializeField] int totalRailSegments = 12;  // 12 continental segments
        [SerializeField] int totalStations = 6;  // 6 mega-stations across zones
        int _segmentsLaid;
        int _stationsBuilt;

        [Header("Spawning")]
        [SerializeField] Vector3 centralStationPoint = new(0f, 2f, 0f);  // Main hub
        [SerializeField] Vector3[] railNodePoints;  // 12 segment waypoints
        [SerializeField] Vector3[] stationPoints;  // 6 mega-station locations
        [SerializeField] GameObject trainPrefab;  // Resonance train engine
        [SerializeField] GameObject railSegmentPrefab;  // Rail track visual

        [Header("Boss")]
        [SerializeField] Vector3 leviathanSpawnPoint = new(300f, 5f, 400f);

        readonly List<GameObject> _railSegments = new();
        readonly List<GameObject> _stations = new();
        readonly List<GameObject> _trains = new();
        readonly List<RailPathNode> _pathNodes = new();
        GameObject _triggerRoom;
        GameObject _orphanTrainPuzzle;
        bool _contentSpawned;
        bool _triggerRoomDiscovered;

        public bool IsMoon10Active => moon10Unlocked && !railNetworkComplete;
        public int RailProgress => _segmentsLaid;
        public float CompletionPercent => (_segmentsLaid + _stationsBuilt) / (float)(totalRailSegments + totalStations);

        void Awake()
        {
            // Check save state
            moon10Unlocked = SaveManager.Instance?.GetMoonProgress(10) > 0f;

            // Wire save/load events
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave += OnSave;
                SaveManager.Instance.OnAfterLoad += OnLoad;
            }
        }

        void OnDestroy()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave -= OnSave;
                SaveManager.Instance.OnAfterLoad -= OnLoad;
            }
        }

        void OnSave(SaveData sd)
        {
            // P0 CRITICAL: Rail network blocks Moon 12
            sd.SetMoonFlag(10, "segmentsLaid", _segmentsLaid);
            sd.SetMoonFlag(10, "stationsBuilt", _stationsBuilt);
            sd.SetMoonFlag(10, "railNetworkComplete", railNetworkComplete);
            sd.SetMoonFlag(10, "orphanPuzzleSolved", orphanPuzzleSolved);
            sd.SetMoonFlag(10, "railLeviathanDefeated", railLeviathanDefeated);
        }

        void OnLoad(SaveData sd)
        {
            // Restore Moon 10 rail network state
            _segmentsLaid = sd.GetMoonFlag(10, "segmentsLaid", 0);
            _stationsBuilt = sd.GetMoonFlag(10, "stationsBuilt", 0);
            railNetworkComplete = sd.GetMoonFlag(10, "railNetworkComplete");
            orphanPuzzleSolved = sd.GetMoonFlag(10, "orphanPuzzleSolved");
            railLeviathanDefeated = sd.GetMoonFlag(10, "railLeviathanDefeated");

            Debug.Log($"[Moon 10] State loaded: rail={_segmentsLaid}/{totalRailSegments}, stations={_stationsBuilt}/{totalStations}, complete={railNetworkComplete}");
        }

        void Start()
        {
            if (moon10Unlocked && !_contentSpawned)
            {
                SpawnMoon10Content();
            }
        }

        public void UnlockMoon10()
        {
            if (moon10Unlocked) return;

            moon10Unlocked = true;
            SaveManager.Instance?.SetMoonProgress(10, 5f);
            Debug.Log("[Moon 10] PLANETARY MOON unlocked — Continental Rail Network awakens");

            SpawnMoon10Content();
        }

        void SpawnMoon10Content()
        {
            _contentSpawned = true;

            Debug.Log("[Moon 10] Spawning Continental Rail Corridor content");

            // Central mega-station (main hub)
            SpawnCentralStation();

            // 6 mega-stations across zones
            SpawnMegaStations();

            // Discovery: Mud Flood trigger room (hidden in central station basement)
            SpawnTriggerRoom();

            // Initial rail segments (partial network)
            SpawnInitialRailSegments();

            // Initialize A* pathfinding graph
            InitializeRailPathfinding();

            // Orphan train puzzle (from Moon 3 children)
            SpawnOrphanTrainPuzzle();

            // Ambient audio
            var railAmbience = Audio.AudioManager.Instance?.PlayLoopingSFX("RailNetworkHum", centralStationPoint, 0.3f);
            if (railAmbience != null)
            {
                Debug.Log("[Moon 10] Rail network ambient hum active at 432 Hz");
            }

            // Quest activation
            QuestManager.Instance?.ActivateQuest("moon10_rail_network_discovery");
            QuestManager.Instance?.ActivateQuest("moon10_restore_12_segments");

            // Dialogue: children NPCs (from Moon 3) now junior engineers
            GameEvents.RaiseHUDShowObjective("The rails sing again. Connect the continent.");
        }

        void SpawnCentralStation()
        {
            var station = new GameObject("CentralStation_Moon10");
            station.transform.position = centralStationPoint;

            // Main hall - large citadel structure
            GameObject mainHallPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Citadel_MainHall");
            GameObject mainHall;
            if (mainHallPrefab != null)
            {
                mainHall = Instantiate(mainHallPrefab, Vector3.zero, Quaternion.identity);
                mainHall.name = "MainHall";
                mainHall.transform.SetParent(station.transform);
                mainHall.transform.localPosition = Vector3.up * 4f;
                mainHall.transform.localScale = new Vector3(25f, 8f, 25f);
            }
            else
            {
                Debug.LogError("[Moon10] Structure_Citadel_MainHall prefab missing - using fallback");
                mainHall = new GameObject("MainHall_FALLBACK");
                mainHall.transform.SetParent(station.transform);
                mainHall.transform.localScale = new Vector3(25f, 8f, 25f);
                mainHall.transform.localPosition = Vector3.up * 4f;
                
                var mf = mainHall.AddComponent<MeshFilter>();
                var mr = mainHall.AddComponent<MeshRenderer>();
                var col = mainHall.AddComponent<BoxCollider>();
                col.size = Vector3.one;
                
                Material mat = Resources.Load<Material>("Materials/Structure");
                if (mat != null) mr.material = mat;
            }

            // East wing - side structure
            GameObject eastWingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Wing");
            GameObject eastWing;
            if (eastWingPrefab != null)
            {
                eastWing = Instantiate(eastWingPrefab, Vector3.zero, Quaternion.identity);
                eastWing.name = "EastWing";
                eastWing.transform.SetParent(station.transform);
                eastWing.transform.localPosition = new Vector3(17f, 3f, 0f);
                eastWing.transform.localScale = new Vector3(10f, 6f, 15f);
            }
            else
            {
                Debug.LogError("[Moon10] Structure_Wing prefab missing - using fallback");
                eastWing = new GameObject("EastWing_FALLBACK");
                eastWing.transform.SetParent(station.transform);
                eastWing.transform.localScale = new Vector3(10f, 6f, 15f);
                eastWing.transform.localPosition = new Vector3(17f, 3f, 0f);
                
                var mf = eastWing.AddComponent<MeshFilter>();
                var mr = eastWing.AddComponent<MeshRenderer>();
                var col = eastWing.AddComponent<BoxCollider>();
                col.size = Vector3.one;
                
                Material mat = Resources.Load<Material>("Materials/Structure");
                if (mat != null) mr.material = mat;
            }

            // West wing - mirrored side structure
            GameObject westWingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Wing");
            GameObject westWing;
            if (westWingPrefab != null)
            {
                westWing = Instantiate(westWingPrefab, Vector3.zero, Quaternion.identity);
                westWing.name = "WestWing";
                westWing.transform.SetParent(station.transform);
                westWing.transform.localPosition = new Vector3(-17f, 3f, 0f);
                westWing.transform.localScale = new Vector3(10f, 6f, 15f);
            }
            else
            {
                Debug.LogError("[Moon10] Structure_Wing prefab missing - using fallback");
                westWing = new GameObject("WestWing_FALLBACK");
                westWing.transform.SetParent(station.transform);
                westWing.transform.localScale = new Vector3(10f, 6f, 15f);
                westWing.transform.localPosition = new Vector3(-17f, 3f, 0f);
                
                var mf = westWing.AddComponent<MeshFilter>();
                var mr = westWing.AddComponent<MeshRenderer>();
                var col = westWing.AddComponent<BoxCollider>();
                col.size = Vector3.one;
                
                Material mat = Resources.Load<Material>("Materials/Structure");
                if (mat != null) mr.material = mat;
            }

            // Clock tower - tall spire structure
            GameObject towerPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Tower_Tall");
            GameObject clockTower;
            if (towerPrefab != null)
            {
                clockTower = Instantiate(towerPrefab, Vector3.zero, Quaternion.identity);
                clockTower.name = "ClockTower";
                clockTower.transform.SetParent(station.transform);
                clockTower.transform.localPosition = new Vector3(0f, 16f, -10f);
                clockTower.transform.localScale = new Vector3(4f, 12f, 4f);
            }
            else
            {
                Debug.LogError("[Moon10] Structure_Tower_Tall prefab missing - using fallback");
                clockTower = new GameObject("ClockTower_FALLBACK");
                clockTower.transform.SetParent(station.transform);
                clockTower.transform.localScale = new Vector3(4f, 12f, 4f);
                clockTower.transform.localPosition = new Vector3(0f, 16f, -10f);
                
                var mf = clockTower.AddComponent<MeshFilter>();
                var mr = clockTower.AddComponent<MeshRenderer>();
                var col = clockTower.AddComponent<CapsuleCollider>();
                col.radius = 0.5f;
                col.height = 2f;
                
                Material mat = Resources.Load<Material>("Materials/Structure");
                if (mat != null) mr.material = mat;
            }

            // Interactable: station console
            GameObject consolePrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Prop_Console");
            GameObject console;
            if (consolePrefab != null)
            {
                console = Instantiate(consolePrefab, Vector3.zero, Quaternion.identity);
                console.name = "StationConsole";
                console.transform.SetParent(station.transform);
                console.transform.localPosition = new Vector3(0f, 1f, 0f);
                console.transform.localScale = new Vector3(2f, 0.5f, 2f);
            }
            else
            {
                Debug.LogError("[Moon10] Prop_Console prefab missing - using fallback");
                console = new GameObject("StationConsole_FALLBACK");
                console.transform.SetParent(station.transform);
                console.transform.localPosition = new Vector3(0f, 1f, 0f);
                console.transform.localScale = new Vector3(2f, 0.5f, 2f);
                
                var mf = console.AddComponent<MeshFilter>();
                var mr = console.AddComponent<MeshRenderer>();
                var col = console.AddComponent<CapsuleCollider>();
                col.radius = 0.5f;
                col.height = 1f;
                
                Material mat = Resources.Load<Material>("Materials/Prop");
                if (mat != null) mr.material = mat;
            }

            var interactable = console.AddComponent<StationConsole>();
            interactable.spawner = this;

            _stations.Add(station);

            Debug.Log("[Moon 10] Central Station spawned at hub");
        }

        void SpawnMegaStations()
        {
            if (stationPoints == null || stationPoints.Length < totalStations)
            {
                Debug.LogWarning($"[Moon 10] Not enough station points defined ({stationPoints?.Length ?? 0}/{totalStations})");
                return;
            }

            for (int i = 0; i < totalStations; i++)
            {
                var station = new GameObject($"MegaStation_{i + 1}");
                station.transform.position = stationPoints[i];

                // Main building body
                GameObject buildingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Station");
                GameObject building;
                if (buildingPrefab != null)
                {
                    building = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
                    building.name = "StationBuilding";
                    building.transform.SetParent(station.transform);
                    building.transform.localPosition = Vector3.up * 3f;
                    building.transform.localScale = new Vector3(18f, 6f, 18f);
                }
                else
                {
                    Debug.LogError($"[Moon10] Structure_Station prefab missing for MegaStation_{i + 1} - using fallback");
                    building = new GameObject("StationBuilding_FALLBACK");
                    building.transform.SetParent(station.transform);
                    building.transform.localScale = new Vector3(18f, 6f, 18f);
                    building.transform.localPosition = Vector3.up * 3f;
                    
                    var mf = building.AddComponent<MeshFilter>();
                    var mr = building.AddComponent<MeshRenderer>();
                    var col = building.AddComponent<BoxCollider>();
                    col.size = Vector3.one;
                    
                    Material mat = Resources.Load<Material>("Materials/Structure");
                    if (mat != null) mr.material = mat;
                }

                // Roof
                GameObject roofPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Roof");
                GameObject roof;
                if (roofPrefab != null)
                {
                    roof = Instantiate(roofPrefab, Vector3.zero, Quaternion.identity);
                    roof.name = "Roof";
                    roof.transform.SetParent(station.transform);
                    roof.transform.localPosition = Vector3.up * 6.5f;
                    roof.transform.localScale = new Vector3(20f, 1f, 20f);
                }
                else
                {
                    Debug.LogError($"[Moon10] Structure_Roof prefab missing for MegaStation_{i + 1} - using fallback");
                    roof = new GameObject("Roof_FALLBACK");
                    roof.transform.SetParent(station.transform);
                    roof.transform.localScale = new Vector3(20f, 1f, 20f);
                    roof.transform.localPosition = Vector3.up * 6.5f;
                    
                    var mf = roof.AddComponent<MeshFilter>();
                    var mr = roof.AddComponent<MeshRenderer>();
                    var col = roof.AddComponent<BoxCollider>();
                    col.size = Vector3.one;
                    
                    Material mat = Resources.Load<Material>("Materials/Structure");
                    if (mat != null) mr.material = mat;
                }

                // Platform base
                GameObject platformBasePrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Platform_Base");
                GameObject platformBase;
                if (platformBasePrefab != null)
                {
                    platformBase = Instantiate(platformBasePrefab, Vector3.zero, Quaternion.identity);
                    platformBase.name = "PlatformBase";
                    platformBase.transform.SetParent(station.transform);
                    platformBase.transform.localPosition = new Vector3(0f, -1.5f, 0f);
                    platformBase.transform.localScale = new Vector3(16f, 2f, 42f);
                }
                else
                {
                    Debug.LogError($"[Moon10] Structure_Platform_Base prefab missing for MegaStation_{i + 1} - using fallback");
                    platformBase = new GameObject("PlatformBase_FALLBACK");
                    platformBase.transform.SetParent(station.transform);
                    platformBase.transform.localPosition = new Vector3(0f, -1.5f, 0f);
                    platformBase.transform.localScale = new Vector3(16f, 2f, 42f);
                    
                    var mf = platformBase.AddComponent<MeshFilter>();
                    var mr = platformBase.AddComponent<MeshRenderer>();
                    var col = platformBase.AddComponent<BoxCollider>();
                    col.size = Vector3.one;
                    
                    Material mat = Resources.Load<Material>("Materials/Structure");
                    if (mat != null) mr.material = mat;
                }

                // Platform surface
                GameObject platformPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Platform");
                GameObject platform;
                if (platformPrefab != null)
                {
                    platform = Instantiate(platformPrefab, Vector3.zero, Quaternion.identity);
                    platform.name = "Platform";
                    platform.transform.SetParent(station.transform);
                    platform.transform.localPosition = new Vector3(0f, -0.3f, 0f);
                    platform.transform.localScale = new Vector3(15f, 0.5f, 40f);
                }
                else
                {
                    Debug.LogError($"[Moon10] Structure_Platform prefab missing for MegaStation_{i + 1} - using fallback");
                    platform = new GameObject("Platform_FALLBACK");
                    platform.transform.SetParent(station.transform);
                    platform.transform.localPosition = new Vector3(0f, -0.3f, 0f);
                    platform.transform.localScale = new Vector3(15f, 0.5f, 40f);
                    
                    var mf = platform.AddComponent<MeshFilter>();
                    var mr = platform.AddComponent<MeshRenderer>();
                    var col = platform.AddComponent<BoxCollider>();
                    col.size = Vector3.one;
                    
                    Material mat = Resources.Load<Material>("Materials/Structure");
                    if (mat != null) mr.material = mat;
                }

                _stations.Add(station);
            }

            Debug.Log($"[Moon 10] Spawned {totalStations} mega-stations across zones");
        }

        void SpawnTriggerRoom()
        {
            // Hidden basement beneath central station
            _triggerRoom = new GameObject("TriggerRoom_MudFlood");
            _triggerRoom.transform.position = centralStationPoint + Vector3.down * 15f;

            // Outer chamber walls - dungeon structure
            GameObject chamberOuterPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Chamber_Outer");
            GameObject chamberOuter;
            if (chamberOuterPrefab != null)
            {
                chamberOuter = Instantiate(chamberOuterPrefab, Vector3.zero, Quaternion.identity);
                chamberOuter.name = "ChamberOuter";
                chamberOuter.transform.SetParent(_triggerRoom.transform);
                chamberOuter.transform.localPosition = Vector3.zero;
                chamberOuter.transform.localScale = new Vector3(12f, 7f, 12f);
            }
            else
            {
                Debug.LogError("[Moon10] Structure_Chamber_Outer prefab missing - using fallback");
                chamberOuter = new GameObject("ChamberOuter_FALLBACK");
                chamberOuter.transform.SetParent(_triggerRoom.transform);
                chamberOuter.transform.localScale = new Vector3(12f, 7f, 12f);
                chamberOuter.transform.localPosition = Vector3.zero;
                
                var mf = chamberOuter.AddComponent<MeshFilter>();
                var mr = chamberOuter.AddComponent<MeshRenderer>();
                var col = chamberOuter.AddComponent<BoxCollider>();
                col.size = Vector3.one;
                
                Material mat = Resources.Load<Material>("Materials/Structure");
                if (mat != null) mr.material = mat;
            }

            // Inner sanctum
            GameObject chamberInnerPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Chamber_Inner");
            GameObject chamberInner;
            if (chamberInnerPrefab != null)
            {
                chamberInner = Instantiate(chamberInnerPrefab, Vector3.zero, Quaternion.identity);
                chamberInner.name = "ChamberInner";
                chamberInner.transform.SetParent(_triggerRoom.transform);
                chamberInner.transform.localPosition = Vector3.zero;
                chamberInner.transform.localScale = new Vector3(8f, 5f, 8f);
            }
            else
            {
                Debug.LogError("[Moon10] Structure_Chamber_Inner prefab missing - using fallback");
                chamberInner = new GameObject("ChamberInner_FALLBACK");
                chamberInner.transform.SetParent(_triggerRoom.transform);
                chamberInner.transform.localScale = new Vector3(8f, 5f, 8f);
                chamberInner.transform.localPosition = Vector3.zero;
                
                var mf = chamberInner.AddComponent<MeshFilter>();
                var mr = chamberInner.AddComponent<MeshRenderer>();
                var col = chamberInner.AddComponent<BoxCollider>();
                col.size = Vector3.one;
                
                Material mat = Resources.Load<Material>("Materials/Structure");
                if (mat != null) mr.material = mat;
            }

            // Device core (sphere) - ancient orb artifact
            GameObject deviceCorePrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Prop_Orb");
            GameObject deviceCore;
            if (deviceCorePrefab != null)
            {
                deviceCore = Instantiate(deviceCorePrefab, Vector3.zero, Quaternion.identity);
                deviceCore.name = "DeviceCore";
                deviceCore.transform.SetParent(_triggerRoom.transform);
                deviceCore.transform.localPosition = Vector3.zero;
                deviceCore.transform.localScale = Vector3.one * 2f;
            }
            else
            {
                Debug.LogError("[Moon10] Prop_Orb prefab missing - using fallback");
                deviceCore = new GameObject("DeviceCore_FALLBACK");
                deviceCore.transform.SetParent(_triggerRoom.transform);
                deviceCore.transform.localPosition = Vector3.zero;
                deviceCore.transform.localScale = Vector3.one * 2f;
                
                var mf = deviceCore.AddComponent<MeshFilter>();
                var mr = deviceCore.AddComponent<MeshRenderer>();
                var col = deviceCore.AddComponent<SphereCollider>();
                col.radius = 0.5f;
                
                Material mat = Resources.Load<Material>("Materials/Prop");
                if (mat != null) mr.material = mat;
            }

            // Device ring array (3 rings) - amplifier rings
            GameObject ringPrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Prop_Ring");
            for (int i = 0; i < 3; i++)
            {
                GameObject ring;
                if (ringPrefab != null)
                {
                    ring = Instantiate(ringPrefab, Vector3.zero, Quaternion.identity);
                    ring.name = $"AmplifierRing_{i}";
                    ring.transform.SetParent(_triggerRoom.transform);
                    ring.transform.localPosition = Vector3.zero;
                    ring.transform.localScale = new Vector3(3f + i * 0.5f, 0.1f, 3f + i * 0.5f);
                    ring.transform.rotation = Quaternion.Euler(0f, i * 60f, 0f);
                }
                else
                {
                    if (i == 0) Debug.LogError("[Moon10] Prop_Ring prefab missing - using fallback");
                    ring = new GameObject($"AmplifierRing_{i}_FALLBACK");
                    ring.transform.SetParent(_triggerRoom.transform);
                    ring.transform.localPosition = Vector3.zero;
                    ring.transform.localScale = new Vector3(3f + i * 0.5f, 0.1f, 3f + i * 0.5f);
                    ring.transform.rotation = Quaternion.Euler(0f, i * 60f, 0f);
                    
                    var mf = ring.AddComponent<MeshFilter>();
                    var mr = ring.AddComponent<MeshRenderer>();
                    var col = ring.AddComponent<CapsuleCollider>();
                    col.radius = 0.5f;
                    col.height = 0.2f;
                    
                    Material mat = Resources.Load<Material>("Materials/Prop");
                    if (mat != null) mr.material = mat;
                }
            }

            // Control panel interactable
            GameObject panelPrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Prop_Panel");
            GameObject panel;
            if (panelPrefab != null)
            {
                panel = Instantiate(panelPrefab, Vector3.zero, Quaternion.identity);
                panel.name = "ControlPanel";
                panel.transform.SetParent(_triggerRoom.transform);
                panel.transform.localPosition = new Vector3(0f, -1f, 4f);
                panel.transform.localScale = new Vector3(2f, 1f, 0.2f);
            }
            else
            {
                Debug.LogError("[Moon10] Prop_Panel prefab missing - using fallback");
                panel = new GameObject("ControlPanel_FALLBACK");
                panel.transform.SetParent(_triggerRoom.transform);
                panel.transform.localPosition = new Vector3(0f, -1f, 4f);
                panel.transform.localScale = new Vector3(2f, 1f, 0.2f);
                
                var mf = panel.AddComponent<MeshFilter>();
                var mr = panel.AddComponent<MeshRenderer>();
                var col = panel.AddComponent<BoxCollider>();
                col.size = Vector3.one;
                
                Material mat = Resources.Load<Material>("Materials/Prop");
                if (mat != null) mr.material = mat;
            }

            var interactable = panel.AddComponent<TriggerRoomPanel>();
            interactable.spawner = this;

            // Initially hidden until player discovers basement entrance
            _triggerRoom.SetActive(false);

            Debug.Log("[Moon 10] Mud Flood trigger room spawned (hidden until discovery)");
        }

        void SpawnInitialRailSegments()
        {
            if (railNodePoints == null || railNodePoints.Length < 3)
            {
                Debug.LogWarning("[Moon 10] Not enough rail node points defined");
                return;
            }

            // Spawn first 3 segments (partial network)
            for (int i = 0; i < 3; i++)
            {
                SpawnRailSegment(i);
            }

            Debug.Log("[Moon 10] Initial 3 rail segments spawned (9 remaining to build)");
        }

        void InitializeRailPathfinding()
        {
            if (railNodePoints == null || railNodePoints.Length == 0)
            {
                Debug.LogWarning("[Moon 10] No rail node points defined for pathfinding");
                return;
            }

            // Create pathfinding nodes for each rail segment
            for (int i = 0; i < railNodePoints.Length; i++)
            {
                RailPathNode node = new RailPathNode
                {
                    index = i,
                    position = railNodePoints[i],
                    connections = new List<int>()
                };

                // Connect to next node (linear for now, could add branches)
                if (i < railNodePoints.Length - 1)
                {
                    node.connections.Add(i + 1);
                }
                // Wrap around (circular network)
                if (i == railNodePoints.Length - 1)
                {
                    node.connections.Add(0);
                }

                _pathNodes.Add(node);
            }

            Debug.Log($"[Moon 10] Rail pathfinding initialized: {_pathNodes.Count} nodes");
        }

        public List<Vector3> CalculateRailPath(Vector3 start, Vector3 end)
        {
            // Simple A* pathfinding through rail network
            if (_pathNodes.Count == 0) return new List<Vector3> { start, end };

            // Find closest start/end nodes
            int startNode = FindClosestNode(start);
            int endNode = FindClosestNode(end);

            if (startNode == endNode)
                return new List<Vector3> { start, _pathNodes[startNode].position, end };

            // A* search
            List<int> openSet = new List<int> { startNode };
            Dictionary<int, int> cameFrom = new Dictionary<int, int>();
            Dictionary<int, float> gScore = new Dictionary<int, float>();
            Dictionary<int, float> fScore = new Dictionary<int, float>();

            foreach (var node in _pathNodes)
            {
                gScore[node.index] = float.MaxValue;
                fScore[node.index] = float.MaxValue;
            }

            gScore[startNode] = 0f;
            fScore[startNode] = Vector3.Distance(_pathNodes[startNode].position, _pathNodes[endNode].position);

            while (openSet.Count > 0)
            {
                // Find node with lowest fScore
                int current = openSet[0];
                float lowestF = fScore[current];
                foreach (int node in openSet)
                {
                    if (fScore[node] < lowestF)
                    {
                        current = node;
                        lowestF = fScore[node];
                    }
                }

                if (current == endNode)
                {
                    // Reconstruct path
                    return ReconstructPath(cameFrom, current, start, end);
                }

                openSet.Remove(current);

                // Check neighbors
                foreach (int neighbor in _pathNodes[current].connections)
                {
                    float tentativeG = gScore[current] + Vector3.Distance(
                        _pathNodes[current].position,
                        _pathNodes[neighbor].position
                    );

                    if (tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = gScore[neighbor] + Vector3.Distance(
                            _pathNodes[neighbor].position,
                            _pathNodes[endNode].position
                        );

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            // No path found, return direct
            Debug.LogWarning("[Moon 10] A* pathfinding failed, returning direct path");
            return new List<Vector3> { start, end };
        }

        int FindClosestNode(Vector3 position)
        {
            int closest = 0;
            float minDist = float.MaxValue;

            for (int i = 0; i < _pathNodes.Count; i++)
            {
                float dist = Vector3.Distance(position, _pathNodes[i].position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = i;
                }
            }

            return closest;
        }

        List<Vector3> ReconstructPath(Dictionary<int, int> cameFrom, int current, Vector3 start, Vector3 end)
        {
            List<Vector3> path = new List<Vector3> { _pathNodes[current].position };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, _pathNodes[current].position);
            }

            // Add start/end waypoints
            path.Insert(0, start);
            path.Add(end);

            Debug.Log($"[Moon 10] Rail path calculated: {path.Count} waypoints");
            return path;
        }

        void SpawnOrphanTrainPuzzle()
        {
            Vector3 puzzlePos = centralStationPoint + new Vector3(20f, 0f, 15f);

            _orphanTrainPuzzle = new GameObject("OrphanTrainPuzzle");
            _orphanTrainPuzzle.transform.position = puzzlePos;

            // Puzzle platform
            GameObject platformPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Platform");
            GameObject platform;
            if (platformPrefab != null)
            {
                platform = Instantiate(platformPrefab, Vector3.zero, Quaternion.identity);
                platform.name = "PuzzlePlatform";
                platform.transform.SetParent(_orphanTrainPuzzle.transform);
                platform.transform.localPosition = Vector3.zero;
                platform.transform.localScale = new Vector3(10f, 0.5f, 10f);
            }
            else
            {
                Debug.LogError("[Moon10] Structure_Platform prefab missing for puzzle - using fallback");
                platform = new GameObject("PuzzlePlatform_FALLBACK");
                platform.transform.SetParent(_orphanTrainPuzzle.transform);
                platform.transform.localPosition = Vector3.zero;
                platform.transform.localScale = new Vector3(10f, 0.5f, 10f);
                
                var mf = platform.AddComponent<MeshFilter>();
                var mr = platform.AddComponent<MeshRenderer>();
                var col = platform.AddComponent<BoxCollider>();
                col.size = Vector3.one;
                
                Material mat = Resources.Load<Material>("Materials/Structure");
                if (mat != null) mr.material = mat;
            }

            // 3 orphan children NPCs (from Moon 3) — KayKit Rogue scaled down
            for (int i = 0; i < 3; i++)
            {
                GameObject childPrefab = Resources.Load<GameObject>("Prefabs/Characters/KayKit/Char_Rogue");
                GameObject childObj;
                if (childPrefab != null)
                {
                    childObj = Instantiate(childPrefab, Vector3.zero, Quaternion.identity);
                    childObj.name = $"OrphanEngineer_{i}";
                    childObj.transform.SetParent(_orphanTrainPuzzle.transform);
                    childObj.transform.localPosition = new Vector3(i * 2f - 2f, 0f, 2f);
                    childObj.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f); // Child scale
                }
                else
                {
                    Debug.LogError("[Moon10ContentSpawner] CRITICAL: Char_Rogue prefab missing for orphan children");
                    childObj = new GameObject($"OrphanEngineer_{i}_MISSING_PREFAB");
                    childObj.transform.SetParent(_orphanTrainPuzzle.transform);
                    childObj.transform.localPosition = new Vector3(i * 2f - 2f, 1.5f, 2f);
                }

                OrphanEngineerNPC engineer = childObj.AddComponent<OrphanEngineerNPC>();
                engineer.engineerIndex = i;
                engineer.spawner = this;
            }

            // Puzzle console (resonance tuning mini-game)
            GameObject consolePrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Prop_Console");
            GameObject console;
            if (consolePrefab != null)
            {
                console = Instantiate(consolePrefab, Vector3.zero, Quaternion.identity);
                console.name = "PuzzleConsole";
                console.transform.SetParent(_orphanTrainPuzzle.transform);
                console.transform.localPosition = new Vector3(0f, 1f, -3f);
                console.transform.localScale = new Vector3(2f, 1.5f, 1f);
            }
            else
            {
                Debug.LogError("[Moon10] Prop_Console prefab missing for puzzle - using fallback");
                console = new GameObject("PuzzleConsole_FALLBACK");
                console.transform.SetParent(_orphanTrainPuzzle.transform);
                console.transform.localPosition = new Vector3(0f, 1f, -3f);
                console.transform.localScale = new Vector3(2f, 1.5f, 1f);
                
                var mf = console.AddComponent<MeshFilter>();
                var mr = console.AddComponent<MeshRenderer>();
                var col = console.AddComponent<BoxCollider>();
                col.size = Vector3.one;
                
                Material mat = Resources.Load<Material>("Materials/Prop");
                if (mat != null) mr.material = mat;
            }

            OrphanTrainPuzzleConsole puzzleComp = console.AddComponent<OrphanTrainPuzzleConsole>();
            puzzleComp.spawner = this;

            Debug.Log("[Moon 10] Orphan train puzzle spawned — 3 children engineers await player guidance");
        }

        public void OnOrphanPuzzleSolved()
        {
            orphanPuzzleSolved = true;
            Debug.Log("[Moon 10] Orphan train puzzle solved! Children: 'We did it! The rails are singing!'");

            // Unlock advanced rail segments
            QuestManager.Instance?.CompleteQuest("moon10_orphan_puzzle");

            // Children dialogue
            DialogueManager.Instance?.PlayContextDialogue("moon10_orphans_success");

            // Spawn Rail Leviathan boss (awakened by network reactivation)
            if (_segmentsLaid >= 8 && !railLeviathanDefeated)
            {
                SpawnRailLeviathan();
            }
        }

        void SpawnRailLeviathan()
        {
            Debug.Log("[Moon 10] CONFLICT: Rail Leviathan awakens! Ancient guardian of the network!");

            // CRITICAL: Multi-part serpent boss (7 segments)
            GameObject bossObj = new GameObject("RailLeviathan_Boss");
            bossObj.transform.position = leviathanSpawnPoint;
            bossObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Horizontal

            // Try to load serpent segment prefabs
            GameObject headPrefab = Resources.Load<GameObject>("Prefabs/Enemies/KayKit/Enemy_Serpent_Head");
            GameObject bodyPrefab = Resources.Load<GameObject>("Prefabs/Enemies/KayKit/Enemy_Serpent_Body");
            GameObject tailPrefab = Resources.Load<GameObject>("Prefabs/Enemies/KayKit/Enemy_Serpent_Tail");

            bool usePrefabs = (headPrefab != null && bodyPrefab != null && tailPrefab != null);
            if (!usePrefabs)
            {
                Debug.LogError("[Moon10] Rail Leviathan serpent prefabs missing - using fallback primitives");
            }

            // Head segment (largest)
            GameObject head;
            if (usePrefabs)
            {
                head = Instantiate(headPrefab, Vector3.zero, Quaternion.identity);
                head.name = "Segment_Head";
                head.transform.SetParent(bossObj.transform);
                head.transform.localPosition = new Vector3(0f, 0f, 0f);
                head.transform.localScale = new Vector3(4f, 6f, 4f);
            }
            else
            {
                head = new GameObject("Segment_Head_FALLBACK");
                head.transform.SetParent(bossObj.transform);
                head.transform.localPosition = new Vector3(0f, 0f, 0f);
                head.transform.localScale = new Vector3(4f, 6f, 4f);
                
                var mf = head.AddComponent<MeshFilter>();
                var mr = head.AddComponent<MeshRenderer>();
                var col = head.AddComponent<CapsuleCollider>();
                col.radius = 0.5f;
                col.height = 2f;
                
                Material mat = Resources.Load<Material>("Materials/Enemy");
                if (mat != null) {
                    mr.material = mat;
                } else {
                    mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mr.material.color = new Color(0.3f, 0.25f, 0.2f);
                }
            }

            // Body segments (5 segments tapering)
            for (int i = 0; i < 5; i++)
            {
                GameObject segment;
                float scale = 3.5f - (i * 0.4f); // Taper from 3.5 to 1.9
                float length = 5f - (i * 0.3f); // Taper length
                Vector3 pos = new Vector3(0f, -6f - (i * 5f), 0f);

                if (usePrefabs)
                {
                    segment = Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity);
                    segment.name = $"Segment_Body_{i}";
                    segment.transform.SetParent(bossObj.transform);
                    segment.transform.localPosition = pos;
                    segment.transform.localScale = new Vector3(scale, length, scale);
                }
                else
                {
                    segment = new GameObject($"Segment_Body_{i}_FALLBACK");
                    segment.transform.SetParent(bossObj.transform);
                    segment.transform.localPosition = pos;
                    segment.transform.localScale = new Vector3(scale, length, scale);
                    
                    var mf = segment.AddComponent<MeshFilter>();
                    var mr = segment.AddComponent<MeshRenderer>();
                    var col = segment.AddComponent<CapsuleCollider>();
                    col.radius = 0.5f;
                    col.height = 2f;
                    
                    Material mat = Resources.Load<Material>("Materials/Enemy");
                    if (mat != null) {
                        mr.material = mat;
                    } else {
                        mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        mr.material.color = new Color(0.3f, 0.25f, 0.2f);
                    }
                }
            }

            // Tail segment (smallest)
            GameObject tail;
            if (usePrefabs)
            {
                tail = Instantiate(tailPrefab, Vector3.zero, Quaternion.identity);
                tail.name = "Segment_Tail";
                tail.transform.SetParent(bossObj.transform);
                tail.transform.localPosition = new Vector3(0f, -31f, 0f);
                tail.transform.localScale = new Vector3(1.5f, 3f, 1.5f);
            }
            else
            {
                tail = new GameObject("Segment_Tail_FALLBACK");
                tail.transform.SetParent(bossObj.transform);
                tail.transform.localPosition = new Vector3(0f, -31f, 0f);
                tail.transform.localScale = new Vector3(1.5f, 3f, 1.5f);
                
                var mf = tail.AddComponent<MeshFilter>();
                var mr = tail.AddComponent<MeshRenderer>();
                var col = tail.AddComponent<CapsuleCollider>();
                col.radius = 0.5f;
                col.height = 2f;
                
                Material mat = Resources.Load<Material>("Materials/Enemy");
                if (mat != null) {
                    mr.material = mat;
                } else {
                    mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mr.material.color = new Color(0.3f, 0.25f, 0.2f);
                }
            }

            // Boss light
            Light bossLight = bossObj.AddComponent<Light>();
            bossLight.type = LightType.Point;
            bossLight.color = new Color(0.8f, 0.3f, 0.2f); // Ember red
            bossLight.range = 50f;
            bossLight.intensity = 4f;

            // RailLeviathan component
            RailLeviathan leviathan = bossObj.AddComponent<RailLeviathan>();
            leviathan.spawner = this;
            leviathan.railPath = CalculateRailPath(leviathanSpawnPoint, centralStationPoint);

            // Audio: boss encounter
            Audio.AudioManager.Instance?.PlaySFX3D("Moon10_LeviathanRoar", leviathanSpawnPoint);

            // Quest: defeat leviathan
            QuestManager.Instance?.ActivateQuest("moon10_defeat_rail_leviathan");

            Debug.Log("[Moon 10] Rail Leviathan spawned — 5000 HP, follows rail pathfinding");
        }

        public void OnLeviathanDefeated()
        {
            railLeviathanDefeated = true;
            Debug.Log("[Moon 10] Rail Leviathan defeated! Network fully secured!");

            // Quest complete
            QuestManager.Instance?.CompleteQuest("moon10_defeat_rail_leviathan");

            // Revelation dialogue
            DialogueManager.Instance?.PlayContextDialogue("moon10_leviathan_defeated");

            // Complete moon if all conditions met
            if (_segmentsLaid >= totalRailSegments && _stationsBuilt >= totalStations)
            {
                CompleteMoon10();
            }
        }

        void SpawnRailSegment(int index)
        {
            if (railNodePoints == null || index >= railNodePoints.Length)
                return;

            GameObject segment;
            if (railSegmentPrefab != null)
            {
                segment = Instantiate(railSegmentPrefab, railNodePoints[index], Quaternion.identity);
                segment.name = $"RailSegment_{index}";
            }
            else
            {
                // Multi-part rail track structure (no prefab assigned)
                segment = new GameObject($"RailSegment_{index}");
                segment.transform.position = railNodePoints[index];

                // Load rail track components
                GameObject trackPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Rail_Track");
                GameObject tiesPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Rail_Ties");
                GameObject ballastPrefab = Resources.Load<GameObject>("Prefabs/Buildings/KayKit/Structure_Rail_Ballast");

                bool usePrefabs = (trackPrefab != null && tiesPrefab != null && ballastPrefab != null);
                if (!usePrefabs && index == 0)
                {
                    Debug.LogError("[Moon10] Rail structure prefabs missing - using fallback primitives");
                }

                // Rail track (top)
                GameObject track;
                if (usePrefabs)
                {
                    track = Instantiate(trackPrefab, Vector3.zero, Quaternion.identity);
                    track.name = "RailTrack";
                    track.transform.SetParent(segment.transform);
                    track.transform.localPosition = Vector3.up * 0.2f;
                    track.transform.localScale = new Vector3(2f, 0.2f, 20f);
                }
                else
                {
                    track = new GameObject("RailTrack_FALLBACK");
                    track.transform.SetParent(segment.transform);
                    track.transform.localPosition = Vector3.up * 0.2f;
                    track.transform.localScale = new Vector3(2f, 0.2f, 20f);
                    
                    var mf = track.AddComponent<MeshFilter>();
                    var mr = track.AddComponent<MeshRenderer>();
                    var col = track.AddComponent<BoxCollider>();
                    col.size = Vector3.one;
                    
                    Material mat = Resources.Load<Material>("Materials/Rail");
                    if (mat != null) mr.material = mat;
                }

                // Rail ties (middle)
                GameObject ties;
                if (usePrefabs)
                {
                    ties = Instantiate(tiesPrefab, Vector3.zero, Quaternion.identity);
                    ties.name = "RailTies";
                    ties.transform.SetParent(segment.transform);
                    ties.transform.localPosition = Vector3.zero;
                    ties.transform.localScale = new Vector3(2.5f, 0.3f, 20f);
                }
                else
                {
                    ties = new GameObject("RailTies_FALLBACK");
                    ties.transform.SetParent(segment.transform);
                    ties.transform.localPosition = Vector3.zero;
                    ties.transform.localScale = new Vector3(2.5f, 0.3f, 20f);
                    
                    var mf = ties.AddComponent<MeshFilter>();
                    var mr = ties.AddComponent<MeshRenderer>();
                    var col = ties.AddComponent<BoxCollider>();
                    col.size = Vector3.one;
                    
                    Material mat = Resources.Load<Material>("Materials/Rail");
                    if (mat != null) mr.material = mat;
                }

                // Ballast base (bottom)
                GameObject ballast;
                if (usePrefabs)
                {
                    ballast = Instantiate(ballastPrefab, Vector3.zero, Quaternion.identity);
                    ballast.name = "RailBallast";
                    ballast.transform.SetParent(segment.transform);
                    ballast.transform.localPosition = Vector3.down * 0.3f;
                    ballast.transform.localScale = new Vector3(3f, 0.4f, 20f);
                }
                else
                {
                    ballast = new GameObject("RailBallast_FALLBACK");
                    ballast.transform.SetParent(segment.transform);
                    ballast.transform.localPosition = Vector3.down * 0.3f;
                    ballast.transform.localScale = new Vector3(3f, 0.4f, 20f);
                    
                    var mf = ballast.AddComponent<MeshFilter>();
                    var mr = ballast.AddComponent<MeshRenderer>();
                    var col = ballast.AddComponent<BoxCollider>();
                    col.size = Vector3.one;
                    
                    Material mat = Resources.Load<Material>("Materials/Rail");
                    if (mat != null) mr.material = mat;
                }
            }

            _railSegments.Add(segment);
        }

        public void BuildRailSegment(int segmentIndex)
        {
            if (segmentIndex < 3 || segmentIndex >= totalRailSegments)
            {
                Debug.LogWarning($"[Moon 10] Invalid segment index {segmentIndex}");
                return;
            }

            SpawnRailSegment(segmentIndex);
            _segmentsLaid++;

            Debug.Log($"[Moon 10] Rail segment {segmentIndex} built — {_segmentsLaid}/{totalRailSegments} complete");

            // Play construction sound
            Audio.AudioManager.Instance?.PlaySFX2D("RailConstruction");

            // Update quest progress
            QuestManager.Instance?.ProgressObjective("moon10_rail_network", 0, 1);

            // Check completion
            if (_segmentsLaid >= totalRailSegments && _stationsBuilt >= totalStations)
            {
                CompleteMoon10();
            }
        }

        public void BuildStation(int stationIndex)
        {
            if (stationIndex >= totalStations)
            {
                Debug.LogWarning($"[Moon 10] Invalid station index {stationIndex}");
                return;
            }

            _stationsBuilt++;

            Debug.Log($"[Moon 10] Mega-station {stationIndex} constructed — {_stationsBuilt}/{totalStations} complete");

            // Play construction sound
            Audio.AudioManager.Instance?.PlaySFX2D("StationConstruction");

            // Update quest progress
            QuestManager.Instance?.ProgressObjective("moon10_rail_network", 1, 1);

            // Check completion
            if (_segmentsLaid >= totalRailSegments && _stationsBuilt >= totalStations)
            {
                CompleteMoon10();
            }
        }

        public void DiscoverTriggerRoom()
        {
            if (_triggerRoomDiscovered) return;

            _triggerRoomDiscovered = true;
            _triggerRoom?.SetActive(true);

            Debug.Log("[Moon 10] Mud Flood trigger room discovered — 3 fingerprint sets found (1 giant, 2 human)");

            // Dialogue
            DialogueManager.Instance?.PlayContextDialogue("trigger_room_discovery");

            // Quest update
            QuestManager.Instance?.ActivateQuest("moon10_trigger_room_analysis");

            // Achievement
            AchievementSystem.Instance?.Unlock("trigger_room_found");
        }

        void CompleteMoon10()
        {
            if (railNetworkComplete) return;

            railNetworkComplete = true;

            Debug.Log("[Moon 10] PLANETARY MOON complete — Continental rail network operational!");

            // Spawn full continental train
            SpawnContinentalTrain();

            // Completion VFX
            var vfx = new GameObject("Moon10_CompletionVFX");
            vfx.transform.position = centralStationPoint;
            var particles = vfx.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.8f, 0.6f, 0.2f, 1f);  // Bronze/rail color
            main.startSize = 3f;
            main.startLifetime = 10f;
            main.maxParticles = 2000;

            // Quest complete
            QuestManager.Instance?.CompleteQuest("moon10_rail_network_complete");

            // RS Reward for Moon completion
            GameLoopController.Instance?.QueueRSReward(700f, "Moon 10 Complete: Continental Railway");

            // HUD: Moon trophy
            GameEvents.RaiseHUDShowMoonTrophy("MOON 10 COMPLETE", "The Manifestation of Producing");

            // Audio: completion fanfare
            AudioManager.Instance?.PlaySFX2D("MoonCompleteFanfare");

            // Unlock Moon 11
            SaveManager.Instance?.SetMoonProgress(10, 100f);

            // Revelation dialogue
            DialogueManager.Instance?.PlayContextDialogue("moon10_revelation");

            // Prophecy stones 7-9 appear
            Debug.Log("[Moon 10] Prophecy Stones 7-9 (Giants, Children, Rail) now accessible");
        }

        void SpawnContinentalTrain()
        {
            if (trainPrefab == null)
            {
                Debug.LogWarning("[Moon 10] Train prefab not assigned");
                return;
            }

            var train = Instantiate(trainPrefab, centralStationPoint + Vector3.up * 2f, Quaternion.identity);
            train.name = "ContinentalTrain";
            _trains.Add(train);

            Debug.Log("[Moon 10] Continental train spawned — ready for full network journey");

            // Train journey dialogue
            DialogueManager.Instance?.PlayContextDialogue("continental_train_journey");
        }

        /// <summary>
        /// Station Console interactable — build rail segments from this interface
        /// </summary>
        public class StationConsole : MonoBehaviour, IInteractable
        {
            public Moon10ContentSpawner spawner;

            public string GetInteractPrompt()
            {
                if (spawner == null || spawner._segmentsLaid >= spawner.totalRailSegments)
                    return "";
                return $"Hold [E] to Build Rail Segment ({spawner._segmentsLaid}/{spawner.totalRailSegments})";
            }

            public void Interact(GameObject interactor)
            {
                if (spawner == null) return;

                // Build next segment
                int nextSegment = spawner._segmentsLaid;
                if (nextSegment < spawner.totalRailSegments)
                {
                    spawner.BuildRailSegment(nextSegment);
                    Debug.Log($"[StationConsole] Building segment {nextSegment}");
                }
            }
        }

        /// <summary>
        /// Trigger Room Panel interactable — analyze Mud Flood device
        /// </summary>
        public class TriggerRoomPanel : MonoBehaviour, IInteractable
        {
            public Moon10ContentSpawner spawner;
            bool _analyzed;

            public string GetInteractPrompt()
            {
                return _analyzed ? "" : "Hold [E] to Analyze Control Panel";
            }

            public void Interact(GameObject interactor)
            {
                if (_analyzed || spawner == null) return;

                _analyzed = true;

                Debug.Log("[TriggerRoom] Control panel analysis:");
                Debug.Log("  - 3 fingerprint sets detected");
                Debug.Log("  - 1 giant-sized (matches Zereth's proportions)");
                Debug.Log("  - 2 human-sized (unknown, matches Parasite Cabal operatives)");
                Debug.Log("  - Device is a massive dissonance amplifier pointed at star fort network");

                // Dialogue
                DialogueManager.Instance?.PlayContextDialogue("trigger_room_analysis");

                // Quest progress
                QuestManager.Instance?.ProgressObjective("moon10_trigger_room_analysis", 0, 1);
            }
        }
    }

    /// <summary>
    /// Rail pathfinding node for A* algorithm.
    /// </summary>
    public class RailPathNode
    {
        public int index;
        public Vector3 position;
        public List<int> connections; // Indices of connected nodes
    }

    /// <summary>
    /// Orphan engineer NPC from Moon 3 — helps with train puzzle.
    /// </summary>
    public class OrphanEngineerNPC : MonoBehaviour, IInteractable
    {
        public int engineerIndex;
        public Moon10ContentSpawner spawner;
        bool _hasSpoken;

        readonly string[] _dialogues = {
            "I remember the rails from the visions! Milo taught me the resonance frequencies!",
            "Thorne let me steer the airship once. Now I get to fix trains!",
            "The giant who made the Flood... was he trying to help or hurt us?"
        };

        public string GetInteractPrompt() => _hasSpoken ? "" : "Talk to Engineer (E)";

        public void Interact(GameObject player)
        {
            if (_hasSpoken || engineerIndex >= _dialogues.Length) return;

            _hasSpoken = true;
            Debug.Log($"[OrphanEngineer {engineerIndex}] {_dialogues[engineerIndex]}");

            UI.GameEvents.RaiseHUDShowDialogue("Engineer", _dialogues[engineerIndex]);
        }
    }

    /// <summary>
    /// Orphan train puzzle console — resonance tuning mini-game.
    /// </summary>
    public class OrphanTrainPuzzleConsole : MonoBehaviour, IInteractable
    {
        public Moon10ContentSpawner spawner;

        int _tuningProgress;
        const int RequiredTunings = 3;

        public string GetInteractPrompt() =>
            _tuningProgress >= RequiredTunings ? "Puzzle Complete" : $"Tune Resonance ({_tuningProgress}/{RequiredTunings}) [E]";

        public void Interact(GameObject player)
        {
            if (_tuningProgress >= RequiredTunings) return;

            _tuningProgress++;
            Debug.Log($"[OrphanPuzzle] Resonance tuning {_tuningProgress}/{RequiredTunings} — frequency aligned!");

            // Audio: tuning harmonic
            Audio.AudioManager.Instance?.PlaySFX3D("RailTuning", transform.position);

            // VFX: resonance pulse
            GameObject vfxObj = new GameObject("TuningPulse_VFX");
            vfxObj.transform.position = transform.position;

            ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.startSize = 0.5f;
            main.loop = false;
            main.maxParticles = 200;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 200) });

            Destroy(vfxObj, 2f);

            if (_tuningProgress >= RequiredTunings)
            {
                Debug.Log("[OrphanPuzzle] All frequencies aligned! Puzzle solved!");
                spawner?.OnOrphanPuzzleSolved();
            }
        }
    }

    /// <summary>
    /// Rail Leviathan boss — ancient serpent guardian of rail network.
    /// Follows rail pathfinding, attacks with seismic tremors.
    /// </summary>
    public class RailLeviathan : MonoBehaviour
    {
        public Moon10ContentSpawner spawner;
        public List<Vector3> railPath;

        float _health = 5000f;
        float _maxHealth = 5000f;
        float _attackCooldown;
        int _currentPathIndex;
        float _moveSpeed = 8f;
        bool _isDefeated;
        int _currentPhase = 1;

        void Start()
        {
            Debug.Log("[RailLeviathan] Boss engaged! HP: 5000");

            // Show boss health bar
            GameEvents.RaiseHUDShowBossHealth("Rail Leviathan", 1f);
        }

        void Update()
        {
            if (_isDefeated) return;

            // Follow rail path
            if (railPath != null && railPath.Count > 0)
            {
                Vector3 target = railPath[_currentPathIndex];
                Vector3 direction = (target - transform.position).normalized;

                transform.position += direction * _moveSpeed * Time.deltaTime;

                // Look along movement direction
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }

                // Check if reached waypoint
                if (Vector3.Distance(transform.position, target) < 2f)
                {
                    _currentPathIndex = (_currentPathIndex + 1) % railPath.Count;
                }
            }

            // Phase transitions
            if (_health < 3000f && _currentPhase == 1)
            {
                EnterPhase2();
            }
            else if (_health < 1500f && _currentPhase == 2)
            {
                EnterPhase3();
            }

            // Attack pattern
            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown <= 0f)
            {
                SeismicTremor();
                _attackCooldown = 5f;
            }

            // Pulse light
            Light light = GetComponent<Light>();
            if (light != null)
            {
                light.intensity = 4f + Mathf.Sin(Time.time * 2f) * 1.5f;
            }
        }

        void EnterPhase2()
        {
            _currentPhase = 2;
            Debug.Log("[RailLeviathan] PHASE 2: Leviathan speeds up — rails crack beneath!");

            // Increase movement speed
            _moveSpeed = 12f;

            // VFX: rail sparks
            // Audio phase transition
            Audio.AudioManager.Instance?.PlaySFX3D("BossPhase2", transform.position);

            // More frequent attacks
            _attackCooldown = 3f;
        }

        void EnterPhase3()
        {
            _currentPhase = 3;
            Debug.Log("[RailLeviathan] PHASE 3: ENRAGE — Network destabilization!");

            // Maximum speed
            _moveSpeed = 16f;

            // Audio phase transition
            Audio.AudioManager.Instance?.PlaySFX3D("BossPhase3", transform.position);

            // Rapid attacks
            _attackCooldown = 2f;
        }

        void SeismicTremor()
        {
            Debug.Log("[RailLeviathan] Seismic tremor! Ground shakes!");

            // Spawn shockwave VFX (ParticleSystem replacement)
            GameObject shockwaveVFX = new GameObject("SeismicShockwave_VFX");
            shockwaveVFX.transform.position = transform.position;
            
            ParticleSystem psShock = shockwaveVFX.AddComponent<ParticleSystem>();
            var mainShock = psShock.main;
            mainShock.startLifetime = 2.5f;
            mainShock.startSpeed = 10f;
            mainShock.startSize = 1.0f;
            mainShock.startColor = new Color(0.8f, 0.4f, 0.2f, 0.7f);
            mainShock.maxParticles = 150;
            mainShock.loop = false;
            mainShock.duration = 2.0f;
            
            var emissionShock = psShock.emission;
            emissionShock.rateOverTime = 0;
            emissionShock.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 100) });
            
            var shapeShock = psShock.shape;
            shapeShock.shapeType = ParticleSystemShapeType.Circle;
            shapeShock.radius = 1f;
            
            var rendererShock = shockwaveVFX.GetComponent<ParticleSystemRenderer>();
            rendererShock.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rendererShock.material.SetColor("_BaseColor", new Color(0.8f, 0.4f, 0.2f));
            
            psShock.Play();

            // Expand shockwave
            StartCoroutine(ExpandShockwave(shockwaveVFX));

            // Audio
            Audio.AudioManager.Instance?.PlaySFX3D("SeismicTremor", transform.position);

            // Damage player if in range (simplified for beta)
        }

        System.Collections.IEnumerator ExpandShockwave(GameObject wave)
        {
            float duration = 1.5f;
            float elapsed = 0f;
            Vector3 startScale = wave.transform.localScale;
            Vector3 endScale = startScale * 3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                wave.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);

                Renderer rend = wave.GetComponent<Renderer>();
                if (rend != null)
                {
                    Color c = rend.material.color;
                    c.a = 1f - (elapsed / duration);
                    rend.material.color = c;
                }

                yield return null;
            }

            Destroy(wave);
        }

        public void TakeDamage(float damage)
        {
            if (_isDefeated) return;

            _health -= damage;
            Debug.Log($"[RailLeviathan] Took {damage} damage, {_health} HP remaining");

            // Update HUD boss health
            GameEvents.RaiseHUDUpdateBossHealth(_health / _maxHealth);

            if (_health <= 0f)
            {
                DefeatBoss();
            }
        }

        void DefeatBoss()
        {
            if (_isDefeated) return;
            _isDefeated = true;

            Debug.Log("[RailLeviathan] DEFEATED! Network guardian falls!");

            // Hide boss health bar
            GameEvents.RaiseHUDHideBossHealth();

            // Death VFX
            GameObject vfxObj = new GameObject("LeviathanDefeat_VFX");
            vfxObj.transform.position = transform.position;

            ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 4f;
            main.startSpeed = 15f;
            main.startSize = 2.5f;
            main.loop = false;
            main.maxParticles = 1500;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1500) });

            Destroy(vfxObj, 5f);

            // Audio
            Audio.AudioManager.Instance?.PlaySFX3D("LeviathanDeath", transform.position);

            // RS Reward for boss kill
            GameLoopController.Instance?.QueueRSReward(300f, "Rail Leviathan Defeated");

            // Notify spawner
            spawner?.OnLeviathanDefeated();

            // Destroy boss
            Destroy(gameObject, 1f);
        }
    }
}

