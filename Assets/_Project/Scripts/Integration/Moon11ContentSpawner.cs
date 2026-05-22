using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 11: SPECTRAL MOON — "The Liberation of Releasing"
    /// Planetary fountain chain restoration + ancient aquifer purification.
    /// Ionized mist network heals structures, NPCs, and Echo remnants. 85% grid completion.
    /// Discovery: corrupted aquifer beneath oldest star fort, source of all pure water.
    /// </summary>
    public class Moon11ContentSpawner : MonoBehaviour
    {
        [Header("Moon 11 State")]
        [SerializeField] bool moon11Unlocked;
        [SerializeField] bool aquiferPurified;

        [Header("Fountain Network")]
        [SerializeField] int totalFountains = 10;  // 10 planetary fountains
        [SerializeField] int totalAquiferNodes = 5;  // 5 aquifer purification nodes
        int _fountainsActivated;
        int _aquiferNodesPurified;

        [Header("Memory Echoes")]
        [SerializeField] GameObject memoryEchoSystemPrefab;
        MemoryEchoSystem _memoryEchoSystem;

        [Header("Spawning")]
        [SerializeField] Vector3 aquiferCenterPoint = new(0f, -20f, 0f);  // Deep underground
        [SerializeField] Vector3[] fountainPoints;  // 10 surface fountain locations
        [SerializeField] Vector3[] aquiferNodePoints;  // 5 underground nodes
        [SerializeField] GameObject fountainPrefab;  // Pure water fountain

        readonly List<GameObject> _fountains = new();
        readonly List<GameObject> _aquiferNodes = new();
        GameObject _aquiferCore;
        bool _contentSpawned;
        bool _aquiferDiscovered;

        public bool IsMoon11Active => moon11Unlocked && !aquiferPurified;
        public int FountainProgress => _fountainsActivated;
        public float CompletionPercent => (_fountainsActivated + _aquiferNodesPurified) / (float)(totalFountains + totalAquiferNodes);

        void Awake()
        {
            // Check save state
            moon11Unlocked = SaveManager.Instance?.GetMoonProgress(11) > 0f;
        }

        void Start()
        {
            if (moon11Unlocked && !_contentSpawned)
            {
                SpawnMoon11Content();
            }
        }

        public void UnlockMoon11()
        {
            if (moon11Unlocked) return;

            moon11Unlocked = true;
            SaveManager.Instance?.SetMoonProgress(11, 5f);
            Debug.Log("[Moon 11] SPECTRAL MOON unlocked — Ancient aquifer stirs beneath the mud");

            SpawnMoon11Content();
        }

        void SpawnMoon11Content()
        {
            _contentSpawned = true;

            Debug.Log("[Moon 11] Spawning Ancient Aquifer Sanctum content");

            // Central aquifer core (corrupted)
            SpawnAquiferCore();

            // 5 aquifer purification nodes
            SpawnAquiferNodes();

            // 10 surface fountains (inactive until aquifer restored)
            SpawnSurfaceFountains();

            // Memory echo system (temporal visions)
            SpawnMemoryEchoSystem();

            // Ambient audio: corrupted water gurgling
            var aquiferAmbience = Audio.AudioManager.Instance?.PlayLoopingSFX("CorruptedAquifer", aquiferCenterPoint, 0.4f);
            if (aquiferAmbience != null)
            {
                Debug.Log("[Moon 11] Corrupted aquifer ambient active");
            }

            // Quest activation
            QuestManager.Instance?.ActivateQuest("moon11_aquifer_discovery");

            // Lirael dialogue
            DialogueManager.Instance?.PlayContextDialogue("lirael_aquifer_sensing");
        }

        void SpawnAquiferCore()
        {
            _aquiferCore = new GameObject("AquiferCore_Ancient");
            _aquiferCore.transform.position = aquiferCenterPoint;

            // Core chamber (large sphere)
            var chamber = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            chamber.name = "CoreChamber";
            chamber.transform.SetParent(_aquiferCore.transform);
            chamber.transform.localScale = Vector3.one * 20f;
            chamber.transform.localPosition = Vector3.zero;

            // Water source (inner sphere, corrupted black)
            var waterSource = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waterSource.name = "WaterSource";
            waterSource.transform.SetParent(_aquiferCore.transform);
            waterSource.transform.localScale = Vector3.one * 10f;
            waterSource.transform.localPosition = Vector3.zero;
            var renderer = waterSource.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);  // Dark corrupted water
            }

            // Interactable: purification console
            var console = GameObject.CreatePrimitive(PrimitiveType.Cube);
            console.name = "PurificationConsole";
            console.transform.SetParent(_aquiferCore.transform);
            console.transform.localPosition = new Vector3(0f, -8f, 0f);
            console.transform.localScale = new Vector3(3f, 1.5f, 3f);

            var interactable = console.AddComponent<AquiferConsole>();
            interactable.spawner = this;

            Debug.Log("[Moon 11] Aquifer core spawned (corrupted, awaiting purification)");
        }

        void SpawnAquiferNodes()
        {
            if (aquiferNodePoints == null || aquiferNodePoints.Length < totalAquiferNodes)
            {
                Debug.LogWarning($"[Moon 11] Not enough aquifer node points ({aquiferNodePoints?.Length ?? 0}/{totalAquiferNodes})");
                return;
            }

            for (int i = 0; i < totalAquiferNodes; i++)
            {
                var node = new GameObject($"AquiferNode_{i + 1}");
                node.transform.position = aquiferNodePoints[i];

                // Node crystal (corrupted)
                var crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crystal.name = "NodeCrystal";
                crystal.transform.SetParent(node.transform);
                crystal.transform.localScale = Vector3.one * 3f;
                crystal.transform.localPosition = Vector3.zero;
                var renderer = crystal.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.2f, 0.1f, 0.1f, 1f);  // Dark red corruption
                }

                // Interactable
                var interactable = crystal.AddComponent<AquiferNode>();
                interactable.spawner = this;
                interactable.nodeIndex = i;

                _aquiferNodes.Add(node);
            }

            Debug.Log($"[Moon 11] Spawned {totalAquiferNodes} corrupted aquifer nodes");
        }

        void SpawnSurfaceFountains()
        {
            if (fountainPoints == null || fountainPoints.Length < totalFountains)
            {
                Debug.LogWarning($"[Moon 11] Not enough fountain points ({fountainPoints?.Length ?? 0}/{totalFountains})");
                return;
            }

            for (int i = 0; i < totalFountains; i++)
            {
                GameObject fountain;
                if (fountainPrefab != null)
                {
                    fountain = Instantiate(fountainPrefab, fountainPoints[i], Quaternion.identity);
                    fountain.name = $"PureFountain_{i + 1}";
                }
                else
                {
                    // Fallback: create simple fountain visual
                    fountain = new GameObject($"PureFountain_{i + 1}");
                    fountain.transform.position = fountainPoints[i];

                    var basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    basin.name = "Basin";
                    basin.transform.SetParent(fountain.transform);
                    basin.transform.localScale = new Vector3(5f, 1f, 5f);
                    basin.transform.localPosition = Vector3.zero;

                    var spout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    spout.name = "Spout";
                    spout.transform.SetParent(fountain.transform);
                    spout.transform.localScale = new Vector3(0.5f, 3f, 0.5f);
                    spout.transform.localPosition = Vector3.up * 2f;
                }

                // Initially inactive
                fountain.SetActive(false);

                _fountains.Add(fountain);
            }

            Debug.Log($"[Moon 11] Spawned {totalFountains} surface fountains (inactive until aquifer purified)");
        }

        void SpawnMemoryEchoSystem()
        {
            // Create memory echo system
            var echoSystemObj = new GameObject("MemoryEchoSystem_Aquifer");
            echoSystemObj.transform.position = aquiferCenterPoint + Vector3.up * 20f;
            echoSystemObj.transform.SetParent(transform);

            if (memoryEchoSystemPrefab != null)
            {
                _memoryEchoSystem = Instantiate(memoryEchoSystemPrefab, echoSystemObj.transform).GetComponent<MemoryEchoSystem>();
            }
            else
            {
                _memoryEchoSystem = echoSystemObj.AddComponent<MemoryEchoSystem>();
            }

            // Configure echo locations (7 memory points around aquifer)
            var echoPoints = new Vector3[7];
            var echoDialogues = new string[7];
            for (int i = 0; i < 7; i++)
            {
                float angle = i * (360f / 7f) * Mathf.Deg2Rad;
                float radius = 30f;
                echoPoints[i] = aquiferCenterPoint + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Random.Range(-10f, 10f),
                    Mathf.Sin(angle) * radius
                );
                echoDialogues[i] = $"echo_aquifer_{i + 1}";
            }

            // Use reflection to set private fields (or make them public in MemoryEchoSystem)
            var pointsField = typeof(MemoryEchoSystem).GetField("echoPointLocations", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dialoguesField = typeof(MemoryEchoSystem).GetField("echoDialogueIds", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (pointsField != null) pointsField.SetValue(_memoryEchoSystem, echoPoints);
            if (dialoguesField != null) dialoguesField.SetValue(_memoryEchoSystem, echoDialogues);

            // Activate once aquifer nodes are purified
            if (_aquiferNodesPurified >= totalAquiferNodes)
            {
                _memoryEchoSystem?.ActivateSystem();
            }

            Debug.Log("[Moon 11] Memory echo system spawned — 7 temporal visions available after purification");
        }

        public void PurifyAquiferNode(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= totalAquiferNodes)
            {
                Debug.LogWarning($"[Moon 11] Invalid node index {nodeIndex}");
                return;
            }

            _aquiferNodesPurified++;

            Debug.Log($"[Moon 11] Aquifer node {nodeIndex + 1} purified — {_aquiferNodesPurified}/{totalAquiferNodes} complete");

            // Visual change: node turns blue (pure water)
            if (_aquiferNodes[nodeIndex] != null)
            {
                var crystal = _aquiferNodes[nodeIndex].transform.Find("NodeCrystal");
                if (crystal != null)
                {
                    var renderer = crystal.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = new Color(0.2f, 0.6f, 1f, 1f);  // Blue pure water
                    }
                }
            }

            // Play purification sound
            Audio.AudioManager.Instance?.PlaySFX2D("AquiferPurification");

            // Update quest progress
            QuestManager.Instance?.ProgressObjective("moon11_aquifer_purification", 0, 1);

            // Check if all nodes purified
            if (_aquiferNodesPurified >= totalAquiferNodes)
            {
                PurifyAquiferCore();
            }
        }

        void PurifyAquiferCore()
        {
            Debug.Log("[Moon 11] All aquifer nodes purified — core restoration unlocked");

            // Aquifer core visual change
            if (_aquiferCore != null)
            {
                var waterSource = _aquiferCore.transform.Find("WaterSource");
                if (waterSource != null)
                {
                    var renderer = waterSource.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = new Color(0.3f, 0.7f, 1f, 0.7f);  // Bright blue pure water
                    }
                }
            }

            // Activate all surface fountains
            ActivateAllFountains();

            // Activate memory echo system
            if (_memoryEchoSystem != null)
            {
                _memoryEchoSystem.ActivateSystem();
                Debug.Log("[Moon 11] Memory echo visions now accessible — witness the aquifer's history");
            }

            // Dialogue
            DialogueManager.Instance?.PlayContextDialogue("lirael_water_remembers");
        }

        void ActivateAllFountains()
        {
            for (int i = 0; i < _fountains.Count; i++)
            {
                if (_fountains[i] != null)
                {
                    _fountains[i].SetActive(true);
                    _fountainsActivated++;

                    // Spawn ionized mist VFX
                    var mist = new GameObject($"IonizedMist_{i}");
                    mist.transform.position = _fountains[i].transform.position + Vector3.up * 3f;
                    var particles = mist.AddComponent<ParticleSystem>();
                    var main = particles.main;
                    main.startColor = new Color(0.8f, 0.9f, 1f, 0.5f);  // Pale blue mist
                    main.startSize = 2f;
                    main.startLifetime = 5f;
                    main.maxParticles = 500;
                }
            }

            Debug.Log($"[Moon 11] ALL {_fountainsActivated} fountains activated — planetary ionized mist network online");

            // Play activation sound
            Audio.AudioManager.Instance?.PlaySFX2D("FountainChainActivation");

            CompleteMoon11();
        }

        void CompleteMoon11()
        {
            if (aquiferPurified) return;

            aquiferPurified = true;

            Debug.Log("[Moon 11] SPECTRAL MOON complete — Planetary fountain chain restored!");

            // Completion VFX: continent-wide aurora veils
            var vfx = new GameObject("Moon11_AuroraVeils");
            vfx.transform.position = aquiferCenterPoint + Vector3.up * 50f;
            var particles = vfx.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.4f, 0.8f, 1f, 0.6f);  // Aurora blue
            main.startSize = 50f;
            main.startLifetime = 15f;
            main.maxParticles = 5000;

            // Quest complete
            QuestManager.Instance?.CompleteQuest("moon11_fountain_chain_complete");

            // Unlock Moon 12
            SaveManager.Instance?.SetMoonProgress(11, 100f);

            // Revelation dialogue: Thorne airship view
            DialogueManager.Instance?.PlayContextDialogue("thorne_kairos_moment");

            // Prophecy Stone 10-11 appear
            Debug.Log("[Moon 11] Prophecy Stones 10-11 (Healing, Warning) now accessible");

            // Achievement
            AchievementSystem.Instance?.Unlock("planetary_fountain_restoration");
        }

        /// <summary>
        /// Aquifer Console interactable — restore core after nodes purified
        /// </summary>
        public class AquiferConsole : MonoBehaviour, IInteractable
        {
            public Moon11ContentSpawner spawner;
            bool _coreRestored;

            public string GetInteractPrompt()
            {
                if (spawner == null) return "";
                if (_coreRestored) return "";
                if (spawner._aquiferNodesPurified < spawner.totalAquiferNodes)
                    return $"Purify all nodes first ({spawner._aquiferNodesPurified}/{spawner.totalAquiferNodes})";
                return "Hold [E] to Restore Aquifer Core";
            }

            public void Interact(GameObject interactor)
            {
                if (spawner == null || _coreRestored) return;
                if (spawner._aquiferNodesPurified < spawner.totalAquiferNodes) return;

                _coreRestored = true;

                Debug.Log("[AquiferConsole] Core restoration initiated — planetary fountain chain activating");

                // Already handled by PurifyAquiferCore()
            }
        }

        /// <summary>
        /// Aquifer Node interactable — purify with 6-band resonance
        /// </summary>
        public class AquiferNode : MonoBehaviour, IInteractable
        {
            public Moon11ContentSpawner spawner;
            public int nodeIndex;
            bool _purified;

            public string GetInteractPrompt()
            {
                return _purified ? "" : "Hold [E] to Purify Node (6-Band Resonance)";
            }

            public void Interact(GameObject interactor)
            {
                if (_purified || spawner == null) return;

                _purified = true;

                Debug.Log($"[AquiferNode] Node {nodeIndex + 1} purified with 6-band resonance");

                // Purify via spawner
                spawner.PurifyAquiferNode(nodeIndex);

                // VFX: purification wave
                var vfx = new GameObject("PurificationWave");
                vfx.transform.position = transform.position;
                var particles = vfx.AddComponent<ParticleSystem>();
                var main = particles.main;
                main.startColor = new Color(0.3f, 0.7f, 1f, 1f);  // Blue wave
                main.startSize = 5f;
                main.startLifetime = 2f;
                main.maxParticles = 200;
            }
        }
    }
}
