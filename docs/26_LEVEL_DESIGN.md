# TARTARIA WORLD OF WONDER — Level Design Guide
## Zone Layout, Traversal, Encounter Pacing, POI Density & Verticality

---

> *"Every zone is a buried symphony. The level designer's job is to compose the order in which the player hears each note."*

**Cross-References:**
- [04_ARCHITECTURE_GUIDE.md](04_ARCHITECTURE_GUIDE.md) — Sacred geometry building, dome/spire/star fort dimensions
- [15_MVP_BUILD_SPEC.md](15_MVP_BUILD_SPEC.md) — Echohaven vertical slice (500m radius)
- [09_TECHNICAL_SPEC.md](09_TECHNICAL_SPEC.md) — Zone streaming (500m radius), LOD pipeline, memory budgets
- [06_COMBAT_PROGRESSION.md](06_COMBAT_PROGRESSION.md) — Encounter frequency, escalation curves
- [03B_EXPANSION_PACKS.md](03B_EXPANSION_PACKS.md) — DLC zones and Moon-to-zone mapping
- [07_PC_UX.md](07_PC_UX.md) — Session length design, keyboard/mouse navigation
- [19_ECONOMY_BALANCE.md](19_ECONOMY_BALANCE.md) — Resource placement (Aether bands, material drops)
- [20_QUEST_DATABASE.md](20_QUEST_DATABASE.md) — 184 quests distributed across zones

---

## Table of Contents

