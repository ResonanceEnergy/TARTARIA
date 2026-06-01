# Moon 1 Mini-Game Readiness Audit (v2)

*Date:* 2026-05-31  ·  *Scope:* read-only audit of tuning variants A/B/C, Pipe Organ, InteractableBuilding, and scene pedestal wiring.

---

## 1. Variant A — Frequency Slider — `TuningMiniGame.cs`  →  **PARTIAL**

- UI auto-build path: complete. `EnsureUIBuilt()` creates a shared screen-space canvas (sortingOrder 32000), panel, target/current text, feedback meter, slider with full visuals + white-sprite fix. Confirmed working at runtime.
- Input: kb arrows / A-D + gamepad left stick, both feed `frequencySlider.value`. Good.
- Tolerance & timer: `tolerance = 5f` Hz on a ±100 Hz range = ±5% absolute (≈5% of range, **not** spec's "±8%"). Time limit field `timeLimit = 30f`, **not** spec's 15s. Spec §9 violated on both counts.
- Tier mapping (`GetAccuracyTier`) is correct: Perfect ≥ 0.95, Great ≥ 0.80, Good ≥ 0.60.
- **`GameEvents.OnTuningProgress` is never fired**, here or anywhere in the codebase. `OnFrequencyChanged?.Invoke(current)` fires a local C# event, but `GameEvents.FireTuningProgress(...)` is defined and unused. `AdaptiveMusicController.HandleTuningProgress` therefore never receives a value → Layer 2 cannot react.
- Signature drift: `InteractableBuilding.StartTuning` calls `_tuningController.StartTuning(new TuningPuzzleConfig{...})` and `_tuningController.StartTuning(definition.nodePuzzles[_nodesCompleted])` (lines 355 & 366). `TuningMiniGame.StartTuning` only exposes `(Vector3, System.Action)` — **this will not compile** as written. Either an overload was deleted or these call-sites were never compiled together.

## 2. Variant B — Waveform Trace — `TuningVariantB_Waveform.cs`  →  **STUB (orphan)**

- Class is wired internally: real `Texture2D` waveform (256×96) regenerated per frame in `RedrawWave()`, cursor moves on right-stick Y / mouse delta Y / up-down arrows, scoring tracks on-line time, 20s duration. The mini-game itself is playable.
- Implements `ITuningVariant`, fires `OnTuningComplete` / `OnTuningFailed`.
- **Not wired by any caller.** No `AddComponent<TuningVariantB_Waveform>()` anywhere; `InteractableBuilding.Start()` hard-codes `GetComponent<TuningMiniGame>()` (Variant A only). It is impossible to reach Variant B from gameplay.
- Does not fire `GameEvents.OnTuningProgress` either.

## 3. Variant C — Harmonic Pattern — `TuningVariantC_Pattern.cs`  →  **STUB (orphan)**

- 5 beat circles built in a row, highlight intensity lerps in as `dueAt` approaches, kb E / gamepad South captures press, color-codes Perfect ≤ 100 ms / Good ≤ 200 ms / OK ≤ 300 ms / Miss — windows match spec §9.
- Final scoring averages over 5 beats; passes ≥ 0.60.
- Same orphan status as Variant B — no caller, no `AddComponent`, no `OnTuningProgress` fire.

## 4. Pipe Organ centerpiece  →  **MISSING / BROKEN**

Two classes claim the name `Tartaria.Gameplay.PipeOrganMiniGame`:

- `Assets/_Project/Scripts/Gameplay/PipeOrganMiniGame.cs` — 308 lines, 7-pipe Tartarian organ keyed to digits 1-7, golden-ratio chord generation, `StartOrgan(PipeOrganConfig)`. Comment says "Moon 2 (Crystalline Caverns)".
- `Assets/_Project/Scripts/Integration/PipeOrganMiniGame.cs` — 64 lines, plays a hard-coded 5-note melody, `StartGame()` only. Same namespace `Tartaria.Gameplay`.

**Duplicate type in the same namespace = CS0101 at compile.** One must be deleted/renamed before either can run. `InteractableBuilding.TryStartBuildingMiniGame()` calls `.StartOrgan()`, which only the Gameplay-folder version has.

- `PipeOrganPuzzle.cs` (separate Solfeggio 7-pipe `ITuningVariant`) is the closest match to spec §7 + §9 "canonical first puzzle inside the Dome." It builds UI, plays sine-wave tones, previews the sequence, scores on time remaining. **Also orphan — never AddComponented, never gated by being inside the Dome.**
- Neither Pipe Organ class fires `OnSeventeenthHour`. `OnSeventeenthHour` is only fired by `TartarianHourCycle` on 17-hour wrap, and only on perfect timer accumulation — perfect-solve of the organ does **not** trigger it. Cathedral Light Eruption (`Moon1NarrativeBeats.HandleSeventeenthHour`) requires the hour wrap + 1 restored hero building; organ solve is not in the predicate.
- Visual prefab `Assets/_Project/Prefabs/Moon1/Blender/PipeOrganCathedral.prefab` is referenced **by string only** in `Moon1BlenderPrefabPlacer.cs:43` (`Place(root, "PipeOrganCathedral", ...)`). No script holds a typed `GameObject` reference; gameplay code does not load it via `AssetDatabase` / `Resources`.

## 5. `InteractableBuilding` node→variant dispatch  →  **PARTIAL**

- 3-node tracking is real: `_nodesCompleted++` in `OnTuningComplete`, `BeginEmergence()` fires after `_nodesCompleted >= 3`. Working.
- Variant dispatch is **not implemented at the component level**. `_tuningController` is unconditionally `TuningMiniGame` (Variant A). The fallback `TuningPuzzleConfig.variant = (TuningVariant)(_nodesCompleted % 3)` sets a field on a config object that the current `TuningMiniGame.StartTuning` overload doesn't accept — so Node 1/2/3 all run Variant A.
- No 5 m proximity prompt. `PlayerInputHandler.interactRadius = 3.0f` (line 35), spec asks 5 m. Prompt text comes from `GetInteractPrompt()`; surfacing via HUD is `RaiseHUDShowInteractionPrompt` in transitional events but not as a proximity-driven overlay.

## 6. Variant random assignment  →  **WRONG (deterministic, not per-spec)**

- `BuildingDefinitionCreator.CreatePuzzleConfigs(i)` assigns `variant = (TuningVariant)(i % 3)` — Node 0=A, Node 1=B, Node 2=C, deterministic, no randomization, no `Random.Range`, no "Node 2 = B-or-C / Node 3 = C-or-A" gating per docs/15 §9.
- The runtime fallback in `InteractableBuilding.StartTuning()` uses the same `_nodesCompleted % 3` formula. No flag, no randomness anywhere.

## 7. Scene pedestals  →  **NOT WIRED**

`Echohaven_VerticalSlice.unity` contains 9 `TuningPedestal_0..8` placements (matches `Moon1BlenderPrefabPlacer` triplet-per-hero-building layout). Grep returns 9 pedestal entries vs 7 `InteractableBuilding` script refs — pedestals are static prefab placements with no `InteractableBuilding`, no `ITuningVariant`, no `IInteractable`. They are decorative only. The 3-node restoration runs from the hero building's collider, not from any pedestal.

---

## Top 3 missing pieces for Pipe Organ end-to-end

1. **Delete or rename the duplicate `Tartaria.Gameplay.PipeOrganMiniGame`** in `Assets/_Project/Scripts/Integration/PipeOrganMiniGame.cs`. The codebase will not compile while both exist. Keep the 308-line Gameplay-folder version (real chord progression + RS reward).
2. **Wire `PipeOrganPuzzle` (or the chosen PipeOrgan class) into the Dome's `InteractableBuilding` as the Variant override for buildingId="dome"**, and load `PipeOrganCathedral.prefab` via `Resources.Load`/`AssetDatabase.LoadAssetAtPath` in the spawner so the visual centerpiece is the same GameObject that hosts the puzzle script.
3. **Make perfect organ solve raise `OnSeventeenthHour` (and/or a new `OnPipeOrganSolved`)** so `Moon1NarrativeBeats.HandleSeventeenthHour` triggers the Cathedral Light Eruption on solve, not only on day-cycle wrap. Today the eruption can only fire after ~8.5 real minutes of day-cycle ticking past hour 16 → 0, which decouples it from the puzzle.

## Other immediate blockers worth flagging

- `GameEvents.FireTuningProgress(offset)` is dead code; call it from every variant's `Update` so `AdaptiveMusicController.HandleTuningProgress` actually drives Layer 2.
- `InteractableBuilding.StartTuning()` references a `TuningMiniGame.StartTuning(TuningPuzzleConfig)` overload that does not exist on the current class — restore the overload or rewrite the call to pass `(Vector3, Action)`.
- Set `tolerance = 0.08 × targetFrequency` and `timeLimit = 15f` in `TuningMiniGame` defaults to match spec §9.
- Add Variant B/C `AddComponent` paths so node 2 / 3 can actually swap mini-games, and randomize per spec §9.
- Raise `PlayerInputHandler.interactRadius` to 5 m, and surface `GetInteractPrompt()` via the HUD overlay on proximity.
