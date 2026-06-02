# Sprint 11 Lane 6 — Moon 1 Prefab Integrity Audit

**Date:** 2026-06-02
**Branch:** `agent/audit/prefab-integrity`
**Worktree:** `C:\dev\_wt_s11_l6_prefabs`
**Scope:** Every prefab under `Assets/_Project/Prefabs/Moon1/`, `Assets/_Project/Prefabs/Characters/`, plus the three `Assets/_Project/Prefabs/Buildings/Echohaven_*` hero buildings.
**Total prefabs scanned:** 390 (365 in Moon1, 22 in Characters, 3 in Buildings).
**Companion data:** `_sprint11_l6_data.csv` in this folder.

---

## TL;DR — Three project-wide defects, not 390 isolated bugs

This audit was scoped to "find broken prefabs" but discovered three repo-wide failure modes that invalidate the entire Moon 1 art pass. None of the 390 prefabs surveyed will render a real model at runtime today.

1. **Every FBX in the repo is a Git LFS pointer, not a binary mesh.** All 1,269 `.fbx` files under `Assets/` are 130 - 131-byte LFS stub files (header `version https://git-lfs.github.com/spec/v1`). LFS smudge has not run in this worktree. Every prefab that references an FBX therefore imports an empty mesh asset.
2. **All 347 "Blender" Moon 1 prefabs are binary-serialized `PrefabInstance` variants that point at those LFS-pointer FBX files.** They contain nothing but a name, a transform offset, and a `m_SourcePrefab` GUID. With the source unresolved, the variant is empty.
3. **Every Cathedral kit piece and every Echohaven hero building is composed from Unity built-in primitives (`Cube` / `Sphere` / `Cylinder`), not real geometry.** This is a direct violation of the 2026-05-30 CLAUDE.md NO-STUBS mandate: *"NEVER use `GameObject.CreatePrimitive` without an immediate URP-safe fallback path... Better: don't use primitives at all - load the real KayKit FBX or Cathedral kit prefab."*

The corollary is that the headcount classification below understates the damage: the 347 "LFS-VARIANT" prefabs would technically import as named-but-empty GameObjects, while the 27 "PRIMITIVE" prefabs render boxes/spheres - neither is a real Moon 1 asset.

---

## 1. Classification summary

| Class | Count | Meaning |
|---|---:|---|
| LFS-VARIANT (binary `PrefabInstance` -> LFS-pointer FBX) | 347 | All `Prefabs/Moon1/Blender/*.prefab`. Will fail to load mesh until `git lfs pull` runs AND the source-prefab `m_SourcePrefab` GUID actually resolves to a non-stub FBX importer. |
| PRIMITIVE (built from `Library/unity default resources` or a `m_Mesh: {fileID: 102xx, guid: 0...}`) | 27 | All 18 Cathedral kit pieces, all 3 Echohaven hero buildings, all 4 Moon 1 NPCs in `Prefabs/Characters/`, the Player prefab, plus MudGolem in Characters (8 primitives). |
| OTHER (text-YAML PrefabInstance to KayKit FBX, OR empty enemy stubs) | 16 | 12 KayKit character variants (Knight / Mage / Ranger / Rogue / Rogue_Hooded / Barbarian / 2 Mannequins / 4 Skeletons). All reference LFS-pointer FBX so will render empty. Plus 4 empty-stub Characters/ binaries (Korath, Thorne, CrystalSentry, ShadowStalker) which have neither a script nor a mesh. |

**Net assessment:** **0 prefabs of 390 are runtime-healthy** (every one of them either references a missing FBX, renders a primitive, or is an empty named GameObject).

---

## 2. Per-prefab table

The full per-prefab table is in `_sprint11_l6_data.csv` (390 rows x 8 columns: name, path, size, isBinary, orphanScripts, primMesh, libDefaultRefs, magenta, sourcePrefabRefs, classification). Below are the high-signal subsets.

### 2a. PRIMITIVE prefabs (27 - violation of NO-STUBS mandate)

