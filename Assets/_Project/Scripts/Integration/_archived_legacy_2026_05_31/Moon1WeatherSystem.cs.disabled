using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Weather System — Echohaven atmospheric conditions
    /// Dynamic fog, light rain, occasional resonance aurora effects.
    /// Mood: Melancholic, mysterious, slightly oppressive → gradually uplifting as player progresses
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon1WeatherSystem : MonoBehaviour
    {
        [Header("Fog System")]
        [SerializeField] bool enableFog = true;
        [SerializeField] Color fogColor = new Color(0.5f, 0.5f, 0.6f);  // Gray-blue
        [SerializeField] float fogDensity = 0.02f;
        [SerializeField] float fogDensityVariation = 0.01f;  // Breathing fog
        [SerializeField] float fogPulseSpeed = 0.5f;
        
        [Header("Rain System")]
        [SerializeField] ParticleSystem rainPrefab;
        [SerializeField] bool enableRain = true;
        [SerializeField] float rainIntensity = 0.3f;         // Light drizzle
        [SerializeField] float rainInterval = 120f;          // Rain every 2 minutes
        [SerializeField] float rainDuration = 45f;           // Lasts 45 seconds
        
        [Header("Aurora Effects")]
        [SerializeField] GameObject auroraEffectPrefab;
        [SerializeField] bool enableAurora = true;
        [SerializeField] float auroraTriggerProgress = 0.5f;  // Appears at 50% Moon progress
        [SerializeField] Color auroraColor = new Color(0.3f, 0.8f, 1f);  // Cyan-blue
        
        [Header("Wind")]
        [SerializeField] float windStrength = 0.5f;
        [SerializeField] float windVariation = 0.3f;
        
        ParticleSystem _activeRain;
        GameObject _activeAurora;
        float _nextRainTime;
        float _rainEndTime;
        bool _isRaining;
        bool _auroraActive;
        float _baseFogDensity;
        
        void Start()
        {
            _baseFogDensity = fogDensity;
            
            SetupFog();
            SetupRain();
            
            _nextRainTime = Time.time + rainInterval;
            
            Debug.Log("[Moon1WeatherSystem] ✅ Initialized - Fog: " + enableFog + ", Rain: " + enableRain);
        }
        
        void SetupFog()
        {
            if (!enableFog) return;
            
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = fogDensity;
        }
        
        void SetupRain()
        {
            if (!enableRain || rainPrefab == null) return;
            
            _activeRain = Instantiate(rainPrefab, transform);
            _activeRain.transform.localPosition = Vector3.up * 20f;  // High above player
            _activeRain.Stop();  // Start stopped
        }
        
        void Update()
        {
            UpdateFog();
            UpdateRain();
            UpdateAurora();
        }
        
        void UpdateFog()
        {
            if (!enableFog) return;
            
            // Pulsing fog density (breathing effect)
            float pulse = Mathf.Sin(Time.time * fogPulseSpeed) * fogDensityVariation;
            RenderSettings.fogDensity = _baseFogDensity + pulse;
        }
        
        void UpdateRain()
        {
            if (!enableRain || _activeRain == null) return;
            
            // Rain cycle
            if (!_isRaining && Time.time >= _nextRainTime)
            {
                StartRain();
            }
            
            if (_isRaining && Time.time >= _rainEndTime)
            {
                StopRain();
            }
            
            // Follow player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 targetPos = player.transform.position + Vector3.up * 20f;
                _activeRain.transform.position = Vector3.Lerp(
                    _activeRain.transform.position,
                    targetPos,
                    Time.deltaTime * 2f
                );
            }
        }
        
        void StartRain()
        {
            _isRaining = true;
            _rainEndTime = Time.time + rainDuration;
            
            if (_activeRain != null)
            {
                _activeRain.Play();
                
                // Increase fog during rain
                _baseFogDensity = fogDensity * 1.3f;
            }
            
            Debug.Log("[Moon1WeatherSystem] Rain started");
        }
        
        void StopRain()
        {
            _isRaining = false;
            _nextRainTime = Time.time + rainInterval;
            
            if (_activeRain != null)
            {
                _activeRain.Stop();
                
                // Reduce fog after rain
                _baseFogDensity = fogDensity;
            }
            
            Debug.Log("[Moon1WeatherSystem] Rain stopped");
        }
        
        void UpdateAurora()
        {
            if (!enableAurora || _auroraActive) return;
            
            // Check if player has made enough progress to see aurora
            float moonProgress = 0f;
            if (GameStateManager.Instance != null)
            {
                moonProgress = GameStateManager.Instance.GetMoonProgress(1);
            }
            
            if (moonProgress >= auroraTriggerProgress)
            {
                ActivateAurora();
            }
        }
        
        void ActivateAurora()
        {
            _auroraActive = true;
            
            if (auroraEffectPrefab != null)
            {
                _activeAurora = Instantiate(auroraEffectPrefab, transform);
                _activeAurora.transform.position = Vector3.up * 50f;  // High in sky
                
                Debug.Log("[Moon1WeatherSystem] ✨ Resonance Aurora activated!");
                
                // Reduce fog when aurora appears (hope returning)
                _baseFogDensity = fogDensity * 0.7f;
                RenderSettings.fogColor = Color.Lerp(fogColor, auroraColor, 0.3f);
            }
        }
        
        /// <summary>
        /// Trigger heavy rain for dramatic moments
        /// </summary>
        public void TriggerStorm(float duration = 60f)
        {
            if (_activeRain == null) return;
            
            StartRain();
            _rainEndTime = Time.time + duration;
            
            // Intensify effects
            var emission = _activeRain.emission;
            emission.rateOverTime = emission.rateOverTime.constant * 2f;
            
            _baseFogDensity = fogDensity * 1.5f;
            
            Debug.Log("[Moon1WeatherSystem] Storm triggered!");
        }
        
        /// <summary>
        /// Clear weather for peaceful moments
        /// </summary>
        public void ClearWeather()
        {
            if (_isRaining && _activeRain != null)
            {
                _activeRain.Stop();
                _isRaining = false;
            }
            
            _baseFogDensity = fogDensity * 0.5f;  // Very light fog
            
            Debug.Log("[Moon1WeatherSystem] Weather cleared");
        }
        
        void OnDestroy()
        {
            // Cleanup
            if (_activeRain != null)
                Destroy(_activeRain.gameObject);
                
            if (_activeAurora != null)
                Destroy(_activeAurora);
        }
    }
}
