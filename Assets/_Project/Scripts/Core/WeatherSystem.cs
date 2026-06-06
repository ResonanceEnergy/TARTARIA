using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Weather System — controls rain, snow, fog for ambient atmosphere.
    /// Attach to scene manager or create as singleton. Integrates with DayNightCycle.
    /// </summary>
    public class WeatherSystem : MonoBehaviour
    {
        [Header("Weather Types")]
        [SerializeField] ParticleSystem rainParticles;
        [SerializeField] ParticleSystem snowParticles;
        [SerializeField] GameObject fogVolume;

        [Header("Settings")]
        [SerializeField] WeatherType currentWeather = WeatherType.Clear;
        [SerializeField] float transitionDuration = 3f;
        [SerializeField] bool enableRandomWeather = false;
        [SerializeField] float weatherChangeInterval = 300f;  // 5 minutes

        float _weatherTimer;
        float _transitionTimer;
        WeatherType _targetWeather;
        bool _isTransitioning;

        public enum WeatherType
        {
            Clear,
            Rain,
            Snow,
            Fog
        }

        void Start()
        {
            ApplyWeather(currentWeather, immediate: true);
            _weatherTimer = weatherChangeInterval;
        }

        void Update()
        {
            // Random weather changes
            if (enableRandomWeather)
            {
                _weatherTimer -= Time.deltaTime;
                if (_weatherTimer <= 0f)
                {
                    _weatherTimer = weatherChangeInterval;
                    ChangeWeatherRandom();
                }
            }

            // Transition handling
            if (_isTransitioning)
            {
                _transitionTimer -= Time.deltaTime;
                float t = 1f - (_transitionTimer / transitionDuration);

                // Fade out old, fade in new
                UpdateWeatherTransition(t);

                if (_transitionTimer <= 0f)
                {
                    _isTransitioning = false;
                    currentWeather = _targetWeather;
                }
            }
        }

        public void SetWeather(WeatherType weather)
        {
            if (weather == currentWeather) return;

            _targetWeather = weather;
            _isTransitioning = true;
            _transitionTimer = transitionDuration;

            Debug.Log($"[WeatherSystem] Transitioning to {weather}");
        }

        void ChangeWeatherRandom()
        {
            int rand = Random.Range(0, 4);
            SetWeather((WeatherType)rand);
        }

        void ApplyWeather(WeatherType weather, bool immediate = false)
        {
            // Disable all weather effects
            if (rainParticles != null) rainParticles.gameObject.SetActive(false);
            if (snowParticles != null) snowParticles.gameObject.SetActive(false);
            if (fogVolume != null) fogVolume.SetActive(false);

            // Enable target weather
            switch (weather)
            {
                case WeatherType.Rain:
                    if (rainParticles != null)
                    {
                        rainParticles.gameObject.SetActive(true);
                        if (immediate) rainParticles.Play();
                    }
                    break;

                case WeatherType.Snow:
                    if (snowParticles != null)
                    {
                        snowParticles.gameObject.SetActive(true);
                        if (immediate) snowParticles.Play();
                    }
                    break;

                case WeatherType.Fog:
                    if (fogVolume != null)
                    {
                        fogVolume.SetActive(true);
                    }
                    break;

                case WeatherType.Clear:
                    // All disabled already
                    break;
            }

            Debug.Log($"[WeatherSystem] Weather set to {weather}");
        }

        void UpdateWeatherTransition(float t)
        {
            // Simplified transition: just crossfade particle emission rates
            if (currentWeather != _targetWeather)
            {
                // Fade out current
                switch (currentWeather)
                {
                    case WeatherType.Rain:
                        if (rainParticles != null)
                        {
                            var emission = rainParticles.emission;
                            emission.rateOverTime = Mathf.Lerp(100f, 0f, t);
                        }
                        break;

                    case WeatherType.Snow:
                        if (snowParticles != null)
                        {
                            var emission = snowParticles.emission;
                            emission.rateOverTime = Mathf.Lerp(50f, 0f, t);
                        }
                        break;
                }

                // Fade in target
                ApplyWeather(_targetWeather);
                switch (_targetWeather)
                {
                    case WeatherType.Rain:
                        if (rainParticles != null)
                        {
                            var emission = rainParticles.emission;
                            emission.rateOverTime = Mathf.Lerp(0f, 100f, t);
                        }
                        break;

                    case WeatherType.Snow:
                        if (snowParticles != null)
                        {
                            var emission = snowParticles.emission;
                            emission.rateOverTime = Mathf.Lerp(0f, 50f, t);
                        }
                        break;
                }
            }
        }
    }
}
