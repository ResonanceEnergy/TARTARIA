# Dialogue Completeness Audit — 2026-05-31

Read-only audit. Spec references: `docs/15_MVP_BUILD_SPEC.md` §10 (Milo), `docs/18_PRINCESS_ANASTASIA.md` (Anastasia), `CLAUDE.md` task spec (Lirael/Cassian).

## Yarn / Dialogue-asset inventory (project, excluding `Library/PackageCache`)

| Path | Spoken-line count | Nodes |
|---|---|---|
| `Assets/_Project/Data/Dialogue/Milo_Intro.yarn` | 6 (Milo) | `Milo_Intro` (older/duplicate draft) |
| `Assets/_Project/Dialogue/Moon1/milo_intro.yarn` | 11 (Milo) | `milo_intro`, `milo_warming_up`, `milo_sincere` |
| `Assets/_Project/Dialogue/Moon1/lore_whispers.yarn` | 12 (Stone — environmental, not character) | 6 lore-stone nodes |
| `Assets/_Project/Dialogue/Moon1/anastasia_greeting.yarn` | 10 (Anastasia) | `anastasia_greeting`, `anastasia_dome_restored`, `anastasia_fountain_restored` |
| `Assets/_Project/Config/Dialogue/Dialogue_Anastasia_AwakenStarDome.asset` | 4 (Anastasia, ScriptableObject) | `AnastasiaDialogueDatabase` — id 1–4, all `awaken_star_dome_*` contexts |
| `Assets/_Project/Config/AnastasiaDialogue.asset` | 224 entries (Anastasia, all-moons SO bank) | `AnastasiaDialogueDatabase` — spans Moons 1–13 |

No `.yarnproject` files exist outside `Library/PackageCache`. The Yarn runtime adapter is `Assets/_Project/Scripts/Integration/YarnDialogueAdapter.cs`; the SO bank is consumed by `Tartaria.Integration.AnastasiaDialogueDatabase` (script GUID `783c8e7f21e8206409406032bcc04911`).

## Spec vs actual

| Character | Spec target | Actual (project) | Verdict |
|---|---|---|---|
| **Anastasia — `AwakenStarDome` quest** | 4 lines minimum, silence-first, "presence not demand" | 4 lines in `Dialogue_Anastasia_AwakenStarDome.asset` (ids 1–4: intro / lore / prompt / task). Tone matches spec — restrained, second-person, no exposition dump. Also 10 lines in `anastasia_greeting.yarn` + 224 entries in master SO bank. | **COMPLETE** (minimum hit). The four AwakenStarDome lines are written to spec. |
| **Milo — 40 total** (10 tutorial / 8 discovery / 8 lore / 8 ambient / 4 combat / 2 celebration) | 40 lines, 6 categorised buckets | 11 lines in `milo_intro.yarn` + 6 in older `Milo_Intro.yarn` duplicate. No category buckets present, no combat lines, no celebration node, no ambient idle pool, no discovery-on-POI lines beyond the intro. Net unique ≈ 11. | **STUB** — 11 / 40 (28%). Five of six categories absent. |
| **Lirael — Spectral Architect, Day 25-28 reveal** (432 Hz lullaby hum + "why grown-ups build houses then live in the attic" line) | First-pass reveal dialogue + signature lines | Zero Yarn nodes. `LiraelLullaby.cs` exists (audio-only behavior); `LiraelController.cs.disabled`, `LiraelControllerComplete.cs.disabled`. Quests reference her (`Quest_lunar_side_lirael`, `Quest_cathedral_lirael`, `Quest_r7_m1_lirael_calendar_echo`, `Quest_r7_m2_lirael_crystal_choir`) but no dialogue text is wired. `Lirael.prefab.corrupt` — prefab is broken. The signature "attic" line and 432 Hz lullaby line do not appear in any `.yarn` or `.asset` searched. | **MISSING** |
| **Cassian — foreshadow ally, doesn't yet contradict** | First-pass foreshadow lines | Zero Yarn nodes. `CassianController.cs.disabled`, `CassianNPCController.cs.disabled`, `Moon2CassianArrival.cs` exists. Quests exist (`Quest_echo_companion_cassian`, `Quest_cassian_confrontation`, `Quest_star_fort_cassian`, `Quest_r7_m2_cassian_*`) but no dialogue text. `Cassian.prefab.corrupt`. | **MISSING** |

## Missing dialogue, by character and context

**Milo (29 lines needed):**
- Tutorial (need 10, have ~3 from intro): scan with Aether Vision; tuning frequency adjust D-Pad ←/→; resonance pulse on enemy; shield on LT; sprint LB; aether-vision Y; restore-mode interact; map ping; lore-stone interact; save-at-inn.
- Discovery (need 8, have 0): POI proximity on each of 6 lore stones + 3 hero domes + village wells.
- Lore (need 8, have 0): cross-references to `docs/01_LORE_BIBLE.md` (mud-flood, Tartarian song, 432 Hz origin, Listeners' choir, etc.).
- Ambient (need 8, have 0): idle chatter for `Milo.cs` IDLE state after 5 s of standing still.
- Combat (need 4, have 0): mud-golem warning, encouragement, low-HP, victory.
- Celebration (need 2, have 0): post-restoration jump/spin trigger lines.

**Lirael (first-pass needed):**
- Day-25 reveal beat (in `docs/18` she "hums 432 Hz lullaby").
- The signature line: "why do grown-ups build houses then live in the attic?"
- Days 26-28 escalation toward Crystal Choir / Crystalline Caverns hand-off.

**Cassian (first-pass foreshadow needed):**
- Moon 1 arrival cameo (per `Moon2CassianArrival.cs` reference).
- Navigator-flavored math/observation lines (per `docs/18` line 543: *"The navigator sees patterns I can feel but not name"*).
- Foreshadow — must read as ally, not yet contradict; later confrontation arc lives in `Quest_cassian_confrontation`.

## Recommended write order

1. **Milo Tutorial bucket (10)** — gates Moon 1 onboarding, highest player-touch frequency.
2. **Milo Combat (4) + Celebration (2)** — unblocks mud-golem encounter and restoration loop feedback.
3. **Milo Discovery (8) + Lore (8) + Ambient (8)** — fills the 40-line target.
4. **Cassian Moon 1 foreshadow lines** — needed before Moon 2 arrival cinematic plays.
5. **Lirael Day-25 reveal + signature lullaby/attic lines** — Moon 1 closer; without these the Cathedral quest reads cold.
6. Resolve `Lirael.prefab.corrupt` / `Cassian.prefab.corrupt` (prefab repair, not dialogue) before wiring new nodes to NPCs.

## Note on duplication

`Assets/_Project/Data/Dialogue/Milo_Intro.yarn` (6 lines, capital-M, `<<declare $rs = 0>>` resonance variable) and `Assets/_Project/Dialogue/Moon1/milo_intro.yarn` (11 lines, `$milo_met` flag) cover overlapping intro content. `Moon1DialogueBindings.cs` only references the lowercase `milo_intro` node. Recommend archiving the older `Data/Dialogue/Milo_Intro.yarn` (or merging the `$rs` branch into the active file) before the Milo write-out begins, to avoid two competing first-meet beats.
