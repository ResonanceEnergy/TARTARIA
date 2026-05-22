using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Save;
using Tartaria.UI;

namespace Tartaria.Integration
{
    // ============================================================
    // MOON 8: GALACTIC MOON — "The Integrity of Harmonizing"
    // Galactic Tone: Integrity / Harmonizing
    // Scene prefix: "GalacticMoon" | "SkyIsles" | "AirshipGraveyard"
    // 5-Beat: Discovery → Restoration → Conflict → Climax → Revelation
    // Key: Captain Thorne arrives; repair 3-ship armada; aerial combat; megalith transport
    // ============================================================

    public class Moon8GalacticArc : MonoBehaviour
    {
        public static Moon8GalacticArc Instance { get; private set; }

        private const int MOON_NUM = 8;

        private const int BEAT_DISCOVERY   = 0;
        private const int BEAT_RESTORATION = 1;
        private const int BEAT_CONFLICT    = 2;
        private const int BEAT_CLIMAX      = 3;
        private const int BEAT_REVELATION  = 4;

        [Header("Airship Refs")]
        [SerializeField] private GameObject _thorneFlagship;
        [SerializeField] private GameObject _airship2;
        [SerializeField] private GameObject _airship3;
        [SerializeField] private Transform  _airshipDockPoint;
        [SerializeField] private GameObject _thorneCompanionProxy;

        [Header("Sky Isles")]
        [SerializeField] private Transform[] _megaliths;
        [SerializeField] private GameObject _antigravFieldFX;

        private bool _thorneArrived;
        private int  _shipsRepaired;   // 0-3
        private bool _aerialCombatWon;
        private bool _nightFlightComplete;
        private bool _loreCrystalFound;
        private bool _moonCleared;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!scene.StartsWith("GalacticMoon") &&
                !scene.StartsWith("SkyIsles") &&
                !scene.StartsWith("AirshipGraveyard"))
                return;

