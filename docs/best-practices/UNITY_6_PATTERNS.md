# UNITY 6 PATTERNS — Best-Practice Distillation for TARTARIA

> **R205 UPDATE (2026-06-08):** Per NATRIX directive after deep-research synthesis. Sections 1-12 below were rewritten to reflect the 60+ cited sources from `docs/research/UNITY_RPG_LEVEL_BUILDING_DEEP_DIVE_2026-06-08.md`. The original Sprint-11 content is preserved under section 13 (legacy).
> **Status:** AUTHORITY. Overrides any older patterns scattered across CLAUDE.md or per-feature docs.

Every Unity-side decision in TARTARIA defers to this file. If something isn't here, default to Unity 6 manual + the deep-research report.

---

## R205 SECTION — Distilled Unity 6 Patterns (2026-06-08)

### 1. Scene composition — multi-scene additive

**Canonical pattern:** Split into ≥4 additive scenes loaded with `LoadSceneMode.Additive`:

| Scene | Lifetime | What lives here |
|---|---|---|
| `Boot.unity` | Whole session | EventSystem, GameManager, SaveManager, persistent singletons |
| `UI_Overlay.unity` | Whole session | HUD canvas, dialogue UI, pause menu |
| `Managers_Moon1.unity` | While in Moon 1 | Moon1NarrativeBeats, EchohavenContentSpawner, AnastasiaController, LiraelController, ZoneController, TartarianHourCycle |
| `Echohaven_VerticalSlice.unity` | While in Moon 1 | Static environment (Terrain, hero buildings, prop instances, lighting, NavMesh) |

**Cross-scene wiring:** ScriptableObject event channels per Unity's official "Create modular game architecture with ScriptableObjects (Unity 6 edition)" guide. NO direct cross-scene `GameObject.Find` calls.

**TARTARIA status:** Multi-scene already in use (`Boot` + `UI_Overlay` + `Echohaven_VerticalSlice` + `Moon1_Systems`). Honest gap: Moon1_Systems is currently a prefab inside Echohaven_VerticalSlice, not an additive scene. R210+ task: split out.

### 2. Prefab Variants — base + override discipline

**Unity blog rule:** *"Most scenes should be constructed from Prefabs with minimal overrides."*

**Canonical pattern:**
- Author 1 base prefab per archetype (Wall, Cottage, NPC, Enemy)
- Author N variants for visual swaps (warm-stone Wall vs cold-stone Wall)
- Variants override mesh refs / materials only — NOT logic components
- Base + variants in SAME Addressables group (prevents duplicate-bake bug)

**TARTARIA application:**
- R172 12-piece modular kit = 12 base prefabs. Each Moon's stone palette = a variant set.
- 1 Mud Golem mesh × 13 Moon biome variants = 13 prefab variants of one base.
- R181 6 villager archetypes = base prefabs; each Moon's villager NPCs = material variants.

**Honest gap:** R172-R200 used direct `Instantiate(FBX)` not Prefab Variants. R220+ refactor.

### 3. Addressables — Moon as group

| Addressables group | Contents |
|---|---|
| `Core` | Modular kit + 6 villager archetypes + 1 Mud Golem mesh + Player Elara + UI canvas |
| `Moon1` | Moon 1 scene + Moon 1 palette materials + Moon 1 specific props (Anastasia Rocker, Pipe Organ, Giant Skeleton) |
| `MoonN` | One group per Moon (palette materials + specific props + scene shell) |

**Loading:** `Addressables.LoadSceneAsync("Moon1", LoadSceneMode.Additive)`.

**Honest gap:** 0 Addressables groups today. R210+ task.

### 4. Lighting — APV (Adaptive Probe Volumes) is Unity 6 default

**Setup:**
- Lights → **Mixed** or **Baked**
- GameObjects → **"Contribute Global Illumination"**
- MeshRenderers → **"Receive GI: Light Probes"**
- Reflection Probes → enable **"Probe Volumes"** in BOTH "Realtime" and "Baked"
- Bake: `Window → Rendering → Adaptive Probe Volumes → Bake`

