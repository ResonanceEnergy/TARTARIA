# ASSET AUDIT — 2026-05-30

> NATRIX pushed back on Claude glossing over the art work to focus on logic.
> This audit is the honest accounting of what visual assets exist, what's
> planned, and what's still being faked with primitives.

---

## 1. The damning numbers

| Metric | Count |
|---|---|
| `GameObject.CreatePrimitive` calls in scripts (primitive stubs) | **87** |
| Files containing primitive stubs | **30** |
| KayKit FBX models extracted but mostly unwired | **426** |
| Hovl Magic VFX prefabs available | **76** |
| Game-owned prefabs built | **258** |
| TODO / PLACEHOLDER / STUB comments | **38** |

---

## 2. KayKit asset library — what's available

| Pack | FBX count | Use case |
|---|---|---|
| `KayKit_Forest_Nature_Pack_1.0_FREE` | 210 | Trees, bushes, rocks, foliage scatter — Moon 1 forest/forest-clearing visuals |
| `KayKit_RPGToolsBits_1.0_FREE` | 98 | Anvil, axe, blueprint, bucket, chisel, compass, drafting, file, grindstone, etc — excavation site props, NPC tool clutter |
| `KayKit_Adventurers_2.0_FREE` | 70 | Player character models + variants (Barbarian/Knight/Mage/Ranger/Rogue) |
| `KayKit_Skeletons_1.1_FREE` | 32 | Skeleton meshes — Moon 1 giant skeleton hum + buried-giant theme + giant skeleton key visuals |
| `KayKit_Character_Animations_1.1` | 16 | Animation rigs for Adventurer characters |
| **Total** | **426 FBX** | (mostly unwrapped — exists as raw FBX, not yet as `.prefab` references) |

**Hovl Studio Magic Effects Pack: 76 VFX prefabs** — AoE slashes, crystal crossfades, ground AoE, laser AoE, meteors, plexus. Available, completely unused so far.

---

## 3. Already-built game prefabs (258 total)

| Folder | Prefab count |
|---|---|
| `Prefabs/Props/` | 204 |
| `Prefabs/Characters/` | 22 (incl. 6 named NPCs: Anastasia, Cassian, Lirael, Milo, Korath, Thorne, MudGolem, Player, ShadowStalker, CrystalSentry, plus 6 KayKit adventurers + skeleton variants) |
| `Prefabs/Moon1/Cathedral/` | **18** (Archway, Column_Ornate, Dome_Segment × 8, Door_Grand, Foundation_16x16, RoseWindow, Spire_Base/Mid/Top, Wall × 2) |
| `Prefabs/VFX/` | 4 |
| `Prefabs/PowerUps/` | 3 |
| `Prefabs/Buildings/` | 3 (Echohaven_StarDome, _HarmonicFountain, _CrystalSpire) |
| `Prefabs/Collectibles/` | 2 |
| `Prefabs/Interactive/` | 1 (TuningNode) |
| `Prefabs/Enemies/` | 1 |

---

## 4. The damning primitive-stub ranking

| File | Primitive count | Severity |
|---|---|---|
| `Moon1LevelBuilder.cs` | 12 | 🔴 critical — Moon 1 village built from cubes |
| `Moon1HeroBuildingSpawner.cs` | 12 | 🔴 critical — 3 hero buildings built from cubes despite 18 Cathedral kit prefabs existing |
| `TartarianArchitectureEnhancer.cs` | 10 | 🟡 medium — env decoration uses primitives |
| `TartarianArchitectureBuilder.cs` | 7 | 🟡 medium — same |
| `Editor/PrefabGeneratorTool.cs` | 6 | ⚪ low — Editor tool, OK to use primitives during dev |
| `MudGolemAI.cs` | 6 | 🟠 high — enemy uses primitives, has `MudGolem.prefab` already built |
| `Moon1NarrativeBeats.cs` | 3 | 🟡 giant skeleton key uses stretched cube |
| `EchohavenObelisk.cs` | 3 | 🟠 obelisk built from primitives |
| `ResetScout.cs` | 3 | 🟠 Victorian goon = capsule + cube hat (already noted in playtest) |
| `Phase2Stubs.cs` | 2 | ⚪ stubs by design |
| `Moon1NPCSpawner.cs` | 2 | 🟠 NPC fallback when prefabs not assigned |
| `Moon1ExcavationSites.cs` | 2 | 🟡 dig piles built from cubes |
| `BuildingSpawner.cs` | 2 | 🟡 same |
| `PlayerRanged.cs` | 2 | 🟡 projectile = sphere |
| `TemporalWraithAI.cs` | 2 | 🟡 |
| `ResonanceDroneAI.cs` | 2 | 🟡 |
| `CrystalSentryAI.cs` | 2 | 🟡 |
| `RuntimePBRApplier.cs` | 1 | ⚪ |
| `Moon1PlayerSetup.cs` | 1 | 🟡 |
| `BuildingRestorationCeremony.cs` | 1 | 🟡 |
| **TOTAL** | **87** | |

