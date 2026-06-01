# TICKET: MudGolemAI prefab refactor — kill 6 primitive stubs

## Output destination
`Assets/_Project/Scripts/AI/MudGolemAI.cs`
**REPLACES the existing file.**

## Acceptance criteria
- Namespace: `Tartaria.AI`
- One C# file, brace-balanced, ends on namespace close `}`
- Compiles against Unity 6 LTS, asmdef `Tartaria.AI` (which references `Tartaria.Core` + `Tartaria.Gameplay` — do NOT add Integration ref)
- Zero `GameObject.CreatePrimitive` calls in the final file (currently 6)
- Public API preserved (the existing class has methods other systems call — keep their signatures)
- URP material conventions when a runtime fallback IS needed

## Spec

The existing `MudGolemAI.cs` builds the Mud Golem's body from 6 primitive cubes/spheres on Awake. Replace with **`MudGolem.prefab`** instantiation. The prefab already exists at `Assets/_Project/Prefabs/Characters/MudGolem.prefab`.

Behavior change:
- On `Awake()`, if `transform.childCount == 0` (i.e., no visual provided), `Resources.Load<GameObject>("MudGolem")` OR `AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/MudGolem.prefab")` (Editor-only fallback) and `Instantiate` it as a child of `transform`
- If the prefab load fails, fall back to a SINGLE primitive marker (a brown sphere) at local position (0, 0.6, 0) with URP/Lit material `_BaseColor = (0.32, 0.22, 0.14)`. Log a warning explaining the fallback.

The rest of the AI behavior (state machine, FindWithTag("Player"), navigation, attacks) stays identical — only the visual generation changes.

## Public API to preserve

Whatever public methods exist on the current class — keep them. The most likely public surface based on naming conventions:

```csharp
public float MaxHealth { get; }
public float CurrentHealth { get; }
public bool IsAlive { get; }
public void TakeDamage(float damage, GameObject instigator = null);
public void Die(GameObject instigator = null);
```

If those don't exist exactly, match what's currently in the file.

## Reference excerpt — Editor prefab load with runtime fallback

```csharp
static GameObject LoadMudGolemPrefab()
{
#if UNITY_EDITOR
    var p = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/MudGolem.prefab");
    if (p != null) return p;
#endif
    return Resources.Load<GameObject>("MudGolem");
}
```

## Reference excerpt — URP fallback marker (only if prefab load fails)

```csharp
void EnsureFallbackVisual()
{
    var urpLit = Shader.Find("Universal Render Pipeline/Lit");
    var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    marker.name = "FallbackVisual_MudGolem";
    marker.transform.SetParent(transform);
    marker.transform.localPosition = new Vector3(0f, 0.6f, 0f);
    marker.transform.localScale = Vector3.one * 0.9f;
    Destroy(marker.GetComponent<Collider>());
    if (urpLit != null)
    {
        var mat = new Material(urpLit);
        mat.SetColor("_BaseColor", new Color(0.32f, 0.22f, 0.14f));
        marker.GetComponent<Renderer>().sharedMaterial = mat;
    }
    Debug.LogWarning("[MudGolemAI] Prefab load failed — using fallback sphere marker. Check Assets/_Project/Prefabs/Characters/MudGolem.prefab");
}
```

## Do NOT
- Do not modify `MudGolemHealth.cs` or any other file.
- Do not delete the `MudGolem.prefab` asset.
- Do not add `using Tartaria.Integration;` — asmdef cycle.
- Do not change AI behavior, only the visual setup.
- Do not regenerate animations or rigs.
