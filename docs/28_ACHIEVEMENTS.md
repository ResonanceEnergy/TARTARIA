# TARTARIA WORLD OF WONDER — Achievement & Progression System
## Achievement Taxonomy, Unlock Criteria, Rewards & Seasonal Challenges

---

> *"True achievement is not a trophy on a shelf — it's a civilization rebuilt, one dome at a time."*

**Cross-References:**
- [20_QUEST_DATABASE.md](20_QUEST_DATABASE.md) — 184 quests, quest categories
- [19_ECONOMY_BALANCE.md](19_ECONOMY_BALANCE.md) — Resource rewards, currency flow
- [06_COMBAT_PROGRESSION.md](06_COMBAT_PROGRESSION.md) — Combat mastery milestones
- [08_MONETIZATION.md](08_MONETIZATION.md) — Battle Pass, cosmetic rewards
- [04_ARCHITECTURE_GUIDE.md](04_ARCHITECTURE_GUIDE.md) — Building archetypes, golden ratio
- [13_MINI_GAMES.md](13_MINI_GAMES.md) — ★★★ scoring, mini-game types
- [22_DIALOGUE_BRANCHING.md](22_DIALOGUE_BRANCHING.md) — Choice tracking, companion loyalty, endings
- [21_PLAYER_PERSONAS.md](21_PLAYER_PERSONAS.md) — Persona motivation mapping

---

## Table of Contents