            var go = new GameObject("Moon8GalacticArc");
            go.AddComponent<Moon8GalacticArc>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            RestoreStateFromSave();
            StartCoroutine(RunArc());
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void RestoreStateFromSave()
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) return;

            _thorneArrived      = save.GetMoonFlag(MOON_NUM, "thorne_arrived");
            _shipsRepaired      = save.GetMoonFlag(MOON_NUM, "ships_repaired_int", 0);
            _aerialCombatWon    = save.GetMoonFlag(MOON_NUM, "aerial_combat_won");
            _nightFlightComplete= save.GetMoonFlag(MOON_NUM, "night_flight_complete");
            _loreCrystalFound   = save.GetMoonFlag(MOON_NUM, "lore_crystal_found");
            _moonCleared        = save.GetMoonFlag(MOON_NUM, "moon_cleared");

            if (_moonCleared)
                ApplyPersistentWorldState();
        }

        private void ApplyPersistentWorldState()
        {
            if (_thorneArrived && _thorneFlagship != null)
                _thorneFlagship.SetActive(true);
            if (_shipsRepaired >= 3)
            {
                if (_airship2 != null) _airship2.SetActive(true);
                if (_airship3 != null) _airship3.SetActive(true);
            }
            if (_moonCleared)
                HUDController.Instance?.ShowObjective(
                    "<b>GALACTIC MOON — ARMADA FLIES</b>\nAll three ships operational. Sky routes open. " +
                    "Thorne stands permanent watch on the bridge.");
        }

        private IEnumerator RunArc()
        {
            if (_moonCleared) yield break;

            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) yield break;

            if (!_thorneArrived)        yield return StartCoroutine(Beat1_Discovery());
            if (_shipsRepaired < 3)     yield return StartCoroutine(Beat2_Restoration());
            if (!_aerialCombatWon)      yield return StartCoroutine(Beat3_Conflict());
            if (!_nightFlightComplete)  yield return StartCoroutine(Beat4_Climax());
            if (!_loreCrystalFound)     yield return StartCoroutine(Beat5_Revelation());
        }

        // ─── Beat 1: Discovery ─────────────────────────────────────
        private IEnumerator Beat1_Discovery()
        {
            AudioManager.Instance?.PlaySFX2D("airship_descent_engines");

            HUDController.Instance?.ShowBanner(
                "GALACTIC MOON — THORNE ARRIVES",
                "A battered Tartarian flagship descends through the clouds to your White City dock. " +
                "Sacred-geometry hull, mercury-orb engines (cold), a bridge sized for giants.",
                8f);
            HUDController.Instance?.ShowObjective("Guide Captain Thorne's flagship to the airship dock.");

            if (_thorneFlagship != null)
            {
                _thorneFlagship.SetActive(true);
                // Animate descent
                StartCoroutine(AnimateDescentToPoint(_thorneFlagship.transform, _airshipDockPoint));
            }

            yield return new WaitForSeconds(5f);

            _thorneArrived = true;
            if (_thorneCompanionProxy != null) _thorneCompanionProxy.SetActive(true);

            HUDController.Instance?.ShowBanner(
                "CAPTAIN THORNE",
                "\"Two centuries circling, living on stale air and stubbornness. " +
                "This bucket flies like it's still offended we dug it out of the mud.\"",
                9f);
            AudioManager.Instance?.PlaySFX2D("thorne_arrival_vo");

            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "thorne_arrived", true);
            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_DISCOVERY);
            GameEvents.FireCriticalSaveTrigger("thorne_companion_joined");
            GameEvents.FireCriticalSaveTrigger("moon8_discovery_complete");
        }

        private IEnumerator AnimateDescentToPoint(Transform ship, Transform dock)
        {
            if (dock == null || ship == null) yield break;
            float t = 0f;
            var startPos = ship.position;
            var endPos   = dock.position;
            while (t < 1f)
            {
                t += Time.deltaTime * 0.15f;
                ship.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }
        }

        // ─── Beat 2: Restoration ───────────────────────────────────
        private IEnumerator Beat2_Restoration()
        {
            HUDController.Instance?.ShowObjective(
                "Repair the airship armada (3 ships scattered in the graveyard zone). Tune mercury-orb anti-grav engines with 9-band Aether. [0/3]");

            AudioManager.Instance?.PlaySFX2D("airship_graveyard_ambience");

            var repairPoints = FindObjectsOfType<Moon8ShipRepairPoint>();
            foreach (var rp in repairPoints)
                rp.OnRepaired += OnShipRepaired;

            yield return new WaitUntil(() => _shipsRepaired >= 3);

            HUDController.Instance?.ShowBanner(
                "ARMADA OPERATIONAL",
                "All three ships airworthy. Anti-grav mercury-orb engines humming. " +
                "The children from Moon 3 have already claimed the bridge deck.",
                7f);
            AudioManager.Instance?.PlaySFX2D("armada_launch_fanfare");

            HUDController.Instance?.ShowBanner(
                "THORNE",
                "\"Little ones on my bridge. Wonderful. Now I need child-sized railings. And more patience.\"",
                6f);
            AudioManager.Instance?.PlaySFX2D("thorne_children_vo");

            // Megalith transport demo
            yield return new WaitForSeconds(2f);
            HUDController.Instance?.ShowBanner(
                "MEGALITH TRANSPORT",
                "Giant-mode rock cutting → airship anti-grav field lifts 300-ton stones → fly them to construction sites. " +
                "Korath's echo: \"We sang the stones across the sky.\"",
                9f);
            AudioManager.Instance?.PlaySFX2D("korath_echo_megalith_vo");
            GameEvents.FireCriticalSaveTrigger("megalith_transport_unlocked");

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_RESTORATION);
            GameEvents.FireCriticalSaveTrigger("moon8_restoration_complete");
        }

        private void OnShipRepaired(int shipIndex)
        {
            _shipsRepaired = Mathf.Max(_shipsRepaired, shipIndex + 1);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "ships_repaired_int", _shipsRepaired);
            HUDController.Instance?.ShowObjective($"Repair armada: [{_shipsRepaired}/3]");
            HUDController.Instance?.ShowBanner(
                $"SHIP {shipIndex + 1} REPAIRED",
                "Mercury-orb engine purring. Anti-grav lift confirmed.",
                5f);
            AudioManager.Instance?.PlaySFX2D("engine_restart_tone");
            GameEvents.FireCriticalSaveTrigger($"moon8_ship_{shipIndex + 1}_repaired");
        }

        // ─── Beat 3: Conflict ──────────────────────────────────────
        private IEnumerator Beat3_Conflict()
        {
            HUDController.Instance?.ShowBanner(
                "GALACTIC MOON — CONFLICT",
                "Reset supply lines detected. Aerial combat: resonance cannons vs. Reset anti-Aether drones. " +
                "Target their dissonance generators to disable entire squadrons.",
                8f);
            HUDController.Instance?.ShowObjective(
                "Raid Reset supply lines. Destroy 3 drone squadrons with airship resonance cannons.");

            AudioManager.Instance?.PlaySFX2D("aerial_combat_alarm");

            HUDController.Instance?.ShowBanner(
                "THORNE (mid-dogfight)",
                "\"Hold tight, spark. We're about to remind these parasites what it feels like to be small.\"",
                6f);
            AudioManager.Instance?.PlaySFX2D("thorne_dogfight_vo");

            var combatSystem = FindObjectOfType<BossEncounterSystem>();
            if (combatSystem != null)
                combatSystem.SpawnBoss("moon8_reset_drone_squadron_commander");

            yield return new WaitUntil(() =>
                SaveManager.Instance?.CurrentSave?.GetMoonFlag(MOON_NUM, "aerial_combat_won") ?? false);

            _aerialCombatWon = true;
            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CONFLICT);
            GameEvents.FireCriticalSaveTrigger("moon8_conflict_complete");
        }

        // ─── Beat 4: Climax ────────────────────────────────────────
        private IEnumerator Beat4_Climax()
        {
            HUDController.Instance?.ShowBanner(
                "NIGHT FLIGHT — ALL THREE SHIPS",
                "Formation flight under the full moon. Below: ley lines glow as golden rivers threading through the dark continent.",
                8f);
            HUDController.Instance?.ShowObjective(
                "Complete the night formation flight across the restored zones.");

            AudioManager.Instance?.PlaySFX2D("moon8_night_flight_music");

            yield return new WaitForSeconds(8f);

            HUDController.Instance?.ShowBanner(
                "THORNE (quiet)",
                "\"Look at that. Rivers of light from here to the edge of the world. " +
                "Makes a captain almost believe in endings that aren't tragic.\"",
                10f);
            AudioManager.Instance?.PlaySFX2D("thorne_quiet_reflection_vo");

            _nightFlightComplete = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "night_flight_complete", true);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_CLIMAX);
            GameEvents.FireCriticalSaveTrigger("moon8_climax_complete");
        }

        // ─── Beat 5: Revelation ────────────────────────────────────
        private IEnumerator Beat5_Revelation()
        {
            yield return new WaitForSeconds(2f);

            HUDController.Instance?.ShowBanner(
                "LORE REVELATION",
                "Airships once ferried giants between continents — one civilization connected by sky and rail. " +
                "The Reset's greatest crime was not destroying buildings but severing connections.",
                10f);
            AudioManager.Instance?.PlaySFX2D("moon8_lore_reveal_vo");

            _loreCrystalFound = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "lore_crystal_found", true);

            // Crossover seeds
            GameEvents.FireCriticalSaveTrigger("moon8_seed_children_airship_crew");
            GameEvents.FireCriticalSaveTrigger("moon8_seed_airship_train_network");
            GameEvents.FireCriticalSaveTrigger("moon8_seed_korath_echo_megalith");
            GameEvents.FireCriticalSaveTrigger("moon8_seed_fast_travel_backbone");

            _moonCleared = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "moon_cleared", true);

            MoonProgressTracker.Instance?.MarkBeatCleared(MOON_NUM, BEAT_REVELATION);
            MoonProgressTracker.Instance?.MarkCleared(MOON_NUM);
            GameEvents.FireCriticalSaveTrigger("moon8_complete");

            HUDController.Instance?.ShowBanner(
                "GALACTIC MOON — COMPLETE",
                "The armada flies. The sky routes are open. The grid grows.",
                7f);
            AudioManager.Instance?.PlaySFX2D("moon8_completion_sting");

            ApplyPersistentWorldState();
        }
    }

    // ─── Repair Point Helper ────────────────────────────────────────────
    public class Moon8ShipRepairPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _shipIndex;
        public event System.Action<int> OnRepaired;
        private bool _repaired;

        public void Interact(GameObject interactor)
        {
            if (_repaired) return;
            _repaired = true;
            OnRepaired?.Invoke(_shipIndex);
            gameObject.SetActive(false);
        }

        public string GetInteractPrompt() => $"Tune Mercury-Orb Engine — Ship {_shipIndex + 1}";
        public bool CanInteract(GameObject interactor) => !_repaired;
    }
}
