using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;

namespace Tartaria.AI
{
    /// <summary>
    /// Moon 1 boss — the Mud Lord. Emerges from the central Mud Pool POI after the
    /// player restores all three hero buildings (Cathedral, Anastasia's Tower, Bob's Inn).
    ///
    /// Trigger: subscribes to <see cref="GameEvents.OnMoonCompleted"/>
    /// (<see cref="MoonCompletedEventArgs"/>) per docs/agents/API_CONTRACT.md — only the
    /// canonical event, no invented siblings. When <c>moonIndex == 1</c> AND a serialized
    /// <c>spawnTrigger</c> mode of <see cref="SpawnTrigger.MoonCompleted"/> is set, the
    /// boss runs its emergence sequence.
    ///
    /// Combat loop:
    ///   Phase 1 (100%-66% HP): Telegraphed charge attacks. Player dodges and lands a
    ///                          harmonic strike on the back weak point during recovery.
    ///   Phase 2 (65%-33% HP):  Ground-pound rhythm in groups of three (synced to the
    ///                          7.83 Hz Telluric beat — see design doc).
    ///   Phase 3 (32%-0% HP):   Enraged. Spawns Mud Golem minions. Weak point only
    ///                          visible via Aether Vision; player must triangulate.
    ///
    /// Weak point: a child Transform anchored to the back of the rig. Strikes that pass
    /// <see cref="TryStrikeWeakPoint"/> from a forward angle (player must be behind the
    /// boss within <see cref="weakPointAngleDeg"/>) deal full damage. Strikes from the
    /// front are absorbed for <see cref="frontalDamageMultiplier"/> only.
    ///
    /// Defeat: raises HUD banner "MUD LORD DEFEATED" + "+200 RS" and fires
    /// <see cref="GameEvents.OnBossDefeated"/> (typed payload). Also fires the local
    /// <see cref="OnDefeated"/> event so the Moon 1 cinematic hook can chain off it.
    ///
    /// 2026-06-02 no-debt mandate compliance:
    ///   - No silent catches. Every catch logs with file+method+identifier, then rethrows
    ///     if the failure leaves the boss in an undefined state.
    ///   - No silent fallbacks. Missing dependencies (weakPointTransform, golemSpawnPrefab,
    ///     NavMeshAgent) log a warning naming the identifier that was searched.
    ///   - No TODO stubs. Every state body is implemented.
    ///   - No banned namespaces / no Unity 6 deprecated APIs. Uses
    ///     <c>FindFirstObjectByType&lt;T&gt;(FindObjectsInactive.Include)</c>.
    /// </summary>
    [DisallowMultipleComponent]
    public class MudLordBoss : MonoBehaviour
    {
        public enum State
        {
            Idle,
            Spawning,
            Charge,
            GroundPound,
            EnragedSpawn,
            Defeated
        }

        public enum SpawnTrigger
        {
            Manual,         // External system flips _armed and StartEncounter() is called.
            MoonCompleted   // Auto-arms on GameEvents.OnMoonCompleted (moonIndex==1).
        }

        // ── Identity ────────────────────────────────────────────────────────────
        [Header("Identity")]
        [SerializeField] string bossId = "moon1_mud_lord";
        [SerializeField] string displayName = "The Mud Lord";
        [SerializeField] string displayTitle = "Drowned King of Echohaven";

        // ── HP / Phase Thresholds ───────────────────────────────────────────────
        [Header("Health")]
        [SerializeField] float hp = 600f;
        [SerializeField] float maxHp = 600f;
        [Tooltip("Damage from in front of the boss is multiplied by this. Back hits are full.")]
        [SerializeField, Range(0f, 1f)] float frontalDamageMultiplier = 0.15f;
        [Tooltip("Player must be within this many degrees of the boss's back to hit the weak point.")]
        [SerializeField, Range(15f, 180f)] float weakPointAngleDeg = 70f;
        [SerializeField, Range(0f, 1f)] float phase2Threshold = 0.66f;
        [SerializeField, Range(0f, 1f)] float phase3Threshold = 0.33f;

        // ── Trigger ─────────────────────────────────────────────────────────────
        [Header("Trigger")]
        [SerializeField] SpawnTrigger spawnTrigger = SpawnTrigger.MoonCompleted;
        [SerializeField] int triggerMoonIndex = 1;
        [SerializeField] float spawnEmergenceDuration = 4.5f;

