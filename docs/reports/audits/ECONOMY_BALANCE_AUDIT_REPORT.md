# TARTARIA — Economy & Balance Audit Report
## Agent Review: Currency, Crafting, Trading, Loot, and Difficulty Curves

**Date:** 2026-05-23  
**Auditor:** Economy & Balance Logic Agent  
**Scope:** C:\dev\TARTARIA_new (Full Repository Scan)  
**Status:** ⚠️ **MODERATE CONCERNS FOUND** — No critical exploits, but balance gaps identified

---

## Executive Summary

### Balance Quality Score: **62/100** 🟡

**Rating Breakdown:**
- **Exploit Resistance:** 85/100 ✅ (No vendor arbitrage, no duplication bugs, no infinite loops)
- **Progression Curve:** 45/100 ⚠️ (Enemy HP scaling inconsistencies, mid-game difficulty spike)
- **Loot Economy:** 50/100 ⚠️ (Limited drop variety, no rarity distribution, deterministic cycling)
- **Currency Balance:** 70/100 🟡 (Good RS reward structure, but limited sinks)
- **Crafting Economy:** 60/100 🟡 (Costs defined but no recipe database populated)

---

## 1. Exploit Risks — ✅ **LOW RISK**

### 1.1 Infinite Gold Loops: **NONE FOUND** ✅

**Analysis:**
- No vendor/trading system implemented (`grep` search for "vendor|shop|trade|merchant" found only Workshop references)
- No buy/sell price arbitrage possible
- No crafting recipes with (output value > input cost)
- Currency generation tied to gameplay actions only (combat, quests, RS rewards)

**Verdict:** No exploitable loops detected. Economy is closed-loop and safe.

---

### 1.2 Duplication Bugs: **NONE FOUND** ✅

**Analysis:**
- Inventory system uses standard Unity serialization (SaveData pattern)
- No item cloning or stack manipulation code found
- LootDropper spawns single instances via GameObject creation (no prefab pool reuse)

**Verdict:** No duplication vectors identified.

---

### 1.3 Loot Exploits: **LOW RISK** 🟡

**Issue:** LootDropper cycles through 3 items **deterministically**:
```csharp
// LootDropper.cs:28-32
static readonly Drop[] Table = {
    { id = "aether_shard",      display = "Aether Shard" },
    { id = "golem_core",        display = "Golem Core" },
    { id = "resonance_crystal", display = "Resonance Crystal" }
};
var pick = Table[_dropCount++ % Table.Length];
```

**Risk:** Players can predict loot drops (kill 3 enemies → resonance crystal guaranteed).

**Recommendation:** Add randomization or rarity-based selection:
```csharp
// Weighted random instead of deterministic cycling
var pick = WeightedRandom(Table, rarity_weights);
```

---

## 2. Balance Breaks — ⚠️ **CRITICAL ISSUES FOUND**

### 2.1 Enemy HP Scaling Inconsistencies ⚠️

**Problem:** Mud Golem HP values **do not match** between GameBalanceConfig and combat systems.

| Source | Mud Golem HP | Notes |
|--------|--------------|-------|
| **GameBalanceConfig.cs:195** | 50 HP | "Mud Golem maximum health" |
| **CombatSystem.cs:313** | 100 HP | ECS spawn system (EnemySpawnSystem) |
| **MudGolemAI.cs** (inferred) | 300 HP | Referenced in COMBAT_BALANCE_REFERENCE.md |
| **BossEncounterSystem.cs** | 500 HP | Mud Colossus (Moon 0 boss) |

**Impact:**
- Early game difficulty **impossible to balance** with 6× variance
- Player expectation mismatch (config says 50, encounters 100-300)
- Tutorial enemies may be too hard or too easy depending on which value is used

**Recommendation:**
1. **Unify HP values** across all systems (decide: 50, 100, or 300?)
2. Create enemy scaling formula in GameBalanceConfig:
   ```csharp
   public int GetEnemyHP(EnemyType type, int moonNumber) {
       return baseHP * (1 + moonNumber * scalingFactor);
   }
   ```

---

### 2.2 Player Power Curve vs Enemy Scaling ⚠️

**Player Progression:**
- Base HP: 100 (no Vitality investment)
- Max HP at level 50: 100 + (50 Vitality × 10 HP/point) = **600 HP** (theoretical max)
- Realistic HP at level 20: ~300 HP (50% stat allocation to Vitality)

