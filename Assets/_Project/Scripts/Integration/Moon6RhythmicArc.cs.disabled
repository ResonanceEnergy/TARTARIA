// Moon 6 — Rhythmic Moon: "The Equality of Flow" — full 5-beat vertical slice.
//
// Scene: Assets/_Project/Scenes/Moons/LivingLibrary.unity
// Self-bootstraps when that scene is active; otherwise dormant.
//
// Beats (per docs/03_CAMPAIGN_13_MOONS.md MOON 6):
//   1. Discovery     — Sunken cathedral below White City. Broken pipe organ plays reversed
//                      melody; mud storms form across the map. Lirael hears the crying pipes.
//   2. Restoration   — Repair 6 crystal pipes (excavation + precision cuts). Restore hydraulic
//                      fountain bellows (3-6-9 escalating conduction). Cymatic mandalas bloom.
//   3. Conflict      — Dissonance interrupts mid-performance (QuickTime events); each break
//                      spawns PipeMicroGolem from the organ. Milo hides behind a pew.
//   4. Climax        — Full Cymatic Requiem: all pipes + fountains + bells reach 100 RS.
//                      Ionized mist rain falls; Lirael sings at the crystal microphone.
//                      Children's choir from Moon 3 appears and joins.
//   5. Revelation    — Organ silenced mid-performance during the Mud Flood. Last calibrated
//                      by "Z." — Zereth. His calibration is FLAWLESS. Villain narrative cracks.
//
// New mechanic introduced: OrganConductorMiniGame (sequential pipe activation 3→6→9 bands).
//
// Crossover seeds (PlayerPrefs "moon6_seed_<tag>=1"):
//   moon6_seed_lirael_choir_buff   → Lirael conducts choirs in every restored zone (+buff)
//   moon6_seed_organ_prereq        → Moon 12 planetary bell sync prerequisite unlocked
//   moon6_seed_zereth_calibration  → Zereth mystery deepens (contradicts villain narrative)
//   moon6_seed_giant_harvest       → Giant-scale resources fuel Moon 7 construction

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 (Rhythmic) arc orchestrator. Owns the 5-beat vertical slice for the
    /// Living Library / Sunken Cathedral zone. Idempotent. Self-bootstraps in LivingLibrary scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon6RhythmicArc : MonoBehaviour
    {
        public static Moon6RhythmicArc Instance { get; private set; }

        public enum Beat { Discovery = 0, Restoration = 1, Conflict = 2, Climax = 3, Revelation = 4 }

        [Header("Pacing")]
        public float startDelay = 4f;
        public float minBeatTime = 6f;

        Beat _current = Beat.Discovery;
        bool _running;
        readonly HashSet<Beat> _completed = new();
        readonly List<GameObject> _spawnedThisRun = new();

        // Crossover seed tags
        public const string SEED_LIRAEL_CHOIR_BUFF   = "lirael_choir_buff";    // → passive buff all zones
        public const string SEED_ORGAN_PREREQ         = "organ_prereq";         // → Moon 12 bell sync
        public const string SEED_ZERETH_CALIBRATION   = "zereth_calibration";   // → Zereth not the villain
        public const string SEED_GIANT_HARVEST        = "giant_harvest";        // → Moon 7 construction

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!active.IsValid()) return;
            if (!active.name.StartsWith("LivingLibrary", System.StringComparison.OrdinalIgnoreCase)) return;
            if (Instance != null) return;

            var go = new GameObject("Moon6RhythmicArc");
            Instance = go.AddComponent<Moon6RhythmicArc>();
            Debug.Log("[Moon6Arc] Bootstrapped in LivingLibrary zone.");
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()  { MoonBeatRunner.OnBeatStarted += HandleBeatRunner; }
        void OnDisable() { MoonBeatRunner.OnBeatStarted -= HandleBeatRunner; }

        void Start()     { StartCoroutine(RunArc()); }

        void HandleBeatRunner(int moon, MoonBeatRunner.Beat b)
        {
            if (moon != 6) return;
            var ours = (Beat)(int)b;
            if ((int)ours > (int)_current) _current = ours;
        }

        IEnumerator RunArc()
        {
            if (_running) yield break;
            _running = true;

            yield return new WaitForSeconds(startDelay);

            yield return Beat1_Discovery();
            _completed.Add(Beat.Discovery);
            MoonProgressTracker.Instance?.MarkBeatCleared(6, 0);

            yield return Beat2_Restoration();
            _completed.Add(Beat.Restoration);
            MoonProgressTracker.Instance?.MarkBeatCleared(6, 1);

            yield return Beat3_Conflict();
            _completed.Add(Beat.Conflict);
            MoonProgressTracker.Instance?.MarkBeatCleared(6, 2);

            yield return Beat4_Climax();
            _completed.Add(Beat.Climax);
            MoonProgressTracker.Instance?.MarkBeatCleared(6, 3);

            yield return Beat5_Revelation();
            _completed.Add(Beat.Revelation);
            MoonProgressTracker.Instance?.MarkBeatCleared(6, 4);

            MoonProgressTracker.Instance?.MarkCleared(6);
            GameEvents.FireCriticalSaveTrigger("moon6_arc_complete");
            GameEvents.RaiseHUDShowBanner(
                "MOON 6 COMPLETE",
                "The organ sings. Lirael conducts. And Zereth's name — once a curse — now sounds like a question.",
                9f);
            Debug.Log("[Moon6Arc] Rhythmic Moon arc COMPLETE — Moon 7 unlocked.");

            _running = false;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // BEAT 1 — Discovery
        //   Sunken cathedral below White City. Broken organ plays backwards melody.
        //   Lirael hears it crying; mud storms form across the map.
        // ─────────────────────────────────────────────────────────────────────────────
        IEnumerator Beat1_Discovery()
        {
            _current = Beat.Discovery;
            Debug.Log("[Moon6Arc] Beat 1 — Discovery");

            GameEvents.RaiseHUDShowBanner(
                "MOON 6 — DISCOVERY",
                "The Rhythmic Moon. Deep beneath the White City — a sunken cathedral of impossible scale. Its organ cries.",
                8f);

            Vector3 playerPos = SafePlayerPos();

            // Cathedral entry echo: 4 CrystalPipeEcho markers at cardinal points of the nave
            for (int i = 0; i < 4; i++)
            {
                float ang = (i / 4f) * Mathf.PI * 2f;
                float radius = 18f;
                Vector3 pos = playerPos + new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius);
                var echo = new CathedralEntryEcho(pos);
                _spawnedThisRun.Add(echo.Root);
                echo.Activate();
            }

            // Lirael discovery dialogue
            yield return new WaitForSeconds(3f);
            GameEvents.RaiseHUDShowObjective(
                "Lirael: \"The pipes are crying. Can you hear it? They're trying to sing but the words come out backwards.\"");

            // Broken organ ambient: dissonant reversed chord played via AudioManager
            AudioManager.Instance?.PlaySFX2D("CrystalPipe_Broken");
            yield return new WaitForSeconds(2f);

            // Spawn MudStormAura around the organ base to signal environmental distress
            var stormAura = new OrganMudStormAura(playerPos + Vector3.forward * 25f);
            _spawnedThisRun.Add(stormAura.Root);
            stormAura.Activate();

            // Spawn 3 CrystalPipeFragment pickups — breadcrumb trail toward the organ
            for (int i = 0; i < 3; i++)
            {
                float t = (i + 1) / 4f;
                Vector3 frag = Vector3.Lerp(playerPos, playerPos + Vector3.forward * 25f, t)
                               + new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-2f, 2f));
                var collectible = new CrystalPipeFragment(frag);
                _spawnedThisRun.Add(collectible.Root);
                collectible.Activate();
            }

            yield return new WaitForSeconds(minBeatTime);
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // BEAT 2 — Restoration
        //   Repair 6 crystal pipes. Restore fountain hydraulic bellows.
        //   Conduct the 3-6-9 escalating sequence. Cymatic mandalas bloom.
        // ─────────────────────────────────────────────────────────────────────────────
        IEnumerator Beat2_Restoration()
        {
            _current = Beat.Restoration;
            Debug.Log("[Moon6Arc] Beat 2 — Restoration");

            GameEvents.RaiseHUDShowObjective(
                "Restoration: Repair the crystal pipes and restore the hydraulic fountain bellows.");

            AudioManager.Instance?.PlaySFX2D("Restoration_Begin");

            Vector3 playerPos = SafePlayerPos();

            // Place 6 CrystalPipeRepairNode in a semicircle representing the organ loft
            var pipeNodes = new List<CrystalPipeRepairNode>();
            for (int i = 0; i < 6; i++)
            {
                float ang = Mathf.Lerp(-Mathf.PI * 0.5f, Mathf.PI * 0.5f, i / 5f);
                float radius = 14f + (i % 2) * 3f;   // alternate depth for visual interest
                Vector3 pos = playerPos + new Vector3(Mathf.Sin(ang) * radius, 2f + i * 0.5f,
                                                      Mathf.Cos(ang) * radius * 0.6f);
                var node = new CrystalPipeRepairNode(pos, i);
                pipeNodes.Add(node);
                _spawnedThisRun.Add(node.Root);
                node.Activate();
            }

            // Wait until all 6 pipes repaired (poll progress tracker)
            GameEvents.RaiseHUDShowObjective("Repair 6 crystal pipes (0 / 6)");
            float timeout = 120f;
            float elapsed = 0f;
            while (elapsed < timeout)
            {
                int repaired = 0;
                foreach (var n in pipeNodes) if (n.IsRepaired) repaired++;
                if (repaired >= 6) break;
                GameEvents.RaiseHUDShowObjective($"Repair 6 crystal pipes ({repaired} / 6)");
                elapsed += Time.deltaTime;
                yield return null;
            }

            GameEvents.RaiseHUDShowObjective("Pipes repaired! Restore the hydraulic fountain bellows.");
            AudioManager.Instance?.PlaySFX2D("PipeRepaired_All");

            yield return new WaitForSeconds(1.5f);

            // 3 FountainBellowsNode arranged in a triangle — hydraulic bellows for the organ
            var bellowsNodes = new List<FountainBellowsNode>();
            for (int i = 0; i < 3; i++)
            {
                float ang = (i / 3f) * Mathf.PI * 2f;
                Vector3 pos = playerPos + new Vector3(Mathf.Cos(ang) * 22f, 0f, Mathf.Sin(ang) * 22f);
                var node = new FountainBellowsNode(pos, i);
                bellowsNodes.Add(node);
                _spawnedThisRun.Add(node.Root);
                node.Activate();
            }

            GameEvents.RaiseHUDShowObjective("Restore 3 hydraulic fountain bellows (0 / 3)");
            timeout = 90f; elapsed = 0f;
            while (elapsed < timeout)
            {
                int restored = 0;
                foreach (var n in bellowsNodes) if (n.IsRestored) restored++;
                if (restored >= 3) break;
                GameEvents.RaiseHUDShowObjective($"Restore 3 hydraulic fountain bellows ({restored} / 3)");
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Organ Conductor mini-game: 3-6-9 escalating sequence
            GameEvents.RaiseHUDShowObjective("Conduct the 3-6-9 harmony sequence to prove mastery.");
            yield return new WaitForSeconds(1f);

            var conductor = new OrganConductorMiniGame(playerPos + Vector3.forward * 5f);
            _spawnedThisRun.Add(conductor.Root);
            conductor.Activate();

            // Wait for successful performance
            timeout = 120f; elapsed = 0f;
            while (!conductor.IsComplete && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Cymatic mandala bloom effect on success
            AudioManager.Instance?.PlaySFX2D("OrganRestoredSymphony");
            GameEvents.RaiseHUDShowBanner(
                "PIPES TUNED",
                "Rose windows project kaleidoscopic cymatic mandalas. The water forms sacred-geometry patterns. The cathedral breathes.",
                7f);

            // Spawn CathedralCytomaticBloom visual marker
            var bloom = new CathedralCytomaticBloom(playerPos);
            _spawnedThisRun.Add(bloom.Root);
            bloom.Activate();

            yield return new WaitForSeconds(minBeatTime);
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // BEAT 3 — Conflict
        //   Dissonance attacks mid-performance with quick-time events.
        //   Each break in the sequence spawns PipeMicroGolem from organ pipes.
        //   Milo hides behind a pew.
        // ─────────────────────────────────────────────────────────────────────────────
        IEnumerator Beat3_Conflict()
        {
            _current = Beat.Conflict;
            Debug.Log("[Moon6Arc] Beat 3 — Conflict");

            GameEvents.RaiseHUDShowObjective(
                "The Dissonance strikes! Maintain harmony or golems erupt from the pipes!");

            AudioManager.Instance?.PlaySFX2D("DissonanceStrike_Begin");

            // Milo dialogue — he's hiding
            yield return new WaitForSeconds(1.5f);
            GameEvents.RaiseHUDShowObjective(
                "Milo: \"I signed up for treasure hunting, not a haunted concert hall!\"");
            yield return new WaitForSeconds(2.5f);

            Vector3 playerPos = SafePlayerPos();

            // 3 waves of DissonanceInterrupts; each failed interrupt spawns a PipeMicroGolem
            var microGolems = new List<PipeMicroGolem>();

            for (int wave = 0; wave < 3; wave++)
            {
                GameEvents.RaiseHUDShowObjective($"Hold the harmony! (Wave {wave + 1} / 3)");
                AudioManager.Instance?.PlaySFX2D("Dissonance_Crack");

                // Spawn DissonanceInterruptNode at a random pipe offset
                float ang = Random.Range(0f, Mathf.PI * 2f);
                Vector3 interruptPos = playerPos + new Vector3(Mathf.Cos(ang) * 12f, 1f, Mathf.Sin(ang) * 12f);
                var interrupt = new DissonanceInterruptNode(interruptPos, wave);
                _spawnedThisRun.Add(interrupt.Root);
                interrupt.Activate();

                // If interrupt not resolved in 8s → spawn a micro-golem from the pipe
                float waveTimer = 0f;
                bool resolved = false;
                while (waveTimer < 8f)
                {
                    if (interrupt.IsResolved) { resolved = true; break; }
                    waveTimer += Time.deltaTime;
                    yield return null;
                }

                if (!resolved)
                {
                    // Spawn golem from nearest pipe position
                    Vector3 golemPos = playerPos + new Vector3(Mathf.Cos(ang) * 8f, 0f, Mathf.Sin(ang) * 8f);
                    var golem = new PipeMicroGolem(golemPos);
                    microGolems.Add(golem);
                    _spawnedThisRun.Add(golem.Root);
                    golem.Activate();
                    Debug.Log($"[Moon6Arc] Dissonance wave {wave + 1}: harmony broke — PipeMicroGolem spawned.");
                }
                else
                {
                    AudioManager.Instance?.PlaySFX2D("HarmonyRestored_Short");
                    Debug.Log($"[Moon6Arc] Dissonance wave {wave + 1}: harmony held.");
                }

                yield return new WaitForSeconds(2f);
            }

            // Wait for all spawned golems to die (or max timeout)
            float timeout = 60f; float elapsed = 0f;
            while (elapsed < timeout)
            {
                bool allDead = true;
                foreach (var g in microGolems) if (!g.IsDead) { allDead = false; break; }
                if (allDead) break;
                elapsed += Time.deltaTime;
                yield return null;
            }

            GameEvents.RaiseHUDShowObjective("The Dissonance retreats. The organ holds.");
            AudioManager.Instance?.PlaySFX2D("ConflictResolved_Cathedral");
            yield return new WaitForSeconds(minBeatTime);
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // BEAT 4 — Climax
        //   Full Cymatic Requiem: all pipes + fountains + bells → 100 RS.
        //   Ionized mist rain. Lirael sings at the crystal microphone stand.
        //   Children's choir from Moon 3 appears and joins.
        //   Giant-scale resources grow overnight.
        // ─────────────────────────────────────────────────────────────────────────────
        IEnumerator Beat4_Climax()
        {
            _current = Beat.Climax;
            Debug.Log("[Moon6Arc] Beat 4 — Climax");

            GameEvents.RaiseHUDShowBanner(
                "MOON 6 — CLIMAX",
                "The Cymatic Requiem. All pipes. All fountains. All bells. A moment of perfect resonance.",
                8f);

            AudioManager.Instance?.PlaySFX2D("OrganClimax_Full");

            Vector3 playerPos = SafePlayerPos();

            // Spawn CrystalMicrophoneStand — Lirael's solo position
            var microStand = new CrystalMicrophoneStand(playerPos + Vector3.forward * 10f);
            _spawnedThisRun.Add(microStand.Root);
            microStand.Activate();

            yield return new WaitForSeconds(3f);

            // Lirael at the microphone
            GameEvents.RaiseHUDShowObjective(
                "Lirael steps to the crystal microphone and begins to sing a solo line from her lullaby.");
            AudioManager.Instance?.PlaySFX2D("Lirael_Lullaby_Solo");

            yield return new WaitForSeconds(4f);

            // Children's choir from Moon 3 materialises
            for (int i = 0; i < 5; i++)
            {
                float ang = (i / 5f) * Mathf.PI * 2f;
                Vector3 pos = playerPos + new Vector3(Mathf.Cos(ang) * 8f, 0f, Mathf.Sin(ang) * 8f);
                var child = new ChildChoirMember(pos, i);
                _spawnedThisRun.Add(child.Root);
                child.Activate();
            }

            GameEvents.RaiseHUDShowObjective(
                "The children's choir from Moon 3 appears — their voices joining Lirael's.");
            AudioManager.Instance?.PlaySFX2D("ChildrenChoir_Join");

            yield return new WaitForSeconds(3f);

            // Ionized mist rain — city-wide particle marker
            var mistRain = new IonizedMistRain(playerPos);
            _spawnedThisRun.Add(mistRain.Root);
            mistRain.Activate();

            // Giant harvest resonance marker — resources swell to 10x overnight
            var giantHarvest = new GiantHarvestResonanceMarker(playerPos + Vector3.right * 30f);
            _spawnedThisRun.Add(giantHarvest.Root);
            giantHarvest.Activate();

            GameEvents.RaiseHUDShowBanner(
                "CYMATIC REQUIEM COMPLETE",
                "City-wide ionized mist rain falls. Cymatic gardens swell to 10x. The cathedral heals itself.",
                9f);

            yield return new WaitForSeconds(minBeatTime);
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // BEAT 5 — Revelation
        //   The organ was silenced mid-performance during the Mud Flood.
        //   Last calibrated by "Z." — Zereth. His calibration is FLAWLESS.
        //   The villain narrative begins to crack.
        //   Four crossover seeds planted.
        // ─────────────────────────────────────────────────────────────────────────────
        IEnumerator Beat5_Revelation()
        {
            _current = Beat.Revelation;
            Debug.Log("[Moon6Arc] Beat 5 — Revelation");

            GameEvents.RaiseHUDShowObjective(
                "The organ's tuning records reveal a name: Z. — Zereth. His calibration was flawless.");

            Vector3 playerPos = SafePlayerPos();

            // Spawn ZerethCalibrationScroll — readable collectible
            var scroll = new ZerethCalibrationScroll(playerPos + Vector3.forward * 7f);
            _spawnedThisRun.Add(scroll.Root);
            scroll.Activate();

            yield return new WaitForSeconds(2f);

            // Revelation text
            GameEvents.RaiseHUDShowObjective(
                "\"This organ was silenced mid-performance during the Mud Flood. The organist's final note still " +
                "hangs in the pipe — a 9-band purity no accident can create.\"");

            yield return new WaitForSeconds(3f);

            GameEvents.RaiseHUDShowObjective(
                "\"The tuning is marked: Z. If Zereth was the villain — why does his work ring with such perfect harmony?\"");

            yield return new WaitForSeconds(3f);

            AudioManager.Instance?.PlaySFX2D("Revelation_Sting");

            // Plant 4 crossover seeds
            PlantSeed(SEED_LIRAEL_CHOIR_BUFF,   "Lirael now conducts choirs in every restored zone — passive Aether buff active.");
            PlantSeed(SEED_ORGAN_PREREQ,          "Moon 12 planetary bell sync prerequisite — UNLOCKED.");
            PlantSeed(SEED_ZERETH_CALIBRATION,    "Zereth's organ calibration: flawless. The villain narrative has a hole in it.");
            PlantSeed(SEED_GIANT_HARVEST,         "Giant-scale resources activated. Moon 7 construction is now possible.");

            // Spawn 4 Moon6Collectible drops
            string[] collectibles = { "OrganistandNote", "CrystalPipeShard_Calibrated", "LiraelLullabyScore", "ZerethTuningFork" };
            for (int i = 0; i < collectibles.Length; i++)
            {
                float ang = (i / 4f) * Mathf.PI * 2f;
                Vector3 pos = playerPos + new Vector3(Mathf.Cos(ang) * 5f, 0.5f, Mathf.Sin(ang) * 5f);
                var col = new Moon6Collectible(pos, collectibles[i]);
                _spawnedThisRun.Add(col.Root);
                col.Activate();
            }

            GameEvents.RaiseHUDShowBanner(
                "REVELATION",
                "The organ speaks. Zereth calibrated it. Lirael will carry the song. And the Mud Flood silenced the most important note in history.",
                10f);

            yield return new WaitForSeconds(minBeatTime);
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────────────

        static Vector3 SafePlayerPos()
        {
            var p = GameObject.FindWithTag("Player");
            return p != null ? p.transform.position : Vector3.zero;
        }

        static void PlantSeed(string tag, string hint)
        {
            string key = $"moon6_seed_{tag}";
            if (PlayerPrefs.GetInt(key, 0) == 1) return;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Debug.Log($"[Moon6Arc] Seed planted: {tag} — {hint}");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // INNER GAMEPLAY OBJECTS
    // All are self-contained MonoBehaviour types instantiated at runtime.
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Beat 1: Ambient echo marker showing where the cathedral pipes are located.
    /// Emits a low reversed-chord particle glow to guide the player.
    /// </summary>
    public class CathedralEntryEcho : MonoBehaviour
    {
        public GameObject Root => gameObject;
        readonly Vector3 _pos;
        public CathedralEntryEcho(Vector3 pos) { _pos = pos; }

        public void Activate()
        {
            transform.position = _pos;
            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.4f, 0.3f, 0.8f);
            light.intensity = 1.8f;
            light.range = 8f;

            var src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 1f;
            src.volume = 0.35f;
            src.loop = true;
            StartCoroutine(PulseLoop(light));
        }

        IEnumerator PulseLoop(Light l)
        {
            float t = 0f;
            float period = 2.5f;
            while (true)
            {
                t += Time.deltaTime;
                l.intensity = 1.8f + Mathf.Sin(t * Mathf.PI * 2f / period) * 0.6f;
                yield return null;
            }
        }
    }

    /// <summary>
    /// Beat 1: Storm aura around the broken organ base — visual signal of mud-storm distress.
    /// </summary>
    public class OrganMudStormAura : MonoBehaviour
    {
        public GameObject Root => gameObject;
        readonly Vector3 _pos;
        public OrganMudStormAura(Vector3 pos) { _pos = pos; }

        public void Activate()
        {
            transform.position = _pos;
            // Mud-brown fog sphere
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 12f;
            var mr = sphere.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.45f, 0.32f, 0.15f, 0.35f);
                mr.sharedMaterial.SetFloat("_Surface", 1f); // transparent
            }
            UnityEngine.Object.Destroy(sphere.GetComponent<Collider>());
            StartCoroutine(PulseStorm(sphere.transform));
        }

        IEnumerator PulseStorm(Transform t)
        {
            float time = 0f;
            while (true)
            {
                time += Time.deltaTime;
                float scale = 12f + Mathf.Sin(time * 0.7f) * 2.5f;
                t.localScale = Vector3.one * scale;
                t.Rotate(Vector3.up, 18f * Time.deltaTime);
                yield return null;
            }
        }
    }

    /// <summary>
    /// Beat 1: Breadcrumb crystal pipe shard leading toward the organ.
    /// </summary>
    public class CrystalPipeFragment : MonoBehaviour, IInteractable
    {
        public GameObject Root => gameObject;
        readonly Vector3 _pos;
        bool _collected;

        public CrystalPipeFragment(Vector3 pos) { _pos = pos; }

        public void Activate()
        {
            transform.position = _pos;
            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mesh.transform.SetParent(transform);
            mesh.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            mesh.transform.localScale = new Vector3(0.25f, 1.2f, 0.25f);
            var mr = mesh.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.6f, 0.9f, 1f);
                mr.sharedMaterial.SetColor("_EmissionColor", new Color(0.3f, 0.7f, 1f) * 1.5f);
                mr.sharedMaterial.EnableKeyword("_EMISSION");
            }
            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.5f;
            StartCoroutine(Bob());
        }

        IEnumerator Bob()
        {
            float t = 0f; Vector3 base_ = transform.position;
            while (!_collected)
            {
                t += Time.deltaTime;
                transform.position = base_ + Vector3.up * (Mathf.Sin(t * 1.4f) * 0.18f);
                yield return null;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || _collected) return;
            Interact(other.gameObject);
        }

        public void Interact(GameObject player)
        {
            if (_collected) return;
            _collected = true;
            AudioManager.Instance?.PlaySFX2D("CrystalPipe_Collect");
            GameEvents.RaiseHUDShowObjective("Crystal Pipe Fragment collected — the organ's voice grows clearer.");
            gameObject.SetActive(false);
        }

        public string GetInteractPrompt() => _collected ? "" : "Collect Crystal Pipe Fragment";
    }

    /// <summary>
    /// Beat 2: One of 6 crystal pipe repair nodes. Proximity-activate to repair.
    /// </summary>
    public class CrystalPipeRepairNode : MonoBehaviour, IInteractable
    {
        public GameObject Root => gameObject;
        public bool IsRepaired { get; private set; }
        readonly Vector3 _pos;
        readonly int _index;

        public CrystalPipeRepairNode(Vector3 pos, int index) { _pos = pos; _index = index; }

        public void Activate()
        {
            transform.position = _pos;

            // Visual: tall cyan cylinder representing an organ pipe
            float height = 2.5f + _index * 0.7f; // ascending pipe heights
            var pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.transform.SetParent(transform);
            pipe.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            pipe.transform.localScale = new Vector3(0.4f, height * 0.5f, 0.4f);
            var mr = pipe.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.5f, 0.85f, 0.95f, 1f);
            }
            UnityEngine.Object.Destroy(pipe.GetComponent<Collider>());

            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 2.5f;

            // Damage crack visual
            var crack = GameObject.CreatePrimitive(PrimitiveType.Quad);
            crack.transform.SetParent(transform);
            crack.transform.localPosition = new Vector3(0.21f, 1f, 0f);
            crack.transform.localScale = new Vector3(0.3f, 0.8f, 1f);
            var cmr = crack.GetComponent<MeshRenderer>();
            if (cmr != null)
            {
                cmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                cmr.sharedMaterial.color = new Color(0.2f, 0.1f, 0.05f);
            }
            UnityEngine.Object.Destroy(crack.GetComponent<Collider>());
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || IsRepaired) return;
            Interact(other.gameObject);
        }

        public void Interact(GameObject player)
        {
            if (IsRepaired) return;
            IsRepaired = true;
            AudioManager.Instance?.PlaySFX2D("PipeRepaired_Single");
            GameEvents.RaiseHUDShowObjective($"Crystal pipe {_index + 1} repaired — resonance restored.");
            // Glow green on repair
            var mr = GetComponentInChildren<MeshRenderer>();
            if (mr != null)
                mr.sharedMaterial.color = new Color(0.3f, 1f, 0.5f);
            Debug.Log($"[Moon6Arc] CrystalPipeRepairNode {_index} repaired.");
        }

        public string GetInteractPrompt() => IsRepaired ? "" : $"Repair Crystal Pipe {_index + 1}";
    }

    /// <summary>
    /// Beat 2: Hydraulic fountain bellows node — feeds air pressure to the organ.
    /// </summary>
    public class FountainBellowsNode : MonoBehaviour, IInteractable
    {
        public GameObject Root => gameObject;
        public bool IsRestored { get; private set; }
        readonly Vector3 _pos;
        readonly int _index;

        public FountainBellowsNode(Vector3 pos, int index) { _pos = pos; _index = index; }

        public void Activate()
        {
            transform.position = _pos;

            // Fountain basin
            var basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basin.transform.SetParent(transform);
            basin.transform.localPosition = Vector3.zero;
            basin.transform.localScale = new Vector3(3f, 0.3f, 3f);
            var bmr = basin.GetComponent<MeshRenderer>();
            if (bmr != null)
            {
                bmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                bmr.sharedMaterial.color = new Color(0.7f, 0.55f, 0.4f);
            }
            UnityEngine.Object.Destroy(basin.GetComponent<Collider>());

            // Water column (dormant — dark)
            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 3f;

            StartCoroutine(Pulse());
        }

        IEnumerator Pulse()
        {
            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.3f, 0.5f, 1f);
            light.intensity = 0.6f;
            light.range = 6f;
            float t = 0f;
            while (!IsRestored)
            {
                t += Time.deltaTime;
                light.intensity = 0.4f + Mathf.Sin(t * 1.2f) * 0.3f;
                yield return null;
            }
            light.color = new Color(0.2f, 0.9f, 0.5f);
            light.intensity = 2f;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || IsRestored) return;
            Interact(other.gameObject);
        }

        public void Interact(GameObject player)
        {
            if (IsRestored) return;
            IsRestored = true;
            AudioManager.Instance?.PlaySFX2D("FountainBellows_Restore");
            GameEvents.RaiseHUDShowObjective($"Fountain bellows {_index + 1} restored — hydraulic pressure rising.");
            Debug.Log($"[Moon6Arc] FountainBellowsNode {_index} restored.");
        }

        public string GetInteractPrompt() => IsRestored ? "" : $"Restore Fountain Bellows {_index + 1}";
    }

    /// <summary>
    /// Beat 2: Organ conductor mini-game.
    /// Player activates 3 pipe groups in order (3-band → 6-band → 9-band) to prove mastery.
    /// Each activation prompt uses keyboard/gamepad input.
    /// </summary>
    public class OrganConductorMiniGame : MonoBehaviour, IInteractable
    {
        public GameObject Root => gameObject;
        public bool IsComplete { get; private set; }
        public bool IsActive { get; private set; }
        readonly Vector3 _pos;
        int _stage; // 0=idle, 1=3-band, 2=6-band, 3=9-band, 4=complete

        public OrganConductorMiniGame(Vector3 pos) { _pos = pos; }

        public void Activate()
        {
            transform.position = _pos;

            // Conductor podium
            var podium = GameObject.CreatePrimitive(PrimitiveType.Cube);
            podium.transform.SetParent(transform);
            podium.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            podium.transform.localScale = new Vector3(1.2f, 1f, 0.8f);
            var mr = podium.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.6f, 0.45f, 0.2f);
            }
            UnityEngine.Object.Destroy(podium.GetComponent<Collider>());

            var col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 2f, 3f);
            col.center = new Vector3(0f, 1f, 0f);

            // Score stand light
            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.9f, 0.8f, 0.4f);
            light.intensity = 2f;
            light.range = 5f;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || IsActive || IsComplete) return;
            Interact(other.gameObject);
        }

        public void Interact(GameObject player)
        {
            if (IsActive || IsComplete) return;
            IsActive = true;
            GameEvents.RaiseHUDShowObjective("Conduct the 3-band sequence — [E] / [A Button]");
            StartCoroutine(RunSequence());
        }

        IEnumerator RunSequence()
        {
            string[] prompts = { "Conduct 3-band harmony — [E] / [A Button]",
                                  "Conduct 6-band harmony — [E] / [A Button]",
                                  "Conduct 9-band harmony — [E] / [A Button]" };
            string[] sfx = { "Organ_3Band", "Organ_6Band", "Organ_9Band" };

            for (int i = 0; i < 3; i++)
            {
                _stage = i + 1;
                GameEvents.RaiseHUDShowObjective(prompts[i]);
                float wait = 0f;
                while (wait < 15f)
                {
                    if (UnityEngine.Input.GetKeyDown(KeyCode.E) ||
                        UnityEngine.Input.GetButtonDown("Jump"))
                    {
                        AudioManager.Instance?.PlaySFX2D(sfx[i]);
                        GameEvents.RaiseHUDShowObjective($"Band {(i + 1) * 3} — HARMONY HELD!");
                        yield return new WaitForSeconds(1.5f);
                        break;
                    }
                    wait += Time.deltaTime;
                    yield return null;
                }
            }

            IsComplete = true;
            _stage = 4;
            AudioManager.Instance?.PlaySFX2D("OrganSequence_Complete");
            GameEvents.RaiseHUDShowObjective("3-6-9 sequence complete — the organ remembers its voice.");
            Debug.Log("[Moon6Arc] OrganConductorMiniGame complete.");
        }

        public string GetInteractPrompt() => IsComplete ? "" : "Conduct the Pipe Organ";
    }

    /// <summary>
    /// Beat 2: Cymatic mandala bloom — rose window projection marker.
    /// </summary>
    public class CathedralCytomaticBloom : MonoBehaviour
    {
        public GameObject Root => gameObject;
        readonly Vector3 _pos;
        public CathedralCytomaticBloom(Vector3 pos) { _pos = pos; }

        public void Activate()
        {
            transform.position = _pos;
            for (int ring = 0; ring < 3; ring++)
            {
                int segments = 6 * (ring + 1);
                float radius = 4f + ring * 4f;
                for (int s = 0; s < segments; s++)
                {
                    float ang = (s / (float)segments) * Mathf.PI * 2f;
                    var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.transform.SetParent(transform);
                    quad.transform.position = _pos + new Vector3(Mathf.Cos(ang) * radius, 0.05f, Mathf.Sin(ang) * radius);
                    quad.transform.rotation = Quaternion.Euler(90f, ang * Mathf.Rad2Deg, 0f);
                    quad.transform.localScale = Vector3.one * (0.8f + ring * 0.2f);
                    var mr = quad.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        Color c = Color.HSVToRGB((s / (float)segments + ring * 0.15f) % 1f, 0.8f, 0.95f);
                        mr.sharedMaterial.color = c;
                        mr.sharedMaterial.SetColor("_EmissionColor", c * 1.8f);
                        mr.sharedMaterial.EnableKeyword("_EMISSION");
                    }
                    UnityEngine.Object.Destroy(quad.GetComponent<Collider>());
                }
            }
        }
    }

    /// <summary>
    /// Beat 3: Dissonance interrupt node — quick-time event to maintain harmony.
    /// </summary>
    public class DissonanceInterruptNode : MonoBehaviour, IInteractable
    {
        public GameObject Root => gameObject;
        public bool IsResolved { get; private set; }
        readonly Vector3 _pos;
        readonly int _wave;

        public DissonanceInterruptNode(Vector3 pos, int wave) { _pos = pos; _wave = wave; }

        public void Activate()
        {
            transform.position = _pos;

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform);
            sphere.transform.localPosition = new Vector3(0f, 1f, 0f);
            sphere.transform.localScale = Vector3.one * 1.8f;
            var mr = sphere.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.8f, 0.1f, 0.15f);
                mr.sharedMaterial.SetColor("_EmissionColor", new Color(1f, 0.1f, 0.1f) * 2f);
                mr.sharedMaterial.EnableKeyword("_EMISSION");
            }
            UnityEngine.Object.Destroy(sphere.GetComponent<Collider>());

            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 3f;

            StartCoroutine(Warn());
        }

        IEnumerator Warn()
        {
            float t = 0f;
            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.2f, 0.1f);
            light.intensity = 3f;
            light.range = 8f;
            while (!IsResolved && t < 8f)
            {
                t += Time.deltaTime;
                light.intensity = 3f * (1f - t / 8f) * (0.5f + 0.5f * Mathf.Sin(t * 10f));
                yield return null;
            }
            if (!IsResolved) gameObject.SetActive(false);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || IsResolved) return;
            Interact(other.gameObject);
        }

        public void Interact(GameObject player)
        {
            if (IsResolved) return;
            IsResolved = true;
            AudioManager.Instance?.PlaySFX2D("Dissonance_Sealed");
            GameEvents.RaiseHUDShowObjective($"Dissonance wave {_wave + 1} sealed — harmony holds!");
            Debug.Log($"[Moon6Arc] DissonanceInterruptNode wave {_wave} resolved.");
            gameObject.SetActive(false);
        }

        public string GetInteractPrompt() => IsResolved ? "" : "Seal the Dissonance!";
    }

    /// <summary>
    /// Beat 3: Micro-golem spawned from organ pipes when dissonance is not sealed in time.
    /// Small, fast, 30 HP.
    /// </summary>
    public class PipeMicroGolem : MonoBehaviour
    {
        public GameObject Root => gameObject;
        public bool IsDead { get; private set; }
        readonly Vector3 _pos;
        float _hp = 30f;

        public PipeMicroGolem(Vector3 pos) { _pos = pos; }

        public void Activate()
        {
            transform.position = _pos;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(transform);
            body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
            var mr = body.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.3f, 0.25f, 0.2f);
            }

            var rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            gameObject.tag = "Enemy";
            StartCoroutine(Chase());
        }

        IEnumerator Chase()
        {
            while (!IsDead)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    Vector3 dir = (player.transform.position - transform.position).normalized;
                    transform.position += dir * 3.5f * Time.deltaTime;
                    transform.LookAt(player.transform);
                }
                yield return null;
            }
        }

        public void TakeDamage(float dmg)
        {
            _hp -= dmg;
            if (_hp <= 0f) Die();
        }

        void Die()
        {
            if (IsDead) return;
            IsDead = true;
            AudioManager.Instance?.PlaySFX2D("MicroGolem_Death");
            Debug.Log("[Moon6Arc] PipeMicroGolem defeated.");
            Destroy(gameObject, 0.2f);
        }

        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.CompareTag("PlayerProjectile")) TakeDamage(10f);
            if (col.gameObject.CompareTag("Player")) TakeDamage(0f); // contact damage handled by player health
        }
    }

    /// <summary>
    /// Beat 4: Crystal microphone stand — Lirael's solo position marker.
    /// </summary>
    public class CrystalMicrophoneStand : MonoBehaviour, IInteractable
    {
        public GameObject Root => gameObject;
        readonly Vector3 _pos;
        bool _activated;

        public CrystalMicrophoneStand(Vector3 pos) { _pos = pos; }

        public void Activate()
        {
            transform.position = _pos;

            // Stand pole
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.transform.SetParent(transform);
            pole.transform.localPosition = new Vector3(0f, 1f, 0f);
            pole.transform.localScale = new Vector3(0.08f, 2f, 0.08f);
            var mr = pole.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.7f, 0.85f, 1f);
                mr.sharedMaterial.SetColor("_EmissionColor", new Color(0.5f, 0.7f, 1f) * 1.4f);
                mr.sharedMaterial.EnableKeyword("_EMISSION");
            }
            UnityEngine.Object.Destroy(pole.GetComponent<Collider>());

            // Microphone crystal sphere
            var crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crystal.transform.SetParent(transform);
            crystal.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            crystal.transform.localScale = Vector3.one * 0.28f;
            var cmr = crystal.GetComponent<MeshRenderer>();
            if (cmr != null)
            {
                cmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                cmr.sharedMaterial.color = new Color(0.9f, 0.95f, 1f);
                cmr.sharedMaterial.SetColor("_EmissionColor", new Color(0.8f, 0.9f, 1f) * 3f);
                cmr.sharedMaterial.EnableKeyword("_EMISSION");
            }
            UnityEngine.Object.Destroy(crystal.GetComponent<Collider>());

            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 2.5f;

            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.8f, 0.9f, 1f);
            light.intensity = 3f;
            light.range = 10f;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || _activated) return;
            Interact(other.gameObject);
        }

        public void Interact(GameObject player)
        {
            if (_activated) return;
            _activated = true;
            GameEvents.RaiseHUDShowObjective(
                "The crystal microphone hums. Lirael's voice will carry from here across every restored zone.");
            AudioManager.Instance?.PlaySFX2D("CrystalMic_Activate");
        }

        public string GetInteractPrompt() => _activated ? "" : "Activate Crystal Microphone";
    }

    /// <summary>
    /// Beat 4: One child choir member from Moon 3 who materialises during the Climax.
    /// Sings (plays proximity audio) and bows to the player.
    /// </summary>
    public class ChildChoirMember : MonoBehaviour
    {
        public GameObject Root => gameObject;
        readonly Vector3 _pos;
        readonly int _index;

        public ChildChoirMember(Vector3 pos, int index) { _pos = pos; _index = index; }

        public void Activate()
        {
            transform.position = _pos;

            // Simple humanoid silhouette
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(transform);
            capsule.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            capsule.transform.localScale = new Vector3(0.35f, 0.7f, 0.35f);
            var mr = capsule.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                Color c = Color.HSVToRGB((_index * 0.2f) % 1f, 0.4f, 0.9f);
                mr.sharedMaterial.color = c;
                mr.sharedMaterial.SetColor("_EmissionColor", c * 0.8f);
                mr.sharedMaterial.EnableKeyword("_EMISSION");
            }
            UnityEngine.Object.Destroy(capsule.GetComponent<Collider>());

            // Children face toward the organ (forward in scene)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            StartCoroutine(SingAndSway());
        }

        IEnumerator SingAndSway()
        {
            float t = 0f;
            Vector3 base_ = transform.position;
            while (true)
            {
                t += Time.deltaTime;
                transform.position = base_ + Vector3.up * Mathf.Sin(t * 1.1f + _index) * 0.05f;
                yield return null;
            }
        }
    }

    /// <summary>
    /// Beat 4: City-wide ionized mist rain particle marker — covers two zones.
    /// </summary>
    public class IonizedMistRain : MonoBehaviour
    {
        public GameObject Root => gameObject;
        readonly Vector3 _pos;
        public IonizedMistRain(Vector3 pos) { _pos = pos; }

        public void Activate()
        {
            transform.position = _pos + Vector3.up * 20f;
            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.6f, 0.8f, 1f);
            light.intensity = 0.4f;

            // Mist plane — large translucent quad simulating rain over the zone
            var plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.transform.SetParent(transform);
            plane.transform.localPosition = Vector3.zero;
            plane.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            plane.transform.localScale = new Vector3(200f, 200f, 1f);
            var mr = plane.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.7f, 0.85f, 1f, 0.12f);
                mr.sharedMaterial.SetFloat("_Surface", 1f);
            }
            UnityEngine.Object.Destroy(plane.GetComponent<Collider>());
        }
    }

    /// <summary>
    /// Beat 4: Giant-scale harvest resonance marker — resources swell to 10x overnight.
    /// Interactable — inspecting it shows the Moon 7 seed info.
    /// </summary>
    public class GiantHarvestResonanceMarker : MonoBehaviour, IInteractable
    {
        public GameObject Root => gameObject;
        readonly Vector3 _pos;
        bool _inspected;

        public GiantHarvestResonanceMarker(Vector3 pos) { _pos = pos; }

        public void Activate()
        {
            transform.position = _pos;

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform);
            sphere.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            sphere.transform.localScale = Vector3.one * 2.5f;
            var mr = sphere.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.3f, 0.9f, 0.4f);
                mr.sharedMaterial.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 0.3f) * 2f);
                mr.sharedMaterial.EnableKeyword("_EMISSION");
            }
            UnityEngine.Object.Destroy(sphere.GetComponent<Collider>());

            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 3f;

            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.3f, 1f, 0.4f);
            light.intensity = 4f;
            light.range = 12f;
            StartCoroutine(GrowPulse(sphere.transform, light));
        }

        IEnumerator GrowPulse(Transform t, Light l)
        {
            float time = 0f;
            while (true)
            {
                time += Time.deltaTime;
                float scale = 2.5f + Mathf.Sin(time * 0.6f) * 0.5f;
                t.localScale = Vector3.one * scale;
                l.intensity = 4f + Mathf.Sin(time * 1.3f) * 1.5f;
                yield return null;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || _inspected) return;
            Interact(other.gameObject);
        }

        public void Interact(GameObject player)
        {
            if (_inspected) return;
            _inspected = true;
            GameEvents.RaiseHUDShowObjective(
                "Giant Harvest Pulse: cymatic resonance has swelled crop yields to 10x. " +
                "Moon 7's massive construction will have all the material it needs.");
        }

        public string GetInteractPrompt() => _inspected ? "" : "Inspect Giant Harvest Resonance";
    }

    /// <summary>
    /// Beat 5: Zereth's tuning calibration scroll — readable interactable.
    /// The revelation that Zereth's work is flawless.
    /// </summary>
    public class ZerethCalibrationScroll : MonoBehaviour, IInteractable
    {
        public GameObject Root => gameObject;
        readonly Vector3 _pos;
        bool _read;

        public ZerethCalibrationScroll(Vector3 pos) { _pos = pos; }

        public void Activate()
        {
            transform.position = _pos;

            var scroll = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            scroll.transform.SetParent(transform);
            scroll.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            scroll.transform.localScale = new Vector3(0.25f, 0.6f, 0.25f);
            var mr = scroll.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.95f, 0.9f, 0.75f);
                mr.sharedMaterial.SetColor("_EmissionColor", new Color(0.9f, 0.85f, 0.5f) * 1.2f);
                mr.sharedMaterial.EnableKeyword("_EMISSION");
            }
            UnityEngine.Object.Destroy(scroll.GetComponent<Collider>());

            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.8f;

            StartCoroutine(Bob());
        }

        IEnumerator Bob()
        {
            float t = 0f; Vector3 base_ = transform.position;
            while (!_read)
            {
                t += Time.deltaTime;
                transform.position = base_ + Vector3.up * (0.3f + Mathf.Sin(t * 1.6f) * 0.15f);
                yield return null;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || _read) return;
            Interact(other.gameObject);
        }

        public void Interact(GameObject player)
        {
            if (_read) return;
            _read = true;
            AudioManager.Instance?.PlaySFX2D("ScrollRead");
            GameEvents.RaiseHUDShowBanner(
                "ZERETH'S CALIBRATION LOG",
                "\"Final pipe tuning — 9-band chromatic resonance achieved. All harmonics align to the φ spiral. " +
                "The cathedral is ready. — Z.\" \n\n" +
                "This calibration is mathematically flawless. No enemy of humanity wrote this note.",
                12f);
            Debug.Log("[Moon6Arc] ZerethCalibrationScroll read — seed planted.");
        }

        public string GetInteractPrompt() => _read ? "" : "Read Tuning Calibration Log";
    }

    /// <summary>
    /// Beat 5: One of 4 Moon 6 collectible items dropped after Revelation.
    /// </summary>
    public class Moon6Collectible : MonoBehaviour, IInteractable
    {
        public GameObject Root => gameObject;
        readonly Vector3 _pos;
        readonly string _itemName;
        bool _collected;

        public Moon6Collectible(Vector3 pos, string itemName) { _pos = pos; _itemName = itemName; }

        public void Activate()
        {
            transform.position = _pos;

            var gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gem.transform.SetParent(transform);
            gem.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            gem.transform.localScale = Vector3.one * 0.35f;
            var mr = gem.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                Color c = new Color(0.9f, 0.75f, 0.3f);
                mr.sharedMaterial.color = c;
                mr.sharedMaterial.SetColor("_EmissionColor", c * 2.5f);
                mr.sharedMaterial.EnableKeyword("_EMISSION");
            }
            UnityEngine.Object.Destroy(gem.GetComponent<Collider>());

            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.2f;

            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.85f, 0.4f);
            light.intensity = 2f;
            light.range = 4f;

            StartCoroutine(Bob(light));
        }

        IEnumerator Bob(Light l)
        {
            float t = 0f; Vector3 base_ = transform.position;
            while (!_collected)
            {
                t += Time.deltaTime;
                transform.position = base_ + Vector3.up * (0.4f + Mathf.Sin(t * 2f) * 0.12f);
                l.intensity = 2f + Mathf.Sin(t * 3f) * 0.5f;
                yield return null;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || _collected) return;
            Interact(other.gameObject);
        }

        public void Interact(GameObject player)
        {
            if (_collected) return;
            _collected = true;
            AudioManager.Instance?.PlaySFX2D("Collectible_Moon");
            GameEvents.RaiseHUDShowObjective($"Moon 6 Relic collected: {_itemName}");
            GameEvents.FireCriticalSaveTrigger($"moon6_collected_{_itemName}");
            gameObject.SetActive(false);
        }

        public string GetInteractPrompt() => _collected ? "" : $"Collect: {_itemName}";
    }
}
