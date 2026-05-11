# Player Mesh Drop-Zone

Drop a **female humanoid Mixamo FBX** here and the build pipeline will
auto-configure it as Humanoid, generate an Avatar, and replace the capsule
placeholder on `Assets/_Project/Prefabs/Characters/Player.prefab` with one
`SkinnedMeshRenderer` (closing §8 items 5 + 8).

## Filename convention

Any `.fbx` works, but for unambiguous selection name it:

```
Player_Mesh.fbx
```

If multiple FBX files are present, the binder prefers (in order):
1. `Player_Mesh.fbx`
2. Any filename containing `Female`, `Eve`, `Kachujin`, `Liam`, `Elara`
3. The first `.fbx` alphabetically

## Recommended free female Mixamo bases

- **Eve By J.Gonzales** — slim 20s frame, fits Elara
- **Kachujin Re Sculpt** — battle-ready, slightly heavier
- **Liam** — neutral young adult
- **Mremireh O Desbiens** — sci-fi explorer (good fallback)

Download flow:
1. mixamo.com → search → select character → **Download**
2. Format: **FBX Binary**, Pose: **T-Pose**, no animation
3. Save as `Player_Mesh.fbx` in this folder
4. Run `tartaria-play.ps1 -BatchOnly` — Phase 9j2 auto-binds.

## Routing

Phase 9j2 (`HumanoidAutoBinder.BindIfAvailable`) runs after Phase 9j (Capoeira
animation integration). It is silently skipped when this folder is empty, so
the build stays green pre-asset-acquisition.

The binder is idempotent — re-running it just updates the prefab to match
whichever FBX is currently present.