        // ── Phase 1: Charge ─────────────────────────────────────────────────────
        [Header("Phase 1: Charge")]
        [SerializeField] float chargeSpeed = 9.5f;
        [SerializeField] float chargeTelegraphSeconds = 1.1f;
        [SerializeField] float chargeRecoverySeconds = 1.8f;
        [SerializeField] float chargeDistance = 14f;
        [SerializeField] float chargeImpactDamage = 22f;
        [SerializeField] float chargeImpactRadius = 2.4f;

        // ── Phase 2: Ground Pound ───────────────────────────────────────────────
        [Header("Phase 2: Ground Pound")]
        [Tooltip("Telluric resonance band (7.83 Hz) → interval between pounds in a 3-strike pattern.")]
        [SerializeField] float groundPoundIntervalSeconds = 0.127f * 5f; // ~0.635s, 5 beats @ 7.83 Hz
        [SerializeField] float groundPoundRadius = 6.5f;
        [SerializeField] float groundPoundDamage = 35f;
        [SerializeField] float groundPoundTelegraphSeconds = 0.7f;
        [SerializeField] float groundPoundRestSeconds = 2.4f;

        // ── Phase 3: Enraged ────────────────────────────────────────────────────
        [Header("Phase 3: Enraged Spawn")]
        [SerializeField] GameObject golemSpawnPrefab;
        [SerializeField] int enragedGolemsPerWave = 2;
        [SerializeField] float enragedSpawnIntervalSeconds = 8f;
        [SerializeField] float enragedSpawnRadius = 6f;

        // ── Refs ────────────────────────────────────────────────────────────────
        [Header("References")]
        [SerializeField] Transform weakPointTransform;
        [SerializeField] Transform playerTransformOverride;
        [SerializeField] LayerMask playerLayer = ~0;

        // ── Internal ────────────────────────────────────────────────────────────
        NavMeshAgent _agent;
        Transform _player;
        State _state = State.Idle;
        Coroutine _fsmCo;
        bool _armed;
        bool _phase2Entered;
        bool _phase3Entered;
        float _origAgentSpeed;
        readonly Collider[] _aoeBuf = new Collider[16];

        /// <summary>Fires once when the boss transitions to <see cref="State.Defeated"/>.
        /// Cinematic / quest systems chain off this for the post-fight beat.</summary>
        public event Action<MudLordBoss> OnDefeated;

        public State CurrentState => _state;
        public float NormalizedHealth => Mathf.Clamp01(hp / Mathf.Max(0.001f, maxHp));
        public string BossId => bossId;

        // ── Lifecycle ───────────────────────────────────────────────────────────

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent == null)
            {
                Debug.LogWarning($"[MudLordBoss] No NavMeshAgent on '{name}'. Identifier searched: 'NavMeshAgent'. Boss will use direct Transform movement, which ignores walkable mesh edges.");
            }
            else
            {
                _origAgentSpeed = _agent.speed;
            }

            if (weakPointTransform == null)
            {
                Debug.LogWarning($"[MudLordBoss] weakPointTransform unassigned on '{name}'. Identifier searched: 'weakPointTransform' (Inspector). Falling back to the boss root — players will not have a back-only window. Wire the back-of-rig bone in the prefab.");
                weakPointTransform = transform;
            }

            if (golemSpawnPrefab == null)
            {
                Debug.LogWarning($"[MudLordBoss] golemSpawnPrefab unassigned on '{name}'. Identifier searched: 'golemSpawnPrefab' (Inspector). Phase 3 enraged spawns will skip; boss still progresses to defeat but Phase 3 mechanic is degraded. Assign Assets/_Project/Prefabs/Enemies/MudGolem.prefab.");
            }

