using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Save;

namespace Tartaria.Integration
{
    // ============================================================
    // MOON 5: OVERTONE MOON -- "The Radiance of Empowerment"
    // Galactic Tone: Radiance / Empowerment
    // Scene prefix: "WhiteCity" | "OvertoneDistrict" | "Moon05"
    // 5-Beat vertical slice: Discovery -> Restoration -> Conflict -> Climax -> Revelation
    // Key: Buried White City reveal, Captain Thorne intro (radio), 6-band healing auras,
    //      airship dock construction, Moon 1 spire fragment completion, Milo jaw-drop
    // New Mechanic: Floating platforms + airship dock construction
    // Companion Focus: Thorne introduction as distant radio voice
    // Aether Band: 6-Band introduction (healing, growth)
    // ============================================================

    public class Moon5OvertoneArc : MonoBehaviour
    {
        public static Moon5OvertoneArc Instance { get; private set; }

        private const int MOON_NUM = 5;

        private const int BEAT_DISCOVERY   = 0;
        private const int BEAT_RESTORATION = 1;
        private const int BEAT_CONFLICT    = 2;
        private const int BEAT_CLIMAX      = 3;
        private const int BEAT_REVELATION  = 4;

        // --- Scene refs ---
        [Header("White City Pavilions (5)")]
        [SerializeField] private GameObject[] _pavilionGOs;
        [SerializeField] private GameObject[] _pavilionGlowFX;
        [SerializeField] private Transform    _whiteCityCenter;

        [Header("Spire Fragment (from Moon 1)")]
        [SerializeField] private GameObject _spireFragmentGO;
        [SerializeField] private GameObject _spireCompleteFX;
        [SerializeField] private Transform  _spireCompletionPoint;

        [Header("Floating Platforms")]
        [SerializeField] private GameObject  _floatingPlatformPrefab;
        [SerializeField] private Transform[] _platformSpawnPoints;

        [Header("Airship Dock")]
        [SerializeField] private GameObject _airshipDockBlueprintGO;
        [SerializeField] private GameObject _airshipDockBuiltGO;
        [SerializeField] private Transform  _dockCenter;

        [Header("Thorne Radio SFX")]
        [SerializeField] private AudioClip _thorneRadioClip;
        [SerializeField] private AudioClip _thorneSignalStrongClip;

        [Header("Pure Water Fountains")]
        [SerializeField] private GameObject[] _fountainGOs;
        [SerializeField] private GameObject   _fountainAuroraFX;

        [Header("Conflict -- Reset Demolition")]
        [SerializeField] private GameObject _resetCrewPrefab;
        [SerializeField] private int        _resetCrewCount = 4;
        [SerializeField] private Transform  _resetSpawnPoint;

        [Header("World's Fair Hologram")]
        [SerializeField] private GameObject _worldsFairHologramFX;

        [Header("Lore Fragment")]
        [SerializeField] private GameObject _demolitionOrderScrollPrefab;
        [SerializeField] private Transform  _scrollSpawnPoint;

        // --- Runtime state ---
        private int  _currentBeat;
        private int  _pavilionsRestored;
        private bool _spireCompleted;
        private bool _dockBuilt;
        private int  _resetCrewKilled;

        private static readonly string[] FORWARD_SEEDS = {
            "moon5_seed_airship_dock_built",
            "moon5_seed_fair_circuit_live_ops",
            "moon5_seed_diaries_coded_letters",
            "moon5_seed_6band_healing_all_zones",
            "moon5_seed_thorne_signal_locked_on",
        };

        // --- Bootstrap ---
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!scene.Contains("WhiteCity") && !scene.Contains("Moon05") && !scene.Contains("OvertoneDistrict")) return;
            if (Instance != null) return;
            var go = new GameObject("[Moon5OvertoneArc]");
            go.AddComponent<Moon5OvertoneArc>();
            DontDestroyOnLoad(go);
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            var save = SaveManager.Instance?.CurrentSave;
            _currentBeat      = save?.GetMoonFlag(MOON_NUM, "beat", 0) ?? 0;
            _pavilionsRestored = save?.GetMoonFlag(MOON_NUM, "pavilions", 0) ?? 0;
            _spireCompleted   = save?.GetMoonFlag(MOON_NUM, "spire_complete") ?? false;
            bool cleared      = save?.GetMoonFlag(MOON_NUM, "moon_cleared") ?? false;

