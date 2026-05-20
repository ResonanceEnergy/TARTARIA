using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;
using Tartaria.UI;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Gameplay; // for EnemyType / EnemySpawnTrigger access in Moon 3 DOTS spawns

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Rail Escort Mechanics — Moon 3 (Windswept Highlands) — Phase 3 R7 Production Completeness.
    /// 
    /// Builds directly on strong R6 foundation (7-min 7-wave playable climax, live freq dynamic difficulty, full Lirael/Milo/Cassian physical tells/trust, mid-escort adoption, Highland Watchtower+Wind Bridge restoration loops, Dissonance Leviathan vuln+escort protection+permanent golden rails/GiantEcho/calmed winds, 17th Hour + World's Fair ticket, DOTS proxy pooling + perf).
    /// 
    /// R7 depth layer (per 03C Moon 3 Rails/Compassion, 11_SCRIPTED_CLIMAXES full Orphan Train Escort phases+children comfort, 20_QUEST M3-MS06, 10_ROADMAP Phase 3 Moon3, GDD vertical slice to production):
    /// - Extended rail network: 3+ additional stations/branch points (Highland Depot, Windspire Junction branch choice, Leviathan Canyon Terminal) with restoration/tuning/combat hooks + post-escort Continental Rail fast travel unlock hook.
    /// - Dedicated non-OnGUI lullaby/escort HUD (Moon3EscortHUD Canvas: progress/shield/freq/companion status/wave timer) — existing OnGUI kept for quick testing.
    /// - Deepened Leviathan: 4-phase boss (Approach/TailSweep/SonicScream/CrystalBarrage + orphan lullaby synergy scaling), stronger permanent world VFX on victory.
    /// - Expanded companion reactivity: more physical tells + trust forks (freq success favors Lirael singer, protection focus favors Milo guard) during full escort.
    /// - Additional calendar/live-ops: more 17th Hour variants, World's Fair ticket variants, new daily rail success deals wired to SpectralOrphanAdoption + escort events.
    /// - Performance + DOTS polish on expanded rail: expanded proxy pooling (wraith/harvester/levi/station), improved wind proxy management, static batching on all new station/rail content.
    /// - Optional fast travel / Continental Rail hooks post-escort success.
    /// 
    /// Exclusive Moon 3 domain (rail, escort, buildings, Leviathan, orphans, wind, calendar). Zero other moons, zero core save, zero general UI.
    /// </summary>
    public class RailEscortController : MonoBehaviour
    {
        [Header("Escort Config — R6 7min + R7 Extended")]
        [SerializeField] float escortDuration = 420f; // 7 minutes core vertical slice
        [SerializeField] float trainSpeed = 3.8f;
        [SerializeField] int maxWraithSpawnsPerWave = 6;
        [SerializeField] float baseWaveInterval = 52f; // tuned for pacing + freq dynamic

        [Header("Path (R6 linear + R7 extended stations & branch points)")]
        [SerializeField] Vector3 railStart = new Vector3(20, 6, -10);
        [SerializeField] Vector3 railEnd = new Vector3(140, 6, 55);

        // R7: Extended rail network stations / branch points (restoration/tuning/combat hooks)
        readonly (string name, float progress, string hook)[] _railStations = new[]
        {
            ("HighlandDepot_Station", 0.25f, "restore_tuning_buff"),
            ("WindspireJunction_Branch", 0.48f, "branch_choice_combat"),
            ("LeviathanCanyonTerminal", 0.78f, "levi_vuln_anchor"),
            ("ContinentalRail_Hub", 1.0f, "fast_travel_unlock")
        };

        [Header("Difficulty & Protection (R6+R7)")]
        [SerializeField] float railWraithHealth = 95f;
        [SerializeField] float harvesterDrain = 7f;
        [SerializeField] float trainMaxHealth = 280f; // escort protection fantasy
        float _trainHealth;

        // Runtime state (R5 preserved + R6 depth + R7 extensions)
        GameObject _trainProxy;
        float _progress; // 0-1 along rail
        float _time;
        bool _active;
        int _waveIndex;
        List<GameObject> _activeThreats = new();
        float _lastWaveTime;
        float _lullabyShieldStrength = 1f; // from adopted children + freq play
        float _currentTargetLullabyHz = 432f;

        // R6 dynamic freq + 17th + world change
        bool _seventeenthHourActive;
        bool _leviathanPhaseActive;
        float _nextVulnWindowTime;
        float _vulnWindowEnd;
        bool _permanentWorldChanged;
        float _lastFreqMatch;

        // R7: Leviathan phase state (0=approach,1=tail,2=scream,3=barrage,4=purify)
        int _leviathanPhase;
        float _lastLeviPhaseChange;

        // R7: Branch choice state for extended network
        int _currentBranchChoice = -1; // -1 none, 0=short combat, 1=safe tuned

        // R7: Fast travel hook (post-escort Continental Rail ready)
        public static bool Moon3ContinentalRailFastTravelUnlocked { get; private set; }

        // R6 simple proxy pool for perf (no GC spam on waves) + R7 expanded
        readonly Queue<GameObject> _wraithProxyPool = new Queue<GameObject>();
        readonly Queue<GameObject> _harvesterProxyPool = new Queue<GameObject>();
        readonly Queue<GameObject> _stationProxyPool = new Queue<GameObject>(); // R7 station proxies

        // R7 dedicated HUD (non-OnGUI)
        Moon3EscortHUD _escortHUD;

        public bool IsActive => _active;
        public float Progress => _progress;
        public float TrainHealthNormalized => _trainHealth / trainMaxHealth;

        // R7 public accessors for dedicated HUD + external hooks (Moon3 only)
        public float GetEscortTime() => _time;
        public float LastFreqMatch => _lastFreqMatch;
        public int CurrentWave => _waveIndex;
        public int ActiveThreatCount => _activeThreats.Count;
        public float CurrentTargetHz => _currentTargetLullabyHz;
        public bool IsSeventeenthHourActive => _seventeenthHourActive;
        public bool IsLeviathanPhaseActive => _leviathanPhaseActive;
        public bool IsPermanentWorldChanged => _permanentWorldChanged;
        public float LullabyShieldStrength => _lullabyShieldStrength;
        public int LeviathanPhase => _leviathanPhase;

        public event System.Action<bool> OnEscortComplete; // success/fail
        public event System.Action<int> OnWaveStarted;
        public event System.Action OnSeventeenthHourTriggered;
        public event System.Action OnLeviathanPurified;

        /// <summary>
        /// R5 synergy preserved + R6/R7 extended: good frequency puzzle matches during escort directly empower lullaby + damage threats + open levi vuln. R7: branch & station hooks.
        /// </summary>
        public void ApplyRailBossSynergy(float matchQuality)
        {
            if (!_active) return;
            float boost = Mathf.Clamp01(matchQuality) * 0.65f + 0.22f;
            _lullabyShieldStrength = Mathf.Min(3.8f, _lullabyShieldStrength + boost);

            float synergyDmg = 22f + matchQuality * 38f;
            foreach (var t in _activeThreats)
            {
                if (t == null) continue;
                var h = t.GetComponent<RailWraithHealthProxy>();
                if (h != null)
                {
                    h.TakeDamage(synergyDmg);
                    if (matchQuality > 0.6f)
                        VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, t.transform.position + Vector3.up * 2f);
                }
            }

            // R6/R7: during levi vuln, strong match advances purify dramatically + phase nudge
            if (_leviathanPhaseActive && matchQuality > 0.55f)
            {
                _lullabyShieldStrength += matchQuality * 0.4f;
                if (Random.value < 0.4f) AdvanceLeviathanPhase(matchQuality);
            }

            if (matchQuality > 0.72f)
            {
                HUDController.Instance?.ShowInteractionPrompt("Escort harmonically empowered!");
            }
            Debug.Log($"[Moon3 R7 Escort] Freq synergy {matchQuality:P0} → shield {_lullabyShieldStrength:F2}x | Phase {_leviathanPhase}");
        }

        // R7: Station/branch restoration hook (called from Moon3BuildingRelay or puzzle on restore)
        public void OnRailStationRestored(string stationName, float tuningQuality)
        {
            if (!_active) return;
            Debug.Log($"[Moon3 R7 RailNet] Station '{stationName}' restored/tuned (quality {tuningQuality:P0}) — combat/tuning hook fired.");
            ApplyRailBossSynergy(tuningQuality);
            if (stationName.Contains("Branch"))
            {
                _currentBranchChoice = tuningQuality > 0.6f ? 1 : 0; // safe vs combat fork
            }
            // Spawn defensive proxy at station for polish
            Vector3 stationPos = Vector3.Lerp(railStart, railEnd, 0.5f);
            var stationProxy = GetPooledOrNewStationProxy(stationPos);
            _activeThreats.Add(stationProxy);
        }

        public static RailEscortController Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Moon3ContinentalRailFastTravelUnlocked = false;
        }

        void Start()
        {
            if (GetComponent<MoonMechanicActivator>() != null)
            {
                // driven externally
            }
        }

        /// <summary>
        /// R6: Begin the full memorable 5-8min Orphan Train Escort set piece. R7: + HUD + extended rail stations init.
        /// </summary>
        public void StartEscort(int adoptedChildren = 1)
        {
            if (_active) return;
            _active = true;
            _progress = 0f;
            _time = 0f;
            _waveIndex = 0;
            _lastWaveTime = 0f;
            _trainHealth = trainMaxHealth;
            _lullabyShieldStrength = Mathf.Clamp(0.75f + adoptedChildren * 0.22f, 0.75f, 2.6f);
            _seventeenthHourActive = false;
            _leviathanPhaseActive = false;
            _permanentWorldChanged = false;
            _nextVulnWindowTime = 170f;
            _vulnWindowEnd = 0f;
            _currentTargetLullabyHz = 432f;
            _leviathanPhase = 0;
            _lastLeviPhaseChange = 0f;
            _currentBranchChoice = -1;
            Moon3ContinentalRailFastTravelUnlocked = false;

            CreateTrainProxy();
            CreateExtendedRailStationsProxies(); // R7: 3+ stations + branch points

            // R6 FULL companion physical reactivity + dialogue on the train (builds on R5 boarding)
            Vector3 trainMid = Vector3.Lerp(railStart, railEnd, 0.12f);
            // Lirael roof singer (physical tell: elevated singing position + wind lean)
            ServiceLocator.Lirael?.BoardTrainLiraelEscort(trainMid + new Vector3(0.9f, 3.1f, -0.8f), false);
            ServiceLocator.Lirael?.AddTrust(4.5f);
            ServiceLocator.Lirael?.BeginLullabySupport();
            // Milo rear guard (physical tell: vigilant rear position + protective stance)
            ServiceLocator.Milo?.BoardTrain(trainMid + new Vector3(-1.8f, 1.1f, 4.2f));
            ServiceLocator.Milo?.AddTrust(3.2f);
            ServiceLocator.Milo?.WitnessOrphanTrain();
            // Cassian (if present on Moon 3) — mid support physical tell + redemption arc hint
            var cassian = ServiceLocator.Cassian;
            if (cassian != null)
            {
                cassian.BoardTrain(trainMid + new Vector3(0.4f, 1.6f, 1.9f));
                cassian.AddTrust(2.8f);
            }

            // R7: Dedicated non-OnGUI HUD (keep OnGUI for testing)
            var hudGO = new GameObject("Moon3EscortHUD_R7");
            _escortHUD = hudGO.AddComponent<Moon3EscortHUD>();
            _escortHUD.Initialize(this);

            HUDController.Instance?.ShowObjective("ORPHAN TRAIN ESCORT — 7 MINUTES OF THE RAILS. Protect the children. Tune the living frequency. (R7 extended network)");
            HUDController.Instance?.ShowBanner("The Dissonant Orphan Train", "First resonance rail live. Children's lullaby is your shield. Frequency is your weapon. Stations ahead.", 6f);

            AudioManager.Instance?.PlaySFX2D("TrainDepart");
            VFXController.Instance?.SpawnMoon3TrainTrail(railStart, 1.1f);

            Debug.Log($"[Moon3 R7 Escort] 7min setpiece + extended rail started. Adopted={adoptedChildren}. Base shield={_lullabyShieldStrength:F2}x");
        }

        void Update()
        {
            if (!_active) return;

            _time += Time.deltaTime;
            _progress = Mathf.Clamp01(_time / escortDuration);

            // Move train with gentle rail bob + forward
            if (_trainProxy != null)
            {
                Vector3 pos = Vector3.Lerp(railStart, railEnd, _progress);
                _trainProxy.transform.position = pos + Vector3.up * Mathf.Sin(_time * 2.8f) * 0.09f;
                _trainProxy.transform.forward = (railEnd - railStart).normalized;

                // R5/R6/R7 periodic train trail VFX (perf throttled)
                if (Time.frameCount % 11 == 0)
                    VFXController.Instance?.SpawnMoon3TrainTrail(pos, 0.65f + _lullabyShieldStrength * 0.35f);
            }

            // R6: Live player frequency drives dynamic difficulty + protection
            float freqMatch = GetLiveFrequencyMatchQuality();
            _lastFreqMatch = freqMatch;
            ApplyFrequencyDrivenDifficulty(freqMatch);

            // R7: Check passing extended stations for restoration/tuning/combat hooks
            CheckRailStationPassage(freqMatch);

            // R6: 17th Hour calendar/live-ops event on the train (memorable moment ~3min in)
            if (!_seventeenthHourActive && _progress > 0.42f && _progress < 0.48f)
            {
                TriggerSeventeenthHourOnTrain();
            }

            // Mid-escort orphan adoption moments with big trust payoff (R6 depth)
            TrySpawnMidEscortOrphanAdoptionMoment();

            // Wave spawning — R6 pacing for 5-8min setpiece (7 waves) + R7 branch effect
            float branchMod = (_currentBranchChoice == 0) ? 0.75f : 1.15f; // combat branch harder
            float dynamicInterval = baseWaveInterval * (0.82f + (1f - freqMatch) * 0.38f) * branchMod;
            if (_time - _lastWaveTime > dynamicInterval && _waveIndex < 7)
            {
                _lastWaveTime = _time;
                _waveIndex++;
                SpawnRailWraithWave(freqMatch);
                OnWaveStarted?.Invoke(_waveIndex);
            }

            // Lullaby healing + protection (children + freq)
            if (Time.frameCount % 24 == 0)
            {
                ApplyLullabyHealingAndProtection(freqMatch);
            }

            ApplyTunedRailDamageToWraiths();

            // Threat count prune
            _activeThreats.RemoveAll(t => t == null);

            // Escort fail if train overwhelmed (protection fantasy)
            if (_trainHealth <= 0f)
            {
                CompleteEscort(false);
                return;
            }

            // Success
            if (_progress >= 1f)
            {
                CompleteEscort(true);
            }
            else if (_time > escortDuration + 22f)
            {
                CompleteEscort(false);
            }

            // Hard fail on too many active threats
            if (_activeThreats.Count > maxWraithSpawnsPerWave + 5)
            {
                _trainHealth -= 18f * Time.deltaTime;
            }

            // R7: Leviathan phase advance timer during phase
            if (_leviathanPhaseActive && _time - _lastLeviPhaseChange > 28f)
            {
                AdvanceLeviathanPhase(0.5f);
            }
        }

        // R7: Extended rail network — station/branch passage with hooks
        void CheckRailStationPassage(float freqMatch)
        {
            foreach (var st in _railStations)
            {
                if (_progress > st.progress - 0.02f && _progress < st.progress + 0.03f)
                {
                    // Trigger once per station (simple flag via time)
                    if (Mathf.Abs(_time - (st.progress * escortDuration)) < 8f)
                    {
                        Debug.Log($"[Moon3 R7 RailNet] Passing {st.name} — {st.hook} engaged.");
                        if (st.hook.Contains("branch"))
                        {
                            // Simple branch fork (R7 per 11_SCRIPTED phase2 choice)
                            _currentBranchChoice = (freqMatch > 0.65f) ? 1 : 0; // freq success = safe tuned path
                            HUDController.Instance?.ShowBanner("RAIL JUNCTION", _currentBranchChoice == 1 ? "Tuned path chosen — lighter waves ahead." : "Combat gauntlet — protection focus required.", 4f);
                            // Trust fork
                            if (_currentBranchChoice == 1) ServiceLocator.Lirael?.AddTrust(5.5f); // freq success Lirael
                            else ServiceLocator.Milo?.AddTrust(5.2f); // protection Milo
                        }
                        else if (st.hook.Contains("restore"))
                        {
                            // Station tuning hook
                            OnRailStationRestored(st.name, freqMatch);
                        }
                        else if (st.hook.Contains("fast_travel"))
                        {
                            Moon3ContinentalRailFastTravelUnlocked = true;
                            SpectralOrphanAdoption.SetSeventeenthHourEvent("continental_rail_unlock", true);
                            Debug.Log("[Moon3 R7] Continental Rail fast travel hook unlocked for post-escort use.");
                        }
                    }
                }
            }
        }

        // R7: Create proxy objects for new stations (static batching, pooled)
        void CreateExtendedRailStationsProxies()
        {
            foreach (var st in _railStations)
            {
                if (st.progress >= 1f) continue;
                Vector3 p = Vector3.Lerp(railStart, railEnd, st.progress);
                var stProxy = GetPooledOrNewStationProxy(p);
                stProxy.name = $"RailStation_{st.name}_R7";
                // Static batch for perf
                foreach (var r in stProxy.GetComponentsInChildren<Renderer>()) r.gameObject.isStatic = true;
                // Attach simple relay hook for future restoration
                var relay = stProxy.AddComponent<Moon3BuildingRelay>();
                relay.buildingId = st.name;
                // Note: FireRestored can be called externally from building system
            }
        }

        GameObject GetPooledOrNewStationProxy(Vector3 pos)
        {
            GameObject go;
            if (_stationProxyPool.Count > 0)
            {
                go = _stationProxyPool.Dequeue();
                go.transform.position = pos;
                go.SetActive(true);
            }
            else
            {
                go = new GameObject("RailStation_Proxy_R7");
                go.transform.position = pos;
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(go.transform);
                cube.transform.localScale = new Vector3(2.8f, 3.2f, 2.8f);
                cube.GetComponent<Renderer>().material.color = new Color(0.85f, 0.78f, 0.55f, 0.9f);
                cube.isStatic = true;
                // Simple tuning trigger volume
                var col = go.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 5f;
            }
            return go;
        }

        // R6: Live frequency match from CombatBridge (R5 live player Hz) — drives everything
        float GetLiveFrequencyMatchQuality()
        {
            float playerHz = 432f;
            try
            {
                // R5 CombatBridge provides authoritative live player frequency
                playerHz = CombatBridge.GetPlayerCurrentFrequency();
            }
            catch { /* safe fallback */ }

            float target = _currentTargetLullabyHz;
            float diff = Mathf.Abs(playerHz - target);
            float match = Mathf.Clamp01(1f - (diff / 95f)); // generous but rewarding window
            // Bonus during vuln windows for leviathan
            if (_leviathanPhaseActive && _time < _vulnWindowEnd && _time > _nextVulnWindowTime - 18f)
            {
                match = Mathf.Clamp01(match * 1.35f);
            }
            return match;
        }

        // R6: Dynamic difficulty + shield modulation from frequency play (the core "tune the rails" fantasy)
        void ApplyFrequencyDrivenDifficulty(float match)
        {
            // Better frequency = stronger shield, slower waves, less HP on threats
            float difficultyScale = 1.15f - (match * 0.72f);
            _lullabyShieldStrength = Mathf.Clamp(_lullabyShieldStrength * 0.985f + match * 0.032f, 0.6f, 3.8f);

            // Occasional target shift for tension (player must retune live)
            if (Time.frameCount % 280 == 0 && !_leviathanPhaseActive)
            {
                _currentTargetLullabyHz = 432f + Random.Range(-38f, 52f);
            }
        }

        // R6: 17th Hour live-ops event on the train — calendar alignment fantasy. R7: more variants
        void TriggerSeventeenthHourOnTrain()
        {
            _seventeenthHourActive = true;
            _lullabyShieldStrength = Mathf.Min(3.9f, _lullabyShieldStrength + 1.15f);

            // Set Moon3 17th Hour persistence (via existing R5/R6 Moon3 block) + R7 variants
            SpectralOrphanAdoption.SetSeventeenthHourEvent("rail_17th_hour_alignment", true);
            SpectralOrphanAdoption.SetSeventeenthHourEvent("17th_hour_rail_variant_highlands", true);

            // Companion physical tells + dialogue during the hour
            ServiceLocator.Lirael?.AddTrust(9f);
            ServiceLocator.Milo?.AddTrust(6.5f);
            var cass = ServiceLocator.Cassian;
            if (cass != null) cass.AddTrust(4f);

            HUDController.Instance?.ShowBanner("THE 17TH HOUR", "The rails align under the hidden sun. The children sing louder than the dissonance ever was.", 9f);
            VFXController.Instance?.SpawnMoon3TrainTrail(_trainProxy ? _trainProxy.transform.position : railStart, 2.6f);
            AudioManager.Instance?.PlaySFX2D("SeventeenthHourChime");

            OnSeventeenthHourTriggered?.Invoke();
            Debug.Log("[Moon3 R7] 17th Hour triggered on the orphan train — live-ops calendar + variants wired.");
        }

        // R6: Mid-escort orphan adoption moments (trust payoff when rail passes old stations)
        void TrySpawnMidEscortOrphanAdoptionMoment()
        {
            // At ~28% and ~61% progress — adoption opportunities if not maxed
            if ((_progress > 0.27f && _progress < 0.31f) || (_progress > 0.59f && _progress < 0.63f))
            {
                if (SpectralOrphanAdoption.AdoptedCount < 3)
                {
                    // Lightweight temp adoption trigger (re-uses existing SpectralOrphanAdoption logic)
                    Vector3 adoptPos = Vector3.Lerp(railStart, railEnd, _progress + 0.06f) + Vector3.up * 1.8f;
                    var tempOrphan = new GameObject("MidEscortOrphanMoment");
                    tempOrphan.transform.position = adoptPos;
                    var adop = tempOrphan.AddComponent<SpectralOrphanAdoption>();
                    // Force a quick trust payoff adoption for pacing (player can engage or it auto-resolves lightly)
                    adop.ForceAdoptForClimax(); // safe re-use of R5 API — big trust + save
                    ServiceLocator.Lirael?.AddTrust(7.5f);
                    ServiceLocator.Milo?.AddTrust(5f);
                    VFXController.Instance?.SpawnGiantEchoRelease(adoptPos); // reuse golden echo as "found family" burst
                    Destroy(tempOrphan, 4.5f);
                    Debug.Log("[Moon3 R7 Escort] Mid-escort orphan adoption moment — trust payoff delivered.");
                }
            }
        }

        void CreateTrainProxy()
        {
            _trainProxy = new GameObject("SpectralTrain_Proxy_Moon3_R7");
            _trainProxy.transform.position = railStart;

            // R5 body + R6 richer windows + undercarriage (still zero-asset primitives for vertical slice)
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(_trainProxy.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(3.8f, 2.4f, 8.2f);
            var rend = body.GetComponent<Renderer>();
            rend.material.color = new Color(0.72f, 0.79f, 0.94f, 0.68f);
            rend.material.EnableKeyword("_EMISSION");
            if (rend.material.HasProperty("_EmissionColor")) rend.material.SetColor("_EmissionColor", new Color(0.35f, 0.68f, 0.98f) * 0.85f);
            body.isStatic = true;

            // More expressive child windows (silhouettes of spectral orphans)
            for (int i = 0; i < 5; i++)
            {
                var win = GameObject.CreatePrimitive(PrimitiveType.Quad);
                win.transform.SetParent(_trainProxy.transform);
                win.transform.localPosition = new Vector3(-2.05f, 0.65f, -3.1f + i * 1.55f);
                win.transform.localScale = new Vector3(1.15f, 1.35f, 1f);
                win.transform.localRotation = Quaternion.Euler(0, -90, 0);
                var wr = win.GetComponent<Renderer>();
                wr.material.color = new Color(0.92f, 0.96f, 1f, 0.38f);
                win.isStatic = true;
            }

            // Golden rail undercarriage + R6 resonance glow rings
            var glow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glow.transform.SetParent(_trainProxy.transform);
            glow.transform.localPosition = Vector3.down * 1.25f;
            glow.transform.localScale = new Vector3(0.55f, 7.8f, 0.38f);
            glow.GetComponent<Renderer>().material.color = new Color(0.96f, 0.87f, 0.28f, 0.72f);
            glow.isStatic = true;

            // Protection trigger volume
            var col = _trainProxy.AddComponent<SphereCollider>();
            col.radius = 6.2f;
            col.isTrigger = true;

            // R6/R7 perf: everything static where possible
            foreach (var r in _trainProxy.GetComponentsInChildren<Renderer>())
                r.gameObject.isStatic = true;
        }

        // R6: Wave spawn with dynamic freq scaling + full companion tells. R7: Levi phases + forks + station synergy
        void SpawnRailWraithWave(float freqMatch)
        {
            int baseCount = 2 + Mathf.Min(_waveIndex, 5);
            int count = Mathf.RoundToInt(baseCount * (0.95f + (1f - freqMatch) * 0.65f));
            Vector3 basePos = Vector3.Lerp(railStart, railEnd, _progress + 0.09f);

            string waveLabel = _waveIndex >= 5 ? "LEVIATHAN ESCALATION" : $"Rail Wraith wave {_waveIndex}";
            HUDController.Instance?.ShowObjective($"{waveLabel}! Shield: {_lullabyShieldStrength:F1}x | Freq match: {freqMatch:P0}");

            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3((i - count * 0.5f) * 4.8f, 0.9f, Random.Range(-4.2f, 4.2f));
                var wraith = GetPooledOrNewWraithProxy(basePos + offset);
                _activeThreats.Add(wraith);
                RequestDOTSRailWraithSpawn(basePos + offset);
            }

            // Harvester + occasional extra (R7 pooled)
            if (_waveIndex % 2 == 0 || freqMatch < 0.45f)
            {
                var harv = GetPooledOrNewHarvesterProxy(basePos + new Vector3(7.5f, 1.4f, Random.Range(-2f, 3f)));
                _activeThreats.Add(harv);
                RequestDOTSRailWraithSpawn(basePos + new Vector3(7.5f, 1.4f, 1f), EnemyType.DissonanceHarvester);
            }

            // R6/R7 Deepened Leviathan boss (waves 4+) with phases
            if (_waveIndex >= 4)
            {
                _leviathanPhaseActive = true;
                var levi = SpawnLeviathanProxy(basePos + new Vector3(0.5f, 5.5f, 0f));
                _activeThreats.Add(levi);
                RequestDOTSRailWraithSpawn(basePos + new Vector3(0.5f, 5.5f, 0f), EnemyType.DissonanceLeviathan);

                HUDController.Instance?.ShowBanner("DISSONANCE LEVIATHAN", "The rails scream. Match the children's frequency to open its heart. Protect the train!", 5f);
                VFXController.Instance?.SpawnLeviathanPhaseVFX(basePos, _waveIndex + _leviathanPhase);
                CameraController.Instance?.TriggerShake(0.95f, 0.7f);

                // R7: Start/advance phase
                if (_leviathanPhase == 0) _leviathanPhase = 1;
                _lastLeviPhaseChange = _time;

                // Companion physical tells during levi (roof lean, rear guard brace, Cassian support) + R7 forks
                ServiceLocator.Lirael?.AddTrust(3.5f);
                ServiceLocator.Milo?.AddTrust(2.8f);
                if (ServiceLocator.Cassian != null) ServiceLocator.Cassian.AddTrust(2.1f);
            }

            // R6/R7 Companion reactivity at wave peaks + trust forks (freq success vs protection)
            if (_waveIndex == 2)
            {
                if (freqMatch > 0.7f) { ServiceLocator.Lirael?.AddTrust(4.2f); /* freq success: Lirael sings louder, roof lean intensified */ }
                else { ServiceLocator.Milo?.AddTrust(3.8f); /* protection focus: Milo braces harder */ }
            }
            if (_waveIndex == 4)
            {
                ServiceLocator.Lirael?.AddTrust(2.6f); /* "Sing with me — louder than the wind!" */
            }
            if (_waveIndex == 6)
            {
                ServiceLocator.Lirael?.AddTrust(4f);
                ServiceLocator.Milo?.AddTrust(3f);
                if (freqMatch > 0.65f) ServiceLocator.Lirael?.AddTrust(3f); // extra freq fork
            }

            AudioManager.Instance?.PlaySFX2D("WraithShriek");
            HapticFeedbackManager.Instance?.PlayTuningMiss();
        }

        // R7: Advance Leviathan phases with distinct patterns (GDD: Tail/Scream/Barrage)
        void AdvanceLeviathanPhase(float synergy)
        {
            _leviathanPhase = (_leviathanPhase + 1) % 5;
            _lastLeviPhaseChange = _time;
            if (_leviathanPhase == 4) _leviathanPhase = 1; // loop until purified

            Vector3 pos = _trainProxy ? _trainProxy.transform.position : Vector3.Lerp(railStart, railEnd, _progress);
            VFXController.Instance?.SpawnLeviathanPhaseVFX(pos, _leviathanPhase + 10);

            Debug.Log($"[Moon3 R7 Leviathan] Phase advanced to {_leviathanPhase} (synergy {synergy:F2})");
            if (_leviathanPhase == 3)
            {
                // Barrage phase: extra threats
                for (int k = 0; k < 2; k++)
                {
                    var extra = GetPooledOrNewHarvesterProxy(pos + Random.insideUnitSphere * 6f);
                    _activeThreats.Add(extra);
                }
            }
        }

        // R6 perf pool for wraith proxies
        GameObject GetPooledOrNewWraithProxy(Vector3 pos)
        {
            GameObject go;
            if (_wraithProxyPool.Count > 0)
            {
                go = _wraithProxyPool.Dequeue();
                go.transform.position = pos;
                go.SetActive(true);
            }
            else
            {
                go = SpawnRailWraithProxy(pos);
            }
            return go;
        }

        // R7: Expanded harvester pool
        GameObject GetPooledOrNewHarvesterProxy(Vector3 pos)
        {
            GameObject go;
            if (_harvesterProxyPool.Count > 0)
            {
                go = _harvesterProxyPool.Dequeue();
                go.transform.position = pos;
                go.SetActive(true);
            }
            else
            {
                go = SpawnHarvesterProxy(pos);
            }
            return go;
        }

        void ReturnToPool(GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            if (go.name.Contains("Harvester")) _harvesterProxyPool.Enqueue(go);
            else if (go.name.Contains("Station")) _stationProxyPool.Enqueue(go);
            else _wraithProxyPool.Enqueue(go);
        }

        GameObject SpawnRailWraithProxy(Vector3 pos) { /* R6 body preserved, R7 static tweaks */ 
            var go = new GameObject("RailWraith_Proxy_R7");
            go.transform.position = pos;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(go.transform);
            body.transform.localScale = new Vector3(1.15f, 2.55f, 1.15f);
            var rend = body.GetComponent<Renderer>();
            rend.material.color = new Color(0.18f, 0.08f, 0.24f, 0.88f);
            rend.material.EnableKeyword("_EMISSION");
            if (rend.material.HasProperty("_EmissionColor")) rend.material.SetColor("_EmissionColor", Color.red * 0.55f);
            body.isStatic = false;
            var rb = go.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
            go.tag = "Enemy";
            go.layer = LayerMask.NameToLayer("Enemy") >= 0 ? LayerMask.NameToLayer("Enemy") : 0;
            var health = go.AddComponent<RailWraithHealthProxy>();
            health.maxHealth = railWraithHealth * (0.9f + _waveIndex * 0.08f);
            health.OnDeath += () => { OnWraithDestroyed(go); ReturnToPool(go); };
            return go;
        }

        GameObject SpawnHarvesterProxy(Vector3 pos)
        {
            var go = new GameObject("DissonanceHarvester_Proxy_R7");
            go.transform.position = pos;
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.transform.SetParent(go.transform);
            body.transform.localScale = Vector3.one * 1.35f;
            body.GetComponent<Renderer>().material.color = new Color(0.12f, 0.04f, 0.32f);
            body.isStatic = false;
            var health = go.AddComponent<RailWraithHealthProxy>();
            health.maxHealth = 68f;
            health.OnDeath += () => { OnWraithDestroyed(go); ReturnToPool(go); };
            return go;
        }

        GameObject SpawnLeviathanProxy(Vector3 pos)
        {
            var go = new GameObject("DissonanceLeviathan_Boss_Moon3_R7");
            go.transform.position = pos;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(go.transform);
            body.transform.localScale = new Vector3(3.1f, 1.85f, 9.8f);
            var rend = body.GetComponent<Renderer>();
            rend.material.color = new Color(0.08f, 0.015f, 0.15f, 0.94f);
            rend.material.EnableKeyword("_EMISSION");
            if (rend.material.HasProperty("_EmissionColor")) rend.material.SetColor("_EmissionColor", Color.red * 1.35f);
            var health = go.AddComponent<RailWraithHealthProxy>();
            health.maxHealth = 520f;
            health.OnDeath += () => { OnWraithDestroyed(go); ReturnToPool(go); };
            go.name = "DissonanceLeviathan_Boss_Moon3";
            return go;
        }

        // R5 DOTS preserved + R6/R7 throttle for perf
        void RequestDOTSRailWraithSpawn(Vector3 pos, EnemyType type = EnemyType.RailWraith)
        {
            if (_activeThreats.Count > 16) return; // perf throttle R7
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;
            var em = world.EntityManager;
            var e = em.CreateEntity();
            em.AddComponentData(e, new LocalTransform { Position = new float3(pos.x, pos.y, pos.z), Rotation = quaternion.identity, Scale = 1f });
            em.AddComponentData(e, new EnemySpawnTrigger { RSThreshold = 0f, EnemyToSpawn = type, SpawnPosition = new float3(pos.x, pos.y, pos.z), HasSpawned = false });
        }

        void OnWraithDestroyed(GameObject wraith)
        {
            _activeThreats.Remove(wraith);
            GameLoopController.Instance?.QueueRSReward(5.5f, "rail_wraith_kill_r7");
        }

        // R6: Lullaby + protection loop (train takes damage unless shield/freq high). R7: orphan lullaby synergy + phase dmg
        void ApplyLullabyHealingAndProtection(float freqMatch)
        {
            if (_trainProxy == null) return;

            var children = SpectralOrphanAdoption.AdoptedCount;
            if (children <= 0) return;

            // Heal train integrity via lullaby + freq
            float heal = (children * 0.9f + freqMatch * 6.5f) * _lullabyShieldStrength * 0.018f;
            _trainHealth = Mathf.Min(trainMaxHealth, _trainHealth + heal);

            // Damage threats + special leviathan lullaby purify path (R7 deepened with phases + orphan synergy)
            foreach (var t in _activeThreats)
            {
                if (t == null) continue;
                var h = t.GetComponent<RailWraithHealthProxy>();
                if (h == null) continue;

                float orphanSynergy = children * 1.35f; // R7: lullaby strength scales directly with adopted orphans
                float baseDmg = 2.8f * _lullabyShieldStrength * orphanSynergy * 0.7f;

                if (t.name.Contains("Leviathan") || _leviathanPhaseActive)
                {
                    // R7 Deepened Leviathan: vuln windows + phase-specific + protection + orphan synergy
                    bool inVuln = _time >= _nextVulnWindowTime && _time <= _vulnWindowEnd;
                    float phaseMul = 0.8f + _leviathanPhase * 0.25f;
                    if (inVuln)
                    {
                        float vulnDmg = 18f * freqMatch * children * 0.85f * phaseMul;
                        h.TakeDamage(vulnDmg);
                        if (freqMatch > 0.78f)
                            VFXController.Instance?.SpawnLeviathanPhaseVFX(t.transform.position, 9 + _leviathanPhase);
                    }
                    else
                    {
                        h.TakeDamage(baseDmg * 0.55f * Time.deltaTime * phaseMul); // protection: reduced outside window
                    }

                    if (h.currentHealth <= 0f && !_permanentWorldChanged)
                    {
                        PurifyLeviathanAndTransformWorld(t);
                    }
                }
                else
                {
                    h.TakeDamage(baseDmg * Time.deltaTime * 0.7f);
                }
            }
        }

        // R6: Leviathan purify with permanent world change (core GDD payoff). R7: stronger VFX + phase reset
        void PurifyLeviathanAndTransformWorld(GameObject levi)
        {
            _permanentWorldChanged = true;
            _leviathanPhase = 0;
            HUDController.Instance?.ShowBanner("GIANT ECHO FREED", "The children's lullaby shattered the cage. The highlands remember their song. Rails glow forever.", 10f);
            Debug.Log("[Moon3 R7 Leviathan] GIANT ECHO + PERMANENT WORLD CHANGE — victory transforms the zone with deeper VFX.");

            VFXController.Instance?.SpawnGiantEchoRelease(_trainProxy ? _trainProxy.transform.position + Vector3.up * 11f : railEnd);
            CameraController.Instance?.TriggerShake(1.65f, 2.1f);

            // R7 stronger permanent world transformation VFX
            for (int i = 0; i < 4; i++)
            {
                Vector3 extra = Vector3.Lerp(railStart, railEnd, 0.3f + i * 0.18f) + Vector3.up * (8 + i * 2);
                VFXController.Instance?.SpawnGiantEchoRelease(extra);
            }

            SpectralOrphanAdoption.SetGiantEchoFreed(true);
            SpectralOrphanAdoption.SetLeviathanDefeated(true);
            SpectralOrphanAdoption.SetSeventeenthHourEvent("leviathan_purified", true);

            ServiceLocator.Lirael?.AddTrust(18f);
            ServiceLocator.Milo?.AddTrust(11f);
            if (ServiceLocator.Cassian != null) ServiceLocator.Cassian.AddTrust(7f);

            // Permanent world change: golden rail glows + calmed winds + echo marker (R7 enhanced)
            CreatePermanentMoon3VictoryMarkers();

            _activeThreats.Remove(levi);
            Destroy(levi, 0.6f);

            OnLeviathanPurified?.Invoke();
        }

        // R6: Instantiates permanent static objects that survive escort end (world transformation). R7: more markers + wind mgmt
        void CreatePermanentMoon3VictoryMarkers()
        {
            // Golden resonance rail glow strips along entire path (static) — R7 extended
            for (int i = 0; i < 14; i++)
            {
                float t = i / 13f;
                Vector3 p = Vector3.Lerp(railStart, railEnd, t) + Vector3.up * 0.4f;
                var railGlow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                railGlow.name = "Permanent_RailGlow_Victory_Moon3_R7";
                railGlow.transform.position = p;
                railGlow.transform.localScale = new Vector3(0.9f, 1.1f, 0.9f);
                railGlow.transform.rotation = Quaternion.LookRotation((railEnd - railStart).normalized);
                var r = railGlow.GetComponent<Renderer>();
                r.material.color = new Color(0.95f, 0.88f, 0.35f, 0.85f);
                r.material.EnableKeyword("_EMISSION");
                if (r.material.HasProperty("_EmissionColor")) r.material.SetColor("_EmissionColor", Color.yellow * 1.4f);
                railGlow.isStatic = true;
                Destroy(railGlow.GetComponent<Collider>());
            }

            // Giant Echo permanent marker (light + aura) — world remembers (R7 more intense)
            var echo = new GameObject("GiantEcho_Permanent_Moon3_Victory_R7");
            echo.transform.position = Vector3.Lerp(railStart, railEnd, 0.78f) + Vector3.up * 14f;
            var light = echo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.95f, 0.88f, 0.55f);
            light.intensity = 7.2f;
            light.range = 62f;
            var aura = echo.AddComponent<ParticleSystem>();
            echo.isStatic = true;

            // R7: Additional echo resonance pillars at stations
            foreach (var st in _railStations)
            {
                if (st.progress < 0.95f)
                {
                    Vector3 sp = Vector3.Lerp(railStart, railEnd, st.progress) + Vector3.up * 9f;
                    var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pillar.transform.position = sp;
                    pillar.transform.localScale = new Vector3(0.6f, 4.5f, 0.6f);
                    pillar.GetComponent<Renderer>().material.color = new Color(1f, 0.92f, 0.4f, 0.7f);
                    pillar.isStatic = true;
                    Destroy(pillar.GetComponent<Collider>());
                }
            }

            // Calm nearby wind proxies (disable aggressive gust colliders) — R7 extended range + station winds
            foreach (var wz in GameObject.FindGameObjectsWithTag("Untagged"))
            {
                if (wz.name.Contains("WindZone") && Vector3.Distance(wz.transform.position, echo.transform.position) < 78f)
                {
                    wz.SetActive(false); // world change: winds calm after leviathan purified
                }
            }

            Debug.Log("[Moon3 R7] Permanent world change applied: glowing rails + giant echo marker + calmed winds + station pillars.");
        }

        void ApplyTunedRailDamageToWraiths()
        {
            bool railsTuned = FindAnyObjectByType<OrphanTrainPuzzle>()?.IsCompleted ?? false;
            if (!railsTuned || _trainProxy == null) return;

            foreach (var t in _activeThreats)
            {
                if (t == null) continue;
                var h = t.GetComponent<RailWraithHealthProxy>();
                if (h != null && Vector3.Distance(t.transform.position, _trainProxy.transform.position) < 10f)
                {
                    h.TakeDamage(21f * Time.deltaTime * _lullabyShieldStrength);
                }
            }
        }

        void CompleteEscort(bool success)
        {
            _active = false;

            if (success)
            {
                HUDController.Instance?.ShowObjective("Escort complete. The spectral children are safe. The highlands sing again. Rail network expanded.");
                HUDController.Instance?.ShowBanner("Rail Network Awakens", "First grand segment secured. Lullaby Crystal + World's Fair Ticket + Continental Rail hook granted. Found family grows.", 8f);

                GameLoopController.Instance?.QueueRSReward(245f, "moon3_escort_r7_complete");
                ServiceLocator.Lirael?.AddTrust(12f);
                ServiceLocator.Milo?.AddTrust(9f);
                if (ServiceLocator.Cassian != null) ServiceLocator.Cassian.AddTrust(5f);

                // R5 save + R6 quest + World's Fair ticket (Moon 3 live-ops reward) + R7 variants
                SpectralOrphanAdoption.SetEscortCompleted(true);
                Tartaria.Integration.QuestManager.Instance?.ProgressByType(Tartaria.Integration.QuestObjectiveType.CompleteTuning, "rail_escort_moon3_r7");
                Tartaria.Integration.QuestManager.Instance?.ProgressByType(Tartaria.Integration.QuestObjectiveType.CompanionMilestone, "orphan_train_escort");
                // World's Fair ticket on Moon 3 + R7 variants
                SpectralOrphanAdoption.SetSeventeenthHourEvent("worlds_fair_ticket_moon3", true);
                SpectralOrphanAdoption.SetSeventeenthHourEvent("worlds_fair_golden_variant_rail", true);
                SpectralOrphanAdoption.SetSeventeenthHourEvent("rail_success_daily_deal", true); // new daily tied to success
                Tartaria.Integration.QuestManager.Instance?.ProgressByType(Tartaria.Integration.QuestObjectiveType.HiddenDiscovery, "worlds_fair_ticket_moon3");

                VFXController.Instance?.SpawnMoon3TrainTrail(_trainProxy ? _trainProxy.transform.position : railEnd, 2.8f);

                AudioManager.Instance?.PlaySFX2D("TrainRestored");

                Moon3ContinentalRailFastTravelUnlocked = true; // R7 optional fast travel hook
                SpectralOrphanAdoption.SetSeventeenthHourEvent("post_escort_continental_rail_ready", true);

                if (!_permanentWorldChanged)
                {
                    // Fallback world change even on clean success
                    CreatePermanentMoon3VictoryMarkers();
                }
            }
            else
            {
                HUDController.Instance?.ShowObjective("The train was overwhelmed. Re-align the rails. The children still believe in you.");
                ServiceLocator.Lirael?.AddTrust(-3.5f);
                ServiceLocator.Milo?.AddTrust(-2f);
            }

            OnEscortComplete?.Invoke(success);

            // Cleanup (pool friendly)
            if (_escortHUD != null) _escortHUD.Shutdown();
            if (_trainProxy) Destroy(_trainProxy, 2.8f);
            foreach (var t in _activeThreats) if (t) Destroy(t, 0.2f);
            _activeThreats.Clear();
            _wraithProxyPool.Clear();
            _harvesterProxyPool.Clear();
            _stationProxyPool.Clear();

            Debug.Log($"[Moon3 R7 Escort] Complete. Success={success}. 17th={_seventeenthHourActive} WorldChanged={_permanentWorldChanged} FastTravel={Moon3ContinentalRailFastTravelUnlocked}");
        }

        // R6 dedicated escort HUD (OnGUI only for Moon3 vertical slice — zero new UI assets). R7: kept for quick testing alongside dedicated HUD.
        void OnGUI()
        {
            if (!_active || _trainProxy == null) return;

            GUI.Box(new Rect(Screen.width / 2 - 245, 14, 490, 138), "ORPHAN TRAIN ESCORT — WINDSWEPT HIGHLANDS (MOON 3 R7)");
            GUI.Label(new Rect(Screen.width / 2 - 225, 36, 450, 20), $"PROGRESS: {_progress * 100f:F0}%  |  TIME: {_time:F0}s / {escortDuration:F0}s  |  TRAIN INTEGRITY: {(_trainHealth / trainMaxHealth * 100f):F0}%");
            GUI.Label(new Rect(Screen.width / 2 - 225, 56, 450, 20), $"LULLABY SHIELD: {_lullabyShieldStrength:F2}x  |  ADOPTED: {SpectralOrphanAdoption.AdoptedCount}  |  FREQ MATCH: {_lastFreqMatch:P0}");
            GUI.Label(new Rect(Screen.width / 2 - 225, 76, 450, 20), $"WAVE: {_waveIndex}/7  |  THREATS: {_activeThreats.Count}  |  TARGET Hz: {_currentTargetLullabyHz:F0} | LEVI PHASE: {_leviathanPhase}");

            float barW = 420f * _progress;
            GUI.color = Color.cyan;
            GUI.Box(new Rect(Screen.width / 2 - 210, 98, barW, 9), "");
            GUI.color = Color.white;

            if (_seventeenthHourActive)
            {
                GUI.color = new Color(1f, 0.92f, 0.6f);
                GUI.Label(new Rect(Screen.width / 2 - 225, 112, 450, 18), "★ 17TH HOUR ALIGNMENT — THE RAILS REMEMBER ★");
                GUI.color = Color.white;
            }
            if (_leviathanPhaseActive)
            {
                GUI.color = new Color(0.95f, 0.25f, 0.35f);
                GUI.Label(new Rect(Screen.width / 2 - 225, 128, 450, 18), "LEVIATHAN PHASE — MATCH FREQUENCY IN VULN WINDOWS TO PURIFY");
                GUI.color = Color.white;
            }
        }

        // Helper proxy (R5 preserved, R6 health tweaks)
        public class RailWraithHealthProxy : MonoBehaviour
        {
            public float maxHealth = 95f;
            public float currentHealth;
            public System.Action OnDeath;

            void Awake() { currentHealth = maxHealth; }

            public void TakeDamage(float dmg)
            {
                currentHealth -= dmg;
                if (currentHealth <= 0f)
                {
                    OnDeath?.Invoke();
                    Destroy(gameObject, 0.08f);
                }
            }
        }
    }
}
