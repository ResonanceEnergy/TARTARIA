# TARTARIA WORLD OF WONDER — Tutorial & Onboarding Design
## Minute-by-Minute Tutorial Flow, Teaching Philosophy & First-Session Script

---

> *"The best tutorial is one the player never notices. They think they're exploring. They think they're making choices. They don't realize they've been taught everything."*

**Cross-References:**
- [07_PC_UX.md §5](07_PC_UX.md) — Onboarding section, session design
- [15_MVP_BUILD_SPEC.md](15_MVP_BUILD_SPEC.md) — 15-minute vertical slice demo, Echohaven scope
- [26_LEVEL_DESIGN.md](26_LEVEL_DESIGN.md) — Echohaven POI map, demo script
- [06_COMBAT_PROGRESSION.md](06_COMBAT_PROGRESSION.md) — Combat primer, frequency system
- [05_CHARACTERS_DIALOGUE.md](05_CHARACTERS_DIALOGUE.md) — Milo as tutorial companion
- [04_ARCHITECTURE_GUIDE.md](04_ARCHITECTURE_GUIDE.md) — Building placement primer
- [13_MINI_GAMES.md](13_MINI_GAMES.md) — Mini-game first-encounter design
- [24_ACCESSIBILITY.md](24_ACCESSIBILITY.md) — Tutorial accessibility provisions

---

## Table of Contents

