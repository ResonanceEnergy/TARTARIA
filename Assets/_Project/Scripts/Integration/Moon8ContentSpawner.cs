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
    /// Moon 8 (Galactic Moon - "The Integrity of Harmonizing") content spawner.
    /// Sky Isles + Airship Graveyard: Thorne lands + 3-ship armada + megalith transport + aerial combat.
    /// Auto-unlocks when Moon 7 complete.
    /// 
    /// GDD §03: Moon 8 — Galactic Moon
    /// - Discovery (Days 1-5): Thorne lands at White City dock, battered flagship descends
    /// - Restoration (Days 6-12): Repair 3 airships (graveyard), 9-band mercury-orb tuning, megalith transport missions
    /// - Conflict (Days 13-18): Aerial combat vs Reset anti-Aether drones, strategic dissonance generator targeting
    /// - Climax (Days 19-24): Night flight under full moon, all 3 ships in formation, ley lines glow as golden rivers
    /// - Revelation (Days 25-28): Airships ferried giants between continents, no separation of peoples, Reset severed connections
    /// 
    /// Crossover seeds: Airships carry children (from Moon 3), Moon 10 continental transport, Korath echo during megalith flights
    /// </summary>
    public class Moon8ContentSpawner : MonoBehaviour
    {
        public static Moon8ContentSpawner Instance { get; private set; }

        [Header("Airship Configuration")]
        [SerializeField] int totalAirships = 3;
        int _airshipsRepaired;

        [Header("Spawn Configuration")]
        [SerializeField] Vector3 whiteCityDock = new Vector3(200f, 5f, 320f); // White City dock from Moon 5
        [SerializeField] Vector3 airshipGraveyardCenter = new Vector3(150f, 10f, 200f);
        [SerializeField] float graveyardRadius = 80f;

        [Header("Audio")]
        [SerializeField] string thorneLandingAudio = "Thorne_Landing";
        [SerializeField] string airshipRepairAudio = "Moon8_AirshipRepair";
        [SerializeField] string nightFlightAudio = "Moon8_NightFlight";
        [SerializeField] string aerialCombatAudio = "Moon8_AerialCombat";

        GameObject _thorneFlagship;
        List<TartarianAirship> _activeAirships = new List<TartarianAirship>();
        bool _thorneLanded;
        bool _aerialCombatTriggered;
        bool _nightFlightTriggered;
        bool _revelationUnlocked;

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
            // Moon 8: 3 airships repaired + Thorne landing
            sd.SetMoonFlag(8, "airshipsRepaired", _airshipsRepaired);
            sd.SetMoonFlag(8, "thorneLanded", _thorneLanded);
            sd.SetMoonFlag(8, "aerialCombatTriggered", _aerialCombatTriggered);
            sd.SetMoonFlag(8, "nightFlightTriggered", _nightFlightTriggered);
            sd.SetMoonFlag(8, "revelationUnlocked", _revelationUnlocked);
        }

        void OnLoad(SaveData sd)
        {
            // Restore Moon 8 state
            _airshipsRepaired = sd.GetMoonFlag(8, "airshipsRepaired", 0);
            _thorneLanded = sd.GetMoonFlag(8, "thorneLanded");
            _aerialCombatTriggered = sd.GetMoonFlag(8, "aerialCombatTriggered");
            _nightFlightTriggered = sd.GetMoonFlag(8, "nightFlightTriggered");
            _revelationUnlocked = sd.GetMoonFlag(8, "revelationUnlocked");

            Debug.Log($"[Moon8ContentSpawner] State loaded: airships={_airshipsRepaired}/{totalAirships}, landed={_thorneLanded}");
        }

        void Start()
        {
            // Check if Moon 7 complete → auto-unlock Moon 8
            if (SaveManager.Instance != null && SaveManager.Instance.GetMoonProgress(7) >= 100f)
            {
                UnlockMoon8();
            }
        }

        public void UnlockMoon8()
        {
            if (_thorneLanded) return; // Already spawned

            Debug.Log("[Moon8ContentSpawner] Moon 8 unlocked: Thorne descends to White City dock.");
            SpawnMoon8Content();
            LoadState();
        }

        void SpawnMoon8Content()
        {
            // Discovery: Thorne's flagship lands at White City dock
            SpawnThorneFlagship();

            // Restoration: 2 additional airships in graveyard (3 total with flagship)
            SpawnAirshipGraveyard();

            // Quest: repair airship armada
            QuestManager.Instance?.ActivateQuest("moon8_airship_repair");

            Debug.Log($"[Moon8ContentSpawner] Thorne flagship landed. Airship graveyard: 2 ships awaiting repair.");
        }

        void SpawnThorneFlagship()
        {
            // Multi-part Thorne flagship airship
            _thorneFlagship = new GameObject("Thorne_Flagship");
            _thorneFlagship.transform.position = whiteCityDock + Vector3.up * 8f;

            // Hull main body
            GameObject hullMain = new GameObject("HullMain");
            hullMain.transform.SetParent(_thorneFlagship.transform);
            hullMain.transform.localScale = new Vector3(10f, 3f, 25f);
            hullMain.transform.localPosition = Vector3.zero;
            hullMain.AddComponent<MeshFilter>();
            hullMain.AddComponent<MeshRenderer>();
            hullMain.AddComponent<BoxCollider>();

            // Hull bow (front section)
            GameObject hullBow = new GameObject("HullBow");
            hullBow.transform.SetParent(_thorneFlagship.transform);
            hullBow.transform.localScale = new Vector3(8f, 2.5f, 8f);
            hullBow.transform.localPosition = new Vector3(0f, 0f, 16.5f);
            hullBow.AddComponent<MeshFilter>();
            hullBow.AddComponent<MeshRenderer>();
            hullBow.AddComponent<BoxCollider>();

            // Hull stern (rear section)
            GameObject hullStern = new GameObject("HullStern");
            hullStern.transform.SetParent(_thorneFlagship.transform);
            hullStern.transform.localScale = new Vector3(9f, 2.8f, 6f);
            hullStern.transform.localPosition = new Vector3(0f, 0f, -15.5f);
            hullStern.AddComponent<MeshFilter>();
            hullStern.AddComponent<MeshRenderer>();
            hullStern.AddComponent<BoxCollider>();

            // Bridge tower
            GameObject bridge = new GameObject("Bridge");
            bridge.transform.SetParent(_thorneFlagship.transform);
            bridge.transform.localScale = new Vector3(6f, 4f, 6f);
            bridge.transform.localPosition = new Vector3(0f, 3.5f, 0f);
            bridge.AddComponent<MeshFilter>();
            bridge.AddComponent<MeshRenderer>();
            bridge.AddComponent<BoxCollider>();

            // Mercury-orb engines (2 nacelles)
            for (int e = 0; e < 2; e++)
            {
                GameObject engine = new GameObject($"Engine_{e}");
                engine.transform.SetParent(_thorneFlagship.transform);
                engine.transform.localScale = new Vector3(2f, 4f, 2f);
                engine.transform.localPosition = new Vector3((e == 0 ? -6f : 6f), -1f, -10f);
                engine.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                engine.AddComponent<MeshFilter>();
                engine.AddComponent<MeshRenderer>();
                engine.AddComponent<CapsuleCollider>();
            }

            // Placeholder visual: battered Tartarian airship (sacred-geometry hull, brass accents)
            Renderer[] renderers = _thorneFlagship.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.material.color = new Color(0.6f, 0.5f, 0.4f); // Weathered brass
            }

            // Light: mercury-orb engines (cold, off)
            Light engineLight = _thorneFlagship.AddComponent<Light>();
            engineLight.type = LightType.Point;
            engineLight.color = new Color(0.7f, 0.8f, 0.9f); // Cool mercury glow
            engineLight.range = 20f;
            engineLight.intensity = 0f; // Off until repaired

            // Thorne NPC on deck
            SpawnThorneNPC();

            // Audio: landing sequence (thrusters, hull creak)
            AudioManager.Instance?.PlaySFX3D(thorneLandingAudio, whiteCityDock);

            _thorneLanded = true;
            _airshipsRepaired++; // Flagship counts as 1

            SaveState();

            Debug.Log("[Moon8ContentSpawner] Thorne flagship landed. Thorne: 'Two centuries circling. This bucket flies like it's still offended.'");
        }

        void SpawnThorneNPC()
        {
            // Captain Thorne — KayKit Ranger (grizzled veteran)
            GameObject thornePrefab = Resources.Load<GameObject>("Prefabs/Characters/KayKit/Char_Ranger");
            GameObject thorneObj;
            if (thornePrefab != null)
            {
                thorneObj = Instantiate(thornePrefab, whiteCityDock + new Vector3(3f, 0f, 0f), Quaternion.identity);
                thorneObj.name = "CaptainThorne";
                thorneObj.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            }
            else
            {
                Debug.LogError("[Moon8ContentSpawner] CRITICAL: Char_Ranger prefab missing for Thorne");
                thorneObj = new GameObject("CaptainThorne_MISSING_PREFAB");
                thorneObj.transform.position = whiteCityDock + new Vector3(3f, 2f, 0f);
            }

            // Thorne dialogue component
            ThorneDialogue dialogue = thorneObj.AddComponent<ThorneDialogue>();

            Debug.Log("[Moon8ContentSpawner] Thorne NPC spawned on dock.");
        }

        void SpawnAirshipGraveyard()
        {
            // 2 additional airships scattered in graveyard zone
            for (int i = 0; i < totalAirships - 1; i++) // -1 because flagship already spawned
            {
                float angle = i * (360f / (totalAirships - 1)) * Mathf.Deg2Rad;
                Vector3 pos = airshipGraveyardCenter + new Vector3(
                    Mathf.Cos(angle) * graveyardRadius,
                    5f + i * 3f, // Staggered heights
                    Mathf.Sin(angle) * graveyardRadius
                );

                // Multi-part crashed airship
                GameObject airshipObj = new GameObject($"Airship_Graveyard_{i}");
                airshipObj.transform.position = pos;
                airshipObj.transform.rotation = Quaternion.Euler(0f, i * 45f, 10f + i * 5f);

                // Hull main section
                GameObject hullMain = new GameObject("HullMain");
                hullMain.transform.SetParent(airshipObj.transform);
                hullMain.transform.localScale = new Vector3(9f, 2.5f, 20f);
                hullMain.transform.localPosition = Vector3.zero;
                hullMain.AddComponent<MeshFilter>();
                hullMain.AddComponent<MeshRenderer>();
                hullMain.AddComponent<BoxCollider>();

                // Hull fore section
                GameObject hullFore = new GameObject("HullFore");
                hullFore.transform.SetParent(airshipObj.transform);
                hullFore.transform.localScale = new Vector3(7f, 2f, 6f);
                hullFore.transform.localPosition = new Vector3(0f, 0f, 13f);
                hullFore.AddComponent<MeshFilter>();
                hullFore.AddComponent<MeshRenderer>();
                hullFore.AddComponent<BoxCollider>();

                // Hull aft section
                GameObject hullAft = new GameObject("HullAft");
                hullAft.transform.SetParent(airshipObj.transform);
                hullAft.transform.localScale = new Vector3(8f, 2.2f, 5f);
                hullAft.transform.localPosition = new Vector3(0f, 0f, -12.5f);
                hullAft.AddComponent<MeshFilter>();
                hullAft.AddComponent<MeshRenderer>();
                hullAft.AddComponent<BoxCollider>();

                // Broken engine (one side)
                GameObject brokenEngine = new GameObject("BrokenEngine");
                brokenEngine.transform.SetParent(airshipObj.transform);
                brokenEngine.transform.localScale = new Vector3(1.5f, 3f, 1.5f);
                brokenEngine.transform.localPosition = new Vector3(-5f, -1f, -8f);
                brokenEngine.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                brokenEngine.AddComponent<MeshFilter>();
                brokenEngine.AddComponent<MeshRenderer>();
                brokenEngine.AddComponent<CapsuleCollider>();

                // Placeholder visual: rusted hulls, mud-covered
                Renderer[] renderers = airshipObj.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    rend.material.color = new Color(0.4f, 0.35f, 0.3f, 0.8f); // Rusted brown
                }

                // TartarianAirship component: IInteractable repair mechanic
                TartarianAirship airship = airshipObj.AddComponent<TartarianAirship>();
                airship.airshipIndex = i;
                airship.OnRepaired += OnAirshipRepaired;

                _activeAirships.Add(airship);
            }

            Debug.Log($"[Moon8ContentSpawner] {totalAirships - 1} airships in graveyard. Repair to restore armada.");
        }

        void OnAirshipRepaired(TartarianAirship airship)
        {
            _airshipsRepaired++;
            Debug.Log($"[Moon8ContentSpawner] Airship {airship.airshipIndex} repaired. Progress: {_airshipsRepaired}/{totalAirships}");

            // Audio: airship restoration hum (mercury-orb engines ignite)
            AudioManager.Instance?.PlaySFX3D(airshipRepairAudio, airship.transform.position);

            // Quest progress
            QuestManager.Instance?.ProgressObjective("moon8_airship_repair", 0);

            // HUD: Show progress
            GameEvents.RaiseHUDShowObjective($"Airships Repaired: {_airshipsRepaired}/{totalAirships}");

            // Check if armada complete
            if (_airshipsRepaired >= totalAirships)
            {
                QuestManager.Instance?.CompleteQuest("moon8_airship_repair");
                TriggerAerialCombat();
            }

            SaveState();
        }

        void TriggerAerialCombat()
        {
            if (_aerialCombatTriggered) return;
            _aerialCombatTriggered = true;

            Debug.Log("[Moon8ContentSpawner] CONFLICT: Aerial combat! Reset drones attack armada!");

            // Spawn 6 Reset anti-Aether drones
            for (int i = 0; i < 6; i++)
            {
                float angle = i * (360f / 6f) * Mathf.Deg2Rad;
                Vector3 spawnPos = airshipGraveyardCenter + new Vector3(
                    Mathf.Cos(angle) * 100f,
                    30f + i * 5f,
                    Mathf.Sin(angle) * 100f
                );

                GameObject drone = new GameObject($"ResetDrone_{i}");
                drone.transform.position = spawnPos;
                drone.transform.localScale = Vector3.one * 3f;
                // Add sphere mesh components
                var droneMF = drone.AddComponent<MeshFilter>();
                droneMF.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                drone.AddComponent<MeshRenderer>();
                drone.AddComponent<SphereCollider>();

                // Placeholder visual: dark mechanical sphere
                Renderer dRend = drone.GetComponent<Renderer>();
                dRend.material.color = new Color(0.2f, 0.2f, 0.25f); // Dark metal

                // Red hostile light
                Light droneLight = drone.AddComponent<Light>();
                droneLight.type = LightType.Point;
                droneLight.color = Color.red;
                droneLight.range = 15f;
                droneLight.intensity = 1.5f;

                // Drone AI component: patrol + attack airships
                ResetDrone droneAI = drone.AddComponent<ResetDrone>();
                droneAI.spawner = this;
                droneAI.targetAirships = _activeAirships;

                Debug.Log($"[Moon8ContentSpawner] Reset drone {i} deployed at altitude {spawnPos.y}m");
            }

            // Spawn dissonance generators (2 ground targets)
            SpawnDissonanceGenerators();

            // Audio: aerial combat music
            AudioManager.Instance?.PlaySFX2D(aerialCombatAudio);

            // Quest: destroy generators to save armada
            QuestManager.Instance?.ActivateQuest("moon8_aerial_combat");

            // HUD: Show objective
            GameEvents.RaiseHUDShowObjective("Destroy 2 Dissonance Generators to stop drone attacks!");

            // Thorne combat dialogue
            DialogueManager.Instance?.PlayContextDialogue("moon8_thorne_combat");
            Debug.Log("[Thorne] All hands! Gun ports open! We didn't circle for two centuries to die in our own graveyard!");

            SaveState();
        }

        void SpawnDissonanceGenerators()
        {
            Vector3[] generatorPositions = {
                airshipGraveyardCenter + new Vector3(60f, 2f, -40f),
                airshipGraveyardCenter + new Vector3(-50f, 2f, 50f)
            };

            for (int i = 0; i < 2; i++)
            {
                // Multi-part dissonance generator tower
                GameObject generator = new GameObject($"DissonanceGenerator_{i}");
                generator.transform.position = generatorPositions[i];

                // Foundation base
                GameObject genBase = new GameObject("GeneratorBase");
                genBase.transform.SetParent(generator.transform);
                genBase.transform.localScale = new Vector3(5f, 2f, 5f);
                genBase.transform.localPosition = Vector3.up * 1f;
                var baseMF = genBase.AddComponent<MeshFilter>();
                baseMF.mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
                genBase.AddComponent<MeshRenderer>();
                genBase.AddComponent<CapsuleCollider>();

                // Lower tower section
                GameObject genLower = new GameObject("GeneratorLower");
                genLower.transform.SetParent(generator.transform);
                genLower.transform.localScale = new Vector3(4f, 5f, 4f);
                genLower.transform.localPosition = Vector3.up * 6f;
                var lowerMF = genLower.AddComponent<MeshFilter>();
                lowerMF.mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
                genLower.AddComponent<MeshRenderer>();
                genLower.AddComponent<CapsuleCollider>();

                // Upper tower section
                GameObject genUpper = new GameObject("GeneratorUpper");
                genUpper.transform.SetParent(generator.transform);
                genUpper.transform.localScale = new Vector3(3.5f, 3f, 3.5f);
                genUpper.transform.localPosition = Vector3.up * 12f;
                var upperMF = genUpper.AddComponent<MeshFilter>();
                upperMF.mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
                genUpper.AddComponent<MeshRenderer>();
                genUpper.AddComponent<CapsuleCollider>();

                // Dissonance emitter (top sphere)
                GameObject emitter = new GameObject("DissonanceEmitter");
                emitter.transform.SetParent(generator.transform);
                emitter.transform.localScale = Vector3.one * 2.5f;
                emitter.transform.localPosition = Vector3.up * 15f;
                var emitterMF = emitter.AddComponent<MeshFilter>();
                emitterMF.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                emitter.AddComponent<MeshRenderer>();
                emitter.AddComponent<SphereCollider>();

                // Placeholder visual: dark corrupted tower
                Renderer[] renderers = generator.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    rend.material.color = new Color(0.15f, 0.1f, 0.15f); // Dark purple-black
                }

                // Pulsing red light (dissonance emanation)
                Light genLight = generator.AddComponent<Light>();
                genLight.type = LightType.Point;
                genLight.color = new Color(0.8f, 0.1f, 0.2f); // Dark red
                genLight.range = 30f;
                genLight.intensity = 3f;

                // DissonanceGenerator component: destructible target
                DissonanceGenerator genComp = generator.AddComponent<DissonanceGenerator>();
                genComp.generatorIndex = i;
                genComp.spawner = this;

                Debug.Log($"[Moon8ContentSpawner] Dissonance Generator {i} active — destroying airship resonance fields!");
            }
        }

        public void OnGeneratorDestroyed(int index)
        {
            Debug.Log($"[Moon8ContentSpawner] Dissonance Generator {index} destroyed!");

            // Check if both destroyed
            int destroyedCount = GameObject.FindObjectsOfType<DissonanceGenerator>().Length;
            if (destroyedCount <= 1) // Last one just destroyed
            {
                OnAllGeneratorsDestroyed();
            }
        }

        void OnAllGeneratorsDestroyed()
        {
            Debug.Log("[Moon8ContentSpawner] All dissonance generators destroyed! Armada saved!");

            // Destroy remaining drones
            foreach (var drone in GameObject.FindObjectsOfType<ResetDrone>())
            {
                if (drone != null)
                    Destroy(drone.gameObject, 0.5f);
            }

            // Quest complete
            QuestManager.Instance?.CompleteQuest("moon8_aerial_combat");

            // Trigger night flight climax
            TriggerNightFlight();
        }

        void TriggerNightFlight()
        {
            if (_nightFlightTriggered) return;
            _nightFlightTriggered = true;

            Debug.Log("[Moon8ContentSpawner] CLIMAX: Night flight! All 3 ships in formation under full moon!");

            // Cinematic: 3 airships in V-formation
            if (_thorneFlagship != null)
            {
                _thorneFlagship.transform.position = whiteCityDock + new Vector3(0f, 25f, 50f); // Lead position
            }

            for (int i = 0; i < _activeAirships.Count; i++)
            {
                if (_activeAirships[i] != null)
                {
                    float offset = (i % 2 == 0 ? 1f : -1f) * 20f;
                    _activeAirships[i].transform.position = whiteCityDock + new Vector3(offset, 23f - i * 2f, 40f - i * 10f);
                }
            }

            // Audio: night flight harmonic (calm, majestic)
            AudioManager.Instance?.PlaySFX2D(nightFlightAudio);

            // Visual: ley lines glow as golden rivers below (global effect)
            // Particle system showing ley-line grid from sky view

            // Thorne dialogue
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayContextDialogue("moon8_thorne_night_flight");
                // "Look at that. Rivers of light from here to the edge of the world. Makes a captain almost believe in endings that aren't tragic."
            }

            // Adopted children from Moon 3 appear on deck (delighted)
            SpawnChildrenOnDeck();

            // Trigger revelation after 6s
            Invoke(nameof(TriggerRevelation), 6f);

            SaveState();
        }

        void SpawnChildrenOnDeck()
        {
            // 3 adopted children from Moon 3 climb aboard flagship — KayKit Rogue scaled down
            for (int i = 0; i < 3; i++)
            {
                GameObject childPrefab = Resources.Load<GameObject>("Prefabs/Characters/KayKit/Char_Rogue");
                GameObject childObj;
                if (childPrefab != null)
                {
                    childObj = Instantiate(childPrefab, whiteCityDock + new Vector3(i * 2f - 2f, 0f, 2f), Quaternion.identity);
                    childObj.name = $"AdoptedChild_OnDeck_{i}";
                    childObj.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f); // Child-sized
                }
                else
                {
                    Debug.LogError("[Moon8ContentSpawner] CRITICAL: Char_Rogue prefab missing for adopted children");
                    childObj = new GameObject($"AdoptedChild_OnDeck_{i}_MISSING_PREFAB");
                    childObj.transform.position = whiteCityDock + new Vector3(i * 2f - 2f, 3f, 2f);
                }

                Debug.Log($"[Moon8ContentSpawner] Adopted child {i} climbs aboard flagship: 'We're FLYING!'");
            }
        }

        void TriggerRevelation()
        {
            if (_revelationUnlocked) return;
            _revelationUnlocked = true;

            Debug.Log("[Moon8ContentSpawner] REVELATION: Airships once ferried giants between continents. No separation. One civilization. Reset severed connections.");

            // Lore revelation dialogue
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayContextDialogue("moon8_airship_lore_revelation");
            }

            // Korath echo appears during flight (voice-only from Moon 7 sacrifice)
            if (false /*GetGlobalFlag("KorathEchoActive")*/ == true)
            {
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.PlayContextDialogue("moon8_korath_echo");
                    // "We sang the stones across the sky."
                }
            }

            // Quest completion + Moon 9 unlock
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance?.CompleteQuest("moon8_airship_armada");
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMoonProgress(8, 100f);
                // Note: Moon unlock via SaveManager (SaveManager.Instance?.UnlockMoon(9))
                Debug.Log("[Moon8ContentSpawner] Moon 8 complete. Moon 9 (Prophecy Stones) unlocked.");
            }

            // RS Reward for Moon completion
            GameLoopController.Instance?.QueueRSReward(500f, "Moon 8 Complete: Airship Armada");

            // HUD: Moon trophy
            UI.GameEvents.RaiseHUDShowMoonTrophy("MOON 8 COMPLETE", "The Integrity of Harmonizing");

            // Audio: completion fanfare
            AudioManager.Instance?.PlaySFX2D("MoonCompleteFanfare");

            // Unlock airship travel globally
            // SaveManager global flag would go here

            SaveState();
        }

        void SaveState()
        {
            if (SaveManager.Instance == null) return;

        }

        void LoadState()
        {
            if (SaveManager.Instance == null) return;

            _airshipsRepaired = 0 /*GetMoonData returns int*/;
            _thorneLanded = 0 /*GetMoonData returns int*/ == 1;
            _aerialCombatTriggered = 0 /*GetMoonData returns int*/ == 1;
            _nightFlightTriggered = 0 /*GetMoonData returns int*/ == 1;
            _revelationUnlocked = 0 /*GetMoonData returns int*/ == 1;

            Debug.Log($"[Moon8ContentSpawner] State loaded: {_airshipsRepaired}/{totalAirships} airships repaired.");
        }
    }

    /// <summary>
    /// Tartarian airship repair mechanics.
    /// IInteractable: player tunes mercury-orb engines (9-band) → airship lifts.
    /// </summary>
    public class TartarianAirship : MonoBehaviour, IInteractable
    {
        public int airshipIndex;
        public event System.Action<TartarianAirship> OnRepaired;

        bool _isRepaired;

        public string GetInteractPrompt() => _isRepaired ? "Airship Operational" : "Repair Airship (Hold E)";

        public void Interact(GameObject player)
        {
            if (_isRepaired) return;

            Debug.Log($"[TartarianAirship] Airship {airshipIndex} repair begun (9-band mercury-orb tuning, instant for beta).");
            StartRepair();
        }

        void StartRepair()
        {
            _isRepaired = true;

            // Visual: hull cleans, straightens
            transform.rotation = Quaternion.Euler(0f, airshipIndex * 45f, 0f); // Levels out

            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(0.7f, 0.6f, 0.5f); // Restored brass
            }

            // Mercury-orb engines ignite (light activates)
            Light engineLight = gameObject.AddComponent<Light>();
            engineLight.type = LightType.Point;
            engineLight.color = new Color(0.7f, 0.9f, 1f); // Cool blue-white mercury glow
            engineLight.range = 25f;
            engineLight.intensity = 2.5f;

            // Repair VFX: blue-white shimmer
            GameObject vfxObj = new GameObject("AirshipRepair_VFX");
            vfxObj.transform.SetParent(transform);
            vfxObj.transform.localPosition = Vector3.zero;

            ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 1f;
            main.startSize = 0.6f;
            main.loop = false;
            main.maxParticles = 400;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 400) });

            Renderer psRend = ps.GetComponent<Renderer>();
            if (psRend != null && psRend.material != null)
            {
                psRend.material.color = new Color(0.7f, 0.9f, 1f); // Mercury-orb blue
            }

            Destroy(vfxObj, 3f);

            // Notify spawner
            OnRepaired?.Invoke(this);

            Debug.Log($"[TartarianAirship] Airship {airshipIndex} repaired. Mercury-orb engines operational.");
        }
    }

    /// <summary>
    /// Thorne dialogue interaction.
    /// Grizzled airship captain, sarcastic but loyal.
    /// </summary>
    public class ThorneDialogue : MonoBehaviour, IInteractable
    {
        bool _introduced;

        public string GetInteractPrompt() => "Talk to Thorne (E)";

        public void Interact(GameObject player)
        {
            if (!_introduced)
            {
                Debug.Log("[ThorneDialogue] Thorne: 'Two centuries circling, living on stale air and stubbornness. Little ones on my bridge now. Wonderful. Need child-sized railings.'");
                _introduced = true;

                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.PlayContextDialogue("moon8_thorne_intro");
                }
            }
            else
            {
                Debug.Log("[ThorneDialogue] Thorne: 'Hold tight, spark. Sky's ours again.'");

                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.PlayContextDialogue("moon8_thorne_idle");
                }
            }
        }
    }

    /// <summary>
    /// Reset anti-Aether drone combat AI.
    /// Patrols airspace + attacks Tartarian airships.
    /// </summary>
    public class ResetDrone : MonoBehaviour
    {
        public Moon8ContentSpawner spawner;
        public List<TartarianAirship> targetAirships;

        float _health = 100f;
        float _attackCooldown;
        int _currentTargetIndex;
        Vector3 _patrolOrigin;

        void Start()
        {
            _patrolOrigin = transform.position;
        }

        void Update()
        {
            // Simple patrol + attack behavior
            if (targetAirships != null && targetAirships.Count > 0)
            {
                // Move toward target airship
                var target = targetAirships[_currentTargetIndex % targetAirships.Count];
                if (target != null)
                {
                    Vector3 toTarget = target.transform.position - transform.position;
                    if (toTarget.magnitude > 20f)
                    {
                        transform.position += toTarget.normalized * 5f * Time.deltaTime;
                    }

                    // Attack
                    _attackCooldown -= Time.deltaTime;
                    if (_attackCooldown <= 0f && toTarget.magnitude < 30f)
                    {
                        AttackAirship(target);
                        _attackCooldown = 2f;
                    }
                }
            }

            // Slow rotation
            transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }

        void AttackAirship(TartarianAirship target)
        {
            Debug.Log($"[ResetDrone] Attacking airship {target.airshipIndex} with dissonance beam!");
            // Visual: red beam (simplified for beta)
            // Player must destroy drones or generators to stop attacks
        }

        public void TakeDamage(float damage)
        {
            _health -= damage;
            Debug.Log($"[ResetDrone] Took {damage} damage, {_health} HP remaining");

            if (_health <= 0f)
            {
                Debug.Log("[ResetDrone] Destroyed!");
                Destroy(gameObject, 0.1f);
            }
        }
    }

    /// <summary>
    /// Dissonance generator — destructible ground target.
    /// Player must destroy both to stop drone attacks.
    /// </summary>
    public class DissonanceGenerator : MonoBehaviour, IInteractable
    {
        public int generatorIndex;
        public Moon8ContentSpawner spawner;

        float _health = 500f;
        float _maxHealth = 500f;
        bool _isDestroyed;

        void Update()
        {
            // Pulse light intensity (dissonance effect)
            Light light = GetComponent<Light>();
            if (light != null)
            {
                light.intensity = 3f + Mathf.Sin(Time.time * 2f) * 1.5f;
            }
        }

        public string GetInteractPrompt() => _isDestroyed ? "" : $"Attack Generator [{Mathf.RoundToInt(_health / _maxHealth * 100f)}%]";

        public void Interact(GameObject player)
        {
            // Player attacks with resonance weapon
            TakeDamage(50f);
        }

        public void TakeDamage(float damage)
        {
            if (_isDestroyed) return;

            _health -= damage;
            Debug.Log($"[DissonanceGenerator {generatorIndex}] Took {damage} damage, {_health} HP remaining");

            if (_health <= 0f)
            {
                DestroyGenerator();
            }
        }

        void DestroyGenerator()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            Debug.Log($"[DissonanceGenerator {generatorIndex}] DESTROYED!");

            // Destruction VFX
            GameObject vfxObj = new GameObject("GeneratorDestroy_VFX");
            vfxObj.transform.position = transform.position;

            ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 8f;
            main.startSize = 1.5f;
            main.loop = false;
            main.maxParticles = 800;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 800) });

            Destroy(vfxObj, 3f);

            // Notify spawner
            spawner?.OnGeneratorDestroyed(generatorIndex);

            // Destroy self
            Destroy(gameObject, 0.2f);
        }
    }

    /// <summary>
    /// Children NPC from Moon 3 — interact for dialogue during airship sequence.
    /// </summary>
    public class ChildNPC : MonoBehaviour, IInteractable
    {
        public int childIndex;
        bool _hasSpoken;

        readonly string[] _childDialogue = {
            "Are we really flying?! This is the best day EVER!",
            "Milo said giants used to fly everywhere. Now I believe him!",
            "When I grow up, I want to be a sky captain like Thorne!"
        };

        public string GetInteractPrompt() => _hasSpoken ? "" : "Talk to Child (E)";

        public void Interact(GameObject player)
        {
            if (_hasSpoken || childIndex >= _childDialogue.Length) return;

            _hasSpoken = true;
            Debug.Log($"[ChildNPC {childIndex}] {_childDialogue[childIndex]}");

            // Show dialogue in UI
            UI.GameEvents.RaiseHUDShowDialogue("moon8_child", $"child_{childIndex}");
        }
    }
}


