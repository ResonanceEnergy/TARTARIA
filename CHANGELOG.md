# Changelog

All notable changes to the TARTARIA WORLD OF WONDER Game Design Document are documented here.

---

## [1.7.0] — 2025-07-27

**Roadmap v9 — Core Gameplay Loop, Crafting, Accessibility & Scanner Systems**  
*4 new C# files, 8 modified — excavation → scan → craft → economy pipeline complete*

### Added
- **ExcavationSystem.cs** (Gameplay) — Core dig mechanic: site registration, 4-layer progression (Mud→Clay→Rubble→Foundation), RS yield with depth bonus + scan accuracy bonus, VFX/haptic hooks, full save/load
- **CraftingSystem.cs** (Gameplay) — 7-tier recipe-based crafting: recipe registration, tier-gated discovery, resource-cost validation via EconomySystem, inventory management, 8 default recipes from Common through Mythic
- **ResonanceScannerSystem.cs** (Gameplay) — Player's first exploration tool: POI registration (7 types), RS-scaled scan radius (30-80m), accuracy calculation, ExcavationSystem integration for site discovery, compass signal feature, aether cost + cooldown
- **AccessibilityManager.cs** (UI) — WCAG 2.1 AA: ColorblindMode enum (Protanopia/Deuteranopia/Tritanopia) with daltonization matrices, text scale 0.75-2x, subtitle toggles, reduced motion, high contrast, screen shake, haptic intensity scaling, PlayerPrefs persistence

### Changed
- **SaveManager.cs** (Save) — Schema v5→v6 migration step added to MigrateIfNeeded(); CreateNewSave() now sets schemaVersion=6, gameVersion="0.6.0"
- **BossEncounterSystem.cs** (Integration) — Added `SpawnBoss(string bossId)` overload with NamedBossLookup dictionary (15 named bosses mapped to Moon indices)
- **GameLoopController.cs** (Integration) — Wired TutorialSystem.OnTutorialComplete (unlocks echohaven_awakening quest) and BossEncounterSystem.OnBossDefeated (triggers climax, advances Moon, companion notification) with proper unsubscription
- **EconomySystem.cs** (Core) — 4 secondary currencies (HarmonicFragments, EchoMemories, CrystallineDust, ForgeTokens) added to CurrencyType enum; MaterialTier enum (Common through Mythic, 7 tiers); expanded all balance/spend/afford methods and save data
- **DayOutOfTimeController.cs** (Integration) — Sandbox mode (EnterSandbox/ExitSandbox), 3 challenge modes (Speedrun 5min / Pacifist 15min / Creative unlimited) via DotTChallengeMode enum, festival economy with 5 shop items
- **DissonanceLensOverlay.cs** (UI) — ICorruptionWeakPoint interface, frequency-match lock-on targeting via SetFrequency(), FirePurgeBeam() for corruption removal at 10/s with haptic feedback
- **HapticFeedbackManager.cs** (Input) — PlayMoonHaptic(int moonIndex, HapticContext context) with GetMoonProfile() returning 5 context-specific patterns; intensity scales 0.3+moonIndex*0.05 clamped to 0.95
- **ZoneTransitionSystem.cs** (Integration) — RS gate check (rsRequirementToUnlock) before zone transition; FadeScreen upgraded from placeholder to SmoothStep CanvasGroup animation; ShowZoneSubtitle() with lore intro + haptic on zone entry
- **CompanionBehaviorSystem.cs** (AI) — Replaced hardcoded 3.0f dialogue wait with data-driven DialogueDuration from CompanionBehavior component (fallback 5s)
- **AIComponents.cs** (AI) — Added DialogueDuration field to CompanionBehavior IComponentData
- **DialogueManager.cs** (Integration) — Added IsPlaying/CurrentLineDuration properties, per-line duration support via DialogueLine.duration field

---

## [1.6.0] — 2025-07-26

**Roadmap v8 — Full Campaign Systems, Achievements, Dialogue Arcs & Consequence Visuals**  
*SaveData schema v6, 8 new C# files, 5 modified — all 13 Moons wired end-to-end*

