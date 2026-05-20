| PC-04 | **Korath's Symphony** | Event | Watch Korath conduct the planetary frequency symphony | 500 AE, Giant's Baton collectible |
| PC-05 | **Veritas's Testimony** | Event | Experience "The Eternal Resonance" — Veritas's truth performance | 500 AE, Truth Crystal |
| PC-06 | **Anastasia's Moment** | Hidden | If all 13 motes found: witness 10-second solidification | 1000 AE, 50 RC, Crown Cosmetic |
| PC-07 | **Sandbox Unlock** | System | Full sandbox mode — build freely in the restored world | All zones open, infinite Aether |
| PC-08 | **New Game+** | System | Restart with carried-over cosmetics + companion trust | All cosmetics retained |

---

## Quest Completion Statistics

### Per-Moon Breakdown

| Moon | Main | Side | Hidden | Daily | Companion | Total |
|---|---|---|---|---|---|---|
| 1 | 6 | 4 | 2 | 4 (unlocked) | 2 | 18 |
| 2 | 5 | 4 | 2 | 1 (unlocked) | 2 | 14 |
| 3 | 6 | 4 | 2 | 2 (unlocked) | 1 | 15 |
| 4 | 5 | 4 | 2 | 1 (unlocked) | 2 | 14 |
| 5 | 6 | 4 | 2 | 1 (unlocked) | 2 | 15 |
| 6 | 5 | 4 | 2 | 0 | 1 | 12 |

---

## Moon 2 Companion Stories & Reactivity R7 (Cathedral / Corruption / Crystals) — Added Content

**Exclusive Moon 2 domain deliverable.** 4 meaningful companion quest arcs + physical reactivity + trust + permanent world effects for Lirael, Korath (foreshadow), Cassian, Anastasia. Fully integrated with R7 CompanionBehaviorSystem (PhysicalTellIntensity decay, ApplyPhysicalTellForBeat, WorldMutationTier, CompanionBondLevel, calendar/giant), CompanionManager hybrid bridge, CompanionDialogueArcs (12+ new Moon2 cathedral nodes with VO directions), QuestDatabaseBuilder (4 new r7_m2_* quests), CassianNPCController, LirealBehaviorSystem, and 05_CHARACTERS_DIALOGUE.md lines.

### The 4 Arcs (tied to corruption veins, living crystal cathedral geometry, "the song inverted", micro-giant interiors, 17th Hour)

1. **Lirael — Lirael's Fractured Crystal Choir (r7_m2_lirael_crystal_choir)**
   - 3 corrupted crystal nodes in cathedral. Lirael sings, player purges/tunes. 
   - Physical tells: Projection fractures (corruption proximity), solidifies + harmonic glow (success). R7 TriggerPhysicalTellForBeat(0) + LirealBehaviorSystem crystal memory boost.
   - Trust +3. Permanent: "Lirael Crystal Memory" — pre-corruption holograms +15% tuning in Moon2 cathedral + future crystals.

2. **Cassian — Cassian's Cathedral Fracture Analysis (r7_m2_cassian_cathedral_analysis)**
   - Ambiguous map choice during vein mapping in cathedral.
   - Physical tells: Calm ally lean vs violet dissonance cufflink/stance VFX (R7 redemption state).
   - Trust branch. Permanent: "Cassian Intel" — always-visible weakpoint markers on corruption (Dissonance Lens Moon2+).

3. **Korath — Korath's Stone Shadow in the Cathedral (r7_m2_korath_builder_echo)**
   - Giant echo inscription discovery in deepest crystal heart (during M2 main choir chain).
   - Physical tells: Giant silhouette projection + deep stone hum resonance (R7 physical tell + elevated lean).
   - Early trust seed. Permanent: "Korath Stone Memory" — +10% integrity + geometry recall for crystal/stone builds.

4. **Anastasia — Anastasia's Facets of the Archive (r7_m2_anastasia_crystal_archive)**
   - 17th Hour mote-share interaction among cathedral crystals (extends Golden Mote #2).
   - Physical tells: Motes orbit/interact with veins, leave permanent golden tracery, warmer caustics.
   - Trust +2. Permanent: "Anastasia Crystal Warmth" — warmer gold caustics + extra whispers + higher manifestation rate in cathedral forever.

**How Moon 2 feels alive with the party:** Every cathedral exploration, corruption purge, crystal tuning, and 17th Hour now has contextual physical tells, trust shifts, unique dialogue (new nodes in DialogueArcs), and lasting world changes. The companions don't just comment — they *change* the cathedral with you. Lirael makes it remember its songs, Cassian makes it reveal its weaknesses (or lie), Korath's shadow makes the stone feel watched by its maker, Anastasia makes the light warmer. Permanent mutations persist into later Moons. Full R7 systems wiring makes reactivity systemic, not scripted one-offs.

See CONTEXT.md (this R7 note), 05_CHARACTERS_DIALOGUE.md (new cathedral lines), 03C (expanded companion layer), and code files for implementation.

(End of Moon 2 Companion R7 section)