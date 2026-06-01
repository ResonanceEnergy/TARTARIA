# TICKET: Moon2CrystallineCavernBuilder — generate Moon 2 cave + 7 dissonance crystals

## Output destination
`Assets/_Project/Scripts/Editor/Moon2BuildOutCavern.cs`

## Acceptance criteria
- Namespace: `Tartaria.Editor`
- Editor-only assembly (uses `[MenuItem]` + `using UnityEditor;`)
- Compiles against Unity 6 LTS
- Adds menu: `Tartaria/Moon 2/Build Out Crystalline Cavern`
- Does NOT load any scene file — works on whatever scene is currently active
- Idempotent: skips creating things that already exist (look for `Moon2_Cavern_Root` parent)

## Spec

Per `docs/03` Moon 2 (Lunar/Crystalline Caverns):
- 30m × 30m cavern area west of Echohaven (centered at world `(-80, 0, 0)`)
- 7 dissonance crystals scattered through the cavern
- "Pure water font" landmark at cavern center (just a marker GameObject — actual visual is a separate ticket)
- Crystalline ceiling — 12 hanging stalactite primitives (URP-safe cones/cubes scaled tall)
- Entry portal — an Archway (use Cathedral kit) at `(-50, 0, 0)` (between Echohaven and the cavern)

Menu item logic:

```csharp
[MenuItem("Tartaria/Moon 2/Build Out Crystalline Cavern")]
public static void Run()
{
    var existing = GameObject.Find("Moon2_Cavern_Root");
    if (existing != null)
    {
        if (!EditorUtility.DisplayDialog("Cavern Exists",
            "Moon2_Cavern_Root already in scene. Rebuild from scratch?",
            "Rebuild", "Cancel")) return;
        Object.DestroyImmediate(existing);
    }

    var root = new GameObject("Moon2_Cavern_Root");
    root.transform.position = new Vector3(-80f, 0f, 0f);

    BuildEntryPortal(root);     // 1 Cathedral Archway prefab + 2 columns
    BuildCavernFloor(root);     // a flat plane primitive, dark stone color
    BuildStalactites(root, 12); // 12 hanging spikes from a ceiling at Y=12
    Build7Crystals(root);       // 7 DissonanceCrystal MonoBehaviours scattered

    // Mark dirty so the Editor saves
    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
        UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    Debug.Log("[Moon2BuildOutCavern] Built Moon 2 cavern at (-80, 0, 0)");
}
```

### Crystal positions (relative to root at -80,0,0)

7 positions inside a 30×30 area:
```csharp
static readonly Vector3[] CRYSTAL_LOCAL_POS = {
    new Vector3(  6f, 0f,  4f),
    new Vector3( -8f, 0f,  9f),
    new Vector3(  2f, 0f, -11f),
    new Vector3(-13f, 0f, -3f),
    new Vector3( 11f, 0f, -6f),
    new Vector3( -5f, 0f, 13f),
    new Vector3( 10f, 0f, 10f),
};
```

For each: create a child GameObject, set position, attach `Tartaria.Gameplay.DissonanceCrystal` component (just `AddComponent<DissonanceCrystal>()` — the component's own Awake builds the visual).

### Stalactites

12 hanging spikes from random points within the 30×30 area at `localPosition.y = 12f` (top of cavern), each `PrimitiveType.Cube` scaled `(0.5, 4f + Random.Range(-1f, 1.5f), 0.5f)`, URP material `_BaseColor = (0.35, 0.42, 0.55)` (cold cave stone). Mark `// URP-safe` since they're decorative.

### Entry portal

At `(-50, 0, 0)` world (which is `(30f, 0f, 0f)` relative to root):
- 1 Archway from `Assets/_Project/Prefabs/Moon1/Cathedral/Archway_4x7m.prefab`
- 2 Column_Ornate prefabs flanking it
- All in the Moon2 root

## Reference excerpt — load Cathedral kit (Editor-only path)

```csharp
static GameObject LoadCathedral(string fileName)
{
    var path = "Assets/_Project/Prefabs/Moon1/Cathedral/" + fileName;
    return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
}
```

## Reference excerpt — add DissonanceCrystal component

```csharp
var crystal = new GameObject("Moon2_DissonanceCrystal_" + i);
crystal.transform.SetParent(root.transform, false);
crystal.transform.localPosition = CRYSTAL_LOCAL_POS[i];
crystal.AddComponent<Tartaria.Gameplay.DissonanceCrystal>();
```

## Do NOT
- Do not load `Moons/Moon2.unity` — work on active scene.
- Do not modify `DissonanceCrystal.cs` (separate ticket).
- Do not invent new Cathedral kit prefab names.
- Do not modify Moon1*.cs files.
- Do not create over 50 GameObjects total in this build (stalactites + crystals + portal = ~22, well under).
