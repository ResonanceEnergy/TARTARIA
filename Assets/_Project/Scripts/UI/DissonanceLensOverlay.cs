using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.UI
{
    /// <summary>
    /// Dissonance Lens Overlay -- toggleable corruption detection UI.
    ///
    /// When active, reveals dark fractal patterns on corrupted buildings
    /// and highlights corruption sources with a pulsing red-violet glow.
    ///
    /// Visual approach: full-screen post-process-style overlay rendered
    /// via IMGUI (Canvas-free).  In production, this would be a URP
    /// Renderer Feature with a custom shader pass.
    ///
    /// Activation: Tab (Aether Vision) when Dissonance Lens item is acquired.
    /// Aether cost: 2/second while active.
    /// </summary>
    [DisallowMultipleComponent]
    public class DissonanceLensOverlay : MonoBehaviour
    {
        public static DissonanceLensOverlay Instance { get; private set; }

        [Header("Lens Settings")]
        [SerializeField] float aetherCostPerSecond = 2f;
        [SerializeField] float scanPulseInterval = 1.5f;
#pragma warning disable CS0414
        [SerializeField] float scanRadius = 50f;
#pragma warning restore CS0414
        [SerializeField] Color corruptionTint = new(0.6f, 0.1f, 0.4f, 0.3f);
        [SerializeField] Color cleanTint = new(0.2f, 0.8f, 0.6f, 0.15f);

        bool _active;
        bool _unlocked;
        float _pulseTimer;
        float _pulseAlpha;
        float _aetherCharge;
        Texture2D _overlayTexture;

        // Detected corruption data (updated each pulse)
        readonly System.Collections.Generic.List<CorruptionMarker> _markers = new();

        // ─── Frequency Match State ───
        float _targetFrequency;
        float _currentFrequency;
        float _frequencyMatchThreshold = 5f;   // Hz tolerance for a "match"
        bool _isFrequencyLocked;
        ICorruptible _purgeTarget;
        float _purgeBeamTimer;
        const float PURGE_BEAM_RATE = 10f;     // corruption removed per second when locked

        public bool IsActive => _active;
        public bool IsUnlocked => _unlocked;

        public event System.Action<bool> OnLensToggled;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Create a 1x1 white texture for drawing
            _overlayTexture = new Texture2D(1, 1);
            _overlayTexture.SetPixel(0, 0, Color.white);
            _overlayTexture.Apply();
        }

        /// <summary>
        /// Unlock the Dissonance Lens (e.g., quest reward in Moon 2).
        /// </summary>
        public void Unlock()
        {
            _unlocked = true;
            Debug.Log("[DissonanceLens] Unlocked!");
        }

        /// <summary>
        /// Toggle lens on/off. Called by Aether Vision input.
        /// </summary>
        public void Toggle(float currentAether)
        {
            if (!_unlocked) return;

            _active = !_active;
            _aetherCharge = currentAether;

            if (_active)
            {
                _pulseTimer = 0f;
                PerformScan();
            }

            OnLensToggled?.Invoke(_active);
            Debug.Log($"[DissonanceLens] {(_active ? "Activated" : "Deactivated")}");
        }

        void Update()
        {
            if (!_active) return;

            // Drain aether
            _aetherCharge -= aetherCostPerSecond * Time.deltaTime;
            if (_aetherCharge <= 0f)
            {
                _active = false;
                _aetherCharge = 0f;
                OnLensToggled?.Invoke(false);
                return;
            }

            // Scan pulse animation
            _pulseTimer += Time.deltaTime;
            _pulseAlpha = Mathf.PingPong(_pulseTimer / scanPulseInterval, 1f);

            if (_pulseTimer >= scanPulseInterval)
            {
                _pulseTimer = 0f;
                PerformScan();
            }
        }

        void PerformScan()
        {
            _markers.Clear();

            // Find all buildings and check corruption levels
            var buildings = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var obj in buildings)
            {
                // Check for corruption component (added by CorruptionSystem)
                var corruptible = obj as ICorruptible;
                if (corruptible != null && corruptible.CorruptionLevel > 0f)
                {
                    _markers.Add(new CorruptionMarker
                    {
                        worldPosition = obj.transform.position,
                        corruptionLevel = corruptible.CorruptionLevel,
                        name = obj.name
                    });
                }
            }
        }

        void OnGUI()
        {
            if (!_active) return;

            // Full-screen tint overlay
            Color tint = Color.Lerp(cleanTint, corruptionTint, _pulseAlpha * 0.5f);
            GUI.color = tint;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _overlayTexture);

            // Draw corruption markers
            var cam = Camera.main;
            if (cam == null) return;

            var markerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            foreach (var marker in _markers)
            {
                Vector3 screenPos = cam.WorldToScreenPoint(marker.worldPosition);
                if (screenPos.z <= 0) continue; // Behind camera

                float screenY = Screen.height - screenPos.y; // GUI Y is inverted
                float size = Mathf.Lerp(20f, 60f, marker.corruptionLevel);
                float alpha = Mathf.Lerp(0.3f, 1f, marker.corruptionLevel) * _pulseAlpha;

                // Corruption ring
                GUI.color = new Color(0.8f, 0.1f, 0.3f, alpha);
                GUI.DrawTexture(
                    new Rect(screenPos.x - size * 0.5f, screenY - size * 0.5f, size, size),
                    _overlayTexture);

                // Label
                GUI.color = new Color(1f, 0.3f, 0.5f, alpha);
                GUI.Label(
                    new Rect(screenPos.x - 80f, screenY + size * 0.5f, 160f, 20f),
                    $"<b>{marker.name}</b> [{marker.corruptionLevel:P0}]",
                    markerStyle);
            }

            // Reset GUI color
            GUI.color = Color.white;

            // HUD indicator
            GUI.Label(new Rect(10, Screen.height - 30, 300, 25),
                $"<color=#FF66AA><b>DISSONANCE LENS</b> Aether: {_aetherCharge:F0}</color>",
                markerStyle);
        }

        void OnDestroy()
        {
            if (_overlayTexture != null) Destroy(_overlayTexture);
        }

        // ─── Frequency Match Targeting ───────────────

        /// <summary>
        /// Set the player's current tuning frequency (from input dial).
        /// When this matches a corruption source's weak point, the purge beam locks on.
        /// </summary>
        public void SetFrequency(float frequency)
        {
            _currentFrequency = frequency;

            // Check for lock-on against nearest corruption weak point
            _isFrequencyLocked = false;
            _purgeTarget = null;

            if (!_active) return;

            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            float bestDist = float.MaxValue;
            foreach (var marker in _markers)
            {
                float screenDist = Vector3.Distance(
                    cam.WorldToViewportPoint(marker.worldPosition),
                    new Vector3(0.5f, 0.5f, 0f));

                if (screenDist > 0.3f) continue; // Must be roughly centered

                // Check if this object has a weak point with matching frequency
                var buildings = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var obj in buildings)
                {
                    if (obj is ICorruptionWeakPoint wp && obj is ICorruptible corruptible)
                    {
                        if (!wp.IsWeakPointExposed) continue;
                        if (Mathf.Abs(wp.WeakPointFrequency - _currentFrequency) <= _frequencyMatchThreshold)
                        {
                            float dist = Vector3.Distance(cam.transform.position, wp.WeakPointWorldPosition);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                _isFrequencyLocked = true;
                                _purgeTarget = corruptible;
                                _targetFrequency = wp.WeakPointFrequency;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fire the purge beam while frequency-locked. Call each frame while player holds purge button.
        /// </summary>
        public void FirePurgeBeam(float deltaTime)
        {
            if (!_active || !_isFrequencyLocked || _purgeTarget == null) return;

            float purgeAmount = PURGE_BEAM_RATE * deltaTime;
            _purgeTarget.PurgeCorruption(purgeAmount);

            _purgeBeamTimer += deltaTime;

            // VFX on purge
            if (_purgeBeamTimer > 0.5f)
            {
                _purgeBeamTimer = 0f;
                HapticFeedbackManager.Instance?.PlayTuningOnFrequency();
            }

            // Aether drain while purging
            _aetherCharge -= aetherCostPerSecond * 0.5f * deltaTime; // extra drain

            if (_purgeTarget.CorruptionLevel <= 0f)
            {
                _isFrequencyLocked = false;
                _purgeTarget = null;
                HapticFeedbackManager.Instance?.PlayPerfectTune();
                Debug.Log("[DissonanceLens] Corruption purged from target!");
            }
        }

        public bool IsFrequencyLocked => _isFrequencyLocked;
        public float TargetFrequency => _targetFrequency;

        struct CorruptionMarker
        {
            public Vector3 worldPosition;
            public float corruptionLevel;
            public string name;
        }
    }

    /// <summary>
    /// Interface for objects that can be corrupted.
    /// Implemented by building controllers, zone objects, etc.
    /// </summary>
    public interface ICorruptible
    {
        float CorruptionLevel { get; }
        void ApplyCorruption(float amount);
        void PurgeCorruption(float amount);
    }

    /// <summary>
    /// Frequency-match targeting for purge beam integration.
    /// Extend ICorruptible objects with weak-point data.
    /// </summary>
    public interface ICorruptionWeakPoint
    {
        float WeakPointFrequency { get; }
        Vector3 WeakPointWorldPosition { get; }
        bool IsWeakPointExposed { get; }
    }
}
