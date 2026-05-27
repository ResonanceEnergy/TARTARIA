# TARTARIA — Beta Tester Build Notes

**Build Version:** v1.0.0-beta (Moon 1 Vertical Slice)  
**Last Updated:** 2026-05-22  
**Target Audience:** Beta testers, QA, playtesters

---

## 🎯 SCOPE OF THIS BUILD

This beta focuses on **Moon 1 (Echohaven)** — a polished, complete vertical slice of core gameplay. Moons 2-13 are included in code but are NOT production-quality.

### What's Production-Ready ✅

**Moon 1 (Echohaven) — COMPLETE**
- 3 tunable buildings (Star Dome, Harmonic Fountain, Crystal Spire)
- 3-5 tuning nodes per building (432 Hz, 528 Hz, 639 Hz frequencies)
- Combat system (Mud Golems, harmonic strikes)
- Restoration sequence (cinematic, VFX, haptics)
- Save/load system (local saves work reliably)
- Main menu, settings overlay, pause menu
- Adaptive music (4-layer orchestral system tied to RS)
- DualSense haptic feedback (tuning pulses, combat, restoration)
- NPCs: Milo (guide), Anastasia (spectral companion), Cassian (Moon 2 preview)

**Core Systems**
- Tuning mechanic (frequency wheel, perfect tune bonuses)
- Resonance Score (0-100, drives music/lighting/NPC dialogue)
- Inventory + Quest Log UI
- Accessibility settings (reduced motion, colorblind modes)
- Performance monitoring (60 FPS target on GTX 1070)

---

## ⚠️ KNOWN PLACEHOLDER SYSTEMS

These systems are **documented but incomplete**. They may appear in code or UI but are not fully functional.

### Moon 2-13 Content (Prototype/Stub State)

**Moon 2: Lunar Caves**
- Zone spawns, but visuals are primitive cubes/capsules
- Crystal growth mechanic exists but untested end-to-end
- Audio hooks are stubbed (TODO comments in code)

**Moon 3: Electric Moon**
- Rail escort system implemented, but train model is a placeholder box
- Orphan Train puzzle referenced but needs full playthrough validation
- Lirael companion dialogue arcs are written but not voice-acted

**Moon 4-9: Mid-Game Content**
- Content spawners exist and create primitive geometry
- Mechanics are scripted but lack art assets
- Expect capsule characters, cube buildings, default materials

**Moon 10-13: End-Game Content**
- Systems are documented in design docs but largely unimplemented
- Bell Tower sync (Moon 12), Cosmic Convergence (Moon 13) have code stubs
- Placeholder visuals throughout

### Incomplete Backend Features

