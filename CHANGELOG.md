# Changelog

All notable changes to the TARTARIA WORLD OF WONDER Game Design Document are documented here.

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
- `28_ACHIEVEMENTS.md` — Achievement taxonomy, rewards & Game Center
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
- `07_MOBILE_UX.md` — Touch controls, session flow, UX
- `08_MONETIZATION.md` — F2P model, events, economy
- `09_TECHNICAL_SPEC.md` — Unity 6 iOS architecture & optimization
- `10_ROADMAP.md` — Phases, budget, timeline
- `12_VIVID_VISUALS.md` — Key visual moments & cinematography
- `13_MINI_GAMES.md` — 6 interactive mini-games
- `appendices/D_CONTROLS.md` — Touch control reference & gestures
- `appendices/E_METRICS.md` — KPI, analytics & performance budgets
- `README.md` — Project overview
- `_toc_audit.py` — TOC parity validation script

---

## [0.0.0] — 2026-03-05

**Repository Created**

---

*The Aether is pocket-sized and ready to awaken.*