| Path | Size | libDefault | primMesh | Defect |
|---|---:|---:|---:|---|
| `Assets/_Project/Prefabs/Buildings/Echohaven_CrystalSpire.prefab` | 225 663 | 1 | 0 | Composed from `Library/unity default resources` primitives (Crystal Spire built from sphere/cylinder primitives). |
| `Assets/_Project/Prefabs/Buildings/Echohaven_HarmonicFountain.prefab` | 229 567 | 1 | 0 | Same - procedural primitive composition. |
| `Assets/_Project/Prefabs/Buildings/Echohaven_StarDome.prefab` | 212 623 | 1 | 0 | ~60 `Detail_Ring0/1_*`, `Detail_Buttress_*`, `Detail_AntennaBar_*` children, all primitives. |
| `Assets/_Project/Prefabs/Characters/Player.prefab` | 10 844 | 1 | 0 | Capsule-primitive mesh, MeshFilter+MeshRenderer+CapsuleCollider+Rigidbody, **0 MonoBehaviours** (no PlayerInputHandler). Will not move on WASD/F310. |
| `Assets/_Project/Prefabs/Characters/Anastasia.prefab` | 10 844 | 1 | 0 | Same capsule shape, 0 MonoBehaviour, no NavMeshAgent, no Animator. |
| `Assets/_Project/Prefabs/Characters/Lirael.prefab` | 10 844 | 1 | 0 | Same. |
| `Assets/_Project/Prefabs/Characters/Cassian.prefab` | 10 844 | 1 | 0 | Same. |
| `Assets/_Project/Prefabs/Characters/Milo.prefab` | 10 828 | 1 | 0 | Same. |
| `Assets/_Project/Prefabs/Characters/MudGolem.prefab` | 24 971 | 0 | 8 | Text YAML, but assembles 8 primitive meshes. |
| `Assets/_Project/Prefabs/Moon1/Cathedral/Archway_4x7m.prefab` | 2 277 | 0 | 1 | `m_Mesh: {fileID: 102xx, guid: 0000...}` Unity built-in primitive + 16-hex (truncated) material GUID `d4f8e2c9a7b3f5e1`. |
| `Assets/_Project/Prefabs/Moon1/Cathedral/Column_Ornate_6.5m.prefab` | 2 277 | 0 | 1 | Same pattern. |
| `Assets/_Project/Prefabs/Moon1/Cathedral/Dome_Segment_E.prefab` ... `_W.prefab` (8 segments) | 2 285 - 2 286 | 0 | 1 each | Same pattern x 8. |
| `Assets/_Project/Prefabs/Moon1/Cathedral/Door_Grand_3x6m.prefab` | 2 282 | 0 | 1 | Same. |
| `Assets/_Project/Prefabs/Moon1/Cathedral/Foundation_16x16m.prefab` | 2 273 | 0 | 1 | Same. |
| `Assets/_Project/Prefabs/Moon1/Cathedral/RoseWindow_4x4m.prefab` | 2 292 | 0 | 1 | Same. |
| `Assets/_Project/Prefabs/Moon1/Cathedral/Spire_Base_2x2m.prefab` | 2 267 | 0 | 1 | Same. |
| `Assets/_Project/Prefabs/Moon1/Cathedral/Spire_Mid_Taper.prefab` | 2 281 | 0 | 1 | Same. |
| `Assets/_Project/Prefabs/Moon1/Cathedral/Spire_Top_MercuryBall.prefab` | 2 285 | 0 | 1 | `m_Mesh: {fileID: 10202, guid: 0000000000000000e000000000000000, type: 0}` (Unity Sphere) + material `guid: d4f8e2c9a7b3f5e1` (16 hex chars = truncated/invalid). |
| `Assets/_Project/Prefabs/Moon1/Cathedral/Wall_4x4m_Stone.prefab` | 2 273 | 0 | 1 | Same. |
| `Assets/_Project/Prefabs/Moon1/Cathedral/Wall_Corner_4x4m.prefab` | 2 281 | 0 | 1 | Same. |