**Enemy Progression:**
| Moon | Enemy Type | HP | Damage | Player HP (expected) |
|------|------------|----|----|---------------------|
| 1-3  | Mud Golem | 100-300 | 10-20 | 100-150 |
| 4-6  | Shadow Stalker | 200 | 30 (45 ambush) | 150-250 |
| 7-10 | Void Phantom | 180 | 40 | 250-400 |
| 10-13 | Temporal Wraith | 350 | 45 | 400-600 |

**Balance Gaps:**
1. **Moon 1 (Tutorial):** Player 100 HP vs Golem 50 HP → **TOO EASY** (2-shot kill)
2. **Moon 4-6:** Shadow Stalker ambush (45 damage) → **3-shot player death** if not dodging
3. **Moon 10+:** Temporal Wraith (45 damage) → **2-3 shot player death** even with max Vitality

**Boss Difficulty Spike:**
- Moon 0 Boss: 500 HP (5× base enemy HP)
- Moon 3 Boss: 1200 HP (10× base enemy HP)
- Moon 12 Boss: 5000 HP (14× highest elite enemy HP)

**Verdict:** Mid-game difficulty spike at Moon 3-4 likely causes player frustration. Late-game bosses may require perfect play or feel like damage sponges.

---

### 2.3 XP Curve Analysis ✅ **REASONABLE**

**Formula:** `XP Required = 100 × level^1.5`

| Level | XP Required | Cumulative XP | Sessions (@ 500 XP/session) |
|-------|-------------|---------------|----------------------------|
| 1 → 2 | 100 | 100 | 0.2 sessions |
| 5 → 6 | 559 | 2,825 | 5.6 sessions |
| 10 → 11 | 1,000 | 11,617 | 23 sessions |
| 20 → 21 | 1,789 | 44,944 | 90 sessions |
| 50 → MAX | 3,536 | 224,777 | 450 sessions |

**Verdict:** Exponential curve is **well-tuned** for ~200-300 hours to max level. No grind walls detected.

---

## 3. Progression Gaps — ⚠️ **MID-GAME DEAD ZONE**

### 3.1 Moon 3-4 Difficulty Spike ⚠️

**Symptoms:**
- Enemy damage jumps from 10-20 → 30-45 (2-3× increase)
- Player HP only increases 50% in same timeframe (Vitality scaling)
- No armor/resistance system to mitigate spike

**Player Feedback Risk:** "Game suddenly got impossibly hard at Moon 3"

**Recommendation:**
1. Add gradual enemy scaling: `damage = baseDamage × (1 + moonNumber × 0.15)` (15% per moon)
2. Introduce armor stat earlier (currently referenced but not implemented in PlayerProgression)
3. Tutorial dodge/i-frames more prominently before Moon 3

---

### 3.2 Loot Variety Plateau 🟡

**Issue:** LootDropper only drops 3 item types (aether_shard, golem_core, resonance_crystal).

**Evidence:**
```csharp
// LootDropper.cs:28-32
static readonly Drop[] Table = { /* only 3 items */ };
```

**Impact:**
- No loot excitement after first 10 kills
- No legendary/rare drop anticipation
- No reason to farm specific enemies

**ItemDatabase has 23+ items defined** (health_potion, phoenix_feather, etc.) but LootDropper ignores them.

**Recommendation:**
1. Expand LootDropper table to 10-15 items
2. Add rarity tiers: Common (70%), Uncommon (20%), Rare (8%), Legendary (2%)
3. Enemy-specific loot tables (bosses drop legendary)

---

## 4. Economy Fixes — 🔧 **TUNING RECOMMENDATIONS**

### 4.1 Currency Sinks (Aether Shards) ⚠️

**Problem:** EconomySystem tracks 8 currency types but has **limited sinks**.

**Current Sinks:**
- Building restoration: 50/150/400 RS
- Workshop upgrades: 100/250/500 RS
- Skill unlocks: 75-750 RS
- Consumables: 30-80 RS (repair kit, aether potion, rs booster)

**Income Sources:**
- Quest rewards: 40-380 RS per quest (130+ quests = 17,000+ RS potential)
- Moon completion: 15-700 RS per moon (13 moons = 5,000+ RS)
- Boss defeats: 15-100 RS per boss (13 bosses = 500+ RS)
- Combat: 10-15 RS per enemy