**Unity 6 APV killer features:**
- **Lighting Scenario Blending** for day-night cycle (Day 1 dawn → Day 17 17th-hour eruption blend)
- **Sky Occlusion** for outdoor Moons
- **Disk Streaming** for large worlds (1km² Echohaven)

**Hybrid:** Lightmap hero buildings (Dome, Fountain, Spire) for crisp shadows. APV handles everything else.

**TARTARIA status:** R152 added URP Volume + Bloom. APV not wired. R213 task.

### 5. Terrain — Unity Terrain outdoor, mesh for caves

**Use Unity Terrain when:** splat-mapped texture layers + detail mesh / grass billboards + tree instancing + NavMesh bake.

**Use mesh-based:** Underground Dome chamber (overhangs/cliffs require negative Y), Mud Pool depressions.

**Splat layers per R171 rules:** Matte stone, low normal density, Roughness 0.85+. NOT Polyhaven 4K (explicitly rejected). Hand-authored stylized.

**TARTARIA status:** Unity Terrain in use for 1km² Echohaven (R132). Splat textures still stock. R214 task.

### 6. ProBuilder — graybox ONLY, never ship

**Rule:** ProBuilder is in-Editor blockout iteration. Replace with Blender hero meshes once layout locks. Don't ship ProBuilder geo.

**TARTARIA application:** ProBuilder NOT used this session — went straight from primitive blockouts (R148) to Blender FBX (R151+).

### 7. Snap grid — 1m standard

**Rule:** Modular kit MUST snap to 1m. Matches Synty POLYGON, KayKit Medieval, Quaternius defaults.

**TARTARIA application:** R172 12-piece kit authored at 1m. Bounds verified — wall 1m × 3m × 0.3m, floor 1m × 0.2m × 1m, column 0.5m × 3m. Compliant.

### 8. URP renderer — performance rules

| Setting | Value | Why |
|---|---|---|
| **Strip Unused Post-Processing Variants** | Enabled | Build size + shader compile |
| **SSAO** | Renderer Feature (NOT Volume override) | Independent of post-process stack |
| **Decal Renderer Feature** | Minimize use | Unity guidance — extra render pass cost |
| **Volume Update Mode** | Via Scripting | Manual `UpdateVolumeStack` on transitions |
| **Bloom** | Threshold 1.2, Intensity 0.5 (R203 tuned) | Aether-Gold reads without washout |
| **Tonemap** | Neutral | Per Art Bible R171 |
| **Color Adjustments** | Post-exposure -0.3, Contrast 5, Saturation 0 (R203) | Matte stone visibility |
| **Camera POV target** | y=1.7 player, y=3-5 hero shots | Avoid panorama compression |

**TARTARIA status:** SSAO wired R152. Bloom + tonemap + color adjustments wired R152 + R203. Strip Unused Variants: R216 task.

### 9. Occlusion Culling — skip outdoor, bake interior

**Rule:** Bake only for distinct enclosed zones (Dome interior). Skip outdoor.

**TARTARIA status:** Not yet baked. R218 task (interior Dome).

### 10. Static flags + lightmap UVs

**Per import:**
- StaticEditorFlags: **BatchingStatic + NavigationStatic + ContributeGI + OccluderStatic + OccludeeStatic**
- ModelImporter → **Generate Secondary UV** = true (baked lighting)
- ModelImporter → **Material Search** = Local
- ModelImporter → **Material Location** = External

**TARTARIA status:** Static flags applied in R151+ instance code. Generate Secondary UV: NOT set in `BlenderImportPostprocessor.cs:67`. R215 task.

### 11. Vegetation density — Lonely Mountains pattern

**Per deep-research:** Lonely Mountains hand-placed *hundreds of thousands of instances* from small library (EST 30-50 foliage types). Small library + dense placement.

