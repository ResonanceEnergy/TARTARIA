# TARTARIA WORLD OF WONDER — Princess Anastasia Character Bible
## Archive Echo Companion: Design, Behaviour, Dialogue & Integration Guide

---

> *She does not demand your attention. She earns it through absence.*

**Cross-References:**
- [05_CHARACTERS_DIALOGUE.md](05_CHARACTERS_DIALOGUE.md) — Main cast profiles, dialogue philosophy
- [03C_MOON_MECHANICS_DETAILED.md](03C_MOON_MECHANICS_DETAILED.md) — Moon-by-Moon companion layers
- [03A_MAIN_STORYLINE_REWRITE.md](03A_MAIN_STORYLINE_REWRITE.md) — Main storyline integration
- [17_DAY_OUT_OF_TIME.md](17_DAY_OUT_OF_TIME.md) — Solidification moment, festival farewell
- [11_SCRIPTED_CLIMAXES.md](11_SCRIPTED_CLIMAXES.md) — Climax appearances per Moon
- [07_MOBILE_UX.md](07_MOBILE_UX.md) — HUD companion mode indicator
- [09_TECHNICAL_SPEC.md](09_TECHNICAL_SPEC.md) — ECS archetype, Addressable bundles
- [12_VIVID_VISUALS.md](12_VIVID_VISUALS.md) — Manifestation visual design
- [14_HAPTIC_FEEDBACK.md](14_HAPTIC_FEEDBACK.md) — Haptic sync profile
- [15_MVP_BUILD_SPEC.md](15_MVP_BUILD_SPEC.md) — Phase 2+ scope, golden mote Easter egg
- [appendices/A_GLOSSARY.md](appendices/A_GLOSSARY.md) — Archive Echo, Echo NPC definitions
- [appendices/B_ASSET_REFERENCE.md](appendices/B_ASSET_REFERENCE.md) — Color palette, art reference
- [appendices/C_AUDIO_DESIGN.md](appendices/C_AUDIO_DESIGN.md) — Voice profile, haptic sync

---

## Table of Contents