### Added
- **Moon 8-13 Climax Classes** (ClimaxSequenceSystem.cs) — Armada flyover, Aurora City, Continental Train, Fountain Activation, Bell Tower Ring, True Convergence scripted sequences with RS rewards
- **AchievementSystem.cs** (Integration) — 52 achievements across 8 categories (Restoration, Combat, Exploration, Lore, Campaign, Social, MiniGames, Hidden) with progress tracking, event-driven unlocks, and RS rewards
- **WorldChoiceTracker.cs** (Integration) — 6 major branching World Choices W1-W6 gated by Moon progression, consequence application via controller singletons, full save/load
- **ConsequenceVisuals.cs** (Integration) — Applies zone palette shifts, fog density, and ambient light changes based on World Choice decisions (militarized vs sanctified, open vs sealed, restoration vs transcendence)
- **CompanionDialogueArcs.cs** (Integration) — 40 dialogue nodes across 6 companions (Lirael, Thorne, Korath, Veritas, Milo, Anastasia), Moon-gated with trust progression (Stranger→Bonded), 4 World Choice-branched dialogues, full save/load
- **AquiferPurgeMiniGame.cs** (Integration) — Moon 11 fractal purge mini-game: 3 corruption layers, golden ratio pattern tracing, boss spawn at layer 3, VFX cascade on completion
- **ContinentalRailSystem.cs** (Integration) — Moon 10 rail network: 12 stations, ring+hub graph, A* pathfinding, rail segment repair, train movement, Rail Leviathan boss guard, network completion VFX
- **CosmicConvergenceMiniGame.cs** (Integration) — Moon 13 meta-game: 6-phase coroutine orchestrating all mini-game types, accuracy scoring, RS rewards, achievement M06

### Changed
- **SaveData.cs** — Schema v5→v6, gameVersion 0.5.0→0.6.0, added 7 save blocks: AirshipFleet, LeyLineProphecy, BellTowerSync, GiantMode, WorldChoice, Achievement, DialogueArcs
- **GameLoopController.cs** — OnBeforeSave/OnAfterLoad wired for all 7 new save blocks (27 total blocks)
- **GiantModeController.cs** — Added tracking fields (totalActivations, buildingsLifted, rubbleCleared, totalTimeAsGiant) and GetSaveData()/LoadSaveData() methods
- **DayOutOfTimeController.cs** — Added 6 companion performance coroutines between solidification and true ending (LiraelConcert, ThorneFlyover, KorathSymphony, VeritasOrganFinale, MiloCommerceFestival, AnastasiaSolidificationCelebration)
- **VFXController.cs** — Added 5 spectacle VFX methods: SpawnAnastasiaSolidificationEffect, SpawnPlanetaryBellRing, SpawnContinentalTrainAurora, SpawnAquiferPurificationCascade, SpawnProphecyStoneAlignment

---

## [1.5.0] — 2025-07-25

**BUILD ALL — Runtime Systems Complete + Gate 1 Readiness**  
*36 C# files compile clean under Unity 6000.3.6f1 — full vertical slice gameplay loop wired*

### Added
- **WorldInitializer.cs** (Integration) — Creates ECS entities requiring cross-assembly types: Milo companion (CompanionBehavior + MiloPersonality), 3 enemy spawn triggers (MudGolem at RS 25/50/75), 3 building entities (Dome/Fountain/Spire with DiscoveryTrigger + MudDissolution). Runs after GameBootstrap, before GameLoopController
- **Player ECS position sync** — GameLoopController now copies MonoBehaviour CharacterController position → ECS PlayerTag entity LocalTransform every frame, enabling DiscoverySystem, EnemyAISystem, and CompanionBehaviorSystem to track the player
- **Zone victory sequence** — Coroutine at RS 100: cinematic state → Milo zone_complete dialogue → 6s camera sweep → "ECHOHAVEN RESTORED" HUD overlay → return to exploration
- **ProjectSetupWizard** updated to add WorldInitializer GameObject to generated scenes

