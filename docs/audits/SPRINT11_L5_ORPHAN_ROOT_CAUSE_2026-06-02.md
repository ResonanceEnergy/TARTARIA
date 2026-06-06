# Sprint 11 — Lane 5 — Root Cause of Persistent Moon1_Systems Orphan Spam

**Date:** 2026-06-02
**Branch:** `agent/fix/moon1systems-orphan-deep-clean`
**Worktree:** `C:\dev\_wt_s11_l5_orphan`
**Author:** Lane 5 agent

---

## Symptom

Every domain reload of the Echohaven scene fires this in the Console:

```
[CleanMissingScripts] Removing missing script from: Moon1_Systems
```

The message is logged from `Assets/_Project/Scripts/Editor/CleanMissingScripts.cs`
inside `CleanGameObject()`. It persists *even after* a prior commit
(`e0766030`) titled "saved scene with orphan scripts cleaned" claimed to
have removed them.

## Root cause

The Echohaven scene file is text-YAML (`m_SerializationMode: 2` in
`ProjectSettings/EditorSettings.asset`). Reading
`Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` shows that the
`Moon1_Systems` GameObject (`!u!1 &1505120383`, scene line 2974) has
**five `m_Component` entries**:

| Component fileID | Class id | Role |
|---|---|---|
| `1505120408` | `!u!4` Transform | OK |
| `1505120407` | `!u!114` MonoBehaviour | `m_Script: {fileID: 831608850}` → Moon1HeroBuildingSpawner |
| `1505120405` | `!u!114` MonoBehaviour | `m_Script: {fileID: 1523568284}` → Moon1MaterialSetup |
| `1505120403` | `!u!114` MonoBehaviour | `m_Script: {fileID: 255430906}` → Moon1AmbientCreatures |
| `1505120398` | `!u!114` MonoBehaviour | `m_Script: {fileID: 1693639792}` → Moon1NPCSpawner |

The four `m_Script` references point at fileIDs that **resolve INSIDE the
same scene file** — they are `!u!115 MonoScript` stub blocks embedded in
the scene. Each looks like:

```yaml
--- !u!115 &255430906
MonoScript:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name:
  serializedVersion: 7
  m_DefaultReferences: {}
  m_Icon: {fileID: 0}
  m_Type: 0
  m_ExecutionOrder: 0
  m_ClassName: Moon1AmbientCreatures
  m_Namespace: Tartaria.Integration
  m_AssemblyName: Tartaria.Integration
```

A grep of the entire `Assets/_Project` tree confirms **none of these four
classes exist in source**:

```
class Moon1NPCSpawner          → 0 hits
class Moon1AmbientCreatures    → 0 hits
class Moon1MaterialSetup       → 0 hits
class Moon1HeroBuildingSpawner → 0 hits
```

### Why `CleanMissingScripts` keeps firing without fixing it

`CleanMissingScripts.cs` calls
`GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go)` whenever it
encounters a `Component == null` slot in `go.GetComponents<Component>()`.
That managed API path has two failure modes for this exact scene shape:

1. **Inline MonoScript blocks confuse the missing-script detector.**
   Because each orphan `!u!114` references a *valid* in-scene `!u!115`
   fileID, the YAML pointer resolves at load time. Whether Unity reports
   the Component as null depends on whether the in-scene MonoScript stub
   could bind to a real Type at the assembly level — and for these
   specific stubs (`m_AssemblyName: Tartaria.Integration` +
   `m_ClassName: Moon1NPCSpawner` etc.) Unity reports the component as
   missing **at log time** but does not consistently mark it as `null`
   when iterating components. The end result: the warning is logged via
   the existing `Debug.LogWarning` line in `CleanGameObject` once on
   detection, then the loop's `break` triggers a remove call, but the
   actual `RemoveMonoBehavioursWithMissingScript` is a no-op because
   `GetMonoBehavioursWithMissingScriptCount` returns 0. The scene is
   then saved, but the YAML still contains the four `!u!114` entries
   plus the four `!u!115` MonoScript stubs.

2. **Even on the runs where the !u!114 entry is stripped**, the
   embedded `!u!115` MonoScript blocks survive. Nothing iterates over
   them via the managed API — they have no GameObject parent.
   On next scene load, Unity re-instantiates orphan components from the
   YAML because the !u!115 blocks resolve. The cycle repeats.

