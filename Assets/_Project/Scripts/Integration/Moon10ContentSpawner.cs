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
    /// Moon 10 (Planetary Moon - "The Manifestation of Producing") content spawner.
    /// Continental Rail Network: Build resonance trains connecting all restored zones + junior engineers + Mud Flood trigger device.
    /// Auto-unlocks when Moon 9 complete.
    /// 
    /// GDD §03: Moon 10 — Planetary Moon
    /// - Discovery (Days 1-5): Rail network hums spontaneously, buried stations surface, Mud Flood trigger device found
    /// - Restoration (Days 6-12): Build continental rail segments, adopted children become junior engineers
    /// - Conflict (Days 13-18): Dissonant rails (Zereth's inverted frequency experiments), elite golems on corrupted tracks
    /// - Climax (Days 19-24): First continental train ride through all restored zones, see work from window
    /// - Revelation (Days 25-28): Trigger device fingerprints: 1 giant-sized + 2 human-sized (Zereth + 2 Cabal infiltrators)
    /// 
    /// Crossover seeds: Train network fast-travel, children operate trains, Zereth exoneration evidence (Mon 13)
    /// </summary>
    public class Moon10ContentSpawner : MonoBehaviour
    {
        public static Moon10ContentSpawner Instance { get; private set; }

        [Header("Rail Configuration")]
        [SerializeField] int totalRailSegments = 8; // Connect 8 major zones
        int _railSegmentsBuilt;

        [Header("Spawn Configuration")]
        [SerializeField] Vector3 continentalStationCenter = new Vector3(300f, 0f, 400f);

        [Header("Audio")]
        [SerializeField] string railHumAudio = "Moon10_RailHum";
        [SerializeField] string trainRideAudio = "Moon10_TrainRide";
        [SerializeField] string triggerDeviceAudio = "Moon10_TriggerDevice";

        bool _triggerDeviceFound;
        bool _continentalRideComplete;
        bool _revelationUnlocked;

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
            // Check if Moon 9 complete → auto-unlock Moon 10
            if (SaveManager.Instance != null && SaveManager.Instance.GetMoonProgress(9) >= 100f)
            {
                UnlockMoon10();
            }
        }

        public void UnlockMoon10()
        {
            if (_railSegmentsBuilt > 0) return; // Already spawned

            Debug.Log("[Moon10ContentSpawner] Moon 10 unlocked: Continental rail network hums spontaneously.");
            SpawnMoon10Content();
            LoadState();
        }

        void SpawnMoon10Content()
        {
            // Discovery: Continental station + Mud Flood trigger device
            SpawnContinentalStation();
            SpawnMudFloodTriggerDevice();

            Debug.Log($"[Moon10ContentSpawner] Continental rail network activated. Trigger device discovered.");
        }

        void SpawnContinentalStation()
        {
            GameObject stationObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stationObj.name = "ContinentalStation";
            stationObj.transform.position = continentalStationCenter;
            stationObj.transform.localScale = new Vector3(20f, 5f, 30f); // Large station hall

            // Placeholder visual: precision-cut stone, copper inlays
            Renderer rend = stationObj.GetComponent<Renderer>();
            rend.material.color = new Color(0.5f, 0.45f, 0.4f); // Gray stone with copper tint

            // Station hums (rails spontaneously active)
            AudioSource audioSrc = stationObj.AddComponent<AudioSource>();
            audioSrc.loop = true;
            audioSrc.spatialBlend = 1f;
            audioSrc.maxDistance = 40f;
            // audioSrc.clip = railHumAudio (432 Hz rail resonance)
            audioSrc.Play();

            Debug.Log("[Moon10ContentSpawner] Continental station surfaced. Rails hum at 432 Hz.");
        }

        void SpawnMudFloodTriggerDevice()
        {
            Vector3 hiddenRoomPos = continentalStationCenter + new Vector3(-15f, -5f, -10f); // Hidden room beneath station

            GameObject triggerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            triggerObj.name = "MudFloodTriggerDevice";
            triggerObj.transform.position = hiddenRoomPos;
            triggerObj.transform.localScale = new Vector3(3f, 2f, 3f); // Console-sized

            // Placeholder visual: dissonance amplifier (pointed at star fort network)
            Renderer rend = triggerObj.GetComponent<Renderer>();
            rend.material.color = new Color(0.2f, 0.15f, 0.15f); // Dark metallic

            // Red warning light (dissonance active historically)
            Light triggerLight = triggerObj.AddComponent<Light>();
            triggerLight.type = LightType.Point;
            triggerLight.color = Color.red;
            triggerLight.range = 8f;
            triggerLight.intensity = 1f;

            // TriggerDeviceInteract component: examine fingerprints + reveal
            TriggerDeviceInteract interact = triggerObj.AddComponent<TriggerDeviceInteract>();
            interact.OnExamined += HandleTriggerDeviceExamined;

            Debug.Log("[Moon10ContentSpawner] Mud Flood trigger device found in hidden room. 3 sets of fingerprints visible.");
        }

        void HandleTriggerDeviceExamined()
        {
            _triggerDeviceFound = true;
            Debug.Log("[Moon10ContentSpawner] Trigger device examined: 1 giant-sized + 2 human-sized fingerprints. Zereth + 2 Parasite Cabal infiltrators.");

            // Audio: ominous revelation tone
            AudioManager.Instance?.PlaySFX2D(triggerDeviceAudio);

            SaveState();
        }

        public void OnRailSegmentBuilt()
        {
            _railSegmentsBuilt++;
            Debug.Log($"[Moon10ContentSpawner] Rail segment built: {_railSegmentsBuilt}/{totalRailSegments}");

            if (_railSegmentsBuilt >= totalRailSegments)
            {
                TriggerContinentalRide();
            }

            SaveState();
        }

        void TriggerContinentalRide()
        {
            if (_continentalRideComplete) return;
            _continentalRideComplete = true;

            Debug.Log("[Moon10ContentSpawner] CLIMAX: First continental train ride! Silent, smooth, through all restored zones!");

            // Cinematic: train passes through all 8+ restored zones
            // Player sees their work from window: domes glowing, fountains spraying, ley lines pulsing

            // Audio: train ride harmonic (silent smooth movement)
            AudioManager.Instance?.PlaySFX2D(trainRideAudio);

            // Junior engineers from Moon 3 operate train
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayDialogue("moon10_child_engineer");
                // "Korath said the rails should sing. I tuned this one myself — listen!"
            }

            // Trigger revelation after ride
            Invoke(nameof(TriggerRevelation), 10f);

            SaveState();
        }

        void TriggerRevelation()
        {
            if (_revelationUnlocked) return;
            _revelationUnlocked = true;

            Debug.Log("[Moon10ContentSpawner] REVELATION: Trigger device fingerprints prove Zereth + 2 Cabal members. Zereth was VICTIM, not villain!");

            // Lore revelation dialogue
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayDialogue("moon10_zereth_exoneration");
            }

            // Quest completion + Moon 11 unlock
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.CompleteQuest("moon10_continental_rail");
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMoonProgress(10, 100f);
                SaveManager.Instance.UnlockMoon(11);
                Debug.Log("[Moon10ContentSpawner] Moon 10 complete. Moon 11 (Spectral Aquifer) unlocked.");
            }

            SaveState();
        }

        void SaveState()
        {
            if (SaveManager.Instance == null) return;

            SaveManager.Instance.SetMoonData(10, "railSegmentsBuilt", _railSegmentsBuilt);
            SaveManager.Instance.SetMoonData(10, "triggerDeviceFound", _triggerDeviceFound ? 1 : 0);
            SaveManager.Instance.SetMoonData(10, "continentalRideComplete", _continentalRideComplete ? 1 : 0);
            SaveManager.Instance.SetMoonData(10, "revelationUnlocked", _revelationUnlocked ? 1 : 0);
        }

        void LoadState()
        {
            if (SaveManager.Instance == null) return;

            _railSegmentsBuilt = SaveManager.Instance.GetMoonData(10, "railSegmentsBuilt", 0);
            _triggerDeviceFound = SaveManager.Instance.GetMoonData(10, "triggerDeviceFound", 0) == 1;
            _continentalRideComplete = SaveManager.Instance.GetMoonData(10, "continentalRideComplete", 0) == 1;
            _revelationUnlocked = SaveManager.Instance.GetMoonData(10, "revelationUnlocked", 0) == 1;

            Debug.Log($"[Moon10ContentSpawner] State loaded: {_railSegmentsBuilt}/{totalRailSegments} rail segments built.");
        }
    }

    /// <summary>
    /// Mud Flood trigger device interaction.
    /// Examine fingerprints → reveal Zereth + 2 Cabal infiltrators (Zereth was victim).
    /// </summary>
    public class TriggerDeviceInteract : MonoBehaviour, IInteractable
    {
        public event System.Action OnExamined;

        bool _examined;

        public string GetInteractPrompt() => _examined ? "Device Examined" : "Examine Trigger Device (E)";

        public void Interact(GameObject player)
        {
            if (_examined) return;

            Debug.Log("[TriggerDeviceInteract] Examining Mud Flood trigger device... 3 sets of fingerprints visible.");

            _examined = true;
            OnExamined?.Invoke();

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayDialogue("moon10_fingerprints_reveal");
                // "One giant-sized. Two human-sized. The Cabal infiltrated Zereth's lab and reversed the polarity. HE tried to stop it."
            }
        }
    }
}
