using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Aether Vision overlay — toggled by Y (keyboard) or buttonNorth (F310 gamepad Y).
    /// Manages an Aether Stamina pool that drains while active and regenerates while off.
    /// Fires GameEvents.RaiseAetherVisionToggled(bool) on every toggle so URP feature,
    /// HUD, and audio can react.
    ///
    /// Auto-bootstraps as a DontDestroyOnLoad singleton after every scene load —
    /// no scene wiring required.
    ///
    /// The visual look (desaturation, ley-line glow, building auras) is owned by
    /// <see cref="AetherVisionURPFeature"/> — added to the UniversalRendererData asset
    /// by the art / tools lane. This script owns timing, input, and state only.
    /// </summary>
    [DisallowMultipleComponent]
    public class AetherVisionOverlay : MonoBehaviour
    {
        public static AetherVisionOverlay Instance { get; private set; }

        // ─── Stamina balance ──────────────────────────────────────────────
        [Header("Aether Stamina")]
        [SerializeField] float maxStamina = 10f;
        [SerializeField] float drainPerSec = 1.5f;
        [SerializeField] float regenPerSec = 0.8f;

        // ─── Band colors (consumed by URP feature later) ──────────────────
        [Header("Aether Band Colors")]
        [Tooltip("Color used to highlight active ley lines while vision is on.")]
        [SerializeField] Color leyLineColor = new Color(1f, 0.84f, 0.0f, 1f); // gold
        [Tooltip("Telluric band aura (7.83 Hz) — earthy brown.")]
        [SerializeField] Color tellucColor = new Color(0.45f, 0.30f, 0.15f, 1f);
        [Tooltip("Harmonic band aura (432 Hz) — calm blue.")]
        [SerializeField] Color harmonicColor = new Color(0.20f, 0.55f, 0.90f, 1f);
        [Tooltip("Celestial band aura (528 Hz) — luminous white.")]
        [SerializeField] Color celestialColor = Color.white;

        public bool IsActive { get; private set; }
        public float Stamina { get; private set; }
        public float MaxStamina => maxStamina;
        public Color LeyLineColor => leyLineColor;
        public Color TellucColor => tellucColor;
        public Color HarmonicColor => harmonicColor;
        public Color CelestialColor => celestialColor;

        /// <summary>Local fallback event for subscribers that don't want to depend on Tartaria.Core.</summary>
        public event Action<bool> OnToggled;

        // Cached reflection probe to avoid spamming the warning fallback every frame.
        static MethodInfo s_raiseAetherVisionToggled;
        static bool s_canonicalEventResolved;

        // ─── Auto-bootstrap ───────────────────────────────────────────────
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            // Only bootstrap once across scenes. Re-entrant calls (additive scene loads)
            // find the existing Instance and bail.
            if (Instance != null) return;

            var go = new GameObject("[AetherVisionOverlay]");
            DontDestroyOnLoad(go);
            go.AddComponent<AetherVisionOverlay>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning(
                    $"[AetherVisionOverlay] Duplicate instance on '{gameObject.name}'. Destroying duplicate; existing instance lives on '{Instance.gameObject.name}'.");
                Destroy(this);
                return;
            }
            Instance = this;
            Stamina = maxStamina;
            ResolveCanonicalEvent();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Frame loop ───────────────────────────────────────────────────
        void Update()
        {
            // Use fully-qualified UnityEngine.Time per CLAUDE.md API_CONTRACT (banned-namespace defense).
            float dt = UnityEngine.Time.deltaTime;

            // Poll input — Y key OR gamepad north (Logitech F310 Y in X-mode).
            bool keyboardToggle = Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame;
            bool gamepadToggle = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
            if (keyboardToggle || gamepadToggle)
            {
                Toggle();
            }

            // Stamina pool — drain while active, regen while off.
            if (IsActive)
            {
                Stamina -= drainPerSec * dt;
                if (Stamina <= 0f)
                {
                    Stamina = 0f;
                    SetActive(false, "stamina depleted");
                }
            }
            else if (Stamina < maxStamina)
            {
                Stamina = Mathf.Min(maxStamina, Stamina + regenPerSec * dt);
            }
        }

        // ─── Public API ───────────────────────────────────────────────────
        public void Toggle() => SetActive(!IsActive, "toggle");

        public void SetActive(bool active, string reason)
        {
            if (IsActive == active) return;

            // Block re-activation if stamina is empty — feel rule: no flicker on/off.
            if (active && Stamina <= 0.01f)
            {
                Debug.Log("[AetherVisionOverlay] Activation blocked — stamina empty (regenerating).");
                return;
            }

            IsActive = active;
            Debug.Log($"[AetherVisionOverlay] {(active ? "ON" : "OFF")} ({reason}) — stamina={Stamina:F2}/{maxStamina:F2}");

            FireToggled(active);
            OnToggled?.Invoke(active);
        }

        // ─── Event fan-out ────────────────────────────────────────────────
        void FireToggled(bool enabled)
        {
            if (s_canonicalEventResolved && s_raiseAetherVisionToggled != null)
            {
                // Reflection invoke against the resolved Tartaria.Core.GameEvents method.
                // No silent catches per no-debt rule 3 — exceptions surface to console.
                try
                {
                    s_raiseAetherVisionToggled.Invoke(null, new object[] { enabled });
                    return;
                }
                catch (TargetInvocationException tie)
                {
                    Debug.LogError($"[AetherVisionOverlay] GameEvents.RaiseAetherVisionToggled threw: {tie.InnerException}");
                    return;
                }
            }

            // Fallback: surface state through the banner so the player still sees something,
            // and the canonical-API gap shows up in HANDOFFS the next time someone greps.
            Debug.LogWarning("[AetherVisionOverlay] GameEvents.RaiseAetherVisionToggled not found — using HUD banner fallback.");
            try
            {
                GameEvents.RaiseHUDShowBanner(
                    "[AetherVision]",
                    enabled ? "Vision Active" : "Vision Off",
                    2.0f);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AetherVisionOverlay] HUD banner fallback failed: {ex}");
            }
        }

        static void ResolveCanonicalEvent()
        {
            if (s_canonicalEventResolved) return;
            s_canonicalEventResolved = true;

            // Resolve Tartaria.Core.GameEvents.RaiseAetherVisionToggled(bool) by reflection so the
            // file still compiles if the canonical event ever moves. Per API_CONTRACT.md: grep first,
            // don't invent. As of 2026-06-02 the canonical signature is RaiseAetherVisionToggled(bool).
            Type t = typeof(GameEvents);
            s_raiseAetherVisionToggled = t.GetMethod(
                "RaiseAetherVisionToggled",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(bool) },
                null);

            if (s_raiseAetherVisionToggled == null)
            {
                Debug.LogWarning(
                    "[AetherVisionOverlay] Could not resolve GameEvents.RaiseAetherVisionToggled(bool) — falling back to HUD banner. " +
                    "Update AetherVisionOverlay.cs once the canonical event is reintroduced.");
            }
        }
    }
}