**Density target near plaza:** 1 plant per 1-2m² (so 30m radius zone = ~700 plants).

**TARTARIA status:** R177 placed 110, R201 increased to 800+. Compliant.

### 12. Camera presentation rules

**For non-panorama screenshots:**
- Player POV: y=1.7 (human eye), pitch 2-5°
- Hero shots: y=3-5, yaw 25-35°
- Avoid y=10+ panoramas — they compress everything, make level read sparse even when dense

**TARTARIA application:** R201-R204 ground shots show good density. Future hero shots default to y≤5 + small angle.

---

## R210+ Sprint E task list (carrying forward)

| Round | Task | Per pattern |
|---|---|---|
| R210 | Split Moon1_Systems into additive scene `Managers_Moon1.unity` | §1 multi-scene |
| R211 | Convert hero buildings + characters to Prefab Variants | §2 prefab variants |
| R212 | Create Addressables groups: Core + Moon1...Moon13 | §3 addressables |
| R213 | Bake APV + lightmaps on hero buildings | §4 APV |
| R214 | Re-author terrain splats to stylized matte | §5 terrain |
| R215 | Set `generateSecondaryUV = true` in BlenderImportPostprocessor | §10 static UVs |
| R216 | Enable Strip Unused Post-Processing Variants | §8 URP perf |
| R217 | Add ScriptableObject event channels for cross-scene wiring | §1 SO glue |
| R218 | Bake occlusion on Dome interior chamber | §9 occlusion |
| R219 | Verify static flags applied on all 600+ scene placements | §10 static |
| R220 | Full pass smoke test — verify nothing broken | gauntlet |

R210-R220 are CODE/SCENE WIRING tasks. Distinct from R171-R200's content-authoring sprint.

---

## 13. Legacy Sprint-11 patterns (2026-06-02, preserved)

> Unity 6 (6000.x) manual best practices for the patterns we use heavily. Sourced from the official Unity Manual + Scripting Reference at https://docs.unity3d.com/6000.3/Documentation/Manual/ and verified against our codebase. 2026-06-02.
>
> When in doubt, the manual is the source of truth. Cite specific manual section when a debate arises.

---

## 1. Git LFS for binary assets

**The rule:** every binary asset Unity touches (FBX, audio, prefab variants pointing at FBX, textures >1 MB) MUST be tracked by Git LFS. Without LFS, clones get 130-byte pointer files where real binaries should be — Unity loads them, sees garbage, and silently fails to import.

**Why it matters for us:** Sprint 11 L6 found **0 of 390 prefabs runtime-healthy** because every FBX was an unpulled LFS pointer. Prefab variants pointed at "FBX" files containing literally the text `version https://git-lfs.github.com/spec/v1`.

**Setup once per repo:**

```
git lfs install
git lfs track "*.fbx"
git lfs track "*.wav"
git lfs track "*.mp3"
git lfs track "*.ogg"
git lfs track "*.png" --filter-only
git add .gitattributes
```

**Per-fresh-clone:**

```
git clone <repo>
cd <repo>
git lfs pull
```

**Verify a file is real (not a pointer):**

```powershell
Get-Item Assets\_Project\Models\Blender\Moon1\Lirael.fbx | Select-Object Length
# If < 200 bytes, it's still a pointer
```

**Manual references:**
- Unity Manual → Asset Workflow → Importing → Native File Formats
- Git LFS docs at https://git-lfs.com/

---

## 2. Singleton bootstrap pattern

**The rule:** singletons that must exist regardless of scene contents should use `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` to self-instantiate. Do NOT rely on a scene-placed component — sibling agents delete the scene reference and the singleton vanishes silently.

