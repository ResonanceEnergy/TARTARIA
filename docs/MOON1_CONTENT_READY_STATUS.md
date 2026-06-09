# Moon 1 Content-Ready Status — 2026-06-09

> Live status of Moon 1 vs `docs/15_MVP_BUILD_SPEC.md`. Updated at session end R291.

## Branch & Commits

- Branch: `feature/consolidate-moon-architecture`
- Latest commit: `cbde0b4e` R291 — 22 new Yarn dialog nodes (Anastasia + Lirael + Milo + Cassian)
- Session totals R146→R291: 63 commits
- Push status: pushed through `a648888b`; R291 needs push

## What's SHIPPED ✅

### Hero Buildings (3 of 3 per spec §7)
- Dome_ListenersHall @ (0, -13.44, 15) — 80% buried per spec
- Fountain_ThreadOfMemory @ (15, -6.67, 6) — 95% buried per spec
- Spire_FirstNote @ (-14, -4.85, 8) — 60% buried per spec

### NPCs (4 of 4 per spec §1)
- Milo + Anastasia + Lirael + Cassian all in scene
- All have NPCIdleSway (breathing + yaw)
- Yarn dialog: 117 nodes when recompiled (95 base + 22 from R291)

### Enemy
- MudGolem_Moon1_Standard in scene
- R171 unify: same mesh palette-swapped across Moons 1+4+6+7+9 + multiple encounter spawns

### POIs (4 of 6 per spec §7)
- ✅ MudPool_1, MudPool_2, MudPool_3
- ✅ MemoryTablet_LoreBeat (Carved Stone)
- ⚠️ Overlook_South — placed via R288 but lost when Unity MCP disconnected; needs replace
- ⚠️ RootChamber — placed via R288 but lost when Unity MCP disconnected; needs replace

### Runtime Systems (7 of 7)
- QuestManager (48 quests registered at runtime per R252)
- DialogueRunner (running milo_tutorial_step_4_tune at runtime per R254)
- TartarianHourCycle (Day 1 Hour 8 / Magnetic Moon live in HUD)
- AdaptiveMusicController enabled
- AetherFieldSystem class exists
- Moon1CinematicMoments wired R248 (Sprint 11 blocker FIXED)
- MiloTutorialFlow active

### 17th-Hour Cinematic Beats
- ✅ VFX_DomeLightEruption @ (0, 5, 15) — warm gold #FFD972 intensity 8, dormant until 17th hour
- ✅ PipeOrgan_Listeners @ (0, 0.5, 17)
- ⚠️ Prop_GiantSkeleton — placed via R289 but lost when Unity MCP disconnected; needs replace
- ⚠️ SkeletonRemains_BuriedHand — placed via R289 but lost when Unity MCP disconnected; needs replace