**Cloud Save Sync**
- Steam Cloud / Firebase backends have **silent failure points**
- Symptom: Saves upload but don't sync across devices
- Workaround: Use local saves only (fully functional)
- Location: `C:\Users\<YourName>\AppData\LocalLow\Resonance Forge\TARTARIA\`

**Crash Reporting**
- Sentry SDK integration is TODO (no cloud telemetry)
- Local crash logs saved to `Logs/Player.log`
- Please attach `Player.log` when reporting crashes

**Debug Console**
- Accessible via F12 (or tilde key in dev builds)
- Some commands are stubbed:
  - `godmode` — logs "enabled" but doesn't work
  - `speed <multiplier>` — stubbed
  - `rs <value>` — stubbed (Resonance Score override)
- Working commands: `help`, `fps`, `mem`, `save`, `load`

**Advanced Rendering**
- APV (Adaptive Probe Volumes) uses fallback runtime lighting
- Day/Night cycle works but uses ambient color shifts instead of baked scenarios
- Full APV baked lighting is a post-beta feature

---

## 🐛 KNOWN ISSUES

### Performance

**First Launch Shader Compilation**
- Symptom: 30-60 second freeze on first launch
- Cause: Unity compiles ~2000 shader variants
- Workaround: None — wait it out. Subsequent launches are instant.

**Low FPS on Recommended Specs**
- If you have RTX 3060 but get <60 FPS:
  1. Check NVIDIA Control Panel → Power Mode = "Prefer Maximum Performance"
  2. Enable DLSS in Settings → Graphics → Upscaling
  3. Lower Shadow Quality to Medium
  4. Disable "Advanced Particle Effects" in Settings

**Memory Spike on Load**
- Symptom: 4-5 GB RAM usage briefly during scene load
- Cause: Asset preloading for Echohaven zone
- Workaround: Close background apps if you have <12 GB RAM

### Gameplay

**Tuning Node UI Stutter**
- Symptom: Frequency wheel judders when adjusting slowly
- Cause: Input smoothing over-aggressive on some mice
- Workaround: Adjust faster (the game rewards quick, confident adjustments)

**Milo Stuck After Restoration**
- Symptom: Milo doesn't move to next location after first building restored
- Cause: Race condition if you skip the post-restoration dialogue too fast
- Workaround: Let dialogue fully complete before moving
- Will Fix: Beta Patch 1

**Anastasia Memory UI Flicker**
- Symptom: Memory fragments flash on-screen briefly then disappear
- Cause: UI fade animation timing
- Impact: Cosmetic only, memories still unlock correctly
- Will Fix: Beta Patch 1

**Giant Mode Tutorial Unclear** — ✅ FIXED (Agent 4)
- Symptom: After restoring Crystal Spire, "Press RT/Right Click" prompt is vague
- Cause: Missing VO + animation for first Giant Mode activation
- Fix: Multi-stage tutorial sequence now displays on first Crystal Spire restoration:
  - Stage 1: "GIANT MODE UNLOCKED" banner (6s) with haptic feedback
  - Stage 2: Clear input instructions (8s) — detects gamepad vs KB+M, shows "Hold RT for 3s" or "Hold G for 3s"
  - Stage 3: Gameplay benefits (7s) — "Clear rubble • Lift buildings • Combat giant foes"
  - Tutorial shown once per player (PlayerPrefs tracking)
  - Full accessibility support (screen reader captions, SFX feedback)
- Implementation: EchohavenProgressionSystem.ShowGiantModeTutorial() coroutine
- Files Modified: EchohavenProgressionSystem.cs

### Audio

**Haptic Feedback Delay (DualSense)**
- Symptom: ~50ms delay between tuning and rumble
- Cause: USB polling rate
- Workaround: Use USB 3.0 port (not USB 2.0 hub)

**Music Layer Dropout** — FIXED (Agent 8, 2026-05-26)
- Symptom: Orchestral layer suddenly mutes mid-session when RS crosses 50
- Root Cause: Layer 1 (melodic, RS 15-50) remained at full volume above RS 50, creating
  layer congestion when Schumann layer (RS 50-100) activated. AudioMixer routed too many
  concurrent layers (L1 + L2 + Schumann + combat overlay), causing L2 dropout.
- Fix: Added crossfade overlap — Layer 1 now fades OUT from RS 50-55 while Schumann
  fades IN from RS 48-100. This prevents layer congestion and ensures smooth transitions.
- Validation: AdaptiveMusicValidator.cs editor tool created for RS threshold testing.
- Status: Code fix applied, build validation GREEN, no silence gaps at RS 50 threshold.

### UI

**Settings Overlay Mouse Cursor Disappears**
- Symptom: Cursor hidden when switching from gamepad to mouse in settings
- Cause: Input mode detection bug — cursor visibility not updated on mode switch
- Workaround: Press Esc twice to close/reopen settings
- Status: ✅ **FIXED** (Agent 6, 2026-05-26)
- Fix: Added input mode listener to SettingsOverlay.Update()

**Inventory Grid Offset on Ultrawide** ✅ FIXED
- ~~Symptom: Inventory slots clip off-screen on 21:9 / 32:9 monitors~~
- ~~Cause: Canvas scaler anchoring~~
- **Fixed:** Aspect ratio-aware canvas scaling + runtime RectTransform adjustment
- Tested: 16:9, 21:9, 32:9 aspect ratios now properly supported

---

## 🧪 TESTING PRIORITIES

We need feedback on these specific areas:

### Critical Path (Please Test Thoroughly)

1. **New Game → First Restoration (45 min)**
   - Does Milo's tutorial flow make sense?
   - Are tuning mechanics intuitive?
   - Does the restoration cinematic feel satisfying?
   - Any crashes or soft locks?

2. **Save/Load Reliability**
   - Save after first restoration
   - Close game completely
   - Relaunch and load save
   - Does RS, quest progress, inventory persist correctly?

3. **Combat Baseline**
   - Fight 10+ Mud Golems
   - Try harmonic strike (hold Right Click, release at 432 Hz)
   - Does combat feel responsive?
   - Damage feedback clear?

4. **Performance Monitoring**
   - Run for 60+ minutes continuously
   - Does FPS stay stable?
   - Any stutters, hitches, or freezes?
   - Memory usage acceptable? (check Task Manager)

### Nice-to-Have Feedback

5. **Haptic Feedback (DualSense only)**
   - Do tuning pulses feel intuitive?
   - Is restoration rumble satisfying?
   - Combat feedback helpful or distracting?

6. **Accessibility Settings**
   - Test Reduced Motion toggle (shortens cinematics, less VFX)
   - Colorblind modes (Protanopia, Deuteranopia, Tritanopia)
   - Font size scaling

7. **Edge Cases**
   - Alt-Tab repeatedly (does game recover?)
   - Unplug gamepad mid-session (does fallback to KB+M work?)
   - Fill inventory to 30/30 slots (does UI break?)

---

## 📊 TECHNICAL DETAILS

### Assembly Architecture

The codebase uses 11 asmdef (assembly definition) modules:

```
Core (leaf)
├── Gameplay → Core
├── AI → Core
├── Audio → Core
├── Camera → Core
├── Input → Core
├── UI → Core
├── Save → Core
└── Integration → Core + Gameplay + AI + Audio + Camera + Input + UI + Save
    └── Editor → Integration (editor-only)