**Why it matters for us:** Sprint 11 found DialogueManager was authored as a scene singleton. Earlier this session we added an AutoBootstrap:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
static void AutoBootstrap()
{
    if (Instance != null) return;
    var existing = UnityEngine.Object.FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
    if (existing != null) { Instance = existing; return; }
    var go = new GameObject("[DialogueManager]");
    UnityEngine.Object.DontDestroyOnLoad(go);
    Instance = go.AddComponent<DialogueManager>();
    Debug.Log("[DialogueManager] AutoBootstrap created singleton.");
}
```

**Manual references:**
- Unity Manual → Scripting → Scripting Concepts → Scripting Restrictions → Domain Reload
- Unity Manual → Scripting → Attributes → RuntimeInitializeOnLoadMethod
- `RuntimeInitializeLoadType` enum values: `SubsystemRegistration`, `AfterAssembliesLoaded`, `BeforeSplashScreen`, `BeforeSceneLoad`, `AfterSceneLoad`. Use `BeforeSceneLoad` for systems other scene objects subscribe to in `Awake`.

---

## 3. Prefab Variants pipeline (FBX → URP material → Prefab)

**The canonical Unity 6 import chain for character / prop assets:**

1. FBX import → `ModelImporter` sets animationType, scale, axis. Best practice: use `[ScriptedImporter]` or `AssetPostprocessor.OnPreprocessModel` for project-wide defaults.
2. FBX → Materials extracted on first import. For URP, materials should be assigned URP/Lit shader, not the legacy Standard.
3. Prefab Variant created from the imported FBX. The variant is what gameplay scripts reference.

**Our pattern:** `Assets/_Project/Scripts/Editor/BlenderImportPostprocessor.cs` (Sprint 10 L6) does this auto-config for NPCs under `Assets/_Project/Models/Blender/Moon1/`.

**Common failure modes:**

- **Magenta material** = shader not URP-compatible. Fix: assign URP/Lit and call `material.SetColor("_BaseColor", color)` not the legacy `material.color = color` (which writes `_Color` ignored by URP/Lit).
- **Capsule fallback** = code-path calls `GameObject.CreatePrimitive(PrimitiveType.Capsule)`. Per CLAUDE.md NO-STUBS this is failure unless the call has `// URP-safe` and sets `_BaseColor` on the resulting material — and even then it's only a placeholder.
- **Invalid material GUID** (16 chars instead of 32) → Unity treats as missing → magenta. Regenerate the material or fix the YAML GUID.
- **animationType = None** on a humanoid → no animation playback. Set to Generic (no humanoid retargeting) or Humanoid (with avatar definition).

**Manual references:**
- Unity Manual → Asset Workflow → Models → Model Import Settings
- Unity Manual → URP → Materials and shaders
- Unity Manual → Animation → Mecanim → Humanoid setup

---

## 4. Mecanim humanoid retargeting

**The rule:** for character animations to retarget across rigs (so you can swap NPC bodies and keep the same idle animation), use `animationType = Humanoid` with a properly mapped Avatar.

**Requirements for Humanoid:**

1. The rig must have the canonical Mecanim bone hierarchy: Hips → Spine → Chest → Neck → Head, with Hips → LeftUpLeg → LeftLeg → LeftFoot (and right), Spine → LeftShoulder → LeftArm → LeftForeArm → LeftHand (and right).
2. Unity's Avatar Configuration window auto-maps if bone names are conventional; manual mapping if non-standard.
3. The Animator Controller drives bone transforms via the Avatar — no muscle-by-muscle wiring needed.

**Our current state:** Sprint 9 L5 rendered Lirael/Anastasia/Cassian FBX via Blender, but the Blender `gen_*.py` scripts `bpy.ops.object.join()` all meshes and emit zero bone hierarchy. So `animationType = Humanoid` would fail Avatar mapping.

**Workaround until rig pipeline exists:** Sprint 10 L6 sets `animationType = Generic`. Animations can play but won't retarget — every NPC needs its own animation clip authored against its specific (non-rig) geometry.

**Sprint 11+ TODO** (documented in `Moon1RebindNPCPrefabs.cs` source comment): upgrade Blender script to emit Mecanim-compatible armature + skin weights. Then re-import as Humanoid.

