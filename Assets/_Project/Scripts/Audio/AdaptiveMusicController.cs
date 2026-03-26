using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Audio
{
    /// <summary>
    /// Adaptive Music Controller — 2-layer prototype:
    ///
    /// Layer 1 (Bed): Always active, crossfades based on RS
    ///   RS 0–50:  Minor key, sparse, melancholic (solo cello at 432 Hz)
    ///   RS 50–100: Major key, lush, hopeful (full strings + crystalline synth)
    ///
    /// Layer 2 (Reactive): Event-driven
    ///   Discovery: ascending arpeggio (harp)
    ///   Tuning:    real-time frequency matching
    ///   Combat:    percussive rhythm, dissonant overtones
    ///   Restoration: expanding harmonic series (brass + choir)
    ///
    /// All content tuned to A4 = 432 Hz.
    /// Budget: 0.5ms per frame.
    /// </summary>
    public class AdaptiveMusicController : MonoBehaviour
    {
        public static AdaptiveMusicController Instance { get; private set; }

        [Header("Bed Layers")]
        [SerializeField] AudioClip bedLayerLow;      // RS 0–50: melancholic
        [SerializeField] AudioClip bedLayerHigh;      // RS 50–100: hopeful

        [Header("Reactive Stingers")]
        [SerializeField] AudioClip discoveryStinger;
        [SerializeField] AudioClip combatStinger;
        [SerializeField] AudioClip restorationStinger;
        [SerializeField] AudioClip zoneShiftStinger;

        [Header("Config")]
        [SerializeField] float crossfadeSpeed = 1.0f;
        [SerializeField] float bedVolume = 0.5f;
        [SerializeField] float stingerVolume = 0.7f;

        AudioSource _bedSourceA;
        AudioSource _bedSourceB;
        AudioSource _stingerSource;
        float _currentRS;
        float _targetBlend;  // 0 = low layer, 1 = high layer

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupAudioSources();
        }

        void SetupAudioSources()
        {
            _bedSourceA = gameObject.AddComponent<AudioSource>();
            _bedSourceA.loop = true;
            _bedSourceA.clip = bedLayerLow;
            _bedSourceA.volume = bedVolume;
            _bedSourceA.playOnAwake = false;

            _bedSourceB = gameObject.AddComponent<AudioSource>();
            _bedSourceB.loop = true;
            _bedSourceB.clip = bedLayerHigh;
            _bedSourceB.volume = 0f;
            _bedSourceB.playOnAwake = false;

            _stingerSource = gameObject.AddComponent<AudioSource>();
            _stingerSource.loop = false;
            _stingerSource.playOnAwake = false;
        }

        void Start()
        {
            if (bedLayerLow != null) _bedSourceA.Play();
            if (bedLayerHigh != null) _bedSourceB.Play();
        }

        void Update()
        {
            // Crossfade based on RS
            _targetBlend = Mathf.Clamp01(_currentRS / 100f);

            float blendA = (1f - _targetBlend) * bedVolume;
            float blendB = _targetBlend * bedVolume;

            _bedSourceA.volume = Mathf.Lerp(_bedSourceA.volume, blendA,
                Time.deltaTime * crossfadeSpeed);
            _bedSourceB.volume = Mathf.Lerp(_bedSourceB.volume, blendB,
                Time.deltaTime * crossfadeSpeed);
        }

        // ─── Public API ──────────────────────────────

        public void UpdateResonanceScore(float rs)
        {
            _currentRS = rs;
        }

        public void PlayDiscovery()
        {
            PlayStinger(discoveryStinger);
        }

        public void PlayCombatStart()
        {
            PlayStinger(combatStinger);
        }

        public void PlayRestoration()
        {
            PlayStinger(restorationStinger);
        }

        public void PlayZoneShift()
        {
            PlayStinger(zoneShiftStinger);
        }

        void PlayStinger(AudioClip clip)
        {
            if (clip == null || _stingerSource == null) return;
            _stingerSource.PlayOneShot(clip, stingerVolume);
        }
    }
}
