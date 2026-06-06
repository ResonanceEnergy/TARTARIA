# TICKET: HovlVFXBindings — load Hovl Magic VFX prefabs and expose by name

## Output destination
`Assets/_Project/Scripts/Integration/HovlVFXBindings.cs`

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- One C# file, one static class
- Compiles against Unity 6 LTS, assemblies `Tartaria.Core` + `Tartaria.Integration`
- All Hovl prefab paths use `AssetDatabase.LoadAssetAtPath<GameObject>` inside `#if UNITY_EDITOR`, with a `Resources.Load` fallback path for runtime builds (Resources path falls back to null if the asset isn't in a Resources folder yet — that's OK, we'll move them later)
- Provides a single static method: `public static GameObject Spawn(string slot, Vector3 position, Transform parent = null, float autoDestroyAfterSeconds = 4f)` where `slot` is a TARTARIA-flavored name (not the Hovl name)
- Internal lookup table maps slot names → Hovl prefab paths

## Spec

Hovl Studio Magic Effects pack lives at:
```
Assets/Hovl Studio/Magic effects pack/Prefabs/
  AoE effects/
  ... (and other subfolders)
```

There are 76 prefabs. We want to expose a curated 10-12 by TARTARIA-flavored slot names so other systems (BuildingRestorationCeremony, Moon1NarrativeBeats, GiantMode) can spawn VFX without knowing Hovl naming.

### Slot mapping (curated picks)

| TARTARIA slot | Hovl prefab path (relative to "Assets/Hovl Studio/Magic effects pack/Prefabs/") | Use case |
|---|---|---|
| `"restoration_burst"` | `AoE effects/Ground AOE explosion.prefab` | Building restoration moment |
| `"restoration_pillar"` | `AoE effects/Laser AOE.prefab` | 17th-hour cathedral light eruption |
| `"resonance_blue"` | `AoE effects/AoE slash blue.prefab` | Aether reading at 528 Hz |
| `"resonance_green"` | `AoE effects/AoE slash green.prefab` | Aether at 432 Hz |
| `"resonance_orange"` | `AoE effects/AoE slash orange.prefab` | Dissonance hit / mistune |
| `"crystal_idle"` | `AoE effects/Crystals crossfade.prefab` | Crystal Spire restoration idle FX |
| `"crystal_attack"` | `AoE effects/Crystals front attack.prefab` | Hostile crystal (Moon 2 dissonance) |
| `"meteor_ritual"` | `AoE effects/Meteors AOE.prefab` | Major Aether ritual / Moon 6 climax |
| `"plexus_aether"` | `AoE effects/Plexus AoE.prefab` | Ley line activation |
| `"hit_spark"` | `AoE effects/Ground AOE explosion.prefab` | Generic combat hit |
| `"giant_slam"` | `AoE effects/Meteors AOE.prefab` | Giant Mode landing slam |
| `"dissonance_pulse"` | `AoE effects/AoE slash orange.prefab` | Moon 2 dissonance crystal pulse |

If a path is wrong or the file doesn't exist, log `Debug.LogWarning($"[HovlVFXBindings] Missing: {slot} → {path}")` and return null. The CALLERS already gracefully no-op on null spawn returns.

## API to provide

```csharp
namespace Tartaria.Integration
{
    public static class HovlVFXBindings
    {
        const string HOVL_ROOT = "Assets/Hovl Studio/Magic effects pack/Prefabs/";

        static readonly System.Collections.Generic.Dictionary<string, string> _slots =
            new System.Collections.Generic.Dictionary<string, string>
            {
                { "restoration_burst",  "AoE effects/Ground AOE explosion.prefab" },
                { "restoration_pillar", "AoE effects/Laser AOE.prefab" },
                // ... (fill in the rest from the table above)
            };

        public static GameObject Spawn(string slot, Vector3 position, Transform parent = null, float autoDestroyAfterSeconds = 4f)
        {
            if (!_slots.TryGetValue(slot, out var rel))
            {
                Debug.LogWarning($"[HovlVFXBindings] Unknown slot: {slot}");
                return null;
            }
            var prefab = LoadPrefab(rel);
            if (prefab == null) return null;
            var go = Object.Instantiate(prefab, position, Quaternion.identity, parent);
            if (autoDestroyAfterSeconds > 0f) Object.Destroy(go, autoDestroyAfterSeconds);
            return go;
        }

        static GameObject LoadPrefab(string relPath)
        {
#if UNITY_EDITOR
            var full = HOVL_ROOT + relPath;
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(full);
            if (asset == null) Debug.LogWarning($"[HovlVFXBindings] Asset not found: {full}");
            return asset;
#else
            // Runtime fallback if assets are moved to a Resources folder later
            var name = System.IO.Path.GetFileNameWithoutExtension(relPath);
            return Resources.Load<GameObject>("HovlVFX/" + name);
#endif
        }
    }
}
```

## Do NOT
- Do not modify the Hovl asset folder or the prefabs.
- Do not require new asmdef references — `using UnityEditor;` is wrapped in `#if UNITY_EDITOR` so the Editor assembly isn't pulled in at runtime.
- Do not invent new Hovl prefab paths — only use the ones in the table.
- Do not add a `[MenuItem]` — this is a runtime helper, not an Editor tool.
