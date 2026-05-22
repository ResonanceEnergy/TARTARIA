// Moon 4 — Self-Existing Moon: "The Form of Foundations" — full 5-beat vertical slice.
//
// Scene: Assets/_Project/Scenes/Moons/StarFortBastion.unity
// Self-bootstraps when that scene is active; otherwise dormant.
//
// Beats (per docs/03_CAMPAIGN_13_MOONS.md MOON 4 + docs/03C_MOON_MECHANICS_DETAILED.md):
//   1. Discovery     — Buried star fort emerges. Echo NPC garrison ghosts mutter "the commander..."
//   2. Restoration   — 5-bastion AetherConduit puzzle (φ snap), "For my brother — Z" inscription stone
//   3. Conflict      — Corrupted golem Maelix (30-ft mud+stone humanoid) — giant-mode wrestling
//   4. Climax        — Moats flood, bell tower activates (scalar ping to distant zones),
//                       golem crumbles peacefully, memory crystal drops
//   5. Revelation    — Maelix = Korath's brother. Z = Zereth (Dissonant One). 17-Hour Clock Fragment.
//
// Uses existing systems: AetherConduitMiniGame (5-bastion φ-puzzle), GiantModeController,
// MoonProgressTracker, HUDController, AetherFieldManager, GameEvents, AudioManager,
// InputPromptHelper, IInteractable.
//
// Crossover seeds (PlayerPrefs "moon4_seed_<tag>=1"):
//   moon4_seed_brother_reveal      → Moon 7 (Korath's full reveal of Maelix)
//   moon4_seed_zereth_id           → central mystery (Dissonant One identity)
//   moon4_seed_17hour_fragment     → Moon 9 (clock tower assembly)
//   moon4_seed_routing_node        → Moon 3 (star fort routing empowers trains)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 (Self-Existing) arc orchestrator. Owns the 5-beat vertical slice for the
    /// Star Fort Bastion zone. Idempotent. Self-bootstraps in StarFortBastion scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon4SelfExistingArc : MonoBehaviour
    {
        public static Moon4SelfExistingArc Instance { get; private set; }

        public enum Beat { Discovery = 0, Restoration = 1, Conflict = 2, Climax = 3, Revelation = 4 }

        [Header("Pacing")]
        public float startDelay = 4f;
        public float minBeatTime = 6f;

        Beat _current = Beat.Discovery;
        bool _running;
        readonly HashSet<Beat> _completed = new();
        readonly List<GameObject> _spawnedThisRun = new();

        // Crossover seed tags (saved to PlayerPrefs as "moon4_seed_<tag>")
        public const string SEED_BROTHER_REVEAL  = "brother_reveal";     // → Moon 7
        public const string SEED_ZERETH_ID       = "zereth_id";          // → central mystery
        public const string SEED_17HOUR_FRAGMENT = "17hour_fragment";    // → Moon 9
        public const string SEED_ROUTING_NODE    = "routing_node";       // → Moon 3 train boost

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!active.IsValid()) return;
            if (!active.name.StartsWith("StarFortBastion", System.StringComparison.OrdinalIgnoreCase)) return;
            if (Instance != null) return;

            var go = new GameObject("Moon4SelfExistingArc");
            Instance = go.AddComponent<Moon4SelfExistingArc>();
            Debug.Log("[Moon4Arc] Bootstrapped in StarFortBastion zone.");
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            MoonBeatRunner.OnBeatStarted += HandleBeatRunner;
        }

        void OnDisable()
        {
            MoonBeatRunner.OnBeatStarted -= HandleBeatRunner;
        }

        void Start()
        {
            StartCoroutine(RunArc());
        }

        void HandleBeatRunner(int moon, MoonBeatRunner.Beat b)
        {
            if (moon != 4) return;
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
            MoonProgressTracker.Instance?.MarkBeatCleared(4, 0);

            yield return Beat2_Restoration();
            _completed.Add(Beat.Restoration);
            MoonProgressTracker.Instance?.MarkBeatCleared(4, 1);

            yield return Beat3_Conflict();
            _completed.Add(Beat.Conflict);
            MoonProgressTracker.Instance?.MarkBeatCleared(4, 2);

            yield return Beat4_Climax();
            _completed.Add(Beat.Climax);
            MoonProgressTracker.Instance?.MarkBeatCleared(4, 3);

            yield return Beat5_Revelation();
            _completed.Add(Beat.Revelation);
            MoonProgressTracker.Instance?.MarkBeatCleared(4, 4);

            MoonProgressTracker.Instance?.MarkCleared(4);
            GameEvents.FireCriticalSaveTrigger("moon4_arc_complete");
            HUDController.Instance?.ShowBanner(
                "MOON 4 COMPLETE",
                "The Star Fort holds. The grid extends. Three brothers — one cleansed, one ally, one hunter.",
                8f);
            Debug.Log("[Moon4Arc] Self-Existing Moon arc COMPLETE — Moon 5 unlocked.");

            _running = false;
        }

        // ───────────────────────────────────────────────────────────────
        // BEAT 1 — Discovery
        //   Buried star fort. Garrison echo ghosts. "The commander..."
        // ───────────────────────────────────────────────────────────────
        IEnumerator Beat1_Discovery()
        {
            _current = Beat.Discovery;
            Debug.Log("[Moon4Arc] Beat 1 — Discovery");

            HUDController.Instance?.ShowBanner(
                "MOON 4 — DISCOVERY",
                "The Self-Existing Moon. Geometric bastions cracked from the mud. Something below resists your tuning.",
                7f);

            Vector3 playerPos = SafePlayerPos();

            // Spawn 3 garrison ghosts wandering the fort foundation
            for (int i = 0; i < 3; i++)
            {
                float ang = (i / 3f) * Mathf.PI * 2f;
                Vector3 pos = playerPos + new Vector3(Mathf.Cos(ang) * 10f, 0f, Mathf.Sin(ang) * 10f);
                _spawnedThisRun.Add(GarrisonEchoGhost.Spawn(pos).gameObject);
            }

            yield return new WaitForSeconds(3f);
            HUDController.Instance?.ShowObjective($"Approach the garrison echoes. Use {InputPromptHelper.Scan} to scan their resonance trail.");
            AudioManager.Instance?.PlaySFX2D("Discovery");

            yield return new WaitForSeconds(Mathf.Max(minBeatTime, 6f));
        }

        // ───────────────────────────────────────────────────────────────
        // BEAT 2 — Restoration
        //   AetherConduitMiniGame (5-bastion φ-puzzle) + Z-inscription stone
        // ───────────────────────────────────────────────────────────────
        IEnumerator Beat2_Restoration()
        {
            _current = Beat.Restoration;
            Debug.Log("[Moon4Arc] Beat 2 — Restoration");

            HUDController.Instance?.ShowBanner(
                "RESTORATION — STAR FORT",
                "Place 5 bastions. Snap each to the golden ratio. The geometry will sing.",
                6f);

            // Drop the Zereth inscription stone
            Vector3 p = SafePlayerPos();
            _spawnedThisRun.Add(ZerethInscriptionStone.Spawn(p + new Vector3(6f, 0f, 4f)).gameObject);

            // Find or create the conduit mini-game
            var conduit = FindAnyObjectByType<AetherConduitMiniGame>();
            if (conduit == null)
            {
                var conduitGO = new GameObject("Moon4_AetherConduit");
                conduit = conduitGO.AddComponent<AetherConduitMiniGame>();
                _spawnedThisRun.Add(conduitGO);
            }

            HUDController.Instance?.ShowObjective(
                $"Place 5 bastions on φ-snap nodes. {InputPromptHelper.Interact} to begin the conduit puzzle.");

            // Track completion via the puzzle's event
            bool completed = false;
            float bestAccuracy = 0f;
            void OnDone(float quality) { completed = true; bestAccuracy = quality; }
            conduit.OnPuzzleCompleted += OnDone;

            // Kick off the puzzle automatically after a short pause so the player can read prompts
            yield return new WaitForSeconds(4f);
            if (!conduit.IsActive && !conduit.IsCompleted)
                conduit.StartPuzzle();

            // Wait for completion or 90s cap
            float t0 = Time.time;
            while (!completed && Time.time - t0 < 90f)
                yield return new WaitForSeconds(0.5f);

            conduit.OnPuzzleCompleted -= OnDone;

            if (completed)
            {
                HUDController.Instance?.ShowBanner(
                    "GEOMETRY SINGS",
                    $"The five bastions resonate. φ-accuracy: {(bestAccuracy * 100f):F0}%. Routing node armed.",
                    5f);
                AetherFieldManager.Instance?.AddResonanceScore(15f + bestAccuracy * 10f);
                GameEvents.FireBuildingRestored("star_fort_routing_node");
            }
            else
            {
                HUDController.Instance?.ShowBanner(
                    "GEOMETRY HOLDS",
                    "The fort accepts a partial alignment. The commander still slumbers.",
                    4f);
            }
            AudioManager.Instance?.PlaySFX2D("BuildingRestore");

            yield return new WaitForSeconds(Mathf.Max(minBeatTime - 2f, 2f));
        }

        // ───────────────────────────────────────────────────────────────
        // BEAT 3 — Conflict
        //   Maelix golem appears (30-ft mud+stone humanoid). Giant-mode wrestling.
        // ───────────────────────────────────────────────────────────────
        IEnumerator Beat3_Conflict()
        {
            _current = Beat.Conflict;
            Debug.Log("[Moon4Arc] Beat 3 — Conflict");

            HUDController.Instance?.ShowBanner(
                "CORRUPTED GOLEM — MAELIX",
                $"Thirty feet of mud, stone, and broken song. Activate Giant Mode. {InputPromptHelper.Strike} to grapple.",
                7f);

            // Spawn Maelix 22m in front of player
            Vector3 spawnPos = SafePlayerPos() + SafePlayerForward() * 22f;
            var golem = MaelixGolem.Spawn(spawnPos);
            _spawnedThisRun.Add(golem.gameObject);

            // Prompt giant mode after 4s
            yield return new WaitForSeconds(4f);
            TryActivateGiantMode();

            HUDController.Instance?.ShowObjective("Wrestle Maelix to submission. Strike harmonic blows — do not destroy him.");

            // Watch HP: peaceful cleanse at 30% HP (no kill). 90s cap.
            float t0 = Time.time;
            while (golem != null && !golem.IsCleansed && Time.time - t0 < 90f)
            {
                if (golem.HPFraction <= 0.30f && !golem.IsCleansed)
                    golem.BeginCleansing();
                yield return new WaitForSeconds(0.5f);
            }

            // Force cleanse if timed out
            if (golem != null && !golem.IsCleansed)
                golem.BeginCleansing();

            // Wait for the cleanse animation to fully play out
            yield return new WaitForSeconds(4f);

            yield return new WaitForSeconds(Mathf.Max(minBeatTime - 4f, 2f));
        }

        // ───────────────────────────────────────────────────────────────
        // BEAT 4 — Climax
        //   Moats flood. Bell tower activates. Scalar ping lights distant spires.
        //   Memory crystal drops from cleansed golem.
        // ───────────────────────────────────────────────────────────────
        IEnumerator Beat4_Climax()
        {
            _current = Beat.Climax;
            Debug.Log("[Moon4Arc] Beat 4 — Climax");

            HUDController.Instance?.ShowBanner(
                "MOATS FLOOD — TOWER WAKES",
                "Conductive pure-water rushes in. The bell tower hums. Distant spires answer.",
                7f);

            Vector3 center = SafePlayerPos();

            // Rising moat water plane
            var moat = MoatFloodPlane.Spawn(center);
            _spawnedThisRun.Add(moat.gameObject);

            // Bell tower with beam-up light
            var bellTower = StarFortBellTower.Spawn(center + new Vector3(0f, 0f, 14f));
            _spawnedThisRun.Add(bellTower.gameObject);

            // Memory crystal from Maelix (drop near player)
            var crystal = MaelixMemoryCrystal.Spawn(center + SafePlayerForward() * 6f + Vector3.up * 0.6f);
            _spawnedThisRun.Add(crystal.gameObject);

            HUDController.Instance?.ShowObjective($"Activate the bell tower. {InputPromptHelper.Interact} the resonating cylinder.");

            // Wait for tower activation OR 60s cap
            float t0 = Time.time;
            while (bellTower != null && !bellTower.IsActivated && Time.time - t0 < 60f)
                yield return new WaitForSeconds(0.5f);

            if (bellTower != null && !bellTower.IsActivated)
                bellTower.ForceActivate();

            // Spectacle reward
            HUDController.Instance?.ShowBanner(
                "SCALAR PING — GRID EXTENDS",
                "Across the horizon, distant towers flicker awake. The Continental Rail (Moon 3) glows brighter.",
                7f);
            AetherFieldManager.Instance?.AddResonanceScore(25f);
            AudioManager.Instance?.PlaySFX2D("BuildingRestore");

            yield return new WaitForSeconds(Mathf.Max(minBeatTime, 4f));
        }

        // ───────────────────────────────────────────────────────────────
        // BEAT 5 — Revelation
        //   Maelix = Korath's brother. Z = Zereth (Dissonant One). 17-Hour Clock Fragment.
        // ───────────────────────────────────────────────────────────────
        IEnumerator Beat5_Revelation()
        {
            _current = Beat.Revelation;
            Debug.Log("[Moon4Arc] Beat 5 — Revelation");

            Vector3 pos = SafePlayerPos() + SafePlayerForward() * 5f + Vector3.up * 0.5f;
            var reveal = MaelixRevelationEncounter.Spawn(pos);
            _spawnedThisRun.Add(reveal.gameObject);

            HUDController.Instance?.ShowBanner(
                "REVELATION — THREE BROTHERS",
                "The golem was Maelix. The inscription 'Z' was Zereth — the Dissonant One. Korath has two brothers.",
                8f);

            yield return reveal.PlayCoroutine();

            // Drop the 4 crossover seeds at the player's feet
            Vector3 dropBase = SafePlayerPos() + Vector3.up * 0.4f;
            _spawnedThisRun.Add(Moon4Collectible.Spawn(dropBase + new Vector3( 1.2f, 0f,  0f), SEED_BROTHER_REVEAL,
                "Maelix's Memory Crystal", "Korath's grief becomes power in Moon 7.").gameObject);
            _spawnedThisRun.Add(Moon4Collectible.Spawn(dropBase + new Vector3(-1.2f, 0f,  0f), SEED_ZERETH_ID,
                "Zereth's Mark — 'Z'", "The Dissonant One has a name. The hunt begins.").gameObject);
            _spawnedThisRun.Add(Moon4Collectible.Spawn(dropBase + new Vector3( 0f,   0f,  1.2f), SEED_17HOUR_FRAGMENT,
                "17-Hour Clock Fragment", "1 of N. The Tartarian day was longer than ours.").gameObject);
            _spawnedThisRun.Add(Moon4Collectible.Spawn(dropBase + new Vector3( 0f,   0f, -1.2f), SEED_ROUTING_NODE,
                "Star Fort Routing Node", "Boosts Continental Rail power on revisit (Moon 3).").gameObject);

            HUDController.Instance?.ShowObjective(
                $"Collect the 4 crossover seeds. {InputPromptHelper.Interact} on each.");

            yield return new WaitForSeconds(Mathf.Max(minBeatTime, 5f));
        }

        // ───────────────────────────────────────────────────────────────
        // Helpers
        // ───────────────────────────────────────────────────────────────
        Vector3 SafePlayerPos()
        {
            var p = GameObject.FindWithTag("Player");
            return p != null ? p.transform.position : Vector3.zero;
        }
        Vector3 SafePlayerForward()
        {
            var p = GameObject.FindWithTag("Player");
            return p != null ? p.transform.forward : Vector3.forward;
        }

        void TryActivateGiantMode()
        {
            var gm = GiantModeController.Instance;
            if (gm == null) return;
            var mi = gm.GetType().GetMethod("Activate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (mi != null) mi.Invoke(gm, null);
            else gm.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Garrison Echo Ghost — wandering translucent NPC
    // ═══════════════════════════════════════════════════════════════════
    public class GarrisonEchoGhost : MonoBehaviour
    {
        static readonly string[] _lines = {
            "The commander… something happened to the commander…",
            "The song was wrong. He could not unhear it.",
            "We held the eastern bastion. Then the eastern bastion held us."
        };

        float _nextMutter;
        int _lineIdx;
        Vector3 _wanderTarget;

        public static GarrisonEchoGhost Spawn(Vector3 pos)
        {
            var root = new GameObject("GarrisonEcho");
            root.transform.position = pos;

            // Translucent capsule body
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
            ApplyGhostMaterial(body);
            Object.Destroy(body.GetComponent<Collider>());

            // Helmet (cylinder)
            var helmet = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            helmet.name = "Helmet";
            helmet.transform.SetParent(root.transform, false);
            helmet.transform.localPosition = new Vector3(0f, 1.95f, 0f);
            helmet.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f);
            ApplyGhostMaterial(helmet);
            Object.Destroy(helmet.GetComponent<Collider>());

            // Soft cyan glow
            var l = new GameObject("GhostGlow").AddComponent<Light>();
            l.transform.SetParent(root.transform, false);
            l.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            l.type = LightType.Point;
            l.range = 5f;
            l.intensity = 1.4f;
            l.color = new Color(0.55f, 0.85f, 1f);

            var g = root.AddComponent<GarrisonEchoGhost>();
            g._wanderTarget = pos;
            return g;
        }

        static void ApplyGhostMaterial(GameObject go)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(sh);
            m.color = new Color(0.6f, 0.85f, 1f, 0.55f);
            m.SetFloat("_Surface", 1f); // transparent (URP)
            m.SetFloat("_Blend", 0f);   // alpha blend
            m.SetColor("_EmissionColor", new Color(0.3f, 0.55f, 0.9f) * 1.2f);
            m.EnableKeyword("_EMISSION");
            m.renderQueue = 3000;
            go.GetComponent<MeshRenderer>().material = m;
        }

        void Update()
        {
            // Slow wander
            Vector3 toTarget = _wanderTarget - transform.position; toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.5f)
                _wanderTarget = transform.position + new Vector3(Random.Range(-6f, 6f), 0f, Random.Range(-6f, 6f));
            else
                transform.position += toTarget.normalized * 0.6f * Time.deltaTime;

            // Periodic banner mutter
            if (Time.time >= _nextMutter)
            {
                _nextMutter = Time.time + Random.Range(8f, 16f);
                var line = _lines[_lineIdx % _lines.Length];
                _lineIdx++;
                HUDController.Instance?.ShowBanner("Garrison Echo", line, 3f);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Zereth Inscription Stone — readable lore prop
    // ═══════════════════════════════════════════════════════════════════
    public class ZerethInscriptionStone : MonoBehaviour, IInteractable
    {
        bool _read;

        public static ZerethInscriptionStone Spawn(Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "ZerethInscriptionStone";
            go.transform.position = pos + Vector3.up * 1.2f;
            go.transform.localScale = new Vector3(0.9f, 2.4f, 0.4f);
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(sh);
            m.color = new Color(0.42f, 0.40f, 0.38f);
            m.SetColor("_EmissionColor", new Color(0.55f, 0.45f, 0.2f) * 0.5f);
            m.EnableKeyword("_EMISSION");
            go.GetComponent<MeshRenderer>().material = m;
            // Replace box collider with trigger
            var bc = go.GetComponent<BoxCollider>();
            if (bc != null) bc.isTrigger = true;
            return go.AddComponent<ZerethInscriptionStone>();
        }

        public string GetInteractPrompt() =>
            _read ? "The inscription is read." : $"{InputPromptHelper.Interact} Read the inscription";

        public void Interact(GameObject player)
        {
            if (_read) return;
            _read = true;
            HUDController.Instance?.ShowBanner(
                "INSCRIPTION — OLD TARTARIAN",
                "\"For my brother, the Builder. Hold the line. — Z.\"",
                7f);
            AudioManager.Instance?.PlaySFX2D("Discovery");
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Maelix Golem — corrupted guardian giant (30ft, mud+stone)
    // ═══════════════════════════════════════════════════════════════════
    public class MaelixGolem : MonoBehaviour
    {
        public float maxHealth = 250f;
        public float moveSpeed = 1.4f;
        public float attackRange = 5f;
        public float attackDamage = 12f;
        public float attackCooldown = 3f;

        Transform _target;
        float _hp;
        float _nextAttack;
        bool _cleansed;
        Renderer[] _rends;
        Light _coreGlow;

        public float HPFraction => _hp / maxHealth;
        public bool IsCleansed => _cleansed;

        public static MaelixGolem Spawn(Vector3 pos)
        {
            var root = new GameObject("MaelixGolem");
            root.transform.position = pos;

            // Scale = ~30ft (~9 meters). Build at unit scale, then root-scale 5x.
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var stone = new Material(sh);
            stone.color = new Color(0.35f, 0.28f, 0.22f);
            stone.SetFloat("_Smoothness", 0.15f);
            var mud = new Material(sh);
            mud.color = new Color(0.22f, 0.18f, 0.14f);
            mud.SetFloat("_Smoothness", 0.05f);

            // Torso (capsule)
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(root.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            torso.transform.localScale = new Vector3(1.4f, 1.2f, 1f);
            torso.GetComponent<MeshRenderer>().material = stone;
            Object.Destroy(torso.GetComponent<Collider>());

            // Head (cube — squared off)
            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 2.7f, 0f);
            head.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            head.GetComponent<MeshRenderer>().material = stone;
            Object.Destroy(head.GetComponent<Collider>());

            // Shoulders (cubes)
            for (int s = -1; s <= 1; s += 2)
            {
                var shoulder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shoulder.name = $"Shoulder_{s}";
                shoulder.transform.SetParent(root.transform, false);
                shoulder.transform.localPosition = new Vector3(s * 1.0f, 2.1f, 0f);
                shoulder.transform.localScale = new Vector3(0.7f, 0.7f, 0.8f);
                shoulder.GetComponent<MeshRenderer>().material = stone;
                Object.Destroy(shoulder.GetComponent<Collider>());

                var arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                arm.name = $"Arm_{s}";
                arm.transform.SetParent(root.transform, false);
                arm.transform.localPosition = new Vector3(s * 1.3f, 1.2f, 0.2f);
                arm.transform.localScale = new Vector3(0.32f, 1.2f, 0.32f);
                arm.GetComponent<MeshRenderer>().material = mud;
                Object.Destroy(arm.GetComponent<Collider>());

                var fist = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fist.name = $"Fist_{s}";
                fist.transform.SetParent(root.transform, false);
                fist.transform.localPosition = new Vector3(s * 1.3f, 0.1f, 0.3f);
                fist.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
                fist.GetComponent<MeshRenderer>().material = stone;
                Object.Destroy(fist.GetComponent<Collider>());
            }

            // Legs (cylinders)
            for (int s = -1; s <= 1; s += 2)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = $"Leg_{s}";
                leg.transform.SetParent(root.transform, false);
                leg.transform.localPosition = new Vector3(s * 0.45f, 0.1f, 0f);
                leg.transform.localScale = new Vector3(0.4f, 0.9f, 0.4f);
                leg.GetComponent<MeshRenderer>().material = mud;
                Object.Destroy(leg.GetComponent<Collider>());
            }

            // Dissonance core glow (red) in chest
            var core = new GameObject("DissonanceCore").AddComponent<Light>();
            core.transform.SetParent(root.transform, false);
            core.transform.localPosition = new Vector3(0f, 1.4f, 0.5f);
            core.type = LightType.Point;
            core.color = new Color(1.0f, 0.25f, 0.18f);
            core.range = 6f;
            core.intensity = 5f;

            // Root scale = 5x to read at ~30ft
            root.transform.localScale = Vector3.one * 5f;

            // Collider + rigidbody on root (scaled to fit)
            var caps = root.AddComponent<CapsuleCollider>();
            caps.center = new Vector3(0f, 1.3f, 0f);
            caps.radius = 1.2f;
            caps.height = 3.2f;
            var rb = root.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.mass = 40f;

            var m = root.AddComponent<MaelixGolem>();
            m._coreGlow = core;
            return m;
        }

        void Start()
        {
            _hp = maxHealth;
            _rends = GetComponentsInChildren<Renderer>();
            _target = GameObject.FindWithTag("Player")?.transform;
            // Distorted voice line on spawn
            HUDController.Instance?.ShowBanner("Maelix (distorted)", "The song… the song was… WRONG…", 5f);
            AudioManager.Instance?.PlaySFX2D("GolemAwaken");
        }

        void Update()
        {
            if (_cleansed) return;
            if (_target == null)
            {
                _target = GameObject.FindWithTag("Player")?.transform;
                return;
            }

            float dist = Vector3.Distance(transform.position, _target.position);
            Vector3 dir = (_target.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), Time.deltaTime * 2f);

            if (dist > attackRange)
            {
                Vector3 step = dir.normalized * moveSpeed * Time.deltaTime;
                transform.position += new Vector3(step.x, 0f, step.z);
            }
            else if (Time.time >= _nextAttack)
            {
                _nextAttack = Time.time + attackCooldown;
                var ph = _target.GetComponent("PlayerHealth") as MonoBehaviour;
                if (ph != null)
                {
                    var mi = ph.GetType().GetMethod("TakeDamage", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (mi != null) mi.Invoke(ph, new object[] { attackDamage });
                }
                AudioManager.Instance?.PlaySFX2D("HarmonicHit");
            }
        }

        public void TakeDamage(float amt)
        {
            if (_cleansed) return;
            _hp = Mathf.Max(0f, _hp - amt);
        }

        public void BeginCleansing()
        {
            if (_cleansed) return;
            _cleansed = true;
            StartCoroutine(CleansingSequence());
        }

        IEnumerator CleansingSequence()
        {
            HUDController.Instance?.ShowBanner(
                "GOLEM CLEANSED",
                "The dissonance leaves him. He kneels. The mud falls away in silver flakes.",
                6f);
            AudioManager.Instance?.PlaySFX2D("HarmonicChoir");

            // Disable AI collisions
            var col = GetComponent<Collider>(); if (col != null) col.enabled = false;
            var rb = GetComponent<Rigidbody>(); if (rb != null) rb.isKinematic = true;

            // Core glow fades from red → gold → out over 5s
            float t = 0f;
            while (t < 5f)
            {
                t += Time.deltaTime;
                float k = t / 5f;
                if (_coreGlow != null)
                {
                    _coreGlow.color = Color.Lerp(new Color(1f, 0.25f, 0.18f), new Color(1f, 0.85f, 0.4f), k);
                    _coreGlow.intensity = Mathf.Lerp(5f, 0f, k);
                }
                // Sink kneel
                transform.position += Vector3.down * 0.05f * Time.deltaTime;
                yield return null;
            }

            // Crumble — destroy children one by one
            var children = new List<Transform>();
            foreach (Transform c in transform) children.Add(c);
            foreach (var c in children)
            {
                if (c != null) Destroy(c.gameObject);
                yield return new WaitForSeconds(0.15f);
            }
            // Keep root alive briefly so memory crystal placement has a spawn anchor
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        void OnHarmonicHit(float dmg) => TakeDamage(dmg);
    }

    // ═══════════════════════════════════════════════════════════════════
    // Moat Flood Plane — rising blue translucent water
    // ═══════════════════════════════════════════════════════════════════
    public class MoatFloodPlane : MonoBehaviour
    {
        public static MoatFloodPlane Spawn(Vector3 center)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "Moon4_MoatFlood";
            go.transform.position = center + new Vector3(0f, -2f, 0f);
            go.transform.localScale = new Vector3(8f, 1f, 8f); // 80m x 80m
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(sh);
            m.color = new Color(0.25f, 0.55f, 0.85f, 0.65f);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_Smoothness", 0.92f);
            m.SetColor("_EmissionColor", new Color(0.2f, 0.45f, 0.85f) * 0.4f);
            m.EnableKeyword("_EMISSION");
            m.renderQueue = 3000;
            go.GetComponent<MeshRenderer>().material = m;
            var col = go.GetComponent<Collider>(); if (col != null) Object.Destroy(col);
            return go.AddComponent<MoatFloodPlane>();
        }

        void Update()
        {
            // Rise slowly toward y = +0.2 over ~8 seconds, then hold
            if (transform.position.y < 0.2f)
                transform.position += Vector3.up * 0.3f * Time.deltaTime;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Star Fort Bell Tower — climax interactable
    // ═══════════════════════════════════════════════════════════════════
    public class StarFortBellTower : MonoBehaviour, IInteractable
    {
        public bool IsActivated { get; private set; }
        Light _beamLight;

        public static StarFortBellTower Spawn(Vector3 pos)
        {
            var root = new GameObject("StarFortBellTower");
            root.transform.position = pos;

            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var stone = new Material(sh);
            stone.color = new Color(0.78f, 0.74f, 0.68f);

            // Multi-part bell tower structure
            // Foundation base
            var towerBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            towerBase.name = "Base";
            towerBase.transform.SetParent(root.transform, false);
            towerBase.transform.localPosition = new Vector3(0f, 1f, 0f);
            towerBase.transform.localScale = new Vector3(2.2f, 2f, 2.2f);
            towerBase.GetComponent<MeshRenderer>().material = stone;
            Object.Destroy(towerBase.GetComponent<Collider>());

            // Lower shaft segment
            var shaftLower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaftLower.name = "ShaftLower";
            shaftLower.transform.SetParent(root.transform, false);
            shaftLower.transform.localPosition = new Vector3(0f, 3.5f, 0f);
            shaftLower.transform.localScale = new Vector3(1.8f, 3f, 1.8f);
            shaftLower.GetComponent<MeshRenderer>().material = stone;
            Object.Destroy(shaftLower.GetComponent<Collider>());

            // Upper shaft segment
            var shaftUpper = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaftUpper.name = "ShaftUpper";
            shaftUpper.transform.SetParent(root.transform, false);
            shaftUpper.transform.localPosition = new Vector3(0f, 6.5f, 0f);
            shaftUpper.transform.localScale = new Vector3(1.6f, 3f, 1.6f);
            shaftUpper.GetComponent<MeshRenderer>().material = stone;
            Object.Destroy(shaftUpper.GetComponent<Collider>());

            // Bell chamber platform
            var bellChamber = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bellChamber.name = "BellChamber";
            bellChamber.transform.SetParent(root.transform, false);
            bellChamber.transform.localPosition = new Vector3(0f, 9.2f, 0f);
            bellChamber.transform.localScale = new Vector3(2f, 0.4f, 2f);
            bellChamber.GetComponent<MeshRenderer>().material = stone;
            Object.Destroy(bellChamber.GetComponent<Collider>());

            // Bell (sphere on top)
            var bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bell.name = "Bell";
            bell.transform.SetParent(root.transform, false);
            bell.transform.localPosition = new Vector3(0f, 10f, 0f);
            bell.transform.localScale = Vector3.one * 1.3f;
            var bronze = new Material(sh);
            bronze.color = new Color(0.65f, 0.45f, 0.18f);
            bronze.SetFloat("_Metallic", 0.85f);
            bronze.SetFloat("_Smoothness", 0.55f);
            bronze.SetColor("_EmissionColor", new Color(0.9f, 0.6f, 0.2f) * 0.4f);
            bronze.EnableKeyword("_EMISSION");
            bell.GetComponent<MeshRenderer>().material = bronze;
            Object.Destroy(bell.GetComponent<Collider>());

            // Trigger collider for interaction
            var trig = root.AddComponent<SphereCollider>();
            trig.isTrigger = true;
            trig.center = new Vector3(0f, 4f, 0f);
            trig.radius = 4.5f;

            // Beam-up light (spot pointing skyward)
            var beam = new GameObject("BeamLight").AddComponent<Light>();
            beam.transform.SetParent(root.transform, false);
            beam.transform.localPosition = new Vector3(0f, 9f, 0f);
            beam.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            beam.type = LightType.Spot;
            beam.spotAngle = 25f;
            beam.range = 60f;
            beam.intensity = 0f; // off until activated
            beam.color = new Color(0.95f, 0.85f, 0.4f);

            var t = root.AddComponent<StarFortBellTower>();
            t._beamLight = beam;
            return t;
        }

        public string GetInteractPrompt() =>
            IsActivated
                ? "The bell tower hums in harmony."
                : $"{InputPromptHelper.Interact} Strike the bell — scalar ping";

        public void Interact(GameObject player)
        {
            if (IsActivated) return;
            ForceActivate();
        }

        public void ForceActivate()
        {
            if (IsActivated) return;
            IsActivated = true;
            StartCoroutine(ActivationSequence());
        }

        IEnumerator ActivationSequence()
        {
            HUDController.Instance?.ShowBanner("THE BELL TOLLS", "Scalar waves climb the tower. The grid widens.", 5f);
            AudioManager.Instance?.PlaySFX2D("BellTower");
            GameEvents.FireBuildingRestored("star_fort_bell_tower");

            // Climb intensity over 4s
            float t = 0f;
            while (t < 4f)
            {
                t += Time.deltaTime;
                if (_beamLight != null) _beamLight.intensity = Mathf.Lerp(0f, 12f, t / 4f);
                yield return null;
            }
            if (_beamLight != null) _beamLight.intensity = 12f;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Maelix Memory Crystal — interactable that plays giant's final memory
    // ═══════════════════════════════════════════════════════════════════
    public class MaelixMemoryCrystal : MonoBehaviour, IInteractable
    {
        bool _played;

        public static MaelixMemoryCrystal Spawn(Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "MaelixMemoryCrystal";
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
            go.transform.localRotation = Quaternion.Euler(0f, 45f, 30f);
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(sh);
            m.color = new Color(0.85f, 0.7f, 0.35f, 0.85f);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_Smoothness", 0.95f);
            m.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.35f) * 2.2f);
            m.EnableKeyword("_EMISSION");
            m.renderQueue = 3000;
            go.GetComponent<MeshRenderer>().material = m;
            var bc = go.GetComponent<BoxCollider>(); if (bc != null) bc.isTrigger = true;

            var l = new GameObject("CrystalGlow").AddComponent<Light>();
            l.transform.SetParent(go.transform, false);
            l.transform.localPosition = Vector3.zero;
            l.type = LightType.Point;
            l.color = new Color(1f, 0.8f, 0.4f);
            l.range = 6f;
            l.intensity = 3f;

            return go.AddComponent<MaelixMemoryCrystal>();
        }

        void Update()
        {
            transform.Rotate(0f, 30f * Time.deltaTime, 0f, Space.World);
        }

        public string GetInteractPrompt() =>
            _played ? "The memory has played." : $"{InputPromptHelper.Interact} Touch the memory crystal";

        public void Interact(GameObject player)
        {
            if (_played) return;
            _played = true;
            HUDController.Instance?.ShowBanner(
                "MAELIX'S FINAL MEMORY",
                "A giant kneels before a starlit fort. A voice he loves whispers a song he cannot quite hold. Then static.",
                8f);
            AudioManager.Instance?.PlaySFX2D("HarmonicChoir");
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Maelix Revelation Encounter — Beat 5 dialogue + Korath echo
    // ═══════════════════════════════════════════════════════════════════
    public class MaelixRevelationEncounter : MonoBehaviour
    {
        public static MaelixRevelationEncounter Spawn(Vector3 pos)
        {
            var root = new GameObject("MaelixRevelationEncounter");
            root.transform.position = pos;
            // A faint kneeling silhouette (capsule scaled wide, low)
            var silhouette = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            silhouette.name = "KorathEcho";
            silhouette.transform.SetParent(root.transform, false);
            silhouette.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            silhouette.transform.localScale = new Vector3(0.9f, 0.7f, 0.9f);
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(sh);
            m.color = new Color(0.85f, 0.7f, 0.5f, 0.6f);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetColor("_EmissionColor", new Color(0.95f, 0.7f, 0.35f) * 1.5f);
            m.EnableKeyword("_EMISSION");
            m.renderQueue = 3000;
            silhouette.GetComponent<MeshRenderer>().material = m;
            Object.Destroy(silhouette.GetComponent<Collider>());

            var l = new GameObject("RevelationGlow").AddComponent<Light>();
            l.transform.SetParent(root.transform, false);
            l.transform.localPosition = Vector3.up * 1f;
            l.type = LightType.Point;
            l.range = 8f;
            l.intensity = 2.5f;
            l.color = new Color(1f, 0.8f, 0.45f);

            return root.AddComponent<MaelixRevelationEncounter>();
        }

        public IEnumerator PlayCoroutine()
        {
            HUDController.Instance?.ShowBanner("Korath (memory)", "\"Maelix. Brother. They made you wrong. I am so sorry.\"", 5f);
            yield return new WaitForSeconds(5f);
            HUDController.Instance?.ShowBanner("Korath (memory)", "\"The 'Z' on the stone… that was Zereth. Our youngest. He survived.\"", 5f);
            yield return new WaitForSeconds(5f);
            HUDController.Instance?.ShowBanner("Korath (memory)", "\"The Dissonant One is family. The hunt is family. May the song forgive us all.\"", 6f);
            yield return new WaitForSeconds(2f);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Moon 4 Collectible — crossover seed (mirrors Moon1Collectible behavior)
    // ═══════════════════════════════════════════════════════════════════
    public class Moon4Collectible : MonoBehaviour, IInteractable
    {
        public string seedTag;
        public string title;
        public string body;
        bool _collected;

        public static Moon4Collectible Spawn(Vector3 pos, string tag, string title, string body)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Moon4_Seed_" + tag;
            go.transform.position = pos + Vector3.up * 1.2f;
            go.transform.localScale = Vector3.one * 0.5f;
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(sh);
            Color c = ColorForSeed(tag);
            m.color = c;
            m.SetFloat("_Smoothness", 0.9f);
            m.SetColor("_EmissionColor", c * 2.5f);
            m.EnableKeyword("_EMISSION");
            go.GetComponent<MeshRenderer>().material = m;
            var sc = go.GetComponent<SphereCollider>(); if (sc != null) sc.isTrigger = true;

            var l = new GameObject("SeedGlow").AddComponent<Light>();
            l.transform.SetParent(go.transform, false);
            l.transform.localPosition = Vector3.zero;
            l.type = LightType.Point;
            l.color = c;
            l.range = 4f;
            l.intensity = 2f;

            go.AddComponent<Moon4CollectibleBobber>();

            var col = go.AddComponent<Moon4Collectible>();
            col.seedTag = tag;
            col.title = title;
            col.body = body;
            return col;
        }

        static Color ColorForSeed(string tag) => tag switch
        {
            Moon4SelfExistingArc.SEED_BROTHER_REVEAL  => new Color(0.95f, 0.7f, 0.35f),  // amber (memory)
            Moon4SelfExistingArc.SEED_ZERETH_ID       => new Color(0.85f, 0.2f, 0.25f),  // crimson (Dissonant)
            Moon4SelfExistingArc.SEED_17HOUR_FRAGMENT => new Color(0.45f, 0.9f, 0.95f),  // cyan (chronology)
            Moon4SelfExistingArc.SEED_ROUTING_NODE    => new Color(0.55f, 0.85f, 0.4f),  // green (Moon 3 rails)
            _ => Color.white
        };

        public string GetInteractPrompt() =>
            _collected ? "Collected." : $"{InputPromptHelper.Interact} Take \"{title}\"";

        public void Interact(GameObject player)
        {
            if (_collected) return;
            _collected = true;
            PlayerPrefs.SetInt("moon4_seed_" + seedTag, 1);
            PlayerPrefs.Save();
            HUDController.Instance?.ShowBanner(title, body, 5f);
            AudioManager.Instance?.PlaySFX2D("Discovery");
            // Float up and dissolve
            StartCoroutine(CollectAnim());
        }

        IEnumerator CollectAnim()
        {
            float t = 0f;
            Vector3 start = transform.position;
            while (t < 1.2f)
            {
                t += Time.deltaTime;
                transform.position = start + Vector3.up * (t * 1.5f);
                transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 0f, t / 1.2f);
                yield return null;
            }
            Destroy(gameObject);
        }

        public static bool HasSeed(string tag) => PlayerPrefs.GetInt("moon4_seed_" + tag, 0) == 1;
    }

    public class Moon4CollectibleBobber : MonoBehaviour
    {
        Vector3 _origin;
        float _phase;
        void Start() { _origin = transform.position; _phase = Random.value * Mathf.PI * 2f; }
        void Update()
        {
            transform.position = _origin + Vector3.up * Mathf.Sin(Time.time * 2f + _phase) * 0.15f;
            transform.Rotate(0f, 60f * Time.deltaTime, 0f, Space.World);
        }
    }
}
