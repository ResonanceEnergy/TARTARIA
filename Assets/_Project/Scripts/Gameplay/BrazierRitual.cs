using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Audio;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Brazier-touch ritual interaction (Sprint 9 Lane 7 — audit v2 ❌ #8.1).
    ///
    /// Author: spawned per-brazier under <see cref="Tartaria.Integration.Moon1Braziers"/> at
    /// runtime, or attached in-Editor to any GameObject that has (or will get) a child
    /// transform named "Flame". When the Player enters the trigger collider an interaction
    /// prompt is shown via <see cref="GameEvents.RaiseHUDShowInteractionPrompt"/>; pressing
    /// E (keyboard) or South face button (gamepad) lights the brazier:
    ///   * Enables the "Flame" child VFX (logs error w/ expected path + degrades gracefully
    ///     if missing — SFX + counter still fire so partial wiring keeps progressing the
    ///     ritual)
    ///   * Plays the ignite one-shot via <see cref="AudioManager"/> (string lookup first,
    ///     then Resources/Audio/SFX/torch_ignite fallback)
    ///   * Increments <see cref="LitCount"/> and fires <see cref="GameEvents.OnBrazierLit"/>
    ///   * When <see cref="LitCount"/> >= <see cref="RingCompleteThreshold"/>, raises the
    ///     "Braziers Wake" banner + <see cref="GameEvents.OnBrazierRingComplete"/> exactly
    ///     once.
    ///
    /// Per CLAUDE.md NO-STUBS: every code path has a real body. The flame child reference
    /// is NOT a TODO — if absent we log a clear error including the expected child path and
    /// skip ONLY that brazier's VFX.
    ///
    /// Per API_CONTRACT.md §5: uses <c>UnityEngine.InputSystem.Keyboard.current</c> +
    /// <c>Gamepad.current</c> — no legacy <c>UnityEngine.Input</c> reads.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    public class BrazierRitual : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Stable id used in OnBrazierLit payload (defaults to GameObject name on Awake if blank).")]
        [SerializeField] string brazierId = string.Empty;

        [Header("Interaction Trigger")]
        [SerializeField, Range(0.5f, 6f), Tooltip("Player must be within this radius to see the prompt.")]
        float interactionRadius = 2.4f;

        [Header("Ritual Tuning")]
        [Tooltip("How many braziers must be lit before the ring is considered complete and the banner fires.")]
        [SerializeField, Range(1, 24)] int ringCompleteThreshold = 3;

        [Header("Audio")]
        [Tooltip("Optional explicit clip. If null, falls back to AudioManager.PlaySFX(\"torch_ignite\", ...) then Resources/Audio/SFX/torch_ignite.")]
        [SerializeField] AudioClip igniteClip;

        [Header("Banner Copy")]
        [SerializeField] string ringCompleteTitle = "The Braziers Wake";
        [SerializeField, TextArea] string ringCompleteSubtitle = "The ring is complete — the village remembers.";
        [SerializeField, Range(1f, 12f)] float ringCompleteBannerDuration = 5f;

        // ─── Static counter / threshold (shared across all instances) ────────────────
        public static int LitCount { get; private set; }
        public static int RingCompleteThreshold { get; private set; } = 3;
        static bool s_ringCompleteFired;

        // ─── Per-instance runtime state ──────────────────────────────────────────────
        bool _lit;
        bool _playerInRange;
        GameObject _player;
        SphereCollider _trigger;
        GameObject _flameChild;

        void Reset()
        {
            // Make trigger collider sensible on initial add via the Inspector.
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 2.4f;
        }

        void Awake()
        {
            if (string.IsNullOrEmpty(brazierId))
                brazierId = gameObject.name;

            // Trigger collider config — author-time friendly + runtime-safe.
            _trigger = GetComponent<SphereCollider>();
            _trigger.isTrigger = true;
            _trigger.radius = interactionRadius;

            // The flame VFX child is expected at transform/Flame (matches Moon1Braziers naming).
            var flameTf = transform.Find("Flame");
            if (flameTf == null)
            {
                Debug.LogError(
                    $"[BrazierRitual] '{brazierId}' is missing expected child path '{transform.name}/Flame'. " +
                    "Flame VFX will be skipped for THIS brazier (SFX + counter still fire — partial wiring degrades gracefully). " +
                    "Fix: ensure Moon1Braziers.BuildBrazier created a 'Flame' child (or author one in the prefab).");
                _flameChild = null;
            }
            else
            {
                _flameChild = flameTf.gameObject;
                // Pre-ritual state: flame is off. Moon1Braziers turns it on at creation, so explicitly douse here.
                _flameChild.SetActive(false);
            }

            // Sync the shared threshold from the highest-configured instance (allows designers
            // to set the value on any brazier — last-wins is fine since they should all match).
            RingCompleteThreshold = Mathf.Max(1, ringCompleteThreshold);
        }

        void OnTriggerEnter(Collider other)
        {
            if (_lit) return;
            if (!other.CompareTag("Player")) return;

            _playerInRange = true;
            _player = other.gameObject;
            GameEvents.RaiseHUDShowInteractionPrompt("[E] Light brazier");
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (other.gameObject != _player) return;

            _playerInRange = false;
            _player = null;
            GameEvents.RaiseHUDHideInteractionPrompt();
        }

        void Update()
        {
            if (_lit || !_playerInRange) return;

            // Unity 6 InputSystem reads — see API_CONTRACT.md §5 (no legacy UnityEngine.Input).
            bool pressed = false;
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame) pressed = true;
            var gp = Gamepad.current;
            if (!pressed && gp != null && gp.buttonSouth.wasPressedThisFrame) pressed = true;

            if (pressed) LightBrazier();
        }

        void LightBrazier()
        {
            if (_lit) return;
            _lit = true;

            // 1) Flame VFX — enable child (already error-logged in Awake if missing).
            if (_flameChild != null)
            {
                _flameChild.SetActive(true);
            }
            // else: error already logged in Awake, partial-wiring policy = keep going.

            // 2) SFX — explicit clip > AudioManager name lookup > Resources fallback.
            PlayIgniteSFX();

            // 3) Counter + per-brazier event.
            LitCount++;
            GameEvents.RaiseBrazierLit(brazierId);

            // 4) Hide the prompt now that this brazier is done.
            GameEvents.RaiseHUDHideInteractionPrompt();
            _playerInRange = false;

            Debug.Log($"[BrazierRitual] Lit '{brazierId}' ({LitCount}/{RingCompleteThreshold}).");

            // 5) Ring-complete trigger (one-shot).
            if (!s_ringCompleteFired && LitCount >= RingCompleteThreshold)
            {
                s_ringCompleteFired = true;
                GameEvents.RaiseHUDShowBanner(ringCompleteTitle, ringCompleteSubtitle, ringCompleteBannerDuration);
                GameEvents.RaiseBrazierRingComplete();
                Debug.Log($"[BrazierRitual] Ring complete — {LitCount} braziers lit. Banner + OnBrazierRingComplete fired.");
            }
        }

        void PlayIgniteSFX()
        {
            var am = AudioManager.Instance;
            if (am == null)
            {
                Debug.LogWarning($"[BrazierRitual] '{brazierId}' — AudioManager.Instance null; ignite SFX skipped.");
                return;
            }

            if (igniteClip != null)
            {
                am.PlaySFX(igniteClip, transform.position, 0.85f);
                return;
            }

            // Resources fallback — try the canonical path first.
            var resClip = UnityEngine.Resources.Load<AudioClip>("Audio/SFX/torch_ignite");
            if (resClip != null)
            {
                am.PlaySFX(resClip, transform.position, 0.85f);
                return;
            }

            Debug.LogWarning(
                $"[BrazierRitual] '{brazierId}' — no ignite clip assigned and Resources/Audio/SFX/torch_ignite not found. " +
                "Falling back to AudioManager.PlaySFX(\"torch_ignite\", ...) by name — author the clip when convenient.");
            am.PlaySFX("torch_ignite", transform.position, 0.85f);
        }

        /// <summary>
        /// Resets the static counter + ring-complete latch. Wire to scene unload / new game
        /// boots so a second playthrough doesn't start at the previous LitCount.
        /// </summary>
        public static void ResetRitualState()
        {
            LitCount = 0;
            s_ringCompleteFired = false;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = _lit ? new Color(1f, 0.55f, 0.18f, 0.85f) : new Color(0.4f, 0.7f, 1f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
