# TARTARIA Art Production Plan — 2026-05-31

> Comprehensive production plan for visual assets across all 13 Moons.
> Blender pipeline proven functional (7 FBX generated in 0.16s — see `MOON1_ART_AUDIT_2026-05-31.md`).
> This doc is the master priority queue.

## Production status snapshot

| Bucket | Assets total | Built | KayKit-covered | Needs Blender | Needs hand-author |
|---|---|---|---|---|---|
| Moon 1 buildings | 14 | 14 ✅ | (Cathedral kit 18 pieces) | 0 | 0 |
| Moon 1 props | ~30 | 7 (this session) | KayKit RPG Tools (98 FBX) | ~10 specifically Tartarian | 0 |
| Moon 1 NPCs | 8 | 10 wrappers, 6 corrupt | KayKit Adventurers covers stand-ins | 4 hero NPCs need real models | 4 hero NPCs |
| Moon 1 enemies | 2 (MudGolem, ResetScout) | 1 + procedural ResetScout | KayKit Skeletons + Rogue_Hooded for ResetScout | 0 (KayKit reskins fine) | 0 |
| Moon 1 environment | ~15 | 14 procedural in code | KayKit Forest 210 FBX + FAE | 1 (mud pool basin proper) | 0 |
| Moon 1 VFX | 9 | 9 procedural particles | Hovl Magic exists | 0 | 0 |
| Moon 1 audio | ~25 | 7 procedural + 5 imported | - | 0 | ~15 (commissioned tracks) |
| **Moon 2–13** | ~250 | ~0 real | shared kits cover ~40% | ~150 | ~60 |

## Moon 1 remaining art queue (priority order)

Each item below maps to a `tools/blender/gen_*.py` script. Items marked ✅ already shipped this session.

### Tier 1 — ship in next batch (highest impact)
1. ✅ EchohavenBrazier (DONE 2026-05-31)
2. ✅ AnastasiaRockingChair (DONE)
3. ✅ Aether Crystals E/A/D × 3 variants (DONE)
4. ✅ BobsInn (DONE — triggers Moon 2 transition)
5. ✅ TuningPedestal (DONE — Aether tuning interaction prop)
6. **MudPoolBasin** — raised stone rim around mud pool (replaces flat cylinder primitive)
7. **LoreArtifactScroll** — collectible parchment scroll on stone pedestal
8. **GiantSkeletonKey** — keystone-shaped 3-segment lore item, gold veins
9. **SkeletonRemains** — partial skeleton at Carved Stone POI (femur, ribs, skull)
10. **PipeOrganCathedral** — the canonical Moon 1 puzzle centerpiece (pipes, manuals, pedals)

### Tier 2 — Moon 1 polish (do after Tier 1)
11. **RoseWindowCymatic** — proper detailed rose window vs current Cathedral kit RoseWindow_4x4m
12. **PureWaterFont** — ornate basin with carved spout (currently primitive)
13. **CarvedStoneObelisk** — POI marker stone with Tartarian glyphs
14. **MercuryBallSpire** — already in Cathedral kit; this is a fancier hero variant
15. **MiloSatchel** — handheld lantern + satchel for companion Milo
16. **WatchCaptainArmor** — KayKit Knight reskin via Blender material baking
17. **CassianCoat** — long dark coat character (Moon 2 NPC pre-built for early scenes)

### Tier 3 — characters (long-form Blender)
18. **AnastasiaCharacter** — properly-rigged old woman in crimson, white shawl
19. **LiraelCharacter** — semi-translucent child (Aether echo)
20. **CassianCharacter** — middle-aged man with long coat
21. **ResetScoutVictorian** — alt to KayKit reskin if NATRIX wants original art

## Moon 2 (Lunar Moon — Crystal Caverns) — high-priority placeholder list
- **CrystallineCavernWall** — fractal hexagonal crystal wall segment (tileable)
- **DissonanceCrystal** (already coded as DissonanceCrystal.cs) — 3 variants: black angular, fractured red, sickly green
- **CrystalThrone** — for the dissonance corruption boss
- **CassianRevealedFigure** — Reset-sympathizer pose variant of Cassian
- **MicroGiantMarker** — pedestal/portal for the shrink mechanic

## Moon 3+ landmark anchors (1 per Moon to establish the look)
- **Moon 3 — OrphanTrainCar** (mercurial lake transit)
- **Moon 4 — DeepForgeAnvil** (giant forge prop)
- **Moon 5 — WhiteCitySpire** (the big payoff)
- **Moon 6 — LivingLibraryPodium** (knowledge spire)
- **Moon 7 — AuroralRing** (sky shrine)
- **Moon 8 — ClockworkGear** (massive citadel cog)
- **Moon 9 — StarFortBastion** (Korath's sacrifice site)
- **Moon 10 — CelestialOrrery** (observatory)
- **Moon 11 — PlanetaryNexusGlobe** (water-grid node)
- **Moon 12 — BellTowerScalarRing** (planetary grid bell)
- **Moon 13 — ThroneOfSeven** (final crescendo throne)

## Workflow

```
NATRIX:
  1. Open Blender, edit any gen_*.py to tweak geometry
  2. In Unity: Tartaria → Moon 1 → Run Blender Batch
  3. FBX appears in Assets/_Project/Models/Blender/Moon1/
  4. BlenderImportPostprocessor.cs auto-converts to URP/Lit + makes .prefab variants
  5. Drop the prefab variant into the scene
```

## Time budget estimate

- Tier 1 props (10 items): ~3 hours of script-writing per asset × 5 remaining = 15 hours
- Tier 2 polish (7 items): ~2 hours each = 14 hours
- Tier 3 characters (4 items): 10-15 hours each = 40-60 hours
- Moon 2 placeholders (5 items): ~2 hours each = 10 hours
- Moon 3-13 landmark anchors (11 items, simple): ~1.5 hours each = 16.5 hours

**Total to ship a visually-complete Moon 1: ~30 hours of scripting**
**Total to ship landmark anchors for Moon 2-13: ~26 hours**

Tonight's session added 7 assets in ~1 hour of script-writing — pace is sustainable.
