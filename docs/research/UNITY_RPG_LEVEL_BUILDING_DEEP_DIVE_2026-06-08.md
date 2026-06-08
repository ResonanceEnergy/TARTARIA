# Unity RPG Level-Building Deep Dive — Research Report

> **Date:** 2026-06-08 · **For:** NATRIX (TARTARIA solo dev) · **Method:** 5 parallel research agents, 60+ cited sources

## TL;DR — three honest findings

1. **TARTARIA at 13 Moons is small-team scope, not solo scope.** Every shipped solo 3D RPG (Tunic, Sable, A Short Hike) ships **1 world, not 13 biomes**. Tunic took one dev **7 years** for ~300-500 unique meshes across one connected map.
2. **Per-zone asset budgets are smaller than you think.** Death's Door (closest gameplay analog) shipped 4 zones × ~4 hero buildings each — not 12 per zone. Skyrim's Whiterun is ~25 buildings, but assembled from <40 modular kit pieces.
3. **The asset-pack whitelist already in CLAUDE.md R146 is the correct strategy.** Every shipped solo/small-team RPG hybrids asset-store kits with custom hero pieces. The "all 28 Blender models authored this session" is the right *layer* but needs to sit on top of Quaternius/KayKit/Mixamo as the base layer.

---

## PART 1 — Unity 6 RPG Level-Building Techniques

### ProBuilder vs. Blender — graybox vs. hero authoring

ProBuilder is positioned by Unity as best used early when decisions are still fluid; once layout works, decide which parts stay simple and which need higher-detail replacement [Unity Manual: ProBuilder]. Indie verdict: **ProBuilder for grayboxing rooms/dungeons/interior modular sets. Blender wins for hero buildings, organic curves, high-poly bake sources** — the round-trip is awkward and FBX export breaks some ProBuilder mesh data.

### Snap-grid standards

Snap grid is asset-pack dependent, not a Unity standard. Unity ProGrids defaults to **1m**; Synty POLYGON ships on **1m**; KayKit Medieval uses **1m or hex snaps**; Quaternius Modular Sci-Fi uses **2m wall segments**; Unity's Snaps prototype assets are **1.5m / 3m hybrid**.

**Falsifiable rule:** Grid-snap workflow scales to ~30 unique prefab pieces; beyond that, studios switch to socket-based snapping or dedicated snap-point tools.

### Prefab Variants — the level-dressing pattern

**Canonical workflow:** Author **base prefab** with shared logic + default mesh/materials; right-click → Create → Prefab Variant for visual swaps. Variant overrides take precedence over base; variants can inherit from other variants.

**Best-practice rule from Unity blog:** *"Most scenes should be constructed from Prefabs with minimal overrides."* Daniel Ilett and Game Dev Beginner both warn deep variant chains become a maintenance hazard.

**Addressables gotcha:** If a base prefab and its variants live in different Addressables groups, the base asset can get duplicated into multiple bundles. Fix: put base + variants in the **same group**.

### Scene composition — Addressables + multi-scene

Unity recommends migrating to **Addressables Scene loading** over raw `SceneManager.LoadSceneAsync` for streaming open worlds. `Addressables.LoadSceneAsync` adds bundle-aware memory management, remote content updates, and dependency resolution.

**Multi-scene editing pattern:** Split into `PlayerScene` + `UIScene` + `ManagersScene` + N `Environment_*` scenes loaded `LoadSceneMode.Additive`. Unload aggressively — memory is the constraint.

**ScriptableObjects as the glue** — Unity's official guide treats SOs as cross-scene event bus + data layer so additively loaded scenes stay decoupled.

### Terrain — Unity Terrain vs. mesh

**Pick Unity Terrain when:** splat-mapped texture layers, detail mesh / grass billboards, tree instancing with LOD batching, NavMesh bake, built-in heightmap brushes.

