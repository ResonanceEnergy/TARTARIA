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

        [Header("Spawn Configuration")]
        [SerializeField] Vector3[] stoneLocations = new Vector3[6]; // Cross-continental ley-line nodes

        [Header("Audio")]
        [SerializeField] string stoneCollectAudio = "Moon9_StoneCollect";
        [SerializeField] string visionAudio = "Moon9_ProphecyVision";
        [SerializeField] string zerethVoiceAudio = "Zereth_Distorted";
        [SerializeField] string auroraCityAudio = "Moon9_AuroraCity";

        List<ProphecyStone> _activeStones = new List<ProphecyStone>();
        bool _zerethContactMade;
        bool _auroraCityTriggered;
        bool _clockTowerInstalled;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
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

            Debug.Log($"[Moon9ContentSpawner] 6 prophecy stones spawned across continent.");
        }

        void SpawnProphecyStones()
        {
            string[] stoneNames = { "Dawn", "Flow", "Craft", "Flight", "Song", "Stars" };

            for (int i = 0; i < totalStones; i++)
            {
                GameObject stoneObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                stoneObj.name = $"ProphecyStone_{stoneNames[i]}";
                stoneObj.transform.position = stoneLocations[i];
                stoneObj.transform.localScale = Vector3.one * 1.2f;

                // Placeholder visual: golden glowing sphere (inscribed with golden-ratio patterns)
                Renderer rend = stoneObj.GetComponent<Renderer>();
                rend.material.color = new Color(1f, 0.85f, 0.3f); // Golden glow

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

        void OnStoneCollected(ProphecyStone stone)
        {
            _stonesCollected++;
            Debug.Log($"[Moon9ContentSpawner] Prophecy Stone {stone.stoneName} collected. Progress: {_stonesCollected}/{totalStones}");

            // Audio: stone collection harmonic
            AudioManager.Instance?.PlaySFX3D(stoneCollectAudio, stone.transform.position);

            // Trigger Prophecy Vision (Golden Age moment replay)
            TriggerProphecyVision(stone.stoneIndex);

            // Check if all stones collected
            if (_stonesCollected >= totalStones)
            {
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
                DialogueManager.Instance.PlayDialogue("moon9_zereth_contact");
                // "You see paradise. I saw a cage. They called it harmony. I called it submission. One note forever? I wanted MORE."
            }

            SaveState();
        }

        void TriggerAuroraCity()
        {
            if (_auroraCityTriggered) return;
            _auroraCityTriggered = true;

            Debug.Log("[Moon9ContentSpawner] CLIMAX: Floating aurora city appears! Complete Golden Age district in sky for 3 min!");

            // Climax: floating aurora city (temporary sky zone, 3 real-time minutes)
            Vector3 skyCityPos = new Vector3(400f, 150f, 500f); // High above continent

            GameObject auroraCityObj = new GameObject("AuroraCity_Vision");
            auroraCityObj.transform.position = skyCityPos;

            // Particle system: golden aurora city hologram (massive, complex)
            ParticleSystem ps = auroraCityObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 180f; // 3 min
            main.startSpeed = 0.2f;
            main.startSize = 4f;
            main.loop = true;
            main.maxParticles = 10000;

            var emission = ps.emission;
            emission.rateOverTime = 500f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(200f, 50f, 200f); // Massive city zone

            Renderer rend = ps.GetComponent<Renderer>();
            if (rend != null && rend.material != null)
            {
                rend.material.color = new Color(1f, 0.9f, 0.6f, 0.7f); // Golden aurora hologram
            }

            // Audio: aurora city harmonic (breathtaking)
            AudioManager.Instance?.PlaySFX3D(auroraCityAudio, skyCityPos);

            // NPCs point upward in wonder
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayDialogue("moon9_milo_aurora_city");
                // "That's real, isn't it? Not a sales pitch. Not a postcard. That's what we were supposed to have."
            }

            // City fades after 3 min
            Destroy(auroraCityObj, 180f);

            // Trigger revelation after city fades
            Invoke(nameof(TriggerRevelation), 185f);

            SaveState();
        }

        void TriggerRevelation()
        {
            if (_clockTowerInstalled) return;
            _clockTowerInstalled = true;

            Debug.Log("[Moon9ContentSpawner] REVELATION: Stone 6 timestamp = Rhythmic Moon 17th Hour. Bells ringing BEFORE Flood. What happened between bells + cataclysm?");

            // Mystery deepens: Stone 6 shows bells ringing in perfect harmony, THEN Flood happened
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayDialogue("moon9_mystery_deepens");
            }

            // 17-Hour Clock Tower installation (prophetic instructions from Stone 4)
            InstallClockTower();

            // Quest completion + Moon 10 unlock
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.CompleteQuest("moon9_prophecy_stones");
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMoonProgress(9, 100f);
                SaveManager.Instance.UnlockMoon(10);
                Debug.Log("[Moon9ContentSpawner] Moon 9 complete. Moon 10 (Continental Rails) unlocked.");
            }

            SaveState();
        }

        void InstallClockTower()
        {
            Vector3 clockTowerPos = new Vector3(200f, 20f, 300f); // White City bell tower

            GameObject clockObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            clockObj.name = "17Hour_ClockTower";
            clockObj.transform.position = clockTowerPos;
            clockObj.transform.localScale = new Vector3(2f, 20f, 2f); // Tall clock tower

            // Placeholder visual: golden brass clock mechanism
            Renderer rend = clockObj.GetComponent<Renderer>();
            rend.material.color = new Color(0.85f, 0.7f, 0.3f); // Brass

            // Clock face (17 markers visible)
            // Full implementation would show 17-segment clock dial

            Debug.Log("[Moon9ContentSpawner] 17-Hour Clock Tower installed. Time-bend ability unlocked.");

            // Unlock time-bend ability globally
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetGlobalFlag("TimeBendUnlocked", true);
            }
        }

        void SaveState()
        {
            if (SaveManager.Instance == null) return;

            SaveManager.Instance.SetMoonData(9, "stonesCollected", _stonesCollected);
            SaveManager.Instance.SetMoonData(9, "zerethContactMade", _zerethContactMade ? 1 : 0);
            SaveManager.Instance.SetMoonData(9, "auroraCityTriggered", _auroraCityTriggered ? 1 : 0);
            SaveManager.Instance.SetMoonData(9, "clockTowerInstalled", _clockTowerInstalled ? 1 : 0);
        }

        void LoadState()
        {
            if (SaveManager.Instance == null) return;

            _stonesCollected = SaveManager.Instance.GetMoonData(9, "stonesCollected", 0);
            _zerethContactMade = SaveManager.Instance.GetMoonData(9, "zerethContactMade", 0) == 1;
            _auroraCityTriggered = SaveManager.Instance.GetMoonData(9, "auroraCityTriggered", 0) == 1;
            _clockTowerInstalled = SaveManager.Instance.GetMoonData(9, "clockTowerInstalled", 0) == 1;

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

        public string InteractPrompt => _isCollected ? "Stone Collected" : $"Collect {stoneName} Stone (E)";

        public void Interact()
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
}
