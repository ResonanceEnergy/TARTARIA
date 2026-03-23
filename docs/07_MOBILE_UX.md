# TARTARIA WORLD OF WONDER — Mobile UX Design
## Touch Controls, Session Design, Accessibility & Onboarding

---

## Table of Contents

1. [UX Philosophy](#1-ux-philosophy)
2. [Session Design](#2-session-design)
3. [Touch Control Scheme](#3-touch-control-scheme)
4. [HUD & Interface Layout](#4-hud--interface-layout)
5. [Onboarding Flow](#5-onboarding-flow)
6. [Daily & Weekly Engagement Loops](#6-daily--weekly-engagement-loops)
7. [Notification Strategy](#7-notification-strategy)
8. [Accessibility](#8-accessibility)
9. [Performance UX](#9-performance-ux)
10. [Wireframe Concepts](#10-wireframe-concepts)

---

## 1. UX Philosophy

**One-Thumb Wonder:** Every core action in the game can be performed with one thumb while holding the phone in one hand. Complex actions use two hands but are never required.

**Core Principles:**
1. **5-minute entry, 15-minute sessions, 30-minute deep dives.** Every session length is valid and rewarding.
2. **No dead time.** Loading screens show lore fragments. Transitions feature ambient Aether animations. Every second has value.
3. **Wonder over clutter.** The screen should feel like a window into the Tartarian world, not a spreadsheet. Minimize HUD. Maximize beauty.
4. **Interrupt-safe.** Save state every 10 seconds. Any interruption (call, notification, app switch) loses zero progress.
5. **Progressive disclosure.** Show only what the player needs NOW. Advanced systems reveal as they become relevant.
6. **Haptic quality.** Every tap, swipe, and interaction has a tailored haptic response. The phone should feel resonant.

---

## 2. Session Design

### 2.1 Session Types

| Type | Duration | Activities | Reward Density |
|---|---|---|---|
| **Micro** | 1–3 min | Daily harvest, quick check-in, send kids to build | 1 reward |
| **Short** | 5–10 min | One excavation + tuning + combat encounter | 3–5 rewards |
| **Standard** | 10–20 min | Full zone progress: excavate, build, tune, fight, explore | 8–12 rewards |
| **Deep Dive** | 20–45 min | Multi-zone, story progression, boss fights, train/airship journeys | 15–25 rewards |
| **Epic** | 45–90 min | Moon climax events, discoveries, planetary events | 30+ rewards |

### 2.2 Session Pacing

```
Minute 0–1:    ARRIVAL — Zone loads, ambient hum, Milo says something
Minute 1–3:    DISCOVERY — Scan reveals buried feature, excavation begins
Minute 3–5:    ACTION — First combat or tuning challenge
Minute 5–8:    RESTORATION — Building / precision cutting mini-game
Minute 8–10:   PAYOFF — Structure activates, ley line lights up, NPC reacts
Minute 10–12:  CHOICE — Continue deeper or bank progress
Minute 12–15:  CLIMAX — Boss encounter or major reveal (if continuing)
Minute 15:     NATURAL PAUSE — Session recap screen + "come back for harvest"
```

### 2.3 Interruption Recovery

| Interruption | Recovery |
|---|---|
| Phone call | Auto-pause, resume exactly where you were |
| App switch (<30s) | Instant resume, no reload |
| App switch (30s–5min) | Quick resume with 2-second re-orient animation |
| App killed | Restore from last auto-save (max 10 seconds lost) |
| Battery warning (<5%) | Auto-save + graceful exit prompt |
| Low signal | Full offline support; sync when signal returns |

---

## 3. Touch Control Scheme

### 3.1 Core Controls

| Context | One-Thumb (Minimal) | Two-Thumb (Standard) | Advanced |
|---|---|---|---|
| **Move** | Tap destination | Virtual joystick (left) | Tilt-to-move (optional) |
| **Camera** | Auto-follow | Drag right side | Pinch zoom |
| **Interact** | Tap object | Tap + context button | Long-press for options |
| **Combat** | Tap target → auto-attack | Left move + right swipe attack | Draw patterns for abilities |
| **Build** | Tap blueprint → tap placement | Drag to position, pinch to rotate | Two-finger rotate + scale |
| **Excavate** | Swipe over mud | Multi-finger fast clear | Shake phone (optional) |
| **Giant Mode** | Double-tap giant icon | Same | Same + tilt for direction |
| **Tune** | Follow on-screen prompts | Manual frequency selection | Free-play mode |

### 3.2 Gesture Dictionary

| Gesture | Action | Context |
|---|---|---|
| Tap | Select / Interact / Attack | Universal |
| Double Tap | Activate special (Giant Mode / Ground Slam) | Combat / Giant |
| Swipe Right | Dodge right / Navigate forward | Combat / Exploration |
| Swipe Left | Dodge left / Navigate back | Combat / Exploration |
| Swipe Up | Charged attack / Jump | Combat |
| Swipe Down | Guard / Crouch | Combat |
| Long Press | Inspect / Options menu | Universal |
| Pinch | Zoom camera | Exploration / Building |
| Two-finger Rotate | Rotate camera / Rotate building | Exploration / Building |
| Draw Circle | Cymatic ability activation | Combat (Moon 3+) |
| Draw Spiral | Golden spiral cut | Rock cutting mini-game |
| Three-finger Tap | Pause / Menu | Universal |

### 3.3 Control Customization

- **Joystick position:** Moveable anchor point (left, center-left, right)
- **Button size:** Small / Medium / Large
- **Sensitivity sliders:** Camera rotation, zoom speed, gesture threshold
- **Handedness:** Left-hand and right-hand layouts
- **Tap vs. Hold:** Configurable per action (e.g., attack on tap or hold)

---

## 4. HUD & Interface Layout

### 4.1 In-Game HUD (Minimal Mode)

```
┌──────────────────────────────────────────────┐
│ [RS: 73]                          [Moon 5/13]│
│                                              │
│                                              │
│                                              │
│                  GAME WORLD                  │
│              (90% of screen)                 │
│                                              │
│                                              │
│                                              │
│ [⚡Aether]  [💬Companion]     [🎵Tune] [⚙️] │
└──────────────────────────────────────────────┘
```

**HUD Elements (always visible):**
- **RS Counter** (top-left): Zone resonance score — pulses when increasing
- **Moon Counter** (top-right): Current Moon / 13 — tiny glyph, not number
- **Aether Meter** (bottom-left): Current energy — fills upward like mercury in a thermometer
- **Companion Chat** (bottom-center-left): Latest companion line + tap to see history
- **Tuning Wheel** (bottom-right): Frequency selector (appears only in combat)
- **Settings Gear** (bottom-far-right): Menu access

**Design Rule:** Total HUD covers <10% of screen area. The world is the star.

### 4.2 Combat HUD (Expands Automatically)

```
┌──────────────────────────────────────────────┐
│ [RS: 73]  [COMBO: 6x]           [Moon 5/13] │
│                                              │
│                                              │
│                  COMBAT                      │
│                                              │
│                                              │
│                                              │
│ [⚡Aether]  [Giant: 80%]   [🎵174] [🛡️] │
│ [Dodge←]            [Dodge→]   [Blast]       │
└──────────────────────────────────────────────┘
```

Addtional combat elements:
- **Combo Counter** (top-center): Grows with chain hits, pulses at 3/6/9
- **Giant Meter** (bottom-center): Fills from kills and building
- **Frequency Display** (bottom-right): Current active frequency
- **Dodge Zones** (bottom corners): Swipe zones for dodging

### 4.3 Building Mode HUD

```
┌──────────────────────────────────────────────┐
│ [Blueprint: Cathedral]       [Materials: ✓✓✗]│
│                                              │
│    [Golden guides visible as ghost lines]    │
│                                              │
│         BUILDING PLACEMENT VIEW              │
│         (isometric top-down shift)           │
│                                              │
│                                              │
│ [Rotate]  [Scale]  [Snap: ON]    [✓Place]   │
│ [Cancel]              [Template: Seed of Life]│
└──────────────────────────────────────────────┘
```

### 4.4 Map Screen

```
┌──────────────────────────────────────────────┐
│            CONTINENTAL MAP                   │
│                                              │
│    [Zone 1: 100%]═══[Zone 2: 73%]           │
│         ║                  ║                 │
│    [Zone 3: 45%]   [Zone 4: ???]            │
│         ║                                    │
│    [Zone 5: 12%]                             │
│                                              │
│ [🚂Train Routes]  [✈️Airship Routes]        │
│ [Global RS: 42%]         [Active Moon: 5]    │
└──────────────────────────────────────────────┘
```

Zones pulse with RS-proportional glow. Ley lines drawn as golden threads between connected zones. Trains and airships visible as tiny moving icons.

---

## 5. Onboarding Flow

### 5.1 First 30 Minutes (Moon 1, Days 1–2)

| Minute | Player Does | System Teaches | Emotional Target |
|---|---|---|---|
| 0–3 | Watch prologue cinematic | Nothing — pure wonder | AWE |
| 3–5 | Swipe to excavate first wall | Gesture: swipe to clear | CURIOSITY |
| 5–7 | Tap revealed structure | Interaction: tap to examine | DISCOVERY |
| 7–9 | Follow Milo to dome interior | Movement: tap-to-move | GUIDED |
| 9–11 | Simple 3-note organ tune | Mini-game: tuning | SATISFACTION |
| 11–13 | Watch dome light up | RS system (passive introduction) | DELIGHT |
| 13–15 | Place first spire | Building: guided placement | OWNERSHIP |
| 15–18 | Combat tutorial (Reset scouts) | Tap-to-attack, swipe-to-dodge | EMPOWERMENT |
| 18–20 | First giant-mode burst | Giant mode: spectacular power | THRILL |
| 20–25 | Explore restored cathedral | Free exploration + Lirael appears | WONDER |
| 25–30 | First ley line lights up | Grid system (visual, no UI) | MOTIVATION |

### 5.2 First Week (Moon 1 complete)

By end of Moon 1, player should:
- ✅ Know all basic controls
- ✅ Have built their first dome + spire
- ✅ Have fought and won basic combat
- ✅ Have experienced giant mode
- ✅ Understand RS intuitively (not numerically)
- ✅ Have met Milo and Lirael
- ✅ Feel motivated to discover what's beyond the first ley line

### 5.3 Tooltip Strategy

**No pop-up tutorials after onboarding.**

Instead:
- Companion dialogue hints ("Have you tried the tuning wheel, spark?" — Milo)
- Visual ghost-guides (golden lines showing optimal placement)
- Context-sensitive icons (build icon appears near buildable sites)
- Discovery rewards (try a new action → immediate positive feedback)

---

## 6. Daily & Weekly Engagement Loops

### 6.1 Daily Loop (2–5 minutes)

| Activity | Duration | Reward |
|---|---|---|
| **Aether Harvest** | 30s | Collect overnight generation |
| **Junior Architect Report** | 30s | See what children built overnight |
| **Daily Fortune** | 15s | Moon-phase bonus (rotation) |
| **Quick Tune** | 60s | Maintain RS on best building |
| **Milo's Deal** | 30s | Buy/sell one item at "special" price |

### 6.2 Weekly Loop (20–30 minutes)

| Activity | Duration | Reward |
|---|---|---|
| **Moon Phase Shift** | 5 min | New Moon modifier activates |
| **Weekly Excavation** | 10 min | Major dig site refreshes |
| **Lirael's Recital** | 5 min | Children's choir performance (buffs) |
| **Thorne's Patrol** | 5 min | Airship reconnaissance reveals hidden sites |
| **Community Challenge** | -- | Global RS contribution leaderboard |

### 6.3 Seasonal Loop (Per Moon — ~4 weeks)

| Week | Theme | Key Activity |
|---|---|---|
| Week 1 | Discovery | New zone opens, excavation focus |
| Week 2 | Restoration | Building and tuning focus |
| Week 3 | Conflict | Combat and defense missions |
| Week 4 | Revelation | Story climax + lore drops |

---

## 7. Notification Strategy

### 7.1 Notification Philosophy

**Never annoy. Always intrigue.**

Players should WANT to open notifications because they're written in-character and offer genuine value.

### 7.2 Notification Types

| Type | Frequency | Character | Example |
|---|---|---|---|
| **Harvest Ready** | Every 8h (max) | Milo | "Your Aether tanks are full, spark. I may have skimmed a bit." |
| **Construction Complete** | On event | Lirael | "The children finished the dome. It's smiling." |
| **Story Available** | New Moon | Thorne | "New signal on the horizon. Time to fly." |
| **Community Event** | Weekly | Aether Whisper | "The 13 Moons align. The grid awaits." |
| **Comeback** | After 3 days absent | Korath's echo | "The stones remember you. Will you return?" |

### 7.3 Notification Rules

- **Maximum 2 per day** (hard cap regardless of events)
- **Always dismissable** without losing content
- **Never FOMO-driven** (no "YOU'RE MISSING OUT" language)
- **Respect Do Not Disturb** (no night notifications)
- **Earnable opt-out** per category

---

## 8. Accessibility

### 8.1 Visual Accessibility

| Feature | Description |
|---|---|
| **Color-blind modes** | Protanopia, Deuteranopia, Tritanopia presets — all frequency/combat tells use shape + pattern in addition to color |
| **High contrast mode** | Increases edge contrast on interactive elements |
| **Text scaling** | 75%–200% of default |
| **Subtitle background** | Optional opaque background for all dialogue |
| **Reduced motion** | Disables parallax, particle density, and screen shake |
| **Screen reader support** | VoiceOver (iOS) descriptions for all buttons and states |

### 8.2 Motor Accessibility

| Feature | Description |
|---|---|
| **One-hand mode** | All inputs mapped to single-thumb reach zone |
| **Switch control** | iOS Switch Control compatibility |
| **Auto-dodge** | Optional automatic evasion at low health |
| **Hold vs. tap** | All hold-actions have tap equivalents |
| **Adjustable timing** | Quick-time events have extended timing mode (2× window) |
| **Touch target size** | Minimum 44×44pt (Apple HIG) for all interactive elements |

### 8.3 Cognitive Accessibility

| Feature | Description |
|---|---|
| **Quest log** | Clear, numbered, current-objective highlighted |
| **Mini-map breadcrumbs** | Path to current objective always shown |
| **Difficulty adjustment** | Can change at any time without penalty |
| **Session recap** | On return, brief "last time you..." summary |
| **Build assist** | Optional auto-snap to best golden-ratio position |
| **Frequency hint** | Optional enemy weakness indicator (always visible) |

### 8.4 Auditory Accessibility

| Feature | Description |
|---|---|
| **Visual audio indicators** | Frequency combat tells shown as screen-edge pulses |
| **Haptic audio mapping** | Distinct vibration patterns for each frequency band |
| **Closed captions** | All environmental audio described |
| **Mono audio** | Full mix in both/either ear |
| **Sign language NPC** | Echo NPC in hub zone uses ASL/BSL gestures |

---

## 9. Performance UX

### 9.1 Loading Strategy

| Scenario | Target Time | User Experience |
|---|---|---|
| Initial launch | <3 seconds | Aether swirl animation + lore quote |
| Zone transition | <1.5 seconds | Train/airship travel animation (masked load) |
| Building placement | Instant | Assets pre-loaded during zone exploration |
| Combat start | <0.5 seconds | Seamless — no loading screen |
| Map screen | <1 second | Low-detail map → detail streams in |

### 9.2 Battery-Aware Mode

**Auto-activates at 20% battery or when device temperature rises:**

| Adaptation | Effect |
|---|---|
| Frame rate | 60 FPS → 30 FPS |
| Particle density | 100% → 50% |
| Shadow quality | Dynamic → Baked only |
| Resolution | Native → 75% |
| Background sync | Paused |
| Haptic feedback | Reduced |

**Player notification:** Subtle battery icon in HUD + "Power Save mode active"

### 9.3 Storage Management

| Component | Size | Details |
|---|---|---|
| **Core app** | ~800 MB | Onboarding + Moon 1 content |
| **Per-Moon content** | ~200 MB each | Streaming download, can pre-download on Wi-Fi |
| **Full game** | ~3.5 GB | All 13 Moons + assets |
| **Save data** | <50 MB | Local + iCloud sync |
| **Cache** | ~200 MB | Evictable texture + audio cache |

**Smart Downloads:** Game pre-downloads next Moon's assets when on Wi-Fi + charging. Players never see a download screen during gameplay.

---

## 10. Wireframe Concepts

### 10.1 Main Gameplay Screen

```
┌────────────────────────────────────────────────────┐
│  RS: ████░░ 73%              ☾ Moon 5 / Day 14    │
│                                                    │
│                                                    │
│                                                    │
│            ╔═══════════════╗                       │
│            ║  RESTORED     ║                       │
│            ║  CATHEDRAL    ║                       │
│            ║  (glowing)    ║                       │
│            ╚═══════════════╝                       │
│                  ↕                                  │
│         ═══ Ley Line ═══                           │
│                                                    │
│  ⚡████░░     💬"Not bad,    🎵[174]  [285]  ⚙   │
│  Aether       spark!"        [396]  [528]         │
└────────────────────────────────────────────────────┘
```

### 10.2 Building Placement Screen

```
┌────────────────────────────────────────────────────┐
│  Blueprint: Grand Dome          Materials: ✓ ✓ ✗   │
│                                                    │
│         ·  ·  ·  ·  ·  ·  ·  ·  ·                │
│         ·     ╭─────────╮     ·                    │
│         ·    │  GOLDEN   │    ·     ← Ghost guide  │
│         ·    │  SPIRAL   │    ·        (gold line) │
│         ·     ╰─────────╯     ·                    │
│         ·  ·  ·  ·  ·  ·  ·  ·  ·                │
│                                                    │
│  [↻ Rotate]  [↔ Scale]  [📐 Snap: ON]            │
│  [✗ Cancel]              [✓ PLACE]  [📋 Template] │
└────────────────────────────────────────────────────┘
```

### 10.3 Tuning Mini-Game Screen

```
┌────────────────────────────────────────────────────┐
│           ORGAN CONSOLE — 3-6-9 Sequence           │
│                                                    │
│    ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐   │
│    │ 1 │ │ 2 │ │ 3 │ │ 4 │ │ 5 │ │ 6 │ │ 7 │   │
│    └───┘ └───┘ └─★─┘ └───┘ └───┘ └─★─┘ └───┘   │
│            ┌───┐                 ┌───┐             │
│            │ 8 │                 │ 9 │  ← 3,6,9   │
│            └───┘                 └─★─┘    glow    │
│                                                    │
│    Sequence: [3] → [6] → [9] → [?] → [?]         │
│    Accuracy: ████████░░ 82%                        │
│                                                    │
│    Cymatic Pattern: ◎ (forming — 82% complete)     │
└────────────────────────────────────────────────────┘
```

### 10.4 Session End / Recap Screen

```
┌────────────────────────────────────────────────────┐
│                                                    │
│              ☾ SESSION COMPLETE ☾                  │
│              Moon 5, Day 14                        │
│                                                    │
│    ┌────────────────────────────────────┐          │
│    │  RS Progress:  67% → 73%  (+6%)   │          │
│    │  Aether Earned: 1,240              │          │
│    │  Buildings Restored: 2             │          │
│    │  Enemies Purified: 14              │          │
│    │  Lore Discovered: 3 entries        │          │
│    └────────────────────────────────────┘          │
│                                                    │
│    Milo says: "Another day, another               │
│    dazzlingly improbable achievement."             │
│                                                    │
│    Next: Aether harvest in 6h 23m                  │
│    [Continue Playing]  [Exit to Map]               │
└────────────────────────────────────────────────────┘
```

---

**Document Status:** DRAFT  
**Cross-References:** `00_MASTER_GDD.md`, `06_COMBAT_PROGRESSION.md`, `09_TECHNICAL_SPEC.md`  
**Last Updated:** March 23, 2026
