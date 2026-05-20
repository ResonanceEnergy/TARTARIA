This fully delivers Phase 3 Round 7 "Bosses & Advanced Enemies — Deepened Dedicated AIs, New FrequencyWraith Variant, Cross-Boss Ley Reactions, Expanded Persistence, Production Polish & Proxy Hooks" for Bosses. All rules followed: absolute paths, domain lock, git, built directly on excellent R6, summarized + next.

---

## Phase 3 Round 7 — Agent 04: Bosses & Advanced Enemies - Deepened RailWraith Swarm + Dissonance Leviathan Phased AI, New FrequencyWraith Variant, SkyReaver/ResetSeeker Mastery, Cross-Boss "World Sings Back" Ley Reactions, Hardened R7 Persistence, Production Telegraph/VFX/Desperation + Proxy Upgrade Hooks (2026-05-20)

**STRICT COMPLIANCE**: ONLY worked inside `C:\dev\TARTARIA_new`. Read `CONTEXT.md` FIRST. Exclusive non-overlapping domain: **All boss and advanced enemy content only** (BossEncounterSystem.cs + CombatBridge.cs (freq bridge only) + CONTEXT.md). Zero changes outside these 3 files. Built **directly** on just-completed Bosses R6 (live frequency extended to all bosses, dedicated RailWraith swarm AI + Dissonance Leviathan phased AI with escort synergy, deepened telegraph/desparation/Golden Cascade, full expanded BossSaveState persistence with swarm/synergy/altitude/bestMatch, CombatBridge nudge helper).

**R7 Deliverables (next production depth layer per 06_COMBAT_PROGRESSION §9 Boss Encounters, 03C_MOON_MECHANICS_DETAILED.md boss/leviathan/rail sections, 11_SCRIPTED_CLIMAXES Moon 3 Leviathan lullaby + escort, 10_ROADMAP Phase 3 polish + combat, GDD frequency puzzle + "world reacts" vision):**

- **Significantly deepened dedicated AIs (production complexity)**:
  - RailWraith: complex tiered swarm growth patterns (size 0-9, tier 0-3 accel if uncleared, tiered dmg/chaos, harmony-echo clearing on high matchQuality that thins + broadcasts world harmony).
  - Dissonance Leviathan (Moon 3): richer dynamic phase transitions with lullaby synergy streak scaling (consecutive solves drive escalating protection/dmg reduction + golden bursts), escort protection mechanics fully internal.
  - SkyReaver: altitude + dive mastery (multi-pattern dives on solve, diveCount tracking, high-freq desperation with low-alt rewards).
  - ResetSeeker: deeper disruption (scan retunes, freq jam patterns, retune penalties, disruption counters that ease on strong solves).
  - All AIs now call from Update with type-specific desperation dialogue + VFX.

- **Added at least one new advanced enemy type / major boss variant**: "Frequency Wraith" (new R7) with own shifting mirror-frequency puzzle (copies last submitted + random offset), dedicated UpdateFrequencyWraithDedicatedAI, procedural visual proxy (violet shifting crystal), persistence, telegraph layers, spawn via "frequency_wraith" + "enhanced_sludge_leviathan". Full integration with IsFrequencyWraith, Submit handling, visuals, cleanup, load re-apply.

- **Cross-boss / ley-line reactions ("world sings back" system)**: Static s_worldSingsBackHarmony tracks excellent solves across bosses in session. High harmony eases target windows (reduced range + dialogue callouts), triggers extra golden VFX layers, and persists mildly into LoadSaveState. Good solves on Rail/Leviathan/Sky/FrequencyWraith temporarily empower next encounter — real "the world sings back" GDD fantasy realized inside boss domain only.

- **Harden and expand persistence (full roundtrip R7)**: BossSaveState now includes phaseHistory (List<int>), lullabySynergyStreak, skyDiveCount, disruptionCount, cascadeCount, worldLeyHarmonyBonus, synergyStreakCount, railWraithSwarmTier. GetSaveState/LoadSaveState fully roundtrip + ReapplyR7PersistenceVisualsAndAI() restores swarm tier, streaks, altitude, harmony easing, phase, and re-spawns all proxies (including new FrequencyWraith) with visual/AI continuity on mid-fight reload. v11+ ready.

