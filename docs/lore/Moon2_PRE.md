# Moon 2 — Pre-Production Design Brief

*Status: PRE (pre-production). Moon 1 still in flight per `PHASE_1_SCOPE.md`. This brief exists so the narrative pipeline has a target shape when Moon 2 enters active build.*

---

## 1. Zone identity

**Moon name:** Moon 2 — *The Hollow Tide*
**Zone name:** **Tideheart**

Rationale for `Tideheart` over `Drownwell`:

- *Drownwell* leans on the same downward, suffocating semantics as Echohaven's mud and would feel like a wetter rerun of Moon 1.
- *Tideheart* keeps the water register but introduces a *pulse* — tides have rhythm, and rhythm is the thematic spine of the Aether bands (every Moon is tuned to a frequency). The city's heart still beats; the player's job is to make it beat in time again.
- The name also carries a wistful, mythic register without political flavour — important per the CLAUDE.md sensitivity callouts. There is no Romanov echo, no "reset" language, no parasite imagery.

**One-line pitch:** *Tideheart is a flooded canal city that has forgotten how to breathe with the moon. The water lies still where it should rise. The player tunes the channels back into rhythm before the high tide that comes only once every Tartarian month drowns the last lit lantern.*

---

## 2. Aether band

**Harmonic — 432 Hz — blue/water.**

Per CLAUDE.md the three bands are Telluric (7.83 Hz, brown/earth, Moon 1's register), Harmonic (432 Hz, blue/water), and Celestial (528 Hz, gold/light). Tideheart's identity is water in motion, so Harmonic is the natural fit and gives the player a band they have not yet tuned with in Moon 1.

Player-facing consequence: the resonance pulse and tuning mini-games in Tideheart all centre on 432 Hz with sweep ranges of roughly 410–460 Hz. Aether Vision tints the world blue here instead of the brown-gold cast it had over Echohaven's mud.

---

## 3. Three hero buildings to restore

Each hero building anchors one act of Moon 2 and gates the next. All three are derelict water-architecture, not religious — Moon 1 owned the cathedral motif.

1. **The Tide Lock**
   - Function in lore: the great sluice gate that once let the outer ocean's tide enter the inner canals on a controlled rhythm.
   - State at arrival: jammed shut and half-buried in silt; the channels behind it are stagnant.
   - Restoration mechanic: player tunes the lock's resonance ring to 432 Hz to free the gate, then opens it in time with the moonrise. First running water in the city since the Hollowing.

2. **The Canal Spire**
   - Function in lore: a tall, slender lighthouse-bell whose chime once called the tide in and out. The spire is what *named* the city — Tideheart, the heart that beats with the spire's chime.
   - State at arrival: bell missing (sunk in the canal at its base), spire walls weeping brackish water.
   - Restoration mechanic: dive into the canal, retrieve the bell, re-hang it, and tune its sustain so its strike harmonises with the Tide Lock's rhythm.

3. **The Aqueduct Heart**
   - Function in lore: the underground confluence where the three main canal arteries meet and the city's potable water was once filtered through tuned-crystal weirs.
   - State at arrival: the Heart is the source of the Flood Wraiths — the crystal weirs have been shattered for so long the stagnant water has *learned a shape*.
   - Restoration mechanic: re-seat the three crystal weirs at their correct frequency offsets so the confluence pulses cleanly. Cleanses the Heart and resolves Moon 2.

---

## 4. Antagonist — The Flood Wraiths

Counterpart to Moon 1's Mud Golems. Where the Golems were earthen, slow, and accreted from buried debt, the Wraiths are fluid, fast, and accreted from *stilled* water — water that has been still so long it forgot it was water.

Visual: humanoid silhouettes the colour of deep canal water, rimmed in a faint blue Aether shimmer. They have no fixed surface; they bloom and collapse like a pour. Movement is gliding, not walking. They cannot leave the water more than a few metres — drawing them onto dry stone is a viable combat tactic.

Sound: a low, sustained tone roughly a quarter-tone below 432 Hz. The dissonance is intentional — when the player nears one, the audio mix bends until the Wraith is destroyed or fled.

Lore framing: the Wraiths are not malicious. They are *grief* in the water. They formed because Anastasia, when she fell at the end of Moon 1, sang the harmonic that should have called Tideheart's tide and the city heard her and tried to answer — but the channels were blocked and the answer had nowhere to go. The Wraiths are that unspoken answer. Resolving Moon 2 lets them dissolve, not die.

This framing keeps the antagonist mournful rather than evil and avoids the "monster" register the Mud Golems already occupy.

---

## 5. Central puzzle — the drained channels

Moon 1's core verb was *excavation*: digging mud out of buried structures. Moon 2 inverts it. The verb is **re-flooding**: most of Tideheart's channels are bone-dry, and the player has to send water back into them at the right frequency for each channel to hold.

Mechanical loop per channel:

1. Find the channel's silent **resonance stone** at its head.
2. Aether Vision reveals the channel's native frequency (e.g. 428 Hz, 435 Hz — all close to but not exactly 432 Hz; each channel is a slight detuning of the band).
3. Player tunes the stone with the same D-Pad ←/→ frequency-adjust the tuning mini-game already uses (per `docs/15 §9` mini-game variants).
4. Water *answers from the source* — a slow blue flood crawls down the channel, audibly humming at the tuned frequency.
5. If the player tuned wrong, the water enters but goes stagnant within seconds, spawning a Flood Wraith. This is the punishment loop and also the tutorial for what stagnant water *is*.

There are nine channels in the city; three must be restored to gate each hero building, in sequence.

This is mechanically distinct from Moon 1's mud excavation, so the player learns a new verb, but it reuses the same tuning UI and frequency-adjust controls, so we don't add new control surfaces.

---

## 6. Climactic moment — the Tidal Alignment

Moon 1's climax is the 17th Tartarian hour. Moon 2's equivalent is the **Tidal Alignment** — the once-per-Tartarian-month moment when all three of Tideheart's inner moons (small captive sub-moons that orbit the city's inner sky) line up and pull the deep ocean inward.

