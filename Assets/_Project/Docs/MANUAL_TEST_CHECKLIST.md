# TARTARIA - Manual Test Checklist
## Session: {DATE}
## Tester: {NAME}

---

## Moon 1-4 Regression Test (30 min)

### Moon 1: Echohaven Restoration
- [ ] Boot to Main Menu loads without errors
- [ ] New Game creates save and loads Echohaven scene
- [ ] Player spawns at expected position (near main plaza)
- [ ] Star Dome building can be restored
  - [ ] Resonance Stones appear and can be collected
  - [ ] Building restoration progress updates (HUD/Quest)
  - [ ] Audio/VFX play on restoration complete
- [ ] Excavation site interaction works
  - [ ] Dig prompt appears when near site
  - [ ] Shard collected and added to inventory
  - [ ] Inventory UI shows shard
- [ ] Save game function works (Pause Menu > Save)
- [ ] Quit to Main Menu
- [ ] Load saved game
  - [ ] Player position restored correctly
  - [ ] Building restoration state persists
  - [ ] Inventory contents persist (shard present)

**Notes:**
_______________________________________________

### Moon 2: Crystalline Caverns
- [ ] Moon 2 unlocks after completing Moon 1
- [ ] Cathedral scene loads
- [ ] Dissonance crystals visible and interactable
- [ ] Cassian companion NPC appears
- [ ] Shrink mechanic (micro-giant mode) works
- [ ] Bell tower repair interaction available
- [ ] Mud Golems spawn as enemies

**Notes:**
_______________________________________________

### Moon 3: Rail Escort
- [ ] Mine cart spawns on rails
- [ ] Enemies spawn during escort
- [ ] Cart moves along rail for at least 1 minute
- [ ] Player can defend cart from enemies
- [ ] Escort progress tracked (HUD/Quest)

**Notes:**
_______________________________________________

### Moon 4: φ-Snap Alignment
- [ ] Deep Forge scene loads
- [ ] φ-snap mechanic tutorial appears
- [ ] Objects snap to φ-ratio positions
- [ ] Visual/audio feedback on correct alignment
- [ ] At least one forge puzzle completable

**Notes:**
_______________________________________________

---

## Moon 5-10 Smoke Test (30 min)

### Moon 5: Verdant Pavilion
- [ ] Scene loads
- [ ] At least 1 pavilion restoration mechanic works

### Moon 6: Organ Pipes
- [ ] Scene loads
- [ ] At least 1 pipe interaction works (sound/puzzle)

### Moon 7: Ice Thaw
- [ ] Scene loads
- [ ] At least 1 ice patch can be thawed

### Moon 8: Airship
- [ ] Scene loads
- [ ] Airship is interactable

### Moon 9: Golden Codex
- [ ] Scene loads
- [ ] Codex is interactable

### Moon 10: Rail Station
- [ ] Scene loads
- [ ] Rail station interaction works

**Notes:**
_______________________________________________

---

## Moon 11-13 Finale Test (20 min)

### Moon 11: Fountain Healing
- [ ] Scene loads
- [ ] At least 1 fountain provides healing

### Moon 12: Bell Tower Sync
- [ ] Scene loads
- [ ] At least 1 bell tower is interactable

### Moon 13: Echo Gate + Ending
- [ ] Scene loads
- [ ] At least 1 echo gate interaction works
- [ ] Ending choice UI appears
- [ ] All 3 ending branches visible in UI:
  - [ ] Harmony (restore balance)
  - [ ] Domination (control the power)
  - [ ] Transcendence (ascend beyond)
- [ ] Chose Harmony ending
- [ ] Ending cinematic plays without errors

**Notes:**
_______________________________________________

---

## Performance Metrics

### Moon 1 Profiling
- [ ] Unity Profiler run on Moon 1 for 5 minutes
- Average FPS: ______
- Lowest FPS: ______
- Frame spikes > 16.67ms: ______ (count)
- Memory usage: ______ MB
- Meets 60fps target: YES / NO
- Stays under 3.6GB budget: YES / NO

**Notes:**
_______________________________________________

---

## Blocker Issues Found

### Critical (blocks testing)
1. _______________________________________________
2. _______________________________________________

### Major (impacts experience)
1. _______________________________________________
2. _______________________________________________

### Minor (cosmetic/polish)
1. _______________________________________________
2. _______________________________________________

---

## Overall Test Result

- [ ] PASS - All critical tests passed, ready for next phase
- [ ] PASS WITH ISSUES - Core functionality works, minor issues found
- [ ] FAIL - Critical issues block progression

**Summary:**
_______________________________________________
_______________________________________________
_______________________________________________

**Time Spent:** ______ minutes
**Tested By:** __________________
**Date:** __________________
