# TARTARIA WORLD OF WONDER — Quest Database
## Complete Quest Catalog by Moon, Type & Reward Structure

---

> *Every quest in Tartaria serves the restoration. No fetch quests, no filler — each task deepens the world, advances the grid, or reveals buried truth.*

**Cross-References:**
- [03C_MOON_MECHANICS_DETAILED.md](03C_MOON_MECHANICS_DETAILED.md) — Moon-by-Moon mechanics & spectacles
- [03_CAMPAIGN_13_MOONS.md](03_CAMPAIGN_13_MOONS.md) — Campaign narrative arcs
- [06_COMBAT_PROGRESSION.md](06_COMBAT_PROGRESSION.md) — Combat & skill progression
- [19_ECONOMY_BALANCE.md](19_ECONOMY_BALANCE.md) — Reward values & economy tuning
- [18_PRINCESS_ANASTASIA.md](18_PRINCESS_ANASTASIA.md) — Anastasia companion quests

---

## Table of Contents

1. [Quest Design Philosophy](#quest-design-philosophy)
2. [Quest Types & Structure](#quest-types-structure)
3. [Quest Reward Reference](#quest-reward-reference)
4. [Moon 1 — Magnetic Moon Quests](#moon-1-magnetic-moon-quests)
5. [Moon 2 — Lunar Moon Quests](#moon-2-lunar-moon-quests)
6. [Moon 3 — Electric Moon Quests](#moon-3-electric-moon-quests)
7. [Moon 4 — Self-Existing Moon Quests](#moon-4-self-existing-moon-quests)
8. [Moon 5 — Overtone Moon Quests](#moon-5-overtone-moon-quests)
9. [Moon 6 — Rhythmic Moon Quests](#moon-6-rhythmic-moon-quests)
10. [Moon 7 — Resonant Moon Quests](#moon-7-resonant-moon-quests)
11. [Moon 8 — Galactic Moon Quests](#moon-8-galactic-moon-quests)
12. [Moon 9 — Solar Moon Quests](#moon-9-solar-moon-quests)
13. [Moon 10 — Planetary Moon Quests](#moon-10-planetary-moon-quests)
14. [Moon 11 — Spectral Moon Quests](#moon-11-spectral-moon-quests)
15. [Moon 12 — Crystal Moon Quests](#moon-12-crystal-moon-quests)
16. [Moon 13 — Cosmic Moon Quests](#moon-13-cosmic-moon-quests)
17. [Recurring Daily Tasks](#recurring-daily-tasks)
18. [Companion Quest Chains](#companion-quest-chains)
19. [Anastasia Hidden Quest Line](#anastasia-hidden-quest-line)
20. [Ending Variant Quest Triggers](#ending-variant-quest-triggers)
21. [Post-Campaign & Day Out of Time](#post-campaign-day-out-of-time)
22. [Quest Completion Statistics](#quest-completion-statistics)

---

## Quest Design Philosophy

### Core Principles
1. **Every quest restores something** — a structure, a connection, a memory, a relationship
2. **No pure fetch quests** — collection objectives always tie to restoration or lore
3. **Player agency over player obedience** — multiple valid approaches to every objective
4. **5-15 minute session targeting** — quests breakable into sub-steps, auto-save between steps
5. **Emotional payoff proportional to effort** — longer chains earn bigger spectacles
6. **Quests teach mechanics** — new systems introduced through guided quest progression

### Difficulty Curve
Each Moon follows a consistent internal quest difficulty ramp:
- **Week 1 (Days 1-7):** Tutorial quests (1-star), introduce Moon's new mechanic
- **Week 2 (Days 8-14):** Standard quests (2-star), combine Moon mechanic with prior skills
- **Week 3 (Days 15-21):** Challenge quests (3-star), combat escalation + moral choices
- **Week 4 (Days 22-28):** Climax quests (4-star), Moon-ending spectacle chain

---

## Quest Types & Structure

| Type | Icon | Avg Duration | Availability | Description |
|---|---|---|---|---|
| **Main Story** | 🌙 | 10-15 min | Sequential | Drives campaign narrative, unlocks zones/mechanics |
| **Side Quest** | ⭐ | 5-10 min | Unlocked per Moon | Optional restoration/lore objectives |
| **Daily Task** | 🔄 | 3-5 min | Refreshes daily | Repeatable micro-objectives for steady progression |
| **Companion** | 💬 | 8-12 min | Triggered by events | Deepens companion relationships & unlocks abilities |
| **Hidden** | 👁️ | Varies | Discovery-based | Secret quests found through exploration/interaction |
| **Climax Chain** | ⚡ | 15-20 min | Moon Day 22-28 | Multi-part spectacle sequence, Moon finale |

### Quest State Machine
```
LOCKED → AVAILABLE → ACCEPTED → IN_PROGRESS → COMPLETE → REWARDED
                                     ↓
                                  FAILED → RETRY (no penalty)
```
- No quest is permanently missable — failed quests can always be retried
- Hidden quests remain LOCKED until discovery condition met
- Climax chains are the only quests that cannot be paused mid-step

---

## Quest Reward Reference

Standard reward tiers (see [19_ECONOMY_BALANCE.md](19_ECONOMY_BALANCE.md) for full economy):

| Tier | AE (Aether) | BM (Materials) | SC (Skill Crystals) | RS (Resonance) | RC (Premium) |
|---|---|---|---|---|---|
| **Minor** (Daily) | 50-100 | 10-20 | 0-1 | +1-2% zone | 0 |
| **Standard** (Side) | 150-300 | 30-60 | 1-2 | +3-5% zone | 0-5 |
| **Major** (Main) | 400-800 | 80-150 | 2-4 | +5-10% zone | 5-15 |
| **Epic** (Climax) | 1000-2000 | 200-400 | 5-8 | +10-20% zone | 20-50 |
| **Legendary** (Hidden) | 500-1500 | 100-300 | 3-5 | +5-15% zone | 10-30 |

---

## Moon 1 — Magnetic Moon Quests
*Zone: The Buried Cathedral / First Dome*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M1-MS01 | **The First Note** | Prologue complete | Perform first resonance scan on buried structure | 100 AE, Scanning Tutorial |
| M1-MS02 | **Layers of Silence** | MS01 | Excavate 3 mud layers from cathedral entrance | 200 AE, 30 BM, Excavation Tools |
| M1-MS03 | **Echoes in Stone** | MS02 | Clear cathedral nave, discover first lore fragment | 300 AE, 50 BM, 1 SP |
| M1-MS04 | **The Dome Remembers** | MS03 | Tune cathedral dome via waveform alignment mini-game | 500 AE, 100 BM, 2 SP, +10% RS |
| M1-MS05 | **Giant's Whisper** | MS04 | Discover giant skeleton key, trigger first Giant-Mode burst | 400 AE, 80 BM, 2 SP, Giant-Mode unlock |
| M1-MS06 | **The Dome Awakening** | MS05, Day 22+ | Complete Moon 1 climax: first dome resonance broadcast | 1500 AE, 300 BM, 5 SP, +20% RS, 30 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M1-SQ01 | **Fragments of the Flood** | MS02 | Collect 5 artifacts from mud layers (newspaper scraps, toys, tools) | 200 AE, 40 BM, Lore Entry |
| M1-SQ02 | **Fox in the Vents** | Milo joined | Follow Milo's Aether vein trail to a hidden chamber | 250 AE, 50 BM, Milo Trust +1 |
| M1-SQ03 | **Blueprint Memory** | Lirael joined | Help Lirael project 3 complete blueprints over ruins | 300 AE, 60 BM, 1 SP |
| M1-SQ04 | **The Buried Bell** | MS03 | Locate and excavate the cathedral bell tower | 200 AE, 80 BM, Bell Tower Vantage |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M1-HQ01 | **The 17th Hour** | Play during 17th-hour clock alignment | Witness hidden Aether surge, scan revealed chamber | 500 AE, 1 SP, Lore: "The Hour Between" |
| M1-HQ02 | **Golden Mote #1** | Aether Scan at specific location during 17th Hour | Find Anastasia's first Golden Mote | Anastasia Event trigger, 10 RC |

---

## Moon 2 — Lunar Moon Quests
*Zone: The Shadow District / Underground Cisterns*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M2-MS01 | **Beneath the Surface** | Moon 1 complete | Enter newly revealed underground cistern network | 150 AE, Advanced Scanning |
| M2-MS02 | **Purge Protocols** | MS01 | Cleanse 3 corruption nodes using resonance purification | 300 AE, 60 BM, 1 SP |
| M2-MS03 | **The Shadow Archive** | MS02 | Discover the sunken archives, recover Pre-Flood records | 400 AE, 80 BM, 2 SP, Lore Bundle |
| M2-MS04 | **Frequency War** | MS03 | Defeat the Dissonance Conductor (first Reset Agent boss) | 600 AE, 120 BM, 3 SP, +10% RS |
| M2-MS05 | **Cistern Crescendo** | MS04, Day 22+ | Restore the Grand Cistern, purify the underground water network | 1500 AE, 300 BM, 5 SP, +20% RS, 30 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M2-SQ01 | **Drowned Memories** | MS01 | Recover 5 waterlogged journals from cistern chambers | 200 AE, 40 BM, Lore Entry |
| M2-SQ02 | **Milo's Stash** | MS02 | Help Milo retrieve his "definitely legitimate" hidden supply cache | 250 AE, 50 BM, Milo Trust +1 |
| M2-SQ03 | **Lirael's Lament** | MS03 | Accompany Lirael to a place she remembers; help her process grief | 300 AE, 1 SP, Lirael Trust +2 |
| M2-SQ04 | **Water Memory** | MS02 | Tune 3 water channels to restore flow patterns | 200 AE, 60 BM, Purification upgrade |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M2-HQ01 | **The Weeping Wall** | Scan specific cistern wall at night | Discover hidden Pre-Flood mural, decode its message | 500 AE, 1 SP, Lore: "Before the Silence" |
| M2-HQ02 | **Golden Mote #2** | Aether Scan in Shadow Archive during 17th Hour | Find Anastasia's second Golden Mote | Anastasia whisper trigger, 10 RC |

---

## Moon 3 — Electric Moon Quests
*Zone: The Abandoned Rail Network / Orphan Stations*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M3-MS01 | **Tracks in the Dark** | Moon 2 complete | Discover the first buried crystallized-Aether rail segment | 200 AE, Rail Scanning upgrade |
| M3-MS02 | **The First Echo Child** | MS01 | Find and adopt Aria at the abandoned schoolhouse | 300 AE, 60 BM, 1 SP, Aria joins |
| M3-MS03 | **Rails Remember** | MS02 | Restore 3 rail segments via frequency-alignment mini-game | 400 AE, 80 BM, 2 SP |
| M3-MS04 | **The Engineer's Son** | MS03 | Find and adopt Toren at the main terminal | 400 AE, 80 BM, 1 SP, Toren joins |
| M3-MS05 | **Crystal Locomotive** | MS04 | Reactivate the first spectral train | 600 AE, 120 BM, 3 SP, +10% RS |
| M3-MS06 | **Orphan Train Escort** | MS05, Day 22+ | Escort the grand spectral train across 3 zones, defeat Dissonance Leviathan | 2000 AE, 400 BM, 8 SP, +20% RS, 50 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M3-SQ01 | **The Fountain Girl** | MS02 | Find and adopt Syl near the frozen fountains | 300 AE, 60 BM, Syl joins |
| M3-SQ02 | **Lullaby Circuit** | 2+ children adopted | Children sing to heal 3 corrupted rail zones | 250 AE, 1 SP, Lullaby Buff unlock |
| M3-SQ03 | **Toren's Toy Wrench** | Toren joined | Help Toren find parts to repair his toy wrench (reveals it's an actual Aether tool) | 300 AE, 80 BM, Toren Trust +2 |
| M3-SQ04 | **Station Master's Log** | MS03 | Recover the station master's journal from the buried office | 200 AE, 40 BM, Lore Entry |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M3-HQ01 | **Aria's Song** | Listen to Aria hum near a specific ruin for 30 seconds | Follow the melody to a hidden Pre-Flood music box | 500 AE, 1 SP, Lore: "Songs of the Rails" |
| M3-HQ02 | **Golden Mote #3** | Aether Scan at Orphan Station during 17th Hour | Find Anastasia's third Golden Mote | Anastasia whisper, 10 RC |

---

## Moon 4 — Self-Existing Moon Quests
*Zone: The Star Fort / Precision Quarry*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M4-MS01 | **The Fort Emerges** | Moon 3 complete | Discover the star fort's geometric foundation | 250 AE, Precision Cutting tools |
| M4-MS02 | **Angles of Perfection** | MS01 | Cut 5 stone blocks to golden-ratio specifications | 400 AE, 80 BM, 2 SP |
| M4-MS03 | **The Star Pattern** | MS02 | Reconstruct the star fort's central courtyard using sacred geometry templates | 500 AE, 100 BM, 2 SP, +8% RS |
| M4-MS04 | **Fortification Protocol** | MS03 | Activate star fort defensive grid (anti-Dissonance perimeter) | 600 AE, 120 BM, 3 SP, +10% RS |
| M4-MS05 | **The Star Fort Siege** | MS04, Day 22+ | Defend the completed star fort against massive Reset Agent assault | 1800 AE, 350 BM, 6 SP, +20% RS, 40 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M4-SQ01 | **Quarry Echoes** | MS01 | Explore the precision quarry, discover giant-scale stone cutting tools | 200 AE, 60 BM, Lore Entry |
| M4-SQ02 | **Milo the Merchant** | MS02 | Help Milo set up his first "legitimate" trade post in the fort | 300 AE, 50 BM, Milo Trust +2 |
| M4-SQ03 | **Lirael's Blueprint** | MS03 | Lirael reveals the fort's original complete design — restore a hidden wing | 400 AE, 100 BM, 1 SP, Hidden Chamber |
| M4-SQ04 | **The Mason's Mark** | MS02 | Find and align 7 mason's marks hidden in the stonework | 250 AE, 40 BM, Lore: "The Builders" |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M4-HQ01 | **The Perfect Cut** | Achieve 100% precision on 3 consecutive stone blocks | Unlock the master mason's vault beneath the quarry | 600 AE, 2 SP, Legendary Tool |
| M4-HQ02 | **Golden Mote #4** | Aether Scan at Star Fort apex during 17th Hour | Find Anastasia's fourth Golden Mote | Anastasia whisper, 10 RC |

---

## Moon 5 — Overtone Moon Quests
*Zone: The White City Pavilions / Exposition Grounds*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M5-MS01 | **Overtone Alert** | Moon 4 complete | Investigate the first automatic harmonic feedback loop | 300 AE, Overtone Harvesting |
| M5-MS02 | **Hall of Industry** | MS01 | Restore White City Pavilion 1 — unlock advanced crafting | 500 AE, 100 BM, 2 SP, +8% RS |
| M5-MS03 | **Palace of Mechanics** | MS02 | Restore Pavilion 2 — discover airship dock blueprints | 500 AE, 100 BM, 2 SP, Thorne signal |
| M5-MS04 | **The Living Network** | MS03 | Build the first Grid Amplification Network (5-node ring) | 600 AE, 120 BM, 3 SP, +10% RS |
| M5-MS05 | **Central Grand Basin** | MS04 | Restore the 20-foot fountain columns — ionized aurora display | 500 AE, 100 BM, 2 SP, +5% RS |
| M5-MS06 | **Intercontinental Bridge** | MS05, Day 22+ | Complete the White City spire, trigger first intercontinental connection | 2000 AE, 400 BM, 8 SP, +20% RS, 50 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M5-SQ01 | **Hall of Agriculture** | MS02 | Restore Pavilion 3 — activate cymatic growing chambers | 400 AE, 80 BM, 1 SP, Garden System |
| M5-SQ02 | **Pavilion of Electricity** | MS03 | Restore Pavilion 4 — first 6-Band healing aura structure | 400 AE, 80 BM, 1 SP, Healing Auras |
| M5-SQ03 | **Milo's Genuine Wonder** | Milo present during Pavilion 1 restoration | Milo drops his cynical persona — explore the White City together | 250 AE, Milo Trust +3, Appraisal upgrade |
| M5-SQ04 | **Network Node Survey** | MS04 | Map 7 potential amplification node locations across the zone | 300 AE, 60 BM, Network Blueprint |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M5-HQ01 | **The Festival Projection** | Restore all 5 Pavilions | Witness holographic Pre-Flood festival replay — giants and humans celebrating | 800 AE, 2 SP, Lore: "The World Before" |
| M5-HQ02 | **Golden Mote #5** | Aether Scan at Grand Basin during 17th Hour | Find Anastasia's fifth Golden Mote | Anastasia whisper, 10 RC |

---

## Moon 6 — Rhythmic Moon Quests
*Zone: The Grand Organ Cathedral / Acoustic Ruins*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M6-MS01 | **The Silent Pipes** | Moon 5 complete | Discover the Grand Organ Cathedral buried in a mountain of silence | 300 AE, Acoustic Scanning |
| M6-MS02 | **Pipe by Pipe** | MS01 | Restore 12 organ pipes via individual frequency tuning | 500 AE, 100 BM, 2 SP |
| M6-MS03 | **The Composer's Ghost** | MS02 | Meet the spectral composer — learn music-as-weapon mechanics | 400 AE, 80 BM, 2 SP, Music Combat |
| M6-MS04 | **Requiem Rehearsal** | MS03 | Perform a 3-movement pipe organ piece that reconstructs ruined buildings | 600 AE, 120 BM, 3 SP, +10% RS |
| M6-MS05 | **The Pipe Organ Requiem** | MS04, Day 22+ | Full organ performance — music resonance rebuilds an entire district simultaneously | 2000 AE, 400 BM, 8 SP, +20% RS, 50 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M6-SQ01 | **Acoustic Archaeology** | MS01 | Map 5 acoustic hotspots in the ruins — each reveals buried chambers | 300 AE, 60 BM, 1 SP |
| M6-SQ02 | **Lirael's Voice** | MS03 | Lirael sings with the organ — unlocks her spectral harmony ability | 400 AE, Lirael Trust +3, Harmony Ability |
| M6-SQ03 | **The Broken Bellows** | MS02 | Repair the organ's Aether-powered bellows system | 250 AE, 80 BM, Organ Power +50% |
| M6-SQ04 | **Sonic Cartography** | MS04 | Use resonance echoes to map the entire underground acoustic network | 300 AE, 60 BM, Full Zone Map |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M6-HQ01 | **The Forbidden Chord** | Play a specific 3-note sequence on the organ | Discover a locked vault beneath the organ — the composer's final secret | 700 AE, 2 SP, Lore: "The Last Song" |
| M6-HQ02 | **Golden Mote #6** | Aether Scan at Organ Cathedral apex during 17th Hour | Find Anastasia's sixth Golden Mote | Anastasia whisper, 10 RC |

---

## Moon 7 — Resonant Moon Quests
*Zone: The Giant's Graveyard / Megalith Fields*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M7-MS01 | **Bones of the Earth** | Moon 6 complete | Enter the Giant's Graveyard — discover preserved giant remains | 300 AE, Giant Lore unlock |
| M7-MS02 | **Korath's Memory** | MS01 | Encounter Korath the giant echo; learn true giant history | 500 AE, 100 BM, 2 SP, Korath joins |
| M7-MS03 | **Megalith Alignment** | MS02 | Move megaliths to their original positions using Giant-Mode | 600 AE, 120 BM, 3 SP, +8% RS |
| M7-MS04 | **The Giant's Perspective** | MS03 | Extended Giant-Mode sequence — see the world as giants saw it | 500 AE, 100 BM, 2 SP, Giant-Mode upgrade |
| M7-MS05 | **Giant's Awakening** | MS04, Day 22+ | Reactivate the megalith circle — massive formation projects holographic giant civilization | 2000 AE, 400 BM, 8 SP, +20% RS, 50 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M7-SQ01 | **Stone Stories** | MS01 | Decode inscriptions on 5 megaliths — piece together giant culture | 300 AE, 60 BM, Lore Entry |
| M7-SQ02 | **Korath's Regret** | MS02 | Korath shares memories of the day the giants chose silence — companion depth | 350 AE, 1 SP, Korath Trust +2 |
| M7-SQ03 | **Milo vs. Megaliths** | MS03 | Milo attempts to appraise a megalith — comedy sequence with genuine insight | 200 AE, Milo Trust +1 |
| M7-SQ04 | **The Smallest Giant** | MS02, children adopted | Orphan echoes interact with Korath — mutual healing moment | 300 AE, 80 BM, Korath Trust +2 |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M7-HQ01 | **Giant's Lullaby** | Play Aria's melody near Korath while in Giant-Mode | Korath remembers a song from before the Flood — unique lore cinematic | 800 AE, 2 SP, Lore: "Before They Slept" |
| M7-HQ02 | **Golden Mote #7** | Aether Scan at Megalith Circle center during 17th Hour | Find Anastasia's seventh Golden Mote | Anastasia dialogue trigger, 10 RC |

---

## Moon 8 — Galactic Moon Quests
*Zone: The Airship Dock / Cloud District*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M8-MS01 | **Signal Lock** | Moon 7 complete | Thorne's signal intensifies — complete the airship dock | 300 AE, Dock Construction |
| M8-MS02 | **The Captain Arrives** | MS01 | Thorne lands — meet the grumpiest airship captain in history | 500 AE, 100 BM, 2 SP, Thorne joins |
| M8-MS03 | **Fleet Reconnaissance** | MS02 | Fly with Thorne to map 5 hidden ruin sites from the air | 600 AE, 120 BM, 3 SP, Aerial Map |
| M8-MS04 | **Airship Armada Assembly** | MS03 | Activate 3 dormant airships in the fleet hangar | 600 AE, 120 BM, 3 SP, +10% RS |
| M8-MS05 | **The Armada Launches** | MS04, Day 22+ | Full fleet launch — synchronized aerial + ground restoration of an entire region | 2000 AE, 400 BM, 8 SP, +20% RS, 50 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M8-SQ01 | **Thorne's Log** | MS02 | Read Thorne's 200-year flight log — comedy and tragedy in entries | 300 AE, 60 BM, Lore Entry, Thorne Trust +1 |
| M8-SQ02 | **Megalith Transport** | MS03 | Use airships to transport megaliths to their original positions | 400 AE, 100 BM, 1 SP |
| M8-SQ03 | **Cloud Garden** | MS02 | Discover Thorne's secret — he's been growing an Aether garden in the clouds | 350 AE, 80 BM, Thorne Trust +3 |
| M8-SQ04 | **Milo Takes Flight** | MS02, Milo Trust 5+ | Milo's first airship ride — snarky terror masking genuine wonder | 250 AE, Milo Trust +2 |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M8-HQ01 | **The Ghost Fleet** | Fly through a specific cloud bank at sunset | Discover the spectral remnants of the original Tartarian air fleet | 800 AE, 2 SP, Lore: "Wings of Gold" |
| M8-HQ02 | **Golden Mote #8** | Aether Scan at Dock apex during 17th Hour | Find Anastasia's eighth Golden Mote | Anastasia dialogue, 10 RC |

---

## Moon 9 — Solar Moon Quests
*Zone: The Ley Line Nexus / Prophecy Chamber*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M9-MS01 | **Convergent Lines** | Moon 8 complete | Discover where 7 ley lines converge — the Nexus | 400 AE, Ley Line Perception |
| M9-MS02 | **The Prophecy Wall** | MS01 | Decode the Pre-Flood prophecy inscribed at the Nexus | 500 AE, 100 BM, 2 SP, Lore Bundle |
| M9-MS03 | **Zereth's Challenge** | MS02 | Confront Zereth the Dissonant — first encounter with the anti-villain | 600 AE, 120 BM, 3 SP, Zereth Introduced |
| M9-MS04 | **Ley Line Tuning** | MS03 | Tune all 7 ley lines converging at the Nexus | 700 AE, 140 BM, 3 SP, +12% RS |
| M9-MS05 | **The Solar Flare** | MS04, Day 22+ | Ley line alignment triggers solar event — massive spectacle with branching choice | 2000 AE, 400 BM, 8 SP, +20% RS, 50 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M9-SQ01 | **Cassian's Whisper** | MS02 | Encounter Cassian — moral choice that reshapes Moon 9's prophecy | 400 AE, 1 SP, Branching Flag set |
| M9-SQ02 | **Lirael at the Nexus** | MS01, Lirael Trust 5+ | Lirael's memory fully awakens at the Nexus — major character revelation | 500 AE, 2 SP, Lirael Trust +3 |
| M9-SQ03 | **Zereth's Argument** | MS03 | Listen to Zereth's full perspective on the old harmony — moral complexity | 300 AE, 1 SP, Zereth relationship flag |
| M9-SQ04 | **Node Cartography** | MS01 | Map all ley line intersections in the zone | 350 AE, 80 BM, Full Ley Map |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M9-HQ01 | **The Zereth Paradox** | Choose specific dialogue options with Zereth | Unlock Zereth's backstory — discovering he was once a Tartarian harmony-keeper | 800 AE, 2 SP, Lore: "The Dissonant's Origin" |
| M9-HQ02 | **Golden Mote #9** | Aether Scan at Nexus center during 17th Hour | Find Anastasia's ninth Golden Mote | Anastasia conversation, 10 RC |

---

## Moon 10 — Planetary Moon Quests
*Zone: The Living Grid / Continental Network Hub*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M10-MS01 | **The Grid Breathes** | Moon 9 complete | Witness the grid reaching sentience — structures communicate independently | 400 AE, Grid Perception |
| M10-MS02 | **Continental Calibration** | MS01 | Connect 3 continental grid segments via intercontinental conduits | 600 AE, 120 BM, 3 SP, +10% RS |
| M10-MS03 | **The Grid's Voice** | MS02 | The living grid communicates directly with the player for the first time | 500 AE, 100 BM, 2 SP, Grid Dialogue |
| M10-MS04 | **Planetary Harmony** | MS03 | Achieve synchronized resonance across all connected continents | 700 AE, 140 BM, 3 SP, +12% RS |
| M10-MS05 | **The Planetary Pulse** | MS04, Day 22+ | Grid reaches critical mass — planetary-scale restoration spectacle | 2000 AE, 400 BM, 8 SP, +20% RS, 50 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M10-SQ01 | **Thorne's Patrol Routes** | MS02, Thorne joined | Establish airship patrol circuits to monitor grid health | 400 AE, 80 BM, Patrol System |
| M10-SQ02 | **Korath's Heritage** | MS01, Korath Trust 5+ | Korath visits places where giants tended the grid — deep lore | 500 AE, 2 SP, Korath Trust +3 |
| M10-SQ03 | **Grid Gardening** | MS03 | Tend 5 grid nodes that are underperforming — restoration as caretaking | 300 AE, 80 BM, 1 SP |
| M10-SQ04 | **Network Resilience** | MS02 | Reinforce 3 grid weak points before the climax | 350 AE, 100 BM, Defense Bonus |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M10-HQ01 | **The Grid's Memory** | Achieve 80% RS in 5+ zones simultaneously | The grid projects a holographic recording of the original builders | 1000 AE, 3 SP, Lore: "The Architects" |
| M10-HQ02 | **Golden Mote #10** | Aether Scan at Continental Hub during 17th Hour | Find Anastasia's tenth Golden Mote | Anastasia full conversation, 10 RC |

---

## Moon 11 — Spectral Moon Quests
*Zone: The Veil Threshold / Spirit Borderlands*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M11-MS01 | **The Veil Thins** | Moon 10 complete | Enter the Spectral Zone — boundaries between timelines blur | 400 AE, Spectral Sight |
| M11-MS02 | **Timeline Echoes** | MS01 | Witness 3 Pre-Flood events playing out in real-time overlays | 500 AE, 100 BM, 2 SP, Lore Bundle |
| M11-MS03 | **Veritas Awakens** | MS02 | Meet Veritas at the Veil — the truth-keeper who sees all timelines | 600 AE, 120 BM, 3 SP, Veritas joins |
| M11-MS04 | **Veil Navigation** | MS03 | Navigate between timelines to restore structures in both simultaneously | 700 AE, 140 BM, 3 SP, +12% RS |
| M11-MS05 | **Veil Crescendo** | MS04, Day 22+ | Stabilize the Veil — spectral and physical worlds achieve equilibrium | 2000 AE, 400 BM, 8 SP, +20% RS, 50 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M11-SQ01 | **Spirits of the Builders** | MS02 | Communicate with Pre-Flood architects through the thinned Veil | 400 AE, 80 BM, Lore Entry |
| M11-SQ02 | **Lirael's True Form** | MS03, Lirael Trust 8+ | The Veil reveals what Lirael was before becoming an echo | 500 AE, 2 SP, Lirael final trust |
| M11-SQ03 | **Milo's Past** | MS02, Milo Trust 7+ | The Veil shows Milo's origin — who was he before the Fox? | 400 AE, 2 SP, Milo Trust +3 |
| M11-SQ04 | **Zereth at the Veil** | MS03, Zereth relationship | Zereth visits the Veil — sees the consequences of his choices | 400 AE, 1 SP, Zereth relationship deepened |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M11-HQ01 | **The 14th Moon** | Stand at the exact Veil center during 17th Hour | Glimpse a hidden 14th timeline — cryptic DLC foreshadowing | 1000 AE, 3 SP, Lore: "The Hidden Moon" |
| M11-HQ02 | **Golden Mote #11** | Aether Scan at Veil center during 17th Hour | Find Anastasia's eleventh Golden Mote | Anastasia revelation, 10 RC |

---

## Moon 12 — Crystal Moon Quests
*Zone: The Planetary Bell-Tower Network / Crystalline Peaks*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M12-MS01 | **The Crystal Summit** | Moon 11 complete | Ascend to the first bell-tower peak — discover crystalline architecture | 400 AE, Crystal Ring tools |
| M12-MS02 | **Bell-Tower Tuning** | MS01 | Tune the first 3 bell-towers to ring in harmonic sequence | 600 AE, 120 BM, 3 SP, +10% RS |
| M12-MS03 | **Continental Chime** | MS02 | Connect bell-tower rings across 2 continents — synchronized resonance | 700 AE, 140 BM, 3 SP, +10% RS |
| M12-MS04 | **The Crystal Network** | MS03 | Activate the crystalline conduit system between all towers | 700 AE, 140 BM, 3 SP, +10% RS |
| M12-MS05 | **Planetary Bell Ring** | MS04, Day 22+ | All bell-towers ring simultaneously — global resonance event | 2000 AE, 400 BM, 8 SP, +20% RS, 50 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M12-SQ01 | **The Bell-Keeper's Legacy** | MS01 | Discover the last bell-keeper's journal — centuries of solitary duty | 400 AE, 80 BM, Lore Entry |
| M12-SQ02 | **Korath's Mountain** | MS02, Korath Trust 7+ | Korath remembers the bell-towers when giants rang them | 500 AE, 2 SP, Korath Trust +3 |
| M12-SQ03 | **Crystal Harmonics** | MS03 | Tune 5 crystal formations in the peaks — each reveals a hidden chamber | 350 AE, 100 BM, 1 SP |
| M12-SQ04 | **Thorne's Dedication** | MS02, Thorne Trust 7+ | Thorne reveals why he kept flying — his promise to the last builder | 500 AE, 2 SP, Thorne final trust |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M12-HQ01 | **The Perfect Ring** | Achieve 100% tuning on all bell-towers in one session | All bells ring the lost chord — the universe responds | 1200 AE, 3 SP, Lore: "The Sound Before Time" |
| M12-HQ02 | **Golden Mote #12** | Aether Scan at tallest bell-tower during 17th Hour | Find Anastasia's twelfth Golden Mote | Anastasia near-solid, 10 RC |

---

## Moon 13 — Cosmic Moon Quests
*Zone: The True Timeline / Convergence Point*

### Main Story Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M13-MS01 | **Timeline Convergence** | Moon 12 complete | Enter the Convergence Point — all timelines visible simultaneously | 500 AE, Timeline Perception |
| M13-MS02 | **The Final Truth** | MS01 | Learn the complete truth about the Mud Flood and Tartaria's fall | 600 AE, 120 BM, 3 SP, Full Lore |
| M13-MS03 | **Zereth's Last Stand** | MS02 | Final confrontation with Zereth — combat + dialogue, outcome determined by choices | 800 AE, 160 BM, 4 SP, Zereth Resolution |
| M13-MS04 | **The Eternal Resonance** | MS03 | Activate the final ley line node — begin the planetary convergence | 800 AE, 160 BM, 4 SP, +15% RS |
| M13-MS05 | **True Timeline Convergence** | MS04, Day 22+ | Ultimate spectacle — all timelines merge, global grid reaches 100%, all companions present | 3000 AE, 600 BM, 10 SP, +100% RS, 100 RC |

### Side Quests

| ID | Quest Name | Prerequisite | Objective | Reward |
|---|---|---|---|---|
| M13-SQ01 | **Companion Reflections** | MS02 | Each companion shares their final thoughts before convergence | 500 AE, 2 SP, All Trust bonuses |
| M13-SQ02 | **The Builder's Name** | MS02 | Discover who built the first Tartarian structure — the origin of everything | 600 AE, 3 SP, Lore: "The First Builder" |
| M13-SQ03 | **Zereth's Redemption** | MS03, Zereth relationship 5+ | If enough relationship built, Zereth defects — alternate resolution | 800 AE, 3 SP, Zereth joins (variant) |
| M13-SQ04 | **The 17th Hour Vigil** | MS04 | Stand vigil at the Convergence Point through the final 17th Hour | 500 AE, 2 SP, Lore: "Eternity" |

### Hidden Quests

| ID | Quest Name | Discovery Trigger | Objective | Reward |
|---|---|---|---|---|
| M13-HQ01 | **The True Name** | Collect all 13 Golden Motes + complete all lore entries | The world speaks your name — ultimate hidden ending variation | 2000 AE, 5 SP, 50 RC, Special Ending |
| M13-HQ02 | **Golden Mote #13** | Aether Scan at Convergence apex during 17th Hour | Find Anastasia's final Golden Mote — triggers solidification countdown | Solidification event, 10 RC |

---

## Recurring Daily Tasks

Available every Moon after unlock, refreshing each in-game day:

| ID | Task Name | Unlock | Objective | Reward |
|---|---|---|---|---|
| DT-01 | **Aether Harvest** | Moon 1 | Harvest Aether from 3 sources in any zone | 75 AE |
| DT-02 | **Zone Patrol** | Moon 1 | Clear 5 Dissonance pockets in any restored zone | 50 AE, 15 BM |
| DT-03 | **Tuning Rounds** | Moon 1 | Retune 2 structures whose RS has drifted | 60 AE, +2% RS |
| DT-04 | **Companion Chat** | Moon 1 | Speak with a companion — unique daily dialogue line | 30 AE, Trust +0.5 |
| DT-05 | **Blueprint Sketch** | Moon 2 | Photograph a ruin for Lirael's reconstruction archive | 50 AE, 10 BM |
| DT-06 | **Rail Maintenance** | Moon 3 | Retune 1 rail segment showing frequency drift | 60 AE, 20 BM |
| DT-07 | **Children's Song** | Moon 3 | Listen to an orphan echo sing — zone healing bonus | 40 AE, +1% zone RS |
| DT-08 | **Precision Practice** | Moon 4 | Cut 3 perfect stone blocks (100% golden-ratio accuracy) | 50 AE, 30 BM |
| DT-09 | **Network Monitor** | Moon 5 | Check 3 grid amplification nodes for stability | 60 AE, 20 BM |
| DT-10 | **Aerial Survey** | Moon 8 | Fly one patrol route with Thorne | 80 AE, 20 BM |
| DT-11 | **Bell Tuning** | Moon 12 | Retune 1 bell-tower in the crystal network | 80 AE, +2% RS |
| DT-12 | **Feed the Grid** | Moon 10 | Donate excess Aether to a node below 50% RS | 0 AE (donated), +3% RS, 1 SP |

---

## Companion Quest Chains

Each companion has a **5-quest loyalty chain** that unlocks across the campaign:

### Milo (Fox Spirit) — "The Honest Thief"

| Step | Moon | Quest Name | Trigger | Reward |
|---|---|---|---|---|
| 1 | 1 | Fox in the Vents | Milo joined | Milo Trust +1, Hidden Chamber |
| 2 | 2 | Milo's Stash | Moon 2 started | Milo Trust +1, Supply Cache |
| 3 | 4 | Milo the Merchant | Star Fort built | Milo Trust +2, Trade Post |
| 4 | 5 | Milo's Genuine Wonder | White City restored | Milo Trust +3, Appraisal 2× |
| 5 | 11 | Milo's Past | Veil opened | Milo Trust +3, Origin Lore, Final Ability: Aether Sight |

### Lirael (Spectral Architect) — "The Memory Weaver"

| Step | Moon | Quest Name | Trigger | Reward |
|---|---|---|---|---|
| 1 | 1 | Blueprint Memory | Lirael joined | Lirael Trust +1, Blueprint Projection |
| 2 | 2 | Lirael's Lament | Moon 2 mid-point | Lirael Trust +2, Emotional Depth |
| 3 | 6 | Lirael's Voice | Organ Cathedral | Lirael Trust +3, Harmony Ability |
| 4 | 9 | Lirael at the Nexus | Ley Line Nexus | Lirael Trust +3, Full Memory |
| 5 | 11 | Lirael's True Form | Veil opened | Lirael final trust, True Form, Final Ability: Full Restoration Projection |

### Korath (Giant Echo) — "The Gentle Colossus"

| Step | Moon | Quest Name | Trigger | Reward |
|---|---|---|---|---|
| 1 | 7 | Korath's Memory | Giant's Graveyard | Korath Trust +1, Giant Lore |
| 2 | 7 | Korath's Regret | After Memory | Korath Trust +2, Emotional Depth |
| 3 | 7 | The Smallest Giant | Children + Korath | Korath Trust +2, Mutual Healing |
| 4 | 10 | Korath's Heritage | Living Grid | Korath Trust +3, Deep Lore |
| 5 | 12 | Korath's Mountain | Crystal Peaks | Korath Trust +3, Origin Lore, Final Ability: Megalith Command |

### Captain Thorne (Airship Pilot) — "The Endless Flight"

| Step | Moon | Quest Name | Trigger | Reward |
|---|---|---|---|---|
| 1 | 5 | Radio Contact | Dock signal active | Thorne Trust +1, Aerial Recon |
| 2 | 8 | Thorne's Log | Thorne landed | Thorne Trust +1, Flight Log Lore |
| 3 | 8 | Cloud Garden | After Log | Thorne Trust +3, Secret Garden |
| 4 | 8 | Milo Takes Flight | Milo Trust 5+ | Thorne Trust +1, Comedy Scene |
| 5 | 12 | Thorne's Dedication | Crystal Peaks | Thorne Trust +3, Origin Lore, Final Ability: Fleet Command |

### Veritas (Truth-Keeper) — "The Eternal Witness"

| Step | Moon | Quest Name | Trigger | Reward |
|---|---|---|---|---|
| 1 | 11 | Veritas Awakens | Veil thins | Veritas Trust +1, Timeline Sight |
| 2 | 11 | Zereth at the Veil | Zereth + Veil | Veritas Trust +2, Moral Complexity |
| 3 | 12 | Crystal Revelations | Bell-towers ring | Veritas Trust +2, Deep Truth |
| 4 | 13 | Final Testimony | Convergence | Veritas Trust +3, Full History |
| 5 | 13 | Eternal Resonance | Post-convergence | Veritas final trust, Final Ability: "Eternal Resonance" performance |

---

## Anastasia Hidden Quest Line

Princess Anastasia's quest chain is entirely triggered by Golden Mote discovery (see [18_PRINCESS_ANASTASIA.md](18_PRINCESS_ANASTASIA.md)):

| Step | Moon | Trigger | Event | Effect |
|---|---|---|---|---|
| 1 | 1 | Golden Mote #1 found | Faint golden shimmer at screen edge | Anastasia awareness begins |
| 2 | 2 | Golden Mote #2 found | First whispered word (barely audible) | Sound curiosity |
| 3 | 3 | Golden Mote #3 found | Motes orbit player briefly | Visual intrigue |
| 4 | 4 | Golden Mote #4 found | Anastasia's silhouette visible for 2 seconds | Mystery deepens |
| 5 | 5 | Golden Mote #5 found | First clear whispered sentence | Emotional hook |
| 6 | 6 | Golden Mote #6 found | Motes form her crown pattern | Character recognition |
| 7 | 7 | Golden Mote #7 found | She speaks directly to player — 1 line | Connection established |
| 8 | 8 | Golden Mote #8 found | Extended spectral appearance (5 seconds) | Visual confirmation |
| 9 | 9 | Golden Mote #9 found | Full conversation unlocked (3 lines) | Relationship built |
| 10 | 10 | Golden Mote #10 found | She walks beside you for 30 seconds | Companion feeling |
| 11 | 11 | Golden Mote #11 found | She shares her story | Lore revelation |
| 12 | 12 | Golden Mote #12 found | Near-solid form — companions react | Full integration |
| 13 | 13 | Golden Mote #13 found | 10-second solidification at Day Out of Time | Ultimate payoff |

**Completion Reward:** If all 13 motes collected before Moon 13 climax, Anastasia appears during the True Timeline Convergence as a participant, not just an observer. Alternative ending narration.

---

## Ending Variant Quest Triggers

The campaign resolves into one of four endings based on cumulative player choices, quest completions, and companion trust levels. Each ending is triggered by specific conditions evaluated at the Moon 13 Convergence (quest M13-MS05).

| Ending | Trigger Conditions | Key Quest Dependencies | Narrative Outcome |
|---|---|---|---|
| **Harmony** | Global grid ≥ 90% + all 13 bells rung + Zereth confrontation resolved peacefully (M13-MS03 "Harmonize" choice) | M13-MS03 (peace), M13-SQ03 (Zereth relationship 5+), all Moon climax events completed | Full Golden Age restoration; Mud Flood reversed; Zereth healed. Joyful sandbox with god-mode tools. |
| **Echo** | Global grid 60–89% OR chose "Preserve Both" at Convergence OR Zereth relationship 3–4 | M13-MS04 (dual-timeline choice), M13-SQ01 (companion reflections reveal dual possibility) | Dual timeline maintained; player switches between restored and fallen realities. Philosophical sandbox. |
| **Reset** | Global grid < 60% OR chose "Seize Power" at Convergence OR Zereth relationship ≤ 2 | M13-MS03 (force choice), missed 3+ Moon climax events | Controlled grid; power without wonder. Dark sandbox with advanced combat but faded beauty. |
| **The True Name** (Hidden) | All 13 Golden Motes collected + all lore entries complete + Harmony conditions met + M13-HQ01 triggered | M13-HQ01, M13-HQ02, Anastasia chain (13 motes), full Codex | The world speaks the player's name. Anastasia solidifies permanently. Ultimate hidden ending with unique cutscene and narration. |

**Evaluation Order:** The True Name is checked first (supercedes Harmony). Then Harmony → Echo → Reset as fallback.

**Post-Ending Flags:**

| Flag | Set By | Effect |
|---|---|---|
| `ending_harmony` | Harmony or True Name path | Unlocks god-mode creative tools in sandbox |
| `ending_echo` | Echo path | Unlocks dual-timeline toggle in sandbox |
| `ending_reset` | Reset path | Unlocks advanced combat arenas in sandbox |
| `ending_true_name` | True Name path | Unlocks Anastasia as permanent companion + Crown Cosmetic + unique title "The Named One" |

---

## Post-Campaign & Day Out of Time

After Moon 13 completion, the following become available:

| ID | Quest Name | Type | Objective | Reward |
|---|---|---|---|---|
| PC-01 | **Festival Preparations** | Main | Prepare all 13 zones for the Day Out of Time celebration | 2000 AE, 500 BM |
| PC-02 | **Lirael's Concert** | Event | Attend Lirael's choir concert in the Crystal Amphitheater | 500 AE, Music Score collectible |
| PC-03 | **Thorne's Flyover** | Event | Witness the full fleet flyover — player rides with Thorne | 500 AE, Flight Medal |
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
| 7 | 5 | 4 | 2 | 0 | 3 | 14 |
| 8 | 5 | 4 | 2 | 1 (unlocked) | 3 | 15 |
| 9 | 5 | 4 | 2 | 0 | 2 | 13 |
| 10 | 5 | 4 | 2 | 1 (unlocked) | 1 | 13 |
| 11 | 5 | 4 | 2 | 0 | 3 | 14 |
| 12 | 5 | 4 | 2 | 1 (unlocked) | 2 | 14 |
| 13 | 5 | 4 | 2 | 0 | 2 | 13 |
| **Total** | **68** | **52** | **26** | **12** | **26** | **184** |

### Grand Totals
- **184 unique quests** across the 13-Moon campaign
- **8 post-campaign quests** (Day Out of Time / Sandbox)
- **12 recurring daily tasks** (repeatable indefinitely)
- **5 companion loyalty chains** (5 quests each = 25 steps total)
- **13 Anastasia Golden Mote events** (hidden progressive chain)
- **Estimated total playtime:** 60-80 hours for 100% completion (main story: ~40 hours)

---

*Every dome tuned, every bell rung, every child saved — the quest log tells the story of a world being remembered into existence.*

---

**Document Status:** FINAL  
**Author:** Nathan / Resonance Energy  
**Last Updated:** March 25, 2026
