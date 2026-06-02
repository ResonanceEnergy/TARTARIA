# HANDOFFS.md

Cross-agent hand-off log per `docs/agents/COORDINATION.md` § "Hand-off protocol".
Append new entries at the bottom. Format: `## YYYY-MM-DD HH:MM --- <FromRole> -> <ToRole>`.

---

## 2026-06-01 --- Systems -> Tools (Cowork / Editor)
**Need:** Author `Moon1Content` Addressables group and assign hero addresses.
- Window > Asset Management > Addressables > Groups
- Create new group: `Moon1Content`
- Drag `Assets/_Project/Prefabs/Moon1/**` entries into it
- For each Moon 1 hero prefab, set the entry Address to its hero id (e.g. `Cathedral`, `Fountain`, `Spire`) so `AddressableContentLoader.LoadHero(id)` resolves it directly.
- Build content: Addressables > Build > New Build > Default Build Script.

**Why:** Closes the loop on `agent/systems/addressables-baseline`. Until this lands, `AddressableContentLoader.LoadHero` silently falls back to `Resources.Load<GameObject>("Moon1/Heroes/{id}")` and logs a warning per miss. Runtime stays green either way.

**Blocking:** Nothing hard-blocked; quality-of-life + sets the pattern for Moons 2-13 content streaming.

**Files:** `.asset` files under `Assets/AddressableAssetsData/` (Editor-only, GUID-sensitive --- avoid scripting it; do it from the Addressables window).

---

## 2026-06-01 --- Systems -> Gameplay (HeroBuildingSpawner integration)
**Need:** Swap legacy `Resources.Load<GameObject>(...)` call sites in Moon 1 hero spawn paths to `AddressableContentLoader.LoadHero(id)` (or `LoadHeroBlocking` for sync sites).

**Why:** Original ticket asked for a one-line swap in `Moon1HeroBuildingSpawner`, but that file was removed in `Moon1MegaCleanup` (cite: `Assets/_Project/Scripts/Editor/Moon1MegaCleanup.cs:68` --- "STEP 3 - Delete old Moon1HeroBuildingSpawner remnants"). Canonical spawn path is now `Moon1BuildOutBuildings` (cite: `Assets/_Project/Scripts/Editor/Moon1MasterBootstrap.cs:15`). That lives in `Editor/` (Tools-owned by path-ownership rule, but the gameplay-flavored spawn semantics make this a Gameplay call).

**Scope:** When the Gameplay agent next touches Moon 1 hero building spawning, replace any `Resources.Load<GameObject>("Moon1/Heroes/<id>")` or equivalent direct prefab fetch with `Tartaria.Core.ContentLoading.AddressableContentLoader.LoadHero("<id>")`. One-line swap per site.

**Blocking:** Nothing --- fallback keeps current behavior identical until the swap happens.


---

## 2026-06-02 --- Gameplay -> QA (Cowork) (Echohaven movement fix verification)
**Need:** Verify Echohaven_VerticalSlice player capsule translates under WASD in Play mode after the runtime-guard fix.

**Why:** Cowork reported W registers in InputProbeHUD but the capsule does not move. Root cause was `HandleMovementInput()` trusting a potentially-zeroed `InputActionAsset.Move` read over a direct `Keyboard.current` read. Fix lands in:
- `Assets/_Project/Scripts/Input/PlayerInputHandler.cs` (HandleMovementInput rewritten — direct keyboard read FIRST, asset value overrides only if non-zero, gamepad direct-read preserved).
- `Assets/_Project/Scripts/Integration/Moon1PlayerSetup.cs` (recovery guard auto-adds PlayerInputHandler if missing after player is located).

**Verification steps:** See `docs/playtests/2026-06-02-echohaven-movement-fix.md` § "What Cowork needs to verify". Need a Play-mode confirmation that:
1. Console shows `[Moon1PlayerSetup] PlayerInputHandler attached + ready.`
2. Holding W translates the capsule forward (camera-relative).
3. Holding A/S/D translates left/back/right.

**Blocking:** Sprint-2 movement milestone — capsule was reported stationary, so visible-Play artifact for the Echohaven slice is gated on this verify.

**Files:** Already edited (see above). HUD overlay: `Assets/_Project/Scripts/Input/InputProbeHUD.cs:81-91` shows live state.
