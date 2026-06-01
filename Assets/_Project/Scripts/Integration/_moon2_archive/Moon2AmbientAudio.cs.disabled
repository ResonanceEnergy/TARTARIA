using UnityEngine;
using Tartaria.Core;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-82)]
    public class Moon2AmbientAudio : MonoBehaviour
    {
        [Header("Cave Ambience")]
        [SerializeField] AudioClip caveAmbienceClip;
        [SerializeField] AudioClip waterDripsClip;
        [SerializeField] AudioClip crystalHumClip;
        
        [Header("Music")]
        [SerializeField] AudioClip explorationTheme;
        [SerializeField] AudioClip mysteryTheme;
        [SerializeField] AudioClip combatTheme;
        
        AudioSource _ambience;
        AudioSource _drips;
        AudioSource _crystalHum;
        AudioSource _music;
        
        void Start()
        {
            SetupAudioSources();
            StartAmbience();
            
            GameEvents.OnCombatStarted += TransitionToCombatMusic;
            GameEvents.OnCombatEnded += TransitionToExplorationMusic;
            
            Debug.Log("[Moon2AmbientAudio] ✅ Cave soundscape initialized");
        }
        
        void OnDestroy()
        {
            GameEvents.OnCombatStarted -= TransitionToCombatMusic;
            GameEvents.OnCombatEnded -= TransitionToExplorationMusic;
        }
        
        void SetupAudioSources()
        {
            _ambience = gameObject.AddComponent<AudioSource>();
            _ambience.loop = true;
            _ambience.spatialBlend = 0f;  // 2D
            _ambience.volume = 0.4f;
            
            _drips = gameObject.AddComponent<AudioSource>();
            _drips.loop = true;
            _drips.spatialBlend = 0.6f;  // Slightly 3D
            _drips.volume = 0.2f;
            
            _crystalHum = gameObject.AddComponent<AudioSource>();
            _crystalHum.loop = true;
            _crystalHum.spatialBlend = 0.3f;
            _crystalHum.volume = 0.3f;
            
            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.spatialBlend = 0f;
            _music.volume = 0.5f;
        }
        
        void StartAmbience()
        {
            if (caveAmbienceClip != null) _ambience.clip = caveAmbienceClip;
            if (waterDripsClip != null) _drips.clip = waterDripsClip;
            if (crystalHumClip != null) _crystalHum.clip = crystalHumClip;
            
            _ambience.Play();
            _drips.Play();
            _crystalHum.Play();
            
            if (explorationTheme != null)
            {
                _music.clip = explorationTheme;
                _music.Play();
            }
        }
        
        void TransitionToCombatMusic(CombatEventArgs args)
        {
            if (combatTheme != null)
                CrossfadeMusic(combatTheme, 2f);
        }
        
        void TransitionToExplorationMusic(CombatEventArgs args)
        {
            if (explorationTheme != null)
                CrossfadeMusic(explorationTheme, 3f);
        }
        
        void CrossfadeMusic(AudioClip newClip, float duration)
        {
            LeanTween.value(gameObject, _music.volume, 0f, duration * 0.5f)
                .setOnUpdate((float v) => _music.volume = v)
                .setOnComplete(() => {
                    _music.clip = newClip;
                    _music.Play();
                    LeanTween.value(gameObject, 0f, 0.5f, duration * 0.5f)
                        .setOnUpdate((float v) => _music.volume = v);
                });
        }
    }
}
