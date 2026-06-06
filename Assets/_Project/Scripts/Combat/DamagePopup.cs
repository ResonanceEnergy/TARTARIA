// DamagePopup.cs
// Sprint 6 Lane 5 — agent/anim/combat-hit-feedback
//
// Floating damage number that rises 1m and fades 1->0 alpha over 0.8s, then
// destroys itself. Drives the visual portion of HitFeedback.NotifyHit().
//
// Authoring: build via Tartaria/Combat/Build Damage Popup Prefab (see
// Editor/DamagePopupPrefabBuilder.cs).
//
// global::UnityEngine.Camera.main is fully qualified to defeat the Tartaria.Camera
// namespace shadow that exists at Assets/_Project/Scripts/Camera/*.cs.

using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace Tartaria.Gameplay.Combat
{
    [DisallowMultipleComponent]
    public sealed class DamagePopup : MonoBehaviour
    {
        // ---------------------------------------------------------------------
        // Tuning
        // ---------------------------------------------------------------------

        [Header("Animation")]
        [Tooltip("Vertical world-space rise distance over the lifetime.")]
        [SerializeField] private float _riseMeters = 1.0f;

        [Tooltip("Total lifetime in seconds. Alpha fades 1->0 across this window.")]
        [SerializeField] private float _lifetimeSeconds = 0.8f;

        [Header("Visual")]
        [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color _critColor = new Color(1f, 0.7f, 0.15f, 1f);

        [Tooltip("Crit scale multiplier applied on Configure.")]
        [SerializeField] private float _critScale = 1.4f;

        [Header("Bindings")]
        [Tooltip("Optional explicit TMP reference. Auto-resolved on Awake when null.")]
        [SerializeField] private TMP_Text _label;

        // ---------------------------------------------------------------------
        // Runtime state
        // ---------------------------------------------------------------------

        private Coroutine _lifetimeRoutine;
        private Vector3 _startWorldPos;
        private Color _activeColor;
        private global::UnityEngine.Camera _cam;

        // ---------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------

        private void Awake()
        {
            if (_label == null)
            {
                _label = GetComponentInChildren<TMP_Text>();
            }
            if (_label == null)
            {
                Debug.LogWarning($"[DamagePopup] '{name}' has no TMP_Text component in children. The popup will rise and destroy but show no text. Build via Tartaria/Combat/Build Damage Popup Prefab.");
            }
        }

        private void Start()
        {
            _startWorldPos = transform.position;
            // global::UnityEngine.Camera.main resolved here once instead of every frame.
            _cam = global::UnityEngine.Camera.main;
            _lifetimeRoutine = StartCoroutine(LifetimeRoutine());
        }

        private void OnDisable()
        {
            // Per the no-debt mandate: ensure coroutine cleanup if the object is
            // disabled (Destroy will fire OnDisable then OnDestroy).
            if (_lifetimeRoutine != null)
            {
                StopCoroutine(_lifetimeRoutine);
                _lifetimeRoutine = null;
            }
        }

        private void OnDestroy()
        {
            if (_lifetimeRoutine != null)
            {
                StopCoroutine(_lifetimeRoutine);
                _lifetimeRoutine = null;
            }
        }

        private void LateUpdate()
        {
            // Billboard toward main camera, if present, so 3D world-space text stays readable.
            if (_cam == null)
            {
                _cam = global::UnityEngine.Camera.main;
                if (_cam == null) return;
            }
            transform.forward = _cam.transform.forward;
        }

        // ---------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------

        /// <summary>
        /// Set the damage value, crit flag, and visual color/scale. Safe to call before Start().
        /// </summary>
        public void Configure(float dmg, bool isCrit)
        {
            _activeColor = isCrit ? _critColor : _normalColor;

            if (_label != null)
            {
                _label.text = Mathf.RoundToInt(dmg).ToString();
                _label.color = _activeColor;
            }

            transform.localScale = Vector3.one * (isCrit ? _critScale : 1f);
        }

        // ---------------------------------------------------------------------
        // Animation
        // ---------------------------------------------------------------------

        private IEnumerator LifetimeRoutine()
        {
            float elapsed = 0f;
            while (elapsed < _lifetimeSeconds)
            {
                float t = elapsed / _lifetimeSeconds;
                // Position: linear rise
                transform.position = _startWorldPos + Vector3.up * (_riseMeters * t);

                // Alpha: linear fade 1 -> 0 over lifetime
                if (_label != null)
                {
                    var c = _activeColor;
                    c.a = 1f - t;
                    _label.color = c;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Final snap to fully transparent before destroying — covers any over-shoot.
            if (_label != null)
            {
                var c = _activeColor;
                c.a = 0f;
                _label.color = c;
            }

            Destroy(gameObject);
        }
    }
}
