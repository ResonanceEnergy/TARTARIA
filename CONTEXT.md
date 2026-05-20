---
## Moon 2 Exploration, Secrets & Collectibles (R8) — 2026-05-20 (This Delivery — Secrets Agent)

**STRICT COMPLIANCE**: ONLY worked inside `C:\dev\TARTARIA_new`. Read CONTEXT.md FIRST, docs/26_LEVEL_DESIGN.md (Secret Placement Rules + 8.1 taxonomy), docs/03C_MOON_MECHANICS_DETAILED.md (Moon 2 Lunar Moon: Shadow & Purge + fractal cathedral / micro-giant / corruption veins), docs/12_VIVID_VISUALS.md (Moon 2 Fractal Corruption Purge + "golden light floods the corrupted veins, burning them away like fire along a fuse" + cathedral-within-cathedral), and prior R7 visual context. **Exclusive domain**: Hidden areas, secrets, puzzles, collectibles, and exploration rewards for Moon 2 / Crystalline Caverns ONLY. Zero work on other moons, core mechanics, other zones, or non-exploration content. Built directly on R6/R7 visuals (TartarianArchitectureBuilder fractal veins + thickness fuse variants, VFXController Moon2CavernVisualManager with godrays/dome breathing/crystal growth/recursive lights/ley sparks) and existing scaffold/scene.

**Deliverables (8–12 meaningful secrets of varying scale, rich network)**:
- Designed and implemented a complete 10-secret exploration network across Moon 2, tied 100% to the "fractal cathedral" fantasy.
  - **3 Small (Vein Echo Shards)**: Off-path subtle pulsing dark fractal vein tips (R7 visual language). Player uses Dissonance Lens / scanner / resonance trace to "solve". Payoff: quick golden fuse burn (all 3 R7 thickness styles), micro crystal growth shards, Aether + short lore codex entries ("Crystal Carver's Lament").
  - **4 Medium (Refractive Alcoves)**: Walls with impossible light refraction (R7 caustics/probe aesthetic). Scanner + sustained resonance alignment opens pocket chambers. Payoff: unique Moon2 collectibles ("Prism of Refraction", "Bell Echo Prism", etc.) granting mechanical (scan radius) + visual (temp godray/caustics boost in micro) + narrative (Lirael affinity) rewards.
  - **2 Large (Micro-Giant Corruption Vein Puzzles)**: Inside cathedral_dome and ley_chamber micro interiors. Optional side branches with 3 veins of differing thickness (exact R7 "thick embers / medium classic / thin fast sparks"). Solve in visual order to open hidden sub-chambers. Payoff: "Amber Growth Catalyst" / "Ley Heart Fragment" — on macro restore triggers extra crystal growth + breathing amplitude + probe/godray boosts (directly extends R7 SubtleCrystalGrowthOnRestore, StartDomeBreathing, CreateOrBoostGodrayShafts).
  - **1 Epic (The Fractal Cathedral Heart)**: Multi-condition (all 5+ buildings restored + 2 micro puzzles solved + correct bell sequence via visual ripple cues). Deepest recursive chamber at zone center. Massive godray + recursive geometry + intensified everything. Payoff: "Fractal Keystone" (unique Moon2 item) — permanent session-deep visual escalation of entire zone (stronger dome breathing, intensified ley sparks across all 5 structures, extra recursive lights, auto first corruption node pulse in future micro purges) + major pre-Flood vision lore entry + high Aether/RS + companion peak dialogue.
- New runtime file: `Moon2ExplorationSecrets.cs` (Integration) — data-driven 10 secrets with positions matching scaffold, proximity + tool discovery, breadcrumb spawning (emissive R7-style vein/crystal hints), collectible spawning via existing PickupInteractable (moon2_secret_* + keystone), ArchiveManager lore, GameLoop Aether/RS rewards, and full dispatch to Moon2CavernVisualManager for every visual payoff.
- Extended `VFXController.cs` (Moon2CavernVisualManager) with rich secret-specific methods: RevealMoon2SecretVisual (type-dispatched), SpawnSecretVeinBurnSequence (thickness variants), SpawnSecretRefractiveAlcoveOpen (godrays + caustics + prism), SpawnSecretMicroFractalChamber (recursive + growth), SpawnSecretFractalCathedralHeart (full escalation), TriggerSecretCrystalGrowthBonus, ApplyMoon2EpicSecretPermanentVisualUpgrade — all reuse and intensify R6/R7 systems exactly.
- Extended `Moon2ZoneScaffold.cs` (Editor): 
  - 10 named secret anchors placed in BuildSceneTemplate (Moon2_Secrets_ExplorationNetwork_10Secrets) at precise positions for runtime discovery.
  - New editor menu: "Tartaria/Moon 2/Apply Moon 2 Secrets & Collectibles Network (10 Rich Exploration Secrets)" — wires Moon2ExplorationSecrets + manager on dressing root, validates, logs full design.
  - Updated R7 polish + Phase 3 menus to preserve + extend coverage.
