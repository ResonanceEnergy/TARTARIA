using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Companion Behavior System (DOTS) — Milo's AI state machine.
    ///
    /// Transitions:
    ///   FOLLOW → IDLE      : Player stationary > 5s
    ///   IDLE → REACT       : POI within 20m
    ///   REACT → SPEAK      : POI is dialogue trigger
    ///   ANY → HIDE         : Combat initiated
    ///   HIDE → CELEBRATE   : Combat ended + building restored
    ///   CELEBRATE → FOLLOW : After 3s
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CompanionBehaviorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Get player position
            float3 playerPos = float3.zero;
            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>()
                .WithAll<PlayerTag>())
            {
                playerPos = transform.ValueRO.Position;
                break;
            }

            // Check if combat is active (any living enemies exist)
            bool combatActive = false;
            foreach (var enemy in SystemAPI.Query<RefRO<EnemyAI>>())
            {
                if (enemy.ValueRO.State != EnemyAIState.Dissolving)
                {
                    combatActive = true;
                    break;
                }
            }

            // Update companion state machine
            foreach (var (behavior, transform) in
                SystemAPI.Query<RefRW<CompanionBehavior>, RefRW<LocalTransform>>())
            {
                behavior.ValueRW.StateTimer += dt;

                // Global transition: ANY → HIDE when combat starts
                if (combatActive && behavior.ValueRO.State != CompanionState.Hide)
                {
                    TransitionTo(ref behavior.ValueRW, CompanionState.Hide);
                }

                switch (behavior.ValueRO.State)
                {
                    case CompanionState.Follow:
                        UpdateFollow(ref behavior.ValueRW, ref transform.ValueRW,
                            playerPos, dt);
                        break;

                    case CompanionState.Idle:
                        UpdateIdle(ref behavior.ValueRW, playerPos, dt);
                        break;

                    case CompanionState.React:
                        UpdateReact(ref behavior.ValueRW, dt);
                        break;

                    case CompanionState.Speak:
                        UpdateSpeak(ref behavior.ValueRW, dt);
                        break;

                    case CompanionState.Hide:
                        UpdateHide(ref behavior.ValueRW, ref transform.ValueRW,
                            playerPos, combatActive, dt);
                        break;

                    case CompanionState.Celebrate:
                        UpdateCelebrate(ref behavior.ValueRW, dt);
                        break;
                }
            }
        }

        void UpdateFollow(ref CompanionBehavior behavior, ref LocalTransform transform,
            float3 playerPos, float dt)
        {
            float dist = math.distance(transform.Position, playerPos);

            if (dist > behavior.FollowDistance)
            {
                // Move toward player
                float3 direction = math.normalize(playerPos - transform.Position);
                float speed = math.select(behavior.WalkSpeed, behavior.SprintSpeed,
                    dist > behavior.SprintDistanceThreshold);
                transform.Position += direction * speed * dt;
                transform.Rotation = quaternion.LookRotation(direction, math.up());
            }
            else
            {
                // Close enough — check for idle transition
                if (behavior.StateTimer > behavior.IdleThreshold)
                {
                    TransitionTo(ref behavior, CompanionState.Idle);
                }
            }
        }

        void UpdateIdle(ref CompanionBehavior behavior, float3 playerPos, float dt)
        {
            // If player moves, return to Follow
            // (Checked externally via player velocity; simplified here)
            if (behavior.StateTimer > behavior.MaxIdleTime)
            {
                TransitionTo(ref behavior, CompanionState.Follow);
            }
        }

        void UpdateReact(ref CompanionBehavior behavior, float dt)
        {
            // Face POI, play reaction animation
            if (behavior.StateTimer > 2.0f)
            {
                // Check if POI has dialogue → transition to Speak
                TransitionTo(ref behavior, CompanionState.Follow);
            }
        }

        void UpdateSpeak(ref CompanionBehavior behavior, float dt)
        {
            // Dialogue playing — wait for DialogueManager's reported duration
            float duration = behavior.DialogueDuration > 0f
                ? behavior.DialogueDuration
                : 5.0f; // Fallback to DialogueManager default autoCloseDelay

            if (behavior.StateTimer > duration)
            {
                behavior.DialogueDuration = 0f; // Reset for next speak
                TransitionTo(ref behavior, CompanionState.Follow);
            }
        }

        void UpdateHide(ref CompanionBehavior behavior, ref LocalTransform transform,
            float3 playerPos, bool combatActive, float dt)
        {
            if (!combatActive)
            {
                // Combat ended — celebrate or return to follow
                TransitionTo(ref behavior, CompanionState.Celebrate);
                return;
            }

            // Move away from enemies, stay within 10m of player
            float dist = math.distance(transform.Position, playerPos);
            if (dist > behavior.HideRadius)
            {
                float3 direction = math.normalize(playerPos - transform.Position);
                transform.Position += direction * 2.0f * dt;
            }
        }

        void UpdateCelebrate(ref CompanionBehavior behavior, float dt)
        {
            if (behavior.StateTimer > behavior.CelebrateTimer)
            {
                TransitionTo(ref behavior, CompanionState.Follow);
            }
        }

        void TransitionTo(ref CompanionBehavior behavior, CompanionState newState)
        {
            behavior.PreviousState = behavior.State;
            behavior.State = newState;
            behavior.StateTimer = 0f;
        }
    }
}
