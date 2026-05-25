using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Profiling;
using Tartaria.Audio;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Day-8: Player melee combat.
    /// LMB / Gamepad West button → forward sphere overlap → damage MudGolemAI (int)
    /// + MudGolemHealth (float) via SendMessage (asmdef-cycle-safe).
    /// Auto-attached by CharacterPrefabFactory.
    /// 
    /// Sprint features: Hit-stop, damage numbers, camera punch on hit-confirmed.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerCombat : MonoBehaviour
    {
        static readonly ProfilerMarker s_UpdateMarker = new ProfilerMarker("PlayerCombat.Update");
        static readonly ProfilerMarker s_SwingMarker = new ProfilerMarker("PlayerCombat.Swing");

        [Header("Melee")]
        [SerializeField] int meleeDamage = 25;
        [SerializeField] float reach = 2.6f;
        [SerializeField] float radius = 1.4f;
        [SerializeField] float cooldown = 0.45f;
        [SerializeField] float swingDuration = 0.25f;

        public static event System.Action OnSwing;
        public bool IsSwinging => Time.time - _lastSwingStart < swingDuration;

        float _lastSwingStart = -10f;
        Unity.Cinemachine.CinemachineImpulseSource _impulseSource;
        
        // AGENT 6: Performance optimization - pre-allocated buffer for Physics.OverlapSphereNonAlloc
        readonly Collider[] _hitBuffer = new Collider[16];

        void Awake()
        {
            _impulseSource = GetComponent<Unity.Cinemachine.CinemachineImpulseSource>();
        }

        void Update()
        {
            using (PerformanceGuard.Profile(SystemTag.Player))
            using (s_UpdateMarker.Auto())
            {
                bool fire = false;
                var mouse = Mouse.current;
                if (mouse != null && mouse.leftButton.wasPressedThisFrame) fire = true;
                var pad = Gamepad.current;
                if (pad != null && pad.buttonWest.wasPressedThisFrame) fire = true;

                if (fire && Time.time - _lastSwingStart >= cooldown)
                    Swing();
            }
        }

        void Swing()
        {
            using (s_SwingMarker.Auto())
            {
                _lastSwingStart = Time.time;
                try { OnSwing?.Invoke(); } catch (System.Exception ex) { Debug.LogWarning($"[PlayerCombat] OnSwing listener failed: {ex.Message}"); }
                AudioManager.Instance?.PlaySFX("CombatHit", transform.position);

                // Moon1 Echohaven polish: light benefit from PulseDamage (E_SpireResonance +10% from spire restore) for player melee feel + power fantasy consistency with pulse/strike.
                float dmgMod = 1f + (Gameplay.SkillTreeSystem.Instance?.GetModifier(Gameplay.SkillModifierType.PulseDamage) ?? 0f) * 0.5f;
                int effectiveDamage = Mathf.RoundToInt(meleeDamage * dmgMod);

                // Sphere swept forward in front of player chest
                Vector3 origin = transform.position + Vector3.up * 1.2f + transform.forward * (reach * 0.5f);
                int hit = 0;
                
                // AGENT 6: Use NonAlloc variant to eliminate GC allocation
                int colCount = Physics.OverlapSphereNonAlloc(origin, radius, _hitBuffer, ~0, QueryTriggerInteraction.Collide);
                for (int i = 0; i < colCount; i++)
                {
                    var c = _hitBuffer[i];
                    if (c == null) continue;
                    if (c.transform.IsChildOf(transform) || c.transform == transform) continue;
                    
                    // Bridge to enemy components living in AI / Integration asmdefs
                    c.SendMessageUpwards("TakeDamage", (int)effectiveDamage, SendMessageOptions.DontRequireReceiver);
                    c.SendMessageUpwards("TakeDamage", (float)effectiveDamage, SendMessageOptions.DontRequireReceiver);
                    
                    // Sprint: Spawn damage number at hit position
                    DamageNumberPool.Spawn(effectiveDamage, c.transform.position);
                    
                    hit++;
                }

                if (hit > 0)
                {
                    AudioManager.Instance?.PlaySFX("EnemyDeath", origin);
                    Debug.Log($"[PlayerCombat] Hit {hit} target(s) for {effectiveDamage} (base {meleeDamage}, mod {dmgMod:F2} from PulseDamage)");
                    
                    // Sprint: Hit-stop on confirmed hit
                    HitStopController.Trigger(meleeDamage);
                    
                    // Sprint: Camera punch
                    if (_impulseSource != null)
                        _impulseSource.GenerateImpulse(0.5f);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.45f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.2f + transform.forward * (reach * 0.5f), radius);
        }
    }
}