All 18 Cathedral pieces share the same truncated 16-char material GUID `d4f8e2c9a7b3f5e1`. A valid Unity asset GUID is 32 hex chars; this one will not resolve at import, so each Cathedral mesh will appear pink/magenta in Editor and use the URP error shader at runtime.

### 2b. OTHER - empty character placeholders (4) + KayKit text-YAML variants (12)

The 4 empty placeholders are 4 728-byte binary `PrefabInstance` files containing **just a Transform + CharacterController, no mesh, no MonoBehaviour, no Animator**:

| Path | Size | Defect |
|---|---:|---|
| `Assets/_Project/Prefabs/Characters/Korath.prefab` | 4 728 | Empty PrefabInstance - only Transform + CharacterController. No mesh, no scripts. |
| `Assets/_Project/Prefabs/Characters/Thorne.prefab` | 4 728 | Same. |
| `Assets/_Project/Prefabs/Characters/CrystalSentry.prefab` | 4 728 | Same. There is a `Prefabs/Moon1/Blender/CrystalSentry.prefab` (LFS-VARIANT) but the canonical Characters/ one is empty. |
| `Assets/_Project/Prefabs/Characters/ShadowStalker.prefab` | 4 728 | Same. Also has an LFS-VARIANT sibling in Moon1/Blender. |

The 12 KayKit text-YAML variants (`Char_Barbarian.prefab` through `Char_Skeleton_Warrior.prefab`) reference well-formed FBX GUIDs (e.g. `f565c162608a6b44990b3d1d46ed2c18` for `Knight.fbx`) - but the target FBX `Assets/_Project/Models/Characters/KayKit/Knight.fbx` is **131 bytes** (LFS pointer). So while the prefab YAML is healthy, no mesh will resolve at runtime until `git lfs pull` succeeds and the FBX is imported.

### 2c. LFS-VARIANT prefabs (347, all in `Prefabs/Moon1/Blender/`)

Every prefab in this folder is a 3 140 - 3 200-byte binary file with this structure:
- One `PrefabInstance` record
- A handful of `PropertyModification` rows (typically `m_LocalPosition`, `m_LocalRotation`, `m_LocalEulerAnglesHint`, `m_Name`)
- A `m_SourcePrefab: {guid: <16 bytes>, type: 3}` pointing at an FBX-derived prefab whose source FBX is a 130-byte LFS pointer.

Sample: `Assets/_Project/Prefabs/Moon1/Blender/LiraelGuardian.prefab` (3 160 bytes) sources a GUID that resolves to a 130-byte `Assets/_Project/Models/Blender/Moon1/LiraelGuardian.fbx` LFS pointer file (sha256:93cc5ae1bca806ba96e861eae97602a336f7bb3a00c312c058f787037a4ffabc, size 46940). The real 46 940-byte FBX is in LFS storage but not checked out in this worktree.

Every single one of the 347 will report as "empty" or "broken" at import unless LFS smudge runs and the BlenderImportPostprocessor regenerates a healthy prefab variant.

---

## 3. Defect colour-codes

### Red - BROKEN (will not render anything usable)

All 347 LFS-VARIANT prefabs + the 4 empty Characters/ placeholders (Korath, Thorne, CrystalSentry, ShadowStalker) + 4 NPC capsule placeholders (Anastasia, Lirael, Cassian, Milo) + Player. **357 prefabs total.**

The reason all 347 Blender prefabs land here rather than in "Suspicious": the audit per-CLAUDE.md is meant to verify *runtime artifacts*. A binary `PrefabInstance` whose `m_SourcePrefab` resolves to a 130-byte LFS pointer is not a runtime artifact. The prefab will instantiate as a named empty GameObject + transform offset, with no mesh, collider, animator, or script.

### Yellow - SUSPICIOUS (primitive geometry, may be intentional placeholder)

