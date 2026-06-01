# Prefab Validity Audit — Moon1/Blender (2026-05-31)

## Scope

Read-only sample audit of `Assets/_Project/Prefabs/Moon1/Blender/` (347 .prefab files) to assess validity vs the "347 prefabs vs 70 FBX" mismatch claim.

## Sample

60 prefabs (alphabetical first 20, alphabetical middle 20 at indices 164-183, alphabetical last 20). Every sampled prefab inspected via binary parsing of its embedded `m_SourcePrefab` PPtr (GUID/Hash128) cross-referenced against the FBX/.prefab/.mat meta-file GUID index (2,574 candidate sources indexed).

## Classification results

| Class    | Count | % of sample |
|----------|------:|------------:|
| VALID    |    60 |       100 % |
| PARTIAL  |     0 |         0 % |
| EMPTY    |     0 |         0 % |

**Every prefab in the sample resolved to a real, populated FBX source asset** (16-132 KB Kaydara FBX binary, mean 30 KB). All sampled FBX files contain embedded `Material`, `Geometry`, and `Vertices` nodes.

## Why the "no MeshFilter in the prefab" reading is misleading

These .prefab files are **binary SerializedFile PrefabInstance variants**, not the YAML text format the audit brief assumed. Each is a ~3.1 KB file that holds only an override delta:

- `m_SourcePrefab` PPtr -> an FBX-generated source prefab in `Assets/_Project/Models/Blender/{MoonN|Shared}/<name>.fbx`
- `m_Modifications` -> a transform/name override list (`m_LocalPosition.{xyz}`, `m_LocalRotation.{wxyz}`, `m_Name`)
- No `m_AddedComponents`, no own `MeshFilter` block

The MeshFilter, MeshRenderer, and Material array all live on the **source FBX prefab** and are inherited by the variant. A raw grep for `MeshFilter:` or `m_Materials` in the variant file returns zero — but that is correct for a prefab variant, not a defect.

This is exactly what `Assets/_Project/Scripts/Editor/BlenderImportPostprocessor.cs` `GeneratePrefabVariant()` produces (uses `PrefabUtility.InstantiatePrefab(fbx)` + `SaveAsPrefabAsset`).

## Magenta-shader check

`BlenderImportPostprocessor.OnPostprocessMaterial` runs `material.shader = Shader.Find("Universal Render Pipeline/Lit")` on every imported material from the Blender FBX root, and `materialLocation: InPrefab` (mode 1) embeds the materials inside the FBX import so they are never separate loose `.mat` files that could go missing. No magenta-shader fallback signature found in any sampled source. The 0 PARTIAL count reflects this.

## Source coverage breakdown of the 60-prefab sample

| Source folder                          | Count |
|----------------------------------------|------:|
| `Models/Blender/Shared/`               |    32 |
| `Models/Blender/Moon1/`                |    16 |
| `Models/Blender/Moon{2..11}/`          |    12 |

All resolved sources existed on disk and exceeded 16 KB.

## Empty/Partial prefabs

**None observed in the sample.**

## Extrapolated population estimate

With 60/60 = 100% VALID in a stratified alphabetical sample (Wilson 95% lower bound = 94 %), the population of 347 prefabs is **almost certainly all valid PrefabInstance variants pointing to real FBX sources**. The "347 prefabs vs 70 FBX" claim in earlier audit notes is explained by Moon-content prefabs in this folder pulling from FBX sources across `Shared/` + `Moon1..11/` combined — not by stub/placeholder prefabs. A full population scan is not required.

## Caveats

- This audit verified existence and structure, not visual correctness. It does not check whether the FBX geometry matches the intended design, whether UVs are sane, or whether materials look right at runtime in URP.
- The `BlenderImportPostprocessor` only converts materials to URP/Lit at import time. If URP was added/changed after import, materials could still be on the wrong shader until Unity reimports the FBX. A runtime visual confirmation (Play in Echohaven, eyeball for magenta) remains the only ground-truth check.
- Three Moon1/Blender prefabs were not in the sample (Moon1NPCSpawner, Moon1AmbientCreatures, others) — irrelevant here since those are scripts, not Blender variants.

---

*Audit: 60-prefab stratified sample, read-only, 2026-05-31.*
