# TARTARIA WORLD OF WONDER — Haptic Feedback Design Bible
## Complete Core Haptics (Taptic Engine) Specification
### Every Touch, Every Pulse, Every Cosmic Moment

---

> *Haptics in Tartaria are not decorative — they are the player's physical connection to resonance. When a dome breathes, the player feels it. When a bell rings across continents, the player's hands carry the wave. When the timeline cracks open, the player's body knows before their eyes confirm it. This document specifies every haptic interaction in the game, from idle hums to cataclysmic cosmic events.*

**Cross-References:**
- [09_TECHNICAL_SPEC.md](09_TECHNICAL_SPEC.md) — Core Haptics API, Taptic Engine specs, iOS platform
- [07_MOBILE_UX.md](07_MOBILE_UX.md) — Touch control integration
- [12_VIVID_VISUALS.md](12_VIVID_VISUALS.md) — Visual events paired with haptic profiles
- [13_MINI_GAMES.md](13_MINI_GAMES.md) — Mini-game haptic accompaniment
- [11_SCRIPTED_CLIMAXES.md](11_SCRIPTED_CLIMAXES.md) — Climax haptic escalation

---

## Table of Contents

1. [Haptic Design Philosophy](#haptic-design-philosophy)
2. [General Principles (6 Categories)](#general-principles)
3. [Moon-Specific Haptic Profiles](#moon-specific-haptic-profiles)
4. [Airship Core Profile](#airship-core-profile)
5. [Flight Controls](#flight-controls)
6. [Megalith Lift (5-Stage)](#megalith-lift-5-stage)
7. [Raid Cannon Fire](#raid-cannon-fire)
8. [Aurora Glide](#aurora-glide)
9. [Ley-Line River Sync Pulses](#ley-line-river-sync-pulses)
10. [Formation Flight with Armada](#formation-flight-with-armada)
11. [Overload Climax Release](#overload-climax-release)
12. [Supernova Burst Thump](#supernova-burst-thump)
13. [Planetary Bell-Tower Peal](#planetary-bell-tower-peal)
14. [True Timeline Merge](#true-timeline-merge)
15. [Day Out of Time Festival](#day-out-of-time-festival)
16. [Aurora Veil Dance Ripples](#aurora-veil-dance-ripples)
17. [Swirling Spiral Ripple Texture](#swirling-spiral-ripple-texture)
18. [Peak Vortex Swell](#peak-vortex-swell)
19. [Layered Spiral Shockwaves](#layered-spiral-shockwaves)
20. [Cataclysmic Spiral Slam](#cataclysmic-spiral-slam)
21. [Accessibility & Battery Considerations](#accessibility-battery-considerations)

---

## Haptic Design Philosophy

**The Resonance Principle:** Every haptic event must feel like it originates from within the world — not from a phone. Players should unconsciously associate haptic feedback with the living architecture, the cosmic forces, and the resonance grid they're rebuilding.

**Hierarchy of Intensity:**
1. **Ambient (0.1–0.2 intensity):** Background world-feel — dome humming, water flowing, idle Aether
2. **Interactive (0.3–0.5 intensity):** Player actions — tapping, building, tuning, combat hits
3. **Dramatic (0.5–0.7 intensity):** Story moments — revelations, companion beats, danger signals
4. **Spectacular (0.7–0.9 intensity):** Moon-ending events — requiems, sieges, convergences
5. **Cosmic (0.9–1.0 intensity):** Once-per-campaign moments — planetary ring, timeline crack, final choice

**Technical Constraints:**
- Core Haptics API on iOS (CHHapticEngine)
- Maximum pattern duration: 30 seconds per continuous event (chain for longer)
- Battery-aware: intensity reduces by 20% below 30% battery, 40% below 15%
- Framerate-synced: haptic events tied to visual frames, never free-running

---

## General Principles

### Category 1: Restoration Haptics
- **Mud excavation:** Progressive resistance — starts heavy (dense mud), lightens as intact architecture is revealed. Intensity drops from 0.4 to 0.15 over 3–5 seconds per layer.
- **Structure discovery:** Sharp "ping" (0.6 intensity, 50ms) when first intact stone is exposed. Clean, crystalline feel.
- **Dome tuning:** Rhythmic pulse matching the dome's frequency — starts irregular (waveform misaligned), regularizes as player tunes correctly. 432 Hz base rhythm at 0.3 intensity.
- **Completion:** Deep satisfying "lock" (0.7, 100ms) followed by warm sustained hum (0.2, 3 seconds).

### Category 2: Combat Haptics
- **Resonance weapon swing:** Swift sweep (0.3, 80ms, directional — left-to-right or right-to-left matching swipe direction)
- **Hit connection:** Sharp impact (0.5, 40ms) with brief "ring" tail (0.2, 150ms) — the weapon resonates
- **Golem shatter:** Heavy thud (0.6, 60ms) → crystalline scatter (0.3, rapid irregular pulses over 300ms)
- **Wraith phase:** Unsettling hollow pulse (0.2, 100ms) — feels like something passing through the device
- **Player damage:** Jarring irregular burst (0.7, 50ms) — deliberately unpleasant, breaking the world's natural rhythm
- **Giant mode combat:** All impacts scaled 2× in duration and 1.5× in intensity. Player FEELS large.

### Category 3: Navigation Haptics
- **Walking on restored ground:** Faint rhythmic pulse matching footstep pace (0.1, 30ms per step)
- **Walking on corrupted ground:** Same rhythm but irregular — occasional missed beats, wrong timing (0.15)
- **Airship boarding:** Transition: ground vibration fades → floating hum begins (see Airship Core Profile)
- **Train boarding:** Rhythmic rail hum establishes (120 BPM, 0.15 per beat)
- **Bell tower ascent:** Each step up produces a faint ascending tone pulse (pitch suggestion through intensity pattern)

### Category 4: Companion Haptics
- **Milo's proximity alert:** Rapid triple-tap (0.2, 0.3, 0.4 — ascending) — fox growl warning
- **Lirael's memory flash:** Slow sustained pulse (0.3, 2 seconds) — warmth, gentleness
- **Korath's echoing voice:** Deep low-frequency sustained (0.4, 1 second) with slight tremor
- **Orphan child approach:** Very light rhythmic (0.1, child-like pace — faster than adult rhythm)
- **Companion dialogue moment:** Single gentle pulse at dialogue start (0.15, 100ms) — attention cue

### Category 5: Environmental Haptics
- **Fountain activation:** Flowing water simulation — smooth sine-wave intensity cycling (0.1–0.25, 2-second period)
- **Bell tower ring (ambient):** Single clean strike (0.4, 80ms) → decaying ring (0.3 → 0.05 over 4 seconds)
- **Pipe organ ambient:** Multi-layered sustained hum (0.15 base + 0.05 overtone cycling)
- **Aurora overhead:** Very gentle flowing pattern (0.05–0.1, random slow undulation) — barely perceptible, creates subliminal mood
- **Earthquake/tremor:** Irregular heavy low-frequency (0.5–0.7, 1–3 seconds, chaotic pattern)
- **Ley-line crossing:** Brief resonance pulse (0.25, 200ms) — feel the energy path beneath your feet

### Category 6: UI Haptics
- **Menu navigation:** Crisp tap (0.15, 20ms)
- **Slider adjustment:** Detent clicks at regular intervals (0.1, 10ms per detent)
- **Confirmation:** Solid double-tap (0.25, 30ms gap, 30ms)
- **Error/invalid action:** Soft buzz (0.2, 100ms, irregular)
- **Achievement unlock:** Ascending three-note tap pattern (0.2, 0.3, 0.5 with 100ms gaps)

---

## Moon-Specific Haptic Profiles

Each Moon adds or modifies the ambient haptic layer:

| Moon | Ambient Character | Intensity Range | Notes |
|---|---|---|---|
| 1 Magnetic | Warm, steady pulse | 0.1–0.2 | First contact with resonance. Comforting. |
| 2 Lunar | Irregular, slightly wrong | 0.1–0.25 | Corruption creates off-rhythm interference |
| 3 Electric | Lively, quick rhythms | 0.1–0.3 | Children's singing creates fast light patterns |
| 4 Self-Existing | Precise, geometric | 0.15–0.25 | Every haptic beat is exact, φ-timed |
| 5 Overtone | Layered, complex | 0.15–0.3 | Multiple ambient signatures overlapping |
| 6 Rhythmic | Musical, dynamic | 0.1–0.4 | Organ vibrations permeate the world |
| 7 Resonant | Deep, powerful | 0.2–0.5 | Giant-scale resonance everywhere |
| 8 Galactic | Floating, untethered | 0.05–0.2 | Anti-grav ambience, lightness |
| 9 Solar | Warm, building | 0.2–0.4 | Ley lines intensifying |
| 10 Planetary | Alive, responsive | 0.2–0.5 | Grid consciousness — haptics react to grid state |
| 11 Spectral | Dual-layer (solid + ghost) | 0.1–0.3 | Two timelines create two haptic tracks |
| 12 Crystal | Brilliant, sharp | 0.2–0.6 | Bell tower resonance saturates everything |
| 13 Cosmic | Everything, maximum | 0.1–1.0 | Full spectrum. All layers active. |

---

## Airship Core Profile

The airship is a living vehicle — its haptic presence communicates its state to the player at all times.

### Hover/Idle
- **Pattern:** Low continuous hum (0.15 intensity, 2 Hz oscillation)
- **Character:** Anti-gravity engine vibration — smooth, mechanical-organic hybrid
- **Direction:** Centered below thumbs (where the controls are)
- **Mercury orb:** Subtle liquid sloshing overlay (0.05, random gentle shifts every 2–4 seconds)

### Anti-Gravity Engagement
- **Pattern:** Hum intensifies (0.15 → 0.3 over 2 seconds) as anti-grav spools up
- **Character:** Rising tone suggested through increasing pulse frequency (2 Hz → 8 Hz)
- **Peak:** Clean "lock" pulse (0.4, 60ms) when hover altitude stabilized

### Mercury Orb Resonance
- **Pattern:** Periodic warm pulse (0.2, 500ms, every 3 seconds) — the orb's heartbeat
- **Character:** Liquid, flowing — suggests the mercury shifting inside its chamber
- **Contextual:** Pulse rate increases near ley lines, near corruption, during combat

---

## Flight Controls

### Ascent
- **Pattern:** Upward-sweeping intensity (0.15 → 0.35 over the ascent duration)
- **Character:** Like the device is lifting — haptic weight shifts toward the top edge
- **Audio sync:** Matches engine tone rise

### Descent
- **Pattern:** Downward settling (0.35 → 0.15)
- **Character:** Gravity returning — haptic weight shifts toward bottom edge
- **Landing:** Final "touchdown" thump (0.4, 80ms) + settling vibration (0.1, 500ms decay)

### Banking (Turn)
- **Pattern:** Directional pulse favoring the turn direction
- **Left bank:** Stronger haptic on left side of device (0.25 left, 0.1 right)
- **Right bank:** Reversed
- **Duration:** Sustained through the turn, returns to centered on straightening
- **Sharp turn:** Intensity spikes briefly (0.4 peak) — the airship straining

---

## Megalith Lift (5-Stage)

*The signature haptic experience of the airship system — lifting impossibly heavy precision-cut stone through resonance.*

### Stage 1: Approach
- **Trigger:** Airship enters megalith pickup zone
- **Pattern:** Ambient hum deepens (0.15 → 0.25). Slow pulse develops (1 Hz)
- **Character:** Anticipation — the ship sensing the mass below
- **Duration:** 5–10 seconds (player positioning)

### Stage 2: Field Engagement
- **Trigger:** Player activates anti-grav lift beam
- **Pattern:** Pulse rate increases (1 Hz → 4 Hz). Intensity builds (0.25 → 0.5). Resistance feel — the beam is grabbing.
- **Character:** Effort — the ship's systems working against gravity
- **Mercury orb:** Liquid sloshing intensifies (0.1, irregular)
- **Duration:** 3–5 seconds

### Stage 3: Lift-Off
- **Trigger:** Megalith breaks free from ground
- **Pattern:** SPIKE (0.7, 100ms) — the moment of separation. Then immediate settle to elevated hum (0.4, 3 Hz).
- **Character:** Triumph and strain — the stone is free but heavy
- **Ground reaction:** Brief deep thud (0.6, 60ms) as ground rebounds
- **Duration:** 1–2 seconds (transition)

### Stage 4: In-Flight Carry
- **Trigger:** Megalith suspended, airship in transit
- **Pattern:** Elevated sustained hum (0.35, 3 Hz) + pendulum sway overlay (0.1 oscillating left-right at 0.5 Hz)
- **Character:** The stone introduces inertia — the airship handles differently
- **Turbulence:** Occasional irregular spikes (0.4–0.5, 200ms) — stone swaying in the beam
- **Player skill:** Smoother flight = less turbulence haptics. Skill-based reward.
- **Duration:** Variable (30 seconds – 3 minutes depending on distance)

### Stage 5: Precision Placement
- **Trigger:** Approach construction site
- **Pattern:** Hum steadies (turbulence reduces). Alignment pulses begin — rhythmic taps that increase in frequency as the stone approaches perfect position.
  - Far from target: 0.5 Hz tap, 0.2 intensity
  - Near: 2 Hz, 0.3
  - Very near: 4 Hz, 0.4
  - Perfect alignment: Rapid pulse → hold → LOCK (0.6, 100ms clean strike)
- **Character:** Transition from effort to precision to satisfaction
- **Golden ratio placement:** Extra harmonic "ring" (0.3, 500ms decay) — the stone singing as it joins the structure

---

## Raid Cannon Fire

*Combat from the airship — resonance weaponry haptic signature.*

### Charging
- **Trigger:** Player holds fire button
- **Pattern:** Building vibration (0.2 → 0.5 over 2 seconds). Pulse rate increases (2 Hz → 8 Hz).
- **Character:** Energy accumulating — the cannon drawing Aether from the grid
- **Mercury orb:** Accelerates (heartbeat → flutter)
- **Visual sync:** Cannon barrel glows brighter with haptic intensity

### Firing
- **Trigger:** Player releases fire button
- **Primary impact:** Sharp recoil thump (0.8, 80ms) — the most intense single combat haptic
- **Cannon ring:** Resonance decay tail (0.4 → 0.1 over 1 second)
- **Directional:** Haptic weight shifts opposite to fire direction (recoil feel)
- **Ship reaction:** Brief instability (0.2, irregular 300ms) — the airship absorbing recoil

### Chain-Light Overload
- **Trigger:** Cannon shot hits chain-light crystal on target
- **Pattern:** Initial hit (0.8) → cascade pulses (0.5, 0.4, 0.3, 0.2 at 100ms intervals) — the resonance chaining to secondary targets
- **Character:** Expanding explosion feel — diminishing intensity as energy spreads
- **Bonus:** Critical chain (5+ targets) adds a final "bloom" pulse (0.6, 200ms) — the overload moment

### Enemy-Specific Feedback
- **Golem hit:** Heavy thud (0.6, 100ms) — like hitting stone
- **Wraith hit:** Hollow disruption (0.3, 150ms, uneven) — like hitting smoke
- **Shield hit:** Sharp deflection ping (0.5, 30ms) + tingling scatter (0.2, 200ms)
- **Boss weak-point hit:** Unique satisfying CRACK (0.7, 50ms) + crystalline shatter (0.4, 400ms dispersal)

---

## Aurora Glide

*Flying through the northern lights — the most meditative haptic experience.*

### Base Glide
- **Context:** Airship flying at high altitude, aurora bands visible
- **Pattern:** Smooth, flowing (0.1 intensity, slow sine wave 0.3 Hz — like breathing)
- **Character:** Serenity. Weight lifted. Floating through light itself.
- **Duration:** Sustained as long as in aurora zone

### Entering a Curtain
- **Trigger:** Airship passes through an aurora band
- **Pattern:** Brief intensification (0.1 → 0.3) + tingling overlay (0.15, rapid random micro-pulses for 500ms)
- **Character:** Like passing through a waterfall of light — brief, cool, sparkling
- **Color sync:** Different aurora colors produce subtly different intensities:
  - Green aurora: lightest (0.25 peak)
  - Violet aurora: medium (0.35 peak)
  - Gold aurora: warmest, longest sustain (0.3, 1-second extended tingle)

### Exiting a Curtain
- **Pattern:** Tingling fades over 300ms. Base glide resumes.
- **Character:** Clean transition — no abruptness

### Ley-Line River (Below)
- **When visible from above:** Additional low pulse (0.1, 1 Hz) — the river's presence felt through altitude
- **Flying directly over river:** Pulse strengthens (0.2, 1.5 Hz) — proximity amplification

---

## Ley-Line River Sync Pulses

*Walking/riding alongside the golden energy rivers beneath the surface.*

### Base Rhythm (Near Ley-Line)
- **Detection:** Within 20m of a ley-line
- **Pattern:** Gentle pulse matching the line's flow direction (0.1, 1 Hz)
- **Character:** The earth's heartbeat — steady, alive, warm
- **Directional:** Pulse subtly stronger on the side nearest the ley-line

### Entering the River
- **Trigger:** Player steps into or onto ley-line river (bridges, crossing points)
- **Pattern:** Immediate intensification (0.1 → 0.25). Flow feel — directional haptic sweeping from the river's source toward its destination.
- **Character:** Being carried — the energy wants to move you along its path
- **Duration:** Sustained while in contact

### Building Sync (3 Layers)

**Layer 1 — Surface Pulse:**
- **At 33% sync:** Primary rhythm establishes (0.2, 1.5 Hz, steady)
- **Character:** Your footsteps matching the river's rhythm — walking in time

**Layer 2 — Deep Resonance:**
- **At 66% sync:** Secondary pulse adds beneath the first (0.15, 0.75 Hz — half-speed, double-depth)
- **Character:** The river's deeper current. Feels like standing on a sleeping giant's chest.

**Layer 3 — Harmonic Unity:**
- **At 100% sync:** Third layer — high-frequency shimmer (0.1, 4 Hz, tingling)
- **Combined:** All three layers active simultaneously: surface pulse + deep resonance + harmonic shimmer
- **Character:** Perfect synchronization with the ley-line. The player IS part of the grid in this moment.

### Peak Perfect Sync
- **When maintained for 5+ seconds:** Brief overload pulse (0.5, 200ms) → warm sustained glow (0.3, 3 seconds)
- **Character:** The ley line acknowledges you. Bonus Aether reward.
- **Visual sync:** Golden aura around player intensifies

### Drift Warning
- **When sync begins to decay:** Rhythm stutters — missed beats, irregular timing (0.15, erratic)
- **Character:** Losing the connection — the river is moving on without you
- **Recovery:** Re-match within 3 seconds to avoid full reset

---

## Formation Flight with Armada

*Multiple airships moving in coordinated harmonic patterns.*

### Base Sync (Solo Flight Near Armada)
- **Context:** Flying near NPC airships
- **Pattern:** Engine hum (0.15) with additional harmonic overlay from nearby ships (0.05 per ship, max 0.2 total)
- **Character:** Awareness of fleet — you're not alone in the sky

### Joining Formation
- **Trigger:** Player aligns with formation position markers
- **Pattern:** Harmonic pulses pull you into position — gentle directional nudges (stronger on the side you need to move toward)
- **Lock-in:** Clean snap (0.3, 50ms) when position achieved
- **Character:** Magnetic — the resonance wants you in the right place

### Perfect Alignment
- **When all ships in formation:** Unified pulse (0.2, 2 Hz, all ships synchronized)
- **Character:** The fleet IS one entity. Every ship's engine contributes to a single combined resonance.
- **Visual:** Formation leaves golden contrails. Haptic confirms: formation bonuses active.

### Maneuvering in Formation
- **Banking together:** Synchronized directional haptics — the formation tilts as one
- **Speed change:** Unified acceleration/deceleration pulse (0.25 → 0.35 for speed-up, reverse for slow-down)
- **Formation break (combat):** Sharp scattered disruption (0.4, 100ms) — the unity broken — then return to solo flight haptics

### Companion Ship Layers
- **Thorne's flagship:** Deep authoritative pulse (0.2, lower frequency overlay)
- **Scout wings:** Light, quick flutter (0.1, fast irregular)
- **Cargo haulers:** Heavy slow pulse (0.15, 0.5 Hz)
- Each companion ship type adds its signature to the formation's combined haptic, creating a "living" fleet feel.

### Climactic Formation Overload
- **During DLC 5 armada raid:** All ships fire simultaneously
- **Pattern:** Combined recoil (0.9, 120ms) → harmonic ring cascade (each ship's cannon ring overlapping at 50ms offsets for 600ms total)
- **Character:** Devastating coordinated strike — the most intense formation haptic
- **Aftermath:** Brief silence (200ms) → unified hum resumes stronger (0.25)

---

## Overload Climax Release

*When resonance systems hit critical mass — the most intense non-cosmic haptic.*

### Critical Build-Up
- **Context:** Any system approaching overload (organ requiem, bell ring, grid surge)
- **Pattern:** Intensity ramps continuously (0.4 → 0.8 over 5–10 seconds). Pulse rate accelerates (2 Hz → 12 Hz).
- **Character:** Everything building. Tension. Almost too much. The device feels like it's vibrating apart.
- **UI warning:** HUD elements tremble in sync with haptics

### Climax Trigger
- **The moment of release:**
- **Pattern:** PEAK (1.0 intensity, 150ms solid hold) — the loudest haptic moment in the standard game
- **Then:** Immediate drop to silence (0ms — everything stops, including ambient haptics)
- **Silent beat:** 500ms of absolute haptic silence. Powerful. The absence is felt.

### Chain-Light Cascade
- **After the silent beat:** Cascade begins
- **Pattern:** Radiating pulses from center outward — 5 waves, each at diminishing intensity (0.7, 0.5, 0.4, 0.3, 0.2) with 150ms gaps
- **Character:** The overload energy rippling outward through the world
- **Duration:** 1.5 seconds total cascade

### Afterglow
- **Post-cascade:** Warm sustained hum (0.25) slowly decays to new ambient baseline over 8 seconds
- **Character:** The world settling into its new, more resonant state
- **New baseline:** Ambient haptics permanently slightly stronger in zones affected by the overload (+0.02 base intensity)

---

## Supernova Burst Thump

*The single most powerful haptic event in the game — reserved for once-per-campaign moments.*

### Trigger Moment
- **Context:** Planetary Bell-Tower Ring peak, True Timeline crack, Korath's sacrifice
- **Pattern:** 1.0 intensity, 200ms sustained hold. No modulation — pure maximum output.
- **Character:** Not a tap, not a pulse — a THUMP. The device communicates: this is the biggest moment.

### Cascade Decay
- **Immediately following:** 3-tier decay
  1. Heavy resonant ring (0.7, 500ms, slowly modulating)
  2. Medium warm sustained (0.4, 2 seconds)
  3. Light ambient shimmer (0.2, 5 seconds, gradually fading)
- **Total duration:** ~8 seconds
- **Character:** An echo that takes time to settle — the world processing what just happened

### Extended Afterglow
- **For 60 seconds post-event:** Ambient haptics elevated (base +0.1)
- **Character:** The player's hands still remember. The world has permanently changed.

---

## Planetary Bell-Tower Peal

*Moon 12 climax — every bell on the planet rings at once.*

### Pre-Peal Charge
- **Context:** Player activating central bell tower
- **Pattern:** Building tension — ascending intensity pulses (0.2, 0.3, 0.4, 0.5 at 500ms intervals)
- **Character:** Counting down to the moment. Each pulse is a tower preparing.
- **Duration:** 2 seconds

### Unified Peal Trigger
- **The strike:**
- **Pattern:** Clean bell strike (0.8, 100ms) → sustained ring (0.6, oscillating at bell harmonic frequency, 3 seconds)
- **Character:** One bell, perfectly clear. Then…

### Cascade Continental Wave
- **As other towers join:** Each tower adds a ring layer
- **Pattern:** Overlapping rings build (0.6 → 0.7 → 0.8 → 0.9 as more towers join over 5 seconds)
- **Character:** A wave of sound circling the planet — felt as a sweeping directional haptic (the wave's direction trackable through haptic flow)
- **Peak:** All towers active — sustained complex chord (0.9, multi-frequency overlay, 5 seconds)
- **Supernova trigger:** At peak → Supernova Burst Thump (see above)

### Extended Afterglow
- **Post-peal:** All bell towers leave a residual ring in the haptic layer
- **Pattern:** Fading harmonics (0.3 → 0.05 over 30 seconds)
- **Character:** The world still vibrating. Every surface humming. Gradually settling into a new, more resonant normal.

---

## True Timeline Merge

*Moon 13 climax — reality cracks open.*

### Pre-Merge Tension
- **Context:** All convergence conditions met, 13th Moon overhead, 17th hour
- **Pattern:** Dual-layer — steady grid hum (0.3, 2 Hz) + unstable crackle overlay (0.1–0.3 random spikes)
- **Character:** Reality straining. The grid is at maximum capacity. Something has to give.
- **Duration:** 10–15 seconds of building tension

### Merge Trigger
- **Player activates convergence node:**
- **Pattern:** SILENCE (200ms) → then the biggest haptic event in the game:
  - Supernova Burst Thump (1.0, 200ms)
  - Followed immediately by: reality TEAR — a unique haptic: sharp descending then ascending sweep (0.8, 300ms, feels like something ripping)
  - Then splitting sensation: dual competing rhythms simultaneously (left side: Golden Age steady warmth 0.3; right side: Eternal Silence cold stutter 0.2)
- **Character:** The timeline literally splitting. The player feels two realities pulling in opposite directions.

### Cascade Golden Age Emergence
- **If player chooses Golden Age:**
- **Pattern:** The warm left-side haptic wins — expands to fill both sides (0.3 → 0.5 over 3 seconds)
- **Steady golden pulse establishes** (0.4, 1 Hz — heartbeat of the new world)
- **Cold right side fades** (0.2 → 0 over 2 seconds)
- **Character:** Resolution. Warmth. The right timeline won.
- **Final pulse:** Deep satisfying THUMP (0.6, 150ms) — the merge is complete.

### Extended Afterglow
- **Post-merge ambient:** Permanently elevated (0.2 base + gentle 432 Hz rhythm)
- **Character:** The world has a heartbeat the player can always feel. This is the new constant.
- **Duration:** Permanent (post-game state)

---

## Day Out of Time Festival

*Post-Moon 13 celebration — haptic joy and cosmic play.*

### Activation
- **Context:** Festival begins, all systems online
- **Pattern:** Festive pulse — bright, rhythmic, musical (0.3, 3 Hz with playful irregularity)
- **Character:** Celebration. Energy. No tension — only joy.

### Choir
- **All NPC voices singing simultaneously:**
- **Pattern:** Complex multi-layer rhythm (3–5 overlapping pulse patterns at 0.1–0.2 each)
- **Character:** A crowd singing — felt as a rich textured vibration, no single dominant pulse

### Aurora Veil Dance
- **Festival aurora event:**
- **Pattern:** Flowing, ethereal (0.1–0.2, smooth sine wave with color-temperature variation)
- **Character:** Dancing with light itself (see Aurora Veil Dance Ripples below for detail)

### Fountain Cascade
- **All fountains activate in sequence:**
- **Pattern:** Ascending water-feel pulses (0.15 → 0.3, each fountain adding a layer every 500ms)
- **Character:** Liquid joy — the waters celebrating

### Armada Flyover
- **Formation passes overhead:**
- **Pattern:** Building engine presence (0.1 → 0.3 as fleet approaches → 0.3 → 0.1 as it passes)
- **Directional:** Haptic sweeps from the approach direction to the departure direction
- **Contrails:** Tailing shimmer (0.1, 2 seconds post-passage)

### Planetary Sigh
- **Festival finale:**
- **Pattern:** All haptics synchronize into a single deep exhalation — intensity rises (0.3 → 0.5 over 3 seconds) then slowly releases (0.5 → 0.05 over 10 seconds)
- **Character:** The planet breathing out — relief, completion, peace
- **Final silence:** 3 seconds of absolute haptic silence. The world at rest.
- **New baseline:** Lowest possible ambient (0.05) — permanent post-game serenity

---

## Aurora Veil Dance Ripples

*Interactive aurora experience — tracing the lights with your fingers.*

### Entry
- **Trigger:** Player enters aurora veil (ground-level aurora during festival or high-altitude encounter)
- **Pattern:** Gentle initial wash (0.1, 500ms smooth onset)
- **Character:** Like stepping into warm water — enveloping, gradual

### Flow & Tracing
- **When player moves through/traces aurora bands:**
- **Pattern:** Directional flow matching finger/movement direction (0.15–0.25)
- **Texture:** Rippling — smooth but with micro-variations every 100ms (0.02 variance)
- **Color response:** Different aurora colors produce different haptic textures:
  - Green: smooth flowing (standard sine)
  - Blue: crisp and cool (sharp-edged pulses)
  - Violet: warm and deep (lower frequency, broader pulses)
  - Gold: sparkling (rapid micro-taps overlay)

### Peak Sync
- **When player finds the aurora's rhythm (timing-based):**
- **Pattern:** Intensity doubles (0.25 → 0.5). Rhythm locks — player and aurora in perfect harmony.
- **Duration:** Maintained as long as sync holds (5–20 seconds)
- **Character:** Transcendence — the player and the cosmic light are one

### Release
- **Breaking sync or exiting veil:**
- **Pattern:** Graceful fade (0.5 → 0.1 over 2 seconds)
- **Character:** Gentle separation — no jarring transitions

---

## Swirling Spiral Ripple Texture

*The sensation of resonance spiraling outward from a central point.*

### Initiating Spiral
- **Trigger:** Any radial resonance event (dome activation, fountain bloom, bell tower ring expanding)
- **Pattern:** Rotational haptic — intensity sweeps clockwise (or counterclockwise for corruption) around the device
- **One revolution:** 500ms
- **Starting intensity:** 0.2 (center) → 0.15 (edge) — spiral carries energy outward
- **Character:** Like holding a spinning lens of energy

### Building Depth
- **As spiral grows:** Revolution speed increases (500ms → 300ms → 200ms)
- **Layers:** Multiple spiral arms develop (2nd arm at 180° offset, then 3rd at 120°)
- **Intensity escalation:** 0.2 → 0.3 → 0.4 as arms multiply
- **Character:** Deepening vortex — the resonance gathering strength

### Peak Resonance
- **Maximum spiral state:**
- **Three spiral arms, 200ms revolution, 0.5 intensity**
- **Texture:** Smooth but with a "grain" — like feeling the golden ratio in the spiral's spacing
- **Duration:** Sustained 3–5 seconds at peak before resolving

### Releasing
- **Spiral slows and dissipates:**
- **Pattern:** Revolution speed decays (200ms → 500ms → 1000ms). Arms merge back to single spiral. Intensity fades (0.5 → 0.1).
- **Final pulse:** Single centered tap (0.3, 50ms) — the spiral collapsing to a point
- **Character:** Energy settling, order returning

---

## Peak Vortex Swell

*Maximum resonance vortex — the spiral's ultimate expression.*

### Pre-Swell
- **Context:** Grid or system approaching critical resonance (precedes overloads, convergences)
- **Pattern:** Compressed spiral (very tight, fast revolution — 150ms, 0.4 intensity, 4 arms)
- **Character:** Pressure building. The vortex is drawn tight like a bowstring.
- **Duration:** 3–5 seconds

### Trigger
- **The release:**
- **Pattern:** Spiral EXPLODES outward — revolution instantly slows to 1000ms while intensity peaks (0.8)
- **The feel:** Like the device expanded. The tight vortex became vast.
- **Single-frame hard pulse:** (0.9, 50ms) at the moment of expansion — the "pop"

### Cascade Resonance
- **Post-expansion:** Multiple concentric spiral waves propagate outward
- **Pattern:** 3 waves at 300ms intervals, diminishing (0.6, 0.4, 0.2)
- **Each wave:** A single slow revolution carrying the released energy
- **Character:** Shockwave — but beautiful. Expanding rings of resonance.

### Release
- **Settling:** Vortex decays to ambient warmth (0.2, gentle non-directional pulse)
- **Duration:** 5 seconds transition
- **Character:** Peace after power

---

## Layered Spiral Shockwaves

*Multiple overlapping spiral events — used for planetary-scale haptics.*

### Pre-Shockwave
- **Context:** Planetary Bell-Tower peal cascade, continental ley-line activation
- **Pattern:** Multiple small spirals activate at different points (each at 0.15 intensity, 0.5 Hz)
- **Character:** Scattered energy centers preparing to synchronize
- **Duration:** 3 seconds

### Ignition
- **Spirals synchronize:**
- **Pattern:** All spirals snap to unified revolution speed (300ms) and phase-lock
- **Combined intensity:** Each spiral's 0.15 combines (4 spirals = 0.6 total)
- **Character:** Multiple energy sources becoming one — cohesion from chaos

### Cascading Resonance
- **Shockwaves propagate:**
- **Pattern:** Phase-locked spiral waves expand from each source, overlap and constructively interfere
- **Where waves overlap:** Intensity peaks (up to 0.8 at constructive interference points)
- **Movement:** Haptic sweeps across the device as wave fronts cross the player's "position"
- **Duration:** 5–8 seconds of cascading, overlapping, building waves
- **Character:** A planetary heartbeat felt through layers — complex, rich, awe-inspiring

### Release
- **Waves dissipate:** Interference patterns simplify as energy disperses
- **Final state:** Single unified pulse replacing the multiple spirals (0.3, 1 Hz)
- **Character:** Unity achieved — many became one

---

## Cataclysmic Spiral Slam

*The absolute maximum haptic event — reserved for the True Timeline merge and final boss defeat.*

### Pre-Slam Tension
- **Context:** Reality cracking, timeline converging, Dissonant One final phase
- **Pattern:** ALL haptic systems active simultaneously at reduced intensity
  - Spiral vortex (tight, fast, 0.3)
  - Layered shockwaves (building, 0.2)
  - Bell tower reverb (sustained ring, 0.2)
  - Engine hum (maxed, 0.3)
  - Irregular corruption crackle (0.1–0.2)
- **Total perceived intensity:** ~0.7–0.8 (all layers combined)
- **Character:** Everything happening at once. The world at capacity.
- **Duration:** 5 seconds of maximum chaos

### Trigger
- **THE SLAM:**
- **Pattern:** ALL haptic activity ceases for exactly 200ms (the breath before). Then:
  - 1.0 intensity, 250ms sustained SLAM — device at absolute maximum
  - Immediately: spiral slam — explosive outward spiral (single revolution at 1.0 intensity over 300ms)
  - Then: total silence for 500ms
- **Battery protection:** Even at low battery, this event fires at minimum 0.7 intensity
- **Character:** Cataclysm. Finality. The biggest single moment in the entire game.

### Spiral Shockwave Cascade
- **After silence:** 
- **5 diminishing shockwaves** (0.7, 0.5, 0.4, 0.3, 0.2) at 400ms intervals
- Each wave is a full spiral revolution, slowing from 200ms to 600ms
- **Character:** Aftershock — the world rebounding from the impact

### Release
- **Extended settle:** 15 seconds of slowly decaying ambient
- **0.3 → 0.05 over 15 seconds**
- **Final state:** New permanent ambient — either warm (Golden Age) or cold (Silence) or dual (Both)
- **Character:** The new world establishing its baseline. The game's haptic identity has permanently changed.

---

## Accessibility & Battery Considerations

### Accessibility Modes
- **Reduced Haptics:** All intensities halved (multiply by 0.5). No directional effects. Recommended for haptic-sensitive players.
- **Essential Only:** Only Category 2 (combat) and Category 6 (UI) haptics active. All ambient/environmental disabled.
- **Off:** Complete haptic disable. Visual indicators substitute for all haptic cues.
- **Custom:** Per-category intensity sliders (0–100%) for each of the 6 general categories.

### Battery-Aware Scaling
| Battery Level | Intensity Multiplier | Disabled Features |
|---|---|---|
| 50–100% | 1.0× (full) | None |
| 30–49% | 0.8× | Ambient environmental (Cat. 5) reduced further to 0.5× |
| 15–29% | 0.6× | Ambient disabled. Only interactive + dramatic + cosmic remain. |
| <15% | 0.4× | Only combat + UI + cosmic events. Reduced to minimum effective intensity. |

**Exception:** Supernova Burst Thump and Cataclysmic Spiral Slam always fire at minimum 0.7× intensity regardless of battery — these are once-per-campaign events that must land.

### Visual Substitutions (Haptics Off)
When haptics are disabled, every haptic cue must have a visual or audio equivalent so no gameplay information is lost.

| Haptic Category | Visual Substitute | Audio Substitute |
|---|---|---|
| **Ambient (Cat. 1)** | Subtle screen-edge glow pulse | Low-frequency ambient hum |
| **Combat (Cat. 2)** | Screen shake + red flash on damage, white flash on hit | Impact SFX at higher volume |
| **Dramatic (Cat. 3)** | Slow-motion camera + radial blur | Orchestral swell |
| **Cosmic (Cat. 4)** | Full-screen bloom + particle burst | Deep bass rumble + choir |
| **Environmental (Cat. 5)** | Environmental particle increase | Spatial audio cue |
| **UI (Cat. 6)** | Button scale animation + highlight | Click/confirm SFX |

### Deaf/HOH Haptic Enhancement
For deaf and hard-of-hearing players, haptics become a PRIMARY information channel. When the audio accessibility profile is active:
- All frequency-based audio cues gain paired haptic patterns at +30% intensity
- Bell Tower Synchronization mini-game adds stronger haptic rhythm cues
- Enemy frequency indicators use distinct haptic textures (not just pitch)
- Pipe Organ Symphony Conduction adds visual waveform overlay + haptic beat guide

### Performance Sync
- All haptic patterns are frame-synced via `CADisplayLink` callback
- Haptic events queued in `CHHapticPatternPlayer` — minimum 16ms granularity
- Complex layered haptics (e.g., Formation Flight with 5+ ships) may simplify on older A-series chips
- Profiling target: haptic computation ≤ 0.5ms per frame

---

*Your hands are the final interface between the player and the world.*
*Every haptic event is a promise: this world is real, alive, and worth rebuilding.*

---

*See also:*
- [09_TECHNICAL_SPEC.md](09_TECHNICAL_SPEC.md) — Core Haptics API implementation
- [07_MOBILE_UX.md](07_MOBILE_UX.md) — Touch + haptic integration
- [12_VIVID_VISUALS.md](12_VIVID_VISUALS.md) — Visual events paired with these haptics

---

**Document Status:** FINAL  
**Author:** Nathan / Resonance Energy  
**Last Updated:** March 25, 2026