            Debug.Log($"[Moon5] Arc booted. Beat={_currentBeat}, Pavilions={_pavilionsRestored}, Cleared={cleared}");

            if (cleared)
            {
                GameEvents.RaiseHUDShowObjective(
                    "<b>OVERTONE MOON -- THE WHITE CITY SHINES</b>\nThorne circles above. Airship dock ready.");
                return;
            }
            StartCoroutine(RunArc());
        }

        // --- Main arc coroutine ---
        private IEnumerator RunArc()
        {
            yield return new WaitForSeconds(2f);
            if (_currentBeat <= BEAT_DISCOVERY)   yield return StartCoroutine(Beat1_Discovery());
            if (_currentBeat <= BEAT_RESTORATION) yield return StartCoroutine(Beat2_Restoration());
            if (_currentBeat <= BEAT_CONFLICT)    yield return StartCoroutine(Beat3_Conflict());
            if (_currentBeat <= BEAT_CLIMAX)      yield return StartCoroutine(Beat4_Climax());
            if (_currentBeat <= BEAT_REVELATION)  yield return StartCoroutine(Beat5_Revelation());
        }

        // --- Beat 1: Discovery ---
        // Grid ~30% reveals buried White City. Thorne's first crackling radio contact.
        private IEnumerator Beat1_Discovery()
        {
            GameEvents.RaiseHUDShowBanner(
                "OVERTONE MOON -- DISCOVERY",
                "Your strengthened grid reveals the buried White City -- 1893 World's Fair pavilions glowing under the Overtone Moon.",
                7f);
            GameEvents.RaiseHUDShowObjective("Explore the White City. Something is circling overhead.");

            AudioManager.Instance?.PlaySFX2D("moon5_white_city_emerge_fanfare");
            AudioManager.Instance?.PlaySFX2D("moon5_ambient_aurora_hum");

            if (_pavilionGOs != null)
                foreach (var p in _pavilionGOs)
                    if (p != null) p.SetActive(true);

            if (_fountainGOs != null)
                foreach (var f in _fountainGOs)
                    if (f != null) f.SetActive(true);

            if (_fountainAuroraFX != null) _fountainAuroraFX.SetActive(true);

            yield return new WaitForSeconds(2f);

            // Thorne's crackling radio signal
            PlayRadioSFX(_thorneRadioClip, "moon5_thorne_first_contact_vo");
            GameEvents.RaiseHUDShowObjective("Explore the White City pavilions. Thorne watches from 10,000 ft.");

            yield return WaitForPlayerProximity(_whiteCityCenter, 15f, 60f);

            AudioManager.Instance?.PlaySFX2D("milo_white_city_jaw_drop_vo");
            ClearBeat(BEAT_DISCOVERY);
        }

        // --- Beat 2: Restoration ---
        // Restore 5 pavilions using golden-ratio templates. 6-band healing domes activate.
        // Airship dock blueprints found in pavilion basement.
        private IEnumerator Beat2_Restoration()
        {
            GameEvents.RaiseHUDShowObjective($"Restore the 5 White City Pavilions and activate 6-band healing.");
            AudioManager.Instance?.PlaySFX2D("moon5_restoration_music");

            // Spawn floating platforms as progress prop
            if (_floatingPlatformPrefab != null && _platformSpawnPoints != null)
                foreach (var sp in _platformSpawnPoints)
                    Instantiate(_floatingPlatformPrefab, sp.position, sp.rotation);

            // Wait for pavilion restorations (proximity-driven simulation)
            float elapsed = 0f;
            while (_pavilionsRestored < 5 && elapsed < 600f)
            {
                yield return new WaitForSeconds(4f);
                elapsed += 4f;

                if (_whiteCityCenter != null)
                {
                    var player = GameObject.FindWithTag("Player");
                    if (player != null &&
                        Vector3.Distance(player.transform.position, _whiteCityCenter.position) < 30f)
                    {
                        _pavilionsRestored = Mathf.Min(5, _pavilionsRestored + 1);
                        ActivatePavilionGlow(_pavilionsRestored - 1);
                        SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "pavilions", _pavilionsRestored);
                        GameEvents.RaiseHUDShowObjective($"Pavilions restored: {_pavilionsRestored}/5");
                    }
                }
            }

            // Reveal airship dock blueprints
            if (_airshipDockBlueprintGO != null) _airshipDockBlueprintGO.SetActive(true);
            AudioManager.Instance?.PlaySFX2D("milo_airship_blueprints_vo");
            GameEvents.RaiseHUDShowObjective("Blueprints found! Defend the pavilions from demolition crews.");
            yield return new WaitForSeconds(2f);
            ClearBeat(BEAT_RESTORATION);
        }

        // --- Beat 3: Conflict ---
        // Reset demolition crews attack. Defend with 6-band healing aura keeping buildings alive.
        private IEnumerator Beat3_Conflict()
        {
            GameEvents.RaiseHUDShowBanner(
                "OVERTONE MOON -- CONFLICT",
                "Reset demolition crews attack! 'Pavilion 7-12: dismantled by March 1894. Claim structural insufficiency.'",
                6f);
            GameEvents.RaiseHUDShowObjective(
                "Defend the pavilions! Use 6-band healing aura to keep them alive while you fight.");
            AudioManager.Instance?.PlaySFX2D("moon5_reset_crew_arrival_sting");
            AudioManager.Instance?.PlaySFX2D("moon5_combat_music");

            if (_resetCrewPrefab != null && _resetSpawnPoint != null)
            {
                for (int i = 0; i < _resetCrewCount; i++)
                {
                    Vector3 offset = new Vector3(
                        UnityEngine.Random.Range(-8f, 8f), 0f,
                        UnityEngine.Random.Range(-8f, 8f));
                    var crew = Instantiate(_resetCrewPrefab, _resetSpawnPoint.position + offset, Quaternion.identity);
                    crew.name = "ResetDemolitionCrew_" + i;
                }
            }

            // Activate pavilion healing glow FX
            if (_pavilionGlowFX != null)
                foreach (var fx in _pavilionGlowFX)
                    if (fx != null) fx.SetActive(true);

            _resetCrewKilled = 0;
            MudGolemHealth.OnAnyGolemDied += OnEnemyKilled;

            float timeout = 240f;
            float elapsed = 0f;
            while (_resetCrewKilled < _resetCrewCount && elapsed < timeout)
            {
                yield return new WaitForSeconds(2f);
                elapsed += 2f;
            }
            MudGolemHealth.OnAnyGolemDied -= OnEnemyKilled;

            // Build the airship dock
            if (_airshipDockBlueprintGO != null) _airshipDockBlueprintGO.SetActive(false);
            if (_airshipDockBuiltGO     != null) _airshipDockBuiltGO.SetActive(true);
            _dockBuilt = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "dock_built", true);

            AudioManager.Instance?.PlaySFX2D("milo_outraged_history_vo");
            GameEvents.RaiseHUDShowObjective("Demolition crew repelled! Airship dock under construction.");
            yield return new WaitForSeconds(3f);
            ClearBeat(BEAT_CONFLICT);
        }

        // --- Beat 4: Climax ---
        // 5 pavilions fully lit -> ionized fountain aurora replays World's Fair festival holograms.
        private IEnumerator Beat4_Climax()
        {
            GameEvents.RaiseHUDShowBanner(
                "OVERTONE MOON -- CLIMAX",
                "Five pavilions restored! Ionized fountain auroras replay pre-flood festivals. Giants and humans celebrating together.",
                7f);
            AudioManager.Instance?.PlaySFX2D("moon5_world_fair_hologram_fanfare");

            yield return new WaitForSeconds(2f);

            // Full brightness on all pavilions
            if (_pavilionGOs != null)
                foreach (var p in _pavilionGOs)
                {
                    if (p == null) continue;
                    var r = p.GetComponentInChildren<Renderer>();
                    if (r != null) r.material.color = Color.white;
                }

            // World's Fair hologram aurora replay
            if (_worldsFairHologramFX != null) _worldsFairHologramFX.SetActive(true);

            AudioManager.Instance?.PlaySFX2D("milo_bulldozed_outrage_vo");
            yield return new WaitForSeconds(3f);

            // Thorne's signal strengthens
            PlayRadioSFX(_thorneSignalStrongClip, "moon5_thorne_signal_strong_vo");

            ServiceLocator.MoonProgress?.MarkBeatCleared(MOON_NUM, BEAT_CLIMAX);
            GameEvents.RaiseHUDShowObjective("Thorne is inbound. The airship dock awaits.");
            yield return new WaitForSeconds(5f);
            ClearBeat(BEAT_CLIMAX);
        }

        // --- Beat 5: Revelation ---
        // Moon 1 spire fragment placed -> first multi-zone ley-line corridor.
        // Fair diaries encode Moon 9 coded letters. Forward seeds planted.
        private IEnumerator Beat5_Revelation()
        {
            GameEvents.RaiseHUDShowObjective(
                "Revelation: Place the Spire Fragment to bridge the ley-line corridor between two zones.");
            AudioManager.Instance?.PlaySFX2D("moon5_revelation_music");

            // Spawn lore demolition order scroll
            if (_demolitionOrderScrollPrefab != null && _scrollSpawnPoint != null)
                Instantiate(_demolitionOrderScrollPrefab, _scrollSpawnPoint.position, _scrollSpawnPoint.rotation);

            yield return WaitForPlayerProximity(_spireCompletionPoint, 6f, 120f);

            // Place Moon 1 spire fragment
            if (_spireFragmentGO != null)
            {
                _spireFragmentGO.transform.position = _spireCompletionPoint != null
                    ? _spireCompletionPoint.position
                    : transform.position + Vector3.up * 10f;
                _spireFragmentGO.SetActive(true);
            }
            if (_spireCompleteFX != null) _spireCompleteFX.SetActive(true);

            _spireCompleted = true;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "spire_complete", true);

            AudioManager.Instance?.PlaySFX2D("moon5_spire_completion_fanfare");
            AudioManager.Instance?.PlaySFX2D("milo_demolition_order_lore_vo");
            yield return new WaitForSeconds(4f);

            PlantForwardSeeds();

            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
            {
                save.SetMoonFlag(MOON_NUM, "moon_cleared", true);
                save.SetMoonFlag(MOON_NUM, "beat", BEAT_REVELATION);
            }
            ServiceLocator.MoonProgress?.MarkCleared(MOON_NUM);

            GameEvents.RaiseHUDShowObjective(
                "<b>MOON 5 COMPLETE!</b>\nThe White City shines. Captain Thorne descends.");
            Debug.Log("[Moon5] COMPLETE. Airship dock built. Thorne inbound for Moon 8.");
        }

        // --- Helpers ---
        private void ClearBeat(int beat)
        {
            _currentBeat = beat + 1;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "beat", _currentBeat);
            ServiceLocator.MoonProgress?.MarkBeatCleared(MOON_NUM, beat);
        }

        private void ActivatePavilionGlow(int index)
        {
            if (_pavilionGlowFX == null || index < 0 || index >= _pavilionGlowFX.Length) return;
            if (_pavilionGlowFX[index] != null) _pavilionGlowFX[index].SetActive(true);
            Debug.Log($"[Moon5] Pavilion {index + 1} glow activated.");
        }

        private void OnEnemyKilled(MudGolemHealth _) => _resetCrewKilled++;

        private void PlayRadioSFX(AudioClip clip, string fallbackKey)
        {
            if (clip != null)
            {
                var go = new GameObject("ThorneRadio_Temp");
                var src = go.AddComponent<AudioSource>();
                src.clip = clip;
                src.spatialBlend = 0f;
                src.volume = 0.75f;
                src.Play();
                Destroy(go, clip.length + 0.5f);
            }
            AudioManager.Instance?.PlaySFX2D(fallbackKey);
        }

        private IEnumerator WaitForPlayerProximity(Transform target, float radius, float timeout)
        {
            if (target == null) { yield return new WaitForSeconds(5f); yield break; }
            float t = 0f;
            while (t < timeout)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null && Vector3.Distance(player.transform.position, target.position) < radius)
                    yield break;
                yield return new WaitForSeconds(1f);
                t += 1f;
            }
        }

        private void PlantForwardSeeds()
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) return;
            foreach (var seed in FORWARD_SEEDS)
            {
                save.SetMoonFlag(MOON_NUM, seed, true);
                Debug.Log($"[Moon5] Seed planted: {seed}");
            }
        }
    }
}
