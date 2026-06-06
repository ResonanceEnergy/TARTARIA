# GAMEPLAY SYSTEMS - QUICK ACTION CHECKLIST
**TARTARIA Vertical Slice Unblocking**  
**Priority: CRITICAL - Week 1 Sprint**

---

## ⚠️ BLOCKING ISSUES (Must fix for vertical slice)

### 🔴 CRITICAL - Do First (Day 1-2, ~16 hours)

- [ ] **Re-enable PlayerProgression.cs** (4 hours)
  ```powershell
  cd C:\dev\TARTARIA_new
  Move-Item "Assets\_Project\Scripts\Gameplay\PlayerProgression.cs.disabled" `
            "Assets\_Project\Scripts\Gameplay\PlayerProgression.cs"
  ```
  - Verify compilation (should be green)
  - Test: Add Debug XP via console, verify level-up
  - Wire OnLevelUp event to UI notification

- [ ] **Re-enable PlayerAbilityController.cs** (4 hours)
  ```powershell
  Move-Item "Assets\_Project\Scripts\Gameplay\PlayerAbilityController.cs.disabled" `
            "Assets\_Project\Scripts\Gameplay\PlayerAbilityController.cs"
  ```
  - Verify compilation
  - Test: Press Q/E/R keys, verify cooldown tracking
  - Wire to HUD for cooldown display

- [ ] **Create ItemDatabase.asset** (2 hours)
  - Unity menu: Assets → Create → Tartaria → Item Database
  - Save to: `Assets/_Project/Resources/ItemDatabase.asset`
  - Test: InventorySystem should no longer log error on startup

- [ ] **Create 20 ItemData assets** (6 hours)
  - Use template in report Section 2
  - Save to: `Assets/_Project/Resources/Items/`
  - Categories: Consumables (4), Materials (6), QuestItems (3), Currency (1), Equipment (6)
  - Test: Add items via console, verify inventory display

### 🟡 HIGH PRIORITY - Do Next (Day 3-4, ~14 hours)

- [ ] **Create 10 EquipmentItemData assets** (8 hours)
  - Unity menu: Assets → Create → Tartaria → Equipment Item
  - Save to: `Assets/_Project/Resources/Equipment/`
  - Template:
    ```
    Weapons (3): rusty_sword, iron_axe, resonance_staff
    Armor (2): leather_vest, iron_chestplate
    Helmets (2): leather_cap, iron_helm
    Gloves (1): leather_gloves
    Boots (1): leather_boots
    Accessories (1): resonance_amulet
    ```
  - Test: Equip via inspector, verify stat bonuses calculated

- [ ] **Wire Equipment stats to PlayerProgression** (6 hours)
  - Edit `PlayerProgression.cs` derived stat properties
  - Add: `+ EquipmentSlotManager.Instance?.GetEquipmentBonus(StatType.Strength) ?? 0`
  - For all 5 stats: Strength, Agility, Vitality, Resonance, Attunement
  - Test: Equip item, verify damage/HP increases

### 🟢 MEDIUM PRIORITY - Nice to Have (Day 5, ~6 hours)

- [ ] **Create 5 EnemyData assets** (4 hours)
  - Unity menu: Assets → Create → Tartaria → Enemy Data
  - Save to: `Assets/_Project/Resources/Enemies/`
  - Template:
    ```
    mud_golem.asset (base enemy)
    crystal_shardling.asset (fast melee)
    vein_crawler.asset (ranged)
    golem_bruiser.asset (elite)
    echo_phantom.asset (phase-through)
    ```
  - Populate loot tables with item IDs created earlier
  - Test: Spawn enemy via console, kill it, verify loot drops

- [ ] **Populate LootDropper tables** (2 hours)
  - Edit `LootDropper.cs` static Drop[] Table (Line 18-24)
  - Add 5 more drops (total 8 items in rotation)
  - Reference actual ItemData asset IDs
  - Test: Kill enemy, verify new items drop

---

## 📊 VERIFICATION TESTS