**Manual references:**
- Unity Manual → Animation → Mecanim → Avatar Creation
- Unity Manual → Animation → Mecanim → Animator Controller
- Unity Manual → Asset Workflow → Models → Rigging

---

## 5. Input System Package (not legacy Input)

**The rule:** in Unity 6, the Input System Package is the canonical input layer. Legacy `UnityEngine.Input.GetKey(...)` throws `InvalidOperationException` if Input System is the active backend.

**Detect the active backend:** check `Edit → Project Settings → Player → Active Input Handling`. Should be "Input System Package (New)" or "Both".

**The canonical pattern:**

```csharp
using UnityEngine.InputSystem;

void Update()
{
    if (Keyboard.current.eKey.wasPressedThisFrame) { /* interact */ }
    if (Keyboard.current.escapeKey.isPressed) { /* pause */ }
    var move = new Vector2(
        Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
        Keyboard.current.sKey.isPressed ? -1 : Keyboard.current.wKey.isPressed ? 1 : 0);
    var gp = Gamepad.current;
    if (gp != null) move += gp.leftStick.ReadValue();
}
```

**For gated cases** (e.g. `Moon2FirstPurgeTrigger.cs` was using `Input.GetKeyDown` in an `#else` branch — which Unity 6 still compiles in Safe Mode even if `#if ENABLE_INPUT_SYSTEM` is set):

```csharp
#if ENABLE_INPUT_SYSTEM
    bool pressed = Keyboard.current?.eKey.wasPressedThisFrame ?? false;
#else
    bool pressed = UnityEngine.Input.GetKeyDown(KeyCode.E); // fully qualified to defeat Tartaria.Input namespace shadow
#endif
```

**Manual references:**
- Unity Manual → Input → Input System
- Input System Package docs at https://docs.unity3d.com/Packages/com.unity.inputsystem@latest

---

## 6. URP material setup

**The rule:** URP (Universal Render Pipeline) uses different shaders than the Built-in pipeline. Code that writes legacy `_Color` won't reflect in URP/Lit — must write `_BaseColor`.

**Property name mapping (Standard → URP/Lit):**

| Built-in property | URP/Lit property |
|---|---|
| `_Color` | `_BaseColor` |
| `_MainTex` | `_BaseMap` |
| `_Metallic` | `_Metallic` (same) |
| `_Glossiness` | `_Smoothness` |
| `_EmissionColor` | `_EmissionColor` (same) |

**The runtime-tint pattern:**

```csharp
var urpLit = Shader.Find("Universal Render Pipeline/Lit");
if (urpLit != null && material.shader.name.Contains("Standard"))
{
    material.shader = urpLit;
}
material.SetColor("_BaseColor", desiredColor); // URP-safe
```

**Detecting magenta-on-error:**

```csharp
if (renderer.material.shader.name.Contains("Hidden/InternalErrorShader"))
{
    Debug.LogError($"[Magenta] {renderer.name} fell back to InternalErrorShader. Material GUID likely invalid.");
}
```

**Manual references:**
- Unity Manual → Universal Render Pipeline → Lit shader
- URP package docs at https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest

---

## 7. Asset serialization (force text mode)

**The rule:** for scene + prefab files to be diffable and surgically editable, Unity must save in YAML text mode. Default in Unity 6 is binary.

**Set once per project:**

`ProjectSettings/EditorSettings.asset`:

```yaml
m_AssetSerializationMode: 2  # 0=Mixed, 1=Force Binary, 2=Force Text
```

OR via Editor → Project Settings → Editor → Asset Serialization → Mode: Force Text.

**Why it matters for us:** Sprint 11 L5 found the Moon1_Systems orphan was 4 inline `!u!115 MonoScript` blocks in the scene YAML. `RemoveMonoBehavioursWithMissingScript` (the managed API) only sees the `!u!114 MonoBehaviour` entries that point at them, not the `!u!115` definitions themselves. YAML surgery on the saved file is required — and YAML surgery requires text serialization.

