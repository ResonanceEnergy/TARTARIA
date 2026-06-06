# TICKET: Moon1ExcavationSites — wire KayKit RPGToolsBits as dig site props

## Output destination
`Assets/_Project/Scripts/Integration/Moon1ExcavationSites.cs`
**REPLACES the existing file.** Currently uses 2 `GameObject.CreatePrimitive` calls for placeholder dig piles.

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- Brace-balanced, ends on namespace close `}`
- Compiles against Unity 6 LTS, assemblies `Tartaria.Core` + `Tartaria.Integration`
- AT MOST 1 primitive fallback per excavation site, marked `// URP-safe`
- Do NOT split string literals across lines
- Public API preserved

## Spec

Per docs/03 Days 6-12 (Moon 1 Restoration phase): "The player digs at four excavation sites scattered around Echohaven, uncovering tools, blueprints, and lore fragments left by the Tartarian builders."

Available KayKit RPGToolsBits FBX files (path: `Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/`):
- `anvil.fbx`, `grindstone.fbx` — heavy stone tools (gold for "ancient workshop" vibe)
- `bucket_metal.fbx`, `chisel.fbx`, `file.fbx`, `axe.fbx` — hand tools
- `blueprint.fbx`, `blueprint_stacked.fbx`, `drafting_compass.fbx`, `compass_base.fbx` — Tartarian architect props

Four excavation sites at fixed positions:

| Site | Position | Theme | KayKit props (3-5 per site) |
|---|---|---|---|
| 1 — Workshop | (-12, 0, 8) | "blacksmith's pit" | anvil + grindstone + bucket_metal |
| 2 — Architect's table | (8, 0, 12) | "drafting station" | blueprint + drafting_compass + compass_base + blueprint_stacked |
| 3 — Tool cache | (15, 0, -8) | "buried tool box" | chisel + file + axe + bucket_metal |
| 4 — Ceremonial | (-10, 0, -14) | "altar / offering" | anvil (tilted) + blueprint + chisel |

For each site:
- Empty parent GameObject named `Excavation_<n>_<theme>` at the position
- A flat dirt circle (a Cylinder primitive, scaled `(2.5, 0.1, 2.5)`, URP-safe material with `_BaseColor = (0.30, 0.22, 0.14)` for muddy brown) — this IS allowed as it's the dig pit base, not a building stub
- Instantiate the 3-5 KayKit FBX as children, scattered within a 1.5m radius, random Y-rotation, some slightly tilted/buried (Y between -0.1 and 0.3)
- A `SphereCollider` trigger radius 4m on the parent for the "approach prompt"
- Add a `GameEvents.RaiseExcavationSiteEntered` call when player enters (skip if that event doesn't exist on GameEvents — graceful no-op)

## Helper

```csharp
#if UNITY_EDITOR
static GameObject LoadFbx(string assetPath) =>
    UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
#else
static GameObject LoadFbx(string assetPath) => null;
#endif

const string TOOLS_DIR = "Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/";

void SpawnProp(Transform parent, string fbxName, Vector3 localPos, float yRot, float tilt = 0f)
{
    var fbx = LoadFbx(TOOLS_DIR + fbxName);
    if (fbx == null) return;
    var p = Instantiate(fbx, parent);
    p.transform.localPosition = localPos;
    p.transform.localRotation = Quaternion.Euler(tilt, yRot, 0f);
}
```

## Do NOT
- Do not modify any other Moon1*.cs file.
- Do not import `Tartaria.AI` (asmdef cycle).
- Do not exceed 6 child GameObjects per site (perf budget).
- Do not generate sites at runtime if `Moon1ExcavationSites` instance already has children (idempotent on re-bootstrap).