1. [Level Design Philosophy](#1-level-design-philosophy)
2. [Zone Architecture Template](#2-zone-architecture-template)
3. [Zone Catalog](#3-zone-catalog)
4. [Traversal Systems](#4-traversal-systems)
5. [Point of Interest (POI) Design](#5-point-of-interest-poi-design)
6. [Encounter Pacing](#6-encounter-pacing)
7. [Verticality & Layer Design](#7-verticality-layer-design)
8. [Secret Placement Rules](#8-secret-placement-rules)
9. [Social & Gathering Spaces](#9-social-gathering-spaces)
10. [Streaming & Technical Constraints](#10-streaming-technical-constraints)
11. [MVP Zone: Echohaven Deep Dive](#11-mvp-zone-echohaven-deep-dive)

---

## 1. Level Design Philosophy

### Six Tenets of Tartarian Level Design

1. **Every step reveals.** The player should discover something new every 15–30 seconds of movement — a buried dome, a lore fragment, an Aether vein, a companion reaction.
2. **Landmark navigation, not mini-maps.** The world teaches its own geography. Tall spires, glowing ley lines, and unique silhouettes guide the player. Mini-map is optional (see [24_ACCESSIBILITY.md](24_ACCESSIBILITY.md)).
3. **Restoration is exploration.** Cleaning a building isn't just a reward — it changes the zone's geometry, opening new paths and sight-lines. The level transforms as you play.
4. **The golden route.** Every zone has a "golden route" — a natural walking path that hits 80% of the key content in the optimal order. Players who wander find secrets. Players who follow the architecture find the story.
5. **Respect session length.** Zone beats are tuned to the 5 session types from [07_PC_UX.md](07_PC_UX.md): Micro (1–3 min), Quick (5–10 min), Standard (15–30 min), Deep (30–60 min), Epic (45–90 min). A player can complete a meaningful loop in any session.
6. **Verticality tells time.** Tartarian zones are layered: underground = buried past, ground = corrupted present, restored heights = hopeful future. Ascending through a zone IS the restoration arc.

---

## 2. Zone Architecture Template

Every zone follows this structural template:

### 2.1 Zone Anatomy

```
Zone ({name}, {radius}m)
│
├── Central Landmark (visible from zone edge)
│   └── Marks the zone's identity — always a major restoration target
│
├── Quadrants (4 sectors, roughly equal)
│   ├── Q1: Story Quadrant — main quest content, NPC hubs
│   ├── Q2: Exploration Quadrant — secrets, lore, hidden paths
│   ├── Q3: Combat Quadrant — enemy density, combat encounters
│   └── Q4: Builder Quadrant — open building space, resource nodes
│
├── Connectors (2–4 paths to adjacent zones)
│   ├── Primary road (wide, obvious, story-mandated)
│   └── Secondary paths (narrow, hidden, reward exploration)
│
├── Ley Line Network (Aether veins running through zone)
│   ├── Major ley line (connects central landmark to connector)
│   └── Minor veins (branch to POIs)
│
└── Underground Layer (optional, per zone)
    ├── Tunnels beneath corrupted areas
    ├── Hidden chambers (lore, mini-bosses)
    └── Connects to surface via entrances unlocked by restoration
```

### 2.2 Density Standards

| Metric | Standard | Per 100m² |
|---|---|---|
| **Restorable buildings** | 8–12 per zone | ~0.3 |
| **Aether nodes** | 15–25 per zone | ~0.6 |
| **Lore fragments** | 5–10 per zone | ~0.2 |
| **Combat encounters** | 4–8 per zone (respawning) | ~0.2 |
| **NPCs** | 3–6 per zone | ~0.1 |
| **Secret areas** | 2–4 per zone | ~0.1 |
| **Mini-game locations** | 1–2 per zone | ~0.05 |
| **Companion trigger points** | 4–8 per zone | ~0.2 |

### 2.3 Zone Size Categories

| Category | Radius | Area | Typical Use |
|---|---|---|---|
| **Hub** | 250m | ~196,000 m² | Central cities (Echohaven, Capital) |
| **Standard** | 500m | ~785,000 m² | Main campaign zones |
| **Corridor** | 150m wide × 800m long | ~120,000 m² | Travel zones connecting hubs |
| **Pocket** | 100m | ~31,000 m² | Secret areas, mini-dungeons |
| **DLC** | 500–750m | variable | Expansion zones |

---

## 3. Zone Catalog

### 3.1 Campaign Zones (13 Moons)

| Zone | Moon | Category | Theme | Central Landmark |
|---|---|---|---|---|
| **Echohaven** | 1 | Hub | Sunlit valley, first restoration | Great Dome of Echohaven |
| **Murmuring Hollows** | 2 | Standard | Dense forest, whisper lore | Ancient Library Tree |
| **Crystal Veil** | 3 | Standard | Crystal caves, refraction | Grand Crystal Organ |
| **Ironhold Reach** | 4 | Standard | Industrial ruins, Star Fort | Star Fort Ironhold |
| **Skyfire Plateau** | 5 | Standard | Mountain plateau, airship dock | Airship Spire |
| **Sunken Gardens** | 6 | Standard | Flooded lowlands, water puzzles | Fountain of Resonance |
| **Ashen Spine** | 7 | Corridor | Volcanic ridge, Korath's trial | Obsidian Cathedral |
| **Veilstorm Coast** | 8 | Standard | Stormy coastline, lighthouse | Grand Lighthouse |
| **Clockwork Undercity** | 9 | Standard | Underground mechanical zone | Eternal Clocktower |
| **Starseed Plains** | 10 | Standard | Open prairie, vast sky | Antenna Array Field |
| **Frozen Resonance** | 11 | Standard | Tundra, ice-preserved buildings | Ice Dome of the Ancients |
| **Shattered Prism** | 12 | Standard | Fractured reality, all biomes | The Prism Core |
| **The Cosmic Threshold** | 13 | Hub | Final zone, convergence | The Grand Nexus |

### 3.2 Connector Zones

| Connector | Connects | Category | Feature |
|---|---|---|---|
| **Echohaven Road** | Echohaven ↔ Murmuring Hollows | Corridor | Tutorial travel path |
| **Crystal Pass** | Hollows ↔ Crystal Veil | Corridor | Vertical climb |
| **Iron Bridge** | Crystal Veil ↔ Ironhold Reach | Corridor | Bridge over chasm |
| **Sky Rail** | Ironhold ↔ Skyfire Plateau | Corridor | Rail cart ride (mini-game) |
| **River of Echoes** | Skyfire ↔ Sunken Gardens | Corridor | Boat traversal |
| **Ember Trail** | Gardens ↔ Ashen Spine | Corridor | Volcanic path |
| **Storm Crossing** | Spine ↔ Veilstorm Coast | Corridor | Weather challenge |
| **Depths Gate** | Coast ↔ Clockwork Undercity | Corridor | Descent sequence |
| **Starlit Road** | Undercity ↔ Starseed Plains | Corridor | Emergence into open sky |
| **Frostfall Pass** | Plains ↔ Frozen Resonance | Corridor | Blizzard traversal |
| **Prism Rift** | Resonance ↔ Shattered Prism | Corridor | Reality fracture walk |
| **The Final Approach** | Prism ↔ Cosmic Threshold | Corridor | All companions converge |

---

## 4. Traversal Systems

### 4.1 Movement Modes

| Mode | Speed | Unlock | Use Case |
|---|---|---|---|
| **Walk** | 5 m/s | Default | Exploration, building, conversation |
| **Sprint** | 9 m/s | Default | Quick traversal (drains stamina) |
| **Ley Line Glide** | 15 m/s | Moon 2 | On ley lines (free, no stamina) |
| **Airship** | 25 m/s | Moon 5 | Between zones (fast travel) |
| **Rail Cart** | 12 m/s | Moon 4 | Corridor, mini-game at junctions |
| **Giant Mode** | 20 m/s | Moon context | Combat + traversal (barriers destroyed) |
| **Boat** | 8 m/s | Moon 6 | Water zones only |

### 4.2 Fast Travel Rules

| Rule | Detail |
|---|---|
| **Unlock condition** | Must physically visit a zone's central landmark first |
| **Fast travel network** | Ley Line Nexus points (1 per zone, at central landmark) |
| **Cost** | Free (no resource cost — removing friction, not adding gates) |
| **Loading disguise** | Airship flight animation covers zone streaming |
| **Restriction** | Cannot fast travel during combat or active quest sequences |
| **Moon 1** | No fast travel (walk Echohaven, learn the world) |
| **Moon 5+** | Full airship fast travel between all visited zones |

### 4.3 Traversal Pacing

Target: **Player reaches next discovery every 15–30 seconds of movement.**

| Distance from Last POI | Expected Discovery |
|---|---|
| 0–50m | Nothing (breathing room after last discovery) |
| 50–100m | Minor discovery: Aether node, environmental detail, companion comment |
| 100–200m | Medium discovery: lore fragment, resource cluster, NPC encounter |
| 200–300m | Major discovery: building to restore, combat encounter, quest trigger |
| >300m | Too far — indicates design hole; fill with minor discovery or path fork |

---

## 5. Point of Interest (POI) Design

### 5.1 POI Taxonomy

| Type | Visual Cue | Interaction | Time to Complete |
|---|---|---|---|
| **Restorable Building** | Mud-covered structure, Aether shimmer | Restoration mini-game + building placement | 2–10 min |
| **Aether Node** | Glowing ground vent (band-colored) | Harvest (tap and hold) | 5–15 sec |
| **Lore Fragment** | Floating text glyph, musical hum | Read/collect + codex entry | 30 sec |
| **Combat Encounter** | Corruption zone, enemy silhouettes | Frequency combat | 1–5 min |
| **NPC Hub** | Campfire/structure, character animations | Dialogue, quests, companion interactions | 2–15 min |
| **Secret Area** | Hidden entrance (behind waterfall, under rubble) | Discover + explore | 5–20 min |
| **Mini-Game Station** | Unique interactive object (organ, tuning fork) | Mini-game (see [13_MINI_GAMES.md](13_MINI_GAMES.md)) | 2–5 min |
| **Vista Point** | High ground, panoramic view | Camera shift, companion comment, map reveal | 30 sec |
| **Ley Line Nexus** | Converging energy lines, fast-travel stone | Fast travel activation | 10 sec |
| **Resource Cache** | Chest, rubble pile, crystal deposit | Material reward | 15 sec |

### 5.2 POI Placement Rules

1. **Never more than 150m between two POIs.** The player should always see or sense the next thing.
2. **POIs cluster around ley lines.** The natural navigation paths (ley lines) guide players past the richest content.
3. **Major POIs are visible from distance.** A dome silhouette at 200m, a spire glow at 300m, a ley line visible for its full length.
4. **Secrets are off the golden route.** Reward players who leave the path with lore, cosmetics, and Aether deposits.
5. **Combat encounters guard resources.** The best Aether nodes and resource caches are behind combat zones.
6. **Companion triggers near POIs.** Companions comment on nearby buildings, lore, and enemies — layer emotional content onto spatial content.

---

## 6. Encounter Pacing

### 6.1 Encounter Density Per Session Type

| Session | Duration | Expected Encounters | Rhythm |
|---|---|---|---|
| **Micro** | 1–3 min | 0–1 (harvest nodes, read lore) | Discovery → reward → exit |
| **Quick** | 5–10 min | 1–2 (1 combat + 1 building) | Explore → encounter → restore → exit |
| **Standard** | 15–30 min | 3–5 (mixed types) | Explore → combat → restore → quest → rest |
| **Deep** | 30–60 min | 6–10 | Full zone exploration arc |
| **Epic** | 45–90 min | 10+ | Multi-zone, boss encounter, story climax |

### 6.2 Encounter Escalation Curve

Within a zone, encounters follow a sine-wave intensity pattern:

```
Intensity
    │
  ★ │        ╱╲            ╱╲
    │      ╱    ╲        ╱    ╲            ← Boss/climax
    │    ╱        ╲    ╱        ╲
    │  ╱            ╲╱            ╲        ← Medium encounters
    │╱                              ╲
    │ gentle    build    rest    build  ╲   ← Exploration/calm
    └────────────────────────────────────── Distance from zone entry
         0%     25%      50%     75%    100%
```

- **0–25%:** Gentle introduction. Minor enemies, easy harvesting, companion sets the scene.
- **25–50%:** Rising action. Tougher encounters, first restorations, quest threads emerge.
- **50%:** Mid-zone rest beat — safe NPC hub, vista point, mini-game station.
- **50–75%:** Escalation. Multi-enemy encounters, complex buildings, choice points.
- **75–100%:** Climax and resolution. Zone boss/challenge → central landmark restoration.

### 6.3 Combat Encounter Templates

| Template | Enemies | Arena Size | Duration | Loot |
|---|---|---|---|---|
| **Patrol** | 2–3 basic | Open field | 30–60 sec | Aether Essence |
| **Ambush** | 3–5 mixed | Enclosed area | 60–90 sec | Materials + Fragments |
| **Guardian** | 1 elite + 2 basic | Building entrance | 90–120 sec | Building unlock + Shards |
| **Swarm** | 8–12 weak | Open area | 60–90 sec | Frequency Token |
| **Mini-Boss** | 1 boss + 3 support | Arena (walled) | 3–5 min | Star Points + Cosmetic |
| **Zone Boss** | 1 major boss | Custom arena | 5–8 min | Resonance Shards + Story |

---

## 7. Verticality & Layer Design

### 7.1 The Three Layers

Every zone has three conceptual layers representing Tartaria's temporal arc:

| Layer | Altitude | Theme | Content |
|---|---|---|---|
| **Underground** | Below ground | Buried past | Tunnels, hidden chambers, pre-corruption artifacts, lore-heavy |
| **Surface** | Ground level | Corrupted present | Main gameplay — buildings, combat, NPCs, quests |
| **Heights** | Elevated | Restored future | Spire tops, dome interiors, vista points, rewards |

### 7.2 Vertical Traversal

| Mechanism | Unlock | Direction |
|---|---|---|
| **Stairs (restored)** | Restore building | Up/Down |
| **Ley Line Elevator** | Restore ley line node | Up/Down |
| **Vine Climb** | Default | Up only |
| **Crater Descent** | Default | Down only |
| **Airship Dock** | Moon 5 | Zone-to-zone |
| **Giant Mode Leap** | Context-specific | Up (platform smash) |

### 7.3 Verticality Metrics

| Zone Category | Min Vertical Range | Vertical POIs | Underground |
|---|---|---|---|
| **Hub** | 30m | 4+ | Optional |
| **Standard** | 50m | 6+ | 1 chamber min |
| **Corridor** | 20m | 2+ | No |
| **Pocket** | 40m | 3+ | Core feature |

---

## 8. Secret Placement Rules

### 8.1 Secret Taxonomy

| Secret Type | Discovery Method | Reward | Frequency |
|---|---|---|---|
| **Hidden Path** | Walk behind waterfall / through illusory wall | Pocket zone access | 1–2 per zone |
| **Buried Chamber** | Restore building → reveals basement access | Lore + rare cosmetic | 1 per zone |
| **Ley Line Anomaly** | Match specific frequency at specific location | Aether Shard cache | 2–3 per zone |
| **Companion Secret** | Bring specific companion to specific location | Companion backstory + affinity | 1 per companion per zone |
| **Alignment Secret** | World-Shaping choice unlocks unique area | Story variant + cosmetic | 1–2 per zone (post-choice) |
| **Seasonal Secret** | Only accessible during specific live-ops event | Exclusive cosmetic | 0–1 per zone per season |

### 8.2 Placement Rules

1. **Secrets are never on the golden route.** The obvious path should feel complete without them.
2. **Visual breadcrumbs.** Every secret has a subtle visual hint: unusual geometry, off-color stone, Aether glow behind a wall.
3. **Companion hints.** After 30 seconds within 50m of a secret, companions offer a contextual comment (never direct — "Something feels different here...").
4. **No permanently missable secrets.** Moon Replay allows revisiting all zones with all companions.
5. **Reward proportional to difficulty.** Hard-to-find secrets give rare cosmetics. Easy-to-find give resources.

### 8.3 Hidden Flower Patch Locations

Aether-bloom flower patches are scattered off the golden route in every zone. Each patch glows faintly at night and triggers a companion comment. Discovering all patches across every zone unlocks the **H04 Flower Child** achievement (see [28_ACHIEVEMENTS.md](28_ACHIEVEMENTS.md)).

| Zone | Patch Name | Location Hint | Flowers |
|---|---|---|---|
| **Echohaven** | Dome Garden | Behind the Great Dome waterfall, ground-level alcove | Golden Aether Lilies (×6) |
| **Murmuring Hollows** | Whispering Bloom | Inside the hollow of the Ancient Library Tree's roots | Violet Resonance Ferns (×4) |
| **Crystal Veil** | Prismatic Meadow | Refracted light alcove behind the Grand Crystal Organ | Rainbow Quartz Blossoms (×5) |
| **Ironhold Reach** | Rust Garden | Overgrown courtyard inside the Star Fort inner wall | Iron Rose Vines (×8) |
| **Skyfire Plateau** | Cloud Petals | Floating soil pocket at the plateau's eastern cliff edge | Sky Orchids (×3) |
| **Sunken Gardens** | Submerged Bloom | Underwater grotto below the Fountain of Resonance | Aqua Lilies (×6) |
| **Ashen Spine** | Ember Flowers | Volcanic vent with unexpected moist soil, northeast ridge | Cinder Poppies (×4) |
| **Veilstorm Coast** | Storm Roses | Sheltered tidal cave, accessible at low tide only | Salt Roses (×5) |
| **Clockwork Undercity** | Gear Garden | Abandoned greenhouse in the clocktower's basement level | Brass-Stem Daisies (×7) |
| **Starseed Plains** | Starfield Bloom | Center of the Antenna Array Field, grows between pylons | Starlight Asters (×10) |
| **Frozen Resonance** | Ice Blossoms | Thermal vent inside the Ice Dome, heated pocket | Crystal Snowdrops (×4) |
| **Shattered Prism** | Reality Flowers | Where three biome fragments overlap — flowers shift color | Prism Petals (×6) |
| **The Cosmic Threshold** | Convergence Garden | Hidden behind the Grand Nexus, only visible from above | Aether Lotuses (×13) |

**Design Notes:**
- Patches are placed 100–200m off the golden route, requiring intentional exploration
- Each patch has a unique ambient particle effect (pollen glow matching its zone's color palette)
- Companion comments trigger within 30m: Lirael hums near flowers, Milo tries to appraise them, Korath remembers giant-tended gardens
- Flower patches regenerate every 7 in-game days — players can revisit for the ambient experience

---

## 9. Social & Gathering Spaces

### 9.1 Hub Design

Each zone's NPC hub serves as a social rest point:

| Feature | Purpose | Design Rule |
|---|---|---|
| **Campfire / Pavilion** | Visual anchor for the safe zone | Visible from 100m, warm lighting |
| **NPC cluster** | Quest givers, merchants, lore characters | 3–6 NPCs within 30m radius |
| **Build pad** | Dedicated open space for player building | Flat terrain, pre-placed snap points |
| **Mini-game station** | Recreational activity | Within 50m of campfire |
| **Companion hangout** | Companions idle with unique animations | Near campfire, context-specific |
| **Ley Line Nexus** | Fast travel point | At hub center |

### 9.2 World's Fair Spaces

Per [08_MONETIZATION.md](08_MONETIZATION.md), community events feature player-built structures:

| Space | Size | Capacity | Location |
|---|---|---|---|
| **Zone Fair Grounds** | 200m × 200m | 50 featured builds | Adjacent to each hub zone |
| **Grand Exhibition** | 500m × 500m | 200 featured builds | Cosmic Threshold (Moon 13 zone) |
| **Seasonal Plaza** | 100m × 100m | 25 themed builds | Rotating zone per season |

---

## 10. Streaming & Technical Constraints

### 10.1 Zone Streaming Budget

Per [09_TECHNICAL_SPEC.md §5](09_TECHNICAL_SPEC.md), zone streaming uses Addressables:

| Ring | Distance | Content | Memory |
|---|---|---|---|
| **Active** | 0–200m | Full LOD 0–1, all systems active | 800 MB |
| **Transition** | 200–500m | LOD 1–2, reduced systems | 400 MB |
| **Background** | 500–750m | LOD 2–3 (impostors), audio ambient only | 200 MB |
| **Unloaded** | >750m | Not in memory | 0 |

### 10.2 Per-Zone Data Budgets

| Data Type | Budget per Zone | Format |
|---|---|---|
| **Geometry (all LODs)** | 80 MB | Meshes + collision |
| **Textures** | 120 MB | ASTC compressed, mip-streamed |
| **Lightmaps** | 40 MB | ASTC baked |
| **Audio** | 20 MB | Ambient + SFX triggers |
| **AI / NPC data** | 5 MB | Behavior trees + dialogue nodes |
| **Aether field** | 3 MB | 3D grid (2m cells) |
| **Total per zone** | **~268 MB** | Addressable group |

### 10.3 Level Design Constraints

| Constraint | Limit | Reason |
|---|---|---|
| **Max concurrent buildings (restored, LOD0)** | 30 | Draw call budget |
| **Max concurrent NPCs** | 12 | Animation + AI budget |
| **Max concurrent enemies** | 15 | Combat system budget |
| **Max concurrent VFX sources** | 40 | Particle budget |
| **Max terrain patches** | 16 | Heightmap streaming |
| **Max unique materials per zone** | 64 | SRP Batcher limit |
| **Max Aether nodes (active)** | 100 | ECS query budget |

---

## 11. MVP Zone: Echohaven Deep Dive

### 11.1 Echohaven Layout

Per [15_MVP_BUILD_SPEC.md](15_MVP_BUILD_SPEC.md), Echohaven is the vertical slice zone:

```
                    N
                    │
            ┌───────┼───────┐
            │  Q2: Explore  │
            │  ┌─Waterfall─┐│
            │  │ Secret     ││
            │  │ Chamber    ││
     W ─────│──┤   ★ DOME  ├│───── E
            │  │ (Central)  ││
            │  └────────────┘│
            │  Q1: Story    │ Q3: Combat
            │  ┌─NPC Hub──┐ │ ┌─Corruption─┐
            │  │Campfire   │ │ │ Zone       │
            │  │Milo + 2NPC│ │ │ 4 encounters│
            │  └───────────┘ │ └────────────┘
            │  Q4: Builder  │
            │  ┌─Open Pad──┐│
            │  │Build Space ││
            │  │Snap Points ││
            │  └───Entrance─┘│
            └───────┼───────┘
                    │
                    S
             (Echohaven Road → Murmuring Hollows)
```

### 11.2 Echohaven POI Map

| POI | Quadrant | Type | Session Length | Quest |
|---|---|---|---|---|
| Great Dome of Echohaven | Center | Restorable Building | Standard (15 min) | Main Quest M1-Q01 |
| Milo's Campfire | Q1 | NPC Hub | Quick (5 min) | Tutorial + M1-Q02 |
| Aether Spring (3 nodes) | Q1/Q2 boundary | Aether Nodes | Micro (2 min) | — |
| Waterfall Cave | Q2 | Secret Area | Standard (15 min) | Optional lore |
| Corruption Patch Alpha | Q3 | Combat Encounter | Quick (5 min) | M1-Q03 |
| First Tuning Fork | Q3 | Mini-Game Station | Quick (5 min) | M1-Q04 |
| Builder's Terrace | Q4 | Build Pad | Standard (15 min) | Building tutorial |
| Ley Line Nexus | Center (under Dome) | Fast Travel | Micro (1 min) | Unlocks M2 |
| Echohaven Overlook | Q2 (height) | Vista Point | Micro (1 min) | — |
| Lore Stone (×3) | Q1, Q2, Q3 | Lore Fragment | Micro (30 sec each) | Codex entries |

### 11.3 Echohaven 15-Minute Demo Script

Per [15_MVP_BUILD_SPEC.md](15_MVP_BUILD_SPEC.md):

| Minute | Location | Activity | Teaches |
|---|---|---|---|
| 0:00–1:00 | Entrance | Cinematic: mud-covered valley, camera reveals dome | World setup |
| 1:00–3:00 | Path to campfire | Walk + 2 Aether nodes | Movement, harvesting |
| 3:00–5:00 | Milo's Campfire | Meet Milo, first dialogue | Companion system |
| 5:00–7:00 | Corruption Patch | First combat encounter (2 enemies) | Frequency combat |
| 7:00–9:00 | Tuning Fork | First mini-game | Mini-game loop |
| 9:00–12:00 | Great Dome approach | Restoration sequence | Building/cleaning |
| 12:00–14:00 | Builder's Terrace | Place first player building | Building system |
| 14:00–15:00 | Dome interior | Vista + Milo reaction + ley line activation | Emotional payoff |

---

*The world isn't just a stage for the story — it IS the story. Every buried dome, every corrupted path, every restored spire tells you who the Tartarians were and what we're fighting to bring back.*

---

**Document Status:** FINAL
**Author:** Nathan / Resonance Energy
**Last Updated:** March 25, 2026
