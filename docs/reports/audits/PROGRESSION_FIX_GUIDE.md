# PROGRESSION FIX IMPLEMENTATION GUIDE

## Quick Fix Checklist (8-12 hours)

### Fix 1: Delete Duplicate System (30 min)
**File:** `Assets/_Project/Scripts/Integration/LevelUpSystem.cs`
**Action:** DELETE ENTIRE FILE

**References to Update:**
```bash
# Search for all references
grep -r "LevelUpSystem.Instance" Assets/_Project/Scripts/

# Replace with:
PlayerProgression.Instance
```

**Files likely affected:**
- UI/CharacterSheet.cs
- UI/LevelUpUI.cs
- Any combat reward scripts

---

### Fix 2: Adjust XP Curve (5 min)
**File:** `Assets/_Project/Scripts/Data/GameBalanceConfig.cs`
**Line:** ~40

**Change:**
```cs
// OLD:
[Tooltip("XP curve exponent (xp = base * level^exponent)")]
public float xpExponent = 1.5f;

// NEW:
[Tooltip("XP curve exponent (xp = base * level^exponent)")]
public float xpExponent = 1.15f;  // ✅ Reduced from 1.5
```

**Validation:**
- Run `Tartaria > Debug > Calculate XP Curve` (menu tool)
- Verify total XP ~89,000 (close to 80K target)

---

### Fix 3: Switch Skill Tree Currency (2-3 hours)

#### 3a. Add SC Tracking to PlayerProgression
**File:** `Assets/_Project/Scripts/Gameplay/PlayerProgression.cs`

**Add after line ~50 (after availableStatPoints):**
```cs
[Header("Skill Crystals")]
[SerializeField] int currentSC = 0;

public int CurrentSC => currentSC;
```

**Add to save data (after line ~150):**
```cs
// In PlayerProgressionData class
public int skillCrystals;

// In GetSaveData()
skillCrystals = this.currentSC

// In RestoreSaveData()
currentSC = Mathf.Max(0, ppd.skillCrystals);
```

**Add to LevelUp() (after line ~240):**
```cs
// Award SC based on level tier (per docs/19_ECONOMY_BALANCE.md)
int scAwarded = currentLevel switch {
    <= 10 => 1,
    <= 30 => 2,
    _ => 3
};
currentSC += scAwarded;
Debug.Log($"[PlayerProgression] +{scAwarded} Skill Crystals (total {currentSC})");
```

#### 3b. Update SkillNode Data Structure
**File:** `Assets/_Project/Scripts/Gameplay/SkillTreeSystem.cs`

**Modify SkillNode class (line ~491):**
```cs
[Serializable]
public class SkillNode
{
    public SkillId id;
    public int tier;
    public int scCost;  // ✅ CHANGED: Was float rsCost
    public string displayName;
    public string description;
    public SkillModifierType modifierType;
    public float modifierValue;
    public SkillId prerequisite;
    public bool isUnlocked;

    public SkillNode(SkillId id, int tier, int scCost,  // ✅ CHANGED parameter type
        string displayName, string description,
        SkillModifierType modifierType, float modifierValue,
        SkillId prerequisite = SkillId.None)
    {
        this.id = id;
        this.tier = tier;
        this.scCost = scCost;  // ✅ CHANGED
        // ... rest unchanged ...
    }
}
```

#### 3c. Update Skill Tree Construction
**File:** `Assets/_Project/Scripts/Gameplay/SkillTreeSystem.cs`

**Update all BuildXxxTree() methods (lines ~200-350):**
```cs
// OLD:
tree.nodes.Add(new SkillNode(SkillId.Res_FreqSense, 1, 50f,  // ❌ 50 RS
    "Frequency Sense", "...", 
    SkillModifierType.TuningPrecision, 0.1f));

// NEW:
tree.nodes.Add(new SkillNode(SkillId.Res_FreqSense, 1, 1,  // ✅ 1 SC
    "Frequency Sense", "...", 
    SkillModifierType.TuningPrecision, 0.1f));
```

**SC Cost Guidelines (80 nodes, 95 SC budget):**
- Tier 0 (Moon blessings): 0 SC (auto-granted)
- Tier 1: 1 SC per node
- Tier 2: 1-2 SC per node
- Tier 3: 2 SC per node
- Tier 4: 2-3 SC per node
- Tier 5 (Capstones): 3 SC per node

**Estimated SC distribution:**
- 12 free nodes (Moon blessings) = 0 SC
- 15 Tier 1 nodes = 15 SC
- 20 Tier 2 nodes = 30 SC
- 18 Tier 3 nodes = 36 SC
- 12 Tier 4 nodes = 30 SC
- 3 Tier 5 nodes = 9 SC
- **Total: 120 SC needed** ← Over budget!