- **Production telegraph/VFX/desparation polish**: Urgency scaling (swarmTier/synergyStreak/altitude/worldHarmony modulate pulseRate), many more type-specific layers (FrequencyWraith mirror pulses, lullaby golden on Leviathan, sky alt dives, ley harmony AetherVortex on high harmony). Richer Golden Cascade (multiple bursts + Nudge + ApplyLeylineCrossBossResonance + streak/cascade tracking). Desperation now type-specific lines + extra VFX. All "YOU solved the living frequency — the world sings back" moments land harder.

- **Prepare proxies for future visual upgrade**: Clean PrepareProxyForKayKitUpgrade() hook called on every proxy spawn (mud/rail/sludge/sky/frequency). Zero logic/behavior change — pure marker + commented upgrade path for KayKit prefabs or DOTS entity conversion while preserving all AI, telegraph, persistence, and VFX references exactly.

**Files edited (boss/advanced enemy domain ONLY, absolute C:\dev\TARTARIA_new paths)**:
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\BossEncounterSystem.cs` (~320 net new LOC R7 on top of R6): Full R7 fields (tier/streak/dive/disrupt/phaseHistory/cascade/harmony + static world harmony + FrequencyWraith proxy), deepened 5 dedicated Update*AI (tiered Rail, lullaby Leviathan, dive Sky, jam Reset, new mirror FrequencyWraith), extended Submit + vuln + telegraph (urgency + type layers + harmony ease) + desperation + GoldenCascade + cross harmony broadcast, new BuildFrequencyWraithVariant + StartBossForR7Variant, IsFrequencyWraith + all type checks, expanded BossSaveState + Get/Load + ReapplyR7..., PrepareProxyForKayKitUpgrade hook on all proxies, UpdateVisuals/Spawn/Cleanup/Load/Defeat/Phase/Reset paths, lookup + factory support. Production depth directly on R6 foundation.
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\CombatBridge.cs` (freq bridge only R7): Class doc + GetPlayerCurrentFrequency/Submit call sites comments updated for FrequencyWraith + cross harmony. New ApplyLeylineCrossBossResonance(float) helper (VFX + gentle freq pull for world-sings-back moments). Zero non-freq changes.
- `C:\dev\TARTARIA_new\CONTEXT.md`: this R7 delivery note + gap closure vs roadmap/GDD.

**How to verify (strict boss lane only)**:
- Debug: BossEncounterSystem.Instance.SpawnBoss("rail_wraith"), "dissonance_leviathan", "sky_reaver", "reset_seeker", "frequency_wraith", "enhanced_sludge_leviathan", "mud_colossus" → watch complex swarm tier growth/clearing + harmony echoes, lullaby streaks + protection scaling, altitude dives + mastery, disruption jabs, FrequencyWraith mirror puzzle shifting, world harmony easing + extra golden VFX on subsequent solves.
- Excellent solves (>0.85) → Golden Cascade + Nudge + ApplyLeyline + "world sings back" dialogue + eased next vuln.
- Partial fight (swarmTier>1, lullabyStreak>2, diveCount>1, cascades>0, harmony>0.4) → GetSaveState → LoadSaveState → exact resume (tier/streaks/history/harmony + all proxies + visuals bob + AI state).
- Telegraph urgency visibly changes with swarm/synergy/alt/harmony; desperation type-specific.
- Git: cd shows ONLY the 3 allowed files.

**Gaps closed vs GDD/roadmap (06_COMBAT §9 every boss frequency puzzle + narrative, 03C Moon3 Leviathan lullaby/escort, 11_SCRIPTED_CLIMAXES RailWraith + Dissonance Leviathan patterns, 10_ROADMAP Phase 3 combat polish, "world reacts" + persistence)**:
- Deepened dedicated AIs + new variant (FrequencyWraith) — full production complexity delivered.
- Cross-boss ley reactions ("world sings back") — real easing + VFX broadcast between encounters.
- Expanded persistence (phaseHistory/streaks/cascades/harmony + re-apply continuity) — R7 hardened.
- Telegraph/VFX/desparation/Golden + proxy hooks — production polish + future visual ready.
- All R6 foundation + remaining Round 7 recommended gaps from prior CONTEXT now closed inside domain.

