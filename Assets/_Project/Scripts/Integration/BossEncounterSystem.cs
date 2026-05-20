using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Audio;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Boss Encounter System — multi-phase boss fights at Moon climaxes.
    ///
    /// Design per GDD §06 (Combat), §11 (Scripted Climaxes):
    ///   - Each Moon ends with a boss encounter before the climax sequence
    ///   - Bosses have multiple phases with unique mechanics
    ///   - Phase transitions at HP thresholds with cinematic beats
    ///   - Vulnerability windows tied to frequency-matching mechanics
    ///   - RS rewards scale with performance (no-hit bonus, time bonus)
    ///
    /// Boss types:
    ///   - CorruptionTitan (Moon 1-4): brute force + corruption AOE
    ///   - MirrorSovereign (Moon 5-8): reflection/clone mechanics
    ///   - VoidArchitect (Moon 9-12): reality-warping, ley line disruption
    ///   - TrueHistoryGuardian (Moon 13): all mechanics combined
    ///
    /// Performance budget: 2ms (within Combat 2ms budget, takes over from normal combat)
    /// </summary>
    public class BossEncounterSystem : MonoBehaviour
    {
        public static BossEncounterSystem Instance { get; private set; }

        // ─── Events ───
        public event Action<BossDefinition> OnBossSpawned;
        public event Action<int> OnPhaseChanged;          // new phase index
        public event Action<float> OnBossHealthChanged;   // normalized 0-1
        public event Action<BossResult> OnBossDefeated;
        public event Action OnBossFailed;
        public event Action<string> OnBossDialogue;       // dialogue line

        // ─── State ───
        BossDefinition _currentBoss;
        int _currentPhase;
        float _bossHP;
        float _bossMaxHP;
        float _encounterTime;
        int _playerHits;         // hits taken by player
        bool _isActive;
        float _vulnerableTimer;
        bool _isVulnerable;
        float _phaseTransitionTimer;

        // Phase mechanics
        float _attackCooldown;
        float _patternTimer;
        int _patternIndex;

        // Round 4: Frequency puzzle integration + visual sync state (Bosses & Advanced Enemies)
        float _currentTargetFrequency;
        bool _frequencyPuzzleActive;
        float _lastHealthSync;
        GameObject _colossusVisualProxy; // for Mud Colossus dynamic health visuals (procedural)
        GameObject _railWraithVisual;    // procedural/DOTS-style visual proxy for RailWraith 3-phase
        GameObject _sludgeVisual;        // procedural/DOTS-style for SludgeLeviathan 3-phase

        // ─── Public Getters ───
        public bool IsActive => _isActive;
        public float BossHPNormalized => _bossMaxHP > 0 ? _bossHP / _bossMaxHP : 0f;
        public int CurrentPhase => _currentPhase;
        public bool IsVulnerable => _isVulnerable;
        public BossDefinition CurrentBoss => _currentBoss;
        public float CurrentTargetFrequency => _currentTargetFrequency;
        public bool IsFrequencyPuzzleActive => _frequencyPuzzleActive && _isVulnerable;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("BossEncounterSystem");
            DontDestroyOnLoad(go);
            go.AddComponent<BossEncounterSystem>();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Named Boss Lookup ────────────────────────
        static readonly Dictionary<string, int> NamedBossLookup = new()
        {
            { "mud_colossus", 0 },
            { "quartz_defiler", 1 },
            { "spire_breaker", 2 },
            { "iron_corruptor", 3 },
            { "echo_sovereign", 4 },
            { "crystal_phantom", 5 },
            { "fractal_tyrant", 6 },
            { "mirror_empress", 7 },
            { "void_shaper", 8 },
            { "rail_leviathan", 9 },
            { "sludge_leviathan", 10 },
            { "anti_resonance", 11 },
            { "guardian_of_true_history", 12 },
            { "rift_walker", 9 },
            { "ley_devourer", 10 }
        };

        // ─── Start / Stop ────────────────────────────

        /// <summary>Begin a boss encounter by string ID (e.g. "sludge_leviathan").</summary>
        public void SpawnBoss(string bossId)
        {
            if (string.IsNullOrEmpty(bossId))
            {
                Debug.LogWarning("[Boss] SpawnBoss called with null/empty bossId.");
                return;
            }

            string key = bossId.ToLowerInvariant().Replace(' ', '_');
            if (NamedBossLookup.TryGetValue(key, out int moonIndex))
            {
                StartBoss(moonIndex);
            }
            else
            {
                Debug.LogWarning($"[Boss] Unknown bossId: {bossId}. Spawning default.");
                StartBoss(-1); // triggers default case in BuildBossForMoon
            }
        }

        /// <summary>Begin a boss encounter for the given Moon.</summary>
        public void StartBoss(int moonIndex)
        {
            _currentBoss = BuildBossForMoon(moonIndex);
            _bossMaxHP = _currentBoss.totalHP;
            _bossHP = _bossMaxHP;
            _currentPhase = 0;
            _encounterTime = 0f;
            _playerHits = 0;
            _isActive = true;
            _isVulnerable = false;
            _vulnerableTimer = 0f;
            _phaseTransitionTimer = 0f;
            _attackCooldown = 0f;
            _patternTimer = 0f;
            _patternIndex = 0;

            // Round 4 frequency puzzle + visual state reset
            _currentTargetFrequency = 432f;
            _frequencyPuzzleActive = false;
            _lastHealthSync = 1f;
            CleanupBossVisualProxies();

            GameStateManager.Instance?.TransitionTo(GameState.Combat);
            OnBossSpawned?.Invoke(_currentBoss);
            OnBossDialogue?.Invoke(_currentBoss.phases[0].entranceDialogue);

            Debug.Log($"[Boss] {_currentBoss.bossName} spawned — {_currentBoss.phases.Count} phases, {_bossMaxHP} HP");
        }

        /// <summary>Force-abort the boss encounter.</summary>
        public void AbortBoss()
        {
            _isActive = false;
            CleanupBossVisualProxies();
            OnBossFailed?.Invoke();
            GameStateManager.Instance?.ReturnToPrevious();
            Debug.Log("[Boss] Encounter aborted gracefully (player retreat / manual).");
        }

        /// <summary>
        /// Graceful fail with reason (time-out, too many hits, puzzle abort). Called from external or internal timers.
        /// </summary>
        public void FailBoss(string reason)
        {
            if (!_isActive) return;
            _isActive = false;
            CleanupBossVisualProxies();
            OnBossDialogue?.Invoke($"The encounter ends... {reason}");
            OnBossFailed?.Invoke();
            GameStateManager.Instance?.ReturnToPrevious();
            Debug.Log($"[Boss] Encounter failed gracefully: {reason}");
            VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position);
        }

        void CleanupBossVisualProxies()
        {
            if (_colossusVisualProxy != null) { Destroy(_colossusVisualProxy); _colossusVisualProxy = null; }
            if (_railWraithVisual != null) { Destroy(_railWraithVisual); _railWraithVisual = null; }
            if (_sludgeVisual != null) { Destroy(_sludgeVisual); _sludgeVisual = null; }
        }

        /// <summary>
        /// Round 4: Wire frequency puzzle submission into real combat via HarmonicStrike/ResonancePulse hooks.
        /// Call this from CombatBridge when boss is active + vulnerable: match quality drives scaled DealDamage.
        /// </summary>
        public void SubmitFrequencyPuzzle(float submittedFreq, float baseDamageMultiplier = 1f)
        {
            if (!_isActive || !_isVulnerable || !_frequencyPuzzleActive) return;

            float delta = Mathf.Abs(submittedFreq - _currentTargetFrequency);
            float tolerance = 55f; // forgiving but rewarding window for boss puzzle
            float matchQuality = (delta < tolerance) ? Mathf.Clamp01(1f - (delta / tolerance)) : 0f;

            if (matchQuality > 0.05f)
            {
                float damage = 35f * (0.6f + matchQuality * 1.8f) * baseDamageMultiplier;
                DealDamage(damage);

                OnBossDialogue?.Invoke($"Harmonic lock: {matchQuality:P0} match @ {_currentTargetFrequency:F0}Hz");
                HapticFeedbackManager.Instance?.PlayPerfectTune();
                VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 2f);

                // Keep puzzle window open but nudge target slightly on strong hits (dynamic)
                if (matchQuality > 0.7f)
                    _currentTargetFrequency = Mathf.Lerp(_currentTargetFrequency, _currentTargetFrequency + UnityEngine.Random.Range(-18f, 18f), 0.3f);
            }
            else
            {
                OnBossDialogue?.Invoke("Dissonant — retune your strike!");
                // small feedback damage even on miss for tension (graceful)
                DealDamage(4f);
            }
        }

        // ─── Round 4 Boss Visuals & Health Sync (procedural, no new assets) ───

        void SpawnOrUpdateBossVisuals(string phaseName)
        {
            if (_currentBoss == null) return;
            string name = _currentBoss.bossName.ToLowerInvariant();

            Vector3 spawnPos = transform.position + new Vector3(0, 1.2f, 8f); // forward of system for arena feel

            if (name.Contains("mud") || name.Contains("colossus"))
            {
                if (_colossusVisualProxy == null)
                {
                    _colossusVisualProxy = new GameObject("MudColossus_VisualProxy");
                    _colossusVisualProxy.transform.position = spawnPos;
                    var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    body.transform.SetParent(_colossusVisualProxy.transform);
                    body.transform.localScale = new Vector3(2.8f, 3.2f, 2.8f);
                    body.GetComponent<Renderer>().material.color = new Color(0.35f, 0.28f, 0.18f, 1f); // deep mud
                    body.name = "ColossusBody";
                    Debug.Log("[Boss] Mud Colossus procedural visual proxy spawned (post-fountain climax).");
                }
                SyncMudColossusVisuals(BossHPNormalized);
            }
            else if (name.Contains("rail"))
            {
                if (_railWraithVisual == null)
                {
                    _railWraithVisual = new GameObject("RailWraith_VisualProxy");
                    _railWraithVisual.transform.position = spawnPos + new Vector3(4f, 2f, 0);
                    var railBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    railBody.transform.SetParent(_railWraithVisual.transform);
                    railBody.transform.localScale = new Vector3(0.6f, 1.8f, 0.6f);
                    railBody.GetComponent<Renderer>().material.color = new Color(0.25f, 0.22f, 0.3f);
                    Debug.Log("[Boss] RailWraith procedural 3-phase visual spawned.");
                }
                UpdateRailWraithVisualPhase(phaseName);
            }
            else if (name.Contains("sludge"))
            {
                if (_sludgeVisual == null)
                {
                    _sludgeVisual = new GameObject("SludgeLeviathan_VisualProxy");
                    _sludgeVisual.transform.position = spawnPos + new Vector3(-4f, 0.8f, 0);
                    var sludgeBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sludgeBody.transform.SetParent(_sludgeVisual.transform);
                    sludgeBody.transform.localScale = new Vector3(3.5f, 1.6f, 3.5f);
                    sludgeBody.GetComponent<Renderer>().material.color = new Color(0.18f, 0.32f, 0.22f);
                    Debug.Log("[Boss] SludgeLeviathan procedural 3-phase visual spawned.");
                }
                UpdateSludgeLeviathanVisualPhase(phaseName);
            }
        }

        void SyncMudColossusVisuals(float normalizedHP)
        {
            if (_colossusVisualProxy == null) return;
            // Dynamic scale (shrinks as damaged) + emission-like color shift
            float scale = Mathf.Lerp(1.4f, 2.8f, normalizedHP);
            var body = _colossusVisualProxy.transform.Find("ColossusBody");
            if (body != null)
            {
                body.localScale = new Vector3(scale, scale * 1.15f, scale);
                var rend = body.GetComponent<Renderer>();
                if (rend != null)
                {
                    // "Emission" via color brightening on high HP, dark on low (mud cracking)
                    Color c = Color.Lerp(new Color(0.12f, 0.1f, 0.07f), new Color(0.55f, 0.45f, 0.22f), normalizedHP);
                    rend.material.color = c;
                }
            }
            _lastHealthSync = normalizedHP;

            // Occasional procedural VFX pulse on health change (DOTS-like visual reactivity)
            if (Mathf.Abs(normalizedHP - _lastHealthSync) > 0.08f || normalizedHP < 0.4f)
            {
                VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, _colossusVisualProxy.transform.position);
            }
        }

        void UpdateRailWraithVisualPhase(string phaseName)
        {
            if (_railWraithVisual == null) return;
            var rend = _railWraithVisual.GetComponentInChildren<Renderer>();
            if (rend == null) return;
            // 3-phase procedural visuals: phase 0 dark rail, phase1 sparking, phase2 enraged red
            if (phaseName.ToLower().Contains("true") || phaseName.ToLower().Contains("collapse"))
                rend.material.color = new Color(0.7f, 0.15f, 0.1f);
            else if (phaseName.ToLower().Contains("multi") || phaseName.ToLower().Contains("decon"))
                rend.material.color = new Color(0.9f, 0.85f, 0.3f); // sparks
            else
                rend.material.color = new Color(0.25f, 0.22f, 0.3f);
            // Procedural "DOTS" bob for rail motion feel
            _railWraithVisual.transform.position = transform.position + new Vector3(4f + Mathf.Sin(Time.time * 4f) * 0.8f, 2f, 0);
        }

        void UpdateSludgeLeviathanVisualPhase(string phaseName)
        {
            if (_sludgeVisual == null) return;
            var rend = _sludgeVisual.GetComponentInChildren<Renderer>();
            if (rend == null) return;
            // 3-phase: initial murky, mid bubbling, final enraged viscous
            if (phaseName.ToLower().Contains("truth") || phaseName.ToLower().Contains("void"))
                rend.material.color = new Color(0.45f, 0.1f, 0.08f);
            else if (phaseName.ToLower().Contains("demo") || phaseName.ToLower().Contains("decon"))
                rend.material.color = new Color(0.15f, 0.42f, 0.28f); // bubbling green
            else
                rend.material.color = new Color(0.18f, 0.32f, 0.22f);
            // Procedural pulse scale for sludge breathing / DOTS reactivity
            float pulse = 1f + Mathf.Sin(Time.time * 2.2f) * 0.08f * (1f - BossHPNormalized * 0.5f);
            _sludgeVisual.transform.localScale = Vector3.one * pulse;
        }

        void UpdateBossVisuals()
        {
            if (!_isActive) return;
            // Lightweight per-frame procedural animation for RailWraith / SludgeLeviathan (DOTS-feel without DOTS overhead)
            if (_railWraithVisual != null)
            {
                _railWraithVisual.transform.position = transform.position + new Vector3(4f + Mathf.Sin(Time.time * 4.5f) * 1.1f, 2f + Mathf.Cos(Time.time * 1.8f) * 0.4f, Mathf.Sin(Time.time * 1.2f) * 0.6f);
            }
            if (_sludgeVisual != null)
            {
                _sludgeVisual.transform.position = transform.position + new Vector3(-4f, 0.8f + Mathf.Sin(Time.time * 3f) * 0.35f, 0);
            }
            // Colossus already synced on health; occasional idle pulse if low
            if (_colossusVisualProxy != null && BossHPNormalized < 0.35f && Time.frameCount % 18 == 0)
            {
                VFXController.Instance?.PlayEffect(VFXEffect.Spark, _colossusVisualProxy.transform.position);
            }
        }

        void Update()
        {
            if (!_isActive) return;

            _encounterTime += Time.deltaTime;

            // Round 4: Graceful fail handling (timeout / excessive hits)
            if (_encounterTime > (_currentBoss?.parTime ?? 90f) * 2.8f)
            {
                FailBoss("the frequencies grow too wild — time runs out");
                return;
            }
            if (_playerHits > 14)
            {
                FailBoss("overwhelmed by dissonance — retreat and retune");
                return;
            }

            // Phase transition cinematic
            if (_phaseTransitionTimer > 0f)
            {
                _phaseTransitionTimer -= Time.deltaTime;
                return; // Freeze during transition
            }

            UpdateBossAI();
            UpdateVulnerability();
            UpdateBossVisuals();
        }

        // ─── Damage ─────────────────────────────────

        /// <summary>Deal damage to the boss. Only effective during vulnerability.</summary>
        public void DealDamage(float damage)
        {
            if (!_isActive || !_isVulnerable) return;

            _bossHP -= damage;
            OnBossHealthChanged?.Invoke(BossHPNormalized);

            // Round 4: Dynamic health sync for Mud Colossus visuals (scale + emission intensity)
            if (_currentBoss != null && _currentBoss.bossName.Contains("Mud") && _currentBoss.bossName.Contains("Colossus"))
            {
                SyncMudColossusVisuals(BossHPNormalized);
            }

            if (_bossHP <= 0f)
            {
                DefeatBoss();
                return;
            }

            // Check phase transitions
            CheckPhaseTransition();
        }

        /// <summary>Player was hit by boss attack.</summary>
        public void RegisterPlayerHit()
        {
            _playerHits++;
        }

        // ─── AI ──────────────────────────────────────

        void UpdateBossAI()
        {
            if (_currentPhase >= _currentBoss.phases.Count) return;
            var phase = _currentBoss.phases[_currentPhase];

            _attackCooldown -= Time.deltaTime;
            _patternTimer -= Time.deltaTime;

            if (_attackCooldown <= 0f)
            {
                ExecuteAttackPattern(phase);
                _attackCooldown = phase.attackInterval;
            }
        }

        void ExecuteAttackPattern(BossPhase phase)
        {
            if (phase.attackPatterns.Count == 0) return;

            var pattern = phase.attackPatterns[_patternIndex % phase.attackPatterns.Count];
            _patternIndex++;

            float baseDamage = 10f + _currentPhase * 5f;
            var combat = CombatBridge.Instance;
            var playerPos = combat != null ? combat.transform.position : Vector3.zero;

            switch (pattern)
            {
                case BossAttackPattern.Sweep:
                    // Wide 180° cone attack
                    combat?.DamagePlayer(baseDamage, "boss_sweep");
                    VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position);
                    HapticFeedbackManager.Instance?.PlayGolemSpawn();
                    break;

                case BossAttackPattern.Slam:
                    // AOE ground slam centered on boss
                    combat?.DamagePlayer(baseDamage * 1.5f, "boss_slam");
                    VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position);
                    HapticFeedbackManager.Instance?.PlayBuildingEmergence();
                    break;

                case BossAttackPattern.CorruptionWave:
                    // Expanding corruption ring — also applies zone corruption
                    combat?.DamagePlayer(baseDamage * 0.8f, "corruption_wave");
                    CorruptionSystem.Instance?.ApplyCorruption(
                        "boss_arena", _currentPhase * 5f);
                    VFXController.Instance?.PlayEffect(
                        VFXEffect.CorruptionPulse, transform.position);
                    break;

                case BossAttackPattern.MirrorClone:
                    // Spawns a decoy — reduced damage but disorients
                    combat?.DamagePlayer(baseDamage * 0.5f, "mirror_clone");
                    VFXController.Instance?.PlayEffect(
                        VFXEffect.HarmonicCascade, transform.position);
                    break;

                case BossAttackPattern.VoidRift:
                    // Opens a rift that pulls player and deals DOT
                    combat?.DamagePlayer(baseDamage * 1.2f, "void_rift");
                    VFXController.Instance?.PlayEffect(
                        VFXEffect.AetherVortex, transform.position);
                    break;

                case BossAttackPattern.FrequencyJam:
                    // Disables tuning for 5 seconds + minor damage
                    combat?.DamagePlayer(baseDamage * 0.3f, "freq_jam");
                    HapticFeedbackManager.Instance?.PlayGolemSpawn();
                    break;

                case BossAttackPattern.LeyLineSever:
                    // Severs a ley line node and deals damage
                    combat?.DamagePlayer(baseDamage * 0.6f, "ley_sever");
                    Core.LeyLineManager.Instance?.SeverNode(0);
                    VFXController.Instance?.PlayEffect(
                        VFXEffect.Spark, transform.position);
                    break;

                case BossAttackPattern.Enrage:
                    // Boss speeds up — halve attack interval for this phase
                    _attackCooldown *= 0.5f;
                    VFXController.Instance?.PlayEffect(
                        VFXEffect.CorruptionPulse, transform.position);
                    break;
            }

            bool dealtDamage = pattern != BossAttackPattern.Enrage;
            if (dealtDamage)
                RegisterPlayerHit();

            // Audio
            Audio.AudioManager.Instance?.PlayTone(180f, 0.5f);
        }

        void UpdateVulnerability()
        {
            if (_isVulnerable)
            {
                _vulnerableTimer -= Time.deltaTime;
                if (_vulnerableTimer <= 0f)
                {
                    _isVulnerable = false;
                    _frequencyPuzzleActive = false; // puzzle window closed
                }
            }
            else
            {
                // Periodically become vulnerable (frequency-matching window)
                if (_currentPhase < _currentBoss.phases.Count)
                {
                    var phase = _currentBoss.phases[_currentPhase];
                    _vulnerableTimer -= Time.deltaTime;
                    if (_vulnerableTimer <= -phase.invulnerableDuration)
                    {
                        _isVulnerable = true;
                        _vulnerableTimer = phase.vulnerableDuration;
                        _frequencyPuzzleActive = true;

                        // Round 4: Deepened frequency puzzle — assign target on each vuln window
                        // Boss-specific ranges for flavor (Mud low earth, Rail high rail hum, Sludge mid sludge gurgle)
                        float bossBase = 280f;
                        if (_currentBoss != null)
                        {
                            if (_currentBoss.bossName.Contains("Mud") || _currentBoss.bossName.Contains("Colossus")) bossBase = 174f;
                            else if (_currentBoss.bossName.Contains("Rail") || _currentBoss.bossName.Contains("Leviathan")) bossBase = 210f;
                            else if (_currentBoss.bossName.Contains("Sludge")) bossBase = 155f;
                        }
                        _currentTargetFrequency = bossBase + UnityEngine.Random.Range(-35f, 95f);

                        OnBossDialogue?.Invoke($"The boss staggers! Strike now — target ~{_currentTargetFrequency:F0} Hz!");
                        HapticFeedbackManager.Instance?.PlayCombatHit();
                        AudioManager.Instance?.PlayTone(528f, 0.3f);

                        // Spawn/refresh procedural visuals for advanced bosses (RailWraith + SludgeLeviathan 3-phase + Mud Colossus)
                        SpawnOrUpdateBossVisuals(phase.phaseName);
                    }
                }
            }
        }

        // ─── Phase Transitions ───────────────────────

        void CheckPhaseTransition()
        {
            if (_currentPhase >= _currentBoss.phases.Count - 1) return;

            float nextThreshold = _currentBoss.phases[_currentPhase].hpThresholdToAdvance;
            if (BossHPNormalized <= nextThreshold)
            {
                _currentPhase++;
                _phaseTransitionTimer = 2f; // 2s cinematic pause
                _isVulnerable = false;
                _patternIndex = 0;

                var newPhase = _currentBoss.phases[_currentPhase];
                OnPhaseChanged?.Invoke(_currentPhase);
                OnBossDialogue?.Invoke(newPhase.entranceDialogue);

                // VFX burst on phase change
                VFXController.Instance?.PlayEffect(
                    VFXEffect.HarmonicCascade, transform.position);
                AdaptiveMusicController.Instance?.PlayZoneShift();
                HapticFeedbackManager.Instance?.PlayGolemSpawn();

                Debug.Log($"[Boss] Phase {_currentPhase + 1}: {newPhase.phaseName}");

                // Refresh procedural visuals for Rail/Sludge 3-phase behavior on phase shift
                SpawnOrUpdateBossVisuals(newPhase.phaseName);
            }
        }

        // ─── Defeat ─────────────────────────────────

        void DefeatBoss()
        {
            _isActive = false;
            _bossHP = 0f;

            // Score calculation
            float timeBonus = Mathf.Clamp01(1f - _encounterTime / (_currentBoss.parTime * 2f));
            float noHitBonus = _playerHits == 0 ? 0.5f : 0f;
            float performanceScore = 0.5f + timeBonus * 0.25f + noHitBonus;

            float rsReward = _currentBoss.baseRSReward * performanceScore;
            AetherFieldManager.Instance?.AddResonanceScore(rsReward);

            var result = new BossResult
            {
                bossName = _currentBoss.bossName,
                encounterTime = _encounterTime,
                playerHitsReceived = _playerHits,
                performanceScore = performanceScore,
                rsRewarded = rsReward,
                noHitClear = _playerHits == 0
            };

            OnBossDefeated?.Invoke(result);
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.DefeatBoss, _currentBoss.bossName);

            // Defeat VFX / audio / haptics
            VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position);
            VFXController.Instance?.TriggerZoneComplete();
            AudioManager.Instance?.PlaySFX("BossDefeat", transform.position);
            AdaptiveMusicController.Instance?.ExitCombat();
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.BossDefeat);
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            EconomySystem.Instance?.AddCurrency(CurrencyType.AetherShards, Mathf.RoundToInt(rsReward / 5f));

            Debug.Log($"[Boss] {_currentBoss.bossName} DEFEATED! Score: {performanceScore:P0}, RS: {rsReward:F0}");

            CleanupBossVisualProxies();

            // Restore ley lines severed during fight
            // (handled by ClimaxSequenceSystem EnvironmentShift)
        }

        // ─── Boss Factory ────────────────────────────

        static BossDefinition BuildBossForMoon(int moonIndex)
        {
            return moonIndex switch
            {
                0 => BuildCorruptionTitan("Mud Colossus", 500f, 15f, 60f),
                1 => BuildCorruptionTitan("Quartz Defiler", 700f, 20f, 75f),
                2 => BuildCorruptionTitan("Spire Breaker", 900f, 22f, 80f),
                3 => BuildCorruptionTitan("Iron Corruptor", 1200f, 28f, 90f),
                4 => BuildMirrorSovereign("Echo Sovereign", 1000f, 25f, 90f),
                5 => BuildMirrorSovereign("Crystal Phantom", 1300f, 30f, 100f),
                6 => BuildMirrorSovereign("Fractal Tyrant", 1500f, 32f, 110f),
                7 => BuildMirrorSovereign("Mirror Empress", 1800f, 35f, 120f),
                8 => BuildVoidArchitect("Void Shaper", 1600f, 30f, 120f),
                9 => BuildVoidArchitect("Rift Walker", 2000f, 35f, 130f),
                10 => BuildVoidArchitect("Ley Devourer", 2200f, 38f, 140f),
                11 => BuildVoidArchitect("Anti-Resonance", 2500f, 42f, 150f),
                12 => BuildTrueHistoryGuardian(),
                _ => BuildCorruptionTitan("Unnamed Boss", 500f, 15f, 60f)
            };
        }

        static BossDefinition BuildCorruptionTitan(string name, float hp, float rsReward, float parTime)
        {
            return new BossDefinition
            {
                bossName = name,
                bossType = BossType.CorruptionTitan,
                totalHP = hp,
                baseRSReward = rsReward,
                parTime = parTime,
                phases = new List<BossPhase>
                {
                    new()
                    {
                        phaseName = "Awakening",
                        entranceDialogue = $"The {name} rises from corrupted earth!",
                        hpThresholdToAdvance = 0.5f,
                        attackInterval = 3f,
                        vulnerableDuration = 2f,
                        invulnerableDuration = 4f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.Sweep, BossAttackPattern.Slam, BossAttackPattern.CorruptionWave }
                    },
                    new()
                    {
                        phaseName = "Frenzy",
                        entranceDialogue = "The corruption surges! The titan enters a frenzy!",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 2f,
                        vulnerableDuration = 1.5f,
                        invulnerableDuration = 5f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.Slam, BossAttackPattern.CorruptionWave, BossAttackPattern.Enrage, BossAttackPattern.Sweep }
                    }
                }
            };
        }

        static BossDefinition BuildMirrorSovereign(string name, float hp, float rsReward, float parTime)
        {
            return new BossDefinition
            {
                bossName = name,
                bossType = BossType.MirrorSovereign,
                totalHP = hp,
                baseRSReward = rsReward,
                parTime = parTime,
                phases = new List<BossPhase>
                {
                    new()
                    {
                        phaseName = "Reflection",
                        entranceDialogue = $"The {name} materializes from shattered mirrors!",
                        hpThresholdToAdvance = 0.6f,
                        attackInterval = 2.5f,
                        vulnerableDuration = 1.5f,
                        invulnerableDuration = 4f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.MirrorClone, BossAttackPattern.Sweep, BossAttackPattern.FrequencyJam }
                    },
                    new()
                    {
                        phaseName = "Multiplication",
                        entranceDialogue = "Mirror images splinter across the arena!",
                        hpThresholdToAdvance = 0.3f,
                        attackInterval = 2f,
                        vulnerableDuration = 1.2f,
                        invulnerableDuration = 5f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.MirrorClone, BossAttackPattern.MirrorClone, BossAttackPattern.Slam, BossAttackPattern.FrequencyJam }
                    },
                    new()
                    {
                        phaseName = "True Form",
                        entranceDialogue = "All mirrors shatter! The sovereign reveals its true frequency!",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 1.5f,
                        vulnerableDuration = 2f,
                        invulnerableDuration = 3f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.Sweep, BossAttackPattern.Slam, BossAttackPattern.Enrage }
                    }
                }
            };
        }

        static BossDefinition BuildVoidArchitect(string name, float hp, float rsReward, float parTime)
        {
            return new BossDefinition
            {
                bossName = name,
                bossType = BossType.VoidArchitect,
                totalHP = hp,
                baseRSReward = rsReward,
                parTime = parTime,
                phases = new List<BossPhase>
                {
                    new()
                    {
                        phaseName = "Construction",
                        entranceDialogue = $"The {name} tears open the fabric of the zone!",
                        hpThresholdToAdvance = 0.65f,
                        attackInterval = 3f,
                        vulnerableDuration = 1.5f,
                        invulnerableDuration = 5f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.VoidRift, BossAttackPattern.LeyLineSever, BossAttackPattern.Sweep }
                    },
                    new()
                    {
                        phaseName = "Deconstruction",
                        entranceDialogue = "Reality warps! The architect unmakes the buildings around you!",
                        hpThresholdToAdvance = 0.3f,
                        attackInterval = 2f,
                        vulnerableDuration = 1.2f,
                        invulnerableDuration = 4f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.VoidRift, BossAttackPattern.LeyLineSever, BossAttackPattern.CorruptionWave, BossAttackPattern.FrequencyJam }
                    },
                    new()
                    {
                        phaseName = "Void Collapse",
                        entranceDialogue = "The void collapses inward! All frequencies converge!",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 1.5f,
                        vulnerableDuration = 2.5f,
                        invulnerableDuration = 3f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.Slam, BossAttackPattern.Enrage, BossAttackPattern.VoidRift, BossAttackPattern.CorruptionWave }
                    }
                }
            };
        }

        static BossDefinition BuildTrueHistoryGuardian()
        {
            return new BossDefinition
            {
                bossName = "Guardian of True History",
                bossType = BossType.TrueHistoryGuardian,
                totalHP = 5000f,
                baseRSReward = 100f,
                parTime = 180f,
                phases = new List<BossPhase>
                {
                    new()
                    {
                        phaseName = "The Burial",
                        entranceDialogue = "You dare unbury what was hidden? I am the seal. I am the silence. I am the lie made manifest!",
                        hpThresholdToAdvance = 0.75f,
                        attackInterval = 2.5f,
                        vulnerableDuration = 1.5f,
                        invulnerableDuration = 5f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.CorruptionWave, BossAttackPattern.Sweep, BossAttackPattern.Slam }
                    },
                    new()
                    {
                        phaseName = "The Demolition",
                        entranceDialogue = "World Fairs! Grand Exhibitions! And then... the wrecking balls. I remember every brick that fell!",
                        hpThresholdToAdvance = 0.5f,
                        attackInterval = 2f,
                        vulnerableDuration = 1.2f,
                        invulnerableDuration = 4f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.MirrorClone, BossAttackPattern.FrequencyJam, BossAttackPattern.LeyLineSever, BossAttackPattern.Slam }
                    },
                    new()
                    {
                        phaseName = "The Erasure",
                        entranceDialogue = "History rewrites itself! The frequency of forgetting grows louder!",
                        hpThresholdToAdvance = 0.25f,
                        attackInterval = 1.5f,
                        vulnerableDuration = 1f,
                        invulnerableDuration = 5f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.VoidRift, BossAttackPattern.CorruptionWave, BossAttackPattern.MirrorClone, BossAttackPattern.Enrage }
                    },
                    new()
                    {
                        phaseName = "The Truth",
                        entranceDialogue = "No... the resonance... it's too strong. You carry all thirteen frequencies. The truth... cannot be buried forever!",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 1f,
                        vulnerableDuration = 3f,
                        invulnerableDuration = 2f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.Slam, BossAttackPattern.Sweep, BossAttackPattern.Enrage }
                    }
                }
            };
        }
    }

    // ─── Data Types ──────────────────────────────

    public enum BossType : byte
    {
        CorruptionTitan = 0,
        MirrorSovereign = 1,
        VoidArchitect = 2,
        TrueHistoryGuardian = 3
    }

    public enum BossAttackPattern : byte
    {
        Sweep = 0,
        Slam = 1,
        CorruptionWave = 2,
        MirrorClone = 3,
        VoidRift = 4,
        FrequencyJam = 5,
        LeyLineSever = 6,
        Enrage = 7
    }

    [Serializable]
    public class BossDefinition
    {
        public string bossName;
        public BossType bossType;
        public float totalHP;
        public float baseRSReward;
        public float parTime; // seconds for "par" clear time
        public List<BossPhase> phases;
    }

    [Serializable]
    public class BossPhase
    {
        public string phaseName;
        public string entranceDialogue;
        public float hpThresholdToAdvance; // normalized HP to trigger next phase
        public float attackInterval;       // seconds between attacks
        public float vulnerableDuration;   // seconds vulnerable
        public float invulnerableDuration; // seconds invulnerable
        public List<BossAttackPattern> attackPatterns;
    }

    [Serializable]
    public class BossResult
    {
        public string bossName;
        public float encounterTime;
        public int playerHitsReceived;
        public float performanceScore; // 0-1
        public float rsRewarded;
        public bool noHitClear;
    }
}
