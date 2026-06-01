# TICKET: Moon3MercurialLakeBuilder — Editor menu to scaffold Moon 3 zone

## Output destination
`Assets/_Project/Scripts/Editor/Moon3BuildOutMercurialLake.cs`

## Acceptance criteria
- Namespace: `Tartaria.Editor`
- Editor-only (uses `[MenuItem]` + `using UnityEditor;`)
- Compiles against Unity 6 LTS
- Adds menu: `Tartaria/Moon 3/Build Out Mercurial Lake`
- Idempotent: skip if `Moon3_MercurialLake_Root` already exists in scene (with dialog confirmation to rebuild)

## Spec

Per `docs/03_CAMPAIGN_13_MOONS.md` Moon 3 (Lunar / Mercurial Lakes):
"Three reflective pools of liquid Aether, ringed by inverted columns. The player wades through to chase the Orphan Train fragments. Pools double as time-anchored Resonance amplifiers — stand in one and time slows."

Build zone at world position `(-160, 0, 0)` (further west of Moon 2 cavern):

### Three Mercurial Lakes

Three flat circular pools, mirror-finish material (high smoothness + emissive):
- Pool A — center `(-160, 0, 0)`, radius 8m, mercury color `(0.7, 0.75, 0.78)` with `_Metallic = 1.0` and `_Smoothness = 0.98`
- Pool B — center `(-180, 0, 12)`, radius 6m, same material
- Pool C — center `(-145, 0, -10)`, radius 7m, same material

Each pool is a `PrimitiveType.Cylinder` scaled flat — `Vector3(radius, 0.05, radius)`. Mark `// URP-safe`.

### Inverted columns

For each pool, generate 6 columns ringing it (60° apart), each scaled `Vector3(0.6, 4, 0.6)` and INVERTED in Y (negative scale.y) so they appear to grow downward from sky into the pool. Cathedral kit prefab if available: `Column_Ornate_6.5m.prefab`. Apply URP `_BaseColor = (0.45, 0.42, 0.40)` (weathered stone).

### Orphan-train track segments

Add 5 broken train-track segments scattered around the lakes — `PrimitiveType.Cube` scaled `Vector3(0.4, 0.2, 6)` rotated random Y. Dark iron color `(0.18, 0.18, 0.22)`. Mark `// URP-safe`. These are the "fragments" the player chases.

### Entry portal

At world `(-130, 0, 0)`: an Archway from Cathedral kit prefab, scaled `1.2x` to feel grander.

### Menu skeleton

```csharp
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    public static class Moon3BuildOutMercurialLake
    {
        const string ROOT_NAME = "Moon3_MercurialLake_Root";

        [MenuItem("Tartaria/Moon 3/Build Out Mercurial Lake")]
        public static void Run()
        {
            var existing = GameObject.Find(ROOT_NAME);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Mercurial Lake Exists",
                    "Moon3_MercurialLake_Root already in scene. Rebuild from scratch?",
                    "Rebuild", "Cancel")) return;
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(ROOT_NAME);
            root.transform.position = new Vector3(-160f, 0f, 0f);

            BuildLake(root, new Vector3(  0f, 0f,   0f), 8f);
            BuildLake(root, new Vector3(-20f, 0f,  12f), 6f);
            BuildLake(root, new Vector3( 15f, 0f, -10f), 7f);

            BuildTrackFragments(root, 5);
            BuildEntryPortal(root, new Vector3(30f, 0f, 0f));

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("[Moon3BuildOutMercurialLake] Built Moon 3 zone at " + root.transform.position);
        }

        static void BuildLake(GameObject parent, Vector3 localPos, float radius) { /* ... */ }
        static void BuildTrackFragments(GameObject parent, int count) { /* ... */ }
        static void BuildEntryPortal(GameObject parent, Vector3 localPos) { /* ... */ }
    }
}
```

## Reference excerpt — load Cathedral kit (Editor-only)

```csharp
static GameObject LoadKit(string fileName)
{
    var path = "Assets/_Project/Prefabs/Moon1/Cathedral/" + fileName;
    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
}
```

## Reference excerpt — URP material

```csharp
static void ApplyURP(GameObject go, Color baseColor, float metallic = 0f, float smoothness = 0.5f, Color? emission = null)
{
    var sh = Shader.Find("Universal Render Pipeline/Lit");
    if (sh == null) return;
    var mat = new Material(sh);
    mat.SetColor("_BaseColor", baseColor);
    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
    if (emission.HasValue)
    {
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emission.Value);
    }
    foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
}
```

## Do NOT
- Don't load any scene file — work on active scene.
- Don't add Moon3 components other than this Editor builder.
- Don't invent new Cathedral kit prefab names.
- Don't split string literals across lines.
- Don't use `mat.color = ...`.