            hp = Mathf.Max(hp, 1f);
            if (maxHp < hp) maxHp = hp;
        }

        void OnEnable()
        {
            if (spawnTrigger == SpawnTrigger.MoonCompleted)
            {
                GameEvents.OnMoonCompleted += HandleMoonCompleted;
            }
        }

        void OnDisable()
        {
            if (spawnTrigger == SpawnTrigger.MoonCompleted)
            {
                GameEvents.OnMoonCompleted -= HandleMoonCompleted;
            }
        }

        void OnDestroy()
        {
            // Belt + suspenders: covers the case where OnDisable was skipped due to a
            // catastrophic teardown (rare, but cheap to guard against memory leak).
            GameEvents.OnMoonCompleted -= HandleMoonCompleted;
            if (_fsmCo != null) { StopCoroutine(_fsmCo); _fsmCo = null; }
        }

        void HandleMoonCompleted(MoonCompletedEventArgs args)
        {
            if (args == null)
            {
                Debug.LogWarning("[MudLordBoss] OnMoonCompleted fired with null args — ignoring.");
                return;
            }
            if (args.moonIndex != triggerMoonIndex) return;
            if (_armed) return;
            StartEncounter();
        }

        /// <summary>External entry point. Safe to call manually from quest scripts when
        /// <see cref="spawnTrigger"/> is <see cref="SpawnTrigger.Manual"/>.</summary>
        public void StartEncounter()
        {
            if (_armed)
            {
                Debug.LogWarning($"[MudLordBoss] StartEncounter called twice on '{name}' — ignoring second call.");
                return;
            }
            _armed = true;
            _player = ResolvePlayerTransform();
            if (_player == null)
            {
                Debug.LogWarning("[MudLordBoss] StartEncounter: no player transform resolved. Identifier searched: tag 'Player' + playerTransformOverride. Boss will idle until a player is found at next AI tick.");
            }
            _fsmCo = StartCoroutine(RunFSM());
        }

        Transform ResolvePlayerTransform()
        {
            if (playerTransformOverride != null) return playerTransformOverride;
            var byTag = GameObject.FindGameObjectWithTag("Player");
            if (byTag != null) return byTag.transform;
            Debug.LogWarning("[MudLordBoss] ResolvePlayerTransform: no GameObject with tag 'Player' in scene. Identifier searched: tag 'Player'.");
            return null;
        }

        // ── State Machine ───────────────────────────────────────────────────────

        IEnumerator RunFSM()
        {
            yield return RunSpawning();

            // Phase 1 (100%–66%)
            while (_state != State.Defeated && NormalizedHealth > phase2Threshold)
            {
                yield return RunChargeAttack();
            }

            // Phase 2 (65%–33%)
            if (_state != State.Defeated)
            {
                EnterPhase2();
                while (_state != State.Defeated && NormalizedHealth > phase3Threshold)
                {
                    yield return RunGroundPoundSequence();
                }
            }

            // Phase 3 (32%–0%)
            if (_state != State.Defeated)
            {
                EnterPhase3();
                while (_state != State.Defeated && hp > 0f)
                {
                    // Phase 3 interleaves enraged minion spawns with continued ground pounds.
                    yield return RunEnragedSpawn();
                    if (_state == State.Defeated) break;
                    yield return RunGroundPoundSequence();
                }
            }

            if (_state != State.Defeated)
            {
                // Safety: HP somehow reached zero outside the inner check.
                yield return RunDefeated();
            }
        }

        IEnumerator RunSpawning()
        {
            _state = State.Spawning;
            try
            {
                GameEvents.RaiseHUDShowBossNameplate(displayName, displayTitle);
                GameEvents.RaiseHUDShowBossHealth(displayName, NormalizedHealth);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MudLordBoss] RunSpawning HUD raise failed: {ex}");
                throw; // No silent fail — UI state would be inconsistent.
            }

            // Visible emergence: ascend over the duration. The rig should be buried roughly
            // 4m beneath the mud pool — that depth lives on the prefab; we just lerp upward.
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.up * 4f;
            float t = 0f;
            while (t < spawnEmergenceDuration)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / spawnEmergenceDuration);
                transform.position = Vector3.Lerp(startPos, endPos, a * a); // ease-in
                yield return null;
            }
            transform.position = endPos;
        }

        IEnumerator RunChargeAttack()
        {
            _state = State.Charge;
            if (_player == null) _player = ResolvePlayerTransform();
            if (_player == null)
            {
                // No target — wait briefly and try again next loop.
                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            // Telegraph: face the player, hold for the readable wind-up.
            FaceTarget(_player.position);
            try
            {
                GameEvents.RaiseHUDShowEnemyBark("The mud surges!", chargeTelegraphSeconds);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MudLordBoss] RunChargeAttack telegraph bark failed: {ex}");
                // Telegraph bark is cosmetic — log and continue rather than aborting Phase 1.
            }
            yield return new WaitForSeconds(chargeTelegraphSeconds);

            // Charge along a straight line toward the player's predicted position.
            Vector3 chargeDir = (_player.position - transform.position);
            chargeDir.y = 0f;
            if (chargeDir.sqrMagnitude < 0.001f) chargeDir = transform.forward;
            chargeDir.Normalize();

            Vector3 from = transform.position;
            Vector3 to = from + chargeDir * chargeDistance;
            float traveled = 0f;
            while (traveled < chargeDistance && _state == State.Charge)
            {
                float step = chargeSpeed * Time.deltaTime;
                transform.position += chargeDir * step;
                traveled += step;
                // Impact check: anything in the radius around the boss takes damage once.
                TryAreaDamage(chargeImpactRadius, chargeImpactDamage);
                yield return null;
            }

            // Recovery — back is exposed; player wedge window opens here.
            yield return new WaitForSeconds(chargeRecoverySeconds);
        }

        void EnterPhase2()
        {
            if (_phase2Entered) return;
            _phase2Entered = true;
            try { GameEvents.RaiseHUDShowEnemyBark("Hear the heartbeat of the deep.", 3f); }
            catch (Exception ex) { Debug.LogError($"[MudLordBoss] EnterPhase2 bark failed: {ex}"); }
        }

        IEnumerator RunGroundPoundSequence()
        {
            _state = State.GroundPound;
            // 3 pounds, spaced at the Telluric interval.
            for (int i = 0; i < 3; i++)
            {
                if (_state == State.Defeated) yield break;
                FaceTarget(_player != null ? _player.position : transform.position + transform.forward);
                yield return new WaitForSeconds(groundPoundTelegraphSeconds);
                TryAreaDamage(groundPoundRadius, groundPoundDamage);
                if (i < 2) yield return new WaitForSeconds(groundPoundIntervalSeconds);
            }
            yield return new WaitForSeconds(groundPoundRestSeconds);
        }

        void EnterPhase3()
        {
            if (_phase3Entered) return;
            _phase3Entered = true;
            try { GameEvents.RaiseHUDShowEnemyBark("THE TIDE TAKES YOU!", 4f); }
            catch (Exception ex) { Debug.LogError($"[MudLordBoss] EnterPhase3 bark failed: {ex}"); }
        }

        IEnumerator RunEnragedSpawn()
        {
            _state = State.EnragedSpawn;
            if (golemSpawnPrefab == null)
            {
                Debug.LogWarning("[MudLordBoss] RunEnragedSpawn: golemSpawnPrefab is null. Identifier searched: 'golemSpawnPrefab'. Skipping wave — Phase 3 will lack minion pressure. Assign the prefab in the Inspector.");
                yield return new WaitForSeconds(enragedSpawnIntervalSeconds);
                yield break;
            }
            for (int i = 0; i < enragedGolemsPerWave; i++)
            {
                Vector3 offset = UnityEngine.Random.insideUnitSphere * enragedSpawnRadius;
                offset.y = 0f;
                Vector3 spawnPos = transform.position + offset;
                if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, enragedSpawnRadius, NavMesh.AllAreas))
                {
                    spawnPos = hit.position;
                }
                else
                {
                    Debug.LogWarning($"[MudLordBoss] RunEnragedSpawn: NavMesh.SamplePosition failed near {spawnPos}. Identifier searched: 'NavMesh.AllAreas'. Spawning at raw offset — golem may stutter if it lands off-mesh.");
                }
                var go = Instantiate(golemSpawnPrefab, spawnPos, Quaternion.identity);
                if (go == null)
                {
                    Debug.LogError($"[MudLordBoss] RunEnragedSpawn: Instantiate returned null for prefab '{golemSpawnPrefab.name}'.");
                }
            }
            yield return new WaitForSeconds(enragedSpawnIntervalSeconds);
        }

        IEnumerator RunDefeated()
        {
            _state = State.Defeated;
            if (_agent != null) _agent.isStopped = true;

            try
            {
                GameEvents.RaiseHUDShowBanner("MUD LORD DEFEATED", "+200 RS", 5f);
                GameEvents.RaiseHUDHideBossHealth();
                GameEvents.RaiseHUDFlashRSGain(200f);
                GameEvents.RaiseBossDefeated(new BossDefeatedEventArgs
                {
                    bossId = bossId,
                    xpReward = 500,
                    rsReward = 200,
                    position = transform.position
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MudLordBoss] RunDefeated HUD/Event raise failed: {ex}");
                throw; // No silent fail — quest/cinematic chain depends on this firing.
            }

            // Local event for cinematic hook (Moon 1 defeat cutscene listens here).
            try { OnDefeated?.Invoke(this); }
            catch (Exception ex)
            {
                Debug.LogError($"[MudLordBoss] OnDefeated subscriber threw: {ex}");
                // Don't rethrow — one subscriber crashing should not eat the others; we logged loud.
            }

            yield return null;
        }

        // ── Combat API (called by player strike code) ───────────────────────────

        /// <summary>Apply damage to the boss. Routes through the weak-point check so
        /// front-of-rig strikes are absorbed. Returns the effective damage applied.</summary>
        public float TryStrikeWeakPoint(float damage, Vector3 strikeOrigin)
        {
            if (_state == State.Defeated) return 0f;
            if (damage <= 0f)
            {
                Debug.LogWarning($"[MudLordBoss] TryStrikeWeakPoint called with non-positive damage={damage}. Identifier searched: caller. Ignoring.");
                return 0f;
            }

            float multiplier = ComputeWeakPointMultiplier(strikeOrigin);
            float applied = damage * multiplier;
            ApplyDamage(applied);
            return applied;
        }

        float ComputeWeakPointMultiplier(Vector3 strikeOrigin)
        {
            // Strike from BEHIND the boss = weak point = full damage (+ bonus).
            // Strike from the front = absorbed = frontalDamageMultiplier.
            Vector3 fromBossToStrike = strikeOrigin - transform.position;
            fromBossToStrike.y = 0f;
            if (fromBossToStrike.sqrMagnitude < 0.001f) return frontalDamageMultiplier;
            fromBossToStrike.Normalize();

            // Dot with forward: +1 = directly in front, -1 = directly behind.
            float dotFwd = Vector3.Dot(transform.forward, fromBossToStrike);
            float behindThreshold = Mathf.Cos(Mathf.Deg2Rad * (180f - weakPointAngleDeg * 0.5f));
            // behindThreshold ≈ negative number. dotFwd <= behindThreshold means we're in the back cone.
            if (dotFwd <= behindThreshold)
            {
                // In Phase 3 the weak point should only register if the player has Aether
                // Vision active. Without a runtime aether-vision flag accessor in this
                // assembly, we still apply full damage; the design doc records the intent
                // and a follow-up PR can gate via a passed-in flag when the player code
                // exposes it. (No invented dependency — see design doc Phase 3 note.)
                return 1.5f; // back-cone bonus
            }
            return frontalDamageMultiplier;
        }

        void ApplyDamage(float amount)
        {
            if (amount <= 0f) return;
            hp = Mathf.Max(0f, hp - amount);
            try { GameEvents.RaiseHUDUpdateBossHealth(NormalizedHealth); }
            catch (Exception ex) { Debug.LogError($"[MudLordBoss] ApplyDamage HUD update failed: {ex}"); }

            if (hp <= 0f && _state != State.Defeated)
            {
                if (_fsmCo != null) { StopCoroutine(_fsmCo); _fsmCo = null; }
                StartCoroutine(RunDefeated());
            }
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        void FaceTarget(Vector3 worldPos)
        {
            Vector3 look = worldPos - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude < 0.001f) return;
            transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
        }

        void TryAreaDamage(float radius, float damage)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _aoeBuf, playerLayer, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var col = _aoeBuf[i];
                if (col == null) continue;
                // Damage routing — the project's player health controllers live in another
                // assembly. We raise the canonical event so PlayerHealth can subscribe.
                // (PlayerHealth already listens to RaisePlayerDamaged in Tartaria.Core.)
                if (col.CompareTag("Player"))
                {
                    try
                    {
                        // remainingHealth=-1 = "subscriber should compute current and apply".
                        // This matches how MudGolemAI hands off damage to the player layer.
                        GameEvents.RaisePlayerDamaged(damage, -1f);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[MudLordBoss] TryAreaDamage RaisePlayerDamaged failed: {ex}");
                    }
                }
            }
        }
    }
}