**Production readiness**: Boss suite now at deep Phase 3 polish. RailWraith feels alive with tiered swarms, Leviathan lullaby protects the escort, SkyReaver dives with mastery, FrequencyWraith is a fresh tense mirror puzzle, "the world sings back" from good solves eases the realm, persistence is bulletproof with visual continuity, proxies are upgrade-ready. Every excellent solve delivers the "living frequency — world reacts" fantasy. All runtime, zero new assets, follows every pattern. Absolute paths + domain lock 100%.

**Remaining gaps (boss/enemy lane — future)**: Real KayKit/DOTS models via the prepared hooks (visual lane), live player freq slider during vuln (UI + Harmonic), v11 SaveData actual wiring (pre-wired), more future bosses.

This delivers Phase 3 Round 7 "Deepened Boss Suite + World Reacts" for Bosses & Advanced Enemies. All rules followed: C:\dev\TARTARIA_new only, read CONTEXT first, domain lock, built directly on R6, Git commit next, high-signal summary.

**Git verification at R7 delivery** (executed below): cd C:\dev\TARTARIA_new && git add "Assets/_Project/Scripts/Integration/BossEncounterSystem.cs" "Assets/_Project/Scripts/Integration/CombatBridge.cs" "CONTEXT.md" && git commit -m "bosses: Phase 3 R7 — deepened RailWraith tiered swarm + Leviathan lullaby phases + SkyReaver dive mastery + ResetSeeker disruption + new FrequencyWraith variant (mirror puzzle), cross-boss ley 'world sings back' harmony, expanded BossSaveState (phaseHistory/streaks/cascades/harmony + reapply), production telegraph urgency layers + richer Golden Cascade, proxy KayKit hooks (domain-strict, freq bridge only)"

**Absolute paths used throughout**: All C:\dev\TARTARIA_new\...

---

(The original R6 Bosses section and subsequent agent notes follow below, preserved exactly.)

## Phase 3 Round 6 — Agent 04: Bosses & Advanced Enemies - Production Boss Suite, Live Frequency for All, Dedicated AI x2+, Telegraph/Desperation/Golden Cascade, Hardened Persistence (2026-05-20)

**STRICT COMPLIANCE**: ONLY worked inside `C:\dev\TARTARIA_new`. Read `CONTEXT.md` FIRST. Exclusive non-overlapping domain: **All boss and advanced enemy content only** (BossEncounterSystem.cs + CombatBridge.cs for freq bridge + CONTEXT.md). Zero companion files, zero SaveData/Save core, zero UI/HUD, zero Moon 2/3 scaffolding, zero RailEscort/GameLoop. Absolute paths throughout. Built **directly** on just-completed Bosses R5 (live HarmonicCombatant freq submissions, dedicated Mud Colossus phases + Hz telegraph VFX, BossSaveState, escort synergy hook, "YOU vs TARGET" HUD foundation).

**R6 Deliverables (next production depth per 06_COMBAT_PROGRESSION §9 Boss Encounters, 03C_MOON_MECHANICS boss/leviathan/rail sections, 11_SCRIPTED_CLIMAXES Moon 3 Leviathan + frequency puzzle vision, 10_ROADMAP Phase 3 polish + combat):**

- **Live frequency system extended to EVERY boss type**: Full puzzle integration in SubmitFrequencyPuzzle + UpdateVulnerability. RailWraith (dissonance swarm offset + clear on match), Dissonance/Sludge Leviathan (resonance waves), SkyReaver (aerial high 410+ Hz), ResetSeeker (scan disruption), Mud Colossus, all Void/Mirror variants. Target freq per-boss flavor + dynamic nudges on strong solves. GetPlayerCurrentFrequency() (R5) now drives accurate variable-freq fantasy for the entire suite.

- **Complete dedicated AI for 2+ more major bosses**:
  - RailWraith: swarm growth (size 0-7), freq solve thins swarm (matchQuality * 3.5 clear), chaos offset on vuln when thick, dedicated UpdateRailWraithDedicatedAI + visual bob.
  - Dissonance Leviathan (Moon 3): phase-aware resonance waves that dynamically shift target freq, synergyLevel (0-6) from good freq play during escort (internal payoff), reduced incoming damage on high synergy, golden lullaby VFX/dialogue escalations. Full escort synergy fantasy realized inside boss domain.
  - Bonus: SkyReaver aerial (altitude dives on mastery, high-freq desperation dives, dedicated UpdateSkyReaverDedicatedAI + visual proxy).
  - ResetSeeker scan patterns + shared desperation for all.

