# TARTARIA WORLD OF WONDER — PC Input & UX Design
## Keyboard, Mouse, Gamepad Controls, Session Design, Accessibility & Onboarding

---

## Table of Contents

1. [UX Philosophy](#1-ux-philosophy)
2. [Session Design](#2-session-design)
3. [Input Control Scheme](#3-input-control-scheme)
4. [HUD & Interface Layout](#4-hud-interface-layout)
5. [Onboarding Flow](#5-onboarding-flow)
6. [Daily & Weekly Engagement Loops](#6-daily-weekly-engagement-loops)
7. [Notification Strategy](#7-notification-strategy)
8. [Accessibility](#8-accessibility)
9. [Performance UX](#9-performance-ux)
10. [Wireframe Concepts](#10-wireframe-concepts)

---

## 1. UX Philosophy

**Immersive Desktop Experience:** Every core action is fluid with keyboard + mouse, with full gamepad support as a first-class alternative. The PC version leverages the power of a desktop to deliver richer visuals, more complex UI, and deeper session experiences.

**Core Principles:**
1. **Flexible session length.** 15-minute bursts, 2-hour deep dives, and everything in between. Every session length is valid and rewarding.
2. **No dead time.** Loading screens show lore fragments. Transitions feature ambient Aether animations. Every second has value.
3. **Wonder over clutter.** The screen should feel like a window into the Tartarian world, not a spreadsheet. Minimize HUD. Maximize beauty.
4. **Alt-Tab safe.** Save state every 10 seconds. Any interruption (alt-tab, system alert, minimization) loses zero progress.
5. **Progressive disclosure.** Show only what the player needs NOW. Advanced systems reveal as they become relevant.
6. **Haptic quality.** Every interaction has a tailored gamepad rumble response (XInput / DualSense). The controller should feel resonant.

---

## 2. Session Design

### 2.1 Session Types

| Type | Duration | Activities | Reward Density |
|---|---|---|---|
| **Quick** | 5–15 min | Daily harvest, quick check-in, send kids to build | 1–5 rewards |
| **Standard** | 15–30 min | Full zone progress: excavate, build, tune, fight, explore | 8–12 rewards |
| **Deep Dive** | 30–60 min | Multi-zone, story progression, boss fights, train/airship journeys | 15–25 rewards |
| **Epic** | 60–120 min | Moon climax events, major discoveries, planetary events | 30+ rewards |
| **Marathon** | 2+ hours | Full campaign arcs, completionist sweeps, sandbox building | Unlimited |

### 2.2 Session Pacing

```
Minute 0–1:    ARRIVAL — Zone loads, ambient hum, Milo says something
Minute 1–3:    DISCOVERY — Scan reveals buried feature, excavation begins
Minute 3–5:    ACTION — First combat or tuning challenge
Minute 5–8:    RESTORATION — Building / precision cutting mini-game
Minute 8–10:   PAYOFF — Structure activates, ley line lights up, NPC reacts
Minute 10–12:  CHOICE — Continue deeper or bank progress
Minute 12–15:  CLIMAX — Boss encounter or major reveal (if continuing)
Minute 15:     NATURAL PAUSE — Session recap screen + "continue or save & quit"
```

### 2.3 Interruption Recovery

| Interruption | Recovery |
|---|---|
| Alt-Tab / minimize | Auto-pause (optional: continue in background), resume instantly |
| Window resize | Instant UI reflow, no reload |
| System alert / overlay | Auto-pause, resume on dismiss |
| Application crash | Restore from last auto-save (max 10 seconds lost) |
| PC sleep / hibernate | Full state preserved, resume on wake |
| GPU driver crash | Auto-save on detection, graceful restart prompt |

---

## 3. Input Control Scheme

### 3.1 Keyboard & Mouse Controls

| Context | Mouse | Keyboard | Notes |
|---|---|---|---|
| **Move** | Click to move (optional) | WASD | Standard PC movement |
| **Camera** | Right-click drag / mouse look | Q/E rotate, scroll wheel zoom | Free camera with auto-follow toggle |
| **Interact** | Left-click object | E / F | Context-sensitive interaction |
| **Combat** | Left-click: attack, Right-click: ability | Number keys 1–7: frequency select | Draw patterns with mouse for abilities |
| **Build** | Left-click: place, Right-click: cancel | R: rotate, scroll: scale | Grid snap with Shift modifier |
| **Excavate** | Click-drag over mud to clear | Hold E on target area | Multi-select with Shift+drag |
| **Giant Mode** | Double-click giant icon | G key toggle | Same abilities, keyboard shortcuts |
| **Tune** | Click frequency nodes | Number keys or mouse drag | Free-play mode with keyboard note input |

### 3.2 Gamepad Controls (Xbox / PlayStation / Steam Deck)

| Context | Left Stick | Right Stick | Face Buttons | Triggers/Bumpers |
|---|---|---|---|---|
| **Move** | Movement | Camera | A: Interact, B: Cancel | RT: Attack, LT: Ability |
| **Combat** | Movement | Camera | A: Dodge, B: Guard, X: Light, Y: Heavy | RT: Frequency blast, LT: Giant Mode, RB/LB: Cycle frequency |
| **Build** | Position | Rotate | A: Place, B: Cancel, X: Snap toggle, Y: Template | RT/LT: Scale up/down |
| **Excavate** | Cursor movement | Camera | A: Dig, X: Scan | RT: Fast clear, LB: Tool switch |
| **Menus** | Navigate | — | A: Select, B: Back | RB/LB: Tab switch |

### 3.3 Keybinding Reference

| Action | Default Key | Category |
|---|---|---|
| Move Forward/Back/Left/Right | W / S / A / D | Movement |
| Sprint | Left Shift | Movement |
| Jump | Space | Movement |
| Interact / Confirm | E | Universal |
| Cancel / Back | Escape | Universal |
| Inventory | I | UI |
| Map | M | UI |
| Quest Log | J | UI |
| Toggle Giant Mode | G | Gameplay |
| Aether Scan | Tab | Gameplay |
| Quick Save | F5 | System |
| Quick Load | F9 | System |
| Toggle HUD | H | UI |
| Frequency 1–7 (174–852 Hz) | 1–7 | Combat |
| Dodge | Space (in combat) | Combat |
| Guard / Block | Ctrl | Combat |
| Building Mode | B | Building |
| Rotate Building | R / Mouse Wheel | Building |
| Companion Command | C | Companion |
| Photo Mode | F12 | Social |
| Pause Menu | Escape | System |

### 3.4 Control Customization

- **Full keybinding remapping** for all keyboard/mouse actions
- **Gamepad button remapping** with preset layouts (Default, Southpaw, Classic RPG)
- **Mouse sensitivity sliders:** Camera rotation, zoom speed, cursor acceleration
- **Toggle vs. Hold:** Configurable per action (e.g., sprint toggle or hold, crouch toggle or hold)
- **Mouse invert:** X-axis, Y-axis independently
- **Gamepad dead zones:** Adjustable per stick
- **Simultaneous KB/M + Gamepad:** Auto-detect active input, swap prompts dynamically

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
- **Companion Chat** (bottom-center-left): Latest companion line + click to see history
- **Tuning Wheel** (bottom-right): Frequency selector (appears only in combat)
- **Settings Gear** (bottom-far-right): Menu access
- **Mini-map** (top-right, below Moon counter): Toggleable with M key

**Design Rule:** Total HUD covers <10% of screen area. The world is the star. HUD scales with resolution (1080p → 4K).

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

Additional combat elements:
- **Combo Counter** (top-center): Grows with chain hits, pulses at 3/6/9
- **Giant Meter** (bottom-center): Fills from kills and building
- **Frequency Display** (bottom-right): Current active frequency (1–7 hotkeys shown)
- **Ability Cooldowns** (bottom bar): Keyboard shortcut labels visible

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
│ [R:Rotate]  [Scroll:Scale]  [Snap: ON]  [LMB:Place]│
│ [Esc:Cancel]              [Template: Seed of Life]  │
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

Zones pulse with RS-proportional glow. Ley lines drawn as golden threads between connected zones. Trains and airships visible as tiny moving icons. Mouse hover reveals zone details.

---

## 5. Onboarding Flow

### 5.1 First 30 Minutes (Moon 1, Days 1–2)

| Minute | Player Does | System Teaches | Emotional Target |
|---|---|---|---|
| 0–3 | Watch prologue cinematic | Nothing — pure wonder | AWE |
| 3–5 | Click/WASD to excavate first wall | Movement: WASD + mouse to clear | CURIOSITY |
| 5–7 | Click revealed structure | Interaction: E key / left-click to examine | DISCOVERY |
| 7–9 | Follow Milo to dome interior | Movement: WASD, camera: mouse | GUIDED |
| 9–11 | Simple 3-note organ tune (keyboard keys) | Mini-game: tuning with keyboard | SATISFACTION |
| 11–13 | Watch dome light up | RS system (passive introduction) | DELIGHT |
| 13–15 | Place first spire | Building: B mode, click to place, R to rotate | OWNERSHIP |
| 15–18 | Combat tutorial (Reset scouts) | Click-to-attack, Space-to-dodge | EMPOWERMENT |
| 18–20 | First giant-mode burst (G key) | Giant mode: spectacular power | THRILL |
| 20–25 | Explore restored cathedral | Free exploration + Lirael appears | WONDER |
| 25–30 | First ley line lights up | Grid system (visual, no UI) | MOTIVATION |

### 5.2 First Week (Moon 1 complete)

By end of Moon 1, player should:
- ✅ Know all basic controls (KB/M and gamepad)
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
- Control prompts auto-switch between KB/M and gamepad based on active input

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

Players should WANT to check in because in-game events and Steam notifications are written in-character and offer genuine value.

### 7.2 Notification Types

| Type | Frequency | Character | Example |
|---|---|---|---|
| **Harvest Ready** | Every 8h (max) | Milo | "Your Aether tanks are full, spark. I may have skimmed a bit." |
| **Construction Complete** | On event | Lirael | "The children finished the dome. It's smiling." |
| **Story Available** | New Moon | Thorne | "New signal on the horizon. Time to fly." |
| **Community Event** | Weekly | Aether Whisper | "The 13 Moons align. The grid awaits." |
| **Comeback** | After 3 days absent | Korath's echo | "The stones remember you. Will you return?" |

### 7.3 Notification Rules

- **Delivered via Steam Rich Presence + optional desktop toast** (using OS notification API)
- **Maximum 2 per day** (hard cap regardless of events)
- **Always dismissable** without losing content
- **Never FOMO-driven** (no "YOU'RE MISSING OUT" language)
- **Respect Windows Focus Assist / Do Not Disturb** (no notifications during focus mode)
- **Earnable opt-out** per category in Settings

---

## 8. Accessibility

### 8.1 Visual Accessibility

| Feature | Description |
|---|---|
| **Color-blind modes** | Protanopia, Deuteranopia, Tritanopia presets — all frequency/combat tells use shape + pattern in addition to color |
| **High contrast mode** | Increases edge contrast on interactive elements |
| **Text scaling** | 75%–200% of default (separate from resolution scaling) |
| **Subtitle background** | Optional opaque background for all dialogue |
| **Reduced motion** | Disables parallax, particle density, and screen shake |
| **Screen reader support** | Windows Narrator / NVDA / JAWS descriptions for all buttons and states (future: VoiceOver for iOS port) |
| **UI scaling** | Independent UI scale slider (50%–200%) for high-DPI / ultrawide monitors |

### 8.2 Motor Accessibility

| Feature | Description |
|---|---|
| **Full remapping** | Every keyboard, mouse, and gamepad binding is remappable |
| **One-hand presets** | Keyboard-only and mouse-only control presets |
| **Auto-dodge** | Optional automatic evasion at low health |
| **Hold vs. toggle** | All hold-actions have toggle equivalents |
| **Adjustable timing** | Quick-time events have extended timing mode (2× window) |
| **Click target size** | All UI elements meet minimum 44×44px click targets at 1080p |
| **Sticky keys support** | Modifier key (Shift, Ctrl) combinations work with OS sticky keys |
| **Mouse-only mode** | Point-and-click for all gameplay actions |

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
| **Haptic audio mapping** | Distinct gamepad rumble patterns for each frequency band |
| **Closed captions** | All environmental audio described |
| **Mono audio** | Full mix in both/either channel |
| **Sign language NPC** | Echo NPC in hub zone uses ASL/BSL gestures |

---

## 9. Performance UX

### 9.1 Loading Strategy

| Scenario | Target Time | User Experience |
|---|---|---|
| Initial launch | <5 seconds | Aether swirl animation + lore quote |
| Zone transition | <2 seconds | Train/airship travel animation (masked load) |
| Building placement | Instant | Assets pre-loaded during zone exploration |
| Combat start | <0.5 seconds | Seamless — no loading screen |
| Map screen | <1 second | Low-detail map → detail streams in |

### 9.2 GPU-Aware Quality Scaling

**Auto-detects GPU capability on first launch; player can override in Settings:**

| Preset | Target | Resolution | Effects |
|---|---|---|---|
| **Low** | GTX 1070 / RX 580 | 1080p | Reduced particles, baked shadows, no volumetrics |
| **Medium** | RTX 2060 / RX 5700 | 1440p | Standard particles, dynamic shadows, basic volumetrics |
| **High** | RTX 3070 / RX 6800 | 1440p–4K | Full particles, ray-traced reflections, volumetric fog |
| **Ultra** | RTX 4070+ / RX 7900+ | 4K native | Full RT, maximum particles, enhanced draw distance |

**Adaptive quality:** Optional auto-adjust to maintain 60 FPS target. Reduces shadow distance → particle count → resolution scale in that order.

### 9.3 Storage Management

| Component | Size | Details |
|---|---|---|
| **Base install** | ~4 GB | All Moon 1–3 content + core assets |
| **Per-Moon content** | ~300 MB each | Optional pre-download or stream on demand |
| **Full game** | ~8 GB | All 13 Moons + assets |
| **Save data** | <10 MB | Local + Steam Cloud sync |
| **Shader cache** | ~500 MB | Pre-compiled on first launch per GPU |

**Smart Downloads:** Game can pre-download upcoming Moon content in the background. Players never see a download screen during gameplay.

---

## 10. Wireframe Concepts

### 10.1 Main Gameplay Screen

```
┌────────────────────────────────────────────────────┐
│  RS: ████░░ 73%              ☾ Moon 5 / Day 14    │
│                                               [M]  │
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
│  ⚡████░░     💬"Not bad,    🎵[1] [2] [3]  ⚙   │
│  Aether       spark!"        [4] [5] [6] [7]     │
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
│  [R: Rotate]  [Scroll: Scale]  [📐 Snap: ON]     │
│  [Esc: Cancel]              [LMB: PLACE] [Tab: Template]│
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
│    ┌────────────────────────────────────────┐      │
│    │  RS Progress:  67% → 73%  (+6%)       │      │
│    │  Aether Earned: 1,240                 │      │
│    │  Buildings Restored: 2                │      │
│    │  Enemies Purified: 14                 │      │
│    │  Lore Discovered: 3 entries           │      │
│    └────────────────────────────────────────┘      │
│                                                    │
│    Milo says: "Another day, another               │
│    dazzlingly improbable achievement."             │
│                                                    │
│    Next: Aether harvest in 6h 23m                  │
│    [Continue Playing]  [Save & Quit]               │
└────────────────────────────────────────────────────┘
```

---

**Document Status:** FINAL — PC-First Platform Migration  
**Cross-References:** `00_MASTER_GDD.md`, `06_COMBAT_PROGRESSION.md`, `09_TECHNICAL_SPEC.md`  
**Last Updated:** March 26, 2026
