# Tuning Variant B — Waveform Trace — Audit (Content Lane 3)

**Branch:** `agent/c/variant-b-status` · **Date:** 2026-06-03 · **Worktree:** `C:\dev\_wt_c_variant_b`

## Verdict: REAL (variant implementation), BUT routing layer ignores it

The Variant B file exists, compiles, implements `ITuningVariant` with real method bodies, and fires the canonical completion event. However, the dispatch layer (`TuningPedestalLink` → `InteractableBuilding`) hardcodes Variant A (`TuningMiniGame`) and never instantiates B/C/D regardless of the `assignedVariant` enum value.

This was assigned to Content Lane 3 as an audit lane (no build required since the variant is real). The routing bug is documented here as a finding for whichever lane owns dispatch.

## File inventory (all 4 variants present + interface)

| Variant | File | LOC | Implements `ITuningVariant` |
|---|---|---|---|
| Interface | `Assets/_Project/Scripts/Gameplay/ITuningVariant.cs` | 25 | n/a |
| A — Frequency Slider | `Assets/_Project/Scripts/Gameplay/TuningMiniGame.cs` | 17 declares, has both `StartTuning(Vector3, Action)` and `StartTuning(TuningPuzzleConfig)` overloads | yes (line 17) |
| **B — Waveform Trace** | **`Assets/_Project/Scripts/Gameplay/TuningVariantB_Waveform.cs`** | **237** | **yes (line 17)** |
| C — Harmonic Pattern | `Assets/_Project/Scripts/Gameplay/TuningVariantC_Pattern.cs` | ~210 | yes (line 18) |
| D — Pipe Organ Puzzle | `Assets/_Project/Scripts/Gameplay/PipeOrganPuzzle.cs` | — | yes (line 31) |
| (alt D — Cymatic) | `Assets/_Project/Scripts/Gameplay/CymaticWaterTuningMiniGame.cs` | 897 | **NO** — does not implement `ITuningVariant`; fires `GameEvents.RaiseBuildingRestored` directly (line 575) |

## Variant B implementation — what's real

`Assets/_Project/Scripts/Gameplay/TuningVariantB_Waveform.cs` (237 LOC, no `// TODO`, no empty bodies):

- **Geometry per docs/15 §9 (Waveform Trace):** Golden sine wave scrolls horizontally (configurable `scrollSpeed`, `waveFrequency`, `waveAmplitude`); player cursor moves on Y axis. Accuracy = fraction of duration the cursor stayed within `tolerance` of the curve Y. 20-second duration, threshold 60% for success. (lines 26-32, 174-188)
- **Input:** Reads `Gamepad.current.rightStick.y`, `Mouse.current.delta.y`, and `Keyboard.current.upArrowKey/downArrowKey` as accessibility fallback (lines 144-162). Uses `UnityEngine.InputSystem`, no banned legacy `Input.GetKey`.
- **UI:** Auto-builds its own `ScreenSpaceOverlay` canvas (`TuningCanvas_VariantB`), 900x260 panel, golden status text, RawImage with a procedurally generated `Texture2D` of the wave + a white cursor pip (lines 66-134, 192-214).
- **Event surface (matches `ITuningVariant`):** Fires `OnFrequencyChanged(waveY01 * 1000f)` every frame for HUD parity (line 183), `OnTuningComplete(accuracy)` on success ≥0.6 (line 227), `OnTuningFailed()` on miss (line 233). Calls `TuningMiniGame.GetAccuracyTier(accuracy)` to share the same tier ladder as Variant A (line 220).
- **HUD banner:** `ServiceLocator.HUD?.ShowBanner("TUNED!", "<tier> - Waveform locked", 3f)` on success, `ShowBanner("FAILED", ...)` on failure (lines 226, 232).

## Variant B does NOT directly fire `GameEvents.RaiseBuildingRestored`

That is correct and matches the Variant A pattern: `InteractableBuilding.cs:91` subscribes `_tuningController.OnTuningComplete += OnTuningComplete`, and `InteractableBuilding.OnTuningComplete` fires `Core.GameEvents.RaiseBuildingRestored` (line 647). So if the dispatch layer had routed B properly, the restoration chain would fire end-to-end through the same path as A.

## Routing bug — out of scope for this lane, documented for hand-off

**`Assets/_Project/Scripts/Integration/TuningPedestalLink.cs:91-99`** (single source of dispatch from pedestal interaction):

```csharp
var mini = b.GetComponentInChildren<TuningMiniGame>(true);
if (mini == null) mini = b.gameObject.AddComponent<TuningMiniGame>();
mini.StartTuning(config);
```

The code reads `assignedVariant` only to pick `timeLimitSeconds` and `tolerancePercent` (lines 71-75), then **unconditionally instantiates `TuningMiniGame` (Variant A)** regardless of whether `assignedVariant == TuningVariant.WaveformTrace`, `HarmonicPattern`, etc. The fallback at line 98 also hardcodes `TuningMiniGame`.

**`Assets/_Project/Scripts/Integration/InteractableBuilding.cs:89`** likewise: `_tuningController = gameObject.AddComponent<TuningMiniGame>();` — hardcoded.

**Recommended fix (a future lane):** In both call sites, switch on `config.variant` (or `assignedVariant`) to add the right `MonoBehaviour` (`TuningMiniGame` / `TuningVariantB_Waveform` / `TuningVariantC_Pattern` / `PipeOrganPuzzle`). The receiving variant must accept a config-style entry point — Variant A has `StartTuning(TuningPuzzleConfig)` (line 267); B/C/D currently only expose `StartTuning(Vector3, Action)` so they'd either need a config overload, or the dispatcher needs to pass `transform.position` + a delegate that pumps `OnTuningComplete` → `RaiseBuildingRestored`.

## Conformance grep (all 4 variants share interface)

```
$ grep -rn ": ITuningVariant" Assets/_Project/Scripts/Gameplay/
PipeOrganPuzzle.cs:31:    public class PipeOrganPuzzle : MonoBehaviour, ITuningVariant
TuningMiniGame.cs:17:    public class TuningMiniGame : MonoBehaviour, ITuningVariant
TuningVariantC_Pattern.cs:18: public class TuningVariantC_Pattern : MonoBehaviour, ITuningVariant
TuningVariantB_Waveform.cs:17: public class TuningVariantB_Waveform : MonoBehaviour, ITuningVariant
```

Four implementations. `CymaticWaterTuningMiniGame` is a separate Cymatic-Water mini-game (P1.L5, 897 LOC) that fires `RaiseBuildingRestored` directly and is not part of the `ITuningVariant` hub — that's a known design choice from Sprint 9, not a regression.

## Summary

- Variant B (Waveform Trace) is REAL, no stubs, mirrors Variants A/C/D event surface.
- All 4 ITuningVariant implementers share a single interface and accuracy tier helper.
- Dispatch layer (`TuningPedestalLink.cs:91`, `InteractableBuilding.cs:89`) hardcodes `TuningMiniGame` and ignores `assignedVariant` — **none of B/C/D ever gets instantiated** by the live pedestal flow. Fixing the dispatcher is a separate lane's work.
