# TARTARIA WORLD OF WONDER — Appendix C: Audio Design & Soundtrack Bible
## Adaptive 432 Hz Soundtrack, Cymatic Sound Design, Procedural Music Systems & Voice Direction

---

> *In Tartaria, music is not decoration — it is physics. Sound built these cities. Sound will restore them. The player does not merely listen to the world — they conduct it.*

**Cross-References:**
- [14_HAPTIC_FEEDBACK.md](../14_HAPTIC_FEEDBACK.md) — Audio-haptic sync specs, Hz-intensity mappings
- [13_MINI_GAMES.md](../13_MINI_GAMES.md) — Musical gameplay (Organ, Bell Tower, Cymatic Tuning, Rock Cutting)
- [03C_MOON_MECHANICS_DETAILED.md](../03C_MOON_MECHANICS_DETAILED.md) — Per-Moon audio moments and mechanics
- [B_ASSET_REFERENCE.md](B_ASSET_REFERENCE.md) — VFX particle systems that sync with audio

---

## Table of Contents

1. [Audio Philosophy](#1-audio-philosophy)
2. [Core Frequency Architecture](#2-core-frequency-architecture)
3. [Adaptive Soundtrack System](#3-adaptive-soundtrack-system)
4. [Procedural Music Engine](#4-procedural-music-engine)
5. [Environmental Audio Design](#5-environmental-audio-design)
6. [Instrument Design: The Tartarian Orchestra](#6-instrument-design-the-tartarian-orchestra)
7. [Combat Audio](#7-combat-audio)
8. [Moon-Specific Audio Profiles](#8-moon-specific-audio-profiles)
9. [Cinematic Audio & Spectacles](#9-cinematic-audio-spectacles)
10. [Voice Acting Direction](#10-voice-acting-direction)
11. ["Old Tartarian" Constructed Language](#11-old-tartarian-constructed-language)
12. [Audio-Haptic Synchronization Map](#12-audio-haptic-synchronization-map)
13. [Technical Audio Specifications](#13-technical-audio-specifications)

---

## 1. Audio Philosophy

### The Sonic Trinity

| Pillar | Principle | Player Experience |
|---|---|---|
| **Sound as Physics** | Sound is the mechanism of restoration — frequencies build, tune, heal, and destroy | Player hears cause-and-effect: correct tuning produces harmonic beauty, errors produce dissonance that damages the world |
| **Sound as Emotion** | Music deepens the narrative — silence is as powerful as symphony | Quiet underground excavation punctured by sudden choir sting when architecture is discovered; gradual orchestral build as zones restore |
| **Sound as Reward** | The world sings more beautifully as the player succeeds | Zones at 0% RS are nearly silent; at 100% they are full symphonic environments — the entire soundtrack is earned |

### Guiding Rules

1. **432 Hz is law.** All music, tuning, and harmonic systems derive from the 432 Hz base. No equal temperament — use just intonation tuned to 432.
2. **Silence has purpose.** Mud-buried zones are quiet. Corrupted zones emit anti-harmonics. The absence of beauty is always intentional.
3. **The player is the conductor.** Every restoration action produces sound. The cumulative effect is that the player is composing the soundtrack through gameplay.
4. **Diegetic first.** Music should feel like it comes from the world — organs, bells, crystal resonance, singing — not from an invisible orchestra. Non-diegetic scoring supplements, never dominates.
5. **Audio tells the truth.** Dissonance warns. Harmony confirms. The player can trust their ears.

---

## 2. Core Frequency Architecture

### The 432 Hz System

All Tartarian sound is built on a base tuning of **A = 432 Hz** (vs. modern standard A = 440 Hz). This is not arbitrary — it connects to the 3-6-9 numerological foundation of the game world.

| Mathematical Property | Value |
|---|---|
| **Base frequency** | 432 Hz (A4) |
| **Digital root** | 4+3+2 = 9 |
| **Octave below** | 216 Hz (2+1+6 = 9) |
| **Octave above** | 864 Hz (8+6+4 = 18 → 1+8 = 9) |
| **Harmonic series** | Every octave sums to 9 |

### The Three Bands — Sonic Identity

| Band | Frequency Character | Sonic Palette | Emotional Register |
|---|---|---|---|
| **3-Band** | Cool, crystalline, precise | Crystal bowls, high bell tones, water drops, silver chimes | Discovery, clarity, revelation |
| **6-Band** | Warm, golden, flowing | Pipe organ middle registers, cello, warm brass, children's voices | Growth, harmony, community |
| **9-Band** | Deep, cosmic, overwhelming | 32-foot organ bass pipes, thunder drums, massed choir, aurora hum | Transcendence, sacrifice, cosmic truth |

### Dissonance / Corruption

- **Anti-harmonics:** Frequencies deliberately set at 440 Hz or irrational intervals — creates subtle "wrongness" the ear detects before the mind
- **Sonic character:** Grinding metal, reversed audio, detuned bells, static interference, low-frequency rumble beneath hearing threshold (felt through haptics)
- **Zereth's theme:** Built on tritone (augmented 4th, the "devil's interval") — resolves to perfect 5th only in healing path

---

## 3. Adaptive Soundtrack System

### Layer Architecture

The soundtrack is not a fixed score — it is a living system of layers that respond to game state in real time.

**Layer Stack (bottom to top):**

| Layer | Content | Trigger | Fade Time |
|---|---|---|---|
| **L0: Silence** | Nothing | Zone RS = 0%, no structures nearby | — |
| **L1: Ambient Drone** | Single sustained tone (432 Hz root or 5th) | RS > 0%, player in zone | 8 sec fade-in |
| **L2: Harmonic Bed** | 3-note chord pads, just-intonation | RS > 15% or near restored structure | 4 sec crossfade |
| **L3: Melodic Fragment** | Short melodic phrases on period instruments | RS > 30%, exploration active | 2 sec crossfade |
| **L4: Rhythmic Pulse** | Subtle percussion, heartbeat-like | Combat, excavation, mini-game active | 1 sec snap |
| **L5: Full Ensemble** | Complete orchestration — strings, brass, organ, choir | RS > 70% or cinematic trigger | 6 sec build |
| **L6: Diegetic Override** | Organ performance, bell tower ring, choir singing | Mini-game active, Moon spectacle | Immediate |
| **L7: Cosmic Crescendo** | Full 9-Band resonance — all layers maximal | Moon-end climax, True Timeline finale | 10 sec build |

### State-Driven Mixing

| Game State | Active Layers | Mix Character |
|---|---|---|
| **Underground excavation** | L0 + L1 (very quiet) | Near-silence, player breathing, distant water drip, faint hum from walls |
| **Discovery moment** | L1 → L3 (sudden jump) | Choir sting burst, then settle to harmonic bed as player explores find |
| **Casual restoration** | L2 + L3 | Warm, contemplative, melodic fragments responding to player actions |
| **Mini-game (Organ/Bell)** | L6 dominant, L4 support | Player IS the musician — diegetic audio dominates |
| **Combat** | L4 + L5 | Percussion drives, dissonant enemy themes clash with player's harmonic |
| **Zone fully restored** | L2 + L3 + L5 | Full ambient symphony — achieved, earned, beautiful |
| **Moon-end spectacle** | L5 → L7 | Escalating orchestral build to overwhelming cosmic crescendo |

### RS-Driven Instrument Density

As a zone's Resonance Score increases, instruments join the ambient mix:

| RS Range | Instruments Added |
|---|---|
| **0-10%** | Silence → single sustained drone |
| **10-25%** | Crystal bowl harmonics, water trickle |
| **25-40%** | Strings (cello, viola), subtle bell echoes |
| **40-55%** | Warm brass (horn, flugelhorn), children humming |
| **55-70%** | Pipe organ low registers, percussion (soft timpani) |
| **70-85%** | Full organ, string ensemble, choir entrance |
| **85-95%** | Massed choir, full orchestral brass, overtone singing |
| **95-100%** | Everything + aurora harmonic overlay (9-Band) — sonic perfection |

---

## 4. Procedural Music Engine

### Architecture

The music engine generates adaptive compositions in real time from a library of stems, melodic cells, and harmonic rules.

**Components:**

1. **Harmonic Grammar:** Rules for chord progressions in 432 Hz just-intonation. Generates contextually correct harmony based on zone state, time of day, and Moon phase.
2. **Melodic Cell Library:** Short melodic phrases (2-8 bars) pre-composed and tagged by mood, band affinity, and intensity. System selects and sequences cells based on game context.
3. **Stem Mixer:** Pre-recorded instrument stems at multiple intensities. System crossfades between stem sets based on RS and game state.
4. **Diegetic Synthesizer:** Real-time audio for player-triggered instruments (organ, bells, tuning fork). Uses FM synthesis tuned to 432 Hz with physically modeled instrument characteristics.

### Harmonic Grammar Rules

**Permitted intervals (just intonation, relative to 432 Hz A):**

| Interval | Ratio | Hz from A | Character |
|---|---|---|---|
| Unison | 1:1 | 432 | Stability, foundation |
| Perfect 5th | 3:2 | 648 | Openness, strength |
| Perfect 4th | 4:3 | 576 | Resolve, completion |
| Major 3rd | 5:4 | 540 | Warmth, beauty |
| Minor 3rd | 6:5 | 518.4 | Melancholy, depth |
| Major 6th | 5:3 | 720 | Tenderness, hope |
| Octave | 2:1 | 864 | Transcendence |

**Forbidden in healthy Aether zones:** Tritone (45:32), minor 2nd (16:15), augmented intervals — these are reserved exclusively for corruption/dissonance audio.

### Procedural Composition Flow

```
[Zone RS] → Harmonic Grammar → Chord Sequence (4-bar loop)
                ↓
[Moon Phase + Time] → Melodic Cell Selector → Melody (2-8 bar phrase)
                ↓
[Player Action] → Stem Mixer → Instrument Set (density per RS table)
                ↓
[Combat/Mini-game] → Diegetic Override → Player-driven audio takes priority
                ↓
[Final Mix] → Spatial Audio Positioning → Output to speakers + haptic sync bus
```

---

## 5. Environmental Audio Design

### Zone Acoustic Profiles

| Zone Type | Acoustic Space | Key Sounds | Reverb Character |
|---|---|---|---|
| **Underground / Excavation** | Tight, reflective | Dripping water (irregular), distant stone stress creaks, player tool impacts echoing, faint Aether hum from walls, breathing | Short decay (0.5s), reflective, hard surfaces |
| **Open Terrain** | Wide, natural | Wind, grass rustle, distant bird analogs, ley-line electric crackle, mud squelch underfoot | Minimal reverb, wide stereo image |
| **Cathedral Interior** | Enormous, sacred | Vast silence broken by footsteps (each step echoes 4-5 times), stone resonance from walls, shaft-light hum | Very long decay (3-5s), warm, diffuse |
| **Restored City** | Bustling, alive | NPC ambient chatter, fountain spray, children laughter, distant organ, crystal chimes in breeze, market sounds | Medium decay (1-2s), open-air with nearby reflections |
| **Airship / Sky** | Open, wind-dominant | Wind past rigging, mercury-orb engine hum (steady pitch, responsive to throttle), crystal sail resonance, crew calls, flag snap | No reverb — open sky, Doppler on passing objects |
| **Bell Tower High** | Resonant, narrow | Wind at altitude, pigeon-analogs, creaking wood, bell overtones always faintly present, vertigo-inducing height sounds | Long metallic decay (4-6s), narrow, resonant |
| **Corruption Zone** | Oppressive, wrong | Static, reversed audio fragments, detuned drones, scratching, breathing that isn't the player's, silence in wrong places | Irregular — shifts between no reverb and infinite reverb, disorienting |

### Footstep System

Player footsteps are a core audio identity element — they tell the ground's story:

| Surface | Sound Character | Speed Variation |
|---|---|---|
| **Mud (wet)** | Thick, sucking squelch with slight pull on each step | Slow — effort sounds included |
| **Mud (dried)** | Dull, crumbling thuds | Normal |
| **Clean Stone** | Crisp, authoritative clicks with subtle ring | Normal — echo per zone reverb |
| **Marble (polished)** | Clear, musical tap with slight shimmer | Slightly fast — smooth surface |
| **Crystal** | High-pitched chime-step, almost musical scale | Pitch varies by crystal size/type |
| **Metal (copper grating)** | Hollow metallic clang with ring | Varied — responsive to grating type |
| **Wood (airship deck)** | Warm, creaking thud | Normal — creaks on stops |
| **Water (shallow)** | Splash with gurgle, sparkle sound if Aether-infused | Slow — wading sounds |
| **Giant-mode (any)** | Deep, earth-shaking boom per step, rattles nearby objects | Slow — seismic weight |

---

## 6. Instrument Design: The Tartarian Orchestra

### Primary Instruments

#### The Great Pipe Organ

The central musical instrument of the game, both mechanically and thematically.

**Physical Specifications:**
- **Pipes:** 61 total (5 registers × 12 notes per register + 1 Master Resonance Pipe)
- **Largest pipe:** 32 feet (9.75m) — bass register, extends through cathedral roof
- **Material:** Crystal-infused copper alloy — each pipe tuned to 432 Hz just-intonation
- **Power source:** Hydraulic bellows driven by pressurized pure water
- **Console:** 5-manual keyboard (one per register), foot pedals, stop knobs

**Sonic Character by Register:**

| Register | Range | Tone Quality | Gameplay Function |
|---|---|---|---|
| **Pedal (32')** | Sub-bass (27-108 Hz) | Seismic rumble, felt more than heard | Foundation — drives haptic feedback, structural effects |
| **Diapason (16')** | Bass (54-216 Hz) | Full, warm, room-filling | Core harmony — RS building, area healing |
| **Flute (8')** | Mid (108-432 Hz) | Sweet, clear, singing | Melody — thematic content, children's lullabies |
| **String (4')** | High-mid (216-864 Hz) | Bright, shimmering, sustained | Overtone richness — Aether enhancement |
| **Mixture (2')** | Treble (432-1728 Hz) | Brilliant, cutting, crystalline | Peak expression — cymatic pattern generation |
| **Master Resonance** | Variable | All registers fused | Boss-phase override — full-power Cymatic Requiem |

#### Bell Towers

**Bell Specifications:**
- **Size range:** 0.3m (smallest, high chime) to 3m (great bourdon bell)
- **Tuning:** Each bell tuned to a specific note in 432 Hz system — 12 bells per tower cover the full chromatic scale
- **Strike tone:** Sharp attack, complex overtone series lasting 4-8 seconds
- **Material:** Bronze alloy with Aether-conductive trace minerals (gives slightly crystalline shimmer to overtones)

**Gameplay Audio Behavior:**
- Bells ring automatically at time intervals (each Moon-hour, matching 17-hour clock)
- Player-rung bells: full strike + complete overtone decay (audience can hear each harmonic fade)
- Synchronized multi-tower ringing: bells in different towers harmonize — polyrhythmic patterns emerge
- Moon 12 Planetary Ring: 12+ towers × 12 bells each = 144+ bells sounding in cascading harmonic sequence

#### Crystal Bowls / Resonance Vessels

- **Cymatic water tuning:** Bowl filled with sacred water, player adjusts frequency — water forms visible standing-wave patterns
- **Sonic character:** Pure sine-like tone with slowly evolving overtone shimmer
- **Size determines base pitch:** Small bowls (high, clear), large bowls (deep, meditative)
- **Gameplay use:** Puzzle solving — find the frequency that matches the zone's natural resonance

#### Tuning Fork (Weapon)

- **Base tone:** 432 Hz — strikes produce pure tone with 2-second decay
- **Charged strikes:** Add overtones per band (3-Band adds crystalline shimmer, 6-Band adds warmth, 9-Band adds cosmic depth)
- **Impact sound:** Metallic ring + resonance bloom that responds to the struck enemy's corruption level — more corrupt = longer dissonant clash before resolution

### The Children's Choir

- **Voices:** 12-20 children's voices, ages 7-12 in vocal character
- **Tuning:** Natural, untempered 432 Hz — intentionally not perfectly in tune with each other (human quality)
- **Signature piece:** "The Lullaby of the Lost City" — a slow, hauntingly beautiful melody that the spectral orphan echoes sing
- **Combat application:** Children's choir singing 432 Hz in restored zones provides +10% player damage and dissonance suppression
- **Recording direction:** Record with natural room reverb, child-like imperfections preserved — NO pitch correction, NO artificial perfection

---

## 7. Combat Audio

### Audio Design Principles for Combat

1. **Harmonic weapons sound beautiful.** Player attacks produce musical tones that are pleasant to hear.
2. **Enemies sound wrong.** Corruption-based enemies produce anti-harmonics, detuned tones, grinding dissonance.
3. **Clash = tonal collision.** When player resonance meets enemy dissonance, the audio produces a brief tonal struggle that resolves in a satisfying harmonic chord on hit, or a dissonant screech on miss.
4. **Scale communicates power.** Bigger enemies = lower, more subsonic audio. Boss encounters = full seismic range.

### Enemy Sound Profiles

| Enemy Type | Audio Identity | Key Sounds |
|---|---|---|
| **Mud Golem** | Thick, sluggish | Wet clay impacts, grinding stone, slow gurgling breath |
| **Harmonic Parasite** | High-pitched, irritating | Detuned violin-like shreik, feeding on architecture produces glass-scraping sound, defeated produces crystalline shatter |
| **Dissonant Conductor** | Anti-orchestral | Reversed chord progressions, augmented intervals, their "conducting" produces cacophonous collision of instruments |
| **Living Sludge** | Subsonic, queasy | Below-threshold rumble, liquid impacts, corrosion hiss (like acid on stone) |
| **Spectral Guardian** | Echoing, ancient | Reverberant metallic clang, voice-like tones in forgotten language fragments |
| **Titan (boss-scale)** | Seismic, overwhelming | Earthquake-level footsteps, stone-stress groaning, 16 Hz subsonic (haptic-only, below human hearing) |
| **Zereth (final)** | Tragic, powerful | Corrupted version of Korath's warm theme — same melody in tritone, resolves to original key when healed |

### Combat Mix Priority

During combat, audio layering follows this priority (high to low):
1. Player weapon impacts (always audible, never masked)
2. Enemy signature sounds (identity critical)
3. Companion callouts (gameplay information)
4. Environmental destruction (spatial feedback)
5. Music layer L4-L5 (rhythmic drive, ducked during impacts)
6. Ambient environment (ducked significantly)

---

## 8. Moon-Specific Audio Profiles

### Moon Ambient Signatures

Each Moon has a dominant sonic character that colors all audio during that chapter:

| Moon | Ambient Keynote | Dominant Instrument | Atmospheric Sound | Emotional Tone |
|---|---|---|---|---|
| **1 — Magnetic** | A3 (216 Hz) | Crystal bowl drone | Underground drip, distant stone stress | Isolation, potential |
| **2 — Lunar** | E4 (324 Hz) | Solo cello | Bell tower echoes, night wind | Melancholy, purification |
| **3 — Electric** | F#4 (364.5 Hz) | Children's voices | Train whistle, lullaby fragments | Tenderness, protection |
| **4 — Self-Existing** | D4 (288 Hz) | Military drums + horn | Stone cutting percussion, cannon recoil | Defiance, precision |
| **5 — Overtone** | A4 (432 Hz) | Lirael's soprano | Grid hum, overtone shimmer | Connection, wonder |
| **6 — Rhythmic** | D3 (144 Hz) | Pipe organ (full) | Raindrops as rhythm, cathedral reverb | Sacred, overwhelming |
| **7 — Resonant** | A2 (108 Hz) | Giant's voice (bass) | Ice cracking, stone singing, deep earth | Ancient, sacrificial |
| **8 — Galactic** | E3 (162 Hz) | Mercury-orb engine | Wind, rigging creak, sky silence | Freedom, vastness |
| **9 — Solar** | G#4 (409.5 Hz) | Prophecy whispers | Clock tower ticking, temporal distortion | Mystery, urgency |
| **10 — Planetary** | G3 (192 Hz) | Train rail resonance | Children singing from approaching train, rail hum (120 BPM) | Journey, connection |
| **11 — Spectral** | C4 (256 Hz) | Water (tuned) | Aquifer flow, fountain spray, ionized mist | Healing, cleansing |
| **12 — Crystal** | F#3 (182.25 Hz) | Bell tower ensemble | All towers ringing, planetary resonance | Integration, final preparation |
| **13 — Cosmic** | A (all octaves) | Full Tartarian orchestra | All previous sounds layered | Transcendence, resolution |

### Key Audio Moments Per Moon

**Moon 1 — The Dome Awakening:**
- Near-silence → player clears mud → first stone resonance ping → dome tuning (432 Hz drone builds) → bells ring sympathetically across zone → fountains activate with water cascade → choir sting as camera pulls back to reveal restored dome broadcasting resonance

**Moon 3 — The Orphan Lullaby:**
- Children humming in distance (barely audible) → player approaches orphan echo → lullaby rhythm mini-game → successful lullaby causes echo to solidify → children's choir layer added to zone ambient permanently → Moon-end: combined children's voices create golden dome shield (crescendo to 9-Band resonance)

**Moon 6 — The Cymatic Requiem:**
- Organ reconstruction sequence (each pipe added = new note available) → first full composition attempt → Veritas teaches bass pedal technique → rehearsal (partial orchestra + choir) → PERFORMANCE: 5-register organ + Veritas bass/pedal + Lirael "Silver Passage" soprano solo + children's 432 Hz choir → rose windows blaze → city-wide ionized mist rain → all fountains pulse rhythmically → zone-wide sound envelope (every surface making music)

**Moon 7 — Korath's Song:**
- Giant stasis thawing: ice cracking sounds (deep, resonant) → first movement → Korath's voice (subsonic bass, "rattles through ice") → as he wakes, ambient pitch drops an octave → rock-cutting lessons: each lesson produces increasingly pure stone-singing tones → 9-Band Activation: all bands sound simultaneously for first time → Korath's Sacrifice: his theme modulates from warm major to bittersweet resolution as he dissolves — final note sustains for 10 seconds, transitioning into bell tower chime

**Moon 12 — The Planetary Bell-Tower Ring:**
- 12 towers activating in sequence (each at its tuned pitch) → polyrhythmic build as multiple towers overlap → player conducting the final synchronization → 144+ bells cascading in harmonic waterfall → 60-second sustained ring: clean bell strike (0.8 intensity haptic) → continental wave builds → crescendo to maximum audio and haptic → supernova burst → 500ms absolute silence → 30-second harmonic decay as the world settles into its new permanent sonic state

**Moon 13 — Three Endings:**
- *Harmony Path:* All previous themes return as a unified composition — children's choir + organ + bells + orchestra + aurora harmonic = the "Symphony of Tartaria" — new permanent ambient for post-game world. World gains a heartbeat: 432 Hz pulse felt through haptics permanently.
- *Echo Path:* Music freezes, crystallizes, becomes a music-box version of itself — beautiful but static, eternal but not alive.
- *Reset Path:* Score deconstructs in reverse order of Moon progression — instruments drop out one by one until only the original silence remains. Devastating.

---

## 9. Cinematic Audio & Spectacles

### Spectacle Audio Escalation Pattern

All Moon-end spectacles follow a consistent audio escalation curve:

```
Phase 1 — Preparation:    Ambient drops to L1-L2, quiet tension, heartbeat rhythm
Phase 2 — Initiation:     L3-L4, first thematic statement, instruments entering
Phase 3 — Build:          L5, full ensemble, rising dynamics, accelerating rhythm
Phase 4 — Climax:         L6-L7, maximum audio intensity, all systems active
Phase 5 — Release:        Brief silence (100-500ms) → sustained resolution chord → slow decay to new ambient state
```

### Silence as Audio Event

Critically, the game uses **absolute silence** as a deliberate audio event in three specific moments:

1. **Post-Planetary Ring silence (Moon 12):** 500ms of zero audio after the supernova burst — the most intense silence in the game, felt as absence after maximum intensity
2. **Zereth's healing moment (Moon 13):** 2 seconds of silence as corruption drains — the world holds its breath
3. **Reset ending final moment:** Last instrument fades → 3 full seconds of silence → title card appears in silence → player must choose to tap (and hear the first note of whatever comes next)

### Stinger Library

Pre-composed musical stingers for gameplay moments:

| Stinger | Duration | Instruments | Trigger |
|---|---|---|---|
| **Discovery** | 2s | Crystal bowl hit + ascending choir "ahh" | First architecture reveal in an excavation zone |
| **Restoration Complete** | 4s | Bell strike + string swell + choir resolve | Zone aspect reaches 100% RS |
| **Companion Bond** | 3s | Solo instrument (companion-specific) + warm harmonic bed | Trust level milestone achieved |
| **Corruption Warning** | 1.5s | Reversed bell + dissonant string cluster | Corruption spreading toward restored zone |
| **Giant Awakening** | 5s | Seismic sub-bass + ice crack + deep horn | Korath stirs / giant reveals |
| **Moon Transition** | 6s | Fading theme of old Moon → rising theme of new Moon | Chapter change |
| **Death / Failure** | 2s | Sustained minor chord with slow decay, crystal shatter | Player defeated |
| **Perfect Score** | 3s | Golden ascending arpeggio + bell cluster | 100% on any mini-game |

---

## 10. Voice Acting Direction

### General Voice Direction

- **Tone:** Naturalistic, not theatrical. Characters speak like real people in extraordinary circumstances.
- **Accents:** No single cultural accent dominates — the Tartarian world is pre-national. Slight, unplaceable inflection for all characters.
- **Emotion:** Grounded. Giants speak slowly but not stupidly. Children are genuine, not precocious. Nobody monologues — dialogue is conversational even when profound.
- **Audio quality:** Clean close-mic, minimal room. Reverb added procedurally per environment.

### Character Voice Profiles

**Player Character:**
- Silent protagonist with effort sounds (grunts, exertion, gasps of wonder)
- Breathing: audible during excavation, climbing, running — humanizes the player
- Giant-mode: voice drops one octave, effort sounds become deeper, heavier

**Milo (Fox Spirit):**
- No traditional speech — communicates through vocalizations
- Chitters (interest), growling (danger), soft yips (approval), whimpering (distress)
- One single word, spoken once in the entire game: at the moment of greatest sacrifice, Milo says the player's name (entered at game start) in a clear, gentle voice — the only time Milo speaks "human"
- Voice casting: warm, slightly rough, animal-like but with intelligence behind it

**Lirael (Spectral Architect):**
- Voice evolves with manifestation: Moon 1-3 heavy reverb + spectral shimmer overlay, Moon 4-7 clearer but still distant, Moon 8-11 natural with slight echo, Moon 12-13 fully present
- Tone: elegant, precise, warmth buried under centuries of loneliness
- Sings rarely but when she does (Silver Passage solo), it is the most beautiful single vocal performance in the game — soprano, 432 Hz perfect pitch, emotionally devastating
- Key line delivery: "I remember building this..." — said with quiet, private wonder (not exposition)

**Captain Thorne:**
- Military crispness softened by solitude — a man who hasn't spoken casually in 200 years
- Dry humor, understatement, occasional unexpected tenderness
- Commands are clear but not barked — he requests rather than demands (a commander who knows he has no crew)
- Airship sounds audible in his scenes — one with his ship
- Key line delivery: "The sky remembers what the ground forgot." — said looking at horizon, factual rather than poetic

**Korath the Builder:**
- Deep bass (very low register — lower than any human voice), slow, deliberate, enormous gentleness
- Each word carefully chosen — he speaks like someone who hasn't needed language in millennia
- Stone-on-stone resonance in his voice — as if his vocal cords are made of living granite
- Pauses between sentences (3-5 seconds), comfortable with silence
- Key line delivery: "I built for them. I always built for them." — said simply, without self-pity, absolute truth

**Cassian:**
- Smooth, articulate, educated — the voice immediately puts you at ease (that's the danger)
- Micro-hesitations before lies — trained actors only: the hesitation must be almost imperceptible
- Redeemed Cassian: loses the smoothness, speaks more haltingly, more honestly — voice becomes rougher
- Key line delivery: "You're asking the wrong questions." — delivered with a smile you can hear

**Zereth (The Dissonant One):**
- Moons 1-9: whisper-only, heavily processed — words barely distinguishable from wind
- Moon 10-12: revealed voice — unmistakably related to Korath (same bass register) but in agony, words broken by dissonant audio glitches
- Moon 13 (corrupted): full voice, raw pain — sounds like Korath screaming
- Moon 13 (healed): Korath's warmth returns — the same gentle bass, scars audible as slight vocal fry
- Key line delivery: "I was the first to forget." — the most quietly devastating line in the game

**Veritas (Cathedral Organist Echo):**
- Speaks in musical metaphors ("That passage needs more rubato," "You're rushing the fermata")
- Precise, exacting, but not cold — the passion of someone who devoted their existence to perfection
- Pipe organ harmonics faintly audible in their voice (subtle processing)
- Key line delivery: "The organ remembers every hand that played it. Even mine." — wistful, proud, sad

**Children (Spectral Echoes):**
- Ages 7-12 vocally; genuine, unaffected, occasionally heartbreaking
- Speak in sentence fragments and observations: "The stars look wrong here" / "Is this the way home?"
- Group singing: not in perfect unison — staggered breath marks, slightly different tempi, human
- Aria (lead child): unusually perceptive, calm — describes impossible things matter-of-factly
- Key line delivery (Aria): "That one's humming wrong." — pointing at corruption, completely unconcerned

---

## 11. "Old Tartarian" Constructed Language

### Design Principles

"Old Tartarian" is not a full conlang — it is a phonesthetic system designed to SOUND meaningful while remaining untranslatable. The player should feel they almost understand it.

### Phonological Rules

- **Vowels:** Predominantly open vowels — /a/, /o/, /ɛ/ — resonant, singable, cathedral-filling
- **Consonants:** Liquid and nasal heavy — /l/, /r/, /m/, /n/ — smooth, flowing, no hard stops
- **Forbidden sounds:** No /k/, /g/, /t/ (hard), /p/ explosives — the language has no harsh consonants
- **Syllable structure:** (C)V(N) — consonant-vowel with optional nasal coda. Every syllable can be sustained as a musical note
- **Stress:** Every third syllable stressed (3-based rhythm): ta-ra-**MA** / li-se-**NO** / va-re-**LO**

### Numerical System (3-6-9 Based)

| Number | Old Tartarian | Significance |
|---|---|---|
| 1 | *en* | Unity |
| 2 | *al* | Duality |
| 3 | *sar* | First sacred number |
| 6 | *elen* | Second sacred number |
| 9 | *namor* | Third sacred number / totality |
| 12 | *sarelen* (3×4 or 6×2) | Celestial count |
| 13 | *namorsaren* | The complete cycle |

### Key Vocabulary

| Tartarian | Meaning | Usage |
|---|---|---|
| *Aethar* | Living energy / breath of creation | Universal term — the Aether |
| *Valar* | To build / to sing (same word) | Building IS singing in Tartarian cosmology |
| *Namori* | The Harmony / the natural state | "All is namori" = everything resonant |
| *Seranath* | Giant / builder / elder one | Korath is "Seranath Korath" |
| *Liranel* | The memory that sings | Lirael's true name |
| *Dissonath* | The breaking / the forgetting | The word for corruption / Reset |
| *Rathema* | Child of harmony | How giants refer to humans |
| *Valoren* | The world-song | The totality of resonance — the goal of the game |

### Usage in Game

- **Giants speak Old Tartarian** in emotional moments — untranslated, but tone conveys meaning
- **Organ compositions** have Old Tartarian titles (displayed on screen, not spoken)
- **Inscriptions** on architecture use Old Tartarian script — a flowing, cursive system based on wave patterns
- **The Lullaby** lyrics are in Old Tartarian — children sing in a language they feel but don't understand
- **Subtitles for Old Tartarian:** Available as a toggle — some players prefer the mystery, completion-seekers want the translation

---

## 12. Audio-Haptic Synchronization Map

### Sync Architecture

Audio and haptic systems share a synchronization bus that ensures every felt vibration corresponds to a heard sound (or deliberate absence).

**Sync Rules:**
1. Haptic onset precedes audio onset by 10-20ms (haptic travels through body faster — creates unified perception)
2. Haptic intensity correlates with audio amplitude (not frequency — low sounds can be gentle, loud sounds always have haptic)
3. Haptic frequency matches the fundamental pitch of the audio event (where hardware allows)
4. Silence = haptic silence (no orphaned vibrations — except the 432 Hz world-heartbeat post-game)

### Key Sync Events

| Event | Audio | Haptic | Sync Detail |
|---|---|---|---|
| **Stone Resonance Ping** | Crystal chime (high, brief) | Sharp crystalline pulse (0.6, 50ms) | Haptic onset 15ms before audio |
| **Dome Tuning** | 432 Hz drone (sustained, building) | Rhythmic pulse at 432 Hz period (0.3 intensity) | Phase-locked to audio fundamental |
| **Bell Strike** | Full bell spectrum (attack + 4s decay) | Strike (0.4, 80ms) → ring decay (0.3→0.05, 4s) | Haptic decay envelope mirrors audio |
| **Organ Chord** | Multi-register chord (sustained) | Layered pulse: 0.15 base + 0.05 overtone cycle | Haptic layers match register count |
| **Cannon Fire** | Explosion + resonance blast | Sharp recoil (0.8, 80ms) → cascade pulses | Haptic recoil BEFORE audio explosion by 20ms |
| **Giant Footstep** | Seismic thud + ground crack | Maximum single impact (0.9, 120ms) + tremor | Screen shake + haptic + audio simultaneous |
| **Corruption Encounter** | Dissonant screech + static | Irregular arrhythmic pulses (unsettling) | Deliberately de-synced from audio (discomfort) |
| **Fountain Activation** | Water cascade + sparkle | Gentle spreading warmth (0.2, 500ms build) | Haptic follows water arc trajectory |
| **Moon Climax Peak** | Full orchestra fortissimo | Supernova Burst (1.0 max, 200ms) | Simultaneous with audio peak |
| **Post-Climax Silence** | Absolute zero audio | Absolute zero haptic (500ms) | Perfectly synchronized absence |
| **World Heartbeat (post-game)** | 432 Hz subtle pulse | Gentle rhythmic (0.1, continuous) | Permanent — the world alive at rest |

---

## 13. Technical Audio Specifications

### Platform: PC (Windows 10/11, Vulkan/DX12)

| Parameter | Specification |
|---|---|
| **Sample Rate** | 48 kHz (all assets) |
| **Bit Depth** | 24-bit source, 16-bit compressed delivery |
| **Format (music stems)** | Vorbis 256 kbps VBR (streamed from Addressable bundle) |
| **Format (SFX)** | OPUS for long sounds, uncompressed WAV for short stingers (<500ms) |
| **Simultaneous voices** | 96 max (48 audio + 48 streaming) |
| **Memory budget** | 128 MB active audio (from total 4 GB RAM budget) |
| **Latency target** | <12ms input-to-sound (WASAPI exclusive / ASIO) |
| **Haptic sync** | Gamepad haptic API with pattern files, phase-locked to audio timeline |
| **Spatial audio** | Steam Audio binaural HRTF (headphones), stereo/5.1/7.1 fallback for speakers |
| **Dynamic range** | -60 dB (silence floor) to 0 dB (spectacle peak), 48 dB effective dynamic range |

### Mixing Strategy

**Master Mix Bus Levels:**

| Bus | Normal | Combat | Spectacle | Silence Event |
|---|---|---|---|---|
| **Dialogue** | -6 dB | -6 dB (priority) | -12 dB (ducked) | -∞ (muted) |
| **Music** | -12 dB | -18 dB (ducked) | -3 dB (featured) | -∞ (muted) |
| **SFX** | -9 dB | -6 dB (boosted) | -9 dB | -∞ (muted) |
| **Ambience** | -15 dB | -24 dB (heavily ducked) | -18 dB | -∞ (muted) |
| **Diegetic (gameplay)** | -6 dB | -9 dB | 0 dB (organ/bell priority) | -∞ (muted) |

### Compression & Delivery

- **Initial download:** Core SFX library + Moon 1 music stems (~150 MB audio)
- **Per-Moon download:** Music stems + zone-specific SFX via Addressable bundles (~20-40 MB per Moon)
- **Streaming stems:** Music layers streamed from disk, crossfaded in real-time — never fully loaded simultaneously
- **Memory pooling:** SFX voices pooled and recycled — no voice allocation during gameplay (pre-warmed on zone load)

### Accessibility Audio Features

| Feature | Implementation |
|---|---|
| **Audio descriptions** | Optional narrator describes visual spectacles for visually impaired players |
| **Mono downmix** | Full spatial audio collapses cleanly to mono (single-sided hearing) |
| **Subtitle system** | All dialogue subtitled, environmental audio captioned ("[bell tolls in distance]") |
| **Volume per bus** | Separate sliders for Music, SFX, Dialogue, Ambience, Diegetic |
| **Mini-game audio mode** | All visual mini-game cues have equivalent audio cues for eyes-free play |
| **Haptic-only option** | For deaf/HoH players — haptic system conveys gameplay-critical audio information |
| **No audio-only gating** | No puzzle or mechanic requires hearing — visual fallbacks for all audio events |

---

**Document Status:** FINAL  
**Cross-References:** `14_HAPTIC_FEEDBACK.md`, `13_MINI_GAMES.md`, `03C_MOON_MECHANICS_DETAILED.md`, `B_ASSET_REFERENCE.md`  
**Last Updated:** March 25, 2026