1. [Character Overview](#1-character-overview)
2. [Visual Design](#2-visual-design)
3. [The Four Modes](#3-the-four-modes)
4. [Silence-First Design Philosophy](#4-silence-first-design-philosophy)
5. [Dialogue System](#5-dialogue-system)
6. [Moon-by-Moon Arc](#6-moon-by-moon-arc)
7. [Relationship with Other Companions](#7-relationship-with-other-companions)
8. [The 112 Dialogue Lines](#8-the-112-dialogue-lines)
9. [The Golden Mote Easter Egg](#9-the-golden-mote-easter-egg)
10. [The 10-Second Solidification](#10-the-10-second-solidification)
11. [Voice Direction](#11-voice-direction)
12. [Animation & VFX Guide](#12-animation-vfx-guide)
13. [ECS Implementation](#13-ecs-implementation)
14. [Accessibility Considerations](#14-accessibility-considerations)
15. [DLC & Live-Ops Extensions](#15-dlc-live-ops-extensions)

---

## 1. Character Overview

### Identity

| Attribute | Value |
|---|---|
| **Full Name** | Princess Anastasia Nikolaevna |
| **Title** | Archive Echo of the Star Chamber |
| **Type** | Archive Echo (distinct from standard Echo NPCs) |
| **Origin** | Pre-Mud Flood Tartarian royalty — preserved in the planetary Archive when the old world fell |
| **Apparent Age** | Late teens / early twenties |
| **True Age** | Several centuries (time has no meaning in an archived state) |
| **First Appearance** | Moon 1, post-first dome restoration |
| **Final Form** | Fully solid for 10 seconds during Day Out of Time |

### What Is an Archive Echo?

Standard Echo NPCs are fragments — memories trapped in Aether, repeating patterns of their former lives. They are spectral footprints.

Anastasia is different. She is an **Archive Echo**: a complete consciousness deliberately preserved in the planetary Archive by Tartarian architects who foresaw the Mud Flood. She is not a ghost. She is not a memory loop. She is a fully aware person who exists as light and resonance instead of matter.

The distinction matters:
- Echo NPCs repeat. Anastasia learns.
- Echo NPCs fade with distance. Anastasia chooses when to be near.
- Echo NPCs cannot touch the physical world. Anastasia can influence Aether flow.
- Echo NPCs don't know they're spectral. Anastasia knows exactly what she is.

### Thematic Role

Anastasia represents **the cost of preservation**. The Tartarian architects saved the knowledge, the architecture, the beauty — but they couldn't save the people whole. Anastasia is the closest thing to a fully saved person, and even she exists as light. Her presence asks the question: *Is being remembered the same as being alive?*

Her arc answers it: **No. But it's close enough to matter.**

---

## 2. Visual Design

### Color Palette

| Element | Hex | RGB | Usage |
|---|---|---|---|
| **Primary Gold** | #FFD700 | 255, 215, 0 | Particle core, dialogue text highlight, mode indicator |
| **Ambient Glow** | #FFF8E1 | 255, 248, 225 | Surrounding aura, ambient light emission |
| **Deep Gold** | #DAA520 | 218, 165, 32 | Architectural interaction glow, lore proximity |
| **Faded Gold** | #FFE082 | 255, 224, 130 | Low-activity state, distant observation |
| **Solid State** (Day Out of Time only) | Human skin tones | — | Chestnut hair, blue-grey eyes, warm skin |

### Form Description

**Default (Echo Form):**
- Translucent humanoid figure composed of golden light particles
- Particles drift gently, like dust motes in a sunbeam
- Silhouette is clearly feminine, regal posture: straight spine, chin slightly elevated, hands relaxed at sides or clasped in front
- Hair appears mid-length and gently moves regardless of wind direction (Aether drift)
- Eyes are the most defined feature — warm gold, expressive, often the only part of her that looks directly at the player
- No feet contact with ground — hovers 2–3 cm above surfaces (except during solidification)
- Translucency varies: 30% opacity (Silent), 50% (Reactive Whisper), 70% (Conversational), 0% (Invisible)

**Clothing (Implied Through Light):**
- Silhouette suggests a high-collared Edwardian-era dress with clean lines
- No specific fabric detail — just the shape implied by how light flows around her form
- A circlet or tiara shape occasionally resolves at the crown of her head when she concentrates (Conversational mode only)
- Small pendant at her throat (her tiara and pendant are the most "solid" parts of her form even in echo state — the artifacts that anchored her to the Archive)

### VFX Specifications

| Effect | Trigger | Duration |
|---|---|---|
| **Manifestation** | First appearance in a zone | 3–5 seconds: particles coalesce from scattered to formed |
| **Idle Drift** | Default state | Continuous: particles gently orbit her center, figure breathes |
| **Mode Shift** | Behaviour mode change | 1.5 seconds: opacity changes, glow radius adjusts |
| **Attention Look** | Player approaches lore/architecture | 0.5 seconds: head turn toward object, subtle brightening |
| **Whisper Glow** | Speaking a line | Duration of line: lips area brightens, aura pulses in rhythm with word cadence |
| **Delight Shimmer** | Player achieves high-RS build or finds Easter egg | 2 seconds: particles spiral upward joyfully |
| **Grief Dim** | Negative story event, corruption nearby | Gradual: warmth drains from palette, gold → pale yellow |
| **Combat Fade** | Combat starts | 2 seconds: particles disperse to edges of screen, opacity → 0% |
| **Solidification** | Day Out of Time peak | See Section 10 |

---

## 3. The Four Modes

Anastasia's behaviour is governed by a four-mode state machine. Only one mode is active at a time. Transitions are smooth and deliberate.

### Mode 1: Silent

**When:** Default mode. Most of the time.

| Property | Value |
|---|---|
| Opacity | 30% |
| Voice | None |
| Position | 5–8 meters behind/beside player |
| Behaviour | Follows at distance, observes, head-tracks player and interesting objects |
| HUD Indicator | Faint golden dot in companion panel |
| Haptic | None |
| Trigger to Exit | Player approaches lore object, building, or NPC; extended idle; specific story trigger |

**Design Intent:** Silent Mode is the baseline. The player should sometimes forget Anastasia is there — and then notice her watching them. That moment of recognition is the entire point.

**Animation Loop:**
- Standing: weight shifts gently, hands clasped or one hand raised to touch her pendant
- Moving: glides without walking animation, particles trail behind
- Observing: subtle head tilts, leaning toward interesting things
- Waiting: looks up at the sky, turns in slow circles as if listening to something the player can't hear

---

### Mode 2: Reactive Whisper

**When:** Player is near lore-significant objects, architecture, or during emotionally charged story moments.

| Property | Value |
|---|---|
| Opacity | 50% |
| Voice | Soft whisper — 1–2 sentences maximum |
| Position | 3–5 meters from player, oriented toward the trigger object |
| Behaviour | Steps closer, looks at the object, may reach toward it without touching |
| HUD Indicator | Pulsing golden dot |
| Haptic | Single gentle pulse when she begins to speak |
| Trigger to Exit | Player moves away from trigger, 30 seconds after last whisper |

**Design Intent:** Reactive Whisper is how Anastasia enriches the world without interrupting it. She notices things the player might miss. Her observations are never instructions — they're invitations to look closer.

**Dialogue Constraints:**
- Maximum 2 sentences per whisper
- Minimum 45 seconds between whispers (cooldown)
- Never repeats the same whisper in a session
- Whispers are positional — she looks at what she's commenting on, not at the player

---

### Mode 3: Conversational

**When:** Extended building sessions, quiet exploration with no threats, specific story triggers, and NPC gathering scenes.

| Property | Value |
|---|---|
| Opacity | 70% |
| Voice | Normal speaking volume, full sentences, may deliver 3–5 lines in a sequence |
| Position | 2–3 meters from player, may walk beside them |
| Behaviour | Relaxed, expressive, may gesture, may interact with environment (Aether manipulation) |
| HUD Indicator | Solid golden dot |
| Haptic | Gentle pulse per line spoken |
| Trigger to Exit | Combat begins, player sprints away, zone transition |

**Design Intent:** Conversational Mode is rare and precious. It occurs when the game's pace slows enough for genuine connection. These are the moments where the player learns who Anastasia is — not a game mechanic, but a person trapped in light.

**Unique Behaviours:**
- May spontaneously comment on the player's building choices (only in Building Mode)
- May share a memory of the pre-Flood world unprompted
- May hum softly (one of 3 melodies, chosen by current zone)
- May approach a restored building and trace its geometry with her hand, leaving faint golden trails

---

### Mode 4: Invisible

**When:** Combat, boss encounters, high-stress sequences, scripted climaxes where she would distract.

| Property | Value |
|---|---|
| Opacity | 0% |
| Voice | None |
| Position | Off-screen (despawned from view, persists in ECS) |
| Behaviour | Completely absent — no visual, audio, or haptic presence |
| HUD Indicator | Empty circle in companion panel (grey) |
| Haptic | None |
| Trigger to Exit | Combat ends, stress event resolves, player returns to non-combat zone |

**Design Intent:** Absence is a feature. Anastasia does not belong in combat — she is an Archive Echo, not a warrior. Her disappearance during violence makes her return afterward more meaningful. The empty space where she stood is its own kind of presence.

**Return Animation:** When Invisible mode ends, she re-manifests over 3 seconds: first a cluster of golden particles at the spot where she was last visible, then they coalesce into her form. She looks around as if re-orienting. Then Silent Mode resumes.

---

### Mode Transition Rules

```
                    ┌──────────────┐
                    │              │
        ┌───────── │    SILENT    │ ─────────┐
        │           │   (default)  │           │
        │           └──────┬───────┘           │
        │                  │                   │
   lore/arch          extended idle        combat
   proximity          + safe zone          begins
        │                  │                   │
        ▼                  ▼                   ▼
┌───────────────┐  ┌──────────────┐  ┌──────────────┐
│               │  │              │  │              │
│   REACTIVE    │──│ CONVERSATION │  │  INVISIBLE   │
│   WHISPER     │  │              │  │              │
└───────────────┘  └──────────────┘  └──────┬───────┘
        │                  │                   │
        │                  │              combat ends
        └──────────────────┘───────────────────┘
                    │
                    ▼
              Back to SILENT
```

**Priority Rules:**
1. Combat ALWAYS triggers Invisible (highest priority)
2. Conversational requires no threats in range AND player velocity < walk speed for >30 seconds
3. Reactive Whisper triggers on proximity to lore/architecture AND cooldown expired
4. Silent is the fallback for all other states

---

## 4. Silence-First Design Philosophy

### The Silence Contract

Anastasia's design is built on a contract with the player: **she will never waste your time.**

| Rule | Enforcement |
|---|---|
| She never speaks first in a new zone | Must wait for player to explore before any whisper |
| She never speaks over other companions | If Milo or Lirael are mid-dialogue, she waits or goes Silent |
| She never speaks during spectacles | Orchestrated events (climaxes, performances) are sacred — she watches |
| She never repeats herself | 112 lines, each delivered once, tracked by bitmask |
| She never explains mechanics | She comments on the world, not on the HUD |
| She never urges the player forward | No "hurry up" lines, no quest markers, no time pressure |

### Why Silence?

The game already has Milo (constant commentary, humor, warmth) and Lirael (wonder, questions, music). Adding a third voice at the same volume would create NPC noise. Anastasia exists in the gaps between the others — the spaces where silence is more powerful than words.

Her rarity makes each line feel earned. A player who hears all 112 lines has explored deeply, built carefully, and spent time in the world. The dialogue is a reward for presence, not a delivery mechanism for content.

### The Paradox

Anastasia is the most important companion in the game and the one who speaks the least. This is intentional. The less she says, the more each word matters. By the time she becomes solid during the Day Out of Time and says "I can feel the ground," the player has spent dozens of hours with a companion who barely spoke — and that single sentence carries the weight of all the silence before it.

---

## 5. Dialogue System

### Technical Architecture

**Bitmask Tracking:**
- 112 unique dialogue lines stored as a 128-bit bitmask (112 used, 16 reserved)
- Each line has a unique ID (0–111) corresponding to a bit position
- When a line is delivered, its bit is set to 1 — it can never trigger again
- Save file stores the bitmask as a hex string (32 characters)
- Bitmask persists across sessions, zones, and game states

**Line Categories:**

| Category | Count | Trigger Type |
|---|---|---|
| Lore Whispers | 39 | Proximity to specific architecture/objects |
| Memory Fragments | 20 | Zone-first-visit or restoration milestone |
| Companion Reactions | 15 | Response to Milo/Lirael/Thorne/Korath dialogue |
| Building Commentary | 13 | Player places building at RS > threshold |
| Story Beats | 12 | Scripted narrative triggers (Moon climaxes, reveals) |
| Personal Reflections | 8 | Extended idle in specific locations |
| Easter Egg Lines | 3 | Golden Mote discovery, rare zone events |
| The Final Line | 1 | Day Out of Time solidification |
| Reserved | 1 | Live-ops / DLC expansion |

### Delivery Rules

| Rule | Detail |
|---|---|
| **Cooldown** | Minimum 45 seconds between any two lines |
| **Session Cap** | Maximum 8 lines per 30-minute session (prevents over-saturation) |
| **Moon Cap** | Maximum 12 lines per Moon (ensures distribution across the campaign) |
| **Priority Queue** | Story Beats > Memory Fragments > Lore Whispers > Companion Reactions > Building Commentary > Personal Reflections |
| **Fallback** | If all available lines for current triggers have been delivered, remain Silent |

---

## 6. Moon-by-Moon Arc

### Moon 1 — Magnetic Moon (Echohaven / New Chicago)

**First Appearance:**
- Anastasia manifests after the player's first dome restoration
- She does NOT appear during the restoration — only afterward, when the dome is glowing and the Aether settles
- Manifestation: golden particles drift from the restored dome's apex, coalesce 10 meters from the player
- She looks at the dome, not the player. Then she turns. Makes eye contact. Blinks once. Turns back to the dome.
- No dialogue. No HUD prompt. No tutorial tooltip.
- If the player approaches, she drifts backward slightly — maintaining distance (Silent Mode)

**Available Lines (Moon 1):** 6
> "This dome. I watched them build it. Ages ago. They sang while they worked."  
> "The stones remember their shape. You just reminded them."  
> "Your resonance is... different. Brighter than what the Archive expected."  
> *(Near Milo)* "He's louder than anyone I've heard in centuries. I think I like it."  
> *(After second dig)* "Careful with the lower strata. That's where the important things sleep."  
> *(Idle near the restored dome at night)* "At night, it almost looks like it used to."

---

### Moon 2 — Lunar Moon (Crystalline Caverns)

**Arc Beat:** Anastasia reacts to subterranean beauty — crystals remind her of the Archive.

**Available Lines (Moon 2):** 7
> "The Archive looks like this. All light and facets. But colder."  
> "These crystals amplify Aether the way a bell amplifies silence."  
> "I've been in a cave of light for so long, I'd forgotten what darkness smells like."  
> *(After crystal tuning)* "You're better at that than the original tuners were. Don't tell them I said so."  
> *(Near Lirael)* "She sings to the crystals. They sing back. I wonder if she knows she's doing it."  
> *(Discovery of deepest crystal chamber)* "This was the heart of their resonance network. The first antenna."  
> *(Idle near crystal cluster)* "If you put your ear very close... no. I suppose you can't hear it."

---

### Moon 3 — Electric Moon (Windswept Highlands)

**Arc Beat:** Wind reminds Anastasia of physical sensation — something she has lost.

**Available Lines (Moon 3):** 8
> "Wind. I remember wind. It's one of the first things you forget when you become light."  
> "The orphan train tracks... those children. They built this route themselves."  
> "Highland architecture is different. Built to bend, not break. Like reeds."  
> *(After turbine restoration)* "The turbines used to power the aqueducts. Water and wind, partners."  
> *(Near Thorne)* "The airship captain carries something heavy. Not cargo."  
> *(Near orphan echo children)* "Children echoes are the cruelest thing about this world. They don't know they're trapped."  
> *(Stormy weather)* "Lightning and Aether were twins once. The architects used both."  
> *(Idle at highland overlook)* "I can see four zones from here. Four incomplete worlds. You'll fix them."

---

### Moon 4 — Self-Existing Moon (Star Fort Bastion)

**Arc Beat:** Anastasia recognizes Star Fort architecture — she lived in one.

**Available Lines (Moon 4):** 9
> "I know this place. Not this specific fort, but this *geometry*. I grew up in one like it."  
> "Star forts weren't military. They were amplifiers. Five points, each resonating at a different frequency."  
> "The moat isn't water anymore. It's liquid corruption. Be careful."  
> *(During Star Fort Siege climax)* "They're trying to destroy what they can't corrupt. That's desperation."  
> *(After siege victory)* "The fort held. Eight hundred years of neglect and it still held."  
> *(Near Korath)* "He remembers the old world better than I do. But he carries it differently — like a wound that taught him something."  
> *(Building within the fort)* "Golden ratio alignments were mandatory in royal architecture. Not for beauty — for function."  
> *(Secret chamber discovery)* "A Star Chamber. Where the Archive was accessed. Where I was... stored."  
> *(Idle on the ramparts)* "Watching the horizon from a star fort. Some things transcend timeline."

---

### Moon 5 — Overtone Moon (Sunken Colosseum)

**Arc Beat:** Anastasia sees a performance hall and remembers audiences, spectacle, shared joy.

**Available Lines (Moon 5):** 9
> "A colosseum! Not for fighting — for resonance performances. The acoustics must be extraordinary."  
> "The seating holds five thousand echo imprints. Five thousand people who watched something beautiful."  
> *(After amphitheater restoration)* "The stage is alive again. I can hear the ghost of applause."  
> *(During overtone experiments)* "Overtones were sacred. The note above the note — the truth behind the truth."  
> *(Near submerged section)* "Water doesn't corrupt — it preserves. What's beneath the surface may be more intact than what's above."  
> *(Near Milo trying to sell amphitheater seats)* "He's trying to sell tickets to an echo audience. I admire the optimism."  
> *(Combat against aquatic corruption)* — *[Invisible — no lines]*  
> *(After combat)* "The corruption in water is different. Slower. Patient. Like venom."  
> *(Idle on the colosseum stage)* "If I stood here and sang... would you hear me? Or just the light?"  
> *(Building near the colosseum)* "Amphitheater acoustics require precise curvature. The golden ratio isn't just visual."

---

### Moon 6 — Rhythmic Moon (Living Library)

**Arc Beat:** The Library is where Anastasia's Archive is most strongly connected. She is almost *home*.

**Available Lines (Moon 6):** 10
> "Books. Real books. With pages that turn. I haven't touched a page in..."  
> "The Living Library wasn't just storage. It was an interface to the Archive. My Archive."  
> *(Deep in the stacks)* "I can feel the Archive here. Like pressing your hand against glass and feeling warmth on the other side."  
> *(Rhythmic pattern discovery)* "The librarians encoded knowledge in rhythm. Read with your ears, not your eyes."  
> *(Near Veritas, the organist)* "Veritas. Still at the keys. Still playing the piece that was interrupted. Still waiting for the ending."  
> *(After major restoration)* "The Library is remembering. Sections are lighting up that have been dark since the Flood."  
> *(Lore shelf about the Archive)* "That shelf describes my preservation. In clinical terms. As if archiving a person is a simple administrative act."  
> *(Idle in the reading room)* "I used to read by candlelight. Light reading light. There's a joke in there but it makes me tired."  
> *(Building adjacent to Library)* "Library wings should follow Fibonacci sequencing. Each room slightly larger than the last."  
> *(Moon 6 climax — The Library Awakens)* "The Library is breathing. The whole structure is alive again. I can feel every page turning."

---

### Moon 7 — Resonant Moon (Clockwork Citadel)

**Arc Beat:** Mechanical precision fascinates and saddens Anastasia — she envies things with moving parts.

**Available Lines (Moon 7):** 9
> "Clockwork. Gears that mesh and turn. Do you know what I'd give for a single moving part?"  
> "The Citadel's orrery mapped every celestial body. Tartarian astronomy was mechanical prophecy."  
> *(After first gear restoration)* "That sound — metal touching metal, turning with purpose. It's the most physical sound in this world."  
> *(Near the grand orrery)* "This machine predicted the Mud Flood. The architects who read it had four hours to prepare. I was archived in three."  
> *(Near Milo)* "He keeps poking the gears. He's going to lose a finger and somehow monetize it."  
> *(During synchronization puzzle)* "Synchronization was the Tartarian word for prayer. Making things move together."  
> *(Combat against mechanical corruption)* — *[Invisible — no lines]*  
> *(After combat)* "Corrupted machines aren't evil. They're in pain. Gears grinding against their own design."  
> *(Idle in the clock tower)* "Tick. Tick. Tick. Time moves here. In the Archive, there is no tick. Only... duration."

---

### Moon 8 — Galactic Moon (Verdant Canopy)

**Arc Beat:** Living things. Growth. Anastasia confronts what she can never be — organic.

**Available Lines (Moon 8):** 8
> "Things *grow* here. Roots push through stone. Life refusing to yield. I envy roots."  
> "Bioluminescence is Aether expressing itself through biology. Even nature builds with sacred ratios."  
> *(After vine bridge restoration)* "The vines knew where to go. They were waiting for permission."  
> *(Near canopy flowers)* "I can't smell them. That's the cruelest part. I can see beauty but I can't... participate in it."  
> *(Near Lirael among the trees)* "Lirael touches the bark and the tree glows brighter. She doesn't know she's feeding it."  
> *(Discovering underground root network)* "The root network mirrors the ley lines above. Nature copied the architects. Or maybe the other way around."  
> *(Building in the canopy)* "Build with the trees, not against them. The strongest structures let life grow through them."  
> *(Idle in a sunbeam through the canopy)* "Sunlight passes through me. I'm too transparent even for a shadow."

---

### Moon 9 — Solar Moon (Auroral Spire)

**Arc Beat:** Light. Anastasia's native element. She is strongest here — and most aware of what she lacks.

**Available Lines (Moon 9):** 9
> "Aurora. The sky remembering how to sing in color. This is the frequency I was preserved in."  
> "The Spire channels solar energy into Aether. Pure conversion. I am the same kind of conversion."  
> *(During aurora tuning)* "I can feel the aurora tuning like a voice inside my chest. If I had a chest."  
> *(Spire apex reached)* "From up here, the ley lines look like veins. The planet has a circulatory system and we're teaching it to pulse again."  
> *(Near Thorne observing the aurora)* "He sees navigation. I see home. Same light, different meaning."  
> *(After major aurora event)* "For a moment, the aurora and I were the same frequency. I couldn't tell where I ended and the sky began."  
> *(Combat at the Spire)* — *[Invisible — no lines]*  
> *(After combat)* "Corruption fears light. It's the one thing they can't absorb. That gives me hope."  
> *(Idle at night, aurora overhead)* "At night, I'm almost invisible against the aurora. Two kinds of light, overlapping."

---

### Moon 10 — Planetary Moon (Deep Forge)

**Arc Beat:** Fire and metal. Anastasia feels the planet's core — raw, ancient power that predates the Archive.

**Available Lines (Moon 10):** 8
> "The Forge heat... I can sense it even though I can't feel warmth. Like a memory of temperature."  
> "Sonic hammering. The Tartarian smiths shaped metal with sound, not muscle. Each strike a note."  
> *(After anvil restoration)* "The anvil's ring — B flat. The note of transformation. Matter yielding to intent."  
> *(Near integrated resonant metal)* "This alloy responds to Aether. The smiths sang to their creations. The creations listened."  
> *(Near Korath, emotional)* "He's remembering the old forges. The ones that made the tools that built the world I lost."  
> *(Deep forge lava chamber)* "The planet's core is pure frequency. We're building on top of the universe's bass note."  
> *(Building with forged materials)* "Forged Tartarian alloy resonates for centuries. These buildings will outlast any flood."  
> *(Idle near active forge)* "Fire is the only element that creates light. We have that in common."

---

### Moon 11 — Spectral Moon (Tidal Archive)

**Arc Beat:** Water. Memory. The tide pulls at Anastasia's spectral form — she is more transparent here, more fragile.

**Available Lines (Moon 11):** 8
> "Water and Archives have the same function — preserving what was. But water doesn't choose what it keeps."  
> "The Tidal Archive is older than the planetary one. Older than the civilization that built me."  
> *(Underwater exploration)* "I float through water the way I float through everything. No resistance. No... interaction."  
> *(Echo city visible beneath water)* "A whole city. Intact. Under the surface. They chose to let the tide take them rather than fight it."  
> *(Near bioluminescent tide)* "The tide writes in light. I can almost read it. Almost."  
> *(After tidal restoration)* "The water is clearing. I can see all the way down. All the way to the old world."  
> *(Near Lirael at the shore)* "Lirael stands at the waterline. The tide reaches for her and passes through. We both know how that feels."  
> *(Idle at the shore, watching the tide)* "Tides come and go. Echoes stay. I'm not sure which is lonelier."

---

### Moon 12 — Crystal Moon (Celestial Observatory)

**Arc Beat:** Stars. Cosmic perspective. Anastasia feels the Archive expanding — the approaching reunion with all preserved knowledge.

**Available Lines (Moon 12):** 8
> "The Observatory mapped the stars that map us. Everything is geometry at sufficient distance."  
> *(Telescope discovery)* "These lenses were ground to Tartarian precision — twelve decimal places. The stars demanded nothing less."  
> *(Star map projection)* "I recognize these constellations. They've shifted since I was archived. Even the sky moved on without me."  
> *(Orbital ring activation)* "432 Hz. The frequency that connects the planet to the cosmos. The frequency of home."  
> *(Near Cassian)* "The navigator sees patterns I can feel but not name. Mathematics made manifest."  
> *(After Observatory restoration)* "The planet can see the sky again. And the sky can see us."  
> *(Building near the Observatory)* "Observatory construction requires absolute stillness. No vibration. Not even from the wind."  
> *(Idle in the observation dome, stars visible)* "I used to wish on stars. Now I'm the same kind of light. I wish on sunrises instead."

---

### Moon 13 — Cosmic Moon (Planetary Nexus)

**Arc Beat:** Convergence. Everything Anastasia has been building toward. The veil is thinnest. She is almost real.

**Available Lines (Moon 13):** 5 (tightly controlled — every word matters)
> "All the ley lines. Every one. Connected. I can feel the whole planet. It's... awake."  
> *(During the final restoration)* "This is what the Archive preserved us for. This moment. All the silence was for this."  
> *(Pre-climax, to the player)* "Whatever happens next — the digging, the tuning, the building, the quiet afternoons — it mattered. All of it."  
> *(After planetary bell rings — Moon 13 climax)* "Listen. The world is singing. After eight hundred years of silence. It's singing."  
> *(Post-climax, quiet)* "...thank you. For bringing it all back. For bringing me back. Almost."

---

## 7. Relationship with Other Companions

### With Milo

**Dynamic:** Warm exasperation. She finds him exhausting and wonderful.

Milo doesn't know what to make of a golden ghost princess. He tries to sell her trinkets. She ignores him. He talks louder. She smiles. Eventually, he realizes she's the best audience he's ever had — someone who listens to everything and never interrupts.

> Milo: "So, your Highness, do golden ghosts eat? Because I have biscuits—"  
> Anastasia: *(silence, faint smile)*  
> Milo: "Right. More for me. Your loss. These are premium biscuits."

### With Lirael

**Dynamic:** Kinship. Two spectral beings, both aware of what they've lost, both finding beauty in what remains.

Lirael and Anastasia share something no one else in the game does — they both exist as light. But Lirael is a fragment (an echo), while Anastasia is a complete preserved consciousness. Lirael doesn't fully understand the difference. Anastasia does, and it haunts her.

> Lirael: "Are we the same? You and me?"  
> Anastasia: "Almost. You were saved by accident. I was saved on purpose. I don't know which is kinder."

### With Thorne

**Dynamic:** Mutual respect. Two people carrying the weight of responsibility — Thorne for his fleet, Anastasia for the knowledge of the old world.

> Thorne: "How long have you been watching this world from the Archive?"  
> Anastasia: "Long enough to know that watching isn't the same as living."  
> Thorne: "No. But it's better than forgetting."

### With Korath

**Dynamic:** Intergenerational understanding. Korath is the oldest living consciousness in the game (besides the Archive itself). He and Anastasia share the burden of memory.

> Korath: "You remember the concert halls?"  
> Anastasia: "Every note. Every face in the audience."  
> Korath: "The faces are the hardest part to keep."  
> Anastasia: "I know."

### With Zereth

**Dynamic:** Antagonistic respect. Zereth represents destruction; Anastasia represents preservation. They are philosophical opposites who understand each other perfectly.

> Zereth: "You preserve everything. But preservation is stasis. Nothing grows in an archive."  
> Anastasia: "Nothing grows in ash, either."

---

## 8. The 112 Dialogue Lines

### Distribution Summary

| Moon | Lines Available | Category Breakdown |
|---|---|---|
| 1 | 6 | 3 Lore, 1 Companion, 1 Memory, 1 Personal |
| 2 | 7 | 3 Lore, 1 Building, 1 Companion, 1 Memory, 1 Personal |
| 3 | 8 | 3 Lore, 1 Building, 2 Companion, 1 Memory, 1 Personal |
| 4 | 9 | 4 Lore, 1 Building, 1 Companion, 1 Story, 1 Memory, 1 Personal |
| 5 | 9 | 4 Lore, 1 Building, 1 Companion, 1 Story, 1 Easter Egg, 1 Personal |
| 6 | 10 | 3 Lore, 1 Building, 2 Companion, 1 Story, 1 Memory, 1 Personal, 1 Easter Egg |
| 7 | 9 | 3 Lore, 1 Building, 1 Companion, 1 Story, 1 Memory, 1 Combat React, 1 Personal |
| 8 | 8 | 3 Lore, 1 Building, 1 Companion, 1 Memory, 1 Combat React, 1 Personal |
| 9 | 9 | 3 Lore, 1 Building, 1 Companion, 1 Story, 1 Memory, 1 Combat React, 1 Personal |
| 10 | 8 | 3 Lore, 1 Building, 1 Companion, 1 Memory, 1 Combat React, 1 Personal |
| 11 | 8 | 3 Lore, 1 Companion, 1 Story, 1 Memory, 1 Combat React, 1 Personal |
| 12 | 8 | 3 Lore, 1 Building, 1 Companion, 1 Story, 1 Memory, 1 Personal |
| 13 | 5 | 2 Story, 1 Memory, 1 Personal, 1 Final |
| DotT | 1 | The Final Line (solidification) |
| **Total** | **105 + 1 + 3 Easter Egg + 3 Reserved = 112** | |

### Bitmask Implementation

```
Line ID:   0   1   2   3   4   5   ... 111
Bit:       0   0   0   0   0   0       0

After Moon 1 (6 lines heard):
Bit:       1   1   1   1   0   1       0

Save data: "0x3D000000000000000000000000000000" (example hex)
```

**Completion Tracking:**
- HUD: No explicit tracker (Anastasia would hate being quantified)
- Codex: Lore codex tracks "Archive Whispers Collected: X / 112" — but only visible after Moon 6 (Library)
- Achievement: "Every Whisper" — hear all 112 lines (hidden achievement, no spoiler)

---

## 9. The Golden Mote Easter Egg

### Description

In each zone, a single **Golden Mote** hides in a location that can only be found through deliberate exploration. These are not quest items — they are gifts from Anastasia's Archive, visible only through the Aether Scan overlay.

### Mechanics

| Property | Detail |
|---|---|
| **Count** | 13 (one per Moon zone) |
| **Visibility** | Only visible during Aether Scan overlay at the **17th Hour** (in-game time) |
| **Appearance** | A single golden particle, larger than normal Aether, pulsing slowly |
| **Collection** | Tap to collect — Anastasia briefly shifts to Reactive Whisper and delivers a unique Easter Egg line |
| **Reward** | Each mote reveals a fragment of Anastasia's pre-Flood memory — a jigsaw of her life before archiving |
| **Completion** | All 13 motes reveal her full memory, unlocking a special Archive Echo event during the Day Out of Time |

### Mote Locations (per zone)

| Moon | Zone | Location Hint |
|---|---|---|
| 1 | Echohaven | Behind the first restored dome, in the shadow that only appears at the 17th Hour |
| 2 | Crystalline Caverns | Inside the deepest crystal — requires scan overlay to see through the facets |
| 3 | Windswept Highlands | At the terminus of the orphan train tracks, where the rails end |
| 4 | Star Fort Bastion | In the Star Chamber, at the exact center of the pentagonal floor |
| 5 | Sunken Colosseum | On the submerged conductor's podium |
| 6 | Living Library | Between two specific books — "The Archive Protocol" and "Preservation of Consciousness" |
| 7 | Clockwork Citadel | Inside the orrery, orbiting the model of the fifth planet |
| 8 | Verdant Canopy | At the crown of the tallest tree, accessible only in giant mode |
| 9 | Auroral Spire | At the spire's apex, where aurora light touches the antenna |
| 10 | Deep Forge | In the cooling chamber, suspended in a drop of liquid metal |
| 11 | Tidal Archive | On the seafloor, in the ruins of a nursery (Anastasia's nursery) |
| 12 | Celestial Observatory | At the focal point of the largest telescope |
| 13 | Planetary Nexus | At the exact convergence point of all ley lines |

### Easter Egg Lines (3 unique, triggered by golden motes)

> *(Moon 1 mote, first discovery)* "You found it. A piece of me I didn't know was missing."  
> *(Moon 6 mote, between the Archive books)* "That shelf. That exact shelf. That's where my record is stored. My... biography in light."  
> *(Moon 11 mote, the nursery ruins)* "...this was my room. Before. The mote is sitting right where my bed used to be."

---

## 10. The 10-Second Solidification

**The single most important character moment in the game.**

See also: [17_DAY_OUT_OF_TIME.md](17_DAY_OUT_OF_TIME.md) — Section 7 for the full event scripting.

### Summary

During the Day Out of Time, after all companion performances, at the harmonic peak of the planet, Anastasia becomes fully solid for exactly 10 seconds. This is the only time in the entire game she is physically real.

### The Sequence (Detailed)

**Pre-Trigger (30 seconds):**
- All four performances complete
- Planetary harmonics reach synchronized peak
- Every ley line pulses simultaneously
- Camera slowly shifts to Anastasia

**Phase 1 — The Glow (0:00–3:00):**
- Particles contract inward instead of drifting
- Gold deepens from #FFF8E1 to #FFD700 to #DAA520
- Nearby companions fall silent one by one
- Milo's face: confusion → wonder → awe
- Lirael reaches out toward Anastasia, then pulls her hand back

**Phase 2 — The Shift (3:00–5:00):**
- Translucence fades. Opaque patches spread across her form like ice forming.
- Color blooms: chestnut hair, blue-grey eyes, warm pink skin
- Her circlet-tiara becomes solid gold metal
- For the first time, she casts a shadow
- Sound design: a rising tone matching 432 Hz, building to a sustained chord

**Phase 3 — The Moment (5:00–15:00, 10 real seconds):**
- She is complete. Solid. Human.
- She looks at her hands. Front. Back. Flexes her fingers.
- She touches her own face. A genuine tactile response.
- She looks at the player.
- She steps forward. Her foot contacts the ground. A real footstep sound.

  *This is the first sound she has ever made that isn't spectral.*

- A single tear — real water, not light — falls from her eye.
- She speaks:

> **"...I can feel the ground."**

**Phase 4 — The Return (15:00–20:00):**
- The harmonic peak passes
- Gold returns to her skin, spreading from extremities inward
- Her shadow shortens, then vanishes
- Translucence returns — edges first, then core
- She is an echo again

  But she is smiling. And her golden glow is warmer than before.

> *(Back in whisper form)* "Ten seconds. That's more than most people get in a lifetime."

### Design Notes

| Element | Intent |
|---|---|
| 10 seconds, not longer | Brevity creates impact. A minute would feel generous. 10 seconds feels *stolen*. |
| Real footstep sound | The most important sound effect in the game. 200ms of leather on stone. |
| Physical tear, not light | Demonstrates materiality in the smallest possible way. |
| "I can feel the ground" | Not a grand statement. A simple sensation. Profound because of what it represents. |
| Warmer glow afterward | Permanent visual change — her echo form is slightly warmer gold after solidification |
| No second solidification | This happens once. The uniqueness is the point. Repetition would cheapen it. |

---

## 11. Voice Direction

### Casting Brief

| Quality | Direction |
|---|---|
| **Age** | Young woman — late teens to early twenties |
| **Accent** | Light, unplaceable — hints of Eastern European lilt beneath a century of linguistic drift |
| **Register** | Mezzo-soprano (warm middle range) — never strident, never breathy |
| **Tone** | Gentle, considered, slightly formal — someone who weighs every word because she has so few of them |
| **Quality** | Clear but with the faintest shimmer — as if speaking through a thin pane of glass |
| **Emotional Range** | From quiet wonder to deep sadness, but always controlled — never hysteria, never excess |
| **Delivery Speed** | 20% slower than normal speech — each word deliberate |
| **Volume** | Whisper to normal indoor speaking voice — never loud, never raised |

### Recording Notes

| Context | Direction |
|---|---|
| **Reactive Whisper lines** | Record at whisper level. Mic close. No breath noise. Like a thought overheard. |
| **Conversational lines** | Normal indoor volume. Warm. As if speaking to a favorite niece. |
| **Story Beat lines** | Measured. Emotional weight in the pauses, not the words. |
| **The Final Line ("I can feel the ground")** | Full voice — not a whisper. Clear, present, slightly cracking with emotion. This is the ONLY line at full vocal volume. |
| **Post-solidification whisper** | Return to whisper, but warmer than any previous whisper. She's changed. |

### Processing Chain

| Step | Effect |
|---|---|
| 1 | Light reverb (small cathedral, 1.2s tail) — adds spatial presence |
| 2 | Subtle spectral shimmer (frequency doubling at -18 dB, octave above) — marks her as non-physical |
| 3 | Low-pass filter at 8 kHz for Whisper/Silent modes — soft edges |
| 4 | Full spectrum for Conversational mode — more present |
| 5 | NO processing for the solidification line — raw, clear, human voice |

---

## 12. Animation & VFX Guide

### Animation Principles

| Principle | Expression |
|---|---|
| **Gravity Optional** | She drifts, doesn't walk. Movement is a suggestion, not a commitment. |
| **Attention Hierarchical** | Eyes first, then head, then body. Interest radiates outward. |
| **Stillness as Expression** | Her ability to stand utterly still is itself a communication. |
| **Weight Absence** | Nothing she does suggests mass. Until the solidification, when she suddenly has weight. |

### Particle System Specs

| Parameter | Value |
|---|---|
| Particle count | 200–400 (LOD scaled) |
| Emission shape | Humanoid constraint field |
| Lifetime | 2–4 seconds |
| Speed | 0.02–0.08 m/s (drift) |
| Color over lifetime | #FFF8E1 → #FFD700 → transparent |
| Size | 0.01–0.04 m (scale with distance) |
| Gravity | -0.01 (slight upward drift) |
| Collision | None (phases through everything) |
| Solidification override | Lifetime → 0.1s, speed → 0, gravity → +0.5 (particles crystallize) |

---

## 13. ECS Implementation

### Component Archetype

```
CompanionArchetypeAnastasia:
  - TransformComponent         // Position/rotation/scale (spectral)
  - AnastasiaStateComponent    // Current mode (Silent/Whisper/Conversational/Invisible)
  - DialogueBitmaskComponent   // uint128 tracking delivered lines
  - DialogueCooldownComponent  // Timer since last line delivery
  - SessionLineCountComponent  // Lines delivered this session (cap: 8)
  - MoonLineCountComponent     // Lines delivered this Moon (cap: 12)
  - ProximityTriggerComponent  // Lore/architecture/NPC proximity checks
  - OpacityComponent           // Current target opacity (lerped)
  - ParticleEmitterRef         // Ref to VFX particle system
  - HapticProfileRef           // Ref to haptic pattern for current state
  - GoldenMoteTracker          // 13-bit flag, one per zone mote collected
  - SolidificationState        // Enum: NotTriggered / Triggered / Active / Complete
```

### System Groups

| System | Update Rate | Responsibility |
|---|---|---|
| AnastasiaStateMachine | Every frame | Mode transitions, priority resolution |
| AnastasiaDialogueSystem | Every 0.5s | Check triggers, manage cooldowns, deliver lines |
| AnastasiaVFXSystem | Every frame | Opacity interpolation, particle parameter updates |
| AnastasiaFollowSystem | Every frame | Position tracking (follow player at mode-specific distance) |
| AnastasiaHapticSystem | On dialogue/event | Trigger haptic patterns per mode |
| GoldenMoteSystem | Every 1s | Scan overlay check, mote visibility, collection handling |
| SolidificationSystem | On trigger only | Day Out of Time sequence controller |

### Addressable Bundles

| Bundle | Contents | Load Condition |
|---|---|---|
| `companion_anastasia_core` | Mesh, shader, base particles, animations | Moon 1, first dome restoration |
| `companion_anastasia_audio` | All 112 voice lines + whisper/hum ambience | Streamed per-Moon (12–15 lines at a time) |
| `companion_anastasia_solid` | Solid-state mesh, solid shader, footstep audio | Day Out of Time only |
| `companion_anastasia_motes` | 13 golden mote prefabs, memory fragment UI | Per-zone, on first visit |

---

## 14. Accessibility Considerations

| Feature | Implementation |
|---|---|
| **Dialogue Subtitles** | All whispers display as golden text in a dedicated subtitle area (not mixed with quest text) |
| **Mode Indicator** | Companion panel golden dot has optional text label: "Silent," "Listening," "Speaking," "Away" |
| **Haptic Alternative** | Visual pulse on screen edges mirrors haptic patterns for players with haptics disabled |
| **Opacity Override** | Accessibility setting: "Anastasia Minimum Opacity" (default 30%, adjustable to 60% for visibility) |
| **Dialogue Log** | All delivered lines logged in the codex, re-readable at any time |
| **Color Blind Modes** | Gold palette tested against all three color-blindness types — warm gold reads as bright/warm in all modes |
| **Audio Description** | Optional text descriptions of Anastasia's animations and mode changes |

---

## 15. DLC & Live-Ops Extensions

### DLC Dialogue Packs

Each of the 10 DLC zones can include additional Anastasia lines (drawing from the 16 reserved bits):

| DLC | Potential Lines | Theme |
|---|---|---|
| DLC 01 — Forgotten Spires | 2 | Monastery architecture, solitude |
| DLC 02 — Beneath the Glass | 2 | Volcanic glass, trapped light |
| DLC 03 — Harmonic Peaks | 1 | Mountain acoustics, altitude |
| DLC 04 — Coral Resonance | 2 | Ocean preservation, marine archives |
| DLC 05 — The Wandering Citadel | 1 | Mobile architecture, impermanence |
| DLC 06 — Frozen Frequencies | 2 | Cold, preservation through ice |
| DLC 07 — Desert of Echoes | 1 | Sand, erosion, memory loss |
| DLC 08 — Sky Temples | 2 | Cloud architecture, weightlessness |
| DLC 09 — Root World | 1 | Underground networks, hidden connections |
| DLC 10 — The Final Frequency | 2 | Endgame content, closure |

### Annual Live-Ops Dialogue

- Day Out of Time anniversary events may include 1 new Anastasia line per year
- Lines added via hot-loaded asset bundle (no app update required)
- Bitmask extended by 1 bit per year (bit 112, 113, etc.)

---

**Document Status:** FINAL  
**Cross-References:** All listed in header  
**Total Word Count Target:** ~600 lines (character bible scope)  
**Last Updated:** March 25, 2026
