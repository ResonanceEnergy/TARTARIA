# TARTARIA — Build Guide (Beta Vertical Slice)

**Target Build:** Echohaven Vertical Slice — 15-30 min playable beta loop

---

## Prerequisites

### Required Software

- **Unity 6000.3.6f1** (exact version, URP 17.3.0 included)
  - Install via Unity Hub: `unityhub://6000.3.6f1/bbb010bdb8a3`
  - Modules: Windows Build Support (IL2CPP)
- **Git** (for cloning + version control)
- **PowerShell 7+** (for build automation scripts)

### Hardware Requirements

| Tier | CPU | GPU | RAM | Target Performance |
|------|-----|-----|-----|-------------------|
| **Minimum (Low)** | 4-core 2.5 GHz | GTX 1050 / 4 GB | 8 GB | 30 fps @ 720p-1080p |
| **Recommended (Medium)** | 6-core 3.0 GHz | GTX 1070 / 8 GB | 16 GB | 60 fps @ 1080p |
| **High** | 8-core 3.5 GHz | RTX 3060 / 12 GB | 16 GB | 60 fps @ 1440p |
| **Ultra** | 12-core 4.0 GHz | RTX 4070+ / 16 GB | 32 GB | 60 fps @ 4K |

---

## Quick Start (Developer Play Mode)

### 1. Clone Repository

```bash
git clone https://github.com/ResonanceEnergy/TARTARIA.git
cd TARTARIA
```

### 2. Open Project in Unity

- Launch Unity Hub
- Click **Add → Add project from disk**
- Select the `TARTARIA` folder
- Unity 6000.3.6f1 will open and import assets (~5 min first launch)

### 3. One-Click Build + Play

Run the automated build+play pipeline from PowerShell:

```powershell
.\tartaria-play.ps1
```

**What it does:**
- Closes Device Simulator (if open)
- Builds all scenes (Boot, Echohaven_VerticalSlice, 12 Moon stubs)
- Generates prefabs (buildings, VFX, player, companions)
- Bakes NavMesh + Adaptive Probe Volumes (APV)
- Runs 31 readiness checks (ServiceLocator, audio mixer, input, save system)
- Launches Unity in play mode
- **Expected:** CS:0, EXIT:0, "All checks passed. Ready to play." in ~90 seconds

**Build report:** `Logs\tartaria-build-report.txt`

**Editor log:** `%LOCALAPPDATA%\Unity\Editor\Editor.log`

---

## Build Modes

### Interactive Play Mode (Default)

```powershell
.\tartaria-play.ps1
```
- Builds + opens Unity Editor in play mode
- Full validation + gameplay testing
- Access to Unity Profiler, Scene view, Hierarchy debugging

### Headless Validation (CI/CD)

```powershell
.\tartaria-play.ps1 -BatchOnly
```
- Builds project headless (no Editor GUI)
- Runs BatchReadinessValidator (31 checks)
- Returns exit code (0=success, 1=fail)
- Use for GitHub Actions / automated testing

### Clear Cache + Rebuild

```powershell
Remove-Item -Recurse -Force Library\ShaderCache, Library\ArtifactDB, Temp -ErrorAction SilentlyContinue
.\tartaria-play.ps1
```
- Clears Unity cache to fix shader/import corruption
- Full reimport of all assets (~10 min)

---

## Creating Standalone Build (.exe)

### Via OneClickBuild (Editor Menu)

1. Open Unity Editor
2. **Menu → TARTARIA → One-Click Build & Play**
3. Build outputs to `Build/` directory
4. Executable: `Build\TARTARIA.exe`
5. Data: `Build\TARTARIA_Data\`

### Via Build Menu

1. **File → Build Settings**
2. **Target Platform:** Windows x64
3. **Architecture:** Intel 64-bit + SSE2
4. **Development Build:** OFF (for final beta package)
5. **Script Debugging:** OFF
6. **Compression:** LZ4 (faster) or LZ4HC (smaller)
7. **Click Build** → Select output folder

**Build time:** 3-5 minutes (depending on hardware)

**Build size:** ~2.5 GB (uncompressed), ~1.2 GB (compressed .zip)

---

## Performance Validation

### Run Performance Gates (M3 Milestone)

Performance gates validate 60 fps target on GTX 1070 baseline (Medium tier):

```powershell
# Close Unity Editor first (gates must run in clean batchmode)
cd C:\dev\TARTARIA_new