Beats:

1. Player has restored the three hero buildings.
2. The Aqueduct Heart's pulse, the Canal Spire's chime, and the Tide Lock's gate must fire in sequence on the alignment.
3. The player stands at the Spire and strikes the bell at the exact frequency. If on-time, the Tide Lock releases, the Heart pulses clean, and the entire city floods correctly for the first time in centuries — a slow, beautiful inrush, not a disaster.
4. If late, the tide arrives anyway but rough and unfiltered. The city still survives (no failure state — this is a restoration game, not a soulslike) but the player misses the alignment cinematic and Lirael's reveal at the end is shorter.

This mirrors Moon 1's 17th-hour structure (a single timed climactic input) without copying it — Moon 1 was a *pulse*, Moon 2 is a *chime + release sequence*.

---

## 7. Cassian's role — the escalation

Cassian closed Moon 1 by stepping into the cathedral as the dust settled and threatening Lirael and the player. In Moon 2 he is not present in person yet — that's saved for Moon 3 — but his *voice* is. The Bureau has reached Tideheart ahead of the player.

Specific beats:

- Three of the nine channels are found already *poisoned* — re-tuning them does nothing because someone has placed a small Bureau-stamped iron rod in the resonance stone. The rods read 0 Hz: a deliberate silence. The player must pull the rod before tuning. This is the player's first concrete evidence the Bureau is *physically* sabotaging restoration, not just monitoring it.
- Cassian's voice carries from the water itself in the mid-Moon hand-off (see `moon2_intro.yarn` line set 12–15). The line is mournful and warning, not gloating — he tells the player Anastasia's awakening has woken *others*, and the Bureau is no longer the only thing pursuing them.
- He never appears on screen in Moon 2. The last shot of the Moon shows a Bureau dinghy retreating into fog at the city's outer ring. He was here. He left before the alignment. He will be at Moon 3.

The escalation is in the *evidence* he leaves behind, not in a combat encounter.

---

## 8. Lirael's role — the carrier of moonlight

Moon 1 ended with Lirael appearing as Anastasia fell, picking up a fragment of moonlight from her mother's hand. In Moon 2 she is in Tideheart already when the player arrives, ankle-deep in a flooded plaza, her hair wet, her dress soaked at the hem.

The visual change is the point: between Moon 1 and Moon 2, Lirael has *been somewhere wet*. She does not say where. She does not remember all of it.

Specific beats:

- First sighting (yarn lines 1–3): she is on the far side of a broken bridge, looking at the canal as if listening to it. She does not see the player at first.
- Mid-Moon: she helps with the Canal Spire restoration by remembering the bell's strike pattern — a melody she should not know.
- End-of-Moon (post-alignment): she touches the water at the Aqueduct Heart and the blue Aether shimmer travels *up her arm*. She says: *"It moves through me. I think it has been waiting for somewhere to go."*
- This is the player's first hint that Lirael is carrying Aether between Moons — that she is the thread, not a companion who happens to be present in each zone.

This sets up Moon 3's reveal (where she carries the Celestial band into the next city) without spelling it out here.

---

## 9. Scope alignment with CLAUDE.md mandates

- **No new GameEvents.** All Moon 2 systems route through the canonical events listed in `docs/agents/API_CONTRACT.md` (`OnBuildingRestored`, `OnQuestStatusChanged`, `OnMoonCompleted`, `OnTartarianHourChanged`). The Tidal Alignment uses `FireSeventeenthHour()` semantics or a sibling fire method to be added in code-phase, NOT a new event in lore docs.
- **No banned namespaces.** Code phase for Moon 2 will use `Tartaria.Moon2.Tideheart.*` — none of `Time`, `Input`, `Camera`, `Animation`, `Random`, `Object`, `Color` appear in the namespace tree.
- **No political flavour.** No Bureau-as-cabal language, no reset-agent naming, no Romanov-by-stealth references. Cassian is a man doing a job he believes in. The Bureau is wrong about Aether, not evil about it.
- **No stubs.** When Moon 2 enters build, every building, NPC, and mini-game variant ships fully implemented per the 2026-05-30 late-night mandate.
- **Out of scope until Moon 1 ships.** This brief is *pre*-production. It does not unblock Moon 2 build work while `PHASE_1_SCOPE.md` is still in flight.

---

*Moon2_PRE.md v0.1 · 2026-06-01 · Pre-production brief. Update when Moon 1 ships and Moon 2 enters active build.*
