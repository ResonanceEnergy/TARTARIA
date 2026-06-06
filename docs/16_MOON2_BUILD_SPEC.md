# TARTARIA — Moon 2 Build Specification
## Zone: Crystalline Caverns — From Outside-In to Inside-Out

---

> *"Moon 1 taught the player to listen to the world. Moon 2 teaches them to listen to themselves — by shrinking inside the architecture and finding that the world they restored is hollow inside, and the corruption was already there, baked into the lattice."*

**Document Purpose:** Exact specification for the Moon 2 vertical slice. Built on top of a finished Moon 1 — every system in this doc assumes Moon 1's GATE 1 criteria are satisfied. This is the GATE 2 deliverable.

**Cross-References:**
- [15_MVP_BUILD_SPEC.md](15_MVP_BUILD_SPEC.md) — Moon 1 build spec (structural template + carry-over systems)
- [03_CAMPAIGN_13_MOONS.md](03_CAMPAIGN_13_MOONS.md) §Moon 2 — narrative canon (Lunar Moon — "The Challenge of Shadows")
- [MOON1_GAP_REPORT_2026-06-04.md](MOON1_GAP_REPORT_2026-06-04.md) — Moon 1 gaps Moon 2 must NOT inherit
- [PREFAB_LAYOUT.md](PREFAB_LAYOUT.md) — per-Moon prefab bucket conventions
- [CLAUDE.md](../CLAUDE.md) — band-name canon (Telluric 7.83 Hz / Harmonic 432 Hz / Celestial 528 Hz) + anti-circling mandate

---

## Table of Contents

