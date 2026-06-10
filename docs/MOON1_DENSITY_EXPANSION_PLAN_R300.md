# Moon 1-13 Density Expansion Plan — R300/R302

> **Date:** 2026-06-09 · **Owner:** NATRIX · **Status:** Plan + Phase 1+5 executed on Moon 1; batch-ship to Moons 2-13 via R171 palette swap
> **Trigger:** NATRIX directive "I WANT TO EXPAND THE DETAIL OF MOON 1 LEVEL SIGNIFICANTLY" → "ENSURE THIS PLAN IS MOON 1-13 WIDE THEN HAMMER ALL LETS FILL THIS OUT"

## R171 Unify Mandate applies here too

The R171 unify mandate ("build once, palette-swap across 13 Moons") covers density too. Every plaza density pass is one Blender prop pool + one scatter algorithm + one set of 13 palette tints. The 7 phases below apply identically to every Moon — only the palette + a few biome-specific prop swaps change.

### 13-Moon density palette (matches CLAUDE.md R272 unify table)

| # | Moon | Stone base | Accent emissive | Particle hue | Sentinel material |
|---|---|---|---|---|---|
| 1 | Echohaven (Awakening) | warm tan `#A8916F` | Aether-Gold `#FFD973` | Aether-Gold | warm stone |
| 2 | Lunar (Shadows) | cool slate `#525763` | Violet `#D98CFF` | violet mist | dark slate |
| 3 | Electric (Spark) | copper `#8C6B4D` | amber spark `#FFC050` | amber motes | copper |
| 4 | Bronze (Star Fort) | sand bronze `#99804D` | bronze bell `#C4A050` | bronze dust | bronze |
| 5 | Obsidian (Diaries) | dark glass `#2D2D38` | violet crystal `#A060FF` | violet shards | obsidian |
| 6 | Aqua Sunken | pale aqua `#7090A0` | cyan `#8CD9FF` | water mist | wet stone |
| 7 | Frost Vault (Korath) | pale frost `#C7D6EB` | Art Bible cyan `#8CD9FF` | frost crystals | ice stone |
| 8 | Aether Airship (Sky) | warm cream `#D9C79E` | Art Bible gold `#FFD973` | brass dust | brass |
| 9 | Cinder Solar | burnt umber `#73402E` | ember `#FF7030` | ember sparks | charred stone |
| 10 | Verdant Grove | moss `#527A52` | bright leaf `#A8D070` | pollen | moss stone |
| 11 | Mist Fountain | pale grey-blue `#9EADB8` | mist white `#E8F0F4` | water vapor | wet stone |
| 12 | Mirror Bell (Korath echo) | silver `#B8B8C7` | warm white `#FFF5E0` | crystalline | silver |
| 13 | Cosmic Harmony | deep indigo `#2A2A4A` | rainbow prism `#FFE0FF` | starlight | obsidian + gold |

## Authoring rule (per R171)

NO new unique props per Moon. Reuse Moon 1's 28-prop pool + 14 vegetation variants. Apply per-Moon palette to:
- Plaza scatter mat (override _BaseColor on instances)
- Banner cloth color (per accent column)
- Sentinel material
- Dust mote particle color
- Ground fog tint
- Sunshaft emissive tint
- Lantern glow color



---

## 1. The Honest Diagnosis

### What the docs say Moon 1 should be (canon)
- **Style** (`docs/32 §0`): Stylized PBR Realism — A Plague Tale + Outer Wilds + Hellblade
- **Spec** (`docs/15 §7`): 500m radius valley, 1 km² terrain, 3 hero buildings (Dome/Fountain/Spire), 6 POIs, 4 NPCs
- **Density rule from research** (`docs/research/UNITY_RPG_LEVEL_BUILDING_DEEP_DIVE_2026-06-08.md`): Lonely Mountains pattern — 1 plant per 1–2 m² near hero areas
- **Composition rule** (`docs/32 §3`): layered fg/mg/bg, 3-5 hue palette, silhouette readable at 25% scale
- **Lighting** (`docs/32 §5`): warm key + cool sky 3:1, bloom 1.1, SSAO mandatory, tonemap neutral

### What's actually in the scene (R300 audit)
| Metric | Count | Note |
|---|---|---|
| Total active GameObjects | 1778 | High |
| Active mesh renderers | 1501 | High total but spatially uneven |
| Unique meshes used | 80 | Healthy |
| Trees | 50 | Pushed to hill perimeter, **not in plaza** |
| Rocks | 221 | Same — perimeter |
| Plants / grass / weeds | 411 | Same — perimeter |
| Village buildings | 11 | Uniform `VillageHouse.prefab` cube + pyramid placeholder; not 9 spec'd unique silhouettes |
| NPCs | 23 | Mostly NPCIdleSway placeholders |
| Lights | 20 | Single Directional + ~19 point — no torches in plaza |
| Particle systems | 9 | Sparse |
| Mud Pools | 3 | ✓ |
| Lore stones | 7 | ✓ |

### What the bird's-eye shows in the central 60m × 60m plaza area
- 1 dome + spire + fountain in the middle
- ~10 small uniform pink boxes ringed at 28m radius (the "village")
- 2 visible tree silhouettes on the SW corner
- **Vast empty tan terrain** between them
- No paths, no walls, no clusters, no mud band, no broken architectural debris
- No silhouette hierarchy beyond the hero trio