1. [Achievement Philosophy](#1-achievement-philosophy)
2. [Achievement Categories](#2-achievement-categories)
3. [Full Achievement Catalog](#3-full-achievement-catalog)
4. [Hidden Achievements](#4-hidden-achievements)
5. [Challenge Modifiers](#5-challenge-modifiers)
6. [Seasonal Rotating Achievements](#6-seasonal-rotating-achievements)
7. [Reward Structure](#7-reward-structure)
8. [Backend & Storage](#8-backend-storage)
9. [Steam Achievements Integration](#9-steam-achievements-integration)
10. [Design Gaps — Open Items](#10-design-gaps-open-items)

---

## 1. Achievement Philosophy

### Design Principles

1. **Achievements celebrate play style, not grind.** No "Kill 10,000 enemies." Yes, "Defeat a boss using only 528 Hz resonance."
2. **Every persona has their achievements.** Lore Seekers, Builders, Combat fans, Puzzle masters, and Completionists all find achievements that match their motivation (see [21_PLAYER_PERSONAS.md](21_PLAYER_PERSONAS.md)).
3. **No achievements require purchase.** Every achievement is earnable through gameplay alone. Battle Pass cosmetics are rewards, not requirements.
4. **Hidden achievements create discovery moments.** Players share them socially — "Wait, you can DO that?"
5. **Progression, not perfection.** Tiered achievements (Bronze → Silver → Gold) let casual players feel accomplished while giving completionists a target.

### Achievement Counts

| Type | Count | Visibility |
|---|---|---|
| **Standard achievements** | 40 | Always visible in journal |
| **Hidden achievements** | 12 | Revealed on unlock |
| **Seasonal achievements** | 6 per season | Visible during active season |
| **Total (launch)** | **52** | |

---

## 2. Achievement Categories

| Category | Icon | Count | Primary Persona |
|---|---|---|---|
| **Restoration** | 🏛 | 8 | Harmonic Builder |
| **Combat** | ⚔ | 8 | All (varied) |
| **Exploration** | 🗺 | 6 | Lore Seeker |
| **Lore** | 📖 | 6 | Lore Seeker |
| **Building** | 🔨 | 6 | Harmonic Builder |
| **Social** | 👥 | 4 | All |
| **Mini-Games** | 🎵 | 6 | Puzzle Maestro |
| **Campaign** | ⭐ | 8 | Completionist |
| **Hidden** | ❓ | 12 | Completionist |

---

## 3. Full Achievement Catalog

### 3.1 Restoration Achievements

| # | Name | Criteria | Tier | Reward |
|---|---|---|---|---|
| R01 | **First Light** | Restore your first building | — | 50 AE, "Restorer" title |
| R02 | **Dawn of a Civilization** | Restore 10 buildings | Bronze | 200 AE |
| R03 | **Architect of Ages** | Restore 25 buildings | Silver | 500 AE + "Architect" title |
| R04 | **The World Remembers** | Restore 50 buildings | Gold | 1,000 AE + exclusive robe |
| R05 | **Dome Master** | Restore all 5 dome sizes (5m–50m) | Silver | Dome blueprint cosmetic |
| R06 | **Spire Keeper** | Restore all 5 antenna spire variants | Silver | Spire glow cosmetic |
| R07 | **Perfect Proportion** | Achieve 0.98+ golden ratio on any building | Gold | Golden outline effect |
| R08 | **Echohaven Reborn** | Restore 100% of Echohaven | Gold | 2,000 AE + zone banner |

### 3.2 Combat Achievements

| # | Name | Criteria | Tier | Reward |
|---|---|---|---|---|
| C01 | **First Strike** | Win your first combat encounter | — | 30 AE |
| C02 | **Frequency Adept** | Defeat enemies with all 7 frequencies | Bronze | 200 AE |
| C03 | **Chain Reaction** | Achieve a 6-hit combo | Silver | Combo VFX upgrade |
| C04 | **Golden Cascade** | Achieve the maximum 12-hit combo | Gold | 1,000 AE + "Resonant" title |
| C05 | **Untouchable** | Complete a combat encounter without taking damage | Silver | Shield skin cosmetic |
| C06 | **Giant Slayer** | Defeat a boss in Giant Mode | Silver | 500 AE |
| C07 | **Pacifist's Path** | Complete an entire zone without initiating combat (dodge/avoid all) | Gold | "Peacekeeper" title + robe |
| C08 | **Harmony Over Violence** | Defeat the final boss using only 528 Hz (Love frequency) | Gold | Unique aura effect |

### 3.3 Exploration Achievements

| # | Name | Criteria | Tier | Reward |
|---|---|---|---|---|
| E01 | **First Steps** | Visit your second zone | — | 50 AE |
| E02 | **Wanderer** | Visit 5 different zones | Bronze | 200 AE |
| E03 | **Cartographer** | Reveal 100% fog-of-war in any zone | Silver | Map cosmetic frame |
| E04 | **Allseer** | Reveal 100% fog-of-war in ALL zones | Gold | 2,000 AE + "Allseer" title |
| E05 | **Peak Seeker** | Reach the highest point in 5 different zones | Silver | 500 AE |
| E06 | **Depths Diver** | Discover 5 underground chambers | Silver | Lantern cosmetic |

### 3.4 Lore Achievements

| # | Name | Criteria | Tier | Reward |
|---|---|---|---|---|
| L01 | **Curious Mind** | Collect your first lore fragment | — | 30 AE |
| L02 | **Scholar** | Collect 25 lore fragments | Bronze | 200 AE + "Scholar" title |
| L03 | **Archivist** | Collect 50 lore fragments | Silver | 500 AE + codex cosmetic |
| L04 | **Living Library** | Complete the full lore codex | Gold | 2,000 AE + "Living Library" title |
| L05 | **Tartarian Linguist** | Discover all Old Tartarian conlang entries | Silver | Glyph effect cosmetic |
| L06 | **Anastasia's Secret** | Unlock all Anastasia lore fragments | Gold | Unique companion outfit |

### 3.5 Building Achievements

| # | Name | Criteria | Tier | Reward |
|---|---|---|---|---|
| B01 | **Foundation** | Place your first player building | — | 50 AE |
| B02 | **Village Builder** | Place 10 buildings | Bronze | 200 AE |
| B03 | **City Planner** | Place 25 buildings with golden ratio ≥0.90 | Silver | Golden building outline |
| B04 | **Star Fort Commander** | Complete your first star fort | Gold | 1,000 AE + fort banner |
| B05 | **Sacred Geometry Mastery** | Place 5 buildings with golden ratio ≥0.95 in one zone | Gold | Geometry particle effect |
| B06 | **The Grand Design** | Restore + build 100% of any zone | Gold | Zone-specific monument cosmetic |

### 3.6 Social Achievements

| # | Name | Criteria | Tier | Reward |
|---|---|---|---|---|
| S01 | **Fair Exhibitor** | Submit a build to the World's Fair | — | 100 AE |
| S02 | **People's Choice** | Receive 100 votes on a World's Fair build | Silver | Voting ribbon cosmetic |
| S03 | **Master Exhibitor** | Win a World's Fair category | Gold | Trophy cosmetic + 2,000 AE |
| S04 | **Community Spirit** | Vote on 50 World's Fair builds | Bronze | 200 AE |

### 3.7 Mini-Game Achievements

| # | Name | Criteria | Tier | Reward |
|---|---|---|---|---|
| M01 | **Tuned In** | Complete your first mini-game | — | 30 AE |
| M02 | **Bronze Ensemble** | Earn ★ on all 6 mini-game types | Bronze | 300 AE |
| M03 | **Silver Virtuoso** | Earn ★★ on all 6 mini-game types | Silver | Musical note aura |
| M04 | **Gold Maestro** | Earn ★★★ on all 6 mini-game types | Gold | 2,000 AE + "Maestro" title |
| M05 | **Organ Prodigy** | Score 95%+ on an Organ Resonance challenge | Gold | Organ glow cosmetic |
| M06 | **Bell Ringer** | Ring every bell tower in the game | Silver | Bell chime emote |

### 3.8 Campaign Achievements

| # | Name | Criteria | Tier | Reward |
|---|---|---|---|---|
| K01 | **Moonrise** | Complete Moon 1 | — | 100 AE |
| K02 | **Cistern Song** | Complete Moon 2 | — | 150 AE |
| K03 | **Orphan's Train** | Complete Moon 3 | Bronze | 200 AE |
| K04 | **Quarter Moon** | Complete Moon 4 | Bronze | 300 AE |
| K05 | **Bridge Builder** | Complete Moon 5 | Bronze | 350 AE |
| K06 | **Requiem Played** | Complete Moon 6 | Silver | 500 AE |
| K07 | **Half Moon** | Complete Moon 7 | Silver | 750 AE, Moon cosmetic |
| K08 | **Armada Aloft** | Complete Moon 8 | Silver | 800 AE |
| K09 | **Solar Flare** | Complete Moon 9 | Silver | 900 AE |
| K10 | **Planetary Pulse** | Complete Moon 10 | Gold | 1,000 AE |
| K11 | **Veil Lifted** | Complete Moon 11 | Gold | 1,200 AE |
| K12 | **Bell Ring** | Complete Moon 12 | Gold | 1,500 AE |
| K13 | **Full Moon** | Complete Moon 13 (any ending) | Gold | 3,000 AE + ending-specific cosmetic |
| K14 | **Best Friends Forever** | Max loyalty with any companion | Silver | Companion friendship frame |
| K15 | **The Complete Circle** | Max loyalty with ALL 5 companions (across replays) | Gold | Group portrait cosmetic |
| K16 | **Every Path Walked** | See all 4 ending variants (across replays) | Gold | "Eternal Wanderer" title |
| K17 | **Speedrunner's Dream** | Complete Moon 1–13 in under 15 hours | Gold | Speedrun timer cosmetic |

---

## 4. Hidden Achievements

Revealed only upon unlock. Designed for moments of delight and social sharing.

| # | Name | Criteria | Reward |
|---|---|---|---|
| H01 | **Against All Odds** | Defeat a boss with <5% HP remaining | "Against All Odds" title |
| H02 | **The Long Walk** | Walk 100km total distance (tracked cumulatively) | Pilgrim staff cosmetic |
| H03 | **Night Owl** | Play for 30 real-world minutes between 12AM–5AM | Starfield robe cosmetic |
| H04 | **Flower Child** | Find and interact with all hidden flower patches | Flower crown cosmetic |
| H05 | **Milo's Best Joke** | Witness Milo's rarest dialogue line (0.1% chance per interaction) | "Lucky" title |
| H06 | **The Gravity of the Situation** | Jump from the highest point in the game | Featherfall trail effect |
| H07 | **Reverse Engineering** | Destroy a building you placed yourself (hold + deconstruct) | "Overthinker" title |
| H08 | **Frequency Master** | Defeat 7 enemies of 7 different frequencies in a single combat | Rainbow resonance VFX |
| H09 | **Silence Is Golden** | Complete Moon 1 without clicking any dialogue option (listen only) | "Silent" title |
| H10 | **Korath's Respect** | Raise Korath from -5 to +5 loyalty in a single playthrough | Korath's armor cosmetic |
| H11 | **Anastasia's Awakening** | Trigger all 5 of Anastasia's spoken words | Celestial veil cosmetic |
| H12 | **100% Pure Resonance** | Achieve 100 RS in every zone simultaneously | "Perfect Resonance" title + golden aura |

---

## 5. Challenge Modifiers

Post-campaign unlocks for replayability:

### 5.1 Modifier List

| Modifier | Effect | Unlock Condition |
|---|---|---|
| **Ironclad** | No healing items, no auto-heal | Complete Moon 13 |
| **Speed Demon** | All timers halved, enemies attack faster | Complete Moon 13 |
| **Minimalist** | Cannot place buildings — only restore originals | Complete Moon 13 |
| **Purist** | No skill tree upgrades | Complete Moon 7 |
| **Curator** | All lore fragments visible on map — find them all | Collect 50% of lore |
| **Phantom** | Enemies cannot see you — but you also cannot attack | Complete Pacifist's Path |

### 5.2 Modifier Achievements

| # | Name | Criteria | Reward |
|---|---|---|---|
| X01 | **Iron Will** | Complete Moon 1–13 with Ironclad active | "Iron Will" title + armored robe |
| X02 | **Lightning Conductor** | Complete Moon 1–13 with Speed Demon active | "Lightning" crown cosmetic |
| X03 | **True Restorer** | Complete Moon 1–13 with Minimalist active | Original blueprint aura |

---

## 6. Seasonal Rotating Achievements

### 6.1 Season Structure

| Element | Detail |
|---|---|
| **Season length** | 90 days (4 seasons per year) |
| **Achievements per season** | 6 (themed to seasonal live-ops event) |
| **Expiry** | Achievements expire with season; rewards persist |
| **Unlocked retroactively?** | No — must complete during active season |

### 6.2 Example: Season 1 — "Echoes of Spring"

| # | Name | Criteria | Reward |
|---|---|---|---|
| SP01 | **Spring Cleaning** | Restore 5 buildings during the event | Floral building trim |
| SP02 | **Bloom Harvester** | Collect 500 AE from seasonal spring nodes | Cherry blossom aura |
| SP03 | **Garden Party** | Place 3 buildings in the Seasonal Plaza | Garden gate cosmetic |
| SP04 | **Pollinator** | Complete 10 seasonal quests | Bee companion skin (Milo) |
| SP05 | **Perfect Bloom** | Achieve Gold (★★★) on seasonal mini-game | Flower instrument cosmetic |
| SP06 | **Spring Champion** | Complete all 5 seasonal achievements above | Season 1 banner + 1,000 AE |

---

## 7. Reward Structure

### 7.1 Reward Types

| Reward Type | Examples | Source |
|---|---|---|
| **Aether Essence (AE)** | 30–3,000 per achievement | Primary currency |
| **Titles** | "Restorer," "Maestro," "Living Library" | Display under player name |
| **Cosmetics** | Robes, auras, outfits, building trims, instrument skins | Equippable visual changes |
| **VFX Upgrades** | Combo effects, resonance glow, trail effects | Persistent visual upgrades |
| **Companion Outfits** | Themed companion visuals | Per-companion equippable |
| **Frames & Banners** | Profile decoration | Social display |

### 7.2 Reward Distribution by Tier

| Tier | AE Reward Range | Cosmetic Rarity | Titles |
|---|---|---|---|
| **No Tier** (first-time) | 30–100 AE | None | Sometimes |
| **Bronze** | 200–300 AE | Common cosmetic | Rarely |
| **Silver** | 500–750 AE | Rare cosmetic | Often |
| **Gold** | 1,000–3,000 AE | Epic cosmetic | Always |
| **Hidden** | 0 AE (cosmetic only) | Unique cosmetic | Always |

### 7.3 Total AE from All Achievements

| Category | Total AE |
|---|---|
| Restoration (8) | 4,800 |
| Combat (8) | 2,230 |
| Exploration (6) | 2,750 |
| Lore (6) | 2,730 |
| Building (6) | 2,250 |
| Social (4) | 2,300 |
| Mini-Games (6) | 2,330 |
| Campaign (8) | 4,150 |
| Modifiers (3) | 0 (cosmetic only) |
| **Total (standard)** | **~21,540 AE** |
| Seasonal (per season) | ~2,500 AE |

This represents approximately 3 weeks of passive Aether income — meaningful but not economy-breaking.

---

## 8. Backend & Storage

### 8.1 Achievement State

Per [25_SAVE_SYSTEM.md](25_SAVE_SYSTEM.md), achievements are stored in the Meta block:

```json
{
  "meta": {
    "achievements_unlocked": "0b110101...1001",
    "achievement_progress": {
      "R02": { "current": 7, "target": 10 },
      "E03": { "current": 0.85, "target": 1.0, "zone": "echohaven" },
      "M03": { "mini_games_gold": ["tuning", "organ", "bell"] }
    },
    "seasonal_achievements": {
      "season_1": "0b110100",
      "season_1_expiry": "2027-06-15T00:00:00Z"
    },
    "hidden_discovered": "0b000000000011",
    "titles_unlocked": ["restorer", "scholar", "silent"],
    "active_title": "scholar"
  }
}
```

### 8.2 Server Validation

| Achievement Type | Validation |
|---|---|
| **Standard** | Client-tracked, server-verified on cloud sync |
| **Combat-specific** | Server validates kill counts and combat logs against theoretical maximums |
| **Hidden** | Client-only (no server validation — these are fun, not competitive) |
| **Seasonal** | Server validates event participation + timing |
| **Modifier** | Server verifies modifier was active for full campaign duration |

---

## 9. Steam Achievements Integration

### 9.1 Steam Achievements

All 52 standard + hidden achievements synced to Steam:

| Steam Feature | Usage |
|---|---|
| **Achievements** | 1:1 mapping (52 achievements) |
| **Progress** | Achievement progress tracked via Steamworks Stats API |
| **Hidden** | 12 achievements marked as hidden in Steamworks dashboard |
| **Leaderboards** | RS by zone, total RS, speedrun time, World's Fair votes |
| **Global Achievement Stats** | Public completion percentages for all achievements |

### 9.2 Steam Leaderboards

| Leaderboard | Metric | Scope |
|---|---|---|
| **World Resonance** | Total RS across all zones | Global + Friends |
| **Zone Champion: {Zone}** | RS for specific zone (×13) | Global + Friends |
| **Speedrun** | Fastest Moon 1–13 completion | Global |
| **Master Builder** | Highest average golden ratio across all buildings | Global + Friends |
| **Lore Collector** | Total lore fragments found | Global + Friends |

---

*Achievement unlocked: You read the entire document. Reward: the knowledge to build a system that makes players feel like heroes.*

---

## 10. Design Gaps — Open Items

| ID | Category | Issue | Resolution |
|---|---|---|---|
| GAP-01 | H04 Flower Child | Achievement references "hidden flower patches" — no flower patch locations documented | **RESOLVED** — 13-zone flower patch table added to [26_LEVEL_DESIGN.md](26_LEVEL_DESIGN.md) § 8.3 |
| GAP-02 | S01–S04 World's Fair | Social achievement series references a build-submission/voting feature with no spec | **RESOLVED** — Submission & Voting mechanic spec added to [08_MONETIZATION.md](08_MONETIZATION.md) § 7.2.1 |
| GAP-03 | K16 Every Path Walked | References "4 ending variants" but quest DB had no ending-variant quests | **RESOLVED** — Ending Variant Quest Triggers section added to [20_QUEST_DATABASE.md](20_QUEST_DATABASE.md) with trigger conditions, quest dependencies, and post-ending flags |

---

**Document Status:** FINAL
**Author:** Nathan / Resonance Energy
**Last Updated:** March 25, 2026
