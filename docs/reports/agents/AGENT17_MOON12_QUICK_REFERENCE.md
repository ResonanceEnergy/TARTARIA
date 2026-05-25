# AGENT 17 QUICK REFERENCE: Moon 12 Crystal

## FILE LOCATIONS
- **Spawner:** `Assets/_Project/Scripts/Integration/Moon12ContentSpawner.cs` (1133 lines)
- **Quests:** `Assets/_Project/Editor/QuestDataFactory.cs` → CreateMoon12Quests()
- **Report:** `AGENT17_MOON12_CRYSTAL_REPORT.md`

## KEY STATS
- **30 quests** fully wired (1 main + 12 towers + 6 mechanics + 3 combat + 5 companions + 3 hidden)
- **3-act structure** with automatic triggers
- **32 dialogue triggers** across all acts
- **1133 lines** of code (133 over target)

## 3-ACT STRUCTURE

### ACT 1: Discovery (Towers 1-4)
- **Trigger:** Moon 12 unlock
- **Content:** Initial tower quests, cymatic puzzle intro
- **Transition:** 4/12 towers → StartAct2()

### ACT 2: Restoration + Combat (Towers 5-8)
- **Trigger:** 4 towers synchronized
- **Content:** 6 mechanic integration quests
- **Climax:** 8 towers → TriggerResetAssault() (3 combat waves)
- **Transition:** 12/12 towers → StartAct3()

### ACT 3: Planetary Ring (Towers 9-12)
- **Trigger:** 12 towers synchronized
- **Content:** 5 companion witness quests, planetary ring cinematic
- **Timeline:**
  - 0s: Ring begins (all 12 bells)
  - 5s: Milo witnesses
  - 10s: Lirael witnesses
  - 15s: Thorne fleet
  - 20s: Korath echo
  - 25s: Children choir
  - 30s: Korath in bells
  - 45s: Prophecy Stone #12
  - 60s: Moon complete, 95% grid

## QUEST ACTIVATION FLOW

```
Moon Unlock
  ↓
moon12_crystal_cooperation (main)
moon12_bell_tower_1
  ↓
[Sequential tower quests 2-12]
  ↓
Tower 4 Complete → ACT 2
  ↓
moon12_mechanic_1-6 (6 quests)
  ↓
Tower 8 Complete → RESET ASSAULT
  ↓
moon12_reset_assault_1-3 (3 quests)
moon12_defend_bell_network
  ↓
Tower 12 Complete → ACT 3
  ↓
moon12_companion_1-5 (5 quests)
moon12_korath_echo_bell
moon12_stone_promise
moon12_ninety_five_grid
  ↓
60s Planetary Ring → MOON COMPLETE
```

## DIALOGUE HOOKS

**ACT 1 Intro:**
- korath_bells_were_first
- lirael_feel_song_waiting
- milo_planetary_guitar

**12 Tower Dialogues:**
- korath_first_bell_memory (T1)
- lirael_harmony_awakening (T2)
- milo_three_down_nine_go (T3)
- thorne_airship_support (T4)
- lirael_halfway_song (T5)
- milo_continental_network (T6)
- korath_brothers_bell (T7)
- lirael_reset_coming (T8)
- thorne_hold_line (T9)
- milo_almost_there (T10)
- lirael_one_more_bell (T11)
- korath_final_bell_ready (T12)

**ACT 2 Transitions:**
- lirael_song_growing_stronger (Act 2 start)
- thorne_airship_fleet_ready
- milo_eighty_percent_grid

**Combat:**
- reset_commander_final_assault
- lirael_they_fear_harmony
- thorne_all_hands_battle_stations

**ACT 3 Ring:**
- lirael_its_happening (pre-ring)
- milo_whole_planet_singing
- thorne_glory_restored
- [5 companion witness dialogues]
- korath_feel_dawn_again (30s)

**Post-Ring:**
- lirael_we_remember_together
- milo_to_forgetting_less
- thorne_sky_is_ours
- korath_song_resumes
- moon12_prophecy_stone_promise
- moon12_two_shadows_doubt

## PUBLIC API

**Moon12ContentSpawner Methods:**
```csharp
UnlockMoon12()                      // Activate Moon 12
SynchronizeTower(int towerIndex)    // Complete tower N (0-11)
OnResetAgentDefeated(GameObject)    // Track combat progress
CompleteMechanicQuest_Organ()       // Complete mechanic quest 1
CompleteMechanicQuest_Cymatic()     // Complete mechanic quest 2
CompleteMechanicQuest_Cutting()     // Complete mechanic quest 3
CompleteMechanicQuest_Fountain()    // Complete mechanic quest 4
CompleteMechanicQuest_Routing()     // Complete mechanic quest 5
CompleteMechanicQuest_GiantMode()   // Complete mechanic quest 6
```

**BellTowerConsole:**
```csharp
Interact(GameObject interactor)     // Tune and sync tower
```

**ResetAgentAI:**
```csharp
TakeDamage(float damage)            // Combat damage handler
```

**ProphecyStoneInteractable:**
```csharp
Interact(GameObject interactor)     // View prophecy vision
```

**CrystalFrequencyMatrix:**
```csharp
ActivateBand(int bandIndex, float intensity)  // Set band state
PulseAllBands()                              // Visual feedback
ResetAllBands()                              // Clear state
```

## ACHIEVEMENTS

1. bell_network_third_synchronized (Tower 4)
2. korath_in_the_bells (30s mark)
3. planetary_bell_harmony (Moon complete)
4. ninety_five_percent_grid (Moon complete)
5. stone_of_promise_collected (Stone #12)

## LORE SEEDS → MOON 13

- **Doubt seed:** "Was Zereth alone?" → 2 Cabal humans revealed
- **95% grid:** Final connection beneath New Chicago
- **Korath's echo:** Manifests fully in convergence
- **Lirael nearly solid:** Full manifestation in final choice
- **All companions present:** Participate in Moon 13 ending choice

## TESTING CHECKLIST

- [ ] Moon 11 complete → Moon 12 unlocks
- [ ] Tower 1-4: Sequential quest flow
- [ ] Tower 4 → ACT 2: Mechanic quests activate
- [ ] Tower 8 → Combat: Reset assault spawns
- [ ] Tower 12 → ACT 3: Companion quests activate
- [ ] 60s planetary ring: All 9 timed events fire
- [ ] Prophecy Stone #12 appears at 45s
- [ ] Moon complete → 95% grid → Moon 13 unlocks
- [ ] All 30 quests marked complete
- [ ] All 32 dialogues play
- [ ] All 5 achievements unlock

## DEPENDENCIES

- QuestManager (quest activation/completion)
- DialogueManager (32 dialogue triggers)
- SaveManager (state persistence)
- Audio.AudioManager (bell tones, music)
- HUDController (objectives, prompts)
- AchievementSystem (5 achievements)
- Input.HapticFeedbackManager (warning, harmonic feedback)
- GameEvents (HUD objective updates)
- CymaticTuningPuzzle (7-band frequency puzzles)

---

**STATUS:** ✅ COMPLETE — Ready for integration testing
