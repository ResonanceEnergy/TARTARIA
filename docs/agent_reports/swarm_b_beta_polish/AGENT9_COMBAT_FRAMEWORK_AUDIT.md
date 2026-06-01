# AGENT 9: COMBAT FRAMEWORK EXTENSIBILITY AUDIT
**Agent**: Combat Framework Validator (Dr. Vex Aurelian's Team)  
**Mission**: Assess combat system architecture for deep RPG extensibility  
**Date**: Session 2 (Data Architecture Sprint)  
**Status**: ⚠️ MODERATE — Solid foundation, critical gaps in extensibility

---

## EXECUTIVE SUMMARY

**Current State**: Combat system has excellent thematic design (frequency-based resonance combat) and strong variety (23 enemy types defined). However, **extensibility is compromised** by hardcoded abilities, missing status effect system, and non-centralized damage calculation.

**Verdict**: **6.5/10 Extensibility**
- ✅ Strengths: Frequency combat unique, enemy variety, DOTS/ECS hybrid, damage types defined
- ❌ Critical Gaps: No status effects, hardcoded abilities, resistances not implemented, no crit system
- ⚠️ Risk: Designers **cannot add 100 abilities** without major refactoring

**Recommendation**: P0 status effect system + data-driven ability architecture before content expansion.

---

## 1. COMBAT ARCHITECTURE AUDIT

### 1.1 Damage Calculation — FRAGMENTED ⚠️

**Location**: Split across 3 systems:
1. `PlayerCombat.cs` — Basic melee (MonoBehaviour)
2. `PlayerCombatController.cs` — Raycast-based attacks (MonoBehaviour)
3. `HarmonicCombatSystem.cs` — DOTS/ECS frequency combat

**Formula (PlayerCombat)**:
```csharp
float dmgMod = 1f + (SkillTreeSystem.GetModifier(PulseDamage) ?? 0f) * 0.5f;
int effectiveDamage = Mathf.RoundToInt(meleeDamage * dmgMod);
```

**Formula (HarmonicCombatSystem)**:
```csharp
// Base damage
finalDamage = dmg.Amount;

// Frequency matching bonus
switch (dmg.Type) {
    case ResonancePulse:
        if (freqDelta < PulseFreqTolerance)
            finalDamage *= PulseFreqMatchBonus; // 1.5x
        break;
    case HarmonicStrike:
        finalDamage *= StrikeBaseMultiplier; // 5x
        if (hsDelta < StrikeFreqTolerance)
            finalDamage *= StrikeTightMatchBonus; // 1.6x
        break;
}
combatant.ValueRW.Health -= finalDamage;
```

**Issues**:
- ❌ No centralized damage calculation function
- ❌ Armor values exist in `EnemyData.resistances` but **NOT applied** in damage formula
- ❌ No critical hit system
- ❌ No damage type resistances (physical/resonance/environmental defined but unused)
- ✅ Frequency-matching system is unique and well-designed

**Diagram**:
```
┌─────────────────────────────────────────────────────────┐
│ DAMAGE FLOW (Current)                                   │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  Input: Attack Action (Melee/Ability/Enemy)            │
│         ↓                                                │
│  [3 SEPARATE SYSTEMS]                                   │
│    • PlayerCombat.Swing() → Physics.OverlapSphere       │
│    • PlayerCombatController.TryAttack() → Raycast       │
│    • HarmonicCombatSystem.OnUpdate() → DamageEvent      │
│         ↓                                                │
│  Damage Calculation:                                     │
│    • Base damage × stat multiplier (PlayerProgression)  │
│    • Frequency match bonus (HarmonicCombatSystem only)  │
│    • ❌ NO armor reduction                              │
│    • ❌ NO critical hits                                │
│    • ❌ NO damage type resistances                      │
│         ↓                                                │
│  Apply Damage: Health -= finalDamage                    │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 1.2 Damage Types — DEFINED BUT UNDERUTILIZED ⚠️

**Defined** (`CombatComponents.cs`):
```csharp
public enum DamageType : byte {
    ResonancePulse  = 0,  // AOE pushback
    HarmonicStrike  = 1,  // High single-target
    GolemSlam       = 2,  // Enemy melee
    Environmental   = 3   // Bypasses armor
}
```

**Resistances** (`EnemyData.cs`):
```csharp
public struct DamageResistances {
    public float physical;      // % reduction
    public float resonance;     // % reduction
    public float environmental; // % reduction
}
```

**Issues**:
- ❌ Resistances exist in data but **NOT checked** during damage calculation
- ❌ No elemental damage types (fire/ice/lightning)
- ❌ Environmental damage "bypasses armor" but armor not implemented anyway
- ✅ DamageType enum is extensible

**Recommendation**: 
- P0: Apply `EnemyData.resistances` in damage formula
- P1: Add elemental types (fire/ice/lightning) to match frequency system

### 1.3 Status Effects — **MISSING** 🚨

**Gap Analysis**:
- ❌ **NO status effect system exists**
- ❌ No buff/debuff framework
- ❌ No DoT (damage over time)
- ❌ No CC (crowd control: stun, root, slow)
- ❌ No dispel mechanics

**Partial Implementations** (not generic):
- Shield: `PlayerAbilityController.ShieldActive` (boolean flag, not a buff)
- Stun: `MudGolem.StunTimer` (float on enemy component, not generic)
- HitStun: `HitStunTimer` component (DOTS, only prevents AI updates)
- Knockback: `KnockbackImpulse` component (DOTS, movement only)

**Impact**:
- Designers **cannot add**: Poison, Burn, Freeze, Slow, Root, Charm, Silence
- Deep RPG progression blocked (no buff stacking, duration management)
- Boss mechanics limited (no phase-triggered debuffs)

**Recommendation**: P0 — Build status effect system (see Section 3 for design).

### 1.4 Critical Hits — **MISSING** 🚨

**Gap Analysis**:
- ❌ No crit chance stat
- ❌ No crit damage multiplier
- ❌ No crit roll in damage calculation

**Existing Stats** (`PlayerProgression.cs`):
- Strength, Agility, Vitality, Resonance, Attunement
- **No** crit-related derived stats

**Recommendation**: P1 — Add crit system:
```csharp
// Proposed integration
public float CritChance => 0.05f + (agility * 0.005f);  // 5% + 0.5% per AGI
public float CritMultiplier => 1.5f + (GetModifier(CritDamage));
```

---

## 2. ABILITY SYSTEM ASSESSMENT

### 2.1 Current Abilities — HARDCODED ❌

**Implemented** (`PlayerAbilityController.cs`):
1. **Harmonic Strike (F)**: AOE damage (50 damage, 5m radius, 8s CD, 20 RS cost)
2. **Frequency Shield (Q)**: Damage mitigation (5s duration, 12s CD, 15 RS cost)
3. **Aether Vision (V)**: Highlight interactables (toggle, no cost)

**Melee**: Basic attack (20 base damage, 0.8s cooldown, raycast-based)

**Issues**:
- ❌ Abilities hardcoded in PlayerAbilityController.cs (not data-driven)
- ❌ Execution logic hardcoded in `TryHarmonicStrike()`, `TryFrequencyShield()`, `ToggleAetherVision()`
- ❌ No ScriptableObject-based ability definitions
- ❌ Cannot add new abilities without code changes

### 2.2 Ability Data — PARTIAL SOLUTION ⚠️

**PlayerAbilityManager** has `AbilityData` class:
```csharp
[System.Serializable]
public class AbilityData {
    public string abilityName;
    public AbilityType abilityType;  // Damage/Buff/Mobility/Utility
    public float cooldown;
    public int rsCost;
    public bool unlockedByDefault;
    public string castSFX;
    public string castVFX;
}
```

**Issues**:
- ⚠️ `AbilityType` is enum (Damage/Buff/Mobility/Utility) — **NOT extensible** for 100 abilities
- ❌ No damage values, radius, duration in data
- ❌ Execution logic still hardcoded in `ExecuteAbility()` switch statement
- ❌ Not ScriptableObject (inspector-unfriendly)

### 2.3 Extensibility — LIMITED 🚨

**Question**: Can designers add 100 abilities?

**Answer**: **NO** — Current architecture requires:
1. Add ability to `PlayerAbilityController` (hardcoded method)
2. Wire input event (hardcoded in `OnEnable()`)
3. Add case to `ExecuteAbility()` switch statement
4. Manually handle cooldowns, costs, targeting

**Ideal State**:
- ScriptableObject-based ability definitions
- Data-driven execution (effect chains, not switch statements)
- Runtime ability registration (mods, DLC)

### 2.4 Ability Targeting — BASIC ⚠️

**Current**:
- Self: Frequency Shield, Aether Vision
- AOE (fixed radius): Harmonic Strike (5m sphere)
- Forward raycast: Melee attack (2.5m, 45° cone)

**Missing**:
- ❌ Targeted (click enemy)
- ❌ Ground-targeted (click location)
- ❌ Directional (skillshot)
- ❌ Chaining (bounces between enemies)
- ❌ Ally-targeted (healing, buffs)

### 2.5 Ability Combos — **MISSING** 🚨

**Gap Analysis**:
- ❌ No combo system
- ❌ No ability chaining bonuses
- ❌ No sequence detection
- ✅ Frequency-matching is a form of "soft combo" (correct frequency = bonus damage)

**Recommendation**: P2 — Add combo system:
```csharp
// Example combo detection
if (lastAbilityUsed == HarmonicStrike && currentAbility == ResonancePulse) {
    ApplyComboBonus(1.25f);  // 25% damage bonus
}
```

---

## 3. STATUS EFFECT SYSTEM — **CRITICAL GAP** 🚨

### 3.1 Current State — NONE

**No status effect framework exists.** Individual systems have partial implementations:
- `PlayerAbilityController.ShieldActive` (boolean)
- `MudGolem.StunTimer` (float)
- `HitStunTimer` (DOTS component)
- `KnockbackImpulse` (DOTS component)

**None of these are generic/extensible.**

### 3.2 Required Features

A proper status effect system needs:

1. **Effect Types**:
   - DoT: Poison, Burn, Bleed
   - Buff: Strength, Speed, Damage Reduction
   - Debuff: Weakness, Slow, Vulnerability
   - CC: Stun, Root, Silence, Fear

2. **Stacking**:
   - None: Only 1 instance (e.g., Stun)
   - Refresh: Reset duration on reapply
   - Stack Count: Multiple instances (e.g., Poison stacks)
   - Stack Intensity: Increase effect strength

3. **Duration System**:
   - Fixed duration (5s)
   - Tick interval (DoT every 1s)
   - Permanent (until dispelled)

4. **Dispel**:
   - Remove all debuffs (cleanse ability)
   - Remove specific types (e.g., remove CC only)
   - Immunity windows

### 3.3 Proposed Architecture

**ScriptableObject Definition**:
```csharp
[CreateAssetMenu(fileName = "StatusEffect", menuName = "Tartaria/Status Effect")]
public class StatusEffectData : ScriptableObject {
    public string effectID;
    public StatusEffectType type;  // Buff/Debuff/DoT/CC
    public float duration;
    public float tickInterval;     // For DoT
    public int maxStacks;
    public bool isDispellable;
    public Sprite icon;
    
    // Effect parameters
    public float damagePerTick;    // DoT
    public float statModifier;     // Buff/Debuff
    public StatType affectedStat;  // Which stat to modify
}
```

**Runtime Component** (DOTS):
```csharp
public struct StatusEffect : IBufferElementData {
    public Entity Source;
    public int EffectID;           // Reference to StatusEffectData
    public float RemainingDuration;
    public float NextTickTime;
    public int StackCount;
}
```

**System**:
```csharp
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct StatusEffectSystem : ISystem {
    public void OnUpdate(ref SystemState state) {
        float dt = SystemAPI.Time.DeltaTime;
        
        foreach (var (combatant, effectBuffer) in 
            SystemAPI.Query<RefRW<HarmonicCombatant>, DynamicBuffer<StatusEffect>>()) {
            
            for (int i = effectBuffer.Length - 1; i >= 0; i--) {
                var effect = effectBuffer[i];
                effect.RemainingDuration -= dt;
                
                // Tick DoT
                if (Time.time >= effect.NextTickTime) {
                    ApplyDoTDamage(combatant, effect);
                    effect.NextTickTime = Time.time + effect.TickInterval;
                }
                
                // Remove expired
                if (effect.RemainingDuration <= 0f)
                    effectBuffer.RemoveAt(i);
            }
        }
    }
}
```

**Integration Points**:
- Apply effect: `ApplyStatusEffect(Entity target, StatusEffectData effect, Entity source)`
- Remove effect: `RemoveStatusEffect(Entity target, int effectID)`
- Check immunity: `HasStatusEffect(Entity target, StatusEffectType type)`
- Stat modifiers: Integrate with `PlayerProgression` derived stats

**Recommendation**: P0 — Implement this system before adding more abilities/enemies.

---

## 4. DAMAGE FORMULA ANALYSIS

### 4.1 Current Formula

**PlayerCombat** (melee):
```csharp
damage = baseDamage × (1 + skillModifier × 0.5)
```

**HarmonicCombatSystem** (frequency combat):
```csharp
// Step 1: Type-based multiplier
switch (damageType) {
    case ResonancePulse:
        damage = baseDamage × 1.0;
        if (frequencyMatch)
            damage × 1.5;
        break;
    case HarmonicStrike:
        damage = baseDamage × 5.0;
        if (frequencyMatch)
            damage × 1.6;
        break;
}

// Step 2: Subtract from health (no armor)
health -= damage;
```

**PlayerProgression** (stat modifiers):
```csharp
MeleeDamageMultiplier = 1 + (strength × 0.03);  // 3% per STR
MagicDamageMultiplier = 1 + (attunement × 0.03);  // 3% per ATT
```

### 4.2 Missing Calculations

**Not Implemented**:
- ❌ Armor reduction: `damage × (1 - armorPercent)`
- ❌ Critical hits: `if (roll < critChance) damage × critMultiplier`
- ❌ Damage type resistances: `damage × (1 - resistance[type])`
- ❌ Penetration: `effectiveArmor = max(0, armor - penetration)`
- ❌ Damage range: `Random.Range(minDamage, maxDamage)`

### 4.3 Extensibility — POOR ❌

**Question**: Can formula change per ability?

**Answer**: **PARTIALLY**
- Frequency-matching is ability-specific (ResonancePulse vs HarmonicStrike)
- Base multipliers hardcoded in `CombatBalance` constants
- Cannot add new formula types without code changes

**Ideal State**:
```csharp
// In AbilityData ScriptableObject
public DamageFormula damageFormula;  // Can be: Flat, Scaling, Frequency, Combo, etc.
public float[] formulaParameters;    // Custom per formula type
```

### 4.4 Moddability — NONE ❌

**Question**: Can mods add new formulas?

**Answer**: **NO**
- Damage calculation hardcoded in systems
- No plugin architecture for custom formulas
- Constants in `CombatBalance` are sealed

**Recommendation**: P2 — Add formula plugin system for modding.

---

## 5. ENEMY VARIETY ASSESSMENT

### 5.1 Enemy Types — EXCELLENT VARIETY ✅

**Defined** (`CombatComponents.cs`): **23 enemy types**

**Moon 1 (Echohaven)**:
- MudGolem: Slow melee, 300 HP, 15 damage

**Moon 2 (Crystalline Caverns)**:
- FractalWraith: Phases through matter, drains Aether
- MirrorWraith: Copies player's last 3 attacks (elite)
- CrystalShardling: Swarm enemy, shatters on frequency match
- VeinCrawler: Ambush predator, latches + drains
- ResonanceDisruptor: Scrambles frequency wheel
- WindveilPhantom: Wind-riding ranged wraith
- GravityPillar: Heavy tank, gravity wells

**Moon 3+**:
- RailWraith, DissonanceHarvester, DissonanceLeviathan (boss)
- SiegeGolem, HarmonicParasite, DissonantConductor
- CorruptedCraft, SkyReaver, ProphecyGuardian
- ResetSeeker, TemporalWraith, LivingSludge, SludgeLeviathan (boss)
- TitanGolem, FrequencyWraith

**Boss Mechanics**: 
- ✅ Multi-phase HP pools (DissonanceLeviathan, SludgeLeviathan)
- ✅ Unique mechanics (MirrorWraith attack copying, FractalWraith phasing)
- ✅ Environmental interactions (VeinCrawler on veins, WindveilPhantom in gusts)

### 5.2 Enemy Data — SCALABLE ✅

**EnemyData.cs** (ScriptableObject):
```csharp
- Identity: enemyID, displayName, description
- Stats: maxHealth, moveSpeed, attackDamage, attackRange, attackCooldown
- Behavior: archetype (Melee/Ranged/Tank/Swarm/Elite/Boss/Support/Caster)
- Resistances: physical/resonance/environmental
- Loot: rsReward, xpReward, lootTable
- Spawn: spawnMoons, minPlayerLevel
```

**Issues**:
- ✅ Designer-friendly ScriptableObject
- ✅ Comprehensive stats
- ⚠️ Resistances defined but not used in combat
- ❌ No ability definitions (special attacks defined as strings, not data)

### 5.3 AI Behaviors — HARDCODED PER TYPE ⚠️

**Implemented AI**:
- `EnemyAIController.cs` (generic state machine)
- `MudGolemAI.cs` (MonoBehaviour)
- `FractalWraithAISystem.cs` (DOTS)
- `MirrorWraithAISystem.cs` (DOTS)
- `Moon2CrystalEnemyAISystem.cs` (DOTS)
- `CrystalSentryAI.cs`, `ShadowStalkerAI.cs`, `TemporalWraithAI.cs`, etc.

**Issues**:
- ⚠️ Each enemy type requires custom AI script
- ❌ No behavior tree or data-driven AI
- ✅ State machines are consistent (Idle/Chase/Attack/Retreat)
- ❌ Cannot add new behaviors without code changes

**Recommendation**: P2 — Add behavior tree system for data-driven AI.

### 5.4 Boss Fight Framework — PARTIAL ⚠️

**Boss Components** (DOTS):
- `DissonanceLeviathan`: Multi-phase, serpentine, lullaby susceptibility
- `SludgeLeviathan`: Multi-stage, water pressure vulnerability

**Issues**:
- ⚠️ Boss mechanics hardcoded in components
- ❌ No generic phase system (each boss implements phases differently)
- ❌ No enrage mechanic
- ❌ No add spawning framework

**Recommendation**: P1 — Create BossHealthSystem with:
- Generic phase triggers (HP thresholds)
- Enrage timer (damage buff after X minutes)
- Add spawning (summon minions at phase transitions)
- Phase-specific abilities

---

## 6. GAPS & RECOMMENDATIONS

### 6.1 P0 (Critical — Blocks Content Expansion)

1. **Status Effect System** 🚨
   - Issue: Cannot add poison, stun, buff, debuff
   - Impact: Blocks deep RPG progression
   - Effort: 2-3 days
   - Design: See Section 3.3

2. **Damage Resistance Integration** 🚨
   - Issue: `EnemyData.resistances` exists but not used
   - Impact: All enemies same effective HP vs all damage types
   - Effort: 4 hours
   - Fix: Apply resistances in HarmonicCombatSystem damage calculation

3. **Data-Driven Ability System** 🚨
   - Issue: Cannot add abilities without code changes
   - Impact: Designers blocked from creating content
   - Effort: 3-4 days
   - Design: ScriptableObject + effect chain system (see 6.3)

### 6.2 P1 (High Priority — Quality of Life)

4. **Critical Hit System**
   - Issue: No crit chance/damage
   - Impact: Combat feels flat, no "big hit" moments
   - Effort: 1 day
   - Fix: Add `CritChance` and `CritMultiplier` to PlayerProgression, roll in damage calculation

5. **Centralized Damage Calculation**
   - Issue: 3 separate systems (PlayerCombat, PlayerCombatController, HarmonicCombatSystem)
   - Impact: Bug risk, inconsistent modifiers
   - Effort: 2 days
   - Fix: Create `DamageCalculator.Calculate(attacker, target, baseDamage, damageType)` utility

6. **Boss Fight Framework**
   - Issue: Each boss hardcoded, no generic phase system
   - Impact: Adding new bosses requires custom components
   - Effort: 2-3 days
   - Design: BossHealthSystem with phase triggers, enrage, add spawning

### 6.3 P2 (Polish — Post-MVP)

7. **Ability Combos**
   - Issue: No combo detection/bonuses
   - Impact: Missed opportunity for skill expression
   - Effort: 2 days
   - Design: Track last 3 abilities, detect sequences, apply bonuses

8. **Behavior Tree AI**
   - Issue: Each enemy needs custom AI script
   - Impact: Adding 100 enemies = 100 scripts
   - Effort: 1 week
   - Design: Visual behavior tree editor (Unity Behavior Tree package)

9. **Damage Formula Plugin System**
   - Issue: Cannot add new formula types
   - Impact: Modding limited
   - Effort: 3 days
   - Design: `IDamageFormula` interface + registry

### 6.4 Recommended Ability Architecture

**ScriptableObject Structure**:
```csharp
[CreateAssetMenu(fileName = "Ability", menuName = "Tartaria/Ability")]
public class AbilityData : ScriptableObject {
    // Identity
    public string abilityID;
    public string displayName;
    public Sprite icon;
    
    // Costs & Cooldown
    public int rsCost;
    public float cooldown;
    public int levelRequirement;
    
    // Targeting
    public TargetingType targeting;  // Self/Enemy/Ground/AOE/Cone
    public float range;
    public float radius;
    
    // Effects (composable chain)
    public AbilityEffect[] effects;
}

[System.Serializable]
public class AbilityEffect {
    public EffectType type;  // Damage/Heal/Buff/Debuff/Teleport/Spawn
    public float value;      // Damage amount, heal amount, etc.
    public StatusEffectData statusEffect;  // If type == Buff/Debuff
    public float delay;      // Execute after X seconds
}
```

**Execution System**:
```csharp
public class AbilityExecutor {
    public void Execute(AbilityData ability, Entity caster, Entity target) {
        foreach (var effect in ability.effects) {
            switch (effect.type) {
                case EffectType.Damage:
                    DealDamage(target, effect.value);
                    break;
                case EffectType.Buff:
                    ApplyStatusEffect(target, effect.statusEffect);
                    break;
                case EffectType.Teleport:
                    TeleportToTarget(caster, target, effect.value);
                    break;
                // ... extensible via new EffectType enum values
            }
        }
    }
}
```

**Benefits**:
- Designers can create 100+ abilities in inspector (no code)
- Effects are composable (damage + slow in 1 ability)
- Moddable (mods can add new AbilityData assets)

---

## 7. COMBAT ARCHITECTURE DIAGRAM

```
┌─────────────────────────────────────────────────────────────────────┐
│ TARTARIA COMBAT SYSTEM ARCHITECTURE (Current State)                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ INPUT LAYER                                                   │  │
│  │  • PlayerInputHandler (New Input System)                     │  │
│  │  • OnResonancePulse, OnHarmonicStrike, OnFrequencyShield    │  │
│  └──────────────────┬───────────────────────────────────────────┘  │
│                     │                                               │
│  ┌──────────────────▼───────────────────────────────────────────┐  │
│  │ ABILITY LAYER (FRAGMENTED ⚠️)                                │  │
│  │  • PlayerAbilityController (MonoBehaviour) — 3 hardcoded    │  │
│  │  • PlayerAbilityManager (partial data-driven)               │  │
│  │  • PlayerCombat (basic melee)                               │  │
│  │  • PlayerCombatController (raycast attacks)                 │  │
│  └──────────────────┬───────────────────────────────────────────┘  │
│                     │                                               │
│  ┌──────────────────▼───────────────────────────────────────────┐  │
│  │ DAMAGE CALCULATION (3 SEPARATE SYSTEMS ❌)                   │  │
│  │  • PlayerCombat: baseDamage × skillMod                      │  │
│  │  • PlayerCombatController: baseDamage × progMod             │  │
│  │  • HarmonicCombatSystem: baseDamage × freqMatch × typeMod   │  │
│  └──────────────────┬───────────────────────────────────────────┘  │
│                     │                                               │
│  ┌──────────────────▼───────────────────────────────────────────┐  │
│  │ STATUS EFFECTS (MISSING 🚨)                                  │  │
│  │  • No buff/debuff system                                     │  │
│  │  • No DoT (poison, burn)                                     │  │
│  │  • No CC (stun, root, slow)                                  │  │
│  │  • Partial: Shield (boolean), Stun (per-enemy timer)        │  │
│  └──────────────────┬───────────────────────────────────────────┘  │
│                     │                                               │
│  ┌──────────────────▼───────────────────────────────────────────┐  │
│  │ DAMAGE APPLICATION                                           │  │
│  │  • MudGolemHealth.TakeDamage() (MonoBehaviour)              │  │
│  │  • HarmonicCombatant.Health (DOTS)                          │  │
│  │  • ❌ No armor reduction                                     │  │
│  │  • ❌ No resistance checks (data exists, unused)            │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ ENEMY AI (HYBRID — DOTS + MonoBehaviour)                     │  │
│  │  • EnemyAIController (MonoBehaviour) — generic state machine│  │
│  │  • MudGolemAI (MonoBehaviour) — Echohaven enemy             │  │
│  │  • FractalWraithAISystem (DOTS) — Moon 2 phasing wraith    │  │
│  │  • MirrorWraithAISystem (DOTS) — Moon 2 attack mimic       │  │
│  │  • ⚠️ Each enemy type = custom script (not data-driven)    │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ STAT SYSTEM (PlayerProgression)                              │  │
│  │  • 5 stats: STR/AGI/VIT/RES/ATT                             │  │
│  │  • Derived: MaxHP, MaxRS, DamageMultipliers, DodgeChance    │  │
│  │  • ❌ No crit chance/damage                                 │  │
│  │  • ❌ No penetration/armor stats                            │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ DATA LAYER (ScriptableObjects — PARTIAL)                     │  │
│  │  • EnemyData ✅ (comprehensive, resistances unused)         │  │
│  │  • EquipmentItemData ✅ (armor value not integrated)        │  │
│  │  • SkillTreeAsset ✅ (modifiers applied)                    │  │
│  │  • ❌ NO AbilityData ScriptableObject                       │  │
│  │  • ❌ NO StatusEffectData ScriptableObject                  │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘

LEGEND:
✅ Well-implemented, extensible
⚠️ Partial implementation, needs work
❌ Missing, critical gap
```

---

## 8. FINAL VERDICT

### 8.1 Extensibility Score: **6.5/10**

**Breakdown**:
- Damage Types: 7/10 (defined, but resistances unused)
- Ability System: 4/10 (hardcoded, not data-driven)
- Status Effects: 0/10 (does not exist)
- Enemy Variety: 8/10 (23 types, but AI hardcoded per type)
- Boss Mechanics: 6/10 (multi-phase exists, but no framework)
- Damage Formula: 5/10 (frequency-matching unique, but no armor/crit)

### 8.2 Risks

**If Not Addressed**:
1. Adding 100 abilities = 100 code files (not scalable)
2. No deep status effect combos (poison + burn stacking)
3. All enemies feel similar (resistances not applied)
4. Combat feels flat (no crits, no armor breaks)
5. Boss fights hardcoded (adding 10 bosses = 10 custom components)

### 8.3 Priority Roadmap

**Week 1 (P0 — Unblock Content)**:
- Day 1-3: Status Effect System (ScriptableObject + DOTS component + System)
- Day 4: Integrate EnemyData.resistances into damage calculation
- Day 5: Centralize damage calculation (DamageCalculator utility)

**Week 2 (P1 — Quality)**:
- Day 1: Critical hit system (PlayerProgression + damage roll)
- Day 2-3: Data-driven ability system (AbilityData ScriptableObject + executor)
- Day 4-5: Boss fight framework (generic phase system)

**Week 3 (P2 — Polish)**:
- Day 1-2: Ability combo detection
- Day 3-5: Behavior tree foundation (Unity Behavior Tree package)

### 8.4 Comparison to Industry Standards

**ARPG Gold Standard** (Diablo, Path of Exile):
- ✅ Data-driven abilities (skill gems)
- ✅ Complex status effects (DoT, buff, debuff stacking)
- ✅ Deep damage formula (armor, resistances, penetration, crit)
- ✅ Boss mechanics (phases, enrage, telegraphed attacks)

**Tartaria Current State**:
- ❌ Abilities hardcoded
- ❌ No status effect system
- ⚠️ Partial damage formula (no armor/crit)
- ⚠️ Partial boss mechanics (phases exist, no framework)

**Gap**: **40% of industry standard**. P0 recommendations close this to **75%**.

---

## 9. CONCLUSION

**Summary**: Tartaria's combat system has a **strong thematic foundation** (frequency-based resonance combat) and **excellent enemy variety** (23 types). However, **extensibility is compromised** by:
1. No status effect system (critical blocker)
2. Hardcoded abilities (not data-driven)
3. Resistances/armor not applied (despite data existing)
4. No critical hit system

**Recommendation**: Prioritize P0 items (status effects, resistance integration, data-driven abilities) before adding more content. Current system can support ~10-20 abilities; reaching 100 requires architectural refactoring.

**Next Steps**: Agent 10 (Combat Balance Validator) should review balance tuning once P0 systems implemented.

---

**END AUDIT**
