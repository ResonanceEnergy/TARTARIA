# TARTARIA WORLD OF WONDER — Phase 1 MVP Build Specification
## Vertical Slice: From First Dig to First Wonder

---

> *"If the first 15 minutes don't feel like magic, nothing else matters."*

**Document Purpose:** Exact specification for the Phase 1 vertical slice (Months 1–3). Every system described here must be playable on an iPhone 17 Pro at 60 FPS by the end of Month 3. This is the GATE 1 deliverable — the foundation upon which everything else is built.

**Cross-References:**
- [00_MASTER_GDD.md](00_MASTER_GDD.md) — Full game overview
- [10_ROADMAP.md](10_ROADMAP.md) — Development roadmap context
- [01_LORE_BIBLE.md](01_LORE_BIBLE.md) — Lore foundation
- [07_MOBILE_UX.md](07_MOBILE_UX.md) — UX/Touch patterns
- [14_HAPTIC_FEEDBACK.md](14_HAPTIC_FEEDBACK.md) — Core Haptics spec

---

## Table of Contents

1. [Vertical Slice Scope](#1-vertical-slice-scope)
2. [Target Hardware & Performance Budgets](#2-target-hardware-performance-budgets)
3. [Unity Project Architecture](#3-unity-project-architecture)
4. [Core Loop Implementation](#4-core-loop-implementation)
5. [Aether Field System](#5-aether-field-system)
6. [Resonance Score Engine](#6-resonance-score-engine)
7. [Zone: Echohaven](#7-zone-echohaven)
8. [Building Restoration System](#8-building-restoration-system)
9. [Tuning Mini-Game](#9-tuning-mini-game)
10. [Companion: Milo](#10-companion-milo)
11. [Combat: Harmonic Combat Prototype](#11-combat-harmonic-combat-prototype)
12. [Camera & Controls](#12-camera-controls)
13. [Audio & Haptics Foundation](#13-audio-haptics-foundation)
14. [Visual Pipeline: Shaders & VFX](#14-visual-pipeline-shaders-vfx)
15. [Save & Persistence](#15-save-persistence)
16. [GATE 1 Exit Criteria](#16-gate-1-exit-criteria)
17. [Week-by-Week Sprint Plan](#17-week-by-week-sprint-plan)
18. [Risk Register](#18-risk-register)
19. [Appendix A: Asset Budget Summary](#appendix-a-asset-budget-summary)
20. [Appendix B: Key Technical Decisions](#appendix-b-key-technical-decisions)

---

## 1. Vertical Slice Scope

### What's In
| Feature | Scope | Status |
|---|---|---|
| Echohaven zone | 500m radius, terrain + architecture blockout | Full |
| Aether field | 3-band visualization, flow sim, grid partition | Full |
| Resonance Score | Golden-ratio validation, RS accumulation | Full |
| Building restoration | 3 buildings: dome, fountain, spire | Full |
| Tuning mini-game | Frequency matching for Aether nodes (3 variants) | Full |
| Milo companion | Dialogue, idle chatter, follow AI, lore triggers | Full |
| Mud Golem enemy | 1 enemy type, harmonic combat, patrol AI | Full |
| Core Haptics | Touch feedback on all interactions | Full |
| Adaptive music | 2-layer prototype (ambient + RS-reactive) | Prototype |
| Day/night cycle | 17-hour visual cycle (no calendar system yet) | Prototype |

### What's NOT In (Phase 2+)
- Giant Mode, skill trees, campaign/Moon system, additional zones
- Additional companions (Lirael, Thorne), dialogue trees
- Boss encounters, star forts, DLC content
- Battle Pass, IAP, monetization, social features
- Cloud save, analytics, crash reporting

### The 15-Minute Demo
The vertical slice must support this exact play session:

```
0:00  — Player awakens in a modern ruin. Mud everywhere. A hum beneath.
1:00  — First movement. Touch to walk. Camera follows. The hum grows.
2:00  — Discovery: a buried dome, only the tip visible. Milo appears.
3:00  — Milo speaks: "You can hear it, can't you? The frequency beneath the mud."
4:00  — Tutorial: Aether scan. The dome lights up. Connection points revealed.
5:00  — First tuning: frequency-match mini-game at Node 1. Success. RS +10.
6:00  — The dome begins to emerge. Mud dissolves. Golden stone appears.
7:00  — Second tuning: Node 2. Harder. Player discovers 432 Hz baseline.
8:00  — Third tuning: Node 3. RS crosses 50. Aether field visible.
9:00  — Building restored. Full dome. Fountain inside begins to flow.
10:00 — Milo: "Something's wrong. The corruption noticed us."
11:00 — Mud Golem emerges. First combat. Harmonic attacks. Dodge.
12:00 — Combat victory. Golem dissolves into purified mud.
13:00 — RS crosses 75. The zone SHIFTS — colors brighten, music swells.
14:00 — Player discovers the spire. Third building. Aether pours from the dome.
14:30 — Milo: "This is just the beginning. Can you feel it? The world wants to come back."
15:00 — View from the restored dome: the horizon. Ruins everywhere. Promise and scale.
```

---

## 2. Target Hardware & Performance Budgets

### Primary Target
| Spec | Value |
|---|---|
| Device | iPhone 17 Pro (A19 Bionic) |
| OS | iOS 18.0+ |
| GPU API | Metal 3 |
| Upscaling | MetalFX Temporal |
| Render Resolution | 1080p → 2796×1290 native via MetalFX |
| Frame Target | 60 FPS sustained, 120 FPS UI elements |
| Thermal Envelope | ≤ 38°C skin temperature after 30 min play |
| Memory Budget | ≤ 1.8 GB (of 8 GB device RAM) |

### Fallback Target
| Spec | Value |
|---|---|
| Device | iPhone 15 Pro (A17 Pro) |
| Render Resolution | 720p → native via MetalFX |
| Frame Target | 30 FPS sustained |
| Thermal Envelope | ≤ 40°C after 30 min |
| Memory Budget | ≤ 1.5 GB |

### Per-Frame Budgets (at 60 FPS = 16.67ms)
| System | Budget (ms) | Notes |
|---|---|---|
| Render (URP) | 8.0 | Forward+, single directional light |
| Aether GPU compute | 2.0 | Compute shader, 3 bands |
| Physics + ECS | 1.5 | Burst compiled jobs |
| AI (companion + enemy) | 1.0 | DOTS-based state machines |
| Audio | 0.5 | Spatial audio + adaptive layers |
| Haptics | 0.2 | Core Haptics CHHapticEngine |
| Input processing | 0.3 | Touch + gesture recognition |
| UI | 0.5 | SwiftUI overlay via Unity bridge |
| Headroom | 2.67 | Thermal spikes, GC, OS interrupts |

### Thermal State Machine
```
NOMINAL (< 35°C)
  → Full quality, 60 FPS, all VFX
WARM (35–38°C)
  → Reduce particle count 50%, disable secondary shadows
HOT (38–40°C)
  → Drop to 30 FPS, reduce Aether bands to 2
CRITICAL (> 40°C)
  → Force 30 FPS, minimal VFX, save & warn player
```

---

## 3. Unity Project Architecture

### Engine Configuration
| Setting | Value |
|---|---|
| Unity Version | Unity 6 LTS (6000.x) |
| Render Pipeline | URP (Universal Render Pipeline) |
| ECS Framework | DOTS (Entities 1.x, Burst, Jobs) |
| Asset Management | Addressables (local for Phase 1) |
| Build Backend | IL2CPP (ARM64) |
| Scripting Runtime | .NET Standard 2.1 |

### Project Folder Structure
```
Assets/
├── _Project/
│   ├── Art/
│   │   ├── Shaders/           # URP shaders, compute shaders
│   │   ├── Materials/         # PBR materials, Tartarian palette
│   │   ├── Textures/          # Albedo, Normal, Mask maps
│   │   ├── Models/            # FBX: buildings, characters, props
│   │   ├── VFX/               # Visual Effect Graph assets
│   │   └── UI/                # UI sprites, fonts
│   ├── Audio/
│   │   ├── Music/             # Adaptive layers (432 Hz base)
│   │   ├── SFX/               # Interaction, combat, ambient
│   │   └── Haptics/           # AHAP files for Core Haptics
│   ├── Scripts/
│   │   ├── Core/              # Aether, RS, golden ratio engine
│   │   ├── Gameplay/          # Building, tuning, combat
│   │   ├── AI/                # Companion, enemy state machines
│   │   ├── Camera/            # Camera rig + touch controls
│   │   ├── Audio/             # Adaptive music controller
│   │   └── Save/              # Local persistence
│   ├── Scenes/
│   │   ├── Boot.unity         # Initialization
│   │   ├── Echohaven.unity    # Main zone scene
│   │   └── UI_Overlay.unity   # Additive UI scene
│   ├── Data/
│   │   ├── Buildings/         # ScriptableObjects: building definitions
│   │   ├── Enemies/           # ScriptableObjects: enemy configs
│   │   ├── Dialogue/          # JSON: Milo's dialogue trees
│   │   └── Tuning/            # ScriptableObjects: tuning puzzles
│   └── Config/
│       ├── PerformanceProfile.asset
│       ├── GoldenRatioConstants.asset
│       └── AetherConfig.asset
├── Plugins/
│   └── iOS/                   # Core Haptics bridge, Metal shaders
└── StreamingAssets/
    └── Audio/                 # Runtime-loaded adaptive stems
```

### DOTS Architecture
| System Group | Systems | Priority |
|---|---|---|
| AetherSimGroup | AetherFlowSystem, AetherBandSystem, AetherVisualizationSystem | High |
| ResonanceGroup | RSAccumulationSystem, GoldenRatioValidationSystem | High |
| BuildingGroup | RestorationProgressSystem, MudDissolutionSystem | Medium |
| AIGroup | CompanionBehaviorSystem, EnemyPatrolSystem, EnemyEngageSystem | Medium |
| CombatGroup | HarmonicCombatSystem, DamageResolutionSystem | Medium |
| AudioGroup | AdaptiveMusicSystem, SpatialAudioSystem | Low |
| HapticsGroup | HapticEventSystem, HapticThermalGateSystem | Low |

---

## 4. Core Loop Implementation

### Loop Definition
```
EXPLORE → DISCOVER → TUNE → RESTORE → HARVEST → EXPAND → COMBAT → LOOP
```

### Loop Systems

**EXPLORE:**
- Player navigates Echohaven using touch controls
- Aether overlay (toggle) reveals hidden energy flows and buried structures
- Milo provides contextual commentary ("Look at the formation in that hillside...")

**DISCOVER:**
- Building detection radius: 30m (Aether scan highlights buried structure)
- Discovery animation: resonance pulse reveals geometry beneath mud
- RS reward: +5 per discovery

**TUNE:**
- Approach connection nodes (3 per building)
- Activate tuning mini-game (see Section 9)
- Success: node activates, building restoration begins
- RS reward: +10–25 per node (based on accuracy)

**RESTORE:**
- When all 3 nodes complete: building emerges (5-second cinematic)
- Mud dissolves via shader transition (see Section 14)
- Building becomes functional (zone buff, Aether generator)
- RS reward: +50 per building restored

**HARVEST:**
- Restored buildings generate Aether passively
- Player collects by proximity (within 15m)
- Aether used to power tuning on subsequent buildings
- Loop reinforcement: restoration feeds future restoration

**COMBAT:**
- Corruption responds to restoration: enemies spawn at RS thresholds (25, 50, 75)
- Combat encounters are brief (30–60s) and avoidable with skill
- Defeating enemies yields purified mud (crafting material for Phase 2)

---

## 5. Aether Field System

### 3-Band Model
The Aether field is the game's core visual identity — a flowing energy field visible throughout the world.

| Band | Frequency | Color | Behavior | Visual |
|---|---|---|---|---|
| Telluric | 7.83 Hz (Schumann) | Deep amber | Flows along terrain, pools in valleys | Ground fog, golden mist |
| Harmonic | 432 Hz (base) | Bright gold | Connects restored structures | Light threads, arcs |
| Celestial | 1296 Hz (3×432) | White-gold | Descends from sky at peak RS | Particle rain, aurora wisps |

### GPU Compute Implementation
```
// Aether simulation runs as Metal compute shader
// Grid: 64×64×32 voxels covering 500m zone
// Each voxel: float4 (density, flow_x, flow_y, flow_z) per band

AetherSimKernel:
  1. Advect density along flow field (semi-Lagrangian)
  2. Apply resonance sources (restored buildings emit)
  3. Apply resonance sinks (corruption absorbs)
  4. Apply golden-ratio enforcement (φ-proportioned dissipation)
  5. Write to 3D texture for URP to sample

Per frame: ~120k voxels × 3 bands = 360k cells @ ~5.5 FLOPS/cell
Budget: 2.0ms on A19 GPU (well within)
```

### Resonance Source/Sink Model
| Entity | Type | Strength | Radius |
|---|---|---|---|
| Restored Dome | Source | 1.0 | 50m |
| Restored Fountain | Source | 0.6 | 30m |
| Restored Spire | Source | 0.8 | 40m |
| Tuned Node | Source | 0.3 | 15m |
| Mud Golem | Sink | -0.5 | 20m |
| Corruption Patch | Sink | -0.2 | 10m |

### Visual Rendering
- Aether rendered as volumetric particles in URP's Forward+ path
- Instanced quad billboards (2k–8k particles per band, LOD by distance)
- Color palette: golden (#D4A017) → white-gold (#F5E6CC) → amber (#FF8C00)
- Alpha blending with multiplicative mode for a "glowing mist" look
- MetalFX temporal reprojection: render at half-res, upscale; saves 40% fill rate

---

## 6. Resonance Score Engine

### RS Accumulation Rules
| Action | RS Reward | Multiplier Condition |
|---|---|---|
| Discover buried structure | +5 | ×1.618 if golden-ratio proportioned |
| Tune Aether node (basic) | +10 | ×1.5 if within 2% of 432 Hz |
| Tune Aether node (perfect) | +25 | Auto ×1.618 |
| Restore building | +50 | ×2.0 if all 3 nodes were perfect |
| Defeat Mud Golem | +15 | ×1.3 if defeated with harmonics only |
| Collect Aether | +1-3 | ×1.1 per consecutive collection |

### Golden Ratio Validation Engine
Every RS-modifying event passes through the φ-validator:

```csharp
public static class GoldenRatioValidator
{
    const float PHI = 1.6180339887f;
    const float TOLERANCE = 0.02f; // 2% for "close enough"

    public static float GetMultiplier(float ratio)
    {
        float deviation = Mathf.Abs(ratio - PHI) / PHI;
        if (deviation <= TOLERANCE)
            return PHI;           // Perfect golden ratio
        if (deviation <= 0.1f)
            return 1.0f + (0.618f * (1.0f - deviation / 0.1f));
        return 1.0f;             // No bonus
    }
}
```

### RS Thresholds (Vertical Slice)
| Threshold | Event | Visual Change |
|---|---|---|
| 0 | Start | Desaturated, grey-brown palette |
| 25 | First Golem spawns | Faint golden hue at edges |
| 50 | Aether becomes visible (ambient) | Golden mist, music layer 2 |
| 75 | Zone shift | Full color, Aether flowing, Celestial band |
| 100 | Zone complete | Aurora wisps, harmonic hum, all buildings active |

---

## 7. Zone: Echohaven

### Geography
- **Shape:** Roughly circular, 500m radius, cupped terrain (hills on 3 sides, open vista on 4th)
- **Elevation:** Central depression where the dome is buried, rising 30m to the south ridge
- **Water:** Underground spring emerges post-restoration as a small stream
- **Vegetation:** Sparse modern weeds on mud layer; lush Tartarian plants revealed by restoration

### Architecture (3 Restorable Buildings)

**The Dome — "Listeners' Hall"**
- Size: 25m diameter, 18m height (golden ratio: 25/18 ≈ 1.389 — close to √φ)
- Buried: 80% below mud, only apex visible at start
- Interior: acoustic chamber with central fountain, rose-window style openings
- Function: Primary Aether generator (+1.0 source strength)
- Narrative: The building where Tartarians gathered to "listen to the Earth"

**The Fountain — "Thread of Memory"**
- Size: 8m diameter basin, 5m central column (8/5 = φ exactly)
- Buried: 95% — completely hidden until dome restoration reveals it
- Function: Purifies local corruption, small Aether source
- Special: When active, water carries harmonic sound — the first hint of 432 Hz

**The Spire — "The First Note"**
- Size: 3m base diameter, 15m height (base-to-height ratio approaches φ²)
- Buried: 60% — tip visible as a "strange rock formation"
- Function: Long-range Aether beacon, extends Aether visibility to zone edge
- Narrative: A tuning spire — the Tartarian equivalent of a radio tower

### Points of Interest (Non-Restorable)
| Location | Description | Discovery Reward |
|---|---|---|
| The Mud Pools | Corruption hotspots, Golem spawn points | +5 RS each (3 pools) |
| The Carved Stone | Fragment of a larger structure (DLC tease) | +5 RS, Milo dialogue |
| The Overlook | Southern ridge with vista of distant ruins | +10 RS, camera pan |
| The Root Chamber | Small underground cavity with Aether glow | +5 RS, lore text |

### Terrain Technical Spec
| Param | Value |
|---|---|
| Heightmap Resolution | 1025×1025 |
| Terrain Size | 1000m × 1000m (500m radius playable) |
| Texture Splat Layers | 4 (mud, stone, grass, Tartarian tile) |
| Detail Objects | ~5k instances (grass, debris, small rocks) |
| Trees/Large Props | ~100 instances |
| LOD Distance | 100m (detail), 250m (medium), 500m (silhouette) |

---

## 8. Building Restoration System

### State Machine
```
BURIED
  → Player discovers (Aether scan)
REVEALED
  → Player approaches (3 node markers appear)
TUNING
  → Player completes node 1/2/3 (progressive)
  → Each node: mud recedes, structure emerges further
EMERGING
  → All 3 nodes complete: cinematic emergence (5 sec)
  → Mud dissolution shader plays
ACTIVE
  → Building functional, Aether generating
  → Visual polish: clean stone, flowing elements, light
```

### Mud Dissolution Shader
The signature visual effect: mud peeling away to reveal golden stone.

```
// Dissolution driven by per-texel noise mask + world-space height gradient
// Shader inputs: _DissolveProgress (0-1), _NoiseTex, _MudColor, _StoneColor

// In fragment shader:
float noise = tex2D(_NoiseTex, uv).r;
float heightFactor = (worldPos.y - _BuildingBase) / _BuildingHeight;
float dissolve = _DissolveProgress + heightFactor * 0.3;
float edge = smoothstep(dissolve - 0.05, dissolve + 0.05, noise);
// Edge glow: golden emission at dissolution front
float edgeGlow = smoothstep(dissolve - 0.02, dissolve, noise)
               - smoothstep(dissolve, dissolve + 0.02, noise);
float3 color = lerp(_StoneColor, _MudColor, edge);
color += _GoldenEmission * edgeGlow * 3.0;
```

- Duration: 5 seconds for full dissolution
- Particle accompaniment: rising golden particles at dissolution front
- Haptic accompaniment: sustained warm vibration ramping with progress
- Audio accompaniment: rising crystalline tone converging on 432 Hz

### Building Data (ScriptableObject)
```csharp
[CreateAssetMenu(menuName = "Tartaria/BuildingDefinition")]
public class BuildingDefinition : ScriptableObject
{
    public string buildingName;
    public string loreDescription;
    public float goldenRatio;          // Target proportion for RS bonus
    public float aetherSourceStrength;
    public float aetherSourceRadius;
    public int nodeCount;              // Always 3 for Phase 1
    public TuningPuzzleConfig[] nodePuzzles;
    public GameObject buriedPrefab;
    public GameObject revealedPrefab;
    public GameObject activePrefab;
    public AudioClip emergenceSound;
    public HapticPattern emergenceHaptic;
}
```

---

## 9. Tuning Mini-Game

### Overview
The primary skill-based interaction. The player matches frequencies to "tune" buried Aether nodes, unlocking buildings.

### 3 Variants (Phase 1)

**Variant A: Frequency Slider**
- Interface: horizontal slider, target frequency shown as a golden band
- Player drags to match; accuracy determines RS reward
- Audio: real-time sine wave at the selected frequency (432 Hz target)
- Haptic: vibration frequency matches slider position — player FEELS the right frequency
- Time limit: 15 seconds

**Variant B: Waveform Trace**
- Interface: a golden waveform scrolls; player traces it with finger
- Accuracy: percentage of waveform correctly traced
- Audio: the waveform IS the audio — the player draws sound
- Haptic: smooth vibration when on-trace, sharp buzz when off
- Time limit: 20 seconds

**Variant C: Harmonic Pattern**
- Interface: 5 circles appear in sequence; player taps them in rhythm
- Accuracy: timing precision (±100ms = perfect, ±200ms = good, ±300ms = ok)
- Audio: each tap produces a note; correct sequence creates a chord
- Haptic: each tap produces a distinct tap pattern; correct rhythm feels natural
- Time limit: 10 seconds

### Scoring
| Accuracy | RS Multiplier | Visual Feedback |
|---|---|---|
| Perfect (>95%) | ×1.618 (φ) | Golden burst, full emergence | 
| Great (80–95%) | ×1.3 | Silver burst, strong emergence |
| Good (60–80%) | ×1.0 | Bronze pulse, standard emergence |
| Fail (<60%) | Retry (no penalty) | Red flash, node resets |

### Difficulty Scaling
| Node | Variant | Speed | Tolerance | Time |
|---|---|---|---|---|
| 1st (any building) | A (Slider) | Slow | ±8% | 15s |
| 2nd | B or C (random) | Medium | ±5% | 20s |
| 3rd | C or A (random) | Fast | ±3% | 10s |

---

## 10. Companion: Milo

### Character Spec
| Attribute | Detail |
|---|---|
| Name | Milo |
| Species | Fox-like creature, Tartarian origin |
| Size | 40cm tall (thigh-height to player) |
| Visual | Luminous fur, golden-tipped ears, eyes that glow with Aether |
| Personality | Curious, encouraging, occasionally sarcastic, loyal |
| Voice | Warm, slightly childlike, clear enunciation |
| Function | Tutorial guide, lore delivery, emotional anchor |

### AI Behavior (DOTS State Machine)
```
States:
  FOLLOW    — Stay within 3m of player, match pace
  IDLE      — Play ambient animations (sniff, stretch, look around)
  REACT     — Face point of interest, vocalise
  SPEAK     — Deliver dialogue line (triggered by proximity/event)
  HIDE      — During combat, find cover within 10m
  CELEBRATE — Post-restoration, jump/spin animation

Transitions:
  FOLLOW → IDLE      : Player stationary > 5s
  IDLE → REACT       : POI within 20m (building, corruption, vista)
  REACT → SPEAK      : POI is dialogue trigger
  ANY → HIDE         : Combat initiated
  HIDE → CELEBRATE   : Combat ended + building restored
  CELEBRATE → FOLLOW : After 3s
```

### Dialogue System (Phase 1)
- 40 total voice lines (estimated 25 minutes of content)
- Categories:
  - **Tutorial (10):** Teach core mechanics ("Try scanning with Aether vision!")
  - **Discovery (8):** React to finding buildings ("That's Tartarian stonework! Look at the proportions!")
  - **Lore (8):** Deliver world-building context ("This hall was where they listened to the Earth's pulse.")
  - **Ambient (8):** Idle chatter ("The air here tastes different. Cleaner.")
  - **Combat (4):** Warning/encouragement ("Something's coming from the mud!" / "You've got this!")
  - **Celebration (2):** Post-restoration joy ("It's BACK! I can feel it humming!")

### Dialogue Trigger System
```json
{
  "triggers": [
    {
      "id": "milo_discover_dome",
      "type": "proximity",
      "target": "dome_buried",
      "radius": 20,
      "line": "Do you see that? The apex... that's Tartarian stonework.",
      "priority": 10,
      "cooldown": 0,
      "playOnce": true
    }
  ]
}
```

---

## 11. Combat: Harmonic Combat Prototype

### Design Philosophy
Combat in Tartaria is not about violence — it's about FREQUENCY. The player doesn't kill enemies; they RETUNE them. Corruption is dissonance; resolution is harmony.

### Player Abilities (Phase 1: 3 abilities)
| Ability | Input | Effect | Cooldown |
|---|---|---|---|
| Resonance Pulse | Tap enemy | AOE burst, pushes enemies back, minor damage | 2s |
| Harmonic Strike | Swipe toward enemy | Directed attack, high single-target damage | 3s |
| Frequency Shield | Two-finger hold | absorbs incoming attacks for 3s | 8s |

### Mud Golem (Enemy Spec)
| Attribute | Value |
|---|---|
| Size | 2.5m tall (1.5x player height) |
| HP | 100 (3-4 Harmonic Strikes to defeat) |
| Speed | 0.6× player speed |
| Attack | Melee slam (20 damage, 1s windup — telegraphed) |
| Weakness | Resonance Pulse stuns for 2s after 3 consecutive hits |
| Spawn Trigger | RS reaches 25, 50, 75 (1 golem each) |
| Death | Dissolution into purified mud (particles + haptic) |
| Drop | Purified Mud × 3 (crafting material for Phase 2) |

### Combat Flow
```
Golem spawns (mud eruption VFX, rumble haptic)
  → Player gets warning (Milo: "Watch out!")
  → 3s grace period (golem forming)
  → Golem patrols toward nearest restored building
  → Player engages:
     Tap: Resonance Pulse (push back, buy time)
     Swipe: Harmonic Strike (deal damage)
     Hold: Shield (block Golem slam)
  → After 3-4 strikes: Golem dissolution
  → RS +15, purified mud drops, Milo celebrates
Total encounter: 30-60 seconds
```

### DOTS Combat System
```csharp
// CombatSystem runs in CombatGroup, processes HarmonicAttackComponent
// Enemy health stored as ECS component, no MonoBehaviour

[UpdateInGroup(typeof(CombatSystemGroup))]
public partial struct HarmonicCombatSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Process all pending harmonic attacks
        // Calculate resonance damage (base × frequency accuracy)
        // Apply to enemy health components
        // Trigger dissolution when HP <= 0
    }
}
```

---

## 12. Camera & Controls

### Camera Rig
| Mode | Behavior |
|---|---|
| Exploration | 3/4 top-down, 45° pitch, 15m follow distance, smooth lerp |
| Close-Up | Zoom to 5m on POI approach, dolly along golden spiral |
| Tuning | Fixed overhead, slight tilt toward tuning node |
| Combat | Pull back to 20m, wider FOV, track nearest enemy |
| Cinematic | Pre-authored Cinemachine paths for restoration reveals |

### Touch Controls
| Gesture | Action |
|---|---|
| Tap ground | Move to location (pathfinding) |
| Tap object | Interact (examine, start tuning) |
| Tap enemy | Resonance Pulse attack |
| Swipe (combat) | Harmonic Strike |
| Two-finger hold | Frequency Shield |
| Pinch | Zoom |
| Two-finger rotate | Camera orbit |
| Long press | Aether vision toggle |

### Control Configuration
- Virtual joystick option (accessibility)
- Haptic confirmation on all taps
- Dead zone: 10px to prevent misfire
- Gesture recognition priority: combat > interaction > movement

---

## 13. Audio & Haptics Foundation

### Audio Architecture
| Layer | Content | Trigger |
|---|---|---|
| Ambient Bed | Environmental atmo (wind, distant rumble) | Always playing |
| RS Layer | Harmonic drones, intensity tracks RS | RS threshold crossings |
| Interactive | SFX: tuning, combat, discovery | Player actions |
| Dialogue | Milo voice lines | Event triggers |

### 432 Hz Foundation
All musical content tuned to A4 = 432 Hz (not standard 440 Hz). This is the game's core audio identity.

| Frequency | Usage |
|---|---|
| 7.83 Hz | Telluric Aether band (sub-bass pulse) |
| 432 Hz | Base tuning, Resonance Pulse SFX, building emergence |
| 528 Hz | Healing/restoration overlay |
| 1296 Hz | Celestial band, high RS states |

### Adaptive Music System (2-Layer Prototype)
```
Layer 1 (Bed): Always active
  - RS 0-50: Minor key, sparse, melancholic — solo cello (432 Hz)
  - RS 50-100: Major key, lush, hopeful — full strings + crystalline synth

Layer 2 (Reactive): Event-driven
  - Discovery: Ascending arpeggio (harp)
  - Tuning: Real-time frequency matching (player-controlled pitch)
  - Combat: Percussive rhythm, dissonant overtones
  - Restoration: Expanding harmonic series (brass + choir)
```

### Core Haptics Integration
| Event | Haptic Pattern | Duration | Intensity |
|---|---|---|---|
| Footstep | Single tap | 20ms | 0.3 |
| Discovery | Rising buzz | 500ms | 0.4→0.8 |
| Tuning (on-frequency) | Sustained hum | Continuous | 0.5 |
| Tuning (off-frequency) | Irregular buzz | Continuous | 0.3 |
| Perfect tune | Warm cascade | 1000ms | 0.6→1.0→0.0 |
| Building emergence | Rolling wave | 5000ms | 0.3→1.0 |
| Golem spawn | Deep rumble | 2000ms | 0.7 |
| Combat hit | Sharp snap | 50ms | 0.9 |
| Golem death | Dissolving crackle | 1500ms | 0.8→0.0 |

### AHAP File Example (Building Emergence)
```json
{
  "Version": 1.0,
  "Pattern": [
    {"Event": {"Time": 0.0, "EventType": "HapticContinuous",
      "EventParameters": [
        {"ParameterID": "HapticIntensity", "ParameterValue": 0.3},
        {"ParameterID": "HapticSharpness", "ParameterValue": 0.2}
      ],
      "EventDuration": 5.0}},
    {"ParameterCurve": {
      "ParameterID": "HapticIntensityControl",
      "Time": 0.0,
      "ParameterCurveControlPoints": [
        {"Time": 0.0, "ParameterValue": 0.3},
        {"Time": 2.5, "ParameterValue": 0.7},
        {"Time": 4.0, "ParameterValue": 1.0},
        {"Time": 5.0, "ParameterValue": 0.0}
      ]}}
  ]
}
```

---

## 14. Visual Pipeline: Shaders & VFX

### Tartarian Color Palette
| Name | Hex | Usage |
|---|---|---|
| Tartarian Gold | #D4A017 | Primary Aether, UI accents, building highlight |
| Deep Mud | #4A3728 | Corruption, buried state, pre-restoration |
| Clean Stone | #E8DCC8 | Restored Tartarian architecture |
| Celestial White | #F5E6CC | High RS states, celestial band |
| Corruption Red | #8B1A1A | Enemy accents, dissonance VFX |
| Sky Gradient | #87CEEB → #FFD700 | Dynamic sky based on RS |

### Key Shaders (Phase 1)
| Shader | Purpose | Technique |
|---|---|---|
| MudDissolution | Building restoration reveal | Noise-masked dissolve + edge glow |
| AetherFlow | Volumetric Aether visualization | Billboard particles + compute |
| TartarianStone | Clean architectural surfaces | PBR + emissive detail maps |
| CorruptionPulse | Enemy/corruption visual | Animated emission, vertex displacement |
| SkyGradient | RS-reactive sky | Gradient blend driven by RS value |

### VFX (Visual Effect Graph)
| Effect | Particle Count | Trigger |
|---|---|---|
| Aether Background | 2,000–8,000 | Always (LOD) |
| Dissolution Particles | 500 | Building restoration |
| Resonance Pulse | 200 | Player attack |
| Golem Spawn | 300 | Enemy appearance |
| Golem Death | 400 | Enemy defeat |
| Golden Burst (perfect tune) | 150 | Perfect mini-game |

---

## 15. Save & Persistence

### Phase 1: Local Only
- Data: JSON serialized to Application.persistentDataPath
- Contents: RS value, building states (buried/revealed/tuning/active), node completion flags, discovered POIs, Milo dialogue history
- Auto-save: on every RS change, building state change, and app backgrounding
- Manual save: not exposed (auto-save only for Phase 1)

### Save Data Structure
```csharp
[Serializable]
public class SaveData
{
    public int version = 1;
    public float resonanceScore;
    public BuildingState[] buildings;
    public bool[] discoveredPOIs;
    public string[] playedDialogueIds;
    public float playTimeSeconds;
    public string lastSaveTimestamp;
}

[Serializable]
public class BuildingState
{
    public string buildingId;
    public int state; // 0=buried, 1=revealed, 2=tuning, 3=active
    public bool[] nodesComplete;
    public float[] nodeAccuracy;
}
```

---

## 16. GATE 1 Exit Criteria

### Mandatory (All Must Pass)

| # | Criterion | Measurement |
|---|---|---|
| 1 | 15-minute play session complete | Documented playtest video |
| 2 | 60 FPS on iPhone 17 Pro (sustained) | Xcode GPU profiler, 30-min session |
| 3 | 30 FPS on iPhone 15 Pro (sustained) | Same |
| 4 | Thermal ≤ 38°C after 30 min (iPhone 17 Pro) | IR thermometer reading |
| 5 | Aether field visible, flowing, RS-responsive | Qualitative + visual test |
| 6 | 3 buildings restorable with tuning mini-games | Functional test |
| 7 | Mud dissolution shader working | Visual quality assessment |
| 8 | Milo functional (follow, speak, hide) | Behavior tree test matrix |
| 9 | 1 enemy type engageable and defeatable | Combat flow test |
| 10 | Core Haptics working on all interactions | Haptic review on device |
| 11 | Adaptive music responds to RS changes | Audio review |
| 12 | No crashes in 1-hour stress test | Automated + manual test |

### Subjective Gate
After criteria 1–12 pass, the core team plays the 15-minute demo and answers:
> **"Do I want to keep playing?"**
If the answer is not unanimously "yes" — iterate before proceeding to Phase 2.

---

## 17. Week-by-Week Sprint Plan

### Month 1: Engine & Core Systems

| Week | Focus | Deliverables | Exit Test |
|---|---|---|---|
| 1 | Project Setup | Unity 6, URP, DOTS scaffolding, Git, CI (Xcode Cloud) | Clean build on device |
| 2 | Aether System | 3-band compute shader, volumetric render, grid partition | Aether visible at 60 FPS |
| 3 | Resonance Score | RS accumulation, golden-ratio validator, threshold events | RS accrues from test inputs, sky color shifts |
| 4 | Performance | MetalFX integration, thermal state machine, profiling | 60 FPS + ≤35°C for 30 min (blank scene + Aether) |

### Month 2: Building & Exploration

| Week | Focus | Deliverables | Exit Test |
|---|---|---|---|
| 5 | Building System | Placement, state machine, ScriptableObject data | Building transitions through all states |
| 6 | Restoration VFX | Mud dissolution shader, emergence particles, sound | Beautiful restoration on all 3 buildings |
| 7 | Zone: Echohaven | Terrain, blockout architecture, POI placement, lighting | Walk the full zone, discover all POIs |
| 8 | Controls & Camera | Touch input, camera rig, Cinemachine, gesture handling | Responsive input, smooth camera on device |

### Month 3: Gameplay & Integration

| Week | Focus | Deliverables | Exit Test |
|---|---|---|---|
| 9 | Tuning Mini-Game | 3 variants, scoring, RS integration, haptics | All 3 variants playable with feedback |
| 10 | Milo Companion | State machine, 40 voice lines, trigger system | Milo follows, speaks, reacts, hides |
| 11 | Combat Prototype | Mud Golem, 3 player abilities, spawn triggers | Full combat loop functional |
| 12 | Integration Sprint | Full loop polish, audio pass, saves, GATE 1 review | 15-min demo plays start to finish |

---

## 18. Risk Register

| Risk | Probability | Impact | Mitigation |
|---|---|---|---|
| MetalFX upscaling artifacts | Medium | Medium | Fallback to native 1080p if quality insufficient |
| Aether compute exceeds 2ms budget | Low | High | Reduce voxel grid (48³), simplify advection |
| Thermal limit exceeded on A17 Pro | Medium | Medium | Pre-authored thermal profiles per chip |
| Mud dissolution shader banding | Medium | Low | Add dithering + noise variation |
| Milo pathfinding in complex terrain | Medium | Medium | NavMesh + off-mesh links for terrain gaps |
| 432 Hz tuning feels "off" to western ears | Low | Low | A/B test with 440 Hz variant, data-decide |
| Touch combat gestures conflict | Medium | Medium | Combat mode toggle vs exploration mode |
| Save corruption on backgrounding | Medium | High | Double-write + checksum verification |
| Core Haptics latency on older devices | Low | Low | Hardware-check: disable on non-Taptic devices |
| Scope creep from "just one more feature" | High | High | Strict GATE 1 scope freeze after Week 4 |

---

## Appendix A: Asset Budget Summary

| Category | Count | Avg Size | Total |
|---|---|---|---|
| 3D Models (buildings) | 9 (3×3 states) | 2 MB | 18 MB |
| 3D Models (characters) | 2 (player+Milo) | 3 MB | 6 MB |
| 3D Models (enemy) | 1 (golem×3 LOD) | 1.5 MB | 4.5 MB |
| Textures | 30 (PBR sets) | 2 MB avg | 60 MB |
| Audio (music stems) | 6 layers | 5 MB | 30 MB |
| Audio (SFX) | 40 clips | 200 KB | 8 MB |
| Audio (dialogue) | 40 lines | 500 KB | 20 MB |
| Haptic (AHAP files) | 15 patterns | 2 KB | 30 KB |
| Shaders | 5 custom | — | — |
| VFX graphs | 6 | — | — |
| **Estimated Build Size** | | | **~160 MB** |

---

## Appendix B: Key Technical Decisions

| Decision | Choice | Rationale |
|---|---|---|
| ECS vs MonoBehaviour | DOTS ECS for core (Aether, RS, combat), MonoBehaviour for UI/Camera | Performance-critical systems benefit from Burst+Jobs; camera/UI need MonoBehaviour flexibility |
| Addressables vs Resources | Addressables (local bundles) | Future-proofs for Phase 2 remote loading |
| Audio middleware | Native Unity Audio + Core Haptics bridge | No middleware dependency; full haptic control |
| Save format | JSON (local) | Simple, debuggable, human-readable; Phase 3 adds Firebase |
| UI framework | Unity UI Toolkit + SwiftUI overlay | Native iOS feel for menus; Unity for HUD |

---

*This is the foundation. 12 weeks. One zone. One companion. One enemy. One wonder. If this doesn't feel like magic, nothing we add afterward will fix it. Build it right.*

---

**Document Status:** FINAL  
**Author:** Nathan / Resonance Energy  
**Last Updated:** March 25, 2026
