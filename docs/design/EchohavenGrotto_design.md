# Echohaven Hidden Grotto — Design Doc

*Moon 1 secret area. Owner: Level Design. Date: 2026-06-02.*

> A still-water cavern hidden behind the Crystal Spire. Revealed only after the
> player restores the Spire. Houses the second Giant Skeleton Key and the
> first concrete hint of the Moon 2 portal.

---

## 1. Purpose in the Moon 1 arc

The Grotto is the reward beat for finishing the Spire restoration loop. It
gives Moon 1 a real ending — not a dialog line and a fade, but a place the
player physically walks into and discovers. It also seeds Moon 2: the sealed
east-wall door is the first time the player sees a portal that *isn't* part
of the Aetheric overworld. Their next question becomes "how do I open that?"
which is exactly the question Moon 2 answers.

Narratively, the Grotto is the literal "heart of resonance" the Lorebook keeps
referencing. The still pool is the silent counterpoint to the Spire's
active radiance — Telluric grounding to the Spire's Celestial reach.

---

## 2. Trigger / reveal

- Grotto entrance is GEOMETRICALLY present from level start (door collapsed
  inward, blocked by Tartarian rubble) but the rubble has the
  `RestorationGated` tag so it cannot be cleared by Resonance Pulse pre-Spire.
- On `GameEvents.OnBuildingRestored` firing with `buildingId == "CrystalSpire"`
  (or the canonical Moon 1 spire id used by `BuildingRestorationCeremony`),
  the rubble dissolves via the existing restoration VFX (Hovl shockwave +
  dust) and the cavern entrance is revealed.
- When the player enters a 2 m radius of `grottoEntrancePosition`,
  `Moon1HiddenGrotto.OpenGrotto()` runs the HUD banner +
  Lorebook + reward bundle. After the banner, the player crosses the
  threshold into the cavern proper.

`Moon1HiddenGrotto.cs` owns the trigger logic. Scene authoring places no new
prefabs — the Grotto props are spawned at runtime by sibling Moon1 builder
scripts in a later sprint (out of scope for this design pass).

---

## 3. Layout — 8 m circular cavern, 3 m vertical clearance

```
                  N (glyph wall)
                   _____________
                  /  inscription \
                 /   inscription  \
                /    inscription   \
   W (rubble   |    .  .  .  .  .   |  E (sealed
    debris,    |   .   .  pool   .  |   portal-tease
    nav-       |  .  .   4 m diam.   |   door —
    blocked    |  .  .            .  |   Moon 2 hint)
    once       |   .   .         .   |
    opened)    |    .   .  .  .  .   |
                \    OBELISK +KEY   /
                 \   (centre, 1m)  /
                  \_______________/
                  S (entrance from
                     restored door)
```

- **Floor:** wet-mud plane, slightly concave toward the pool. Subtle
  parallax mapping with damp-stone normals.
- **Walls:** cathedral-kit stone blocks (existing Moon 1 KayKit), dressed
  with `Tartarian_Glyph_*` decals on the north wall.
- **Ceiling:** 3 m of carved rock with hanging stalactites
  (re-use existing cathedral debris prop set).
- **Entry:** south arch, 2.2 m tall, leads directly back to the Spire base.

---

## 4. Props (descriptive only — no .cs in this design pass)

| Prop | Where | Notes |
|---|---|---|
| Still pool | centre of room, 4 m diameter, 0.4 m deep | mirror-flat surface, low blue self-illumination ramp (Aether Vision shows it glowing brighter). No ripples — silence is the point. |
| Tartarian glyph wall | north wall | three-row carved script, decal layered onto the cathedral-kit stone. The three rows are the three inscriptions in §5. |
| Centre obelisk | dead centre, ~1 m tall, 0.4 m square base | basalt finish, plinth for the Skeleton Key. Faint emissive seam along vertical edges. |
| Giant Skeleton Key #2 | resting on obelisk | re-uses `tools/blender/gen_giant_skeleton_key.py` model; slowly rotates and bobs. PickupInteractable component handles collection. |
| Portal-tease door | east wall | sealed wooden + iron door, Tartarian sigil glowing faint cyan in the centre. Interact prompt says "Sealed. The lock waits for a sigil you do not yet hold." (this is the Moon 2 hint — do not open in Moon 1.) |