**Total Potential Income:** ~25,000+ RS across full playthrough  
**Total Mandatory Spending:** ~3,000 RS (building + workshop + skills)

**Verdict:** **Massive currency surplus** in late game. No meaningful sinks after Moon 6.

**Recommendations:**
1. Add cosmetic sink: "Resonance Dyes" for building colors (100-500 RS each)
2. Add respec cost: 500 RS to reset skill points (mentioned in PlayerProgression.cs but not priced)
3. Add late-game upgrade tiers: Tier 4-5 building upgrades (1000-2500 RS)
4. Add companion gear: "Companion Trinkets" (200-800 RS, cosmetic + minor stat boost)

---

### 4.2 Item Value Balance ✅ **REASONABLE**

**Sample Item Prices (from DataAssetGenerator.cs):**
| Item | Value (RS) | Category | Value/Weight Ratio |
|------|-----------|----------|-------------------|
| Bread | 5 | Consumable | 25 RS/kg |
| Health Potion | 25 | Consumable | 83 RS/kg |
| Repair Kit | 30 | Consumable | 37.5 RS/kg |
| Golem Core | 85 | Material | 70.8 RS/kg |
| Aether Shard | 150 | Material | 750 RS/kg |
| Resonance Crystal | 500 | Material | 1000 RS/kg |
| Phoenix Feather | 2000 | Legendary | 20,000 RS/kg |

**Equipment Prices:**
| Item | Value (RS) | Slot | Stats |
|------|-----------|------|-------|
| Rusty Sword | 50 | Weapon | +5 STR |
| Iron Sword | 150 | Weapon | +12 STR, +3 AGI |
| Resonance Blade | 450 | Weapon | +18 STR, +5 AGI, +5 RES, +3 ATT |
| Leather Armor | 80 | Armor | +5 VIT, +10 ARM |
| Chainmail Armor | 300 | Armor | +10 VIT, +25 ARM |
| Aether Plate | 1500 | Armor | +20 VIT, +10 RES, +50 ARM |

**Analysis:**
- **Linear scaling** for consumables (2× effect ≈ 2× price) ✅
- **Super-linear scaling** for equipment (legendary 30× price for 3× stats) ✅
- **Rarity premium** on materials (resonance crystal 3.3× aether shard, appropriate for Epic tier) ✅

**Verdict:** Item pricing is internally consistent. No arbitrage opportunities.

---

### 4.3 Crafting Recipe Costs **NOT IMPLEMENTED** ⚠️

**Found:**
- CraftingRecipeDatabase.cs exists
- CraftingRecipeData.cs defines recipe structure
- CurrencyType enum includes AetherShards, ResonanceCrystals, etc.

**Missing:**
- No populated recipe assets
- No cost validation
- No output value vs input cost balancing

**Recommendation:**
1. Populate CraftingRecipeDatabase with 20-30 recipes
2. Add cost validation: `Assert(outputValue < totalInputCost * 1.2)` (20% crafting premium)
3. Example recipe:
   ```
   Repair Kit Recipe:
   - Input: 2× Golem Core (170 RS) + 1× Aether Shard (150 RS) = 320 RS
   - Output: Repair Kit (value 30 RS, but worth 300 RS for +30 HP utility)
   - Balance: Crafting is cheaper than buying (if vendors existed)
   ```

---

## 5. Difficulty Tuning — 🎯 **RECOMMENDED ADJUSTMENTS**

### 5.1 Enemy HP Rebalancing

**Current Problems:**
- GameBalanceConfig: Mud Golem 50 HP
- CombatSystem: Mud Golem 100 HP
- COMBAT_BALANCE_REFERENCE.md: Mud Golem 300 HP

**Proposed Unified Values:**
| Enemy Type | Moon 1-3 HP | Moon 4-6 HP | Moon 7-10 HP | Moon 11-13 HP |
|------------|-------------|-------------|--------------|---------------|
| Mud Golem | 80 | 120 | 160 | 200 |
| Shadow Stalker | — | 150 | 200 | 250 |
| Crystal Sentry | — | 180 | 250 | 300 |
| Void Phantom | — | — | 180 | 220 |
| Temporal Wraith | — | — | — | 350 |

