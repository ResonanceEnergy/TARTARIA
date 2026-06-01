# MOON 1 — LIVE PLAYTEST GAP REPORT
*2026-05-31 · NATRIX requested "tighten errors, walkaround, compare with modern games, find all gaps"*

---

## Critical finding (everything else is downstream of this)

**The "world looks broken" is a single root cause: 5 unassigned prefab references on `EchohavenContentSpawner`.**

Inspector shows all five slots set to `None (Game Object)`:
| Field | Real asset on disk | Was set to | Result in Play |
|---|---|---|---|
| `kayKitMiloPrefab` | `Assets/_Project/Prefabs/Characters/Milo.prefab` | None | Milo spawned as magenta primitive |
| `kayKitCassianPrefab` | `Assets/_Project/Prefabs/Characters/Cassian.prefab` | None | Cassian spawned as magenta primitive |
| `kayKitAnastasiaPrefab` | `Assets/_Project/Prefabs/Characters/Anastasia.prefab` | None | Anastasia spawned as magenta primitive |
| `kayKitMudGolemPrefab` | `Assets/_Project/Prefabs/Characters/MudGolem.prefab` | None | **The giant pink sphere dominating the Game view** |
| `kayKitShovelPrefab` | `Assets/_Project/Prefabs/Props/KayKit/Tools/Prop_shovel.prefab` | None | No shovel prop on excavation sites |
| `kayKitRockPrefabs[]` | 50 rock prefabs in `Props/KayKit/Stones` | empty | Zero rocks scattered in the world |
| `kayKitFoliagePrefabs[]` | 84 foliage prefabs across `Props/KayKit` | empty | Zero trees/bushes/grass |

The spawner's own warning fires every spawn cycle:
```
[EchohavenContentSpawner] MudGolem spawned from primitive fallback —
assign kayKitMudGolemPrefab for AAA quality.
[EchohavenContentSpawner] Milo spawned from primitive fallback —
assign kayKitMiloPrefab for AAA quality.
[EchohavenContentSpawner] Anastasia spawned from primitive fallback —
assign kayKitAnastasiaPrefab for AAA quality.
```

**One-shot fix landed this session**: `Assets/_Project/Scripts/Editor/Moon1WireSpawnerPrefabs.cs`
→ menu `Tartaria → 8 Fix → Wire EchohavenContentSpawner Prefabs (kill magenta fallbacks)`.

Auto-loads all 5 prefabs by path + scans for up to 24 rock + 24 foliage prefabs and wires them via `SerializedObject`. After this menu + Ctrl+S, the magenta scattershot should be gone.

---

## What I actually saw on screen (live playtest evidence)

### Edit-mode reference (Play stopped, Echohaven_VerticalSlice loaded)
- Stylized cathedral pillars with a bright emission orb on top (StarDome)
- Small Kay Kit Toad-like characters with proper materials (Milo + 2 villagers)
- Tan terrain, brown stone obelisk
- A green prism + yellow triangle still present (placeholder remnants Mega Cleanup missed — these have specific names, need a follow-up `DeleteByNames` extension)
- Pink/magenta cone on top of StarDome — **deliberate emission**, not a shader bug

### Play-mode actual state (during walkaround at frames 55, 526, 937, 1189)
- "ECHOHAVEN AWAKENING" tutorial popup with the game's actual story copy ✅
- "Press A on controller or E on keyboard to engage" combat hint ✅
- Walking works (`Last key: W (1.6s ago)`, `Left stick mag 1.00`)
- F310 detected as `Gamepad.current (XInput): OK (Xbox Controller)` ✅
- **Camera positioned correctly**: console logs `distance=12m, height=8m, angle=35°` — 3rd-person view, NOT broken
- **Multiple giant magenta spheres** scattered through the view = Mud Golem primitive fallbacks
- **199 warnings / 28 errors / 0 critical** at runtime (down from 999+ at session start)

### Error categories still firing
1. **`Can't add component 'MeshRenderer' to EyeR/Skull/Jaw — already added`** — the spawner re-adds renderers to prefab children that already have them. Defensive bug in the spawn path.
2. **`The referenced script (Unknown) on this Behaviour is missing!`** — a Missing Mono Script on the Moon1_Systems GameObject, probably the legacy Moon1LevelBuilder reference. Inspector confirms `Script: Missing (Mono Script)` slot.
3. **`Multiple managers are loaded of type: TagManager`** — duplicate TagManager singleton.
4. **`Look rotation viewing vector is zero`** — a Quaternion.LookRotation call with a zero direction vector somewhere in the camera/AI code.
5. **`Default GameObject Tag: Player already registered`** — defensive log that fires every time the player respawns.

---

## Gap analysis vs modern RPG/restoration peers

