# KayKit Character Wiring - Manual Guide

## Status
**Created:** 6 new character prefabs (Thorne, Lirael, Korath, Cassian, ShadowStalker, CrystalSentry)  
**Next:** Wire all 9 characters to KayKit models in Unity Editor

## Character → Model Mapping

| Character | KayKit Model | Scale | Type |
|-----------|--------------|-------|------|
| Milo | Char_Ranger | 1.0x | Companion (Scout) |
| Thorne | Char_Barbarian | 1.0x | Companion (Fleet Captain) |
| Lirael | Char_Mage | 1.0x | Companion (Crystal Singer) |
| Korath | Char_Barbarian | 2.0x | Companion (Stone Giant) |
| Cassian | Char_Rogue_Hooded | 1.0x | Companion (Archivist) |
| Anastasia | Char_Mage | 1.0x | Companion |
| MudGolem | Skeletons/Char_Skeleton_Warrior | 1.0x | Enemy |
| ShadowStalker | Skeletons/Char_Skeleton_Rogue | 1.0x | Enemy |
| CrystalSentry | Skeletons/Char_Skeleton_Minion | 1.0x | Enemy |

## Manual Wiring Steps (Unity Editor)

### Option 1: Use the Editor Tool (Recommended)
1. Open Unity Editor
2. Menu: **Tartaria → Character Wiring → Wire All Characters to KayKit Models**
3. Click **"Wire All Characters"** button
4. Wait for processing (should take ~10 seconds)
5. Check results in the tool window

### Option 2: Manual Prefab Editing
For each character prefab in `Assets/_Project/Prefabs/Characters/`:

1. **Open the prefab** in Prefab editing mode (double-click)

2. **Remove old visual mesh children**
   - Delete any child GameObjects that only have MeshFilter/MeshRenderer
   - Keep any children with scripts

3. **Add KayKit model**
   - Drag the corresponding KayKit prefab from `Assets/_Project/Prefabs/Characters/KayKit/`
   - Or for enemies: `Assets/_Project/Prefabs/Characters/KayKit/Skeletons/`
   - Place as child of root GameObject
   - Rename child to "VisualModel"
   - Set Transform: Position (0,0,0), Rotation (0,0,0), Scale (see table above)

4. **Add Animator component**
   - Select root GameObject
   - Add Component → Animation → Animator
   - Set Controller: `Assets/_Project/Animations/KayKit/Controllers/AC_KayKit_Medium.controller`
   - Uncheck "Apply Root Motion"
   - Update Mode: Normal

5. **Save the prefab** (Ctrl+S)

### Example: Wiring Milo

```
Milo (root)
  ├─ CharacterController component
  ├─ Animator component (Controller: AC_KayKit_Medium)
  └─ VisualModel (child GameObject)
      └─ [Char_Ranger prefab instance]
          ├─ Ranger mesh
          └─ All sub-parts
```

## Animation Controller Setup

The `AC_KayKit_Medium` controller already has:
- **States:** Idle, Walk, Run, Attack, Death
- **Parameters:** Speed (float), Attack (trigger), Death (trigger)
- **Transitions:** Auto-blend between locomotion states

## Verification Steps

1. **Open Echohaven_VerticalSlice scene**
2. **Press Play**
3. **Check Milo spawns:**
   - Should see Ranger model (NOT capsule primitive)
   - Model should be textured with KayKit materials
   - Should have bow weapon attached

4. **Check other characters in Project window:**
   - Navigate to `Assets/_Project/Prefabs/Characters/`
   - Drag each prefab into scene temporarily
   - Verify KayKit model appears (not placeholder)

## Troubleshooting

### Character appears as capsule/primitives
- VisualModel child wasn't added correctly
- Re-add the KayKit prefab as child

### No animations playing
- Animator component missing or controller not assigned
- Verify AC_KayKit_Medium.controller is set

### Model is too large/small
- Check Transform scale on VisualModel child
- Korath should be 2.0x, all others 1.0x

### Missing textures/pink materials
- Unity hasn't imported KayKit materials yet
- Wait for import to complete or reimport Assets/KayKit_* folders

## File Locations

- **Character Prefabs:** `Assets/_Project/Prefabs/Characters/*.prefab`
- **KayKit Models:** `Assets/_Project/Prefabs/Characters/KayKit/`
- **Skeleton Models:** `Assets/_Project/Prefabs/Characters/KayKit/Skeletons/`
- **Animation Controller:** `Assets/_Project/Animations/KayKit/Controllers/AC_KayKit_Medium.controller`
- **Editor Tool:** `Assets/_Project/Scripts/Editor/KayKitCharacterWiringTool.cs`

## Next Steps After Wiring

1. Test in Echohaven scene
2. Verify all 9 characters have models
3. Commit changes:
   ```
   git add Assets\_Project\Prefabs\Characters\*.prefab Assets\_Project\Prefabs\Characters\*.prefab.meta
   git commit -m "KAYKIT WIRING: All 9 characters wired to production models"
   ```
4. Run full build to ensure no errors
5. Update build notes with character model status
