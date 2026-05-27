using UnityEngine;

namespace Tartaria.Audio
{
    /// <summary>
    /// Lightweight AudioController facade — provides simplified interface to AudioManager.
    /// Used by UI components for 2D sound effects (button clicks, equip sounds, etc.).
    /// All methods forward to AudioManager.Instance.
    /// </summary>
    public class AudioController
    {
        /// <summary>
        /// Singleton accessor — forwards to AudioManager.Instance.
        /// Returns null if AudioManager not initialized.
        /// </summary>
        public static AudioController Instance
        {
            get
            {
                if (AudioManager.Instance == null)
                    return null;
                return _instance ?? (_instance = new AudioController());
            }
        }
        static AudioController _instance;

        // Private constructor (singleton pattern)
        AudioController() { }

        /// <summary>
        /// Play 2D sound effect (UI sounds, button clicks, equip sounds).
        /// Forwards to AudioManager.PlaySFX2D.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1.0f)
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[AudioController] AudioManager not initialized");
                return;
            }
            AudioManager.Instance.PlaySFX2D(clip, volume);
        }

        /// <summary>
        /// Play 2D sound effect by name from cue library.
        /// Forwards to AudioManager.PlaySFX2D.
        /// </summary>
        public void PlaySFX(string clipName, float volume = 1.0f)
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[AudioController] AudioManager not initialized");
                return;
            }
            AudioManager.Instance.PlaySFX2D(clipName, volume);
        }

        /// <summary>
        /// Play 3D spatial sound effect at world position.
        /// Forwards to AudioManager.PlaySFX.
        /// </summary>
        public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1.0f)
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[AudioController] AudioManager not initialized");
                return;
            }
            AudioManager.Instance.PlaySFX(clip, position, volume);
        }

        /// <summary>
        /// Play 3D spatial sound effect by name at world position.
        /// Forwards to AudioManager.PlaySFX3D.
        /// </summary>
        public void PlaySFX3D(string clipName, Vector3 position, float volume = 1.0f)
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[AudioController] AudioManager not initialized");
                return;
            }
            AudioManager.Instance.PlaySFX3D(clipName, position, volume);
        }
    }
}