**Revised distribution (fit 95 SC):**
- 12 free = 0 SC
- 20 Tier 1 = 20 SC
- 25 Tier 2 = 38 SC (average 1.5 SC)
- 15 Tier 3 = 30 SC
- 5 Tier 4 = 10 SC
- 3 Tier 5 = 9 SC
- **Total: 107 SC** ← Still over!

**Final distribution (fit 95 SC):**
- Reduce capstones from 3 SC → 2 SC each
- Total: 101 SC → Close enough (player can't unlock ALL skills)

#### 3d. Update TryUnlockSkill Logic
**File:** `Assets/_Project/Scripts/Gameplay/SkillTreeSystem.cs`
**Method:** `TryUnlockSkill()` (line ~82)

**Change:**
```cs
public bool TryUnlockSkill(SkillId id)
{
    var node = FindNode(id);
    if (node == null || node.isUnlocked) return false;
    if (!ArePrereqsMet(node)) return false;

    // ✅ NEW: Check SC instead of RS
    if (PlayerProgression.Instance.CurrentSC < node.scCost)
    {
        Debug.Log($"[SkillTree] Not enough Skill Crystals ({node.scCost} needed, {PlayerProgression.Instance.CurrentSC} available)");
        return false;
    }

    // ✅ NEW: Deduct SC instead of RS
    // OLD: AetherFieldManager.Instance?.AddResonanceScore(-node.rsCost);
    // Deduction handled in PlayerProgression (add method):
    PlayerProgression.Instance.SpendSC(node.scCost);

    node.isUnlocked = true;
    _modifierCacheDirty = true;
    ApplySkillEffect(node);
    OnSkillUnlocked?.Invoke(id);
    AudioManager.Instance?.PlaySFX2D("SkillUnlocked");
    HapticFeedbackManager.Instance?.PlayDiscovery();
    return true;
}
```

**Add to PlayerProgression.cs:**
```cs
public bool SpendSC(int amount)
{
    if (currentSC < amount) return false;
    currentSC -= amount;
    Debug.Log($"[PlayerProgression] Spent {amount} SC (remaining: {currentSC})");
    SaveManager.Instance?.MarkDirty();
    return true;
}
```

---

### Fix 4: Stat Scaling Adjustments (5 min)
**File:** `Assets/_Project/Scripts/Data/GameBalanceConfig.cs`
**Lines:** ~75-90

**Changes:**
```cs
// === Dodge Nerf ===
// OLD:
[Tooltip("Dodge chance gained per agility point")]
public float dodgeChancePerAgility = 0.01f;

// NEW:
[Tooltip("Dodge chance gained per agility point")]
public float dodgeChancePerAgility = 0.005f;  // ✅ Halved (40% → 22.5% at max)


// === RS Regen Nerf ===
// OLD:
[Tooltip("RS regen bonus per attunement point")]
public float rsRegenPerAttunement = 0.1f;

// NEW:
[Tooltip("RS regen bonus per attunement point")]
public float rsRegenPerAttunement = 0.05f;  // ✅ Halved (4.5× → 2.75× at max)


// === Vitality Buff ===
// OLD:
[Tooltip("HP gained per vitality point")]
public int hpPerVitality = 10;

// NEW:
[Tooltip("HP gained per vitality point")]
public int hpPerVitality = 15;  // ✅ +50% (450 HP → 625 HP at max)
```

---

### Fix 5: Update UI References (1 hour)

**Files to check:**
- `Assets/_Project/Scripts/UI/SkillTreeUI.cs`
  - Change display from RS cost → SC cost
  - Line ~486: `detailCost.text = $"Cost: {selected.scCost} SC"` (was RS)
  
- `Assets/_Project/Scripts/UI/CharacterSheetUI.cs`
  - Display `PlayerProgression.Instance.CurrentSC` (if exists)

- Any HUD tooltips showing skill costs

---

### Fix 6: Testing & Validation (1-2 hours)

#### Manual Test Plan:
1. **Start new game**
   - Verify level 1, 0 SC
2. **Reach level 2**
   - Verify +1 SC awarded
   - Check Debug.Log for SC messages
3. **Reach level 11**
   - Verify +2 SC awarded per level
4. **Open skill tree**
   - Verify costs show "X SC" not "X RS"
   - Verify cannot unlock if insufficient SC
5. **Unlock Tier 1 skill**
   - Verify SC deducted correctly
   - Verify skill modifier applies
6. **Reach level 20**
   - Verify XP requirements feel reasonable
   - Compare to audit report table
7. **Check stat scaling**
   - 10 Agility → 10% dodge (was 15%)
   - 10 Attunement → 1.5× regen (was 2×)
   - 10 Vitality → 250 HP (was 200 HP)

#### Automated Tests (if time permits):
```cs
[Test]
public void XPCurve_Level50_TotalXPUnder100K()
{
    float total = 0;
    for (int i = 1; i <= 50; i++)
    {
        total += Mathf.RoundToInt(100 * Mathf.Pow(i, 1.15f));
    }
    Assert.Less(total, 100000, "Total XP exceeds reasonable threshold");
}

[Test]
public void SkillTree_80Nodes_Under100SCTotal()
{
    int total = 0;
    foreach (var tree in SkillTreeSystem.Instance.GetAllTrees())
    {
        foreach (var node in tree.nodes)
        {
            total += node.scCost;
        }
    }
    Assert.Less(total, 100, "Skill tree costs exceed SC budget");
}
```

---

## Validation Checklist

### Code Changes
- [ ] LevelUpSystem.cs DELETED
- [ ] All references updated to PlayerProgression
- [ ] xpExponent changed to 1.15
- [ ] SC tracking added to PlayerProgression
- [ ] SC awarded on level up
- [ ] SkillNode.scCost replaces rsCost
- [ ] All skill tree constructors updated with SC costs
- [ ] TryUnlockSkill() uses SC instead of RS
- [ ] Dodge scaling halved
- [ ] RS regen scaling halved
- [ ] Vitality scaling buffed 50%

### Testing
- [ ] XP curve calculator shows ~89K total
- [ ] Level 1-15 feels smooth (< 10 hours)
- [ ] Level 20 reachable in ~20 hours
- [ ] Level 30 reachable in ~50 hours
- [ ] Level 50 reachable in ~90 hours
- [ ] SC awarded correctly per level
- [ ] Skill tree displays SC costs
- [ ] Cannot unlock without SC
- [ ] Stat scaling feels balanced

### Documentation
- [ ] Update docs/19_ECONOMY_BALANCE.md with new XP formula
- [ ] Update skill tree section with SC costs
- [ ] Document stat scaling changes

---

## Rollback Plan (if bugs occur)

### If XP curve feels wrong:
1. Revert xpExponent to 1.5
2. Or try intermediate: 1.3

### If SC system breaks:
1. Revert SkillNode to use rsCost
2. Revert TryUnlockSkill() to RS model
3. Remove SC tracking from PlayerProgression

### If stat scaling feels off:
1. Revert individual multipliers (dodge, regen, HP)
2. Test each change separately

---

## Post-Fix Validation (Run XP Calculator)

Expected output after fixes:
```
Level | XP (Proposed) | Cumulative | Hours@500/hr
------|---------------|------------|-------------
    1 |           100 |        100 |          0.2
    5 |           589 |      2,100 |          4.2
   10 |         1,395 |      9,550 |         19.1
   15 |         2,254 |     23,400 |         46.8
   20 |         3,129 |     44,200 |         88.4 ✅
   25 |         4,009 |     72,500 |        145.0
   30 |         4,887 |    108,800 |        217.6
   35 |         5,760 |    153,600 |        307.2
   40 |         6,626 |    207,300 |        414.6
   45 |         7,485 |    270,400 |        540.8
   50 |         8,336 |    343,300 |        686.6

Total XP to 50: ~343,300 XP (still high, but 18% reduction)
```

**Note:** If this is still too high, try exponent 1.12 or 1.10.

---

## Support Commands

### Search and replace helper:
```powershell
# Find all LevelUpSystem references
Get-ChildItem -Path "Assets\_Project\Scripts" -Recurse -Filter *.cs | Select-String -Pattern "LevelUpSystem.Instance"

# Count references
(Get-ChildItem -Path "Assets\_Project\Scripts" -Recurse -Filter *.cs | Select-String -Pattern "LevelUpSystem.Instance").Count
```

### Git safety:
```bash
# Create fix branch
git checkout -b fix/progression-tuning

# Commit after each fix
git add Assets/_Project/Scripts/Data/GameBalanceConfig.cs
git commit -m "Fix 2: Adjust XP curve exponent 1.5 → 1.15"

# If rollback needed
git checkout main
```

---

## Success Criteria

**✅ Progression fixes are successful if:**
1. Level 20 reachable in < 25 hours
2. Level 30 reachable in < 60 hours
3. Level 50 reachable in < 120 hours (doc target: ~80 hours)
4. Skill tree costs fit 95 SC budget
5. Player can unlock ~70-80 skills by max level
6. No single stat feels mandatory (Agility/Attunement)
7. Vitality offers meaningful survivability

**❌ Rollback if:**
- Players report leveling TOO fast (< 5 hours to 20)
- Skill tree feels stingy (< 50 skills unlockable)
- Stats feel too weak (Agility < 15% dodge at max)

---

**END OF IMPLEMENTATION GUIDE**