**Manual references:**
- Unity Manual → Editor → Project Settings → Editor Settings → Asset Serialization

---

## 8. Build pipeline (BuildPipeline.BuildPlayer)

**The Unity 6 canonical pattern:**

```csharp
using UnityEditor;
using UnityEditor.Build.Reporting;

public static void BuildWin64()
{
    var scenes = EditorBuildSettings.scenes
        .Where(s => s.enabled)
        .Select(s => s.path)
        .ToArray();

    var buildOpts = new BuildPlayerOptions
    {
        scenes = scenes,
        locationPathName = "Builds/itch_assets/TARTARIA_Moon1.exe",
        target = BuildTarget.StandaloneWindows64,
        options = BuildOptions.None,
    };

    BuildReport report = BuildPipeline.BuildPlayer(buildOpts);
    if (report.summary.result == BuildResult.Succeeded)
        Debug.Log($"[Build] Win64 OK: {report.summary.totalSize / 1024 / 1024} MB");
    else
        Debug.LogError($"[Build] Win64 FAILED: {report.summary.totalErrors} errors");
}
```

**Batchmode invocation from PowerShell:**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe" `
    -batchmode -nographics -projectPath "C:\dev\TARTARIA_new" `
    -executeMethod Tartaria.Editor.Moon1ItchBuild.BuildWin64 `
    -quit -logFile "Logs\build_win64.log"
```

**Caveat:** screenshot capture during build requires a display surface — `-nographics` disables `ScreenCapture.CaptureScreenshot`. For Moon1ItchScreenshotCapture, omit `-nographics`.

**Manual references:**
- Unity Manual → Build pipeline → Builds
- BuildPipeline API reference at https://docs.unity3d.com/6000.3/Documentation/ScriptReference/BuildPipeline.html

---

## 9. Scene management (additive scenes, scene events)

**The rule:** Unity 6's `SceneManager` supports additive loading + per-scene events. For "Bootstrap" scenes that load gameplay scenes additively, use `LoadSceneMode.Additive` and `SceneManager.SetActiveScene` to control which scene is "active" (for skybox, lighting settings).

**The canonical pattern:**

```csharp
using UnityEngine.SceneManagement;

void Start()
{
    SceneManager.sceneLoaded += OnSceneLoaded;
    SceneManager.LoadScene("Echohaven_VerticalSlice", LoadSceneMode.Additive);
}

void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == "Echohaven_VerticalSlice")
        SceneManager.SetActiveScene(scene);
}
```

**Detecting scene changes** (used by SaveSlotPanel for thumbnail capture):

```csharp
SceneManager.activeSceneChanged += (oldScene, newScene) =>
{
    Debug.Log($"[Scene] active: {oldScene.name} → {newScene.name}");
};
```

**Manual references:**
- Unity Manual → Scenes → Scene Management
- SceneManager API at https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.SceneManager.html

---

## 10. TextMeshPro setup

**The rule:** Unity 6 ships TextMeshPro by default. Use TMP_Text + TMP_FontAsset for any in-game text.

**Common pitfall:** missing TMP Essential Resources on fresh clone shows "TMP Essentials" prompt when opening any scene with TMP_Text. Fix: `Window → TextMeshPro → Import TMP Essential Resources` once per fresh clone.

**Asmdef:** any script that touches `TMPro` types needs `Unity.TextMeshPro` in its asmdef references. Sprint 11 found `Tartaria.Combat.asmdef` was missing this (DamagePopup used TMP_Text → CS0246 → cascade compile fail).

**Manual references:**
- TextMeshPro package docs

---

## 11. Yarn Spinner DialogueRunner (case sensitivity!)

**The rule:** `DialogueRunner.NodeExists(nodeName)` and `StartDialogue(nodeName)` are **case-sensitive**. Yarn node titles are typically `snake_case_lower`; if your speaker-map lookup returns `PascalCase`, NodeExists returns false silently.

**The fix:** keep speaker keys + node values consistent with the actual Yarn file. Best: write the `(speaker, message) → node` lookup directly from the `.yarn` files at Editor time so PascalCase drift is impossible.

**Our pattern that broke** (Sprint 11 L7):

```csharp
// BROKEN — PascalCase node target, doesn't match any Yarn node
{ "Milo Brightway", "Milo_TutorialIntro" },

