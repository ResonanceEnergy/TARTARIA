using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Audio;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Day-8: Player melee combat.
    /// LMB / Gamepad West button → forward sphere overlap → damage MudGolemAI (int)
    /// + MudGolemHealth (float) via SendMessage (asmdef-cycle-safe).
    /// Auto-attached by CharacterPrefabFactory.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Melee")]
        [SerializeField] int meleeDamage = 25;
        [SerializeField] float reach = 2.6f;
        [SerializeField] float radius = 1.4f;
        [SerializeField] float cooldown = 0.45f;
        [SerializeField] float swingDuration = 0.25f;

        public static event System.Action OnSwing;
        public bool IsSwinging => Time.time - _lastSwingStart < swingDuration;

        float _lastSwingStart = -10f;

        void Update()
        {
            bool fire = false;
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) fire = true;
            var pad = Gamepad.current;
            if (pad != null && pad.buttonWest.wasPressedThisFrame) fire = true;

            if (fire && Time.time - _lastSwingStart >= cooldown)
                Swing();
        }

        void Swing()
        {
            _lastSwingStart = Time.time;
            try { OnSwing?.Invoke(); } catch { }
            AudioManager.Instance?.PlaySFX("CombatHit", transform.position);

            // Sphere swept forward in front of player chest
            Vector3 origin = transform.position + Vector3.up * 1.2f + transform.forward * (reach * 0.5f);
            int hit = 0;
            var cols = Physics.OverlapSphere(origin, radius, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                if (c == null) continue;
                if (c.transform.IsChildOf(transform) || c.transform == transform) continue;
                // Bridge to enemy components living in AI / Integration asmdefs
                c.SendMessageUpwards("TakeDamage", (int)meleeDamage, SendMessageOptions.DontRequireReceiver);
                c.SendMessageUpwards("TakeDamage", (float)meleeDamage, SendMessageOptions.DontRequireReceiver);
                hit++;
            }

            if (hit > 0)
            {
                AudioManager.Instance?.PlaySFX("EnemyDeath", origin);
                Debug.Log($"[PlayerCombat] Hit {hit} target(s) for {meleeDamage}");
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.45f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.2f + transform.forward * (reach * 0.5f), radius);
        }
    }
}
