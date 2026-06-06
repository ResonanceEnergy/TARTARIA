using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Core
{
    /// <summary>
    /// AGENT 28: Generic Object Pool
    /// Reduces GC allocations from frequent Instantiate/Destroy calls
    /// Use for: VFX particles, projectiles, UI elements, audio sources
    /// </summary>
    /// <typeparam name="T">Component type to pool</typeparam>
    public class ObjectPool<T> where T : Component
    {
        readonly T _prefab;
        readonly Transform _parent;
        readonly Queue<T> _available = new();
        readonly HashSet<T> _active = new();
        readonly int _initialSize;
        readonly int _maxSize;

        public int CountAvailable => _available.Count;
        public int CountActive => _active.Count;
        public int CountTotal => CountAvailable + CountActive;

        /// <summary>
        /// Create a new object pool
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="initialSize">Objects to pre-spawn</param>
        /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
        /// <param name="parent">Optional parent transform for pooled objects</param>
        public ObjectPool(T prefab, int initialSize = 10, int maxSize = 100, Transform parent = null)
        {
            _prefab = prefab;
            _initialSize = initialSize;
            _maxSize = maxSize;
            _parent = parent;

            // Pre-warm pool
            for (int i = 0; i < initialSize; i++)
            {
                var obj = Object.Instantiate(prefab, parent);
                obj.gameObject.SetActive(false);
                _available.Enqueue(obj);
            }
        }

        /// <summary>
        /// Get an object from the pool (or create new if empty)
        /// </summary>
        public T Get()
        {
            T obj;

            if (_available.Count > 0)
            {
                obj = _available.Dequeue();
            }
            else
            {
                // Pool exhausted - create new instance if under max size
                if (_maxSize > 0 && CountTotal >= _maxSize)
                {
                    Debug.LogWarning($"[ObjectPool] Max size reached ({_maxSize}), reusing oldest active object");
                    // Get oldest active object
                    var enumerator = _active.GetEnumerator();
                    enumerator.MoveNext();
                    obj = enumerator.Current;
                    Return(obj);
                    obj = _available.Dequeue();
                }
                else
                {
                    obj = Object.Instantiate(_prefab, _parent);
                }
            }

            obj.gameObject.SetActive(true);
            _active.Add(obj);
            return obj;
        }

        /// <summary>
        /// Get an object with position and rotation
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            var obj = Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;
            if (!_active.Remove(obj)) return; // Not from this pool

            obj.gameObject.SetActive(false);
            if (_parent != null)
                obj.transform.SetParent(_parent, false);

            _available.Enqueue(obj);
        }

        /// <summary>
        /// Return an object after a delay
        /// </summary>
        public void ReturnAfterDelay(T obj, float delay, MonoBehaviour coroutineRunner)
        {
            if (coroutineRunner != null)
                coroutineRunner.StartCoroutine(ReturnAfterDelayCoroutine(obj, delay));
        }

        System.Collections.IEnumerator ReturnAfterDelayCoroutine(T obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            Return(obj);
        }

        /// <summary>
        /// Return all active objects to the pool
        /// </summary>
        public void ReturnAll()
        {
            var activeList = new List<T>(_active);
            foreach (var obj in activeList)
                Return(obj);
        }

        /// <summary>
        /// Destroy all pooled objects
        /// </summary>
        public void Clear()
        {
            ReturnAll();
            while (_available.Count > 0)
            {
                var obj = _available.Dequeue();
                if (obj != null)
                    Object.Destroy(obj.gameObject);
            }
            _active.Clear();
        }
    }

    /// <summary>
    /// AGENT 28: VFX Pool Manager
    /// Centralized VFX pooling system for particle effects
    /// Automatically returns particles after their lifetime
    /// </summary>
    public class VFXPoolManager : MonoBehaviour
    {
        static VFXPoolManager _instance;
        public static VFXPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("VFXPoolManager");
                    _instance = go.AddComponent<VFXPoolManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        readonly Dictionary<GameObject, ObjectPool<ParticleSystem>> _particlePools = new();

        /// <summary>
        /// Spawn a particle effect from the pool
        /// </summary>
        public ParticleSystem SpawnParticle(GameObject prefab, Vector3 position, Quaternion rotation, float autoReturnDelay = 0f)
        {
            if (prefab == null) return null;

            // Get or create pool for this prefab
            if (!_particlePools.TryGetValue(prefab, out var pool))
            {
                var particleSystem = prefab.GetComponent<ParticleSystem>();
                if (particleSystem == null)
                {
                    Debug.LogError($"[VFXPoolManager] Prefab {prefab.name} has no ParticleSystem component");
                    return null;
                }

                pool = new ObjectPool<ParticleSystem>(particleSystem, initialSize: 5, maxSize: 50, parent: transform);
                _particlePools[prefab] = pool;
            }

            var ps = pool.Get(position, rotation);
            ps.Play();

            // Auto-return after particle lifetime
            if (autoReturnDelay > 0f)
            {
                pool.ReturnAfterDelay(ps, autoReturnDelay, this);
            }
            else if (ps.main.loop == false)
            {
                // Auto-calculate return time based on particle lifetime
                float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
                pool.ReturnAfterDelay(ps, lifetime, this);
            }

            return ps;
        }

        // DISABLED: Generic GameObject pooling broken (GameObject is not Component)
        // Use SpawnParticle for ParticleSystem VFX instead
        /*
        /// <summary>
        /// Spawn a generic VFX GameObject from the pool
        /// </summary>
        public GameObject SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, float autoReturnDelay = 5f)
        {
            if (prefab == null) return null;

            // Get or create pool for this prefab
            if (!_genericPools.TryGetValue(prefab, out var pool))
            {
                pool = new ObjectPool<GameObject>(prefab, initialSize: 5, maxSize: 50, parent: transform);
                _genericPools[prefab] = pool;
            }

            var vfx = pool.Get(position, rotation);

            if (autoReturnDelay > 0f)
                pool.ReturnAfterDelay(vfx, autoReturnDelay, this);

            return vfx;
        }
        */

        /// <summary>
        /// Return a particle effect to its pool
        /// </summary>
        public void ReturnParticle(ParticleSystem ps)
        {
            if (ps == null) return;

            ps.Stop();
            ps.Clear();

            foreach (var pool in _particlePools.Values)
            {
                pool.Return(ps);
                return;
            }
        }

        // DISABLED: Generic GameObject pooling broken (GameObject is not Component)
        /*
        /// <summary>
        /// Return a VFX GameObject to its pool
        /// </summary>
        public void ReturnVFX(GameObject vfx)
        {
            if (vfx == null) return;

            foreach (var pool in _genericPools.Values)
            {
                pool.Return(vfx);
                return;
            }
        }
        */

        /// <summary>
        /// Get pool statistics for debugging
        /// </summary>
        public string GetPoolStats()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== VFX Pool Statistics ===");

            foreach (var kvp in _particlePools)
            {
                sb.AppendLine($"{kvp.Key.name}: {kvp.Value.CountActive} active, {kvp.Value.CountAvailable} available, {kvp.Value.CountTotal} total");
            }

            // DISABLED: _genericPools removed (GameObject not Component)
            /*
            foreach (var kvp in _genericPools)
            {
                sb.AppendLine($"{kvp.Key.name}: {kvp.Value.CountActive} active, {kvp.Value.CountAvailable} available, {kvp.Value.CountTotal} total");
            }
            */

            return sb.ToString();
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
