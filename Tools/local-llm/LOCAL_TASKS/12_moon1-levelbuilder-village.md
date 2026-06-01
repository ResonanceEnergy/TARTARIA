# TICKET: Moon1LevelBuilder — kill 12 primitives, wire KayKit village FBX

## Output destination
`Assets/_Project/Scripts/Integration/Moon1LevelBuilder.cs`
**REPLACES the existing file.** Currently builds 9 village buildings as `GameObject.CreatePrimitive(PrimitiveType.Cube)` stacks (12 sites).

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- Complete C# file, brace-balanced, ends on namespace close `}`
- Compiles against Unity 6 LTS, assemblies `Tartaria.Core` + `Tartaria.Integration`
- ZERO unconditional `GameObject.CreatePrimitive` calls (at most one fallback marker, marked `// URP-safe`)
- Do NOT split string literals across lines (compile error in C#)
- Public API preserved — methods called by Moon1MasterBootstrap and Editor menus

## Spec

The Moon 1 village contains 9 secondary structures arranged in a 25m radius ring around the central Echohaven plaza. Per docs/03 Days 1-5 they should look like "buried Tartarian remnants — half-sunk stone footings, broken arches, partial walls."

Available KayKit assets to compose from (path prefix `Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/`):
- `anvil.fbx`, `grindstone.fbx`, `bucket_metal.fbx` — workshop props
- Wood/stone-themed misc props

Also useful — existing Cathedral kit pieces at `Assets/_Project/Prefabs/Moon1/Cathedral/`:
- `Wall_4x4m_Stone.prefab` — partial walls
- `Column_Ornate_6.5m.prefab` — broken columns
- `Archway_4x7m.prefab` — village arches
- `Foundation_16x16m.prefab` (scaled down) — footings

For each of 9 village positions, compose a simple structure:
1. Foundation prefab scaled 0.4 (about 6m square footprint)
2. 1-2 wall fragments using `Wall_4x4m_Stone.prefab`
3. 1 broken column using `Column_Ornate_6.5m.prefab` scaled 0.7 + tilted ~15°
4. A KayKit prop (anvil/grindstone/bucket) at the base for "lived-in" feel

Village positions (angle-based around (0, 0, 0), radius 25m):
```csharp
static readonly Vector3[] VILLAGE_POSITIONS = {
    new Vector3(25.00f, 0f,   0.00f),  // E
    new Vector3(17.68f, 0f,  17.68f),  // NE
    new Vector3( 0.00f, 0f,  25.00f),  // N
    new Vector3(-17.68f,0f,  17.68f),  // NW
    new Vector3(-25.00f,0f,   0.00f),  // W
    new Vector3(-17.68f,0f, -17.68f),  // SW
    new Vector3( 0.00f, 0f, -25.00f),  // S
    new Vector3(17.68f, 0f, -17.68f),  // SE
    new Vector3( 0.00f, 0f,   0.00f),  // Center anchor (the Echohaven plaza marker)
};
```

## Helper to add

```csharp
#if UNITY_EDITOR
static GameObject LoadPrefab(string assetPath)
{
    return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
}
#else
static GameObject LoadPrefab(string assetPath) => null;
#endif

static void ApplyURPStone(GameObject go)
{
    var sh = Shader.Find("Universal Render Pipeline/Lit");
    if (sh == null) return;
    var mat = new Material(sh);
    mat.SetColor("_BaseColor", new Color(0.55f, 0.52f, 0.50f));
    foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
}
```

## Method to add (or replace existing village-building logic)

```csharp
public void BuildVillage(Transform parent)
{
    var foundationPrefab = LoadPrefab("Assets/_Project/Prefabs/Moon1/Cathedral/Foundation_16x16m.prefab");
    var wallPrefab       = LoadPrefab("Assets/_Project/Prefabs/Moon1/Cathedral/Wall_4x4m_Stone.prefab");
    var columnPrefab     = LoadPrefab("Assets/_Project/Prefabs/Moon1/Cathedral/Column_Ornate_6.5m.prefab");

    for (int i = 0; i < VILLAGE_POSITIONS.Length; i++)
    {
        var center = VILLAGE_POSITIONS[i];
        var root = new GameObject("Moon1_Village_" + i);
        root.transform.SetParent(parent, false);
        root.transform.position = center;

        if (foundationPrefab != null)
        {
            var f = Instantiate(foundationPrefab, root.transform);
            f.transform.localPosition = Vector3.zero;
            f.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            ApplyURPStone(f);
        }
        // ... two wall fragments offset to the back + side, broken column tilted, prop at the base
        // Use the existing methods if your file has helpers, otherwise inline
    }
}
```

## Do NOT
- Do not split string literals across lines.
- Do not modify any other file.
- Do not import or reference `Tartaria.AI` (asmdef cycle).
- Do not create over 10 children per village instance (perf budget — Moon 1 frame budget is 16.67ms).
- Do not delete any Cathedral kit prefab.
