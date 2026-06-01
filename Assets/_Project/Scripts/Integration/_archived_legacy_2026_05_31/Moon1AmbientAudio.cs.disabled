using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Ambient Audio — Echohaven soundscape
    /// Layered ambient audio: cathedral echoes, distant bells, wind, water drips, mechanical hums
    /// Mood: Haunting → mysterious → hopeful (matches visual progression)
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon1AmbientAudio : MonoBehaviour
    {
        [Header("Ambient Loops")]
        [SerializeField] AudioClip cathedralAmbience;        // Reverberant space
        [SerializeField] AudioClip distantBells;             // Slow 432 Hz bell tones
        [SerializeField] AudioClip windHowl;                 // Exterior wind
        [SerializeField] AudioClip waterDrips;               // Interior echoes
        [SerializeField] AudioClip mechanicalHum;            // Clockwork resonance
        [SerializeField] AudioClip resonanceHum;             // Aether field presence
        
        [Header("Music Stems")]
        [SerializeField] AudioClip explorationTheme;         // Low-intensity background
        [SerializeField] AudioClip mysteryTheme;             // Puzzle/discovery
        [SerializeField] AudioClip combatTheme;              // Enemy encounters
        [SerializeField] AudioClip victoryStinger;           // Completion cues
        
        [Header("Volume Mix")]
        [SerializeField][Range(0f, 1f)] float ambienceVolume = 0.3f;
        [SerializeField][Range(0f, 1f)] float musicVolume = 0.5f;
        [SerializeField][Range(0f, 1f)] float sfxVolume = 0.7f;
        
        [Header("3D Sound Settings")]
        [SerializeField] float maxAudioDistance = 50f;
        [SerializeField] AnimationCurve distanceFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        readonly List<AudioSource> _activeSources = new();
        AudioSource _musicSource;
        AudioSource _ambienceSource;
        AudioSource _bellSource;
        AudioSource _mechanicalSource;
        AudioSource _resonanceSource;
        
        MusicState _currentMusicState = MusicState.Exploration;
        
        void Start()
        {
            SetupAudioSources();
            StartAmbientLoops();
            
            // Wire game events
            GameEvents.OnCombatStarted += TransitionToCombatMusic;
            GameEvents.OnCombatEnded += TransitionToExplorationMusic;
            GameEvents.OnTuningNodeActivated += PlayResonanceStinger;
            
            Debug.Log("[Moon1AmbientAudio] ✅ Initialized - Ambient soundscape active");
        }
        
        void OnDestroy()
        {
            GameEvents.OnCombatStarted -= TransitionToCombatMusic;
            GameEvents.OnCombatEnded -= TransitionToExplorationMusic;
            GameEvents.OnTuningNodeActivated -= PlayResonanceStinger;
            
            // Stop all audio
            foreach (var source in _activeSources)
            {
                if (source != null)
                    Destroy(source.gameObject);
            }
        }
        
        void SetupAudioSources()
        {
            // Cathedral ambience (global, always playing)
            _ambienceSource = CreateAudioSource("CathedralAmbience", cathedralAmbience);
            _ambienceSource.volume = ambienceVolume;
            _ambienceSource.loop = true;
            _ambienceSource.spatialBlend = 0f;  // 2D global sound
            
            // Distant bells (spatial, from north tower)
            _bellSource = CreateAudioSource("DistantBells", distantBells);
            _bellSource.volume = ambienceVolume * 0.7f;
            _bellSource.loop = true;
            _bellSource.spatialBlend = 0.5f;  // Semi-3D
            _bellSource.transform.position = new Vector3(0f, 30f, 40f);  // North tower position
            
            // Mechanical hum (spatial, from clockwork areas)
            _mechanicalSource = CreateAudioSource("MechanicalHum", mechanicalHum);
            _mechanicalSource.volume = ambienceVolume * 0.5f;
            _mechanicalSource.loop = true;
            _mechanicalSource.spatialBlend = 0.8f;  // Mostly 3D
            _mechanicalSource.transform.position = new Vector3(-15f, 5f, 10f);  // Gear mechanism location
            
            // Resonance hum (grows with player progress)
            _resonanceSource = CreateAudioSource("ResonanceHum", resonanceHum);
            _resonanceSource.volume = 0f;  // Starts silent
            _resonanceSource.loop = true;
            _resonanceSource.spatialBlend = 0.3f;  // Mostly 2D, slightly spatial
            
            // Music layer
            _musicSource = CreateAudioSource("MusicLayer", explorationTheme);
            _musicSource.volume = musicVolume;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;  // 2D global
        }
        
        AudioSource CreateAudioSource(string sourceName, AudioClip clip)
        {
            GameObject sourceObj = new GameObject($"AudioSource_{sourceName}");
            sourceObj.transform.SetParent(transform);
            
            AudioSource source = sourceObj.AddComponent<AudioSource>();
            source.clip = clip;
            source.playOnAwake = false;
            source.maxDistance = maxAudioDistance;
            
            _activeSources.Add(source);
            
            return source;
        }
        
        void StartAmbientLoops()
        {
            if (_ambienceSource != null && cathedralAmbience != null)
                _ambienceSource.Play();
                
            if (_bellSource != null && distantBells != null)
                _bellSource.Play();
                
            if (_mechanicalSource != null && mechanicalHum != null)
                _mechanicalSource.Play();
                
            if (_musicSource != null && explorationTheme != null)
                _musicSource.Play();
        }
        
        void Update()
        {
            UpdateResonanceHum();
            UpdateSpatialAudio();
        }
        
        void UpdateResonanceHum()
        {
            // Resonance hum volume increases with Moon progress
            float moonProgress = 0f;
            if (GameStateManager.Instance != null)
            {
                moonProgress = GameStateManager.Instance.GetMoonProgress(1);
            }
            
            if (_resonanceSource != null)
            {
                float targetVolume = Mathf.Lerp(0f, ambienceVolume * 0.8f, moonProgress);
                _resonanceSource.volume = Mathf.Lerp(
                    _resonanceSource.volume,
                    targetVolume,
                    Time.deltaTime * 0.5f
                );
                
                if (!_resonanceSource.isPlaying && moonProgress > 0.1f)
                {
                    _resonanceSource.Play();
                }
            }
        }
        
        void UpdateSpatialAudio()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            // Update listener position
            if (AudioListener.GetInstanceID() == 0)  // Check if listener exists
            {
                Camera.main?.gameObject.AddComponent<AudioListener>();
            }
        }
        
        void TransitionToCombatMusic()
        {
            if (_currentMusicState == MusicState.Combat) return;
            
            _currentMusicState = MusicState.Combat;
            
            if (_musicSource != null && combatTheme != null)
            {
                CrossfadeMusic(combatTheme, 2f);
            }
            
            Debug.Log("[Moon1AmbientAudio] → Combat music");
        }
        
        void TransitionToExplorationMusic()
        {
            if (_currentMusicState == MusicState.Exploration) return;
            
            _currentMusicState = MusicState.Exploration;
            
            if (_musicSource != null && explorationTheme != null)
            {
                CrossfadeMusic(explorationTheme, 3f);
            }
            
            Debug.Log("[Moon1AmbientAudio] → Exploration music");
        }
        
        void CrossfadeMusic(AudioClip newClip, float duration)
        {
            if (_musicSource == null) return;
            
            // Simple crossfade using LeanTween
            LeanTween.value(gameObject, _musicSource.volume, 0f, duration * 0.5f)
                .setOnUpdate((float val) => _musicSource.volume = val)
                .setOnComplete(() =>
                {
                    _musicSource.clip = newClip;
                    _musicSource.Play();
                    
                    LeanTween.value(gameObject, 0f, musicVolume, duration * 0.5f)
                        .setOnUpdate((float val) => _musicSource.volume = val);
                });
        }
        
        void PlayResonanceStinger(int nodeID)
        {
            if (victoryStinger == null) return;
            
            // Play one-shot stinger for tuning node activation
            AudioSource.PlayClipAtPoint(victoryStinger, Camera.main.transform.position, sfxVolume);
            
            Debug.Log($"[Moon1AmbientAudio] Resonance stinger for node {nodeID}");
        }
        
        /// <summary>
        /// Play mystery/discovery music for puzzle moments
        /// </summary>
        public void PlayMysteryTheme()
        {
            if (mysteryTheme != null && _musicSource != null)
            {
                CrossfadeMusic(mysteryTheme, 2f);
                _currentMusicState = MusicState.Mystery;
            }
        }
        
        /// <summary>
        /// Mute all ambient audio (for cinematics)
        /// </summary>
        public void MuteAmbience(bool mute)
        {
            float targetVolume = mute ? 0f : ambienceVolume;
            
            foreach (var source in _activeSources)
            {
                if (source != null && source != _musicSource)
                {
                    LeanTween.value(source.gameObject, source.volume, targetVolume, 1f)
                        .setOnUpdate((float val) => source.volume = val);
                }
            }
        }
    }
    
    enum MusicState
    {
        Exploration,
        Mystery,
        Combat
    }
}