### Verified
- **Unity batch compile**: Exit code 0, zero CS errors/warnings across all 10 assemblies
- **Assembly dependency graph**: Core → Gameplay/AI/Audio/Camera/Input/UI/Save → Integration → Editor (no circular references)
- **Complete vertical slice loop**: Explore → Discover (proximity) → Tune (3 variants) → Restore (emergence cinematic) → Combat (3 attacks + stun) → Victory (RS 100)

---

## [1.4.0] — 2025-07-25

**Unity 6000.3.6f1 Migration & Batch Setup Execution**  
*Project compiles and setup wizard runs headlessly — scene, ScriptableObjects, and test geometry all generated*

### Fixed
- **manifest.json** — Corrected URP package name from `com.unity.rendering.universal` to `com.unity.render-pipelines.universal` (17.3.0) for Unity 6000.3.x compatibility
- **CombatBridge.cs** — Fixed `DamageEvent` field name: `DamageType` → `Type` (2 occurrences) to match `CombatComponents.cs` definition
- **DebugOverlay.cs** — Replaced `_em.Debug != null` (struct, not nullable in Entities 1.4.2) with `_world.IsCreated` check
- **Tartaria.Editor.asmdef** — Added missing `Unity.Collections` reference required by Entities 1.4.2

### Changed
- **ProjectVersion.txt** — Updated from `6000.0.37f1` to `6000.3.6f1` (installed version)
- **manifest.json** — Auto-upgraded 11 packages for Unity 6000.3.6f1: Entities 1.3.5→1.4.2, Burst 1.8.18→1.8.27, Collections 2.5.1→2.6.2, Physics 1.3.5→1.4.2, InputSystem 1.11.2→1.18.0, Addressables 2.3.1→2.8.0, VFX Graph 17.0.3→17.3.0, and others
- **TextMeshPro** removed from manifest (merged into `com.unity.ugui` in Unity 6000.3.x; TMPro namespace still available)

### Added
- **ProjectSetupWizard.RunSetup()** — Batch-mode-safe entry point that skips `EditorUtility.DisplayDialog` calls when running headless
- **Echohaven_VerticalSlice.unity** — Generated via batch mode: 13 game managers, Player capsule, CameraRig, test geometry (3 buildings, 6 pillars, 4 mud patches, 3 enemy spawns, 1 Aether vent)
- **ScriptableObject configs** — Generated: 3 building configs (CrystalSpire, HarmonicFountain, StarDome), 2 performance profiles, TartariaConstants
- **Asset directories** — Prefabs, Prefabs/Buildings, Materials, VFX, Audio/Music, Audio/SFX, Textures, Models

---

## [1.3.0] — 2025-07-25

**Unity Deep Audit — Compilation & Runtime Fixes**  
*10 files changed — comprehensive code audit, 6 critical fixes*

### Fixed
- **Tartaria.AI.asmdef** — Added missing `Tartaria.Gameplay` assembly reference (compilation blocker: AI systems use `PlayerTag`, `EnemyAI`, `MudGolem` from Gameplay)
- **GameBootstrap.cs** — Now creates `PlayerTag` entity with `LocalTransform` in ECS world; without this, `DiscoverySystem`, `CompanionBehaviorSystem`, and `EnemyAISystem` would never execute (`RequireForUpdate<PlayerTag>`)
- **CombatSystem.cs / EnemySpawnSystem** — Spawned enemies now receive `EnemyAI` component (with state, patrol, engage radius) and `LocalToWorld` for rendering; previously spawned entities had no AI behavior and no transform
- **GameLoopController.cs** — `PollResonanceScore()` now distributes RS to `ZoneController.UpdateRS()` alongside HUD/Music/VFX; zone atmosphere was never updating
- **GameStateManager.cs** — Replaced thread-unsafe `??=` singleton pattern with `Lazy<T>` for reliable initialization
- **CombatBridge.cs** — Added `OnDestroy()` that cleans up the player combat ECS entity; previously leaked across scene loads

