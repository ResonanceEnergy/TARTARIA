# Moon 1 Art Asset Audit — 2026-05-31

> Full inventory of what's built, what's placeholder, what's missing, and what needs Blender authoring.

## ✅ BUILT — production-grade assets

### Hero buildings (3 / 3)
| Building | Prefab | Size | State |
|---|---|---|---|
| StarDome (Cathedral) | `Echohaven_StarDome.prefab` | 207 KB | Real geometry, materials, working |
| Harmonic Fountain | `Echohaven_HarmonicFountain.prefab` | 224 KB | Real geometry, materials, working |
| Crystal Spire | `Echohaven_CrystalSpire.prefab` | 220 KB | Real geometry, materials, working |

### Cathedral modular kit (18 pieces)
Foundation_16x16m, Wall_4x4m_Stone, Wall_Corner_4x4m, Door_Grand_3x6m, Column_Ornate_6.5m, Archway_4x7m, RoseWindow_4x4m, Dome_Segment_N/NE/E/SE/S/SW/W/NW, Spire_Base_2x2m, Spire_Mid_Taper, Spire_Top_MercuryBall.
**State:** all 18 pieces wired into Moon1LevelBuilder.cs via Cathedral kit prefab-first guard.

### Imported third-party packs
- KayKit Adventurers 2.0 (6 character classes + weapons)
- KayKit Character Animations 1.1 (Rig_Medium + Rig_Large, 16 FBX animation clips)
- KayKit Forest Nature 1.0 (210 FBX — trees, rocks, bushes, grass)
- KayKit RPGToolsBits 1.0 (98 FBX — anvil, hammer, blueprint, etc)
- KayKit Skeletons 1.1 (4 enemy variants)
- Fantasy Adventure Environment (cliffs, rocks, vegetation, effects — DustMotes, Fireflies, Sunshafts, RollingFog)
- Hovl Studio Magic Effects (AoE, character auras, hits, magic circles, portals)

### Materials & textures
- 12 PBR materials (Bricks075A, Ground037/054, Marble006, Metal032/047B/048A, MetalPlates006, PavingStones150, Plaster001, Rocks023, Wood063)
- 54 PNG textures
- 68 audio files (procedural SFX generated + Ambient_HarmonicChoir.wav, Ambient_Wind.wav, Building_Hum.wav, Drake Stafford 432 Hz track)

### Custom shaders (5)
AetherFlow.shader, AetherFog.shader, AetherVein.shader, AetherVeinStone.shader, ColorblindCorrection.shader
**Status:** unknown — task #71 to verify they compile. Run `Tartaria/Moon 1/Diagnose Custom Shaders` Editor menu to check.

## ⚠️ PLACEHOLDER / NEEDS WORK

### Character prefabs (10 — 6 with .corrupt siblings)
| Character | Live prefab | .corrupt sibling | State |
|---|---|---|---|
| Player | 10 KB | - | ✅ light wrapper, real CharacterController inside |
| Milo | 10 KB | - | ✅ working in scene |
| Anastasia | 10 KB | ⚠️ yes | Wrapper OK, but no real Blender model — procedural in code |
| Cassian | 10 KB | ⚠️ yes | Same as Anastasia |
| Lirael | 10 KB | ⚠️ yes | Same |
| Korath | 4 KB | ⚠️ yes | Very small — likely empty wrapper |
| Thorne | 4 KB | ⚠️ yes | Same |
| MudGolem | 24 KB | - | ✅ real geometry |
| CrystalSentry | 4 KB | ⚠️ yes | Likely placeholder |
| ShadowStalker | 4 KB | ⚠️ yes | Likely placeholder |
| ResetScout | ❌ MISSING | - | Editor menu `Build ResetScout Prefab` will generate from Char_Rogue_Hooded |

### Triage for the 6 `.corrupt` siblings
Run `Tartaria/Moon 1/Triage Corrupt Characters` — will delete `.corrupt` files when the live `.prefab` is larger.

## ❌ NEEDS BLENDER AUTHORING (script tickets below)

### Custom Tartaria buildings (not in any kit)
1. **Bob's Inn** — Moon 1 end-of-arc rest spot. Cabin-style with thatched roof, warm-glowing windows, signpost. Triggers Moon 2 transition.
2. **Anastasia's Rocking Chair** — currently procedural in code. Should be a real Blender mesh: curved rockers, slat back, oak finish.
3. **Echohaven Brazier** — currently procedural cylinder+sphere. Should be a real iron brazier: ornate basin, claw feet, fluted column.
4. **Mud Pool basin** — currently flat cylinder primitive. Should have raised stone rim, weathered texture.

### Custom resonance/lore props
5. **Aether Crystal (3 variants for mud pools)** — E/A/D tuned crystals. Faceted octahedrons with internal glow channels.
6. **Lore Artifact base** — collectible scroll/tablet model.
7. **Tuning Node pedestal** — ornate stone pillar with crystal slot on top.
8. **Skeleton Hum bones** — partial skeleton at Carved Stone POI.
9. **Giant Skeleton Key #1** — keystone-shaped lore item, gold-veined stone.

### Custom characters (long-form Blender work)
10. **Anastasia** — proper character model with rigging. Old woman in muted crimson, white shawl.
11. **Lirael** — child or young woman, semi-translucent appearance (Aether echo).
12. **Cassian** — middle-aged man, long dark coat, resonance engineer aesthetic.
13. **Reset Scout** — alternate to KayKit reskin. Victorian gentleman gone wrong: black coat, pale skin, top hat, mechanical eye implant.

## NEXT STEPS

1. Run the 5 existing Editor menus FIRST to capture all available quick wins:
   - `Build ResetScout Prefab` (uses KayKit Char_Rogue_Hooded base, Victorian tint)
   - `Triage Corrupt Characters` (auto-deletes 6 corrupt siblings)
   - `Attach KayKit Equipment` (sword/staff/bow/etc per archetype)
   - `Bind KayKit Animators` (Rig_Medium + Rig_Large controllers + 16 anim clips)
   - `Add Hero Post-State Markers` (rose-window, water column, mercury rotor child markers)
2. Then Blender scripts (in `tools/blender/`) for #1-13 above.
3. Blender→Unity FBX import settings configured by `BlenderImportPostprocessor.cs` Editor script.

## Volume estimates

Total assets needed for Moon 1 to feel finished: ~30-40 new Blender models. Most are simple/medium (brazier, chair, crystals, pedestals = 1-3 hours each). Characters are the long pole (10-15 hours each).
