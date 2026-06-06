# TICKET: Cathedral Kit wire-up — replace Moon1HeroBuildingSpawner primitives with prefabs

## Output destination
`Assets/_Project/Scripts/Integration/Moon1HeroBuildingSpawner.cs`
**THIS REPLACES THE EXISTING FILE.** Output a complete .cs file that, when copied to that path, replaces what's there. The current file uses `GameObject.CreatePrimitive(...)` 12 times — that's the scandal we're fixing.

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- Complete C# file, brace-balanced, ends on namespace close `}`
- Compiles against Unity 6 LTS, assemblies `Tartaria.Core` + `Tartaria.Integration`
- URP material conventions (`SetColor("_BaseColor", c)`) — but if you only Instantiate prefabs and don't touch their materials, you don't need to touch shaders at all
- ZERO `GameObject.CreatePrimitive` calls in this file. Use `Resources.Load<GameObject>` instead.
- Public API preserved: methods called by other systems must still exist

## Spec

The Moon 1 hero buildings (Cathedral / Dome, Pure Water Fountain, Crystal Spire) currently build themselves from `PrimitiveType.Cube` stacks at runtime. Replace with prefab instantiation using the 18 already-authored Cathedral kit prefabs in `Assets/_Project/Prefabs/Moon1/Cathedral/`.

Available prefabs (load via `Resources.Load<GameObject>` — note: these prefabs need to be in a `Resources/` subfolder OR loaded via AssetDatabase from `Assets/_Project/Prefabs/Moon1/Cathedral/`. **Use the editor-only `AssetDatabase.LoadAssetAtPath<GameObject>` path AT LOAD TIME from a `[RuntimeInitializeOnLoadMethod]` hook**, since these aren't in a Resources folder. If `#if UNITY_EDITOR` is needed, wrap appropriately):

```
Foundation_16x16m.prefab
Wall_4x4m_Stone.prefab
Wall_Corner_4x4m.prefab
Archway_4x7m.prefab
Column_Ornate_6.5m.prefab
Door_Grand_3x6m.prefab
RoseWindow_4x4m.prefab
Dome_Segment_N.prefab   (and NE/E/SE/S/SW/W/NW — 8 total)
Spire_Base_2x2m.prefab
Spire_Mid_Taper.prefab
Spire_Top_MercuryBall.prefab
```

Path prefix: `Assets/_Project/Prefabs/Moon1/Cathedral/`

### Hero building #1 — Listeners' Hall (Dome) — position (-30, 0, 30)

Compose a cathedral structure:
- 1 × Foundation_16x16m at base
- 4 corners using Wall_Corner_4x4m
- 4 walls using Wall_4x4m_Stone (between corners on each side)
- 1 × Door_Grand_3x6m at south face center
- 1 × RoseWindow_4x4m at north face center top
- 4 × Column_Ornate_6.5m at the inner corners
- 8 × Dome_Segment_{N,NE,E,SE,S,SW,W,NW} arranged in a circle on top
- The whole structure is buried 60% per docs/15 §7 (offset Y by -3.5m at start), and `RaiseBuildingOnRestoration` (already in InteractableBuilding.cs) will lift it back up.

### Hero building #2 — Pure Water Fountain — position (0, 0, -30)

Smaller composition:
- 1 × Foundation_16x16m scaled (0.5, 0.3, 0.5)
- 4 × Column_Ornate_6.5m at corners of the smaller foundation, scaled (0.4, 0.6, 0.4)
- An empty GameObject named "WaterFontMount" at center top (so BuildingRestorationCeremony can attach the water particle there)

### Hero building #3 — Crystal Spire — position (60, 0, 40)

Tall composition:
- 1 × Foundation_16x16m scaled (0.4, 0.4, 0.4)
- 1 × Spire_Base_2x2m at center
- 1 × Spire_Mid_Taper above the base (Y offset = base height)
- 1 × Spire_Top_MercuryBall on top of mid taper

## Public API to preserve (existing callers)

The class likely already has these methods (you'll need to keep them):

```csharp
public void SpawnHeroBuildings()          // master entry point
public GameObject SpawnDome(Vector3 pos)
public GameObject SpawnFountain(Vector3 pos)
public GameObject SpawnSpire(Vector3 pos)
```

If you change the signatures, also document the change in a header comment.

## How to load the prefabs (use AssetDatabase since they're not in Resources/)

```csharp
#if UNITY_EDITOR
using UnityEditor;
#endif

static GameObject LoadCathedralPrefab(string fileName)
{
#if UNITY_EDITOR
    var path = $"Assets/_Project/Prefabs/Moon1/Cathedral/{fileName}";
    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
    // Runtime fallback: Resources.Load if anyone moves these into a Resources folder
    return Resources.Load<GameObject>($"Moon1/Cathedral/{fileName.Replace(".prefab","")}");
#endif
}
```

If `LoadCathedralPrefab` returns null, log a warning and create a small primitive marker (NOT a full building — just a 1m cube at the position so the dev sees something is missing, with a clear `Debug.LogWarning($"[Moon1HeroBuildingSpawner] Missing prefab: {fileName} — using marker cube")`). This warning IS allowed since it's a fallback, not the primary path.

## What success looks like

After this ticket lands:
- `grep -c "GameObject.CreatePrimitive" Assets/_Project/Scripts/Integration/Moon1HeroBuildingSpawner.cs` → returns at most 1 (the marker fallback)
- Open Echohaven scene in Unity, run `Tartaria → Build Out Moon 1 Buildings`, see actual stone cathedral kit composed in scene instead of brown cubes
- The three hero buildings are still buried 60% and rise on restoration

## Do NOT
- Do not modify other files. This is a single-file replacement.
- Do not delete or move the 18 Cathedral .prefab files.
- Do not invent new prefab names — only use the 18 listed above.
- Do not require new Resources folder setup or asmdef changes.
- Do not change the building IDs (`echohaven_dome`, `echohaven_fountain`, `echohaven_spire`) — InteractableBuilding instances rely on them.