### Why it reads as sparse to a player
The 700+ scattered props are 100m+ from the dome, beyond the camera's first-20-minute travel range. The player spends the **opening 15 minutes** within the central 60m plaza, and *that area is empty*.

---

## 2. Touchstone Reference Synthesis

| Game | Density rule that applies |
|---|---|
| *A Plague Tale: Innocence* | Every screen has 3 layered planes (fg/mg/bg) of clutter; foot-level debris (broken cart wheels, scattered cloth, mud, vines) every 1–2m within hero shots. Stone walls show cracks/moss/scorch. |
| *Outer Wilds* | Every zone has 3–4 elevation tiers + glow-source variety + path erosion. Sparse total but every prop tells a story. |
| *Hellblade* | 3:1 key:fill or higher; modular weathered stone kit (12 pieces); volumetric fog at horizon hides edges; particle ash/embers always present. |
| *Death Stranding* | Sparse iconic structures separated by walkable emptiness — but the emptiness is *full* of grass, rock, scattered cargo, footprints. |
| *Lonely Mountains: Downhill* | 30–50 unique foliage hand-placed at 100K+ instances. The benchmark for "indie density." |
| Tartarian/Mud-Flood photo refs | Tall ornate Tartarian dome + horse-and-cart users + visible mud band ~1m up the building base + cracked carved stone debris. |

**Pattern:** none of these games have empty plazas. The hero structure is always nested inside a layered ecology of clutter that tells the player **what happened here**.

---

## 3. The 7-Phase Expansion Plan (each phase = one hammer round)

### Phase 1 — Plaza Density Pass (THIS ROUND, highest leverage)
Within 35 m of dome center, place:
- **80-120 prop scatter instances** (reuse existing 28 props, no new authoring per R171 mandate)
- **Modular Tartarian floor tiles** — hex slabs + broken pieces carpeting plaza ground
- **6-8 stone benches** ringing the dome perimeter
- **8-12 hanging Aether-Gold lanterns** at varying heights
- **Mud band** at dome/fountain/spire bases (decal or fan-mesh)
- **6-8 wooden carts/crate stacks** at village edge
- **Cloth banners** on poles (windsway material)
- **30+ broken architectural drums** (column fragments, cracked slabs)
- **Footprint decal trail** from spawn south to dome

### Phase 2 — Village House Variety
- Author 4 unique villagehouse variants in Blender (small/med/large/ruined) — replaces the 11 uniform pink cubes
- Each variant: door with handle, shuttered window, chimney with curling smoke particle
- Each cottage: firewood stack + barrel + clothesline outside, 1 candle-lit interior window
- Reposition village ring to spec 12-15 m radius (currently uniform 28 m)

### Phase 3 — Path-to-Dome Storytelling
- Dirt path FBX from spawn (south, y=-45) to dome
- 12-15 footprint decals along path
- Half-buried Tartarian column at path midpoint (Lirael Day-25 appearance anchor)
- Mud-flood high-water mark band on one ruined wall
- Broken Reset Scout cart + clipboard + jackhammer at path junction

### Phase 4 — Vertical Composition
- 4 tall sentinel statues (giant skeleton arm sculptures) at cardinal compass points
- Star-fort fragment ruins on south horizon (low silhouette)
- Distant mountain silhouettes via skybox extension
- Hanging vines + cloth banners on dome
- Sky-shadow gobos with light cookie

### Phase 5 — Atmosphere
- Custom skybox with cloud layer + sun disc
- Ground fog 0-2m height (URP volumetric)
- Aether-Gold dust motes (particle field at 0.5/m²)
- 5-7 sunshaft GameObjects with light cookie
- Bird flock LineRenderer circling distant spire
- Distant cathedral-light decal in sky (foreshadows 17th-hour)

### Phase 6 — Audio + VFX richness
- 8-10 per-area ambient sound zones with different reverb
- Wind cloth flap audio near banners
- Distant 7.83Hz hum subwoofer bed
- Footstep variety per terrain (mud, stone, grass, tile)

### Phase 7 — Dome Interior Dressing
- Bench seating ring around the central organ
- Pipe organ with visible bellows
- 3-4 hand-carved relief panels on the walls
- Cracked floor with Aether-Gold seam glow
- Anastasia's rocker by south arch
- Candlelight dust shimmer + cobwebs

---

## 4. Acceptance Criteria (per phase)

A phase is "shipped" when:
1. **Visual proof:** Game-view screenshot showing the density change at standard 35-m hero distance
2. **Bird's-eye proof:** Top-down screenshot showing the area covered
3. **Count proof:** Scene query showing added instance count
4. **R171 compliance:** No new unique props authored (Phase 1-6); palette stays 3-5 hue; mid-poly only

---

## 5. Phase 1 — IMMEDIATE EXECUTE

Authoring script: `Assets/_Project/Scripts/Editor/Moon1PlazaDensityR300.cs`
Menu: `Tartaria/1 Build/Moon 1 Plaza Density R300`
What it does: scatters existing prop prefabs across plaza in 3 concentric rings using deterministic seed.

See screenshots in `Assets/Screenshots/R300_*.png` for before/after.