| Path | Reason |
|---|---|
| All 18 `Prefabs/Moon1/Cathedral/*.prefab` | Real text-YAML prefabs, but each is one Unity primitive + one truncated material GUID. May be intentional kit-building placeholders awaiting KayKit Medieval Hexagon mesh swap. |
| `Prefabs/Buildings/Echohaven_CrystalSpire.prefab`, `Echohaven_HarmonicFountain.prefab`, `Echohaven_StarDome.prefab` | Hero buildings built procedurally from Unity primitives (visible in the `Detail_Ring0_*`, `Detail_Buttress_*`, `Detail_AntennaSpire`, `Detail_Crystal_*` child names). Likely the procedural builder from `MoonsRegistry`/`MoonContentGenerator`. |
| `Prefabs/Characters/MudGolem.prefab` | 8-primitive composition. Could be intentional voxel-golem style, but should at least use URP/Lit material. |

**Total: 22 yellow.**

### White - HEALTHY (renders a real model at runtime)

**Zero prefabs qualify** in this worktree because every FBX is an LFS pointer. The 12 KayKit text-YAML variants would qualify if `git lfs pull` were run, but in their current state they reference 131-byte stubs.

---

## 4. Top 10 fix-priority prefabs

Priority is "what unlocks the most Moon 1 gameplay" per the NO-STUBS mandate, not "which file is most broken."

| # | Path | Defect | Recommended fix |
|---:|---|---|---|
| 1 | `Assets/_Project/Prefabs/Characters/Player.prefab` | Binary PrefabInstance, 0 MonoBehaviour, capsule mesh, no `PlayerInputHandler`, `CharacterController` absent (only Rigidbody). | Rebuild as text-YAML prefab from `KayKit/Char_Knight.prefab` (or a slimmer alternative). Attach `PlayerInputHandler`, `CameraController` hook, `CharacterController`, `Rigidbody isKinematic=true`. Without this, neither F310 nor WASD will produce movement regardless of input wiring. |
| 2 | `Assets/_Project/Prefabs/Buildings/Echohaven_StarDome.prefab` | Primitive composition (60+ Detail_* primitives). Hero building. | Swap base mesh to KayKit Castle Dome FBX once LFS is pulled, OR ship a real Blender export under `Assets/_Project/Models/Blender/Moon1/StarDome.fbx`. Currently the 60-child primitive build is the procedural fallback. |
| 3 | `Assets/_Project/Prefabs/Buildings/Echohaven_CrystalSpire.prefab` | Same - primitive composition with 4 `Detail_Crystal_*` children. | Replace with `Cathedral_Spire.fbx` mesh once available. Per CLAUDE.md this is exactly the "GameObject.CreatePrimitive without URP-safe fallback" anti-pattern. |
| 4 | `Assets/_Project/Prefabs/Buildings/Echohaven_HarmonicFountain.prefab` | Same. | Same fix path. |
| 5 | `Assets/_Project/Prefabs/Characters/Anastasia.prefab` | Capsule + zero MonoBehaviour. Anastasia is a named Moon 1 character. | Rebuild as Prefab Variant of an LFS-resolved `AnastasiaPrincess.fbx`, attach `NPC` + `Animator` + `NavMeshAgent`. (LFS-pointer source exists at `Models/Blender/Moon1/AnastasiaPrincess.fbx`.) |
| 6 | `Assets/_Project/Prefabs/Characters/Lirael.prefab` | Same. | Rebuild as Prefab Variant of `LiraelGuardian.fbx` (LFS-pointer source exists). |
| 7 | `Assets/_Project/Prefabs/Characters/Milo.prefab` | Same. Hero quest NPC. | Rebuild as Prefab Variant of `MiloBoy.fbx` (LFS-pointer source exists). |
| 8 | `Assets/_Project/Prefabs/Characters/Cassian.prefab` | Same. | Rebuild as Prefab Variant of `CassianCarter.fbx` (LFS-pointer source exists). |
| 9 | All 18 `Assets/_Project/Prefabs/Moon1/Cathedral/*.prefab` | Each = 1 Unity primitive mesh + 1 truncated 16-char material GUID (`d4f8e2c9a7b3f5e1`). | First fix material GUID to a valid 32-char URP/Lit material. Then swap each `m_Mesh` to a KayKit cathedral kit fileID. Cathedral is the Moon 1 hero building per docs/15. |
| 10 | `Assets/_Project/Prefabs/Characters/Korath.prefab`, `Thorne.prefab`, `CrystalSentry.prefab`, `ShadowStalker.prefab` | All four 4 728 bytes, empty (no mesh, no script). | Either delete (and re-source from `Prefabs/Moon1/Blender/<Name>.prefab` siblings once LFS resolves), or attach a `SkinnedMeshRenderer` + `Animator` + `NavMeshAgent` + AI script (e.g. `Moon1MudGolemAI` for ShadowStalker). |