### Changed
- **PlayerTag** moved from `Tartaria.Gameplay.BuildingSystem` to `Tartaria.Core.ResonanceComponents` — avoids circular dependency (Core ← Gameplay) and makes it available to `GameBootstrap`
- **EnemyAI + EnemyAIState** moved from `Tartaria.AI.AIComponents` to `Tartaria.Gameplay.CombatComponents` — avoids circular dependency (Gameplay ↔ AI) since AI already references Gameplay
- **EnemyAISystem.cs** — Added `using Tartaria.Core;` for `PlayerTag` resolution after namespace move

---

## [1.2.0] — 2025-07-24

**Integration Layer — 8 new bridging scripts wiring all systems together**

---

## [1.1.0] — 2026-03-26

**Platform Migration: iOS → PC-First (Steam)**  
*30+ files changed — complete platform pivot from iOS/mobile-first to PC/Steam-first*

### Changed
- **Primary platform** changed from iOS (iPhone 17+) to **Windows 10/11 (Steam)**; iOS retained as future secondary port
- `00_MASTER_GDD.md` — Platform table, monetization model, dev overview rewritten for PC/Steam
- `07_MOBILE_UX.md` → `07_PC_UX.md` — Complete rewrite: touch/gesture → keyboard+mouse & gamepad; session design updated for desktop
- `09_TECHNICAL_SPEC.md` — Metal 3 → Vulkan/DX12; MetalFX → FSR 2/DLSS/XeSS; iPhone device matrix → PC GPU tiers; build pipeline → Steamworks
- `10_ROADMAP.md` — TestFlight → Steam Early Access; App Store launch → Steam release; risk assessment updated
- `14_HAPTIC_FEEDBACK.md` — Core Haptics/Taptic Engine → DualSense adaptive triggers & Xbox impulse triggers
- `15_MVP_BUILD_SPEC.md` — Target hardware → PC GPU tiers; performance budgets updated; project folders restructured
- `16_PLAYTHROUGH_PROTOTYPES.md` — Device matrix → PC hardware tiers; technical requirements updated
- `21_PLAYER_PERSONAS.md` — iOS market sizing → Steam/PC market sizing
- `23_LOCALIZATION.md` — App Store references → Steam Store
- `24_ACCESSIBILITY.md` — VoiceOver/UIAccessibility → Windows Narrator/NVDA/JAWS; iOS Switch Control → Xbox Adaptive Controller; device matrix → PC tiers
- `25_SAVE_SYSTEM.md` — iCloud → Steam Cloud; iOS Keychain → Windows DPAPI; storage paths → %LOCALAPPDATA%
- `28_ACHIEVEMENTS.md` — Game Center → Steam Achievements
- `29_PRODUCTION_PIPELINE.md` — Xcode/TestFlight/Instruments → Visual Studio/Steam/Unity Profiler; Core Haptics AHAP → gamepad haptic patterns
- `30_MARKETING_POSITIONING.md` — App Store featuring → Steam featuring; ASO → Steam Store Optimization; press targets updated
- `README.md` — Vision, tech stack, project structure, and navigation table updated for PC-first
- `appendices/A_GLOSSARY.md` — Dynamic Island, Live Activities, MetalFX, ProMotion, Taptic Engine → PC equivalents
- `appendices/D_CONTROLS.md` — Title & content updated from iOS touch to PC keyboard+mouse & gamepad
- Scattered iOS/mobile references updated across 02, 03, 04, 08, 12, 13, 19 and other docs

---

## [1.0.0] — 2026-03-25

**Documentation Complete — 55 documents, 30 main docs + 10 appendices + 10 DLC + 5 support files.**

### Phase 7 — Formatting Standardization
- Fixed title separator in `00_MASTER_GDD.md` and `README.md` (`:` → `—`)
- Synced Master GDD version to 1.0.0
- Fixed typo in `DLC_10_TRUE_TIMELINE.md` (`mirrorss` → `mirrors`)
- Fixed typo in `12_VIVID_VISUALS.md` (`consumingthe` → `consuming the`)
- TOC audit: 54 PASS / 0 FAIL

