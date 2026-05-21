# TARTARIA — Known Issues (Beta Vertical Slice Sprint)

**Sprint:** 12-Hour Closed Beta Readiness — Echohaven Vertical Slice  
**Date:** 2026-05-21 (Cycle 1 start)  
**Status:** Actively triaging under autonomous crew

---

## P0 — Blockers (Must fix in this sprint for beta sign-off)

- **Full project compile pollution from Moon 3 content**  
  RailEscortController.cs and related Moon 3 files (SpectralOrphanAdoption, certain Moon3* relays) previously had direct Integration references and missing ServiceLocator registrations.  
  **Current status (Cycle 1):** On-disk code in RailEscort now routes through ServiceLocator (IVFXService, ICassianService, IGameLoopService, IQuestService). No `Tartaria.Integration` usings remain in Gameplay. Stale build logs show old errors. **Verification needed** via fresh compile or PlayReadinessWindow in-editor.  
  **Workaround for Moon 1 beta:** Echohaven_VerticalSlice does not load RailEscortController. Moon 3 scenes will fail until full Moon framework (MoonBeatRunner + MoonRewardService + MoonProgressTracker + registrations in GameBootstrap) is complete.  
  **Owner:** Lead Architect + Systems

- **MainMenuOverlay is IMGUI prototype**  
  Functional (New Game / Continue / Settings / Quit + gamepad/keyboard nav) but not production UI. No graphics settings, no volume sliders wired to mixer, limited accessibility.  
  **Plan:** Polish in Cycle 2-3 (UI/UX Engineer) or replace with proper UGUI/UIToolkit panels using existing UI prefabs.

- **Haptic & VFX Bridges newly introduced (staged)**  
  HapticBridge.cs and VFXBridge.cs added to Audio/. Need wiring into AudioManager, HapticFeedbackManager, VFXController, and registration to ServiceLocator if required. May cause nulls on first run if not initialized early.  
  **Owner:** Audio Designer + Technical Artist

**Cycle 1 Fix Applied (2026-05-21):** Added `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)] Bootstrap()` creation + DontDestroy to both `HapticFeedbackManager` and `VFXController`. They now self-instantiate and VFX registers to `ServiceLocator.VFX`. This directly resolves the top two Red singletons from the QA playthrough audit. Haptics and restoration VFX should now fire on tuning success / building emergence in Echohaven. Re-test in-editor required.

## P1 — Polish & Experience Issues (Fix before final beta package)

- **TutorialSystem / FTUE may have duplicate or conflicting prompts** (per old CONTEXT R10 guard added). Verify in Echohaven_VerticalSlice that only one tutorial flow runs (Movement → Discovery → Tuning → Restore → Companion milestone).
- **Save schema v8+** has many blocks (companions, combat waves, quests, moon progress). Resume from checkpoint after first Great Dome restore must restore exact Milo trust, quest state, tutorial progress, Aether field. Edge cases (force quit mid-tune, load before any save) need hardening.
- **Moon* services (MoonBeatRunner, MoonProgressTracker, MoonRewardService, MoonFrameworkBinder)** partially wired. For Moon 1 Echohaven they should provide calendar hints + giant mode teaser + RS rewards without crashing if not fully implemented.
- **Performance on minimum profile:** Not yet gate-validated in this session (existing PerformanceGateRunner + profiles exist). Target 60 fps on GTX 1060-class during full restoration sequence + VFX.
- **OneClickBuild** has recent edits (MoonFrameworkBinder integration). Verify it produces clean, versioned Windows build with TartariaIcon and no dev artifacts.

## P2 — Future / Out of Scope for this 12h beta

- All 12 other Moon scenes are scaffolded but not content-complete (only Echohaven_VerticalSlice is the beta target).
- Giant Mode core implementation, full skill trees, 13-Moon campaign UI, multiplayer, cloud saves, monetization, full 184 quests, DLC.
- Full localization, advanced accessibility (beyond basic colorblind), voice acting for all lines.
- Mac/Linux/Android/iOS builds (Windows primary for itch/Steam playtest).

---

**How to report new issues during sprint:** Add to this file under the current cycle section + notify Director in chat. All P0 must be 0 before Milestone 4.

**Last Updated:** M1 Stabilization complete — 2026-05-21 (Gate passed, proceeding to M2 with user approval)

---

## Cycle 1 Audit Summary (QA + Architect + Gameplay + Systems parallel review)

**Compile Status:** On-disk state is **clean for Moon 1 vertical slice path**. No `Tartaria.Integration` direct references remain in Gameplay or Input assemblies (RailEscortController.cs fully migrated to ServiceLocator.* interfaces). Stale build logs in repo are historical. Full project still carries Moon 3 residue (acceptable — Echohaven does not load those controllers).

**Runtime Playthrough Audit (full code-path simulation of new player 15-30min loop):** See detailed subagent report (spawned during Cycle 1). 

**Top Findings Converted to Actions:**
- **Red (now mitigated):** HapticFeedbackManager.Instance and VFXController.Instance were never created at runtime → all haptic/VFX calls dead. **Fixed in Cycle 1** with bootstrap methods (see above).
- **Yellow (watch in Cycle 2):** Execution timing races on scene load (EchohavenContentSpawner Start + Invoke(3s) Milo vs PlayerSpawner vs UI_Overlay), GameLoopController serialized refs always null (camera focus dead on restore), MoonBeatRunner not attached to Echohaven_VerticalSlice.unity (new Moon framework bypassed for slice — intentional fallback to TutorialSystem path), load resume player position weak.
- **Green:** Quest wiring (after the one-line ID fix below), Milo intro + trust + beckon, tutorial force-complete on restore, save schema roundtrips for companions/tutorial/quests, ServiceLocator pattern used correctly everywhere.

**Concrete Fix in Cycle 1:**
- `EchohavenContentSpawner.cs:561`: Changed `GetQuestDefinition("quest_echohaven_awakening")` → `"echohaven_awakening"` (matched QuestDatabaseBuilder + asset + ActivateQuest call). Prevents silent HUD objective fail on start.

**Next (M1 Stabilization):** 
- Harden load safety + add runtime Find for GameLoopController camera refs.
- Decide: attach minimal MoonBeatRunner + MoonDefinition (moon=1) to Echohaven scene (via editor tool or runtime) for 5-beat banners, or keep pure Tutorial path.
- Full in-editor validation of the now-alive haptics + VFX on first restoration sequence.
- Begin menu/settings polish.

All P0 compile/runtime singletons for happy path now addressed. Proceeding to M1 polish with user gate.

