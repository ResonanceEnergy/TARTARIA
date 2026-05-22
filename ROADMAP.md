# TARTARIA — ROADMAP

> Last updated: Session 4 (Moon 3+5 arcs, GameCompleteOverlay)
> Build: CS:0 | "All checks passed. Ready to play."

---

## COMPLETED ✅

### Foundation (Sessions 1–2)
- [x] URP 17 Forward+ / GPU Resident Drawer / STP / APV pipeline
- [x] Core asmdef hierarchy (Core → Input/Audio/Camera → Gameplay → AI → UI/Integration)
- [x] PlayerController, CameraController, HapticFeedbackManager
- [x] ServiceLocator bus (ISaveService, IHUDService, IMoonProgressService, ICompanionService, etc.)
- [x] SaveManager + SaveData v14 schema (moon flags bool/int, companions, cloud sync)
- [x] GameEvents static event bus (cross-assembly decoupling)
- [x] AudioManager singleton + PlaySFX2D
- [x] HUDController singleton (ShowObjective, ShowBanner, ShowToast)
- [x] MoonProgressTracker + MoonProgressService
- [x] CompanionManager (Lirael, Milo, Anastasia trust arcs)

### Moon Arcs (complete vertical slices — Sessions 2–4)
- [x] **Moon 1** — Magnetic / Echohaven (CrystalSpire, Lirael introduction)
- [x] **Moon 2** — Lunar / Crystalline Caverns (5-phase purge, Cassian, Giant Mode)
- [x] **Moon 3** — Electric / Buried Rail Junction (Orphan Train, Lirael backstory, 432Hz lullaby)
- [x] **Moon 4** — Self-Existing / Settlement (Autonomous building, ley-line nodes)
- [x] **Moon 5** — Overtone / White City Pavilions (Thorne radio, airship dock, World's Fair holograms)
- [x] **Moon 6** — Rhythmic / Living Library (Pipe organ requiem, Milo awakening)
- [x] **Moon 7** — Resonant / Resonant Spire (Aether beacon tower, crossover seeds)
- [x] **Moon 8** — Galactic / Airship dock (Thorne arrives, aerial combat, giant arc)
- [x] **Moon 9** — Solar / Sun Temple (Anastasia lore, solar lens mechanic)
- [x] **Moon 10** — Planetary / Cathedral (planetary alignment puzzle, boss)
- [x] **Moon 11** — Spectral / Underworld (spectral veil mechanic, ghost companion)
- [x] **Moon 12** — Crystal / Crystal Core (lattice resonance, final villain reveal)
- [x] **Moon 13** — Cosmic / Universal Spire (true timeline restoration, Zereth confrontation, game complete)

### UI
- [x] HUDController (objectives, banner, toast, minimap hooks)
- [x] DeathOverlay (4s auto-respawn, IMGUI)
- [x] PauseOverlay / PauseMenu
- [x] MainMenuOverlay (skip flag, new game confirm)
- [x] SettingsOverlay (graphics, audio, controls)
- [x] InventoryUI + CraftingOverlay
- [x] SkillTreeUI
- [x] AchievementToastOverlay + AchievementListOverlay
- [x] **GameCompleteOverlay** (fade-in credits, completion stats, Continue/Main Menu)

### Systems
- [x] BuildingSystem + BuildingDefinition (restoration phases)
- [x] CombatSystem + KnockbackSystem + HitStopController
- [x] InventorySystem + CraftingSystem
- [x] SkillTreeSystem (Moon 2 permanent blessings)
- [x] ExcavationSystem
- [x] DayNightCycleController
- [x] SpectralOrphanAdoption (Moon 3 mechanic)
- [x] OrphanTrainPuzzle
- [x] BatchReadinessValidator (35/35 phases, OneClickBuild)

---

## IN PROGRESS 🔨

- [ ] **Settings persistence** — verify PlayerPrefs round-trip for all SettingsOverlay sliders
- [ ] **Moon 3 rail escort scene** — hook Moon3ElectricArc to Moon3StartEscortTrigger + Moon3EscortHUD
- [ ] **Airship dock scene** — Moon 5 dock layout with platform prefabs wired to Moon5OvertoneArc refs

---

## BACKLOG — BETA PRIORITIES 🎯

### Gameplay Polish
- [ ] Player footstep audio (terrain-type driven, AudioManager integration)
- [ ] Enemy hit-reaction animations (stagger, death FX) for all 5 enemy archetypes
- [ ] Resonance Scanner visual polish (scan pulse radius, color by building tier)
- [ ] Giant Mode screen-shake + chromatic aberration ramp

### UI / UX
- [ ] Minimap — Moon portal icons + zone boundary overlay
- [ ] Journal / Archive — lore scroll unlock flow (DeathOverlay → AchievementToast pattern)
- [ ] Save slot UI — show Moon progress per slot in MainMenuOverlay
- [ ] Input rebinding — confirm InputRemappingUI saves to PlayerPrefs correctly

### Audio
- [ ] Compose placeholder SFX manifest (all `PlaySFX2D` keys across 13 moons)
- [ ] AudioMixer groups: Master / Music / SFX / Voice — expose to SettingsOverlay
- [ ] 432 Hz ambient base layer per biome

### Integration / Wiring
- [ ] MoonPortalSelector — unlock gate based on moon cleared flags
- [ ] MoonRuntimeBootstrapper — ensure correct arc activates per loaded scene
- [ ] Cross-moon forward seeds audit (all 13 FORWARD_SEEDS arrays → downstream consumers)

### Performance
- [ ] GPU Resident Drawer warm-up on scene load (avoid first-frame spike)
- [ ] APV probe placement audit for Echohaven + Moon 2 Caverns
- [ ] LOD bias settings for open-world Moon 5 White City

---

## CLOSED BETA DEFINITION OF DONE 🏁

All of the following must be true before tag `v0.1-beta`:

1. CS:0 build, EXIT:0, BatchReadinessValidator PASSED
2. All 13 moon arcs playable (reach Beat 5 / Revelation without crash)
3. GameCompleteOverlay shows on Moon 13 completion
4. Save/load round-trip: quit mid-moon, reload, continue from correct beat
5. DeathOverlay → respawn loop works 10× without error
6. Settings panel: volume + quality changes survive restart (PlayerPrefs)
7. No scene-load null-ref floods in Logs/
8. Input: keyboard + gamepad both navigate all overlays

---

## ARCHITECTURE NOTES

### Asmdef Dependency Order (strict — no cycles)
```
Core → {Input, Audio, Camera} → Gameplay → AI
UI    refs: Core, Gameplay, Input, Audio, Camera
Integration refs: everything
```

### Key Singleton Chain
```
SaveManager.Instance?.CurrentSave?.SetMoonFlag(moonNum, key, value)
HUDController.Instance?.ShowObjective("text")
AudioManager.Instance?.PlaySFX2D("sfx_key")
ServiceLocator.MoonProgress?.MarkCleared(moonNum)
ServiceLocator.MoonProgress?.MarkBeatCleared(moonNum, beatIndex)
```

### Event Bus (GameEvents.cs)
- `FireCriticalSaveTrigger("game_complete")` → GameCompleteOverlay.Show()
- `FireCriticalSaveTrigger("fountain_restored")` → SaveManager auto-save
- `FireMoon3FastTravelUnlocked()` → MoonPortalSelector unlock
