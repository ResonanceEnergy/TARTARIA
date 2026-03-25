# TARTARIA WORLD OF WONDER — Development Roadmap
## Phases, Milestones, Team Structure & Risk Assessment

---

## Table of Contents

1. [Development Philosophy](#1-development-philosophy)
2. [Phase Overview](#2-phase-overview)
3. [Phase 1: Foundation (Months 1–3)](#3-phase-1-foundation-months-13)
4. [Phase 2: MVP Build (Months 4–6)](#4-phase-2-mvp-build-months-46)
5. [Phase 3: Polish & Soft Launch (Months 7–9)](#5-phase-3-polish-soft-launch-months-79)
6. [Phase 4: Full Launch (Months 10–12)](#6-phase-4-full-launch-months-1012)
7. [Phase 5: Live-Ops & Expansion (Months 13–24)](#7-phase-5-live-ops-expansion-months-1324)
8. [Team Structure](#8-team-structure)
9. [Risk Assessment](#9-risk-assessment)
10. [Decision Gates](#10-decision-gates)
11. [Summary: Key Dates](#summary-key-dates)

---

## 1. Development Philosophy

**Ship a small, polished wonder — then expand it.**

- The MVP (3 zones, Prologue + Moon 1–3) must be a complete, satisfying experience
- Every milestone has a playable deliverable — no "engine-only" milestones
- Build the Aether system and golden ratio enforcement FIRST — everything else layers on top
- Playtest every 2 weeks with real iOS devices
- Performance targets are non-negotiable from day one

---

## 2. Phase Overview

```
Month:  1   2   3   4   5   6   7   8   9   10  11  12  13→24
        ├───────────┼───────────┼───────────┼───────────┼──────────→
Phase:  │ FOUNDATION│  MVP BUILD│POLISH/SOFT│FULL LAUNCH│ LIVE-OPS
        │           │           │           │           │
Gate:   ■           ■           ■     ■     ■           ■
        G1          G2          G3    G4    G5          G6
        Vertical    Feature     Soft  Data  Global      Expansion
        Slice       Complete    Launch Review Launch     Decision
```

| Phase | Duration | Budget | Deliverable |
|---|---|---|---|
| **1: Foundation** | 3 months | $50,000 | Vertical slice (1 zone, core loop) |
| **2: MVP Build** | 3 months | $60,000 | 3 zones, Prologue + Moon 1–3, all core systems |
| **3: Polish + Soft Launch** | 3 months | $65,000 | TestFlight → limited markets |
| **4: Full Launch** | 3 months | $75,000 | Global iOS + iPad release |
| **5: Live-Ops** | 12 months | $100,000 | Moons 4–13, DLC expansions, events |
| **Total** | **24 months** | **$350,000** | Complete 13-Moon experience |

---

## 3. Phase 1: Foundation (Months 1–3)

**Goal:** Prove the core loop works and feels magical on iPhone.

### Month 1: Engine & Core Systems

| Week | Deliverable |
|---|---|
| 1 | Unity 6 project setup, URP configured, DOTS scaffolding, Git + CI/CD |
| 2 | Aether field system (3 bands, flow calculation, grid partitioning) |
| 3 | Resonance Score system + golden ratio validation engine |
| 4 | MetalFX integration + thermal state machine + device matrix testing |

**Exit Criteria:** Aether flows visually on iPhone 15 Pro at 60 FPS. Golden ratio snapping works in editor.

### Month 2: Building & Exploration

| Week | Deliverable |
|---|---|
| 1 | Building placement system (sacred geometry snap, proportion checks) |
| 2 | Mud corruption shader + restoration reveal dissolve |
| 3 | First zone: Echohaven (500m radius, terrain, basic architecture blockout) |
| 4 | Player movement + camera + basic touch controls |

**Exit Criteria:** Player can walk through Echohaven, find a buried dome, and begin restoration. RS updates in real-time.

### Month 3: Vertical Slice

| Week | Deliverable |
|---|---|
| 1 | Tuning mini-game (frequency matching for Aether nodes) |
| 2 | First companion (Milo) with basic dialogue + idle chatter |
| 3 | First enemy type (Mud Golem) + harmonic combat prototype |
| 4 | Integration pass — full loop: explore → tune → restore → harvest → combat |

**Exit Criteria (GATE 1):** A single continuous 15-minute play session that includes exploration, tuning, building restoration, aether harvesting, and one combat encounter. Runs at 60 FPS on iPhone 15 Pro. Internal team plays it and says "I want to keep playing."

---

## 4. Phase 2: MVP Build (Months 4–6)

**Goal:** Build out 3 complete zones with full campaign through Moon 3.

### Month 4: Content Pipeline + Zone 2

| Week | Deliverable |
|---|---|
| 1 | Architecture art pipeline (dome, spire, star fort — modular kit) |
| 2 | Zone 2: Resonance Falls (waterfalls, precision rock cutting introduced) |
| 3 | Companion #2 (Lirael) + Companion #3 (Thorne) with dialogue trees |
| 4 | Giant Mode prototype (activation, 3 abilities, scaling, camera) |

### Month 5: Zone 3 + Campaign

| Week | Deliverable |
|---|---|
| 1 | Zone 3: Ironheart Citadel (star fort, military architecture) |
| 2 | Prologue campaign ("The First Note Beneath the Mud") |
| 3 | Moon 1–2 campaign (Magnetic + Lunar Moon storylines) |
| 4 | Moon 3 campaign (Electric Moon) + first major boss |

### Month 6: Systems Completion

| Week | Deliverable |
|---|---|
| 1 | 4 skill trees implemented (Resonator, Architect, Guardian, Historian) |
| 2 | Adaptive music system (4 layers, RS-responsive) |
| 3 | 17-hour day/night cycle + 13-moon calendar system |
| 4 | Save system + Firebase integration + IAP framework |

**Exit Criteria (GATE 2):** 3 zones playable with 4+ hours of content. Campaign through Moon 3 complete with all companions, dialogue, combat encounters, and boss fight. Performance within budget on all target devices.

---

## 5. Phase 3: Polish & Soft Launch (Months 7–9)

**Goal:** Make it beautiful, test with real players, iterate on data.

### Month 7: Polish Sprint

| Week | Deliverable |
|---|---|
| 1 | Visual polish pass — shaders, particles, lighting, VFX |
| 2 | Audio pass — all SFX, ambient loops, voice lines for Moons 1–3 |
| 3 | Onboarding flow (first 30 minutes — see `07_MOBILE_UX.md`) |
| 4 | Accessibility pass — VoiceOver, motor options, colorblind modes |

### Month 8: Monetization + QA

| Week | Deliverable |
|---|---|
| 1 | Battle Pass implementation (free + premium tracks) |
| 2 | Cosmetic shop + IAP integration + receipt validation |
| 3 | Rewarded ad integration (in-character prompts) |
| 4 | Full QA pass: 500+ test cases, device matrix, edge cases |

### Month 9: Soft Launch

| Week | Deliverable |
|---|---|
| 1 | TestFlight internal → 50 external testers |
| 2 | Soft launch: Canada, Australia, Netherlands (small English-speaking markets) |
| 3 | Data collection: retention, FTUE completion, monetization, crash rates |
| 4 | Iterate based on data — fix top 3 drop-off points |

**Exit Criteria (GATE 3 - Soft Launch):** App on TestFlight, 50+ external testers.

**Exit Criteria (GATE 4 - Data Review):**
- Day 1 retention >40%
- Day 7 retention >20%
- FTUE completion >70%
- Crash rate <1%
- Average session >10 minutes
- Player sentiment positive (qualitative feedback)

---

## 6. Phase 4: Full Launch (Months 10–12)

**Goal:** Global release + first live event.

### Month 10: Launch Prep

| Week | Deliverable |
|---|---|
| 1 | Iterate on soft launch data — final balancing pass |
| 2 | App Store assets: screenshots, preview video, metadata |
| 3 | Marketing campaign begin (social, press kit, influencer outreach) |
| 4 | Submission to App Store Review |

### Month 11: Global Launch

| Week | Deliverable |
|---|---|
| 1 | **LAUNCH DAY** — worldwide iOS release |
| 2 | Hotfix window — monitor crashes, server load, exploit reports |
| 3 | First "Cosmic Alignment" live event |
| 4 | First Moon battle pass goes live |

### Month 12: Stabilization

| Week | Deliverable |
|---|---|
| 1 | Performance optimization pass (based on real-world device data) |
| 2 | First "World's Fair" event |
| 3 | Community feedback integration (top 10 QoL requests) |
| 4 | Metrics review + planning for Phase 5 |

**Exit Criteria (GATE 5):**
- 100,000+ downloads in first month
- Revenue tracking toward projections
- 4.5+ App Store rating
- Day 30 retention >10%
- No critical bugs or server issues

---

## 7. Phase 5: Live-Ops & Expansion (Months 13–24)

**Goal:** Complete the 13 Moon campaign + launch expansion DLCs.

### Content Cadence

| Month | Content Drop |
|---|---|
| 13 | Moon 4 (Self-Existing) + Zone 4 + **DLC 4: Star Fort Rebellion** ($6.99) |
| 14 | Moon 5 (Overtone) + Zone 5 + **DLC 2: Whispers of the White City** ($6.99) |
| 15 | Moon 6 (Rhythmic) + World's Fair #2 + **DLC 6: Cymatic Requiem** ($6.99) |
| 16 | Moon 7 (Resonant) + Zone 6 + **DLC 8: Giant's Last Stand** ($9.99) |
| 17 | Moon 8 (Galactic) + Zone 7 + **DLC 5: The Airship Armada** ($9.99) |
| 18 | Moon 9 (Solar) + World's Fair #3 + **DLC 9: The Ley Line Prophecy** ($6.99) |
| 19 | Moon 10 (Planetary) + Zone 8–9 |
| 20 | Moon 11 (Spectral) + Zone 10 |
| 21 | Moon 12 (Crystal) + World's Fair #4 |
| 22 | Moon 13 (Cosmic) + Zone 11–12 + Endgame + **DLC 10: True Timeline Divergence** ($12.99) |
| 23 | Post-game sandbox + Golden Age free-build mode + Anastasia Golden Mote epilogue |
| 24 | **Day Out of Time** expansion + Year 2 roadmap announcement |

**Launch-Window DLCs (available from Phases 2–4, Moons 1–3):**

| DLC | Integrated Moon | Unlock Trigger | Price |
|---|---|---|---|
| **DLC 1: The Buried Beacon** | Moon 1 (Magnetic) | After first cathedral restoration | $4.99 |
| **DLC 7: The Parasite Within** | Moon 2 (Lunar) | First corruption purge | $6.99 |
| **DLC 3: The Dissonant Orphan Train** | Moon 3 (Electric) | First train station reactivation | $6.99 |

> These 3 DLCs are purchasable from soft launch onward, providing early revenue and extending the MVP's replayability.

**Full DLC Portfolio:** 10 expansions totaling $78.89 at full price ($4.99–$12.99 range). See [I_DLC_INDEX.md](appendices/I_DLC_INDEX.md) for dependency map.

### Live-Ops Rhythm

| Frequency | Event |
|---|---|
| **Daily** | Milo's Daily Deal |
| **Weekly** | Milo's Market + Lirael's Recital |
| **Bi-weekly** | Thorne's Expedition |
| **Monthly** | Community Build + new Moon battle pass |
| **Quarterly** | World's Fair (flagship event — submission & voting, see [08_MONETIZATION.md](08_MONETIZATION.md) § 7.2.1) |
| **Per DLC** | Major expansion with new zones, companions, mechanics |

**Exit Criteria (GATE 6 — Expansion Decision):**
- Revenue sustaining 2-person live-ops team
- MAU >100,000
- Community actively engaged (event participation >20% of MAU)
- DLC sell-through >10% of MAU

---

## 8. Team Structure

### Core Team (Phases 1–4)

| Role | Count | Responsibility |
|---|---|---|
| **Lead Dev / Tech Director** | 1 | Architecture, DOTS/ECS, Metal 3, performance |
| **Gameplay Programmer** | 1 | Combat, building, Aether system, skill trees |
| **3D Artist** | 1 | Architecture kits, characters, VFX |
| **2D Artist / UI Designer** | 1 | UI Toolkit, HUD, cosmetics, marketing assets |
| **Narrative Designer** | 0.5 | Campaign, dialogue, lore (contract) |
| **Sound Designer** | 0.5 | Music, SFX, adaptive layers (contract) |
| **QA** | 0.5 | Test cases, device matrix, regression (contract) |

**Total: 3 full-time + 1.5 contract = ~4.5 people**

### Live-Ops Team (Phase 5)

| Role | Count |
|---|---|
| **Lead Dev (continuing)** | 1 |
| **Content Designer** | 1 |
| **Community Manager** | 0.5 (contract) |

### Key Hires Timeline

| Hire | When | Why |
|---|---|---|
| Gameplay Programmer | Month 1 | Core systems require 2 devs |
| 3D Artist | Month 2 | Art pipeline needs ramping |
| Narrative Designer | Month 4 | Campaign writing begins |
| Sound Designer | Month 6 | Audio pass timing |
| Content Designer | Month 12 | Live-ops content creation |

---

## 9. Risk Assessment

### High Risk

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| **DOTS/ECS complexity** | Schedule slip | High | Prototype in Month 1; fallback to MonoBehaviour for non-perf-critical systems |
| **MetalFX integration issues** | Visual quality compromise | Medium | Build fallback render path without MetalFX (lower res, still playable) |
| **Scope creep (13 Moons)** | Budget overrun | High | Ship 3 Moons at launch; remaining Moons funded by revenue |
| **App Store rejection** | Launch delay | Medium | Follow guidelines from day 1; pre-submission review with Apple |
| **Player retention below targets** | Revenue shortfall | Medium | Soft launch in 3 markets; 4 weeks to iterate before global |

### Medium Risk

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| **Art style doesn't resonate** | Lower downloads | Medium | Early concept testing with focus group |
| **Combat feels shallow** | Lower retention | Medium | Playtest every 2 weeks; combat redesign window in Month 7 |
| **Server costs exceed projections** | Margin pressure | Low | Firebase free tier covers soft launch; scale only with revenue |
| **Key team member departure** | Schedule slip | Low | Document all systems; pair programming on critical code |
| **Unity 6 bugs** | Dev velocity loss | Medium | Stay on LTS; avoid bleeding-edge packages |

### Contingency Plans

| Trigger | Action |
|---|---|
| Gate 1 fails (vertical slice not fun) | 2-week redesign sprint; if still failing, pivot to simpler mechanics |
| Gate 4 fails (soft launch metrics poor) | Delay global launch by 1 month; deep-dive on retention data |
| Budget runs out before launch | Reduce to 3-zone MVP with 2 Moons; seek publishing partner |
| Revenue below projections post-launch | Reduce live-ops to 1-person; focus on retention over content volume |

---

## 10. Decision Gates

### Gate Checklist Template

Each gate requires sign-off on:

- [ ] **Playable build** — demonstrates gate-specific features
- [ ] **Performance targets** — within budget on all target devices
- [ ] **Quality bar** — team consensus that it "feels right"
- [ ] **Budget status** — on track or with approved variance
- [ ] **Risk review** — updated risk register, no blockers
- [ ] **Next phase plan** — confirmed scope, timeline, resources

### Gate Schedule

| Gate | Date (from start) | Decision |
|---|---|---|
| **G1** | Month 3 | Proceed to MVP build? |
| **G2** | Month 6 | Feature complete — proceed to polish? |
| **G3** | Month 8 | Soft launch ready? |
| **G4** | Month 9 | Data supports global launch? |
| **G5** | Month 11 | Launch successful — proceed to live-ops? |
| **G6** | Month 18 | Expansion viable — invest in DLC? |

---

## Summary: Key Dates

| Milestone | Month | Date (if starting April 2026) |
|---|---|---|
| **Project Kickoff** | 1 | April 2026 |
| **Vertical Slice** | 3 | June 2026 |
| **Feature Complete** | 6 | September 2026 |
| **Soft Launch** | 9 | December 2026 |
| **Global Launch** | 11 | February 2027 |
| **Moon 7 (midpoint)** | 16 | July 2027 |
| **Moon 13 (complete)** | 22 | January 2028 |
| **Year 2 Roadmap** | 24 | March 2028 |

---

**Document Status:** FINAL  
**Cross-References:** `00_MASTER_GDD.md`, `08_MONETIZATION.md`, `09_TECHNICAL_SPEC.md`  
**Last Updated:** March 25, 2026
