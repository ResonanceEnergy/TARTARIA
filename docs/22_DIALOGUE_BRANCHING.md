# TARTARIA WORLD OF WONDER — Dialogue Branching Tree Specification
## Choice Architecture, Consequence Tracking & Narrative State Management

---

> *"The player never speaks — but their choices echo through every Moon. Silence is not passivity. It is the loudest form of agency."*

**Cross-References:**
- [05_CHARACTERS_DIALOGUE.md](05_CHARACTERS_DIALOGUE.md) — Character profiles, banter library, voice direction
- [18_PRINCESS_ANASTASIA.md](18_PRINCESS_ANASTASIA.md) — Anastasia's 112-line dialogue system, 128-bit bitmask
- [03_CAMPAIGN_13_MOONS.md](03_CAMPAIGN_13_MOONS.md) — 13-Moon campaign structure
- [03A_MAIN_STORYLINE_REWRITE.md](03A_MAIN_STORYLINE_REWRITE.md) — Full storyline with acts & arcs
- [20_QUEST_DATABASE.md](20_QUEST_DATABASE.md) — 184-quest catalog with prerequisites
- [09_TECHNICAL_SPEC.md](09_TECHNICAL_SPEC.md) — ECS architecture, save system

---

## Table of Contents

1. [Dialogue Design Architecture](#1-dialogue-design-architecture)
2. [Choice Taxonomy](#2-choice-taxonomy)
3. [Consequence Tracking System](#3-consequence-tracking-system)
4. [Companion Relationship Model](#4-companion-relationship-model)
5. [Major Choice Points by Moon](#5-major-choice-points-by-moon)
6. [Dialogue Node Specification](#6-dialogue-node-specification)
7. [Branching Flowcharts — Key Sequences](#7-branching-flowcharts--key-sequences)
8. [Ending Variants](#8-ending-variants)
9. [Replay & Recovery Design](#9-replay--recovery-design)
10. [Implementation & Data Format](#10-implementation--data-format)
11. [QA Testing Matrix](#11-qa-testing-matrix)

---

## 1. Dialogue Design Architecture

### Core Principles

Tartaria's dialogue system serves a **silent protagonist** (Elara). The player never selects dialogue lines — instead, they make choices through **actions, object interactions, and binary world-state decisions.** NPCs interpret and react to these choices.

| Principle | Rule |
|---|---|
| **Show, Don't Say** | Player agency comes from doing, not picking dialogue options |
| **Action-is-Dialogue** | Restoring a building before combat = pacifist choice. Fighting first = militant choice. |
| **NPC Interpretation** | Companions verbalize what the player's action "meant" — Milo: "So you're the build-first type, huh?" |
| **Irreversible & Visible** | Major choices change the world visibly. No invisible flags — the player must see the result. |
| **No Wrong Endings** | Every ending is earned and valid. No "bad end" punishments — only different resonances. |

### The Silent Protagonist Advantage

Because Elara never speaks, the dialogue system avoids:
- Player voice acting (costly, breaks immersion if tone mismatches)
- Dialogue wheel awkwardness ("Sarcastic" options that aren't sarcastic)
- Reading speed mismatches
- Translation of player dialogue (major localization savings)

Instead, NPCs carry all dialogue load. The player's "voice" is their behavior:

| Player Action | NPC Interpretation | World Effect |
|---|---|---|
| Restore dome before clearing enemies | "She trusts the resonance to protect her." — Lirael | Dome acts as combat shield next encounter |
| Clear enemies before restoration | "Practical. Giants would've done the same." — Thorne | Extra combat loot, no dome shield |
| Feed orphans before advancing quest | "Your heart's bigger than your strategy." — Milo | Orphan companion loyalty +10 |
| Skip orphan encounter | Lirael falls silent for 3 dialogue cycles | Korath offers alternative adoption later |
| Investigate prophecy stone thoroughly | Anastasia whispers a Gold Mote clue | Hidden quest flag activated |
| Walk past prophecy stone | Nothing — but Milo says "Might've been something there..." | No flag; revisitable |

---

## 2. Choice Taxonomy

### Tier 1: World-Shaping Choices (6 Total — One Per Act)

These are the major narrative forks. Each permanently alters a zone, a companion relationship, or the ending state.

| # | Moon | Choice | Option A | Option B |
|---|---|---|---|---|
| W1 | Moon 2 | **The Cassian Dilemma** | Accept Cassian's help to reach the buried archive | Reject Cassian — find the archive alone |
| W2 | Moon 4 | **Star Fort Allegiance** | Connect the Star Fort to the Resistance grid | Connect the Star Fort to the Council of Echoes |
| W3 | Moon 6 | **Korath's Sacrifice** | Allow Korath to channel the Requiem (he survives, weakened) | Attempt the Requiem yourself (Korath dies, you gain new ability) |
| W4 | Moon 9 | **The Ley Line Split** | Route the continental ley line through the White City (trade power) | Route through the Orphan Colony (humanitarian power) |
| W5 | Moon 11 | **Zereth's Revelation** | Trust Zereth's claim about the true timeline | Reject Zereth — restore the original timeline |
| W6 | Moon 13 | **The Cosmic Reconciliation** | Forgive the Dissonant One | Seal the Dissonant One permanently |

### Tier 2: Companion-Shaping Choices (3–5 Per Companion)

Each companion has a loyalty track with 3–5 binary choice points. These affect:
- Companion ability unlocks (loyalty 3/5 = tier 2 ability, 5/5 = ultimate)
- Companion-specific dialogue in Moon 13 finale
- Companion participation in ending cinematics

### Tier 3: Quest-Level Choices (~30 Total Across All Moons)

Smaller choices within quests. These don't fork the narrative but adjust local outcomes:
- Side quest resolution method (combat vs. diplomacy vs. puzzle)
- Resource allocation (help village A or village B)
- Mini-game approach (speed vs. precision vs. style)

### Tier 4: Micro-Choices (Implicit & Continuous)

The game constantly reads player behavior as choices:
- Play order within a Moon (which zone first)
- Time spent with each companion
- Building style (functional vs. aesthetic)
- Combat approach (aggressive vs. defensive)

These feed into a **Player Harmony Profile** (see §3) that subtly adjusts NPC tone.

---

## 3. Consequence Tracking System

### 3.1 State Variables

All choices are tracked as typed flags in the save system:

| Variable Type | Storage | Example |
|---|---|---|
| `WorldChoice` | `uint8` (6 bits) | `W1_CASSIAN = 0\|1` |
| `CompanionLoyalty` | `int8[5]` | `MILO_LOYALTY = -5..+5` |
| `QuestOutcome` | `uint32` bitmask | Bit 17 = "Orphan Train: fed children" |
| `HarmonyProfile` | `float[4]` | `[builder: 0.7, fighter: 0.2, explorer: 0.8, diplomat: 0.6]` |
| `AnastasiaFlags` | `uint128` bitmask | 112 dialogue lines + 13 Golden Motes (per [18_PRINCESS_ANASTASIA.md](18_PRINCESS_ANASTASIA.md)) |

### 3.2 Harmony Profile

The Harmony Profile is a 4-axis personality fingerprint that accumulates from Tier 3 & 4 choices:

| Axis | Increased By | Decreased By |
|---|---|---|
| **Builder** | Placing buildings, golden-ratio mastery, architecture exploration | Rushing past unbuilt zones |
| **Fighter** | Combat encounters, combo mastery, enemy-first approach | Avoiding combat, pacifist paths |
| **Explorer** | Hidden quest discovery, lore fragments, zone completion % | Linear quest-only play |
| **Diplomat** | Companion interactions, orphan quests, peaceful resolutions | Skipping dialogue, ignoring NPCs |

NPC dialogue shifts subtly based on dominant axis:
- High Builder: Thorne speaks more about giant architecture
- High Fighter: Korath shares combat memories
- High Explorer: Anastasia whispers more frequently
- High Diplomat: Milo opens up earlier about his past

### 3.3 Consequence Visibility

| Choice Tier | Visibility | Example |
|---|---|---|
| **World-Shaping** | Immediate + Permanent | Star Fort glows blue (Resistance) or gold (Council) |
| **Companion** | 1–3 Moon delay | Milo starts sharing personal past (loyalty 3+) |
| **Quest** | Immediate local | Village A rebuilt vs. Village B rebuilt |
| **Micro** | Gradual NPC tone shift | "You're always building..." — Lirael (high Builder score) |

---

## 4. Companion Relationship Model

### 4.1 Loyalty Tracks

Each companion has a linear loyalty track (-5 to +5) with specific trigger points:

| Level | State | Effect |
|---|---|---|
| **-5 to -3** | Hostile | Companion reduces dialogue frequency; may refuse to join zones |
| **-2 to -1** | Distrustful | Reduced banter; no personal revelations |
| **0** | Neutral | Default state at first meeting |
| **+1 to +2** | Friendly | Personal stories begin; combat synergy unlocked |
| **+3** | Trusted | Tier 2 companion ability unlocked; backstory revealed |
| **+4** | Devoted | Unique dialogue in Moon 13; companion-specific quest unlocked |
| **+5** | Bonded | Ultimate companion ability; companion-specific ending variant |

### 4.2 Per-Companion Choice Points

#### Milo (5 Choice Points)

| Moon | Trigger | +1 Action | -1 Action |
|---|---|---|---|
| 1 | Milo offers "artifact" for sale | Decline politely | Buy the fake artifact |
| 3 | Orphan Train: Milo hoards supplies | Confront him directly | Ignore the hoarding |
| 5 | White City: Milo finds genuine Tartarian relic | Let him keep it | Take it for the archive |
| 7 | Korath's crisis: Milo panics | Reassure him | Tell him to be useful or leave |
| 10 | Train ride: Milo reveals his real name | Listen silently (don't react) | Ask for more (he clams up) |

#### Lirael (5 Choice Points)

| Moon | Trigger | +1 Action | -1 Action |
|---|---|---|---|
| 2 | Lirael sings to calm corrupted water | Sit and listen | Interrupt to purify faster |
| 3 | Orphan adoption: Lirael chooses Aria | Support her choice | Suggest a different child |
| 6 | Lirael's fear of the Deep Forge | Stay with her | Send her back to safety |
| 9 | Lirael discovers she's an Echo, not alive | Hold silence (let her process) | Try to comfort her verbally |
| 12 | Lirael asks to stay behind in the Ley Line | Agree (she gains power) | Refuse (she stays mortal) |

#### Korath (4 Choice Points)

| Moon | Trigger | +1 Action | -1 Action |
|---|---|---|---|
| 1 | Korath tests your combat resolve | Fight honorably (no cheap shots) | Use dishonorable tactics |
| 4 | Korath remembers his fallen city | Ask about his family | Change the subject |
| 6 | Korath offers to teach giant combat | Accept training (full cutscene) | Decline — too busy |
| 7 | Korath's sacrifice moment | Trust his judgment | Override his decision |

#### Thorne (4 Choice Points)

| Moon | Trigger | +1 Action | -1 Action |
|---|---|---|---|
| 5 | Thorne explains radio technology | Show genuine curiosity | Dismiss it as "old tech" |
| 8 | Thorne's airship needs critical repair | Help repair manually | Rush repair with Aether shortcut |
| 10 | Thorne reveals why he left his people | Respect his privacy | Press for details |
| 12 | Thorne asks to fly one final sortie | Fly with him | Bench him for safety |

#### Anastasia (Special — See [18_PRINCESS_ANASTASIA.md](18_PRINCESS_ANASTASIA.md))

Anastasia does not use the standard loyalty system. Her relationship is tracked via the 128-bit bitmask (112 dialogue lines + 13 Golden Motes). There are no negative actions — only discovery and silence. Her "loyalty" is the count of unique lines triggered.

| Discovery Count | Relationship State |
|---|---|
| 0–20 | Invisible (player may not know she exists) |
| 21–50 | Occasional golden shimmer at the edge of view |
| 51–80 | Reactive whispers during emotional moments |
| 81–100 | Conversational mode — full sentences, wisdom |
| 101–112 | Moon 13: fully solid for 10 seconds, speaks her name |

---

## 5. Major Choice Points by Moon

### Moon-by-Moon Choice Distribution

| Moon | World Choice | Companion Choices | Quest Choices | Total |
|---|---|---|---|---|
| 1 (Magnetic) | — | Milo #1, Korath #1 | 2 | 4 |
| 2 (Lunar) | W1: Cassian | Lirael #1 | 2 | 4 |
| 3 (Electric) | — | Milo #2, Lirael #2 | 3 | 5 |
| 4 (Self-Existing) | W2: Star Fort | Korath #2 | 2 | 4 |
| 5 (Overtone) | — | Milo #3, Thorne #1 | 3 | 5 |
| 6 (Rhythmic) | W3: Korath | Lirael #3, Korath #3 | 2 | 5 |
| 7 (Resonant) | — | Milo #4, Korath #4 | 2 | 4 |
| 8 (Galactic) | — | Thorne #2 | 3 | 4 |
| 9 (Solar) | W4: Ley Line | Lirael #4 | 2 | 4 |
| 10 (Planetary) | — | Milo #5, Thorne #3 | 2 | 4 |
| 11 (Spectral) | W5: Zereth | — | 3 | 4 |
| 12 (Crystal) | — | Lirael #5, Thorne #4 | 2 | 4 |
| 13 (Cosmic) | W6: Dissonant One | — | 2 | 3 |
| **Totals** | **6** | **22** | **30** | **58** |

---

## 6. Dialogue Node Specification

### 6.1 Node Types

| Type | Code | Description | Example |
|---|---|---|---|
| **Standard** | `STD` | NPC speaks; player continues automatically | Milo's idle banter |
| **Reaction** | `RXN` | NPC reacts to player action (no choice) | Lirael gasps when dome activates |
| **Action Choice** | `ACT` | Player chooses an action (not dialogue) | "Restore dome" vs. "Fight enemy first" |
| **World Gate** | `WGT` | Major fork with permanent consequences | Star Fort allegiance |
| **Conditional** | `CND` | Dialogue varies based on state flags | Milo references Moon 2 events differently based on W1 |
| **Whisper** | `WSP` | Anastasia-specific; triggered by proximity + bitmask | Golden shimmer + ambient text |
| **Ambient** | `AMB` | Environmental dialogue (no speaker) | "A hum rises from below..." |

### 6.2 Node Data Schema

```json
{
  "id": "M03_MILO_ORPHAN_CONFRONT",
  "type": "ACT",
  "moon": 3,
  "speaker": "MILO",
  "prerequisite": {
    "quest": "M3-MS03",
    "flag": "ORPHAN_TRAIN_ACTIVE"
  },
  "context_text": "Milo's satchel bulges with rations meant for the orphans.",
  "options": [
    {
      "action": "CONFRONT",
      "description": "step_toward_milo",
      "result_text": "Milo freezes. Slowly opens the satchel. 'Yeah... I know. Old habits.'",
      "effects": {
        "MILO_LOYALTY": "+1",
        "flag_set": "MILO_HONEST_M3"
      }
    },
    {
      "action": "IGNORE",
      "description": "turn_away",
      "result_text": "Milo clutches the satchel tighter. Says nothing. Lirael watches.",
      "effects": {
        "MILO_LOYALTY": "-1",
        "LIRAEL_LOYALTY": "-1",
        "flag_set": "MILO_HOARDED_M3"
      }
    }
  ],
  "timeout_action": "IGNORE",
  "timeout_seconds": 15
}
```

### 6.3 Timeout Behavior

If the player doesn't act within the timeout window (context-dependent, 10–30 seconds):
- The system selects a default action (usually passive/neutral)
- NPC reacts to inaction: "Guess that's a 'no,' then." — Milo
- This prevents blocking quest progression while still counting as a choice

---

## 7. Branching Flowcharts — Key Sequences

### 7.1 The Cassian Dilemma (W1 — Moon 2)

```
                    ┌─── [ACCEPT CASSIAN] ──────────────────┐
                    │                                        │
[BURIED ARCHIVE     │   Cassian guides to archive            │
 DISCOVERED]────────┤   Shortcut: skip 2 combat encounters   ├──→ [ARCHIVE REACHED]
                    │   Cassian plants tracking device        │        │
                    │   Zereth aware of player location       │        │
                    │                                        │        ▼
                    │                                   [Moon 5: Cassian returns]
                    │                                        │
                    │                                   ┌────┴────┐
                    │                                   │ TRUST?   │
                    │                                   │  Y → ally│
                    │                                   │  N → flee│
                    │                                   └─────────┘
                    │
                    └─── [REJECT CASSIAN] ──────────────────┐
                        │   Cassian: "You'll regret this."   │
                        │   Harder path: 3 extra encounters   ├──→ [ARCHIVE REACHED]
                        │   No tracking device                │        │
                        │   Zereth unaware until Moon 7       │        ▼
                        │                                     │   [Moon 5: Cassian absent]
                        └─────────────────────────────────────┘
```

### 7.2 Korath's Sacrifice (W3 — Moon 6)

```
[CYMATIC REQUIEM SEQUENCE]
        │
        ▼
[Korath volunteers to channel the pipe organ's cosmic frequency]
        │
   ┌────┴────────────────────────────┐
   │                                  │
[ALLOW KORATH]                    [ATTEMPT YOURSELF]
   │                                  │
   ▼                                  ▼
Korath channels successfully      Player channels — overwhelmed
Korath survives but weakened      Korath intercepts — absorbs
  → Lost giant combat ability       fatal backlash
  → Gains "Elder Resonance"        → Korath dies
    (passive RS boost)              → Player gains "Requiem Echo"
  → Moon 7: recovers slowly          (devastating AoE ability)
  → Moon 13: stands beside you     → Moon 7: memorial scene with Milo
                                   → Moon 13: Korath's ghost appears
```

### 7.3 The Cosmic Reconciliation (W6 — Moon 13)

```
[DISSONANT ONE DEFEATED — CHOOSING COSMIC FATE]
        │
   ┌────┴────────────────────────────────────┐
   │                                          │
[FORGIVE]                                 [SEAL]
   │                                          │
   ▼                                          ▼
Dissonant One's frequency                 Dissonant One sealed
  absorbed into the grid                    in eternal dissonance
  → World: golden aurora (warm)            → World: silver aurora (stark)
  → Companions: celebrate together         → Companions: solemn victory
  → Anastasia: smiles (warm gold)          → Anastasia: nods (cool gold)
  → Post-credit: new harmony frequency     → Post-credit: sealed vault hums
  → Day Out of Time: festival of unity     → Day Out of Time: memorial march
```

---

## 8. Ending Variants

### 8.1 Ending Matrix

The ending cinematic (Moon 13 post-climax) varies based on accumulated choices:

| Factor | Weight | Variants |
|---|---|---|
| W6 (Forgive/Seal) | 40% | Warm ending vs. Stark ending |
| Companion loyalty aggregate | 30% | Full gathering vs. partial gathering |
| Korath alive/dead (W3) | 15% | Korath present vs. ghost |
| Zereth trusted/rejected (W5) | 10% | True timeline glimpse vs. sealed history |
| Anastasia discovery % | 5% | Solid Anastasia vs. faint shimmer |

### 8.2 Four Core Endings

| Ending | Requirements | Cinematics |
|---|---|---|
| **The Eternal Harmony** | Forgive + all companions loyal + Korath alive | Full companion gathering; Anastasia solid; aurora song; golden age begins |
| **The Solemn Restoration** | Forgive + mixed loyalty + Korath dead | Partial gathering; Korath's ghost salutes; bittersweet wonder |
| **The Sealed Resonance** | Seal + high loyalty + Korath alive | Companions surround sealed vault; Korath stands guard; determined resolve |
| **The Lone Conductor** | Seal + low loyalty + Korath dead | Elara stands alone on the highest spire; single bell rings; austere beauty |

**Key Design Rule:** All four endings feel EARNED and COMPLETE. There is no "bad" ending — only different resonances. The Lone Conductor is as valid and moving as The Eternal Harmony.

### 8.3 Post-Ending Content

After any ending, the player:
- Unlocks Day Out of Time festival (full sandbox)
- Can replay any Moon with choices preserved
- New Game+ available with all companion abilities retained
- Ending selection screen lets players view all four endings once any one is achieved

---

## 9. Replay & Recovery Design

### 9.1 Moon Replay System

After Moon 13, players can replay any Moon:
- All choices reset for that Moon only
- World state for that Moon reverts, other Moons unchanged
- Allows experiencing both paths without full restart
- Quest rewards already earned are not re-granted (codex entries still track)

### 9.2 Choice Preview

Before any Tier 1 (World-Shaping) choice:
- Screen dims slightly, atmospheric sound shifts
- A subtle golden pulse radiates from the choice point
- No explicit "THIS IS A BIG CHOICE" UI — but the atmosphere signals weight
- Post-choice, the consequence is immediately visible (building changes, NPC reacts)

### 9.3 No Permanently Missable Content

| Content Type | Recovery Method |
|---|---|
| Main story choices | Moon Replay |
| Companion loyalty | Replay companion's Moon + make different choice |
| Hidden quests | Always available — no time gates on hidden content |
| Anastasia's Golden Motes | Persist across replays — once found, always found |
| Codex entries | Cumulative across all playthroughs |

---

## 10. Implementation & Data Format

### 10.1 Dialogue Asset Format

All dialogue is stored as **JSON dialogue packs** per Moon, loaded via Addressables:

```
Addressables/
├── Dialogue/
│   ├── Moon01_Dialogue.json     (~120 nodes)
│   ├── Moon02_Dialogue.json     (~140 nodes)
│   ├── Moon03_Dialogue.json     (~160 nodes)
│   ├── ...
│   ├── Moon13_Dialogue.json     (~200 nodes)
│   ├── Anastasia_Whispers.json  (~112 nodes)
│   └── Ambient_Environmental.json (~80 nodes)
```

### 10.2 ECS Integration

| Component | Data | System |
|---|---|---|
| `DialogueStateComponent` | Current active node ID, speaker, timeout timer | `DialogueProgressionSystem` |
| `ChoiceTrackingComponent` | WorldChoice bits, CompanionLoyalty array, QuestOutcome bitmask | `ChoiceTrackingSystem` |
| `HarmonyProfileComponent` | 4-axis float array | `HarmonyProfileUpdateSystem` |
| `AnastasiaStateComponent` | 128-bit bitmask (per [18_PRINCESS_ANASTASIA.md](18_PRINCESS_ANASTASIA.md)) | `AnastasiaPresenceSystem` |

### 10.3 Estimated Node Counts

| Content | Node Estimate |
|---|---|
| Main storyline dialogue (13 Moons) | ~1,800 nodes |
| Companion banter (5 companions) | ~600 nodes |
| Side quest dialogue | ~400 nodes |
| Anastasia whispers | 112 nodes |
| Environmental ambient | ~80 nodes |
| DLC dialogue (10 packs) | ~1,000 nodes |
| **Total** | **~4,000 nodes** |

---

## 11. QA Testing Matrix

### 11.1 Critical Path Coverage

Every World-Shaping choice must be tested in both states × all companion loyalty combinations:

| Test Case | Count |
|---|---|
| 6 World Choices × 2 options | 12 |
| 12 combinations with Korath alive/dead | 24 |
| 4 ending variants | 4 |
| 5 companions × loyalty states | 15 |
| **Minimum critical paths** | **55** |

### 11.2 Regression Tests

| Test | Trigger | Expected |
|---|---|---|
| Choice persistence after save/load | Save mid-choice, load | Same choice state restored |
| Companion loyalty across Moon boundaries | Complete Moon with loyalty changes | Loyalty persists into next Moon |
| Anastasia bitmask integrity | Trigger dialogue, save, reload | Same bits set |
| Timeout defaults | AFK during Tier 1 choice | Default action applied, NPC reacts |
| Conditional dialogue accuracy | W1=accept, reach Moon 5 | Cassian appears with tracking reference |
| Moon Replay state isolation | Replay Moon 3, change choice | Moon 4+ unaffected |

### 11.3 Localization Verification

Every dialogue node must be verified in all target languages for:
- Text overflow (2-sentence mobile limit per bubble)
- Pronoun consistency (silent protagonist = no gendered references)
- Cultural sensitivity (see [23_LOCALIZATION.md](23_LOCALIZATION.md))
- Lip-sync placeholders (if VO added later)

---

*Every choice is a frequency. Every consequence is a resonance. The player conducts their own ending.*

---

**Document Status:** DRAFT
**Author:** Nathan / Resonance Energy
**Last Updated:** March 24, 2026
