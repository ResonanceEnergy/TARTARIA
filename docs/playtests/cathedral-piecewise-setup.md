# Cathedral Piecewise Restore — Scene Setup

*Owner: Level Designer (code) + Cowork (scene wiring). Moon 1 climactic beat.*

---

## What this is

The Echohaven cathedral restoration is the emotional payoff of Moon 1. Instead of the building popping into existence on tuning success, it rises in **5 staged sub-events** so the player can read each architectural element emerging from the mud:

1. **Foundation** — stone base lifts into place
2. **Walls** — nave and transept walls rise
3. **Roof** — vaulted ceiling drops into position above walls
4. **Buttresses** — flying buttresses settle onto the walls
5. **Spire** — central spire ascends last, marking completion

Each piece rises 4 metres over 1 second with a SmoothStep ease, followed by a **1.5 second gap** before the next piece. Total sequence: ~12.5 seconds end-to-end.

---

## Code location

`Assets/_Project/Scripts/Integration/Moon1CathedralRestore.cs`

- Listens for `GameEvents.OnBuildingRestored(string buildingId)`.
- Filters on `buildingId.ToLowerInvariant().Contains("cathedral")` — any cathedral-tagged restore triggers the sequence.
- Singleton pattern (`Instance`) — only one instance active per scene.
- Coroutine-driven, re-entrant safe (`StopAllCoroutines()` on retrigger).

---

## Cowork's scene-wiring tasks (Echohaven_VerticalSlice.unity)

1. **Locate the cathedral parent GameObject** in the scene hierarchy. This is the root of the cathedral prefab placed in Echohaven (likely under `Moon1_Buildings/Cathedral` or similar).

2. **Attach the `Moon1CathedralRestore` component** to the cathedral parent GameObject:
   - Inspector → Add Component → search "Moon1CathedralRestore"

3. **Drag the 5 child piece GameObjects** into the SerializedFields:
   | Inspector field | Drag from hierarchy |
   |---|---|
   | `Foundation` | Cathedral's foundation child (stone slab base) |
   | `Walls` | Cathedral's wall child (nave/transept geometry) |
   | `Roof` | Cathedral's roof child (vaulted ceiling) |
   | `Buttresses` | Cathedral's buttress child (flying buttress array) |
   | `Spire` | Cathedral's spire child (central tower) |

   If the prefab does not yet have these 5 sub-meshes split out, **Cowork creates 5 empty child GameObjects** named `Foundation`, `Walls`, `Roof`, `Buttresses`, `Spire` and assigns the cathedral mesh fragments to them, parented under the cathedral root. Use the existing cathedral prefab's pieces if they're already authored as separate meshes.

4. **Leave `gapSeconds` at the default 1.5** unless playtest tuning suggests otherwise.

5. **Initial state:** set all 5 piece GameObjects to inactive in the Inspector (uncheck the active checkbox at the top). The script calls `piece.SetActive(true)` as each one rises — they should not be visible before their cue.

---

## Trigger flow (E-key chain)

1. Player walks into cathedral interact volume → prompt appears.
2. Player presses **E** → tuning mini-game opens.
3. Player completes tuning → `GameEvents.RaiseBuildingRestored("cathedral_echohaven")` (or similar id) fires.
4. `Moon1CathedralRestore.HandleBuildingRestored` matches on substring `"cathedral"` → starts `PiecewiseRise()`.
5. 5 pieces rise sequentially with 1.5s gaps.
6. Final log: `[Moon1CathedralRestore] All 5 pieces risen — cathedral complete.`

---

## QA checklist (Cowork, after wiring)

- [ ] All 5 piece fields assigned in Inspector (no `null` warnings in console)
- [ ] All 5 pieces start inactive
- [ ] Triggering the cathedral restore makes pieces appear bottom-up in order
- [ ] No visible overlap during the rise — each piece is at its final localPosition when the next one starts
- [ ] Total sequence reads as ~12.5 seconds wall-clock
- [ ] Re-triggering mid-sequence cleanly restarts (no double-coroutine)

---

## Timing summary

| Beat | t (s) | Event |
|---|---|---|
| 0.0 | Foundation begins rise (1.0s SmoothStep over 4m) |
| 1.0 | Foundation seated; wait 1.5s |
| 2.5 | Walls begin rise |
| 3.5 | Walls seated; wait 1.5s |
| 5.0 | Roof begins rise |
| 6.0 | Roof seated; wait 1.5s |
| 7.5 | Buttresses begin rise |
| 8.5 | Buttresses seated; wait 1.5s |
| 10.0 | Spire begins rise |
| 11.0 | Spire seated — cathedral complete |

---

*cathedral-piecewise-setup.md · 2026-06-01 · Level Designer hand-off to Cowork*
