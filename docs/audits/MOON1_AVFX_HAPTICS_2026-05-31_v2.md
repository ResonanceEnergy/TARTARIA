# Moon 1 — Audio / VFX / Cinematic / Haptics audit (v2)

Date: 2026-05-31 (read-only)
Scope: re-verify the four pillars per CLAUDE.md "no stubs" mandate after the late-evening Layer 2 + climactic-VFX work.

Verdict in one line: assets are mostly **on disk**, but the Layer 2 music patch is **broken at the syntax level**, and the climactic VFX prefabs are **orphaned** (no runtime instantiator).

---

## AUDIO

| # | Item | Status | Notes |
|---|------|--------|-------|
| 1 | `AdaptiveMusicController.cs` Layer 1 bed (minor cello → major + crystalline) | WORKING | `UpdateLayerVolumes()` blends L0/L1/L2/L3 + Schumann via RS curve; `GenChord` real, `Bootstrap()` real. RS 50 crossfade with Schumann present. |
| 2 | `AdaptiveMusicController.cs` Layer 2 reactive (discovery / tuning / combat / restoration) | **STUB / BROKEN** | The Layer 2 block (fields, `BindLayer2Events`, `HandlePOIDiscovered`, `GenDiscoveryArpeggio` etc., lines 423-547) is **physically inside the `StingerType` enum body**, not the class. C# does not permit fields, methods or non-enum members inside an enum. **The file will not compile.** The Awake-side calls to `BindLayer2Events()` / `UnbindLayer2Events()` (lines 79, 87) and `UpdateLayer2Reactive()` (line 96) reference symbols that, after the file is parsed correctly, do not exist on the class. This is the headline blocker tonight. |
| 3 | `Moon1AudioAtmosphere.cs` 3 concentric zones + procedural fallback | WORKING | Bootstraps only in Echohaven scene. Spawns 6 looping sources (village hum, perimeter wind, mud gurgle, 3 Aether bands). Fades Harmonic/Celestial/Telluric in on building restoration. Self-cleaning restoration stinger. Linear rolloff, spatial blend 1. |
| 4 | `Moon1ProceduralLore.cs` Editor menu | WORKING — and **has been run** | Menu item `Tartaria/1 Build/Moon 1 — Audio Lore ...` is present. WAV writer is real PCM-16. |
| 5 | `Audio/Moon1_Lore/*.wav` files | WORKING (5/5 present, not 3/3) | All five exist on disk: `Lirael_Lullaby_432Hz.wav`, `Skeleton_Hum_Prophecy.wav`, `Cathedral_Restoration_Stinger.wav`, `Reset_Scout_Taunt.wav`, `Milo_Blimey_Chime.wav`. |

Caveat on #5: the WAVs exist, but **no runtime script references them by path** — they are currently dead assets. `Moon1NarrativeBeats.SkeletonHumProphecyRoutine` generates its own 55Hz hum inline instead of loading `Skeleton_Hum_Prophecy.wav`; `Moon1AudioAtmosphere.PlayRestorationStinger` generates its own cascade instead of loading `Cathedral_Restoration_Stinger.wav`.

---

## VFX

| # | Item | Status | Notes |
|---|------|--------|-------|
| 6 | 4 Moon 1 VFX prefabs at `Assets/_Project/Prefabs/VFX/Moon1/` | PARTIAL | All 4 `.prefab` files exist on disk (`VFX_CathedralLightEruption`, `VFX_SpirePlacementSparks`, `VFX_GiantModeBurst`, `VFX_SeventeenthHourBeam`). Each is a real `ParticleSystem` + point light setup, not a stub. |
| 7 | `Moon1NarrativeBeats.cs` fires Cathedral eruption / hum prophecy / Giant Key #1 | PARTIAL | Subscribes to `TartarianHourCycle.OnSeventeenthHour` + `GameEvents.OnBuildingRestoredTyped`. Eruption + hum + key all real coroutines and run. **BUT** it builds the pillar inline with `new GameObject` / raw `ParticleSystem` — it never `Instantiate`s `VFX_CathedralLightEruption.prefab`. The premium prefab is orphaned. |
| 8 | Mud dissolution shader | PARTIAL | `Assets/_Project/Shaders/MudDissolution.shader` exists, three materials reference it (`M_Mud_Fresh`, `M_Mud_Cracking`, `M_Mud_Dissolving`, `MudGolem_Body`), but the only C# `MudDissolution` references are ECS `IComponentData` writes in `BuildingSystem.cs` / `WorldInitializer.cs`. No script currently animates `_Dissolution` over time on a building Renderer. |
| 9 | Resonance Pulse VFX / Golem Spawn VFX / Golem Death VFX | PARTIAL | `VFXController.PlayResonancePulse(pos, radius)` exists (line 480) and is called once internally. `PlayBuildingEmergence` exists. **No `PlayGolemSpawn` / `PlayGolemDeath` VFX methods** on `VFXController`, and no live (`.cs`, non-`.disabled`) subscriber wires `GameEvents.OnEnemyKilled` into any VFX spawn. `MudGolemHealth.OnDeath` fires nothing visual beyond a damage-flash color. |

---

## CINEMATIC