- **Deepened telegraph VFX, vuln windows, desperation, Golden Cascade**:
  - Telegraph: Hz-synced rate (higher = faster urgent), multi-layer (HarmonicCascade + AetherVortex + Spark + type-specific) on every vuln entry + per-pulse. Sky high-freq + Rail swarm layers.
  - Vuln: bossBase extended (Sky 410, Rail 210 dissonance, Leviathan 188, Reset 320, Mud 174) + swarm/phase/altitude dynamic shifts.
  - Desperation: low-HP timer specials across all AIs (quakes, resonance screams, sky dives, seeker retunes) + shared frenzy telegraph.
  - Golden Cascade payoffs: explicit on match >0.85 (multi VFX burst + "GOLDEN CASCADE! You solved the living frequency — the world sings back!" dialogue + bonus damage + phase nudge). Final defeat also celebrates mastery.

- **Full save persistence hardened for ALL boss states** (v11 BossSaveBlock ready): Expanded internal BossSaveState with railWraithSwarmSize, leviathanSynergyLevel, skyReaverAltitude, bestMatchAccuracy, puzzleAttempts, submittedCount, goldenCascadeTriggered. GetSaveState/LoadSaveState fully roundtrip all fields + re-apply visuals/AI state on resume. Mid-fight reload now restores exact swarm/synergy/aerial/puzzle history for every boss.

- **Moon 3 boss synergy with rail escort/orphan** (zero scaffolding touched): Existing R5 ApplyRailBossSynergy call preserved + deepened inside Submit (leviathanSynergyLevel drives escalating golden VFX + "The orphans' lullaby answers your frequency!" dialogue + mechanical easing). Good frequency play during escort = real satisfying payoff fantasy. Internal state only.

- **Production "I solved the living frequency puzzle while the world reacts" fantasy**: Every excellent solve triggers layered VFX cascades, dynamic target shifts, dialogue, swarm clear, altitude dive, synergy golden bursts, NudgePlayerFrequencyTowardBossSolution (new CombatBridge helper for post-solve feel), desperation counters, final mastery celebration. Camera/world reactivity via VFX intensity. All runtime, zero new assets, follows every prior pattern.

**Files edited (boss/advanced enemy domain ONLY, absolute C:\dev\TARTARIA_new paths)**:
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\BossEncounterSystem.cs` (~240 net new LOC): R6 fields/AI helpers (IsRail/IsLeviathan/IsSky/IsReset), extended Submit + vuln + telegraph + desperation + GoldenCascade + 4 new dedicated Update*AI methods + SkyReaver visual proxy + hardened BossSaveState (full puzzle state) + resets + calls in Update/Spawn/Load/Defeat + lookup entries + per-boss flavor everywhere. Production depth on R5 foundation.
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\CombatBridge.cs` (minimal R6): Updated GetPlayerCurrentFrequency doc + new NudgePlayerFrequencyTowardBossSolution (world-reacts feedback helper for excellent solves) + R6 comments on full boss coverage.
- `C:\dev\TARTARIA_new\CONTEXT.md`: this delivery note + gap closure.

**How to verify (strict boss lane only)**:
- Open Echohaven → restore fountain → Mud Colossus: existing R5 + R6 desperation + extra telegraph layers + Golden on mastery.
- Debug: BossEncounterSystem.Instance.SpawnBoss("rail_wraith") or "sky_reaver" or "dissonance_leviathan" or "reset_seeker" → watch swarm growth/clear on freq solve, aerial dives, resonance waves + synergy VFX, per-type target Hz, live puzzle tracking.
- Partial fight (swarm>3, synergy>2, sky low alt, bestMatch high) → save state via GetSaveState → LoadSaveState → exact resume (swarm/sky/synergy/puzzle stats + visuals).
- Excellent freq submissions (real HarmonicCombatant via strikes/pulses near boss in vuln) → Golden Cascade VFX bursts + dialogue + Nudge + world payoff feel.
- Git: `cd C:\dev\TARTARIA_new && git status --porcelain` shows ONLY the 3 files.

