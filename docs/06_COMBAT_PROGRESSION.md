# TARTARIA WORLD OF WONDER — Combat & Progression Systems
## Resonance Combat, Skill Trees, Giant Mode & Character Growth

---

## Table of Contents

1. [Combat Philosophy](#1-combat-philosophy)
2. [Core Combat System](#2-core-combat-system)
3. [Giant Mode](#3-giant-mode)
4. [Weapons & Resonance Tools](#4-weapons-resonance-tools)
5. [Enemy Bestiary](#5-enemy-bestiary)
6. [Skill Trees](#6-skill-trees)
7. [Cross-Class Synergies](#7-cross-class-synergies)
8. [Progression Economy](#8-progression-economy)
9. [Boss Encounters](#9-boss-encounters)
10. [Difficulty & Accessibility](#10-difficulty-accessibility)

---

## 1. Combat Philosophy

Combat in Tartaria is **harmonic, not violent.** You don't destroy enemies — you restore dissonant frequencies to harmony or repel corrupted forces using resonance technology. This distinction matters:

- **No Blood.** Enemies shatter into mud, dissolve into static, or are purified into Aether light
- **Frequency Over Force.** Match the right frequency to the enemy's weakness — brute force is always suboptimal
- **Building IS Combat.** Restoring a structure mid-fight strengthens your position. Combat and building are inseparable
- **Giant Mode = Power Fantasy.** The 60-second bursts of 15–25 ft height are the "ultimate ability" — rare, spectacular, deeply satisfying
- **Mobile-First.** All combat works with one thumb. Complex combos are optional depth, not requirements

**Design Inspiration:** Zelda's lock-on + Okami's brush strokes + Into the Breach's clarity

---

## 2. Core Combat System

### 2.1 Controls (Touch)

| Input | Action |
|---|---|
| **Tap enemy** | Target lock (auto-face) |
| **Swipe toward enemy** | Resonance blast (ranged) |
| **Swipe across screen** | Dodge roll |
| **Hold + release** | Charged harmonic burst (AoE) |
| **Double-tap ground** | Ground pound (giant mode only) |
| **Draw pattern** | Cymatic ability (unlocked Moon 3+) |
| **Tap building** | Activate defensive structure |

### 2.2 Combat Resources

| Resource | Source | Function |
|---|---|---|
| **Aether Charge** | Regenerates near structures; fills from harvesting | Powers all attacks and abilities |
| **Resonance Combo** | Builds from consecutive matched-frequency hits | Multiplies damage + rewards |
| **Giant Meter** | Fills from kills, restoring, and RS milestones | Activates Giant Mode when full |
| **Harmony Shield** | Passive from nearby high-RS buildings | Damage reduction zone |

### 2.3 Frequency Matching System

Every enemy vibrates at a specific **dissonant frequency.** Finding and countering it is the core skill:

| Enemy Frequency | Counter Frequency | Visual Tell |
|---|---|---|
| **174 Hz (Red)** | 285 Hz (Orange) | Red pulse in enemy's core |
| **285 Hz (Orange)** | 396 Hz (Yellow) | Orange shimmer on limbs |
| **396 Hz (Yellow)** | 528 Hz (Green) | Yellow sparks on impact |
| **528 Hz (Green)** | 639 Hz (Blue) | Green smoke on dodge |
| **639 Hz (Blue)** | 741 Hz (Indigo) | Blue frost on surfaces |
| **741 Hz (Indigo)** | 852 Hz (Violet) | Indigo crystal formation |
| **Dissonant (Black)** | Any frequency in 3-6-9 pattern | No color — absence of light |

**Frequency Switching:** Tap the tuning wheel (corner HUD) to cycle through unlocked frequencies. Experienced players can mid-combo switch for devastating chain attacks.

### 2.4 Combo System

Consecutive correct-frequency hits build a **Resonance Chain:**

| Chain | Multiplier | Bonus Effect |
|---|---|---|
| 3 hits | 1.5× | Minor Aether burst (area heal) |
| 6 hits | 2.5× | Enemies staggered |
| 9 hits | 4.0× | Resonance Overload (massive AoE purification) |
| 12 hits | 6.0× | GOLDEN CASCADE — all enemies in zone stunned, structures gain +10 RS |

Missing a frequency resets the chain. This rewards mastery without punishing new players (can't fail — just lower damage).

---

## 3. Giant Mode

### 3.1 Activation

**Giant Meter** fills from:
- Combat kills (20% per kill)
- Building restoration milestones (30% per major milestone)
- 17th Hour bonus (fills 50% passively)
- Korath's echo proximity (10% passive)

**When full:** Player triggers with a satisfying visual — Aether surges through bloodline markers, bones glow, and the camera pulls back as you grow to 15–25 ft.

(Full Giant Mode and other sections preserved. Moon 2 additions below in Boss section.)

---

## 9. Boss Encounters

### 9.1 Boss Design Principles

1. Every boss teaches a mechanic the player needs for the next Moon
2. Every boss has a **frequency puzzle** phase before the damage phase
3. Every boss has a **narrative moment** where the story pauses for emotional impact
4. No boss is pure evil — corrupted golems were once allies, Reset commanders have motivations

### 9.2 Boss Roster

| Moon | Boss | Arena | Key Mechanic Taught |
|---|---|---|---|
| 1 | **Reset Scout Captain** | Cathedral interior | Target locking + dodge |
| 2 | **Cathedral Vein Warden** (NEW cathedral guardian) + **Fractal Vein Mirror** + **Dissonance Root Core** | Crystalline Caverns / moon2_cathedral_dome + crystal_hall + ley_chamber roots | Micro-Giant navigation + vein frequency purge + Giant/ companion synergy + permanent world catharsis |
| 3 | **Rail Wraith** | Resonance train tracks | Protect mobile objective |
| 4 | **Maelix (Corrupted Golem)** | Star fort courtyard | Giant-mode wrestling |
| 5 | **Reset Demolition Commander** | White City pavilion | Defend building while fighting |
| 6 | **The Broken Symphony** | Cathedral organ chamber | Frequency matching under pressure |
| 7 | **Cassian OR Golem Siege** | Star fort cluster | Choice-driven encounter |
| 8 | **Anti-Aether Warship** | Aerial (on airship) | Mounted cannon combat |
| 9 | **Frequency Phantom King** | Cross-continental | Shifting frequency boss |
| 10 | **The Silence** | Train tunnel | Fight without audio cues |
| 11 | **Sludge Titan** | Aquifer depths | Environmental combat (water) |
| 12 | **Reset Armada** | Multi-zone defense | All defensive systems |
| 13 | **Zereth (Resonance Dialogue)** | Echo Realm | Harmony combat — no violence |

**Moon 2 Exclusive (detailed in 03C + Moon2BossEncounters.cs):** The three memorable cavern climaxes deliver the "living crystal cathedral under threat" fantasy with production telegraphing, phases, freq integration, and permanent payoffs (golden veins, new chambers, grid boosts). Cathedral Vein Warden is the flagship guardian encounter.

### 9.3 Moon 13: Zereth (Final "Boss")

**Not a fight.** A resonance dialogue:

**Phase 1 — Rage:** Zereth fires dissonant blasts. Counter with the lullaby frequency (Moon 1 + Moon 3). Each countered blast calms him slightly.

**Phase 2 — Grief:** Zereth shows memories of his brothers. Play Korath's teaching frequency (Moon 7). Each matched note heals a fracture in his form.

**Phase 3 — Truth:** Zereth opens. Play the organ's frozen note (Moon 6). He hears the harmony he once composed and breaks completely.

**Phase 4 — Choice:** The three paths open. Player decides.

**Design Note:** The "hardest boss" requires no reflexes — only everything the player has learned about harmony, empathy, and listening. It's the most demanding combat encounter in the game because it demands the player play with their heart.

---

## 10. Difficulty & Accessibility

### 10.1 Difficulty Modes

| Mode | Description | Who It's For |
|---|---|---|
| **Wanderer** | Auto-frequency match, enemies slow, giant mode 2× duration | Story lovers, casual players |
| **Resonance** | Standard experience — balanced challenge | Most players |

(Full doc sections preserved from source. Moon 2 bosses added to roster and philosophy.)

---