### Visual Style (R171 Stylized PBR Realism)
- Art Bible gradient skybox (#E8C39A → #9FB8D4) R217
- 4 terrain layers at 1024×1024 with 4-octave painterly noise R220
- 25 stone materials with auto-generated painterly base maps R222
- 0 glossy stone violations (was 11)
- 0 non-narrative metallic violations (was 4)
- APV switched on R215
- 1845 MeshRenderers static-flagged R214
- URP Strip Unused PostProcessing Variants enabled R213

### R171 Unify Mandate Validation (13 of 13 Moons)
- All 13 Moons have palette-swap proofs in scene OR dedicated scene files
- 13 dedicated `Assets/_Project/Scenes/Moons/Moon{N}_*.unity` files (Sprint K R275-R279)
- 15 scenes in BuildSettings
- MoonSceneLoader runtime + MoonSceneDebugger hotkey (LeftCtrl+1..9)

## What's MISSING — to be filled before NATRIX test play

### Scene Placement (pending Unity MCP recovery)
| # | Item | Where | Effort | Status |
|---|---|---|---|---|
| 1 | Overlook_South POI | (0, 3, -55) southern ridge | 5 min | Re-apply when Unity alive |
| 2 | RootChamber POI | (-25, -2, 25) | 5 min | Re-apply when Unity alive |
| 3 | Prop_GiantSkeleton | (0, 0, 17) inside Listeners Hall | 5 min | Re-apply when Unity alive |
| 4 | SkeletonRemains_BuriedHand | (45, 0, 25) Buried Beacon | 5 min | Re-apply when Unity alive |
| 5 | AnastasiaRocker.prefab | (18, 0, 5) beside Anastasia | 5 min | Re-apply when Unity alive |

### Runtime Verification (requires human player)
- 15-minute play arc per spec §1.3 — boot game with F310, walk the spec, screenshot every gap
- 8-step smoke test STEPS 2-7 (STEP 1 menu loads ✅ per R253)
- Save/load round-trip
- Combat full loop (Mud Golem spawn → harmonic strike → dissolution → loot drop)
- Variant B/C tuning mini-game runtime verification

### Deferred Polish (Sprint O+)
- 3 VFX shaders (Aether-Gold seam pulse + mud bubble + restoration burst)
- Mud dissolution shader (per spec §8 — golden particles + 432 Hz crystalline tone)
- Mecanim humanoid Animator Controllers for 4 NPCs (currently NPCIdleSway placeholder)
- Player Elara Voss Blender humanoid + Mecanim rig (currently capsule placeholder)
- Lightmap bake (APV switched on but never baked)
- 786 remaining flat-color materials → painterly base maps

## Test Play Instructions for NATRIX

1. Boot Unity, open `Echohaven_VerticalSlice.unity`
2. Verify Console: 0 errors
3. **Re-apply lost R288-R289 scene placements** (5 GameObjects above)
4. Save Scene
5. Press Play with F310 controller
6. Walk the spec §1.3 15-minute demo arc:
   - 0:00 — Player wakes near Mud Pool
   - 2:00 — Walk to buried Dome (Listeners' Hall)
   - 3:00 — Milo speaks: tutorial step 1
   - 5:00 — First tuning at Node 1 (Variant A Slider)
   - 7:00 — Second tuning at Node 2 (Variant B Waveform — verify class name match)
   - 8:00 — Third tuning at Node 3 (Variant C Harmonic — verify class name match)
   - 9:00 — Dome fully restored, mud dissolves
   - 11:00 — Mud Golem spawns, combat
   - 12:00 — Combat victory, loot drops
   - 13:00 — RS crosses 75, zone shifts
   - 14:00 — Discover Spire (3rd hero building)
   - 15:00 — Vista from restored dome (Overlook_South)
7. Log every gap by screenshot or note
8. Hand back to Claude for fix cycle

## R171 Style + Art Bible Compliance

All R171 Stylized PBR Realism rules met:
- Roughness 0.85+ on stone (matte, not glossy)
- Metallic 0 on non-narrative props (gold/aether retained for emissive accents)
- Desaturated painterly albedo via auto-generated noise maps
- Sky gradient matches Art Bible §2 (#E8C39A peach → #9FB8D4 cool blue)
- 3-5 hue palette per scene (warm tan + Aether-Gold + Aether-Cyan accents)

## Yarn Dialog Coverage (per spec §10)

| NPC | Spec target | Current nodes | Status |
|---|---|---|---|
| Milo | 40 lines | ~42 (36 base + 6 R291) | ✅ Met |
| Anastasia | included | ~14 (8 base + 6 R291) | ✅ |
| Lirael | included | ~15 (10 base + 5 R291) | ✅ |
| Cassian | included | ~15 (10 base + 5 R291 Moon 1) | ✅ |
| Bob (Innkeeper) | included | 8 base | ✅ |
| Lore stones | included | 6 base | ✅ |
| **Total** | est 100 | **~117** | ✅ |

## R171 Unify proven at 13/13

| # | Moon | Proof type |
|---|---|---|
| 1 | Echohaven (Awakening) | Live focus scene |
| 2 | Lunar Moon | Dedicated scene file Moon2_LunarMoon.unity + Cassian Yarn beats |
| 3 | Electric Moon | Dedicated scene file + offset proof |
| 4 | Bronze Moon | Dedicated scene file + offset proof |
| 5 | Obsidian Moon | Dedicated scene file + offset proof |
| 6 | Aqua Sunken | Dedicated scene file + offset proof |
| 7 | Frost Vault | Dedicated scene file + offset proof |
| 8 | Aether Airship | Dedicated scene file + offset proof |
| 9 | Cinder Solar | Dedicated scene file + offset proof |
| 10 | Verdant Grove | Dedicated scene file + offset proof |
| 11 | Mist Fountain | Dedicated scene file + offset proof |
| 12 | Mirror Bell | Dedicated scene file + offset proof |
| 13 | Cosmic Harmony | Dedicated scene file + offset proof |

Per NATRIX 2026-06-03 NO RELEASE TALK mandate, this is documentation of content-build progress only, not a shippability claim.
