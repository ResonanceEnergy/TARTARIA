# AGENT 2: ItemDatabase Creation — QUICK START

## ⚡ 5-MINUTE EXECUTION GUIDE

Unity Editor is currently open. Follow these steps to complete asset creation:

---

### STEP 1: Execute Menu Command (1 minute)

**In Unity Editor Menu Bar:**
```
Tartaria > Build Assets > Item Database (Complete)
```

**What Happens:**
- Creates 30+ new item assets
- Skips 10 existing assets
- Shows dialog: "Created: X, Skipped: Y, Total: Z"

**Expected Output:**
```
[ItemDatabasePopulator] Starting item creation...
[ItemDatabasePopulator] Created: health_potion_minor
[ItemDatabasePopulator] Created: health_potion_greater
[ItemDatabasePopulator] Created: mana_crystal_minor
...
[ItemDatabasePopulator] Skipped existing: health_potion
[ItemDatabasePopulator] Skipped existing: repair_kit
...
[ItemDatabasePopulator] ✅ COMPLETE — 30 created, 10 skipped, 40 total
```

**Dialog Box:**
```
Item Database Complete
Created: 30
Skipped (already exist): 10
Total: 40

[OK]
```

---

### STEP 2: Auto-Populate Database (30 seconds)

**In Unity Project Window:**
1. Navigate to: `Assets/_Project/Resources/`
2. **Select:** `ItemDatabase.asset`

**In Unity Inspector:**
3. Scroll to "Database Tools" section
4. **Click:** `Auto-Populate from Assets` button

**Expected Output in Console:**
```
[ItemDatabase] Auto-populated 30 new items
[ItemDatabase] Built lookup with 40 items
```

---

### STEP 3: Verify Creation (1 minute)

**Check Assets Created:**
- In Project window: `Assets/_Project/Resources/Items/`
- Count files: Should see ~30 .asset files
- New files include: health_potion_minor, mana_crystal, crystal_berry, etc.

**Check Database Populated:**
- Select `ItemDatabase.asset` in Inspector
- Expand "Items" list
- Count: Should show 40+ items

**Check Console:**
- No red errors
- Green success messages
- Item count matches expected (40+)

---

### STEP 4: Optional — Test In-Game (3 minutes)

**Open Test Scene:**
- File > Open Scene: `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity`

**Enter Play Mode:**
- Press Play button (or F5)
- Kill an enemy
- **Verify:** Loot drops with glowing cube
- Walk to loot, press E to pick up
- **Verify:** Item appears in inventory UI
- Press Esc to exit Play mode

---

## ✅ SUCCESS CRITERIA

Mission complete when you see:
- [ ] Dialog: "Created: 30, Skipped: 10, Total: 40"
- [ ] Console: `[ItemDatabase] Built lookup with 40 items`
- [ ] Project: 30+ new assets in Items/ and Equipment/ folders
- [ ] Inspector: ItemDatabase shows 40+ items in list

---

## ⚠️ TROUBLESHOOTING

### Problem: Menu item not visible
**Solution:** Wait 10 seconds for Unity to compile scripts, then check again

### Problem: "Script compilation errors"
**Solution:** 
1. Check Console for red errors
2. Most likely: Test files missing references (safe to ignore)
3. If blocking: Window > General > Console, clear errors
4. Try menu command again

### Problem: "No new items found" in auto-populate
**Solution:**
1. Check if items already in database (OK if duplicates)
2. Or: Delete ItemDatabase.asset, recreate, try again

### Problem: "Null reference" when testing in-game
**Solution:**
1. Verify ItemDatabase.asset is in Resources/ folder (not Data/)
2. Check LootDropper.cs uses valid item IDs from database
3. Update LootDropper.Table with actual item IDs created

---

## 📋 ITEM CHECKLIST (40 total)

### Consumables (20)
- [ ] health_potion_minor, health_potion_greater
- [ ] mana_crystal_minor, mana_crystal, mana_crystal_greater
- [ ] resonance_boost_scroll, defenders_tonic, swiftness_elixir, lucky_charm
- [ ] crystal_berry, smoked_fungus, travelers_ration, echohaven_bread, resonant_stew
- [ ] teleport_crystal, lockpick, resonance_detector, camp_beacon

### Equipment (10)
- [ ] iron_blade, resonant_staff
- [ ] travelers_vest, iron_chestplate, crystal_robes, echohaven_armor
- [ ] bronze_ring, berserker_belt

### Materials (10)
- [ ] iron_ore, leather_scrap, wood_plank, resonance_dust
- [ ] echohaven_stone, ancient_alloy, pure_crystal, resonant_core, temporal_fragment

### Existing (10 — should skip)
- health_potion, repair_kit, rusty_sword, resonance_amulet, crystal_shard, etc.

---

## 🚀 NEXT ACTIONS AFTER COMPLETION

1. **Update LootDropper.cs**
   - Replace sample items with real database IDs
   - Test loot drops with new items

2. **Add Icons**
   - Create sprites in `Resources/Textures/Items/`
   - Assign to ItemData.icon field

3. **Test All Systems**
   - Inventory: Pick up items
   - Equipment: Equip weapons/armor
   - Crafting: Use materials in recipes
   - Save/Load: Verify persistence

---

**Questions? Check:** `AGENT2_ITEMDATABASE_ASSET_CREATION_REPORT.md`  
**Script Location:** `Assets/_Project/Editor/ItemDatabasePopulator.cs`  
**Database Location:** `Assets/_Project/Resources/ItemDatabase.asset`
