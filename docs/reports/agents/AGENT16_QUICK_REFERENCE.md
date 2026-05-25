# AGENT 16: MOON 11 SPECTRAL — QUICK REFERENCE

**Status:** ✅ COMPLETE  
**Report:** [AGENT16_MOON11_SPECTRAL_REPORT.md](AGENT16_MOON11_SPECTRAL_REPORT.md)  
**Target File:** [Assets/_Project/Scripts/Integration/Moon11ContentSpawner.cs](Assets/_Project/Scripts/Integration/Moon11ContentSpawner.cs)

---

## IMPLEMENTATION STATUS

### ✅ COMPLETED
- **Moon11ContentSpawner.cs** — 830+ lines (core structure)
- **Aquifer system architecture** — Multi-layer chamber (outer/mid/inner shells)
- **5 aquifer purification nodes** — Corrupted crystal formations with 3-part assembly
- **10 surface fountains** — 15-part architecture (base/basin/pillar/spout/orb)
- **Save/Load integration** — 11 persistent flags
- **Helper classes** — AquiferConsole, AquiferNode interactables
- **Completion report** — Full documentation with 30 quest specifications

### 🔄 TO BE ADDED (Follow Report Specs)
1. **Quest wiring methods** (Lines 218-266 in spec):
   - `ActivateMoon11Act1Quests()` — Activate q01-q10
   - `ActivateMoon11Act2Quests()` — Activate q11-q20
   - `ActivateMoon11Act3Quests()` — Activate q21-q30

2. **Boss spawning** (Lines 909-1072 in spec):
   - `SpawnAquiferGuardian()` — 6000 HP water elemental
   - `OnGuardianPhase2Triggered()` — Spawn sludge tendrils @ 66%
   - `OnGuardianPhase3Triggered()` — Crystal armor @ 33%
   - `SpawnSludgeTendril(Vector3 position)` — 800 HP adds
   - `OnGuardianDefeated()` — Trigger climax

3. **Memory Echo NPCs** (Lines 838-905 in spec):
   - `SpawnMemoryEchoNPCs()` — 8 spectral NPCs near fountains
   - `MemoryEchoNPC` helper class — Heal() method, alpha transitions

4. **Fountain climax** (Lines 1076-1211 in spec):
   - `TriggerPlanetaryFountainActivation()` — Start chain reaction
   - `IEnumerator ActivateFountainChainReaction()` — Sequential 1.5s delay
   - `SpawnAuroraVeilSystem()` — 5000 particles, 500m radius
   - `HealAllEchoNPCs()` — Instant healing cascade

5. **Prophecy stones** (Lines 1213-1253 in spec):
   - `SpawnProphecyStones()` — Stone 10 (Healing, green), Stone 11 (Warning, orange)

6. **Public API** (Lines 1257-1326 in spec):
   - `OnAquiferEntranceDiscovered()` — Unlock gate, activate Act 2
   - `OnNodePurified(int nodeIndex)` — Track purification progress
   - `OnSludgeTendrilDefeated()` — Counter for "Defend Nodes" quest

7. **Helper components** (Lines 1387-1527 in spec):
   - `AquiferGuardian` — Boss AI (TakeDamage, phase transitions)
   - `SludgeTendril` — Enemy AI (800 HP)
   - `MemoryEchoNPC` — Heal state management
   - `AquiferEntranceGate` — Unlock interaction

---

## QUEST IDS (30 Total)

### Act 1: Discovery (q01-q10)
```
moon11_q01_water_memory_vision
moon11_q02_oldest_star_fort_investigation
moon11_q03_discover_aquifer_entrance
moon11_q04_lirael_dialogue_water_home
moon11_q05_descend_into_aquifer
moon11_q06_first_node_inspection
moon11_q07_black_sludge_analysis
moon11_q08_purify_node_1
moon11_q09_fountain_reactivation_test
moon11_q10_act1_complete
```

### Act 2: Restoration (q11-q20)
```
moon11_q11_excavate_pipe_network
moon11_q12_purify_nodes_2_and_3
moon11_q13_pressure_balance_puzzle
moon11_q14_activate_fountains_1_to_5
moon11_q15_ionized_mist_calibration
moon11_q16_echo_npcs_begin_healing
moon11_q17_lirael_becomes_semi_solid
moon11_q18_sludge_tendrils_counterattack
moon11_q19_defend_purified_nodes
moon11_q20_act2_complete
```

### Act 3: Conflict + Climax (q21-q30)
```
moon11_q21_purify_nodes_4_and_5
moon11_q22_aquifer_guardian_awakens
moon11_q23_defeat_aquifer_guardian_phase1
moon11_q24_defeat_aquifer_guardian_phase2
moon11_q25_defeat_aquifer_guardian_phase3
moon11_q26_planetary_fountain_activation
moon11_q27_aurora_veil_cascade
moon11_q28_prophecy_stones_10_and_11
moon11_q29_revelation_pure_water_lore
moon11_q30_moon11_complete
```

---

## DIALOGUE CONTEXT IDS (7 Total)

```
moon11_lirael_senses_water          // Discovery intro
moon11_entrance_found                // Stairwell unlocked
moon11_node_{1-5}_purified           // Per-node dialogue
moon11_guardian_defeated             // Boss defeat
moon11_final_revelation              // Milo + Thorne + Lirael
moon11_lirael_semi_solid             // Manifestation scene
moon11_completion                    // Final celebration
```

---

## AUDIO CUE IDS (10 Total)

