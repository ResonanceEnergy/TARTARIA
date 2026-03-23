# TARTARIA WORLD OF WONDER — Technical Specification
## Unity 6, Metal 3, iOS Optimization & Backend Architecture

---

## Table of Contents

1. [Engine & Platform](#1-engine--platform)
2. [Rendering Pipeline](#2-rendering-pipeline)
3. [Memory & Performance Budgets](#3-memory--performance-budgets)
4. [DOTS/ECS Architecture](#4-dotsecs-architecture)
5. [Asset Pipeline](#5-asset-pipeline)
6. [Audio Architecture](#6-audio-architecture)
7. [Networking & Backend](#7-networking--backend)
8. [iPhone-Specific Optimizations](#8-iphone-specific-optimizations)
9. [Save System](#9-save-system)
10. [Build & Deployment](#10-build--deployment)

---

## 1. Engine & Platform

### 1.1 Core Stack

| Component | Technology |
|---|---|
| **Engine** | Unity 6 LTS (2026.x) |
| **Scripting Backend** | IL2CPP (mandatory for iOS) |
| **Architecture** | DOTS/ECS (Entities 1.x) |
| **Render Pipeline** | URP (Universal Render Pipeline) |
| **Compiler** | Burst Compiler 1.8+ |
| **Parallelism** | C# Job System |
| **Asset Delivery** | Addressables + Asset Bundles |
| **Physics** | Unity Physics (DOTS-compatible) |
| **Animation** | Unity Animation Rigging + DOTS animation |
| **UI** | UI Toolkit (runtime) |

### 1.2 Platform Requirements

| Spec | Requirement |
|---|---|
| **Minimum iOS** | 18.0 |
| **Minimum Device** | iPhone 15 Pro (A17 Pro) |
| **Target Device** | iPhone 17 Pro / Pro Max (A19) |
| **Future-Proofing** | iPhone 18 (A20) optimizations ready |
| **iPad Support** | Universal binary, iPad Air M2+ |
| **Graphics API** | Metal 3 (exclusive — no OpenGL fallback) |
| **App Store Category** | Games > Adventure |
| **Rating** | 9+ (no violence, no real gambling) |

### 1.3 Supported Device Matrix

| Device | Chip | Metal | Target FPS | Resolution |
|---|---|---|---|---|
| iPhone 15 Pro | A17 Pro | Metal 3 | 30 (60 optional) | 720p upscaled |
| iPhone 16 Pro | A18 Pro | Metal 3 | 60 | 900p → 1080p |
| iPhone 17 Pro | A19 | Metal 3+ | 60 locked | 1080p native |
| iPhone 17 Pro Max | A19 | Metal 3+ | 60 locked | 1080p native |
| iPhone 18 (target) | A20 | Metal 3+ | 60 locked | 1200p → 1440p |
| iPad Air M2 | M2 | Metal 3 | 60 | 1080p |
| iPad Pro M4 | M4 | Metal 3 | 60 locked | 1200p |

---

## 2. Rendering Pipeline

### 2.1 URP Configuration

```
URP Asset Settings:
├── HDR: On
├── MSAA: 2x (mobile)
├── Render Scale: Dynamic (0.65–1.0)
├── Shadow Distance: 80m
├── Shadow Cascades: 2
├── Shadow Resolution: 1024
├── Depth Texture: On
├── Opaque Texture: On
├── Post-Processing: On
│   ├── Bloom (threshold 0.9, intensity 0.3)
│   ├── Color Grading (ACES)
│   ├── Vignette (subtle)
│   └── Depth of Field (bokeh, story moments only)
├── SRP Batcher: On
└── Dynamic Batching: Off (unnecessary with SRP Batcher)
```

### 2.2 MetalFX Temporal Upscaling

**Core rendering strategy — render at lower resolution, upscale with hardware AI:**

| Quality Level | Render Resolution | Output | GPU Savings |
|---|---|---|---|
| Performance | 540p | 1080p | ~60% |
| Balanced | 720p | 1080p | ~45% |
| Quality | 900p | 1080p | ~25% |
| Max (A19+) | 1080p | 1080p | 0% (native) |

**Implementation:**
- Custom URP Renderer Feature wrapping MetalFX Temporal Scaler
- Motion vectors from URP pass (required for temporal stability)
- Reactive mask for UI elements (prevents ghosting on HUD)
- Auto-switch based on thermal state (see §8.4)

### 2.3 Lighting

| Technique | Usage |
|---|---|
| **Baked Lightmaps** | Primary illumination for all restored & static architecture |
| **Mixed Lights** | Sun/moon cycle — baked indirect, realtime direct shadows |
| **Light Probes** | Dynamic objects (player, NPCs, particles) |
| **Reflection Probes** | Domes, water surfaces, crystal structures |
| **Screen Space Ambient Occlusion** | Subtle depth on restored buildings |
| **Volumetric Fog** | Corruption zones, dawn/dusk atmospheric |
| **Aether Glow** | Custom emissive shader — pulses with RS |

**Day/Night Cycle:**
- 17-hour Tartarian day mapped to real-time cycle
- Sun angle drives lightmap blending (day ↔ night)
- 8 ambient states: dawn → morning → midday → afternoon → dusk → twilight → night → deep night
- Moon phase affects Aether yield + lighting color temperature

### 2.4 Custom Shaders

| Shader | Purpose | Technique |
|---|---|---|
| **Aether Flow** | Ley line visualization | Vertex displacement + flow map + emission pulse |
| **Mud Corruption** | Covered buildings pre-restoration | Triplanar mud texture + dissolve parameter |
| **Restoration Reveal** | Cleaning animation | World-space dissolve driven by player proximity |
| **Crystal Resonance** | Tuning fork / crystal objects | Subsurface scattering approx + emission flicker |
| **Sacred Geometry** | Building placement guides | Procedural golden spiral + wireframe overlay |
| **Giant Mode** | Scale-up visual effect | Rim glow + ground crack decal + camera shake |
| **Water (Pure Fountain)** | Tartarian fountain water | Caustics + refraction + flow + emission (pure = aether glow) |
| **Organ Resonance** | Sound visualization | Audio-reactive vertex displacement + particle trigger |

---

## 3. Memory & Performance Budgets

### 3.1 Memory Budget

| Category | Budget | Notes |
|---|---|---|
| **Total Peak RAM** | ≤2.8 GB | iOS terminates at ~3.5 GB; keep 700 MB headroom |
| **Texture Memory** | 1.5 GB | ASTC compressed, mip-streamed |
| **Mesh Memory** | 400 MB | LOD'd, Addressable loaded |
| **Audio Memory** | 200 MB | Streaming for music, loaded for SFX |
| **Script/ECS Memory** | 300 MB | Entity archetype pools |
| **UI Memory** | 100 MB | UI Toolkit + atlas textures |
| **System Overhead** | 300 MB | Unity runtime + IL2CPP |

### 3.2 Texture Specifications

| Type | Max Size | Format | Mip Chain |
|---|---|---|---|
| **Character Albedo** | 1024×1024 | ASTC 6×6 | Full |
| **Building Albedo** | 2048×2048 | ASTC 8×8 | Full |
| **Terrain** | 2048×2048 | ASTC 8×8 | Full |
| **Lightmaps** | 1024×1024 | ASTC 4×4 (high quality) | No |
| **UI Atlas** | 2048×2048 | ASTC 4×4 | No |
| **Skybox** | 2048×2048 | ASTC 6×6 | Yes |
| **Normal Maps** | 1024×1024 | ASTC 6×6 | Full |
| **Effects/VFX** | 512×512 | ASTC 6×6 | Full |

### 3.3 Draw Call Budget

| State | Max Draw Calls | Max Triangles |
|---|---|---|
| **Exploration (open world)** | 300 | 1.2M |
| **Building Mode** | 250 | 800K |
| **Combat (standard)** | 350 | 1.5M |
| **Combat (giant mode)** | 200 | 2.0M |
| **Cutscenes** | 200 | 1.0M |
| **Menus/UI** | 50 | 100K |

### 3.4 Frame Budget (16.67ms for 60 FPS)

| Phase | Budget | Notes |
|---|---|---|
| **CPU: ECS Systems** | 4ms | Burst-compiled, job-threaded |
| **CPU: Physics** | 2ms | Unity Physics, spatial partitioning |
| **CPU: Animation** | 1.5ms | Blend tree + IK, DOTS animation |
| **CPU: Audio** | 0.5ms | DSP mix + spatial audio |
| **CPU: UI** | 1ms | UI Toolkit retained mode |
| **CPU → GPU Submission** | 1ms | SRP Batcher |
| **GPU: Render** | 6ms | URP + custom shaders |
| **GPU: Post-Process** | 1ms | Bloom + color grading |
| **GPU: MetalFX Upscale** | 0.5ms | Hardware-accelerated |
| **Headroom** | 1.17ms | Thermal throttle buffer |

---

## 4. DOTS/ECS Architecture

### 4.1 World Structure

```
ECS Worlds
├── Default World (gameplay)
│   ├── Simulation Group
│   │   ├── AetherFieldSystem (aether flow calculation)
│   │   ├── ResonanceScoreSystem (RS computation)
│   │   ├── BuildingStateSystem (restoration state machine)
│   │   ├── NPCBehaviorSystem (companion AI)
│   │   ├── CombatSystem (frequency matching, damage)
│   │   ├── GiantModeSystem (scale transitions)
│   │   ├── DayNightSystem (17-hour cycle)
│   │   ├── MoonPhaseSystem (13-moon calendar)
│   │   └── WeatherSystem (atmospheric effects)
│   ├── Presentation Group
│   │   ├── AnimationSystem
│   │   ├── AudioSyncSystem
│   │   ├── ParticleSpawnSystem
│   │   └── CameraSystem
│   └── LateSimulation Group
│       ├── SaveStateSystem
│       ├── AnalyticsSystem
│       └── NetworkSyncSystem
├── Streaming World (zone loading)
│   ├── ZoneLoadingSystem
│   ├── LODTransitionSystem
│   └── AddressableAssetSystem
└── UI World (decoupled)
    ├── HUDUpdateSystem
    ├── MenuSystem
    └── NotificationSystem
```

### 4.2 Key Component Archetypes

```csharp
// Core Aether entity
struct AetherNode : IComponentData {
    float3 WorldPosition;
    float Intensity;         // 0–1
    HarmonicBand Band;       // Physical, Etheric, Celestial
    float Frequency;         // Hz
    float Coherence;         // purity factor
}

// Building entity
struct TartarianBuilding : IComponentData {
    BuildingArchetype Type;  // Dome, Spire, StarFort, etc.
    float RestorationProgress; // 0–1
    float ResonanceScore;    // 0–100
    float GoldenRatioMatch;  // 0–1 (proportion accuracy)
    int UpgradeTier;         // 0–5
}

// Combat entity
struct HarmonicCombatant : IComponentData {
    float Health;
    float AetherCharge;
    float CurrentFrequency;
    int ComboCount;          // 0–12 (Golden Cascade max)
    bool IsGiantMode;
}
```

### 4.3 Spatial Partitioning

- **Aether field:** 3D grid, 2m cells, BlobAsset for static nodes, dynamic buffer for player-affected
- **Zone streaming:** 500m radius around player, 3 concentric rings (active / LOD / unload)
- **Combat:** Spatial hash for enemy proximity, 50m engagement radius
- **Building placement:** Quadtree for snap points, golden ratio validation per frame

---

## 5. Asset Pipeline

### 5.1 Addressables Groups

| Group | Load Strategy | Unload |
|---|---|---|
| **Core** | Pre-loaded at launch | Never |
| **Zone_{N}** | Loaded on zone approach (500m) | On zone exit (750m) |
| **Moon_{N}_Story** | Loaded on Moon start | On Moon complete |
| **Cosmetics_Owned** | Loaded on login | On logout |
| **Cosmetics_Preview** | Loaded on shop open | On shop close |
| **Audio_Ambient** | Loaded per zone | On zone exit |
| **Audio_Music** | Streamed | Auto-managed |
| **VFX_Common** | Pre-loaded at launch | Never |
| **VFX_Boss** | Loaded on boss encounter | On encounter end |

### 5.2 Download Strategy

| Phase | Content | Size |
|---|---|---|
| **Initial Download** | Core + Zone 1–3 + Prologue | ~1.2 GB |
| **Background Download** | Zone 4–6 + Moon 1–3 story | ~800 MB |
| **On-Demand** | Zone 7+ (per-zone, prompted) | ~200 MB each |
| **Cosmetic Packs** | Downloaded on purchase | ~5–50 MB each |
| **Total (all content)** | | ~4.5 GB |

### 5.3 LOD Pipeline

| LOD | Distance | Tri Budget | Texture |
|---|---|---|---|
| **LOD 0** (Hero) | 0–30m | Full mesh | Full res |
| **LOD 1** | 30–80m | 50% | Half res |
| **LOD 2** | 80–200m | 15% | Quarter res |
| **LOD 3** (Impostor) | 200m+ | Billboard | 256×256 card |

**Domes & cathedrals** get extra LOD budget at LOD 0 (these are architectural marvels — they need to look stunning up close).

---

## 6. Audio Architecture

### 6.1 Audio Stack

| Component | Technology |
|---|---|
| **Engine** | Unity Audio (FMOD optional for advanced spatials) |
| **Spatial Audio** | Apple Spatial Audio (AirPods Pro 2+) |
| **Music** | Adaptive layers — 4 stems per zone |
| **SFX** | Object-pooled AudioSource per archetype |
| **Voice** | Streamed, compressed Vorbis 128kbps |
| **Resonance Audio** | Custom DSP for tuning mini-games |

### 6.2 Adaptive Music System

```
Music Layer Architecture (per zone):
├── Layer 1: Ambient Pad (always playing)
├── Layer 2: Melodic Theme (exploration)
├── Layer 3: Rhythmic Drive (combat / building / events)
├── Layer 4: Harmonic Climax (boss / story peaks / giant mode)
│
├── Transition Rules:
│   ├── RS < 30% → Layers 1 only (desolate)
│   ├── RS 30–60% → Layers 1+2 (hopeful)
│   ├── RS 60–90% → Layers 1+2+3 (vibrant)
│   └── RS > 90% → All layers (triumphant)
│
└── Special States:
    ├── Giant Mode → Shift to bass-heavy, slow tempo
    ├── Organ Playing → Player instrument replaces Layer 2
    ├── Bell Tower → Zone ambient shift to reverb cathedral
    └── Corruption → Dissonant filter on all layers
```

### 6.3 Audio Memory Budget

| Type | Budget | Format |
|---|---|---|
| **Music (streaming)** | 30 MB buffer | AAC 256kbps |
| **SFX (loaded)** | 80 MB | Vorbis 96kbps, mono |
| **Ambient Loops** | 40 MB | Vorbis 128kbps, stereo |
| **Voice Lines** | 50 MB (per zone) | Vorbis 128kbps |
| **Resonance/Tuning** | 20 MB | Uncompressed PCM (precision) |

---

## 7. Networking & Backend

### 7.1 Architecture

```
Client (iOS)
    │
    ├── Firebase Auth ──────── Sign in with Apple + Game Center
    ├── Firestore ──────────── Player profile, save state, cosmetics
    ├── Firebase Functions ──── Server-authoritative validation
    │   ├── Aether harvest validation
    │   ├── IAP receipt verification
    │   └── Event scoring
    ├── Firebase Remote Config ── A/B testing, feature flags, tuning
    ├── Firebase Analytics ───── KPI tracking, funnel analysis
    ├── Firebase Crashlytics ─── Crash reporting + ANR detection
    ├── Cloud Storage ────────── User-generated content (builds)
    │
    └── PlayFab (secondary)
        ├── LiveOps ─────────── Event scheduling, leaderboards
        ├── Economy ─────────── Virtual currency, catalog, stores
        └── Multiplayer ─────── Community builds, World's Fair voting
```

### 7.2 Server-Authoritative Systems

Critical systems that MUST be server-validated:

| System | Why | Validation |
|---|---|---|
| **Aether Harvest** | Prevent infinite aether exploits | Server calculates max yield per session based on zone + RS + time |
| **IAP Purchases** | Receipt fraud prevention | Firebase Function verifies Apple receipt before granting items |
| **Battle Pass Progress** | Prevent tier manipulation | Server tracks XP events, client displays |
| **Leaderboards** | Score integrity | Server validates RS calculations before posting |
| **Cosmetic Ownership** | Duplication prevention | Server of record for owned items |
| **Event Rewards** | Exploit prevention | Server validates event completion before granting |

### 7.3 Offline-First Design

| Feature | Offline | Online |
|---|---|---|
| **Campaign** | Full access | Cloud save sync |
| **Building** | Full access | RS leaderboard update |
| **Combat** | Full access | Event scoring |
| **Cosmetics** | Use owned items | Purchase new items |
| **Events** | Cached event data (24h) | Live updates |
| **Battle Pass** | Progress tracked locally | Sync on reconnect |
| **Ads** | Unavailable (no penalty) | Opt-in rewards |

**Sync strategy:** Last-write-wins with conflict resolution via timestamp + server arbitration. Player always keeps the more progressed state.

---

## 8. iPhone-Specific Optimizations

### 8.1 A-Series Chip Optimizations

| Feature | A17 Pro | A18 Pro | A19 | A20 |
|---|---|---|---|---|
| **Ray Tracing** | Software fallback | Hardware RT (limited) | Full hardware RT | Full + RT GI |
| **MetalFX** | Temporal only | Temporal + Spatial | Full suite | Full + AI upscale |
| **Neural Engine** | NPC pathfinding | + ambient occlusion | + foliage animation | + real-time GI assist |
| **Mesh Shading** | N/A | Basic | Full | Full + amplification |
| **Thread Count** | 6 (2P+4E) | 6 (2P+4E) | 6+ (predicted) | 6+ (predicted) |

### 8.2 Thermal Management

```
Thermal State Machine:
├── NOMINAL (< 35°C)
│   └── Full quality, 60 FPS target
├── FAIR (35–40°C)
│   ├── Drop MetalFX to Balanced
│   ├── Reduce shadow distance → 50m
│   └── Reduce particle count → 75%
├── SERIOUS (40–45°C)
│   ├── Drop MetalFX to Performance
│   ├── Drop to 30 FPS target
│   ├── Reduce shadow → 30m, 1 cascade
│   ├── Reduce particle count → 50%
│   └── Disable volumetric fog
└── CRITICAL (> 45°C)
    ├── 30 FPS hard lock
    ├── Minimal post-processing
    ├── LOD bias +2 (aggressive)
    ├── Disable all optional VFX
    └── Toast: "Your device is warm — we've adjusted quality to keep things comfortable"
```

**Implementation:** Poll `ProcessInfo.thermalState` every 5 seconds. Transition between states with 1-second lerp (no jarring pop).

### 8.3 Battery-Aware Mode

| Battery Level | Action |
|---|---|
| **> 50%** | Full performance |
| **20–50%** | Suggest "Battery Saver" mode (player choice) |
| **< 20%** | Auto-enable Battery Saver + prompt save |
| **< 10%** | Auto-save + reduce to minimum quality |

**Battery Saver Mode:**
- 30 FPS cap
- MetalFX Performance mode
- Reduced draw distance
- Simplified particle effects
- Darker screen brightness suggestion

### 8.4 ProMotion (120Hz Display)

- **Menus/UI:** 120 Hz (smooth scrolling)
- **Gameplay:** 60 Hz (performance target)
- **Cutscenes:** 30 Hz (cinematic, saves battery)
- **Idle:** 24 Hz (when paused/background)

Managed via `CADisplayLink.preferredFrameRateRange`.

### 8.5 Haptic Feedback (Taptic Engine)

| Event | Haptic Pattern |
|---|---|
| **Aether collection** | Light tap (UIImpactFeedbackGenerator.light) |
| **Building snap** | Medium tap |
| **Resonance match** | Three ascending taps |
| **Giant mode activate** | Heavy impact + rumble |
| **Mud clearing** | Continuous soft texture |
| **Bell tower chime** | Sharp tap per chime |
| **Currency purchase** | Satisfying double tap |
| **Combo hit** | Escalating taps per combo |
| **Critical discovery** | Pattern: tap-pause-tap-tap |

### 8.6 Dynamic Island & Live Activities

| Feature | Usage |
|---|---|
| **Live Activity** | Aether harvest timer (background processing completion) |
| **Dynamic Island (compact)** | Current RS + aether level while multitasking |
| **Dynamic Island (expanded)** | Zone name + active quest + session timer |
| **Lock Screen Widget** | Daily aether status + next event countdown |

### 8.7 iCloud Integration

| Data | Storage | Sync |
|---|---|---|
| **Save files** | iCloud Key-Value Store | Automatic |
| **Screenshots** | Local only | N/A |
| **Settings** | NSUbiquitousKeyValueStore | Automatic |
| **Large saves** | CloudKit + iCloud Documents | On-demand |

**Conflict resolution:** Server (Firebase) is source of truth. iCloud is secondary backup for offline-first saves.

### 8.8 Metal 3 Specific Features

| Feature | Implementation |
|---|---|
| **Mesh Shaders** | Procedural foliage, crowd NPCs in restored cities |
| **Offline Compilation** | Pre-compiled Metal shaders in app bundle |
| **Sparse Textures** | Terrain virtual texturing |
| **Lossless Compression** | Lightmap storage on device |
| **Indirect Command Buffers** | GPU-driven rendering for architecture LODs |

---

## 9. Save System

### 9.1 Save Data Structure

```
Save File (~500 KB compressed)
├── Header
│   ├── Version (int)
│   ├── PlayTime (float, hours)
│   ├── CurrentMoon (int, 1–13)
│   ├── CurrentZone (string)
│   └── SaveTimestamp (UTC)
├── Player
│   ├── Position (float3)
│   ├── Level (int)
│   ├── SkillTree (bitfield, 80 nodes)
│   ├── EquippedCosmetics (int[8])
│   ├── Inventory (item_id, count)[]
│   ├── CompanionStates (id, affinity, arc_stage)[]
│   └── QuestFlags (dictionary<string, int>)
├── World
│   ├── Buildings[] (id, restoration%, RS, upgrades)
│   ├── AetherNodes[] (id, intensity, last_harvest)
│   ├── DiscoveredLore[] (bitfield)
│   ├── ZoneStates[] (id, corruption%, unlocked)
│   └── NPC_Dialogues[] (id, last_branch)
├── Economy
│   ├── AetherShards (int)
│   ├── Materials{} (type → count)
│   └── OwnedCosmetics[] (int)
├── Campaign
│   ├── MoonProgress[] (moon, phase, flags)
│   ├── StoryChoices[] (choice_id, option)
│   └── EndingSeeds[] (harmony, echo, reset weights)
└── Meta
    ├── Settings (audio, graphics, controls)
    ├── TotalAetherHarvested (float)
    ├── BuildingsRestored (int)
    └── PlaySessionCount (int)
```

### 9.2 Auto-Save Policy

| Trigger | Save Type |
|---|---|
| Zone transition | Full save |
| Quest completion | Full save |
| Building placement | Incremental (world only) |
| Every 5 minutes of play | Incremental |
| App backgrounding | Emergency full save |
| Before boss encounter | Checkpoint |
| Manual (player-initiated) | Full save |

### 9.3 Save Slots

- **3 manual save slots** + 1 auto-save slot
- Cloud sync on each save (if online)
- Save file versioning for forward compatibility (migration functions per version)

---

## 10. Build & Deployment

### 10.1 Build Configuration

| Setting | Value |
|---|---|
| **Scripting Backend** | IL2CPP |
| **API Compatibility** | .NET Standard 2.1 |
| **Managed Code Stripping** | High |
| **Architecture** | ARM64 only |
| **Minimum iOS** | 18.0 |
| **Bitcode** | Disabled (deprecated by Apple) |
| **App Thinning** | Enabled (asset slicing per device) |
| **On-Demand Resources** | ODR for zone packs beyond initial download |

### 10.2 CI/CD Pipeline

```
Git Push → GitHub Actions
├── Build (Unity Build Server, macOS runner)
│   ├── Run unit tests (EditMode + PlayMode)
│   ├── IL2CPP compile (ARM64)
│   ├── Addressables build (per group)
│   └── Generate IPA
├── Test
│   ├── XCTest (native iOS tests)
│   ├── Performance regression (frame budget checks)
│   └── Memory regression (peak RAM checks)
├── Deploy
│   ├── TestFlight (internal) → auto
│   ├── TestFlight (external) → manual approval
│   └── App Store Connect → manual release
└── Post-Deploy
    ├── Upload dSYM to Crashlytics
    ├── Upload Addressables to CDN
    └── Tag release in Git
```

### 10.3 Performance Testing Gates

No build passes CI without meeting these thresholds:

| Metric | Pass | Fail |
|---|---|---|
| **Average FPS** | ≥55 | <50 |
| **1% Low FPS** | ≥30 | <25 |
| **Peak RAM** | ≤2.6 GB | >2.8 GB |
| **Load Time (zone)** | ≤4s | >6s |
| **Load Time (app launch)** | ≤6s | >10s |
| **IPA Size (thin)** | ≤1.5 GB | >2.0 GB |
| **Battery Drain (30 min session)** | ≤15% | >20% |

### 10.4 App Store Optimization

| Field | Value |
|---|---|
| **Title** | Tartaria: World of Wonder |
| **Subtitle** | Tune the World. Reclaim the Golden Age. |
| **Keywords** | tartaria, restoration, city builder, RPG, open world, adventure, puzzle, aether |
| **Category** | Games > Adventure (primary), Games > Puzzle (secondary) |
| **Screenshots** | 6.7" (iPhone Pro Max) + 6.1" (iPhone Pro) + 12.9" (iPad) |
| **Preview Video** | 30s gameplay trailer — dome restoration → combat → giant mode |

---

**Document Status:** DRAFT  
**Cross-References:** `00_MASTER_GDD.md`, `07_MOBILE_UX.md`, `06_COMBAT_PROGRESSION.md`  
**Last Updated:** March 23, 2026
