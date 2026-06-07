# MOON 1 — FOUNDATIONS FIRST Implementation Protocol

> **Authority**: Replaces the prior `MOON1_TO_100_PLAN.md` content-volume framing.
> **Date**: 2026-06-07
> **Source**: Deep audit summarized in `STATUS.md` 2026-06-07 entry — *"Why Moon 1 looks basic and goes in circles"*.

---

## Why this exists

For 100+ rounds the pattern has been: **fix the biggest visible gap → ship → next session the same class of bug reappears under a different name.** The audit identified the structural reason: TARTARIA is a Unity 6 URP project where the URP fundamentals are not configured AND 95% of scene content is runtime-spawned by 103 racing `RuntimeInitializeOnLoadMethod` hooks, so every scene/prefab edit is silently overwritten before frame 2.

This document is the realistic implementation protocol. It does **not** promise "Moon 1 ships". It defines the **8 phases that must complete in order** before any more "content" work is meaningful, and the rules each session must follow.

---

## Implementation Protocol (rules every session must follow)

### Rule F1 — One phase per session.
Each session works on exactly one phase below. Do not start phase N+1 until phase N's exit criteria are met **with a screenshot**.

### Rule F2 — Exit criteria require a Game-view screenshot, not a script edit.
A script edit + dialog count is NOT phase completion. The phase exit listed below names the screenshot/artifact required. Per the 2026-06-07 NO BSING mandate.

### Rule F3 — No new RuntimeInitializeOnLoadMethod scripts. Ever.
The audit found 103 of them racing each other. Any new such script is a regression. If you need a runtime behavior, attach it to an existing scene GameObject's `Awake/Start` instead. If you need a pre-Play fix, write an Editor menu under `Tartaria/8 Fix/...` that authors into prefab/scene YAML.

### Rule F4 — Delete a `*Upgrader / *Rescue / *Driver / *GodMode / *Override / *Daemon / *Fix*` class for every new feature added.
The CLAUDE.md §7 quarantine is currently violated by 5+ live classes. Each phase below explicitly deletes at least one. Net script count goes DOWN this protocol, not up.

### Rule F5 — Scene/prefab YAML is the source of truth.
If a behavior can be authored in YAML, it must be. Runtime spawners are only allowed for: combat enemies on wave triggers, VFX particle bursts at one-shot moments. Everything else (buildings, NPCs at fixed positions, props, scenery, environment lighting) goes in scene YAML or prefab `m_Component:` lists.

### Rule F6 — Honest verdicts only.
"Partial — runtime warning gone but YarnProject not bound" beats "shipped". "Blocked — needs LFS server access" beats "in progress for 6 sessions". STATUS.md is a record, not a sales document (per NO BSING Rule 9).

### Rule F7 — When in doubt: 1 screenshot in plain language.
If a session round produces no new screenshot of the actual Game view in Play mode at player eye-level, the round is incomplete.

---

## The 8 phases (priority order, do not skip)

### Phase 0 — Pull LFS + inventory what's real (1 hour)

**Why first**: 1252 of 1284 FBX on disk are 130-byte LFS pointer stubs. Until this is resolved, no FBX-based prefab swap can succeed.

**Steps**:
1. `cd C:\dev\TARTARIA_new && git lfs install && git lfs pull` (run from PowerShell, NOT the broken Cowork bash sandbox).
2. Re-run the count: `find Assets -name "*.fbx" -size -200c | wc -l`. Target: ≤ 50 stubs remain.
3. If `git lfs pull` errors with "remote rejected" or "no LFS server configured", document the LFS host in `STATUS.md` — phase 0 is BLOCKED, escalate to NATRIX before any other work.
4. Re-inventory `Assets/_Project/Prefabs/Characters/Anastasia.prefab` — if still missing after `lfs pull`, find via `git log --all --diff-filter=D -- "**/Anastasia.prefab"` and restore via `git checkout <sha>~1 -- Assets/_Project/Prefabs/Characters/Anastasia.prefab`.

**Exit criteria**: STATUS.md entry shows real FBX count > 1100. **Screenshot**: Unity Project window with a previously-stubbed KayKit FBX selected, showing real mesh import (Inspector verts > 100). Anastasia.prefab restored OR documented as permanently lost with a re-bake plan.

**Quarantine class to delete this phase**: none (admin phase).

---

### Phase 1 — Delete the 5 banned runtime mutators (2 hours)