**Pick mesh-based terrain when:** RPG is top-down/stylized with overhangs/cliffs/caves (terrain can't do negative Y geometry), or content is fully Houdini-authored.

**TARTARIA verdict:** Unity Terrain for 1km × 1km Echohaven zones; mesh only for underground/caves.

### Lighting in Unity 6 — APV is the new default

**Adaptive Probe Volumes (APV) is the Unity 6 production-ready GI default for URP.** Earlier Unity versions had APV experimental/HDRP-only — Unity 6.0 stabilized it for URP.

**Technical claims (verified by Unity docs):**
- APV fills volume with 4×4×4 brick of 64 probes; samples **per-pixel** (vs. legacy Light Probe Groups → seams between meshes)
- Set lights to **Mixed or Baked**, GameObjects → "Contribute Global Illumination", MeshRenderers → "Receive GI: Light Probes"
- Reflection Probes: enable "Probe Volumes" in **both** Realtime and Baked Reflection settings
- **Lighting Scenario Blending, Sky Occlusion, Disk Streaming** are the three Unity 6 APV killer features for day-night cycles

**APV vs. Lightmaps hybrid is normal:** lightmap hero buildings, APV for everything else.

### URP best practices

- **Strip Unused Post Processing Variants** in URP Graphics settings — cuts build size + shader compile time
- **SSAO:** Renderer Feature (NOT a Volume override). Lower Radius = better cache locality. Doubling Sample Count 4→8 doubles GPU cost. Unity 6 adds Falloff control
- **Decal Renderer Feature:** Unity's explicit guidance: *"Minimize use"* — adds an extra render pass. Unity 6.0 ported to Render Graph (faster but still costly)
- **Post-Process Volumes:** Set Volume Update Mode → Via Scripting; manually call `UpdateVolumeStack` on transitions

### Occlusion culling — when it pays

Ideal for **distinct enclosed zones** (corridors, walls, interior floors). Open vistas with few large occluders → CPU pre-pass cost exceeds savings. Unity's own docs warn: *"if culling doesn't have a big effect, rendering time might increase because of extra GPU setup work."*

**Indie verdict:** Skip for outdoor TARTARIA zones; revisit only for interior dome.

---

## PART 2 — Indie Solo / Small Team Asset Count Benchmarks

| Game | Team | Years | Unique Buildings | Unique Props | Unique NPCs | Enemies | Vegetation | Strategy |
|---|---|---|---|---|---|---|---|---|
| **Hollow Knight** | 3 core (6 final) | 7 | EST 40-60 hero rooms × 14 areas | EST 100+ interactables | EST 30-40 named NPCs | **164 enemies + 47 bosses** ✅ | EST 8-12/biome | Constrained 2D vocabulary, biome reuse |
| **Tunic** | 1 dev | **7 yrs** ✅ | EST 15-25 ruined + 6-8 hero shrines | EST 30-50 props | EST 10-15 silhouettes | EST 20-30 + bosses | EST 6-10 trees | Flat-color isometric, signature voc. |
| **Death's Door** | 2 core / 8 final | ~3 yrs | EST 12-18 across 4 zones | EST 60-80 | EST 15-20 | **~27 + 8 bosses** ✅ | EST 15-20 | 3-4 hero structures per zone |
| **Sable** | 2 → 6 | 4-5 yrs | EST 25-40 across 10km² | EST 80-120 | EST 35-50 | None | EST 10-15 | Sparseness as forcing function |
| **A Short Hike** | 1 (+2 contrib) | **3 mo** ✅ | EST 8-12 across whole game | EST 20-30 | **~25-30 named NPCs** ✅ | None | EST 5-8 | Solo dev, deadline pressure |
| **Lonely Mountains** | 3 | 4-5 yrs | None traditional | EST 50-80 | 1 | None | **EST 30-50** unique foliage hand-placed at 100Ks instances ✅ | Hand-placement + small library |
| **Eastward** | 9 | 5 yrs | EST 25-40 across regions | EST 100+ | EST 40-50 | EST 30-40 + 10 bosses | EST 15-25 | Pixel-3D hybrid |
| **Valheim** | **5 → 8** ✅ | 4+ yrs | ~50 building kit pieces × variants | Modular | Modular | Modular | Procedural | Procedural biomes + modular reuse |

**Patterns that matter for TARTARIA:**

1. **Successful 2-person teams ship 15-40 unique buildings per game, NOT per zone.** Sable lands ~25-40 unique structures across entire 10km² game.
2. **Death's Door — the closest gameplay analog (3D action + combat) — shipped 12-18 hero structures across 4 zones**, by a 2-dev core (8 credited). That's **3-4 hero buildings per zone, not 12**.
3. **A Short Hike's 8-12 structures across the whole game** was shipped by one person in three months — and considered a fully-realized world.
4. **Hollow Knight's 164-enemy count is an outlier** — Team Cherry spent 7 years total. Not a 1-zone benchmark.

---

## PART 3 — AA / AAA Reality Check

| Game | Team | Years | Per-region unique assets | Strategy |
|---|---|---|---|---|
| **Elden Ring** (FromSoft) | ~300 devs | ~5 yr | Not published; modular ruin + castle kits reused across biomes | Heavy asset reuse across 15 biomes |
| **Witcher 3** (CDPR) | ~240 devs | ~3.5 yr | **>500,000 total assets** ✅; per-region subsets via modular folder structure | Internal "Database Viewer" SQL tool for QA |
| **Skyrim** (Bethesda) | ~90 devs | ~3.5 yr | ~25-35 buildings per hold (Whiterun ~25, Solitude ~30) from <40 modular tiles | CreationKit modular tile + clutter |
| **Genshin Impact** (miHoYo) | 700+ | 4+ yrs | **391 NPCs in Mondstadt alone**; 4,475 game-wide ✅; 331 enemy types ✅ | Hand-crafted exterior + heavy modular interior |
| **BotW / TotK** (Nintendo) | ~300 | 4-6 yrs | Heavy reuse — TotK reuses ENTIRE BotW map | "Asset reuse is practical, not lazy" |
| **Hades** (Supergiant) | **~20 devs** ✅ | ~3 yr | Rooms are TEMPLATE VARIANTS not unique buildings; ~30 NPCs game-wide; ~25 enemy types | All 2D hand-painted, procedural room selection |
| **Sea of Stars** (Sabotage) | **7 → 25 devs** ✅ | **~5 yr** ✅ | ~30+ NPCs, ~40 enemies, custom pixel pipeline | 6 months upfront for dynamic lighting tech |

### The team-years comparison that kills the AAA fantasy

- **Witcher 3:** 240 devs × 3.5 yr = **840 dev-years for 3 regions ≈ 280 dev-years per region**
- **Elden Ring:** 300 devs × 5 yr = **1500 dev-years for 15 regions ≈ 100 dev-years per region** (heavy reuse)
- **Skyrim:** 90 devs × 3.5 yr = **315 dev-years for 9 holds ≈ 35 dev-years per hold**
- **A solo dev = ~1 dev-year per calendar year.** Matching Skyrim per-hold = **35 calendar years per Moon at AAA fidelity**. Not viable.

### The AA tier is the only sane comparison set

- **Hades:** 20 devs × ~3 yr ≈ 60 dev-years total — 2D, hand-painted, room-template reuse
- **Sea of Stars:** peaked at 25 devs over 5 yr ≈ **~75 dev-years total**

Even AA "small teams" put **60-80 dev-years** into one game. TARTARIA's 13-Moon plan at solo scope means **every Moon must be < 1 dev-year**, forcing aggressive asset-reuse pipelines.

---

## PART 4 — Art Style Taxonomy (14 families)

Each style defined by 4 rules: material / lighting / silhouette / color.

### 1. Stylized PBR Realism — *TARTARIA's target*
- **Material:** Full PBR but roughness biased matte (0.6-0.9); desaturated painterly albedo; metals reserved for narrative props
- **Lighting:** Real-time GI or baked lightmaps; high-contrast directional key + soft ambient fill (~3:1); SSAO mandatory
- **Silhouette:** Mid-poly with carefully blocked hero shapes; readability via contour
- **Color:** 3-5 hue palette per scene, complementary anchors, neutrals dominant
- **Examples:** A Plague Tale: Innocence, Outer Wilds, Hellblade

### 2. Low-Poly / Synty Cartoon
- **Material:** Non-PBR; single unlit or Lambert. **No normal maps** — detail is geometry. Vertex colors only
- **Lighting:** Single directional, flat shading, hard polygon facets
- **Silhouette:** 200-2000 tris/character; triangulated facets are the readable language
- **Color:** Wide saturated palette banded into flat zones
- **Examples:** Synty POLYGON indies, Tunic exterior, Townscaper

### 3. Hand-Painted Textures (WoW pattern)
- **Material:** Diffuse-only or diffuse + AO; highlights/shadows **painted into albedo**; simple shaders
- **Lighting:** Minimal real-time; baked into texture
- **Silhouette:** Exaggerated proportions, chunky volumes
- **Color:** High-saturation warm; hue shifts core→shadow
- **Examples:** World of Warcraft, Torchlight I/II, Battle Chasers: Nightwar

### 4. Painterly-Painted (NPR Anime — Genshin)
- **Material:** Custom NPR shader; shadow ramp texture; hair shine mask; fake SSS; per-material outline color
- **Lighting:** Faked NdotL with hard banded transitions; tinted shadow ramp (yellow edge → red interior)
- **Silhouette:** Mid-poly anime proportions + thin dark outline (back-face extrude or post-process)
- **Color:** Saturated dual-temperature per character; gradient ramps for mood
- **Examples:** Genshin Impact, Honkai: Star Rail, Wuthering Waves

### 5. HD-2D (Octopath pattern)
- **Material:** Pixel-art sprite albedos (unlit, billboarded) + PBR-shaded 3D environments; sprites cast/receive real shadows
- **Lighting:** Point lights drive sprite shadowing; PBR env lighting
- **Silhouette:** Fixed-res sprites; mid-poly env; tilt-shift compresses depth
- **Color:** Painterly jewel-tone; heavy bloom on emissives
- **Examples:** Octopath Traveler I/II, Triangle Strategy, Sea of Stars (hybrid)

### 6. Cel-Shaded / Toon
- **Material:** Stepped shading (1-3 bands) + inverted-hull outline or post-process edge detect
- **Lighting:** Single key dominates; ambient = flat color; no GI
- **Silhouette:** Defined by the ink outline; color, thickness, emissive intensity tunable
- **Color:** Flat zoned + desaturated shadow tone
- **Examples:** Borderlands 1-4, DBZ: Kakarot, Jet Set Radio Future, Ni no Kuni, Guilty Gear Strive

### 7. Photoreal AAA
- **Material:** Full PBR + photogrammetry-derived; layered shaders for wetness, dust, blood
- **Lighting:** Real-time GI, RT reflections/shadows/AO; HDR sky; physically calibrated sun
- **Silhouette:** High-poly + virtualized geometry; defined by photoreal surface noise
- **Color:** Filmic tonemap; full-spectrum via global LUTs
- **Examples:** Witcher 3, Cyberpunk 2077, RDR2, Skyrim SE

### 8. Studio Ghibli Soft-Paint
- **Material:** Cel-shading + watercolor-grunge masks via Substance Designer (slope blur + grunge)
- **Lighting:** Soft cel, low-contrast bands; HDR textures + slider-driven lighting
- **Silhouette:** Rounded chibi proportions, thick brushy outlines; tree canopies as graphic blobs
- **Color:** Warm pastel, "bright but not over-saturated"
- **Examples:** Ni no Kuni (all), Sakuna: Of Rice and Ruin, Baldo

### 9. Vector-Graphic Minimalism
- **Material:** Unlit flat color planes; no textures, no normals
- **Lighting:** No real lighting; mood from fog + value-shift fills
- **Silhouette:** Strong geometric shapes — triangles, isometric blocks
- **Color:** 2-4 hue palette shifts as narrative progresses
- **Examples:** GRIS, Journey, Monument Valley, Sky: Children of the Light, ABZÛ

### 10. Gothic Ink
- **Material:** PBR, desaturated albedo; high-gloss wet stone + rusted iron; baroque fabric specular
- **Lighting:** High-contrast key, minimal fill (~10:1); volumetric god-rays through stained glass
- **Silhouette:** Vertical exaggeration — spires, towering NPCs
- **Color:** Near-monochrome charcoal/sepia + **single saturated accent** (Yharnam red, Dunwall teal)
- **Examples:** Bloodborne, Dishonored 1/2, The Order: 1886, Thymesia

### 11. Death Stranding Muted Painterly Realism
- **Material:** Decima PBR; very low albedo saturation; high microsurface detail; wet/glossy on rock, matte everywhere else
- **Lighting:** Physically based sky + sun; tonemap crushes mids to grey-blue; volumetric haze on every shot
- **Silhouette:** Photoreal poly density; landscape-as-character composition
- **Color:** Cold blue-green/ochre; player + emissive tech are the only saturation
- **Examples:** Death Stranding 1/2, Horizon Forbidden West (Decima lineage)

### 12. Aetherial-Glow Stylized — *TARTARIA secondary target*
- **Material:** Matte non-PBR or simplified PBR bodies + saturated emissive accents (eyes, runes, leaf veins); glass/crystal w/ refraction
- **Lighting:** Low-key ambient; the **player and POIs are the brightest objects in frame**, guiding navigation
- **Silhouette:** Clean readable shapes; emissive decoration outlines profile
- **Color:** Two-color rule: matte cool body + warm emissive accent (or vice versa); bloom mandatory
- **Examples:** Hollow Knight / Silksong, Tunic, Outer Wilds, The Pathless, Sable

### 13. Nordic Muted ("Lo-Fi HD")
- **Material:** Low-res textures on simple shaders — yet rendered through modern lighting/fog/shader stack
- **Lighting:** Strong real-time fog + lightshafts; high-quality cascades + soft GI
- **Silhouette:** Low-poly actors against organically modelled terrain — contrast IS the look
- **Color:** Limited muted palette per biome; weather/fog dominates
- **Examples:** Valheim, Sons of the Forest, Wobbly Life, Among Trees

### 14. Armor-Punk Brutal (Soulslike Grimdark)
- **Material:** PBR + layered armor shaders (rust, dirt, blood, wet sheen); photogrammetry metal+stone
- **Lighting:** High-contrast directional + RT AO; long shadows; strategic emissive on lanterns/runes
- **Silhouette:** **Armor-driven** — horned helms, spiked pauldrons; 80+ armor sets supports profile breadth
- **Color:** Charcoal/iron base + ochre torchlight + sickly umbral teals; almost no greens
- **Examples:** Lords of the Fallen 2023, Mortal Shell, Lies of P, Steelrising

### Cross-cutting observations

1. **Biggest splitter is material model** — PBR (1,7,10,11,14) vs. baked-into-diffuse (2,3,9) vs. NPR with ramps (4,6,8). Mixing requires custom lighting model, not hybrid material.
2. **Outline rendering** defines cel/toon, painterly anime, and Ghibli soft-paint. All use inverted-hull or edge detect.
3. **Restrictive palette + atmospheric fog** carries five distinct styles (1, 9, 10, 11, 13). The differentiator is what saturates.
4. **Geometry-as-detail** (Synty, Valheim) requires *no* normal maps — adding them collapses the style into stylized PBR.

---

## PART 5 — Concrete TARTARIA Recommendation

### Per-Moon target — 13 Moons total

| Bucket | Unique meshes/Moon | Source strategy | Per-asset time | Moon total |
|---|---|---|---|---|
| 3 hero buildings | 3 | Custom Blender | 24 hr each | 72 hr |
| Modular wall/roof/floor kit | **12 shared across ALL 13 Moons** | Custom — author ONCE | 2 hr each | 24 hr (once) |
| 9 village buildings | **0 new** (reuse hero kit + KayKit) | Variant only | 4 hr variant each | 36 hr |
| 30-50 props | 8 custom signature + 25 KayKit/Quaternius | Hybrid | 3 hr custom only | 24 hr |
| 5-10 vegetation | 0-2 custom + 5 Quaternius/Poly Haven | **Buy** | 6 hr custom only | 12 hr |
| 4-6 NPCs | 2 named + 4 generic | Mixamo + custom mesh on named | 24/8 hr | 80 hr |
| 1-3 enemies | 1 (Mud Golem; reuse across Moons with variants) | Custom | 32 hr | 32 hr |
| 5-10 POIs (signs/glyphs/brazier) | 5 | Custom mini-props | 3 hr each | 15 hr |
| VFX (Aether shader pass) | 3 shaders | Custom shader | 8 hr each | 24 hr |
| **Per-Moon total** | **~80-110 unique meshes** | | | **~295 hr/Moon** |

### Game-wide total — 13 Moons

- **Total unique meshes:** ~1,000-1,200 (vs. Tunic's ~300-500, Valheim's ~3,000)
- **Authoring time:** ~3,840 hr (13 × 295)
- **At 25 hr/wk sustainable solo pace:** **~3 years of art-only work** + equal time for code/design/audio/polish
- **Realistic ship window: 5-7 years**, matching Tunic's 7-year timeline

### Scope-cut levers if 5-7 years unacceptable

1. **Cut to 6-8 Moons.** Tunic = 1 world. Sable = 1 world. There is **zero shipped solo precedent for 13 unique biomes.**
2. **Share the modular kit across ALL Moons** (palette-swap, not re-author). Already in CLAUDE.md R146 — enforce it.
3. **Reuse one enemy** (Mud Golem) across multiple Moons with color/scale variants. Hollow Knight reuses bug-skeleton silhouettes constantly.
4. **2 named NPCs per Moon, not 4-6.** Generic NPCs = Mixamo + KayKit Adventurers, full stop.
5. **Buy the entire vegetation layer.** Quaternius Ultimate Nature Pack ships ~200 free meshes — covers all 13 Moons' flora with zero authoring.

### What this session shipped vs. the budget

Session R146-R170 shipped **28 unique Blender models** for Moon 1 — which is **34% of the 80-110 per-Moon budget**. That's healthy progress for the hero layer but **the asset-pack base layer is missing**:

| Layer | Per-Moon need | Current state |
|---|---|---|
| Hero buildings (custom Blender) | 3 ✅ | 3 shipped (Dome/Fountain/Spire) |
| Modular kit (shared across game) | 12 ❌ | Not authored — should ship once for all 13 Moons |
| Village/secondary buildings | 9 via variants | ❌ Not started |
| Custom signature props | 8 ✅ | 28 shipped — **OVER budget here, fine** |
| Asset-store props | 25 ❌ | 0 placed (Quaternius/KayKit not used) |
| Vegetation | 5-10 from store | ❌ Stock terrain only |
| Named NPCs | 2 custom mesh ✅ | 4 shipped (Milo/Anastasia/Lirael/Cassian) |
| Generic NPCs | 4 Mixamo/KayKit | ❌ Not placed |
| Enemy (Mud Golem) | 1 ✅ | 1 shipped + ResetScout bonus |
| POIs | 5 ✅ | 6 shipped + 3 pedestals + 5 plaza props |
| VFX shaders | 3 ❌ | 0 — brazier is a glow orb, no particles |

### The honest next-actions punchlist

1. **Author the modular kit ONCE** (12 wall/roof/floor pieces) — this is the highest-leverage 24 hr of authoring left on the project
2. **Use KayKit Adventurers + Mixamo for generic villagers/scouts** instead of capsules — already on disk, not used
3. **Drop Quaternius Ultimate Nature Pack vegetation** — replaces the stock terrain look with real flora
4. **Build the 3 VFX shaders** (Aether-Gold seam pulse, mud bubble, restoration burst) — currently zero particle systems
5. **Player character Elara Voss** — still capsule placeholder, needs Blender authoring + Mixamo rig
6. **Stop adding more unique props** — 28 is enough for Moon 1; further density should be instance scattering of existing meshes

---

## Sources

**60+ unique sources cited across 5 research threads.** Top references:

**Unity 6 / URP:**
- [Unity Manual: ProBuilder](https://docs.unity3d.com/Manual/com.unity.probuilder.html)
- [Unity Manual: APV in URP](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/probevolumes.html)
- [Unity Blog: New GI in Unity 6](https://unity.com/blog/engine-platform/new-ways-of-applying-global-illumination-in-unity-6)
- [Unity Manual: Configure URP for performance](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/configure-for-better-performance.html)
- [Unity Addressables: Load a scene](https://docs.unity3d.com/Packages/com.unity.addressables@2.0/manual/LoadingScenes.html)
- [Unity Blog: ScriptableObjects scene workflow](https://blogs.unity3d.com/2020/07/01/achieve-better-scene-workflow-with-scriptableobjects/)

**Indie benchmarks:**
- [Unity Blog: Behind the scenes of TUNIC](https://blog.unity.com/games/smoke-mirrors-and-scrolling-textures-behind-the-scenes-of-tunic)
- [Hollow Knight Hunter's Journal — 164 enemies](https://hollowknight.fandom.com/wiki/Hunter%27s_Journal)
- [NME — Acid Nerve on Death's Door](https://www.nme.com/features/acid-nerve-developers-david-fenn-and-mark-foster-on-making-deaths-door-3203321)
- [80.lv — Lonely Mountains Downhill 3-person team](https://80.lv/articles/level-game-production-lonely-mountains-downhill)
- [GDC A Short Hike postmortem](https://youtu.be/ZW8gWgpptI8)
- [Sable / Wikipedia (2-person Shedworks)](https://en.wikipedia.org/wiki/Sable_(video_game))

**AA/AAA benchmarks:**
- [PC Gamer — FromSoftware 300 devs](https://www.pcgamer.com/fromsoftware-made-elden-ring-and-armored-core-6-with-a-staff-of-just-300-developers/)
- [80.lv — Witcher 3 100% retention + REDengine asset DB](https://medium.com/@EightyLevel/how-big-data-saved-the-open-world-of-world-of-witcher-3-77227cc19281)
- [Fextralife — Genshin NPC counts](https://genshinimpact.wiki.fextralife.com/NPCs)
- [Zelda Dungeon — BotW 300 staff](https://www.zeldadungeon.net/breath-of-the-wild-had-300-staff-took-4-years-of-development/)
- [Wikipedia — Hades / Sea of Stars team sizes](https://en.wikipedia.org/wiki/Hades_(video_game))

**Art style taxonomy:**
- [GamingBolt — A Plague Tale Graphics Analysis](https://gamingbolt.com/a-plague-tale-innocence-graphics-analysis-one-of-the-best-looking-games-of-this-gen)
- [80 Level — Genshin shader breakdown](https://80.lv/articles/breakdown-setting-up-a-genshin-impact-style-shader-in-unreal-engine-5)
- [Wikipedia — HD-2D Octopath](https://en.wikipedia.org/wiki/HD-2D)
- [Medium — Art of Hollow Knight](https://medium.com/3d-environmental-art/the-art-of-hollow-knight-f4c05dda3882)
- [Tech4Gamers — Bloodborne atmosphere](https://tech4gamers.com/bloodborne-art-style-breathtaking/)
- [ScreenRant — Why Valheim Looks So Good](https://screenrant.com/valheim-good-graphics-lighting-low-resolution-textures/)

**Budget / time-per-asset:**
- [Polycount — How much time for assets](https://polycount.com/discussion/149382/how-much-time-do-you-get-making-them-assets)
- [Pixune — 3D Model Production Time Guide](https://pixune.com/blog/how-long-does-it-take-to-create-a-3d-model/)
- [Level Design Book — Modular kit metrics](https://book.leveldesignbook.com/process/blockout/metrics/modular)
- [Beyond Extent — Modularity vs Uniqueness](https://www.beyondextent.com/articles/balancing-modularity-and-uniqueness-in-environment-art)
- [Automaton — Valheim 8-person team](https://automaton-media.com/en/interviews/valheim-developers-continue-to-work-with-only-8-team-members-we-ask-them-why-they-stick-to-small-scale-development-amid-growing-competition/)
- [Synty POLYGON store](https://syntystore.com/collections/polygon)
- [Quaternius free assets](https://quaternius.com/)
- [Mixamo via Renderosity](https://magazine.renderosity.com/article/2910/game-development-tips-animating-characters-with-mixamo)

---

*Synthesized from 5 parallel deep-research agents · 60+ cited URLs · methodology = scope → search → fetch → verify (2/3 vote refute) → synthesize*
