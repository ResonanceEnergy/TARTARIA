# NPC Pipeline Notes — Sprint 8 Lane 8

> Added 2026-06-02 — upgrades the primitive capsule stand-ins for Lirael,
> Anastasia, and Cassian to Blender-generated low-poly humanoid stand-ins.
> Per `docs/audits/MOON1_ACCEPTANCE_2026-06-02.md`, these three NPC prefabs
> were previously primitive-built (CapsuleCollider + Rigidbody + MeshRenderer
> on ~10.8 KB files). This lane delivers humanoid silhouettes — NOT fully
> rigged Mecanim, but a real torso + head + arms + legs + accessory pass that
> reads correctly at gameplay distance.

## Scope

These are **stand-in humanoid meshes**, not rigged characters. The next pass
(post-Sprint 8) is to bring them through a Mixamo / Mecanim rigging step or
hand-author armatures. For now they slot into the existing prefab system so
Echohaven stops shipping with three blue/green/red capsules where the speaking
NPCs should be.

## Files added

| File | Purpose |
|---|---|
| `tools/blender/gen_npc_lirael.py`    | Echo Guardian — sky-blue robe, silver hair, 432 Hz collar sigil glow |
| `tools/blender/gen_npc_anastasia.py` | Herb-keeper — forest-green dress, dark brown hair, basket strap |
| `tools/blender/gen_npc_cassian.py`   | Antagonist — charcoal robe, jet-black hair, ember-red pauldron |

`tools/blender/run_all_moon1.py` updated to include the three new generators
in the Tier 2 character-roster batch.

## Canonical export names + resource paths

Each script joins all primitives into a single mesh and exports via
`export_current_as(NAME, "Moon1")` so the FBX path is deterministic.

| NPC | FBX path | Auto-generated prefab path (via BlenderImportPostprocessor) |
|---|---|---|
| Lirael    | `Assets/_Project/Models/Blender/Moon1/Lirael.fbx`    | `Assets/_Project/Prefabs/Moon1/Blender/Lirael.prefab` |
| Anastasia | `Assets/_Project/Models/Blender/Moon1/Anastasia.fbx` | `Assets/_Project/Prefabs/Moon1/Blender/Anastasia.prefab` |
| Cassian   | `Assets/_Project/Models/Blender/Moon1/Cassian.fbx`   | `Assets/_Project/Prefabs/Moon1/Blender/Cassian.prefab` |

`Assets/_Project/Scripts/Editor/BlenderImportPostprocessor.cs` handles the
URP/Lit material remap + prefab-variant creation on FBX import — no manual
.prefab authoring required.

## Palette decisions (rgba — alpha implicit 1.0)

### Lirael (Echo Guardian, ally)
- Robe:        `(0.55, 0.75, 0.92)` — sky blue
- Robe trim:   `(0.30, 0.50, 0.78)` — darker indigo
- Skin:        `(0.94, 0.88, 0.83)` — pale
- Hair:        `(0.86, 0.88, 0.92)` — silver-white
- Iris:        `(0.20, 0.55, 0.78)` — cyan
- Boot:        `(0.18, 0.18, 0.22)` — near-black leather
- Sigil glow:  `(0.40, 0.85, 1.00)` — 432 Hz harmonic emission, strength 2.4

### Anastasia (Herb-keeper, ally)
> Per `CLAUDE.md` political-risk callout: Anastasia is **the herb-keeper**,
> NOT a Romanov princess. No imperial crown, no fleur-de-lis, no Russian
> Orthodox motifs. Generic peasant/village garb only.

- Dress:       `(0.20, 0.42, 0.22)` — forest green
- Dress trim:  `(0.55, 0.45, 0.25)` — ochre woven band
- Apron:       `(0.78, 0.70, 0.55)` — cream
- Skin:        `(0.93, 0.78, 0.65)` — warm
- Hair:        `(0.22, 0.14, 0.08)` — dark brown (peasant bun)
- Iris:        `(0.30, 0.20, 0.10)` — warm brown
- Boot:        `(0.28, 0.20, 0.12)` — leather brown
- Strap:       `(0.40, 0.28, 0.16)` — basket strap
- Basket:      `(0.65, 0.48, 0.28)` — woven willow

### Cassian (Antagonist)
- Robe:        `(0.14, 0.14, 0.16)` — charcoal
- Robe trim:   `(0.08, 0.08, 0.10)` — near-black
- Shoulder:    `(0.55, 0.12, 0.08)` — ember red (pauldron + clasp)
- Skin:        `(0.88, 0.82, 0.78)` — pale, slightly ashen
- Hair:        `(0.05, 0.04, 0.05)` — jet black
- Iris:        `(0.50, 0.15, 0.10)` — ember-tinged, faint emission (strength 0.8)
- Boot:        `(0.10, 0.08, 0.08)` — black leather
- Ember glow:  `(0.85, 0.22, 0.08)` — pauldron emission, strength 1.2

## How to regenerate

From Unity: `Tartaria → Moon 1 → Run Blender Batch (Generate All Moon 1 Assets)`
(uses Blender 5.0 headlessly on Windows).

From shell:
```
blender --background --python tools/blender/run_all_moon1.py
```

Or one-at-a-time:
```
blender --background --python tools/blender/gen_npc_lirael.py
blender --background --python tools/blender/gen_npc_anastasia.py
blender --background --python tools/blender/gen_npc_cassian.py
```

## Compliance with CLAUDE.md art-pipeline rules

1. Each script `import _common` and calls `reset_scene()` first.
2. Materials via `make_material(name, base_color, roughness, metallic, emission, emission_strength)`.
3. Each script ends with `select_all → join → rename → export_fbx`.
4. No external dependencies — pure `bpy` primitives + helpers from `_common.py`.
5. Cross-platform paths via `_common.PROJECT_ROOT` detection.
6. Canonical asset names (`Lirael`, `Anastasia`, `Cassian`) — not the older
   `LiraelGuardian` / `AnastasiaPrincess` / `CassianCarter` variants from
   `gen_characters_humanoid.py`, which remain in the repo as separate
   block-figure stand-ins.

## Follow-up work (not in this lane)

- Replace the primitive-built `Lirael.prefab` / `Anastasia.prefab` /
  `Cassian.prefab` references in scene/quest scripts to point at the new
  `Prefabs/Moon1/Blender/*.prefab` variants.
- Author armatures + Mecanim humanoid avatar configs so the NPCs can play
  the existing idle/talk/walk animation clips.
- Replace eye spheres with face decals so the camera doesn't need to push
  in close to read facial direction.