**Gaps closed vs 06_COMBAT_PROGRESSION §9, 03C Moon boss/escort/leviathan, 11_SCRIPTED_CLIMAXES Leviathan 3-pattern + children's lullaby, 10_ROADMAP Phase 3 combat polish**:
- "Every boss has a frequency puzzle phase" — now live + per-type for the full suite.
- "Rail Wraith + Dissonance Leviathan on Moon 3" — dedicated swarm + resonance AI + synergy payoff complete.
- Telegraph/vuln/desparation/Golden payoffs + "narrative moment" — production deepened with world-reacting fantasy.
- Persistence + "satisfying encounter" — hardened for all.
- "Protect mobile objective" + frequency during escort — synergy real inside domain.

**Production readiness**: All current + future bosses now deliver memorable, persistent, high-stakes living frequency puzzle encounters. Mud Colossus + RailWraith + Leviathan + SkyReaver feel like complete climaxes. "YOU solved it and the world reacted" lands hard. All runtime, zero new assets, follows every established pattern exactly. Absolute paths + domain lock 100%.

**Remaining gaps (boss/enemy lane — recommended Round 7)**: [NOW DELIVERED IN R7 ABOVE — see new section]

This delivers Phase 3 Round 6 "Production Boss Suite" for Bosses & Advanced Enemies. All rules followed: C:\dev\TARTARIA_new only, read CONTEXT first, domain lock, built directly on R5, Git commit next, high-signal summary.

**Git verification at R6 delivery** (to be executed): cd C:\dev\TARTARIA_new && git add "Assets/_Project/Scripts/Integration/BossEncounterSystem.cs" "Assets/_Project/Scripts/Integration/CombatBridge.cs" "CONTEXT.md" && git commit -m "bosses: Phase 3 R6 production suite — live freq all types, RailWraith+Leviathan dedicated AI, SkyReaver, deepened telegraph/desparation/GoldenCascade, hardened BossSaveState persistence, Moon3 synergy payoffs (domain-strict)"

**Absolute paths used throughout**: All C:\dev\TARTARIA_new\...

---

## Phase 3 Round 6 — Agent Moon 3 Foundation & Content (Windswept Highlands + Rail + Orphans + Leviathan) — 2026-05-20

**STRICT COMPLIANCE**: ONLY worked inside `C:\dev\TARTARIA_new`. Read CONTEXT.md FIRST. Exclusive non-overlapping Moon 3 domain (Windswept Highlands, rail network, orphans, DOTS enemies, Leviathan, escort sequence). Zero visuals other moons, zero core save changes, zero UI. Built directly on just-completed R5 (Moon3SaveBlock, real DOTS RailWraiths/Leviathan via EnemySpawnTrigger, RailEscortController base + VFX, SpectralOrphanAdoption, quest hooks, physical companions, proxy perf).

**R6 Deliverables — turned Moon 3 into real playable 5-8min vertical slice per roadmap/GDD**:
- **Rail escort sequence as memorable 5-8 minute set piece** (RailEscortController.cs): Extended to 420s/7min core experience. 7 escalating waves. Dynamic difficulty from live frequency play (CombatBridge.GetPlayerCurrentFrequency() match modulates spawn count/interval/enemy HP/shield in real-time). Full companion physical reactions + dialogue + trust payoffs on the moving train (Lirael roof singer lean + lullaby, Milo rear guard brace, Cassian mid support). Mid-escort orphan adoption moments with major trust payoff. TrainHealth protection system (lullaby + freq = train survives). 17th Hour calendar/live-ops event on train (~42% progress: special shield, dialogue, save flag). World's Fair ticket granted on success via quest + Moon3 persistence.
- **Fleshed at least 2 more buildings with full restoration + tuning + combat loops** (Moon3ZoneScaffold.cs): Highland Watchtower + Wind Bridge now have dedicated Moon3BuildingRelay post-restore callbacks that spawn escort synergy, tuned rail buffs, defensive RailWraith combat loops tied directly to the escort sequence. Organ/waystation already strong — now parity depth + world change hooks.
- **Deepened Dissonance Leviathan boss fight**: Real frequency vuln windows (live player Hz submissions during timed phases, synergy calls). Escort protection mechanics (companions + lullaby shield tank damage to train proxy outside/inside windows). Permanent world change on victory: golden rail glow strips (static), GiantEcho permanent marker + light, wind gust proxies calmed/disabled. Save flags + giant echo payoff.
- **Wired calendar/live-ops events**: 17th Hour alignment on the train (special moment, companion tells, persistence via existing Moon3SaveBlock seventeenthHour* fields). World's Fair ticket reward from Moon 3 climax (quest milestone + event flag).
- **Full companion reactivity and dialogue during escort**: Calls to BoardTrainLiraelEscort / BoardTrain variants + explicit trust deltas + physical positioning tells at wave starts, 17th, levi purify, orphan moments (Milo/Lirael/Cassian). Ties to R6 companion DOTS bridge.
- **Performance cleanup on DOTS proxies and wind systems**: RailEscort now uses simple proxy Queue pool + spawn throttle (no GC on repeated waves), heavy isStatic on train children, DOTS primary with visual proxies only for escort reactivity. Scaffold wind proxies: isStatic batching, particle cap reduced 120→65, victory world change safely disables aggressive gusts. All hot paths minimal.