---

## 5. Recommended workstream

The audit's job ends at "name the defects," but the fix order is forced by the dependency graph:

1. **Run `git lfs install && git lfs pull`** in this worktree. Until that happens, no FBX-referencing fix will produce a runtime artifact. (Verify with `[System.IO.File]::ReadAllBytes('Assets/_Project/Models/Blender/Moon1/LiraelGuardian.fbx').Length` - expect ~46 940, not 130.)
2. **Re-trigger `BlenderImportPostprocessor`** (Unity menu `Tartaria -> Moon 1 -> Run Blender Batch`) to regenerate the 347 `Prefabs/Moon1/Blender/*.prefab` variants from the now-real FBX. The current 3.1-KB binary PrefabInstances should grow to 8 - 30 KB text YAML each.
3. **Fix the 18 Cathedral truncated material GUIDs.** Open one prefab in Inspector -> re-assign URP/Lit material -> save -> propagate.
4. **Replace primitive hero buildings** (Echohaven_StarDome / CrystalSpire / HarmonicFountain) with real meshes either from KayKit Castle pack or new Blender exports under `Models/Blender/Moon1/`.
5. **Rebuild Player.prefab + 4 Moon 1 NPC prefabs** as text-YAML variants of resolved FBX sources, with required components (CharacterController + PlayerInputHandler for Player; NavMeshAgent + Animator + dialogue NPC script for NPCs).
6. **Delete or rebuild** the 4 empty Characters/ placeholders (Korath, Thorne, CrystalSentry, ShadowStalker).

---

## 6. Methodology notes

- All file inspection was via `[System.IO.File]::ReadAllBytes` then ASCII-decode + regex over printable runs of >=4 chars. Unity binary `PrefabInstance` files still embed component-class names, GUIDs, property paths, and string values as ASCII, so this method captures most of the semantic surface even without parsing Unity's UnityFS binary tree.
- Primitive-mesh detection regex: `m_Mesh:\s*\{fileID:\s*102\d\d,\s*guid:\s*0{16}[a-fA-F0-9]0{15}` (matches Unity built-in mesh fileID 10201 - 10299 with the all-zeros-plus-one-hex-char guid pattern unique to engine-bundled resources).
- `libDefaultRefs` counts occurrences of the literal string `Library/unity default resources` in the prefab payload (binary or text) - the most reliable cross-encoding signal for "Unity built-in primitive source."
- `m_PrefabInstance: {fileID: 0}` was initially counted as a defect but is in fact the canonical "I am not a nested prefab instance" marker on every object in a non-variant prefab; that signal was discarded.
- No magenta-shader (`Hidden/InternalErrorShader`) references were found in any prefab. The Cathedral truncated-GUID material will *resolve to* magenta at import time, but the prefab text itself never names the error shader.

---

## 7. Cross-references

- CLAUDE.md (root) - 2026-05-30 NO-STUBS mandate, rule 4: "NEVER use `GameObject.CreatePrimitive` without an immediate URP-safe fallback path that sets `_BaseColor` and tags the line with `// URP-safe`. Better: don't use primitives at all".
- `docs/audits/PREFAB_VALIDITY_2026-05-31.md` - prior pass on prefab health (4 094 bytes, narrower scope).
- `docs/audits/MOON1_BUILD_AUDIT_2026-05-31.md` - Moon 1 audit referencing the same Echohaven hero buildings.
- `Assets/_Project/Scripts/Editor/BlenderImportPostprocessor.cs` - the auto-variant generator that *should* convert FBX imports into healthy prefabs (currently inert because every FBX is LFS-stub).

---

*End of Sprint 11 Lane 6 audit.*