### Phase 8 — Document Statistics
- Created `docs/DOC_STATISTICS.md` — comprehensive word count report
- 218,085 words across 54 documents, categorized by type with quality metrics

### Phase 9 — Comprehensive Validator
- Upgraded `_toc_audit.py` from TOC-only checker to 5-pass validation suite
- Checks: TOC parity, inter-doc link resolution, FINAL footer, format/typos, word counts
- Added `--toc` (TOC-only) and `--json` (machine-readable) CLI flags
- Fixed `A_GLOSSARY.md` footer: COMPILED REFERENCE → FINAL
- Added FINAL footer to `DOC_STATISTICS.md`
- All 55 docs pass all checks

### Phase 10 — Release Tag
- Pushed all commits to `origin/main`
- Tagged `v1.0.0` — documentation complete

### Release Package
- Version bump v0.2.0 → v1.0.0
- Created CHANGELOG.md
- README polish with quick-navigation links, doc statistics, and phase history

---

## [0.2.0] — 2026-03-25

**Cross-Reference Integrity Audit & Final Status**  
*56 files changed, 2,432 insertions, 289 deletions*

### Cross-Reference Audit
- Validated 200+ inter-document markdown links — 0 broken
- Validated 32 § (section) references — all targets exist
- Validated numeric consistency: 184 quests, 13 Moons, 10 DLCs, 112 Anastasia dialogue lines, 13 Golden Motes, 432 Hz — all aligned across docs

### Skill Tree Reconciliation
- Fixed node count conflict: `19_ECONOMY_BALANCE` said 4 × 15 = 60 nodes; `06_COMBAT_PROGRESSION` said 4 × 20 = 80 → aligned to 80
- Fixed tree names: Resonance/Architecture/Harmony/Insight → Resonator/Architect/Guardian/Historian
- Fixed currency name: "Skill Points (SP)" → "Skill Crystals (SC)" across 5 docs
- Updated balance model: "unlock all 60" → specialization (95 SC at max level, master 2 trees + invest in 2)

### TOC Audit
- 54 PASS / 0 FAIL / 0 NO_TOC
- Fixed 10 TOC anchor mismatches (`&` → `--` pattern across 7 DLC + 5 appendices)
- Added TOC to `A_GLOSSARY.md` (24 alphabetical entries)
- Added missing TOC entries in `03C`, `28`, `DLC_10`, `20_QUEST_DATABASE`
- Updated `_toc_audit.py` to include appendices F–J

### Design Gap Resolution (3/3 RESOLVED)
- **GAP-01:** Hidden flower patch locations → 13-zone table added to `26_LEVEL_DESIGN.md` § 8.3
- **GAP-02:** World's Fair submission/voting → spec added to `08_MONETIZATION.md` § 7.2.1
- **GAP-03:** Ending variant quest triggers → 4 endings (Harmony/Echo/Reset/True Name) in `20_QUEST_DATABASE.md`

### DLC Structural Fix
- `DLC_03_ORPHAN_TRAIN.md`: Added Companion Reactions section (Milo, Lirael, Thorne, Korath)

### Document Status
- 27 docs updated DRAFT → FINAL
- 26 docs with no footer → FINAL footer added (all DLC, appendices D–J, 8 main docs)
- All 54 documents now carry status metadata

---

## [0.1.2] — 2026-03-24

**Docs 21–30 & Cross-Reference Fixes**  
*12 files changed, 5,111 insertions*

