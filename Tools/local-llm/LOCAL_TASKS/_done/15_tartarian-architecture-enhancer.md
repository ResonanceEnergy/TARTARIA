# TICKET: TartarianArchitectureEnhancer — kill 10 primitives, use Cathedral kit

## Output destination
`Assets/_Project/Scripts/Integration/TartarianArchitectureEnhancer.cs`
**REPLACES the existing file.** Currently uses 10 `GameObject.CreatePrimitive` calls for architectural details (capitals, friezes, finials, etc.).

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- Brace-balanced, ends on namespace close `}`
- Compiles against Unity 6 LTS, assemblies `Tartaria.Core` + `Tartaria.Integration`
- AT MOST 2 primitive fallbacks total, each marked `// URP-safe`
- Do NOT split string literals across lines
- Public API preserved

## Spec

This component sweeps existing buildings (Hero buildings + village) and adds Tartarian-style detail flourishes: ornamental capitals on top of columns, decorative friezes along walls, finials on spires, rose-window-style rings on dome segments.

Currently it builds those detail meshes from primitive cubes/spheres. Replace with `Resources.Load`/`AssetDatabase.LoadAssetAtPath` calls into the Cathedral kit:

| Detail role | Cathedral kit prefab |
|---|---|
| Capital (top of column) | `Spire_Top_MercuryBall.prefab` scaled 0.25 |
| Frieze (above walls) | `Archway_4x7m.prefab` scaled (1, 0.15, 1) — flattened |
| Finial (atop spires) | `Spire_Top_MercuryBall.prefab` scaled 0.4 |
| Rose ring (around dome openings) | `RoseWindow_4x4m.prefab` scaled 0.5 |
| Buttress (corner reinforcement) | `Column_Ornate_6.5m.prefab` scaled (0.4, 1.0, 0.4) + 8° tilt outward |

Path prefix: `Assets/_Project/Prefabs/Moon1/Cathedral/`

If a needed kit prefab is missing at runtime, log a warning and use ONE primitive fallback (cube or sphere depending on role) with URP material applied. Mark each such fallback with `// URP-safe`.

Public API to preserve (likely existing methods):

```csharp
public void EnhanceBuilding(GameObject building, string buildingId);  // adds details to one building
public void EnhanceAll();  // sweeps all InteractableBuilding instances in scene
```

If `EnhanceAll()` doesn't exist in the current file, add it. It should:
1. `FindObjectsOfType<InteractableBuilding>()`
2. For each, call `EnhanceBuilding(b.gameObject, b.BuildingId)`
3. Log count enhanced

## Sample wiring

```csharp
#if UNITY_EDITOR
static GameObject LoadKit(string fileName)
{
    return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
        "Assets/_Project/Prefabs/Moon1/Cathedral/" + fileName);
}
#else
static GameObject LoadKit(string fileName) => null;
#endif

void AddCapital(GameObject parent, Vector3 localPos)
{
    var prefab = LoadKit("Spire_Top_MercuryBall.prefab");
    GameObject capital;
    if (prefab != null)
    {
        capital = Instantiate(prefab, parent.transform);
    }
    else
    {
        capital = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
        capital.transform.SetParent(parent.transform);
        ApplyURPStone(capital);
        Debug.LogWarning("[ArchEnhancer] Spire_Top_MercuryBall.prefab missing — fallback sphere capital");
    }
    capital.transform.localPosition = localPos;
    capital.transform.localScale = Vector3.one * 0.25f;
}
```

## Do NOT
- Do not modify any other file.
- Do not import `Tartaria.AI`.
- Do not exceed 8 detail prefabs per building (perf).
- Do not call `EnhanceAll()` from this class's Awake — Moon1MasterBootstrap will trigger it explicitly.