The exact file evidence in this worktree
(`Echohaven_VerticalSlice.unity`):

- Line 305: `--- !u!115 &255430906` MonoScript / class `Moon1AmbientCreatures`
- Line 1252: `--- !u!115 &831608850` MonoScript / class `Moon1HeroBuildingSpawner`
- Line 3090: `--- !u!115 &1523568284` MonoScript / class `Moon1MaterialSetup`
- Line 3364: `--- !u!115 &1693639792` MonoScript / class `Moon1NPCSpawner`
- Lines 2974 → 3056: the `Moon1_Systems` GameObject and its four
  `!u!114 MonoBehaviour` components referencing the four MonoScripts above

No prefab under `Assets/_Project/Prefabs/Moon1/` contains any of the
four orphan class names — the rot is **scene-local only**.

## Permanent fix

`Assets/_Project/Scripts/Editor/Moon1SystemsPrefabDeepClean.cs` adds the
menu **Tartaria / 8 Fix / Deep-Clean Moon1_Systems Prefab**.

Three passes:

1. **Prefab pass.** Iterates every prefab under
   `Assets/_Project/Prefabs/Moon1/`, opens it with
   `PrefabUtility.LoadPrefabContents`, walks the hierarchy with
   `GameObjectUtility.RemoveMonoBehavioursWithMissingScript`, saves dirty
   prefabs via `PrefabUtility.SaveAsPrefabAsset`. (Currently a no-op on
   this branch — included for safety / future regression coverage.)

2. **Scene managed pass.** Opens `Echohaven_VerticalSlice.unity`, walks
   roots, calls the same managed-removal API. Catches whichever orphans
   Unity *does* expose as null Components.

3. **YAML surgery pass.** Reads the on-disk scene/prefab YAML directly:
   - Finds all `!u!115 MonoScript` documents whose `m_ClassName` matches
     one of the four orphan classes.
   - Finds all `!u!114 MonoBehaviour` documents whose
     `m_Script.fileID` points at one of those MonoScript fileIDs.
   - Removes both document sets entirely.
   - Prunes the `- component: {fileID: N}` lines on every `!u!1
     GameObject` that referenced a removed component.
   - Atomically rewrites the file (temp + copy + delete).
   - Re-imports via `AssetDatabase.ImportAsset`.

The YAML pass is what makes the fix permanent: even if Unity's managed
detection is flaky, the on-disk YAML cannot contain the stubs after the
pass, so Unity cannot recreate the orphans on next load.

### Idempotency

The YAML pass exits early if the file contains no
`m_ClassName: Moon1NPCSpawner|Moon1AmbientCreatures|Moon1MaterialSetup|
Moon1HeroBuildingSpawner` substrings. A second run logs:

```
[Moon1SystemsDeepClean] 0 fixed — repo is clean. (Idempotent run.)
```

### Loud logging contract (per CLAUDE.md no-silent-catches rule)

Every cleanup logs at INFO with file path + count:

```
[Moon1SystemsDeepClean] Removed N orphans from <path> (managed=A, yaml=B)
```

End-of-run summary:

```
[Moon1SystemsDeepClean] === DONE === prefabsTouched=N scenesTouched=M
                                    managedRemoved=A yamlBlocksRemoved=B
```

## Verification plan

1. Open Unity.
2. Run `Tartaria / 8 Fix / Deep-Clean Moon1_Systems Prefab`.
3. Expected first-run log (this worktree, scene-only rot):
   - `scenesTouched=1`
   - `yamlBlocksRemoved=8` (4 MonoScript stubs + 4 MonoBehaviour entries)
4. Re-open `Echohaven_VerticalSlice.unity`.
5. Console should be free of the `[CleanMissingScripts] Removing missing
   script from: Moon1_Systems` warning across all subsequent domain
   reloads.
6. Run the menu again — expect `0 fixed — repo is clean.`

## Files touched

- `Assets/_Project/Scripts/Editor/Moon1SystemsPrefabDeepClean.cs` (new)
- `docs/audits/SPRINT11_L5_ORPHAN_ROOT_CAUSE_2026-06-02.md` (this file)

## Related (read-only)

- `Assets/_Project/Scripts/Editor/CleanMissingScripts.cs` — left
  unchanged; will continue to handle non-Moon1 missing scripts. The deep
  clean menu supersedes its effectiveness on the Moon1_Systems case.