### Added
- `21_PLAYER_PERSONAS.md` — Target audience archetypes & feature priority
- `22_DIALOGUE_BRANCHING.md` — Choice architecture & consequence tracking
- `23_LOCALIZATION.md` — Multi-language pipeline & cultural adaptation
- `24_ACCESSIBILITY.md` — WCAG 2.1 AA compliance & inclusive design
- `25_SAVE_SYSTEM.md` — Persistence, cloud sync & offline-first design
- `26_LEVEL_DESIGN.md` — Zone layout, traversal & encounter pacing
- `27_TUTORIAL_ONBOARDING.md` — First-session script & teaching systems
- `28_ACHIEVEMENTS.md` — Achievement taxonomy, rewards & Steam Achievements
- `29_PRODUCTION_PIPELINE.md` — Art/audio pipeline, outsource & tool chain
- `30_MARKETING_POSITIONING.md` — Competitive landscape & launch strategy

---

## [0.1.1] — 2026-03-24

**New GDD Docs & Master Cross-Refs**  
*9 files changed, 4,051 insertions, 18 deletions*

### Added
- `14_HAPTIC_FEEDBACK.md` — Complete haptic design bible
- `15_MVP_BUILD_SPEC.md` — MVP build specification & scope
- `16_PLAYTHROUGH_PROTOTYPES.md` — 10 playtest scenarios & prototypes
- `17_DAY_OUT_OF_TIME.md` — Post-Moon 13 festival & live-ops event
- `18_PRINCESS_ANASTASIA.md` — Princess Anastasia character bible (~600 lines)
- `19_ECONOMY_BALANCE.md` — Resource systems, crafting & balance
- `20_QUEST_DATABASE.md` — Complete quest catalog (184 quests)

### Changed
- Updated `00_MASTER_GDD.md` and `README.md` cross-references

---

## [0.1.0] — 2026-03-23

**Complete GDD Expansion**  
*16 files changed, 7,868 insertions, 96 deletions*

### Added
- 10 DLC deep-dive documents (`DLC_01` through `DLC_10`)
- `appendices/A_GLOSSARY.md` — Tartarian terms & concepts
- `appendices/B_ASSET_REFERENCE.md` — Art direction & visual guides
- `appendices/C_AUDIO_DESIGN.md` — Cymatic soundtrack & voice design
- `03C_MOON_MECHANICS_DETAILED.md` — Granular Moon 1–13 mechanics & spectacles
- `11_SCRIPTED_CLIMAXES.md` — Beat-by-beat climax scripts

### Changed
- Expanded `15_MVP_BUILD_SPEC.md` with Phase 2+ scope

---

## [0.0.1] — 2026-03-23

**Initial GDD: 18 Documents**  
*19 files changed, 8,105 insertions*

### Added
- `00_MASTER_GDD.md` — Complete Game Design Document
- `01_LORE_BIBLE.md` — Tartarian history, cosmology, factions
- `02_AETHER_ENERGY_SYSTEM.md` — Full energy mechanics deep dive
- `03_CAMPAIGN_13_MOONS.md` — 13 Moon chronological storylines
- `03A_MAIN_STORYLINE_REWRITE.md` — Vivid rewritten main storyline
- `03B_EXPANSION_PACKS.md` — 10 expansion DLC storylines
- `04_ARCHITECTURE_GUIDE.md` — Zones, buildings, sacred geometry
- `05_CHARACTERS_DIALOGUE.md` — Characters, banter, dialogue trees
- `06_COMBAT_PROGRESSION.md` — Combat systems & skill progression
- `07_PC_UX.md` — PC controls, session flow, UX
- `08_MONETIZATION.md` — F2P model, events, economy
- `09_TECHNICAL_SPEC.md` — Unity 6 PC architecture & optimization
- `10_ROADMAP.md` — Phases, budget, timeline
- `12_VIVID_VISUALS.md` — Key visual moments & cinematography
- `13_MINI_GAMES.md` — 6 interactive mini-games
- `appendices/D_CONTROLS.md` — PC input reference & keybindings
- `appendices/E_METRICS.md` — KPI, analytics & performance budgets
- `README.md` — Project overview
- `_toc_audit.py` — TOC parity validation script

---

## [0.0.0] — 2026-03-05

**Repository Created**

---

*The Aether is ready to awaken.*