**Why second**: While these are alive, no scene/prefab edit sticks. The audit traced `PlayerVisualUpgrader.cs:42-47` overwriting `EchohavenObelisk.transform.position` every Play 1.5s after load. Same pattern for player visual, scene rescue, focus fix, light optimizer, dev auto-start.

**Steps** (one per file, in this order):
1. Delete `Assets/_Project/Scripts/Integration/PlayerVisualUpgrader.cs` (+ .meta). Move R97 obelisk fix INTO the prefab YAML for `EchohavenObelisk` (set transform serialized position). Move player visual to `Player.prefab` mesh swap.
2. Delete `Assets/_Project/Scripts/Editor/Moon1SceneRescue.cs` (+ .meta). Lift its dedupe logic into a one-shot `Tartaria/8 Fix/Scrub Duplicate Player + Camera (one-time)` menu.
3. Delete `Assets/_Project/Scripts/Editor/GameViewFocusFix.cs` if the Unity 6 native "Play Focused" toggle works correctly (test by toggling Play Focused mode and observing input). Otherwise demote to a `[InitializeOnLoadMethod]` (Editor-only, no scene mutation).
4. Delete `Assets/_Project/Scripts/Core/RuntimeLightShadowOptimizer.cs` — move its lighting tweaks to URPAsset + RenderSettings YAML.
5. Delete `Assets/_Project/Scripts/Integration/TartariaDevAutoStart.cs` (replaced by `Moon1MCPAutoStart.cs` which only flips EditorPrefs, doesn't mutate scenes).
6. Quarantine grep: `grep -rE "class .*(Driver|GodMode|Override|Safety|Rescue|Daemon|Lifeline|Upgrader|HardOverride)" Assets/_Project/Scripts/` must return ≤ 1 match (allowing for `MoveDriver` if it's a vehicle physics class, not a hack).
7. Per Rule F4: net script delta this phase must be NEGATIVE (≥ 5 files removed, 0 added).

**Exit criteria**: Quarantine grep clean. Compile clean. **Screenshot**: Enter Play, walk to where `EchohavenObelisk` should be at (38, 0, 5), confirm it's still there from scene YAML alone (PlayerVisualUpgrader is deleted). Game view screenshot at player eye level showing obelisk.

**Quarantine class to delete this phase**: 5.

---

### Phase 2 — Fix the 4 critical Unity 6 URP fundamentals (1 hour)

**Why third**: These four settings are single-toggle changes with massive visual impact. Doing them before art work means art work benefits from them.

**Steps** (each one is 1 line of YAML or 1 Inspector toggle):

| # | File | Change |
|---|---|---|
| 1 | `ProjectSettings/GraphicsSettings.asset:67` | `m_LightsUseLinearIntensity: 0` → `1`. ([Unity 6 manual on linear color space](https://docs.unity3d.com/6000.3/Documentation/Manual/linear-color-space.html)) |
| 2 | `Assets/_Project/Config/TartariaURP_Renderer.asset` Renderer Features list | Add `ScreenSpaceAmbientOcclusion` feature. ([How](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/add-ssao-renderer-feature-to-renderer.html)) Defaults are fine. |
| 3 | `Assets/_Project/Config/EchohavenVolumeProfile.asset:123` | `ColorAdjustments.saturation: 9` → `0`. Out-of-range value flagged by audit. |
| 4 | `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` | Add one `ReflectionProbe` GameObject at scene center (real-time, 256 cubemap). Bake. ([Reflection Probes URP](https://docs.unity3d.com/6000.1/Documentation/Manual/urp/lighting/reflection-probes.html)) |

**Exit criteria**: Same scene, same camera position, same time-of-day — visual A/B improvement visible. **Screenshot**: side-by-side of `before.png` + `after.png` of the same camera position with the same object selected (e.g., the bronze tuning fork pedestal). Specular highlights should look clearly different.

**Quarantine class to delete this phase**: 0 (settings, not code).

---

### Phase 3 — Bake APV + lightmaps + NavMesh (3 hours)

**Why fourth**: Static scene content needs baked lighting to look grounded. Enemies + player need NavMesh to not clip through terrain.

**Steps**:
1. Edit `Assets/_Project/Scripts/Editor/BlenderImportPostprocessor.cs:67` — set `modelImporter.generateSecondaryUV = true` for all imported FBX. ([Lightmap UVs](https://docs.unity3d.com/6000.0/Documentation/Manual/LightingGiUvs.html))
2. Re-import all Blender FBXs (Assets > Reimport All under `Models/Blender/Moon1/`).
3. Add a `ProbeVolume` GameObject covering the village area (50×30×50 m). ([APV use](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/probevolumes-use.html))
4. Mark all static scenery (cottages, mountains, fence, tuning fork) as `Static` (all flags except OccludeeStatic). Open Lighting window → Generate Lighting → wait for bake.
5. Uncomment the NavMesh bake in `AutomatedPrefabWiring.cs` (Sprint 11 L3 punchlist) OR add a `NavMeshSurface` component to a new root GameObject `Moon1_NavMesh` and bake from there. ([NavMesh Surface](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshSurface.html))
6. Add `NavMeshAgent` to Mud Golem + Reset Scout prefabs if not present.

**Exit criteria**: `LightingData.asset` exists next to `Echohaven_VerticalSlice.unity`. APV bricks baked (check `apvScenesData.obsoleteSceneBounds` no longer empty). `Library/NavMeshes/Moon1_NavMesh.nav` exists. **Screenshot**: Player at spawn walks toward a mountain, hits collider, can't clip through. Mud Golem actually pursues player along NavMesh.

**Quarantine class to delete this phase**: 0.

---

### Phase 4 — Real ground: Unity Terrain (2 hours)

**Why fifth**: The current flat tan Plane primitive cannot host Terrain Layers, splats, or detail meshes. Visual ground variation requires this.

**Steps**:
1. Delete the giant scaled `Plane` primitive in scene.
2. Add new GameObject `Moon1_Terrain` with `Terrain` + `TerrainCollider` components.
3. Create `TerrainData.asset` at `Assets/_Project/Terrain/Echohaven_TerrainData.asset` (500×500m, heightmap 513×513 16-bit).
4. Paint 3 Terrain Layers: grass-tan (base), stone-grey (paths), mud-brown (mud pool perimeter). ([Terrain Layer](https://docs.unity3d.com/6000.4/Documentation/Manual/class-TerrainLayer.html))
5. Use Terrain heightmap brush to shape the "cupped valley" (3-sided hills) per `docs/15_MVP_BUILD_SPEC.md` §7.
6. Detail-paint grass mesh on the village area.
7. Re-bake NavMesh after terrain swap.

**Exit criteria**: Terrain rendered. **Screenshot**: Same player spawn position as before, showing tan grass with stone path crossing the frame, mud-brown pool perimeter visible, hills on horizon.

**Quarantine class to delete this phase**: 0.

---

### Phase 5 — Acquire + import real medieval architecture pack (2 hours)

**Why sixth**: The audit confirmed KayKit has no medieval/cathedral/village pack on disk. Blender bake scripts produce primitives wearing .fbx hats. No script edit can make a cube look like a Gothic cathedral.

**Candidates** (NATRIX picks one):
- **Synty POLYGON Fantasy Kingdom** — $50 paid, ~1000 prefabs, Unity Asset Store, ships URP variant
- **Kenney Medieval / Castle Kit** — CC0 free, lower poly, www.kenney.nl
- **Quixel Megascans Modular Castle** — free with Unity Quixel Bridge, photoreal but heavy

**Steps**:
1. NATRIX selects pack.
2. Import to `Assets/Vendor/<PackName>/`. Verify FBXs > 1KB (not LFS stubs).
3. Bake URP variants of all materials (Edit > Rendering > Materials > Convert Selected Built-in Materials to URP if needed).
4. Build 1 sample prefab: `Cathedral_RealKit.prefab` using pack pieces.
5. Drop it in scene at hero building position. Test render.

**Exit criteria**: 1 real building visible in Game view. **Screenshot**: Player walks to the building, looks up — Gothic cathedral or castle silhouette, not a primitive cube cluster.

**Quarantine class to delete this phase**: at least 1 of the remaining Blender primitive bake scripts (e.g., `gen_cathedral_facade.py` becomes obsolete).

---

### Phase 6 — Author Moon 1 hero + village buildings as Prefab Variants (4 hours)

**Why seventh**: 9 cottages built from 1 base via Prefab Variants is how Unity 6 does this. ([Prefab Variants](https://docs.unity3d.com/6000.4/Documentation/Manual/PrefabVariants.html)) Today `EchohavenContentSpawner.cs` creates them by `new GameObject(name)` at runtime — which is a banned pattern.

**Steps**:
1. From the real medieval kit, build base prefab `VillageHouse_Base.prefab`.
2. Create 9 prefab variants: `VillageCottageA.prefab`, `..B`, `..C`, `VillageInn.prefab`, `VillageBakery.prefab`, `VillageMill.prefab`, `VillageSmithy.prefab`, `VillageTownHall.prefab`, `VillageWatchtower.prefab`, `VillageApothecary.prefab`. Each variant overrides only the props/color that differ.
3. Same for 3 hero buildings: `Cathedral.prefab`, `StarDome.prefab`, `CrystalSpire.prefab` from kit pieces.
4. Drag each into scene at canonical positions per `docs/15 §7`. Save scene.
5. **Delete `EchohavenContentSpawner.cs`'s building-spawn methods**. Keep only the combat-trigger spawn methods (Mud Golem waves on RS thresholds).

**Exit criteria**: Open scene in Editor — see all 12 buildings authored in Hierarchy. Enter Play — buildings are there from frame 1, not spawned at frame 60. **Screenshot**: Panorama of village showing 3 hero + 9 village buildings, all real architecture.

**Quarantine class to delete this phase**: most of EchohavenContentSpawner.cs's `new GameObject` calls.

---

### Phase 7 — NPCs as real Mecanim humanoids (3 hours)

**Why eighth**: Anastasia.prefab is missing. Other NPCs are capsules. Mecanim humanoid rig is the standard.

**Steps**:
1. If Phase 0 restored `Anastasia.prefab`, verify it's healthy. If not, re-bake from KayKit Adventurers (once LFS-pulled) using `Tartaria/6 Bake/...` workflow.
2. For each of Milo, Anastasia, Lirael, Cassian, Bob — verify model importer Rig type = Humanoid; Avatar Configure passes ([Configuring the Avatar](https://docs.unity3d.com/6000.3/Documentation/Manual/ConfiguringtheAvatar.html)); Skin Weights = Standard (4 bones).
3. Place each NPC at canonical scene position per `docs/15`.
4. Set up Animator Controllers with idle + walk states (minimum).
5. Bind to `YarnTutorialBinding` so Milo tutorial step 1 (brazier) fires when player approaches.

**Exit criteria**: **Screenshot**: Approach Milo at spawn — Milo plays idle animation (not T-pose), Yarn dialogue box pops up with line 1.

**Quarantine class to delete this phase**: 0 (additive NPC work).

---

### Phase 8 — Real 8-step smoke test pass (1 hour)

**Why last**: This is the canonical Moon 1 acceptance test per CLAUDE.md §2. Phases 0-7 are prerequisites.

**Run the 8 steps from `CLAUDE.md §2`**:
1. Click Play → 0 console errors, scene loads.
2. Player visible at spawn — no magenta, no T-pose, no clipping.
3. WASD or F310 left stick walks the player.
4. Camera follows third-person, no orbit drift.
5. Walk ≤30m to a Moon-canonical interactable (brazier).
6. Press E / A — interaction UI appears.
7. Complete the interaction — mini-game succeeds, VFX/audio fires.
8. HUD updates (quest tracker, RS counter, day cycle).

**Exit criteria**: A screen-capture VIDEO (not just stills) of the 8 steps end-to-end. No magenta. No clipping. No console errors past frame 1.

**Quarantine class to delete this phase**: 0.

---

## Out of scope for this protocol

- New mini-game variants beyond Variant A (B + C ship in Phase 8.5, separate plan)
- 17th-hour cinematic (Phase 8.5)
- Save/load round-trip (Phase 8.5)
- Moon 2-13 (locked until Phase 8 passes for Moon 1)
- Win64 build / itch.io / Steam framing (per NATRIX 2026-06-03 mandate, no release talk until 13 Moons complete)

---

## How to track progress

Single source of truth: `STATUS.md` "Foundations First" header.

Per-phase row:
```
| Phase | Status | Session | Screenshot | Quarantine delta |
|---|---|---|---|---|
| 0 — LFS pull | _____ | _____ | _____ | _____ |
| 1 — Delete 5 mutators | _____ | _____ | _____ | _____ |
... etc
```

Update at end of each session. Do not start the next phase until the current row's Screenshot column has a real link.

---

*v1.0 · 2026-06-07 · author Claude (Cowork session) at NATRIX direction*
