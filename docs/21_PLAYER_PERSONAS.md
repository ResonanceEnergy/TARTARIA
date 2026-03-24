# TARTARIA WORLD OF WONDER — Player Persona Framework
## Target Audience Definition, Psychographics & Feature Mapping

---

> *Know who you're building for — and why each of them will stay. A hybrid RPG/builder/puzzle game must serve multiple motivations without fragmenting the experience.*

**Cross-References:**
- [00_MASTER_GDD.md](00_MASTER_GDD.md) — Core pillars (Wonder, Mystery, Harmony, Cosmic Sync, Humor)
- [07_MOBILE_UX.md](07_MOBILE_UX.md) — Session types, touch controls, accessibility
- [08_MONETIZATION.md](08_MONETIZATION.md) — Revenue stream mapping per persona
- [19_ECONOMY_BALANCE.md](19_ECONOMY_BALANCE.md) — Reward curves, "One More" principle
- [appendices/E_METRICS.md](appendices/E_METRICS.md) — Retention & engagement KPIs

---

## Table of Contents

1. [Persona Design Philosophy](#1-persona-design-philosophy)
2. [Persona Overview Matrix](#2-persona-overview-matrix)
3. [Persona 1: The Lore Seeker](#3-persona-1-the-lore-seeker)
4. [Persona 2: The Harmonic Builder](#4-persona-2-the-harmonic-builder)
5. [Persona 3: The Session Sprinter](#5-persona-3-the-session-sprinter)
6. [Persona 4: The Puzzle Maestro](#6-persona-4-the-puzzle-maestro)
7. [Persona 5: The Completionist](#7-persona-5-the-completionist)
8. [Audience Sizing & Market Context](#8-audience-sizing--market-context)
9. [Persona-to-Feature Priority Map](#9-persona-to-feature-priority-map)
10. [Monetization Acceptance by Persona](#10-monetization-acceptance-by-persona)
11. [Retention Hooks by Persona](#11-retention-hooks-by-persona)
12. [Competitor Appeal Mapping](#12-competitor-appeal-mapping)
13. [Design Implications](#13-design-implications)

---

## 1. Persona Design Philosophy

### Why Personas Matter for Tartaria

Tartaria is a **genre hybrid** — restoration RPG + city-builder + puzzle-action + narrative adventure. Each genre attracts a different primary motivation. These personas are not demographics — they are **play-motivation archetypes** based on Bartle's taxonomy adapted for mobile:

| Motivation | Bartle Equivalent | Tartaria Expression |
|---|---|---|
| **Narrative Discovery** | Explorer | Lore fragments, prophecy stones, Anastasia's hidden motes |
| **Creative Expression** | Achiever (builder) | Sacred geometry cities, dome designs, architectural harmony |
| **Session Efficiency** | Achiever (speed) | Quick excavation loops, daily harvests, micro-sessions |
| **Mastery & Skill** | Achiever (puzzle) | Perfect tuning scores, combat chains, mini-game gold |
| **Completion & Collection** | Achiever (100%) | Quest database, codex entries, hidden discoveries |

### The 80/20 Principle

Every player is a **blend** of personas — but one dominates. Features should:
- Serve the **primary persona** deeply (80% of that feature's design)
- Include a **secondary hook** for adjacent personas (20%)
- Never force a player into a persona they haven't chosen

### Mobile Context

All personas share the mobile reality:
- Interrupted frequently — save-anywhere mandatory
- Time-scarce — respect **5–15 minute** session windows
- Touch-native — no controller assumption
- Visually motivated — App Store screenshots sell the first download

---

## 2. Persona Overview Matrix

| Persona | Age Range | Gender Split | Platform Comfort | Session Length | Spending Profile |
|---|---|---|---|---|---|
| **Lore Seeker** | 25–45 | 55F/45M | Moderate | 15–30 min | Low–Mid (battle pass) |
| **Harmonic Builder** | 20–40 | 45F/55M | High | 20–45 min | Mid–High (cosmetics) |
| **Session Sprinter** | 18–35 | 50F/50M | Very High | 3–10 min | Low (rewarded ads) |
| **Puzzle Maestro** | 22–50 | 60F/40M | Moderate | 10–20 min | Mid (battle pass, QoL) |
| **Completionist** | 20–38 | 40F/60M | Very High | 30–90 min | High (subscription, completionist bundles) |

---

## 3. Persona 1: The Lore Seeker

### Identity

> *"I want to uncover the hidden truth. Every prophecy stone, every whisper, every buried library — I will find them all."*

**Real-World Analog:** Reads historical conspiracy theories, watches Ancient Aliens, loves Assassin's Creed's codex entries, bookmarks every Zelda diary page.

### Demographic Profile

| Attribute | Value |
|---|---|
| **Primary Age** | 28–42 |
| **Gender** | 55% Female, 45% Male |
| **Education** | Above average — values intellectual stimulation |
| **Mobile Behavior** | Reads in bed, commutes with lore, weekend deep-dives |
| **Referral Source** | YouTube alt-history channels, book forums, Reddit r/tartaria |
| **Comparable Games** | Genshin Impact (exploration), Baldur's Gate 3 (narrative), Outer Wilds, Firewatch |

### Motivations & Fears

| Motivations | Fears |
|---|---|
| Uncover Tartarian cosmology | Missing lore due to obscure triggers |
| Connect prophecy fragments | Dialogue being skippable/forgettable |
| Understand the Mud Flood truth | Story being shallow or predictable |
| Develop companion relationships | Having to grind combat to access story |
| Find Anastasia's Golden Motes | Lore hidden behind paywalls |

### Session Behavior

```
Minute 0-2:  Checks journal for new lore entries
Minute 2-5:  Explores zone for environmental storytelling
Minute 5-10: Interacts with Echo NPCs, reads dialogue carefully
Minute 10-15: Finds prophecy stone or lore fragment
Minute 15-20: Returns to codex to cross-reference discoveries
Minute 20-30: Deep-dives into companion dialogue branches
```

### Key Feature Dependencies

| Feature | Importance | Notes |
|---|---|---|
| Lore codex / journal | **Critical** | Must auto-assemble lore fragments into readable narratives |
| Companion relationship arcs | **Critical** | Milo → Korath → Lirael → Anastasia progression |
| Environmental storytelling | **High** | Buried newspapers, faded murals, cracked prophecy stones |
| Quest narrative framing | **High** | Every quest needs context — no "fetch 10 rocks" |
| Skip-combat option | **Medium** | Story difficulty mode with reduced combat |
| Voice acting | **Medium** | Written dialogue fine, but VO elevates immersion |

### Churn Risk Triggers

- Story pacing stalls (no new lore for 3+ sessions)
- Forced combat gates blocking narrative access
- Lore fragments feel random/disconnected
- Companion dialogue repeats too early

---

## 4. Persona 2: The Harmonic Builder

### Identity

> *"I want to build a city so beautiful it sings. Every dome perfectly placed, every ratio golden, every ley line harmonized."*

**Real-World Analog:** Plays Cities: Skylines to 3AM, curates Pinterest architecture boards, debates Brutalism vs. Art Nouveau, found Monument Valley meditative.

### Demographic Profile

| Attribute | Value |
|---|---|
| **Primary Age** | 22–38 |
| **Gender** | 45% Female, 55% Male |
| **Platform Comfort** | High — experienced mobile/PC gamer |
| **Mobile Behavior** | Long sessions on couch, tablet preferred, screenshots shared |
| **Referral Source** | Architecture Instagram, r/citiesskylines, design YouTube |
| **Comparable Games** | Cities: Skylines, Townscaper, Islanders, Monument Valley, Genshin (Serenitea Pot) |

### Motivations & Fears

| Motivations | Fears |
|---|---|
| Achieve perfect golden-ratio compliance | Limited building variety |
| See the city light up as RS grows | Buildings feeling "samey" after Moon 5 |
| Discover rare architecture blueprints | Grid-snap being too restrictive or too loose |
| Share city screenshots | Monetization gating best cosmetic domes |
| Unlock all dome/spire variants | Performance drops in dense cities |

### Session Behavior

```
Minute 0-3:  Surveys city overview, plans next building placement
Minute 3-8:  Places 1-2 buildings, adjusts proportions for golden ratio
Minute 8-12: Runs tuning mini-game to activate new structures
Minute 12-18: Tweaks layout, moves fountains, tests ley-line connections
Minute 18-25: Zooms out — admires result, takes screenshot
Minute 25-45: Deep session — plans entire district, experiments with spire variants
```

### Key Feature Dependencies

| Feature | Importance | Notes |
|---|---|---|
| Sacred geometry snap system | **Critical** | Must feel assistive, not restrictive |
| Building variety (dome/spire/fort/fountain) | **Critical** | New archetypes every 2 Moons minimum |
| Visual feedback on RS accumulation | **Critical** | City must visually transform as RS grows |
| Photo mode | **High** | Shareable screenshots drive organic growth |
| Building upgrade chains | **High** | Start stone → copper → crystal progression |
| Free-build sandbox (post-campaign) | **Medium** | Post-Moon 13 relaxation content |

### Churn Risk Triggers

- Building palette exhausted too quickly
- Performance degrades with 50+ structures
- No visual distinction between good and great placement
- Sharing disabled or friction-heavy

---

## 5. Persona 3: The Session Sprinter

### Identity

> *"I have 7 minutes waiting for the train. Let me excavate something, harvest Aether, and feel like I progressed."*

**Real-World Analog:** Plays Subway Surfers, Candy Crush, or Genshin dailies during commutes. Values efficiency per minute.

### Demographic Profile

| Attribute | Value |
|---|---|
| **Primary Age** | 18–32 |
| **Gender** | 50/50 |
| **Platform Comfort** | Very high — phone is primary gaming device |
| **Mobile Behavior** | Plays in transit, lunch breaks, between meetings |
| **Referral Source** | App Store featuring, TikTok gameplay clips, word of mouth |
| **Comparable Games** | Genshin (dailies), Honkai: Star Rail, AFK Arena, Cookie Run |

### Motivations & Fears

| Motivations | Fears |
|---|---|
| Complete daily tasks in <5 min | Mandatory 30-min sessions to progress |
| See progress bar advance every session | Missing time-limited events |
| Feel rewarded quickly | Complex menus wasting limited time |
| Keep streak alive | Being behind friends/community |
| Earn enough for next unlock | Ads interrupting core gameplay |

### Session Behavior

```
Minute 0-1:  Login → daily harvest → RS update
Minute 1-3:  Quick excavation or combat encounter
Minute 3-5:  Collect rewards → check quest progress
Minute 5-7:  Optional: one tuning mini-game or building placement
Minute 7-10: Bank progress → exit
```

### Key Feature Dependencies

| Feature | Importance | Notes |
|---|---|---|
| Quick-play menu | **Critical** | One-tap access to daily tasks without zone loading |
| Auto-save (10-second intervals) | **Critical** | Zero progress loss on interruption |
| Daily harvest shortcut | **Critical** | Collect all zone yields in one swipe |
| Session recap screen | **High** | Show exactly what was earned, what's next |
| Offline progression | **High** | Resources accumulate while away |
| Skip-cinematic button | **Medium** | Respect time — never force unskippable sequences |

### Churn Risk Triggers

- Daily routine takes >5 minutes
- Progress feels invisible in short sessions
- Forced narrative/cinematic blocks quick play
- App launch time >3 seconds

---

## 6. Persona 4: The Puzzle Maestro

### Identity

> *"Gold star on every tuning game. Perfect resonance chain in combat. I play for the satisfaction of flawless execution."*

**Real-World Analog:** Plays rhythm games (Cytus, Phigros), puzzle games (The Witness, Baba Is You), chases S-ranks in everything.

### Demographic Profile

| Attribute | Value |
|---|---|
| **Primary Age** | 24–45 |
| **Gender** | 60% Female, 40% Male |
| **Platform Comfort** | Moderate — selective about games, high skill ceiling valued |
| **Mobile Behavior** | Focused sessions, headphones in, minimal multitasking |
| **Referral Source** | App Store "Games We Love," puzzle game communities, review sites |
| **Comparable Games** | The Witness, Monument Valley, Cytus II, Okami, Into the Breach |

### Motivations & Fears

| Motivations | Fears |
|---|---|
| Achieve ★★★ Gold on every mini-game | Puzzles being too easy / no mastery curve |
| Master frequency-matching combat | RNG undermining skill-based outcomes |
| Find optimal tuning sequences | Mini-games becoming repetitive |
| Discover hidden mechanical interactions | Auto-play options trivializing skill |
| Rank on leaderboards | Being forced to grind non-puzzle content |

### Session Behavior

```
Minute 0-2:  Select zone with pending mini-game or combat challenge
Minute 2-5:  Attempt mini-game at highest difficulty
Minute 5-8:  Retry for gold/perfect score
Minute 8-12: Combat encounter — maximize resonance chain combo
Minute 12-18: Explore for hidden mini-game variant or challenge mode
Minute 18-20: Check personal best records, compare to community
```

### Key Feature Dependencies

| Feature | Importance | Notes |
|---|---|---|
| Mini-game mastery tiers (★★★) | **Critical** | Bronze/Silver/Gold with visible distinction |
| Combat combo depth | **Critical** | Frequency-matching must reward precision |
| Challenge modes | **High** | Timed variants, no-damage runs, speedrun |
| Personal best tracking | **High** | Per-mini-game, per-zone, per-Moon |
| Hidden mechanical interactions | **Medium** | Discovery of emergent combos |
| Leaderboards | **Medium** | Weekly/Moon-based, anti-cheat protected |

### Churn Risk Triggers

- Mini-game difficulty plateaus too early
- RNG-heavy combat (not skill-based)
- No way to replay favorite challenges
- Mastery has no visible reward

---

## 7. Persona 5: The Completionist

### Identity

> *"184 quests, 13 Moons, 13 Golden Motes, every Echo Memory, every prophecy stone — I will find them ALL."*

**Real-World Analog:** 100% Breath of the Wild, platinums everything on PlayStation, maintains spreadsheets for game completion, loves Hollow Knight's map system.

### Demographic Profile

| Attribute | Value |
|---|---|
| **Primary Age** | 20–35 |
| **Gender** | 40% Female, 60% Male |
| **Platform Comfort** | Very high — cross-platform gamer, phone is secondary device |
| **Mobile Behavior** | Long guided sessions, follows checklist, plays late evening |
| **Referral Source** | Gaming subreddits, achievement-hunting communities, YouTube 100% guides |
| **Comparable Games** | BotW, Hollow Knight, Genshin (100% exploration), Assassin's Creed (sync points) |

### Motivations & Fears

| Motivations | Fears |
|---|---|
| 100% quest completion | Missable content (permanently locked quests) |
| Find every hidden quest and Golden Mote | Progress-tracking being unclear |
| Max every companion loyalty chain | Time-gated content expiring before completion |
| Fill entire codex | Hidden prerequisites with no hints |
| Earn all achievements | Bugged quests preventing 100% |

### Session Behavior

```
Minute 0-5:  Review quest tracker, plan zone sweep
Minute 5-15: Systematic zone exploration — every corner, every hidden path
Minute 15-25: Complete side quest chain, check for hidden prerequisites
Minute 25-40: Companion dialogue exhaustion — trigger all available lines
Minute 40-60: Return to previous zones for post-unlock hidden content
Minute 60-90: Moon-end sweep — verify completion before advancing
```

### Key Feature Dependencies

| Feature | Importance | Notes |
|---|---|---|
| Quest tracker with % completion | **Critical** | Per-zone, per-Moon, per-type breakdown |
| Map markers for discovered/undiscovered | **Critical** | Fog of war that clears with exploration |
| No permanently missable content | **Critical** | All quests accessible post-Moon replay |
| Codex / collection tracker | **High** | Lore, companions, buildings, mini-games |
| Hidden content hints | **High** | Environmental clues, companion whispers |
| Post-campaign zone replay | **Medium** | Return to any Moon for missed content |

### Churn Risk Triggers

- Missable content with no replay option
- Tracker shows 99% with no clue where the last 1% is
- Bugged or unobtainable entries in codex
- Time-locked events preventing 100%

---

## 8. Audience Sizing & Market Context

### Total Addressable Market (TAM)

| Segment | Global Size (2026) | Tartaria Relevance |
|---|---|---|
| **Mobile Gamers (iOS)** | ~1.1 billion | Total addressable |
| **Adventure/RPG Mobile** | ~280 million | Primary genre audience |
| **City-Builder Mobile** | ~120 million | Secondary genre overlap |
| **Puzzle Mobile** | ~400 million | Tertiary (mini-games) |
| **Alt-History/Mythology Interest** | ~50 million | Niche but highly engaged |

### Serviceable Addressable Market (SAM)

| Filter | Reduction | Remaining |
|---|---|---|
| iOS 18+ (iPhone 15 Pro+) | ~15% of iOS gamers | ~165 million |
| English-speaking primary | ~40% | ~66 million |
| Adventure/RPG/Builder interest | ~25% | ~16.5 million |
| Alt-history curiosity | ~10% | ~1.65 million |

### Realistic Year-1 Target

| Metric | Target | Basis |
|---|---|---|
| **Downloads** | 500,000 | App Store featuring + organic |
| **D30 Retention** | 12% | 60,000 monthly actives |
| **Conversion Rate** | 3.5% | 21,000 paying users |
| **ARPU** | $4.00 | Hybrid model (cosmetic + pass + sub) |
| **Year 1 Revenue** | $2.0M+ | Aligned with Master GDD target |

### Persona Distribution (Projected)

| Persona | % of Players | % of Revenue |
|---|---|---|
| **Lore Seeker** | 30% | 20% |
| **Harmonic Builder** | 20% | 30% |
| **Session Sprinter** | 25% | 10% |
| **Puzzle Maestro** | 15% | 15% |
| **Completionist** | 10% | 25% |

**Key Insight:** Completionists are 10% of players but 25% of revenue — they buy subscriptions, completionist bundles, and play long enough to accumulate IAP spend. Builders are the second revenue driver through cosmetic spending.

---

## 9. Persona-to-Feature Priority Map

| Feature | Lore Seeker | Builder | Sprinter | Puzzle | Completionist |
|---|---|---|---|---|---|
| Story campaign (13 Moons) | ★★★ | ★★ | ★ | ★★ | ★★★ |
| Building system | ★ | ★★★ | ★★ | ★ | ★★ |
| Mini-games (6 types) | ★ | ★ | ★★ | ★★★ | ★★ |
| Combat system | ★★ | ★ | ★★ | ★★★ | ★★ |
| Companion dialogues | ★★★ | ★ | ★ | ★ | ★★ |
| Quest database (184) | ★★ | ★ | ★ | ★ | ★★★ |
| Codex / collection tracker | ★★★ | ★ | ★ | ★ | ★★★ |
| Quick-play menu | ★ | ★ | ★★★ | ★ | ★ |
| Photo mode | ★ | ★★★ | ★ | ★ | ★★ |
| Achievements | ★★ | ★★ | ★★ | ★★ | ★★★ |
| Challenge modes | ★ | ★ | ★★ | ★★★ | ★★ |
| DLC expansions | ★★★ | ★★ | ★ | ★★ | ★★★ |

**★★★ = Must-have** | **★★ = Important** | **★ = Nice-to-have**

---

## 10. Monetization Acceptance by Persona

| Monetization Vector | Lore Seeker | Builder | Sprinter | Puzzle | Completionist |
|---|---|---|---|---|---|
| **Cosmetic IAP (domes, skins)** | Low | **Very High** | Low | Low | Medium |
| **Battle Pass (Artisan's Ledger)** | **High** | Medium | Medium | Medium | **Very High** |
| **Subscription (Golden Age)** | Medium | Medium | Low | Low | **Very High** |
| **Rewarded Ads** | Low | Low | **Very High** | Medium | Low |
| **DLC Expansions** | **Very High** | Medium | Low | Medium | **Very High** |
| **Completionist Bundles** | Low | Low | Low | Low | **Very High** |

### Monetization Design Rules Per Persona

1. **Lore Seeker:** Sell DLC and battle pass lore drops. Never gate story content.
2. **Builder:** Sell cosmetic dome skins, building ornaments, photo mode frames. Highest cosmetic spend.
3. **Sprinter:** Offer rewarded ads for Aether refills. Never force ad views. Keep rewards proportional.
4. **Puzzle Maestro:** Sell challenge mode access in battle pass. Never sell "skip puzzle" options.
5. **Completionist:** Golden Age subscription is the perfect product — unlimited energy, exclusive codex entries, priority event access. Completionist bundles ("All Moon 5 cosmetics") drive high per-bundle spend.

---

## 11. Retention Hooks by Persona

### Day 1 (FTUE — First-Time User Experience)

| Persona | Must Experience in First 15 Minutes |
|---|---|
| **Lore Seeker** | Milo's first lore-rich dialogue + first prophecy stone discovery |
| **Builder** | First dome restoration + golden ratio "snap" satisfaction |
| **Sprinter** | Complete excavation → build → reward cycle in <5 min |
| **Puzzle Maestro** | First tuning mini-game with visible ★ rating |
| **Completionist** | Quest tracker appears + first hidden quest discovered |

### Day 7

| Persona | Must Have Achieved by D7 |
|---|---|
| **Lore Seeker** | Moon 1 complete + 3+ companion conversations + first Anastasia whisper |
| **Builder** | 5+ buildings placed + visible city forming + second dome variant unlocked |
| **Sprinter** | Daily loop established (<5 min) + 3+ rewards collected daily |
| **Puzzle Maestro** | 2+ mini-game types encountered + first Gold ★★★ rating |
| **Completionist** | 15+ quests completed + quest tracker shows clear path to more |

### Day 30

| Persona | Must Have Achieved by D30 |
|---|---|
| **Lore Seeker** | Moon 3+ storyline depth + multiple companion arcs advancing |
| **Builder** | Multi-zone city + architecture variety + first screenshot shared |
| **Sprinter** | Consistent daily login + meaningful progression without long sessions |
| **Puzzle Maestro** | All 6 mini-game types encountered + challenge variants discovered |
| **Completionist** | 40+ quests done + collection tracker showing clear % + DLC awareness |

---

## 12. Competitor Appeal Mapping

| Competitor | Primary Persona Served | How Tartaria Differentiates |
|---|---|---|
| **Genshin Impact** | Lore Seeker + Builder | No gacha, restoration not conquest, 13-Moon pacing vs. patch-driven |
| **Honkai: Star Rail** | Sprinter + Lore Seeker | Builder pillar, non-combat core loop, sacred geometry depth |
| **Cities: Skylines** | Builder | Mobile-first, narrative campaign, combat + puzzle integration |
| **Monument Valley** | Puzzle Maestro | 100× content depth, persistent world, community features |
| **Breath of the Wild** | Completionist + Lore Seeker | Mobile-native, session-friendly, restoration vs. combat focus |
| **Outer Wilds** | Lore Seeker | Mobile access, persistent progression, replayable content |
| **Cookie Run** | Sprinter | Deeper systems, premium art quality, no auto-play |
| **Townscaper** | Builder | Gameplay depth, narrative, progression systems |

### Unique Position Statement

> **Tartaria occupies the intersection of restoration wonder + sacred geometry building + harmonic puzzle mastery — a space no current mobile game fills.** It serves Genshin's narrative explorers with a non-gacha model, Cities Skylines' builders with sacred geometry, Monument Valley's puzzle lovers with 100× more depth, and completionists with 184+ quests across a 13-Moon campaign.

---

## 13. Design Implications

### Feature Priority Rules (derived from personas)

1. **Never gate narrative behind combat difficulty** → Lore Seekers churn. Add Story Mode difficulty.
2. **Never gate building behind currency timers** → Builders churn. All building always available.
3. **Always provide a <5 min meaningful loop** → Sprinters churn without it. Quick-play menu is mandatory.
4. **Always provide mastery depth in mini-games** → Puzzle Maestros churn on easy plateaus. Scale difficulty.
5. **Never include permanently missable content** → Completionists churn permanently. All content replayable.
6. **Photo mode at launch, not post-launch** → Builders drive organic social media growth. Delay costs downloads.
7. **Quest tracker must be visible from session 1** → Both Completionists and Lore Seekers need progress visibility.
8. **DLC must add story + building + puzzles** → Serve all personas in every expansion, not just one.

### MVP Persona Focus

For the Phase 1 vertical slice (3 zones, Moon 1–3):
- **Primary target:** Lore Seeker + Sprinter (widest initial appeal)
- **Secondary target:** Builder + Puzzle Maestro (deepens engagement)
- **Tertiary target:** Completionist (early hooks, full payoff post-Moon 5)

This means the MVP must nail: fast session loops, compelling Milo/Anastasia narrative, satisfying dome restoration, and at least 2 mini-game types with mastery tiers.

---

*Every player finds their own way into Tartaria. Our job is to make sure every way leads to wonder.*

---

**Document Status:** DRAFT
**Author:** Nathan / Resonance Energy
**Last Updated:** March 24, 2026