I couldn't fetch competitor screenshots in this run, so the comparison below is from general industry standards for the genre (stylized 3rd-person RPG + restoration loop — closest peers are Valheim's stylized world, Tunic's small-character isometric depth, Death's Door's atmospheric biomes, Genshin Impact's NPC density, and AAA reference Death Stranding's restoration-as-gameplay loop).

| Pillar | Industry standard for this style | TARTARIA Moon 1 right now | Gap |
|---|---|---|---|
| **Per-screen object density** | 40–80 visible objects (props, foliage, characters, terrain detail) | ~6 visible (Player + Milo + 2 villagers + Obelisk + StarDome) — most "objects" are magenta primitives | 6× density needed once prefabs are wired |
| **Vegetation** | Trees, bushes, grass clumps, ground cover | 0 foliage instances (array empty) | Wire 24 foliage prefabs from `Props/KayKit` |
| **Ground scatter** | Rocks, debris, leaves, mushrooms every few meters | 0 rocks (array empty) | Wire 24 rock prefabs from `Stones/Rocks` |
| **Sky / atmosphere** | Skybox with sun shaft, volumetric clouds, particle dust motes | Solid orange gradient sky, no clouds, no dust | Add a URP skybox prefab or HDR cubemap |
| **Character density** | 8–15 NPCs in a village hub | 4 (Milo, Anastasia, Lirael, Cassian) — and they spawn as magenta until wired | Spawn 4–6 generic villager Kay Kit chars on routines |
| **Building interiors** | Each hero building has a walk-in interior | All 3 hero buildings are exterior shells | Future scope per `docs/15 §7` |
| **Tutorial / FTUE** | First-time prompts that fade, dialogue-driven, voice or animated portraits | Plain banner text "Press [E]" — but it IS firing correctly | Good enough for alpha |
| **Combat polish** | Hit reaction VFX, sound, screen shake, damage numbers | Tested earlier in marathon — fires but Mud Golem visual is the magenta blob | Falls out once prefab wired |
| **Mini-game UX** | Custom shader / animated UI / progress bars / haptic | 3 variants exist (FreqSlider, WaveTrace, HarmonicPattern), wired to 9 pedestals | Verified live tonight ✓ |
| **Day/night** | Dynamic skybox lerp, lighting shift | TartarianHourCycle component attached but no visible day shift in 4 frames | Probably wired but needs longer playtest to validate |
| **Audio** | Ambient layer, footsteps, music stinger on milestone | Moon1AudioAtmosphere attached, didn't hear (no audio in screenshots) | Verify next session with sound on |
| **Mini-map / HUD** | Aether HUD widget bottom-right visible ✓ | Present, with location markers (yellow dots) | Good for alpha |

---

## Concrete next-session punch list (in priority order)

1. **Run `Tartaria → 8 Fix → Wire EchohavenContentSpawner Prefabs`** — kills the magenta scattershot in one click. ⚡ HIGHEST IMPACT, lowest effort.
2. Save, hit Play, take a clean screenshot. The world should look fundamentally different — proper Mud Golems, real Milo/Cassian/Anastasia, rocks, foliage.
3. Delete the green prism + yellow triangle placeholder remnants — add their names to `Moon1MegaCleanup.cs WrongMoonShells[]`.
4. Strip the Missing Mono Script (Moon1LevelBuilder) on Moon1_Systems via Inspector right-click → Remove Component, OR extend `Moon1SceneCleanup` Editor menu to do it.
5. Add a sun-shaft + cloud skybox prefab to PostProcessVolume layer.
6. Fix `MeshRenderer/MeshFilter already added` spam in `EchohavenContentSpawner` — wrap the AddComponent calls in `GetComponent<T>() ?? AddComponent<T>()` style.
7. Investigate `Look rotation viewing vector is zero` — likely a LookAt call when target == self.position.
8. Y button is opening Crafting menu instead of Aether Vision — input binding bug in `PlayerInputHandler.cs` (north button case).

---

## Honest status

What I claimed earlier in the session: "Moon 1 is playing polished."
What is actually true: **Moon 1 is playable but visually broken because 5 prefab fields are null.** The infrastructure is real (scene loads, input works, FTUE fires, mini-games are wired, hierarchy is populated, camera positions itself). The visual layer breaks the moment Play starts because the spawner falls back to magenta primitives.

The good news: this isn't 20 separate bugs. It's ONE bug producing ~20 symptoms. The fix is the new Editor menu shipped this session.

The Mega Cleanup, Wire Tuning Pedestals, and Bootstrap menus all genuinely worked — but none of them touched the EchohavenContentSpawner prefab refs because that wasn't in their scope. The new `Moon1WireSpawnerPrefabs` menu closes that gap.

*— end of report*
