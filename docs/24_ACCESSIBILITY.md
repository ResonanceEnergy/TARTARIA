# TARTARIA WORLD OF WONDER — Comprehensive Accessibility Audit
## WCAG 2.1 AA Compliance, Motor/Visual/Auditory/Cognitive Access Design

---

> *"The Golden Age was for everyone. So is this game. Every hand, every eye, every ear, every mind should find their way into wonder."*

**Cross-References:**
- [07_PC_UX.md](07_PC_UX.md) — PC controls, session design, accessibility section
- [appendices/D_CONTROLS.md](appendices/D_CONTROLS.md) — Control remapping, gamepad support, accessibility input
- [06_COMBAT_PROGRESSION.md](06_COMBAT_PROGRESSION.md) — Combat difficulty, frequency color system
- [13_MINI_GAMES.md](13_MINI_GAMES.md) — Mini-game accessibility modes
- [14_HAPTIC_FEEDBACK.md](14_HAPTIC_FEEDBACK.md) — Haptic accessibility & battery
- [appendices/E_METRICS.md](appendices/E_METRICS.md) — Accessibility adoption metrics

---

## Table of Contents

1. [Accessibility Philosophy](#1-accessibility-philosophy)
2. [WCAG 2.1 AA Compliance Matrix](#2-wcag-21-aa-compliance-matrix)
3. [Visual Accessibility](#3-visual-accessibility)
4. [Motor Accessibility](#4-motor-accessibility)
5. [Auditory Accessibility](#5-auditory-accessibility)
6. [Cognitive Accessibility](#6-cognitive-accessibility)
7. [Vestibular & Photosensitivity](#7-vestibular-photosensitivity)
8. [PC Platform Integration](#8-pc-platform-integration)
9. [Testing Checklist](#9-testing-checklist)
10. [Budget Impact Assessment](#10-budget-impact-assessment)
11. [Prioritization Matrix](#11-prioritization-matrix)

---

## 1. Accessibility Philosophy

### Why Accessibility Is Non-Negotiable

1. **Steam rewards it.** Steam featuring guidelines strongly favor accessible games. Games with robust accessibility features earn higher visibility, positive community response, and press coverage.
2. **15% of the world population has a disability.** That's hundreds of millions of potential PC gamers.
3. **Accessibility IS good UX.** Subtitles improve comprehension for all users. Larger touch targets help everyone on a train. Customizable controls benefit power users.
4. **Tartaria's theme demands it.** A game about rebuilding a utopia should be playable by everyone.

### Accessibility Tiers

| Tier | Scope | When |
|---|---|---|
| **A (Launch Required)** | Basic WCAG AA, screen reader nav, DPI-aware text scaling, colorblind modes | Phase 1–2 |
| **B (Soft Launch)** | Switch Control, motor remapping, audio descriptions | Phase 3 |
| **C (Post-Launch)** | Full screen reader gameplay, AAC integration, community feedback | Phase 4+ |

---

## 2. WCAG 2.1 AA Compliance Matrix

### Perceivable

| Criterion | WCAG ID | Status | Implementation |
|---|---|---|---|
| Text alternatives for images | 1.1.1 | ✅ Planned | Alt text on all UI icons, building icons, companion portraits |
| Captions for audio content | 1.2.2 | ✅ Planned | Subtitles for all dialogue; visual cue system for SFX |
| Audio descriptions | 1.2.5 | 🔶 Tier B | Narrated scene descriptions for key story moments |
| Color not sole information carrier | 1.4.1 | ✅ Planned | Frequency matching uses color + shape + sound (see §3) |
| Contrast ratio ≥ 4.5:1 (text) | 1.4.3 | ✅ Planned | All UI text on all background states |
| Text resize up to 200% | 1.4.4 | ✅ Planned | DPI-aware text scaling integration |
| Reflow (no horizontal scrolling) | 1.4.10 | ✅ Planned | Responsive UI layout engine |

### Operable

| Criterion | WCAG ID | Status | Implementation |
|---|---|---|---|
| Keyboard accessible | 2.1.1 | ✅ Planned | External keyboard + Switch Control mapping |
| No keyboard traps | 2.1.2 | ✅ Planned | Escape key / controller back button works globally |
| Timing adjustable | 2.2.1 | ✅ Planned | All timed sequences pauseable; mini-game timing adjustable |
| Pause/stop/hide moving content | 2.2.2 | ✅ Planned | All animations respect system Reduce Motion settings |
| Three flashes or below threshold | 2.3.1 | ✅ Planned | No content flashes >3/second (Aether effects capped) |
| Focus order logical | 2.4.3 | ✅ Planned | Screen reader navigation follows visual hierarchy |
| Link/button purpose clear | 2.4.4 | ✅ Planned | Descriptive labels on all interactive elements |

### Understandable

| Criterion | WCAG ID | Status | Implementation |
|---|---|---|---|
| Language of page | 3.1.1 | ✅ Planned | NSLocale-aware language switching |
| On focus, no context change | 3.2.1 | ✅ Planned | No auto-navigation on focus events |
| Error identification | 3.3.1 | ✅ Planned | Clear feedback on failed actions (not just color) |
| Labels on input | 3.3.2 | ✅ Planned | All settings controls labeled for screen readers |

### Robust

| Criterion | WCAG ID | Status | Implementation |
|---|---|---|---|
| Parsing (valid markup) | 4.1.1 | ✅ Planned | Unity UI Toolkit accessibility tree validation |
| Name/role/value for UI | 4.1.2 | ✅ Planned | Accessibility traits on all interactive components |

---

## 3. Visual Accessibility

### 3.1 Colorblind Modes

The frequency-matching combat system relies on color (see [06_COMBAT_PROGRESSION.md](06_COMBAT_PROGRESSION.md)):

| Frequency | Default Color | Protanopia | Deuteranopia | Tritanopia |
|---|---|---|---|---|
| 174 Hz | Red | Orange | Orange | Red |
| 285 Hz | Orange | Gold | Gold | Orange |
| 396 Hz | Yellow | Yellow | Yellow | Cyan |
| 528 Hz | Green | Blue | Blue | Green |
| 639 Hz | Blue | Purple | Cyan | Magenta |
| 741 Hz | Indigo | Pink | Magenta | Orange |
| Dissonant | Black | Dark gray | Dark gray | Dark gray |

**Shape Overlay System:** In addition to color, each frequency gets a unique geometric shape:

| Frequency | Shape | Always Visible (Colorblind OFF/ON) |
|---|---|---|
| 174 Hz | Triangle | Pulsing on enemy core |
| 285 Hz | Diamond | Shimmering on limbs |
| 396 Hz | Square | Sparking on impact |
| 528 Hz | Hexagon | Smoking on dodge |
| 639 Hz | Circle | Frosting on surfaces |
| 741 Hz | Star | Crystallizing |
| Dissonant | X mark | Absence void |

**Setting:** `Settings > Accessibility > Colorblind Mode: Off / Protanopia / Deuteranopia / Tritanopia / Custom`

### 3.2 High Contrast Mode

| Element | Default | High Contrast |
|---|---|---|
| UI backgrounds | Semi-transparent | Solid dark (#000000CC) |
| Button outlines | None | 2px white border |
| Quest markers | Color-only orbs | Color + white outline + label text |
| Companion indicators | Subtle glow | Bold icon + name label |
| Interactive objects | Aether glow | Aether glow + pulsing white outline |

### 3.3 Text Accessibility

| Feature | Implementation |
|---|---|
| **DPI-Aware Scaling** | All UI text responds to system DPI/text scaling settings |
| **Minimum font size** | 11pt body, 13pt dialogue — never below |
| **Font choice** | High x-height sans-serif; CJK uses system fonts (optimal rendering) |
| **Text outline** | 1px dark outline on all in-world floating text |
| **Subtitle backgrounds** | Always-on dark background behind subtitle text |
| **Subtitle size scaling** | Small / Medium / Large / Extra Large (independent of system setting) |

### 3.4 Screen Reader Support

| Context | Screen Reader Behavior |
|---|---|
| **Menus** | Full navigation with traits (button, heading, adjustable) |
| **Dialogue** | Auto-read NPC lines; swipe to repeat; haptic on new line |
| **HUD** | RS, Aether, companion status as accessibility values |
| **Combat** | Audio cues announce enemy frequency + direction |
| **Building** | Announce snap point availability, golden ratio compliance % |
| **Mini-games** | Audio-only mode with narrated cues (see [13_MINI_GAMES.md](13_MINI_GAMES.md)) |

### 3.5 Magnification & Zoom

- Respect system accessibility zoom features
- Custom camera zoom limits extended for low-vision users
- Scroll-to-zoom on dialogue portraits and codex entries

---

## 4. Motor Accessibility

### 4.1 One-Thumb Guarantee

Per [07_PC_UX.md](07_PC_UX.md), every core action is accessible via keyboard, mouse, or gamepad. This is the baseline motor accessibility — not an add-on.

### 4.2 Touch Target Sizes

| Element | Default Size | Accessible Size | Design Minimum |
|---|---|---|---|
| Primary button | 44×44 pt | 54×54 pt | 44×44 pt ✅ |
| Secondary button | 36×36 pt | 48×48 pt | 44×44 pt ⚠️ (increase to 44) |
| Combat target | 60×60 pt | 72×72 pt | 44×44 pt ✅ |
| Building placement | Full object | Full object + 20% padding | N/A ✅ |
| Mini-game input zones | 80×80 pt | 100×100 pt | 44×44 pt ✅ |

**Setting:** `Settings > Accessibility > Button Size: Standard / Large / Extra Large`

### 4.3 Hold Duration & Gesture Forgiveness

| Feature | Default | Accessible Mode |
|---|---|---|
| Long-press threshold | 500ms | 250ms / 750ms / 1000ms (configurable) |
| Swipe gesture sensitivity | Medium | Low / Medium / High |
| Dodge window (combat) | 300ms | 300ms / 450ms / 600ms |
| Combo input window | 500ms | 500ms / 750ms / 1000ms |
| Multi-touch requirement | Some actions | All actions available via single touch |
| Draw gestures (cymatic) | Required for ability | Alternative: tap pattern sequence |

### 4.4 Switch Control & AssistiveTouch

Full mapping for Windows accessibility / Switch Control:

| Game Action | Switch Action |
|---|---|
| Move | Auto-move to selected destination (switch to select) |
| Interact | Select + activate (2-switch workflow) |
| Combat attack | Auto-target nearest enemy; switch activates attack |
| Build placement | Cycle through valid positions; switch confirms |
| Mini-game input | Simplified timing mode with switch-compatible inputs |
| Camera | Auto-follow (manual camera via scanning rotation) |

### 4.5 External Controller Support

Per [appendices/D_CONTROLS.md](appendices/D_CONTROLS.md):

| Controller | Support Level |
|---|---|
| **DualSense (PS5)** | Full mapping + adaptive triggers for haptics |
| **Xbox Wireless** | Full mapping |
| **Xbox Adaptive Controller** | Full mapping — all actions via switch-compatible inputs |
| **Adaptive controllers** | Switch-compatible via Windows accessibility APIs |

### 4.6 Auto-Play Options

For players who cannot perform real-time actions:

| Feature | Effect | Gate |
|---|---|---|
| Auto-combat | Character auto-targets, auto-attacks | Toggle in settings |
| Auto-dodge | Character auto-dodges when attack incoming | Toggle in settings |
| Auto-tune | Mini-games play at Bronze (★) quality automatically | Per-mini-game toggle |
| Auto-build | System places buildings at valid locations | Suggestion mode (player confirms) |

**Design Rule:** Auto-play never exceeds Silver (★★) quality. Gold (★★★) requires manual input — mastery must be earned.

---

## 5. Auditory Accessibility

### 5.1 Visual Equivalents for All Audio

| Audio Event | Visual Equivalent |
|---|---|
| Enemy approaching (footstep SFX) | Directional arrow at screen edge |
| Frequency matching (pitch) | Tuning meter visualization |
| Resonance chain combo (musical scale) | Combo counter with color burst |
| Companion speaks (voice/text) | Portrait flash + text bubble |
| Environmental danger (collision SFX) | Screen edge red pulse |
| Achievement unlock (chime) | Banner slide-in animation |
| Bell tower peal (deep tone) | Screen shake + radial pulse |

### 5.2 Subtitle System

| Feature | Specification |
|---|---|
| **Always available** | Subtitles on by default; cannot be accidentally hidden |
| **Speaker identification** | Color-coded name label: Milo (blue), Lirael (green), Korath (amber) |
| **Sound effect captions** | [mud squelching], [dome humming], [bell ringing] — toggleable |
| **Directional indicator** | ← → ↑ ↓ arrows for off-screen audio sources |
| **Background contrast** | Dark semi-transparent box behind all subtitle text |
| **Font size** | 4 levels independent of system font size |
| **Haptic sync** | Subtitle appearance triggers light haptic pulse |

### 5.3 Audio Description Mode

Narrated descriptions of key visual events for blind/low-vision players:

| Trigger | Audio Description |
|---|---|
| Zone entry | "You enter Echohaven — a sunlit valley with a half-buried golden dome ahead." |
| Dome restoration | "The mud dissolves. Golden stone rises. The dome is complete." |
| Combat start | "A Mud Golem emerges from the earth, 10 meters ahead." |
| Moon climax | "The sky splits. Aurora light pours through the fracture." |

---

## 6. Cognitive Accessibility

### 6.1 Complexity Management

| Feature | Implementation |
|---|---|
| **Quest objective clarity** | Always show next step in HUD (never ambiguous) |
| **No timed puzzles without pause** | All mini-games pauseable mid-action |
| **Simplified mode** | Toggle removes secondary objectives, shows only main path |
| **Tutorial replay** | Any tutorial accessible from settings at any time |
| **Reading pace control** | Dialogue advances manually (tap to continue), never auto-scrolls |
| **Consistent controls** | Same gesture = same action everywhere (tap = interact, always) |

### 6.2 Navigation Assistance

| Feature | Implementation |
|---|---|
| **Waypoint system** | Optional quest marker (off by default per design; toggleable) |
| **Breadcrumb trail** | Subtle golden particle trail to next objective (optional) |
| **Map markers** | Visited locations marked; undiscovered areas dimly outlined |
| **Companion hints** | Milo/Lirael offer directional hints if idle >60 seconds |
| **Mini-map** | Optional corner mini-map with zone progress % |

### 6.3 Difficulty Modes

| Mode | Combat | Mini-Games | Puzzles | Story |
|---|---|---|---|---|
| **Story** | Enemies 50% weaker, auto-dodge available | Bronze auto-pass option | Hints after 30s idle | Full |
| **Balanced** (Default) | Standard difficulty | Standard timing | Hints after 120s idle | Full |
| **Challenge** | Enemies 150% stronger, no auto-dodge | Tight timing windows | No hints | Full |
| **Custom** | Adjustable per system | Adjustable per game | Adjustable | Full |

**Key Rule:** Story is always the same regardless of difficulty. No content is locked behind difficulty selection.

---

## 7. Vestibular & Photosensitivity

### 7.1 Motion Sensitivity

| Feature | Default | Reduced Motion |
|---|---|---|
| Camera shake (combat/impacts) | On | Off (instant cut instead) |
| Screen transitions | Animated dissolve | Instant cut |
| Parallax scrolling | On | Off |
| Particle density | Full | 50% reduction |
| Camera bob during walk | Subtle | None |
| Aether field waves | Animated | Static glow |

**Setting:** Respects OS `Reduce Motion` preference automatically + in-game override.

### 7.2 Photosensitivity

| Event | Default | Safe Mode |
|---|---|---|
| Dome activation flash | Bright golden burst (200ms) | Gentle golden fade (800ms) |
| Combat frequency sparks | Quick flashes per hit | Sustained glow per hit |
| Moon climax aurora | Dynamic rippling | Slow gradient sweep |
| Giant Mode transformation | rapid energy burst | Smooth scale-up transition |
| Aether overload | Flashing warning | Pulsing glow + haptic only |

**Flash Safety Rule:** No visual element flashes more than 3 times per second. All Aether VFX are validated against the Harding FPA test (photosensitive epilepsy standard).

---

## 8. PC Platform Integration

### 8.1 Accessibility APIs & Standards

| API / Standard | Usage |
|---|---|
| **Windows UI Automation** | Screen reader traits, labels, values for all UI elements |
| **Narrator / NVDA / JAWS** | Game-specific screen reader actions (attack, build, interact) |
| **Windows Text Scaling** | `SystemParametersInfo` for scalable text in UI Toolkit |
| **Reduce Motion** | Windows `SPI_GETCLIENTAREAANIMATION` — triggers reduced VFX |
| **High Contrast** | Windows High Contrast theme detection; solid backgrounds on overlays |
| **Color Filters** | Respect Windows colorblind filter settings + in-game override |
| **Steam Big Picture** | Full controller navigation through all game systems |
| **Voice Control** | Voice command mapping for core actions (Windows Speech Recognition) |
| **Xbox Adaptive Controller** | Full scan-and-select path via switch-compatible inputs |

### 8.2 Accessibility Settings Location

```
Settings (⚙️)
├── Controls
│   ├── Button Size: Standard / Large / Extra Large
│   ├── Touch Sensitivity: Low / Medium / High
│   ├── Hold Duration: 250ms / 500ms / 750ms / 1000ms
│   └── One-Hand Mode: Off / Left / Right
├── Visual
│   ├── Colorblind Mode: Off / Protanopia / Deuteranopia / Tritanopia / Custom
│   ├── High Contrast: Off / On
│   ├── Subtitle Size: Small / Medium / Large / Extra Large
│   ├── Sound Effect Captions: Off / On
│   └── Reduced Motion: System Default / Off / On
├── Audio
│   ├── Audio Descriptions: Off / On
│   ├── Directional Audio Cues: Off / On
│   └── Visual Audio Indicators: Off / On
├── Gameplay
│   ├── Difficulty: Story / Balanced / Challenge / Custom
│   ├── Navigation Assist: Off / Hints / Waypoints / Trail
│   ├── Auto-Combat: Off / On
│   ├── Auto-Dodge: Off / On
│   └── Mini-Game Timing: Standard / Relaxed / Very Relaxed
└── Haptic
    ├── Haptic Feedback: Off / Light / Medium / Full
    └── Haptic Events: All / Interactions Only / None
```

---

## 9. Testing Checklist

### 9.1 Device Testing Matrix

| Device | Test Focus | Priority |
|---|---|---|
| GTX 1060 / RX 580 (Min spec) | Baseline screen reader + accessibility features | **High** |
| RTX 3060 / RX 6700 XT (Recommended) | Target hardware — all features | **Critical** |
| RTX 4080+ (Ultra) | High-res layout validation | **High** |
| Steam Deck | Handheld / controller-only navigation | **Medium** |
| Laptop (integrated GPU) | Low-spec graceful degradation | **Low** |

### 9.2 Test Scenarios

| Scenario | Tests | Pass Criteria |
|---|---|---|
| **Screen reader full playthrough** | Navigate menus, enter zone, restore dome, combat, quest | All elements labeled, logical focus order |
| **Switch Control 2-switch** | Complete first 15-minute demo | All mechanics accessible via scanning |
| **Colorblind mode combat** | Fight all 7 frequency enemy types | All frequencies distinguishable via shape + color |
| **Audio-off gameplay** | Play 30 minutes with device muted | All information conveyed visually |
| **Visual-off gameplay** | Play with monitor off, screen reader + gamepad haptics | Navigate menus, basic zone exploration |
| **Reduced Motion** | Full session in reduce motion mode | No jarring cuts, no missing information |
| **Dynamic Type xxxLarge** | All menus, dialogue, HUD | No text clipping, overlaps, or unreadable strings |
| **Story Mode difficulty** | Complete Moon 1 in story mode | Zero fail states, auto-play functional |
| **External controller** | Gamepad full playthrough (DualSense / Xbox) | All actions mapped, no unmapped required inputs |

### 9.3 Automated Accessibility Tests

| Test | Tool | Trigger |
|---|---|---|
| Screen reader label coverage | Accessibility Insights for Windows | Every build |
| Contrast ratio validation | Custom shader analysis tool | Asset import |
| Touch target size audit | UI bounds checker script | Every build |
| Flash frequency analysis | Harding FPA test script | New VFX asset |
| Dynamic Type overflow | TextMeshPro bounds check | Localization import |

---

## 10. Budget Impact Assessment

### 10.1 Cost by Tier

| Tier | Features | Engineering Hours | Cost Estimate |
|---|---|---|---|
| **A (Launch)** | Screen reader nav, text scaling, colorblind, subtitles, difficulty | 200 hrs | $20,000–$30,000 |
| **B (Soft Launch)** | Switch Control, motor remapping, audio descriptions, shape overlays | 120 hrs | $12,000–$18,000 |
| **C (Post-Launch)** | Full screen reader gameplay, voice control, AAC, community tools | 160 hrs | $16,000–$24,000 |
| **Total** | All accessibility features | **480 hrs** | **$48,000–$72,000** |

### 10.2 ROI Justification

| Benefit | Estimated Impact |
|---|---|
| Steam featuring probability | +30% (Steam favors accessible games in discovery) |
| Addressable market expansion | +15% potential users (~75,000 at 500k downloads) |
| Regulatory compliance (EU, US Section 508) | Avoids future forced updates |
| Player satisfaction (all users) | Subtitle users: ~80% of all players; difficulty options: ~40% |
| Press coverage | Accessibility features generate positive gaming press |

---

## 11. Prioritization Matrix

### Must-Have (Launch Block)

| Feature | Effort | Impact | Priority |
|---|---|---|---|
| Screen reader menu navigation | Medium | High | **P0** |
| Dynamic Type support | Medium | High | **P0** |
| Colorblind mode (3 types) | Low | High | **P0** |
| Subtitle system with backgrounds | Low | Very High | **P0** |
| Touch target ≥44pt compliance | Low | High | **P0** |
| Difficulty modes (Story/Balanced/Challenge) | Medium | Very High | **P0** |
| Reduce Motion respect | Low | Medium | **P0** |
| Save-anywhere (interrupt-safe) | Already implemented | Critical | **P0** |

### Should-Have (Soft Launch)

| Feature | Effort | Impact | Priority |
|---|---|---|---|
| Shape overlays on frequency combat | Medium | High | **P1** |
| Switch Control scan path | High | Medium | **P1** |
| Audio descriptions (key scenes) | Medium | Medium | **P1** |
| Motor remapping settings | Medium | High | **P1** |
| High contrast mode | Low | Medium | **P1** |
| Auto-combat/auto-dodge toggles | Low | Medium | **P1** |

### Nice-to-Have (Post-Launch)

| Feature | Effort | Impact | Priority |
|---|---|---|---|
| Full screen reader gameplay narration | Very High | Medium | **P2** |
| Voice Control actions | High | Low | **P2** |
| Navigation breadcrumb trail | Medium | Medium | **P2** |
| Community accessibility feedback tool | Low | Low | **P2** |

---

*Wonder has no prerequisites. Everyone who touches this game should feel the resonance.*

---

**Document Status:** FINAL
**Author:** Nathan / Resonance Energy
**Last Updated:** March 25, 2026