# Run CI perf gates
"C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe" `
  -projectPath . `
  -executeMethod Tartaria.Editor.Perf.PerformanceGateRunner.RunCIGates `
  -batchmode -quit `
  -logFile "Logs\perf-gate.log"
```

**Tested Scenes:**
- Echohaven_VerticalSlice
- CrystallineCaverns (Moon 2)
- WindsweptHighlands (Moon 3)

**Validation Thresholds (per `09_TECHNICAL_SPEC.md`):**

| Tier | Avg FPS | 1% Low FPS | Peak RAM | Load Time |
|------|---------|------------|----------|-----------|
| **Low** | ≥ 28 | ≥ 22 | ≤ 2.8 GB | ≤ 6s |
| **Medium** | ≥ 52 | ≥ 28 | ≤ 3.6 GB | ≤ 5s |
| **High** | ≥ 58 | ≥ 35 | ≤ 4.2 GB | ≤ 4s |
| **Ultra** | ≥ 60 | ≥ 45 | ≤ 5.5 GB | ≤ 3s |

**Results:** `Assets/_Project/Generated/CI_Results/R6_PerfGates_*.json`

---

## Project Structure

```
TARTARIA_new/
├── Assets/
│   └── _Project/
│       ├── Audio/                  # Procedural SFX, master mixer, ambience
│       ├── Config/                 # Volume profiles, input action maps
│       ├── Editor/                 # Build tools, scaffolds, validators
│       ├── Materials/              # URP shaders, building materials
│       ├── Prefabs/                # Buildings, characters, VFX
│       ├── Scenes/                 # Boot, Echohaven, 12 Moon stubs
│       ├── Scripts/                # 11 assemblies (see Architecture Guide)
│       ├── Tools/                  # Mixamo fetch, AmbientCG fetch, capture scripts
│       └── VFX/                    # Restoration VFX, scan pulses, shards
├── docs/                           # 30+ design documents (see README)
├── Logs/                           # Build reports, Editor logs, perf results
├── Packages/                       # URP, Input System, TextMeshPro, Addressables
├── ProjectSettings/                # Unity project config
├── Tools/                          # External utilities
├── README.md                       # Project overview
├── BUILD_GUIDE.md                  # This file
├── KNOWN_ISSUES.md                 # Active issue tracking
└── tartaria-play.ps1               # Automated build+play pipeline
```

---

## Assembly Architecture

**11 Assemblies** (strict dependency hierarchy, NO CYCLES):

```
Tartaria.Core (base)
    ↓
Tartaria.Input, Tartaria.Audio, Tartaria.Camera (parallel, no cross-deps)
    ↓
Tartaria.Gameplay (depends on Core + Input/Audio/Camera)
    ↓
Tartaria.AI (depends on Gameplay)
    ↓
Tartaria.UI (depends on Core + Gameplay + Input + Audio + Camera + Save)
Tartaria.Save (depends on Core only)
    ↓
Tartaria.Integration (top-level glue, depends on ALL)
    ↓
Tartaria.Editor (editor-only utilities)
```

**Cross-assembly communication:** ServiceLocator pattern (see `04_ARCHITECTURE_GUIDE.md`)

---

## Common Issues

### ❌ Compile Errors on First Load

**Symptom:** CS errors about missing types/namespaces after opening project

**Cause:** Unity 6 incremental compilation + assembly load order

**Fix:**
1. **Assets → Reimport All**
2. Wait for full compilation (~2 min)
3. If errors persist, close Unity → delete `Library/` folder → reopen

### ❌ "nul" File Git Error

**Symptom:** `git add` fails with "error: unable to index file 'nul'"`

**Cause:** Windows reserved filename `nul` in repo root (legacy artifact)

**Fix:** Always use `git add --ignore-errors -A` (built into build scripts)

### ❌ MainMenuOverlay UIElements Render Warnings

**Symptom:** Editor log shows `UIRenderDevice:EvaluateChain` errors during play mode

**Cause:** IMGUI/UIElements coexistence race condition on startup

**Status:** Non-fatal rendering warnings, fixed with `GUI.skin` null guard (commit `4e3decc`)

