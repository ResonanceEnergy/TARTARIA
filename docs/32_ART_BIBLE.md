# 32 — Art Bible (TARTARIA)

**Status**: LOCKED — change only via committee of one (you).
**Last update**: 2026-04-29.
**Purpose**: Single source of truth for visual direction. Every asset must defer to this doc.

---

## 1. North Star

> *"Sacred geometry made flesh in the ruins of forgotten empire."*

If a screenshot doesn't read as **Gothic + Aether + Resonance** in 2 seconds, it's wrong.

**Reference touchstones** (study these, do NOT copy):
- *Hollow Knight* — silhouette discipline, 2-3 colour focus per screen.
- *Tunic* — flat shading + chunky sacred motifs + warm-cool hue split.
- *Death Stranding* — sense of vast emptiness, sparse but iconic structures.
- *A Plague Tale* — ruined architecture with bioluminescent drama.
- *Outer Wilds* — naive low-poly with confident lighting.

**Anti-references** (avoid):
- *Generic Unity URP demo* — no committed direction.
- *Photoreal AAA* — out of solo budget.
- *Voxel/Minecraft* — wrong tone for sacred mystery.

---

## 2. Palette (Locked Hex)

### Primary (Aether spectrum)
| Role | Hex | Notes |
|---|---|---|
| Aether-Gold | `#FFD973` | Player resonance, restored buildings, healing |
| Aether-Cyan | `#8CD9FF` | Cool harmonic, water, calm |
| Aether-Violet | `#D98CFF` | High-frequency / celestial / spectral |
| Corruption-Crimson | `#C03030` | Damaged / corrupted / hostile |

### Neutrals (architecture)
| Role | Hex | Notes |
|---|---|---|
| Stone-Cold | `#6B6F75` | Untouched ruins |
| Stone-Warm | `#A8916F` | Restored / blessed |
| Mud-Dark | `#3D2E22` | Buried / unreclaimed land |
| Sky-Dawn | `#E8C39A` → `#9FB8D4` | Top-to-bottom gradient |

### Atmosphere
| Role | Hex |
|---|---|
| Fog-Day | `#C9D6E3` |
| Fog-Night | `#1A1F2E` |
| Aether-Glow | `#FFE9A0` (additive) |

**Rule**: A single shot may use no more than **3 primary hues** + neutrals. If it needs a 4th, you're trying to say two things at once.

---

## 3. Silhouette Rules

- **Tall structures** = sacred (domes, spires, fountains). Vertical lines dominate.
- **Squat structures** = mortal (homes, mills, sheds). Horizontal lines dominate.
- **Curved silhouettes** = Aether-aligned.
- **Sharp angular silhouettes** = Tartarian engineering / corruption.
- **Player silhouette** = compact triangle (head-shoulders-base). Identifiable from 50m.
- **Enemy silhouettes** = irregular, spiked, asymmetric — readable as "wrong" at a glance.

**Test**: Black silhouette pass at 100% / 50% / 25% scale. If you can't tell what it is at 25%, redesign.

---

## 4. Shader Direction

**Locked shader stack** (TARTARIA-namespaced):
1. `Tartaria/AetherVein` — emissive, animated UV scroll, fresnel rim. Used for restored architecture.
2. `Tartaria/Corruption` — desaturated PBR + crimson rim + noise overlay. Used for corrupted props.
3. `Tartaria/Restoration` — warm rim + golden particles + soft bloom. Used for healing FX.
4. `Tartaria/SpectralGhost` — additive translucency + pulse. Used for echoes/memories.

Anything else: **URP/Lit baseline**. No third-party shaders unless a `MaterialVariantSet` is justified in writing.

**Forbidden**:
- Toon shaders (wrong tonal direction)
- Wet/glossy PBR for stone (sacred = matte)
- Alpha-blended foliage (use alpha-tested)

---

## 5. Lighting Rules

- **Primary key**: warm directional sun (`#FFE9A0`, intensity 1.5).
- **Fill**: cool sky (`#9FB8D4`, intensity 0.4) via Environment Lighting.
- **Aether bounce**: APV scenarios (Dawn / Awakening / Night / Storm).
- **Realtime shadows**: Player + 1-2 hero structures only. Bake everything else.
- **Bloom threshold**: 1.1 (industry default 1.0 — slightly tighter to keep highlights heroic, not blown).
- **Tonemap**: Neutral (Filmic = too cinematic, ACES = too dark for stylized).
- **Vignette**: 0.25 max. We are not a horror game.

---

