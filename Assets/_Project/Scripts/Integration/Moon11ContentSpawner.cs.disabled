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
            // Moon 11: 10 fountains + 5 aquifer purification nodes
            sd.SetMoonFlag(11, "fountainsActivated", _fountainsActivated);
            sd.SetMoonFlag(11, "aquiferNodesPurified", _aquiferNodesPurified);
            sd.SetMoonFlag(11, "aquiferPurified", aquiferPurified);
        }

        void OnLoad(SaveData sd)
        {
            // Restore Moon 11 state
            _fountainsActivated = sd.GetMoonFlag(11, "fountainsActivated", 0);
            _aquiferNodesPurified = sd.GetMoonFlag(11, "aquiferNodesPurified", 0);
            aquiferPurified = sd.GetMoonFlag(11, "aquiferPurified");

            Debug.Log($"[Moon 11] State loaded: fountains={_fountainsActivated}/{totalFountains}, nodes={_aquiferNodesPurified}/{totalAquiferNodes}");
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

            // Multi-part aquifer chamber structure - EACH LAYER IS 3-PART ASSEMBLY
            
            // OUTER CONTAINMENT SHELL (3 parts)
            var chamberOuter = new GameObject("ChamberOuter");
            chamberOuter.transform.SetParent(_aquiferCore.transform);
            chamberOuter.transform.localPosition = Vector3.zero;
            
            var outerShell = new GameObject("Shell");
            outerShell.transform.SetParent(chamberOuter.transform);
            outerShell.transform.localScale = Vector3.one * 22f;
            outerShell.transform.localPosition = Vector3.zero;
            outerShell.AddComponent<MeshFilter>();
            outerShell.AddComponent<MeshRenderer>();
            outerShell.AddComponent<SphereCollider>();
            
            var outerBandTop = new GameObject("BandTop");
            outerBandTop.transform.SetParent(chamberOuter.transform);
            outerBandTop.transform.localScale = new Vector3(22.5f, 0.8f, 22.5f);
            outerBandTop.transform.localPosition = Vector3.up * 8f;
            outerBandTop.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            outerBandTop.AddComponent<MeshFilter>();
            outerBandTop.AddComponent<MeshRenderer>();
            outerBandTop.AddComponent<CapsuleCollider>();
            
            var outerBandBot = new GameObject("BandBottom");
            outerBandBot.transform.SetParent(chamberOuter.transform);
            outerBandBot.transform.localScale = new Vector3(22.5f, 0.8f, 22.5f);
            outerBandBot.transform.localPosition = Vector3.down * 8f;
            outerBandBot.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            outerBandBot.AddComponent<MeshFilter>();
            outerBandBot.AddComponent<MeshRenderer>();
            outerBandBot.AddComponent<CapsuleCollider>();

            // MID-LAYER FILTRATION RING (3 parts)
            var chamberMid = new GameObject("ChamberMid");
            chamberMid.transform.SetParent(_aquiferCore.transform);
            chamberMid.transform.localPosition = Vector3.zero;
            
            var midShell = new GameObject("Shell");
            midShell.transform.SetParent(chamberMid.transform);
            midShell.transform.localScale = Vector3.one * 16f;
            midShell.transform.localPosition = Vector3.zero;
            midShell.AddComponent<MeshFilter>();
            midShell.AddComponent<MeshRenderer>();
            midShell.AddComponent<SphereCollider>();
            
            var midRingA = new GameObject("FilterRing1");
            midRingA.transform.SetParent(chamberMid.transform);
            midRingA.transform.localScale = new Vector3(16.5f, 0.5f, 16.5f);
            midRingA.transform.localPosition = Vector3.up * 5f;
            midRingA.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            midRingA.AddComponent<MeshFilter>();
            midRingA.AddComponent<MeshRenderer>();
            midRingA.AddComponent<CapsuleCollider>();
            
            var midRingB = new GameObject("FilterRing2");
            midRingB.transform.SetParent(chamberMid.transform);
            midRingB.transform.localScale = new Vector3(16.5f, 0.5f, 16.5f);
            midRingB.transform.localPosition = Vector3.down * 5f;
            midRingB.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            midRingB.AddComponent<MeshFilter>();
            midRingB.AddComponent<MeshRenderer>();
            midRingB.AddComponent<CapsuleCollider>();

            // INNER RESERVOIR (3 parts)
            var chamberInner = new GameObject("ChamberInner");
            chamberInner.transform.SetParent(_aquiferCore.transform);
            chamberInner.transform.localPosition = Vector3.zero;
            
            var innerShell = new GameObject("Shell");
            innerShell.transform.SetParent(chamberInner.transform);
            innerShell.transform.localScale = Vector3.one * 12f;
            innerShell.transform.localPosition = Vector3.zero;
            innerShell.AddComponent<MeshFilter>();
            innerShell.AddComponent<MeshRenderer>();
            innerShell.AddComponent<SphereCollider>();
            
            var innerCap1 = new GameObject("CapTop");
            innerCap1.transform.SetParent(chamberInner.transform);
            innerCap1.transform.localScale = new Vector3(4f, 6f, 4f);
            innerCap1.transform.localPosition = Vector3.up * 4f;
            innerCap1.AddComponent<MeshFilter>();
            innerCap1.AddComponent<MeshRenderer>();
            innerCap1.AddComponent<SphereCollider>();
            
            var innerCap2 = new GameObject("CapBottom");
            innerCap2.transform.SetParent(chamberInner.transform);
            innerCap2.transform.localScale = new Vector3(4f, 6f, 4f);
            innerCap2.transform.localPosition = Vector3.down * 4f;
            innerCap2.AddComponent<MeshFilter>();
            innerCap2.AddComponent<MeshRenderer>();
            innerCap2.AddComponent<SphereCollider>();

            // WATER SOURCE CORE - corrupted (3 parts)
            var waterSource = new GameObject("WaterSource");
            waterSource.transform.SetParent(_aquiferCore.transform);
            waterSource.transform.localPosition = Vector3.zero;
            
            var sourceCore = new GameObject("Core");
            sourceCore.transform.SetParent(waterSource.transform);
            sourceCore.transform.localScale = Vector3.one * 6f;
            sourceCore.transform.localPosition = Vector3.zero;
            sourceCore.AddComponent<MeshFilter>();
            var coreRend = sourceCore.AddComponent<MeshRenderer>();
            sourceCore.AddComponent<SphereCollider>();
            if (coreRend != null)
            {
                coreRend.material.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);  // Dark corrupted water
            }
            
            var sourcePulse1 = new GameObject("Pulse1");
            sourcePulse1.transform.SetParent(waterSource.transform);
            sourcePulse1.transform.localScale = Vector3.one * 5f;
            sourcePulse1.transform.localPosition = Vector3.zero;
            sourcePulse1.AddComponent<MeshFilter>();
            var pulse1Rend = sourcePulse1.AddComponent<MeshRenderer>();
            sourcePulse1.AddComponent<SphereCollider>();
            if (pulse1Rend != null)
            {
                pulse1Rend.material.color = new Color(0.15f, 0.05f, 0.05f, 0.5f);
            }
            
            var sourcePulse2 = new GameObject("Pulse2");
            sourcePulse2.transform.SetParent(waterSource.transform);
            sourcePulse2.transform.localScale = Vector3.one * 4f;
            sourcePulse2.transform.localPosition = Vector3.zero;
            sourcePulse2.AddComponent<MeshFilter>();
            var pulse2Rend = sourcePulse2.AddComponent<MeshRenderer>();
            sourcePulse2.AddComponent<SphereCollider>();
            if (pulse2Rend != null)
            {
                pulse2Rend.material.color = new Color(0.2f, 0.08f, 0.08f, 0.3f);
            }

            // Interactable: purification console (4-part control station)
            var console = new GameObject("PurificationConsole");
            console.transform.SetParent(_aquiferCore.transform);
            console.transform.localPosition = new Vector3(0f, -8f, 0f);
            
            var consoleBase = new GameObject("Base");
            consoleBase.transform.SetParent(console.transform);
            consoleBase.transform.localScale = new Vector3(3.5f, 0.3f, 3.5f);
            consoleBase.transform.localPosition = Vector3.zero;
            consoleBase.AddComponent<MeshFilter>();
            consoleBase.AddComponent<MeshRenderer>();
            consoleBase.AddComponent<CapsuleCollider>();
            
            var consoleMain = new GameObject("MainUnit");
            consoleMain.transform.SetParent(console.transform);
            consoleMain.transform.localScale = new Vector3(2.5f, 1.2f, 2.5f);
            consoleMain.transform.localPosition = Vector3.up * 0.8f;
            consoleMain.AddComponent<MeshFilter>();
            consoleMain.AddComponent<MeshRenderer>();
            consoleMain.AddComponent<BoxCollider>();
            
            var consoleScreen = new GameObject("Screen");
            consoleScreen.transform.SetParent(console.transform);
            consoleScreen.transform.localScale = new Vector3(1.8f, 1f, 0.2f);
            consoleScreen.transform.localPosition = new Vector3(0f, 1.5f, 1.3f);
            consoleScreen.transform.rotation = Quaternion.Euler(-15f, 0f, 0f);
            consoleScreen.AddComponent<MeshFilter>();
            consoleScreen.AddComponent<MeshRenderer>();
            consoleScreen.AddComponent<BoxCollider>();
            
            var consoleAntenna = new GameObject("Antenna");
            consoleAntenna.transform.SetParent(console.transform);
            consoleAntenna.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
            consoleAntenna.transform.localPosition = Vector3.up * 3f;
            consoleAntenna.AddComponent<MeshFilter>();
            consoleAntenna.AddComponent<MeshRenderer>();
            consoleAntenna.AddComponent<CapsuleCollider>();

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

                // Node crystal cluster (3-part formation - corrupted)
                var crystal = new GameObject("NodeCrystal");
                crystal.transform.SetParent(node.transform);
                crystal.transform.localPosition = Vector3.zero;
                
                // Main crystal
                var mainCrystal = new GameObject("MainCrystal");
                mainCrystal.transform.SetParent(crystal.transform);
                mainCrystal.transform.localScale = new Vector3(2f, 3.5f, 2f);
                mainCrystal.transform.localPosition = Vector3.zero;
                mainCrystal.AddComponent<MeshFilter>();
                var mainRend = mainCrystal.AddComponent<MeshRenderer>();
                mainCrystal.AddComponent<SphereCollider>();
                var light = mainCrystal.AddComponent<Light>();
                light.color = new Color(0.6f, 0.2f, 0.2f, 1f);  // Dim red corruption glow
                light.intensity = 0.8f;
                light.range = 8f;
                if (mainRend != null)
                {
                    mainRend.material.color = new Color(0.2f, 0.1f, 0.1f, 1f);  // Dark red corruption
                }
                
                // Side crystal 1
                var sideCrystal1 = new GameObject("SideCrystal1");
                sideCrystal1.transform.SetParent(crystal.transform);
                sideCrystal1.transform.localScale = new Vector3(1.2f, 2.2f, 1.2f);
                sideCrystal1.transform.localPosition = new Vector3(-1.5f, -0.5f, 0f);
                sideCrystal1.transform.rotation = Quaternion.Euler(0f, 0f, -25f);
                sideCrystal1.AddComponent<MeshFilter>();
                var side1Rend = sideCrystal1.AddComponent<MeshRenderer>();
                sideCrystal1.AddComponent<SphereCollider>();
                if (side1Rend != null)
                {
                    side1Rend.material.color = new Color(0.18f, 0.09f, 0.09f, 1f);
                }
                
                // Side crystal 2
                var sideCrystal2 = new GameObject("SideCrystal2");
                sideCrystal2.transform.SetParent(crystal.transform);
                sideCrystal2.transform.localScale = new Vector3(1f, 1.8f, 1f);
                sideCrystal2.transform.localPosition = new Vector3(1.2f, -0.8f, 0.5f);
                sideCrystal2.transform.rotation = Quaternion.Euler(0f, 0f, 20f);
                sideCrystal2.AddComponent<MeshFilter>();
                var side2Rend = sideCrystal2.AddComponent<MeshRenderer>();
                sideCrystal2.AddComponent<SphereCollider>();
                if (side2Rend != null)
                {
                    side2Rend.material.color = new Color(0.22f, 0.11f, 0.11f, 1f);
                }

                // Interactable (on main crystal)
                var interactable = mainCrystal.AddComponent<AquiferNode>();
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
                    // Multi-part fountain structure - EACH COMPONENT IS 3-PART ASSEMBLY (15 total)
                    fountain = new GameObject($"PureFountain_{i + 1}");
                    fountain.transform.position = fountainPoints[i];

                    // BASE PLATFORM (3 parts)
                    var basePlatform = new GameObject("Base");
                    basePlatform.transform.SetParent(fountain.transform);
                    basePlatform.transform.localPosition = Vector3.up * 0.25f;
                    
                    var baseRing = new GameObject("Ring");
                    baseRing.transform.SetParent(basePlatform.transform);
                    baseRing.transform.localScale = new Vector3(6.5f, 0.3f, 6.5f);
                    baseRing.transform.localPosition = Vector3.zero;
                    baseRing.AddComponent<MeshFilter>();
                    baseRing.AddComponent<MeshRenderer>();
                    baseRing.AddComponent<CapsuleCollider>();
                    
                    var basePlat = new GameObject("Platform");
                    basePlat.transform.SetParent(basePlatform.transform);
                    basePlat.transform.localScale = new Vector3(6f, 0.4f, 6f);
                    basePlat.transform.localPosition = Vector3.up * 0.35f;
                    basePlat.AddComponent<MeshFilter>();
                    basePlat.AddComponent<MeshRenderer>();
                    basePlat.AddComponent<CapsuleCollider>();
                    
                    var baseEdge = new GameObject("Edge");
                    baseEdge.transform.SetParent(basePlatform.transform);
                    baseEdge.transform.localScale = new Vector3(5.5f, 0.2f, 5.5f);
                    baseEdge.transform.localPosition = Vector3.up * 0.6f;
                    baseEdge.AddComponent<MeshFilter>();
                    baseEdge.AddComponent<MeshRenderer>();
                    baseEdge.AddComponent<CapsuleCollider>();

                    // BASIN (3 parts)
                    var basin = new GameObject("Basin");
                    basin.transform.SetParent(fountain.transform);
                    basin.transform.localPosition = Vector3.up * 1f;
                    
                    var basinBase = new GameObject("Base");
                    basinBase.transform.SetParent(basin.transform);
                    basinBase.transform.localScale = new Vector3(4.5f, 0.8f, 4.5f);
                    basinBase.transform.localPosition = Vector3.zero;
                    basinBase.AddComponent<MeshFilter>();
                    basinBase.AddComponent<MeshRenderer>();
                    basinBase.AddComponent<CapsuleCollider>();
                    
                    var basinRim = new GameObject("Rim");
                    basinRim.transform.SetParent(basin.transform);
                    basinRim.transform.localScale = new Vector3(4.7f, 0.3f, 4.7f);
                    basinRim.transform.localPosition = Vector3.up * 0.85f;
                    basinRim.AddComponent<MeshFilter>();
                    basinRim.AddComponent<MeshRenderer>();
                    basinRim.AddComponent<CapsuleCollider>();
                    
                    var basinLip = new GameObject("Lip");
                    basinLip.transform.SetParent(basin.transform);
                    basinLip.transform.localScale = new Vector3(4.3f, 0.2f, 4.3f);
                    basinLip.transform.localPosition = Vector3.up * 1.1f;
                    basinLip.AddComponent<MeshFilter>();
                    basinLip.AddComponent<MeshRenderer>();
                    basinLip.AddComponent<CapsuleCollider>();

                    // CENTRAL PILLAR (3 parts)
                    var pillar = new GameObject("Pillar");
                    pillar.transform.SetParent(fountain.transform);
                    pillar.transform.localPosition = Vector3.up * 2f;
                    
                    var pillarBase = new GameObject("Base");
                    pillarBase.transform.SetParent(pillar.transform);
                    pillarBase.transform.localScale = new Vector3(1f, 0.5f, 1f);
                    pillarBase.transform.localPosition = Vector3.down * 0.3f;
                    pillarBase.AddComponent<MeshFilter>();
                    pillarBase.AddComponent<MeshRenderer>();
                    pillarBase.AddComponent<CapsuleCollider>();
                    
                    var pillarShaft = new GameObject("Shaft");
                    pillarShaft.transform.SetParent(pillar.transform);
                    pillarShaft.transform.localScale = new Vector3(0.8f, 1.8f, 0.8f);
                    pillarShaft.transform.localPosition = Vector3.zero;
                    pillarShaft.AddComponent<MeshFilter>();
                    pillarShaft.AddComponent<MeshRenderer>();
                    pillarShaft.AddComponent<CapsuleCollider>();
                    
                    var pillarCapital = new GameObject("Capital");
                    pillarCapital.transform.SetParent(pillar.transform);
                    pillarCapital.transform.localScale = new Vector3(0.95f, 0.4f, 0.95f);
                    pillarCapital.transform.localPosition = Vector3.up * 1.5f;
                    pillarCapital.AddComponent<MeshFilter>();
                    pillarCapital.AddComponent<MeshRenderer>();
                    pillarCapital.AddComponent<CapsuleCollider>();

                    // SPOUT (3 parts)
                    var spout = new GameObject("Spout");
                    spout.transform.SetParent(fountain.transform);
                    spout.transform.localPosition = Vector3.up * 3.5f;
                    
                    var spoutBase = new GameObject("NozzleBase");
                    spoutBase.transform.SetParent(spout.transform);
                    spoutBase.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
                    spoutBase.transform.localPosition = Vector3.down * 0.5f;
                    spoutBase.AddComponent<MeshFilter>();
                    spoutBase.AddComponent<MeshRenderer>();
                    spoutBase.AddComponent<CapsuleCollider>();
                    
                    var spoutTube = new GameObject("Tube");
                    spoutTube.transform.SetParent(spout.transform);
                    spoutTube.transform.localScale = new Vector3(0.5f, 1.2f, 0.5f);
                    spoutTube.transform.localPosition = Vector3.zero;
                    spoutTube.AddComponent<MeshFilter>();
                    spoutTube.AddComponent<MeshRenderer>();
                    spoutTube.AddComponent<CapsuleCollider>();
                    
                    var spoutTip = new GameObject("Tip");
                    spoutTip.transform.SetParent(spout.transform);
                    spoutTip.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
                    spoutTip.transform.localPosition = Vector3.up * 1f;
                    spoutTip.AddComponent<MeshFilter>();
                    spoutTip.AddComponent<MeshRenderer>();
                    spoutTip.AddComponent<CapsuleCollider>();

                    // WATER ORB (3 parts - core + shells)
                    var orb = new GameObject("WaterOrb");
                    orb.transform.SetParent(fountain.transform);
                    orb.transform.localPosition = Vector3.up * 4.5f;
                    
                    var orbCore = new GameObject("Core");
                    orbCore.transform.SetParent(orb.transform);
                    orbCore.transform.localScale = Vector3.one * 0.6f;
                    orbCore.transform.localPosition = Vector3.zero;
                    orbCore.AddComponent<MeshFilter>();
                    orbCore.AddComponent<MeshRenderer>();
                    orbCore.AddComponent<SphereCollider>();
                    var light = orbCore.AddComponent<Light>();
                    light.color = new Color(0.2f, 0.8f, 1f, 1f);  // Cyan glow
                    light.intensity = 1.5f;
                    light.range = 10f;
                    
                    var orbMid = new GameObject("MidShell");
                    orbMid.transform.SetParent(orb.transform);
                    orbMid.transform.localScale = Vector3.one * 0.75f;
                    orbMid.transform.localPosition = Vector3.zero;
                    orbMid.AddComponent<MeshFilter>();
                    orbMid.AddComponent<MeshRenderer>();
                    orbMid.AddComponent<SphereCollider>();
                    
                    var orbOuter = new GameObject("Mist");
                    orbOuter.transform.SetParent(orb.transform);
                    orbOuter.transform.localScale = Vector3.one * 0.9f;
                    orbOuter.transform.localPosition = Vector3.zero;
                    orbOuter.AddComponent<MeshFilter>();
                    orbOuter.AddComponent<MeshRenderer>();
                    orbOuter.AddComponent<SphereCollider>();
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

            // Configure echo locations (13 memory points - complete aquifer history)
            // 13 echoes represent 13 moons of the calendar, full temporal cycle
            var echoPoints = new Vector3[13];
            var echoDialogues = new string[13];
            for (int i = 0; i < 13; i++)
            {
                float angle = i * (360f / 13f) * Mathf.Deg2Rad;
                // Two rings: inner (7) + outer (6) for spatial variety
                float radius = (i < 7) ? 25f : 40f;
                float heightOffset = Random.Range(-15f, 15f);
                echoPoints[i] = aquiferCenterPoint + new Vector3(
                    Mathf.Cos(angle) * radius,
                    heightOffset,
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

            Debug.Log("[Moon 11] Memory echo system spawned — 13 temporal visions (complete aquifer history) available after purification");
            Debug.Log("[Moon 11] 13 Echoes represent: Giant water rituals, pre-Flood golden age, corruption moment, 200 years of mud sleep");
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
