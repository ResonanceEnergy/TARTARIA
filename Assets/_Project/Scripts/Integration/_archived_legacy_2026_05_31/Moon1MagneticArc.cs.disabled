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
{
#pragma warning disable CS0414 // Placeholder counts for planned features
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
}
