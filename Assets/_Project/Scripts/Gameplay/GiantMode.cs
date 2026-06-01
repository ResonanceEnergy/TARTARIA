using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// GiantMode — Moon 1 climax mechanic per docs/03 Days 13–18:
    /// "First giant-mode burst: skeleton in the mud pulses with your bloodline.
    /// For 60 exhilarating seconds, you're 15 feet tall. Toss the scouts into
    /// a mud pit. It feels *incredible*."
    ///
    /// Press G (or Right Trigger on gamepad) to activate. Player scales up 3×,
    /// move + damage scale with it, enemies within `tossRadius` get launched.
    /// 60-second duration, then scale lerps back to normal over 1.5s. Cooldown
    /// 90s to avoid spam.
    ///
    /// Attach this component to the Player GameObject (PlayerSpawner does this
    /// automatically once GiantMode is in the assembly).
    /// </summary>
    public class GiantMode : MonoBehaviour
    {
        [Header("Giant scale")]
        [SerializeField] private float giantScale = 3.0f;
        [SerializeField] private float scaleRampSeconds = 0.8f;
        [SerializeField] private float scaleDownSeconds = 1.5f;

        [Header("Duration / cooldown")]
        [SerializeField] private float duration = 60f;
        [SerializeField] private float cooldown = 90f;

        [Header("Toss on activate")]
        [SerializeField] private float tossRadius = 8f;
        [SerializeField] private float tossForce = 22f;
        [SerializeField] private LayerMask tossLayers = ~0; // all by default

        [Header("Combat scaling while giant")]
        [SerializeField] private float damageMultiplier = 3f;
        [SerializeField] private float speedMultiplier = 1.6f;

        public bool IsGiant { get; private set; }
        public float TimeRemaining { get; private set; }
        public float CooldownRemaining { get; private set; }
        public float DamageMultiplier => IsGiant ? damageMultiplier : 1f;
        public float SpeedMultiplier => IsGiant ? speedMultiplier : 1f;

        private Vector3 _baseScale;
        private float _nextAllowedAt;

        void Awake()
        {
            _baseScale = transform.localScale;
        }

        void Update()
        {
            // Cooldown tracking
            if (Time.time < _nextAllowedAt) CooldownRemaining = _nextAllowedAt - Time.time;
            else CooldownRemaining = 0f;

            // Input — G on keyboard or RT on gamepad
            if (!IsGiant && CooldownRemaining <= 0f && InputPressedThisFrame())
            {
                StartCoroutine(GiantRoutine());
            }

            if (IsGiant) TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
        }

        bool InputPressedThisFrame()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.gKey.wasPressedThisFrame) return true;
            var pad = Gamepad.current;
            if (pad != null && pad.rightTrigger.wasPressedThisFrame) return true;
            return false;
        }

        IEnumerator GiantRoutine()
        {
            // Pre-cast — quick toss + announce
            Debug.Log("[GiantMode] Activated");
            ServiceLocator.HUD?.ShowBanner("GIANT MODE", "The skeleton in the mud pulses with your blood.", 4f);
            TossNearby();
            SpawnGiantModeBurstVFX();

            // Scale up
            IsGiant = true;
            TimeRemaining = duration;
            _nextAllowedAt = Time.time + duration + cooldown;

            yield return ScaleTo(_baseScale * giantScale, scaleRampSeconds);

            // Hold giant
            while (TimeRemaining > 0f)
            {
                yield return null;
            }

            // Scale back down
            yield return ScaleTo(_baseScale, scaleDownSeconds);
            IsGiant = false;
            ServiceLocator.HUD?.ShowBanner("Giant Mode ends", $"Cooldown {cooldown:F0}s", 3f);
        }

        IEnumerator ScaleTo(Vector3 target, float seconds)
        {
            Vector3 start = transform.localScale;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);
                // Ease-out
                u = 1f - (1f - u) * (1f - u);
                transform.localScale = Vector3.Lerp(start, target, u);
                yield return null;
            }
            transform.localScale = target;
        }

        /// <summary>
        /// Instantiates the authored Giant-Mode burst VFX at the player's feet.
        /// Falls back silently if the prefab is missing from Resources — the
        /// activation HUD banner + scale tween still play.
        /// </summary>
        void SpawnGiantModeBurstVFX()
        {
            var prefab = Resources.Load<GameObject>("VFX/Moon1/VFX_GiantModeBurst");
            if (prefab == null)
            {
                Debug.LogWarning("[GiantMode] VFX_GiantModeBurst prefab missing in Resources — no burst FX");
                return;
            }
            var pos = transform.position;
            var inst = Instantiate(prefab, pos, Quaternion.identity);
            inst.name = "VFX_GiantModeBurst(Instance)";
            Destroy(inst, 6f); // VFX prefab lifetimes peak ~4s, give 2s safety margin
        }

        void TossNearby()
        {
            var hits = Physics.OverlapSphere(transform.position, tossRadius, tossLayers, QueryTriggerInteraction.Ignore);
            int tossed = 0;
            foreach (var h in hits)
            {
                if (h.gameObject == gameObject) continue;
                if (!h.CompareTag("Enemy")) continue;

                Vector3 dir = (h.transform.position - transform.position);
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
                dir.Normalize();
                Vector3 launch = dir * tossForce + Vector3.up * tossForce * 0.6f;

                // Prefer Rigidbody, fall back to CharacterController translation
                var rb = h.attachedRigidbody;
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.AddForce(launch, ForceMode.VelocityChange);
                }
                else
                {
                    var cc = h.GetComponent<CharacterController>();
                    if (cc != null) cc.Move(launch * 0.4f);
                    else h.transform.position += launch * 0.4f;
                }

                // Deal proportional damage to any enemy with a TakeDamage method.
                // Use SendMessage so Gameplay asmdef doesn't need to reference AI
                // (which already references Gameplay — would be circular).
                h.SendMessageUpwards("TakeDamage", 45f, SendMessageOptions.DontRequireReceiver);

                tossed++;
            }
            if (tossed > 0)
            {
                Debug.Log($"[GiantMode] Tossed {tossed} enemy/enemies");
                ServiceLocator.HUD?.ShowBanner("MUD SLAM", $"{tossed} thrown into the mud", 2.5f);
            }
        }
    }
}
