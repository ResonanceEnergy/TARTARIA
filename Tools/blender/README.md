# tools/blender

Headless Blender scripts that emit Unity-ready FBX into
`Assets/_Project/Models/Blender/<Moon>/`. The Unity-side
`BlenderImportPostprocessor.cs` converts the FBX materials to URP/Lit and
spawns a `.prefab` variant automatically on import.

See `CLAUDE.md` ("ART PIPELINE" mandate) for the canonical pipeline rules.

## Common conventions

Every `gen_*.py` script:

1. Imports the cross-platform helpers from `_common.py`:
   `reset_scene`, `make_material`, `export_fbx` / `export_current_as`,
   `cube`, `cyl`, `sphere`, `cone`, `torus`.
2. Calls `reset_scene()` first (clears default cube + leftover datablocks).
3. Builds geometry with bpy primitives + Boolean / Bevel modifiers only —
   no external Python deps beyond `bpy`.
4. Ends with `select_all -> join -> name -> export_current_as(name, moon)`.
5. Lets `_common.py` detect the project root — never hardcode `C:\dev\...`.

Run any script with:

```
blender --background --python tools/blender/gen_<name>.py
```

Or via the Unity Editor menu: `Tartaria -> Moon 1 -> Run Blender Batch`.

---

## Victorian Costume (`gen_victorian_costume.py`)

Parametric Victorian-era costume generator for Echohaven NPCs and named
characters. Each costume is built from real geometry — multi-piece coat
(upper torso + flared skirt + shoulder caps + lapels + buttons), vest with
watch-chain torus, tapered trousers + waistband + boots, optional top hat
(brim + crown + cap + hatband ribbon), and optional walking cane (tapered
wood shaft + brass pommel + metal ferrule).

### Parametric inputs

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `gender` | `"M"` / `"F"` / `"N"` | `"M"` | `F` uses a slightly tighter waistband and `0.98` height. `N` is neutral. |
| `palette` | `[(r,g,b), (r,g,b), (r,g,b)]` | dark grey set | `[coat, vest, trim]`. Trouser colour is derived as 65% of coat. |
| `lapel_style` | `"shawl"` / `"notched"` | `"notched"` | Shawl = single smooth curve per side. Notched = two-piece with a visible gap. |
| `has_tophat` | `bool` | `True` | Brim + crown + hatband. |
| `has_cane` | `bool` | `False` | Held at the figure's right side. |
| `height_scale` | `float` | `1.0` | Vertical scalar applied to all pieces. |
| `moon` | `str` | `"Moon1"` | Target export subdir (e.g. `"Shared"` for cross-Moon reuse). |

### Baked presets

Running the script with no environment overrides builds all four presets in
sequence. Each emits one FBX into `Assets/_Project/Models/Blender/Moon1/`.

| Preset | Filename | Coat / Vest / Trim | Lapel | Top hat | Cane |
|---|---|---|---|---|---|
| Bureau Agent | `VictorianCostume_M_BureauAgent.fbx` | black / grey / dark | notched | yes | no |
| Echohaven Villager (M) | `VictorianCostume_M_EchohavenVillager.fbx` | brown / tan / dark brown | shawl | no | no |
| Echohaven Villager (F) | `VictorianCostume_F_EchohavenVillager.fbx` | green / cream / deep green | shawl | no | no |
| Cassian formal | `VictorianCostume_M_CassianFormal.fbx` | burgundy / gold / deep burgundy | notched | yes | yes |

### Run all four presets

```
blender --background --python tools/blender/gen_victorian_costume.py
```

### Build a single bespoke costume via env vars

Setting `TARTARIA_COSTUME_NAME` activates single-shot mode and skips the
four baked presets.

| Env var | Format | Default |
|---|---|---|
| `TARTARIA_COSTUME_NAME` | string (becomes `VictorianCostume_<gender>_<name>.fbx` if it does not already start with `VictorianCostume_`) | — (required to activate override mode) |
| `TARTARIA_COSTUME_GENDER` | `M` / `F` / `N` | `M` |
| `TARTARIA_COSTUME_PALETTE` | `r,g,b;r,g,b;r,g,b` (coat ; vest ; trim) | dark grey default set |
| `TARTARIA_COSTUME_LAPEL` | `shawl` / `notched` | `notched` |
| `TARTARIA_COSTUME_TOPHAT` | `1` / `0` (also `true`/`false`/`yes`/`no`) | `1` |
| `TARTARIA_COSTUME_CANE` | `1` / `0` | `0` |
| `TARTARIA_COSTUME_HEIGHT` | float | `1.0` |

Example (PowerShell):

```powershell
$env:TARTARIA_COSTUME_NAME = "CustomDandy"
$env:TARTARIA_COSTUME_GENDER = "M"
$env:TARTARIA_COSTUME_PALETTE = "0.1,0.1,0.1;0.7,0.6,0.2;0.9,0.8,0.6"
$env:TARTARIA_COSTUME_LAPEL = "notched"
$env:TARTARIA_COSTUME_TOPHAT = "1"
$env:TARTARIA_COSTUME_CANE = "1"
blender --background --python tools\blender\gen_victorian_costume.py
```

Example (Bash):

```bash
TARTARIA_COSTUME_NAME=CustomDandy \
TARTARIA_COSTUME_GENDER=M \
TARTARIA_COSTUME_PALETTE="0.1,0.1,0.1;0.7,0.6,0.2;0.9,0.8,0.6" \
TARTARIA_COSTUME_LAPEL=notched \
TARTARIA_COSTUME_TOPHAT=1 \
TARTARIA_COSTUME_CANE=1 \
blender --background --python tools/blender/gen_victorian_costume.py
```

### Output path pattern

```
Assets/_Project/Models/Blender/<moon>/VictorianCostume_<gender>_<name>.fbx
```

With `moon` defaulting to `Moon1` for the four baked presets. The Unity
post-import step then drops a matching `.prefab` variant into
`Assets/_Project/Prefabs/Moon1/Blender/`.