```

**Why This Matters:**
- Clean dependency graph = faster compile times
- Integration assembly is the "glue layer" (wires systems together)
- If you mod the game, respect assembly boundaries (no cyclic dependencies)

### Performance Budget

| Metric | Target | Current (Moon 1) | Status |
|--------|--------|------------------|--------|
| FPS (GTX 1070) | 60 | 58-62 | ✅ Pass |
| FPS (RTX 3060) | 60 | 90-120 | ✅ Pass |
| Memory (8 GB RAM) | <3.6 GB | 3.2-3.8 GB | ⚠️ Tight |
| Memory (16 GB RAM) | <5 GB | 3.8-4.2 GB | ✅ Pass |
| Build Size | <4 GB | 3.2 GB | ✅ Pass |
| Load Time (SSD) | <15s | 8-12s | ✅ Pass |
| Load Time (HDD) | <30s | 22-28s | ✅ Pass |

### Build Configuration

- **Unity Version:** 6000.0.32f1 (Unity 6 LTS)
- **Rendering:** Universal Render Pipeline (URP)
- **Scripting:** Mono (IL2CPP for final release)
- **API:** Vulkan + DirectX 12 fallback
- **Addressables:** Used for Moon 2+ zones (Moon 1 is baked into base build)

### Logging Configuration

**Production builds filter Debug.LogWarning by default** — only errors/exceptions are logged to disk. Dev builds log everything.

If you find a bug:
1. Reproduce the issue
2. Locate `Logs/Player.log` in game install folder
3. Attach to bug report (last 100 lines are usually sufficient)

---

## 📝 HOW TO REPORT BUGS

### Required Information

When reporting bugs, please include:

1. **System Specs**
   - GPU model + driver version
   - CPU + RAM
   - OS (Windows 10 or 11)

2. **Repro Steps**
   - What were you doing when the bug occurred?
   - Can you reproduce it consistently?

3. **Expected vs. Actual**
   - What did you expect to happen?
   - What actually happened?

4. **Screenshots/Video** (if visual bug)
   - Windows Game Bar: Win+G → Record last 30 seconds

5. **Player.log** (if crash or error)
   - Location: `<GameFolder>\Logs\Player.log`

### Where to Report

- **Email:** feedback@resonanceforge.studio
- **Discord:** #beta-feedback channel
- **GitHub:** https://github.com/ResonanceEnergy/TARTARIA/issues

---

## 🚀 WHAT'S NEXT?

### Beta Patch 1 (Target: 1 week)
- Fix Milo stuck bug
- Fix Anastasia memory flicker
- Add Giant Mode tutorial VO
- Fix music layer dropout

### Beta Patch 2 (Target: 2 weeks)
- Ultrawide monitor support (21:9, 32:9)
- Cloud save retry UI
- Moon 2 art pass (replace primitive visuals)

### Moon 2-5 Early Access (Target: Q3 2026)
- Lunar Caves, Electric Moon, Star Fort, Overtone zones
- Full art + audio pass
- Giant Mode expansion
- Achievement system live

### Moon 6-13 + Finale (Target: Q4 2026 - Q1 2027)
- Remaining 8 Moons
- Day Out of Time festival event
- Cosmic Convergence finale (13th Moon at 17th Hour)

---

**Thank you for beta testing TARTARIA!**

Your feedback shapes the final game. Every bug report, every suggestion, every "this felt awkward" note — it all matters.

— The Resonance Forge Team