### ❌ Unity Batchmode Conflicts

**Symptom:** Can't run perf gates or headless builds while Editor is open

**Cause:** Unity locks project files when Editor is running

**Fix:** Close Unity Editor before running batchmode commands (`-batchmode -quit`)

### ❌ Missing Haptics/VFX on Gameplay Events

**Symptom:** Building restoration/tuning doesn't trigger rumble or VFX

**Cause:** HapticFeedbackManager or VFXController singleton not bootstrapped

**Status:** Fixed in Cycle 1 — both have `[RuntimeInitializeOnLoadMethod] Bootstrap()` (see KNOWN_ISSUES.md)

---

## Controls (PC)

**Keyboard + Mouse:**
- **WASD:** Movement
- **Mouse:** Camera look
- **Left Click:** Primary action (interact, attack)
- **Right Click:** Secondary action (scan, block)
- **Space:** Jump
- **Shift:** Sprint
- **E:** Interact with interactables (buildings, NPCs, quest objects)
- **F10:** Open Settings
- **F11:** Toggle MoonPortalSelector (debug warp to any Moon scene)
- **Esc:** Pause menu

**Gamepad (Xbox / PlayStation):**
- **Left Stick:** Movement
- **Right Stick:** Camera
- **A / Cross:** Jump
- **B / Circle:** Cancel / Back
- **X / Square:** Interact
- **Y / Triangle:** Scan
- **LB / L1:** Block
- **RB / R1:** Attack
- **LT / L2:** Sprint
- **RT / R2:** Giant Mode (when unlocked)
- **Start:** Pause
- **Select:** MoonPortalSelector toggle (debug)

**Haptic Feedback:** Full controller rumble support (tuning, restoration, combat, boss encounters). See `14_HAPTIC_FEEDBACK.md`.

---

## Testing Checklist (Beta Acceptance)

### ✅ M1: End-to-End Playable

- [ ] Boot → Main Menu → New Game loads Echohaven without errors
- [ ] Player spawns, camera + controls responsive
- [ ] Milo companion intro triggers
- [ ] Tutorial prompts guide to first building
- [ ] Restore Great Dome (Star Dome) — VFX + haptics fire
- [ ] Save checkpoint triggers after first restoration
- [ ] Continue from save resumes exact state

### ✅ M2: Menu/UX + Audio/VFX/Haptics Juice

- [ ] Main Menu gamepad navigation (D-pad + left stick)
- [ ] Settings overlay opens (F10 / gamepad Select button)
- [ ] Volume sliders functional (Master, Music, SFX, Ambience)
- [ ] Resolution + Quality + Fullscreen toggles apply correctly
- [ ] Haptics fire on: tuning success, building emergence, combat hit, moon clear
- [ ] VFX sync with gameplay events (restoration sparkle, scan pulse, shard collect)
- [ ] Adaptive audio responds to game state (exploration → combat → triumph)

### ✅ M3: Performance Gate (60 fps GTX 1070)

- [ ] PerformanceGateRunner passes Medium tier (≥52 avg FPS, ≥28 1%low, ≤3.6GB RAM)
- [ ] Standalone build launches without errors
- [ ] No frame drops during full restoration sequence + VFX
- [ ] Build report: CS:0, all 31 readiness checks passed

### ✅ M4: Final Polish + Docs + Beta Package

- [ ] README.md up to date
- [ ] BUILD_GUIDE.md comprehensive (this file)
- [ ] KNOWN_ISSUES.md current (no unaddressed P0 blockers)
- [ ] Windows build packaged as .zip with README
- [ ] All docs in `docs/` folder reviewed (30+ files)
- [ ] Beta package ready for closed testing distribution

---

## Support & Contact

- **GitHub:** [ResonanceEnergy/TARTARIA](https://github.com/ResonanceEnergy/TARTARIA)
- **Issues:** [GitHub Issues](https://github.com/ResonanceEnergy/TARTARIA/issues)
- **Docs:** All 30+ design documents in `docs/` folder

---

## License

**Proprietary** — All rights reserved. Beta build for internal/closed testing only.

---

*Last Updated: 2026-05-22 (Beta Vertical Slice Sprint, Commit `4e3decc`)*
