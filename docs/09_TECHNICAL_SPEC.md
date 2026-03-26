# TARTARIA WORLD OF WONDER — Technical Specification
## Unity 6, DirectX 12 / Vulkan, PC Optimization & Backend Architecture

---

## Table of Contents

1. [Engine & Platform](#1-engine-platform)
2. [Rendering Pipeline](#2-rendering-pipeline)
3. [Memory & Performance Budgets](#3-memory-performance-budgets)
4. [DOTS/ECS Architecture](#4-dotsecs-architecture)
5. [Asset Pipeline](#5-asset-pipeline)
6. [Audio Architecture](#6-audio-architecture)
7. [Networking & Backend](#7-networking-backend)
8. [PC-Specific Optimizations](#8-pc-specific-optimizations)
9. [Save System](#9-save-system)
10. [Build & Deployment](#10-build-deployment)

---

## 1. Engine & Platform

### 1.1 Core Stack

| Component | Technology |
|---|---|
| **Engine** | Unity 6 LTS (2026.x) |
| **Scripting Backend** | IL2CPP (for performance) |
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
| **Minimum OS** | Windows 10 (64-bit, version 1909+) |
| **Recommended OS** | Windows 11 |
| **Minimum GPU** | NVIDIA GTX 1070 / AMD RX 580 (4 GB VRAM) |
| **Recommended GPU** | NVIDIA RTX 3060 / AMD RX 6700 XT (8 GB VRAM) |
| **Minimum CPU** | Intel i5-8400 / AMD Ryzen 5 2600 |
| **Recommended CPU** | Intel i5-12400 / AMD Ryzen 5 5600X |
| **Minimum RAM** | 8 GB |
| **Recommended RAM** | 16 GB |
| **Graphics API** | DirectX 12 (primary), Vulkan (fallback) |
| **Distribution** | Steam |
| **Rating** | ESRB E10+ / PEGI 7 |
| **Future Port** | iOS 18.0+ (post-PC launch) |

### 1.3 Supported Hardware Tiers

| Tier | GPU Example | VRAM | Target FPS | Resolution |
|---|---|---|---|---|
| **Minimum** | GTX 1070 / RX 580 | 4 GB | 30 (60 optional) | 1080p (FSR Performance) |
| **Recommended** | RTX 3060 / RX 6700 XT | 8 GB | 60 locked | 1080p–1440p |
| **High** | RTX 3070 / RX 6800 XT | 8–12 GB | 60 locked | 1440p–4K (FSR/DLSS Quality) |
| **Ultra** | RTX 4070+ / RX 7900+ | 12+ GB | 60 locked | 4K native |
| **Steam Deck** | RDNA 2 (custom) | 1 GB (shared) | 30–40 | 800p (FSR Performance) |

---

## 2. Rendering Pipeline

### 2.1 URP Configuration

```
URP Asset Settings:
├── HDR: On
├── MSAA: 4x (2x on Minimum tier)
├── Render Scale: Dynamic (0.5–1.0, per quality preset)
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

### 2.2 FSR 2 / DLSS Temporal Upscaling

**Core rendering strategy — render at lower resolution, upscale with temporal reconstruction:**

| Quality Level | Render Resolution | Output (1080p) | Output (1440p) | Output (4K) |
|---|---|---|---|---|
| Performance | 540p | 1080p | 720p → 1440p | 1080p → 4K |
| Balanced | 720p | 1080p | 960p → 1440p | 1440p → 4K |
| Quality | 900p | 1080p | 1200p → 1440p | 2160p → 4K |
| Native | — | 1080p | 1440p | 4K |

**Implementation:**
- AMD FSR 2.2 (all GPUs, primary — works on NVIDIA, AMD, Intel)
- NVIDIA DLSS 3.5 (RTX GPUs, optional — better quality when available)
- Intel XeSS (Arc GPUs, optional)
- Motion vectors from URP pass (required for temporal stability)
- Reactive mask for UI elements (prevents ghosting on HUD)
- Auto-select based on detected GPU vendor and driver version

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
| **Total Peak RAM** | ≤4 GB | PC has more headroom; keep conservative for Minimum tier |
| **Texture Memory** | 2 GB | BC7 compressed, mip-streamed |
| **Mesh Memory** | 600 MB | LOD'd, Addressable loaded |
| **Audio Memory** | 200 MB | Streaming for music, loaded for SFX |
| **Script/ECS Memory** | 400 MB | Entity archetype pools |
| **UI Memory** | 200 MB | UI Toolkit + atlas textures |
| **System Overhead** | 600 MB | Unity runtime + IL2CPP |

### 3.2 Texture Specifications

| Type | Max Size | Format | Mip Chain |
|---|---|---|---|
| **Character Albedo** | 2048×2048 | BC7 | Full |
| **Building Albedo** | 4096×4096 | BC7 | Full |
| **Terrain** | 4096×4096 | BC7 | Full |
| **Lightmaps** | 2048×2048 | BC6H (HDR) | No |
| **UI Atlas** | 4096×4096 | BC7 | No |
| **Skybox** | 4096×4096 | BC6H | Yes |
| **Normal Maps** | 2048×2048 | BC5 | Full |
| **Effects/VFX** | 1024×1024 | BC7 | Full |

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
| **GPU: FSR/DLSS Upscale** | 0.5ms | Temporal upscaling |
| **Headroom** | 1.17ms | Driver overhead buffer |

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
| **Initial Download** | Core + Zone 1–3 + Prologue | ~2 GB |
| **Background Download** | Zone 4–6 + Moon 1–3 story | ~1.5 GB |
| **On-Demand** | Zone 7+ (per-zone, prompted) | ~400 MB each |
| **Cosmetic Packs** | Downloaded on purchase | ~10–100 MB each |
| **Total (all content)** | | ~8 GB |

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
| **Spatial Audio** | Windows Sonic / Dolby Atmos (headphone) |
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
Client (PC)
    │
    ├── Firebase Auth ────────── Steam Authentication
    ├── Firestore ────────────── Player profile, save state, cosmetics
    ├── Firebase Functions ──── Server-authoritative validation
    │   ├── Aether harvest validation
    │   ├── IAP receipt verification
    │   └── Event scoring
    ├── Firebase Remote Config ── A/B testing, feature flags, tuning
    ├── Firebase Analytics ───── KPI tracking, funnel analysis
    ├── Firebase Crashlytics ─── Crash reporting + hang detection
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
| **IAP Purchases** | Receipt fraud prevention | Firebase Function verifies Steam transaction before granting items |
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

## 8. PC-Specific Optimizations

### 8.1 GPU Tier Optimizations

| Feature | Minimum (GTX 1070) | Recommended (RTX 3060) | High (RTX 3070) | Ultra (RTX 4070+) |
|---|---|---|---|---|
| **Ray Tracing** | Software fallback | RT reflections (limited) | Full RT reflections + AO | Full RT + RT GI |
| **Upscaling** | FSR 2 Performance | FSR 2 Balanced / DLSS Quality | FSR 2 Quality / DLSS Quality | Native or DLSS Ultra Quality |
| **Shadow Quality** | 1 cascade, 30m | 2 cascades, 80m | 3 cascades, 120m | 4 cascades, 200m |
| **Volumetric Fog** | Disabled | Half-res | Full-res | Full-res + temporal |
| **Particle Count** | 50% | 100% | 150% | 200% |
| **Draw Distance** | 200m | 400m | 600m | 1000m |

### 8.2 GPU Power Management

```
Quality Auto-Adjust System:
├── 60+ FPS sustained
│   └── Maintain current settings, attempt quality increase after 30s
├── 50–59 FPS detected
│   ├── Reduce shadow distance by one tier
│   └── Reduce particle count by 25%
├── 40–49 FPS detected
│   ├── Drop FSR/DLSS to next lower quality
│   ├── Reduce shadow cascades
│   └── Disable volumetric fog
└── <40 FPS detected
    ├── Drop to Performance upscaling
    ├── Reduce draw distance
    ├── Simplify post-processing
    └── Suggest lower quality preset in Settings
```

**Implementation:** Monitor frame time average over 5-second window. Adjust with 1-second lerp (no jarring pop). Player can disable auto-adjust.

### 8.3 Quality Presets

| Preset | Resolution Target | Effects | VRAM Budget |
|---|---|---|---|
| **Low** | 1080p (FSR Perf) | Baked shadows, no volumetrics, basic particles | 2 GB |
| **Medium** | 1080p–1440p | Dynamic shadows, basic volumetrics | 4 GB |
| **High** | 1440p–4K (FSR Quality) | RT reflections, full volumetrics, high particles | 6 GB |
| **Ultra** | 4K native or DLSS | Full RT, maximum everything | 8+ GB |
| **Custom** | User-defined | Individual setting control | Variable |

### 8.4 Variable Refresh Rate

- **G-Sync / FreeSync / VRR** support — auto-enabled when detected
- **Frame rate cap:** Configurable (30 / 60 / 120 / 144 / Unlimited / Monitor refresh)
- **V-Sync:** On / Off / Adaptive (reduce tearing without forced frame lock)
- **Menus/UI:** Unlocked framerate
- **Gameplay:** Player-configured cap (default 60)
- **Cutscenes:** Optional 30 FPS cinematic mode
- **Background:** 15 FPS when minimized (configurable, or pause)

### 8.5 Haptic Feedback (Gamepad)

| Event | XInput (Xbox) | DualSense (PlayStation) |
|---|---|---|
| **Aether collection** | Light rumble, 100ms | Light haptic + trigger resistance ease |
| **Building snap** | Medium rumble, 150ms | Medium haptic |
| **Resonance match** | Three ascending pulses | Three ascending haptics + adaptive trigger click |
| **Giant mode activate** | Heavy rumble, 500ms | Heavy haptic + both triggers full resistance |
| **Mud clearing** | Continuous low rumble | Continuous texture feedback via adaptive triggers |
| **Bell tower chime** | Sharp pulse per chime | Sharp haptic per chime |
| **Currency purchase** | Double pulse | Double haptic |
| **Combo hit** | Escalating rumble per combo | Escalating haptic + increasing trigger resistance |
| **Critical discovery** | Pulse-pause-pulse-pulse | Pulse-pause-pulse-pulse + trigger snap |

### 8.6 Steam Integration

| Feature | Usage |
|---|---|
| **Steam Rich Presence** | Current zone + RS% + active Moon (shown on friends list) |
| **Steam Overlay** | In-game browser, friend invites, screenshots |
| **Steam Cloud** | Save file sync (automatic, bidirectional) |
| **Steam Input** | Universal controller support (Xbox, PS, Switch Pro, Steam Deck) |
| **Steam Workshop** | Future: community-created building blueprints |
| **Steam Trading Cards** | 13 cards (one per Moon) — craftable badge |

### 8.7 Display & Resolution

| Feature | Details |
|---|---|
| **Resolution** | All standard resolutions from 720p to 8K, custom supported |
| **Window Modes** | Fullscreen, Borderless Windowed, Windowed |
| **Ultrawide** | 21:9 and 32:9 natively supported (extended FOV, not cropped) |
| **Multi-Monitor** | Primary monitor only (UI follows primary) |
| **HDR** | HDR10 supported on compatible displays |
| **UI Scaling** | Independent UI scale (50%–200%) for 4K / ultrawide |

### 8.8 DirectX 12 / Vulkan Features

| Feature | Implementation |
|---|---|
| **Mesh Shaders** | Procedural foliage, crowd NPCs in restored cities (DX12 Ultimate) |
| **Shader Pre-compilation** | Pre-compiled shader cache per GPU on first launch |
| **Virtual Texturing** | Terrain streaming textures |
| **DirectStorage** | Fast asset loading from NVMe SSD (when available) |
| **GPU-Driven Rendering** | Indirect draw calls for architecture LODs |
| **Async Compute** | Parallel GPU work for post-processing and upscaling |

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
| Alt-tab / minimize | Emergency full save |
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
| **Architecture** | x86_64 |
| **Minimum OS** | Windows 10 (1909+) |
| **Graphics APIs** | DirectX 12 (default), Vulkan (fallback) |
| **Installer** | Steamworks SDK, standalone .exe with installer |
| **Anti-Cheat** | Steam Anti-Cheat (optional, for leaderboards) |

### 10.2 CI/CD Pipeline

```
Git Push → GitHub Actions
├── Build (Unity Build Server, Windows runner)
│   ├── Run unit tests (EditMode + PlayMode)
│   ├── IL2CPP compile (x86_64)
│   ├── Addressables build (per group)
│   └── Generate Windows build (.exe + data)
├── Test
│   ├── Unity Test Runner (automated)
│   ├── Performance regression (frame budget checks)
│   └── Memory regression (peak RAM checks)
├── Deploy
│   ├── Steam beta branch (internal) → auto
│   ├── Steam beta branch (external / Early Access) → manual approval
│   └── Steam full release → manual release
└── Post-Deploy
    ├── Upload PDB symbols to Crashlytics
    ├── Upload Addressables to CDN
    ├── Update Steam depot
    └── Tag release in Git
```

### 10.3 Performance Testing Gates

No build passes CI without meeting these thresholds:

| Metric | Pass | Fail |
|---|---|---|
| **Average FPS (Recommended tier)** | ≥55 | <50 |
| **1% Low FPS** | ≥30 | <25 |
| **Peak RAM** | ≤3.5 GB | >4.0 GB |
| **Load Time (zone)** | ≤3s | >5s |
| **Load Time (app launch)** | ≤5s | >8s |
| **Install Size (base)** | ≤3 GB | >4 GB |
| **Shader Compile (first launch)** | ≤60s | >120s |

### 10.4 Steam Store Optimization

| Field | Value |
|---|---|
| **Title** | Tartaria: World of Wonder |
| **Tagline** | Tune the World. Reclaim the Golden Age. |
| **Tags** | Adventure, Open World, City Builder, RPG, Puzzle, Restoration, Atmospheric |
| **Categories** | Single-player, Steam Achievements, Steam Cloud, Full Controller Support, Steam Trading Cards |
| **Screenshots** | 1920×1080 and 3840×2160 (min 5, target 10) |
| **Preview Video** | 30s gameplay trailer — dome restoration → combat → giant mode |
| **System Requirements** | Minimum / Recommended / High (3 tiers) |

---

**Document Status:** FINAL  
**Cross-References:** `00_MASTER_GDD.md`, `07_PC_UX.md`, `06_COMBAT_PROGRESSION.md`  
**Last Updated:** March 25, 2026
