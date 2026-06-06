# TARTARIA — Combat Balance & Enemy Design Reference

**Last Updated:** Session 6 — Combat Balance Pass  
**Status:** ✅ CS:0, Enemy Variety Implemented, Boss System Verified

---

## Combat Philosophy

- **Non-Lethal Retuning:** Enemies dissolve into purified Aether when harmonically corrected
- **Frequency-Based Damage:** Matching enemy resonance frequencies amplifies damage
- **Spatial Awareness Rewards:** Dodging, positioning, and threat prioritization are key
- **Progressive Difficulty:** Moon 1 teaches basics, Moon 13 demands mastery

---

## Player Combat Stats

| Stat | Value | Notes |
|------|-------|-------|
| **Max Health** | 100 HP | |
| **Melee Damage** | 25 base | +50% from PulseDamage skill modifier |
| **Attack Range** | 2.6m | Sphere overlap |
| **Attack Cooldown** | 0.45s | ~2.2 attacks/second |
| **Regen Rate** | 5 HP/s | After 5s delay from last damage |
| **Dodge i-frames** | 0.5s | Invulnerable during dodge roll |

**Damage Formula:**  
```
effectiveDamage = baseDamage * (1 + skillMod * 0.5)
```

---

## Enemy Roster by Moon

### Moon 1-3: Tutorial & Foundation
**Enemy Types:** Mud Golem (basic melee)  
**Encounter Density:** Low (2-3 per area)  
**Design Goal:** Teach combat basics, timing, dodging

| Enemy | HP | Damage | Range | Speed | Behavior |
|-------|----|----|-------|-------|----------|
| **Mud Golem** | 300 | 10 | 3m | 3 m/s | Patrol → Chase → Melee |

### Moon 4-6: Variety & Mechanics
**Enemy Types:** Mud Golem, Shadow Stalker  
**Encounter Density:** Medium (3-4 per area)  
**Design Goal:** Introduce stealth threats, punish tunnel vision

| Enemy | HP | Damage | Range | Speed | Behavior |
|-------|----|----|-------|-------|----------|
| **Mud Golem** | 300 | 10 | 3m | 3 m/s | Patrol → Chase → Melee |
| **Shadow Stalker** | 200 | 30 | 2.2m | 5-7 m/s | Stealth → Ambush (1.5x dmg) → Revealed |

**Shadow Stalker Notes:**
- Invisible beyond 8m from player
- Ambush attack deals 45 damage (30 * 1.5)
- Forced reveal for 2s after attacking
- High threat, teaches spatial awareness

### Moon 5-8: Mixed Engagements
**Enemy Types:** Mud Golem, Shadow Stalker, Crystal Sentry  
**Encounter Density:** Medium-High (4-5 per area)  
**Design Goal:** Ranged threats, priority targeting, vulnerability windows

| Enemy | HP | Damage | Range | Speed | Behavior |
|-------|----|----|-------|-------|----------|
| **Mud Golem** | 300 | 10 | 3m | 3 m/s | Patrol → Chase → Melee |
| **Shadow Stalker** | 200 | 30 | 2.2m | 5-7 m/s | Stealth → Ambush → Revealed |
| **Crystal Sentry** | 250 | 35 | 20m | 0 m/s | Telegraph (0.8s) → Shoot → Reload (2s vuln) |

**Crystal Sentry Notes:**
- Stationary turret, high priority target
- Takes 2x damage during reload window
- Projectiles travel at 15 m/s, dodgeable
- Forces player to close distance under fire

### Moon 7-10: Elite Encounters
**Enemy Types:** Shadow Stalker, Crystal Sentry, Void Phantom  
**Encounter Density:** High (5-6 per area, mixed)  
**Design Goal:** Teleporting unpredictability, punish predictable patterns

| Enemy | HP | Damage | Range | Speed | Behavior |
|-------|----|----|-------|-------|----------|
| **Shadow Stalker** | 200 | 30 | 2.2m | 5-7 m/s | Stealth → Ambush → Revealed |
| **Crystal Sentry** | 250 | 35 | 20m | 0 m/s | Telegraph → Shoot → Reload |
| **Void Phantom** | 180 | 40 | 2.5m | Instant | Teleport → Melee → Phase Out (1s invuln) |

**Void Phantom Notes:**
- Teleports around player every 3s
- Phases out (invulnerable) when hit, then teleports away
- Low HP but hard to pin down
- Tests reaction time and prediction

### Moon 8-11: Combined Arms
**Enemy Types:** Crystal Sentry, Void Phantom, Resonance Drone  
**Encounter Density:** High (6-7 per area)  
**Design Goal:** Support mechanics, force multiplier threats

| Enemy | HP | Damage | Range | Speed | Behavior |
|-------|----|----|-------|-------|----------|
| **Crystal Sentry** | 250 | 35 | 20m | 0 m/s | Telegraph → Shoot → Reload |
| **Void Phantom** | 180 | 40 | 2.5m | Instant | Teleport → Melee → Phase Out |
| **Resonance Drone** | 150 | 15 DoT | 15m | 4 m/s | Orbit → Beam Player + Buff Allies (30%) |

