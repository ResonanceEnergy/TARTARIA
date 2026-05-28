// Moon 1 — Magnetic Moon: "The Pull of Awakening" — full 5-beat vertical slice.
//
// Hub scene: Echohaven_VerticalSlice (Moon 1 = the starting hub).
// Self-bootstraps when that scene is active; otherwise dormant.
//
// Beats (per docs/03_CAMPAIGN_13_MOONS.md + docs/03C_MOON_MECHANICS_DETAILED.md):
//   1. Discovery     — Milo intro + scanner FTUE + first dig site reveal
//   2. Restoration   — Dome/Fountain/Spire restoration prompts + lore fragments (3-6-9, golden ratio)
//   3. Conflict      — Reset Scouts arrive (Victorian goons w/ jackhammers); tutorial combat + first giant-mode burst
//   4. Climax        — Buried Beacon (mercury-ball spire) + 17th-hour alignment tune event
//   5. Revelation    — Lirael appears, 432 Hz lullaby, crossover seeds drop (Moon 5 spire fragment, giant key #1, airship fragment)
//
// All gameplay uses existing systems: ResonanceScannerSystem, ExcavationSystem, TuningMiniGame,
// BellTowerSyncMiniGame, GiantModeController, EchohavenProgressionSystem, MoonProgressTracker,
// SkillTreeSystem (E_FountainEcho/Dome/Spire/HubAwakened), HUDController.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Gameplay;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 1 (Magnetic) arc orchestrator. Owns the 5-beat tutorial-grade vertical slice
    /// for the Echohaven hub. Idempotent. Subscribes to MoonBeatRunner (if present on
    /// Moon 1 definition) and otherwise paces beats by player progression markers.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1MagneticArc : MonoBehaviour
    {
        public static Moon1MagneticArc Instance { get; private set; }

        public enum Beat { Discovery = 0, Restoration = 1, Conflict = 2, Climax = 3, Revelation = 4 }

        [Header("Pacing")]
        [Tooltip("Seconds after scene load before Discovery beat fires.")]
        public float startDelay = 4f;
        [Tooltip("Min seconds spent in each beat before promotion is allowed.")]
        public float minBeatTime = 6f;

        Beat _current = Beat.Discovery;
        bool _running;
        readonly HashSet<Beat> _completed = new();
        readonly List<GameObject> _spawnedThisRun = new();

        // Crossover seed tags (saved to PlayerPrefs as "moon1_seed_<tag>" for cross-moon checks)
        public const string SEED_SPIRE_FRAGMENT  = "spire_fragment";   // → Moon 5 White City spire
        public const string SEED_GIANT_KEY_1     = "giant_key_1";      // → 8-key collection for DLC giant arc
        public const string SEED_AIRSHIP_FRAG    = "airship_fragment"; // → Moon 8 armada
        public const string SEED_LIRAEL_LULLABY  = "lirael_lullaby";   // → Moon 3 + Moon 6 unlocks

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            // Only auto-bootstrap in the Echohaven hub scene.
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!active.IsValid()) return;
            if (!active.name.StartsWith("Echohaven", System.StringComparison.OrdinalIgnoreCase)) return;
            if (Instance != null) return;

            var go = new GameObject("Moon1MagneticArc");
            Instance = go.AddComponent<Moon1MagneticArc>();
            Debug.Log("[Moon1Arc] Bootstrapped in Echohaven hub.");
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            MoonBeatRunner.OnBeatStarted += HandleBeatRunner;
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
        }

        void OnDisable()
        {
            MoonBeatRunner.OnBeatStarted -= HandleBeatRunner;
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
        }

        void Start()
        {
            StartCoroutine(RunArc());
        }

        void HandleBeatRunner(int moon, MoonBeatRunner.Beat b)
        {
            if (moon != 1) return;
            // Sync our internal beat to the runner if it drives us.
            var ours = (Beat)(int)b;
            if ((int)ours > (int)_current) _current = ours;
        }

        void HandleBuildingRestored(string id)
        {
            // The 3 hub restorations during Beat 2 are tracked via EchohavenProgressionSystem.
            // We just listen for the capstone to know Restoration may end early.
            if (string.IsNullOrEmpty(id)) return;
            if (id.ToLowerInvariant().Contains("spire") || id.ToLowerInvariant().Contains("dome") || id.ToLowerInvariant().Contains("fountain"))
            {
                if (_current == Beat.Restoration)
                    Debug.Log($"[Moon1Arc] Hub building restored mid-Restoration beat: {id}");
            }
        }

        IEnumerator RunArc()
        {
            if (_running) yield break;
            _running = true;

            yield return new WaitForSeconds(startDelay);

            // Beat 1
            yield return Beat1_Discovery();
            _completed.Add(Beat.Discovery);
            MoonProgressTracker.Instance?.MarkBeatCleared(1, 0);

            // Beat 2
            yield return Beat2_Restoration();
            _completed.Add(Beat.Restoration);
            MoonProgressTracker.Instance?.MarkBeatCleared(1, 1);

            // Beat 3
            yield return Beat3_Conflict();
            _completed.Add(Beat.Conflict);
            MoonProgressTracker.Instance?.MarkBeatCleared(1, 2);

            // Beat 4
            yield return Beat4_Climax();
            _completed.Add(Beat.Climax);
            MoonProgressTracker.Instance?.MarkBeatCleared(1, 3);

            // Beat 5
            yield return Beat5_Revelation();
            _completed.Add(Beat.Revelation);
            MoonProgressTracker.Instance?.MarkBeatCleared(1, 4);

            // Full clear → mark Moon 1 cleared (unlocks Moon 2 portal via MoonProgressTracker).
            MoonProgressTracker.Instance?.MarkCleared(1);
            GameEvents.FireCriticalSaveTrigger("moon1_arc_complete");
            GameEvents.RaiseHUDShowBanner("MOON 1 COMPLETE", "The Magnetic Moon awakens. Moon 2 — Lunar — beckons. [F2] to travel.", 8f);
            Debug.Log("[Moon1Arc] Magnetic Moon arc COMPLETE — Moon 2 unlocked.");

            _running = false;
        }

        // ───────────────────────────────────────────────────────────────
        // BEAT 1 — Discovery (Days 1–5)
        //   "First resonance scan reveals buried cathedral. Swipe-excavation tutorial."
        // ───────────────────────────────────────────────────────────────
        IEnumerator Beat1_Discovery()
        {
            _current = Beat.Discovery;
            Debug.Log("[Moon1Arc] Beat 1 — Discovery");

            GameEvents.RaiseHUDShowBanner(
                "MOON 1 — DISCOVERY",
                "The Magnetic Moon stirs. Follow Milo. Use your Resonance Scanner.",
                6f);

            // Tutorial prompt: scanner
            yield return new WaitForSeconds(3f);
            GameEvents.RaiseHUDShowObjective($"Hold {InputPromptHelper.Scan} to scan for buried Aether structures.");

            // Spawn the first dig site (north of player spawn) — tagged tutorial
            Vector3 playerPos = SafePlayerPos();
            Vector3 digPos = playerPos + new Vector3(0f, 0f, 18f);
            var dig = SpawnDigBeacon(digPos, "First Mud Brick Site");
            _spawnedThisRun.Add(dig);

            // Give the player time to read prompts + walk toward the dig site
            float t0 = Time.time;
            yield return new WaitForSeconds(8f);

            GameEvents.RaiseHUDShowObjective($"Approach the glowing dig site. Press {InputPromptHelper.Interact} to excavate.");
            AudioManager.Instance?.PlaySFX2D("Discovery");

            // Hold here a few seconds so the player has time to read + walk
            yield return new WaitForSeconds(Mathf.Max(minBeatTime - (Time.time - t0), 4f));
        }

        // ───────────────────────────────────────────────────────────────
        // BEAT 2 — Restoration (Days 6–12)
        //   "First tuning mini-game. Place first spire on dome."
        // ───────────────────────────────────────────────────────────────
        IEnumerator Beat2_Restoration()
        {
            _current = Beat.Restoration;
            Debug.Log("[Moon1Arc] Beat 2 — Restoration");

            GameEvents.RaiseHUDShowBanner(
                "RESTORATION",
                "Tune the dome. Wake the fountain. Plant the spire. Three blessings await.",
                6f);

            // Drop 2 lore fragments in the hub area
            Vector3 p = SafePlayerPos();
            _spawnedThisRun.Add(Moon1LoreFragment.Spawn(
                p + new Vector3(8f, 0f, 6f),
                "Carved Stone — 3·6·9",
                "Tesla's seal. The trinity of dimension, vibration, and frequency. Older than Victorian — older than memory."));
            _spawnedThisRun.Add(Moon1LoreFragment.Spawn(
                p + new Vector3(-7f, 0f, 9f),
                "Carved Stone — Golden Spiral",
                "The φ-ratio etched in pre-Flood granite. Cassian dismisses it as decorative. Milo whispers: 'This stone hums when I lick it.'"));

            GameEvents.RaiseHUDShowObjective(
                $"Restore the 3 hub buildings (Fountain, Dome, Spire). Use {InputPromptHelper.Interact} on each.");

            // Wait until EchohavenProgressionSystem reports full hub OR cap at 60s for solo testing
            float t0 = Time.time;
            while (Time.time - t0 < 60f)
            {
                var prog = EchohavenProgressionSystem.Instance;
                if (prog != null && prog.IsHubFullyRestored())
                {
                    GameEvents.RaiseHUDShowBanner(
                        "HUB AWAKENED",
                        "Blue-white sparks climb the spire. The first ley-line vein lights up — a golden thread points somewhere vast.",
                        5f);
                    AudioManager.Instance?.PlaySFX2D("BuildingRestore");
                    break;
                }
                yield return new WaitForSeconds(1f);
            }

            yield return new WaitForSeconds(Mathf.Max(minBeatTime - (Time.time - t0), 2f));
        }

        // ───────────────────────────────────────────────────────────────
        // BEAT 3 — Conflict (Days 13–18)
        //   "Reset scouts arrive. First giant-mode burst."
        // ───────────────────────────────────────────────────────────────
        IEnumerator Beat3_Conflict()
        {
            _current = Beat.Conflict;
            Debug.Log("[Moon1Arc] Beat 3 — Conflict");

            GameEvents.RaiseHUDShowBanner(
                "RESET SCOUTS INBOUND",
                $"Victorian-costumed goons with jackhammers. Defend the hub. {InputPromptHelper.Strike} to strike.",
                6f);

            // Spawn a ring of Reset Scouts around the hub
            Vector3 center = SafePlayerPos();
            const int scoutCount = 4;
            var scouts = new List<ResetScoutEnemy>();
            for (int i = 0; i < scoutCount; i++)
            {
                float ang = (i / (float)scoutCount) * Mathf.PI * 2f;
                Vector3 spawn = center + new Vector3(Mathf.Cos(ang) * 12f, 0f, Mathf.Sin(ang) * 12f);
                var s = ResetScoutEnemy.Spawn(spawn, center);
                scouts.Add(s);
                _spawnedThisRun.Add(s.gameObject);
            }

            // Watch — when ~half down, prompt giant-mode burst
            GameEvents.RaiseHUDShowObjective($"Defeat the Reset Scouts (0 / {scoutCount}).");
            int lastReported = -1;
            while (true)
            {
                int alive = 0;
                foreach (var s in scouts) if (s != null && !s.IsDead) alive++;
                int dead = scoutCount - alive;
                if (dead != lastReported)
                {
                    lastReported = dead;
                    GameEvents.RaiseHUDShowObjective($"Defeat the Reset Scouts ({dead} / {scoutCount}).");
                    if (dead == scoutCount / 2)
                    {
                        GameEvents.RaiseHUDShowBanner(
                            "GIANT MODE — READY",
                            "Your bloodline pulses. Activate Giant Mode and toss them into the mud pit.",
                            5f);
                        TryActivateGiantMode();
                    }
                }
                if (alive == 0) break;
                yield return new WaitForSeconds(0.5f);
            }

            GameEvents.RaiseHUDShowBanner("WAVE CLEARED", "The mud accepts them. Their clipboards float away.", 4f);
            AudioManager.Instance?.PlaySFX2D("BuildingRestore");
            yield return new WaitForSeconds(Mathf.Max(minBeatTime - 2f, 2f));
        }

        // ───────────────────────────────────────────────────────────────
        // BEAT 4 — Climax (Days 19–24)
        //   "Buried Beacon: mercury-ball spire clutched in a giant's skeletal hand.
        //    Tune it during a 17th-hour alignment → cathedral erupts with light."
        // ───────────────────────────────────────────────────────────────
        IEnumerator Beat4_Climax()
        {
            _current = Beat.Climax;
            Debug.Log("[Moon1Arc] Beat 4 — Climax");

            GameEvents.RaiseHUDShowBanner(
                "THE BURIED BEACON",
                "A giant's skeletal hand clutches a mercury-ball spire. Approach and tune it during the 17th-hour alignment.",
                7f);

            // Spawn the Buried Beacon in front of the player
            Vector3 pos = SafePlayerPos() + SafePlayerForward() * 22f;
            var beacon = BuriedBeaconSpire.Spawn(pos);
            _spawnedThisRun.Add(beacon.gameObject);

            GameEvents.RaiseHUDShowObjective($"Find the Buried Beacon. {InputPromptHelper.Interact} to tune the mercury spire.");

            // Wait for the beacon to be tuned (TuneCompleted) or 90s cap
            float t0 = Time.time;
            while (Time.time - t0 < 90f && beacon != null && !beacon.IsTuned)
                yield return new WaitForSeconds(0.5f);

            // Force completion if timed out so the arc still progresses
            if (beacon != null && !beacon.IsTuned)
                beacon.ForceTune();

            // Spectacle
            GameEvents.RaiseHUDShowBanner(
                "CATHEDRAL ERUPTS",
                "Light spreads outward. Distant spires you cannot yet reach flicker awake on the horizon.",
                6f);
            AetherFieldManager.Instance?.AddResonanceScore(20f);
            AudioManager.Instance?.PlaySFX2D("BuildingRestore");
            yield return new WaitForSeconds(Mathf.Max(minBeatTime, 3f));
        }

        // ───────────────────────────────────────────────────────────────
        // BEAT 5 — Revelation (Days 25–28)
        //   "Lirael appears — translucent, humming a lullaby in 432 Hz."
        // ───────────────────────────────────────────────────────────────
        IEnumerator Beat5_Revelation()
        {
            _current = Beat.Revelation;
            Debug.Log("[Moon1Arc] Beat 5 — Revelation");

            // Spawn the lullaby encounter near the Buried Beacon area
            Vector3 pos = SafePlayerPos() + SafePlayerForward() * 6f + Vector3.up * 0.5f;
            var encounter = LiraelLullabyEncounter.Spawn(pos);
            _spawnedThisRun.Add(encounter.gameObject);

            GameEvents.RaiseHUDShowBanner(
                "LIRAEL APPEARS",
                "Translucent. Humming. 432 Hz. She doesn't remember her name — only the song.",
                7f);

            // Wait for the encounter to play out (~12s) or skip
            yield return encounter.PlayCoroutine();

            // Drop the 4 crossover seeds at the player's feet (collectible items)
            Vector3 dropBase = SafePlayerPos() + Vector3.up * 0.4f;
            _spawnedThisRun.Add(Moon1Collectible.Spawn(dropBase + new Vector3( 1.2f, 0f,  0f), SEED_SPIRE_FRAGMENT,  "Spire Fragment",  "Blooms in the White City (Moon 5)."));
            _spawnedThisRun.Add(Moon1Collectible.Spawn(dropBase + new Vector3(-1.2f, 0f,  0f), SEED_GIANT_KEY_1,     "Giant Skeleton Key #1", "1 of 8. Collect them all to unlock the giant arc."));
            _spawnedThisRun.Add(Moon1Collectible.Spawn(dropBase + new Vector3( 0f,   0f,  1.2f), SEED_AIRSHIP_FRAG,    "Airship Component Fragment", "Blooms in the Airship Armada (Moon 8)."));
            _spawnedThisRun.Add(Moon1Collectible.Spawn(dropBase + new Vector3( 0f,   0f, -1.2f), SEED_LIRAEL_LULLABY,  "Lirael's Lullaby",  "Key to Moon 3 and Moon 6."));

            GameEvents.RaiseHUDShowObjective($"Collect the 4 crossover seeds Lirael left behind. {InputPromptHelper.Interact} on each.");

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
            // GiantModeController is reflection-friendly — call Activate() if present, else send message.
            var mi = gm.GetType().GetMethod("Activate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (mi != null) mi.Invoke(gm, null);
            else gm.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
        }

        GameObject SpawnDigBeacon(Vector3 pos, string label)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Moon1_DigSite_" + label.Replace(' ', '_');
            go.transform.position = pos + Vector3.up * 0.2f;
            go.transform.localScale = new Vector3(1.8f, 0.4f, 1.8f);
            // Strip collider physics — make it a soft trigger
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = new Color(0.95f, 0.75f, 0.25f, 1f);
                mat.SetColor("_EmissionColor", new Color(1.0f, 0.6f, 0.1f) * 2.5f);
                mat.EnableKeyword("_EMISSION");
                mr.material = mat;
            }
            // Soft upward light
            var l = new GameObject("BeaconLight").AddComponent<Light>();
            l.transform.SetParent(go.transform, false);
            l.transform.localPosition = Vector3.up * 1.5f;
            l.type = LightType.Point;
            l.range = 8f;
            l.intensity = 4f;
            l.color = new Color(1f, 0.8f, 0.4f);
            return go;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Reset Scout Enemy — Victorian goon with jackhammer (tutorial combat)
    // ═══════════════════════════════════════════════════════════════════
    public class ResetScoutEnemy : MonoBehaviour
    {
        public float maxHealth = 35f;
        public float moveSpeed = 2.2f;
        public float attackRange = 2.0f;
        public float attackDamage = 4f;
        public float attackCooldown = 1.8f;

        Transform _target;
        float _hp;
        float _nextAttack;
        Renderer[] _rends;
        Color _origColor;
        bool _dead;

        public bool IsDead => _dead;

        public static ResetScoutEnemy Spawn(Vector3 pos, Vector3 lookAt)
        {
            var root = new GameObject("ResetScout");
            root.transform.position = pos;
            root.transform.rotation = Quaternion.LookRotation((lookAt - pos).normalized);

            // Body (drab Victorian tweed)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
            var bmat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            bmat.color = new Color(0.32f, 0.22f, 0.16f);
            body.GetComponent<MeshRenderer>().material = bmat;
            Object.Destroy(body.GetComponent<Collider>());

            // Head (with top hat suggestion)
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.95f, 0f);
            head.transform.localScale = Vector3.one * 0.5f;
            head.GetComponent<MeshRenderer>().material = bmat;
            Object.Destroy(head.GetComponent<Collider>());

            var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hat.name = "TopHat";
            hat.transform.SetParent(root.transform, false);
            hat.transform.localPosition = new Vector3(0f, 2.3f, 0f);
            hat.transform.localScale = new Vector3(0.45f, 0.25f, 0.45f);
            var hatMat = new Material(bmat);
            hatMat.color = new Color(0.08f, 0.08f, 0.08f);
            hat.GetComponent<MeshRenderer>().material = hatMat;
            Object.Destroy(hat.GetComponent<Collider>());

            // Jackhammer
            var jh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            jh.name = "Jackhammer";
            jh.transform.SetParent(root.transform, false);
            jh.transform.localPosition = new Vector3(0.6f, 0.9f, 0.4f);
            jh.transform.localScale = new Vector3(0.15f, 1.6f, 0.15f);
            var jhMat = new Material(bmat);
            jhMat.color = new Color(0.5f, 0.5f, 0.55f);
            jh.GetComponent<MeshRenderer>().material = jhMat;
            Object.Destroy(jh.GetComponent<Collider>());

            // Hit collider (capsule on root)
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 1f, 0f);
            capsule.radius = 0.55f;
            capsule.height = 2.2f;
            capsule.isTrigger = false;

            var rb = root.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var s = root.AddComponent<ResetScoutEnemy>();
            s._target = GameObject.FindWithTag("Player")?.transform;
            return s;
        }

        void Start()
        {
            _hp = maxHealth;
            _rends = GetComponentsInChildren<Renderer>();
            if (_rends != null && _rends.Length > 0 && _rends[0].material != null)
                _origColor = _rends[0].material.color;
        }

        void Update()
        {
            if (_dead) return;
            if (_target == null)
            {
                _target = GameObject.FindWithTag("Player")?.transform;
                return;
            }

            float dist = Vector3.Distance(transform.position, _target.position);
            Vector3 dir = (_target.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), Time.deltaTime * 4f);

            if (dist > attackRange)
            {
                Vector3 step = dir.normalized * moveSpeed * Time.deltaTime;
                transform.position += new Vector3(step.x, 0f, step.z);
            }
            else if (Time.time >= _nextAttack)
            {
                _nextAttack = Time.time + attackCooldown;
                // Lightweight damage: try Tartaria.Gameplay.PlayerHealth via reflection
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
            if (_dead) return;
            _hp -= amt;
            StartCoroutine(FlashRed());
            if (_hp <= 0f) Die();
        }

        IEnumerator FlashRed()
        {
            if (_rends == null) yield break;
            foreach (var r in _rends) if (r != null && r.material != null) r.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            foreach (var r in _rends) if (r != null && r.material != null) r.material.color = _origColor;
        }

        void Die()
        {
            _dead = true;
            // Disable collisions + AI
            var col = GetComponent<Collider>(); if (col != null) col.enabled = false;
            var rb = GetComponent<Rigidbody>(); if (rb != null) rb.isKinematic = true;
            // Tumble into mud
            StartCoroutine(SinkAndDestroy());
            AudioManager.Instance?.PlaySFX2D("GolemDeath");
        }

        IEnumerator SinkAndDestroy()
        {
            float t = 0f;
            Vector3 start = transform.position;
            while (t < 2f)
            {
                t += Time.deltaTime;
                transform.position = start + Vector3.down * (t * 0.6f);
                transform.Rotate(0f, 0f, 60f * Time.deltaTime, Space.Self);
                yield return null;
            }
            Destroy(gameObject);
        }

        // Player melee hook — Tartaria.Gameplay.HarmonicStaff (or similar) should call this via SendMessage on hit.
        void OnHarmonicHit(float dmg) => TakeDamage(dmg);
    }

    // ═══════════════════════════════════════════════════════════════════
    // Buried Beacon — Moon 1 climax interactable
    // ═══════════════════════════════════════════════════════════════════
    public class BuriedBeaconSpire : MonoBehaviour, IInteractable
    {
        public bool IsTuned { get; private set; }
        Light _glow;
        GameObject _orb;

        public static BuriedBeaconSpire Spawn(Vector3 pos)
        {
            var root = new GameObject("BuriedBeacon_MercurySpire");
            root.transform.position = pos;

            // Skeletal "hand" base — 5 fingers + palm (cubes)
            var palm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            palm.name = "Palm";
            palm.transform.SetParent(root.transform, false);
            palm.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            palm.transform.localScale = new Vector3(2.5f, 0.4f, 2.5f);
            var boneMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            boneMat.color = new Color(0.92f, 0.88f, 0.78f);
            palm.GetComponent<MeshRenderer>().material = boneMat;
            Object.Destroy(palm.GetComponent<Collider>());
            for (int i = 0; i < 5; i++)
            {
                float ang = (i / 5f) * Mathf.PI * 2f;
                var finger = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                finger.name = $"Finger_{i}";
                finger.transform.SetParent(root.transform, false);
                finger.transform.localPosition = new Vector3(Mathf.Cos(ang) * 1.2f, 1.2f, Mathf.Sin(ang) * 1.2f);
                finger.transform.localScale = new Vector3(0.18f, 1.4f, 0.18f);
                finger.transform.localRotation = Quaternion.Euler(0f, 0f, ang * Mathf.Rad2Deg * 0.15f);
                finger.GetComponent<MeshRenderer>().material = boneMat;
                Object.Destroy(finger.GetComponent<Collider>());
            }

            // Spire shaft
            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "SpireShaft";
            shaft.transform.SetParent(root.transform, false);
            shaft.transform.localPosition = new Vector3(0f, 3.2f, 0f);
            shaft.transform.localScale = new Vector3(0.25f, 1.8f, 0.25f);
            var shaftMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            shaftMat.color = new Color(0.78f, 0.6f, 0.18f);
            shaftMat.SetFloat("_Metallic", 0.7f);
            shaftMat.SetFloat("_Smoothness", 0.6f);
            shaft.GetComponent<MeshRenderer>().material = shaftMat;
            Object.Destroy(shaft.GetComponent<Collider>());

            // Mercury orb
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "MercuryOrb";
            orb.transform.SetParent(root.transform, false);
            orb.transform.localPosition = new Vector3(0f, 5.4f, 0f);
            orb.transform.localScale = Vector3.one * 1.0f;
            var orbMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            orbMat.color = new Color(0.85f, 0.85f, 0.95f);
            orbMat.SetFloat("_Metallic", 1f);
            orbMat.SetFloat("_Smoothness", 0.95f);
            orbMat.SetColor("_EmissionColor", new Color(0.6f, 0.7f, 1.0f) * 1.5f);
            orbMat.EnableKeyword("_EMISSION");
            orb.GetComponent<MeshRenderer>().material = orbMat;
            Object.Destroy(orb.GetComponent<Collider>());

            // Trigger interaction collider on root
            var trig = root.AddComponent<SphereCollider>();
            trig.isTrigger = true;
            trig.center = new Vector3(0f, 2.5f, 0f);
            trig.radius = 3.5f;

            var glowLight = new GameObject("BeaconGlow").AddComponent<Light>();
            glowLight.transform.SetParent(root.transform, false);
            glowLight.transform.localPosition = new Vector3(0f, 5.4f, 0f);
            glowLight.type = LightType.Point;
            glowLight.color = new Color(0.7f, 0.85f, 1f);
            glowLight.range = 14f;
            glowLight.intensity = 3.5f;

            var bb = root.AddComponent<BuriedBeaconSpire>();
            bb._glow = glowLight;
            bb._orb = orb;
            return bb;
        }

        public string GetInteractPrompt() =>
            IsTuned
                ? "The beacon hums in harmony."
                : $"{InputPromptHelper.Interact} Tune the Mercury Spire (17th-hour alignment)";

        public void Interact(GameObject player)
        {
            if (IsTuned) return;
            StartCoroutine(TuneSequence());
        }

        IEnumerator TuneSequence()
        {
            GameEvents.RaiseHUDShowBanner(
                "17TH HOUR ALIGNMENT",
                "The spire begins to sing. Hold position. Let the resonance climb.",
                4f);
            AudioManager.Instance?.PlaySFX2D("HarmonicChoir");

            // Pulsing brightness for 6 seconds
            float t = 0f;
            float baseRange = _glow != null ? _glow.range : 14f;
            while (t < 6f)
            {
                t += Time.deltaTime;
                float pulse = 1f + Mathf.Sin(t * 6f) * 0.3f;
                if (_glow != null) { _glow.range = baseRange * pulse; _glow.intensity = 3.5f * pulse; }
                if (_orb != null) _orb.transform.localScale = Vector3.one * (1f + Mathf.Sin(t * 5f) * 0.1f);
                yield return null;
            }

            IsTuned = true;
            if (_glow != null) { _glow.range = 24f; _glow.intensity = 6f; _glow.color = new Color(1f, 0.95f, 0.6f); }
            GameEvents.FireBuildingRestored("buried_beacon");
            GameEvents.RaiseHUDShowBanner("BEACON ALIGNED", "The ley lines spread outward. Distant moons flicker.", 5f);
        }

        public void ForceTune()
        {
            if (IsTuned) return;
            IsTuned = true;
            if (_glow != null) { _glow.range = 24f; _glow.intensity = 6f; _glow.color = new Color(1f, 0.95f, 0.6f); }
            GameEvents.FireBuildingRestored("buried_beacon");
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Moon 1 Lore Fragment — readable carved stone
    // ═══════════════════════════════════════════════════════════════════
    public class Moon1LoreFragment : MonoBehaviour, IInteractable
    {
        public string title;
        [TextArea(2, 5)] public string body;
        bool _read;

        public static GameObject Spawn(Vector3 pos, string title, string body)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Moon1Lore_" + title.Replace(' ', '_');
            go.transform.position = pos + Vector3.up * 0.4f;
            go.transform.localScale = new Vector3(1.2f, 0.8f, 0.25f);
            go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(0.55f, 0.5f, 0.45f);
            mat.SetColor("_EmissionColor", new Color(0.4f, 0.35f, 0.15f) * 0.8f);
            mat.EnableKeyword("_EMISSION");
            go.GetComponent<MeshRenderer>().material = mat;

            // Replace solid collider with trigger so player walks through
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            var trig = go.AddComponent<BoxCollider>();
            trig.isTrigger = true;
            trig.size = new Vector3(2.2f, 2f, 2.2f);

            var f = go.AddComponent<Moon1LoreFragment>();
            f.title = title;
            f.body = body;
            return go;
        }

        public string GetInteractPrompt() =>
            _read ? $"{title} (read)" : $"{InputPromptHelper.Interact} Read: {title}";

        public void Interact(GameObject player)
        {
            _read = true;
            GameEvents.RaiseHUDShowBanner(title, body, 7f);
            AetherFieldManager.Instance?.AddResonanceScore(2f);
            AudioManager.Instance?.PlaySFX2D("Discovery");
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Moon 1 Collectible — crossover seed (saves to PlayerPrefs)
    // ═══════════════════════════════════════════════════════════════════
    public class Moon1Collectible : MonoBehaviour, IInteractable
    {
        public string seedTag;
        public string displayName;
        public string flavor;

        public static GameObject Spawn(Vector3 pos, string seedTag, string displayName, string flavor)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Moon1Seed_" + seedTag;
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.55f;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            // Color-code by seed
            Color c = seedTag switch
            {
                Moon1MagneticArc.SEED_SPIRE_FRAGMENT  => new Color(0.5f, 0.9f, 1.0f),
                Moon1MagneticArc.SEED_GIANT_KEY_1     => new Color(1.0f, 0.85f, 0.4f),
                Moon1MagneticArc.SEED_AIRSHIP_FRAG    => new Color(0.85f, 0.6f, 0.95f),
                Moon1MagneticArc.SEED_LIRAEL_LULLABY  => new Color(0.95f, 0.95f, 0.95f),
                _                                      => Color.white,
            };
            mat.color = c;
            mat.SetColor("_EmissionColor", c * 2.0f);
            mat.EnableKeyword("_EMISSION");
            go.GetComponent<MeshRenderer>().material = mat;

            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            // Floaty bob
            go.AddComponent<Moon1CollectibleBobber>();

            var c2 = go.AddComponent<Moon1Collectible>();
            c2.seedTag = seedTag;
            c2.displayName = displayName;
            c2.flavor = flavor;
            return go;
        }

        public string GetInteractPrompt() => $"{InputPromptHelper.Interact} Pick up: {displayName}";

        public void Interact(GameObject player)
        {
            PlayerPrefs.SetInt("moon1_seed_" + seedTag, 1);
            PlayerPrefs.Save();
            GameEvents.RaiseHUDShowBanner(displayName + " — Collected", flavor, 5f);
            AudioManager.Instance?.PlaySFX2D("BuildingRestore");
            AetherFieldManager.Instance?.AddResonanceScore(5f);
            GameEvents.FireCriticalSaveTrigger("moon1_seed_" + seedTag);
            Destroy(gameObject);
        }

        public static bool HasSeed(string tag) => PlayerPrefs.GetInt("moon1_seed_" + tag, 0) == 1;
    }

    public class Moon1CollectibleBobber : MonoBehaviour
    {
        Vector3 _base;
        float _phase;
        void Start() { _base = transform.position; _phase = Random.Range(0f, Mathf.PI * 2f); }
        void Update()
        {
            transform.position = _base + Vector3.up * (Mathf.Sin(Time.time * 2f + _phase) * 0.2f);
            transform.Rotate(0f, 60f * Time.deltaTime, 0f, Space.World);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Lirael Lullaby Encounter — Moon 1 Revelation set-piece
    // ═══════════════════════════════════════════════════════════════════
    public class LiraelLullabyEncounter : MonoBehaviour
    {
        Light _aura;
        GameObject _body;

        public static LiraelLullabyEncounter Spawn(Vector3 pos)
        {
            var root = new GameObject("LiraelLullabyEncounter");
            root.transform.position = pos;

            // Translucent figure
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "LiraelGhost";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
            Object.Destroy(body.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(0.8f, 0.95f, 1f, 0.35f);
            mat.SetColor("_EmissionColor", new Color(0.5f, 0.8f, 1f) * 1.5f);
            mat.EnableKeyword("_EMISSION");
            // Try alpha — URP Lit supports Surface=Transparent
            mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            body.GetComponent<MeshRenderer>().material = mat;

            var aura = new GameObject("Aura").AddComponent<Light>();
            aura.transform.SetParent(root.transform, false);
            aura.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            aura.type = LightType.Point;
            aura.color = new Color(0.7f, 0.9f, 1f);
            aura.range = 10f;
            aura.intensity = 3f;

            var e = root.AddComponent<LiraelLullabyEncounter>();
            e._aura = aura;
            e._body = body;
            return e;
        }

        public IEnumerator PlayCoroutine()
        {
            // Slow rise + lullaby tones (we don't have a real 432Hz synth — play a SFX hint instead)
            AudioManager.Instance?.PlaySFX2D("HarmonicChoir");
            yield return new WaitForSeconds(2f);
            GameEvents.RaiseHUDShowBanner("Lirael (humming)", "...la la la la... 432 Hz... do you remember?", 5f);
            yield return new WaitForSeconds(5f);
            GameEvents.RaiseHUDShowBanner("Lirael", "Why do grown-ups build houses then live in the attic?", 5f);
            yield return new WaitForSeconds(5f);
            // Pulse aura
            float t = 0f;
            while (t < 3f)
            {
                t += Time.deltaTime;
                if (_aura != null) _aura.intensity = 3f + Mathf.Sin(t * 4f) * 1.5f;
                if (_body != null) _body.transform.localPosition = new Vector3(0f, 1f + Mathf.Sin(t * 2f) * 0.15f, 0f);
                yield return null;
            }
            GameEvents.RaiseHUDShowBanner("Deep Lore", "A figure in shadow stands atop a star fort. The Dissonant One reaches for something in the sky.", 6f);
            yield return new WaitForSeconds(4f);
            // Lirael fades — we leave the GO in place as a quiet ambient memorial
        }
    }
}
