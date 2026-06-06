# TARTARIA WORLD OF WONDER — Production Pipeline
## Art Assets, Animation, Audio Recording, Outsource Specs & Tool Chain

---

> *"A great game isn't made — it's cultivated. Every asset, every animation, every sound is a seed planted with intention and harvested with craft."*

**Cross-References:**
- [10_ROADMAP.md](10_ROADMAP.md) — 5-phase development plan, team structure, $350k budget
- [09_TECHNICAL_SPEC.md](09_TECHNICAL_SPEC.md) — Technical specs, texture formats, LOD pipeline, audio architecture
- [05_CHARACTERS_DIALOGUE.md](05_CHARACTERS_DIALOGUE.md) — Character visual design, dialogue specifications
- [04_ARCHITECTURE_GUIDE.md](04_ARCHITECTURE_GUIDE.md) — Building archetypes, sacred geometry constraints
- [23_LOCALIZATION.md](23_LOCALIZATION.md) — Localization handoff pipeline, string management
- [14_HAPTIC_FEEDBACK.md](14_HAPTIC_FEEDBACK.md) — Haptic design specs

---

## Table of Contents

1. [Production Philosophy](#1-production-philosophy)
2. [Team Structure & Roles](#2-team-structure-roles)
3. [Art Asset Pipeline](#3-art-asset-pipeline)
4. [Animation Pipeline](#4-animation-pipeline)
5. [Audio Pipeline](#5-audio-pipeline)
6. [Outsource Contractor Management](#6-outsource-contractor-management)
7. [Version Control & Asset Management](#7-version-control-asset-management)
8. [Review & Approval Process](#8-review-approval-process)
9. [Tool Chain](#9-tool-chain)
10. [Quality Gates](#10-quality-gates)
11. [Risk Management](#11-risk-management)

---

## 1. Production Philosophy

### Core Principles

1. **Small team, big output.** 6 core team members + strategic outsource. Every person owns a domain. No redundancy, no gaps.
2. **Vertical slice proves everything.** No feature is "approved" until it's in the vertical slice (Moon 1 / Echohaven). Paper designs are hypotheses; playable builds are proof.
3. **Asset-driven milestones.** Every milestone is defined by deliverable assets, not time spent. Sprints produce playable content, not reports.
4. **Reuse ruthlessly.** Modular building system means 200+ unique buildings from ~30 base meshes + materials. One character rig drives all humanoid NPCs.
5. **Quality at source.** Fix problems in the content creation tool, not in Unity. A broken mesh costs 10× more to fix in-engine than in Blender.

---

## 2. Team Structure & Roles

Per [10_ROADMAP.md](10_ROADMAP.md), the core team is 6 people:

### 2.1 Core Team

| Role | Responsibility | Tools |
|---|---|---|
| **Creative Director / Lead Designer** (Nathan) | Vision, GDD, game design, narrative, approval authority | Docs, Unity Editor |
| **Lead Engineer** | Unity DOTS/ECS, systems, pipeline, CI/CD, performance | Unity, Rider, GitHub |
| **3D Artist / Environment** | Buildings, zones, props, VFX, lighting | Blender, Substance, Unity |
| **Character Artist / Animator** | Characters, rigs, animations, cloth sim | Blender, Substance, Unity |
| **UI/UX Designer** | UI layout, UX flow, accessibility, touch controls | Figma, Unity UI Toolkit |
| **Audio Designer / Composer** | Music, SFX, tuning system audio, spatial audio | Logic Pro, FMOD, Unity |

### 2.2 Outsource Roles

| Role | Scope | Contract Type | Budget |
|---|---|---|---|
| **Concept Artist** | Character concepts, building concepts, zone mood boards | Per-deliverable | $5,000–$10,000 |
| **Additional 3D Assets** | DLC buildings, cosmetic items, prop batches | Per-deliverable | $15,000–$25,000 |
| **QA Testing** | Test passes, device testing, accessibility audit | Hourly / per-cycle | $10,000–$15,000 |
| **Localization** | Translation, LQA, cultural adaptation | Per-word / per-language | $85,000–$125,000 (see [23_LOCALIZATION.md](23_LOCALIZATION.md)) |
| **Voice Acting** | Selective VO (Phase 2) | Per-session | $5,000–$10,000 |
| **Marketing / Trailer** | Launch trailer, store assets, social media | Per-deliverable | $10,000–$20,000 |

---

## 3. Art Asset Pipeline

### 3.1 Environment Art Pipeline

```
Concept (2D)
│ ← Concept artist (outsource or in-house)
│ Deliverable: 2–3 color concepts per building archetype
│ Approval: Creative Director
│
├── Blockout (3D gray-box)
│   ← 3D Artist
│   Tool: Blender
│   Spec: Low-poly placeholder, correct scale, snap points defined
│   Deliverable: .fbx, tested in Unity with golden ratio validator
│   Approval: Lead Designer + Lead Engineer (performance check)
│
├── High-Poly Sculpt
│   ← 3D Artist
│   Tool: Blender (Sculpt Mode)
│   Spec: Detail pass — ornamental carvings, Tartarian motifs
│   Deliverable: High-poly .blend (bake source)
│
├── Low-Poly + UV
│   ← 3D Artist
│   Tool: Blender
│   Spec: Game-ready mesh, per [09_TECHNICAL_SPEC.md](09_TECHNICAL_SPEC.md) LOD budgets
│   Deliverable: LOD 0–3 meshes, UV mapped, .fbx
│
├── Texturing
│   ← 3D Artist
│   Tool: Substance 3D Painter
│   Spec: PBR metallic workflow (Albedo, Normal, Metallic/Smoothness, Emission)
│   Format: ASTC 6×6 or 8×8, per tech spec
│   Deliverable: Texture sets per material, .png → Unity converts to ASTC
│
├── Material Setup
│   ← 3D Artist / Lead Engineer
│   Tool: Unity URP Shader Graph
│   Spec: Custom shaders for Aether Glow, Mud Corruption, Restoration Reveal
│   Deliverable: .mat files in Unity, shader parameters documented
│
├── LOD Generation
│   ← 3D Artist (semi-automated)
│   Tool: Blender Decimate + manual cleanup
│   Spec: LOD 0 (full), LOD 1 (50%), LOD 2 (15%), LOD 3 (billboard)
│   Deliverable: LOD group in Unity with transition distances set
│
├── Lightmap UV
│   ← 3D Artist
│   Tool: Blender (UV2 channel for lightmaps)
│   Spec: Non-overlapping, adequate texel density
│
└── Integration
    ← Lead Engineer
    Tool: Unity
    Steps: Import .fbx, assign materials, configure Addressable group,
           set collision, register snap points, test golden ratio validation
    Deliverable: Prefab in Unity, tagged, Addressable, tested
```

### 3.2 Environment Asset Count Estimates

| Asset Type | Unique Meshes | LOD Variants | Total Meshes |
|---|---|---|---|
| Domes (5 sizes) | 5 | ×4 LODs | 20 |
| Antenna Spires (5 variants) | 5 | ×4 | 20 |
| Star Forts (3 sizes) | 3 | ×4 | 12 |
| Decorative Buildings | 15 | ×4 | 60 |
| Player-Placeable Buildings | 20 | ×4 | 80 |
| Props (benches, lamps, fountains) | 30 | ×2 | 60 |
| Terrain Features | 10 | ×2 | 20 |
| Zone-Specific Landmarks | 13 | ×4 | 52 |
| **Total** | **~101** | | **~324** |

### 3.3 Character Art Pipeline

```
Concept (2D)
│ ← Concept artist
│ Deliverable: Front/back/side turnaround + expression sheet
│ Approval: Creative Director
│
├── Base Mesh + Sculpt
│   ← Character Artist
│   Tool: Blender
│   Spec: Humanoid topology, facial blend shapes, cloth-ready mesh
│
├── Retopology + UV
│   ← Character Artist
│   Spec: 8K–12K tris (main characters), 3K–5K tris (NPCs)
│
├── Texturing
│   ← Character Artist
│   Tool: Substance 3D Painter
│   Spec: Albedo, Normal, Emission (Aether glow for companions)
│   Format: 1024×1024 ASTC 6×6
│
├── Rigging
│   ← Character Artist
│   Tool: Blender (shared humanoid rig)
│   Spec: Unity Humanoid rig, ~65 bones, facial rig (20 blend shapes)
│
└── Cosmetic Variants
    ← Character Artist (or outsource)
    Spec: Swap materials/meshes on same rig for outfits
    Deliverable: Variant prefabs per cosmetic
```

### 3.4 Character Count Estimates

| Character Type | Count | Tri Budget Each |
|---|---|---|
| Elara (protagonist) | 1 | 12,000 |
| Companions (Milo, Lirael, Korath, Thorne, Anastasia) | 5 | 10,000 |
| Named NPCs | ~20 | 5,000 |
| Generic NPCs | ~10 variants | 3,000 |
| Enemies (base types) | ~8 types | 4,000 |
| Bosses | ~10 | 15,000 |

---

## 4. Animation Pipeline

### 4.1 Animation Categories

| Category | Count | Priority |
|---|---|---|
| **Player locomotion** | 12 (idle, walk, run, dodge, 8 directions) | P0 — Phase 1 |
| **Combat animations** | 20 (7 frequency attacks, combos, dodge, giant mode) | P0 — Phase 1 |
| **Building interactions** | 8 (restore, place, upgrade, demolish) | P0 — Phase 1 |
| **Companion idle/emotes** | 25 (5 per companion) | P1 — Phase 2 |
| **Companion reactions** | 30 (6 per companion, context-specific) | P1 — Phase 2 |
| **NPC ambient** | 15 (generic idle variants, greeting, working) | P1 — Phase 2 |
| **Enemy animations** | 24 (3 per enemy type × 8 types) | P0 — Phase 1 |
| **Boss animations** | 40 (4 per boss × 10 bosses) | P0–P1 (phased by Moon) |
| **Cutscene** | 30 (key narrative moments) | P1 — phased by Moon |
| **Mini-game** | 12 (player interaction per mini-game type) | P0 — Phase 1 |

### 4.2 Animation Technical Specs

| Spec | Value |
|---|---|
| **Rig** | Unity Humanoid, shared skeleton |
| **Frame rate** | 30 FPS (baked, interpolated to 60 at runtime) |
| **Blend tree** | Locomotion (8-directional), combat (state machine) |
| **IK** | Foot IK (terrain adaptation), hand IK (building interaction) |
| **Root motion** | Combat attacks only; locomotion uses code-driven movement |
| **Compression** | Unity Optimal keyframe reduction |
| **Facial** | Blend shapes (20), driven by dialogue system |

### 4.3 Animation Pipeline

```
Reference / Storyboard
│ ← Creative Director
│ Deliverable: Stick-figure pose sequence or video reference
│
├── Blockout (stepped animation)
│   ← Character Artist / Animator
│   Tool: Blender
│   Spec: Key poses at 30 FPS, no polish
│   Approval: Creative Director (does this feel right?)
│
├── Polish (splined animation)
│   ← Animator
│   Tool: Blender
│   Spec: Full arcs, overlapping action, anticipation/follow-through
│
├── Export
│   ← Animator
│   Tool: Blender → .fbx
│   Spec: Unity Humanoid-compatible, root motion where needed
│
└── Integration
    ← Lead Engineer
    Tool: Unity Animator Controller
    Spec: State machine, transition conditions, blend trees
    Test: In-game playback validation
```

---

## 5. Audio Pipeline

### 5.1 Music Production

| Phase | Deliverable | Format | Spec |
|---|---|---|---|
| **Theme composition** | 4-stem adaptive layers per zone (13 zones) | Logic Pro project | 432 Hz tuning, 4 layers (ambient, melodic, rhythmic, climax) |
| **Stem export** | Individual stems | WAV 48kHz/24-bit | Per [09_TECHNICAL_SPEC.md §6.2](09_TECHNICAL_SPEC.md) adaptive system |
| **Master for streaming** | Final mix per zone state | AAC 256kbps | 4 mixes per zone (desolate, hopeful, vibrant, triumphant) |
| **Implementation** | Unity Audio + adaptive trigger system | — | Cross-fade rules per RS threshold |

### 5.2 SFX Production

| Category | Count | Source | Format |
|---|---|---|---|
| **UI sounds** | 30 | Synthesized | WAV → Vorbis 96kbps mono |
| **Combat SFX** | 50 (7 frequency hits + impacts + combos) | Foley + synthesized | WAV → Vorbis 96kbps mono |
| **Environmental** | 40 (per biome type × ambience layers) | Field recording + processed | WAV → Vorbis 128kbps stereo |
| **Building SFX** | 20 (restoration, placement, upgrade, destroy) | Foley | WAV → Vorbis 96kbps mono |
| **Mini-game audio** | 30 (per game type × interactions) | Synthesized (musical) | WAV → PCM (uncompressed, tuning precision) |
| **Companion emotes** | 25 (5 per companion, non-verbal) | Voice actor | WAV → Vorbis 128kbps |

### 5.3 Voice Recording (Phase 2)

Per [23_LOCALIZATION.md](23_LOCALIZATION.md), selective VO in Phase 2:

| Content | Lines | Languages | Sessions |
|---|---|---|---|
| Milo key moments | 30 lines | EN first, then JA/KO/ZH | 2 sessions per language |
| Anastasia 5 words | 5 lines | EN only (ethereal, universal) | 1 session |
| Moon 13 climax narration | 10 lines | EN first | 1 session |
| **Total** | ~45 lines EN | 4 languages max | ~12 sessions |

**VO Spec:**
- Recording: Dry studio, 48kHz/24-bit, pop filter, <-60dB noise floor
- Post-processing: De-noise → EQ → 432 Hz pitch-reference → spatial reverb (per zone)
- Delivery: WAV stems + processed WAV + Vorbis 128kbps compressed

### 5.4 Haptic Design

Per [14_HAPTIC_FEEDBACK.md](14_HAPTIC_FEEDBACK.md):

| Phase | Deliverable | Tool |
|---|---|---|
| Design | Haptic event specification (21 subsystems) | Spreadsheet → Gamepad haptic patterns |
| Implementation | Haptic pattern files per event category | Gamepad Haptics API (DualSense / Xbox) |
| Testing | Device testing matrix (per haptic intensity tier) | Physical controller test |

---

## 6. Outsource Contractor Management

### 6.1 Contractor Selection Criteria

| Criterion | Minimum | Preferred |
|---|---|---|
| Portfolio quality | Stylistically compatible with Tartaria | Previous mobile game / fantasy work |
| Communication | Weekly updates | Daily check-ins during active deliverable |
| Turnaround | Per-deliverable milestones | Ahead of schedule |
| Tool compatibility | Blender + Substance | Same tool chain as core team |
| Revision policy | 2 revisions included | 3 revisions |

### 6.2 Deliverable Specifications (Style Guide)

Every outsource package includes:

| Document | Content |
|---|---|
| **Art Bible** (provided) | Color palette, material reference, architectural motifs, sacred geometry rules |
| **Technical Spec Sheet** | Poly budget, texture size, UV requirements, export format |
| **Reference Pack** | 3–5 approved in-game screenshots + concept art |
| **Naming Convention** | `{type}_{zone}_{variant}_{lod}.fbx` e.g. `dome_echohaven_gold_lod0.fbx` |
| **Checklist** | Pre-delivery checklist (no n-gons, UV2 present, correct scale, etc.) |

### 6.3 Outsource Handoff Flow

```
Brief (Creative Director)
│ Deliverable: Art brief document + reference pack
│
├── Milestone 1: Concept / Blockout (Contractor)
│   Review: Creative Director (approve style and direction)
│   Gate: Must pass before high-poly begins
│
├── Milestone 2: High-Poly / Detail (Contractor)
│   Review: Creative Director + 3D Artist (quality + tech spec)
│
├── Milestone 3: Final Delivery (Contractor)
│   Review: Full review + technical validation (Lead Engineer)
│   Gate: Import into Unity, test in-game
│
└── Payment: Per-milestone (30% / 30% / 40%)
```

---

## 7. Version Control & Asset Management

### 7.1 Repository Structure

```
TARTARIA (GitHub)
├── docs/          ← GDD documents (this repo)
│
TARTARIA-Unity (GitHub, separate repo)
├── Assets/
│   ├── Art/
│   │   ├── Buildings/
│   │   │   ├── Domes/
│   │   │   ├── Spires/
│   │   │   ├── StarForts/
│   │   │   └── Props/
│   │   ├── Characters/
│   │   │   ├── Protagonist/
│   │   │   ├── Companions/
│   │   │   ├── NPCs/
│   │   │   └── Enemies/
│   │   ├── Environments/
│   │   │   └── {ZoneName}/
│   │   ├── UI/
│   │   └── VFX/
│   ├── Animation/
│   │   ├── Locomotion/
│   │   ├── Combat/
│   │   ├── Interaction/
│   │   └── Cutscene/
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   ├── Ambient/
│   │   └── VO/
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Scripts/
│   ├── Shaders/
│   ├── Addressables/
│   └── Resources/
├── Packages/
└── ProjectSettings/
```

### 7.2 Git LFS Configuration

| Extension | LFS Tracked | Reason |
|---|---|---|
| `.fbx` | Yes | 3D meshes (large binary) |
| `.blend` | Yes | Blender source files |
| `.png`, `.tga` | Yes | Textures |
| `.wav`, `.ogg` | Yes | Audio files |
| `.anim` | Yes | Animation clips |
| `.mat`, `.shader` | No | Text-based, small |
| `.cs` | No | Code, merge-friendly |
| `.unity` | Yes | Scene files (binary YAML) |
| `.prefab` | No | Text YAML (merge-friendly with smart merge) |

### 7.3 Branching Strategy

| Branch | Purpose | Merge To |
|---|---|---|
| `main` | Stable, shippable | — |
| `develop` | Integration branch | main (release) |
| `feature/{name}` | Individual features | develop |
| `art/{name}` | Art asset batches | develop |
| `hotfix/{name}` | Critical fixes | main + develop |

---

## 8. Review & Approval Process

### 8.1 Asset Review Workflow

```
Creator submits asset
│
├── Technical Review (Lead Engineer)
│   Checks: poly count, texture size, LOD correctness, import errors
│   Result: ✅ Pass / 🔄 Revision needed (with notes)
│
├── Art Review (Creative Director)
│   Checks: style consistency, sacred geometry compliance, emotional tone
│   Result: ✅ Approve / 🔄 Revision needed (with reference)
│
├── In-Game Test (Lead Engineer + Designer)
│   Checks: in-engine rendering, performance profile, gameplay integration
│   Result: ✅ Ship / 🔄 Polish needed
│
└── Merge to develop branch
```

### 8.2 Review Cadence

| Review Type | Frequency | Attendees |
|---|---|---|
| **Daily standup** | Daily (15 min, async-friendly) | All core team |
| **Art review** | 2× per week | Creative Director + Artists |
| **Sprint review** | Biweekly (per [10_ROADMAP.md](10_ROADMAP.md) sprints) | All + stakeholders |
| **Milestone gate** | Per phase gate | All + external advisors |

---

## 9. Tool Chain

### 9.1 Production Tools

| Tool | Purpose | License |
|---|---|---|
| **Unity 6 LTS** | Game engine | Unity Pro ($2,040/year) |
| **Blender 4.x** | 3D modeling, animation, rigging | Free / open source |
| **Substance 3D Painter** | Texturing | $22/month (Adobe) |
| **Substance 3D Designer** | Procedural materials | $22/month (Adobe) |
| **Figma** | UI/UX design, prototyping | Free tier / $15/month |
| **Logic Pro** | Music composition, audio mixing | $200 one-time (macOS) |
| **FMOD Studio** | Adaptive audio, spatial | Free (indie tier) |
| **JetBrains Rider** | C# IDE | $150/year |
| **GitHub** | Version control, CI/CD | Team plan ($4/user/month) |
| **GitHub Actions** | CI/CD pipeline | Included with repo |
| **Crowdin / Lokalise** | Localization management | $50–$120/month |
| **Steam** | PC beta testing & Early Access | Free (Steamworks) |
| **Visual Studio / Rider** | PC build, profiling | Free / $150/year |
| **Instruments / Unity Profiler** | Performance profiling | Free |
| **Firebase** | Backend, analytics, auth | Free tier → ~$200/month at scale |

### 9.2 Annual Tool Cost

| Category | Annual Cost |
|---|---|
| Unity Pro (1 seat) | $2,040 |
| Adobe Substance (2 seats) | $528 |
| Figma Pro (1 seat) | $180 |
| Logic Pro | $200 (one-time) |
| JetBrains Rider (1 seat) | $150 |
| GitHub Team (6 users) | $288 |
| Localization TMS | $600–$1,440 |
| Firebase (year 1) | $500–$2,400 |
| **Total annual** | **~$4,500–$7,200** |

---

## 10. Quality Gates

### 10.1 Phase Gate Criteria

Per [10_ROADMAP.md](10_ROADMAP.md), each phase has gate criteria:

| Gate | Phase | Must-Have Deliverables |
|---|---|---|
| **Gate 1: Vertical Slice** | Phase 1 → 2 | Echohaven playable (15 min), 3 building types, combat, 1 companion |
| **Gate 2: Alpha** | Phase 2 → 3 | Moon 1–4 playable, 5 zones, all companions, 4 mini-games |
| **Gate 3: Beta** | Phase 3 → 4 | Moon 1–13 playable, all zones, all systems functional |
| **Gate 4: Release Candidate** | Phase 4 → 5 | Full polish, localization Tier 1, accessibility Tier A, store assets |
| **Gate 5: Launch** | Phase 5 | Steam store approved, marketing live, analytics verified |

### 10.2 Automated Quality Checks (CI)

| Check | Trigger | Tool |
|---|---|---|
| Unit tests (EditMode) | Every push | Unity Test Framework |
| PlayMode tests | Every push to develop | Unity Test Framework |
| IL2CPP build | Daily | GitHub Actions (Windows) |
| Addressable bundle validation | Merge to develop | Custom script |
| Texture size audit | Asset import | Custom editor script |
| Poly count audit | Asset import | Custom editor script |
| Shader compile check | Merge to develop | Unity build |
| Localization key validation | Localization import | Crowdin webhook + script |

---

## 11. Risk Management

### 11.1 Production Risks

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| Key team member leaves | High | Medium | Documentation-first culture, no single points of knowledge |
| Outsource quality below standard | Medium | Medium | Milestone-gated payments, art bible, revision policy |
| Unity DOTS breaking change | High | Low | Pin Unity LTS version, test updates before adoption |
| Scope creep (too many building types) | High | High | Lock asset list per phase, defer to DLC |
| Performance on target device | High | Medium | Weekly profiling sessions, thermal budget monitoring |
| Steam review delay | Medium | Low | Follow Steam guidelines from day 1, early Steamworks testing |
| Budget overrun | High | Medium | Monthly budget review, cut cosmetic variety before core content |

### 11.2 Contingency Plans

| Scenario | Plan |
|---|---|
| **Over budget by 20%** | Reduce outsource scope, defer Tier 2 localization to post-launch |
| **Behind schedule by 1 month** | Cut 2 DLC zones from Phase 2, ship Moon 1–10 at launch |
| **Performance target missed** | Reduce LOD 0 detail, disable volumetric fog, simplify Aether VFX |
| **Key artist unavailable** | Outsource partner on standby, modular art allows parallel work |

---

*Production is the art of turning vision into assets and assets into wonder. Plan carefully, execute efficiently, and never compromise on the things the player will feel.*

---

**Document Status:** FINAL
**Author:** Nathan / Resonance Energy
**Last Updated:** March 25, 2026