1. [Moon 2 Scope — What's In / Not In](#1-moon-2-scope)
2. [Carry-over Systems from Moon 1](#2-carry-over-systems-from-moon-1)
3. [Zone: Crystalline Caverns](#3-zone-crystalline-caverns)
4. [Micro-Giant Mode — Shrink-to-Inner-Fractal-Explore](#4-micro-giant-mode)
5. [Dissonance Crystals — Reverse Cymatic Puzzle](#5-dissonance-crystals)
6. [Bell Tower Scalar Waves — Golden Ripple Visualization](#6-bell-tower-scalar-waves)
7. [NPC: Cassian — Ally-Arc-with-Betrayal](#7-npc-cassian)
8. [Building Restoration — 3 Unique Buildings](#8-building-restoration-3-unique-buildings)
9. [Mini-Game Variants — Moon 2 Specific](#9-mini-game-variants-moon-2-specific)
10. [Combat — Crystal Sentry + ResetScout Escalation](#10-combat-crystal-sentry-resetscout-escalation)
11. [Audio — Crystalline Overlays + Bell Scalar Tones](#11-audio-crystalline-overlays)
12. [Visual — Dissonance Shader + Bell-Tower Aurora](#12-visual-dissonance-shader-bell-tower-aurora)
13. [Save Schema Update — Moon 2 Progression Flags](#13-save-schema-update)
14. [GATE 2 Exit Criteria](#14-gate-2-exit-criteria)
15. [Risk Register — Moon 2 Specific](#15-risk-register-moon-2-specific)
16. [Asset Budget](#16-asset-budget)

---

## 1. Moon 2 Scope

### What's In (Moon 2 Vertical Slice — Days 29–56 in-fiction)

| Feature | Scope | Status |
|---|---|---|
| Crystalline Caverns zone | 600 m radius, 3 sub-chambers + 1 hub | Full |
| Micro-Giant Mode | Player shrink ability, sub-meter fractal interior of 1 hero building | Full |
| Dissonance Crystals | 5 placed instances, reverse-cymatic puzzle, shatter shader | Full |
| Bell Tower (1 hero building) | Restorable; scalar wave VFX visible across both zones | Full |
| Pylon (1 utility building) | 1st restorable; powers caverns lighting | Full |
| Resonance Chapel (1 hero) | 2nd hero; Cassian betrayal beat location | Full |
| Cassian NPC | Trust/Doubt arc, 3 diary fragments, dialogue tree | Full |
| Crystal Sentry enemy | 1 new enemy type, lattice combat | Full |
| Mini-game Variant E (Crystal Rotation Match) | New for Moon 2 | Full |
| Mini-game Variant F (Bell Tower Sync) | New for Moon 2 | Full |
| Adaptive music — Moon 2 layers | Crystalline overlay + bell scalar tone | Full |
| Save schema v2 | Trust/Doubt enum, diary flags, micro-giant unlock | Full |
| Moon 1 ↔ Moon 2 transition | Echohaven remains visitable; no regression | Full |

### What's NOT In (deferred to Moon 3+)

- Orphan Train / spectral children (Moon 3 — Electric Moon)
- Resonance Trains as transit (Moon 3)
- Lirael's calendar gate evolution beyond Moon 1 baseline
- Cassian's true confrontation (Moon 7 climax — Moon 2 only plants seeds)
- Star Forts, prophecy stones, junior architect auto-build
- Any Moon 3–13 mechanic

### The 15-Minute Moon 2 Demo

```
0:00  — Player loads from Moon 1 save. Echohaven still glows behind them.
0:30  — A faint dissonance hum from the north. Cathedral's harmonic stutters.
1:00  — Walk to Cavern Mouth. Milo: "I don't like this. Something's *wrong* in there."
2:00  — Enter the Caverns. Lighting is sickly violet. First Dissonance Crystal visible.
3:00  — Cassian's first appearance — emerges from a side passage, charming. "Need help?"
4:30  — Tutorial: Micro-Giant Mode unlock. Cassian shows the shrink glyph.
5:30  — Shrink into the Pylon's inner lattice. Fractal corridors. First reverse-cymatic puzzle.
7:00  — Pylon restored. Cavern lighting flips from violet to gold. Cassian: "See? I told you."
8:00  — Bell Tower discovered in the second chamber. 3-node tuning (Variant F: Bell Sync).
10:00 — Bell rings. Scalar waves visible in sky — golden ripples crossing back to Echohaven.
11:00 — Crystal Sentry spawns. First combat in tight crystal corridor.
12:00 — Sentry defeated. RS +25. Cassian a beat too fast pointing at the next crystal.
13:00 — Resonance Chapel — 3rd building. Variant E (Crystal Rotation Match).
14:00 — Chapel restored. Diary fragment drops at Cassian's feet. Player reads it.
14:30 — Trust/Doubt prompt. Player chooses. Save flag set. Cassian reacts in dialogue.
15:00 — Long shot: bell scalar waves still rippling. Cassian's silhouette against the gold.
```

---

## 2. Carry-over Systems from Moon 1

These systems ship into Moon 2 unchanged. Moon 2 must NOT break them.

| System | Moon 1 file | Carry-over rule |
|---|---|---|
| Aether 3-band sim | `Assets/_Project/Scripts/Core/AetherFieldSystem.cs` | Add new sink type (Dissonance Crystal, strength `-0.8`, radius `25 m`) without modifying the band model itself. Telluric / Harmonic / Celestial naming canon enforced — see CLAUDE.md "Things decided" §. |
| Resonance Score Engine | `Assets/_Project/Scripts/Core/ResonanceScoreSystem.cs` | Same scoring rules, new event types appended (`OnCrystalShattered +20`, `OnBellRung +15`, `OnMicroGiantEntered +5`). |
| Tuning Mini-Game framework | `Assets/_Project/Scripts/Gameplay/Tuning/ITuningVariant.cs` | Add Variant E + F as new `ITuningVariant` implementations. Dispatcher already routes by `assignedVariant` (see Moon 1 C.L5 `519d0c52`). |
| Building Restoration State Machine | `Assets/_Project/Scripts/Gameplay/Building/BuildingRestorationStateMachine.cs` | Reused. Moon 2 hero buildings register new `BuildingDefinition` ScriptableObjects only — no state-machine edits. |
| Input + Camera | `Assets/_Project/Scripts/Input/PlayerInputHandler.cs` + `Camera/CameraController.cs` | Reused. New Micro-Giant transition handled by `CameraController.PushZoom(0.1f, 1.5s)` — no new public methods. |
| Save / Load | `Assets/_Project/Scripts/Save/Moon1SaveCoordinator.cs` | Extended to `SaveCoordinator` (rename in §13). Schema bumped to v2 with backward-compatible v1 reader. |
| Adaptive music | `Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs` | Two new stems added (`music/moon2/crystalline_bed.ogg`, `music/moon2/bell_scalar.ogg`). RS-threshold routing unchanged. |
| F310 controller mapping | `Assets/_Project/Scripts/Input/LogitechControllerSupport.cs` + `docs/appendices/D_CONTROLS_F310.md` | Unchanged. Micro-Giant Mode reuses Y button (Aether Vision toggle gets long-press = Micro-Giant). |

### Anti-regression contract (per CLAUDE.md NO-DEBT)

Per the 2026-06-04 honest reset, Moon 2 must NOT inherit any of these Moon 1 anti-patterns:

- No new `Moon2*Safety.cs`, `Moon2*Fix.cs`, `Moon2*Override.cs`, `Moon2*Daemon.cs`, `Moon2*Rescue.cs`.
- No new `[RuntimeInitializeOnLoadMethod]` that mutates scene state — fix the prefab/scene at author time.
- No `Resources.Load` on a path that doesn't exist on disk (Moon 1 had ~15 broken sites — see MOON1_GAP_REPORT §2.2). All Moon 2 `Resources.Load` callsites must be greppable to a real asset under `Assets/_Project/Resources/`.
- No primitive prefabs masquerading as content. Crystal Sentry, Pylon, Resonance Chapel, Bell Tower must ship as real FBX or kit-composed prefabs — no `GameObject.CreatePrimitive(Cube)` cheats.
- Material assignments verified at author time. No prefab may ship with `m_Materials: -{fileID: 0}` (magenta-at-runtime — see MOON1_GAP_REPORT P2 / AnastasiaRocker).

---

## 3. Zone: Crystalline Caverns

### Geography

- **Shape:** 4 connected sub-chambers carved through a single mountain, accessible from Echohaven via a 50-m approach tunnel. Total playable area ~600 m radius, but heavily corridor-shaped (not open-field like Moon 1).
- **Elevation:** Caverns descend 80 m from the surface entrance. Bell Tower sits at the deepest point and its scalar waves rise back through the rock to reach the Moon 1 sky.
- **Lighting baseline:** Sickly violet `#5C2D8B` cavern fungus glow when corrupted; flips to warm gold `#D4A017` (Tartarian Gold, canonical) post-restoration of the Pylon.
- **Atmosphere:** Cold, damp, ambient drip SFX, slow cavern wind low-pass filtered. Dissonance crystals add high-frequency tinnitus shimmer that resolves to silence on shatter.

### Sub-chamber layout

| Chamber | Hero contents | Approx dimensions | RS gate to enter |
|---|---|---|---|
| Antechamber (hub) | Cavern Mouth tunnel, save altar, Cassian intro | 30 × 40 m | 0 (open at Moon 2 start) |
| Pylon Vault | The Pylon (utility building), 2 dissonance crystals | 50 × 50 m × 30 m H | 0 |
| Bell Chamber | Bell Tower (hero building), 1 dissonance crystal | 80 × 80 m × 60 m H | 100 (post Pylon) |
| Chapel Crypt | Resonance Chapel (hero), 2 dissonance crystals, diary drop | 60 × 60 m × 40 m H | 150 (post Bell) |

### POIs (non-restorable, lore + RS rewards)

| Location | Description | Reward |
|---|---|---|
| The Cracked Mural | Tartarian fresco showing the Mud Flood — partially shattered | +10 RS, Milo dialogue |
| The Brine Pool | Mineral-rich water at chamber boundary; deep tone when Aether scanned | +5 RS |
| Cassian's Camp | Player encounters this *before* meeting him — his bedroll is already laid out, suspicious | +5 RS, Trust/Doubt flag start |
| The Echo Pillar | Vertical Tartarian column; when struck, plays a 432 Hz tone that briefly reveals dissonance crystals through walls | +10 RS, gameplay utility |

### Terrain technical spec (delta from Moon 1)

| Param | Value | Delta |
|---|---|---|
| Render path | URP Forward+, **Linear color space** | Linear enforced (Moon 1 P0 R1 must be fixed before Moon 2 starts) |
| Heightmap resolution | N/A (cavern interior) | Replaced by `ProBuilder + Cavern_Modules` prefab kit |
| Splat layers | 3 (raw stone, calcite vein, dust) | -1 vs Moon 1 |
| Vegetation instances | 60 (cave moss, glowing lichen) | -60 vs Moon 1 (caverns are sparser) |
| Real-time lights | 8 (4 baked, 4 dynamic point lights at crystals) | +2 vs Moon 1 budget — measured against 60 FPS Recommended PC |

---

## 4. Micro-Giant Mode

### Concept

The signature Moon 2 mechanic. Player shrinks to ~10% scale to walk *inside* the Tartarian fractal lattice of one hero building (the Resonance Chapel). The exterior was restored at full scale in Moon 1's vocabulary; the interior reveals it was never finished — corruption nested in the sub-mm geometry.

Per `docs/03_CAMPAIGN_13_MOONS.md:189`: *"shrink to explore the inner fractal architecture of the dome. Crystal corridors, fractal vaulting, impossible geometry."*

### State machine

```
NORMAL_SCALE
  → Player approaches Micro-Giant Glyph (placed at Chapel interior altar)
  → Hold Y (long-press, 1.5 s)
TRANSITIONING_IN
  → 1.5 s shrink animation (camera + player both)
  → Audio: descending 528 Hz → 264 Hz sub-octave
  → Haptic: long warm cascade
MICRO_SCALE (0.1× world units)
  → Player navigates inner lattice
  → Inner lattice uses separate NavMesh ("MicroNavMesh" layer)
  → Camera FOV widened (75° → 90°) to convey claustrophobia/scale
TRANSITIONING_OUT
  → Triggered by re-touching exit glyph OR completing inner restoration
  → 1.5 s grow animation; world returns to scale
```

### Implementation file plan

| File | Role | Approx LOC |
|---|---|---|
| `Assets/_Project/Scripts/Gameplay/MicroGiant/MicroGiantController.cs` | State machine, scale lerp, NavMesh swap | 220 |
| `Assets/_Project/Scripts/Gameplay/MicroGiant/MicroGiantGlyph.cs` | Interactable trigger | 80 |
| `Assets/_Project/Scripts/Camera/CameraController.cs` (extend) | Add `EnterMicroScale()` / `ExitMicroScale()` public methods | +40 |
| `Assets/_Project/Prefabs/Moon2/MicroGiant/Glyph_Entry.prefab` | Authored prefab with glyph mesh, particle, trigger collider | — |
| `Assets/_Project/Resources/Moon2/MicroLattice/` | 6 inner-lattice modular pieces (real FBX, not primitives) | — |

### Scale math

| Quantity | Normal | Micro | Reasoning |
|---|---|---|---|
| Player capsule | 1.0 × 2.0 m | 0.1 × 0.2 m | 10× shrink — small enough to walk between lattice struts |
| Camera follow distance | 15 m | 1.5 m | Maintains framing |
| Player walk speed | 5 m/s | 0.5 m/s (world) but renders as 5 m/s relative | Internal multiplier so feel is unchanged |
| Gravity | 9.81 m/s² | 9.81 m/s² (unchanged) | Don't break physics |

### Input mapping

| Input | Action |
|---|---|
| Y (long-press 1.5 s) | Enter / Exit Micro-Giant Mode (at a Glyph) |
| Y (short tap) | Aether Vision (unchanged from Moon 1) |
| Tab (KB) | Same |

Per `docs/appendices/D_CONTROLS_F310.md` — Y is currently single-press Aether Vision toggle. Moon 2 patch: change to short-tap = vision, long-press = Micro-Giant. Document this in the appendix when shipping.

---

## 5. Dissonance Crystals

### Concept

Black, angular, *wrong* — per `docs/03_CAMPAIGN_13_MOONS.md:184`. Tetrahedral structures that absorb Aether (anti-source / sink). Player solves a **reverse cymatic puzzle**: instead of matching a frequency to harmony, the player must produce **anti-harmony** — a dissonant chord — that shatters the crystal from inside.

### Crystal data

| Property | Value |
|---|---|
| Geometry | Tetrahedron, 1.2 m edge, jagged subdivision |
| Material | Custom shader `Tartaria/Dissonance` (see §12) |
| Aether sink strength | -0.8 |
| Aether sink radius | 25 m |
| HP | N/A — not damageable, only puzzle-shatterable |
| Audio loop | 18 Hz infrasound + 4000 Hz tinnitus (dual-band annoyance) |
| Particles | Black tetrahedral chips orbiting slowly |

### Reverse-Cymatic Puzzle

Per `docs/03_CAMPAIGN_13_MOONS.md:189` — "tuning them to self-destruct".

```
1. Player Aether-scans crystal → it reveals 3 internal resonance bands.
2. Each band has a TRUE frequency (the crystal's natural). 
3. Player must produce a DISSONANT frequency: deviation > 14% from the true.
4. UI shows 3 inverted sliders (target = "as far as possible from center").
5. All 3 bands held dissonant for 4 s → shatter shader plays (1.2 s).
6. Crystal explodes into purified shards (+20 RS, drops "Crystal Shard" pickup).
```

### Placement (5 crystals in Moon 2)

| # | Chamber | RS reward | Notes |
|---|---|---|---|
| C1 | Pylon Vault | 20 | Tutorial crystal — Cassian explains it |
| C2 | Pylon Vault | 20 | Hidden behind a fallen column |
| C3 | Bell Chamber | 20 | Suspended mid-air; requires bell ring to make accessible |
| C4 | Chapel Crypt | 20 | Inside Micro-Giant lattice (small-scale puzzle) |
| C5 | Chapel Crypt | 20 | Cassian's planted crystal (only revealed if player Doubts him) |

### Implementation file plan

| File | Role |
|---|---|
| `Assets/_Project/Scripts/Gameplay/Dissonance/DissonanceCrystal.cs` | Component on each crystal prefab — handles puzzle state, shatter trigger |
| `Assets/_Project/Scripts/Gameplay/Dissonance/ReverseCymaticPuzzle.cs` | UI + input controller for the 3-band dissonance |
| `Assets/_Project/Prefabs/Moon2/Crystals/DissonanceCrystal.prefab` | Authored prefab — real geometry, real material assignment (no `fileID: 0`) |
| `Assets/_Project/Resources/Moon2/VFX/CrystalShatter.prefab` | Shatter particle prefab — under `Resources/` so `Resources.Load("Moon2/VFX/CrystalShatter")` works |

---

## 6. Bell Tower Scalar Waves

### Concept

Per `docs/03_CAMPAIGN_13_MOONS.md:191`: *"Repair bell tower. First bell ring sends scalar waves pulsing across the sky — visible as golden ripples. Distant structures respond with faint echoes."*

Scalar wave = Tartarian-canon energy propagation that ignores inverse-square law. Visualized as concentric golden rings that emanate from the bell tower's apex and travel outward at constant amplitude until they reach the edge of the loaded scene.

### Bell Tower spec

| Property | Value |
|---|---|
| Height | 28 m |
| Base diameter | 6 m |
| Bell material | Bronze, PBR with 0.85 metallic / 0.25 roughness |
| Aether source strength | 1.2 (highest in Moon 2) |
| Aether source radius | 80 m (largest in Moon 2) |
| Tuning variant | F (Bell Sync — see §9) |
| Build prefab path | `Assets/_Project/Prefabs/Moon2/Buildings/BellTower.prefab` (authored, not stub — anti-regression vs MOON1_GAP P3-P6) |

### Scalar Wave VFX

```
On bell ring event:
  - Spawn ScalarWavePulse VFX at tower apex (world Y = 28 m)
  - VFX is a torus-mesh shader, expands outward at 30 m/s
  - Color: Tartarian Gold #D4A017
  - Alpha: starts 0.8, decays to 0.0 over 6 s (longer = farther)
  - Travels through walls (no occlusion test) — that's the "scalar" property
  - Bell rings 3× per restoration completion (one for each tuned node)
  - Echohaven (Moon 1 scene, if loaded as additive) receives a faint shimmer
    on its restored Cathedral spire — visual continuity across Moons
```

### Audio for Bell Tower

- Bell SFX: real bronze bell recording, pitched to A = 432 Hz (per CLAUDE.md canon)
- Sustain: 8 s with exponential decay
- Sub-frequency overlay: 7.83 Hz (Telluric) pulse on each ring — felt rather than heard
- Mid-frequency overlay: 432 Hz (Harmonic) — the bell's fundamental
- High-frequency overlay: 528 Hz (Celestial) at perfect tune only — adds aurora glow effect

### Implementation file plan

| File | Role |
|---|---|
| `Assets/_Project/Scripts/Gameplay/BellTower/BellTowerController.cs` | Bell ring trigger, scalar wave spawn, audio play |
| `Assets/_Project/Scripts/VFX/ScalarWavePulse.cs` | Particle controller — expansion lerp, alpha decay |
| `Assets/_Project/Prefabs/Moon2/Buildings/BellTower.prefab` | Real building, kit-composed, no Detail_* primitives |
| `Assets/_Project/Resources/Moon2/Audio/BellTower_A432.ogg` | Source recording, runtime-loadable |

---

## 7. NPC: Cassian

### Character spec

| Attribute | Detail |
|---|---|
| Name | Cassian |
| Species | Human (ostensibly) |
| Age (apparent) | Late 30s |
| Visual | Tall, lean, dark hair, neat clothing — out of place in mud-flood ruins |
| Voice | Articulate, slightly aristocratic, warm-but-calculating |
| Function (Moon 2) | Apparent ally; planted seeds for Moon 7 betrayal reveal |
| Background canon | Per `docs/03_CAMPAIGN_13_MOONS.md:179` — "introduction as apparent ally" |
| Long-arc canon | Per `docs/03_CAMPAIGN_13_MOONS.md:405-407` — Moon 7 reveals he was a Reset agent infiltrator who *may* have had a change of heart |

### Trust / Doubt System

The first Moon-spanning player choice. Set via a binary save flag at Moon 2 climax; affects Moon 7 path.

```csharp
public enum CassianTrust : byte
{
    Unset = 0,
    Trusted = 1,
    Doubted = 2,
}
```

The choice is presented after Diary Fragment #3 (see below) at Moon 2 day 25-28 in-fiction. UI prompt:

```
DIARY FRAGMENT FOUND
"...the codes worked. The dome's resonance shifted exactly as projected. 
 If the locals ask, I joined them in '04. Show no surprise at the bell."

[ TRUST  Cassian's explanation ]     [ DOUBT  this changes everything ]
```

Trusted: Cassian becomes companion for Moon 7. Easier Moon 7 entry, devastating betrayal.
Doubted: Cassian goes off-grid until Moon 7 confrontation. Player primed but no companion bonus.

### Diary Fragments

3 fragments, found at fixed locations:

| # | Location | Content | Reveals |
|---|---|---|---|
| 1 | Antechamber, behind Cassian's bedroll | "Day 47. They still don't suspect. The mud-flood story works because nobody wants to investigate it." | Strong doubt signal |
| 2 | Bell Chamber, after bell ring | "Day 51. The bell's scalar wave registered on the Eastern network. The Reset will know soon. I should leave." | Suggests internal conflict |
| 3 | Chapel Crypt, on restoration | "Day 55. The girl Lirael recognized something in me. I don't know if I want to be the man she sees or the one who arrived here." | Ambiguous — change of heart possible |

### Dialogue tree (Yarn Spinner)

| Yarn node | Trigger | Lines | Status |
|---|---|---|---|
| `cassian_intro` | Cavern mouth entry | 6 lines, charm + offer to help | Authored |
| `cassian_pylon_explain` | Pylon Vault entry | 4 lines, explains "Reset sabotage" | Authored |
| `cassian_bell_react` | Bell ring success | 3 lines, briefly genuinely awed | Authored |
| `cassian_betrayal_seed` | Diary Fragment #1 read | 5 lines, plausible explanation | Authored |
| `cassian_chapel_climax` | Chapel restored | 8 lines, presents Diary Fragment #3 + Trust/Doubt prompt | Authored |
| `cassian_trusted_outro` | Player chose Trust | 4 lines, grateful, sets Moon 7 flag | Authored |
| `cassian_doubted_outro` | Player chose Doubt | 4 lines, defensive, exits scene | Authored |

All Yarn files under `Assets/_Project/Yarn/Moon2/Cassian.yarn`. NodeExists case-sensitivity rule from Sprint 11 L7 must be respected — all node names lowercase + underscore.

### Prefab spec — NO MOON 1 REGRESSIONS

Per `MOON1_GAP_REPORT_2026-06-04.md` §2.4 N1-N5, all Moon 1 NPC prefabs lack `Animator` components and have FBX-cm collider scaling bugs. Moon 2's Cassian must ship with:

- ✅ Real FBX model (no capsule placeholder)
- ✅ `Animator` component with `Moon2_NPC` controller (idle, walk, talk, point gestures)
- ✅ `CapsuleCollider` Height=2.0, Center=(0,1,0) — Unity-meters, not FBX-cm
- ✅ Real material assignment on all renderer slots (no `m_Materials: -{fileID: 0}`)
- ✅ NavMeshAgent with radius=0.5, height=2.0, baseOffset=0

---

## 8. Building Restoration — 3 Unique Buildings

Per the 2026-06-04 honest reset, all Moon 2 hero buildings ship as **real authored prefabs** — not stub files (MOON1_GAP P3-P6), not Detail_* primitive clusters (MOON1_GAP §2.5).

### 8.1 The Pylon — Utility Building

| Property | Value |
|---|---|
| Function | Powers Crystalline Caverns lighting (flips violet → gold post-restoration) |
| Size | 12 m diameter base, 22 m height |
| Golden-ratio target | Height/base = 22/12 ≈ 1.833 → φ-bonus active if within 2% of φ |
| Aether source | 0.5 strength, 60 m radius |
| Tuning variant | A (Frequency Slider — Moon 1 carry-over) |
| Node count | 3 |
| Prefab path | `Assets/_Project/Prefabs/Moon2/Buildings/Pylon.prefab` |

### 8.2 Bell Tower — Hero Building (covered §6)

Full spec in §6. Tuning Variant F (Bell Sync). 3 nodes, each ring a bell on completion.

### 8.3 Resonance Chapel — Hero Building (Micro-Giant entry)

| Property | Value |
|---|---|
| Function | Outer restoration unlocks Micro-Giant Glyph at interior altar; inner restoration shatters C4 + C5 dissonance crystals from the inside |
| Size | 18 m × 24 m × 16 m H exterior; ~1.8 × 2.4 × 1.6 m interior at micro scale |
| Aether source | 0.9 strength, 50 m radius |
| Tuning variants | Exterior: Variant E (Crystal Rotation Match — see §9). Interior (micro): reverse-cymatic puzzle (see §5). |
| Prefab paths | `Prefabs/Moon2/Buildings/ResonanceChapel.prefab` + `Prefabs/Moon2/Buildings/ResonanceChapel_Interior_Micro.prefab` |

### Building Definition ScriptableObjects (per Moon 1 pattern, see docs/15 §8)

```csharp
[CreateAssetMenu(menuName = "Tartaria/Moon2/BuildingDefinition")]
public class Moon2BuildingDefinition : BuildingDefinition
{
    public bool requiresMicroGiantInterior;
    public Moon2BuildingDefinition microInteriorVariant;
    public DissonanceCrystal[] internalCrystals;
    public BellRingProfile bellProfile; // null unless Bell Tower
}
```

ScriptableObject assets under `Assets/_Project/Data/Moon2/Buildings/`.

---

## 9. Mini-Game Variants — Moon 2 Specific

Moon 1 shipped Variants A-D (Frequency Slider, Waveform Trace, Harmonic Pattern, Cymatic Water). Moon 2 adds two more.

See docs/15 §9 for the variant pattern (ITuningVariant interface, dispatcher routing, scoring tiers).

### Variant E — Crystal Rotation Match

| Spec | Value |
|---|---|
| Interface | Hovering crystal mesh, 3 rotation axes (X/Y/Z) controlled by 3 player inputs |
| Goal | Rotate crystal so that internal etched glyph aligns with target overlay |
| Inputs | Left stick X = X-axis; Left stick Y = Y-axis; D-pad ←/→ = Z-axis |
| Audio | Tonal hum that shifts pitch as alignment improves — pure 432 Hz at perfect alignment |
| Time limit | 25 s |
| Tolerance tiers | ±2° = Perfect (φ multiplier), ±5° = Great (1.3×), ±10° = Good (1.0×), > ±10° = Fail (retry) |
| Implementation | `Assets/_Project/Scripts/Gameplay/Tuning/Variants/CrystalRotationMatchVariant.cs` |

### Variant F — Bell Tower Sync

| Spec | Value |
|---|---|
| Interface | A bell silhouette rocking left-right at a target frequency; player must press A in rhythm |
| Goal | Press A on each peak swing — 8 consecutive successful presses unlock the node |
| Inputs | A button only (timing-based) |
| Audio | Click-track + the bell's real sustain plays on each successful press |
| Time limit | None — but consecutive-press counter resets on miss |
| Tolerance tiers | ±80 ms = Perfect, ±150 ms = Great, ±250 ms = Good, > ±250 ms = Miss (counter reset, not fail) |
| Implementation | `Assets/_Project/Scripts/Gameplay/Tuning/Variants/BellTowerSyncVariant.cs` |

### Variant routing — Moon 2 building → Variant matrix

| Building | Node 1 | Node 2 | Node 3 |
|---|---|---|---|
| Pylon | A (carry-over) | A | A |
| Bell Tower | F (Bell Sync) | F | F |
| Resonance Chapel (exterior) | E (Crystal Rotation) | E | E |
| Resonance Chapel (micro interior) | Reverse-Cymatic | Reverse-Cymatic | Reverse-Cymatic |

Dispatcher: extend `BuildingVariantDispatcher.DispatchTuningByVariant()` (Moon 1 file) with case statements for E and F. Per Sprint 13 C.L5 `519d0c52`, dispatcher is already variant-aware — Moon 2 only adds new enum values.

---

## 10. Combat — Crystal Sentry + ResetScout Escalation

### Carry-overs

Mud Golem (Moon 1) does NOT appear in Crystalline Caverns. ResetScout (Moon 1) escalates to a stronger variant. One new enemy: Crystal Sentry.

### Crystal Sentry — new enemy

| Attribute | Value |
|---|---|
| Size | 2.0 m tall, humanoid silhouette made of black crystal |
| HP | 140 (4 Harmonic Strikes minimum) |
| Speed | 0.8× player |
| Attack | Crystal shard projectile, 25 dmg, 1.2 s windup, telegraphed by purple glow |
| Weakness | Stunned for 3 s after being hit by Resonance Pulse twice consecutively |
| Spawn trigger | When player approaches within 8 m of any uncleared Dissonance Crystal |
| Death | Crystallized fragments scatter; drops Crystal Shard ×2 (Moon 3 material) |
| Loot | Crystal Shard ×2, 5% chance for Cassian Diary Page (cosmetic — does NOT drop fragments 1-3, those are placed) |
| AI file | `Assets/_Project/Scripts/AI/Enemies/CrystalSentry.cs` |
| Prefab path | `Assets/_Project/Prefabs/Moon2/Enemies/CrystalSentry.prefab` |
| **Asset rule** | Must ship as a real FBX-backed prefab — NOT a sphere primitive (anti-regression vs MOON1_GAP P1 / MudGolem) |

### ResetScout escalation

Moon 1 ResetScout reused unchanged at Moon 2 baseline. New variant `ResetScout_Captain` appears at Chapel Crypt only — same base AI, +50% HP, drops a guaranteed Diary Fragment.

| Variant | HP | Speed | Drops |
|---|---|---|---|
| ResetScout (Moon 1 baseline) | 60 | 1.1× player | Purified Mud ×2 |
| ResetScout_Captain (Moon 2 new) | 90 | 1.1× player | Purified Mud ×3 + Diary Page (cosmetic) |

### Encounter pacing

| RS milestone | Encounter |
|---|---|
| 0 (Moon 2 start) | First Crystal Sentry at Pylon Vault tutorial |
| 50 | ResetScout patrol of 2 in Antechamber |
| 100 | Crystal Sentry + ResetScout co-engagement at Bell Chamber |
| 150 | ResetScout_Captain + 2 Crystal Sentries at Chapel Crypt entrance |
| 200 (Moon 2 complete) | No more spawns — climax cleansing wave |

Spawn budget cap: max 3 concurrent enemies in any chamber (perf budget).

---

## 11. Audio — Crystalline Overlays

### New adaptive music stems

| Stem | Path | Trigger | Behavior |
|---|---|---|---|
| Crystalline Bed | `Resources/Moon2/Audio/Music/crystalline_bed.ogg` | Cavern entry | Replaces Moon 1 ambient bed while in Moon 2 zone |
| Bell Scalar | `Resources/Moon2/Audio/Music/bell_scalar.ogg` | Bell ring event | One-shot, 8 s, layers over current bed |
| Dissonance Tinnitus | `Resources/Moon2/Audio/Music/dissonance_tinnitus.ogg` | Within 25 m of uncleared Dissonance Crystal | Looping 4000 Hz shimmer with 18 Hz infrasound rumble |
| Crystal Shatter | `Resources/Moon2/Audio/SFX/crystal_shatter.ogg` | OnCrystalShattered event | One-shot 1.2 s |
| Cassian Betrayal Sting | `Resources/Moon2/Audio/Music/cassian_doubt.ogg` | OnTrustChosen with Doubt | One-shot, descending half-step minor 3rd |
| Cassian Trust Resolution | `Resources/Moon2/Audio/Music/cassian_trust.ogg` | OnTrustChosen with Trust | One-shot, rising major 6th |

### Band overlays (per CLAUDE.md canon — Telluric 7.83 / Harmonic 432 / Celestial 528)

| Band | Moon 2 deepening |
|---|---|
| Telluric (7.83 Hz) | Sub-bass pulse triples in amplitude during Dissonance Crystal proximity — felt in haptics |
| Harmonic (432 Hz) | Bell Tower fundamental + adaptive music base unchanged |
| Celestial (528 Hz) | Aurora overlay during Bell scalar wave events — visible AND audible above 50% Moon 2 RS |

### AdaptiveMusicController extensions

```csharp
// Moon 1 file: Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs
// Moon 2 delta:
public void OnMoon2ZoneEnter() { /* swap ambient bed */ }
public void OnBellRung(int nodeIndex) { /* play bell_scalar.ogg one-shot */ }
public void OnTrustChosen(CassianTrust choice) { /* play sting per choice */ }
```

All trigger methods called from event publishers — no Update-loop polling. Per CLAUDE.md NO-DEBT.

---

## 12. Visual — Dissonance Shader + Bell-Tower Aurora

### Shader 1 — Tartaria/Dissonance

Black-angular look for Dissonance Crystals.

```hlsl
// Inputs: _NoiseTex, _DissonancePulse (0-1, animated), _BaseColor (#0A0008)
// In fragment:
half3 baseCol = _BaseColor;
half angularNoise = step(0.7, tex2D(_NoiseTex, uv).r);  // hard-edged shards
half pulseRim = pow(1.0 - dot(viewDir, normal), 4.0) * _DissonancePulse;
half3 rimColor = half3(0.4, 0.0, 0.6);  // sickly violet rim
half3 final = lerp(baseCol, rimColor, pulseRim) + angularNoise * 0.1;
return half4(final, 1.0);
```

Path: `Assets/_Project/Art/Shaders/Tartaria_Dissonance.shader`

### Shader 2 — Tartaria/BellTowerAurora

Skybox/post-process overlay activated during scalar wave events. Adds aurora-like vertical bands of `#F5E6CC` (Celestial White from Moon 1 palette) sweeping across upper sky hemisphere.

```hlsl
// Inputs: _AuroraStrength (0-1, ramped on bell ring), _SkyGradient
// Sample skybox + add aurora bands modulated by world Y and time
half auroraBand = sin(worldUV.x * 8.0 + _Time.y * 0.5) * 0.5 + 0.5;
auroraBand *= step(0.3, worldUV.y);  // only upper hemisphere
half3 auroraColor = half3(0.96, 0.90, 0.80) * auroraBand * _AuroraStrength;
finalColor.rgb += auroraColor;
```

Path: `Assets/_Project/Art/Shaders/Tartaria_BellTowerAurora.shader`

### VFX (Visual Effect Graph)

| Effect | Particle count | Trigger |
|---|---|---|
| Dissonance Aura (per crystal) | 80 | Always while crystal uncleared |
| Crystal Shatter | 200 | OnCrystalShattered |
| Bell Scalar Wave | 1 mesh (toroidal expansion) | OnBellRung — 3 per restoration |
| Pylon Activation Beam | 150 | Pylon restoration cinematic |
| Micro-Giant Transition | 300 (golden particle swirl) | Enter/Exit Micro Mode |
| Cassian Doubt Cracks | 60 (subtle screen-space cracks) | OnTrustChosen=Doubted |

---

## 13. Save Schema Update

### Schema v2 (extends Moon 1 v1)

```csharp
[Serializable]
public class SaveData
{
    public int version = 2;  // bumped from 1

    // Moon 1 fields (unchanged for backward compat)
    public float resonanceScore;
    public BuildingState[] buildings;
    public bool[] discoveredPOIs;
    public string[] playedDialogueIds;
    public float playTimeSeconds;
    public string lastSaveTimestamp;

    // Moon 2 additions
    public Moon2State moon2;
}

[Serializable]
public class Moon2State
{
    public bool moon2Unlocked;
    public bool[] dissonanceCrystalsShattered;  // length 5
    public bool pylonRestored;
    public bool bellTowerRestored;
    public bool resonanceChapelRestoredExterior;
    public bool resonanceChapelRestoredInterior;
    public bool[] diaryFragmentsFound;  // length 3
    public CassianTrust cassianTrust = CassianTrust.Unset;
    public bool microGiantUnlocked;
}
```

### Backward compatibility

Schema reader (`Moon2SaveCoordinator.cs`, renamed from `Moon1SaveCoordinator`):

```csharp
public SaveData Load(string path)
{
    var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    if (data.version == 1)
    {
        data.moon2 = new Moon2State(); // defaults — Moon 2 not yet started
        data.version = 2;
    }
    return data;
}
```

Saves from a Moon-1-only build load cleanly into a Moon 2 build with `moon2Unlocked = false`. Required for GATE 2 criterion 11.

### Auto-save triggers (Moon 2 additions)

- Dissonance Crystal shattered → save
- Diary Fragment read → save
- Trust/Doubt chosen → save (atomic write, fsync — this is THE crossover-chain pivot)
- Micro-Giant Mode entered first time → save (unlock flag)

---

## 14. GATE 2 Exit Criteria

### Mandatory (all 12 must pass)

| # | Criterion | Measurement |
|---|---|---|
| 1 | 15-minute Moon 2 demo plays intro → first restoration → first combat → first betrayal beat | Uncut play video, controller-driven |
| 2 | 60 FPS on Recommended PC (sustained 30 min) | Unity Profiler readout, no regression vs Moon 1 baseline |
| 3 | 30 FPS on Minimum PC (sustained 30 min) | Same |
| 4 | Memory ≤ 4 GB after Moon 1 + Moon 2 (cumulative) | Profiler reading at end of 30-min combined session |
| 5 | Micro-Giant Mode functional (shrink → explore inner fractal → restore at micro scale) | Functional test, behavior matrix |
| 6 | Dissonance Crystal puzzle solvable end-to-end on all 5 crystals | Functional test |
| 7 | Bell Tower scalar wave visible + audible (sky overlay + bell SFX) | Visual+audio review |
| 8 | Cassian trust/doubt arc playable from intro through Trust-or-Doubt prompt | Functional test, save flag verified |
| 9 | Crystal Sentry combat encounter functional (engage → defeat → loot drop) | Functional test |
| 10 | Adaptive music transitions Moon 1 ↔ Moon 2 cleanly (no pop, no silence gap > 250 ms) | Audio review |
| 11 | Save schema migration: a Moon-1-only save loads in a Moon 2 build with no data loss | Diff-test of save round-trip |
| 12 | No regression to Moon 1 (player can revisit Echohaven, all systems still work) | Run Moon 1 §16 criteria 1, 5-9 inside the Moon 2 build |

### Honest framing per 2026-06-04 reset

Static-grep verification is necessary but not sufficient. Greppable criteria (5, 6, 7, 8, 9, 11) require on-disk artifact verification. Runtime criteria (1, 2, 3, 4, 10, 12) require artifacts checked in:

- §14.1 — 15-minute uncut play video file path: `docs/audits/Moon2_Gate2_Playthrough.mp4`
- §14.2, §14.3 — Profiler captures: `docs/audits/Moon2_Profile_Recommended.profiler` and `..._Minimum.profiler`
- §14.4 — Memory profile log: `docs/audits/Moon2_Memory_60min.csv`
- §14.10 — Audio diff CSV: `docs/audits/Moon2_Music_Transition.csv`
- §14.12 — Moon 1 regression playthrough video: `docs/audits/Moon2_NoMoon1Regression.mp4`

No "Moon 2 done" claim is valid without BOTH (a) all 12 criteria checked AND (b) the 5 runtime artifacts above checked into the repo. This is the GATE 2 contract.

### Subjective Gate

After 1-12 pass, the team plays the Moon 2 demo and answers:

> **"Did the Trust/Doubt prompt feel like it mattered?"**
> **"Did Micro-Giant Mode feel like a real new vocabulary, not a gimmick?"**

Both unanimous yes → ship Moon 2, start Moon 3 spec. Anything else → iterate.

---

## 15. Risk Register — Moon 2 Specific

| Risk | Probability | Impact | Mitigation |
|---|---|---|---|
| Micro-Giant Mode NavMesh swap causes player fall-through | Medium | High | Pre-bake MicroNavMesh on separate physics layer; mandatory exit-glyph fallback teleporter |
| Scalar wave shader breaks on AMD GPUs (TBR tile boundaries) | Medium | Medium | Test on RX 580 minimum spec early Week 6; fallback to non-additive blend |
| Cassian trust prompt feels arbitrary | Low | High | Playtest 3-4× during Week 11; add Milo "what do you think?" prompt if disorienting |
| Dissonance audio (4000 Hz + 18 Hz dual-band) physically uncomfortable | Medium | High | Cap loop volume; respect Accessibility "reduce volatility" toggle; A/B test with 10+ playtesters before lock |
| Save schema v2 breaks Moon 1 saves | Low | Critical | Backward-compat reader (§13) + automated round-trip test in CI |
| Crystal Sentry FBX missing at ship (Moon 1 P1 regression) | Medium | High | Author FBX in tools/blender/gen_crystal_sentry.py end of Week 5; verify on disk |
| Aether sim 2.0 ms budget exceeded with 5 new sinks | Low | Medium | Pre-existing 180-m source spatial pre-filter (Moon 1 R6) already optimizes; new sinks reuse same query |
| Cassian's Reset-themes content offensive in marketing | Medium | High | Per CLAUDE.md unresolved-lore callouts §, do NOT include in marketing until sensitivity review; Moon 2 internal demos only for now |
| Player skips Diary Fragment #3, never sees Trust/Doubt prompt | Medium | Critical | Auto-snap player to fragment at Chapel restoration cinematic; cannot exit Chapel without read |
| Bell scalar wave VFX kills FPS on minimum-spec | Medium | Medium | LOD by distance; cap simultaneous waves to 1; disable on Low quality preset |
| Moon 1 Linear color-space fix (P0 R1) not done before Moon 2 starts | High | Critical | Block-list: Moon 2 work does not start until ProjectSettings color space confirmed Linear. Hard gate. |

---

## 16. Asset Budget

| Category | Count | Avg size | Total |
|---|---|---|---|
| 3D Models — Moon 2 buildings (Pylon, Bell Tower, Chapel ext + int) | 4 unique × 3 LOD = 12 | 2 MB | 24 MB |
| 3D Models — Crystal Sentry (3 LOD) | 3 | 1.5 MB | 4.5 MB |
| 3D Models — Cassian + Diary props | 4 | 1 MB | 4 MB |
| 3D Models — Cavern modular kit | 12 pieces | 1 MB | 12 MB |
| 3D Models — MicroLattice interior pieces | 6 | 0.8 MB | 4.8 MB |
| 3D Models — Dissonance Crystal | 1 + variants | 0.5 MB | 0.5 MB |
| Textures — PBR sets for above | 18 sets | 2 MB | 36 MB |
| Audio — Crystalline + Bell stems | 6 | 5 MB | 30 MB |
| Audio — Cassian voice lines | ~30 lines | 500 KB | 15 MB |
| Audio — Crystal/Bell SFX | 12 clips | 200 KB | 2.4 MB |
| Haptic profiles — Moon 2 | 8 patterns | 2 KB | 16 KB |
| Shaders — Moon 2 custom | 2 | — | — |
| VFX graphs — Moon 2 | 6 | — | — |
| **Moon 2 added build size** | | | **~133 MB** |
| **Cumulative (Moon 1 + Moon 2)** | | | **~383 MB** |

---

*Moon 2 is where the player learns that restoration was never the whole story — corruption hid inside the lattice all along. The Crystalline Caverns are the first place the game stops being about cleaning up and starts being about reckoning. Cassian is the first character whose intentions the player can be wrong about. Get that right, and Moons 3–13 have somewhere to go.*

---

**Document Status:** FINAL  
**Author:** Director Agent — Wave 8  
**Last Updated:** 2026-06-03  
**Spec Version:** 1.0