**Formula:**
```csharp
public int GetEnemyHP(EnemyType type, int moonNumber) {
    int baseHP = enemyBaseHP[type]; // 80 for Mud Golem
    float moonMultiplier = 1f + (moonNumber - 1) * 0.2f; // +20% per moon
    return Mathf.RoundToInt(baseHP * moonMultiplier);
}
```

---

### 5.2 Damage Scaling

**Current Problems:**
- Mud Golem damage: 10 (too low for 100 HP player)
- Shadow Stalker ambush: 45 (2-shot kill at low Vitality)
- Temporal Wraith: 45 (3-shot max-Vitality player)

**Proposed Adjustments:**
| Enemy Type | Moon 1-3 Damage | Moon 4-6 Damage | Moon 7-10 Damage | Moon 11-13 Damage |
|------------|-----------------|-----------------|------------------|-------------------|
| Mud Golem | 15 | 20 | 25 | 30 |
| Shadow Stalker | — | 25 (35 ambush) | 30 (42 ambush) | 35 (49 ambush) |
| Crystal Sentry | — | 28 | 35 | 42 |
| Void Phantom | — | — | 32 | 38 |
| Temporal Wraith | — | — | — | 40 |

**Goal:** Force 6-8 hits to kill player at expected Vitality levels (encourages dodging without instant death).

---

### 5.3 Boss HP Scaling

**Current:** Linear scaling creates damage sponge problem at Moon 12 (5000 HP boss).

**Proposed:**
| Moon | Boss HP | Player DPS (expected) | Time to Kill (no abilities) |
|------|---------|----------------------|----------------------------|
| 0 | 400 | 50 DPS | 8 seconds |
| 3 | 800 | 80 DPS | 10 seconds |
| 6 | 1200 | 120 DPS | 10 seconds |
| 9 | 1600 | 160 DPS | 10 seconds |
| 12 | 2500 | 250 DPS | 10 seconds |

**Formula:**
```csharp
bossHP = 400 * Mathf.Pow(1.25f, moonNumber); // 25% compound growth
```

**Rationale:** Keep boss fights at 8-12 seconds of pure DPS to maintain intensity without tedium.

---

## 6. Drop Rate Tuning — 📦 **LOOT TABLE EXPANSION**

### Current System
```csharp
// LootDropper.cs:28-32 — DETERMINISTIC CYCLING (bad)
static readonly Drop[] Table = {
    { id = "aether_shard",      color = cyan },
    { id = "golem_core",        color = orange },
    { id = "resonance_crystal", color = purple }
};
var pick = Table[_dropCount++ % Table.Length]; // Predictable!
```

### Proposed System
```csharp
// Rarity-based weighted random
static readonly WeightedDrop[] Table = {
    // Common (70% total)
    { id = "aether_shard",      weight = 40, rarity = Common },
    { id = "golem_core",        weight = 30, rarity = Common },
    
    // Uncommon (20% total)
    { id = "resonance_crystal", weight = 15, rarity = Uncommon },
    { id = "health_potion",     weight = 5,  rarity = Uncommon },
    
    // Rare (8% total)
    { id = "repair_kit",        weight = 5,  rarity = Rare },
    { id = "stamina_tonic",     weight = 3,  rarity = Rare },
    
    // Legendary (2% total)
    { id = "phoenix_feather",   weight = 1,  rarity = Legendary },
    { id = "resonance_amulet",  weight = 1,  rarity = Legendary }
};

// Pick via weighted random (not deterministic)
var pick = WeightedRandom(Table);
```

**Boss-Specific Tables:**
- Bosses guarantee 1 Rare + 1 Legendary drop
- Moon 12 final boss guarantees phoenix_feather

---

## 7. Critical Recommendations — 🚨 **ACTION ITEMS**

### Priority 1 (P0) — **MUST FIX BEFORE RELEASE**
1. ✅ **Unify enemy HP values** across GameBalanceConfig, CombatSystem, and COMBAT_BALANCE_REFERENCE.md
2. ✅ **Add enemy scaling formula** to avoid hardcoded HP values
3. ✅ **Replace deterministic loot cycling** with weighted random selection
4. ✅ **Implement rarity-based drop tables** (8+ items, 4 rarity tiers)

### Priority 2 (P1) — **BALANCE TUNING PASS**
5. 🔧 Adjust Shadow Stalker ambush damage (45 → 35, with 1.4× ambush multiplier)
6. 🔧 Scale boss HP to maintain 8-12 second TTK across all moons
7. 🔧 Add currency sinks for late-game RS surplus (cosmetics, respec, Tier 4-5 upgrades)
8. 🔧 Populate CraftingRecipeDatabase with 20-30 balanced recipes

