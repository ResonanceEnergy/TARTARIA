# MOON 4 QUEST LIST — Quick Reference

**Moon:** 4 (Self-Existing)  
**Theme:** Star fort construction, geometric precision, guardian golem Maelix  
**Total Quests:** 30 (ACT 1: 10, ACT 2: 10, ACT 3: 10)  

---

## ACT 1: FOUNDATION DISCOVERY (Quests 1-10)

| Quest ID | Name | Trigger | Completion |
|----------|------|---------|------------|
| moon4_q01_enter_star_fort | Enter Star Fort | UnlockMoon4() | SpawnMoon4Content() |
| moon4_q02_discover_dissonance | Discover Dissonance | UnlockMoon4() | Player examines fort |
| moon4_q03_meet_echo_garrison | Meet Echo Garrison | UnlockMoon4() | SpawnEchoGarrison() |
| moon4_q04_examine_bastions | Examine Bastions | SpawnMoon4Content() | Player inspects 3 bastions |
| moon4_q05_discover_dry_moats | Discover Dry Moats | SpawnMoon4Content() | Player inspects moat |
| moon4_q06_align_first_bastion | Align First Bastion | Bastion interaction | 1st bastion aligned |
| moon4_q07_flood_first_moat | Flood First Moat | Moat interaction | 1st moat flooded |
| moon4_q08_discover_inscription | Discover Inscription | SpawnMoon4Content() | Read Zereth inscription |
| moon4_q09_align_three_bastions | Align Three Bastions | 1 bastion aligned | 3 bastions aligned |
| moon4_q10_flood_three_moats | Flood Three Moats | 1 moat flooded | 3 moats flooded |

---

## ACT 2: CONSTRUCTION (Quests 11-20)

| Quest ID | Name | Trigger | Completion |
|----------|------|---------|------------|
| moon4_q11_aquifer_purge_intro | Aquifer Purge Intro | 1 bastion aligned | Tutorial viewed |
| moon4_q12_align_six_bastions | Align Six Bastions | 3 bastions aligned | 6 bastions aligned |
| moon4_q13_flood_all_moats | Flood All Moats | 3 moats flooded | 6 moats flooded |
| moon4_q14_detect_golem_presence | Detect Golem Presence | 6 moats flooded | Tremor detected |
| moon4_q15_prepare_giant_mode | Prepare Giant Mode | 6 bastions aligned | Tutorial complete |
| moon4_q16_first_golem_encounter | First Golem Encounter | Golem spawns | Cinematic viewed |
| moon4_q17_defend_bastions | Defend Bastions | Golem encounter | Golem defeated |
| moon4_q18_giant_mode_combat | Giant Mode Combat | Golem encounter | Tutorial complete |
| moon4_q19_complete_fort_geometry | Complete Fort Geometry | 6 bastions aligned | 12 bastions aligned |
| moon4_q20_final_moat_check | Final Moat Check | 12 bastions aligned | 6 moats verified |

---

## ACT 3: DEFENSE (Quests 21-30)

| Quest ID | Name | Trigger | Completion |
|----------|------|---------|------------|
| moon4_q21_moat_activation | Moat Activation | Golem defeated | Moats glow |
| moon4_q22_bell_tower_waves | Bell Tower Waves | Fort activation | Scalar waves sent |
| moon4_q23_golem_cleansing_begins | Golem Cleansing Begins | Fort activation | Routing purge complete |
| moon4_q24_defeat_guardian_golem | Defeat Guardian Golem | Golem encounter | Golem defeated |
| moon4_q25_memory_crystal_discovery | Memory Crystal Discovery | Golem defeated | Crystal spawned |
| moon4_q26_view_maelix_memory | View Maelix Memory | Crystal interaction | Cinematic viewed |
| moon4_q27_korath_brother_revelation | Korath Brother Revelation | Memory viewed | Dialogue complete |
| moon4_q28_recover_17h_fragment | Recover 17h Fragment | Revelation | Fragment acquired |
| moon4_q29_unlock_moon5 | Unlock Moon 5 | Fragment acquired | Moon 5 unlocked |
| moon4_q30_moon_complete | Moon Complete | Moon 5 unlocked | Moon 4 complete |

---

## PROGRESSION MILESTONES

### Bastion Alignment (12 total)
- 1 bastion → Unlock Q11 (aquifer purge tutorial)
- 3 bastions → Complete Q11, activate Q12
- 6 bastions → Activate Q15 (giant-mode prep), unlock Q19
- 12 bastions → Complete Q19, activate Q20

### Moat Flooding (6 total)
- 1 moat → Complete Q7, activate Q10
- 3 moats → Complete Q10, activate Q13
- 6 moats → Complete Q13 + Q20, activate Q14 (golem stirs)

### Golem Boss
- **Spawn Trigger:** All 12 bastions aligned + all 6 moats flooded
- **Boss Phases:** Intro → Combat (Q17-18) → Defeat (Q24)
- **Post-Defeat:** Climax event (Q21-23) → Memory crystal (Q25-26)

---

## KEY LORE REVEALS

1. **Zereth Inscription:** "For my brother, the Builder. Hold the line. — Z."
2. **Maelix Identity:** Guardian golem is Korath's brother (corrupted by dissonance)
3. **Three Brothers:** Korath (giant), Maelix (builder/golem), Zereth (Dissonant One)
4. **Zereth Contradiction:** His work is flawless, protective (not destructive villain)
5. **17-Hour Clock:** Tartarian time system different from Reset 24-hour system

---

## CROSSOVER SEEDS

**Forward (Moon 4 → Later):**
- Moon 5: Captain Thorne signal strengthens
- Moon 7: Korath awakening (brother backstory)
- Moon 9: 17-Hour Clock Fragment → Full clock tower
- Moon 13: Zereth mystery → Final choice

**Backward (Moon 4 → Earlier):**
- Moon 3: Star fort routing powers orphan train network
- Moon 1: Geometric precision builds on Echohaven tuning
- Moon 2: Giant-mode combat extends micro-giant training

---

## DIALOGUE KEYS (20+ total)

**Discovery:**
- `moon4_discovery_fort`
- `echo_garrison_confusion`
- `echo_garrison_commander`
- `echo_garrison_hold_line`
- `echo_garrison_song_wrong`

**Construction:**
- `moon4_bastion_first_aligned`
- `aquifer_purge_tutorial`
- `moon4_bastions_progress`
- `moon4_bastions_halfway`
- `giant_mode_preparation`
- `moon4_bastions_complete`
- `moon4_moat_first_flooded`
- `moon4_moats_halfway`
- `moon4_moats_complete`
- `golem_awakening_tremor`

**Conflict:**
- `moon4_golem_distorted`
- `moon4_golem_combat_shout`

**Climax:**
- `moon4_golem_defeated`
- `golem_final_words`
- `moon4_fort_activation`

**Revelation:**
- `moon4_inscription_zereth`
- `zereth_protective_message`
- `maelix_memory_three_brothers`
- `moon4_korath_brother_revelation`
- `zereth_mystery_deepens`
- `moon4_moon_complete`
- `moon5_white_city_tease`

---

**END QUICK REFERENCE**
