using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Save;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12: CRYSTAL MOON — "The Cooperation of Dedicating"
    /// Planetary bell tower network synchronization + final global grid push to 95%.
    /// All companions participate. 12 towers across 12 continents ringing in unison.
    /// Discovery: the bells are the original voice of the cosmos, predating language and giants.
    /// </summary>
    public class Moon12ContentSpawner : MonoBehaviour
    {
        [Header("Moon 12 State")]
        [SerializeField] bool moon12Unlocked;
        [SerializeField] bool bellNetworkSynchronized;

        [Header("Bell Tower Network")]
        [SerializeField] int totalBellTowers = 12;  // 12 continental towers
        int _towersSynchronized;
        bool _resetAssaultActive;
        bool _planetaryRingTriggered;

        [Header("Cymatic Tuning")]
        readonly List<CymaticTuningPuzzle> _tuningPuzzles = new();

        [Header("Spawning")]
        [SerializeField] Vector3[] bellTowerPoints;  // 12 tower locations across zones
        [SerializeField] GameObject bellTowerPrefab;  // Bell tower structure

        readonly List<GameObject> _bellTowers = new();
        bool _contentSpawned;

        public bool IsMoon12Active => moon12Unlocked && !bellNetworkSynchronized;
        public int TowerProgress => _towersSynchronized;
        public float CompletionPercent => _towersSynchronized / (float)totalBellTowers;

        void Awake()
        {
            // Check save state
            moon12Unlocked = SaveManager.Instance?.GetMoonProgress(12) > 0f;

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
            // Moon 12: 12 bell towers synchronized
            sd.SetMoonFlag(12, "towersSynchronized", _towersSynchronized);
            sd.SetMoonFlag(12, "bellNetworkSynchronized", bellNetworkSynchronized);
            sd.SetMoonFlag(12, "resetAssaultActive", _resetAssaultActive);
            sd.SetMoonFlag(12, "planetaryRingTriggered", _planetaryRingTriggered);
        }

        void OnLoad(SaveData sd)
        {
            // Restore Moon 12 state
            _towersSynchronized = sd.GetMoonFlag(12, "towersSynchronized", 0);
            bellNetworkSynchronized = sd.GetMoonFlag(12, "bellNetworkSynchronized");
            _resetAssaultActive = sd.GetMoonFlag(12, "resetAssaultActive");
            _planetaryRingTriggered = sd.GetMoonFlag(12, "planetaryRingTriggered");

            Debug.Log($"[Moon 12] State loaded: towers={_towersSynchronized}/{totalBellTowers}, synchronized={bellNetworkSynchronized}");
        }

        void Start()
        {
            if (moon12Unlocked && !_contentSpawned)
            {
                SpawnMoon12Content();
            }
        }

        public void UnlockMoon12()
        {
            if (moon12Unlocked) return;

            moon12Unlocked = true;
            SaveManager.Instance?.SetMoonProgress(12, 5f);
            Debug.Log("[Moon 12] CRYSTAL MOON unlocked — 12 bell towers await cosmic synchronization");

            SpawnMoon12Content();
        }

        void SpawnMoon12Content()
        {
            _contentSpawned = true;

            Debug.Log("[Moon 12] Spawning Planetary Bell Network content");

            // 12 bell towers across continents
            SpawnBellTowers();

            // Ambient audio: distant bell echoes
            HUDController.Instance?.ShowObjective("Synchronize all 12 towers. The planet will sing.");

            // Quest activation
            QuestManager.Instance?.ActivateQuest("moon12_bell_synchronization");

            // Korath dialogue
            DialogueManager.Instance?.PlayContextDialogue("korath_bells_were_first");
        }

        void SpawnBellTowers()
        {
            if (bellTowerPoints == null || bellTowerPoints.Length < totalBellTowers)
            {
                Debug.LogWarning($"[Moon 12] Not enough bell tower points ({bellTowerPoints?.Length ?? 0}/{totalBellTowers})");
                return;
            }

            for (int i = 0; i < totalBellTowers; i++)
            {
                GameObject tower;
                if (bellTowerPrefab != null)
                {
                    tower = Instantiate(bellTowerPrefab, bellTowerPoints[i], Quaternion.identity);
                    tower.name = $"BellTower_{i + 1}";
                }
                else
                {
                    // Fallback: create simple tower visual
                    tower = new GameObject($"BellTower_{i + 1}");
                    tower.transform.position = bellTowerPoints[i];

                    // Tower structure (tall cylinder)
                    var structure = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    structure.name = "TowerStructure";
                    structure.transform.SetParent(tower.transform);
                    structure.transform.localScale = new Vector3(5f, 30f, 5f);
                    structure.transform.localPosition = Vector3.up * 30f;

                    // Bell (sphere at top)
                    var bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    bell.name = "Bell";
                    bell.transform.SetParent(tower.transform);
                    bell.transform.localScale = Vector3.one * 6f;
                    bell.transform.localPosition = Vector3.up * 60f;

                    // Tuning console
                    var console = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    console.name = "TuningConsole";
                    console.transform.SetParent(tower.transform);
                    console.transform.localScale = new Vector3(2f, 1f, 2f);
                    console.transform.localPosition = Vector3.up * 2f;

                    var interactable = console.AddComponent<BellTowerConsole>();
                    interactable.spawner = this;
                    interactable.towerIndex = i;

                    // Add cymatic tuning puzzle to each tower
                    var tuningPuzzle = tower.AddComponent<CymaticTuningPuzzle>();
                    _tuningPuzzles.Add(tuningPuzzle);
                }

                _bellTowers.Add(tower);
            }

            Debug.Log($"[Moon 12] Spawned {totalBellTowers} bell towers across 12 continents");
        }

        public void SynchronizeTower(int towerIndex)
        {
            if (towerIndex < 0 || towerIndex >= totalBellTowers)
            {
                Debug.LogWarning($"[Moon 12] Invalid tower index {towerIndex}");
                return;
            }

            // Check if cymatic puzzle is solved for this tower
            if (towerIndex < _tuningPuzzles.Count)
            {
                var puzzle = _tuningPuzzles[towerIndex];
                if (puzzle != null && !puzzle.IsSolved)
                {
                    Debug.Log($"[Moon 12] Tower {towerIndex + 1} requires cymatic tuning first");
                    puzzle.ActivatePuzzle();
                    return;
                }
            }

            _towersSynchronized++;

            Debug.Log($"[Moon 12] Bell tower {towerIndex + 1} synchronized — {_towersSynchronized}/{totalBellTowers} complete");

            // Play bell tone at tuned frequency
            Audio.AudioManager.Instance?.PlayTone(432f * (1 + towerIndex * 0.05f), 3f, 0.5f);

            // Visual: tower glows golden
            if (_bellTowers[towerIndex] != null)
            {
                var bell = _bellTowers[towerIndex].transform.Find("Bell");
                if (bell != null)
                {
                    var renderer = bell.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = new Color(1f, 0.9f, 0.4f, 1f);  // Golden glow
                    }
                }
            }

            // Update quest progress
            QuestManager.Instance?.ProgressObjective("moon12_bell_synchronization", 0, 1);

            // Trigger Reset assault at 8/12 towers
            if (_towersSynchronized == 8 && !_resetAssaultActive)
            {
                TriggerResetAssault();
            }

            // Trigger planetary ring at 12/12 towers
            if (_towersSynchronized >= totalBellTowers && !_planetaryRingTriggered)
            {
                TriggerPlanetaryRing();
            }
        }

        void TriggerResetAssault()
        {
            _resetAssaultActive = true;

            Debug.Log("[Moon 12] Reset Agents launch coordinated global assault — defend the bell towers!");

            // Spawn Reset enemies at multiple towers
            for (int i = 0; i < 4; i++)
            {
                int targetTower = Random.Range(0, totalBellTowers);
                SpawnResetSquad(targetTower);
            }

            // Dialogue
            DialogueManager.Instance?.PlayContextDialogue("reset_commander_final_assault");

            // Quest update
            QuestManager.Instance?.ActivateQuest("moon12_defend_bell_network");

            HUDController.Instance?.ShowObjective("Defend bell towers from Reset assault!");
        }

        void SpawnResetSquad(int towerIndex)
        {
            if (towerIndex < 0 || towerIndex >= _bellTowers.Count || _bellTowers[towerIndex] == null)
                return;

            var spawnPoint = _bellTowers[towerIndex].transform.position + Vector3.right * 20f;

            // Spawn 3 Reset agents (placeholder)
            for (int i = 0; i < 3; i++)
            {
                var agent = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                agent.name = $"ResetAgent_Tower{towerIndex}_{i}";
                agent.transform.position = spawnPoint + Vector3.forward * (i * 3f);
                agent.transform.localScale = new Vector3(1f, 2f, 1f);

                // Red color to indicate enemy
                var renderer = agent.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }

            Debug.Log($"[Moon 12] Reset squad spawned at tower {towerIndex + 1}");
        }

        void TriggerPlanetaryRing()
        {
            _planetaryRingTriggered = true;

            Debug.Log("[Moon 12] PLANETARY RING — All 12 bell towers ringing simultaneously!");

            // Play all bell tones in harmony
            for (int i = 0; i < totalBellTowers; i++)
            {
                float frequency = 432f * (1 + i * 0.05f);
                Audio.AudioManager.Instance?.PlayTone(frequency, 60f, 0.6f);  // 60 second ring
            }

            // Visual: golden scalar waves across planet
            for (int i = 0; i < totalBellTowers; i++)
            {
                if (_bellTowers[i] != null)
                {
                    var wave = new GameObject($"ScalarWave_Tower{i}");
                    wave.transform.position = _bellTowers[i].transform.position + Vector3.up * 60f;
                    var particles = wave.AddComponent<ParticleSystem>();
                    var main = particles.main;
                    main.startColor = new Color(1f, 0.9f, 0.4f, 0.8f);  // Golden
                    main.startSize = 100f;
                    main.startLifetime = 60f;
                    main.maxParticles = 10000;
                }
            }

            // Auroras fill sky
            var aurora = new GameObject("PlanetaryAurora");
            aurora.transform.position = Vector3.up * 500f;
            var auroraParticles = aurora.AddComponent<ParticleSystem>();
            var auroraMain = auroraParticles.main;
            auroraMain.startColor = new Color(0.5f, 0.9f, 0.6f, 0.7f);  // Aurora green
            auroraMain.startSize = 200f;
            auroraMain.startLifetime = 60f;
            auroraMain.maxParticles = 50000;

            Debug.Log("[Moon 12] The most beautiful minute in the game — planetary resonance peak");

            // Dialogue: Korath's echo in the bells
            DialogueManager.Instance?.PlayContextDialogue("korath_feel_dawn_again");

            // Complete Moon 12 after 60 second ring
            Invoke(nameof(CompleteMoon12), 60f);
        }

        void CompleteMoon12()
        {
            if (bellNetworkSynchronized) return;

            bellNetworkSynchronized = true;

            Debug.Log("[Moon 12] CRYSTAL MOON complete — Planetary bell network in perfect harmony!");

            // Quest complete
            QuestManager.Instance?.CompleteQuest("moon12_bell_network_synchronized");

            // Global grid hits 95%
            SaveManager.Instance?.SetMoonProgress(12, 100f);

            // Revelation: Final Prophecy Stone (#12) appears
            Debug.Log("[Moon 12] Final Prophecy Stone (#12: Stone of Promise) now accessible");
            Debug.Log("[Moon 12] Two shadows at edge of vision — one giant, two humans, shadow of doubt");

            // Dialogue: revelation
            DialogueManager.Instance?.PlayContextDialogue("moon12_prophecy_stone_promise");

            // Achievement
            AchievementSystem.Instance?.Unlock("planetary_bell_harmony");

            // Unlock Moon 13 (final)
            Debug.Log("[Moon 12] One more connection remains... The 13th Moon rises.");
        }

        /// <summary>
        /// Bell Tower Console interactable — tune and synchronize tower
        /// </summary>
        public class BellTowerConsole : MonoBehaviour, IInteractable
        {
            public Moon12ContentSpawner spawner;
            public int towerIndex;
            bool _synchronized;

            public string GetInteractPrompt()
            {
                if (_synchronized) return "";
                return $"Hold [E] to Synchronize Bell Tower {towerIndex + 1}";
            }

            public void Interact(GameObject interactor)
            {
                if (_synchronized || spawner == null) return;

                _synchronized = true;

                Debug.Log($"[BellTower] Tower {towerIndex + 1} tuning initiated — matching neighbor frequencies");

                // Synchronize via spawner
                spawner.SynchronizeTower(towerIndex);

                // Tuning minigame placeholder (would use organ/cymatic mechanics)
                Debug.Log($"[BellTower] Tuning complete — frequency locked at {432f * (1 + towerIndex * 0.05f)} Hz");
            }
        }
    }
}
