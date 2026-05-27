using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Core
{
    /// <summary>
    /// Particle Effect Pool — object pooling for frequently spawned VFX.
    /// Reduces GC allocations from particle Instantiate/Destroy spam.
    /// Auto-pools common effects: explosions, hits, sparks, aura trails.
    /// </summary>
    public class ParticleEffectPool : MonoBehaviour
    {
        public static ParticleEffectPool Instance { get; private set; }

        [Header("Pool Config")]
        [SerializeField] int defaultPoolSize = 20;
        [SerializeField] bool expandPool = true;

        readonly Dictionary<string, Queue<GameObject>> _pools = new();
        readonly Dictionary<string, GameObject> _prefabs = new();
        readonly HashSet<GameObject> _active = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("ParticleEffectPool");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<ParticleEffectPool>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Register a prefab for pooling. Call before first Spawn().
        /// </summary>
        public void RegisterPrefab(string effectName, GameObject prefab, int poolSize = -1)
        {
            if (_prefabs.ContainsKey(effectName)) return;

            _prefabs[effectName] = prefab;
            _pools[effectName] = new Queue<GameObject>();

            int size = (poolSize > 0) ? poolSize : defaultPoolSize;
            for (int i = 0; i < size; i++)
            {
                CreateInstance(effectName);
            }

            Debug.Log($"[ParticlePool] Registered {effectName} with {size} instances");
        }

        GameObject CreateInstance(string effectName)
        {
            if (!_prefabs.ContainsKey(effectName)) return null;

            var instance = Instantiate(_prefabs[effectName], transform);
            instance.SetActive(false);
            _pools[effectName].Enqueue(instance);
            return instance;
        }

        /// <summary>
        /// Spawn pooled particle effect at position/rotation. Auto-returns to pool after duration.
        /// </summary>
        public GameObject Spawn(string effectName, Vector3 position, Quaternion rotation, float duration = 2f)
        {
            if (!_pools.ContainsKey(effectName))
            {
                Debug.LogWarning($"[ParticlePool] Effect '{effectName}' not registered");
                return null;
            }

            GameObject instance;
            if (_pools[effectName].Count > 0)
            {
                instance = _pools[effectName].Dequeue();
            }
            else if (expandPool)
            {
                instance = CreateInstance(effectName);
            }
            else
            {
                Debug.LogWarning($"[ParticlePool] Pool exhausted for '{effectName}'");
                return null;
            }

            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);

            // Play particle system
            var ps = instance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }

            _active.Add(instance);

            // Auto-return to pool after duration
            StartCoroutine(ReturnAfterDelay(instance, effectName, duration));

            return instance;
        }

        System.Collections.IEnumerator ReturnAfterDelay(GameObject instance, string effectName, float delay)
        {
            yield return new WaitForSeconds(delay);
            Return(instance, effectName);
        }

        void Return(GameObject instance, string effectName)
        {
            if (instance == null) return;

            _active.Remove(instance);

            // Stop particles
            var ps = instance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
                ps.Clear();
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform);

            if (_pools.ContainsKey(effectName))
            {
                _pools[effectName].Enqueue(instance);
            }
        }

        /// <summary>
        /// Preload common effects (call from game boot)
        /// </summary>
        public void PreloadCommonEffects()
        {
            // Placeholder - would load from Resources or AssetBundle if needed
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
