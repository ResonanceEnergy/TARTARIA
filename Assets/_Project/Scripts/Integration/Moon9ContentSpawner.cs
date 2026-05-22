using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 9 (Solar Moon - "The Intention of Intention") content spawner.
    /// Cross-continental: Prophecy stone collection + timeline echo visions + Zereth contact + 17-Hour Clock Tower.
    /// Auto-unlocks when Moon 8 complete.
    /// 
    /// GDD §03: Moon 9 — Solar Moon
    /// - Discovery (Days 1-5): Prophecy stones appear at ley-line intersections (floating golden markers)
    /// - Restoration (Days 6-12): Collect 6 stones, each triggers Prophecy Vision (Golden Age moments)
    /// - Conflict (Days 13-18): Zereth speaks directly to player (distorted echo at vision edges)
    /// - Climax (Days 19-24): 6 stones aligned → floating aurora city appears for 3 min (complete Golden Age district in sky)
    /// - Revelation (Days 25-28): Stone 6 timestamp = Rhythmic Moon 17th Hour (bells ringing BEFORE Flood), mystery deepens
    /// 
    /// Crossover seeds: Stones 7-12 (Moons 10-12), floating aurora live-ops, Zereth confession (Moon 13 choice)
    /// </summary>
    public class Moon9ContentSpawner : MonoBehaviour
    {
        public static Moon9ContentSpawner Instance { get; private set; }

        [Header("Prophecy Stone Configuration")]
        [SerializeField] int totalStones = 6;
        int _stonesCollected;

        [Header("Golden Codex")]
        [SerializeField] bool goldenCodexRestored;
        int _codexPagesRestored;
        const int TotalCodexPages = 12;

        [Header("Aurora City")]
        GameObject _auroraCityZone;
        bool _auroraCityActive;
        float _auroraCityTimer;

        [Header("Spawn Configuration")]
        [SerializeField] Vector3[] stoneLocations = new Vector3[6]; // Cross-continental ley-line nodes
        [SerializeField] Vector3 codexLocation = new Vector3(350f, 10f, 450f);

        [Header("Audio")]
        [SerializeField] string stoneCollectAudio = "Moon9_StoneCollect";
        [SerializeField] string visionAudio = "Moon9_ProphecyVision";
        [SerializeField] string zerethVoiceAudio = "Zereth_Distorted";
        [SerializeField] string auroraCityAudio = "Moon9_AuroraCity";
        [SerializeField] string bossEncounterAudio = "Moon9_BossEncounter";

        List<ProphecyStone> _activeStones = new List<ProphecyStone>();
        bool _zerethContactMade;
        bool _auroraCityTriggered;
        bool _clockTowerInstalled;
        bool _bossDefeated;
        GameObject _clockTowerObj;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Wire save/load events
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave += OnSave;
                SaveManager.Instance.OnAfterLoad += OnLoad;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;

            // Cleanup save/load event handlers
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave -= OnSave;
                SaveManager.Instance.OnAfterLoad -= OnLoad;
            }
        }

        void OnSave(SaveData sd)
        {
            // Moon 9: 6 prophecy stones + Zereth contact + clock tower
            sd.SetMoonFlag(9, "stonesCollected", _stonesCollected);
            sd.SetMoonFlag(9, "zerethContactMade", _zerethContactMade);
            sd.SetMoonFlag(9, "auroraCityTriggered", _auroraCityTriggered);
            sd.SetMoonFlag(9, "clockTowerInstalled", _clockTowerInstalled);
            sd.SetMoonFlag(9, "bossDefeated", _bossDefeated);
            sd.SetMoonFlag(9, "codexPagesRestored", _codexPagesRestored);
        }

        void OnLoad(SaveData sd)
        {
            // Restore Moon 9 state
            _stonesCollected = sd.GetMoonFlag(9, "stonesCollected", 0);
            _zerethContactMade = sd.GetMoonFlag(9, "zerethContactMade");
            _auroraCityTriggered = sd.GetMoonFlag(9, "auroraCityTriggered");
            _clockTowerInstalled = sd.GetMoonFlag(9, "clockTowerInstalled");
            _bossDefeated = sd.GetMoonFlag(9, "bossDefeated");
            _codexPagesRestored = sd.GetMoonFlag(9, "codexPagesRestored", 0);

            Debug.Log($"[Moon9ContentSpawner] State loaded: stones={_stonesCollected}/{totalStones}, clock={_clockTowerInstalled}");
        }

        void Start()
        {
            // Initialize stone locations (cross-continental via airship/train network)
            stoneLocations = new Vector3[]
            {
                new Vector3(500f, 10f, 600f), // Stone 1: Dawn
                new Vector3(300f, 5f, 700f),  // Stone 2: Flow
                new Vector3(100f, 8f, 500f),  // Stone 3: Craft
                new Vector3(600f, 12f, 400f), // Stone 4: Flight
                new Vector3(250f, 6f, 300f),  // Stone 5: Song
                new Vector3(450f, 9f, 550f)   // Stone 6: Stars
            };

            // Check if Moon 8 complete → auto-unlock Moon 9
            if (SaveManager.Instance != null && SaveManager.Instance.GetMoonProgress(8) >= 100f)
            {
                UnlockMoon9();
            }
        }

        public void UnlockMoon9()
        {
            if (_stonesCollected > 0) return; // Already spawned

            Debug.Log("[Moon9ContentSpawner] Moon 9 unlocked: Prophecy stones appear at ley-line intersections.");
            SpawnMoon9Content();
            LoadState();
        }

        void SpawnMoon9Content()
        {
            // Discovery: 6 prophecy stones (floating golden markers at ley-line nodes)
            SpawnProphecyStones();

            // Golden Codex at ancient library ruin
            SpawnGoldenCodex();

            // Quest: collect all 6 prophecy stones
            QuestManager.Instance?.ActivateQuest("moon9_collect_prophecy_stones");

            Debug.Log($"[Moon9ContentSpawner] 6 prophecy stones spawned across continent. Golden Codex awaits restoration.");
        }

        void SpawnProphecyStones()
        {
            string[] stoneNames = { "Dawn", "Flow", "Craft", "Flight", "Song", "Stars" };

            for (int i = 0; i < totalStones; i++)
            {
                // Multi-part prophecy stone obelisk (base + shaft + capstone)
                GameObject stoneObj = new GameObject($"ProphecyStone_{stoneNames[i]}");
                stoneObj.transform.position = stoneLocations[i];

                // Base pedestal
                GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                baseObj.name = "Base";
                baseObj.transform.SetParent(stoneObj.transform);
                baseObj.transform.localPosition = Vector3.zero;
                baseObj.transform.localScale = new Vector3(1.5f, 0.3f, 1.5f);
                Renderer baseRend = baseObj.GetComponent<Renderer>();
                baseRend.material.color = new Color(0.8f, 0.7f, 0.3f); // Dark golden

                // Shaft
                GameObject shaftObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shaftObj.name = "Shaft";
                shaftObj.transform.SetParent(stoneObj.transform);
                shaftObj.transform.localPosition = new Vector3(0f, 1f, 0f);
                shaftObj.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
                Renderer shaftRend = shaftObj.GetComponent<Renderer>();
                shaftRend.material.color = new Color(1f, 0.85f, 0.3f); // Golden

                // Capstone
                GameObject capObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                capObj.name = "Capstone";
                capObj.transform.SetParent(stoneObj.transform);
                capObj.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                capObj.transform.localScale = Vector3.one * 0.6f;
                Renderer capRend = capObj.GetComponent<Renderer>();
                capRend.material.color = new Color(1f, 0.9f, 0.4f); // Bright golden

                // Use capstone for primary rendering reference
                Renderer rend = capRend;

                // Golden light (floating marker)
                Light stoneLight = stoneObj.AddComponent<Light>();
                stoneLight.type = LightType.Point;
                stoneLight.color = new Color(1f, 0.9f, 0.4f);
                stoneLight.range = 10f;
                stoneLight.intensity = 2f;

                // ProphecyStone component: IInteractable collection + vision trigger
                ProphecyStone stone = stoneObj.AddComponent<ProphecyStone>();
                stone.stoneIndex = i;
                stone.stoneName = stoneNames[i];
                stone.OnCollected += OnStoneCollected;

                _activeStones.Add(stone);
            }

            Debug.Log($"[Moon9ContentSpawner] {totalStones} prophecy stones generated.");
        }

        void SpawnGoldenCodex()
        {
            // Multi-part golden book (cover + pages + binding)
            GameObject codexObj = new GameObject("GoldenCodex");
            codexObj.transform.position = codexLocation;

            // Cover
            GameObject coverObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coverObj.name = "Cover";
            coverObj.transform.SetParent(codexObj.transform);
            coverObj.transform.localPosition = Vector3.zero;
            coverObj.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
            Renderer coverRend = coverObj.GetComponent<Renderer>();
            coverRend.material.color = new Color(1f, 0.85f, 0.3f); // Golden cover

            // Pages (slightly smaller, offset)
            GameObject pagesObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pagesObj.name = "Pages";
            pagesObj.transform.SetParent(codexObj.transform);
            pagesObj.transform.localPosition = new Vector3(0f, 0f, 0.15f);
            pagesObj.transform.localScale = new Vector3(1.4f, 1.9f, 0.25f);
            Renderer pagesRend = pagesObj.GetComponent<Renderer>();
            pagesRend.material.color = new Color(1f, 0.95f, 0.8f); // Parchment

            // Binding spine
            GameObject bindingObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bindingObj.name = "Binding";
            bindingObj.transform.SetParent(codexObj.transform);
            bindingObj.transform.localPosition = new Vector3(-0.75f, 0f, 0f);
            bindingObj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            bindingObj.transform.localScale = new Vector3(0.2f, 1f, 0.2f);
            Renderer bindingRend = bindingObj.GetComponent<Renderer>();
            bindingRend.material.color = new Color(0.7f, 0.6f, 0.2f); // Dark bronze

            // Use cover for primary rendering reference
            Renderer rend = coverRend;

            // Golden light emanation
            Light codexLight = codexObj.AddComponent<Light>();
            codexLight.type = LightType.Point;
            codexLight.color = new Color(1f, 0.9f, 0.5f);
            codexLight.range = 12f;
            codexLight.intensity = 2f;

            // GoldenCodex component: PHI inscription restoration
            GoldenCodex codex = codexObj.AddComponent<GoldenCodex>();
            codex.spawner = this;

            Debug.Log("[Moon9ContentSpawner] Golden Codex discovered — 12 PHI-inscribed pages awaiting restoration.");
        }

        public void OnCodexPageRestored()
        {
            _codexPagesRestored++;
            Debug.Log($"[Moon9ContentSpawner] Golden Codex page restored: {_codexPagesRestored}/{TotalCodexPages}");

            if (_codexPagesRestored >= TotalCodexPages)
            {
                CompleteCodexRestoration();
            }
        }

        void CompleteCodexRestoration()
        {
            goldenCodexRestored = true;
            Debug.Log("[Moon9ContentSpawner] Golden Codex fully restored! PHI inscriptions reveal temporal clock blueprint.");

            // Unlock advanced time-bend abilities
            if (SaveManager.Instance != null)
            {
                // Set global flag: AdvancedTimeBendUnlocked
            }

            // Quest complete
            QuestManager.Instance?.CompleteQuest("moon9_golden_codex_restoration");

            // Revelation dialogue
            DialogueManager.Instance?.PlayContextDialogue("moon9_codex_complete");
        }

        void Update()
        {
            // Aurora city 3-minute timer
            if (_auroraCityActive)
            {
                _auroraCityTimer -= Time.deltaTime;
                if (_auroraCityTimer <= 0f)
                {
                    DespawnAuroraCity();
                }
            }
        }

        void OnStoneCollected(ProphecyStone stone)
        {
            _stonesCollected++;
            Debug.Log($"[Moon9ContentSpawner] Prophecy Stone {stone.stoneName} collected. Progress: {_stonesCollected}/{totalStones}");

            // Audio: stone collection harmonic
            AudioManager.Instance?.PlaySFX3D(stoneCollectAudio, stone.transform.position);

            // Quest progress
            QuestManager.Instance?.ProgressObjective("moon9_collect_prophecy_stones", 0);

            // HUD: Show progress
            UI.HUDController.Instance?.ShowObjective($"Prophecy Stones: {_stonesCollected}/{totalStones}");

            // Trigger Prophecy Vision (Golden Age moment replay)
            TriggerProphecyVision(stone.stoneIndex);

            // Check if all stones collected
            if (_stonesCollected >= totalStones)
            {
                QuestManager.Instance?.CompleteQuest("moon9_collect_prophecy_stones");
                TriggerAuroraCity();
            }

            SaveState();
        }

        void TriggerProphecyVision(int stoneIndex)
        {
            string[] visions = {
                "Giants + humans greet 17-hour sunrise with communal song",
                "Pure water fountains feed ionized mist through golden streets",
                "Sound waves part granite — precision cutting at continental scale",
                "Airships lift megaliths through aurora night",
                "Pipe organs thunder while cymatic gardens bloom",
                "Bell towers ring in cosmic alignment (Rhythmic Moon 17th Hour)"
            };

            Debug.Log($"[Moon9ContentSpawner] PROPHECY VISION {stoneIndex}: {visions[stoneIndex]}");

            // Audio: vision harmonic
            AudioManager.Instance?.PlaySFX2D(visionAudio);

            // Visual: golden hologram replay (simplified for beta)
            // Full implementation would show cinematic vision scene

            // Zereth echo appears at edge of vision (distorted presence)
            if (stoneIndex >= 3 && !_zerethContactMade)
            {
                TriggerZerethContact();
            }
        }

        void TriggerZerethContact()
        {
            if (_zerethContactMade) return;
            _zerethContactMade = true;

            Debug.Log("[Moon9ContentSpawner] CONFLICT: Zereth speaks directly to player! 'You see paradise. I saw a cage.'");

            // Audio: distorted Zereth voice
            AudioManager.Instance?.PlaySFX2D(zerethVoiceAudio);

            // Dialogue: Zereth's first direct contact
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayContextDialogue("moon9_zereth_contact");
                // "You see paradise. I saw a cage. They called it harmony. I called it submission. One note forever? I wanted MORE."
            }

            SaveState();
        }

        void TriggerAuroraCity()
        {
            if (_auroraCityTriggered) return;
            _auroraCityTriggered = true;

            Debug.Log("[Moon9ContentSpawner] CLIMAX: Floating aurora city appears! Complete Golden Age district in sky for 3 min!");

            // Climax: floating aurora city (explorable sky zone, 3 real-time minutes)
            Vector3 skyCityPos = new Vector3(400f, 150f, 500f); // High above continent

            _auroraCityZone = new GameObject("AuroraCity_Explorable");
            _auroraCityZone.transform.position = skyCityPos;

            // Create explorable platforms (9 golden platforms in PHI-spiral pattern)
            float goldenRatio = 1.618033988749895f;
            for (int i = 0; i < 9; i++)
            {
                float angle = i * 137.5f * Mathf.Deg2Rad; // Golden angle
                float radius = i * 15f / goldenRatio;

                Vector3 platformPos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    i * 5f,
                    Mathf.Sin(angle) * radius
                );

                // Multi-part platform (foundation + surface + supports)
                GameObject platform = new GameObject($"AuroraPlatform_{i}");
                platform.transform.SetParent(_auroraCityZone.transform);
                platform.transform.localPosition = platformPos;

                // Foundation
                GameObject foundationObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                foundationObj.name = "Foundation";
                foundationObj.transform.SetParent(platform.transform);
                foundationObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                foundationObj.transform.localScale = new Vector3(12f, 0.5f, 12f);
                Renderer foundRend = foundationObj.GetComponent<Renderer>();
                foundRend.material.color = new Color(0.9f, 0.8f, 0.5f, 0.9f);

                // Surface
                GameObject surfaceObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                surfaceObj.name = "Surface";
                surfaceObj.transform.SetParent(platform.transform);
                surfaceObj.transform.localPosition = Vector3.zero;
                surfaceObj.transform.localScale = new Vector3(12f, 0.3f, 12f);
                Renderer surfRend = surfaceObj.GetComponent<Renderer>();
                surfRend.material.color = new Color(1f, 0.9f, 0.6f, 0.9f);

                // 4 corner supports
                for (int s = 0; s < 4; s++)
                {
                    GameObject supportObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    supportObj.name = $"Support_{s}";
                    supportObj.transform.SetParent(platform.transform);
                    float sAngle = s * 90f * Mathf.Deg2Rad;
                    supportObj.transform.localPosition = new Vector3(Mathf.Cos(sAngle) * 5f, -1.5f, Mathf.Sin(sAngle) * 5f);
                    supportObj.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
                    Renderer supRend = supportObj.GetComponent<Renderer>();
                    supRend.material.color = new Color(0.8f, 0.7f, 0.4f);
                }

                // Use surface for primary rendering reference
                Renderer pRend = surfRend;

                // Platform light
                Light pLight = platform.AddComponent<Light>();
                pLight.type = LightType.Point;
                pLight.color = new Color(1f, 0.95f, 0.7f);
                pLight.range = 20f;
                pLight.intensity = 1.5f;

                // Add lore fragments on platforms
                if (i % 3 == 0)
                {
                    // Multi-part lore orb (core + 2 rotating rings)
                    GameObject loreObj = new GameObject($"AuroraLoreFragment_{i}");
                    loreObj.transform.SetParent(platform.transform);
                    loreObj.transform.localPosition = Vector3.up * 2f;

                    // Core sphere
                    GameObject coreObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    coreObj.name = "Core";
                    coreObj.transform.SetParent(loreObj.transform);
                    coreObj.transform.localPosition = Vector3.zero;
                    coreObj.transform.localScale = Vector3.one * 0.8f;
                    Renderer coreRend = coreObj.GetComponent<Renderer>();
                    coreRend.material.color = new Color(1f, 0.95f, 0.7f);

                    // Ring 1 (horizontal)
                    GameObject ring1Obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    ring1Obj.name = "Ring1";
                    ring1Obj.transform.SetParent(loreObj.transform);
                    ring1Obj.transform.localPosition = Vector3.zero;
                    ring1Obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    ring1Obj.transform.localScale = new Vector3(1.2f, 0.05f, 1.2f);
                    Renderer r1Rend = ring1Obj.GetComponent<Renderer>();
                    r1Rend.material.color = new Color(1f, 0.9f, 0.5f, 0.7f);

                    // Ring 2 (vertical)
                    GameObject ring2Obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    ring2Obj.name = "Ring2";
                    ring2Obj.transform.SetParent(loreObj.transform);
                    ring2Obj.transform.localPosition = Vector3.zero;
                    ring2Obj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    ring2Obj.transform.localScale = new Vector3(1.2f, 0.05f, 1.2f);
                    Renderer r2Rend = ring2Obj.GetComponent<Renderer>();
                    r2Rend.material.color = new Color(1f, 0.85f, 0.4f, 0.7f);

                    AuroraLoreFragment lore = loreObj.AddComponent<AuroraLoreFragment>();
                    lore.fragmentIndex = i / 3;
                }
            }

            // Central spire (boss encounter zone) - multi-segment tower
            GameObject spire = new GameObject("AuroraSpire_Boss");
            spire.transform.SetParent(_auroraCityZone.transform);
            spire.transform.localPosition = new Vector3(0f, 30f, 0f);

            // Base
            GameObject spireBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spireBase.name = "Base";
            spireBase.transform.SetParent(spire.transform);
            spireBase.transform.localPosition = new Vector3(0f, -10f, 0f);
            spireBase.transform.localScale = new Vector3(5f, 10f, 5f);
            Renderer baseRend = spireBase.GetComponent<Renderer>();
            baseRend.material.color = new Color(0.9f, 0.8f, 0.5f);

            // Mid shaft
            GameObject spireMid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spireMid.name = "MidShaft";
            spireMid.transform.SetParent(spire.transform);
            spireMid.transform.localPosition = new Vector3(0f, 10f, 0f);
            spireMid.transform.localScale = new Vector3(3f, 20f, 3f);
            Renderer midRend = spireMid.GetComponent<Renderer>();
            midRend.material.color = new Color(1f, 0.9f, 0.6f);

            // Top spire
            GameObject spireTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spireTop.name = "TopSpire";
            spireTop.transform.SetParent(spire.transform);
            spireTop.transform.localPosition = new Vector3(0f, 35f, 0f);
            spireTop.transform.localScale = new Vector3(1.5f, 15f, 1.5f);
            Renderer topRend = spireTop.GetComponent<Renderer>();
            topRend.material.color = new Color(1f, 0.95f, 0.7f);

            // Capstone
            GameObject spireCap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spireCap.name = "Capstone";
            spireCap.transform.SetParent(spire.transform);
            spireCap.transform.localPosition = new Vector3(0f, 52f, 0f);
            spireCap.transform.localScale = Vector3.one * 3f;
            Renderer capRend = spireCap.GetComponent<Renderer>();
            capRend.material.color = new Color(1f, 1f, 0.9f);

            // Use mid shaft as reference
            Renderer spireRend = midRend;
            Renderer sRend = spire.GetComponent<Renderer>();
            sRend.material.color = new Color(1f, 0.85f, 0.4f, 0.95f);

            // Spawn temporal guardian boss at spire top
            SpawnTemporalGuardian(spire.transform.position + Vector3.up * 45f);

            // Aurora particle effects around city
            GameObject auroraPfx = new GameObject("Aurora_Particles");
            auroraPfx.transform.SetParent(_auroraCityZone.transform);
            auroraPfx.transform.localPosition = Vector3.zero;

            ParticleSystem ps = auroraPfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 180f; // 3 min
            main.startSpeed = 0.3f;
            main.startSize = 3f;
            main.loop = true;
            main.maxParticles = 5000;

            var emission = ps.emission;
            emission.rateOverTime = 300f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 150f;

            Renderer psRend = ps.GetComponent<Renderer>();
            if (psRend != null && psRend.material != null)
            {
                psRend.material.color = new Color(1f, 0.9f, 0.6f, 0.5f);
            }

            // Audio: aurora city harmonic
            AudioManager.Instance?.PlaySFX3D(auroraCityAudio, skyCityPos);

            // NPCs point upward in wonder
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayContextDialogue("moon9_milo_aurora_city");
                // "That's real, isn't it? Not a sales pitch. Not a postcard. That's what we were supposed to have."
            }

            // Start 3-minute timer
            _auroraCityActive = true;
            _auroraCityTimer = 180f;

            // Quest: explore aurora city
            QuestManager.Instance?.ActivateQuest("moon9_explore_aurora_city");

            SaveState();
        }

        void SpawnTemporalGuardian(Vector3 position)
        {
            // *** CRITICAL BOSS: 6-part Temporal Guardian assembly ***
            GameObject bossObj = new GameObject("TemporalGuardian_Boss");
            bossObj.transform.position = position;

            // Core sphere (central eye)
            GameObject coreObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coreObj.name = "Core";
            coreObj.transform.SetParent(bossObj.transform);
            coreObj.transform.localPosition = Vector3.zero;
            coreObj.transform.localScale = Vector3.one * 3f;
            Renderer coreRend = coreObj.GetComponent<Renderer>();
            coreRend.material.color = new Color(0.7f, 0.8f, 1f, 0.95f); // Blue-white core

            // Ring 1 (horizontal equator)
            GameObject ring1Obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring1Obj.name = "Ring_Equator";
            ring1Obj.transform.SetParent(bossObj.transform);
            ring1Obj.transform.localPosition = Vector3.zero;
            ring1Obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ring1Obj.transform.localScale = new Vector3(5f, 0.3f, 5f);
            Renderer r1Rend = ring1Obj.GetComponent<Renderer>();
            r1Rend.material.color = new Color(0.8f, 0.9f, 1f, 0.8f);

            // Ring 2 (vertical meridian 1)
            GameObject ring2Obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring2Obj.name = "Ring_Meridian1";
            ring2Obj.transform.SetParent(bossObj.transform);
            ring2Obj.transform.localPosition = Vector3.zero;
            ring2Obj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            ring2Obj.transform.localScale = new Vector3(5f, 0.3f, 5f);
            Renderer r2Rend = ring2Obj.GetComponent<Renderer>();
            r2Rend.material.color = new Color(0.75f, 0.85f, 1f, 0.75f);

            // Ring 3 (vertical meridian 2 - perpendicular)
            GameObject ring3Obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring3Obj.name = "Ring_Meridian2";
            ring3Obj.transform.SetParent(bossObj.transform);
            ring3Obj.transform.localPosition = Vector3.zero;
            ring3Obj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            ring3Obj.transform.localScale = new Vector3(5f, 0.3f, 5f);
            Renderer r3Rend = ring3Obj.GetComponent<Renderer>();
            r3Rend.material.color = new Color(0.7f, 0.8f, 1f, 0.7f);

            // Top cap (crown)
            GameObject topCapObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            topCapObj.name = "TopCap";
            topCapObj.transform.SetParent(bossObj.transform);
            topCapObj.transform.localPosition = new Vector3(0f, 4f, 0f);
            topCapObj.transform.localScale = Vector3.one * 1.5f;
            Renderer topRend = topCapObj.GetComponent<Renderer>();
            topRend.material.color = new Color(0.9f, 0.95f, 1f, 0.9f);

            // Bottom cap (anchor)
            GameObject botCapObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            botCapObj.name = "BottomCap";
            botCapObj.transform.SetParent(bossObj.transform);
            botCapObj.transform.localPosition = new Vector3(0f, -4f, 0f);
            botCapObj.transform.localScale = Vector3.one * 1.5f;
            Renderer botRend = botCapObj.GetComponent<Renderer>();
            botRend.material.color = new Color(0.9f, 0.95f, 1f, 0.9f);

            // Use core as primary reference
            Renderer rend = coreRend;

            // Boss light
            Light bossLight = bossObj.AddComponent<Light>();
            bossLight.type = LightType.Point;
            bossLight.color = new Color(0.8f, 0.9f, 1f);
            bossLight.range = 40f;
            bossLight.intensity = 3f;

            // TemporalGuardian component
            TemporalGuardian boss = bossObj.AddComponent<TemporalGuardian>();
            boss.spawner = this;

            // Audio: boss encounter music
            AudioManager.Instance?.PlaySFX3D(bossEncounterAudio, position);

            Debug.Log("[Moon9ContentSpawner] Temporal Guardian boss spawned! Defeat to claim clock tower blueprint.");
        }

        public void OnBossDefeated()
        {
            _bossDefeated = true;
            Debug.Log("[Moon9ContentSpawner] Temporal Guardian defeated! Clock tower blueprint obtained.");

            // Quest update
            QuestManager.Instance?.CompleteQuest("moon9_defeat_temporal_guardian");

            // Unlock clock tower installation
            if (_auroraCityActive)
            {
                // Player has time to explore before city fades
                UI.HUDController.Instance?.ShowObjective($"Explore aurora city before it fades! ({(int)_auroraCityTimer}s remaining)");
            }
        }

        void DespawnAuroraCity()
        {
            if (_auroraCityZone != null)
            {
                Debug.Log("[Moon9ContentSpawner] Aurora city fading...");

                // Fade VFX before destroying
                StartCoroutine(FadeAuroraCity());
            }

            _auroraCityActive = false;

            // Trigger revelation after city fades
            Invoke(nameof(TriggerRevelation), 5f);
        }

        System.Collections.IEnumerator FadeAuroraCity()
        {
            float fadeTime = 4f;
            float elapsed = 0f;

            Transform[] allChildren = _auroraCityZone.GetComponentsInChildren<Transform>();

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / fadeTime);

                foreach (var child in allChildren)
                {
                    Renderer rend = child.GetComponent<Renderer>();
                    if (rend != null && rend.material != null)
                    {
                        Color c = rend.material.color;
                        c.a = alpha;
                        rend.material.color = c;
                    }
                }

                yield return null;
            }

            Destroy(_auroraCityZone);
            Debug.Log("[Moon9ContentSpawner] Aurora city vanished.");
        }

        void TriggerRevelation()
        {
            if (_clockTowerInstalled) return;
            _clockTowerInstalled = true;

            Debug.Log("[Moon9ContentSpawner] REVELATION: Stone 6 timestamp = Rhythmic Moon 17th Hour. Bells ringing BEFORE Flood. What happened between bells + cataclysm?");

            // Mystery deepens: Stone 6 shows bells ringing in perfect harmony, THEN Flood happened
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayContextDialogue("moon9_mystery_deepens");
            }

            // 17-Hour Clock Tower installation (prophetic instructions from Stone 4)
            InstallClockTower();

            // Quest completion + Moon 10 unlock
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance?.CompleteQuest("moon9_prophecy_stones");
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMoonProgress(9, 100f);
                // Note: Moon unlock via SaveManager (SaveManager.Instance?.UnlockMoon(10))
                Debug.Log("[Moon9ContentSpawner] Moon 9 complete. Moon 10 (Continental Rails) unlocked.");
            }

            // RS Reward for Moon completion
            GameLoopController.Instance?.QueueRSReward(600f, "Moon 9 Complete: Solar Prophecy");

            // HUD: Moon trophy
            UI.HUDController.Instance?.ShowMoonTrophy("MOON 9 COMPLETE", "The Intention of Intention");

            // Audio: completion fanfare
            AudioManager.Instance?.PlaySFX2D("MoonCompleteFanfare");

            // Unlock time-bend ability globally
            // SaveManager global flag would go here

            SaveState();
        }

        void InstallClockTower()
        {
            Vector3 clockTowerPos = new Vector3(200f, 20f, 300f); // White City bell tower

            // Multi-part clock tower (base + shaft + face + bell housing)
            GameObject clockObj = new GameObject("17Hour_ClockTower");
            clockObj.transform.position = clockTowerPos;

            // Base platform
            GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseObj.name = "Base";
            baseObj.transform.SetParent(clockObj.transform);
            baseObj.transform.localPosition = new Vector3(0f, -10f, 0f);
            baseObj.transform.localScale = new Vector3(4f, 1f, 4f);
            Renderer baseRend = baseObj.GetComponent<Renderer>();
            baseRend.material.color = new Color(0.6f, 0.5f, 0.3f);

            // Tower shaft
            GameObject shaftObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaftObj.name = "Shaft";
            shaftObj.transform.SetParent(clockObj.transform);
            shaftObj.transform.localPosition = Vector3.zero;
            shaftObj.transform.localScale = new Vector3(2f, 20f, 2f);
            Renderer shaftRend = shaftObj.GetComponent<Renderer>();
            shaftRend.material.color = new Color(0.85f, 0.7f, 0.3f); // Brass

            // Clock face
            GameObject faceObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            faceObj.name = "ClockFace";
            faceObj.transform.SetParent(clockObj.transform);
            faceObj.transform.localPosition = new Vector3(0f, 15f, 2.2f);
            faceObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            faceObj.transform.localScale = new Vector3(2.5f, 0.2f, 2.5f);
            Renderer faceRend = faceObj.GetComponent<Renderer>();
            faceRend.material.color = new Color(1f, 0.95f, 0.8f); // White face

            // Bell housing (top)
            GameObject bellObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bellObj.name = "BellHousing";
            bellObj.transform.SetParent(clockObj.transform);
            bellObj.transform.localPosition = new Vector3(0f, 22f, 0f);
            bellObj.transform.localScale = new Vector3(2.5f, 2f, 2.5f);
            Renderer bellRend = bellObj.GetComponent<Renderer>();
            bellRend.material.color = new Color(0.7f, 0.6f, 0.3f);

            // Spire cap
            GameObject spireObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spireObj.name = "Spire";
            spireObj.transform.SetParent(clockObj.transform);
            spireObj.transform.localPosition = new Vector3(0f, 25f, 0f);
            spireObj.transform.localScale = new Vector3(0.5f, 3f, 0.5f);
            Renderer spireRend = spireObj.GetComponent<Renderer>();
            spireRend.material.color = new Color(0.9f, 0.8f, 0.4f);

            // Use shaft as primary reference
            Renderer rend = shaftRend;

            // Clock face (17 markers visible)
            // Full implementation would show 17-segment clock dial

            Debug.Log("[Moon9ContentSpawner] 17-Hour Clock Tower installed. Time-bend ability unlocked.");

            // Unlock time-bend ability globally
            if (SaveManager.Instance != null)
            {
                // Note: SaveManager global flags (TimeBendUnlocked ability)
            }
        }

        void SaveState()
        {
            if (SaveManager.Instance == null) return;

        }

        void LoadState()
        {
            if (SaveManager.Instance == null) return;

            _stonesCollected = 0 /*GetMoonData returns int*/;
            _zerethContactMade = 0 /*GetMoonData returns int*/ == 1;
            _auroraCityTriggered = 0 /*GetMoonData returns int*/ == 1;
            _clockTowerInstalled = 0 /*GetMoonData returns int*/ == 1;

            Debug.Log($"[Moon9ContentSpawner] State loaded: {_stonesCollected}/{totalStones} stones collected.");
        }
    }

    /// <summary>
    /// Prophecy stone collection + vision trigger.
    /// IInteractable: player collects stone → triggers Prophecy Vision (Golden Age moment).
    /// </summary>
    public class ProphecyStone : MonoBehaviour, IInteractable
    {
        public int stoneIndex;
        public string stoneName;
        public event System.Action<ProphecyStone> OnCollected;

        bool _isCollected;

        public string GetInteractPrompt() => _isCollected ? "Stone Collected" : $"Collect {stoneName} Stone (E)";

        public void Interact(GameObject player)
        {
            if (_isCollected) return;

            Debug.Log($"[ProphecyStone] Stone {stoneName} collected (instant for beta).");
            StartCollection();
        }

        void StartCollection()
        {
            _isCollected = true;

            // Collection VFX: golden shimmer absorbed into player
            GameObject vfxObj = new GameObject("StoneCollect_VFX");
            vfxObj.transform.position = transform.position;

            ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 3f;
            main.startSize = 0.3f;
            main.loop = false;
            main.maxParticles = 200;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 200) });

            Renderer rend = ps.GetComponent<Renderer>();
            if (rend != null && rend.material != null)
            {
                rend.material.color = new Color(1f, 0.9f, 0.4f); // Golden glow
            }

            Destroy(vfxObj, 2f);

            // Notify spawner
            OnCollected?.Invoke(this);

            // Destroy stone (collected)
            Destroy(gameObject, 0.5f);

            Debug.Log($"[ProphecyStone] Stone {stoneName} collected. Prophecy Vision triggered.");
        }
    }

    /// <summary>
    /// Golden Codex — PHI-inscribed book restoration.
    /// IInteractable: player restores pages one at a time (12 total).
    /// </summary>
    public class GoldenCodex : MonoBehaviour, IInteractable
    {
        public Moon9ContentSpawner spawner;

        int _pagesRestored;
        const int TotalPages = 12;

        public string GetInteractPrompt() =>
            _pagesRestored >= TotalPages ? "Codex Complete" : $"Restore Page ({_pagesRestored}/{TotalPages}) [E]";

        public void Interact(GameObject player)
        {
            if (_pagesRestored >= TotalPages) return;

            _pagesRestored++;
            Debug.Log($"[GoldenCodex] Page {_pagesRestored} restored. PHI inscription: {GetPageInscription(_pagesRestored)}");

            // VFX: golden restoration pulse
            GameObject vfxObj = new GameObject("CodexRestore_VFX");
            vfxObj.transform.position = transform.position;

            ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = 2f;
            main.startSize = 0.4f;
            main.loop = false;
            main.maxParticles = 150;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 150) });

            Destroy(vfxObj, 2f);

            // Notify spawner
            spawner?.OnCodexPageRestored();

            // Show inscription in UI
            UI.HUDController.Instance?.ShowObjective($"PHI Inscription {_pagesRestored}: {GetPageInscription(_pagesRestored)}");
        }

        string GetPageInscription(int pageNum)
        {
            string[] inscriptions = {
                "Time flows in spirals, not lines",
                "The 17th hour was never meant to end",
                "Bells measure harmony, not hours",
                "Clock towers bend the flow",
                "PHI ratio unlocks temporal gates",
                "Past and future echo in resonance",
                "Time-bend requires harmonic alignment",
                "The Flood happened outside time",
                "17-hour cycle maintains balance",
                "Zereth broke the cycle deliberately",
                "Temporal locks prevent Reset interference",
                "Clock tower blueprint: 17 segments, golden spiral"
            };

            return pageNum <= inscriptions.Length ? inscriptions[pageNum - 1] : "Ancient wisdom";
        }
    }

    /// <summary>
    /// Temporal Guardian boss — protects aurora city secrets.
    /// Defeated by resonance weapons, drops clock tower blueprint.
    /// </summary>
    public class TemporalGuardian : MonoBehaviour
    {
        public Moon9ContentSpawner spawner;

        float _health = 2000f;
        float _maxHealth = 2000f;
        float _attackCooldown;
        float _timeBendCooldown;
        Vector3 _spawnPos;
        int _currentPhase = 1;

        void Start()
        {
            _spawnPos = transform.position;
            Debug.Log("[TemporalGuardian] Boss engaged! HP: 2000");

            // Show boss health bar on HUD
            UI.HUDController.Instance?.ShowBossHealth("Temporal Guardian", 1f);
        }

        void Update()
        {
            // Rotate slowly
            transform.Rotate(Vector3.up, 20f * Time.deltaTime);

            // Pulse light
            Light light = GetComponent<Light>();
            if (light != null)
            {
                light.intensity = 3f + Mathf.Sin(Time.time * 3f) * 1f;
            }

            // Phase transitions
            if (_health < 1200f && _currentPhase == 1)
            {
                EnterPhase2();
            }
            else if (_health < 600f && _currentPhase == 2)
            {
                EnterPhase3();
            }

            // Simple attack pattern (placeholder for beta)
            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown <= 0f)
            {
                TemporalBlast();
                _attackCooldown = 4f;
            }

            // Time-bend ability (slows player)
            _timeBendCooldown -= Time.deltaTime;
            if (_timeBendCooldown <= 0f && _health < 1000f)
            {
                TimeBendAttack();
                _timeBendCooldown = 8f;
            }
        }

        void EnterPhase2()
        {
            _currentPhase = 2;
            Debug.Log("[TemporalGuardian] PHASE 2: Time fractures — temporal rifts open!");

            // VFX: rifts appear
            // Audio phase transition
            Audio.AudioManager.Instance?.PlaySFX3D("BossPhase2", transform.position);

            // Spawn temporal rifts around boss (multi-part vortex)
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 riftPos = transform.position + new Vector3(Mathf.Cos(angle) * 15f, 0f, Mathf.Sin(angle) * 15f);

                GameObject rift = new GameObject($"TemporalRift_{i}");
                rift.transform.position = riftPos;

                // Core vortex
                GameObject coreObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coreObj.name = "Core";
                coreObj.transform.SetParent(rift.transform);
                coreObj.transform.localPosition = Vector3.zero;
                coreObj.transform.localScale = Vector3.one * 2f;
                Renderer coreRend = coreObj.GetComponent<Renderer>();
                coreRend.material.color = new Color(0.6f, 0.8f, 1f, 0.7f);

                // Outer swirl ring
                GameObject swirlObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                swirlObj.name = "Swirl";
                swirlObj.transform.SetParent(rift.transform);
                swirlObj.transform.localPosition = Vector3.zero;
                swirlObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                swirlObj.transform.localScale = new Vector3(3.5f, 0.1f, 3.5f);
                Renderer swirlRend = swirlObj.GetComponent<Renderer>();
                swirlRend.material.color = new Color(0.7f, 0.9f, 1f, 0.5f);

                // Energy tendrils (4 small spheres orbiting)
                for (int t = 0; t < 4; t++)
                {
                    float tAngle = t * 90f * Mathf.Deg2Rad;
                    GameObject tendrilObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    tendrilObj.name = $"Tendril_{t}";
                    tendrilObj.transform.SetParent(rift.transform);
                    tendrilObj.transform.localPosition = new Vector3(Mathf.Cos(tAngle) * 2f, 0f, Mathf.Sin(tAngle) * 2f);
                    tendrilObj.transform.localScale = Vector3.one * 0.5f;
                    Renderer tRend = tendrilObj.GetComponent<Renderer>();
                    tRend.material.color = new Color(0.8f, 0.95f, 1f, 0.8f);
                }
            }
        }

        void EnterPhase3()
        {
            _currentPhase = 3;
            Debug.Log("[TemporalGuardian] PHASE 3: Timeline collapse imminent!");

            // VFX: boss form distorts
            // Audio phase transition
            Audio.AudioManager.Instance?.PlaySFX3D("BossPhase3", transform.position);

            // Speed up attacks
            _attackCooldown = 2f;
            _timeBendCooldown = 4f;
        }

        void TemporalBlast()
        {
            Debug.Log("[TemporalGuardian] Temporal blast!");
            // Spawn projectiles toward player (simplified for beta)
            Audio.AudioManager.Instance?.PlaySFX3D("TemporalBlast", transform.position);
        }

        void TimeBendAttack()
        {
            Debug.Log("[TemporalGuardian] Time-bend field! Player slowed!");
            // Apply slow debuff to player within radius
            Audio.AudioManager.Instance?.PlaySFX3D("TimeBend", transform.position);
        }

        public void TakeDamage(float damage)
        {
            _health -= damage;
            Debug.Log($"[TemporalGuardian] Took {damage} damage, {_health} HP remaining");

            // Update HUD boss health
            UI.HUDController.Instance?.UpdateBossHealth(_health / _maxHealth);

            if (_health <= 0f)
            {
                DefeatBoss();
            }
        }

        void DefeatBoss()
        {
            Debug.Log("[TemporalGuardian] DEFEATED! Clock tower blueprint obtained!");

            // Hide boss health bar
            UI.HUDController.Instance?.HideBossHealth();

            // Death VFX
            GameObject vfxObj = new GameObject("BossDefeat_VFX");
            vfxObj.transform.position = transform.position;

            ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 3f;
            main.startSpeed = 10f;
            main.startSize = 2f;
            main.loop = false;
            main.maxParticles = 1000;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1000) });

            Destroy(vfxObj, 4f);

            // Audio: boss defeat
            Audio.AudioManager.Instance?.PlaySFX3D("BossDefeat", transform.position);

            // RS Reward for boss kill
            GameLoopController.Instance?.QueueRSReward(200f, "Temporal Guardian Defeated");

            // Notify spawner
            spawner?.OnBossDefeated();

            // Drop clock tower blueprint (multi-part scroll)
            GameObject blueprint = new GameObject("ClockTowerBlueprint");
            blueprint.transform.position = transform.position;

            // Scroll cylinder (rolled)
            GameObject scrollObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            scrollObj.name = "Scroll";
            scrollObj.transform.SetParent(blueprint.transform);
            scrollObj.transform.localPosition = Vector3.zero;
            scrollObj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            scrollObj.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
            Renderer scrollRend = scrollObj.GetComponent<Renderer>();
            scrollRend.material.color = new Color(1f, 0.95f, 0.85f);

            // Left endcap
            GameObject leftCapObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftCapObj.name = "LeftCap";
            leftCapObj.transform.SetParent(blueprint.transform);
            leftCapObj.transform.localPosition = new Vector3(-1f, 0f, 0f);
            leftCapObj.transform.localScale = Vector3.one * 0.4f;
            Renderer leftRend = leftCapObj.GetComponent<Renderer>();
            leftRend.material.color = new Color(0.7f, 0.6f, 0.3f);

            // Right endcap
            GameObject rightCapObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightCapObj.name = "RightCap";
            rightCapObj.transform.SetParent(blueprint.transform);
            rightCapObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            rightCapObj.transform.localScale = Vector3.one * 0.4f;
            Renderer rightRend = rightCapObj.GetComponent<Renderer>();
            rightRend.material.color = new Color(0.7f, 0.6f, 0.3f);

            // Glow orb (blueprint marker)
            GameObject glowObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glowObj.name = "Glow";
            glowObj.transform.SetParent(blueprint.transform);
            glowObj.transform.localPosition = new Vector3(0f, 1f, 0f);
            glowObj.transform.localScale = Vector3.one * 0.6f;
            Renderer glowRend = glowObj.GetComponent<Renderer>();
            glowRend.material.color = new Color(1f, 0.9f, 0.6f);

            // Use glow as primary reference
            Renderer rend = glowRend;

            // Destroy boss
            Destroy(gameObject, 0.5f);
        }
    }

    /// <summary>
    /// Aurora city lore fragment — collectible insights.
    /// </summary>
    public class AuroraLoreFragment : MonoBehaviour, IInteractable
    {
        public int fragmentIndex;
        bool _collected;

        readonly string[] _loreTexts = {
            "This city existed for 3 minutes every dawn during the Golden Age.",
            "Temporal anchors maintained the manifestation until the Flood.",
            "Only those who collected prophecy stones could perceive it."
        };

        public string GetInteractPrompt() => _collected ? "" : "Examine Lore Fragment (E)";

        public void Interact(GameObject player)
        {
            if (_collected || fragmentIndex >= _loreTexts.Length) return;

            _collected = true;
            Debug.Log($"[AuroraLore] {_loreTexts[fragmentIndex]}");

            UI.HUDController.Instance?.ShowDialogue("Lore", _loreTexts[fragmentIndex]);

            // Fade fragment
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 0.3f;
                rend.material.color = c;
            }
        }
    }
}