**Resonance Drone Notes:**
- Flying enemy, hard to hit
- Buffs all enemies within 10m (+30% damage)
- Priority target — kill first or suffer multiplied damage
- Continuous dissonance beam (15 damage/sec)

### Moon 10-13: Endgame Gauntlet
**Enemy Types:** Void Phantom, Resonance Drone, Temporal Wraith  
**Encounter Density:** Very High (7-8 per area)  
**Design Goal:** All mechanics mastered, elite enemies, time pressure

| Enemy | HP | Damage | Range | Speed | Behavior |
|-------|----|----|-------|-------|----------|
| **Void Phantom** | 180 | 40 | 2.5m | Instant | Teleport → Melee → Phase Out |
| **Resonance Drone** | 150 | 15 DoT | 15m | 4 m/s | Orbit → Beam + Buff |
| **Temporal Wraith** | 350 | 45 | 3m | 4.5-8 m/s | Phase → Melee + Time Slow Aura + Clone Spawn |

**Temporal Wraith Notes:**
- Elite enemy, mini-boss tier
- Time slow aura (12m radius, 50% slow)
- Rewinds health once at 30% HP (restores 40% max HP)
- Spawns temporal clones (50% stats, 10s lifetime) every 15s
- Very high threat, requires focus fire

---

## Boss Encounters

**Philosophy:** Each boss teaches frequency puzzle mastery while world reacts dynamically.

### Boss Scaling by Moon

| Moon | Boss Name | HP | RS Reward | Par Time | Phases |
|------|-----------|----|----|----------|--------|
| 0 | Mud Colossus | 500 | 15 | 60s | 2 |
| 1 | Quartz Defiler | 700 | 20 | 75s | 2 |
| 2 | Spire Breaker | 900 | 22 | 80s | 2 |
| 3 | Iron Corruptor | 1200 | 28 | 90s | 2 |
| 4 | Echo Sovereign | 1000 | 25 | 90s | 2 |
| 5 | Crystal Phantom | 1300 | 30 | 100s | 2 |
| 6 | Fractal Tyrant | 1500 | 32 | 110s | 2 |
| 7 | Mirror Empress | 1800 | 35 | 120s | 2 |
| 8 | Void Shaper | 1600 | 30 | 120s | 2 |
| 9 | Rift Walker | 2000 | 35 | 130s | 2 |
| 10 | Ley Devourer | 2200 | 38 | 140s | 2 |
| 11 | Anti-Resonance | 2500 | 42 | 150s | 2 |
| 12 | Guardian of True History | 5000 | 100 | 180s | 4 |

### Boss Types

**Corruption Titan** (Moons 0-3):
- Earth-based attacks: Sweep, Slam, Corruption Wave
- Teaches basic phase mechanics, vulnerability windows

**Mirror Sovereign** (Moons 4-7):
- Mirror Clone spam, Frequency Jam, Crystal Barrage
- Teaches multi-target prioritization, frequency precision

**Void Architect** (Moons 8-11):
- Void Rifts, Ley Line Sever, reality warping
- Teaches spatial awareness, environmental hazards

**True History Guardian** (Moon 12):
- 4 phases: Burial, Demolition, Erasure, Truth
- Final exam — all mechanics, lore climax

### Boss Mechanics

**Vulnerability Windows:**
- Bosses alternate between invulnerable and vulnerable states
- Vulnerable duration: 1.5-3s (decreases per phase)
- Invulnerable duration: 4-5s (increases per phase)

**Frequency Puzzles:**
- Submit frequency during vulnerable window for bonus damage
- Match quality: 0-100% (within 55 Hz tolerance)
- Perfect match (>85%): Golden Cascade (2x damage + VFX payoff)
- Boss-specific puzzle mechanics:
  - **RailWraith:** Clear swarm with frequency matches
  - **Leviathan:** Build resonance synergy (lullaby protection)
  - **SkyReaver:** Force altitude dive with precise frequency
  - **ResetSeeker:** Disrupt seeking patterns

**Phase Transitions:**
- HP thresholds trigger new phases (typically 60%, 30%)
- Phase transitions include brief invulnerability (1.8s)
- New attack patterns, faster attacks, environmental changes

**Desperation Mode:**
- Activates below 32% HP
- Attack speed +50%
- Requires "perfect frequency solves" to maintain damage

---

## Difficulty Curve Analysis

### Time to Kill (TTK)

**Assumptions:**
- Player deals 25 damage per hit (0.45s cooldown)
- ~55 DPS without frequency bonuses
- Frequency match bonus: +50% average

| Enemy | TTK (Normal) | TTK (Freq Boosted) |
|-------|--------------|---------------------|
| Mud Golem | 5.5s | 3.6s |
| Shadow Stalker | 3.6s | 2.4s |
| Crystal Sentry | 4.5s (9s vuln) | 3s (6s vuln) |
| Void Phantom | 3.3s (+ phase delays) | 2.2s |
| Resonance Drone | 2.7s | 1.8s |
| Temporal Wraith | 6.4s (+ rewind) | 4.2s |

