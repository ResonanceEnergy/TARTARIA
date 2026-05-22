# Contributing to TARTARIA

Thank you for your interest in contributing to TARTARIA! This document provides guidelines for contributing code, reporting bugs, and submitting feedback during beta testing.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How to Contribute](#how-to-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Features](#suggesting-features)
  - [Submitting Pull Requests](#submitting-pull-requests)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)
- [Community](#community)

---

## Code of Conduct

We are committed to providing a welcoming and inclusive environment for all contributors. Please be respectful, constructive, and professional in all interactions.

---

## How to Contribute

### Reporting Bugs

Found a bug? Help us fix it!

**Before reporting:**
1. Check [existing issues](https://github.com/ResonanceEnergy/TARTARIA/issues) to avoid duplicates
2. Try the latest build — your issue may already be fixed
3. Review [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for common issues

**To report a bug:**
1. Go to [GitHub Issues](https://github.com/ResonanceEnergy/TARTARIA/issues)
2. Click "New Issue" → "Bug Report" template
3. Fill in:
   - **Title:** Clear, specific description (e.g. "Player falls through floor near Star Dome")
   - **System specs:** GPU, CPU, RAM, OS version
   - **Steps to reproduce:** Numbered list of exact steps
   - **Expected behavior:** What should have happened
   - **Actual behavior:** What actually happened
   - **Player.log:** Attach log from `%USERPROFILE%\AppData\LocalLow\ResonanceEnergy\Tartaria\Player.log`
   - **Screenshots/video:** If visual bug
4. Submit issue

**Good bug report example:**
```
Title: Player clips through ground after restoring Star Dome

System: Windows 11, RTX 3060, i7-12700K, 32GB RAM

Steps to reproduce:
1. Start New Game
2. Complete Milo intro sequence
3. Walk to Star Dome (first building)
4. Press E to interact and restore building
5. Player character falls through terrain after restoration VFX completes

Expected: Player remains on ground after restoration
Actual: Player falls through floor into void, respawns at spawn point

Logs attached: Player_2026-05-21.log
Screenshot: player_falling.png
```

---

### Suggesting Features

Have an idea for TARTARIA?

**Before suggesting:**
1. Check [existing feature requests](https://github.com/ResonanceEnergy/TARTARIA/issues?q=is%3Aissue+label%3Aenhancement)
2. Review [00_MASTER_GDD.md](docs/00_MASTER_GDD.md) — your idea may already be planned
3. Consider if it fits the game's vision (resonance-based gameplay, 13-Moon campaign, harmonic restoration)

**To suggest a feature:**
1. Go to [GitHub Issues](https://github.com/ResonanceEnergy/TARTARIA/issues)
2. Click "New Issue" → "Feature Request" template
3. Fill in:
   - **Title:** Clear feature description
   - **Problem:** What gameplay need does this address?
   - **Proposed solution:** How would it work?
   - **Alternatives:** Other approaches considered?
   - **Additional context:** Screenshots, mockups, examples from other games
4. Submit issue

---

### Submitting Pull Requests

Want to contribute code? Great!

**Before coding:**
1. Open an issue first to discuss your proposed changes
2. Fork the repository
3. Create a feature branch: `git checkout -b feature/your-feature-name`
4. Review [Coding Standards](#coding-standards) below

**PR requirements:**
- [ ] Code compiles with zero errors and zero warnings (CS:0)
- [ ] Follows project coding standards (see below)
- [ ] Includes comments explaining non-obvious logic
- [ ] Does not break existing functionality
- [ ] Tested in Unity Editor play mode
- [ ] PR description explains what changed and why
- [ ] Linked to relevant issue(s)

**PR process:**
1. Commit changes with clear, descriptive messages:
   ```
   feat: Add haptic feedback to building restoration
   
   - PlayDiscovery() fires on building Discover() call
   - PlayBuildingEmergence() fires after restoration VFX
   - Tested on Xbox controller (F310 verified)
   - Fixes #123
   ```
2. Push to your fork: `git push origin feature/your-feature-name`
3. Open pull request on GitHub
4. Address review feedback
5. Maintainers will merge when approved

---

## Development Setup

### Prerequisites

- **Unity 6000.3.6f1** (exact version required)
  - Install via Unity Hub: `unityhub://6000.3.6f1/bbb010bdb8a3`
  - Modules: Windows Build Support (IL2CPP)
- **Git** (for cloning repository)
- **PowerShell 7+** (for build automation)
- **Visual Studio 2022** or **JetBrains Rider** (recommended IDEs)

### Clone & Setup

```bash
git clone https://github.com/ResonanceEnergy/TARTARIA.git
cd TARTARIA
```

Open project in Unity Hub → Unity 6000.3.6f1 will import assets (~5 min first launch)

### Build & Play

```powershell
.\tartaria-play.ps1
```

**What it does:**
- Builds all scenes, prefabs, ScriptableObjects
- Runs 31 readiness checks
- Opens Unity in play mode
- **Expected:** CS:0, "All checks passed. Ready to play."

**See [BUILD_GUIDE.md](BUILD_GUIDE.md) for full build instructions.**

---

## Coding Standards

### General Principles

- **2026 AAA quality:** No compile errors, no warnings, clean console
- **Performance-first:** No per-frame allocations, cached references, profiler-friendly
- **Maintainable:** Clear naming, comments on non-obvious logic, consistent style

### C# Style

**Naming:**
- `PascalCase` for classes, methods, properties, public fields
- `camelCase` for private fields, local variables, parameters
- `_camelCase` for private instance fields (underscore prefix)
- `s_camelCase` for static fields (s_ prefix)

**Example:**
```csharp
public class ExampleComponent : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;  // Inspector field
    private Transform _cachedTransform;               // Cached reference
    static int s_instanceCount = 0;                   // Static field
    
    void Awake()
    {
        _cachedTransform = transform;  // Cache in Awake, not Update
        s_instanceCount++;
    }
    
    public void Move(Vector3 direction)  // Public method
    {
        _cachedTransform.Translate(direction * _moveSpeed * Time.deltaTime);
    }
}
```

**Unity Best Practices:**
- Cache `GetComponent`, `transform`, `gameObject` in `Awake()`
- Never call `GetComponent` in `Update()`, `FixedUpdate()`, or `LateUpdate()`
- Use `[SerializeField] private` over `public` for inspector fields
- Use `TryGetComponent` over `GetComponent` when null is acceptable
- Use `FindFirstObjectByType<T>()` over `FindObjectOfType<T>()` (Unity 6 API)
- Use object pooling for frequently instantiated GameObjects

**Architecture:**
- Respect assembly boundaries (see `04_ARCHITECTURE_GUIDE.md`)
- Use `ServiceLocator` pattern for cross-assembly communication
- Never reference `Tartaria.Integration` from `Tartaria.Gameplay` or `Tartaria.Input`
- Keep assemblies acyclic (no circular dependencies)

**Performance:**
- No `new List<>()` or `new Dictionary<>()` in `Update()` loops
- No LINQ in hot paths (Update, FixedUpdate)
- No string concatenation in Update (`+` operator allocates)
- Use `StringBuilder` for dynamic strings
- Use Unity Profiler (`Ctrl+7`) to verify zero allocations in performance-critical code

---

## Testing Guidelines

### Before Submitting PR

**Manual Testing:**
1. Run `.\tartaria-play.ps1` — verify CS:0, all checks passed
2. Enter play mode — test your changes in Echohaven_VerticalSlice scene
3. Test with gamepad AND keyboard+mouse
4. Check Unity Console — zero errors, zero warnings
5. Check Editor.log — no stack traces (except from intentional Debug.Log calls)

**Performance Testing:**
- Open Unity Profiler (`Ctrl+7`)
- Enter play mode and exercise your code path
- Verify:
  - No GC.Alloc spikes in hot frames
  - Frame time < 16.6ms (60 fps target)
  - CPU: Main Thread < 14ms, Render Thread < 14ms
  - GPU: < 12ms on Medium hardware tier (GTX 1070)

**Code Quality Checklist:**
- [ ] No `TODO` or `FIXME` comments left in code
- [ ] All `Debug.Log` calls have meaningful context
- [ ] No commented-out code blocks
- [ ] XML documentation on public APIs
- [ ] Consistent indentation (4 spaces, no tabs)

---

## Documentation

### When to Update Docs

**Always update docs when:**
- Adding new systems or mechanics
- Changing player-facing controls or UI
- Modifying build pipeline or setup steps
- Adding new assemblies or dependencies

**Key documentation files:**
- `README.md` — Project overview, quick start
- `BUILD_GUIDE.md` — Build instructions, system requirements
- `TROUBLESHOOTING.md` — Common issues and fixes
- `docs/04_ARCHITECTURE_GUIDE.md` — Code architecture, assembly structure
- `docs/09_TECHNICAL_SPEC.md` — Performance targets, hardware tiers

### Documentation Style

- Use **Markdown** formatting
- Keep lines under 120 characters
- Include code examples for non-obvious features
- Link to related docs with relative paths: `[Architecture Guide](docs/04_ARCHITECTURE_GUIDE.md)`

---

## Community

### Where to Get Help

- **GitHub Issues:** [Bug reports and feature requests](https://github.com/ResonanceEnergy/TARTARIA/issues)
- **Discussions:** [Q&A and general discussion](https://github.com/ResonanceEnergy/TARTARIA/discussions)
- **Discord:** *(Coming soon)* — Real-time chat with dev team and community

### Beta Testing

Want to help test TARTARIA?

1. Download latest beta build from [Releases](https://github.com/ResonanceEnergy/TARTARIA/releases)
2. Extract and run `Tartaria.exe`
3. Play through Echohaven vertical slice (15-30 min)
4. Report bugs via [GitHub Issues](https://github.com/ResonanceEnergy/TARTARIA/issues)
5. Share feedback on gameplay, performance, polish

**What we're looking for:**
- Crash reports with logs
- Performance issues (FPS drops, stuttering)
- Gameplay bugs (interactions broken, physics glitches)
- Polish gaps (missing VFX, audio issues, UI bugs)
- First impressions (pacing, tutorial clarity, feel)

---

## Thank You!

Every bug report, feature suggestion, and pull request helps make TARTARIA better. We appreciate your contributions to the Resonance Energy universe!

---

**Project Links:**
- **Repository:** https://github.com/ResonanceEnergy/TARTARIA
- **Issues:** https://github.com/ResonanceEnergy/TARTARIA/issues
- **Releases:** https://github.com/ResonanceEnergy/TARTARIA/releases

**Last Updated:** 2026-05-21 (Beta Vertical Slice)