1. [Onboarding Philosophy](#1-onboarding-philosophy)
2. [Teaching Framework](#2-teaching-framework)
3. [First 30 Minutes — Beat-by-Beat](#3-first-30-minutes-beat-by-beat)
4. [System Introduction Schedule](#4-system-introduction-schedule)
5. [Failure Recovery & Safety Nets](#5-failure-recovery-safety-nets)
6. [Contextual Hint System](#6-contextual-hint-system)
7. [Returning Player Re-Onboarding](#7-returning-player-re-onboarding)
8. [Accessibility Teaching](#8-accessibility-teaching)
9. [Metrics & Optimization](#9-metrics-optimization)

---

## 1. Onboarding Philosophy

### The Three Laws of Tartarian Teaching

1. **Show, don't tell.** No text walls. No pop-up tutorials. The world demonstrates mechanics through designed situations.
2. **One concept at a time.** Never introduce two systems simultaneously. Each new mechanic gets its own isolated moment.
3. **Succeed first, master later.** The first encounter with any system should be nearly impossible to fail. Mastery comes through repetition and difficulty scaling.

### What We're Fighting Against

| Bad Pattern | Our Alternative |
|---|---|
| "Tap here to move!" overlay | Milo walks ahead; player follows naturally |
| Forced tutorial level | First zone IS the tutorial (Echohaven) |
| Tutorial pop-ups blocking gameplay | Contextual hints that fade if ignored |
| Mandatory text reading | Visual demonstration + optional lore |
| Unskippable tutorial | Returning players can skip (detected via cloud save) |
| Tutorial that frontloads everything | Spread across Moon 1 (28 in-game days) |

### Teaching Completion by Moon

| Moon | % of Systems Taught | New Systems |
|---|---|---|
| **Moon 1** | 75% | Movement, harvesting, companion, combat (basic), restoration, building (basic), first mini-game |
| **Moon 2** | 85% | Ley line glide, advanced combat (combos), lore codex, skill tree |
| **Moon 3** | 95% | Advanced building (golden ratio tuning), economy management, companion loyalty |
| **Moon 4+** | 100% | Final systems: star forts, rail travel, advanced mini-games |

---

## 2. Teaching Framework

### 2.1 The Four-Step Loop

Every mechanic is taught through a four-step loop:

```
Step 1: OBSERVE    → Player sees the mechanic performed (NPC, environment, cutscene)
Step 2: GUIDED     → Player performs the mechanic with guardrails (simplified version)
Step 3: PRACTICE   → Player performs the mechanic freely (standard version)
Step 4: CHALLENGE  → Player uses the mechanic under pressure (combat, time, complexity)
```

### 2.2 Example: Teaching Frequency Combat

| Step | Implementation | Time |
|---|---|---|
| **OBSERVE** | Milo throws a stone at a Mud Golem; it flashes red, Golem staggers. Milo says: "See that? They're tuned to a frequency. Match it, and they shatter." | 15 sec |
| **GUIDED** | Single Golem appears. It glows red. Only one attack option (174 Hz tap). Player taps. Golem shatters. "That's it! You're a natural." | 20 sec |
| **PRACTICE** | Two Golems appear — one red (174 Hz), one yellow (396 Hz). Player must match each. No time pressure. | 45 sec |
| **CHALLENGE** | Three Golems with different frequencies + one Dissonant Golem. Combat timer. Combo system active. | 90 sec |

### 2.3 Teach Points

Each teachable mechanic has a defined "teach point" — a specific moment in the game where it's introduced:

| Mechanic | Teach Point | Location | Trigger |
|---|---|---|---|
| Walking/camera | Game start | Echohaven entrance | Automatic |
| Aether harvesting | Minute 1 | First Aether node | Walk near node |
| Companion interaction | Minute 3 | Milo's Campfire | Arrive at campfire |
| Basic combat | Minute 5 | Corruption Patch Alpha | Enter corruption zone |
| Frequency matching | Minute 6 | Same encounter | Second enemy appears |
| First restoration | Minute 9 | Great Dome approach | Quest objective |
| Building placement | Minute 12 | Builder's Terrace | Quest objective |
| Mini-game (tuning) | Minute 7 | Tuning Fork Station | Optional path |
| Dodge mechanic | Moon 1, Day 3 | Second combat zone | Elite enemy |
| Combo system | Moon 1, Day 7 | Third combat zone | Multi-enemy encounter |
| Ley line glide | Moon 2, Day 1 | First ley line node | Zone transition |
| Skill tree | Moon 1, Day 5 | Level-up event | First level up |
| Golden ratio tuning | Moon 3, Day 1 | First advanced building | Quest objective |
| Giant Mode | Moon context | Story boss encounter | Narrative trigger |

---

## 3. First 30 Minutes — Beat-by-Beat

### MINUTE 0:00 — The Awakening

```
[BLACK SCREEN → FADE IN]

Camera: Slow aerial pan over Echohaven valley. Mud-covered landscape.
Half-buried golden dome visible in the distance.
A single point of blue light — Elara — appears.

Text (center screen, fading): "The world remembers what it was."

Camera descends to ground level behind Elara.
Player gains control. No prompt. No overlay.
```

**Teaching:** Nothing. Let the player absorb the world. Touch the screen to look around.

### MINUTE 0:30 — First Movement

```
Milo appears ahead (20m), waving.
He walks forward slowly, turns back, waits.
If player doesn't move after 10 seconds:
  → Milo: "Hey! Over here!" (subtitle + companion indicator arrow)
If player still doesn't move after 20 seconds:
  → Subtle directional prompt fades in at bottom of screen
```

**Teaching:** Movement via social cue (follow the companion), not instruction.

### MINUTE 1:00 — First Harvest

```
Path to Milo passes a glowing Aether vent (ground glow, soft hum).
Player walks near it.
  → Blue particles drift toward Elara
  → +5 Aether Essence (small HUD counter fades in for first time)
Milo: "Aether! The land still has some life in it."
```

**Teaching:** Aether harvesting = proximity. HUD introduced passively.

### MINUTE 1:30 — Path Design Teaching

```
Path forks:
  LEFT: Obvious, wide, lit → leads to Milo's campfire
  RIGHT: Narrow, overgrown, dim → leads to 2 more Aether nodes

No prompt. Player chooses.
  → Reward curiosity (right path gives more Aether)
  → Main path (left) always works
```

**Teaching:** Exploration is rewarded. There's no wrong choice.

### MINUTE 3:00 — Meet Milo (Companion Introduction)

```
Arrive at campfire. Milo sits down.
Milo: "You're not from around here, are you? That's okay. 
       Neither am I. Well — not anymore."

Companion card slides in (portrait, name, "Archive Echo" label).
Milo stands, gestures toward corruption zone to the east.
Milo: "That mud? It's not natural. And those things in it... 
       they don't like visitors."
```

**Teaching:** Companions exist, they talk, they guide. Silent protagonist established (Elara doesn't speak; Milo interprets her presence).

### MINUTE 5:00 — First Combat

```
Walk toward corruption zone. Ground transitions from grass to mud.
Screen edge darkens slightly. Music shifts (Layer 3 fades in).

Single Mud Golem rises from the ground (slow, non-threatening).
It glows RED (174 Hz).

Milo: "See that glow? Everything here vibrates at a frequency.
       Match it and strike!"

[One button appears on screen: Red circle, pulsing]
Player taps.
  → Elara strikes with 174 Hz resonance
  → Golem shatters into light particles
  → +10 AE, musical chime

Milo: "Ha! You're already better at this than me."
```

**Teaching:** OBSERVE + GUIDED steps for combat. Single-button, impossible to fail.

### MINUTE 6:00 — Second Combat (Two Frequencies)

```
Two Golems rise:
  → One RED (174 Hz)
  → One YELLOW (396 Hz)

Two attack buttons appear. Color-matched to enemies.
Player defeats both in any order.

Milo: "Different frequencies for different foes. 
       You'll get the feel for it."
```

**Teaching:** PRACTICE step. Multiple frequencies, still low-stress.

### MINUTE 7:00 — First Mini-Game (Optional)

```
Path passes a Tuning Fork Station (glowing, musical tones).
Approaching triggers:
  → Tuning fork hums
  → Milo: "Hey, try touching that. Sounds interesting."

Mini-game: Simple tuning challenge (match one note).
Bronze (★) threshold is very generous.
  → Reward: +20 AE + "Tuner" title

If player walks past: no penalty. Can return anytime.
```

**Teaching:** Mini-games exist, are optional, reward participation.

### MINUTE 9:00 — First Restoration

```
Arrive at Great Dome. Massive mud-covered structure.
Milo: "Look at that dome. Buried but not broken. 
       Can you feel the Aether calling from inside?"

Quest marker (subtle golden glow on dome surface).
Player taps dome.
  → Restoration mini-game begins:
    - Swipe to clear mud (simple gesture, large target)
    - Golden stone appears beneath
    - Each swipe reveals more architecture
    - Final swipe → dome gleams, light burst, music crescendo

+50 AE, +25 RS, screen shakes, Milo cheers.
```

**Teaching:** GUIDED restoration. The core fantasy of the game delivered within 10 minutes.

### MINUTE 12:00 — First Building

```
Quest leads to Builder's Terrace (flat area with snap grid visible).
Milo: "This place used to be full of buildings. 
       Think you could put something here?"

Building UI slides up:
  → One building available: Small Antenna (simplest)
  → Snap points highlighted
  → Player drags to position, releases to place
  → Golden ratio guide appears (green = good alignment)

Building placed → +30 AE, +15 RS, Milo applauds.
```

**Teaching:** GUIDED building. One option, clear snap points, generous golden ratio threshold.

### MINUTE 14:00 — The Dome Interior (Emotional Payoff)

```
Restored dome opens. Player enters.
Interior: vast golden space, light streaming through oculus.
Ley line nexus pulses at center.

Milo (quiet): "They built this, you know. The Tartarians. 
               It's still here... waiting for someone to care."

Camera slowly pans up through the dome.
Ley line activates → fast travel unlocked (for Moon 2).

[Title card fades in]: TARTARIA — WORLD OF WONDER

End of tutorial sequence.
```

**Teaching:** Emotional hook. This is what restoration FEELS like. This is why they'll keep playing.

---

## 4. System Introduction Schedule

### Moon 1 (Days 1–28): Foundation

| Day | System Introduced | Context | Complexity |
|---|---|---|---|
| 1 | Movement, camera, Aether harvesting | Echohaven entrance | Trivial |
| 1 | Companion interaction (Milo) | Campfire meeting | Simple dialogue |
| 1 | Basic combat (single frequency) | Corruption Patch | 1 button |
| 1 | Building restoration | Great Dome | Swipe gesture |
| 1 | Basic building placement | Builder's Terrace | 1 building, snap grid |
| 2 | Resonance scanning (tap-hold reveal) | Buried structure near dome | Hold to reveal hidden architecture |
| 3 | Dodge mechanic | Second combat zone | Visual telegraph → swipe |
| 5 | Level up + skill tree | First level-up event | Choose 1 of 2 skills |
| 7 | Combo attacks | Multi-enemy encounter | Chain 2–3 hits |
| 8 | Mini-game: Harmonic Rock Cutting | Building material quest | Trace frequency lines to cut stone |
| 10 | Inventory management | Resource cache discovery | View items, equip cosmetic |
| 14 | Companion affinity (choices) | Milo story beat | First companion choice |
| 20 | Quest journal | Third quest acquired | Navigate quest list |
| 28 | Moon summary / zone complete | Moon 1 climax | Review progress |

### Moon 2 (Days 29–56): Expansion

| Day | System Introduced | Context |
|---|---|---|
| 29 | Ley line glide traversal | First ley line node |
| 31 | Second companion (Lirael) | Story introduction |
| 33 | Dissonance Detection (Lens tool) | Crystal Veil corruption zone — Lirael demonstrates the lens |
| 35 | Advanced combat (3+ frequencies) | Crystal Veil enemies |
| 38 | Corruption Purge (3-stage: identify/isolate/purify) | First dissonance crystal node |
| 40 | Lore codex | Dense lore zone |
| 42 | Micro-Giant Mode (shrink into fractal interiors) | Corrupted dome interior — Milo triggers size shift |
| 44 | Mini-game: Micro-Giant Fractal Purge | Inside corrupted dome — purge parasites at micro scale |
| 45 | Aether band differentiation | Etheric band nodes |
| 50 | Mini-game: Bell Tower Synchronization | Crystal Veil bell tower — ring sequence to purify zone |
| 53 | Charged harmonic burst (hold + release combat) | Elite enemy encounter |
| 56 | Companion switching | Two companions available |

### Moon 3 (Days 57–84): Mastery

| Day | System Introduced | Context |
|---|---|---|
| 57 | Golden ratio tuning (advanced building) | Ironhold architecture |
| 60 | Spectral Echo Adoption (orphan echo system) | First orphan echo encounter — narrative choice |
| 63 | Service Buff Multiplier | Adopted echo provides passive zone buff |
| 65 | Skill tree branching | Enough points for choice |
| 68 | Mini-game: Resonance Rail Alignment | Ironhold rail junction — align frequency to reactivate track |
| 70 | Economy management (trading) | NPC merchant |
| 73 | Cymatic ability (draw pattern combat) | New combat ability unlock — trace sigil pattern |
| 80 | Star fort construction | Ironhold quest |

### Moon 4+ : All Remaining

| System | Moon | Context |
|---|---|---|
| Rail cart travel | Moon 4 | Skyfire Plateau connector |
| Airship fast travel | Moon 5 | Skyfire Plateau dock |
| Boat traversal | Moon 6 | Sunken Gardens |
| Giant Mode | Moon context | Story trigger |
| World-Shaping choices | Moon 5 (first) | Cassian Dilemma |

---

## 5. Failure Recovery & Safety Nets

### 5.1 First-Encounter Failure Prevention

| System | Safety Net | Escalation if Player Struggles |
|---|---|---|
| **Combat** | First encounter enemies have 50% less HP | If player takes 3 hits without attacking: Milo demonstrates |
| **Building** | First placement auto-snaps to ideal position | If player drags outside bounds: gentle pull-back + Milo hint |
| **Restoration** | First swipe gestures are very forgiving (wide detection) | If player doesn't swipe for 10s: subtle animated arrow |
| **Mini-game** | First mini-game Bronze threshold is 60% (normally 70%) | If player fails: "Try again?" + simplified pattern |
| **Navigation** | Golden route is architecturally obvious | If player is lost >60s: companion calls out direction |

### 5.2 Failure Escalation Ladder

When a player struggles with any mechanic, the hint system escalates:

```
Tier 0: (0-30s)    → Nothing. Let player figure it out.
Tier 1: (30-60s)   → Companion makes contextual comment (non-directive).
                      "Hmm, that corruption looks tough..."
Tier 2: (60-90s)   → Visual hint appears (subtle arrow, glow on target).
Tier 3: (90-120s)  → Explicit hint text appears at bottom of screen.
                      "Tap the red enemy to match its frequency."
Tier 4: (120s+)    → Offer to demonstrate (Milo performs the action).
                      "Want me to show you?"
Tier 5: (2nd fail) → Offer simplified mode for this encounter.
                      "Let's try an easier approach."
```

### 5.3 Death & Retry in Tutorial

**Rule:** The player cannot die during the tutorial (first 15 minutes).

| Event | Player Impact | Behind the Scenes |
|---|---|---|
| Health reaches 0 | Elara stumbles, golden shield flashes | HP set to 25%, enemies knocked back 5m |
| Second "death" | Shield flashes again, enemies visibly weaken | HP set to 50%, enemies lose 30% HP |
| Third "death" | Milo intervenes ("I got this one!") | Milo defeats remaining enemies |

After tutorial (Day 2+): Normal death → respawn at last checkpoint with all progress intact.

---

## 6. Contextual Hint System

### 6.1 Hint Types

| Type | Visual | Trigger | Duration |
|---|---|---|---|
| **Companion Comment** | Speech bubble above companion | Proximity to feature + idle time | 3 seconds |
| **Directional Arrow** | Subtle golden arrow at screen edge | Player facing away from objective >15s | Until player turns |
| **Action Prompt** | Small icon near interactive object | First encounter with new interaction type | One-time |
| **Tutorial Card** | Bottom-screen text card | System menu first opened | Dismissable, 8 seconds auto-hide |
| **Demo Mode** | Companion performs action | Tier 4 failure escalation | Player taps to skip |

### 6.2 Hint Frequency Caps

| Rule | Detail |
|---|---|
| **Max 1 hint per 30 seconds** | Prevent hint overload |
| **Max 3 hints per 5 minutes** | Prevent nagging |
| **Same hint never repeats** | Once shown, that specific hint is marked as seen |
| **Hints disabled during combat** | No distractions while fighting (except combat-specific) |
| **Hints disabled during cutscenes** | Never break the narrative |

### 6.3 Player Control

```
Settings > Gameplay > Hints
├── Tutorial Hints: On / Off
├── Navigation Hints: Always / When Lost / Off
├── Combat Hints: On / First Encounter Only / Off
└── Replay Any Tutorial: [List of all tutorials with replay button]
```

---

## 7. Returning Player Re-Onboarding

### 7.1 Absence Detection

| Absence Duration | Re-Onboarding |
|---|---|
| **<24 hours** | None. Resume normally. |
| **1–7 days** | Milo greeting: "Welcome back! Here's what we were doing..." + quest reminder. |
| **7–30 days** | Brief story recap (3-slide summary of recent events) + quest reminder + control refresh prompt. |
| **>30 days** | Full re-onboarding sequence: story recap + control tutorial replay (optional) + companion status review. |

### 7.2 Story Recap System

```
[RETURNING PLAYER — 14-DAY ABSENCE]

┌──────────────────────────────────┐
│  "While you were away..."        │
│                                  │
│  📖 Moon 3, Day 18               │
│  You're in Crystal Veil          │
│  Restoring the Grand Organ       │
│                                  │
│  Companions:                     │
│  🔵 Milo (Loyalty: 3)           │
│  🟢 Lirael (Loyalty: 2)         │
│                                  │
│  Current Quest:                  │
│  "Harmonize the Crystal Veil"    │
│                                  │
│  [Continue]  [Review Controls]   │
└──────────────────────────────────┘
```

### 7.3 New System Patches

When a game update introduces a new system:

| Condition | Action |
|---|---|
| Player hasn't seen the new system | Milo introduces it during next play session (non-intrusive) |
| New system is major (e.g., Giant Mode rework) | Optional "What's New" card on first post-update launch |
| New system is minor (e.g., UI tweak) | No interruption. Changelog in settings menu. |

---

## 8. Accessibility Teaching

### 8.1 First-Launch Accessibility Prompt

```
[FIRST LAUNCH ONLY — after language selection]

┌──────────────────────────────────┐
│  Before we begin...              │
│                                  │
│  Would you like to adjust        │
│  accessibility settings?         │
│                                  │
│  🎮 Controls & Motor             │
│  👁 Visual & Colorblind          │
│  🔊 Audio & Subtitles            │
│  🧠 Difficulty & Cognitive       │
│                                  │
│  [Customize Now]  [Play First]   │
└──────────────────────────────────┘
```

**"Play First"** skips to game. Accessibility is always available from Settings. Never forced.

### 8.2 Adaptive Difficulty Detection

| Signal | Detection | Response |
|---|---|---|
| Player dies 3× in 5 min | Struggle counter | "Would you like to try Story difficulty?" (one-time prompt) |
| Player takes >2 min on tutorial building | Timing monitor | Auto-snap placement, wider gesture forgiveness |
| Player has screen reader enabled at OS level | Windows Accessibility API check | Auto-enable audio descriptions + subtitle backgrounds |
| Player has Reduce Motion enabled at OS level | Windows Accessibility API check | Auto-disable camera shake, particle reduction |
| Player has large text/DPI scaling | Windows Accessibility API check | Auto-scale in-game UI text |

---

## 9. Metrics & Optimization

### 9.1 Funnel Metrics

| Metric | Target | Red Flag |
|---|---|---|
| **Complete first movement** | 99% | <95% indicates touch issues |
| **Reach Milo's Campfire** | 95% | <90% indicates navigation confusion |
| **Complete first combat** | 92% | <85% indicates combat unclear |
| **Complete first restoration** | 88% | <80% indicates restoration unclear |
| **Place first building** | 85% | <75% indicates building UI issues |
| **Enter restored dome** | 82% | <70% indicates pacing issue |
| **Return for Day 2** (D1 retention) | 45% | <35% indicates weak emotional hook |
| **Complete Moon 1** (D7 retention proxy) | 30% | <20% indicates mid-game drop |

### 9.2 Hint Telemetry

| Event | What We Track | Why |
|---|---|---|
| Hint shown | Which hint, tier level, player location | Identify confusion hotspots |
| Hint interacted | Did player follow the hint? | Measure hint effectiveness |
| Failure before hint | Player struggled without hint triggering | Tune hint thresholds |
| Hint dismissed | Player actively closed hint | Too aggressive? |
| Tutorial skipped | Returning player skipped re-onboarding | Good — they're confident |

### 9.3 A/B Test Candidates

| Test | Variant A | Variant B | KPI |
|---|---|---|---|
| First combat difficulty | Current (50% HP enemies) | Normal HP + auto-dodge | Combat completion rate |
| Hint timing | Tier 1 at 30s | Tier 1 at 60s | Completion rate + hint interaction |
| First building complexity | Single building, auto-snap | Two buildings, manual snap | Building completion rate |
| Emotional payoff timing | Dome at minute 14 | Dome at minute 10 | D1 retention |

---

*The first 15 minutes are everything. If the player feels wonder, they'll stay. If they feel confused, they're gone. Design the tutorial like you're welcoming someone into a beautiful home — not like you're reading them the instruction manual.*

---

**Document Status:** FINAL
**Author:** Nathan / Resonance Energy
**Last Updated:** March 25, 2026
