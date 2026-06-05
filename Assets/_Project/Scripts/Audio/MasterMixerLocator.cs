using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.Audio
{
    /// <summary>
    /// Runtime locator for the project's MasterMixer asset.
    /// Lives in Resources/ so SettingsOverlay can load it via Resources.Load.
    /// Authored by MasterMixerExposer editor pass.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Audio/Master Mixer Locator", fileName = "MasterMixerLocator")]
    public class MasterMixerLocator : ScriptableObject
    {
        public AudioMixer mixer;

        public static AudioMixer Load()
        {
            var locator = Resources.Load<MasterMixerLocator>("MasterMixerLocator");
            return locator != null ? locator.mixer : null;
        }
    }
}
