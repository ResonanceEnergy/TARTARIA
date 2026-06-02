// HitFeedback.cs
// Sprint 6 Lane 5 — agent/anim/combat-hit-feedback
//
// Central combat hit-feedback dispatcher.
//
// What it does on every "hit":
//   1. Hitstop      — pinches Time.timeScale to 0.05 for 0.04 unscaled seconds.
//   2. Screen shake — 3-pixel amplitude over 0.12s on Camera.main local position,
//                     smoothstep falloff, applied additively so we restore baseline cleanly.
//   3. Damage popup — instantiates DamagePopup prefab at hit position, rises 1m
//                     and fades 1->0 alpha over 0.8s, then self-destructs.
//
// GameEvents wiring (grep evidence quoted in commit message):
//   - Subscribes to GameEvents.OnPlayerDamaged (Action<PlayerDamagedEventArgs>) for the
//     player-side hit case. PlayerDamagedEventArgs.damageAmount is the popup value.
//     The popup spawns at the player's transform.position because PlayerDamagedEventArgs
//     does not carry a hit position.
//   - There is NO OnEnemyHit / OnEnemyDamaged / OnDamageDealt event in GameEvents.cs
//     (verified by grep — returned "No matches found"). For enemy-side feedback,
//     callers MUST invoke HitFeedback.NotifyHit(pos, dmg, isCrit) directly at the
//     strike site. The first NotifyHit call logs a one-shot warning so this contract
//     is visible in the editor console.
//
// Per CLAUDE.md no-debt mandate: no silent catches, no TODOs, coroutines cleaned in
// OnDestroy. global::UnityEngine.Camera.main is fully qualified to defeat the
// Tartaria.Camera namespace shadow that exists at Assets/_Project/Scripts/Camera/*.cs.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Gameplay.Combat
{
    /// <summary>
    /// Singleton MonoBehaviour that orchestrates hit feedback (hitstop, screen shake,
    /// floating damage numbers). Auto-bootstraps on first NotifyHit call if no scene
    /// instance exists.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class HitFeedback : MonoBehaviour
    {
        // ---------------------------------------------------------------------
        // Tuning (lane spec values — DO NOT silently change without coordination)
        // ---------------------------------------------------------------------

        [Header("Hitstop")]
        [Tooltip("Time.timeScale value while hitstop is active.")]
        [SerializeField] private float _hitstopScale = 0.05f;

        [Tooltip("Hitstop duration in UNSCALED seconds.")]
        [SerializeField] private float _hitstopDurationUnscaled = 0.04f;

        [Header("Screen Shake")]
        [Tooltip("Peak local-space displacement in world units (3 px ~= 0.03 at default ortho/persp scale; tweak in Editor if needed).")]
        [SerializeField] private float _shakeAmplitude = 0.03f;

        [Tooltip("Total shake duration in unscaled seconds.")]
        [SerializeField] private float _shakeDuration = 0.12f;

        [Header("Damage Popup")]
        [Tooltip("Prefab built by Tartaria/Combat/Build Damage Popup Prefab. Resolved at runtime from Resources if null.")]
        [SerializeField] private GameObject _damagePopupPrefab;

        [Tooltip("Resources-relative path used as a fallback when the prefab field is null.")]
        [SerializeField] private string _popupResourcePath = "Combat/DamagePopup";

        // ---------------------------------------------------------------------
        // Runtime state
        // ---------------------------------------------------------------------

        private static HitFeedback _instance;
        private static bool _enemyHitWarningLogged;

        private readonly List<Coroutine> _activeCoroutines = new List<Coroutine>();
        private global::UnityEngine.Camera _shakeCamera;
        private Vector3 _shakeBaselineLocalPos;
        private bool _isShaking;
        private bool _isHitstopped;
        private float _originalTimeScale = 1f;

        // ---------------------------------------------------------------------
        // Bootstrap / lifecycle
        // ---------------------------------------------------------------------

        /// <summary>
        /// Returns the live instance, lazily creating a hidden GameObject if necessary.
        /// </summary>
        public static HitFeedback Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindFirstObjectByType<HitFeedback>(FindObjectsInactive.Include);
                if (_instance != null) return _instance;

                var go = new GameObject("[HitFeedback]");
                go.hideFlags = HideFlags.DontSave;
                _instance = go.AddComponent<HitFeedback>();
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[HitFeedback] Duplicate instance detected on '{name}' — destroying. Existing instance owns '{_instance.name}'.");
                Destroy(this);
                return;
            }
            _instance = this;

            if (_damagePopupPrefab == null)
            {
                _damagePopupPrefab = Resources.Load<GameObject>(_popupResourcePath);
                if (_damagePopupPrefab == null)
                {
                    Debug.LogWarning($"[HitFeedback] Damage popup prefab not assigned and Resources.Load('{_popupResourcePath}') returned null. Run Tartaria/Combat/Build Damage Popup Prefab to author it, then place under Resources/{_popupResourcePath}.prefab.");
                }
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDamaged += HandlePlayerDamaged;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDamaged -= HandlePlayerDamaged;
            StopAllManagedCoroutines();
            RestoreHitstopIfActive();
            RestoreShakeBaselineIfActive();
        }

        private void OnDestroy()
        {
            // Belt-and-braces cleanup (OnDisable normally fires first, but if the object
            // is destroyed mid-coroutine via Destroy() we must still rewind state).
            StopAllManagedCoroutines();
            RestoreHitstopIfActive();
            RestoreShakeBaselineIfActive();

            if (_instance == this) _instance = null;
        }

        // ---------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------

        /// <summary>
        /// Direct-call entry point for combat code at strike sites. Use this when
        /// the GameEvents pipeline does not carry the data you need (no OnEnemyHit
        /// exists). Fires hitstop, screen shake, and a floating damage number at
        /// the supplied world position.
        /// </summary>
        public static void NotifyHit(Vector3 pos, float dmg, bool isCrit)
        {
            if (!_enemyHitWarningLogged)
            {
                _enemyHitWarningLogged = true;
                Debug.LogWarning("[HitFeedback] No GameEvents.OnXxx found for enemy-side hits (grep confirmed OnEnemyHit/OnEnemyDamaged/OnDamageDealt do not exist) — direct-call mode active. Combat code must call HitFeedback.NotifyHit() at strike sites.");
            }

            Instance.DispatchHit(pos, dmg, isCrit);
        }

        /// <summary>
        /// Internal dispatcher shared by NotifyHit and the OnPlayerDamaged subscriber.
        /// </summary>
        private void DispatchHit(Vector3 worldPos, float dmg, bool isCrit)
        {
            // 1. Hitstop
            BeginHitstop();

            // 2. Screen shake
            BeginShake();

            // 3. Floating damage number
            SpawnDamagePopup(worldPos, dmg, isCrit);
        }

        // ---------------------------------------------------------------------
        // GameEvents wiring
        // ---------------------------------------------------------------------

        private void HandlePlayerDamaged(PlayerDamagedEventArgs args)
        {
            if (args == null)
            {
                Debug.LogWarning("[HitFeedback] HandlePlayerDamaged invoked with null args — skipping. Check GameEvents.RaisePlayerDamaged caller.");
                return;
            }

            Vector3 popupPos = ResolvePlayerWorldPosition();
            DispatchHit(popupPos, args.damageAmount, isCrit: false);
        }

        private static Vector3 ResolvePlayerWorldPosition()
        {
            // PlayerDamagedEventArgs does not include a hit position, so we fall back
            // to the player transform. If no Player tag exists in the scene we log and
            // return Vector3.zero so the popup is still visible somewhere.
            try
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) return go.transform.position + Vector3.up * 1.5f;
            }
            catch (UnityException ex)
            {
                Debug.LogWarning($"[HitFeedback] FindGameObjectWithTag('Player') threw UnityException (tag likely missing from TagManager): {ex.Message}");
            }

            Debug.LogWarning("[HitFeedback] Could not locate Player-tagged GameObject for damage popup position — using world origin.");
            return Vector3.zero;
        }

        // ---------------------------------------------------------------------
        // Hitstop
        // ---------------------------------------------------------------------

        private void BeginHitstop()
        {
            if (_isHitstopped)
            {
                // Stacking pinches would silently lose the original scale. Skip and log.
                return;
            }

            _originalTimeScale = Time.timeScale;
            _isHitstopped = true;
            Time.timeScale = _hitstopScale;

            var co = StartCoroutine(HitstopRoutine());
            _activeCoroutines.Add(co);
        }

        private IEnumerator HitstopRoutine()
        {
            yield return new WaitForSecondsRealtime(_hitstopDurationUnscaled);
            RestoreHitstopIfActive();
        }

        private void RestoreHitstopIfActive()
        {
            if (!_isHitstopped) return;
            Time.timeScale = _originalTimeScale;
            _isHitstopped = false;
        }

        // ---------------------------------------------------------------------
        // Screen shake
        // ---------------------------------------------------------------------

        private void BeginShake()
        {
            // global::UnityEngine.Camera explicitly guards against Tartaria.Camera namespace shadow.
            var cam = global::UnityEngine.Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[HitFeedback] global::UnityEngine.Camera.main is null — screen shake skipped. Tag a camera as MainCamera in the scene.");
                return;
            }

            if (_isShaking && _shakeCamera != null)
            {
                // A shake is already running; restart at full amplitude from current baseline.
                // We do NOT update _shakeBaselineLocalPos because the running coroutine will
                // restore to the original baseline when it completes.
            }
            else
            {
                _shakeCamera = cam;
                _shakeBaselineLocalPos = cam.transform.localPosition;
                _isShaking = true;
            }

            var co = StartCoroutine(ShakeRoutine());
            _activeCoroutines.Add(co);
        }

        private IEnumerator ShakeRoutine()
        {
            float elapsed = 0f;
            while (elapsed < _shakeDuration)
            {
                if (_shakeCamera == null) yield break;

                float t = elapsed / _shakeDuration;
                // smoothstep falloff: amplitude(1-t)^2 * (3-2(1-t))
                float u = 1f - t;
                float falloff = u * u * (3f - 2f * u);
                float amp = _shakeAmplitude * falloff;

                Vector3 offset = new Vector3(
                    (UnityEngine.Random.value - 0.5f) * 2f * amp,
                    (UnityEngine.Random.value - 0.5f) * 2f * amp,
                    0f
                );
                _shakeCamera.transform.localPosition = _shakeBaselineLocalPos + offset;

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            RestoreShakeBaselineIfActive();
        }

        private void RestoreShakeBaselineIfActive()
        {
            if (!_isShaking) return;
            if (_shakeCamera != null)
            {
                _shakeCamera.transform.localPosition = _shakeBaselineLocalPos;
            }
            _shakeCamera = null;
            _isShaking = false;
        }

        // ---------------------------------------------------------------------
        // Damage popup
        // ---------------------------------------------------------------------

        private void SpawnDamagePopup(Vector3 worldPos, float dmg, bool isCrit)
        {
            if (_damagePopupPrefab == null)
            {
                // Don't spam — warning was logged once in Awake. Skip silently here
                // (but document with a one-shot debug aside if useful).
                return;
            }

            GameObject popup;
            try
            {
                popup = Instantiate(_damagePopupPrefab, worldPos, Quaternion.identity);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HitFeedback] Instantiate('{_damagePopupPrefab.name}') threw at {worldPos}: {ex}");
                return;
            }

            var popupComponent = popup.GetComponent<DamagePopup>();
            if (popupComponent == null)
            {
                Debug.LogWarning($"[HitFeedback] Prefab '{_damagePopupPrefab.name}' is missing DamagePopup component — adding at runtime so it still renders.");
                popupComponent = popup.AddComponent<DamagePopup>();
            }
            popupComponent.Configure(dmg, isCrit);
        }

        // ---------------------------------------------------------------------
        // Coroutine bookkeeping
        // ---------------------------------------------------------------------

        private void StopAllManagedCoroutines()
        {
            for (int i = 0; i < _activeCoroutines.Count; i++)
            {
                if (_activeCoroutines[i] != null) StopCoroutine(_activeCoroutines[i]);
            }
            _activeCoroutines.Clear();
        }
    }
}
