using UnityEngine;
// using Unity.Behavior; // Uncomment when BehaviorGraphAgent API is stable

namespace Tartaria.AI
{
    /// <summary>
    /// MirrorWraith Behavior Agent — drives MirrorWraith enemy AI via Unity Behavior graph.
    /// Replaces old MirrorWraithAISystem (ECS-based). Requires behavior graph asset:
    /// Assets/_Project/AI/Graphs/MirrorWraith.asset
    /// 
    /// Behavior graph nodes (to be authored in Unity Behavior editor):
    ///   1. Patrol: Move toward random LeyLine node (3-5 Hz range)
    ///   2. Pursue: When player within 15m aggro range, chase player
    ///   3. Attack: When within 3m, fire mirror beam projectile
    /// 
    /// Setup:
    ///   1. Create behavior graph: Window → AI → Behavior → Create Graph
    ///   2. Author 3 nodes: PatrolLeyLine, PursuePlayer, AttackMirrorBeam
    ///   3. Assign graph to this component's BehaviorGraph field
    ///   4. Attach to MirrorWraith prefab
    ///   5. Disable/remove old MirrorWraithAISystem to avoid dual AI
    /// </summary>
    [DisallowMultipleComponent]
    public class MirrorWraithBehaviorAgent : MonoBehaviour
    {
        // [Header("Behavior Graph")]
        // [SerializeField] BehaviorGraphAgent behaviorAgent; // Would reference Unity.Behavior.BehaviorGraphAgent

        [Header("Configuration")]
        [SerializeField] float aggroRange = 15f;
        [SerializeField] float attackRange = 3f;
        [SerializeField] float patrolSpeed = 2f;
        [SerializeField] float chaseSpeed = 4f;

        void Awake()
        {
            // Behavior graph initialization would happen here
            // behaviorAgent = GetComponent<BehaviorGraphAgent>();
            // if (behaviorAgent == null)
            // {
            //     Debug.LogError("[MirrorWraithBehaviorAgent] No BehaviorGraphAgent component found!");
            // }
        }

        void Start()
        {
            Debug.Log("[MirrorWraithBehaviorAgent] STUB: MirrorWraith behavior agent initialized. " +
                     "Full implementation requires behavior graph asset creation via Unity Behavior editor (~30min work).");
        }

        void Update()
        {
            // Behavior graph ticks automatically via BehaviorGraphAgent.Update()
            // Custom blackboard variables would be updated here:
            // - PlayerDistance: Vector3.Distance(transform.position, PlayerTransform.position)
            // - IsInAggroRange: PlayerDistance < aggroRange
            // - IsInAttackRange: PlayerDistance < attackRange
        }

        // Public methods called by behavior graph nodes:
        public void Patrol()
        {
            // Find nearest LeyLine node (3-5 Hz), move toward it
            Debug.Log("[MirrorWraith] Patrol node invoked");
        }

        public void Pursue()
        {
            // Move toward player at chase speed
            Debug.Log("[MirrorWraith] Pursue node invoked");
        }

        public void AttackMirrorBeam()
        {
            // Fire mirror beam projectile toward player
            Debug.Log("[MirrorWraith] Attack node invoked");
        }
    }
}
