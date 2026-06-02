using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.Audio
{
    /// <summary>
    /// AudioMixerBridge — exposes AudioMixer parameters to code + UI sliders.
    /// Handles volume (linear→dB conversion), pitch, effects wet/dry, snapshots.
    /// Integrates with Settings UI for Master/Music/SFX/Voice volume controls.
    /// 
    /// Exposed Parameters (must match AudioMixer):
    /// - MasterVol (-80 to 0 dB)
    /// - MusicVol (-80 to 0 dB)
    /// - SFXVol (-80 to 0 dB)
    /// - VoiceVol (-80 to 0 dB)
    /// - ReverbWet (0 to 1)
    /// - LowPassCutoff (Hz)
    /// 
    /// Usage:
    /// - Attach to AudioManager GameObject
    /// - Assign MasterAudioMixer reference
    /// - Call SetMasterVolume(0-1) from settings UI
    /// - Call SetSnapshot("Combat") to switch audio profile
    /// 
    /// GDD refs: §31 (AudioMixer Setup), §05 (Audio Settings)
    /// </summary>
    public class AudioMixerBridge : MonoBehaviour
    {
        public static AudioMixerBridge Instance { get; private set; }

        [Header("Mixer Reference")]
        [SerializeField] AudioMixer masterMixer;

        [Header("Exposed Parameters (must match AudioMixer)")]
        [SerializeField] string masterVolumeParam = "MasterVol";
        [SerializeField] string musicVolumeParam = "MusicVol";
        [SerializeField] string sfxVolumeParam = "SFXVol";
        [SerializeField] string voiceVolumeParam = "VoiceVol";

        [Header("Snapshots")]
        [SerializeField] AudioMixerSnapshot defaultSnapshot;
        [SerializeField] AudioMixerSnapshot combatSnapshot;
        [SerializeField] AudioMixerSnapshot cinematicSnapshot;
        [SerializeField] AudioMixerSnapshot underwaterSnapshot;
        [SerializeField] float snapshotTransitionTime = 1f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (masterMixer == null)
            {
                Debug.LogError("[AudioMixerBridge] No AudioMixer assigned!");
                enabled = false;
                return;
            }

            // Load saved volumes from PlayerPrefs
            LoadVolumeSettings();

            Debug.Log("[AudioMixerBridge] Initialized");
        }

        /// <summary>
        /// Set master volume (0-1 linear scale, converted to dB).
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            float dB = LinearToDecibel(volume);
            masterMixer.SetFloat(masterVolumeParam, dB);
            PlayerPrefs.SetFloat(masterVolumeParam, volume);

            Debug.Log($"[AudioMixerBridge] Master volume: {volume:F2} ({dB:F1} dB)");
        }

        /// <summary>
        /// Set music volume (0-1 linear scale).
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            float dB = LinearToDecibel(volume);
            masterMixer.SetFloat(musicVolumeParam, dB);
            PlayerPrefs.SetFloat(musicVolumeParam, volume);
        }

        /// <summary>
        /// Set SFX volume (0-1 linear scale).
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            float dB = LinearToDecibel(volume);
            masterMixer.SetFloat(sfxVolumeParam, dB);
            PlayerPrefs.SetFloat(sfxVolumeParam, volume);
        }

        /// <summary>
        /// Set voice volume (0-1 linear scale).
        /// </summary>
        public void SetVoiceVolume(float volume)
        {
            float dB = LinearToDecibel(volume);
            masterMixer.SetFloat(voiceVolumeParam, dB);
            PlayerPrefs.SetFloat(voiceVolumeParam, volume);
        }

        /// <summary>
        /// Get master volume (0-1 linear scale).
        /// </summary>
        public float GetMasterVolume()
        {
            if (masterMixer.GetFloat(masterVolumeParam, out float dB))
            {
                return DecibelToLinear(dB);
            }
            return 1f;
        }

        /// <summary>
        /// Get music volume (0-1 linear scale).
        /// </summary>
        public float GetMusicVolume()
        {
            if (masterMixer.GetFloat(musicVolumeParam, out float dB))
            {
                return DecibelToLinear(dB);
            }
            return 1f;
        }

        /// <summary>
        /// Get SFX volume (0-1 linear scale).
        /// </summary>
        public float GetSFXVolume()
        {
            if (masterMixer.GetFloat(sfxVolumeParam, out float dB))
            {
                return DecibelToLinear(dB);
            }
            return 1f;
        }

        /// <summary>
        /// Get voice volume (0-1 linear scale).
        /// </summary>
        public float GetVoiceVolume()
        {
            if (masterMixer.GetFloat(voiceVolumeParam, out float dB))
            {
                return DecibelToLinear(dB);
            }
            return 1f;
        }

        /// <summary>
        /// Set mixer snapshot by name.
        /// </summary>
        public void SetSnapshot(string snapshotName)
        {
            AudioMixerSnapshot snapshot = snapshotName.ToLower() switch
            {
                "default" => defaultSnapshot,
                "combat" => combatSnapshot,
                "cinematic" => cinematicSnapshot,
                "underwater" => underwaterSnapshot,
                _ => defaultSnapshot
            };

            if (snapshot != null)
            {
                snapshot.TransitionTo(snapshotTransitionTime);
                Debug.Log($"[AudioMixerBridge] Transitioning to '{snapshotName}' snapshot ({snapshotTransitionTime}s)");
            }
            else
            {
                Debug.LogWarning($"[AudioMixerBridge] Snapshot '{snapshotName}' not assigned");
            }
        }

        /// <summary>
        /// Reset to default snapshot.
        /// </summary>
        public void ResetToDefaultSnapshot()
        {
            SetSnapshot("default");
        }

        /// <summary>
        /// Set exposed parameter by name (generic).
        /// </summary>
        public void SetParameter(string paramName, float value)
        {
            if (masterMixer.SetFloat(paramName, value))
            {
                Debug.Log($"[AudioMixerBridge] Set {paramName} = {value}");
            }
            else
            {
                Debug.LogWarning($"[AudioMixerBridge] Failed to set parameter '{paramName}'");
            }
        }

        /// <summary>
        /// Get exposed parameter by name (generic).
        /// </summary>
        public float GetParameter(string paramName)
        {
            if (masterMixer.GetFloat(paramName, out float value))
            {
                return value;
            }
            Debug.LogWarning($"[AudioMixerBridge] Failed to get parameter '{paramName}'");
            return 0f;
        }

        void LoadVolumeSettings()
        {
            // Load from PlayerPrefs, default to 1.0 if not set
            float master = PlayerPrefs.GetFloat(masterVolumeParam, 1f);
            float music = PlayerPrefs.GetFloat(musicVolumeParam, 0.8f);
            float sfx = PlayerPrefs.GetFloat(sfxVolumeParam, 1f);
            float voice = PlayerPrefs.GetFloat(voiceVolumeParam, 1f);

            SetMasterVolume(master);
            SetMusicVolume(music);
            SetSFXVolume(sfx);
            SetVoiceVolume(voice);

            Debug.Log($"[AudioMixerBridge] Loaded volumes: Master {master:F2}, Music {music:F2}, SFX {sfx:F2}, Voice {voice:F2}");
        }

        /// <summary>
        /// Convert linear volume (0-1) to decibels (-80 to 0).
        /// </summary>
        float LinearToDecibel(float linear)
        {
            linear = Mathf.Clamp01(linear);

            if (linear <= 0f)
            {
                return -80f;  // Minimum dB (silence)
            }

            return Mathf.Log10(linear) * 20f;
        }

        /// <summary>
        /// Convert decibels (-80 to 0) to linear volume (0-1).
        /// </summary>
        float DecibelToLinear(float dB)
        {
            return Mathf.Pow(10f, dB / 20f);
        }

        /// <summary>
        /// Mute all audio.
        /// </summary>
        public void MuteAll()
        {
            SetMasterVolume(0f);
        }

        /// <summary>
        /// Unmute all audio (restore to saved value).
        /// </summary>
        public void UnmuteAll()
        {
            LoadVolumeSettings();
        }
    }
}