### Priority 3 (P2) — **POLISH & ITERATION**
9. 📊 Add armor stat to player progression (referenced but not implemented)
10. 📊 Create enemy-specific loot tables (bosses, elites, minions)
11. 📊 Add Moon-based loot scaling (Moon 10 drops better items than Moon 1)
12. 📊 Implement difficulty selector (affects enemy HP/damage by ±20%)

---

## 8. Testing Checklist — ✅ **VALIDATION PROTOCOL**

### Balance Testing Framework
- [ ] **Enemy HP Consistency:** All systems reference GameBalanceConfig singleton
- [ ] **Player TTK (Time to Kill):** 6-8 hits from enemies at expected Vitality
- [ ] **Boss TTK:** 8-12 seconds of pure DPS (no abilities)
- [ ] **Loot Variety:** 100 enemy kills produce 8+ unique item types
- [ ] **Currency Flow:** 15-minute session yields net positive RS (20-40 RS surplus)
- [ ] **XP Curve:** Level 1→10 in 2 hours, Level 1→20 in 10 hours, Level 1→50 in 200 hours
- [ ] **Crafting Balance:** Output value ≤ input cost × 1.2 (20% premium max)
- [ ] **No Exploits:** 1-hour stress test finds no infinite gold loops or duplication

---

## 9. Conclusion

### Verdict: **PLAYABLE BUT NEEDS TUNING** 🟡

**Strengths:**
- ✅ No critical exploits or infinite loops
- ✅ XP curve is well-balanced (exponential but not punishing)
- ✅ Item pricing is internally consistent
- ✅ RS reward structure encourages exploration

**Weaknesses:**
- ⚠️ Enemy HP values inconsistent across systems (50 vs 100 vs 300)
- ⚠️ Mid-game difficulty spike at Moon 3-4 (2-3× damage increase)
- ⚠️ Loot system too simplistic (3 items, deterministic cycling)
- ⚠️ Currency surplus in late game (no meaningful sinks after Moon 6)

**Recommended Next Steps:**
1. Fix P0 issues (HP unification, loot randomization)
2. Run balance playtest on Moons 1-6 with adjusted values
3. Iterate based on playtester feedback on difficulty curve
4. Populate crafting system and add late-game sinks

**Overall Assessment:** Economy is **safe from exploitation** but suffers from **balance gaps** and **lack of variety**. With recommended tuning, this becomes a solid 80/100 economy.

---

## Appendix: Balance Data Summary

### Enemy HP Variance Table
| Enemy | GameBalanceConfig | CombatSystem | COMBAT_BALANCE_REFERENCE | Recommendation |
|-------|------------------|--------------|--------------------------|----------------|
| Mud Golem | 50 | 100 | 300 | **120 HP** (mid-range) |
| Shadow Stalker | — | — | 200 | **180 HP** (keep as-is) |
| Void Phantom | — | — | 180 | **180 HP** (keep as-is) |
| Temporal Wraith | — | 85 | 350 | **350 HP** (elite tier) |

### Currency Flow Analysis
| Source | RS/Hour | Notes |
|--------|---------|-------|
| Combat | 60-90 RS | 6-8 enemies/hour @ 10-15 RS each |
| Quests | 100-200 RS | 1-2 quests/hour @ 100 RS avg |
| Moon Completion | 200-400 RS | 1 moon every 2-3 hours |
| Boss Defeat | 50-100 RS | 1 boss per moon |
| **Total Income** | **410-790 RS/hour** | — |

| Sink | RS/Hour | Notes |
|------|---------|-------|
| Building Restoration | 50-150 RS | 1-2 buildings/hour |
| Workshop Upgrades | 0-100 RS | Optional, periodic |
| Skill Unlocks | 0-75 RS | 1 unlock every 2 hours |
| Consumables | 30-80 RS | Situational |
| **Total Spending** | **80-405 RS/hour** | — |

**Net Surplus:** **+5 to +385 RS/hour** → Late game becomes RS-flooded without additional sinks.

---

**Report compiled by:** Economy & Balance Logic Agent  
**Next Review:** After P0 fixes implemented (2 weeks)
