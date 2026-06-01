using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-83)]
    public class Moon2WeatherSystem : MonoBehaviour
    {
        [Header("Cave Atmosphere")]
        [SerializeField] bool enableCaveFog = true;
        [SerializeField] Color fogColor = new Color(0.15f, 0.1f, 0.2f);  // Dark purple
        [SerializeField] float fogDensity = 0.04f;
        
        [Header("Bioluminescence")]
        [SerializeField] GameObject biolumParticlesPrefab;
        [SerializeField] int biolumSpots = 15;
        
        [Header("Crystal Resonance")]
        [SerializeField] GameObject crystalResonancePrefab;
        
        readonly System.Collections.Generic.List<Light> _biolumLights = new();
        GameObject _resonanceEffect;
        float _resonanceTimer;
        
        void Start()
        {
            SetupCaveFog();
            SpawnBioluminescence();
            
            Debug.Log("[Moon2WeatherSystem] ✅ Cave atmosphere initialized");
        }
        
        void Update()
        {
            UpdateBioluminescencePulse();
            UpdateResonanceEffect();
        }
        
        void SetupCaveFog()
        {
            if (enableCaveFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
            }
        }
        
        void SpawnBioluminescence()
        {
            for (int i = 0; i < biolumSpots; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-40f, 40f),
                    Random.Range(0f, 15f),
                    Random.Range(-40f, 40f)
                );
                
                GameObject spot = new GameObject($"BiolumSpot_{i}");
                spot.transform.position = pos;
                spot.transform.SetParent(transform);
                
                Light light = spot.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.2f, 0.6f, 0.8f);  // Cyan-blue
                light.range = 8f;
                light.intensity = Random.Range(0.8f, 1.5f);
                
                _biolumLights.Add(light);
            }
        }
        
        void UpdateBioluminescencePulse()
        {
            float time = Time.time;
            foreach (Light light in _biolumLights)
            {
                if (light == null) continue;
                float offset = light.transform.position.x * 0.1f;
                light.intensity = 1.2f + Mathf.Sin((time + offset) * 0.8f) * 0.4f;
            }
        }
        
        void UpdateResonanceEffect()
        {
            float progress = GameStateManager.Instance?.GetMoonProgress("Moon2") ?? 0f;
            
            if (progress >= 50f && _resonanceEffect == null)
            {
                ActivateCrystalResonance();
            }
        }
        
        void ActivateCrystalResonance()
        {
            _resonanceEffect = new GameObject("CrystalResonanceEffect");
            _resonanceEffect.transform.position = Vector3.up * 20f;
            
            ParticleSystem ps = _resonanceEffect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.6f, 0.2f, 0.9f);
            main.startLifetime = 3f;
            main.startSpeed = 2f;
            main.startSize = 1f;
            main.maxParticles = 200;
            
            var emission = ps.emission;
            emission.rateOverTime = 20f;
            
            Debug.Log("[Moon2WeatherSystem] Crystal Resonance activated!");
        }
    }
}
