# TARTARIA WORLD OF WONDER — Aether Awakening
## Master Game Design Document (GDD)
### Version 1.0.0 — March 25, 2026

---

## Table of Contents

1. [High-Level Vision](#1-high-level-vision)
2. [Core Fantasy & Pillars](#2-core-fantasy-pillars)
3. [Genre & Influences](#3-genre-influences)
4. [World & Setting](#4-world-setting)
5. [Core Gameplay Loop](#5-core-gameplay-loop)
6. [Key Mechanics Summary](#6-key-mechanics-summary)
7. [Campaign Structure](#7-campaign-structure)
8. [Characters & Factions](#8-characters-factions)
9. [Art Direction & Audio](#9-art-direction-audio)
10. [Platform & Technical Requirements](#10-platform-technical-requirements)
11. [Monetization Model](#11-monetization-model)
12. [Development Overview](#12-development-overview)
13. [Document Cross-References](#13-document-cross-references)

---

## 1. High-Level Vision

**Tagline:** *Tune the World. Light the Ley Lines. Reclaim the Golden Age.*

You are a Tartarian descendant awakening after the Mud Flood — a global liquefaction cataclysm (~1830s–1850s) that buried the greatest civilization in human history beneath meters of hardened mud. The once-globe-spanning Tartarian Empire of giants harnessed free atmospheric Aether through sacred-geometry domes, spires, star forts, and ley lines — delivering limitless wireless power, healing cymatics, silent resonance trains, anti-gravity airships, and utopia.

The Great Reset buried it all. Elites rewrote history, staged World's Fairs to parade and then dynamite the surviving wonders, replaced the natural 13-Moon calendar with chaotic Gregorian time, swapped the 17-hour cosmic day for the artificial 24-hour cycle, and suppressed free energy to usher in the age of scarcity and control.

Your mission: **excavate, tune, restore the planetary grid, and trigger the New Golden Age.**

**PC-First Vision:** "Restore paradise on your desktop — immersive sessions that matter." Deep, rewarding loops deliver wonder without frustration. Every restored dome lights the global map. Every tuned frequency feels magical with keyboard, mouse, and gamepad.

---

## 2. Core Fantasy & Pillars

### The Fantasy
The player is a conductor of cosmic harmony — literally conducting buildings to sing, bells to ring, organs to thunder, trains to glide, airships to soar, and fountains to heal. Every action reconnects humanity with the suppressed resonance of nature and cosmos.

### Design Pillars

| Pillar | Description |
|---|---|
| **Wonder** | Every session reveals buried beauty — precision-cut stone, glowing domes, aurora-lit skies. The world gets more breathtaking as you progress. |
| **Mystery** | Who truly triggered the Mud Flood? Why were 17-hour clocks destroyed? What do the prophecy stones reveal? Answers emerge gradually through excavation and lore fragments. |
| **Harmony** | The core mechanic is tuning — bringing structures, frequencies, water, and stone into alignment. Creation over destruction. Restoration over conquest. |
| **Cosmic Sync** | Game time follows the 13-Moon / 17-Hour system. Lunar phases affect gameplay. The whole experience breathes with natural rhythm. |
| **Humor & Heart** | Witty NPC banter (Milo's snark, Thorne's dry giant complaints) balances emotional depth (Korath's sacrifice, Lirael's innocence, Cassian's fall). |

---

## 3. Genre & Influences

**Genre:** Session-based Open-World Restoration RPG + Light City-Builder + Harmonic Puzzle-Action

| Influence | What We Take |
|---|---|
| **Breath of the Wild** | Observation-based exploration, no quest markers, visual discovery |
| **Genshin Impact** | Session-based gacha-free progression, elemental (Aether) system, world beauty |
| **Cities: Skylines** | City-building satisfaction, grid management, visual upgrade feedback |
| **No Man's Sky** | Procedural wonder zones, seamless exploration, collaborative restoration |
| **Baldur's Gate 3** | Deep narrative branching, meaningful choices with long-term consequences |
| **Crimson Desert (2026)** | Massive seamless open world, fluid physics traversal, region liberation |
| **Honkai: Star Rail** | Strategic depth in focused sessions, character progression |
| **Gothic 1 Remake (2026)** | Living NPC societies, nuanced reputation, visible world-altering choices |

---

## 4. World & Setting

### Geography
A planetary-scale map divided into **12–15 modular Aether Restoration Zones**: buried White Cities, star-fort clusters, sunken cathedrals, resonance rail networks, floating sky isles, and fountain sanctums. Each zone is self-contained yet links via portals and ley lines as the grid activates.

### Time System
- **13-Moon Calendar:** 13 perfect 28-day cycles (364 days) + Day Out of Time (harmony festival)
- **17-Hour Day:** Cosmic alignment with solar/lunar breath; certain quests only activate during specific hours
- **Day-Night Cycle:** Night = stronger Aether flow, glowing architecture, aurora visuals

### The Mud Flood Legacy
Every zone features buried lower floors (sunken windows/doors), precision-cut stone partially submerged, and Reset-era propaganda layered atop Tartarian originals. Dig deeper → find greater wonders.

### Scale & Verticality
Multi-level cities (buried basements → rooftops → floating platforms). Airship traversal for vertical exploration. Giant-mode for mega-scale construction and combat.

---

## 5. Core Gameplay Loop

Every session (5–15 minutes) follows this addictive, rewarding cycle:

```
┌─────────────────────────────────────────────────────┐
│                   ENTER ZONE                         │
│              (Portal / Airship / Train)              │
└──────────────────────┬──────────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────────┐
│              1. EXPLORE & EXCAVATE                   │
│   Swipe-dig mud layers → reveal Tartarian grandeur   │
│   Resonance-scan for buried structures & artifacts   │
└──────────────────────┬──────────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────────┐
│              2. TUNE & ALIGN                         │
│   Place spires/domes/grounding nodes                 │
│   Cymatic puzzle or organ mini-game (3-6-9 rhythms)  │
│   Perfect tune = screen vibration + harmonic chime   │
└──────────────────────┬──────────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────────┐
│              3. RESTORE ARCHITECTURE                 │
│   Snap sacred-geometry templates (golden ratio)      │
│   Precision rock cutting in giant mode               │
│   Buildings become functional power plants            │
└──────────────────────┬──────────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────────┐
│              4. HARVEST & BUFF                        │
│   Surplus Aether flows wirelessly via ley lines      │
│   Buffs: healing auras, crop growth, airship fuel    │
│   Distant zones brighten on global map               │
└──────────────────────┬──────────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────────┐
│              5. DEFEND / EXPAND                       │
│   Combat: resonance weapons + giant grapples         │
│   Repel Reset agents & mud golems                    │
│   Connect new ley-line segments                      │
└──────────────────────┬──────────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────────┐
│              6. PORTAL OUT                            │
│   Exit on a high note → see global grid progress     │
│   Return prompt: "Your star fort is singing"         │
└─────────────────────────────────────────────────────┘
```

---

## 6. Key Mechanics Summary

| System | Core Document | Summary |
|---|---|---|
| **Aether Energy** | `02_AETHER_ENERGY_SYSTEM.md` | 3-6-9 harmonic bands, resonance coefficient, harvesting, storage, distribution, overloads |
| **Sacred Geometry Building** | `04_ARCHITECTURE_GUIDE.md` | Golden-ratio snap, Tartarian templates, domes/spires/forts/fountains/organs |
| **Giant Mode** | `07_PC_UX.md` | Toggle avatar size, precision rock cutting, mega-construction, golem wrestling |
| **Cymatic Tuning** | `02_AETHER_ENERGY_SYSTEM.md` | Organ/crystal mini-games, 3-6-9 rhythm puzzles, frequency alignment |
| **Airship & Train Travel** | `04_ARCHITECTURE_GUIDE.md` | Anti-grav flight, megalith transport, resonance rail network |
| **13-Moon Calendar** | `03_CAMPAIGN_13_MOONS.md` | Lunar phases affect buffs/events, 17-hour day windows |
| **Bell Tower Network** | `04_ARCHITECTURE_GUIDE.md` | Planetary scalar wave broadcast, continental healing |
| **Pure Water Fountains** | `04_ARCHITECTURE_GUIDE.md` | Negative-ion healing, conductive moats, ionized mist auroras |
| **Combat & Progression** | `06_COMBAT_PROGRESSION.md` | Resonance weapons, skill trees, adaptive difficulty |
| **AI NPCs** | `05_CHARACTERS_DIALOGUE.md` | Persistent memory, procedural quests, reputation/faction system |
| **Multiplayer** | `07_PC_UX.md` | Co-op portals, shared restorations, wonder servers |

---

## 7. Campaign Structure

**Main Campaign:** "The Eternal Resonance — Symphony of the 13th Moon"

13 chapters aligned to the 13-Moon calendar, each unlocking in chronological order:

| Moon | Name | Theme | Core Mechanic Introduced |
|---:|---|---|---|
| 1 | Magnetic Moon | The Pull of Awakening | Excavation, first dome, giant-mode bursts |
| 2 | Lunar Moon | The Challenge of Shadows | Dissonance purging, micro-giant mode |
| 3 | Electric Moon | The Spark of Service | Resonance trains, orphan adoption |
| 4 | Self-Existing Moon | The Form of Foundations | Star forts, moat puzzles, routing |
| 5 | Overtone Moon | The Radiance of Empowerment | White City, floating platforms |
| 6 | Rhythmic Moon | The Equality of Flow | Pipe organ symphonies, fountain networks |
| 7 | Resonant Moon | The Attunement of Channeling | Giant companion, rock cutting mastery |
| 8 | Galactic Moon | The Integrity of Harmonizing | Full airship armada, megalith transport |
| 9 | Solar Moon | The Intention of Intention | Prophecy stones, timeline echoes |
| 10 | Planetary Moon | The Manifestation of Producing | Continental train network |
| 11 | Spectral Moon | The Liberation of Releasing | Planetary fountain chain |
| 12 | Crystal Moon | The Cooperation of Dedicating | Bell tower synchronization |
| 13 | Cosmic Moon | The Presence of Enduring | True Timeline convergence |

**+ Day Out of Time:** Post-game festival / sandbox celebration

Full details: `03_CAMPAIGN_13_MOONS.md`

---

## 8. Characters & Factions

### Main Companions

| Character | Role | Personality | Arc |
|---|---|---|---|
| **Elara Voss** | Player avatar | Determined, curious, latent giant blood | Descendant awakening to restore the grid |
| **Milo the Mudslinger** | Comic relief / vendor | Snarky, cynical, secretly idealistic | From fake relic seller to true believer |
| **Lirael** | Spectral child Echo | Innocent, sharp, hauntingly wise | From orphan ghost to fully manifested conductor |
| **Captain Thorne** | Giant airship pilot | Dry-witted, dignified, nostalgic | From grumpy loner to fleet commander |
| **Korath the Builder** | Ancient giant mentor | Gentle thunder, patient, sacrificial | Teaches the old ways, sacrifices for the grid |
| **Cassian** | Fallen Echo / betrayer | Charismatic, conflicted, fearful | Redeemable or purgeable — player's choice |

### Factions

| Faction | Alignment | Role |
|---|---|---|
| **Awakened Descendants** | Ally | Players and NPCs restoring the grid |
| **Echo Remnants** | Neutral → Ally | Spectral survivors with fragmented memories |
| **Reset Agents** | Antagonist | Victorian-era enforcers and modern demolition crews |
| **Parasite Cabal** | Hidden Antagonist | Elites who orchestrated the Reset for control |
| **Mud Flood Sentinels** | Wild | Corrupted guardian golems — once protectors, now hostile |
| **The Dissonant One** | Secret Boss | Fallen giant who may have triggered the cataclysm |

Full details: `05_CHARACTERS_DIALOGUE.md`

---

## 9. Art Direction & Audio

### Visual Style
**Hyper-detailed ornate realism with fantastical glow.** Reference real photos of buried basements, 1893 White City pavilions, antique Tartary maps, and capitol domes — but add living Aether veins, floating crystals, giant-scale proportions, and aurora-lit skies.

- **Restored zones:** Golden-age splendor — precision-cut marble, copper domes pulsing with plasma veins, rose windows as cymatic circuits, pure water fountains spraying ionized rainbow mist
- **Buried zones:** Atmospheric mud layers, sunken windows staring at dirt, half-submerged spires barely protruding, eerie beauty in decay
- **Night cycle:** Stronger Aether glow — ley lines as liquid starlight rivers, domes pulsing, auroras tied to grid resonance

### Audio Design
- **Adaptive 432 Hz soundtrack:** Evolves with grid strength — idle zones emit soft drones; fully tuned cities play full symphonies
- **Cymatic sound design:** Buildings literally hum and sing when powered — procedural harmonic music from pipe organs, bell towers, crystal bowls
- **"Old Tartarian" voice:** Constructed language fragments based on 3-6-9 numerological resonance patterns
- **Environmental:** Mud squelches, stone resonates, water conducts, Aether crackles like distant Tesla coils

---

## 10. Platform & Technical Requirements

| Spec | Target |
|---|---|
| **Engine** | Unity 6 LTS (DOTS/ECS, URP, Addressables, Burst + Jobs) |
| **Primary Platform** | Windows 10/11 PC (DirectX 12 / Vulkan, 8–16 GB RAM, GTX 1070+ / RX 580+) |
| **Secondary** | iOS / iPadOS (future port via same codebase) |
| **Graphics** | DirectX 12 / Vulkan + FSR 2 / DLSS Temporal Upscaling, 60 FPS at 1080p–4K |
| **RAM Target** | <2.8 GB peak, 1.5 GB texture budget, auto-purge at 7 GB |
| **Initial Install Size** | ~4 GB (full base content; DLC downloaded on purchase) |
| **Backend** | Firebase / PlayFab (cloud sync, live-ops, A/B testing, analytics) |
| **Auth** | Steam account (future: Sign in with Apple + Game Center for iOS port) |
| **Offline** | Offline-first progression, cloud sync on login |

Full details: `09_TECHNICAL_SPEC.md`

---

## 11. Monetization Model

**Hybrid F2P — no pay-to-win on core Aether power.**

| Revenue Stream | Description |
|---|---|
| **Free Core Loop** | Full excavation, tuning, building, combat — always free |
| **Rewarded Ads** | Optional: Aether refills, double harvest, extra build slots |
| **Cosmetic IAP** | Dome skins, giant avatar themes, historical template packs |
| **Battle Pass** | Seasonal (per-Moon) with exclusive cosmetics + lore drops |
| **Golden Age Subscription** | $9.99/month: unlimited energy, exclusive wonders, priority events |
| **Family Sharing** | Steam Family Sharing support — goodwill + household uplift |

**Year 1 Revenue Target:** $2M+ at 500k units sold (realistic with hybrid model + Steam featuring)

Full details: `08_MONETIZATION_LIVEOPS.md`

---

## 12. Development Overview

| Phase | Scope | Timeline | Budget |
|---|---|---|---|
| **Pre-Production** | GDD, prototypes, art bible | Months 1–2 | $20k–$30k |
| **Phase 1 (MVP)** | 3 zones + core loop + tuning prototype | Months 3–5 | $100k–$150k |
| **Phase 2 (Early Access)** | 8 zones + monetization + Steam Early Access | Months 6–8 | $150k–$250k |
| **Phase 3 (Full Launch)** | Full 13 zones + campaign + polish | Months 9–11 | $100k–$150k |
| **Post-Launch** | Live-ops, seasonal events, 2-person team | Month 12+ | $15k–$25k/event |

**Total estimated budget:** $350k–$550k  
**Team size:** 6 core (designer, 2 engineers, 2 artists, producer) + outsource

Full details: `09_DEVELOPMENT_ROADMAP.md`

---

## 13. Document Cross-References

| Document | Contents |
|---|---|
| `01_LORE_BIBLE.md` | Origins, cosmology, timeline, factions, artifacts, prophecies |
| `02_AETHER_ENERGY_SYSTEM.md` | Full energy mechanics: 3-6-9 bands, tuning, overloads, uses |
| `03_CAMPAIGN_13_MOONS.md` | 13 storylines in lunar order with crossovers & symmetry |
| `03A_MAIN_STORYLINE_REWRITE.md` | Full vivid rewritten storyline — Prologue through Acts 1-5, Epilogue |
| `03B_EXPANSION_PACKS.md` | 10 expansion DLC storylines with interconnection web |
| `03C_MOON_MECHANICS_DETAILED.md` | Granular mechanics for all 13 Moons + Day Out of Time |
| `04_ARCHITECTURE_GUIDE.md` | Zones, buildings, trains, airships, fountains, bell towers |
| `05_CHARACTERS_DIALOGUE.md` | Character profiles, dialogue trees, banter collections |
| `06_COMBAT_PROGRESSION.md` | Combat systems, skill trees, resonance weapons |
| `07_PC_UX.md` | Input controls (keyboard/mouse/gamepad), session flow, HUD, accessibility |
| `08_MONETIZATION.md` | F2P model, events, economy balancing, live-ops calendar |
| `09_TECHNICAL_SPEC.md` | Unity 6 PC architecture, GPU optimization, performance |
| `10_ROADMAP.md` | Phases, milestones, budget, team, risk mitigation |
| `11_SCRIPTED_CLIMAXES.md` | Beat-by-beat playable climax scripts (Star Fort Siege, Orphan Train Escort) |
| `12_VIVID_VISUALS.md` | Enhanced atmospheric visual direction for 7 key moments |
| `13_MINI_GAMES.md` | 6 detailed interactive mini-games with progression & scoring |
| `14_HAPTIC_FEEDBACK.md` | Complete haptic design bible — 20+ subsystems with Hz/ms specifications |
| `15_MVP_BUILD_SPEC.md` | MVP build specification, scope definition, gate criteria |
| `16_PLAYTHROUGH_PROTOTYPES.md` | 10 playtest scenarios, demo priority, trailer beats |
| `17_DAY_OUT_OF_TIME.md` | Post-Moon 13 festival design, companion performances, live-ops |
| `18_PRINCESS_ANASTASIA.md` | Complete Princess Anastasia character bible — 112 dialogue lines |
| `19_ECONOMY_BALANCE.md` | Resource taxonomy, crafting, reward curves, balance testing |
| `20_QUEST_DATABASE.md` | Complete quest catalog — 184 quests across 13 Moons |
| `21_PLAYER_PERSONAS.md` | Target audience archetypes, TAM/SAM, feature priority matrix |
| `22_DIALOGUE_BRANCHING.md` | Choice architecture, consequence tracking, 4-tier taxonomy |
| `23_LOCALIZATION.md` | Multi-language pipeline, 3 tiers, cultural adaptation |
| `24_ACCESSIBILITY.md` | WCAG 2.1 AA compliance, motor/visual/auditory/cognitive |
| `25_SAVE_SYSTEM.md` | Offline-first persistence, cloud sync, conflict resolution |
| `26_LEVEL_DESIGN.md` | Zone layout, traversal systems, POI density, encounter pacing |
| `27_TUTORIAL_ONBOARDING.md` | First 30 minutes beat-by-beat, teaching philosophy |
| `28_ACHIEVEMENTS.md` | 52 achievements, seasonal challenges, Steam integration |
| `29_PRODUCTION_PIPELINE.md` | Art/audio/animation pipeline, outsource specs, CI/CD |
| `30_MARKETING_POSITIONING.md` | Competitive landscape, brand identity, launch strategy |
| `appendices/A_GLOSSARY.md` | Tartarian terms, in-game vocabulary |
| `appendices/B_ASSET_REFERENCE.md` | Art direction, visual reference boards |
| `appendices/C_AUDIO_DESIGN.md` | Soundtrack design, cymatic audio, voice acting |
| `appendices/D_CONTROLS.md` | Input control reference, keybindings, accessibility remaps |
| `appendices/E_METRICS.md` | KPI tracking, analytics dashboards, performance budgets |
| `appendices/F_MOON_INDEX.md` | Moon-to-Document cross-reference map |
| `appendices/G_NPC_INDEX.md` | Character & companion cross-reference index |
| `appendices/H_MECHANIC_INDEX.md` | Gameplay systems cross-reference index |
| `appendices/I_DLC_INDEX.md` | DLC-to-Campaign integration index |
| `appendices/J_ENEMY_INDEX.md` | Enemy bestiary & frequency combat data |

---

*The empire never fell. It was only buried.*  
*And you are the one conducting its resurrection.*

---

**Document Status:** FINAL  
**Author:** Nathan / Resonance Energy  
**Last Updated:** March 25, 2026
