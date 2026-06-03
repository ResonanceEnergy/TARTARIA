# Win64 Build Smoke — Phase 7.3 Readiness

> Hammer Lane 10 deliverable. Branch: `agent/h/win64-build-smoke`. Status: **READY (validation-only pass)** — Unity batchmode invocation has NOT been executed because Unity is currently open on `C:\dev\TARTARIA_new` and holds a Library lock that prevents a second editor from opening on the same project.

---

## What "READY" means

The build script `Assets/_Project/Scripts/Editor/Moon1ItchBuild.cs` was audited end-to-end and matches the smoke runner's expectations. The smoke runner `scripts/dev/build-moon1-win64-smoke.ps1` is committed and ready to invoke as soon as Unity is closed.

### Build script audit (file:line cites)

`Assets/_Project/Scripts/Editor/Moon1ItchBuild.cs`:

| Concern | File:line | Verdict |
|---|---|---|
| `BuildPipeline.BuildPlayer` is called | `Moon1ItchBuild.cs:97` | OK |
| Target = `StandaloneWindows64` | `Moon1ItchBuild.cs:92` | OK |
| Output path = `Builds/Win64/TARTARIA_Moon1.exe` | `Moon1ItchBuild.cs:25,27,67` | OK — matches CLAUDE.md convention |
| Zip path = `Builds/itch_assets/TARTARIA_Moon1.zip` | `Moon1ItchBuild.cs:26,28,68` | OK — matches CLAUDE.md convention |
| Scenes hardcoded by file existence | `Moon1ItchBuild.cs:75-82` | OK — picks Boot + Echohaven only, ignoring Moon 2-13 |
| Batchmode entry exits with internal code | `Moon1ItchBuild.cs:49-58` | OK — PowerShell wrapper can read `$proc.ExitCode` |
| Manifest sidecar written | `Moon1ItchBuild.cs:132-144` | OK — captures git-traceable build metadata |
| No silent catches | `Moon1ItchBuild.cs:149-154` | OK — catch logs `Debug.LogError` and returns code 9 |

### Scene list verification

Cross-checked `ProjectSettings/EditorBuildSettings.asset` against `Moon1ItchBuild.cs`:

- `EditorBuildSettings.asset:9-13` lists 15 enabled scenes (Boot, Echohaven_VerticalSlice, 12 Moon2-13 scenes, UI_Overlay).
- `Moon1ItchBuild.cs:29-30,75-82` explicitly picks **only** `Boot.unity` + `Echohaven_VerticalSlice.unity` via `File.Exists` checks, ignoring the EditorBuildSettings list.
- This is the correct behavior for a Moon 1 ship — the 12 Moon2-13 scenes are stub-grade per the Sprint 11 honest reset and would bloat the build with no playable content.

Both target scenes verified present on disk:
- `Assets/_Project/Scenes/Boot.unity` (guid `e239cbf810d53fa4aae6a52d6b675175`)
- `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` (guid `be7de6ea1a4cde148bc5325528a52a9a`)

### New-asset compatibility (HUD_Root etc.)

The build script does not enumerate any prefab or asset by hardcoded path. `BuildPipeline.BuildPlayer` walks the scene + Resources + Addressables graph automatically, so:

- HUD_Root.prefab (when added) will be picked up if it is referenced from Boot or Echohaven, or lives under `Resources/`, or is registered as an Addressable.
- No code change to `Moon1ItchBuild.cs` is required to accommodate new prefabs.
- One concern: a "missing prefab" reference logs as a build warning (not error) and produces a broken EXE silently. Mitigation lives outside this lane (Editor-side prefab audit, separate sprint).

---

## How to run

1. **Close Unity Editor** on `C:\dev\TARTARIA_new` (or any project rooted at this worktree).
2. From the repo root:
   ```powershell
   .\scripts\dev\build-moon1-win64-smoke.ps1
   ```
3. Optional flags:
   ```powershell
   .\scripts\dev\build-moon1-win64-smoke.ps1 -UnityVersion "6000.3.6f1"
   .\scripts\dev\build-moon1-win64-smoke.ps1 -TimeoutSeconds 1800
   .\scripts\dev\build-moon1-win64-smoke.ps1 -DryRun     # validates setup, does NOT invoke Unity
   ```

### What the smoke runner does

| Step | Action | Failure exit code |
|---|---|---|
| 1 | Capture git SHA, branch, environment | 1 |
| 2 | Refuse to run if Unity.exe is open on this project root | 2 |
| 3 | Locate `Unity.exe` (6000.3.6f1 default) | 3 |
| 4 | Pre-clean `Builds/Win64/`, zip, manifest, log | (continues) |
| 5 | Invoke Unity `-batchmode -nographics -executeMethod Tartaria.Editor.Moon1ItchBuild.BuildWin64` | 4 |
| 6 | Verify `Builds/Win64/TARTARIA_Moon1.exe` exists | 5 |
| 7 | Verify `Builds/itch_assets/TARTARIA_Moon1.zip` exists and is 50 MB <= size <= 4 GB | 6 |

On success (exit 0), it tails `Builds/itch_assets/build_manifest.txt` to stdout.

On failure inside Unity (exit 4), it tails the last 100 lines of `Logs/win64_smoke.log` so the operator can diagnose.

---

## Concerns / follow-ups

1. **HUD_Root.prefab not present yet** — `Glob HUD_Root.prefab` returns no match across `Assets/`. When Lane work lands a HUD_Root.prefab, the build smoke will continue to pass (it doesn't reference HUD_Root by name), but a separate audit should confirm the prefab's serialized references resolve.
2. **`Library/UnityLockfile` heuristic** — the Unity-running guard checks both running processes AND the lockfile presence; the lockfile may persist after a crash. The script warns and continues in that case rather than refusing, on the theory that the second-Editor error from Unity itself is a more reliable signal.
3. **Itch validation lower bound (500 MB) vs smoke lower bound (50 MB)** — this smoke runner uses 50 MB because the post-Phase 0-5 build is expected to be slim (many vendor folders are not yet wired in). `scripts/dev/itch-smoke-test.ps1` enforces the stricter 500 MB itch-quality bound and remains the gate for ship.
4. **Worktree state at write time** — the `_wt_h_win64` worktree is sparse and the `_Project` folder is not materialized in the working tree. The branch tip on `origin/agent/h/win64-build-smoke` carries only the smoke runner + this doc; the runner is intended to be invoked from a fully checked-out worktree (e.g., `C:\dev\TARTARIA_new` after Unity is closed) by way of branch merge into the main worktree.

---

*Phase 7.3 Hammer Lane 10 · 2026-06-02*