---

## 5. The core scandal

**`Moon1HeroBuildingSpawner.cs` builds the Dome, Fountain, and Spire by stacking `PrimitiveType.Cube` calls.** Meanwhile in `Assets/_Project/Prefabs/Moon1/Cathedral/` we have:

- `Foundation_16x16m.prefab`
- `Wall_4x4m_Stone.prefab` + `Wall_Corner_4x4m.prefab`
- `Archway_4x7m.prefab`
- `Column_Ornate_6.5m.prefab`
- `Door_Grand_3x6m.prefab`
- `RoseWindow_4x4m.prefab`
- 8 × `Dome_Segment_<dir>.prefab`
- 3 × `Spire_Base / Spire_Mid_Taper / Spire_Top_MercuryBall.prefab`

= **18 cathedral kit prefabs already authored, sitting in the repo, NEVER REFERENCED.** The Hero Building Spawner has no field for them. It just builds cubes.

Same pattern with vegetation: 210 KayKit Forest Nature FBXes available, current scatter uses `GameObject.CreatePrimitive(PrimitiveType.Sphere)` calls.

Same with Hovl VFX: 76 ready-made magic effect prefabs, restoration ceremony uses runtime `ParticleSystem` setup with default sprites.

---

## 6. What "Moon 1 complete with real art" actually requires

| Task | Replaces primitives in | Source assets | Effort |
|---|---|---|---|
| **Cathedral kit wire-up** | `Moon1HeroBuildingSpawner.cs` × 12 | 18 existing Moon1/Cathedral prefabs | Medium — refactor to load by Resources.Load or prefab refs |
| **Vegetation kit wire-up** | `Moon1LevelBuilder.cs` × 12 | 210 KayKit Forest Nature FBX | High — needs prefab wrappers + scatter algorithm |
| **KayKit RPGToolsBits as excavation props** | `Moon1ExcavationSites.cs` × 2, `BuildingSpawner.cs` × 2 | 98 RPGToolsBits FBX | Medium — pick 10-15 props for dig piles |
| **Hovl VFX for restoration ceremonies** | `BuildingRestorationCeremony.cs`, `Moon1NarrativeBeats.cs` (eruption) | 76 Hovl prefabs | Low — Resources.Load by name |
| **Skeleton kit for giant skeleton key + hum prophecy** | `Moon1NarrativeBeats.cs` | 32 KayKit Skeleton FBX | Low — replace stretched cube with skeleton bone fragment |
| **MudGolem prefab refactor** | `MudGolemAI.cs` × 6 | `MudGolem.prefab` (already exists) | Low — load prefab in Awake() |
| **Reset Scout proper model** | `ResetScout.cs` × 3 | Pick an Adventurer + add hat accessory | Medium — needs custom mesh blend |
| **Player projectile from Hovl** | `PlayerRanged.cs` × 2 | "Crystals front attack.prefab" or similar | Low |
| **Enemy AI primitive cleanup** | CrystalSentry, ResonanceDrone, TemporalWraith, VoidPhantom, ShadowStalker | Mix of existing character prefabs + Hovl FX | Medium per enemy |
| **Magenta-prevention audit script** | (NEW) `tools/audits/Find-MagentaPrimitives.ps1` | — | Low (already ticketed) |

---

## 7. What this audit means for the local LLM hand-off

These art-wiring tasks are **PERFECT for the local LLM** because:

- Each task has a clear input (FBX folder or existing prefab)
- Each task has a clear output (modified .cs file that does `Resources.Load<GameObject>(...)` or holds a `[SerializeField] GameObject[]` array)
- The pattern repeats — write one wire-up, replicate to N similar files
- No deep architectural reasoning required

**The first batch of art-wiring tickets dropped this session:**

- `06_cathedral-kit-wireup.md` — replace Moon1HeroBuildingSpawner primitives with Cathedral prefabs (#1 priority)
- `07_kaykit-forest-prefab-wrapper.md` — generate prefab wrappers around all 210 Forest FBX files
- `08_hovl-vfx-restoration.md` — wire Hovl VFX into BuildingRestorationCeremony

(Tickets 01-05 from prior session still in queue — completion tracker, tutorial hints, golem loot, magenta audit script, inn rest trigger.)

---

## 8. What stays on Claude

- Driving Unity to verify art shows up correctly in-play
- Bug diagnosis when an FBX import has wrong scale / orientation / missing material
- Picking WHICH FBX to use for which slot (taste call)
- The Magenta-prevention CI gate enforcement

---

*ASSET_AUDIT_2026-05-30.md · written by Claude after NATRIX called out the art-avoidance pattern · 2026-05-30 18:50*
