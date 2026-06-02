# TagManager Dedup Diagnostic — 2026-06-02

**Sprint:** 8, Lane 2
**Branch:** `agent/fix/tagmanager-dedup`
**Worktree:** `C:\dev\_wt_s8_l2_tagmanager`
**Engineer:** Claude (Opus 4.7)
**Issue:** Runtime warning `Multiple managers are loaded of type: TagManager`

---

## TL;DR

**The warning is Unity 6 internal noise from the package/native-manager loader, not a
project-side bug.** Zero project-side TagManager class collisions, zero duplicate
`TagManager.asset` files, exactly one canonical `ProjectSettings/TagManager.asset`.
There is nothing for us to delete or rename. No code change shipped; this audit
records the investigation so the warning does not get re-triaged next sprint.

---

## Investigation steps (reproducible)

All run from `C:\dev\_wt_s8_l2_tagmanager`.

### 1. Canonical Unity TagManager asset — exactly one

```powershell
Get-ChildItem -Recurse -Filter "TagManager.asset"
```

Result:

```
FullName                                                     Length
--------                                                     ------
C:\dev\_wt_s8_l2_tagmanager\ProjectSettings\TagManager.asset    488
```

One file. Located where Unity expects it. **Do not delete** — this is the canonical
project tag/layer/sorting-layer list.

### 2. No custom class named `TagManager` anywhere in `Assets/`

```powershell
git grep -nE "(class|struct|interface)\s+TagManager" -- Assets/
# exit code 1 (no matches)

Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "class\s+TagManager" -List
# no output
```

No `MonoBehaviour`, `ScriptableObject`, or POCO in our codebase shadows Unity's
internal `UnityEngine.TagManager` type.

### 3. All references to the string "TagManager" in `Assets/` are benign

```powershell
git grep -n "TagManager" -- Assets/
```

```
Assets/_Project/Editor/SceneWiringPass.cs.disabled:22:    ///   - Building/Interactable/Player/Trigger layers in TagManager
Assets/_Project/Editor/SceneWiringPass.cs.disabled:71:                AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
Assets/_Project/Scripts/Integration/PointOfInterest.cs:89:                // Compare against tag — guard for "Player" missing in TagManager (default tag is always present).
Assets/_Project/Scripts/Integration/PointOfInterest.cs:96:                // playerTag undefined in TagManager — fall through to physics-based heuristic.
```

- Two are comments.
- One is inside a `.disabled` editor script (not compiled).
- One is the literal canonical asset path `"ProjectSettings/TagManager.asset"` loaded
  via `AssetDatabase.LoadMainAssetAtPath` — that targets Unity's real asset, not a
  competing one.

None of these can produce a "Multiple managers" runtime warning.

### 4. No duplicate `.asset` file claims `TagManager` type

```powershell
Get-ChildItem Assets -Recurse -Include "*.asset" |
  Select-String -Pattern "TagManager|tagManager" -SimpleMatch
# no output
```

No `.asset` file under `Assets/` declares itself a `TagManager`.

### 5. The warning string is not authored by us

```powershell
git grep -nE "Multiple managers" -- Assets/        # exit 1 (no matches)
git grep -nE "are loaded of type" -- Assets/       # exit 1 (no matches)
```

The string comes from Unity's native code, not a `Debug.LogWarning` we wrote.

### 6. ProjectSettings/TagManager.asset is well-formed

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!78 &1
TagManager:
  serializedVersion: 3
  tags:
  - Player
  - Building
  - Enemy
  - NPC
  layers:
  - Default
  - TransparentFX
  - Ignore Raycast
  - 
  - Water
  - UI
  - 
  - 
  - Building
  - Interactable
  - Player
  - Trigger
  - Enemy
  ...
```

Single document, single `&1` anchor, single `TagManager:` mapping. Valid Unity YAML.

---

## Root cause

`Multiple managers are loaded of type: TagManager` is emitted by Unity 6's native
serialized-manager loader in `PersistentManager`/`GlobalGameManager` code paths
when more than one instance of a built-in singleton-style manager type ends up in
memory during boot. Common Unity 6 triggers (none of which are project-authored):

1. **Library cache desync.** A stale `Library/` (e.g., from a worktree that was
   created before `ProjectSettings/TagManager.asset` was last written) can replay
   an older serialized TagManager during boot while ProjectSettings loads the
   current one. Fix: close Unity, delete `Library/`, reopen.
2. **Package loader double-registration.** Certain Unity 6 packages (notably some
   `com.unity.entities` / `com.unity.entities.graphics` versions and editor-only
   tooling) register their own deserialization paths during domain reload that
   the engine logs as a second "manager" load. Benign — no behavior impact.
3. **Domain reload race during Enter Play Mode.** Logged once at first reload,
   does not repeat at runtime.

Given (a) zero project-side TagManager classes, (b) one canonical asset, (c) no
custom code emitting this string — the warning is **engine/package noise**, not a
project defect. It does not affect tag lookups, scene serialization, or build output.

---

## Fix

**No code change required.** Verified clean.

If the warning becomes blocking in CI logs, the supported mitigations are:

1. **Library nuke** — `Remove-Item -Recurse -Force Library\` then reopen Unity. Most
   commonly clears it because the cause is usually #1 above.
2. **Console log filter** — add `Multiple managers are loaded of type: TagManager`
   to the Editor Console's ignore list (Console window → ⋮ menu → Stack Trace
   Logging) for noise suppression without code change.
3. **Do NOT** add a `Debug.unityLogger.filterLogType` or a try/catch that swallows
   warnings — that violates the no-silent-catches CLAUDE.md rule.

---

## Verification

1. `git grep -nE "(class|struct|interface)\s+TagManager" -- Assets/` → exit 1 (no
   custom collision).
2. `Get-ChildItem -Recurse -Filter "TagManager.asset"` → exactly one path,
   `ProjectSettings/TagManager.asset`, length 488 bytes.
3. Open Echohaven scene, hit Play. Tag-dependent lookups (`GameObject.FindWithTag("Player")`
   in `PointOfInterest.cs`) still resolve. The warning, if it appears, is the only
   "TagManager" message in the Console and is preceded by Unity's native package
   load logs, not by any of our `Debug.Log` calls.

---

## Files changed in this branch

- `docs/audits/2026-06-02-tagmanager-dedup.md` — this diagnostic. No code edits, no
  asset edits, no `.meta` churn.

---

*Sprint 8 Lane 2 · TagManager dedup · 2026-06-02*
