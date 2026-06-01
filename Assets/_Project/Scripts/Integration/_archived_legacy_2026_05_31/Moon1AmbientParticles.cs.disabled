using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Ambient Particles — Atmospheric VFX for Echohaven
    /// Dust motes in shafts of light, fireflies, fog wisps, aether sparkles
    /// Performance-optimized particle pooling for mobile/PC
    /// </summary>
    [DefaultExecutionOrder(-83)]
    public class Moon1AmbientParticles : MonoBehaviour
    {
        [Header("Particle Systems")]
        [SerializeField] GameObject dustMotePrefab;
        [SerializeField] GameObject fireflyPrefab;
        [SerializeField] GameObject fogWispPrefab;
        [SerializeField] GameObject aetherSparklePrefab;
        
        [Header("Density Settings")]
        [SerializeField] int maxDustMotes = 30;
        [SerializeField] int maxFireflies = 20;
        [SerializeField] int maxFogWisps = 15;
        [SerializeField] int maxAetherSparkles = 10;  // Only spawn with progress
        
        [Header("Spawn Area")]
        [SerializeField] Vector3 spawnCenter = Vector3.zero;
        [SerializeField] float spawnRadius = 40f;
        [SerializeField] float spawnHeight = 15f;
        
        [Header("Performance")]
        [SerializeField] float particleUpdateInterval = 0.1f;
        [SerializeField] bool usePooling = true;
        [SerializeField] int poolSize = 50;
        
        readonly List<ParticleSystem> _activeDustMotes = new();
        readonly List<GameObject> _activeFireflies = new();
        readonly List<ParticleSystem> _activeFogWisps = new();
        readonly List<ParticleSystem> _activeAetherSparkles = new();
        readonly Queue<GameObject> _particlePool = new();
        
        float _nextUpdateTime;
        bool _aetherSparklesUnlocked;
        
        void Start()
        {
            if (usePooling)
            {
                InitializePool();
            }
            
            SpawnInitialParticles();
            
            Debug.Log("[Moon1AmbientParticles] ✅ Initialized - Atmospheric VFX active");
        }
        
        void InitializePool()
        {
            // Pre-instantiate particle systems for performance
            for (int i = 0; i < poolSize; i++)
            {
                if (dustMotePrefab != null)
                {
                    GameObject obj = Instantiate(dustMotePrefab, transform);
                    obj.SetActive(false);
                    _particlePool.Enqueue(obj);
                }
            }
        }
        
        void SpawnInitialParticles()
        {
            // Dust motes in light shafts
            for (int i = 0; i < maxDustMotes; i++)
            {
                SpawnDustMote();
            }
            
            // Fireflies in dark areas
            for (int i = 0; i < maxFireflies; i++)
            {
                SpawnFirefly();
            }
            
            // Fog wisps near ground
            for (int i = 0; i < maxFogWisps; i++)
            {
                SpawnFogWisp();
            }
        }
        
        void Update()
        {
            // Periodic update to maintain particle counts
            if (Time.time >= _nextUpdateTime)
            {
                _nextUpdateTime = Time.time + particleUpdateInterval;
                MaintainParticleCounts();
            }
            
            // Unlock aether sparkles at 30% progress
            if (!_aetherSparklesUnlocked && GameStateManager.Instance != null)
            {
                float progress = GameStateManager.Instance.GetMoonProgress(1);
                if (progress >= 0.3f)
                {
                    UnlockAetherSparkles();
                }
            }
        }
        
        void SpawnDustMote()
        {
            if (dustMotePrefab == null) return;
            
            Vector3 position = GetRandomSpawnPosition();
            position.y = Random.Range(2f, spawnHeight);  // Floating in air
            
            GameObject moteObj = usePooling ? GetFromPool() : Instantiate(dustMotePrefab, transform);
            if (moteObj == null) return;
            
            moteObj.transform.position = position;
            moteObj.SetActive(true);
            
            ParticleSystem ps = moteObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                _activeDustMotes.Add(ps);
            }
        }
        
        void SpawnFirefly()
        {
            if (fireflyPrefab == null) return;
            
            Vector3 position = GetRandomSpawnPosition();
            position.y = Random.Range(0.5f, 3f);  // Low-flying
            
            GameObject firefly = Instantiate(fireflyPrefab, position, Quaternion.identity, transform);
            firefly.name = $"Firefly_{_activeFireflies.Count}";
            
            // Add simple flying behavior
            Firefly fireflyBehavior = firefly.GetOrAddComponent<Firefly>();
            fireflyBehavior.Initialize(spawnCenter, spawnRadius);
            
            _activeFireflies.Add(firefly);
        }
        
        void SpawnFogWisp()
        {
            if (fogWispPrefab == null) return;
            
            Vector3 position = GetRandomSpawnPosition();
            position.y = Random.Range(0f, 2f);  // Ground level
            
            GameObject wispObj = Instantiate(fogWispPrefab, position, Quaternion.identity, transform);
            
            ParticleSystem ps = wispObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                _activeFogWisps.Add(ps);
            }
        }
        
        void SpawnAetherSparkle()
        {
            if (aetherSparklePrefab == null) return;
            
            Vector3 position = GetRandomSpawnPosition();
            position.y = Random.Range(1f, spawnHeight * 0.5f);
            
            GameObject sparkleObj = Instantiate(aetherSparklePrefab, position, Quaternion.identity, transform);
            
            ParticleSystem ps = sparkleObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                _activeAetherSparkles.Add(ps);
            }
        }
        
        Vector3 GetRandomSpawnPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            return spawnCenter + new Vector3(randomCircle.x, 0f, randomCircle.y);
        }
        
        GameObject GetFromPool()
        {
            if (_particlePool.Count > 0)
            {
                return _particlePool.Dequeue();
            }
            
            return dustMotePrefab != null ? Instantiate(dustMotePrefab, transform) : null;
        }
        
        void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;
            
            obj.SetActive(false);
            _particlePool.Enqueue(obj);
        }
        
        void MaintainParticleCounts()
        {
            // Remove destroyed particles from lists
            _activeDustMotes.RemoveAll(ps => ps == null);
            _activeFireflies.RemoveAll(ff => ff == null);
            _activeFogWisps.RemoveAll(ps => ps == null);
            _activeAetherSparkles.RemoveAll(ps => ps == null);
            
            // Respawn if below thresholds
            while (_activeDustMotes.Count < maxDustMotes)
                SpawnDustMote();
                
            while (_activeFireflies.Count < maxFireflies)
                SpawnFirefly();
                
            while (_activeFogWisps.Count < maxFogWisps)
                SpawnFogWisp();
                
            if (_aetherSparklesUnlocked)
            {
                while (_activeAetherSparkles.Count < maxAetherSparkles)
                    SpawnAetherSparkle();
            }
        }
        
        void UnlockAetherSparkles()
        {
            _aetherSparklesUnlocked = true;
            
            // Spawn initial sparkles
            for (int i = 0; i < maxAetherSparkles; i++)
            {
                SpawnAetherSparkle();
            }
            
            Debug.Log("[Moon1AmbientParticles] ✨ Aether Sparkles unlocked!");
        }
        
        void OnDestroy()
        {
            // Cleanup
            foreach (var ps in _activeDustMotes)
                if (ps != null) Destroy(ps.gameObject);
                
            foreach (var ff in _activeFireflies)
                if (ff != null) Destroy(ff);
                
            foreach (var ps in _activeFogWisps)
                if (ps != null) Destroy(ps.gameObject);
                
            foreach (var ps in _activeAetherSparkles)
                if (ps != null) Destroy(ps.gameObject);
        }
    }
    
    /// <summary>
    /// Simple firefly behavior - flies in random patterns
    /// </summary>
    public class Firefly : MonoBehaviour
    {
        Vector3 _targetPosition;
        Vector3 _spawnCenter;
        float _spawnRadius;
        float _speed = 2f;
        float _directionChangeInterval = 3f;
        float _nextDirectionChange;
        
        Light _light;
        float _lightIntensity;
        
        public void Initialize(Vector3 center, float radius)
        {
            _spawnCenter = center;
            _spawnRadius = radius;
            _targetPosition = GetRandomTarget();
            _nextDirectionChange = Time.time + _directionChangeInterval;
            
            // Setup light
            _light = gameObject.GetOrAddComponent<Light>();
            _light.type = LightType.Point;
            _light.range = 3f;
            _light.color = new Color(1f, 1f, 0.8f);  // Warm yellow
            _lightIntensity = Random.Range(0.3f, 0.6f);
            _light.intensity = _lightIntensity;
        }
        
        void Update()
        {
            // Move toward target
            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPosition,
                _speed * Time.deltaTime
            );
            
            // Change direction periodically
            if (Time.time >= _nextDirectionChange || Vector3.Distance(transform.position, _targetPosition) < 0.5f)
            {
                _targetPosition = GetRandomTarget();
                _nextDirectionChange = Time.time + _directionChangeInterval;
            }
            
            // Pulsing light
            if (_light != null)
            {
                _light.intensity = _lightIntensity * (1f + Mathf.Sin(Time.time * 4f) * 0.3f);
            }
        }
        
        Vector3 GetRandomTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * _spawnRadius;
            float height = Random.Range(0.5f, 3f);
            return _spawnCenter + new Vector3(randomCircle.x, height, randomCircle.y);
        }
    }
}
