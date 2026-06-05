using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.Audio
{
    /// <summary>
    /// Audio Mixer Controller — runtime volume control for the six canonical mixer buses.
    /// Exposed param names MUST match Assets/_Project/Audio/Mixers/MasterMixer.mixer:
    /// MasterVol, MusicVol, SFXVol, UIVol, AmbienceVol, VoiceVol. (API_CONTRACT.md §4)
    /// Attach to AudioManager or create as singleton. Exposes API for settings menu.
    /// </summary>
    public class AudioMixerController : MonoBehaviour
    {
        public static AudioMixerController Instance { get; private set; }

        [Header("Mixer References")]
        [SerializeField] AudioMixer mainMixer;
        [SerializeField] string masterVolumeParam = "MasterVol";
        [SerializeField] string musicVolumeParam = "MusicVol";
        [SerializeField] string sfxVolumeParam = "SFXVol";
        [SerializeField] string uiVolumeParam = "UIVol";
        [SerializeField] string ambienceVolumeParam = "AmbienceVol";
        [SerializeField] string voiceVolumeParam = "VoiceVol";

        [Header("Default Volumes (0-1)")]
        [SerializeField, Range(0f, 1f)] float defaultMasterVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] float defaultMusicVolume = 0.6f;
        [SerializeField, Range(0f, 1f)] float defaultSFXVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] float defaultUIVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] float defaultAmbienceVolume = 0.7f;
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
            SaveVolumeSetting("MasterVol", volume);
        }

        public void SetMusicVolume(float volume)
        {
            SetVolume(musicVolumeParam, volume);
            SaveVolumeSetting("MusicVol", volume);
        }

        public void SetSFXVolume(float volume)
        {
            SetVolume(sfxVolumeParam, volume);
            SaveVolumeSetting("SFXVol", volume);
        }

        public void SetUIVolume(float volume)
        {
            SetVolume(uiVolumeParam, volume);
            SaveVolumeSetting("UIVol", volume);
        }

        public void SetAmbienceVolume(float volume)
        {
            SetVolume(ambienceVolumeParam, volume);
            SaveVolumeSetting("AmbienceVol", volume);
        }

        public void SetVoiceVolume(float volume)
        {
            SetVolume(voiceVolumeParam, volume);
            SaveVolumeSetting("VoiceVol", volume);
        }

        public float GetMasterVolume()   => GetVolume(masterVolumeParam);
        public float GetMusicVolume()    => GetVolume(musicVolumeParam);
        public float GetSFXVolume()      => GetVolume(sfxVolumeParam);
        public float GetUIVolume()       => GetVolume(uiVolumeParam);
        public float GetAmbienceVolume() => GetVolume(ambienceVolumeParam);
        public float GetVoiceVolume()    => GetVolume(voiceVolumeParam);

        void SetVolume(string paramName, float volume)
        {
            if (mainMixer == null) return;

            // Convert 0-1 to decibels (-80dB to 0dB)
            float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(volume));
            if (volume <= 0.001f) db = -80f;  // Silence threshold

            mainMixer.SetFloat(paramName, db);
            Debug.Log($"[AudioMixerCtrl] {paramName}={db:F1}");
        }

        float GetVolume(string paramName)
        {
            if (mainMixer == null) return 0.5f;

            if (mainMixer.GetFloat(paramName, out float db))
            {
                // Convert decibels to 0-1
                return Mathf.InverseLerp(-80f, 0f, db);
            }

            return 0.5f;
        }

        void LoadVolumeSettings()
        {
            // Load from PlayerPrefs (SaveManager integration tracked in KNOWN_PLACEHOLDERS.md)
            float masterVol   = PlayerPrefs.GetFloat("MasterVol",   defaultMasterVolume);
            float musicVol    = PlayerPrefs.GetFloat("MusicVol",    defaultMusicVolume);
            float sfxVol      = PlayerPrefs.GetFloat("SFXVol",      defaultSFXVolume);
            float uiVol       = PlayerPrefs.GetFloat("UIVol",       defaultUIVolume);
            float ambienceVol = PlayerPrefs.GetFloat("AmbienceVol", defaultAmbienceVolume);
            float voiceVol    = PlayerPrefs.GetFloat("VoiceVol",    defaultVoiceVolume);

            SetVolume(masterVolumeParam,   masterVol);
            SetVolume(musicVolumeParam,    musicVol);
            SetVolume(sfxVolumeParam,      sfxVol);
            SetVolume(uiVolumeParam,       uiVol);
            SetVolume(ambienceVolumeParam, ambienceVol);
            SetVolume(voiceVolumeParam,    voiceVol);

            Debug.Log($"[AudioMixerController] Loaded volumes: Master={masterVol:F2}, Music={musicVol:F2}, SFX={sfxVol:F2}, UI={uiVol:F2}, Ambience={ambienceVol:F2}, Voice={voiceVol:F2}");
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
            SetUIVolume(defaultUIVolume);
            SetAmbienceVolume(defaultAmbienceVolume);
            SetVoiceVolume(defaultVoiceVolume);

            Debug.Log("[AudioMixerController] Reset to defaults");
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
