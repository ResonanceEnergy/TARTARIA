using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Save;
using Tartaria.UI;

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

            // Dialogue: children NPCs (from Moon 3) now junior engineers
            HUDController.Instance?.ShowObjective("The rails sing again. Connect the continent.");
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
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "PuzzlePlatform";
            platform.transform.SetParent(_orphanTrainPuzzle.transform);
            platform.transform.localPosition = Vector3.zero;
            platform.transform.localScale = new Vector3(10f, 0.5f, 10f);

            // 3 orphan children NPCs (from Moon 3)
            for (int i = 0; i < 3; i++)
            {
                GameObject childObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                childObj.name = $"OrphanEngineer_{i}";
                childObj.transform.SetParent(_orphanTrainPuzzle.transform);
                childObj.transform.localPosition = new Vector3(i * 2f - 2f, 1.5f, 2f);
                childObj.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);

                // Child material
                Renderer rend = childObj.GetComponent<Renderer>();
                rend.material.color = new Color(0.85f, 0.7f, 0.6f);

                OrphanEngineerNPC engineer = childObj.AddComponent<OrphanEngineerNPC>();
                engineer.engineerIndex = i;
                engineer.spawner = this;
            }

            // Puzzle console (resonance tuning mini-game)
            GameObject console = GameObject.CreatePrimitive(PrimitiveType.Cube);
            console.name = "PuzzleConsole";
            console.transform.SetParent(_orphanTrainPuzzle.transform);
            console.transform.localPosition = new Vector3(0f, 1f, -3f);
            console.transform.localScale = new Vector3(2f, 1.5f, 1f);

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

            GameObject bossObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bossObj.name = "RailLeviathan_Boss";
            bossObj.transform.position = leviathanSpawnPoint;
            bossObj.transform.localScale = new Vector3(8f, 15f, 8f);
            bossObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Horizontal

            // Boss material: dark metallic serpent
            Renderer rend = bossObj.GetComponent<Renderer>();
            rend.material.color = new Color(0.3f, 0.25f, 0.2f);

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

            UI.HUDController.Instance?.ShowDialogue($"Engineer: {_dialogues[engineerIndex]}");
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
        float _attackCooldown;
        int _currentPathIndex;
        float _moveSpeed = 8f;
        bool _isDefeated;

        void Start()
        {
            Debug.Log("[RailLeviathan] Boss engaged! HP: 5000");
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

        void SeismicTremor()
        {
            Debug.Log("[RailLeviathan] Seismic tremor! Ground shakes!");

            // Spawn shockwave VFX
            GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shockwave.name = "SeismicShockwave";
            shockwave.transform.position = transform.position;
            shockwave.transform.localScale = new Vector3(20f, 0.2f, 20f);

            Renderer rend = shockwave.GetComponent<Renderer>();
            rend.material.color = new Color(0.8f, 0.4f, 0.2f, 0.6f);

            // Expand shockwave
            StartCoroutine(ExpandShockwave(shockwave));

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

            // Notify spawner
            spawner?.OnLeviathanDefeated();

            // Destroy boss
            Destroy(gameObject, 1f);
        }
    }
}

