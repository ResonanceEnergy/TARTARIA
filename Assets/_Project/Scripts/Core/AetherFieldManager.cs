using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// AetherFieldManager — MonoBehaviour singleton that tracks the player's Resonance Score (RS).
    /// Lives in Core so both Gameplay and Integration can reference it without circular dependencies.
    ///
    /// The ECS-based AetherFieldSystem handles per-node field simulation;
    /// this manager tracks the global RS economy visible to all assemblies and exposes the
    /// Mono→ECS bridge API for source/sink registration + per-band intensity sampling
    /// (docs/15 §5 "Resonance Source/Sink Model").
    /// </summary>
    [DisallowMultipleComponent]
    public class AetherFieldManager : MonoBehaviour
    {
        public static AetherFieldManager Instance { get; private set; }

        [Header("Resonance Score")]
        [SerializeField, Range(0f, 100f)] float startingRS = 0f;

        [Header("Aether Charge")]
        [SerializeField, Range(0f, 100f)] float maxAetherCharge = 100f;

        float _resonanceScore;
        float _aetherCharge;

        public float ResonanceScore => _resonanceScore;
        public float AetherCharge => _aetherCharge;
        public float MaxAetherCharge => maxAetherCharge;
        public float AetherChargeNormalized => maxAetherCharge > 0 ? _aetherCharge / maxAetherCharge : 0f;

        public event System.Action<float> OnResonanceScoreChanged;
        public event System.Action<float> OnAetherChargeChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            _resonanceScore = startingRS;
        }

        void Start()
        {
            // P2.L2: prime HUD with initial Aether so the meter isn't empty until first event.
            // GameEvents.cs:149, 567 — Action<float> aetherValue (0-100).
            GameEvents.RaiseAetherEnergyChanged(_aetherCharge);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void AddResonanceScore(float amount)
        {
            _resonanceScore = Mathf.Clamp(_resonanceScore + amount, 0f, 100f);
            OnResonanceScoreChanged?.Invoke(_resonanceScore);
        }

        public void AddFieldEnergy(float amount)
        {
            AddResonanceScore(amount);
        }

        public void DeductRS(float amount)
        {
            AddResonanceScore(-amount);
        }

        public void AddAetherCharge(float amount)
        {
            _aetherCharge = Mathf.Clamp(_aetherCharge + amount, 0f, maxAetherCharge);
            OnAetherChargeChanged?.Invoke(_aetherCharge);
            // P2.L2: publish to global HUD bus (Sprint 11 L9 finding — bar was static at 75% because nothing fired).
            // OnAetherEnergyChanged signature: Action<float> aetherValue (0-100). See GameEvents.cs:149, 567.
            GameEvents.RaiseAetherEnergyChanged(_aetherCharge);
        }

        public void DeductAetherCharge(float amount)
        {
            AddAetherCharge(-amount);
        }

        public bool CanSpendAetherCharge(float amount)
        {
            return _aetherCharge >= amount;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Mono → ECS bridge: source / sink registration + band intensity probe
        // (docs/15 §5 "Resonance Source/Sink Model")
        //
        // Restored buildings / tuned nodes call RegisterSource(transform, band, ...)
        // when activated; corruption patches & Mud Golems call RegisterSink(...).
        // The returned AetherFieldHandle is used to unregister on destroy.
        //
        // GetBandIntensity(pos, band) samples the live AetherNode entities the
        // Burst AetherFieldSystem writes to each frame, returning the strongest
        // matching-band node intensity within ~20m of the query position. Falls
        // back to 0f when ECS world isn't ready (pre-bootstrap / scene unload).
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Opaque handle returned by RegisterSource/Sink; pass to UnregisterHandle on cleanup.</summary>
        public readonly struct AetherFieldHandle
        {
            internal readonly Entity Entity;
            internal readonly Transform Transform;
            internal readonly bool IsSink;
            internal AetherFieldHandle(Entity e, Transform t, bool sink) { Entity = e; Transform = t; IsSink = sink; }
            public bool IsValid => Entity != Entity.Null;
            public static AetherFieldHandle Invalid => default;
        }

        readonly List<AetherFieldHandle> _liveHandles = new List<AetherFieldHandle>(64);

        /// <summary>
        /// Register a building / tuned node as an Aether source. Restored Dome=1.0/50m, Fountain=0.6/30m,
        /// Spire=0.8/40m, Tuned Node=0.3/15m (docs/15 §5). Sources push their band's Coherence up on
        /// nearby AetherNodes per the Burst sim falloff (1 − dist/radius)².
        /// </summary>
        public AetherFieldHandle RegisterSource(Transform sourceTransform, HarmonicBand band, float strength = 0.6f, float radius = 30f)
        {
            if (sourceTransform == null)
            {
                Debug.LogWarning("[AetherFieldManager] RegisterSource called with null transform — ignoring.");
                return AetherFieldHandle.Invalid;
            }
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                Debug.LogWarning("[AetherFieldManager] RegisterSource called before ECS world ready — source not registered.");
                return AetherFieldHandle.Invalid;
            }
            var em = world.EntityManager;
            var entity = em.CreateEntity(typeof(AetherSource), typeof(LocalTransform));
            em.SetComponentData(entity, new AetherSource { Strength = strength, Radius = radius, Band = band });
            em.SetComponentData(entity, LocalTransform.FromPosition(sourceTransform.position));
            em.SetName(entity, $"AetherSource[{sourceTransform.name}/{band}]");
            var handle = new AetherFieldHandle(entity, sourceTransform, sink: false);
            _liveHandles.Add(handle);
            return handle;
        }

        /// <summary>
        /// Register an Aether sink (corruption patch, Mud Golem, etc.). Strength typically negative
        /// (e.g. golem −0.5, patch −0.2 per docs/15 §5). Pass positive magnitude; sign is interpreted
        /// by the Burst job's sink loop.
        /// </summary>
        public AetherFieldHandle RegisterSink(Transform sinkTransform, float strength = 0.2f, float radius = 10f)
        {
            if (sinkTransform == null) return AetherFieldHandle.Invalid;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return AetherFieldHandle.Invalid;
            var em = world.EntityManager;
            var entity = em.CreateEntity(typeof(AetherSink), typeof(LocalTransform));
            em.SetComponentData(entity, new AetherSink { Strength = -Mathf.Abs(strength), Radius = radius });
            em.SetComponentData(entity, LocalTransform.FromPosition(sinkTransform.position));
            em.SetName(entity, $"AetherSink[{sinkTransform.name}]");
            var handle = new AetherFieldHandle(entity, sinkTransform, sink: true);
            _liveHandles.Add(handle);
            return handle;
        }

        public void UnregisterHandle(AetherFieldHandle handle)
        {
            if (!handle.IsValid) return;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null && world.IsCreated && world.EntityManager.Exists(handle.Entity))
            {
                world.EntityManager.DestroyEntity(handle.Entity);
            }
            _liveHandles.Remove(handle);
        }

        void LateUpdate()
        {
            // Cheap per-frame sync: push each registered transform position into its ECS LocalTransform.
            // List is sparse (few dozen sources in Moon 1); negligible cost. Removes stale entries lazily.
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;
            var em = world.EntityManager;
            for (int i = _liveHandles.Count - 1; i >= 0; i--)
            {
                var h = _liveHandles[i];
                if (h.Transform == null || !em.Exists(h.Entity))
                {
                    _liveHandles.RemoveAt(i);
                    continue;
                }
                em.SetComponentData(h.Entity, LocalTransform.FromPosition(h.Transform.position));
            }
        }

        /// <summary>
        /// Sample the strongest matching-band AetherNode intensity within 20m of the query position.
        /// Returns 0f when no nodes are present (pre-bootstrap, scene unload, or genuinely zero field).
        /// Saturates at 1.0f per Burst sim invariant (math.saturate on node.Intensity).
        /// </summary>
        public float GetBandIntensity(Vector3 worldPosition, HarmonicBand band)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return 0f;
            var em = world.EntityManager;
            using var query = em.CreateEntityQuery(ComponentType.ReadOnly<AetherNode>(), ComponentType.ReadOnly<LocalTransform>());
            if (query.IsEmpty)
            {
                // No grid baked yet — fall back to a synthetic estimate from registered sources so
                // callers get a useful non-zero reading during early-frame / pre-sim windows.
                return EstimateFromSourcesFallback(worldPosition, band);
            }
            var nodes = query.ToComponentDataArray<AetherNode>(Allocator.Temp);
            var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            const float SAMPLE_RADIUS = 20f;
            float best = 0f;
            float3 qp = new float3(worldPosition.x, worldPosition.y, worldPosition.z);
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Band != band) continue;
                float dist = math.distance(qp, transforms[i].Position);
                if (dist > SAMPLE_RADIUS) continue;
                float weighted = nodes[i].Intensity * (1f - dist / SAMPLE_RADIUS);
                if (weighted > best) best = weighted;
            }
            nodes.Dispose();
            transforms.Dispose();
            return best;
        }

        float EstimateFromSourcesFallback(Vector3 worldPosition, HarmonicBand band)
        {
            // Equivalent of the Burst job's source-influence loop, evaluated for a single position.
            // Used when no AetherNode grid entities exist (e.g. Moon scenes without baked nodes).
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return 0f;
            var em = world.EntityManager;
            using var srcQuery = em.CreateEntityQuery(ComponentType.ReadOnly<AetherSource>(), ComponentType.ReadOnly<LocalTransform>());
            if (srcQuery.IsEmpty) return 0f;
            var sources = srcQuery.ToComponentDataArray<AetherSource>(Allocator.Temp);
            var srcXf = srcQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            float3 qp = new float3(worldPosition.x, worldPosition.y, worldPosition.z);
            float total = 0f;
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i].Band != band) continue;
                float dist = math.distance(qp, srcXf[i].Position);
                if (dist >= sources[i].Radius) continue;
                float falloff = 1f - (dist / sources[i].Radius);
                total += sources[i].Strength * falloff * falloff;
            }
            sources.Dispose();
            srcXf.Dispose();
            return math.saturate(total);
        }
    }
}
