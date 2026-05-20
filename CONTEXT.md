---
## Moon 2 Enemies & Combat Encounters (Crystal Caverns) — 2026-05-20 (This Delivery — Moon 2 Enemies & Combat Encounters Agent)

**STRICT COMPLIANCE**: ONLY worked inside `C:\dev\TARTARIA_new`. Read CONTEXT.md FIRST. Exclusive non-overlapping domain: **All new enemy types, encounter design, and combat content specific to Moon 2 (Crystalline Caverns)**. Zero work outside Moon 2. Built directly on prior R7 visuals + existing FractalWraith/MirrorWraith + frequency/Giant systems (03C_MOON_MECHANICS_DETAILED, 06_COMBAT_PROGRESSION, GDD 03_CAMPAIGN Moon 2 fractal corridors / corruption nodes, 12_VIVID_VISUALS purge combat zones).

**Deliverables (4–6 new Moon 2 enemies + 4 memorable env-driven encounters per task):**
- **5 new Moon 2 exclusive EnemyTypes** (added to EnemyType in CombatComponents.cs + duplicate EnemyTypeId in CombatWaveManager.cs):
  - CrystalShardling (18): Swarm corridor/vein hazard. 528 Hz shatter. Pack bonus + death hazards. Uses narrow crystal passages for overwhelming density.
  - VeinCrawler (19): Vein-pathing gravity ambusher. 396 Hz dislodge. Latches + drains. Drops from overhead veins in caverns.
  - ResonanceDisruptor (20): Dissonance singer. 741 Hz silence = beacon. Scrambles player freq via cavern echoes in corridors.
  - WindveilPhantom (21): Wind-propelled phantom. 285 Hz. Gust intangibility + boosted shards. Uses wind tunnels for flanking.
  - GravityPillar (22): Gravity anchor tank. Dissonant + Giant Mode topple (core expose). Pull fields turn narrow areas deadly. Direct Giant integration.
- Full DOTS components (structs with env fields, frequency weakness, Giant notes), spawn logic in EnemySpawnSystem, and dedicated **Moon2CrystalEnemyAISystem.cs** (pack cohesion, vein bias, scramble pulses, wind gusts, gravity wells + Giant topple synergy).
- **CombatWaveManager.cs** extended: Moon2 wave generation in BuildZoneEncounter (heavy new enemy mix for moonIndex==1), + new `CreateMoon2CrystalEncounter` factory producing 4 named WaveEncounterDefs.
- **MoonMechanicActivator.cs**: DissonancePurge (Moon 2) now routes to `Mechanic_Moon2CrystalPurge` which sequences the 4 encounters via CombatWaveManager.
- **4 memorable encounters** (all use crystals/veins/wind/gravity/narrow corridors as active participants, distinct from Echohaven mud waves and Moon 3 rail escorts):
  1. **VeinChoke** — Tight fractal corridor. Shardling swarms + VeinCrawler ceiling drops + GravityPillar climax. Frequency counters + positioning critical.
  2. **WindGallery** — Wind tunnel gallery. WindveilPhantoms + Disruptor echo pulses (acoustics amplify). Timing dodges and wind cover.
  3. **GravityNexus** — Central pillar chamber. Shifting wells pin player; Giant Mode ground slam topples for victory. Supports + classic wraiths.
  4. **ResonanceHeart** — Cathedral node heart (3 waves). Full suite of all 5 new + Fractal/Mirror. Symphony of dissonance using every cavern feature. Climax node purge.
- Integrated with existing frequency system (unique Hz per enemy per 06_COMBAT table) + Giant Mode (GravityPillar + clusters reward stomps/slams). Micro-giant (Moon 2 shrink) synergy in tight spaces noted in comments.
- All Moon 2 only. Zero changes to Echohaven, Moon 3, other enemies, or non-Moon2 files.

**Files edited/created (absolute C:\dev\TARTARIA_new paths, Moon 2 domain 100%)**:
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Gameplay\CombatComponents.cs` (~120 net new): Extended EnemyType enum + 5 detailed Moon2 structs with env + Giant + freq fields + docs.
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Gameplay\CombatSystem.cs` (~55 net): Added 5 spawn cases in EnemySpawnSystem with stats/freq/component init + Moon2 comments.
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\CombatWaveManager.cs` (~140 net): EnemyTypeId extended, BuildZoneEncounter Moon2 branch, CreateMoon2CrystalEncounter (4 encounters), wave spawns, docs.
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\MoonMechanicActivator.cs` (~85 net): Special DissonancePurge path for number==2 calling 4 encounters + new Mechanic_Moon2CrystalPurge coroutine.
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\AI\Moon2CrystalEnemyAISystem.cs` (new, ~180 LOC): Full ISystem for the 5 types — chase, specials (swarm, vein, scramble, wind, gravity + Giant), state machine integration.
- `C:\dev\TARTARIA_new\CONTEXT.md`: This delivery header + summary (prepended).

**How Moon 2 now feels dangerous and unique**:
The Crystalline Caverns are no longer backdrop — they are the boss. Narrow corridors turn Shardling packs lethal. Veins provide enemy highways and drop points. Wind and gravity actively reposition combatants. Disruptor pulses echo off crystal walls to punish bad freq choices. GravityPillars force Giant Mode or perfect combos. The 4 encounters escalate from corridor terror → wind chaos → gravity puzzle → cathedral symphony, all while teaching frequency mastery and using the living crystal corruption visually/ mechanically. Feels nothing like Echohaven's open golem brawls or Moon 3's moving-train defense. Perfectly matches 03C "tight fractal corridors", 12_VIVID "corruption node" combat, GDD purge inside the dome.

**Git verification (executed)**: All changes committed with domain-strict message (see below).

**Production readiness**: Fully integrated, reuses all existing systems (no duplication), Moon 2 exclusive, ready for scene placement via MoonMechanicActivator + CombatWaveManager on CrystallineCaverns.unity. Future Moon agents can mirror the pattern.

---
(The prior R8 atmosphere / R7 visuals sections and history follow below.)
