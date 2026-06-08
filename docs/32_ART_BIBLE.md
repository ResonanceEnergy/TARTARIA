# 32 — Art Bible (TARTARIA)

**Status**: LOCKED — change only via committee of one (you).
**Last update**: 2026-06-08 (R171 STYLE LOCK — Stylized PBR Realism).
**Purpose**: Single source of truth for visual direction. Every asset must defer to this doc.

---

## 0. R171 STYLE LOCK (2026-06-08, supersedes "Aetherial-Glow" direction)

Per NATRIX, post deep-research synthesis (`docs/research/UNITY_RPG_LEVEL_BUILDING_DEEP_DIVE_2026-06-08.md`, 60+ cited sources, 14-family art-style taxonomy):

**The locked style is *Stylized PBR Realism* — taxonomy family #1.** Touchstones: **A Plague Tale: Innocence + Outer Wilds + Hellblade**. NOT cartoon, NOT flat-shaded, NOT Synty cel, NOT the previous "Aetherial-Glow" framing.

### The four locked rules

| Rule | Spec |
|---|---|
| **Material** | Full PBR but **roughness biased matte (0.6-0.9)**; **desaturated painterly albedo**; **metals reserved for narrative props only** (Aether resonators, weapons, prophecy stones — not everyday wood/stone) |
| **Lighting** | Real-time GI or baked lightmaps; **high-contrast directional key + soft ambient fill (~3:1 ratio)**; **SSAO mandatory** |
| **Silhouette** | **Mid-poly** (~600-3000 verts/character, ~200-1500/prop); **carefully blocked hero shapes**; readability via **contour**, not surface noise |
| **Color** | **3-5 hue palette per scene**, complementary anchors, **neutrals dominant**; saturation reserved for Aether-band accents |

### What this changes from prior Art Bible

- **The four palette colors (Aether-Gold/Cyan/Violet/Corruption-Crimson) STAY canonical.** They become **emissive accents on PBR matte bodies** — not the dominant body color.
- **Stone is now PBR matte** (Roughness 0.85+, Metallic 0), not unlit non-PBR. Substance/hand-painted normal maps for cracks. Roughness map allowed.
- **Wood is PBR matte** (Roughness 0.75-0.95, Metallic 0).
- **Metal is reserved.** Only narrative props (Aether resonator orbs, weapon blades, prophecy ring inlays) use Metallic > 0. **No metallic floors, no metallic body armor on villagers.**
- **Hellblade's stone language is the touchstone:** weathered, matte, layered. NOT toon. NOT vertex-color-only.
- **A Plague Tale's lighting is the touchstone:** real-time GI on indoor scenes, baked lightmaps on outdoor heroes, SSAO grounding everything.

### Examples to study (do NOT copy)

- *A Plague Tale: Innocence* — **R171 primary touchstone.** Ruined architecture with PBR matte materials + bioluminescent drama. Closest shipped match.
- *Outer Wilds* — **R171 secondary touchstone.** Naive PBR low-poly with confident lighting; mid-poly characters; 3-hue palette per scene.
- *Hellblade: Senua's Sacrifice* — **R171 tertiary touchstone.** Stylized PBR character authoring, contour-readable silhouettes, restrained color.
- *Death Stranding* — sense of vast emptiness, sparse iconic structures (composition + palette reference, not material reference).
- *Outer Wilds* (again, for lighting) — confident PBR sun + bounce light.

### Anti-references (avoid)

- *Generic Unity URP demo* — no committed direction.
- *Photoreal AAA* (Skyrim, Witcher 3 PBR density) — out of solo budget, wrong material density.
- *Voxel/Minecraft* — wrong tone for sacred mystery.
- *Synty POLYGON / cartoon* — explicitly rejected.
- *Cel-shading / toon shaders* — explicitly rejected.
- *Wet/glossy PBR for stone* — sacred = matte. Roughness 0.85+.
- *Anime kits* (Genshin, HSR NPR) — wrong material model.
- *Hollow Knight cartoon* — silhouette reference only; we are 3D PBR, they are 2D non-PBR.
- *Tunic flat-shaded* — chunky sacred motif reference only; we are NOT flat-shaded.

### Texture mandate (R206-R208 honest audit findings, 2026-06-08)

**HONEST AUDIT** (978 materials scanned) found these R171 violations and fixed them:

- **4 terrain splats were Polyhaven 4K** (`brown_mud_leaves_01_diff_4k`, `gray_rocks_diff_4k`, `coast_sand_rocks_02_diff_4k`, `painted_plaster_wall_diff_4k`) — these are EXPLICITLY REJECTED by §0 R171 STYLE LOCK + the anti-references list. **REMOVED.** Replaced with 4 stylized matte solid-color terrain layers in `Assets/_Project/Textures/Stylized/` (256×256, 8m tile, subtle painterly noise per layer):
  - `Layer_Warm_Stone_Plaza` (#9F8566) — plaza floor warmth
  - `Layer_Cool_Stone_Path` (#7A736A) — path/walkway
  - `Layer_Mud_Dark` (#523D2E) — Mud Pool zones
  - `Layer_Grass_Worn` (#6B734D) — outer grass
- **35 stone materials had Roughness <0.6** (glossy stone — sacred is matte). FIXED to Roughness 0.90.
- **14 non-narrative materials had Metallic >0.1** (R171 reserves metals for narrative props only). FIXED to Metallic 0. Narrative props (gold pendants, buckles, orbs, chains, brass, smith hammer) preserved.

### Open texture work (Sprint F R225+)

- ⚠️ 839 of 978 materials are flat-color only. R171 spec says "painterly desaturated albedo" which implies textured. Future task: hand-paint base maps for 3 hero buildings + key NPCs.
- ⚠️ Stone surfaces have no normal maps yet. Per R171 "matte stone with subtle normal detail" is allowed (not glossy).
- ⚠️ Skybox is Procedural — R228 task to author custom skybox shader with Art Bible gradient.

### Reuse mandate (unified across all 13 Moons, 100% BLENDER)

Per NATRIX directive 2026-06-08: *"NO KAYKIT, NO PURCHASES, BUILD EVERYTHING WITH BLENDER."* All asset-store dependencies REJECTED. Per `CLAUDE.md §R171 UNIFY MANDATE`:

- **One modular wall/roof/floor kit, 12 pieces, authored ONCE in Blender.** Palette-swap per-Moon (warm stone Moon 1, cold stone Moon 7, glass Moon 5, metal Moon 8).
- **One Mud Golem mesh.** Reuse all 13 Moons with color/scale variants.
- **Vegetation: 15-25 plant/tree variants authored in Blender** (`Tools/blender/gen_canon/Veg_*.py`). Recolor per biome via material variants. NOT Quaternius / asset store.
- **Generic villagers: 6-8 archetypes authored in Blender** (`Tools/blender/gen_canon/Char_Villager_*.py`). NOT Mixamo / KayKit. Material variation + Mecanim retarget. Named NPCs (already 4 shipped) stay custom.
- **3 VFX shaders authored ONCE in Unity Shader Graph** (Aether-Gold seam pulse + mud bubble + restoration burst). Reused all 13 Moons.
- **Player Elara Voss authored in Blender** + custom Mecanim humanoid rig.
- **Animation clips authored in Blender** for all 6 existing characters (idle / walk / talk / hit / die). NOT Mixamo.

### Original North Star (preserved, recontextualized)

> *"Sacred geometry made flesh in the ruins of forgotten empire."*

If a screenshot doesn't read as **Gothic + Aether + Resonance** in 2 seconds, it's wrong. The R171 Stylized PBR Realism rules are HOW we achieve this North Star — not a replacement of it.

---

## 1. North Star (legacy framing, preserved)

> *"Sacred geometry made flesh in the ruins of forgotten empire."*

If a screenshot doesn't read as **Gothic + Aether + Resonance** in 2 seconds, it's wrong.

**Reference touchstones** (R171-updated):
- *A Plague Tale: Innocence* — **R171 primary touchstone** — ruined PBR architecture with bioluminescent drama.
- *Outer Wilds* — **R171 secondary** — naive PBR low-poly with confident lighting.
- *Hellblade* — **R171 tertiary** — Stylized PBR character authoring.
- *Hollow Knight* — silhouette discipline, 2-3 colour focus per screen (silhouette reference only; material model DIFFERENT — they're 2D, we're PBR).
- *Tunic* — chunky sacred motifs + warm-cool hue split (motif reference; flat-shading EXPLICITLY NOT our style).
- *Death Stranding* — sense of vast emptiness, sparse iconic structures (composition + palette).

**Anti-references** (avoid):
- *Generic Unity URP demo* — no committed direction.
- *Photoreal AAA* — out of solo budget.
- *Voxel/Minecraft* — wrong tone for sacred mystery.
- *Synty POLYGON / cartoon* — explicitly rejected (also at line 177).
- *Cel/toon shaders* — explicitly rejected (also at line 82).
- *Wet/glossy PBR for stone* — sacred = matte.
- *Anime kits* — wrong material model.

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
