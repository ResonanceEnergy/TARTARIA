using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Save;

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

        readonly List<GameObject> _railSegments = new();
        readonly List<GameObject> _stations = new();
        readonly List<GameObject> _trains = new();
        GameObject _triggerRoom;
        bool _contentSpawned;
        bool _triggerRoomDiscovered;

        public bool IsMoon10Active => moon10Unlocked && !railNetworkComplete;
        public int RailProgress => _segmentsLaid;
        public float CompletionPercent => (_segmentsLaid + _stationsBuilt) / (float)(totalRailSegments + totalStations);

        void Awake()
        {
            // Check save state
            moon10Unlocked = SaveManager.Instance?.GetMoonProgress(10) > 0f;
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

            // Ambient audio
            var railAmbience = Audio.AudioManager.Instance?.PlayLoopingSFX("RailNetworkHum", centralStationPoint, 0.3f);
            if (railAmbience != null)
            {
                Debug.Log("[Moon 10] Rail network ambient hum active at 432 Hz");
            }

            // Quest activation
            QuestManager.Instance?.ActivateQuest("moon10_rail_network_discovery");

            // Dialogue: children NPCs (from Moon 3) now junior engineers
            // TODO: HUDController not implemented
            // HUDController.Instance?.ShowObjective("The rails sing again. Connect the continent.");
        }

        void SpawnCentralStation()
        {
            var station = new GameObject("CentralStation_Moon10");
            station.transform.position = centralStationPoint;

            // Station building visual (placeholder cube scaled large)
            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = "StationBuilding";
            building.transform.SetParent(station.transform);
            building.transform.localScale = new Vector3(30f, 10f, 30f);
            building.transform.localPosition = Vector3.zero;

            // Interactable: station console
            var console = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            console.name = "StationConsole";
            console.transform.SetParent(station.transform);
            console.transform.localPosition = new Vector3(0f, 1f, 0f);
            console.transform.localScale = new Vector3(2f, 0.5f, 2f);

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

                // Station building
                var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.name = "StationBuilding";
                building.transform.SetParent(station.transform);
                building.transform.localScale = new Vector3(20f, 8f, 20f);
                building.transform.localPosition = Vector3.zero;

                // Platform
                var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.name = "Platform";
                platform.transform.SetParent(station.transform);
                platform.transform.localPosition = new Vector3(0f, -2f, 0f);
                platform.transform.localScale = new Vector3(15f, 0.5f, 40f);

                _stations.Add(station);
            }

            Debug.Log($"[Moon 10] Spawned {totalStations} mega-stations across zones");
        }

        void SpawnTriggerRoom()
        {
            // Hidden basement beneath central station
            _triggerRoom = new GameObject("TriggerRoom_MudFlood");
            _triggerRoom.transform.position = centralStationPoint + Vector3.down * 15f;

            // Room chamber
            var chamber = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chamber.name = "Chamber";
            chamber.transform.SetParent(_triggerRoom.transform);
            chamber.transform.localScale = new Vector3(10f, 6f, 10f);
            chamber.transform.localPosition = Vector3.zero;

            // Trigger device (massive dissonance amplifier)
            var device = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            device.name = "DissonanceAmplifier";
            device.transform.SetParent(_triggerRoom.transform);
            device.transform.localPosition = Vector3.zero;
            device.transform.localScale = Vector3.one * 3f;

            // Control panel interactable
            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "ControlPanel";
            panel.transform.SetParent(_triggerRoom.transform);
            panel.transform.localPosition = new Vector3(0f, -1f, 4f);
            panel.transform.localScale = new Vector3(2f, 1f, 0.2f);

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
                // Fallback: create simple visual
                segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"RailSegment_{index}";
                segment.transform.position = railNodePoints[index];
                segment.transform.localScale = new Vector3(2f, 0.2f, 20f);
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
}
