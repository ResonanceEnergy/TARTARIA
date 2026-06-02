# 30-Minute Soak Test Template

> Fill this in after running the soak. Triggered from Unity via
> `Tartaria → 9 QA → Run 30-Min Soak Test` (Editor menu, must be in Play mode).
> The soak controller writes a machine-readable report to
> `%APPDATA%/../LocalLow/<Company>/<Product>/soak30min_report.txt`
> (i.e. `Application.persistentDataPath/soak30min_report.txt`).
>
> Copy the numbers from that file into the fields below, then mark pass/fail.

---

## Run metadata

| Field | Value |
|---|---|
| Run start time (local) | `YYYY-MM-DD HH:MM:SS` |
| Run end time (local) | `YYYY-MM-DD HH:MM:SS` |
| Unity version | `e.g. 6000.0.32f1` |
| Scene under test | `Echohaven_VerticalSlice.unity` |
| Operator | `name` |
| Build flavor | `Editor / Standalone Mono / Standalone IL2CPP` |
| Reportfile copied from | `…/soak30min_report.txt` |

---

## Scripted sequence (for reference — do not edit)

| Phase | Window (s) | What the controller does |
|---|---|---|
| 1 Walk | 0 – 60 | Player transform lerps through 8 village waypoints |
| 2 Building 1 — Fountain | 60 – 180 | `GameEvents.FireBuildingRestored("Fountain")` at t = 120, 140, 180 |
| 3 Building 2 — Dome | 180 – 360 | `FireBuildingRestored("Dome")` at t = 240, 280, 340 |
| 4 Building 3 — Cathedral | 360 – 540 | `FireBuildingRestored("Cathedral")` at t = 420, 470, 520. `OnMoonCompleted` expected after node 3 |
| 5 Post-win idle | 540 – 720 | No-op, sample frame times |
| 6 Pause cycles x10 | 720 – 1080 | Reflective `PauseMenu.Instance.Toggle()` — 10 pause + 10 unpause |
| 7 Save/load cycles x5 | 1080 – 1440 | Reflective `SaveManager.Instance.QuickSave()` then `QuickLoad()` x5 |
| 8 Final idle | 1440 – 1800 | No-op, sample frame times |

---

## Measured results

Paste from `soak30min_report.txt`:

| Metric | Value |
|---|---|
| Total duration (s) | `1800.X` |
| Frame samples | `X` |
| Avg frame time (ms) | `X.XXX` |
| P95 frame time (ms) | `X.XXX` |
| Exception count | `X` |
| Error count | `X` |
| `OnMoonCompleted` fired | `FIRED / NOT FIRED` |

### First exception stack traces

```
(paste from the "First exception stack traces" section of soak30min_report.txt — max 5)
```

---

## Pass / Fail

Pass criteria (all must hold):

- [ ] Exceptions == 0
- [ ] Errors < 5
- [ ] Avg frame time < 16.6 ms (60 FPS budget)
- [ ] Scripted run reached 1800 s without Unity crash
- [ ] `OnMoonCompleted` fired during Cathedral phase

**Verdict:** `PASS / FAIL / INCONCLUSIVE`

**Verdict explanation:** _(one sentence — e.g. "Exceptions 0, errors 2, avg 12.4 ms — PASS")_

---

## Notes

_(anything noteworthy: Editor warnings observed, save file paths created, any
manual intervention required to start the run, any phase that visibly hitched,
hardware spec if relevant for frame-time interpretation, etc.)_

---

*Template owned by QA Engineer agent. Source: `Assets/_Project/Scripts/Tests/Soak30Min.cs`.*