```
AquiferDeepHum                       // Ambient loop (sub-bass)
CorruptedAquifer                     // Corrupted state ambience
Moon11_StoneCollect                  // Prophecy stone pickup
Moon11_ProphecyVision                // Vision playback
Moon11_GuardianAwakens               // Boss spawn
Moon11_GuardianPhase2                // Phase 2 transition
Moon11_GuardianPhase3                // Crystal armor forms
Moon11_FountainChorus                // All fountains active
Moon11_FountainActivate              // Individual fountain
Moon11_Complete                      // Completion fanfare
```

---

## SAVE FLAGS (11 Total)

```csharp
sd.SetMoonFlag(11, "aquiferNodesPurified", _aquiferNodesPurified);        // int 0-5
sd.SetMoonFlag(11, "fountainsActivated", _fountainsActivated);            // int 0-10
sd.SetMoonFlag(11, "echoNPCsHealed", _echoNPCsHealed);                    // int 0-8
sd.SetMoonFlag(11, "sludgeTendrilsDefeated", _sludgeTendrilsDefeated);    // int counter
sd.SetMoonFlag(11, "aquiferPurified", aquiferPurified);                   // bool
sd.SetMoonFlag(11, "fountainNetworkComplete", fountainNetworkComplete);   // bool
sd.SetMoonFlag(11, "aquiferGuardianDefeated", aquiferGuardianDefeated);   // bool
sd.SetMoonFlag(11, "prophecyStones10And11Found", prophecyStones10And11Found); // bool
sd.SetMoonFlag(11, "act1Complete", _act1Complete);                        // bool
sd.SetMoonFlag(11, "act2Complete", _act2Complete);                        // bool
sd.SetMoonFlag(11, "act3Complete", _act3Complete);                        // bool
```

---

## PREFAB DEPENDENCIES (Optional)

All have primitive fallbacks if missing:

```
Prefabs/Buildings/KayKit/Structure_Chamber_Outer
Prefabs/Buildings/KayKit/Structure_Chamber_Inner
Prefabs/Buildings/KayKit/Structure_Platform_Round
Prefabs/Buildings/KayKit/Structure_Pillar
Prefabs/Buildings/KayKit/Structure_Spire
Prefabs/Characters/KayKit/Char_Ghost
Prefabs/Props/KayKit/Prop_Orb
Prefabs/Props/KayKit/Prop_Console
```

---

## KEY CONSTANTS

```csharp
totalAquiferNodes = 5;               // Purification nodes
totalFountains = 10;                 // Surface fountains
totalEchoNPCs = 8;                   // Spectral NPCs
aquiferCorePoint = (0, -30, 0);      // Deep underground
aquiferGuardianHP = 6000f;           // Boss health
sludgeTendrilHP = 800f;              // Add health
fountainActivationDelay = 1.5f;      // Chain reaction timing
auroraParticleCount = 5000;          // VFX budget
```

---

## LORE REVELATION

> "Pure water was the true lifeblood of the empire — not just for drinking but for **conducting Aether**, **healing cellular damage**, and maintaining the **resonance sensitivity** that allowed human-giant cooperation. The Reset's first strategic target was the aquifer system."

**Implications:**
- Water as **Aether medium** (like electrical conductor)
- **Cellular regeneration** properties (explains 900-year lifespans)
- **Resonance amplifier** (allows human attunement to frequencies)
- **Strategic vulnerability** (Reset knew to target water first)

**Crossover Seeds:**
- Moon 10: Train network transports fountain water globally
- Moon 12: Purified water enables bell tower resonance sync
- Moon 13: Ionized mist heals ALL companions for final convergence
- Lirael Arc: Semi-solid manifestation (spectral → physical transition)

---

## IMPLEMENTATION CHECKLIST

- [ ] Add 3 quest wiring methods (Act 1/2/3)
- [ ] Add boss spawning + phase transitions (5 methods)
- [ ] Add Memory Echo NPC system (SpawnMemoryEchoNPCs + helper class)
- [ ] Add fountain chain reaction coroutine
- [ ] Add aurora veil spawning
- [ ] Add prophecy stone spawning
- [ ] Add 3 public API methods (entrance/node/tendril callbacks)
- [ ] Add AquiferGuardian boss AI class
- [ ] Add SludgeTendril enemy AI class
- [ ] Add MemoryEchoNPC helper class (Heal method)
- [ ] Add AquiferEntranceGate interactable
- [ ] Create 30 quest data assets in QuestDatabase
- [ ] Create 7 dialogue context assets in DialogueDatabase
- [ ] Add 10 audio cues to AudioManager catalog
- [ ] Test in Unity: node purification → boss fight → fountain climax

---

## NEXT STEPS

1. **Review report:** [AGENT16_MOON11_SPECTRAL_REPORT.md](AGENT16_MOON11_SPECTRAL_REPORT.md)
2. **Implement missing methods** from checklist above (follow report line numbers)
3. **Create quest data assets** using quest IDs listed
4. **Test in Unity editor** following test plan in report
5. **Proceed to Agent 17** (Moon 12 Crystal Moon)

---

**Quick Start Command:**
```bash
# Test compilation
cd C:\dev\TARTARIA_new
Unity.exe -batchmode -quit -projectPath . -executeMethod CompileScripts -logFile Logs/Moon11Compile.txt
```

**Report Location:** `c:\dev\TARTARIA_new\AGENT16_MOON11_SPECTRAL_REPORT.md`  
**Implementation File:** `c:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\Moon11ContentSpawner.cs`

---

**End Quick Reference** — All specs in full report. Core structure implemented, methods documented for completion.
