using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Combat Bridge — connects MonoBehaviour combat inputs to DOTS
    /// combat entities. Monitors enemy health, manages state transitions,
    /// and triggers feedback when enemies are defeated.
    ///
    /// The player's combat entity is created once and persists.
    /// Attack actions queue DamageEvents on nearby enemy entities.
    /// </summary>
    [DisallowMultipleComponent]
    public class CombatBridge : MonoBehaviour
    {
        public static CombatBridge Instance { get; private set; }

        [Header("Combat Settings")]
        [SerializeField] float pulseRange = 8f;
        [SerializeField] float pulseDamage = 15f;
        [SerializeField] float strikeRange = 4f;
        [SerializeField] float strikeDamage = 30f;
        [SerializeField] float shieldDuration = 2f;

        [Header("Cooldowns")]
        [SerializeField] float pulseCooldown = 1.5f;
        [SerializeField] float strikeCooldown = 3f;
        [SerializeField] float shieldCooldown = 5f;

        World _world;
        EntityManager _em;
        Entity _playerCombatEntity;
        bool _initialized;

        float _pulseTimer;
        float _strikeTimer;
        float _shieldTimer;

        // Enemy tracking for state transitions
        int _activeEnemyCount;
        bool _inCombat;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            InitECS();
        }

        void InitECS()
        {
            _world = World.DefaultGameObjectInjectionWorld;
            if (_world == null) return;
            _em = _world.EntityManager;

            // Create player combat entity
            _playerCombatEntity = _em.CreateEntity();
            _em.AddComponentData(_playerCombatEntity, new HarmonicCombatant
            {
                Health = 100f,
                MaxHealth = 100f,
                AetherCharge = 0f,
                MaxAetherCharge = 100f,
                CurrentFrequency = 432f, // Harmonic (player)
                ComboCount = 0,
                IsGiantMode = false
            });
            _em.AddComponentData(_playerCombatEntity, new PlayerCombatState());
            _em.AddBuffer<DamageEvent>(_playerCombatEntity);

            _initialized = true;
        }

        void Update()
        {
            if (!_initialized) { InitECS(); return; }

            // Update cooldown timers
            _pulseTimer = Mathf.Max(0, _pulseTimer - Time.deltaTime);
            _strikeTimer = Mathf.Max(0, _strikeTimer - Time.deltaTime);
            _shieldTimer = Mathf.Max(0, _shieldTimer - Time.deltaTime);

            // Monitor enemies for combat state transitions
            MonitorEnemies();

            // Check player health
            CheckPlayerHealth();
        }

        // ─── Combat Actions (called by GameLoopController) ──

        public void FireResonancePulse()
        {
            if (_pulseTimer > 0 || !_initialized) return;
            _pulseTimer = pulseCooldown;

            var playerPos = GetPlayerPosition();
            DamageNearbyEnemies(playerPos, pulseRange, pulseDamage, DamageType.ResonancePulse);

            // Haptic feedback
            HapticFeedbackManager.Instance?.PlayCombatHit();

            // VFX
            VFXController.Instance?.PlayResonancePulse(playerPos, pulseRange);
        }

        public void FireHarmonicStrike()
        {
            if (_strikeTimer > 0 || !_initialized) return;
            _strikeTimer = strikeCooldown;

            var playerPos = GetPlayerPosition();
            var forward = GetPlayerForward();
            DamageEnemiesInCone(playerPos, forward, strikeRange, 60f, strikeDamage, DamageType.HarmonicStrike);

            HapticFeedbackManager.Instance?.PlayCombatHit();
            VFXController.Instance?.PlayHarmonicStrike(playerPos, forward);
        }

        public void ActivateFrequencyShield()
        {
            if (_shieldTimer > 0 || !_initialized) return;
            _shieldTimer = shieldCooldown;

            if (_em.Exists(_playerCombatEntity))
            {
                var pcs = _em.GetComponentData<PlayerCombatState>(_playerCombatEntity);
                pcs.IsShielding = true;
                pcs.ShieldActiveTime = shieldDuration;
                _em.SetComponentData(_playerCombatEntity, pcs);
            }

            VFXController.Instance?.PlayShieldActivation(GetPlayerPosition());
        }

        // ─── Enemy Monitoring ────────────────────────

        void MonitorEnemies()
        {
            int count = 0;
            bool anyEnemyDied = false;
            float3 deathPos = float3.zero;

            var query = _em.CreateEntityQuery(typeof(EnemyTag), typeof(HarmonicCombatant), typeof(LocalTransform));
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var combatant = _em.GetComponentData<HarmonicCombatant>(entities[i]);
                if (combatant.Health > 0)
                {
                    count++;
                }
                else
                {
                    // Enemy just died
                    var transform = _em.GetComponentData<LocalTransform>(entities[i]);
                    deathPos = transform.Position;
                    anyEnemyDied = true;
                    _em.DestroyEntity(entities[i]);
                }
            }
            entities.Dispose();

            // Transition to combat if enemies nearby
            if (count > 0 && !_inCombat)
            {
                _inCombat = true;
                if (GameStateManager.Instance.CurrentState == GameState.Exploration)
                    GameStateManager.Instance.TransitionTo(GameState.Combat);
            }
            else if (count == 0 && _inCombat)
            {
                _inCombat = false;
                // Notify game loop of last enemy death
                if (anyEnemyDied)
                    GameLoopController.Instance?.OnEnemyDefeated(deathPos);
            }

            if (anyEnemyDied && count > 0)
            {
                // Enemy died but combat continues
                HapticFeedbackManager.Instance?.PlayGolemDeath();
                VFXController.Instance?.PlayEnemyDissolution(deathPos);
            }

            _activeEnemyCount = count;
        }

        void CheckPlayerHealth()
        {
            if (!_em.Exists(_playerCombatEntity)) return;

            var combatant = _em.GetComponentData<HarmonicCombatant>(_playerCombatEntity);

            // Regenerate player health slowly in exploration
            if (!_inCombat && combatant.Health < combatant.MaxHealth)
            {
                combatant.Health = Mathf.Min(combatant.MaxHealth,
                    combatant.Health + 5f * Time.deltaTime);
                _em.SetComponentData(_playerCombatEntity, combatant);
            }

            // Update HUD with player health-as-aether
            UI.HUDController.Instance?.UpdateAetherCharge(
                combatant.AetherCharge / combatant.MaxAetherCharge * 100f);
        }

        // ─── Damage Helpers ──────────────────────────

        void DamageNearbyEnemies(Vector3 origin, float range, float damage, DamageType type)
        {
            var query = _em.CreateEntityQuery(typeof(EnemyTag), typeof(HarmonicCombatant), typeof(LocalTransform));
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);

            float3 pos = new float3(origin.x, origin.y, origin.z);

            for (int i = 0; i < entities.Length; i++)
            {
                var transform = _em.GetComponentData<LocalTransform>(entities[i]);
                float dist = math.distance(pos, transform.Position);

                if (dist <= range)
                {
                    var buffer = _em.GetBuffer<DamageEvent>(entities[i]);
                    buffer.Add(new DamageEvent
                    {
                        Source = _playerCombatEntity,
                        Target = entities[i],
                        Amount = damage,
                        Frequency = 432f,
                        Type = type
                    });

                    // Track combo for Mud Golem stun
                    if (_em.HasComponent<MudGolem>(entities[i]) && type == DamageType.ResonancePulse)
                    {
                        var golem = _em.GetComponentData<MudGolem>(entities[i]);
                        golem.ConsecutivePulseHits++;
                        if (golem.ConsecutivePulseHits >= 3)
                        {
                            golem.StunTimer = golem.StunDuration;
                            golem.ConsecutivePulseHits = 0;
                        }
                        _em.SetComponentData(entities[i], golem);
                    }
                }
            }
            entities.Dispose();
        }

        void DamageEnemiesInCone(Vector3 origin, Vector3 forward, float range,
            float coneAngle, float damage, DamageType type)
        {
            var query = _em.CreateEntityQuery(typeof(EnemyTag), typeof(HarmonicCombatant), typeof(LocalTransform));
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);

            float3 pos = new float3(origin.x, origin.y, origin.z);
            float3 fwd = new float3(forward.x, 0, forward.z);
            fwd = math.normalizesafe(fwd);

            for (int i = 0; i < entities.Length; i++)
            {
                var transform = _em.GetComponentData<LocalTransform>(entities[i]);
                float3 toEnemy = transform.Position - pos;
                float dist = math.length(toEnemy);

                if (dist <= range)
                {
                    float angle = math.degrees(math.acos(
                        math.dot(math.normalizesafe(toEnemy), fwd)));

                    if (angle <= coneAngle * 0.5f)
                    {
                        var buffer = _em.GetBuffer<DamageEvent>(entities[i]);
                        buffer.Add(new DamageEvent
                        {
                            Source = _playerCombatEntity,
                            Target = entities[i],
                            Amount = damage,
                            Frequency = 432f,
                            Type = type
                        });
                    }
                }
            }
            entities.Dispose();
        }

        // ─── Utility ─────────────────────────────────

        Vector3 GetPlayerPosition()
        {
            var player = GameObject.FindWithTag("Player");
            return player != null ? player.transform.position : Vector3.zero;
        }

        Vector3 GetPlayerForward()
        {
            var player = GameObject.FindWithTag("Player");
            return player != null ? player.transform.forward : Vector3.forward;
        }

        void OnDestroy()
        {
            if (_initialized && _world != null && _world.IsCreated && _em.Exists(_playerCombatEntity))
                _em.DestroyEntity(_playerCombatEntity);
        }
    }
}
