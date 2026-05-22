using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.Audio
{
    /// <summary>
    /// Audio Mixer Controller — runtime volume control for master/music/sfx/voice.
    /// Attach to AudioManager or create as singleton. Exposes API for settings menu.
    /// </summary>
    public class AudioMixerController : MonoBehaviour
    {
        public static AudioMixerController Instance { get; private set; }

        [Header("Mixer References")]
        [SerializeField] AudioMixer mainMixer;
        [SerializeField] string masterVolumeParam = "MasterVolume";
        [SerializeField] string musicVolumeParam = "MusicVolume";
        [SerializeField] string sfxVolumeParam = "SFXVolume";
        [SerializeField] string voiceVolumeParam = "VoiceVolume";

        [Header("Default Volumes (0-1)")]
        [SerializeField, Range(0f, 1f)] float defaultMasterVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] float defaultMusicVolume = 0.6f;
        [SerializeField, Range(0f, 1f)] float defaultSFXVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] float defaultVoiceVolume = 1f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            LoadVolumeSettings();
        }

        // === Public API ===

        public void SetMasterVolume(float volume)
        {
            SetVolume(masterVolumeParam, volume);
            SaveVolumeSetting("MasterVolume", volume);
        }

        public void SetMusicVolume(float volume)
        {
            SetVolume(musicVolumeParam, volume);
            SaveVolumeSetting("MusicVolume", volume);
        }

        public void SetSFXVolume(float volume)
        {
            SetVolume(sfxVolumeParam, volume);
            SaveVolumeSetting("SFXVolume", volume);
        }

        public void SetVoiceVolume(float volume)
        {
            SetVolume(voiceVolumeParam, volume);
            SaveVolumeSetting("VoiceVolume", volume);
        }

        void SetVolume(string paramName, float volume)
        {
            if (mainMixer == null) return;

            // Convert 0-1 to decibels (-80dB to 0dB)
            float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(volume));
            if (volume <= 0.001f) db = -80f;  // Silence threshold

            mainMixer.SetFloat(paramName, db);
        }

        float GetVolume(string paramName)
        {
            if (mainMixer == null) return 0.5f;

            float db;
            if (mainMixer.GetFloat(paramName, out db))
            {
                // Convert decibels to 0-1
                return Mathf.InverseLerp(-80f, 0f, db);
            }

            return 0.5f;
        }

        void LoadVolumeSettings()
        {
            // Load from PlayerPrefs (TODO: integrate with SaveManager)
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);
            float voiceVol = PlayerPrefs.GetFloat("VoiceVolume", defaultVoiceVolume);

            SetVolume(masterVolumeParam, masterVol);
            SetVolume(musicVolumeParam, musicVol);
            SetVolume(sfxVolumeParam, sfxVol);
            SetVolume(voiceVolumeParam, voiceVol);

            Debug.Log($"[AudioMixerController] Loaded volumes: Master={masterVol:F2}, Music={musicVol:F2}, SFX={sfxVol:F2}, Voice={voiceVol:F2}");
        }

        void SaveVolumeSetting(string key, float volume)
        {
            PlayerPrefs.SetFloat(key, volume);
            PlayerPrefs.Save();
        }

        public void ResetToDefaults()
        {
            SetMasterVolume(defaultMasterVolume);
            SetMusicVolume(defaultMusicVolume);
            SetSFXVolume(defaultSFXVolume);
            SetVoiceVolume(defaultVoiceVolume);

            Debug.Log("[AudioMixerController] Reset to defaults");
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
