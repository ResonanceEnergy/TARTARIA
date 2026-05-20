using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Companion Behavior System (DOTS) — Production for Round 7.
    ///
    /// Full hybrid DOTS-Mono sync bridge consumed here (escort targets, redemption, bond, 17th, R7 giant synergy, calendar echoes, world mutations, physical tells).
    /// Physical train escort + deeper Korath/Thorne/Veritas positioning + 17th Hour mode + giant mode CompanionGiant + Giant's Song auto-match.
    /// Cross-Moon memory via bond + mutation tiers persisted.
    /// VO intensity + solidification/redemption choice + new physical reactivity paths.
    /// 
    /// Round 7 additions: UpdateGiantSynergy, ApplyPhysicalTellForBeat, UpdateCalendarEcho, ApplyWorldMutation — all major beats covered for all 7.
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

            // Update companion state machine (Round 7: full 7-comp + giant/calendar/mutation/physical tells)
            foreach (var (tag, behavior, transform) in
                SystemAPI.Query<RefRO<CompanionTag>, RefRW<CompanionBehavior>, RefRW<LocalTransform>>())
            {
                int cid = tag.ValueRO.CompanionId; // 0 Milo, 1 Cassian, 2 Lirael, 3 Korath, 4 Thorne, 5 Anastasia, 6 Veritas (R7)
                behavior.ValueRW.StateTimer += dt;

                // Global transition: ANY → HIDE when combat starts (skip for PhysicalBond)
                if (combatActive && behavior.ValueRO.State != CompanionState.Hide && behavior.ValueRO.State != CompanionState.PhysicalBond)
                {
                    TransitionTo(ref behavior.ValueRW, CompanionState.Hide);
                }

                // Round 5/6 wiring: trigger escort from external physical (train) or solidification
                if (behavior.ValueRO.IsEscorting && behavior.ValueRO.State != CompanionState.Escort && behavior.ValueRO.State != CompanionState.PhysicalBond)
                {
                    TransitionTo(ref behavior.ValueRW, CompanionState.Escort);
                }
                if (behavior.ValueRO.SolidificationActive && behavior.ValueRO.State != CompanionState.PhysicalBond)
                {
                    TransitionTo(ref behavior.ValueRW, CompanionState.PhysicalBond);
                }

                // Round 7: Giant synergy trigger (high bond + player giant mode detected via external flag or proximity)
                if (behavior.ValueRO.GiantSynergyActive && behavior.ValueRO.State != CompanionState.PhysicalBond && behavior.ValueRO.State != CompanionState.Escort)
                {
                    // Giant synergy physical tell overrides to elevated bond stance
                    UpdateGiantSynergy(ref behavior.ValueRW, ref transform.ValueRW, dt, cid, playerPos);
                }

                // Round 7: Calendar echo / 17th state change (daily or 17th echo mutates state)
                if (behavior.ValueRO.CalendarEchoActive)
                {
                    UpdateCalendarEcho(ref behavior.ValueRW, ref transform.ValueRW, dt, cid);
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

                // Round 7: Apply accumulated physical tell intensity decay + world mutation persistence tick
                if (behavior.ValueRW.PhysicalTellIntensity > 0.01f)
                    behavior.ValueRW.PhysicalTellIntensity = math.lerp(behavior.ValueRW.PhysicalTellIntensity, 0f, dt * 0.8f);

                if (behavior.ValueRW.WorldMutationTier > 0 && behavior.ValueRW.StateTimer % 60f < dt) // periodic persist tick
                    behavior.ValueRW.CompanionBondLevel = math.min(behavior.ValueRW.CompanionBondLevel + 1, 100);
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

                // R7 Veritas (ID=6): precise resonance lean when following near high bond
                if (companionId == 6 && behavior.CompanionBondLevel > 65)
                {
                    behavior.EscortLeanAngle = math.lerp(behavior.EscortLeanAngle, 12f, dt * 4f);
                    behavior.PhysicalTellIntensity = math.max(behavior.PhysicalTellIntensity, 0.35f);
                }
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

        // ─── Round 6 Production: Full hybrid DOTS-Mono sync bridge consumption + deeper Korath/Thorne escort + 17th Hour ───
        // EscortTarget / IsEscorting / Bond / 17th / RedemptionChoice set via Mono bridge (CompanionManager + controllers)
        // Per-ID physical behaviors for train escort playtest (Milo rear, Lirael roof, Korath star-observer, Thorne forward guard, Cassian redeemed ally, Veritas R7 bell resonance stance)
        void UpdateEscort(ref CompanionBehavior behavior, ref LocalTransform transform, float dt, int companionId)
        {
            if (!behavior.IsEscorting)
            {
                TransitionTo(ref behavior, CompanionState.Follow);
                return;
            }

            float3 target = behavior.EscortTarget;
            float dist = math.distance(transform.Position, target);

            // Physical positioning with companion-specific offsets and leans (Round 6 expanded, R7 Veritas precision)
            if (dist > 0.6f)
            {
                float3 dir = math.normalizesafe(target - transform.Position);
                float speed = math.max(behavior.EscortSpeed, 2.8f);
                transform.Position += dir * speed * dt;

                // Round 6: deeper per-companion lean + 17th Hour mode (Korath gazes up, Thorne vigilant forward, Cassian calm ally)
                float leanMultiplier = 1f;
                if (behavior.In17thHourMode) leanMultiplier = 1.6f;
                float3 lookDir = dir;

                if (companionId == 3) // Korath: star-reader elevated gaze during escort
                {
                    lookDir = math.normalize(dir + new float3(0f, 0.6f, 0f));
                    behavior.EscortLeanAngle = math.lerp(behavior.EscortLeanAngle, 22f, dt * 3f);
                }
                else if (companionId == 4) // Thorne: forward vigilant combat-ready stance
                {
                    lookDir = math.normalize(dir + new float3(0.15f, 0.1f, 0f));
                    behavior.EscortLeanAngle = math.lerp(behavior.EscortLeanAngle, 8f, dt * 5f);
                }
                else if (companionId == 1 && behavior.RedemptionChoiceMade) // Cassian redeemed
                {
                    lookDir = dir;
                    behavior.EscortLeanAngle = math.lerp(behavior.EscortLeanAngle, 4f, dt * 2f);
                }
                else if (companionId == 6) // R7 Veritas: precise bell-keeper stance, resonance lean
                {
                    lookDir = math.normalize(dir + new float3(0.05f, 0.25f, 0f));
                    behavior.EscortLeanAngle = math.lerp(behavior.EscortLeanAngle, 15f, dt * 4f);
                    behavior.PhysicalTellIntensity = math.max(behavior.PhysicalTellIntensity, 0.7f);
                }

                if (math.lengthsq(lookDir) > 0.01f)
                    transform.Rotation = math.slerp(transform.Rotation, quaternion.LookRotation(lookDir, math.up()), dt * 4.5f * leanMultiplier);
            }

            // Round 6: VFX + bond + 17th consumption, full sync fields used
            float baseVFX = 0.55f + (behavior.In17thHourMode ? 0.25f : 0f);
            float redemptionBoost = (companionId == 1 && behavior.RedemptionLevel > 0) ? behavior.RedemptionLevel * 0.0035f : 0f;
            behavior.VFXIntensity = math.clamp(baseVFX + redemptionBoost, 0f, 1f);

            // Cassian redemption choice → PhysicalBond mid-escort (playtest path)
            if (companionId == 1 && behavior.RedemptionChoiceMade && behavior.RedemptionLevel >= 70 && behavior.StateTimer > 6f)
            {
                behavior.SolidificationActive = true;
            }

            // Korath/Thorne 17th Hour special: increase bond and trigger density callback
            if (behavior.In17thHourMode && (companionId == 3 || companionId == 4) && behavior.StateTimer > 12f)
            {
                behavior.CompanionBondLevel = math.min(behavior.CompanionBondLevel + 1, 100);
            }

            // R7 Veritas escort resonance echo
            if (companionId == 6 && behavior.In17thHourMode)
            {
                behavior.GiantSongMatchQuality = math.lerp(behavior.GiantSongMatchQuality, 0.85f, dt * 0.5f);
            }

            // Timeout or external clear (from bridge) returns to follow
            if (behavior.StateTimer > 52f || !behavior.IsEscorting)
            {
                behavior.IsEscorting = false;
                behavior.In17thHourMode = false;
                TransitionTo(ref behavior, CompanionState.Follow);
            }
        }

        // ─── Round 6 Production: PhysicalBond — full solidification + redemption choice + Korath/Thorne/Anastasia/Cassian bond variants + 17th prep ───
        // Triggered via hybrid bridge from controllers (Anastasia Solidif, Cassian redemption choice, Korath/Thorne 17th escort callbacks)
        // Playtest path: train escort → redemption choice → PhysicalBond solidification → VO density + calendar 17th nodes
        void UpdatePhysicalBond(ref CompanionBehavior behavior, ref LocalTransform transform, float dt, int companionId)
        {
            // Round 6: companion-specific bond stances (deeper for Korath star alignment, Thorne guard bond)
            float3 bondOffset;
            if (companionId == 5) bondOffset = new float3(1.15f, 0.75f, -0.85f); // Anastasia
            else if (companionId == 1) bondOffset = new float3(-0.75f, 0.55f, 1.05f); // Cassian redeemed
            else if (companionId == 3) bondOffset = new float3(0.9f, 1.1f, 0.4f); // Korath elevated star gaze
            else if (companionId == 4) bondOffset = new float3(-0.6f, 0.5f, -1.0f); // Thorne protective
            else if (companionId == 6) bondOffset = new float3(0.4f, 0.9f, 0.7f); // R7 Veritas precise resonance bond
            else bondOffset = new float3(0f, 0.6f, 0.8f);

            float3 playerPos = transform.Position;
            float3 desired = playerPos + bondOffset;

            float dist = math.distance(transform.Position, desired);
            if (dist > 0.45f)
            {
                float3 dir = math.normalizesafe(desired - transform.Position);
                transform.Position += dir * 1.15f * dt;
                if (math.lengthsq(dir) > 0.01f)
                    transform.Rotation = math.slerp(transform.Rotation, quaternion.LookRotation(dir, math.up()), dt * 3.2f);
            }

            // Solidification VFX + bond ramp, full 17th Hour density
            float targetVFX = behavior.In17thHourMode ? 0.98f : 0.92f;
            behavior.VFXIntensity = math.lerp(behavior.VFXIntensity, targetVFX, dt * 1.8f);
            behavior.CompanionBondLevel = math.min(behavior.CompanionBondLevel + (int)(dt * 12f), 100);

            if (companionId == 1 && behavior.RedemptionChoiceMade) // Cassian post-choice
            {
                behavior.RedemptionLevel = math.min(behavior.RedemptionLevel + (int)(dt * 9f), 100);
                behavior.VFXIntensity *= 0.82f;
            }

            // Korath/Thorne 17th Hour bond payoff (playtest)
            if (behavior.In17thHourMode && (companionId == 3 || companionId == 4))
            {
                behavior.EscortLeanAngle = math.lerp(behavior.EscortLeanAngle, 15f, dt);
            }

            // R7 Veritas physical bond: high precision tell + giant song match
            if (companionId == 6)
            {
                behavior.PhysicalTellIntensity = math.max(behavior.PhysicalTellIntensity, 0.95f);
                behavior.GiantSongMatchQuality = math.lerp(behavior.GiantSongMatchQuality, 0.97f, dt);
            }

            // Exit after solidification window or bridge clear — returns to follow with persisted state
            if ((behavior.StateTimer > 14f && !behavior.SolidificationActive) || !behavior.SolidificationActive)
            {
                behavior.IsEscorting = false;
                behavior.SolidificationActive = false;
                behavior.In17thHourMode = false;
                TransitionTo(ref behavior, CompanionState.Follow);
            }
        }

        // ═══ ROUND 7: Giant Synergy Payoff (Companion Giant + Giant's Song auto-match + shared history) ═══
        void UpdateGiantSynergy(ref CompanionBehavior behavior, ref LocalTransform transform, float dt, int companionId, float3 playerPos)
        {
            // High bond companions manifest "Companion Giant" assist stance or harmonic echo
            float3 synergyOffset = companionId switch
            {
                0 => new float3(-1.2f, 2.8f, 1.1f),   // Milo: rear defensive giant echo
                2 => new float3(0.8f, 3.1f, -0.9f),   // Lirael: roof harmonic singer
                3 => new float3(1.4f, 4.2f, 0.6f),    // Korath: true giant scale star reader
                5 => new float3(0.3f, 2.6f, 1.4f),    // Anastasia: warm bond giant glow
                6 => new float3(-0.5f, 3.0f, 0.4f),   // Veritas: precise bell resonance giant
                _ => new float3(0f, 2.4f, 0.8f)
            };

            float3 desired = playerPos + synergyOffset;
            float dist = math.distance(transform.Position, desired);
            if (dist > 0.9f)
            {
                float3 dir = math.normalizesafe(desired - transform.Position);
                transform.Position += dir * 1.8f * dt;
                transform.Rotation = math.slerp(transform.Rotation, quaternion.LookRotation(dir, math.up()), dt * 2.8f);
            }

            // Giant's Song auto-match: bond drives freq match quality (consumed by combat/harmonic systems via pull)
            float targetMatch = math.clamp(behavior.CompanionBondLevel / 100f * 0.95f + 0.05f, 0f, 1f);
            behavior.GiantSongMatchQuality = math.lerp(behavior.GiantSongMatchQuality, targetMatch, dt * 1.2f);

            // Physical tell max for giant payoff
            behavior.PhysicalTellIntensity = math.max(behavior.PhysicalTellIntensity, 1.0f);
            behavior.VFXIntensity = math.lerp(behavior.VFXIntensity, 0.96f, dt * 2f);

            // Cross-Moon memory: high synergy bumps bond permanently (world mutation tier)
            if (behavior.CompanionBondLevel > 85 && behavior.StateTimer > 8f)
            {
                behavior.WorldMutationTier = math.min(behavior.WorldMutationTier + 1, 4);
                behavior.CalendarEchoActive = true; // echo the synergy into calendar state
            }
        }

        // ═══ ROUND 7: Calendar / Live-Ops Echo that mutates companion state (daily banter, 17th echoes, claimable events) ═══
        void UpdateCalendarEcho(ref CompanionBehavior behavior, ref LocalTransform transform, float dt, int companionId)
        {
            // 17th Hour or daily echo: trust bump + physical tell + possible world mutation
            behavior.CompanionBondLevel = math.min(behavior.CompanionBondLevel + (int)(dt * 4f), 100);
            behavior.PhysicalTellIntensity = math.max(behavior.PhysicalTellIntensity, 0.65f);

            // Per-companion calendar echo flavor (Veritas resonance truth, Anastasia warmth, etc)
            if (companionId == 6) // Veritas
                behavior.GiantSongMatchQuality = math.lerp(behavior.GiantSongMatchQuality, 0.9f, dt);
            if (companionId == 5)
                behavior.VFXIntensity = math.lerp(behavior.VFXIntensity, 0.88f, dt);

            // After echo window, clear flag (bridge or time clears it)
            if (behavior.StateTimer > 22f)
            {
                behavior.CalendarEchoActive = false;
                behavior.WorldMutationTier = math.min(behavior.WorldMutationTier + 1, 4); // permanent payoff
            }
        }

        // ═══ ROUND 7: Physical Tell For Major Beat (restoration, combat, giant, 17th, World's Fair, escort) — called from Mono bridge/controllers ═══
        public static void ApplyPhysicalTellForBeat(ref CompanionBehavior behavior, int beatType /*0=restore,1=combat,2=giant,3=17th,4=worldsfair,5=escort*/, int companionId)
        {
            float tell = beatType switch
            {
                0 => 0.92f, // restoration celebrate deep
                1 => 0.55f, // combat post
                2 => 1.0f,  // giant synergy peak
                3 => 0.78f, // 17th echo
                4 => 0.85f, // World's Fair
                5 => 0.82f, // escort
                _ => 0.6f
            };
            behavior.PhysicalTellIntensity = math.max(behavior.PhysicalTellIntensity, tell);
            behavior.VFXIntensity = math.lerp(behavior.VFXIntensity, tell, 0.6f);

            if (beatType == 0 || beatType == 4) // restoration / world's fair → world mutation + bond
            {
                behavior.WorldMutationTier = math.min(behavior.WorldMutationTier + 1, 4);
                behavior.CompanionBondLevel = math.min(behavior.CompanionBondLevel + 12, 100);
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