# TICKET: KayKit Skeleton wireup — Giant Skeleton Key + skeleton-hum prophecy

## Output destinations

This ticket touches TWO methods inside one file:
`Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs`
**REPLACES the existing file** (it's the smallest of the two narrative-beat methods affected).

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- Compiles, brace-balanced, ends on namespace close `}`
- Reduces `GameObject.CreatePrimitive` count from 3 to AT MOST 1 (one fallback marker allowed if the FBX load fails)
- All existing public + private members preserved (the file has `Moon1NarrativeBeats` MonoBehaviour AND `GiantSkeletonKeyPickup` class — keep both)
- All existing behavior preserved: skeleton-hum prophecy, cathedral light eruption, key pickup +RS via `GameLoopController.Instance?.AwardRS`

## Spec

The current file builds the Giant Skeleton Key out of 3 stretched primitives (KeyShaft cube, KeyTeeth cube, KeyBow sphere). Replace with a KayKit FBX from the Skeleton pack — pick a hand bone or jaw bone to suggest "a fragment of the giant's actual skeleton you found buried in the mud."

Files to look at in `Assets/KayKit_Skeletons_1.1_FREE/KayKit_Skeletons_1.1_FREE/Assets/fbx/`:
- Pick the first .fbx file alphabetically that contains "skull" OR "bone" OR "hand" in its filename, if any
- Otherwise use the first .fbx in the folder

The `SpawnGiantKey(Vector3 worldPos)` method should:
1. Try `AssetDatabase.LoadAssetAtPath<GameObject>` on the chosen path (Editor-only)
2. If found, `Instantiate` it as a child of the key GameObject, scale to `(2.5, 2.5, 2.5)` for "giant" feel, apply URP gold material `_BaseColor = (1.0, 0.86, 0.30)` with `_EMISSION` keyword + emission `BaseColor * 1.2f`
3. If NOT found, fall back to ONE marker primitive (`PrimitiveType.Cube`) with `// URP-safe` comment, URP gold material applied. Log warning explaining missing FBX.

For the **skeleton-hum prophecy** at the cathedral position (-30, 1.5, 30), the current code spawns just an audio-only hum. ADD a visual: a single KayKit skeleton skull FBX (or first FBX in folder) at that position, scaled (4, 4, 4) so it's massive, half-buried below ground (Y = -1.5f world), with a soft URP gold emission to suggest "the giant skeleton is here, beneath you." When the prophecy fade finishes (at end of `SkeletonHumProphecyRoutine`), destroy the visual along with the audio.

## Constants to add at class level

```csharp
const string SKELETON_FBX_DIR = "Assets/KayKit_Skeletons_1.1_FREE/KayKit_Skeletons_1.1_FREE/Assets/fbx";
```

## Helper to add to the class

```csharp
static GameObject LoadSkeletonFbx(string preferredKeyword = null)
{
#if UNITY_EDITOR
    if (!System.IO.Directory.Exists(SKELETON_FBX_DIR)) return null;
    var files = System.IO.Directory.GetFiles(SKELETON_FBX_DIR, "*.fbx");
    string pick = null;
    if (preferredKeyword != null)
        pick = System.Array.Find(files, f => System.IO.Path.GetFileName(f).ToLowerInvariant().Contains(preferredKeyword.ToLowerInvariant()));
    if (pick == null && files.Length > 0) pick = files[0];
    if (pick == null) return null;
    var assetPath = pick.Replace("\\", "/");
    return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
#else
    return null;
#endif
}
```

## Where to call

In `SpawnGiantKey(Vector3 worldPos)` (around the current primitive creation):

```csharp
var fbx = LoadSkeletonFbx("hand"); // or "bone" — try a few keywords
if (fbx != null)
{
    var bone = Instantiate(fbx, _giantKeyGO.transform);
    bone.transform.localPosition = Vector3.zero;
    bone.transform.localScale = Vector3.one * 2.5f;
    ApplyGoldURP(bone);
}
else
{
    // Fallback marker (URP-safe)
    var marker = GameObject.CreatePrimitive(PrimitiveType.Cube); // URP-safe
    Destroy(marker.GetComponent<Collider>());
    marker.transform.SetParent(_giantKeyGO.transform);
    marker.transform.localScale = new Vector3(0.18f, 0.18f, 1.6f);
    ApplyGoldURP(marker);
    Debug.LogWarning("[Moon1NarrativeBeats] Skeleton FBX missing — using fallback cube for giant key");
}
```

And in `SkeletonHumProphecyRoutine()` after the audio spawn:

```csharp
// Visual: half-buried giant skeleton fragment
GameObject skull = null;
var skullFbx = LoadSkeletonFbx("skull");
if (skullFbx != null)
{
    skull = Instantiate(skullFbx);
    skull.transform.position = new Vector3(-30f, -1.5f, 30f);
    skull.transform.localScale = Vector3.one * 4f;
    skull.name = "SkeletonHum_VisualFragment";
    ApplyGoldURP(skull, 0.9f);
}

// ... existing audio + banner code ...

// Cleanup at end (after existing audio fade loop):
if (skull != null) Object.Destroy(skull);
```

## Helper

```csharp
static void ApplyGoldURP(GameObject go, float emissionMul = 1.2f)
{
    var urpLit = Shader.Find("Universal Render Pipeline/Lit");
    if (urpLit == null) return;
    var mat = new Material(urpLit);
    mat.SetColor("_BaseColor", new Color(1f, 0.86f, 0.30f));
    mat.EnableKeyword("_EMISSION");
    mat.SetColor("_EmissionColor", new Color(0.95f, 0.78f, 0.20f) * emissionMul);
    foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
}
```

## Do NOT
- Do not modify `GiantSkeletonKeyPickup` class.
- Do not change `CathedralLightEruption` IEnumerator.
- Do not remove the audio hum — it's the lore beat.
- Do not change the key's BoxCollider trigger setup or the PlayerPrefs counter logic.