### Test 1: Progression Loop (15 min)
1. Play game, kill 5 enemies
2. Verify XP gained, level-up message shown
3. Open character sheet (create if missing)
4. Allocate 3 stat points
5. Verify derived stats update (HP, damage, etc.)

### Test 2: Combat Loop (10 min)
1. Equip weapon from inventory
2. Verify attack damage increases
3. Use ability (Q key)
4. Verify cooldown timer starts
5. Verify RS consumed (if wired)

### Test 3: Inventory Loop (10 min)
1. Kill enemy, loot drops
2. Pick up item (E key)
3. Open inventory (I key)
4. Verify item shown with icon
5. Use consumable, verify effect applies

### Test 4: Equipment Loop (10 min)
1. Open inventory
2. Equip weapon, armor, helmet
3. Open character sheet
4. Verify stat bonuses displayed
5. Unequip, verify bonuses removed

---

## 🚨 COMMON ISSUES & FIXES

### Issue: "ItemDatabase not found in Resources"
**Fix:** Create ItemDatabase.asset as first step (see Critical section)

### Issue: PlayerProgression.cs compilation errors after re-enable
**Fix:** File is complete, should compile green. Check for:
- Missing GameBalanceConfig.Instance
- ISaveDataProvider interface missing (shouldn't happen)

### Issue: Equipment stats not showing in UI
**Fix:** 
1. Create CharacterSheetUI.cs (see report Section 4)
2. Subscribe to EquipmentSlotManager.OnEquipmentChanged event
3. Refresh stat display on event

### Issue: Abilities consume no RS
**Fix:** 
1. Check PlayerAbilityManager.cs Line 123
2. Replace stub with: `RunProgressTracker.Instance?.ConsumeRS(ability.rsCost)`
3. Verify RunProgressTracker exists in scene

### Issue: Loot cubes don't disappear on pickup
**Fix:** Check PickupInteractable.cs OnInteract() method calls Destroy(gameObject)

---

## 📝 DATA ASSET TEMPLATES

### ItemData Template (Health Potion)
```
itemID: health_potion_small
displayName: Small Health Potion
description: Restores 50 HP
icon: [assign sprite]
stackSize: 99
category: Consumable
rarity: Common
weight: 0.1
value: 25
```

### EquipmentItemData Template (Iron Sword)
```
itemID: iron_sword
itemName: Iron Sword
slot: Weapon
icon: [assign sprite]
strengthBonus: 5
agilityBonus: 0
vitalityBonus: 0
resonanceBonus: 0
attunementBonus: 0
armorValue: 0
specialEffects: [empty]
description: A sturdy iron blade
```

### EnemyData Template (Mud Golem)
```
enemyID: mud_golem
displayName: Mud Golem
prefab: [assign golem prefab]
maxHealth: 300
moveSpeed: 4
attackDamage: 15
attackRange: 3
rsReward: 10
xpReward: 50
lootTable:
  - { itemID: "aether_shard", dropChance: 0.8, minQuantity: 1 }
  - { itemID: "golem_core", dropChance: 0.3, minQuantity: 1 }
```

---

## 🎯 SUCCESS CRITERIA

### Vertical Slice Ready When:
- ✅ Player can gain XP and level up
- ✅ Player can allocate stat points
- ✅ Player can equip items from inventory
- ✅ Equipment provides stat bonuses
- ✅ Player can use 3+ abilities with cooldowns
- ✅ Enemies drop items that go to inventory
- ✅ Items have icons, names, descriptions
- ✅ Combat damage scales with stats + equipment

**Timeline:** 30 hours work = 1 week sprint (assuming 6 hours/day)

**Deliverable:** Playable vertical slice for internal testing

---

## 📞 NEXT STEPS AFTER COMPLETION

1. **Sprint 2:** Create UI panels (character sheet, ability bar)
2. **Sprint 3:** Re-enable crafting system
3. **Sprint 4:** Balance tuning based on playtesting
4. **Sprint 5:** Integration assembly revival (Moon 2+ content)

**For detailed technical guidance, see:** `GAMEPLAY_SYSTEMS_ENGINEERING_REPORT.md`
