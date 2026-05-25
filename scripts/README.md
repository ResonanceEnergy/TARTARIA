# Automation Scripts

This directory contains all PowerShell automation scripts for build, test, and development workflows.

## Categories

### Build Scripts (12 files)
- `build-beta.ps1` — Standard beta build
- `build-beta-final.ps1` — Production-ready beta build
- `build-beta-win64.ps1` / `build-beta-win64-mono.ps1` — Platform-specific builds
- `complete-beta-sprint.ps1` — Full beta sprint automation
- `monitor-beta-build.ps1` — Build progress monitoring
- `generate-final-stats.ps1` — Build metrics generation

### Test Scripts (8 files)
- `run-automated-tests.ps1` — Full test suite
- `run-e2e-tests.ps1` — End-to-end player journeys
- `run-moon-tests.ps1` — Moon-specific integration tests
- `run-m3-gates.ps1` — Moon 3 quality gates
- `test-report-generator.ps1` — Test result formatting
- `launch-vertical-slice-test.ps1` — Moon 1 playthrough
- `apply-test-integration.ps1` — Test framework wiring

### Asset Management (4 files)
- `run-asset-replacement.ps1` — Batch asset updates
- `run-itemdb-creation.ps1` — Item database generation
- `create-character-prefabs.ps1` — Character prefab automation
- `wire-kaykit-characters.ps1` — KayKit asset integration

### Performance & Profiling (3 files)
- `perf-profile.ps1` — Performance profiling session
- `profile-moon1.ps1` — Moon 1 specific profiling

### Unity Editor Integration (4 files)
- `open-unity-for-wiring.ps1` — Open Unity for asset wiring
- `launch-*.ps1` — Launch Unity in specific modes

### Code Generation & Fixes (7 files)
- `generate-data-assets.ps1` — ScriptableObject generation
- `convert-hud-to-gameevents.ps1` — Event system migration
- `fix-ui-cyclic-dependency.ps1` — Circular dependency fixes
- `fix-final-primitives.ps1` — Primitive mesh replacement
- `remove-ui-usings.ps1` — Code cleanup

### Packaging & Deployment (3 files)
- `create-beta-package.ps1` / `create-beta-package-final.ps1` — Beta package creation
- `commit-finale.ps1` — Git commit automation

## Usage Notes

### Primary Play Script (Root Directory)
The main play script remains in root for convenience:
- `tartaria-play.ps1` — Build & play (or -BatchOnly for headless validation)

### Running Scripts
Most scripts assume you're in the project root:
```powershell
cd C:\dev\TARTARIA_new
.\scripts\build-beta.ps1
```

### Build Pipeline
Standard beta build workflow:
1. `.\scripts\build-beta.ps1` — Compile & build
2. `.\scripts\run-automated-tests.ps1` — Validate
3. `.\scripts\create-beta-package.ps1` — Package for distribution

### Testing Workflow
Comprehensive test execution:
1. `.\scripts\run-moon-tests.ps1` — Integration tests (79 tests)
2. `.\scripts\run-e2e-tests.ps1` — Player journeys (5 tests)
3. `.\scripts\test-report-generator.ps1` — Generate report

## Script Conventions

- **Exit Codes:** 0 = success, 1 = failure
- **Logging:** All scripts write to `Logs/` directory
- **Batch Mode:** Most Unity-related scripts support `-BatchOnly` flag
- **Transcripts:** PowerShell transcripts captured to `Logs/*.log`

---

**Last Updated:** May 24, 2026  
**TARTARIA Beta v1.0.0-beta2**
