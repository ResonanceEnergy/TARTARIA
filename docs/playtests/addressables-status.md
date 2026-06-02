# Addressables Status — Moon 1 Hero Content

> Template populated by Cowork after running the Editor-window work described in
> `docs/HANDOFFS.md` (Systems -> Tools, 2026-06-01). Audit pass: hit Play, watch
> the Console for `[AddressableContentLoader] LoadHero(...)` lines, fill the
> table below from the live log output, then commit.

Log routes (canonical, emitted by
`Assets/_Project/Scripts/Core/Addressables/AddressableContentLoader.cs`):

| Console line | Meaning |
|---|---|
| `Debug.Log` &nbsp; `... resolved via Addressables group` | Addressables key hit. Group is wired. |
| `Debug.LogWarning` &nbsp; `... fell back to Resources.Load --- Addressables group entry missing for this id` | Resources path resolved; the Moon1Content group is missing an entry for this id. |
| `Debug.LogError` &nbsp; `... returned null --- id not registered anywhere` | Hard miss --- prefab not in Addressables and not under `Resources/Moon1/Heroes/`. Spawner will null-deref. |

---

## Addressables Group Status

| HeroId | Group Resolved (Y/N) | Fallback Used (Y/N) | Notes |
|---|---|---|---|
| Cathedral | | | |
| Fountain | | | |
| Spire | | | |
| StarDome | | | |

Legend:
- **Group Resolved** = saw the `resolved via Addressables group` log line for that id.
- **Fallback Used** = saw the `fell back to Resources.Load` warning for that id.
- A row with both N means the hard-miss error fired (or the id was never requested this session --- note which).

---

## Next steps for Cowork

Per `docs/HANDOFFS.md` -> *2026-06-01 --- Systems -> Tools (Cowork / Editor)*:

1. Open **Window > Asset Management > Addressables > Groups**.
2. Click **Create > Group > Packed Assets** and rename the new group to **`Moon1Content`**.
3. Drag each Moon 1 hero prefab from `Assets/_Project/Prefabs/Moon1/**` into the
   `Moon1Content` group, then set the entry **Address** field to the bare hero id
   (`Cathedral`, `Fountain`, `Spire`, `StarDome`) so
   `AddressableContentLoader.LoadHero(id)` resolves it directly --- no folder
   prefix, no `.prefab` suffix.
4. Click **Build > New Build > Default Build Script** to bake the content catalog.

After step 4, hit Play in the Echohaven scene, drive the spawner that calls
`LoadHero(...)` for each id, and fill in the table above from the Console output.
Expected end-state: every row reads **Group Resolved = Y**, **Fallback Used = N**.

The `.asset` files Cowork touches live under `Assets/AddressableAssetsData/` and
are Editor/GUID-sensitive --- they must be authored from the Addressables window,
not scripted, per the HANDOFFS.md entry.