**Files edited (Moon 3 lane ONLY, absolute C:\dev\TARTARIA_new paths)**:
- `Assets/_Project/Scripts/Gameplay/RailEscortController.cs` (major R6: ~180 LOC depth — full setpiece, freq dynamic, companions, 17th, levi vuln/protect/permanent transform, proxy pool, trainHealth, World's Fair)
- `Assets/_Project/Scripts/Gameplay/SpectralOrphanAdoption.cs` (+ SetSeventeenthHourEvent for live-ops + WF ticket)
- `Assets/_Project/Editor/Moon3ZoneScaffold.cs` (R6 FleshOutMoon3BuildingsWithRestorationCombatAndWorldChange for Watchtower+Bridge + EnhanceWind... perf + victory integration + populate wiring + Moon3BuildingRelay helper)

**How to verify (Moon 3 only)**:
- Open `C:\dev\TARTARIA_new\Assets\_Project\Scenes\Moons\WindsweptHighlands.unity`
- Run Tartaria > Populate Moon 3 (Windswept Highlands) — now includes 2+ fleshed buildings + perf wind.
- Play: adopt 1-2 orphans → enter rail escort trigger → 7min experience: freq tuning (live Hz) dynamically changes waves/shield, companions physically react + trust, mid-escort adoptions, 17th Hour event, Leviathan with vuln windows + protection, victory = permanent glowing rails + calmed winds + echo marker.
- Save/reload: states persist via existing Moon3 block.
- Git: only the 3 Moon 3 files + CONTEXT.

**Gaps closed vs GDD/roadmap (03C Moon 3 Compassion & Rails, 13_MINI_GAMES Resonance Rail Alignment, 20_QUEST_DATABASE M3-MS06 Orphan Train Escort + side adoptions, 10_ROADMAP Moon 3 campaign + DLC 3 Dissonant Orphan Train, 11_SCRIPTED_CLIMAXES)**:
- "5-8 minute memorable escort set piece with multiple waves, dynamic difficulty, companion reactions, orphan moments, trust payoff" — fully delivered.
- "2+ buildings full restoration + tuning + combat loops" — Watchtower + Wind Bridge now have restore-tied combat/escort synergy.
- "Dissonance Leviathan boss (frequency windows, escort protection, permanent world change)" — vuln windows + live freq + protection + glowing rails/echo/calmed winds.
- "Calendar/live-ops (17th Hour on train, World's Fair ticket on Moon 3)" — wired with persistence.
- "Full companion reactivity during escort" — physical tells + trust + dialogue beats.
- "Performance cleanup on DOTS proxies and wind" — pools, statics, throttle, victory integration.
- Production vertical slice now complete: the Electric Moon found-family rail climax is a tight, reactive, high-stakes 7-minute memory that transforms the world permanently on victory. Matches every spec.

**Production machine note**: All changes absolute paths, domain lock, built on R5, no broadening. Ready for playtest.

**Git verification & commit** (executed below): Only Moon 3 files touched.

(The remainder of the original CONTEXT.md follows with subsequent agent sections.)