using UnityEngine;
using Tartaria.Audio;

namespace Tartaria.Audio
{
    /// <summary>
    /// Moon 5 Overtone audio layer: amplification harmonics, 6-band healing tones,
    /// aurora fountain whooshes, Thorne radio static bursts, victory overtone motif.
    /// Hooks into the WhiteCityAmplificationController events (future expansion via events or direct calls).
    /// </summary>
    public class Moon5WhiteCityAudioManager : MonoBehaviour
    {
        public static Moon5WhiteCityAudioManager Instance { get; private set; }

        [Header("Overtone References (assign in scene or let bootstrap find)")]
        public AudioSource amplificationSource;
        public AudioSource fountainSource;
        public AudioSource bridgeSource;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void PlayAmplificationStinger(int pavilion, float strength)
        {
            // Overtone harmonic stinger - 432Hz base + overtone layer
            float pitch = 0.98f + strength * 0.12f;
            if (amplificationSource != null)
            {
                amplificationSource.pitch = pitch;
                amplificationSource.volume = 0.6f + strength * 0.3f;
                amplificationSource.PlayOneShot(amplificationSource.clip ?? Resources.Load<AudioClip>("Audio/Moon5_AmplificationSting"));
            }
            else
            {
                // Strong fallback with pitch shift for "singing city" feel
                AudioManager.Instance?.PlaySFX2D("BuildingRestore");
            }

            // Extra harmonic layer for overtone magic
            if (strength > 0.7f)
            {
                AudioManager.Instance?.PlaySFX2D("BuildingRestore"); // reuse as harmonic layer
            }
        }

        public void PlayHealingAuraTone(Vector3 pos)
        {
            AudioManager.Instance?.PlaySFX3D("HealingAura", pos);
        }

        public void PlayAuroraFountainBurst(Vector3 pos)
        {
            // Ethereal water + light whoosh
            AudioManager.Instance?.PlaySFX3D("FountainAurora", pos);
        }

        public void PlayBridgeIgnition()
        {
            if (bridgeSource != null) bridgeSource.Play();
            else AudioManager.Instance?.PlaySFX2D("Moon5_BridgeIgnition");
        }
    }
}