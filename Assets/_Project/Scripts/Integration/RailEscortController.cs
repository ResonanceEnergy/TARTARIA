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
using Tartaria.Gameplay; // for EnemyType / EnemySpawnTrigger / Moon3BuildingRelay / SpectralOrphanAdoption / QuestObjectiveType
using UnityEngine.InputSystem; // for Gamepad F310 lullaby rhythm input (face buttons + triggers)

namespace Tartaria.Integration
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
        [Header("Escort Config — Moon 3 R7 10-15min Playable Experience")]
        [SerializeField] float escortDuration = 660f; // ~11 minutes for full fun balanced loop (start->waves->stations->levi 4phases->victory)
        [SerializeField] float trainSpeed = 3.8f;
        [SerializeField] int maxWraithSpawnsPerWave = 5; // balanced, not overwhelming
        [SerializeField] float baseWaveInterval = 48f; // tuned for 9-10 waves over 11min + rhythm/freq breathing room

        [Header("Path (R6 linear + R7 extended stations & branch points)")]
        [SerializeField] public Vector3 railStart = new Vector3(20, 6, -10);
        [SerializeField] public Vector3 railEnd = new Vector3(140, 6, 55);

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
        bool _moon3AutoStarted; // ensures one-time auto-start on Moon 3 without external trigger

        // R7: Leviathan phase state (0=approach,1=tail,2=scream,3=barrage,4=purify)
        int _leviathanPhase;
        float _lastLeviPhaseChange;

        // R7: Branch choice state for extended network
        int _currentBranchChoice = -1; // -1 none, 0=short combat, 1=safe tuned

        // R7: Fast travel hook (post-escort Continental Rail ready)
        public static bool Moon3ContinentalRailFastTravelUnlocked { get; set; }

        // Moon 3 Lullaby Rhythm Input (432Hz base, F310 gamepad face buttons + triggers, keyboard fallback)
        float _lullabyBeatInterval = 0.82f; // ~73 BPM lullaby pulse feel, tied to 432 resonance
        float _lastLullabyBeat;
        float _rhythmCombo;
        int _lullabyRhythmHits;

        // R6 simple proxy pool for perf (no GC spam on waves) + R7 expanded
        readonly Queue<GameObject> _wraithProxyPool = new Queue<GameObject>();
        readonly Queue<GameObject> _harvesterProxyPool = new Queue<GameObject>();
        readonly Queue<GameObject> _stationProxyPool = new Queue<GameObject>(); // R7 station proxies

        // R7 dedicated HUD (non-OnGUI)
        Moon3EscortHUD _escortHUD;

        // Moon 3 Exclusive Audio Heart (lullaby rhythm, train, wind, leviathan, Aether Remembers)
        Moon3RailAudioManager _moon3RailAudio;

        // ─── Moon 3 "Compassion & Rails" Visual State (3D/TA R7) ───
        // Train damage states + spectral orphan children visuals that react to lullaby singing
        GameObject _trainBody;
        List<Renderer> _trainRenderers = new List<Renderer>();
        GameObject[] _spectralOrphanVisuals; // Aria, Toren, Syl glow proxies inside/ on train
        float _lastTrainHealthForVFX;
        MaterialPropertyBlock _trainMPB;
        bool _trainDamageVFXActive;

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
        public float LullabyRhythmCombo => _rhythmCombo;
        public int LullabyRhythmHits => _lullabyRhythmHits;
        public int CurrentBranchChoice => _currentBranchChoice;

        public event System.Action<bool> OnEscortComplete; // success/fail
        public event System.Action<int> OnWaveStarted;
        public event System.Action OnSeventeenthHourTriggered;
        public event System.Action OnLeviathanPurified;
        public event System.Action<int> OnBranchChoiceDecided; // 0 = combat gauntlet (Milo favored), 1 = tuned safe path (Lirael favored) at WindspireJunction

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
                        ServiceLocator.VFX?.PlayEffect(VFXEffect.HarmonicCascade, t.transform.position + Vector3.up * 2f);
                }
            }

            // R6/R7: during levi vuln, strong match advances purify dramatically + phase nudge
            if (_leviathanPhaseActive && matchQuality > 0.55f)
            {
                _lullabyShieldStrength += matchQuality * 0.4f;
                if (UnityEngine.Random.value < 0.4f) AdvanceLeviathanPhase(matchQuality);
            }

            if (matchQuality > 0.72f)
            {
                // [Moon1 HUD stub] ShowInteractionPrompt("Escort harmonically empowered!");
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
            // NOTE: station visual proxies pre-created in CreateExtendedRailStationsProxies (with relays).
            // Do NOT add stations to _activeThreats — they are not threats (cleaned duplication / escort logic bug).
            // Restoration only applies synergy + branch fork here.
        }

        public static RailEscortController Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Moon3ContinentalRailFastTravelUnlocked = false;
            _moon3AutoStarted = false;
        }

        void Start()
        {
            if (ServiceLocator.MoonMechanic?.HasActivator(gameObject) ?? false)
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

            // Wire to Moon03 data (if available via campaign) for Moon 3 specific tuning
            try
            {
                if (ServiceLocator.Campaign != null && ServiceLocator.Campaign.CurrentMoonIndex == 3)
                {
                    // Moon 3 OrphanTrain mechanic specific: slightly stronger starting shield + 432 base
                    _currentTargetLullabyHz = 432f;
                    _lullabyShieldStrength = Mathf.Max(_lullabyShieldStrength, 1.15f);
                    Debug.Log("[Moon3 RailEscort] Wired to Moon03 Windswept Highlands data — OrphanTrain escort active.");
                }
            }
            catch { /* safe if campaign not ready */ }
            _nextVulnWindowTime = 170f;
            _vulnWindowEnd = 0f;
            _currentTargetLullabyHz = 432f;
            _leviathanPhase = 0;
            _lastLeviPhaseChange = 0f;
            _currentBranchChoice = -1;
            Moon3ContinentalRailFastTravelUnlocked = false;

            // Lullaby rhythm init for playable input loop
            _lastLullabyBeat = Time.time;
            _rhythmCombo = 0f;
            _lullabyRhythmHits = 0;
            _moon3AutoStarted = true;

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

            // Moon 3 Audio Heart — Lullaby Rhythm System + full dynamic soundscape (432Hz emotional core)
            var audioGO = new GameObject("Moon3_RailAudio_Heart");
            _moon3RailAudio = audioGO.AddComponent<Moon3RailAudioManager>();
            _moon3RailAudio.InitializeForEscort(this);

            // ─── R7 Full event integration for VFX / haptics / F310 rumble (Compassion & Rails narrative) ───
            OnEscortComplete += HandleEscortCompleteVFX;
            OnWaveStarted += HandleWaveVFX;
            OnSeventeenthHourTriggered += HandleSeventeenthHourVFX;
            OnLeviathanPurified += HandleLeviathanPurifiedVFX;

            // [Moon1 HUD stub] ShowObjective("ORPHAN TRAIN ESCORT — 7 MINUTES OF THE RAILS. Protect the children. Tune the living frequency. (R7 extended network)");
            // [Moon1 HUD stub] ShowBanner("The Dissonant Orphan Train", "First resonance rail live. Children's lullaby is your shield. Frequency is your weapon. Stations ahead.", 6f);

            // Replaced by Moon3RailAudioManager (rich Moon3_TrainDepart + loops)
            ServiceLocator.VFX?.SpawnMoon3TrainTrail(railStart, 1.1f);

            Debug.Log($"[Moon3 R7 Escort] 7min setpiece + extended rail started. Adopted={adoptedChildren}. Base shield={_lullabyShieldStrength:F2}x");
        }

        void Update()
        {
            // Moon 3 auto-start for complete playable experience (wired to Moon03 data / campaign)
            if (!_active && !_moon3AutoStarted && ServiceLocator.Campaign?.CurrentMoonIndex == 3)
            {
                int kids = (SpectralOrphanAdoption.AdoptedCount > 0) ? SpectralOrphanAdoption.AdoptedCount : 2;
                StartEscort(kids);
                _moon3AutoStarted = true;
            }

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
                    ServiceLocator.VFX?.SpawnMoon3TrainTrail(pos, 0.65f + _lullabyShieldStrength * 0.35f);
            }

            // R6: Live player frequency drives dynamic difficulty + protection
            float freqMatch = GetLiveFrequencyMatchQuality();
            _lastFreqMatch = freqMatch;
            ApplyFrequencyDrivenDifficulty(freqMatch);

            // Moon 3: Active lullaby rhythm input (playable core loop — F310 gamepad + kb)
            HandleLullabyRhythmInput();

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
            if (_time - _lastWaveTime > dynamicInterval && _waveIndex < 10)
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

            // Playable vuln windows for 4-phase Leviathan climax (Approach/Tail/Scream/Barrage -> Purify on death)
            if (_leviathanPhaseActive && _time > _vulnWindowEnd && _time >= _nextVulnWindowTime)
            {
                _nextVulnWindowTime = _time + 14f; // breathing room between windows
                _vulnWindowEnd = _nextVulnWindowTime + 7.5f; // generous 7.5s window to rhythm/freq match & purify
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
                            // R7: Real branch choice at WindspireJunction — affects difficulty and world
                            bool tunedPath = freqMatch > 0.65f;
                            _currentBranchChoice = tunedPath ? 1 : 0;
                            OnBranchChoiceDecided?.Invoke(_currentBranchChoice);

                            // Visual junction fork
                            Vector3 junctionPos = Vector3.Lerp(railStart, railEnd, st.progress);
                            ServiceLocator.VFX?.PlayResonancePulse(junctionPos + Vector3.up * 5f, tunedPath ? 15f : 10f);

                            // Difficulty shift: tuned path = easier waves + Lirael trust; combat = harder + Milo trust
                            if (tunedPath)
                            {
                                baseWaveInterval = 58f; // safer
                                ServiceLocator.Lirael?.AddTrust(5.5f);
                                Debug.Log("[Moon3 Branch] Tuned safe path chosen — lighter threats, Lirael empowered.");
                            }
                            else
                            {
                                baseWaveInterval = 42f; // gauntlet
                                maxWraithSpawnsPerWave = 8;
                                ServiceLocator.Milo?.AddTrust(5.2f);
                                Debug.Log("[Moon3 Branch] Combat gauntlet chosen — heavier waves, Milo on guard.");
                            }

                            // HUD prompt
                            Moon3EscortHUD.Instance?.ShowBranchPrompt(_currentBranchChoice);
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
                // R7 Visual differentiation for stations (Highland warm earth, Windspire airy, Leviathan dark crystal, Hub golden)
                TintStationVisuals(stProxy, st.name);
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

                // R7 Visual: Proper rail station props (procedural + story rich) matching scaffold detailed visuals for cohesive look
                // Platform base
                var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.transform.SetParent(go.transform);
                platform.transform.localPosition = Vector3.zero;
                platform.transform.localScale = new Vector3(5.8f, 1.12f, 4.5f);
                platform.GetComponent<Renderer>().material.color = new Color(0.62f, 0.55f, 0.48f);
                platform.isStatic = true;

                // Station house
                var house = GameObject.CreatePrimitive(PrimitiveType.Cube);
                house.transform.SetParent(go.transform);
                house.transform.localPosition = new Vector3(0, 2.2f, -0.9f);
                house.transform.localScale = new Vector3(2.4f, 4.1f, 2.0f);
                house.GetComponent<Renderer>().material.color = new Color(0.78f, 0.72f, 0.62f);
                house.isStatic = true;

                // Roof resonance crystal (ties to Grand Crystal Organ / story)
                var crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crystal.transform.SetParent(go.transform);
                crystal.transform.localPosition = new Vector3(0, 4.85f, -0.9f);
                crystal.transform.localScale = new Vector3(0.55f, 0.95f, 0.55f);
                var cr = crystal.GetComponent<Renderer>();
                cr.material.color = new Color(0.85f, 0.78f, 0.55f);
                cr.material.EnableKeyword("_EMISSION");
                if (cr.material.HasProperty("_EmissionColor")) cr.material.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 0.55f);
                crystal.isStatic = true;

                // Rail tracks (two long cylinders representing the resonance rails)
                for (int t = -1; t <= 1; t += 2)
                {
                    var rail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    rail.transform.SetParent(go.transform);
                    rail.transform.localPosition = new Vector3(t * 0.95f, 0.58f, 0.2f);
                    rail.transform.localScale = new Vector3(0.19f, 4.1f, 0.19f);
                    rail.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    rail.GetComponent<Renderer>().material.color = new Color(0.42f, 0.39f, 0.36f);
                    rail.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                    if (rail.GetComponent<Renderer>().material.HasProperty("_EmissionColor"))
                        rail.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.65f, 0.6f, 0.35f) * 0.35f);
                    rail.isStatic = true;
                }

                // Simple tuning trigger volume
                var col = go.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 5.2f;
            }
            return go;
        }

        void TintStationVisuals(GameObject station, string stationId)
        {
            // Story telling colors: Highland Depot = warm earth/amber, Windspire = cool wind blue, Leviathan = ominous purple, Continental = triumphant gold
            Color tint = new Color(0.85f, 0.78f, 0.55f);
            if (stationId.Contains("Highland")) tint = new Color(0.78f, 0.62f, 0.42f);
            else if (stationId.Contains("Windspire")) tint = new Color(0.55f, 0.72f, 0.85f);
            else if (stationId.Contains("Leviathan")) tint = new Color(0.45f, 0.32f, 0.52f);
            else if (stationId.Contains("Continental")) tint = new Color(0.95f, 0.88f, 0.45f);

            foreach (var rend in station.GetComponentsInChildren<Renderer>())
            {
                if (rend == null) continue;
                rend.material.color = Color.Lerp(rend.material.color, tint, 0.65f);
            }
        }

        // R6: Live frequency match from CombatBridge (R5 live player Hz) — drives everything
        float GetLiveFrequencyMatchQuality()
        {
            float playerHz = 432f;
            try
            {
                // R5 CombatBridge provides authoritative live player frequency
            playerHz = ServiceLocator.Combat?.GetPlayerCurrentFrequency() ?? 432f;
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
            // Rhythm hits make live freq "feel" stronger immediately (playable empowerment)
            match = Mathf.Clamp01(match + (_rhythmCombo * 0.035f));
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
                _currentTargetLullabyHz = 432f + UnityEngine.Random.Range(-38f, 52f);
            }
        }

        // =====================================================================
        // MOON 3 PLAYABLE LULLABY RHYTHM INPUT — 432Hz base, F310 gamepad native
        // =====================================================================
        /// <summary>
        /// Active rhythm matcher: player "sings the lullaby" with the orphans using
        /// F310 face buttons (A/B/X/Y) or triggers during escort. Successes boost shield,
        /// nudge live freq toward target, damage threats, advance levi phases.
        /// Creates the core engaging 10-15min loop alongside passive freq + protection.
        /// </summary>
        void HandleLullabyRhythmInput()
        {
            if (!_active) return;

            bool inputPressed = false;
            float pressTime = Time.time;

            // F310 / standard gamepad: face buttons + shoulder triggers for lullaby taps (intuitive "sing")
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame || // A / Cross
                    gamepad.buttonNorth.wasPressedThisFrame || // Y / Triangle
                    gamepad.buttonEast.wasPressedThisFrame ||  // B / Circle
                    gamepad.buttonWest.wasPressedThisFrame ||  // X / Square
                    gamepad.leftTrigger.wasPressedThisFrame ||
                    gamepad.rightTrigger.wasPressedThisFrame)
                {
                    inputPressed = true;
                }
            }

            // Keyboard fallback for dev/playtest (no gamepad required)
            var kb = Keyboard.current;
            if (kb != null &&
                (kb.spaceKey.wasPressedThisFrame ||
                 kb.jKey.wasPressedThisFrame || kb.kKey.wasPressedThisFrame ||
                 kb.lKey.wasPressedThisFrame || kb.semicolonKey.wasPressedThisFrame))
            {
                inputPressed = true;
            }

            if (inputPressed)
            {
                // Compute timing accuracy vs 432Hz-timed lullaby beat grid
                float phase = (pressTime - _lastLullabyBeat) % _lullabyBeatInterval;
                if (phase < 0) phase += _lullabyBeatInterval;
                float distToCenter = Mathf.Abs(phase - _lullabyBeatInterval * 0.5f);
                float normalizedError = Mathf.Clamp01(distToCenter / (_lullabyBeatInterval * 0.5f));

                float timingWindow = 0.30f; // forgiving for fun, rewards practice
                if (normalizedError < timingWindow)
                {
                    float quality = 1f - (normalizedError / timingWindow); // 0.0-1.0
                    ApplyLullabyRhythmHit(quality);
                    _lastLullabyBeat = pressTime; // lock to player for responsive feel
                }
                else
                {
                    _rhythmCombo = Mathf.Max(0, _rhythmCombo - 0.6f);
                }
            }

            // Keep beat reference alive (prevents drift during no-input)
            if (Time.time - _lastLullabyBeat > _lullabyBeatInterval * 2.2f)
            {
                _lastLullabyBeat = Time.time;
            }
        }

        void ApplyLullabyRhythmHit(float quality)
        {
            if (!_active) return;

            _lullabyRhythmHits++;
            _rhythmCombo = Mathf.Min(12f, _rhythmCombo + 1.15f);

            float shieldBoost = quality * 0.38f + (_rhythmCombo * 0.045f);
            _lullabyShieldStrength = Mathf.Min(4.5f, _lullabyShieldStrength + shieldBoost);

            // Close the freq loop: successful lullaby singing nudges player Hz toward 432 target (playable tuning fantasy)
            float liveHz = ServiceLocator.Combat?.GetPlayerCurrentFrequency() ?? _currentTargetLullabyHz;
            float nudge = (_currentTargetLullabyHz - liveHz) * (0.42f * quality);
            if (CombatBridge.Instance != null)
            {
                CombatBridge.Instance.AdjustPlayerFrequency(nudge);
            }

            // Empower escort + threats
            ApplyRailBossSynergy(quality * 0.9f + 0.1f);

            // Extra damage to nearby threats (rhythm as active defense)
            Vector3 center = _trainProxy != null ? _trainProxy.transform.position : Vector3.Lerp(railStart, railEnd, _progress);
            int hitCount = 0;
            foreach (var t in _activeThreats.ToArray())
            {
                if (t == null) continue;
                if (Vector3.Distance(t.transform.position, center) < 16f)
                {
                    var h = t.GetComponent<RailWraithHealthProxy>();
                    if (h != null)
                    {
                        h.TakeDamage(14f * quality * _lullabyShieldStrength * (1f + _rhythmCombo * 0.03f));
                        hitCount++;
                    }
                }
            }

            // Occasional phase advance on strong rhythm during levi (4-phase climax payoff)
            if (_leviathanPhaseActive && quality > 0.65f && UnityEngine.Random.value < 0.22f)
            {
                AdvanceLeviathanPhase(quality);
            }

            // Haptic (F310) lullaby pulse confirmation
            HapticFeedbackManager.Instance?.PlayLullabyPulse();

            // Full audio manager hook for dynamic layers + perfect stinger
            _moon3RailAudio?.TriggerLullabyTap(quality > 0.75f);

            // 3D/TA: F310 rumble-synced visual particles (golden warmth pulses on train/children, tells story of shared song)
            Vector3 syncPos = _trainProxy != null ? _trainProxy.transform.position + Vector3.up * 1.4f : center;
            ServiceLocator.VFX?.PlayResonancePulse(syncPos, 2f + quality * 4f);

            Debug.Log($"[Moon3 Lullaby Rhythm] HIT q={quality:F2} combo={_rhythmCombo:F0} hits={_lullabyRhythmHits} shield+{shieldBoost:F2} nearThreats={hitCount}");
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

            // [Moon1 HUD stub] ShowBanner("THE 17TH HOUR", "The rails align under the hidden sun. The children sing louder than the dissonance ever was.", 9f);
            ServiceLocator.VFX?.SpawnMoon3TrainTrail(_trainProxy ? _trainProxy.transform.position : railStart, 2.6f);
            // Moon3 audio manager handles rich chime + motif internally via event
            AudioManager.Instance?.PlaySFX2D("Moon3_SeventeenthHourChime", 0.9f);

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
                    // SpectralOrphanAdoption is a static partial class — invoke directly (no AddComponent)
                    SpectralOrphanAdoption.AdoptOrphan("spectral_mid_escort", 35f); // full functional adoption + trust + lullaby
                    ServiceLocator.Lirael?.AddTrust(7.5f);
                    ServiceLocator.Milo?.AddTrust(5f);
                    ServiceLocator.VFX?.SpawnGiantEchoRelease(adoptPos); // reuse golden echo as "found family" burst
                    Debug.Log("[Moon3 R7 Escort] Mid-escort orphan adoption moment — trust payoff delivered.");
                }
            }
        }

        void CreateTrainProxy()
        {
            _trainProxy = new GameObject("SpectralTrain_Proxy_Moon3_R7");
            _trainProxy.transform.position = railStart;
            _trainRenderers.Clear();
            _trainMPB = new MaterialPropertyBlock();
            _lastTrainHealthForVFX = trainMaxHealth;

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
            _trainBody = body;
            _trainRenderers.Add(rend);

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
                _trainRenderers.Add(wr);
            }

            // Golden rail undercarriage + R6 resonance glow rings
            var glow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glow.transform.SetParent(_trainProxy.transform);
            glow.transform.localPosition = Vector3.down * 1.25f;
            glow.transform.localScale = new Vector3(0.55f, 7.8f, 0.38f);
            glow.GetComponent<Renderer>().material.color = new Color(0.96f, 0.87f, 0.28f, 0.72f);
            glow.isStatic = true;
            _trainRenderers.Add(glow.GetComponent<Renderer>());

            // Protection trigger volume
            var col = _trainProxy.AddComponent<SphereCollider>();
            col.radius = 6.2f;
            col.isTrigger = true;

            // R6/R7 perf: everything static where possible
            foreach (var r in _trainProxy.GetComponentsInChildren<Renderer>())
                r.gameObject.isStatic = true;

            // ─── R7 "Compassion & Rails" Spectral Orphan Children Visuals (inside train) ───
            // Three named children (Aria singer, Toren protector, Syl youngest) as glowing proxies
            // Their glow + particle intensity directly reflects lullaby shield + singing success
            _spectralOrphanVisuals = new GameObject[3];
            string[] orphanNames = { "Aria_LullabySinger", "Toren_Protector", "Syl_Youngest" };
            Vector3[] offsets = { new Vector3(-1.6f, 0.9f, -1.8f), new Vector3(1.4f, 0.7f, 0.6f), new Vector3(-0.8f, 0.5f, 2.4f) };
            for (int k = 0; k < 3; k++)
            {
                var child = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                child.name = orphanNames[k];
                child.transform.SetParent(_trainProxy.transform);
                child.transform.localPosition = offsets[k];
                child.transform.localScale = Vector3.one * 0.38f;
                var cr = child.GetComponent<Renderer>();
                cr.material.color = new Color(0.98f, 0.95f, 0.7f, 0.65f);
                cr.material.EnableKeyword("_EMISSION");
                if (cr.material.HasProperty("_EmissionColor")) cr.material.SetColor("_EmissionColor", Color.white * 0.9f);
                child.isStatic = false; // will pulse
                _spectralOrphanVisuals[k] = child;
                _trainRenderers.Add(cr);
            }

            // Initial golden rail trail VFX for story start
            ServiceLocator.VFX?.SpawnMoon3TrainTrail(railStart, 1.3f);
        }

        // ─── R7 Visual Polish: Train damage states + spectral orphan singing visuals (Compassion & Rails story) ───
        void UpdateTrainDamageStates()
        {
            if (_trainProxy == null || _trainRenderers.Count == 0) return;
            float healthNorm = Mathf.Clamp01(_trainHealth / trainMaxHealth);
            float severity = 1f - healthNorm;

            // Emission shift: healthy = bright blue-gold, damaged = angry orange-red, critical = dim + dark
            Color baseEm = Color.Lerp(new Color(0.35f, 0.68f, 0.98f), new Color(0.95f, 0.35f, 0.15f), severity);
            if (healthNorm < 0.35f) baseEm *= 0.4f; // critical fade

            foreach (var rend in _trainRenderers)
            {
                if (rend == null) continue;
                rend.GetPropertyBlock(_trainMPB);
                _trainMPB.SetColor("_EmissionColor", baseEm * (0.7f + (1f - severity) * 0.6f));
                rend.SetPropertyBlock(_trainMPB);
                // Color body darker on damage
                if (rend.gameObject == _trainBody)
                    rend.material.color = Color.Lerp(new Color(0.72f, 0.79f, 0.94f), new Color(0.45f, 0.35f, 0.32f), severity * 0.7f);
            }

            // Spawn damage VFX on significant drops (F310 rumble synced)
            float delta = _lastTrainHealthForVFX - _trainHealth;
            if (delta > 18f && !_trainDamageVFXActive)
            {
                ServiceLocator.VFX?.SpawnRailDamageSparks(_trainProxy.transform.position + Vector3.up * 1.2f, severity);
                HapticFeedbackManager.Instance?.PlayTuningMiss(); // light feedback for hits
                if (severity > 0.65f)
                    HapticFeedbackManager.Instance?.PlayDissonanceCorruptionHit(); // heavy for critical
                _trainDamageVFXActive = true;
            }
            else if (delta < 5f)
            {
                _trainDamageVFXActive = false;
            }
            _lastTrainHealthForVFX = _trainHealth;
        }

        void UpdateSpectralOrphanLullabyVisuals(float freqMatch)
        {
            if (_spectralOrphanVisuals == null || _trainProxy == null) return;
            int kids = SpectralOrphanAdoption.AdoptedCount;
            float shield = _lullabyShieldStrength;
            float singIntensity = Mathf.Clamp01( (shield - 0.75f) / 2.8f + freqMatch * 0.35f + kids * 0.12f );

            // Pulse the 3 child spheres (glow brighter when singing strong)
            for (int k = 0; k < _spectralOrphanVisuals.Length; k++)
            {
                var go = _spectralOrphanVisuals[k];
                if (go == null) continue;
                float pulse = 0.75f + Mathf.Sin(_time * 4.2f + k * 1.7f) * 0.18f * singIntensity;
                go.transform.localScale = Vector3.one * (0.32f + singIntensity * 0.22f) * pulse;
                var cr = go.GetComponent<Renderer>();
                if (cr != null)
                {
                    Color c = Color.Lerp(new Color(0.6f, 0.55f, 0.4f, 0.5f), new Color(1f, 0.96f, 0.65f, 0.95f), singIntensity);
                    cr.material.color = c;
                    cr.material.SetColor("_EmissionColor", c * (1.2f + singIntensity * 1.8f));
                }
            }

            // Periodic orphan singing glow VFX + wind reaction (high shield = compassion calms world)
            if (Time.frameCount % 19 == 0 && kids > 0 && singIntensity > 0.35f)
            {
                Vector3 singPos = _trainProxy.transform.position + Vector3.up * 2.2f;
                ServiceLocator.VFX?.SpawnOrphanLullabyGlow(singPos, kids, singIntensity);
                bool success = freqMatch > 0.55f || shield > 1.8f;
                ServiceLocator.VFX?.SpawnWindElectricReaction(singPos + Vector3.forward * 1.5f, success, singIntensity * 0.8f);
            }
        }

        // R6: Wave spawn with dynamic freq scaling + full companion tells. R7: Levi phases + forks + station synergy
        void SpawnRailWraithWave(float freqMatch)
        {
            int baseCount = 2 + Mathf.Min(_waveIndex, 5);
            int count = Mathf.RoundToInt(baseCount * (0.95f + (1f - freqMatch) * 0.65f));
            Vector3 basePos = Vector3.Lerp(railStart, railEnd, _progress + 0.09f);

            string waveLabel = _waveIndex >= 5 ? "LEVIATHAN ESCALATION" : $"Rail Wraith wave {_waveIndex}";
            // [Moon1 HUD stub] ShowObjective($"{waveLabel}! Shield: {_lullabyShieldStrength:F1}x | Freq match: {freqMatch:P0}");

            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3((i - count * 0.5f) * 4.8f, 0.9f, UnityEngine.Random.Range(-4.2f, 4.2f));
                var wraith = GetPooledOrNewWraithProxy(basePos + offset);
                _activeThreats.Add(wraith);
                RequestDOTSRailWraithSpawn(basePos + offset);
            }

            // Harvester + occasional extra (R7 pooled)
            if (_waveIndex % 2 == 0 || freqMatch < 0.45f)
            {
                var harv = GetPooledOrNewHarvesterProxy(basePos + new Vector3(7.5f, 1.4f, UnityEngine.Random.Range(-2f, 3f)));
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

                // [Moon1 HUD stub] ShowBanner("DISSONANCE LEVIATHAN", "The rails scream. Match the children's frequency to open its heart. Protect the train!", 5f);
                ServiceLocator.VFX?.SpawnLeviathanPhaseVFX(basePos, _waveIndex + _leviathanPhase);
                ServiceLocator.CameraShake?.TriggerShake(0.95f, 0.7f);

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

            AudioManager.Instance?.PlaySFX2D("Moon3_WraithShriek", 0.75f);
            HapticFeedbackManager.Instance?.PlayTuningMiss();
        }

        // R7: Advance Leviathan phases with distinct patterns (GDD 4-phase climax for Moon 3)
        // Phase 0: Approach, 1: TailSweep (pressure), 2: SonicScream (area), 3: CrystalBarrage (adds threats), 4: Purify (death trigger)
        void AdvanceLeviathanPhase(float synergy)
        {
            _leviathanPhase = (_leviathanPhase + 1) % 5;
            _lastLeviPhaseChange = _time;
            if (_leviathanPhase == 4) _leviathanPhase = 1; // loop 1-3 until health<=0 triggers PurifyAndWorldChange

            Vector3 pos = _trainProxy ? _trainProxy.transform.position : Vector3.Lerp(railStart, railEnd, _progress);
            ServiceLocator.VFX?.SpawnLeviathanPhaseVFX(pos, _leviathanPhase + 10);

            Debug.Log($"[Moon3 R7 Leviathan] Phase advanced to {_leviathanPhase} (synergy {synergy:F2})");
            if (_leviathanPhase == 3)
            {
                // Barrage phase: extra threats
                for (int k = 0; k < 2; k++)
                {
                    var extra = GetPooledOrNewHarvesterProxy(pos + UnityEngine.Random.insideUnitSphere * 6f);
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
            ServiceLocator.GameLoop?.QueueRSReward(5.5f, "rail_wraith_kill_r7");
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
                            ServiceLocator.VFX?.SpawnLeviathanPhaseVFX(t.transform.position, 9 + _leviathanPhase);
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
            // [Moon1 HUD stub] ShowBanner("GIANT ECHO FREED", "The children's lullaby shattered the cage. The highlands remember their song. Rails glow forever.", 10f);
            Debug.Log("[Moon3 R7 Leviathan] GIANT ECHO + PERMANENT WORLD CHANGE — victory transforms the zone with deeper VFX.");

            ServiceLocator.VFX?.SpawnGiantEchoRelease(_trainProxy ? _trainProxy.transform.position + Vector3.up * 11f : railEnd);
            ServiceLocator.CameraShake?.TriggerShake(1.65f, 2.1f);

            // R7 stronger permanent world transformation VFX (full 3D/TA delivery)
            ServiceLocator.VFX?.TriggerPermanentGoldenRailsAndCalm(railStart, railEnd);
            for (int i = 0; i < 4; i++)
            {
                Vector3 extra = Vector3.Lerp(railStart, railEnd, 0.3f + i * 0.18f) + Vector3.up * (8 + i * 2);
                ServiceLocator.VFX?.SpawnGiantEchoRelease(extra);
            }

            // 3D/TA: Activate scaffold-placed permanent golden rails victory overlay (if present from Populate)
            var victoryOverlay = GameObject.Find("Moon3_Victory_GoldenRails_Permanent");
            if (victoryOverlay != null) victoryOverlay.SetActive(true);

            SpectralOrphanAdoption.SetGiantEchoFreed(true);
            SpectralOrphanAdoption.SetLeviathanDefeated(true);
            SpectralOrphanAdoption.SetSeventeenthHourEvent("leviathan_purified", true);

            ServiceLocator.Lirael?.AddTrust(18f);
            ServiceLocator.Milo?.AddTrust(11f);
            if (ServiceLocator.Cassian != null) ServiceLocator.Cassian.AddTrust(7f);

            // Permanent world change: golden rail glows + calmed winds + echo marker (R7 enhanced)
            CreatePermanentMoon3VictoryMarkers();

            // Full 3D/TA dramatic permanent transformation (golden rails + calm + orphan glow)
            ServiceLocator.VFX?.TriggerPermanentGoldenRailsAndCalm(railStart, railEnd);
            ServiceLocator.VFX?.SpawnOrphanLullabyGlow(railEnd + Vector3.up * 10f, 5, 2.5f);

            // Extra victory fireworks for the golden rails
            for (int i = 0; i < 6; i++)
            {
                Vector3 pos = Vector3.Lerp(railStart, railEnd, i / 5f) + Vector3.up * (4 + i);
                ServiceLocator.VFX?.SpawnGiantEchoRelease(pos);
                ServiceLocator.VFX?.PlayResonancePulse(pos, 10f + i * 2);
            }

            _activeThreats.Remove(levi);
            Destroy(levi, 0.6f);

            OnLeviathanPurified?.Invoke();
        }

        // R6: Instantiates permanent static objects that survive escort end (world transformation). R7: more markers + wind mgmt
        void CreatePermanentMoon3VictoryMarkers()
        {
            // Golden resonance rail glow strips along entire path (static) — R7 extended + actual rail tracks
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

            // Actual beautiful golden rail tracks (LineRenderer for clean look)
            var goldenRails = new GameObject("GoldenRails_Permanent_Moon3");
            goldenRails.transform.position = Vector3.Lerp(railStart, railEnd, 0.5f);
            var lr = goldenRails.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, railStart + Vector3.up * 0.2f);
            lr.SetPosition(1, railEnd + Vector3.up * 0.2f);
            lr.startWidth = 1.8f;
            lr.endWidth = 1.8f;
            lr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            lr.material.color = new Color(1f, 0.9f, 0.4f, 0.95f);
            lr.material.EnableKeyword("_EMISSION");
            goldenRails.isStatic = true;

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

            // R7 TA: Also fire the full VFX permanent golden + calm (particles + fast travel visual) for richer story
            ServiceLocator.VFX?.TriggerPermanentGoldenRailsAndCalm(railStart, railEnd);
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
                // [Moon1 HUD stub] ShowObjective("Escort complete. The spectral children are safe. The highlands sing again. Rail network expanded.");
                // [Moon1 HUD stub] ShowBanner("Rail Network Awakens", "First grand segment secured. Lullaby Crystal + World's Fair Ticket + Continental Rail hook granted. Found family grows.", 8f);

                ServiceLocator.GameLoop?.QueueRSReward(245f, "moon3_escort_r7_complete");
                ServiceLocator.Lirael?.AddTrust(12f);
                ServiceLocator.Milo?.AddTrust(9f);
                if (ServiceLocator.Cassian != null) ServiceLocator.Cassian.AddTrust(5f);

                // R5 save + R6 quest + World's Fair ticket (Moon 3 live-ops reward) + R7 variants
                SpectralOrphanAdoption.SetEscortCompleted(true);
                ServiceLocator.Quest?.ProgressByType(QuestObjectiveType.CompleteTuning, "rail_escort_moon3_r7");
                ServiceLocator.Quest?.ProgressByType(QuestObjectiveType.CompanionMilestone, "orphan_train_escort");
                // World's Fair ticket on Moon 3 + R7 variants
                SpectralOrphanAdoption.SetSeventeenthHourEvent("worlds_fair_ticket_moon3", true);
                SpectralOrphanAdoption.SetSeventeenthHourEvent("worlds_fair_golden_variant_rail", true);
                SpectralOrphanAdoption.SetSeventeenthHourEvent("rail_success_daily_deal", true); // new daily tied to success
                ServiceLocator.Quest?.ProgressByType(QuestObjectiveType.HiddenDiscovery, "worlds_fair_ticket_moon3");

                ServiceLocator.VFX?.SpawnMoon3TrainTrail(_trainProxy ? _trainProxy.transform.position : railEnd, 2.8f);

                // Victory motif handled by Moon3RailAudioManager (The Aether Remembers)
                AudioManager.Instance?.PlaySFX2D("Moon3_TrainRestored", 0.95f);

                Moon3ContinentalRailFastTravelUnlocked = true; // R7 optional fast travel hook
                SpectralOrphanAdoption.SetSeventeenthHourEvent("post_escort_continental_rail_ready", true);

                if (!_permanentWorldChanged)
                {
                    // Fallback world change even on clean success
                    CreatePermanentMoon3VictoryMarkers();

                    // Activate the scaffold's permanent golden rails overlay for the full transformed world
                    var victoryOverlayFallback = GameObject.Find("Moon3_Victory_GoldenRails_Permanent");
                    if (victoryOverlayFallback != null)
                    {
                        victoryOverlayFallback.SetActive(true);
                        Debug.Log("[Moon3] Permanent golden rails overlay activated - the highlands are forever changed by the children's song.");
                    }
                }

                // R7: Spawn Continental Rail Fast Travel Portal at end (permanent)
                Vector3 portalPos = railEnd + Vector3.up * 2f + (railEnd - railStart).normalized * 8f;
                var portal = new GameObject("ContinentalRail_FastTravel_Portal_Moon3");
                portal.transform.position = portalPos;
                var pCol = portal.AddComponent<SphereCollider>();
                pCol.isTrigger = true;
                pCol.radius = 4f;
                var pRenderer = portal.AddComponent<MeshRenderer>();
                pRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                pRenderer.material.color = new Color(0.6f, 0.9f, 1f, 0.7f);
                // Simple portal visual (torus like)
                var pMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pMesh.transform.SetParent(portal.transform);
                pMesh.transform.localScale = new Vector3(6f, 0.3f, 6f);
                pMesh.GetComponent<Renderer>().material = pRenderer.material;
                Destroy(pMesh.GetComponent<Collider>());

                // Trigger for fast travel (stub - can be expanded to load other zone)
                var trigger = portal.AddComponent<Moon3FastTravelTrigger>();
                trigger.targetZone = "ContinentalRail_Hub"; // or scene name

                // Portal spawn VFX (rich 3D/TA delivery)
                ServiceLocator.VFX?.SpawnGiantEchoRelease(portalPos + Vector3.up * 5f);
                ServiceLocator.VFX?.PlayResonancePulse(portalPos, 20f);
                ServiceLocator.VFX?.SpawnOrphanLullabyGlow(portalPos, 3, 1.8f); // children "escort" the new rail
                ServiceLocator.VFX?.SpawnWindElectricReaction(portalPos, true, 1.2f); // calm wind on unlock

                // Activate the permanent golden overlay (the scaffold's disabled root becomes the transformed world)
                var victoryOverlay = GameObject.Find("Moon3_Victory_GoldenRails_Permanent");
                if (victoryOverlay != null) victoryOverlay.SetActive(true);

                Debug.Log("[Moon3 R7] Continental Rail fast travel portal spawned permanently at end of line.");
            }
            else
            {
                // [Moon1 HUD stub] ShowObjective("The train was overwhelmed. Re-align the rails. The children still believe in you.");
                ServiceLocator.Lirael?.AddTrust(-3.5f);
                ServiceLocator.Milo?.AddTrust(-2f);
            }

            OnEscortComplete?.Invoke(success);

            // Cleanup events (R7 integration hygiene)
            OnEscortComplete -= HandleEscortCompleteVFX;
            OnWaveStarted -= HandleWaveVFX;
            OnSeventeenthHourTriggered -= HandleSeventeenthHourVFX;
            OnLeviathanPurified -= HandleLeviathanPurifiedVFX;

            // Cleanup (pool friendly) — Moon3 audio heart fades gracefully
            if (_escortHUD != null) _escortHUD.Shutdown();
            if (_moon3RailAudio != null) Destroy(_moon3RailAudio.gameObject, 0.1f);
            if (_trainProxy) Destroy(_trainProxy, 2.8f);
            foreach (var t in _activeThreats) if (t) Destroy(t, 0.2f);
            _activeThreats.Clear();
            _wraithProxyPool.Clear();
            _harvesterProxyPool.Clear();
            _stationProxyPool.Clear();

            Debug.Log($"[Moon3 R7 Escort] Complete. Success={success}. 17th={_seventeenthHourActive} WorldChanged={_permanentWorldChanged} FastTravel={Moon3ContinentalRailFastTravelUnlocked}");
        }

        // ─── R7 VFX Event Handlers (full integration with RailEscortController events + F310 rumble sync) ───
        // These make the visuals react live to the "Compassion & Rails" beats: lullaby victory = golden world change
        void HandleEscortCompleteVFX(bool success)
        {
            if (success)
            {
                ServiceLocator.VFX?.SpawnMoon3TrainTrail(_trainProxy ? _trainProxy.transform.position : railEnd, 2.9f);
                ServiceLocator.VFX?.TriggerPermanentGoldenRailsAndCalm(railStart, railEnd);
                HapticFeedbackManager.Instance?.PlayClimaxRumble();
                HapticFeedbackManager.Instance?.PlayLullabyPulse();
            }
            else
            {
                ServiceLocator.VFX?.SpawnWindElectricReaction(_trainProxy ? _trainProxy.transform.position : railStart, false, 1.0f);
                HapticFeedbackManager.Instance?.PlayDissonanceCorruptionHit();
            }
        }

        void HandleWaveVFX(int wave)
        {
            if (_trainProxy == null) return;
            Vector3 p = _trainProxy.transform.position;
            if (wave >= 4) // Levi escalation
            {
                ServiceLocator.VFX?.SpawnLeviathanPhaseVFX(p + Vector3.up * 4f, _leviathanPhase);
                HapticFeedbackManager.Instance?.PlayGiantVeinSurge();
            }
            else if (wave % 2 == 0)
            {
                ServiceLocator.VFX?.SpawnWindElectricReaction(p, false, 0.6f);
            }
        }

        void HandleSeventeenthHourVFX()
        {
            if (_trainProxy == null) return;
            Vector3 p = _trainProxy.transform.position + Vector3.up * 5f;
            ServiceLocator.VFX?.SpawnGiantEchoRelease(p);
            ServiceLocator.VFX?.SpawnOrphanLullabyGlow(p, Mathf.Max(1, SpectralOrphanAdoption.AdoptedCount), 1.4f);
            HapticFeedbackManager.Instance?.PlaySynergyResonanceHarmony();
            // Extra golden trail burst
            ServiceLocator.VFX?.SpawnMoon3TrainTrail(p, 2.4f);
        }

        void HandleLeviathanPurifiedVFX()
        {
            if (_trainProxy == null) return;
            Vector3 p = _trainProxy.transform.position + Vector3.up * 6f;
            ServiceLocator.VFX?.SpawnLeviathanPhaseVFX(p, 4); // triggers purify explosion
            ServiceLocator.VFX?.TriggerPermanentGoldenRailsAndCalm(railStart, railEnd);
            ServiceLocator.VFX?.SpawnOrphanLullabyGlow(p + Vector3.up * 2f, 3, 2.0f);
            HapticFeedbackManager.Instance?.PlayClimaxRumble();
            HapticFeedbackManager.Instance?.PlayPerfectTune();
        }

        // R6 dedicated escort HUD (OnGUI only for Moon3 vertical slice — zero new UI assets). R7: kept for quick testing alongside dedicated HUD.
        void OnGUI()
        {
            if (!_active || _trainProxy == null) return;

            GUI.Box(new Rect(Screen.width / 2 - 245, 14, 490, 138), "ORPHAN TRAIN ESCORT — WINDSWEPT HIGHLANDS (MOON 3 R7)");
            GUI.Label(new Rect(Screen.width / 2 - 225, 36, 450, 20), $"PROGRESS: {_progress * 100f:F0}%  |  TIME: {_time:F0}s / {escortDuration:F0}s  |  TRAIN INTEGRITY: {(_trainHealth / trainMaxHealth * 100f):F0}%");
            GUI.Label(new Rect(Screen.width / 2 - 225, 56, 450, 20), $"LULLABY SHIELD: {_lullabyShieldStrength:F2}x  |  ADOPTED: {SpectralOrphanAdoption.AdoptedCount}  |  FREQ: {_lastFreqMatch:P0} | RHYTHM: {_rhythmCombo:F0} hits={_lullabyRhythmHits}");
            GUI.Label(new Rect(Screen.width / 2 - 225, 76, 450, 20), $"WAVE: {_waveIndex}/10  |  THREATS: {_activeThreats.Count}  |  TARGET Hz: {_currentTargetLullabyHz:F0} | LEVI PHASE: {_leviathanPhase}");

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

        /// <summary>Called when a rail segment is reactivated (Moon 3 puzzle progression).</summary>
        public void OnRailSegmentReactivated(int segmentIndex)
        {
            Debug.Log($"[RailEscort] Rail segment {segmentIndex} reactivated");
            // TODO: Update rail network state, spawn effects
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