// FIX — snake_case to match Yarn title:
{ "Milo", "milo_tutorial_step_1_brazier" },
```

**Manual references:**
- Yarn Spinner docs at https://docs.yarnspinner.dev/

---

## 12. Worktree workflow (multi-agent safety)

**The rule (from `docs/agents/WORKTREE_MANDATE.md`):** when N parallel agents work, create N `git worktree`s up front. Agents never share `C:\dev\TARTARIA_new`. Each works only on their pre-assigned `C:\dev\_wt_<sprint>_<lane>_<short>` path.

**Director setup before dispatch:**

```powershell
git worktree add C:\dev\_wt_s12_l1_foo -b agent/<area>/<short> origin/feature/consolidate-moon-architecture
```

**Why:** `.git/index.lock` lives per-worktree. No two agents touch the same lock file. Eliminated all `.git/config` corruption across Sprints 7+8+9+10+11 (50+ lanes).

**Cleanup after merge:**

```powershell
git worktree remove C:\dev\_wt_<sprint>_<lane>_<short> --force
git branch -d agent/<area>/<short>
```

---

## 13. Banned identifiers / namespace shadows

**The rule (from `docs/agents/API_CONTRACT.md`):** these namespace names shadow `UnityEngine` types and cause CS0234 cascades:

- `Tartaria.Time` (shadows `UnityEngine.Time`)
- `Tartaria.Input` (shadows `UnityEngine.Input`)
- `Tartaria.Camera` (shadows `UnityEngine.Camera`) — already in use, every `Camera.main` must be `global::UnityEngine.Camera.main`
- `Tartaria.Animation`, `Tartaria.Random`, `Tartaria.Color`, `Tartaria.Object`, `Tartaria.Debug`, `Tartaria.Mathf`

**Safe namespace roots:** `Tartaria.AI`, `Tartaria.Audio`, `Tartaria.Combat`, `Tartaria.Core`, `Tartaria.Core.GameTime`, `Tartaria.Editor`, `Tartaria.Gameplay`, `Tartaria.Integration`, `Tartaria.Save`, `Tartaria.UI`, `Tartaria.VFX`.

---

## 14. NO silent catches (CLAUDE.md NO-DEBT mandate)

**The rule:** every `catch` block logs `e.GetType().Name`, `e.Message`, and the offending value. No exception is silently swallowed.

**Canonical pattern:**

```csharp
try { dangerousCall(); }
catch (Exception ex)
{
    Debug.LogError($"[{nameof(MyClass)}] {nameof(MyMethod)} failed: {ex.GetType().Name}: {ex.Message}\n  context: {key}={value}\n{ex.StackTrace}");
    // Either rethrow or document the fallback in a comment
}
```

**Sprint 11 L2 found 38 empty catches.** Phase 3 of the Moon 1 plan addresses them.

---

## 15. Recommended further reading

- **Unity Manual main** — https://docs.unity3d.com/6000.3/Documentation/Manual/
- **Scripting Reference** — https://docs.unity3d.com/6000.3/Documentation/ScriptReference/
- **URP** — https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest
- **Input System Package** — https://docs.unity3d.com/Packages/com.unity.inputsystem@latest
- **Addressables** — https://docs.unity3d.com/Packages/com.unity.addressables@latest (for content streaming, Moon 2+ work)
- **AssetPostprocessor** — for project-wide FBX import config without per-asset menu clicks

---

*v1.0 · 2026-06-02 · Unity 6.3.6f1 LTS. Update when Unity releases a new LTS or when we adopt a new pattern.*
