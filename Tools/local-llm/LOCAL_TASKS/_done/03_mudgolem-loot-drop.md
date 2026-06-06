# TICKET: MudGolemLootDrop — RS chunks + clay shards on golem death

## Output destination
`Assets/_Project/Scripts/AI/MudGolemLootDrop.cs`

## Acceptance criteria
- Namespace: `Tartaria.AI`
- One C# file, one class, brace-balanced, ends on namespace close `}`
- Compiles against Unity 6 LTS, assembly `Tartaria.AI` (already references `Tartaria.Core`, `Tartaria.Gameplay`)
- DO NOT add `using Tartaria.Integration;` — AI cannot reference Integration (asmdef one-way)
- Uses URP material conventions: `mat.SetColor("_BaseColor", c)`, NEVER `mat.color = c`
- Any `GameObject.CreatePrimitive` MUST be followed by `Shader.Find("Universal Render Pipeline/Lit")` fallback that assigns `_BaseColor` to ALL renderers in the children

## Spec

A MonoBehaviour to be attached to MudGolem GameObjects (the existing `Tartaria.AI.MudGolemHealth` component will instantiate this via `AddComponent<MudGolemLootDrop>()` from its existing `OnDeath` callback — DO NOT modify MudGolemHealth.cs in this ticket; just provide the loot drop component).

On `void OnEnable()`, cache `transform.position`. The expectation is this component lives on the same GameObject as MudGolemHealth, and gets disabled or destroyed before the body despawns.

Expose a `public void DropLoot(GameObject killer = null)` method that:
1. Spawns 2-4 "clay shards" (small primitives) at `transform.position + Vector3.up * 0.5f` with slight random scatter
2. Each shard is a `PrimitiveType.Cube` scaled `(0.25, 0.18, 0.25)` rotated random Y, with a wet-mud URP material (BaseColor `(0.32, 0.22, 0.14)`, no emission)
3. Each shard has `Rigidbody` (mass 0.4) and a small upward + outward AddForce velocity (`new Vector3(rand(-2,2), rand(2,4), rand(-2,2))`)
4. Each shard has a `SphereCollider` trigger radius 0.4f
5. Each shard auto-destroys after 8 seconds
6. Also spawn ONE "RS coin" primitive (`PrimitiveType.Sphere`, scale 0.3, gold URP `(1.0, 0.86, 0.30)` with `_EMISSION` keyword on, `_EmissionColor = BaseColor * 1.4f`). The coin has `Rigidbody` mass 0.3, a slow rotation `transform.Rotate(0, 60*Time.deltaTime, 0)` (use a nested helper class for the rotator), and a `SphereCollider` trigger radius 0.6f.
7. When the RS coin trigger is touched by `other.CompareTag("Player")`, fire `GameEvents.FireRSChange(8f)` (this method already exists on `Tartaria.Core.GameEvents`), play sound via `ServiceLocator.Audio?.PlaySFX("RSCollect", transform.position, 0.7f)` if Audio exists, then `Destroy(rsCoin)`.

The shards are just visual flavor — they don't grant anything, they just thunk to the ground.

## Required nested types

```csharp
// nested in Tartaria.AI namespace, separate class in same file:
public class _LootShard : MonoBehaviour { /* no-op; just for tagging the shard GO */ }
public class _LootRSCoin : MonoBehaviour
{
    void Update() { transform.Rotate(0f, 60f * Time.deltaTime, 0f, Space.World); }
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Tartaria.Core.GameEvents.FireRSChange(8f);
        Destroy(gameObject);
    }
}
```

## Reference excerpt — URP material pattern (REQUIRED — DO NOT use mat.color)

```csharp
var urpLit = Shader.Find("Universal Render Pipeline/Lit");
if (urpLit != null)
{
    var mat = new Material(urpLit);
    mat.SetColor("_BaseColor", new Color(1f, 0.86f, 0.30f));
    mat.EnableKeyword("_EMISSION");
    mat.SetColor("_EmissionColor", new Color(0.95f, 0.78f, 0.20f) * 1.4f);
    foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
}
```

## Reference excerpt — GameEvents.FireRSChange (already exists)

```csharp
// In Tartaria.Core.GameEvents:
public static event Action<float> OnResonanceChanged;
public static void FireRSChange(float delta) => OnResonanceChanged?.Invoke(delta);
```

## Do NOT
- Do not modify `MudGolemHealth.cs` or `MudGolemAI.cs` — they are not in scope for this ticket.
- Do not reference `Tartaria.Integration.*` (asmdef cycle).
- Do not use `mat.color = ...` — use `mat.SetColor("_BaseColor", ...)`.
- Do not write Editor menu items.
- Do not destroy the parent GameObject — only destroy the shards/coin you spawn.
- Do not pool — these are throwaway spawns, just `Destroy(go, 8f)`.
