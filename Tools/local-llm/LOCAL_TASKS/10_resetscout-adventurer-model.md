# TICKET: ResetScout proper visual — use KayKit Adventurer instead of capsule + cube hat

## Output destination
`Assets/_Project/Scripts/AI/ResetScout.cs`
**REPLACES the existing file.**

## Acceptance criteria
- Namespace: `Tartaria.AI`
- Compiles, brace-balanced
- Zero `GameObject.CreatePrimitive` calls in the final file (currently 3 — body capsule, top hat cube, clipboard cube)
- All current public API + the `TakeDamage(float, GameObject)`, `Die`, `MaxHealth`, `CurrentHealth`, `IsAlive` surface preserved
- All current behavior preserved: aggro at `aggroRange`, attack at `attackRange`, `PerformAttack` deals damage via `SendMessage("TakeDamage", ...)`, dies → `GameEvents.FireRSChange(rsReward)` + banner + Destroy(gameObject, 1.2f)

## Spec

Replace the runtime-built body with the **`Char_Rogue_Hooded.prefab`** from the KayKit Adventurers pack, located at `Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue_Hooded.prefab`. The hood gives a "shadowy bureaucrat" vibe that fits the Reset Scout's docs/03 Days 13-18 description ("Victorian-costumed goons with clipboards and jackhammers").

Tint the loaded prefab with a deep maroon/black accent on Awake by walking child Renderers and adjusting `_BaseColor` to `(0.18, 0.10, 0.12)` for the body and leaving everything else alone.

Add a small "clipboard accent" — a thin red cube at chest position. THIS one primitive IS allowed (a small prop, not the whole character), but it MUST have a URP material applied per the URP convention. Mark the source line with `// URP-safe` so the magenta audit script approves.

Add a "tall hat" — load a single KayKit RPGToolsBits FBX named `bucket_metal.fbx` as the hat. Stretch it (0.6, 0.9, 0.6 scale) and place at head height (~2.1m local). This stays clearly weird (a Victorian bureaucrat with a bucket on his head) — that's the design.

## Loading pattern

```csharp
static GameObject LoadKayKitPrefab(string assetPath)
{
#if UNITY_EDITOR
    return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
#else
    return null; // Runtime fallback handled below
#endif
}

void EnsureVisual()
{
    if (transform.childCount > 0) return;

    var prefab = LoadKayKitPrefab("Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue_Hooded.prefab");
    if (prefab != null)
    {
        var body = Instantiate(prefab, transform);
        body.name = "Body";
        body.transform.localPosition = Vector3.zero;
        TintMaroon(body);
    }
    else
    {
        // Fallback ONLY if prefab is missing — single primitive marker
        // (this is the bureaucrat that lost their model)
        var marker = GameObject.CreatePrimitive(PrimitiveType.Capsule); // URP-safe
        ApplyURP(marker, new Color(0.18f, 0.10f, 0.12f));
        marker.name = "FallbackBody";
        marker.transform.SetParent(transform);
        marker.transform.localPosition = new Vector3(0f, 1f, 0f);
        Destroy(marker.GetComponent<Collider>());
        Debug.LogWarning("[ResetScout] Char_Rogue_Hooded prefab missing — using fallback capsule");
    }

    AddBucketHat();
    AddClipboardAccent();
}

void TintMaroon(GameObject body)
{
    var c = new Color(0.18f, 0.10f, 0.12f);
    var urpLit = Shader.Find("Universal Render Pipeline/Lit");
    if (urpLit == null) return;
    foreach (var r in body.GetComponentsInChildren<Renderer>())
    {
        var mat = new Material(urpLit);
        mat.SetColor("_BaseColor", c);
        r.sharedMaterial = mat;
    }
}

void AddBucketHat()
{
    var fbx = LoadKayKitPrefab("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/bucket_metal.fbx");
    if (fbx == null) return;
    var hat = Instantiate(fbx, transform);
    hat.name = "BucketHat";
    hat.transform.localPosition = new Vector3(0f, 2.1f, 0f);
    hat.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
    ApplyURP(hat, new Color(0.18f, 0.18f, 0.20f));
}

void AddClipboardAccent()
{
    var clipboard = GameObject.CreatePrimitive(PrimitiveType.Cube); // URP-safe — accent only
    clipboard.name = "Clipboard";
    clipboard.transform.SetParent(transform);
    clipboard.transform.localPosition = new Vector3(0.35f, 1.1f, 0.3f);
    clipboard.transform.localRotation = Quaternion.Euler(0f, 25f, 8f);
    clipboard.transform.localScale = new Vector3(0.32f, 0.42f, 0.05f);
    Destroy(clipboard.GetComponent<Collider>());
    ApplyURP(clipboard, new Color(0.65f, 0.18f, 0.18f));
}

static void ApplyURP(GameObject go, Color c)
{
    var urpLit = Shader.Find("Universal Render Pipeline/Lit");
    if (urpLit == null) return;
    var mat = new Material(urpLit);
    mat.SetColor("_BaseColor", c);
    foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
}
```

## Do NOT
- Do not modify Char_Rogue_Hooded.prefab or any KayKit FBX.
- Do not change combat logic — only visual setup.
- Do not delete the Clipboard or BucketHat primitives (those are intentional accents).
- Do not add `using Tartaria.Integration;`.
- Do not regenerate animations.