| # | Item | Status | Notes |
|---|------|--------|-------|
| 10 | `Moon1CinematicMoments.cs` | PARTIAL | Subscribes to `GameEvents.OnBuildingRestored`, runs a 3.5 s smooth dolly orbit + return. Has a public `TriggerSeventeenthHourPan()` 4-leg wide pan, but **nothing calls it** — `TartarianHourCycle.OnSeventeenthHour` is not subscribed here (only `Moon1NarrativeBeats` listens). No Anastasia-reveal hook. Camera-lerp, not jump-cut. |
| 11 | `RestorationCinemachine.cs` | WORKING (with caveat) | Auto-bootstraps `DontDestroyOnLoad`, subscribes to `OnBuildingRestoredTyped`, real 4 s smooth dolly orbit + 1 s return. Caveat: it runs **simultaneously** with `Moon1CinematicMoments.HandleBuildingRestored` (same event, different parameter form). Two cinematics will fight for `Camera.main` on every restoration. |
| 12 | Cinemachine package usage | PARTIAL | `com.unity.cinemachine 3.1.2` is in `Packages/manifest.json`. Only `PlayerCombat.cs` actually uses it (`CinemachineImpulseSource`). `RestorationCinemachine` is camera-lerp despite the name; no `CinemachineCamera` / `CinemachineBrain` in any non-disabled script. |

---

## HAPTICS

| # | Item | Status | Notes |
|---|------|--------|-------|
| 13 | `HapticFeedbackManager.cs` 9 spec events | PARTIAL — 4/9 wired live | Method **defined** for all 9. Caller status (non-`.disabled` only): Footstep DEFINED-UNUSED; Discovery WIRED (Excavation, Inventory, Crafting, Pickup); TuningOn WIRED (HarmonicRockCutting, PipeOrgan, DissonanceLensOverlay); TuningOff WIRED (HarmonicRockCutting, PipeOrgan); PerfectTune WIRED (PipeOrgan, DissonanceLens); BuildingEmergence WIRED (InteractableBuilding + 6 mini-games); GolemSpawn UNUSED in live code (only inside `.disabled` files); CombatHit WIRED (PlayerHealth, EchohavenContentSpawner, CraftingSystem); GolemDeath WIRED (PlayerHealth on player-death only — never on actual golem death). Effective live coverage **6/9**. Unwired: Footstep, GolemSpawn, GolemDeath-on-golem. |
| 14 | Haptic subscribers to `OnBuildingRestored / OnEnemyKilled / OnPOIDiscovered` | MISSING | Zero direct event-subscriber wiring. Building haptics fire from `InteractableBuilding.Restore()` not from the event; enemy-kill haptics fire nowhere; POI-discovery haptics fire nowhere. `AdaptiveMusicController` is the only `OnPOIDiscovered` subscriber. |

---

## TOP 5 PATCHES (priority order)

1. **Fix `AdaptiveMusicController.cs` Layer 2 compile error.** Move the entire block from line ~423 to line 547 out of the `StingerType` enum body and into the `AdaptiveMusicController` class above it (before line 410). Add a closing `}` to the enum and remove the trailing duplicate. Without this, the whole Audio assembly fails to compile.
2. **Wire `Moon1NarrativeBeats.CathedralLightEruption` to instantiate `VFX_CathedralLightEruption.prefab`** via `Resources.Load` (move prefab to `Resources/VFX/Moon1/`) or an Inspector-assigned `GameObject` field. Same for the Skeleton-Hum-Prophecy beat → load `Skeleton_Hum_Prophecy.wav` (`Resources.Load<AudioClip>`) instead of generating 55Hz inline. This is the single biggest "no-stub" win — premium assets exist and are not being used.
3. **Resolve duplicate cinematic subscribers.** `RestorationCinemachine` and `Moon1CinematicMoments.HandleBuildingRestored` both grab `Camera.main` on the same event. Pick one as canonical and gate or remove the other. Recommended: keep `RestorationCinemachine` (4 s) and repurpose `Moon1CinematicMoments` to only own the 17th-hour pan (call `TriggerSeventeenthHourPan` from a new `TartarianHourCycle.OnSeventeenthHour` subscription).
4. **Wire missing haptics on game events.** Add a small `Moon1HapticBridge` MonoBehaviour that subscribes to `GameEvents.OnEnemyKilled` → `PlayGolemDeath`, `OnPOIDiscovered` → `PlayDiscovery`, and on enemy spawn → `PlayGolemSpawn`. Hook `PlayFootstep()` from `FootstepController.PlayFootstep()` (currently only triggers SFX). Lifts live coverage from 6/9 to 9/9.
5. **Animate the MudDissolution shader on restoration.** Either drive `_Dissolution` from `Renderer.material.SetFloat` over the 5 s building-emergence window inside `InteractableBuilding.Restore()`, or have `BuildingSystem`'s ECS system push the value to the GameObject view. The shader and three materials are wasted otherwise.

---

## SECONDARY OBSERVATIONS

- `VFXController.PlayBuildingEmergence` exists and spawns a vortex — a candidate target for the new `OnBuildingRestored` haptic bridge so VFX + haptic + cinematic all fire from the same event.
- `Cinemachine 3.1.2` is installed but unused outside `PlayerCombat`'s impulse source. If genuine Cinemachine cinematics are wanted in Phase 1, the existing camera-lerp coroutines could be replaced with a `CinemachineCamera` + `Priority` swap pattern.
- Generated WAVs use the menu path `Tartaria/1 Build/Moon 1 — Audio Lore...` which suggests they were generated this session — file presence confirms a previous run succeeded.

End of audit.
