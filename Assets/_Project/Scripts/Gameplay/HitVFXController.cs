using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Manages hit VFX particle systems with object pooling.
    /// Singleton pattern for global access: HitVFXController.Instance.SpawnHitVFX(...).
    /// Pools 10 particle systems per VFX type (spark, blood, shield).
    /// Auto-returns particles to pool after 1s duration.
    /// Uses SerializeField prefab references with graceful procedural fallback if missing.
    /// </summary>
    public class HitVFXController : MonoBehaviour
    {
        public static HitVFXController Instance { get; private set; }

        [Header("Pool Configuration")]
        [SerializeField] private int _poolSizePerType = 10;
        [SerializeField] private float _autoReturnDelay = 1f;

        [Header("VFX Prefab References")]
        [SerializeField] private GameObject _sparkVfxPrefab;
        [SerializeField] private GameObject _bloodVfxPrefab;
        [SerializeField] private GameObject _shieldVfxPrefab;

        [Header("Fallback Particle Settings (if prefabs missing)")]
        [SerializeField] private Color _sparkColor = new Color(1f, 0.8f, 0.2f, 1f); // Orange sparks
        [SerializeField] private Color _bloodColor = new Color(0.8f, 0.1f, 0.1f, 1f); // Dark red
        [SerializeField] private Color _shieldColor = new Color(0.3f, 0.5f, 1f, 1f); // Blue

        // Pools by hit type
        private Dictionary<HitType, Queue<ParticleSystem>> _availablePools = new Dictionary<HitType, Queue<ParticleSystem>>();
        private Dictionary<HitType, List<ParticleSystem>> _activePools = new Dictionary<HitType, List<ParticleSystem>>();
        private Dictionary<HitType, GameObject> _prefabCache = new Dictionary<HitType, GameObject>();

        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[HitVFXController] Duplicate instance detected, destroying");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePools();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializePools()
        {
            // Cache VFX prefabs from SerializeField references
            if (_sparkVfxPrefab != null)
            {
                _prefabCache[HitType.Spark] = _sparkVfxPrefab;
                Debug.Log("[HitVFXController] Using assigned Spark VFX prefab");
            }
            else
            {
                Debug.LogWarning("[HitVFXController] Spark VFX prefab not assigned, will use procedural fallback");
            }

            if (_bloodVfxPrefab != null)
            {
                _prefabCache[HitType.Blood] = _bloodVfxPrefab;
                Debug.Log("[HitVFXController] Using assigned Blood VFX prefab");
            }
            else
            {
                Debug.LogWarning("[HitVFXController] Blood VFX prefab not assigned, will use procedural fallback");
            }

            if (_shieldVfxPrefab != null)
            {
                _prefabCache[HitType.Shield] = _shieldVfxPrefab;
                Debug.Log("[HitVFXController] Using assigned Shield VFX prefab");
            }
            else
            {
                Debug.LogWarning("[HitVFXController] Shield VFX prefab not assigned, will use procedural fallback");
            }

            // Initialize pools for each type
            foreach (HitType hitType in System.Enum.GetValues(typeof(HitType)))
            {
                _availablePools[hitType] = new Queue<ParticleSystem>();
                _activePools[hitType] = new List<ParticleSystem>();

                // Create pool instances
                for (int i = 0; i < _poolSizePerType; i++)
                {
                    ParticleSystem ps = CreateVFXInstance(hitType);
                    if (ps != null)
                    {
                        ps.gameObject.SetActive(false);
                        _availablePools[hitType].Enqueue(ps);
                    }
                }
            }

            int totalPooled = _poolSizePerType * System.Enum.GetValues(typeof(HitType)).Length;
            Debug.Log($"[HitVFXController] Initialized VFX pools: {totalPooled} particles across {System.Enum.GetValues(typeof(HitType)).Length} types");
        }

        private ParticleSystem CreateVFXInstance(HitType hitType)
        {
            GameObject vfxObj;

            if (_prefabCache.ContainsKey(hitType) && _prefabCache[hitType] != null)
            {
                // Instantiate from prefab
                vfxObj = Instantiate(_prefabCache[hitType]);
                vfxObj.name = $"{hitType}VFX_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            else
            {
                // Create procedural particle system
                vfxObj = new GameObject($"{hitType}VFX_Procedural");
                ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();

                // Configure particle system
                var main = ps.main;
                main.startLifetime = 0.5f;
                main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
                main.maxParticles = 30;
                main.duration = 0.5f;
                main.loop = false;

                // Color based on hit type
                Color particleColor = GetFallbackColor(hitType);
                main.startColor = particleColor;

                // Emission
                var emission = ps.emission;
                emission.rateOverTime = 0f;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

                // Shape (sphere burst)
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.2f;

                // Velocity over lifetime (gravity simulation)
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.y = new ParticleSystem.MinMaxCurve(-5f);

                // Size over lifetime (shrink)
                var sizeOverLifetime = ps.sizeOverLifetime;
                sizeOverLifetime.enabled = true;
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

                // Renderer
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                // 2026-06-03 HAMMER R4: "Particles/Standard Unlit" is built-in RP only — magenta
                // on URP. Use URP/Particles/Unlit with fallback for safety.
                var hitVfxShader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                                  ?? Shader.Find("Particles/Standard Unlit");
                renderer.material = new Material(hitVfxShader);
                if (renderer.material.HasProperty("_BaseColor"))
                    renderer.material.SetColor("_BaseColor", particleColor);
                else
                    renderer.material.SetColor("_Color", particleColor);
            }

            vfxObj.transform.SetParent(transform);
            ParticleSystem particleSystem = vfxObj.GetComponent<ParticleSystem>();

            if (particleSystem == null)
            {
                Debug.LogError($"[HitVFXController] Created VFX object missing ParticleSystem component!");
                Destroy(vfxObj);
                return null;
            }

            return particleSystem;
        }

        private Color GetFallbackColor(HitType hitType)
        {
            switch (hitType)
            {
                case HitType.Spark:
                    return _sparkColor;
                case HitType.Blood:
                    return _bloodColor;
                case HitType.Shield:
                    return _shieldColor;
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Spawn a hit VFX particle effect at the specified position.
        /// Particle is auto-returned to pool after 1 second.
        /// </summary>
        public void SpawnHitVFX(Vector3 position, HitType type)
        {
            if (!_availablePools.ContainsKey(type))
            {
                Debug.LogWarning($"[HitVFXController] Unknown HitType: {type}");
                return;
            }

            if (_availablePools[type].Count == 0)
            {
                Debug.LogWarning($"[HitVFXController] Pool exhausted for {type}, skipping VFX");
                return;
            }

            ParticleSystem ps = _availablePools[type].Dequeue();
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play();

            _activePools[type].Add(ps);

            // Auto-return to pool after delay
            StartCoroutine(ReturnToPoolAfterDelay(ps, type, _autoReturnDelay));
        }

        /// <summary>
        /// Spawn a hit VFX with optional rotation.
        /// </summary>
        public void SpawnHitVFX(Vector3 position, Quaternion rotation, HitType type)
        {
            if (!_availablePools.ContainsKey(type) || _availablePools[type].Count == 0)
            {
                SpawnHitVFX(position, type); // Fallback to non-rotated version
                return;
            }

            ParticleSystem ps = _availablePools[type].Dequeue();
            ps.transform.SetPositionAndRotation(position, rotation);
            ps.gameObject.SetActive(true);
            ps.Play();

            _activePools[type].Add(ps);

            StartCoroutine(ReturnToPoolAfterDelay(ps, type, _autoReturnDelay));
        }

        private IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, HitType type, float delay)
        {
            yield return new WaitForSeconds(delay);

            // Stop particle emission
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            // Wait for all particles to die
            yield return new WaitForSeconds(0.5f);

            // Return to pool
            ps.gameObject.SetActive(false);
            _activePools[type].Remove(ps);
            _availablePools[type].Enqueue(ps);
        }

        /// <summary>
        /// Manually return a particle to the pool (for immediate cleanup).
        /// </summary>
        public void ReturnToPool(ParticleSystem ps, HitType type)
        {
            if (ps == null) return;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);

            if (_activePools.ContainsKey(type) && _activePools[type].Contains(ps))
            {
                _activePools[type].Remove(ps);
                _availablePools[type].Enqueue(ps);
            }
        }

        /// <summary>
        /// Helper method: Spawn VFX at enemy hit position with normal direction.
        /// </summary>
        public void SpawnEnemyHitVFX(Vector3 position, Vector3 normal)
        {
            Quaternion rotation = Quaternion.LookRotation(normal);
            SpawnHitVFX(position, rotation, HitType.Blood);
        }

        /// <summary>
        /// Helper method: Spawn VFX at shield block position.
        /// </summary>
        public void SpawnShieldBlockVFX(Vector3 position)
        {
            SpawnHitVFX(position, HitType.Shield);
        }

        /// <summary>
        /// Helper method: Spawn VFX at weapon hit position (sparks).
        /// </summary>
        public void SpawnWeaponHitVFX(Vector3 position, Vector3 normal)
        {
            Quaternion rotation = Quaternion.LookRotation(normal);
            SpawnHitVFX(position, rotation, HitType.Spark);
        }
    }

    /// <summary>
    /// Hit VFX types corresponding to different combat interactions.
    /// </summary>
    public enum HitType
    {
        Spark,   // Weapon hits, environmental hits
        Blood,   // Enemy damage
        Shield   // Shield blocks, magic barriers
    }
}