---

## 5. Lore beats — three readable inscriptions

All three use the existing **LorebookCollectible** lane and append to the
existing Lorebook UI panel. Trigger is `PickupInteractable` with a small
`ProximityTrigger` set to E-to-read.

1. **North wall, top row** — *"The Spire sings outward; the pool listens
   inward. Both are required. Neither is enough."* (Telluric/Celestial
   thematic anchor.)
2. **North wall, middle row** — *"When the keepers fell, they hid not the
   keys but the locks. A locked door is a promise of return."* (Foreshadows
   the Moon 2 portal door + the broader 13-key arc.)
3. **North wall, bottom row** — *"Anastasia walked here once. The water
   remembered her footsteps for a thousand years and then forgot. Only the
   stone forgets nothing."* (Character beat — links Anastasia to Echohaven's
   deep history. Sets up Moon 2's flashback level.)

---

## 6. Audio

- **Ambient bed:** low drone pinned at the **Telluric band (7.83 Hz)** of the
  Cymatic Music Engine. This is the same band the Spire's Restored state
  uses for its grounding sub-bass — the Grotto is the indoor expression of
  that same ley-line frequency.
- **Foley:** sparse drip echoes (random 4–9 s interval), faint pool surface
  shimmer (filtered pink noise at -28 dB).
- **Glyph proximity:** when the player closes within 1 m of an inscription
  decal, a sub-audible 7.83 Hz pulse swells +3 dB for the read duration,
  then decays.
- **Key pickup:** Cymatic Engine briefly cross-fades into the Celestial
  528 Hz band (matches the Spire restoration reward stinger) and returns
  to Telluric over 4 s. This is the only Celestial moment in the Grotto.

No music. The Grotto is the quietest room in Moon 1 by design — it's where
the player's ears reset before Moon 2.

---

## 7. Reward bundle

On `OpenGrotto()` + Key pickup:

1. **Giant Skeleton Key #2** added to inventory (item id `key_giant_02`).
   Required for the Moon 4 lockbox sequence; Moon 1 only telegraphs that
   keys matter.
2. **Lorebook entry "Echohaven Grotto"** unlocked, containing the three
   inscriptions verbatim + a screenshot the engine takes when the banner
   fires.
3. **Permanent +5 % Telluric Aether regeneration** — applied as a passive
   modifier on the Aether Resonance System. This is the first permanent
   stat upgrade in the game; it teaches the player that secret zones pay
   real dividends.

---

## 8. Failure-state notes

- If the player triggers the proximity check before the Spire is restored
  (debug path / out-of-sequence load): `Moon1HiddenGrotto` logs the gate
  failure with the player position and the Spire-restored flag value, and
  no banner fires. The rubble stays in place.
- If the Spire is restored but `grottoEntrancePosition` is left at default
  (Vector3.zero), the bootstrap logs an error pointing at the SerializeField
  and the trigger never arms (rather than silently triggering at the world
  origin).
- Banner cannot re-fire. Once opened, the `_grottoOpened` flag persists
  for the session; Moon 1 progress persistence picks it up via
  `Moon1ProgressPersistence`.

---

## 9. Out of scope for this design pass

- Visual prefab assembly of the cavern geometry (will be a follow-up
  Moon1Grotto builder script in `Scripts/Integration/`).
- The Moon 2 portal door open animation + transition (belongs to Moon 2
  design).
- Combat encounter inside the Grotto — there is intentionally none. The
  Grotto is a contemplative room.

---

*Echohaven Grotto design v1.0 · 2026-06-02 · Level Design agent.*
