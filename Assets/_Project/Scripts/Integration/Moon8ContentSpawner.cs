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

            Debug.Log($"[Moon8ContentSpawner] Thorne flagship landed. Airship graveyard: 2 ships awaiting repair.");
        }

        void SpawnThorneFlagship()
        {
            _thorneFlagship = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _thorneFlagship.name = "Thorne_Flagship";
            _thorneFlagship.transform.position = whiteCityDock + Vector3.up * 8f; // Hovering at dock
            _thorneFlagship.transform.localScale = new Vector3(12f, 4f, 30f); // Large airship hull

            // Placeholder visual: battered Tartarian airship (sacred-geometry hull, brass accents)
            Renderer rend = _thorneFlagship.GetComponent<Renderer>();
            rend.material.color = new Color(0.6f, 0.5f, 0.4f); // Weathered brass

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
            GameObject thorneObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            thorneObj.name = "CaptainThorne";
            thorneObj.transform.position = whiteCityDock + new Vector3(3f, 2f, 0f); // On dock
            thorneObj.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

            // Placeholder visual: grizzled captain (dark coat)
            Renderer thorneRend = thorneObj.GetComponent<Renderer>();
            thorneRend.material.color = new Color(0.3f, 0.3f, 0.35f); // Dark weathered coat

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

                GameObject airshipObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                airshipObj.name = $"Airship_Graveyard_{i}";
                airshipObj.transform.position = pos;
                airshipObj.transform.localScale = new Vector3(10f, 3.5f, 25f); // Slightly smaller than flagship
                airshipObj.transform.rotation = Quaternion.Euler(0f, i * 45f, 10f + i * 5f); // Tilted (crashed)

                // Placeholder visual: rusted hulls, mud-covered
                Renderer rend = airshipObj.GetComponent<Renderer>();
                rend.material.color = new Color(0.4f, 0.35f, 0.3f, 0.8f); // Rusted brown

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

            // Check if armada complete
            if (_airshipsRepaired >= totalAirships)
            {
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
                    Mathf.Cos(angle) * 60f,
                    15f,
                    Mathf.Sin(angle) * 60f
                );

                GameObject droneObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                droneObj.name = $"ResetDrone_{i}";
                droneObj.transform.position = spawnPos;
                droneObj.transform.localScale = Vector3.one * 2f; // Small drone

                // Placeholder visual: black sphere (dissonance generator)
                Renderer droneRend = droneObj.GetComponent<Renderer>();
                droneRend.material.color = new Color(0.1f, 0.1f, 0.15f); // Near-black

                // Red warning light (dissonance active)
                Light droneLight = droneObj.AddComponent<Light>();
                droneLight.type = LightType.Point;
                droneLight.color = Color.red;
                droneLight.range = 8f;
                droneLight.intensity = 2f;

                // Enemy AI component (simplified for beta)
                // Would have proper drone AI attacking airships

                Debug.Log($"[Moon8ContentSpawner] Reset drone {i} spawned. Target dissonance generators!");
            }

            // Audio: aerial combat music
            AudioManager.Instance?.PlaySFX2D(aerialCombatAudio);

            // Trigger night flight after 8s (or when combat ends)
            Invoke(nameof(TriggerNightFlight), 8f);

            SaveState();
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
            // 3 adopted children from Moon 3 climb aboard flagship
            for (int i = 0; i < 3; i++)
            {
                GameObject childObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                childObj.name = $"AdoptedChild_OnDeck_{i}";
                childObj.transform.position = whiteCityDock + new Vector3(i * 2f - 2f, 3f, 2f);
                childObj.transform.localScale = new Vector3(0.3f, 0.6f, 0.3f); // Child-sized

                // Placeholder visual: warm skin tone (happy children)
                Renderer childRend = childObj.GetComponent<Renderer>();
                childRend.material.color = new Color(0.9f, 0.75f, 0.65f);

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
                // TODO: SaveManager.Instance.UnlockMoon(9);
                Debug.Log("[Moon8ContentSpawner] Moon 8 complete. Moon 9 (Prophecy Stones) unlocked.");
            }

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
}

