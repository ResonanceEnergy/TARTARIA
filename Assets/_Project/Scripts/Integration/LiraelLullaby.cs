using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// LiraelLullaby — Lirael's 432 Hz lullaby + first appearance trigger.
    /// Per docs/03_CAMPAIGN_13_MOONS.md Days 25–28 (Moon 1 Revelation):
    /// "Lirael appears — translucent, humming a lullaby in 432 Hz. She doesn't
    /// remember her name, only the song."
    ///
    /// Attached to the Lirael NPC GameObject by Moon1BuildOutNPCs. On scene start
    /// she hums a faint loop. When the player gets close (~5m), the hum swells
    /// and an HUD whisper banner shows her line: "Why do grown-ups build houses
    /// then live in the attic?"
    ///
    /// Audio is procedurally synthesized 432 Hz sine + soft envelope so we don't
    /// depend on an external vocal clip. Spatial AudioSource so the player can
    /// triangulate her location by sound.
    /// </summary>
    public class LiraelLullaby : MonoBehaviour
    {
        [Header("Lullaby tuning")]
        [SerializeField] private float fundamentalHz = 432f;
        [SerializeField] private float baseVolume = 0.18f;
        [SerializeField] private float closeVolume = 0.55f;
        [SerializeField] private float proximityRadius = 5f;
        [SerializeField] private float maxAudibleRadius = 22f;

        [Header("Reveal lines (random pool — picks one per close-approach)")]
        [SerializeField] private string[] whisperLines = new string[]
        {
            "Why do grown-ups build houses then live in the attic?",
            "The song… it's broken. But you can hear it, can't you?",
            "I knew the words once. Now there's only humming.",
            "If you finish the bridge, will I remember my name?",
        };

        private AudioSource _audio;
        private Transform _player;
        private bool _hasRevealedOnce;
        private float _nextWhisperAt;

        void Awake()
        {
            // Spatial audio source — players locate her by hearing her hum
            _audio = gameObject.GetComponent<AudioSource>();
            if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
            _audio.clip = GenerateLullabyClip();
            _audio.loop = true;
            _audio.playOnAwake = true;
            _audio.spatialBlend = 1f; // 3D
            _audio.rolloffMode = AudioRolloffMode.Linear;
            _audio.minDistance = 2f;
            _audio.maxDistance = maxAudibleRadius;
            _audio.volume = baseVolume;
            _audio.priority = 80;
            _audio.Play();
        }

        void Update()
        {
            if (_player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p == null) return;
                _player = p.transform;
            }

            float dist = Vector3.Distance(transform.position, _player.position);

            // Swell volume as player approaches
            float t = Mathf.InverseLerp(maxAudibleRadius, proximityRadius, dist);
            _audio.volume = Mathf.Lerp(baseVolume, closeVolume, t);

            // Trigger reveal whisper when player crosses proximityRadius
            if (dist <= proximityRadius && Time.time >= _nextWhisperAt)
            {
                _nextWhisperAt = Time.time + 15f; // cooldown so it doesn't spam
                ShowWhisper();
            }
        }

        void ShowWhisper()
        {
            if (whisperLines == null || whisperLines.Length == 0) return;
            string line = whisperLines[Random.Range(0, whisperLines.Length)];
            string title = _hasRevealedOnce ? "Lirael (humming)" : "Lirael (translucent, humming)";
            ServiceLocator.HUD?.ShowBanner(title, $"\"{line}\"", 6f);

            if (!_hasRevealedOnce)
            {
                _hasRevealedOnce = true;
                // First reveal — show an objective banner. (GameEvents has no
                // dedicated objective channel yet; ShowBanner via ServiceLocator
                // is the canonical path used across Moon 1 systems.)
                ServiceLocator.HUD?.ShowBanner("Objective", "Lirael's lullaby leads you forward.", 6f);
                Debug.Log("[LiraelLullaby] First reveal — 432 Hz lullaby identified.");
            }
        }

        /// <summary>
        /// Procedural 432 Hz hum: fundamental + perfect-fifth (648 Hz) + octave-up
        /// (864 Hz) at low amplitudes, gentle 3-second amplitude envelope so the
        /// loop breathes. 88200 samples = 2 sec at 44.1 kHz — short loop, less RAM.
        /// </summary>
        AudioClip GenerateLullabyClip()
        {
            const int sampleRate = 44100;
            const float duration = 4f; // 4-second loop
            int samples = (int)(sampleRate * duration);
            var clip = AudioClip.Create("LiraelLullaby_432Hz", samples, 1, sampleRate, false);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Slow tremolo envelope — 0.15 Hz so the hum breathes over ~6 sec
                float env = 0.65f + 0.35f * Mathf.Sin(2f * Mathf.PI * 0.15f * t);
                float fundamental = Mathf.Sin(2f * Mathf.PI * fundamentalHz * t);
                float fifth = Mathf.Sin(2f * Mathf.PI * fundamentalHz * 1.5f * t) * 0.35f;
                float octaveUp = Mathf.Sin(2f * Mathf.PI * fundamentalHz * 2f * t) * 0.18f;
                data[i] = (fundamental + fifth + octaveUp) * 0.30f * env;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
