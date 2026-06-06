# TARTARIA WORLD OF WONDER — Appendix D: Control Reference
## Complete Input Design for PC (Keyboard+Mouse & Gamepad)

---

> *Every interaction is a restoration. Your hands conduct the resonance.*

**Cross-References:**
- [07_PC_UX.md](../07_PC_UX.md) — Full UX design & session flow
- [14_HAPTIC_FEEDBACK.md](../14_HAPTIC_FEEDBACK.md) — Haptic response per gesture
- [13_MINI_GAMES.md](../13_MINI_GAMES.md) — Mini-game-specific inputs
- [06_COMBAT_PROGRESSION.md](../06_COMBAT_PROGRESSION.md) — Combat control mechanics

---

## Table of Contents

1. [Design Philosophy](#design-philosophy)
2. [Core Gesture Dictionary](#core-gesture-dictionary)
3. [Context-Specific Controls](#context-specific-controls)
4. [Mini-Game Inputs](#mini-game-inputs)
5. [Giant-Mode Controls](#giant-mode-controls)
6. [HUD Interaction Map](#hud-interaction-map)
7. [Control Customization](#control-customization)
8. [Accessibility Remaps](#accessibility-remaps)
9. [Controller Support](#controller-support)

---

## Design Philosophy

### One-Thumb Guarantee
Every core gameplay action can be performed with a single thumb on the right hand while holding the phone in portrait or landscape. Two-thumb and advanced gestures enhance the experience but are never mandatory.

### Input Hierarchy
1. **Tap** — the default action for everything (select, interact, attack, harvest)
2. **Swipe** — directional intent (move, dodge, excavate, navigate)
3. **Hold** — depth/inspection (scan, inspect, charge attacks, options)
4. **Pinch** — spatial manipulation (zoom, resize, rotate camera)
5. **Draw** — precision/ritual (golden spiral, cymatic circles, tuning curves)

### Response Rules
- Every gesture earns a haptic response within 16 ms (see `14_HAPTIC_FEEDBACK.md`)
- Visual feedback accompanies every input — no silent taps
- Failed/invalid gestures get a gentle "miss" haptic — never punitive
- Gesture recognition tolerance: ±15° for directional swipes, ±20% for drawn shapes

---

## Core Gesture Dictionary

### Universal Gestures (Available in All Contexts)

| Gesture | Input | Action | Haptic Response |
|---|---|---|---|
| **Tap** | Single finger, < 200 ms | Select / Interact / Attack target | Light "click" (1.5 ms) |
| **Double Tap** | Two taps < 300 ms apart | Activate special ability / Giant-Mode | Medium "thud" (3 ms) |
| **Long Press** | Hold > 500 ms | Inspect / Context menu / Charge | Building rumble (continuous) |
| **Swipe Right** | Horizontal drag > 50 px | Dodge right / Navigate forward | Directional sweep (left→right) |
| **Swipe Left** | Horizontal drag > 50 px | Dodge left / Navigate back | Directional sweep (right→left) |
| **Swipe Up** | Vertical drag > 50 px | Jump / Charged attack release | Lift impulse |
| **Swipe Down** | Vertical drag > 50 px | Guard / Crouch / Cancel | Press-down impulse |
| **Pinch In** | Two fingers converge | Zoom out camera | Compression click |
| **Pinch Out** | Two fingers diverge | Zoom in camera | Expansion release |
| **Two-Finger Rotate** | Two fingers pivot | Rotate camera / Rotate object | Smooth rotation friction |
| **Three-Finger Tap** | Three simultaneous taps | Pause / System menu | Sharp triple-click |
| **Shake** | Physical phone shake | Optional excavation acceleration | Rumble burst |

### Contextual Gesture Modifiers

| Context | Tap Becomes | Swipe Becomes | Hold Becomes |
|---|---|---|---|
| **Exploration** | Move to location | Adjust camera | Resonance Scan |
| **Combat** | Attack target | Dodge/Evade | Charge resonance weapon |
| **Building** | Place structure | Adjust position | Rotate structure |
| **Excavation** | Hit dig spot | Clear mud layer | Deep scan |
| **Tuning** | Select frequency | Adjust waveform | Lock resonance |
| **Dialogue** | Advance text | Skip animation | View companion history |
| **Map** | Select zone | Pan view | Zone details |

---

## Context-Specific Controls

### Exploration Mode

| Action | One-Thumb | Two-Thumb | Advanced |
|---|---|---|---|
| Move | Tap destination | Virtual joystick (left thumb) | Tilt-to-move |
| Camera | Auto-follow | Drag right side of screen | Pinch zoom + two-finger rotate |
| Interact | Tap object when prompt appears | Same | Long-press for options menu |
| Resonance Scan | Tap-and-hold on ground/structure | Same | Swipe to expand scan radius |
| Companion Chat | Tap speech bubble | Same | Swipe up to see chat history |
| Switch Companion | Tap companion portrait | Same | Swipe between portraits |

### Combat Mode

| Action | One-Thumb | Two-Thumb | Advanced |
|---|---|---|---|
| Basic Attack | Tap target | Same | Rapid-tap for combo chain |
| Dodge | Swipe away from attack | Same + directional precision | Draw Fibonacci spiral for perfect dodge |
| Guard | Swipe down | Same | Hold swipe-down for sustained block |
| Resonance Strike | Long-press → release on target | Same | Draw tuning-fork pattern |
| Frequency Weapon Switch | Tap tuning wheel segment | Same | Swipe wheel to cycle |
| Cymatic Ability | Draw circle on screen | Same | Draw specific sacred geometry shapes |
| Ground Slam (Giant) | Double-tap ground | Same | Hold for charged slam |
| Target Lock | Tap enemy portrait | Same | Double-tap to cycle targets |

### Building Mode

| Action | One-Thumb | Two-Thumb | Advanced |
|---|---|---|---|
| Open Blueprint | Tap build icon | Same | Long-press for favorites |
| Place Structure | Tap valid location | Drag to precise position | Snap-to-grid toggle |
| Rotate | Tap rotate button | Two-finger rotate on structure | Hold + tilt phone |
| Scale Preview | Pinch on structure ghost | Same | Slider in detail panel |
| Confirm | Tap checkmark | Same | Double-tap structure |
| Cancel | Tap X / Swipe down | Same | Three-finger tap |
| Golden-Ratio Alignment | Auto-assist | Manual drag to grid nodes | Toggle overlay for precision |

### Excavation Mode

| Action | One-Thumb | Two-Thumb | Advanced |
|---|---|---|---|
| Surface Clear | Swipe across mud | Multi-finger fast clear | Shake phone for burst |
| Precision Dig | Tap specific spot | Same | Long-press for tool select |
| Layer Reveal | Swipe in direction of layers | Same | Pinch to peel layers |
| Artifact Collect | Tap glowing spot | Same | Long-press to inspect before collecting |
| Scan Under Mud | Long-press on surface | Same | Swipe to pan scan |

---

## Mini-Game Inputs

### Dome Tuning (Waveform Alignment)

| Action | Input | Description |
|---|---|---|
| Select frequency band | Tap band marker on spectrum | Choose Low / Mid / High Aether band |
| Adjust waveform | Drag waveform curve up/down | Match target resonance pattern |
| Fine-tune | Pinch waveform peaks | Precise amplitude adjustment |
| Lock frequency | Double-tap when aligned | Confirm match (±2% tolerance) |
| Emergency retune | Shake phone | Reset waveform to baseline |

### Rock Cutting (Golden Ratio)

| Action | Input | Description |
|---|---|---|
| Mark cut line | Drag finger along stone face | Draw the cutting path |
| Golden spiral cut | Draw spiral glyph on stone | Precision sacred-geometry cut |
| Measure angle | Two-finger spread on cut lines | Check golden-ratio alignment |
| Strike | Tap cut line firmly | Execute the cut |
| Rotate stone | Two-finger rotate | Examine different faces |

### Rail Tuning (Frequency Matching)

| Action | Input | Description |
|---|---|---|
| Select segment | Tap rail segment | Highlight for tuning |
| Adjust frequency | Slide tuning bar left/right | Match target Hz marker |
| Test resonance | Double-tap tuned segment | Play the segment's tone |
| Chain segments | Draw line connecting 2+ segments | Link tuned segments into a rail |
| Verify alignment | Long-press connected chain | Full-chain resonance check |

### Pipe Organ Performance

| Action | Input | Description |
|---|---|---|
| Play note | Tap organ key | Sound the corresponding pipe |
| Sustained note | Long-press key | Hold note for duration |
| Chord | Multi-finger tap (2-4 keys) | Play simultaneous notes |
| Crescendo | Swipe up while holding key | Increase volume/intensity |
| Bellows pump | Rhythmic tap on bellows icon | Maintain organ air pressure |

### Orphan Lullaby (Rhythm)

| Action | Input | Description |
|---|---|---|
| Follow note | Tap when note reaches marker | Rhythm timing input |
| Hold note | Long-press during sustained marker | Extended note timing |
| Harmony | Two-finger tap on dual markers | Multi-voice harmony |
| Tempo shift | Tilt phone slightly | Subtle tempo influence |

---

## Giant-Mode Controls

When Giant-Mode is activated (60-second burst, Moon 1+):

| Action | Input | Description |
|---|---|---|
| **Activate** | Double-tap giant icon (or skeleton key) | Transform to giant scale |
| **Walk** | Tap destination | Giant-scale pathfinding |
| **Grab Megalith** | Long-press on megalith | Grip and hold for transport |
| **Place Megalith** | Drag to position → release | Snap to alignment nodes |
| **Ground Slam** | Double-tap ground | AoE resonance attack |
| **Charged Slam** | Hold + release on ground | Larger AoE, screen-shake |
| **Inspect Top** | Swipe up on building | View giant-only inscriptions |
| **Tilt Direction** | Physical phone tilt | Subtle movement influence |
| **Deactivate** | Timer expires or three-finger tap | Return to normal scale |

**Giant-Mode Camera:** Automatically pulls to wide overhead angle. Pinch still available for zoom fine-tuning.

---

## HUD Interaction Map

### Minimal HUD (Default)

```
┌──────────────────────────────────────────┐
│ [RS: 73%]  ← tap to expand       [🌙5] │
│                                          │
│              (GAME WORLD)                │
│           90% of screen area             │
│                                          │
│ [⚡AE]  [💬Chat]       [🎵Tune]  [⚙️]  │
└──────────────────────────────────────────┘
```

| Element | Tap | Long-Press | Swipe |
|---|---|---|---|
| **RS Counter** | Expand to full zone RS breakdown | Show global grid progress | — |
| **Moon Glyph** | Show Moon progress / day counter | Show quest log | — |
| **Aether Meter** | Show exact AE count + rate | Show resource panel | Swipe up for full inventory |
| **Companion Chat** | Show last 5 lines | Open full chat history | Swipe up for full log |
| **Tuning Wheel** | Select active frequency | Enter free-tune mode | Swipe to cycle bands |
| **Settings Gear** | Open pause/settings menu | — | — |

### Expanded HUD (Tap RS or Aether)

| Panel | Content | Dismiss |
|---|---|---|
| **Resource Panel** | AE / BM / SP / RS / HF values per zone | Tap outside or swipe down |
| **Quest Tracker** | Active quest steps + progress bars | Tap outside or swipe down |
| **Mini-Map** | Zone overview + objective markers | Pinch to resize, tap outside to dismiss |
| **Companion Panel** | Trust levels + active abilities | Tap outside |

---

## Control Customization

### Adjustable Settings

| Setting | Options | Default |
|---|---|---|
| **Layout** | Right-hand / Left-hand / Ambidextrous | Right-hand |
| **Joystick Position** | Left / Center-Left / Right / Custom (drag) | Left |
| **Joystick Type** | Fixed / Floating / Hidden (tap-only) | Floating |
| **Button Size** | Small / Medium / Large / Extra-large | Medium |
| **Camera Sensitivity** | Slider 0.1 – 3.0 | 1.0 |
| **Zoom Sensitivity** | Slider 0.5 – 2.0 | 1.0 |
| **Gesture Threshold** | Relaxed / Standard / Precise | Standard |
| **Tap-vs-Hold** | Per-action toggle | Default mapping |
| **Vibration Intensity** | Off / Low / Medium / High | Medium |
| **Shake-to-Action** | On / Off | Off |
| **Auto-Target** | On / Off | On |
| **Auto-Camera** | On / Off | On |

### Custom Control Profiles
- **3 save slots** for control profiles
- Import/export profiles as shareable codes
- Profiles activate automatically by game mode (exploration / combat / building) if player configures separate profiles

---

## Accessibility Remaps

### Switch Control Compatibility
- All actions mapped to **Windows accessibility APIs** and Xbox Adaptive Controller
- Sequential scanning of interactive elements
- Dwell selection (configurable timer: 0.5s – 5.0s)
- External switch support via Bluetooth

### AssistiveTouch Integration
- Custom gestures mapped to AssistiveTouch menu
- Multi-finger gestures (pinch, rotate, three-finger tap) collapse to single-tap virtual buttons
- Giant-Mode activation available as a single virtual button

### Motor Accessibility Options

| Option | Description | Default |
|---|---|---|
| **One-Touch Mode** | All actions performable via single taps + on-screen buttons | Off |
| **Auto-Combat** | Player marks target, AI handles timing & dodging | Off |
| **Extended Gesture Time** | Gesture recognition window doubled (tap < 400 ms, hold > 1000 ms) | Off |
| **Large Touch Targets** | All interactive zones expanded 50% | Off |
| **Aim Assist** | Combat/interaction targeting with generous lock-on radius | On |
| **Sequential Inputs** | Multi-finger gestures converted to sequential single-finger steps | Off |
| **Reduced Motion** | Minimized parallax, screen shake, rapid animations | Off |

### Visual Accessibility

| Option | Description |
|---|---|
| **Color-Blind Modes** | Protanopia, Deuteranopia, Tritanopia filters |
| **High Contrast HUD** | Solid backgrounds behind all text/icons |
| **Enlarged Text** | UI text scales 1.0× – 2.5× |
| **Screen Reader** | Windows Narrator / NVDA / JAWS narrates all UI elements + game state changes |
| **Subtitle Size** | Small / Medium / Large / Extra-large |
| **Subtitle Background** | Transparent / Semi-opaque / Solid |

---

## Controller Support

### MFi / Bluetooth Game Controllers

| Button | Action | Context |
|---|---|---|
| **Left Stick** | Move character | Exploration / Combat |
| **Right Stick** | Camera control | All |
| **A / Cross** | Interact / Confirm / Attack | All |
| **B / Circle** | Cancel / Dodge / Back | All |
| **X / Square** | Resonance Scan / Special | Exploration / Combat |
| **Y / Triangle** | Companion menu / Inspect | All |
| **L1** | Cycle weapon left | Combat |
| **R1** | Cycle weapon right | Combat |
| **L2** | Guard / Block | Combat |
| **R2** | Charged attack | Combat |
| **D-Pad Up** | Quest log | All |
| **D-Pad Down** | Inventory | All |
| **D-Pad Left** | Map | All |
| **D-Pad Right** | Companion chat | All |
| **Start** | Pause / Settings | All |
| **Select** | Toggle HUD | All |

**Note:** Controller is fully optional. Touch controls remain primary. No advantage gained from controller use.

---

*The phone is the instrument. The fingers are the tuning forks. The world resonates with every touch.*

---

**Document Status:** FINAL  
**Author:** Nathan / Resonance Energy  
**Last Updated:** March 25, 2026
