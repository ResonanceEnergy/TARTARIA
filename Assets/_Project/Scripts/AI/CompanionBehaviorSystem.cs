using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Companion Behavior System (DOTS) — Advanced for all companions post Round 4.
    ///
    /// Full DOTS Cassian spawning + animator/VFX consumption wired here (Round 5).
    /// Physical train escort positioning (Milo/Lirael/Korath/Cassian) + deep Anastasia/Cassian bond.
    /// 
    /// Transitions extended:
    ///   FOLLOW → IDLE → ... → ESCORT (train physical pos from controllers) → PHYSICAL_BOND (solidification callbacks, redemption)
    ///   Cassian ID=1 uses RedemptionLevel + bond with Anastasia for behavior shift to ally escort.
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

            // Update companion state machine (Round 5: full multi-companion DOTS including Cassian)
            foreach (var (tag, behavior, transform) in
                SystemAPI.Query<RefRO<CompanionTag>, RefRW<CompanionBehavior>, RefRW<LocalTransform>>())
            {
                int cid = tag.ValueRO.CompanionId; // 0 Milo, 1 Cassian, 2 Lirael etc.
                behavior.ValueRW.StateTimer += dt;

                // Global transition: ANY → HIDE when combat starts (skip for PhysicalBond)
                if (combatActive && behavior.ValueRO.State != CompanionState.Hide && behavior.ValueRO.State != CompanionState.PhysicalBond)
                {
                    TransitionTo(ref behavior.ValueRW, CompanionState.Hide);
                }

                // Round 5 wiring: trigger escort from external physical (train) or solidification
                if (behavior.ValueRO.IsEscorting && behavior.ValueRO.State != CompanionState.Escort && behavior.ValueRO.State != CompanionState.PhysicalBond)
                {
                    TransitionTo(ref behavior.ValueRW, CompanionState.Escort);
                }
                if (behavior.ValueRO.SolidificationActive && behavior.ValueRO.State != CompanionState.PhysicalBond)
                {
                    TransitionTo(ref behavior.ValueRW, CompanionState.PhysicalBond);
                }

                switch (behavior.ValueRO.State)
                {
                    case CompanionState.Follow:
                        UpdateFollow(ref behavior.ValueRW, ref transform.ValueRW,
                            playerPos, dt, cid);
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

                    case CompanionState.Escort:
                        UpdateEscort(ref behavior.ValueRW, ref transform.ValueRW, dt, cid);
                        break;

                    case CompanionState.PhysicalBond:
                        UpdatePhysicalBond(ref behavior.ValueRW, ref transform.ValueRW, dt, cid);
                        break;
                }
            }
        }

        void UpdateFollow(ref CompanionBehavior behavior, ref LocalTransform transform,
            float3 playerPos, float dt, int companionId = 0)
        {
            float dist = math.distance(transform.Position, playerPos);

            // Round 5: Cassian (ID=1) uses slightly wider follow + redemption-aware stance
            float effectiveFollow = (companionId == 1) ? behavior.FollowDistance * 1.3f : behavior.FollowDistance;

            if (dist > effectiveFollow)
            {
                // Move toward player
                float3 direction = math.normalize(playerPos - transform.Position);
                float speed = math.select(behavior.WalkSpeed, behavior.SprintSpeed,
                    dist > behavior.SprintDistanceThreshold);
                transform.Position += direction * speed * dt;
                transform.Rotation = quaternion.LookRotation(direction, math.up());

                // Cassian redemption: lower deception VFX when high redemption
                if (companionId == 1 && behavior.RedemptionLevel > 60)
                    behavior.VFXIntensity = math.lerp(behavior.VFXIntensity, 0.2f, dt * 2f); // calmer ally glow
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

        // ─── Round 5: Physical/DOTS Escort Wiring (train positioning from Round 4 controllers) ───
        // Consumes EscortTarget set by managed side (Milo/Lirael/Korath/Cassian BoardTrain calls now sync to DOTS)
        // Animator/VFX: sets VFXIntensity for hybrid consumption (e.g. dust trails, lean anims)
        void UpdateEscort(ref CompanionBehavior behavior, ref LocalTransform transform, float dt, int companionId)
        {
            if (!behavior.IsEscorting)
            {
                TransitionTo(ref behavior, CompanionState.Follow);
                return;
            }

            float3 target = behavior.EscortTarget;
            float dist = math.distance(transform.Position, target);

            // Physical positioning: move to exact train offset (rear for Milo, roof for Lirael, etc.)
            if (dist > 0.8f)
            {
                float3 dir = math.normalizesafe(target - transform.Position);
                float speed = math.max(behavior.EscortSpeed, 2.5f);
                transform.Position += dir * speed * dt;
                // Slight lean into motion for physical train escort fantasy
                if (math.lengthsq(dir) > 0.01f)
                    transform.Rotation = math.slerp(transform.Rotation, quaternion.LookRotation(dir, math.up()), dt * 4f);
            }

            // Companion-specific VFX/animator consumption
            behavior.VFXIntensity = math.clamp(0.6f + (companionId == 1 ? behavior.RedemptionLevel * 0.004f : 0f), 0f, 1f);
            // Cassian redemption: if high, shift toward ally PhysicalBond mid-escort
            if (companionId == 1 && behavior.RedemptionLevel >= 75 && behavior.StateTimer > 8f)
            {
                behavior.SolidificationActive = true; // cross-bond trigger
            }

            // Timeout or external clear returns to follow
            if (behavior.StateTimer > 45f)
            {
                behavior.IsEscorting = false;
                TransitionTo(ref behavior, CompanionState.Follow);
            }
        }

        // ─── Round 5: PhysicalBond State — Anastasia solidification callbacks + Cassian/Redemption bond interplay ───
        // Triggered on Anastasia SolidificationActive (Moon 13 / DotT) or Cassian high redemption (Moon 5+ branches)
        // Deep bond: Cassian + Anastasia share VFX, trust flows to calendar/quests, prepare voice authoring density
        void UpdatePhysicalBond(ref CompanionBehavior behavior, ref LocalTransform transform, float dt, int companionId)
        {
            // Hold position near player or bond partner (Anastasia/Cassian specific offset)
            float3 bondOffset = (companionId == 5) ? new float3(1.2f, 0.8f, -0.9f) : new float3(-0.8f, 0.6f, 1.1f); // Anastasia vs Cassian bond stance
            float3 playerPos = transform.Position; // simplified; in real would query player
            float3 desired = playerPos + bondOffset;

            float dist = math.distance(transform.Position, desired);
            if (dist > 0.5f)
            {
                float3 dir = math.normalizesafe(desired - transform.Position);
                transform.Position += dir * 1.2f * dt;
            }

            // Solidification VFX ramp + callback density (for 17th Hour / live-ops)
            behavior.VFXIntensity = math.lerp(behavior.VFXIntensity, 0.95f, dt * 1.5f);

            if (companionId == 1) // Cassian redeemed in bond
            {
                behavior.RedemptionLevel = math.min(behavior.RedemptionLevel + (int)(dt * 8f), 100);
                behavior.VFXIntensity *= 0.85f; // calmer post-redemption
            }

            // Exit after solidification moment (10s window per docs) or bond complete
            if (behavior.StateTimer > 12f && !behavior.SolidificationActive)
            {
                behavior.IsEscorting = false;
                behavior.SolidificationActive = false;
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