## 6. Texture Resolution Budget

| Asset class | Max res | Notes |
|---|---|---|
| Hero buildings | 2K | Player will see up close |
| Distant architecture | 1K | Trim sheets preferred |
| Props (interactable) | 1K | Reuse trim sheets where possible |
| Foliage | 512 | Atlased |
| UI icons | 256 / vector | TextMeshPro for text, no rasterized fonts |
| Decals | 512 | Footprints, scuffs, sigils |
| HDRI sky | 4K | Poly Haven puresky variants |

**No 4K textures** anywhere in TARTARIA. We sit on the GPU memory budget of a 6GB-VRAM machine.

---

## 7. Animation Rules

- **Player**: Capoeira locomotion (`ginga forward`, `ginga sideways 1`, `au`). Idle uses `ginga variation 1` for breathing motion. Total 8 clips max in base controller.
- **NPCs**: Mixamo Humanoid base, 1 idle + 1 walk per archetype.
- **Enemies**: Asymmetric, jerky timing — never smooth. Anticipation frames mandatory before any attack.
- **No Animation Rigging** until v1.0 polish — too much overhead pre-vertical-slice.
- **Root motion**: OFF for player (CharacterController drives), ON for NPCs (Mixamo defaults).

---

## 8. VFX Vocabulary

Each VFX must answer: **"What does this teach the player?"**

| VFX | Meaning | Color | Owner |
|---|---|---|---|
| `Aurora` | World is alive | Aether-Cyan + Violet | Sky |
| `DomeAwakeningBurst` | Player resonance succeeded | Aether-Gold | Building |
| `RestoreSparkle` | Object healed/repaired | Aether-Gold | Prop |
| `ShardCollect` | Resource gained | Aether-Cyan | Pickup |
| `ScanPulse` | Information revealed | Aether-Violet | Player tool |
| `CorruptionBleed` | Damage taken (future) | Corruption-Crimson | Enemy |

**Forbidden**: Generic dust puffs, smoke unless in-fiction (forge, fountain mist), film grain.

---

## 9. UI / HUD

- **Font**: Cinzel (headings), Crimson Pro (body). Both SIL OFL.
- **Min font size**: 18pt at 1080p (accessibility floor).
- **Diegetic preferred**: in-world resonance meter > floating health bar.
- **No screen-space crosshairs** — Tartaria isn't an FPS.
- **Cursor color**: Aether-Gold on dark, Stone-Cold on light.

---

## 10. Audio Design Echo (cross-ref §31)

Visual decisions must align with audio language:
- Aether-Gold visual = 432 Hz sustain.
- Aether-Cyan = 528 Hz pad.
- Corruption-Crimson = detuned drone (-30 cents off 432).
- Spectral effects = high partials only, no fundamental.

If a VFX has no associated audio cue, it is unfinished.

---

## 11. Asset-Sourcing Whitelist (per palette + style)

When pulling free assets, pick by **silhouette compatibility** first, repaint to palette in Unity:

- **Quaternius Modular Ruins** — silhouette ✓, palette → repaint to Stone-Cold/Warm.
- **KayKit Medieval Builder** — silhouette ✓, palette → repaint trim with Aether-Gold.
- **Mixamo "Eve" / "Kachujin Re Game"** — humanoid female base for Elara.
- **Poly Haven `kloofendal_43d_clear_puresky`** — current sky (compatible).
- **Sonniss GDC** — orchestral, ambient drones for music bed.

**Rejected** (looks wrong with this palette):
- Synty POLYGON kits — colour-saturated and chunky in a way that fights us.
- Most Unity Asset Store "fantasy" packs — purple/teal generic look.
- Anime-style kits — wrong tone entirely.

---

## 12. Acceptance Test (per scene)

Before a scene can ship to the vertical slice:

1. ☐ Loads in <5s on dev hardware.
2. ☐ Black silhouette pass passes at 25% scale.
3. ☐ Uses ≤3 primary hues + neutrals.
4. ☐ Hero structure visible from spawn.
5. ☐ Every interactable has a footprint VFX + audio cue.
6. ☐ No texture exceeds 2K (HDRI exempt).
7. ☐ Frame time <10ms in Game view at default Editor quality.
8. ☐ Shader stack restricted to URP/Lit + 4 Tartaria shaders.

If any box fails, fix before adding new content.

---

## 13. Change Log

| Date | Change | Reason |
|---|---|---|
| 2026-04-29 | Doc created, palette locked, shader stack frozen at 4 | Pre-vertical-slice direction lock |