### Player Survival Time

**Against Single Enemy:**
| Enemy | Player HP / Enemy DPS | Survival Time |
|-------|----------------------|---------------|
| Mud Golem | 100 / 10 | 10s (with regen) |
| Shadow Stalker | 100 / 30 | 3.3s (ambush 2.2s) |
| Crystal Sentry | 100 / 35 | 2.9s (dodgeable) |
| Void Phantom | 100 / 40 | 2.5s |
| Resonance Drone | 100 / 15 | 6.7s |
| Temporal Wraith | 100 / 45 | 2.2s (+ slow) |

**Against Mixed Groups (Moon 10 example):**
- 2 Void Phantoms + 1 Resonance Drone + 1 Temporal Wraith
- Combined DPS: 40 + 40 + 15 + 45 = 140 DPS (+ 30% drone buff = 182 DPS)
- Player survival: **<1 second** without dodging/kiting
- **Teaches:** Prioritize Drone first (remove buff), kite Wraith, dodge Phantoms

---

## Combat Polish Checklist

### ✅ Implemented
- [x] Player melee combat with sphere overlap
- [x] Enemy health/damage systems
- [x] Enemy AI states (Patrol, Chase, Attack)
- [x] Boss phase system with frequency puzzles
- [x] Dodge roll i-frames
- [x] Hit-stop on confirmed hits
- [x] Damage numbers (DamageNumberPool)
- [x] Camera punch on hit (Cinemachine Impulse)
- [x] Visual damage flash (color shift)
- [x] Death animations (fade + destroy)
- [x] Loot drops on enemy death
- [x] VFX on attacks (Spark, Cascade, Vortex)
- [x] Audio feedback (CombatHit, EnemyDeath, Tones)
- [x] Boss telegraphs (visual pulses synced to Hz)
- [x] Boss vulnerability windows
- [x] Golden Cascade payoffs (perfect frequency matches)

### 🔄 Recommended Enhancements
- [ ] Enemy flinch/stagger on hit (interrupt attacks)
- [ ] Enemy attack telegraph animations (wind-up before swing)
- [ ] Directional damage indicators (red arrow on HUD)
- [ ] Combo counter (hit streak rewards)
- [ ] Parry system (perfect dodge → counterattack)
- [ ] Enemy knockback on heavy hits
- [ ] Blood/particle effects on damage
- [ ] Slow-motion on killing blow
- [ ] Dynamic music intensity (combat layers)
- [ ] Haptic feedback tuning (stronger on ambush/crit)

---

## Balance Testing Notes

### Player Power Curve
- **Early Game (Moon 1-3):** 1v1 Mud Golems comfortable, 1v2 challenging
- **Mid Game (Moon 5-7):** Must prioritize Sentries, dodge Stalker ambushes
- **Late Game (Moon 10-13):** Drone must die first, Wraith requires kiting

### Boss Difficulty
- **Par Time:** Designed for skilled players with frequency mastery
- **Casual Clear:** 2-3x par time acceptable (extra hits allowed)
- **No-Hit Clear:** Requires perfect dodges + frequency solves

### Frequency System Integration
- **Base Damage:** 25 per hit (predictable, learnable)
- **Freq Bonus:** +50% average (rewards mastery)
- **Boss Vuln:** Only way to damage bosses (teaches mechanic)
- **Golden Cascade:** Feels amazing (VFX + audio + 2x damage)

---

## Spawn Density Recommendations

| Moon | Regular Enemies | Mini-Boss | Boss | Total Combat Time |
|------|----------------|-----------|------|-------------------|
| 1-3 | 10-15 | 0 | 1 | 5-8 min |
| 4-6 | 15-20 | 1 | 1 | 8-12 min |
| 7-9 | 20-25 | 2 | 1 | 12-15 min |
| 10-13 | 25-30 | 3 | 1 | 15-20 min |

**Mini-Boss Definition:** Elite enemies with 2x HP/Damage (e.g., Temporal Wraith as mini-boss in Moon 9, then regular enemy in Moon 11).

---

## Conclusion

**Combat is satisfying when:**
1. ✅ Player feels powerful but not invincible (25 dmg vs 100-350 HP enemies)
2. ✅ Enemies have clear weaknesses (Sentry reload, Phantom phase, Drone priority)
3. ✅ Frequency system rewards mastery (Golden Cascades feel AMAZING)
4. ✅ Difficulty scales smoothly (Moon 1 forgiving, Moon 13 brutal)
5. ✅ Boss fights are memorable (4-phase Guardian, Wraith swarms, Leviathan lullaby)

**Next Steps:**
- Playtest each Moon's combat density
- Tune enemy spawn positions for fair encounters
- Verify boss par times are achievable
- Polish VFX/SFX for feedback clarity
- Add flinch/stagger for tactical depth