- Secrets follow 26_LEVEL_DESIGN rules: never on golden route, visual breadcrumbs (R7 veins/refractive), companion hints, proportional rewards, non-missable via replay.
- All payoffs make exploration feel magical and tied to the living crystal cathedral: every secret deepens the R7 visuals (fuse like fire, breathing, godrays, recursive geometry) while delivering narrative (pre-Flood memories) and mechanical (keystone upgrades future play) value.
- 0 new assets. 100% runtime + editor using existing R7 toolkit. Domain lock absolute (Moon 2 exploration/secrets only).

**Files edited (Moon 2 exploration/secrets/collectibles domain ONLY, absolute C:\dev\TARTARIA_new paths)**:
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\Moon2ExplorationSecrets.cs` (new — full 10-secret runtime system, discovery, rewards, VFX dispatch)
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\VFXController.cs` (Moon2CavernVisualManager extended with 8+ secret visual payoff coroutines + permanent epic upgrade using every R7 system)
- `C:\dev\TARTARIA_new\Assets\_Project\Editor\Moon2ZoneScaffold.cs` (secrets anchors in template + dedicated R8 menu + integration in polish flows)
- `C:\dev\TARTARIA_new\CONTEXT.md` (this R8 delivery note)

**How to verify (Moon 2 secrets only)**:
- Open `C:\dev\TARTARIA_new\Assets\_Project\Scenes\Moons\CrystallineCaverns.unity`.
- Run Tartaria > Moon 2 > Full Visual Polish Round 7 (Final...) to dress.
- Run Tartaria > Moon 2 > Apply Moon 2 Secrets & Collectibles Network (10 Rich...).
- Play: use scanner / Dissonance Lens / E near the 10 anchors (off golden route around buildings). Discover small → medium → large → epic.
- Watch exact R7 visuals fire on each (fuse variants by thickness, godrays, growth, breathing intensification, ley sparks, recursive lights).
- Collect keystones/prisms via PickupInteractable; note permanent epic upgrade on Heart.
- Check console for lore + companion hints + Aether/RS rewards.
- Re-run after; ForceReDiscover works. Matches 03C micro-giant/corruption puzzles + 12_VIVID purge visuals + 26 secret rules + fractal cathedral fantasy.

**How the secret design encourages deep exploration of the caverns**:
The 10 secrets form a deliberate progression ladder that rewards players who stray from the obvious path and fully engage Moon 2's unique systems (Dissonance Lens, micro-giant interiors, bell sequencing, full restoration order). Small secrets provide immediate "I found something" dopamine with quick R7 fuse visuals. Medium add light puzzle depth and unique collectibles that improve traversal (scanner boost). Large secrets live inside the micro-giant fractal dungeons (using the exact R7 vein thickness visuals as puzzle solution), teaching players to look for side branches during purges. The Epic Heart requires mastery of everything — all buildings + prior secrets + visual cues — and repays with the most spectacular intensification of the entire living crystal cathedral (every R7 system cranked up permanently). 

Breadcrumbs are subtle but unmistakable once players internalize the R7 visual language (pulsing off-color veins, wrong-angle refraction, godray hints in alcoves). Companion comments and scanner pings guide without hand-holding. Rewards are layered (visual beauty that changes how the caverns feel forever, lore that deepens the pre-Flood tragedy, mechanical power that makes future Moon 2 runs richer). 

Players who rush the golden route miss 80% of the magic. Those who explore deeply feel the "cathedral within a cathedral" come alive — exactly the fantasy from 03C and 12_VIVID. The network turns the caverns from a linear purge zone into a rewarding, replayable fractal wonderland. Perfectly matches level design philosophy: "Every step reveals" and "Secrets are off the golden route."

**Production readiness**: Moon 2 now has a complete, rich, visually spectacular secret layer that makes deep exploration the most rewarding part of the zone. All tied to the strongest R6/R7 visuals. Ready for playtesting and integration with full Dissonance Lens / MicroGiant systems. Domain-strict 100%. Git shows only the 4 files.

**Git verification at R8 Secrets delivery** (executed below): cd "C:\dev\TARTARIA_new" && git add "Assets/_Project/Scripts/Integration/Moon2ExplorationSecrets.cs" "Assets/_Project/Scripts/Integration/VFXController.cs" "Assets/_Project/Editor/Moon2ZoneScaffold.cs" "CONTEXT.md" && git commit -m "moon2 exploration secrets & collectibles (R8): rich 10-secret network across Crystalline Caverns — 3 Small VeinEcho, 4 Medium RefractiveAlcove, 2 Large MicroVeinPuzzles (R7 thickness order), 1 Epic FractalCathedralHeart — all payoffs use/extend R6/R7 visuals (fuse variants, godrays, breathing, growth, recursive lights, ley) + unique Moon2 collectibles + lore + permanent epic upgrade + scaffold anchors + new editor menu + runtime manager (domain-strict, matches 03C/12_VIVID/26_LEVEL_DESIGN fractal cathedral fantasy, encourages deep exploration)"

**Absolute paths used throughout**: All C:\dev\TARTARIA_new\...

---
(The prior Moon 2 Buildings Phase 3 + R7 visuals + Moon 3 / Bosses / Companion sections and history follow below.)

