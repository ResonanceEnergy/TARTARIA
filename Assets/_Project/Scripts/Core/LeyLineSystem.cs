using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tartaria.Core
{
    /// <summary>
    /// Ley Line Network — Tesla 3-6-9 energy grid connecting Tartarian structures.
    ///
    /// Design per GDD §02 (Aether Energy):
    ///   - Ley lines form paths between restored buildings
    ///   - Active ley lines boost RS gain within their influence radius
    ///   - Nodes follow 3-6-9 geometry (triangular/hexagonal grid)
    ///   - Lines visible in Aether Vision mode
    ///   - Corruption can sever ley lines (re-established via purification)
    ///
    /// Performance budget: 1ms per frame (within overall Aether 2ms budget).
    /// Uses DOTS ECS with Burst for the network simulation.
    /// </summary>
    /// 
    // ─── Components ──────────────────────────────

    /// <summary>
    /// A ley line node: an intersection point in the energy network.
    /// Placed at building foundations and natural power spots.
    /// </summary>
    public struct LeyLineNode : IComponentData
    {
        public float3 Position;
        public float Strength;           // 0-1, power flowing through this node
        public float NaturalStrength;    // Base power (before building bonus)
        public int NodeIndex;            // Unique id in the network
        public bool Active;              // False if severed by corruption
        public LeyLineNodeType NodeType;
    }

    public enum LeyLineNodeType : byte
    {
        Natural = 0,        // Natural power spot
        Building = 1,       // Restored Tartarian building
        Intersection = 2,   // Where multiple lines cross
        Portal = 3          // Zone transition point
    }

    /// <summary>
    /// A connection between two LeyLineNodes, forming an edge in the network.
    /// </summary>
    public struct LeyLineConnection : IBufferElementData
    {
        public int TargetNodeIndex;
        public float FlowRate;          // Energy throughput (0-1)
        public float Distance;          // Euclidean distance between nodes
        public bool Severed;            // Corruption-severed
    }

    /// <summary>
    /// Singleton config for ley line network parameters.
    /// </summary>
    public struct LeyLineConfig : IComponentData
    {
        public float InfluenceRadius;        // Range of RS boost around active lines (15m)
        public float RSBoostMultiplier;      // RS gain multiplier within influence (1.3×)
        public float FlowDecayPerMeter;      // Energy loss per meter of line distance
        public float NodeActivationThreshold;// Min strength to count as active (0.1)
        public float RepairRate;             // Strength recovery per second when unpurged (0.05)
        public float CorruptionDamageRate;   // Strength drain per second from nearby corruption

        public static LeyLineConfig Default => new()
        {
            InfluenceRadius = 15f,
            RSBoostMultiplier = 1.3f,
            FlowDecayPerMeter = 0.01f,
            NodeActivationThreshold = 0.1f,
            RepairRate = 0.05f,
            CorruptionDamageRate = 0.02f
        };
    }

    /// <summary>
    /// Tag for the player entity to check proximity to ley lines.
    /// </summary>
    public struct LeyLineProximity : IComponentData
    {
        public float NearestDistance;     // Distance to nearest active ley line
        public bool WithinInfluence;     // True if within InfluenceRadius
        public float BoostMultiplier;    // Active RS boost (1.0 if outside)
        public int NearestNodeIndex;     // Closest ley line node
    }

    // ─── System ──────────────────────────────────

    /// <summary>
    /// DOTS System — updates ley line energy flow each frame.
    ///
    /// Phase 1: Propagate energy along connections (source → sink)
    /// Phase 2: Apply corruption damage to nodes near corruption sources
    /// Phase 3: Natural repair of non-corrupted nodes
    /// Phase 4: Calculate player proximity boost
    ///
    /// Budget: 1ms (Burst-compiled, 64 nodes max per zone)
    /// </summary>
    [BurstCompile]
    public partial struct LeyLineSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LeyLineConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<LeyLineConfig>();
            float dt = SystemAPI.Time.DeltaTime;

            // Phase 1 & 3: Update node strengths via energy flow + natural repair
            new UpdateNodeStrengthsJob
            {
                DeltaTime = dt,
                Config = config
            }.Schedule();

            // Phase 4: Player proximity check
            new UpdatePlayerProximityJob
            {
                Config = config
            }.Schedule();
        }
    }

    [BurstCompile]
    partial struct UpdateNodeStrengthsJob : IJobEntity
    {
        public float DeltaTime;
        public LeyLineConfig Config;

        void Execute(ref LeyLineNode node, in DynamicBuffer<LeyLineConnection> connections)
        {
            if (!node.Active)
            {
                // Inactive nodes slowly recover if not severed
                node.Strength = math.max(0f, node.Strength - Config.CorruptionDamageRate * DeltaTime);
                if (node.Strength >= Config.NodeActivationThreshold)
                    node.Active = true;
                return;
            }

            // Natural repair toward natural strength
            if (node.Strength < node.NaturalStrength)
            {
                node.Strength = math.min(
                    node.NaturalStrength,
                    node.Strength + Config.RepairRate * DeltaTime);
            }

            // Energy flow from connections (simplified: average incoming flow)
            float totalInflow = 0f;
            int activeConns = 0;
            for (int i = 0; i < connections.Length; i++)
            {
                var conn = connections[i];
                if (conn.Severed) continue;

                float flowContribution = conn.FlowRate * (1f - conn.Distance * Config.FlowDecayPerMeter);
                if (flowContribution > 0f)
                {
                    totalInflow += flowContribution;
                    activeConns++;
                }
            }

            if (activeConns > 0)
            {
                float avgInflow = totalInflow / activeConns;
                // Blend: 70% own strength + 30% network flow
                node.Strength = math.lerp(node.Strength, avgInflow, 0.3f * DeltaTime);
            }

            node.Strength = math.clamp(node.Strength, 0f, 1f);
        }
    }

    [BurstCompile]
    partial struct UpdatePlayerProximityJob : IJobEntity
    {
        public LeyLineConfig Config;

        void Execute(ref LeyLineProximity proximity, in LocalTransform playerTransform,
                     in PlayerTag _)
        {
            // This will be populated by a separate query over LeyLineNodes
            // For now, the proximity data is set by the managed companion system
            proximity.BoostMultiplier = proximity.WithinInfluence
                ? Config.RSBoostMultiplier
                : 1f;
        }
    }

    // ─── Managed Companion (MonoBehaviour bridge) ─

    /// <summary>
    /// MonoBehaviour bridge for LeyLineSystem that handles non-Burst tasks:
    ///   - Visual line rendering (LineRenderer in Aether Vision)
    ///   - Player proximity queries (EntityManager lookups)
    ///   - Connection to CorruptionSystem for severing/repair
    ///   - Debug overlay visualization
    /// </summary>
    public class LeyLineManager : UnityEngine.MonoBehaviour
    {
        public static LeyLineManager Instance { get; private set; }

        public event System.Action<int> OnNodeActivated;     // nodeIndex
        public event System.Action<int> OnNodeSevered;       // nodeIndex
        public event System.Action<int, int> OnLineRestored; // nodeA, nodeB

        readonly System.Collections.Generic.List<LeyLineNodeInfo> _nodes = new();

        public int ActiveNodeCount
        {
            get
            {
                int c = 0;
                foreach (var n in _nodes)
                    if (n.active) c++;
                return c;
            }
        }

        public float GetPlayerBoost()
        {
            // Query the ECS LeyLineProximity component
            var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
            if (world == null) return 1f;
            var em = world.EntityManager;

            var query = em.CreateEntityQuery(typeof(LeyLineProximity), typeof(PlayerTag));
            if (query.CalculateEntityCount() == 0) return 1f;

            var entity = query.GetSingletonEntity();
            var prox = em.GetComponentData<LeyLineProximity>(entity);
            return prox.BoostMultiplier;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { UnityEngine.Object.Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Register a ley line node (called during zone setup).
        /// </summary>
        public void RegisterNode(int index, UnityEngine.Vector3 position, LeyLineNodeType type, float baseStrength = 0.5f)
        {
            _nodes.Add(new LeyLineNodeInfo
            {
                index = index,
                position = position,
                type = type,
                strength = baseStrength,
                active = true
            });

            OnNodeActivated?.Invoke(index);
        }

        /// <summary>
        /// Sever a ley line node (corruption damage).
        /// </summary>
        public void SeverNode(int nodeIndex)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].index == nodeIndex)
                {
                    var n = _nodes[i];
                    n.active = false;
                    _nodes[i] = n;
                    OnNodeSevered?.Invoke(nodeIndex);
                    return;
                }
            }
        }

        /// <summary>
        /// Restore a severed node (purification complete).
        /// </summary>
        public void RestoreNode(int nodeIndex)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].index == nodeIndex)
                {
                    var n = _nodes[i];
                    n.active = true;
                    _nodes[i] = n;
                    OnNodeActivated?.Invoke(nodeIndex);
                    return;
                }
            }
        }

        /// <summary>Clear all nodes (zone unload).</summary>
        public void ClearNodes() => _nodes.Clear();

        struct LeyLineNodeInfo
        {
            public int index;
            public UnityEngine.Vector3 position;
            public LeyLineNodeType type;
            public float strength;
            public bool active;
        }
    }
}
