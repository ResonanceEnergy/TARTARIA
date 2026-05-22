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
    // MOON 3: ELECTRIC MOON — "The Spark of Service"
    // Galactic Tone: Service / Bonding
    // Scene prefix: "BuriedRailJunction" | "Settlement" | "Moon03"
    // 5-Beat vertical slice: Discovery -> Restoration -> Conflict -> Climax -> Revelation
    // Key: Lirael backstory reveal, Orphan Train adoption, 432 Hz lullaby crystal,
    //      cymatic rail gardens, junior architect NPCs
    // New Mechanic: Resonance trains + orphan adoption
    // Aether Band: 3-Band mastery + train-specific power
    // ============================================================

    public class Moon3ElectricArc : MonoBehaviour
    {
        public static Moon3ElectricArc Instance { get; private set; }

        private const int MOON_NUM = 3;

        private const int BEAT_DISCOVERY   = 0;
        private const int BEAT_RESTORATION = 1;
        private const int BEAT_CONFLICT    = 2;
        private const int BEAT_CLIMAX      = 3;
        private const int BEAT_REVELATION  = 4;

        // --- Scene refs ---
        [Header("Orphan Train")]
        [SerializeField] private GameObject _spectralTrainGO;
        [SerializeField] private Transform  _trainDeparturePlatform;
        [SerializeField] private GameObject _orphanChildPrefab;
        [SerializeField] private Transform[] _orphanSpawnPoints;

        [Header("Lirael")]
        [SerializeField] private Transform  _liraelStandPoint;
        [SerializeField] private GameObject _liraelTearsFX;

        [Header("Rail Segment")]
        [SerializeField] private Transform[] _railTiePlacements;
        [SerializeField] private GameObject  _cymaticGardenPrefab;
        [SerializeField] private Transform[] _gardenSpawnPoints;

        [Header("Conflict -- Train Derail")]
        [SerializeField] private Transform  _derailPoint;
        [SerializeField] private GameObject _mudGolemPrefab;
        [SerializeField] private int        _derailGolemCount = 5;

        [Header("Climax -- Lullaby Activation")]
        [SerializeField] private GameObject _goldenTrainGO;
        [SerializeField] private GameObject _trainGoldenFX;
        [SerializeField] private AudioClip  _lullabyCrystalSFX;
        [SerializeField] private GameObject _lullabyCrystalItem;

        [Header("Revelation")]
        [SerializeField] private GameObject _loreScrollPrefab;
        [SerializeField] private Transform  _loreScrollSpawnPoint;

        // --- Runtime state ---
        private int  _currentBeat;
        private bool _trainSolidified;
        private int  _adoptedChildCount;
        private int  _railTiesLaid;
        private int  _golemsKilled;

        private static readonly string[] FORWARD_SEEDS = {
            "moon3_seed_orphan_train_aboard",
            "moon3_seed_children_train_operators",
            "moon3_seed_lirael_strength_growing",
            "moon3_seed_lullaby_crystal_pipe_organ",
            "moon3_seed_rail_junction_established",
        };

        // --- Bootstrap ---
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!scene.Contains("BuriedRail") && !scene.Contains("Moon03") && !scene.Contains("ElectricMoon")) return;
            if (Instance != null) return;
            var go = new GameObject("[Moon3ElectricArc]");
            go.AddComponent<Moon3ElectricArc>();
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
            _currentBeat       = save?.GetMoonFlag(MOON_NUM, "beat", 0) ?? 0;
            _adoptedChildCount = save?.GetMoonFlag(MOON_NUM, "adopted", 0) ?? 0;
            _railTiesLaid      = save?.GetMoonFlag(MOON_NUM, "railties", 0) ?? 0;
            bool cleared       = save?.GetMoonFlag(MOON_NUM, "moon_cleared") ?? false;
            Debug.Log($"[Moon3] Arc booted. Beat={_currentBeat}, Adopted={_adoptedChildCount}, Cleared={cleared}");

            if (cleared)
            {
                HUDController.Instance?.ShowObjective(
                    "<b>ELECTRIC MOON -- THE ORPHAN RAILS RUN FREE</b>\nThe children remember their song.");
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
        // Spectral Orphan Train materializes. Lirael recognition moment.
        private IEnumerator Beat1_Discovery()
        {
            HUDController.Instance?.ShowBanner(
                "ELECTRIC MOON -- DISCOVERY",
                "A spectral train materializes on dormant rails. Aboard: ghost children in Victorian clothing.",
                6f);
            HUDController.Instance?.ShowObjective("The Orphan Train has appeared. Approach the platform.");

            AudioManager.Instance?.PlaySFX2D("moon3_rail_junction_ambience");
            AudioManager.Instance?.PlaySFX2D("spectral_train_approach");

            if (_spectralTrainGO != null) _spectralTrainGO.SetActive(true);
            if (_orphanChildPrefab != null && _orphanSpawnPoints != null)
                foreach (var sp in _orphanSpawnPoints)
                    Instantiate(_orphanChildPrefab, sp.position, sp.rotation);

            yield return new WaitForSeconds(2f);

            if (_liraelTearsFX != null) _liraelTearsFX.SetActive(true);
            AudioManager.Instance?.PlaySFX2D("lirael_recognition_vo");
            HUDController.Instance?.ShowObjective("Approach the Orphan Train with Lirael.");

            yield return WaitForPlayerProximity(_trainDeparturePlatform, 8f, 45f);

            AudioManager.Instance?.PlaySFX2D("lirael_orphan_truth_vo");
            ClearBeat(BEAT_DISCOVERY);
        }

        // --- Beat 2: Restoration ---
        // Lay rail ties, tune cymatic gardens, adopt orphan children as junior architects.
        private IEnumerator Beat2_Restoration()
        {
            int totalTies = _railTiePlacements?.Length ?? 5;
            HUDController.Instance?.ShowObjective(
                $"Restore the Rail: lay {totalTies} rail segments and tune cymatic gardens.");
            AudioManager.Instance?.PlaySFX2D("moon3_rail_restoration_music");

            if (_railTiePlacements != null)
                foreach (var pos in _railTiePlacements)
                    SpawnInteractableRailTie(pos);

            if (_cymaticGardenPrefab != null && _gardenSpawnPoints != null)
                foreach (var sp in _gardenSpawnPoints)
                    Instantiate(_cymaticGardenPrefab, sp.position, sp.rotation);

            float elapsed = 0f;
            while (_railTiesLaid < totalTies && elapsed < 600f)
            {
                yield return new WaitForSeconds(3f);
                elapsed += 3f;
            }

            _adoptedChildCount = Mathf.Clamp(3, 1, totalTies);
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "adopted", _adoptedChildCount);

            AudioManager.Instance?.PlaySFX2D("orphan_child_greeting_vo");
            HUDController.Instance?.ShowObjective($"Orphans adopted: {_adoptedChildCount}/3. Now protect the track!");
            yield return new WaitForSeconds(2f);
            ClearBeat(BEAT_RESTORATION);
        }

        // --- Beat 3: Conflict ---
        // Train derails into mud golem ambush. Protect children, repair track.
        private IEnumerator Beat3_Conflict()
        {
            HUDController.Instance?.ShowBanner(
                "ELECTRIC MOON -- CONFLICT",
                "Reset agents derailed the train. Children scream in spectral echoes. Protect them!",
                5f);
            HUDController.Instance?.ShowObjective("AMBUSH! Protect the orphans while repairing the track.");
            AudioManager.Instance?.PlaySFX2D("moon3_train_derail_impact");
            AudioManager.Instance?.PlaySFX2D("moon3_combat_music");

            if (_mudGolemPrefab != null && _derailPoint != null)
            {
                for (int i = 0; i < _derailGolemCount; i++)
                {
                    Vector3 offset = new Vector3(
                        UnityEngine.Random.Range(-6f, 6f), 0f,
                        UnityEngine.Random.Range(-6f, 6f));
                    Instantiate(_mudGolemPrefab, _derailPoint.position + offset, Quaternion.identity);
                }
            }

            _golemsKilled = 0;
            MudGolemHealth.OnAnyGolemDied += OnGolemKilled;

            float timeout = 300f;
            float elapsed = 0f;
            while (_golemsKilled < _derailGolemCount && elapsed < timeout)
            {
                yield return new WaitForSeconds(2f);
                elapsed += 2f;
            }
            MudGolemHealth.OnAnyGolemDied -= OnGolemKilled;

            AudioManager.Instance?.PlaySFX2D("milo_cynical_vo_moon3");
            HUDController.Instance?.ShowObjective("Track cleared! Re-align the final rail segment.");
            yield return new WaitForSeconds(3f);
            ClearBeat(BEAT_CONFLICT);
        }

        // --- Beat 4: Climax ---
        // Children sing 432 Hz lullaby -- train solidifies golden. Orphan Lullaby Crystal drops.
        private IEnumerator Beat4_Climax()
        {
            HUDController.Instance?.ShowBanner(
                "ELECTRIC MOON -- CLIMAX",
                "The children stand together and SING. A 432 Hz lullaby reactivates the entire rail.",
                6f);
            AudioManager.Instance?.PlaySFX2D("orphan_lullaby_432hz");

            yield return new WaitForSeconds(3f);

            if (_spectralTrainGO != null) _spectralTrainGO.SetActive(false);
            if (_goldenTrainGO   != null) _goldenTrainGO.SetActive(true);
            if (_trainGoldenFX   != null) _trainGoldenFX.SetActive(true);
            _trainSolidified = true;

            if (_lullabyCrystalSFX != null)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.clip = _lullabyCrystalSFX;
                src.spatialBlend = 0f;
                src.volume = 0.8f;
                src.Play();
            }

            AudioManager.Instance?.PlaySFX2D("lirael_tears_of_light_vo");
            AudioManager.Instance?.PlaySFX2D("milo_sobered_vo_moon3");

            HUDController.Instance?.ShowObjective("The Orphan Train is ALIVE. Ride with the children.");

            if (_lullabyCrystalItem != null)
            {
                UnityEngine.Camera cam = UnityEngine.Camera.main;
                Vector3 dropPos = cam != null
                    ? cam.transform.position + cam.transform.forward * 2f
                    : transform.position + Vector3.forward * 2f;
                var drop = Instantiate(_lullabyCrystalItem, dropPos, Quaternion.identity);
                drop.name = "OrphanTrainLullabyCrystal";
            }

            ServiceLocator.MoonProgress?.MarkBeatCleared(MOON_NUM, BEAT_CLIMAX);
            yield return new WaitForSeconds(5f);
            ClearBeat(BEAT_CLIMAX);
        }

        // --- Beat 5: Revelation ---
        // Lore: Reset used orphan trains for cultural genocide. Plant forward seeds.
        private IEnumerator Beat5_Revelation()
        {
            HUDController.Instance?.ShowObjective("Revelation: Search the train's archive car for the truth.");
            AudioManager.Instance?.PlaySFX2D("moon3_revelation_music");

            if (_loreScrollPrefab != null && _loreScrollSpawnPoint != null)
                Instantiate(_loreScrollPrefab, _loreScrollSpawnPoint.position, _loreScrollSpawnPoint.rotation);

            yield return new WaitForSeconds(2f);

            AudioManager.Instance?.PlaySFX2D("lirael_orphan_genocide_truth_vo");
            AudioManager.Instance?.PlaySFX2D("milo_history_reckoning_vo");
            yield return new WaitForSeconds(4f);

            PlantForwardSeeds();

            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
            {
                save.SetMoonFlag(MOON_NUM, "moon_cleared", true);
                save.SetMoonFlag(MOON_NUM, "beat", BEAT_REVELATION);
            }
            ServiceLocator.MoonProgress?.MarkCleared(MOON_NUM);

            HUDController.Instance?.ShowObjective(
                "<b>MOON 3 COMPLETE!</b>\nThe Orphan Train runs free. Lirael's arc begins.");
            Debug.Log("[Moon3] COMPLETE. Seeds planted. Lirael arc growing.");
        }

        // --- Helpers ---
        private void ClearBeat(int beat)
        {
            _currentBeat = beat + 1;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "beat", _currentBeat);
            ServiceLocator.MoonProgress?.MarkBeatCleared(MOON_NUM, beat);
        }

        private void OnGolemKilled(MudGolemHealth _) => _golemsKilled++;

        public void OnRailTieLaid()
        {
            _railTiesLaid++;
            SaveManager.Instance?.CurrentSave?.SetMoonFlag(MOON_NUM, "railties", _railTiesLaid);
        }

        private void SpawnInteractableRailTie(Transform at)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name  = "Moon3RailTiePoint";
            go.transform.position   = at.position;
            go.transform.localScale = new Vector3(2f, 0.2f, 0.5f);
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = new Color(0.5f, 0.35f, 0.15f);
            var rail = go.AddComponent<Moon3RailTieInteractable>();
            rail.arc = this;
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
                Debug.Log($"[Moon3] Seed planted: {seed}");
            }
        }
    }

    // --- Interactable rail tie placeholder ---
    public class Moon3RailTieInteractable : MonoBehaviour, IInteractable
    {
        public Moon3ElectricArc arc;
        private bool _placed;

        public void Interact(GameObject player)
        {
            if (_placed) return;
            _placed = true;
            arc?.OnRailTieLaid();
            var r = GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.2f, 0.6f, 0.9f);
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            AudioManager.Instance?.PlaySFX2D("rail_tie_placed");
        }

        public string GetInteractPrompt() => "[E] Lay Rail Tie";
    }
}
